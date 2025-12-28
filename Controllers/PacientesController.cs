using GestionAgenda.Context;
using GestionAgenda.DTOs;
using GestionAgenda.Interfaces;
using GestionAgenda.Modelo;
using GestionAgenda.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace GestionAgenda.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacientesController : ControllerBase
    {
        private readonly ContextBd _context;
        private readonly IPacienteService _pacienteService;

        public PacientesController(ContextBd context, IPacienteService pacienteService)
        {
            _context = context;
            _pacienteService = pacienteService;
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

        [Authorize(Roles = "Paciente")]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var paciente = _pacienteService.GetById(int.Parse(userId));

            if (paciente == null)
                return NotFound();

            return Ok(paciente);
        }

        [Authorize(Roles = "Paciente")]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] PacienteDTO dto)
        {
            var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub).Value);

            var paciente = await _context.Pacientes.Include(p => p.Usuario).FirstOrDefaultAsync(p => p.UsuarioId == userId);

            if (paciente == null) return NotFound();

            paciente.FechaNacimiento = dto.FechaNacimiento;
            paciente.Telefono = dto.Telefono;
            paciente.Direccion = dto.Direccion;
            paciente.Usuario.NombreCompleto = dto.NombreCompleto;
            paciente.Usuario.Email = dto.Email;

            await _context.SaveChangesAsync();

            return Ok(paciente);
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
    }
}
