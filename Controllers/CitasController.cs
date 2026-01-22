using GestionAgenda.Context;
using GestionAgenda.DTOs;
using GestionAgenda.Enums;
using GestionAgenda.Modelo;
using GestionAgenda.Services;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GestionAgenda.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CitasController : ControllerBase
    {
        private readonly ContextBd _context;
        private readonly CitasEmailService _emailService;

        public CitasController(ContextBd context, CitasEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        //Alta de cita
        [Authorize(Roles = "Paciente,Admin,Profesional")]
        [HttpPost("nuevaCita")]
        public async Task<IActionResult> NuevaCita([FromBody] CitaDTO cita)
        {
            if (cita == null) return BadRequest("Datos inválidos");

            int pacienteIdFinal;
            Paciente paciente;
            if (User.IsInRole("Paciente"))
            {
                var userIdToken = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdToken == null) return Unauthorized("Token inválido: no contiene el ID del usuario");
                var userId = int.Parse(userIdToken.Value);
                paciente = await _context.Pacientes.Include(p => p.Usuario).FirstOrDefaultAsync(p => p.UsuarioId == userId);
                if (paciente == null) return NotFound("Paciente no encontrado");
                pacienteIdFinal = paciente.Id;
            }
            else
            {
                if (!cita.PacienteId.HasValue) return BadRequest("Debe indicar el paciente");
                paciente = await _context.Pacientes.Include(p => p.Usuario).FirstOrDefaultAsync(p => p.Id == cita.PacienteId.Value);
                if (paciente == null) return NotFound("Paciente no encontrado");
                pacienteIdFinal = cita.PacienteId.Value;
            }

            // TODO: soportar multiples profesionales, por ahora solo esta hecho para uno
            var profesionalId = await _context.Profesionales.OrderBy(p => p.Id).Select(p => p.Id).FirstAsync();

            if (profesionalId == 0) return NotFound("Profesional no encontrado");

            var inicio = cita.FechaHora;
            var fin = inicio.AddMinutes(cita.Duracion);

            var haySolapamiento = await _context.Citas.AnyAsync(c =>
                c.ProfesionalId == profesionalId && c.Estado != EstadoCita.Cancelada &&
                c.FechaAgendada < fin && c.FechaAgendada.AddMinutes(c.DuracionMinutos) > inicio
            );

            if (haySolapamiento) return Conflict("El horario ya está ocupado");
            
            if (cita.FechaHora < DateTime.Today) return BadRequest("No se pueden crear citas en fechas pasadas");

            var nuevaCita = new Cita
            {
                PacienteId = pacienteIdFinal,
                ProfesionalId = profesionalId,
                FechaAgendada = cita.FechaHora,
                DuracionMinutos = cita.Duracion,
                Tratamiento = cita.Tratamiento,
                Observaciones = cita.Observaciones,
                Estado = EstadoCita.Confirmada
            };

            _context.Citas.Add(nuevaCita);
            await _context.SaveChangesAsync();

            await _context.Entry(nuevaCita).Reference(c => c.Paciente).Query().Include(p => p.Usuario).LoadAsync();

            await _emailService.EnviarConfirmacion(nuevaCita, paciente.Usuario.Email, paciente.Usuario.NombreCompleto);

            return Ok(new
            {
                id = nuevaCita.Id,
                pacienteId = nuevaCita.PacienteId,
                pacienteNombre = paciente.Usuario.NombreCompleto,
                profesionalId = nuevaCita.ProfesionalId,
                fechaInicio = nuevaCita.FechaAgendada,
                fechaFin = nuevaCita.FechaAgendada.AddMinutes(nuevaCita.DuracionMinutos),
                duracion = nuevaCita.DuracionMinutos,
                tratamiento = nuevaCita.Tratamiento,
                observaciones = nuevaCita.Observaciones,
                estado = nuevaCita.Estado.ToString()
            });
        }

        [Authorize(Roles = "Profesional,Admin")]
        [HttpGet("fechas-disponibles")]
        public IActionResult GetFechasDisponibles([FromQuery] long chatId)
        {

            var estado = _context.EstadoChats.FirstOrDefault(e => e.chatId == chatId);

            if (estado == null)
            {
                estado = new EstadoChat
                {
                    chatId = chatId,
                    updatedAt = DateTime.UtcNow
                };
                _context.EstadoChats.Add(estado);
            }
            else
            {
                estado.updatedAt = DateTime.UtcNow;
            }

            _context.SaveChanges();

            var fechas = ObtenerProximosDiasLaborales();

            var resultado = fechas.Select(f => new
            {
                fechaIso = f.ToString("yyyy-MM-dd"),
                fechaTexto = f.ToString("dd/MM/yyyy"),
                dia = f.DayOfWeek.ToString()
            });

            return Ok(resultado);
        }

        private List<DateTime> ObtenerProximosDiasLaborales()
        {
            
            int cantidad = 5;
            var fechas = new List<DateTime>();
            var fechaActual = DateTime.Today;

            while (fechas.Count < cantidad)
            {
                // Lunes a Viernes
                if (fechaActual.DayOfWeek != DayOfWeek.Saturday && fechaActual.DayOfWeek != DayOfWeek.Sunday)
                {
                    fechas.Add(fechaActual);
                }

                fechaActual = fechaActual.AddDays(1);
            }

            return fechas;
        }

        [Authorize(Roles = "Profesional,Admin")]
        [HttpGet("horas-disponibles")]
        public IActionResult GetHorasDisponibles([FromQuery] long chatId, [FromQuery] DateTime fecha)
        {
            var estado = _context.EstadoChats.FirstOrDefault(e => e.chatId == chatId);

            if (estado == null) return BadRequest("No existe estado para el chat");

            estado.fecha = DateOnly.FromDateTime(fecha);
            estado.hora = null;
            estado.updatedAt = DateTime.UtcNow;

            _context.SaveChanges();

            var horas = ObtenerHorasDisponibles(fecha);

            var resultado = horas.Select(h => new
            {
                hora = h.ToString(@"hh\:mm")
            });

            return Ok(resultado);
        }

        private List<TimeSpan> ObtenerHorasDisponibles(DateTime fecha)
        {
            var horaInicio = new TimeSpan(8, 0, 0);
            var horaFin = new TimeSpan(19, 0, 0);
            var duracionTurno = TimeSpan.FromMinutes(60);

            var citasDelDia = _context.Citas.Where(c => c.FechaAgendada.Date == fecha.Date).Select(c => c.FechaAgendada.TimeOfDay).ToList();

            var horasDisponibles = new List<TimeSpan>();
            var horaActual = horaInicio;

            while (horaActual < horaFin)
            {
                if (!citasDelDia.Contains(horaActual))
                {
                    horasDisponibles.Add(horaActual);
                }

                horaActual = horaActual.Add(duracionTurno);
            }

            return horasDisponibles;
        }

        [HttpPost("guardar-hora")]
        public IActionResult GuardarHora([FromQuery] long chatId, [FromQuery] TimeOnly hora)
        {
            var estado = _context.EstadoChats.FirstOrDefault(e => e.chatId == chatId);

            if (estado == null || estado.fecha == null) return BadRequest("Falta seleccionar fecha");

            estado.hora = hora;
            estado.updatedAt = DateTime.Now;
            Console.WriteLine($"Hora antes de guardar: {estado.hora}");

            _context.SaveChanges();

            return Ok(new { mensaje = "Hora guardada" });
        }

        [HttpPost("guardar-telefono")]
        public IActionResult GuardarTelefono([FromQuery] long chatId, [FromQuery] string telefono)
        {
            var estado = _context.EstadoChats.FirstOrDefault(e => e.chatId == chatId);

            if (estado == null) return BadRequest("No existe estado para el chat");

            estado.telefono = PacienteService.NormalizarTelefono(telefono);
            estado.updatedAt = DateTime.UtcNow;

            _context.SaveChanges();

            return Ok(new { mensaje = "Teléfono guardado" });
        }

        [HttpGet("preconfirmar")]
        public IActionResult Preconfirmar([FromQuery] long chatId, [FromQuery] string telefono)
        {
            telefono = PacienteService.NormalizarTelefono(telefono);

            var estado = _context.EstadoChats.FirstOrDefault(e => e.chatId == chatId);

            if (estado == null || estado.fecha == null || estado.hora == null) return BadRequest("Datos incompletos");

            var paciente = _context.Pacientes.Include(p => p.Usuario).FirstOrDefault(p => p.Telefono == telefono);

            if (paciente == null) return NotFound("Paciente no encontrado");

            return Ok(new
            {
                pacienteId = paciente.Id,
                nombre = paciente.Usuario.NombreCompleto,
                fecha = estado.fecha.Value.ToString("dd/MM/yyyy"),
                hora = estado.hora.Value.ToString("HH:mm"),
                fechaIso = estado.fecha.Value.ToString("yyyy-MM-dd"),
                horaIso = estado.hora.Value.ToString("HH:mm")
            });
        }

        // GET: api/Citas
        [Authorize(Roles = "Profesional,Admin")]
        [HttpGet("getCitas")]
        public async Task<ActionResult<IEnumerable<Cita>>> GetCitas()
        {
            await NormalizarEstadosCitas();
            return await _context.Citas.Include(c => c.Paciente).ThenInclude(p => p.Usuario).Include(c => c.Profesional).ThenInclude(p => p.Usuario).ToListAsync();
        }

        [Authorize(Roles = "Profesional, Admin, Paciente")]
        [HttpGet("getCitasEntreFechas")]
        public async Task<IActionResult> GetCitasEntreFechas([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            await NormalizarEstadosCitas();

            var citas = await _context.Citas.Include(c => c.Paciente).ThenInclude(p => p.Usuario).Where(c =>
                    c.Estado != EstadoCita.Cancelada && c.FechaAgendada < fechaFin && c.FechaAgendada.AddMinutes(c.DuracionMinutos) > fechaInicio
                ).Select(c => new
                {
                    id = c.Id,
                    title = c.Paciente.Usuario.NombreCompleto,
                    start = c.FechaAgendada,
                    end = c.FechaAgendada.AddMinutes(c.DuracionMinutos),
                    estado = c.Estado.ToString()
                }).ToListAsync();

            return Ok(citas);
        }


        [Authorize(Roles = "Profesional,Admin, Paciente")]
        [HttpGet("filtrar")]
        public async Task<ActionResult<IEnumerable<Cita>>> Filtrar([FromQuery] int? pacienteId, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] string? estado)
        {
            await NormalizarEstadosCitas();

            var query = _context.Citas.Include(c => c.Paciente).ThenInclude(p => p.Usuario).AsQueryable();

            if (User.IsInRole("Paciente"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                query = query.Where(c => c.Paciente.UsuarioId == userId);
            }
            else if (pacienteId.HasValue)
            {
                query = query.Where(c => c.PacienteId == pacienteId.Value);
            }

            if (desde.HasValue) query = query.Where(c => c.FechaAgendada >= desde.Value.Date);

            if (hasta.HasValue)
            {
                var hastaFinDia = hasta.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(c => c.FechaAgendada <= hastaFinDia);
            }

            if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoCita>(estado, out var estadoEnum))
            {
                query = query.Where(c => c.Estado == estadoEnum);
            }

            var resultado = await query
                .OrderByDescending(c => c.FechaAgendada)
                .Select(c => new
                {
                    id = c.Id,
                    fecha = c.FechaAgendada,
                    estado = c.Estado.ToString(),
                    paciente = new
                    {
                        id = c.Paciente.Id,
                        nombre = c.Paciente.Usuario.NombreCompleto
                    }
                }).ToListAsync();

            return Ok(resultado);
        }

        [Authorize(Roles = "Admin, Profesional, Paciente")]
        [HttpPost("{citaId}/archivo")]
        public async Task<IActionResult> SubirArchivo(int citaId, IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0) return BadRequest("Archivo vacío");

            var tiposPermitidos = new[] { "application/pdf", "image/jpeg", "image/png", "image/jpg" };
            if (!tiposPermitidos.Contains(archivo.ContentType)) return BadRequest("Tipo de archivo no permitido");

            var citaExiste = await _context.Citas.AnyAsync(c => c.Id == citaId);
            if (!citaExiste) return NotFound("Cita no encontrada");

            var carpetaFisica = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "citas", citaId.ToString());

            Directory.CreateDirectory(carpetaFisica);
            var rutaFisica = Path.Combine(carpetaFisica, archivo.FileName);

            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            var rutaPublica = $"/uploads/citas/{citaId}/{archivo.FileName}";
            var adjunto = new Archivo
            {
                CitaId = citaId,
                NombreArchivo = archivo.FileName,
                TipoArchivo = archivo.ContentType,
                RutaArchivo = rutaPublica,
                FechaSubida = DateTime.UtcNow
            };

            _context.Archivos.Add(adjunto);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Archivo subido correctamente" });
        }

        [Authorize(Roles = "Admin, Profesional")]
        [HttpDelete("archivo/{archivoId}")]
        public async Task<IActionResult> EliminarArchivo(int archivoId)
        {
            var archivo = await _context.Archivos.FindAsync(archivoId);
            if (archivo == null) return NotFound("Archivo no encontrado");

            var rutaFisica = Path.Combine(
                Directory.GetCurrentDirectory(),
                archivo.RutaArchivo.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            if (System.IO.File.Exists(rutaFisica)) System.IO.File.Delete(rutaFisica);

            _context.Archivos.Remove(archivo);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Archivo eliminado correctamente" });
        }


        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCita(int id)
        {
            var cita = await _context.Citas.Include(c => c.Paciente).ThenInclude(p => p.Usuario).FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null) return NotFound();

            return Ok(new
            {
                id = cita.Id,
                fechaHora = cita.FechaAgendada,
                tratamiento = cita.Tratamiento,
                observaciones = cita.Observaciones,
                pacienteId = cita.PacienteId,
                paciente = new
                {
                    usuario = new
                    {
                        nombreCompleto = cita.Paciente.Usuario.NombreCompleto
                    }
                }
            });
        }

        [Authorize(Roles = "Profesional,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCita(int id, [FromBody] CitaDTO dto)
        {
            var cita = await _context.Citas.FindAsync(id);

            if (cita == null) return NotFound("La cita no existe");

            if (dto.FechaHora < DateTime.Today) return BadRequest("No se pueden crear citas en fechas pasadas");

            cita.FechaAgendada = dto.FechaHora;
            cita.DuracionMinutos = dto.Duracion;
            cita.Tratamiento = dto.Tratamiento;
            cita.Observaciones = dto.Observaciones;
            if (dto.PacienteId.HasValue) cita.PacienteId = dto.PacienteId.Value;

            await _context.SaveChangesAsync();

            var citaCompleta = await _context.Citas.Include(c => c.Paciente).ThenInclude(p => p.Usuario).FirstAsync(c => c.Id == id);

            await _emailService.EnviarModificacion(citaCompleta, citaCompleta.Paciente.Usuario.Email, citaCompleta.Paciente.Usuario.NombreCompleto);

            return NoContent();
        }

        [Authorize(Roles = "Profesional,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCita(int id)
        {
            var cita = await _context.Citas.Include(c => c.Paciente).ThenInclude(p => p.Usuario).FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null) return NotFound("La cita no existe");

            cita.Estado = EstadoCita.Cancelada;

            await _context.SaveChangesAsync();

            await _emailService.EnviarCancelacion(cita, cita.Paciente.Usuario.Email, cita.Paciente.Usuario.NombreCompleto);

            return NoContent();
        }

        private async Task NormalizarEstadosCitas()
        {
            var ahora = DateTime.Now;

            var citasParaActualizar = await _context.Citas.Where(c => c.Estado == EstadoCita.Confirmada && c.FechaAgendada.AddMinutes(c.DuracionMinutos) < ahora).ToListAsync();

            if (!citasParaActualizar.Any()) return;

            foreach (var cita in citasParaActualizar)
            {
                cita.Estado = EstadoCita.PendienteResultado;
            }

            await _context.SaveChangesAsync();
        }


        [Authorize(Roles = "Admin,Profesional")]
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoCitaDTO nuevoEstado)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return NotFound("Cita no encontrada");


            if (cita.Estado != EstadoCita.PendienteResultado) return BadRequest("Solo se puede cambiar el estado de citas pendientes de resultado");
            if (!Enum.TryParse<EstadoCita>(nuevoEstado.Estado, out var estado)) return BadRequest("Estado inválido");
            if (estado != EstadoCita.Realizada && estado != EstadoCita.Inasistencia) return BadRequest("Estado no permitido");

            cita.Estado = estado;
            await _context.SaveChangesAsync();

            return Ok(new { estado = cita.Estado.ToString() });
        }

        [Authorize(Roles = "Admin,Profesional")]
        [HttpGet("reporteCitas")]
        public async Task<IActionResult> ReporteCitas([FromQuery] int? pacienteId, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] string? estado) {
            var query = _context.Citas.Include(c => c.Paciente).ThenInclude(p => p.Usuario).AsQueryable();

            query = query.Where(c => c.Estado == EstadoCita.Realizada || c.Estado == EstadoCita.Cancelada || c.Estado == EstadoCita.Inasistencia);

            if (pacienteId.HasValue) query = query.Where(c => c.PacienteId == pacienteId.Value);

            if (desde.HasValue) query = query.Where(c => c.FechaAgendada >= desde.Value.Date);

            if (hasta.HasValue) query = query.Where(c => c.FechaAgendada <= hasta.Value.Date.AddDays(1).AddTicks(-1));

            if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoCita>(estado, out var estadoEnum))
                query = query.Where(c => c.Estado == estadoEnum);

            var citas = await query.ToListAsync();

            var detalle = citas.Select(c => new CitaReporteDTO
            {
                Id = c.Id,
                FechaHora = c.FechaAgendada,
                DuracionMinutos = c.DuracionMinutos,
                Estado = c.Estado.ToString(),
                Tratamiento = c.Tratamiento,
                Observaciones = c.Observaciones,
                NombrePaciente = c.Paciente.Usuario.NombreCompleto
            }).ToList();

            var resumen = new ResumenReporteCitasDTO
            {
                Total = citas.Count,
                Realizadas = citas.Count(c => c.Estado == EstadoCita.Realizada),
                Canceladas = citas.Count(c => c.Estado == EstadoCita.Cancelada),
                Inasistencias = citas.Count(c => c.Estado == EstadoCita.Inasistencia)
            };

            return Ok(new { resumen, detalle });
        }
    }
}
