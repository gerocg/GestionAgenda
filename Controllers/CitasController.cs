using GestionAgenda.Context;
using GestionAgenda.DTOs;
using GestionAgenda.Enums;
using GestionAgenda.Modelo;
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

        public CitasController(ContextBd context)
        {
            _context = context;
        }

        //Alta de cita
        [Authorize(Roles = "Paciente,Admin,Profesional")]
        [HttpPost("nuevaCita")]
        public async Task<IActionResult> NuevaCita([FromBody] CitaDTO cita)
        {
            if (cita == null)
                return BadRequest("Datos inválidos");

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
                c.ProfesionalId == profesionalId &&
                c.Estado != EstadoCita.Cancelada &&
                c.FechaAgendada < fin &&
                c.FechaAgendada.AddMinutes(c.DuracionMinutos) > inicio
            );

            if (haySolapamiento) return Conflict("El horario ya está ocupado");

            var nuevaCita = new Cita
            {
                PacienteId = pacienteIdFinal,
                ProfesionalId = profesionalId,
                FechaAgendada = cita.FechaHora,
                DuracionMinutos = cita.Duracion,
                Tratamiento = cita.Tratamiento,
                Observaciones = cita.Observaciones,
                Estado = EstadoCita.Pendiente
            };

            _context.Citas.Add(nuevaCita);
            await _context.SaveChangesAsync();

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

        // GET: api/Citas
        [Authorize(Roles = "Profesional,Admin")]
        [HttpGet("getCitas")]
        public async Task<ActionResult<IEnumerable<Cita>>> GetCitas()
        {
            return await _context.Citas.Include(c => c.Paciente).ThenInclude(p => p.Usuario).Include(c => c.Profesional).ThenInclude(p => p.Usuario).ToListAsync();
        }

        [Authorize(Roles = "Profesional,Admin")]
        [HttpGet("getCitasEntreFechas")]
        public async Task<IActionResult> GetCitasEntreFechas([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            var citas = await _context.Citas.Include(c => c.Paciente).ThenInclude(p => p.Usuario).Where(c =>
                    c.Estado != EstadoCita.Cancelada &&
                    c.FechaAgendada < fechaFin &&
                    c.FechaAgendada.AddMinutes(c.DuracionMinutos) > fechaInicio
                ).Select(c => new
                {
                    id = c.Id,
                    title = c.Paciente.Usuario.NombreCompleto,
                    start = c.FechaAgendada,
                    end = c.FechaAgendada.AddMinutes(c.DuracionMinutos)
                }).ToListAsync();

            return Ok(citas);
        }


        [Authorize(Roles = "Profesional,Admin")]
        [HttpGet("filtrar")]
        public async Task<ActionResult<IEnumerable<Cita>>> Filtrar(
            [FromQuery] int? profesionalId,
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            var query = _context.Citas.Include(c => c.Profesional).AsQueryable();

            if (profesionalId.HasValue) query = query.Where(c => c.ProfesionalId == profesionalId.Value);

            if (desde.HasValue) query = query.Where(c => c.FechaAgendada >= desde.Value);

            if (hasta.HasValue) query = query.Where(c => c.FechaAgendada <= hasta.Value);

            return Ok(await query.ToListAsync());
        }

        [Authorize]
        [HttpPost("{citaId}/archivo")]
        public async Task<IActionResult> SubirArchivo(int citaId, IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("Archivo vacío");

            var citaExiste = await _context.Citas.AnyAsync(c => c.Id == citaId);
            if (!citaExiste)
                return NotFound("Cita no encontrada");

            var carpeta = Path.Combine("uploads", "citas", citaId.ToString());
            Directory.CreateDirectory(carpeta);

            var rutaFisica = Path.Combine(carpeta, archivo.FileName);
            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            var adjunto = new Archivo
            {
                CitaId = citaId,
                NombreArchivo = archivo.FileName,
                TipoArchivo = archivo.ContentType,
                RutaArchivo = rutaFisica.Replace("\\", "/"),
                FechaSubida = DateTime.UtcNow
            };

            _context.Archivos.Add(adjunto);
            await _context.SaveChangesAsync();

            return Ok("Archivo subido correctamente");
        }

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

            cita.FechaAgendada = dto.FechaHora;
            cita.DuracionMinutos = dto.Duracion;
            cita.Tratamiento = dto.Tratamiento;
            cita.Observaciones = dto.Observaciones;
            if (dto.PacienteId.HasValue) cita.PacienteId = dto.PacienteId.Value;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Profesional,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCita(int id)
        {
            var cita = await _context.Citas.FindAsync(id);

            if (cita == null) return NotFound("La cita no existe");

            cita.Estado = EstadoCita.Cancelada;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
