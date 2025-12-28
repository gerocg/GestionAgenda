using GestionAgenda.Context;
using GestionAgenda.DTOs;
using GestionAgenda.Interfaces;
using GestionAgenda.Modelo;
using GestionAgenda.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GestionAgenda.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly ContextBd _context;
        private readonly JwtService _jwt;
        private readonly IConfiguration _config;
        private readonly IPacienteService _pacienteService;

        public UsuarioController(ContextBd context, JwtService jwt, IConfiguration config, IPacienteService pacienteService)
        {
            _context = context;
            _jwt = jwt;
            _config = config;
            _pacienteService = pacienteService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }


        [HttpGet("pacientes")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Paciente>>> GetPacientes()
        {
            return await _context.Pacientes.ToListAsync();
        }

        [Authorize]
        [HttpGet("{email}")]
        public async Task<ActionResult<Usuario>> GetUsuario(string email)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);


            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        [Authorize]
        [HttpGet("paciente/{email}")]
        public async Task<ActionResult<Paciente>> GetPaciente(string email)
        {
            var paciente = await _context.Pacientes.FirstOrDefaultAsync(p => p.Usuario.Email == email);

            if (paciente == null)
            {
                return NotFound();
            }

            return paciente;
        }

        [Authorize]
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

        [Authorize]
        [HttpPut("{email}")]
        public async Task<IActionResult> PutUsuario(string email, Usuario u)
        {
            if (email != u.Email)
            {
                return BadRequest();
            }

            _context.Entry(u).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(email))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool PacienteExists(string email)
        {
            return _context.Pacientes.Any(u => u.Usuario.Email == email);
        }

        private bool UsuarioExists(string email)
        {
            return _context.Usuarios.Any(u => u.Email == email);
        }
    }
}
