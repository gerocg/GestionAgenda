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
using System.Security.Claims;
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
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario == null) return NotFound();

            return Ok(usuario);
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UsuarioUpdateDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario == null) return NotFound();

            if (_context.Usuarios.Any(u => u.Email == dto.Email && u.Id != userId)) return BadRequest("El email ya está en uso.");

            usuario.NombreCompleto = dto.NombreCompleto;
            usuario.Email = dto.Email;

            await _context.SaveChangesAsync();

            return Ok(usuario);
        }

    }
}
