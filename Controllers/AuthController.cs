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
using Microsoft.AspNetCore.Identity.Data;
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
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ContextBd _context;
        private readonly JwtService _jwt;
        private readonly IConfiguration _config;
        private readonly CredencialesService _credenciales;
        private readonly IPacienteService _pacienteService;

        public AuthController(ContextBd context, JwtService jwt, IConfiguration config, CredencialesService credenciales, IPacienteService pacienteService)
        {
            _context = context;
            _jwt = jwt;
            _config = config;
            _credenciales = credenciales;
            _pacienteService = pacienteService;

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var usuario = await _context.Usuarios.Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol).FirstOrDefaultAsync(u => u.Email == login.Email);
            
            if (usuario == null) return NotFound("Usuario no encontrado.");

            if (usuario.PasswordHash == null) return Unauthorized("Debe completar su registro.");

            var hasher = new PasswordHasher<Usuario>();
            var resultado = hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, login.Contrasenia);

            if (resultado == PasswordVerificationResult.Failed) return BadRequest("Datos incorrectos.");

            var roles = usuario.UsuarioRoles.Select(r => r.Rol.Nombre).ToList();

            var token = _jwt.GenerateToken(usuario);

            return Ok(new
            {
                token,
                roles,
                requiereCambioContrasena = usuario.RequiereCambioContrasena
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UsuarioDTO dto)
        {
            if (_context.Usuarios.Any(u => u.Email == dto.Email)) return Conflict("Usuario ya existente");

            var usuario = new Usuario
            {
                Email = dto.Email,
                NombreCompleto = dto.NombreCompleto,
                RequiereCambioContrasena = false
            };

            var hasher = new PasswordHasher<Usuario>();
            usuario.PasswordHash = hasher.HashPassword(usuario, dto.Contrasenia);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var rolPaciente = await _context.Roles
                .FirstAsync(r => r.Nombre == "Paciente");

            _context.UsuarioRoles.Add(new UsuarioRol
            {
                UsuarioId = usuario.Id,
                RolId = rolPaciente.Id
            });

            _context.Pacientes.Add(new Paciente
            {
                UsuarioId = usuario.Id,
                FechaNacimiento = dto.FechaNacimiento,
                Telefono = PacienteService.NormalizarTelefono(dto.Telefono)
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                nombre = usuario.NombreCompleto,
                email = usuario.Email
            });
        }

        [HttpPost("recuperarContrasena")]
        public async Task<IActionResult> RecuperarContrasenia([FromBody] RecuperarEmailDTO dto)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario == null) return Ok();

            var nuevaPassword = _credenciales.GenerarContraseniaTemporal();
            _credenciales.SetPassword(usuario, nuevaPassword);
            usuario.RequiereCambioContrasena = true;

            await _context.SaveChangesAsync();
            await _credenciales.EnviarContraseniaTemporal(usuario.Email, usuario.NombreCompleto, nuevaPassword);

            return Ok();
        }

        [Authorize]
        [HttpPost("cambiarContrasenia")]
        public async Task<IActionResult> CambiarContrasenia([FromBody] CambiarContraseniaDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario == null) return Unauthorized();

            var hasher = new PasswordHasher<Usuario>();
            usuario.PasswordHash = hasher.HashPassword(usuario, dto.NuevaContrasenia);
            usuario.RequiereCambioContrasena = false;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
