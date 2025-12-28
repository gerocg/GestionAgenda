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

        public AuthController(ContextBd context, JwtService jwt, IConfiguration config)
        {
            _context = context;
            _jwt = jwt;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var usuario = await _context.Usuarios.Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol).FirstOrDefaultAsync(u => u.Email == login.Email);

            // Compara contraseñas (por ahora sin encriptar)
            if (usuario == null) return NotFound("Datos incorrectos.");

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

            // Crea al usuario con los datos ingresados
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
                Telefono = dto.Telefono
            });

            await _context.SaveChangesAsync();

            // Si todo va bien, devolvemos los datos del usuario (sin la contraseña)
            return Ok(new
            {
                nombre = usuario.NombreCompleto,
                email = usuario.Email
            });
        }

        [HttpPost("recuperarContrasena")]
        public async Task<IActionResult> RecuperarContrasenia([FromBody] RecuperarEmailDTO dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario == null) return Ok();

            var nuevaPassword = GenerarContraseniaTemporal();

            var hasher = new PasswordHasher<Usuario>();
            usuario.PasswordHash = hasher.HashPassword(usuario, nuevaPassword);

            usuario.RequiereCambioContrasena = true;

            await _context.SaveChangesAsync();

            await EnviarContraseniaTemporal(
                usuario.Email,
                usuario.NombreCompleto,
                nuevaPassword
            );

            return Ok();
        }

        [Authorize]
        [HttpPost("cambiarContrasenia")]
        public async Task<IActionResult> CambiarContrasenia([FromBody] CambiarContraseniaDTO dto)
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(x => x.Email == email);

            if (usuario == null) return Unauthorized();

            var hasher = new PasswordHasher<Usuario>();

            usuario.PasswordHash = hasher.HashPassword(usuario, dto.NuevaContrasenia);

            usuario.RequiereCambioContrasena = false;

            await _context.SaveChangesAsync();

            return Ok();
        }


        private static string GenerarContraseniaTemporal(int length = 10)
        {
            const string valid = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789@$";
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);

            return new string(bytes.Select(b => valid[b % valid.Length]).ToArray());
        }

        private async Task EnviarContraseniaTemporal(string email, string nombre, string password)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Gestion Agenda", "no-reply@tuapp.com"));
            message.To.Add(new MailboxAddress(nombre, email));
            message.Subject = "Recuperación de contraseña";

            message.Body = new TextPart("html")
            {
                Text = $"""
                        <h3>Hola {nombre}</h3>
                        <p>Tu nueva contraseña temporal es:</p>
                        <h2>{password}</h2>
                        <p>Ingresá al sistema y se te pedirá que la cambies.</p>
                    """
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                "smtp.gmail.com",
                587,
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
               _config["Smtp:User"],
               _config["Smtp:Pass"]
            );

            await smtp.SendAsync(message);

            await smtp.DisconnectAsync(true);
        }
    }
}
