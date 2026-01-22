using GestionAgenda.Context;
using GestionAgenda.DTOs;
using GestionAgenda.Enums;
using GestionAgenda.Interfaces;
using GestionAgenda.Modelo;
using GestionAgenda.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
    [Route("api/[controller]")]
    [ApiController]
    public class PacientesController : ControllerBase
    {
        private readonly ContextBd _context;
        private readonly IPacienteService _pacienteService;
        private readonly CredencialesService _credenciales;

        public PacientesController(ContextBd context, IPacienteService pacienteService, CredencialesService credenciales)
        {
            _context = context;
            _pacienteService = pacienteService;
            _credenciales = credenciales;
        }

        [Authorize(Roles = "Profesional,Admin")]
        [HttpGet("getPacientes")]
        public async Task<IActionResult> GetPacientes()
        {
            var pacientes = await _context.Pacientes
                .Select(p => new PacienteDTO
                {
                    Id = p.Id,
                    NombreCompleto = p.Usuario.NombreCompleto,
                    Email = p.Usuario.Email,
                    FechaNacimiento = p.FechaNacimiento,
                    Telefono = p.Telefono,
                    Direccion = p.Direccion
                })
                .ToListAsync();

            return Ok(pacientes);
        }

        [Authorize(Roles = "Profesional,Admin")]
        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string texto)
        {
            if (string.IsNullOrWhiteSpace(texto) || texto.Length < 3)
                return Ok(new List<object>());

            var pacientes = await _context.Pacientes.Include(p => p.Usuario).Where(p =>
                    p.Usuario.NombreCompleto.Contains(texto)).OrderBy(p => p.Usuario.NombreCompleto).Take(10)
                    .Select(p => new
                    {
                        id = p.Id,
                        nombreCompleto = p.Usuario.NombreCompleto
                    }).ToListAsync();

            return Ok(pacientes);
        }

        // GET: api/Pacientes/filtrar
        [Authorize(Roles = "Profesional,Admin")]
        [HttpGet("filtrar")]
        public async Task<ActionResult<IEnumerable<Paciente>>> Filtrar(
            [FromQuery] string? nombre,
            [FromQuery] string? email,
            [FromQuery] string? direccion,
            [FromQuery] DateTime? nacidoAntes,
            [FromQuery] DateTime? nacidoDespues,
            [FromQuery] string? telefono)
        {
            var query = _context.Pacientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(p => p.Usuario.NombreCompleto.Contains(nombre));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(p => p.Usuario.Email.Contains(email));

            if (nacidoAntes.HasValue)
                query = query.Where(p => p.FechaNacimiento < nacidoAntes.Value);

            if (nacidoDespues.HasValue)
                query = query.Where(p => p.FechaNacimiento > nacidoDespues.Value);

            if (!string.IsNullOrWhiteSpace(telefono))
                query = query.Where(p => p.Telefono.Contains(telefono));

            var pacientes = await query.ToListAsync();

            if (!pacientes.Any())
                return NotFound("No se encontraron pacientes con esos filtros.");

            return Ok(pacientes);
        }

        [Authorize(Roles = "Admin, Profesional")]
        [HttpGet("consulta")]
        public async Task<IActionResult> Consulta([FromQuery] string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return Ok(new List<object>());

            var pacientes = await _context.Pacientes.Include(p => p.Usuario)
                .Where(p => p.Usuario.NombreCompleto.Contains(texto) || p.Usuario.Email.Contains(texto) || p.Telefono.Contains(texto)).OrderBy(p => p.Usuario.NombreCompleto)
                .Select(p => new
                {
                    id = p.Id,
                    nombreCompleto = p.Usuario.NombreCompleto,
                    email = p.Usuario.Email,
                    fechaNacimiento = p.FechaNacimiento,
                    telefono = p.Telefono
                }).ToListAsync();

            return Ok(pacientes);
        }


        [Authorize(Roles = "Paciente")]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var paciente = await _context.Pacientes.Include(p => p.Usuario).Where(p => p.UsuarioId == userId)
                .Select(p => new PacienteDTO
                {
                    Id = p.Id,
                    NombreCompleto = p.Usuario.NombreCompleto,
                    Email = p.Usuario.Email,
                    FechaNacimiento = p.FechaNacimiento,
                    Telefono = p.Telefono,
                    Direccion = p.Direccion
                }).FirstOrDefaultAsync();

            if (paciente == null) return NotFound();

            return Ok(paciente);
        }



        [Authorize(Roles = "Paciente")]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] PacienteUpdateDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var paciente = await _context.Pacientes.Include(p => p.Usuario).FirstOrDefaultAsync(p => p.UsuarioId == userId);

            if (paciente == null) return NotFound();

            paciente.FechaNacimiento = dto.FechaNacimiento;
            paciente.Telefono = dto.Telefono;
            paciente.Direccion = dto.Direccion;

            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool PacienteExists(string email)
        {
            return _context.Pacientes.Any(p => p.Usuario.Email == email);
        }

        // POST: api/Pacientes
        [HttpPost]
        public async Task<ActionResult<Paciente>> PostPaciente(Paciente paciente)
        {
            _context.Pacientes.Add(paciente);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PacienteExists(paciente.Usuario.Email))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetPaciente", new { email = paciente.Usuario.Email }, paciente);
        }

        [Authorize(Roles = "Admin,Profesional")]
        [HttpPost("nuevoPaciente")]
        public async Task<IActionResult> CrearPaciente([FromBody] NuevoPacienteDTO dto)
        {
            if (_context.Usuarios.Any(u => u.Email == dto.Email)) return BadRequest("El email ya existe.");

            var passwordTemporal = _credenciales.GenerarContraseniaTemporal();

            var usuario = new Usuario
            {
                NombreCompleto = dto.NombreCompleto,
                Email = dto.Email,
                RequiereCambioContrasena = true
            };
            _credenciales.SetPassword(usuario, passwordTemporal);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var rolPaciente = await _context.Roles .FirstAsync(r => r.Nombre == "Paciente");

            _context.UsuarioRoles.Add(new UsuarioRol
            {
                UsuarioId = usuario.Id,
                RolId = rolPaciente.Id
            });

            var paciente = new Paciente
            {
                UsuarioId = usuario.Id,
                Telefono = PacienteService.NormalizarTelefono(dto.Telefono),
                FechaNacimiento = dto.FechaNacimiento
            };

            _context.Pacientes.Add(paciente);

            await _context.SaveChangesAsync();

            await _credenciales.EnviarContraseniaTemporal(usuario.Email, usuario.NombreCompleto, passwordTemporal);

            return Ok();
        }

        

        [Authorize(Roles = "Admin,Profesional")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePaciente(int id, PacienteAdminUpdateDTO dto)
        {
            var paciente = await _context.Pacientes.Include(p => p.Usuario).FirstOrDefaultAsync(p => p.Id == id);

            if (paciente == null) return NotFound();

            paciente.Usuario.NombreCompleto = dto.NombreCompleto;
            paciente.Telefono = dto.Telefono;
            paciente.FechaNacimiento = dto.FechaNacimiento;

            await _context.SaveChangesAsync();
            return Ok();
        }


        [Authorize(Roles = "Admin,Profesional")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaciente(int id)
        {
            var paciente = await _context.Pacientes.Include(p => p.Usuario).FirstOrDefaultAsync(p => p.Id == id);

            if (paciente == null) return NotFound("Paciente no encontrado.");

            return Ok(new
            {
                id = paciente.Id,
                nombreCompleto = paciente.Usuario.NombreCompleto,
                email = paciente.Usuario.Email,
                telefono = paciente.Telefono,
                fechaNacimiento = paciente.FechaNacimiento
            });
        }

        [Authorize(Roles = "Admin,Profesional")]
        [HttpGet("{pacienteId}/historial")]
        public async Task<IActionResult> GetHistorialClinico(int pacienteId)
        {
            var paciente = await _context.Pacientes.Include(p => p.Usuario).FirstOrDefaultAsync(p => p.Id == pacienteId);
            if (paciente == null) return NotFound("Paciente no encontrado");

            var citas = await _context.Citas
                .Include(c => c.Archivos)
                .Where(c => c.PacienteId == pacienteId && c.Estado != EstadoCita.Cancelada)
                .OrderByDescending(c => c.FechaAgendada)
                .Select(c => new
                {
                    id = c.Id,
                    fecha = c.FechaAgendada,
                    tratamiento = c.Tratamiento,
                    observaciones = c.Observaciones,
                    estado = c.Estado.ToString(),
                    archivos = c.Archivos.Select(a => new
                    {
                        id = a.Id,
                        nombre = a.NombreArchivo,
                        tipo = a.TipoArchivo,
                        ruta = a.RutaArchivo,
                        fechaSubida = a.FechaSubida
                    })
                }).ToListAsync();

            return Ok(new
            {
                paciente = new
                {
                    id = paciente.Id,
                    nombreCompleto = paciente.Usuario.NombreCompleto,
                    fechaNacimiento = paciente.FechaNacimiento
                },
                citas
            });
        }

        [Authorize(Roles = "Paciente")]
        [HttpGet("me/historial")]
        public async Task<IActionResult> MiHistorial()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);

            var paciente = await _context.Pacientes.Include(p => p.Usuario).FirstOrDefaultAsync(p => p.UsuarioId == userId);

            if (paciente == null) return NotFound();

            return await GetHistorialClinico(paciente.Id);
        }

    }
}
