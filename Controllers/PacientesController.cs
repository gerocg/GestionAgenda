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
    public class PacientesController : ControllerBase
    {
        private readonly ContextBd _context;
        private readonly JwtService _jwt;
        private readonly IConfiguration _config;
        private readonly IPacienteService _pacienteService;

        public PacientesController(ContextBd context, JwtService jwt, IConfiguration config, IPacienteService pacienteService)
        {
            _context = context;
            _jwt = jwt;
            _config = config;
            _pacienteService = pacienteService;
        }

        // GET: api/Pacientes
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Paciente>>> GetPacientes()
        {
            return await _context.Pacientes.ToListAsync();
        }

        // GET: api/Pacientes/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Paciente>> GetPaciente(string id)
        {
            var paciente = await _context.Pacientes.FindAsync(id);

            if (paciente == null)
            {
                return NotFound();
            }

            return paciente;
        }

        // GET: api/Pacientes/filtrar
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
                query = query.Where(p => p.nombre_completo_paciente.Contains(nombre));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(p => p.email.Contains(email));

            if (nacidoAntes.HasValue)
                query = query.Where(p => p.fecha_nacimiento < nacidoAntes.Value);

            if (nacidoDespues.HasValue)
                query = query.Where(p => p.fecha_nacimiento > nacidoDespues.Value);

            if (!string.IsNullOrWhiteSpace(telefono))
                query = query.Where(p => p.telefono.Contains(telefono));

            var pacientes = await query.ToListAsync();

            if (!pacientes.Any())
                return NotFound("No se encontraron pacientes con esos filtros.");

            return Ok(pacientes);
        }

        // PUT: api/Pacientes/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaciente(string id, Paciente paciente)
        {
            if (id != paciente.usuario_paciente)
            {
                return BadRequest();
            }

            _context.Entry(paciente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PacienteExists(id))
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

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var paciente = _pacienteService.GetById(int.Parse(userId));

            if (paciente == null)
                return NotFound();

            return Ok(paciente);
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
                if (PacienteExists(paciente.usuario_paciente))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetPaciente", new { id = paciente.usuario_paciente }, paciente);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            if(PacienteExists(login.usuario_paciente) == false)
            {
                return NotFound("Datos incorrectos.");
            }

            // Busca el paciente por usuario
            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.usuario_paciente == login.usuario_paciente);

            // Compara contraseñas (por ahora sin encriptar)
            if (paciente == null) return NotFound("Datos incorrectos.");

            var hasher = new PasswordHasher<Paciente>();
            var resultado = hasher.VerifyHashedPassword(paciente, paciente.contrasenia_paciente, login.contrasenia_paciente);

            if (resultado == PasswordVerificationResult.Failed) return BadRequest("Datos incorrectos.");

            var token = _jwt.GenerateToken(
                paciente.usuario_paciente,
                paciente.email);


            // Si todo va bien, devolvemos el token y si requiere cambio de contrasena
            return Ok(new{token, requiereCambioContrasenia = paciente.requiere_cambio_contrasena });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] PacienteDTO registro)
        {
            if (PacienteExists(registro.email) == true)
            {
                return Conflict("Usuario ya existente.");
            }

            // Crea al usuario con los datos ingresados
            var paciente = new Paciente
            {
                usuario_paciente = registro.email,
                nombre_completo_paciente = registro.nombre_completo_paciente,
                email = registro.email,
                fecha_nacimiento = registro.fecha_nacimiento,
                telefono = registro.telefono
            };

            var hasher = new PasswordHasher<Paciente>();
            paciente.contrasenia_paciente = hasher.HashPassword(paciente, registro.contrasenia_paciente);

            _context.Pacientes.Add(paciente);

            // 🔹 Guardar cambios en la base de datos
            await _context.SaveChangesAsync();

            // Si todo va bien, devolvemos los datos del usuario (sin la contraseña)
            return Ok(new
            {
                usuario = paciente.usuario_paciente,
                nombre = paciente.nombre_completo_paciente,
                email = paciente.email
            });
        }


        private bool PacienteExists(string id)
        {
            return _context.Pacientes.Any(e => e.usuario_paciente == id);
        }


        [HttpPost("recuperarContrasena")]
        public async Task<IActionResult> RecuperarContrasenia([FromBody] RecuperarEmailDTO dto)
        {
            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.email == dto.email);

            if (paciente == null)
                return Ok();

            var nuevaPassword = GenerarContraseniaTemporal();

            var hasher = new PasswordHasher<Paciente>();
            paciente.contrasenia_paciente = hasher.HashPassword(paciente, nuevaPassword);

            paciente.requiere_cambio_contrasena = true;

            await _context.SaveChangesAsync();

            await EnviarContraseniaTemporal(
                paciente.email,
                paciente.nombre_completo_paciente,
                nuevaPassword
            );

            return Ok();
        }

        [Authorize]
        [HttpPost("cambiarContrasenia")]
        public async Task<IActionResult> CambiarContrasenia([FromBody] CambiarContraseniaDTO dto)
        {
            var usuario = User.Identity?.Name;

            if (string.IsNullOrEmpty(usuario))
                return Unauthorized();

            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(x => x.usuario_paciente == usuario);

            if (paciente == null)
                return Unauthorized();

            var hasher = new PasswordHasher<Paciente>();

            paciente.contrasenia_paciente =
                hasher.HashPassword(paciente, dto.nueva_contrasenia);

            paciente.requiere_cambio_contrasena = false;

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
