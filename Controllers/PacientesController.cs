using GestionAgenda.Context;
using GestionAgenda.DTOs;
using GestionAgenda.Modelo;
using GestionAgenda.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestionAgenda.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacientesController : ControllerBase
    {
        private readonly ContextBd _context;
        private readonly JwtService _jwt;

        public PacientesController(ContextBd context, JwtService jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        // GET: api/Pacientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Paciente>>> GetPacientes()
        {
            return await _context.Pacientes.ToListAsync();
        }

        // GET: api/Pacientes/5
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

            if (!string.IsNullOrWhiteSpace(direccion))
                query = query.Where(p => p.direccion.Contains(direccion));

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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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

        // POST: api/Pacientes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
            if (paciente.contrasenia_paciente != login.contrasenia_paciente)
            {
                return BadRequest("Datos incorrectos.");
            }

            // Generar token JWT
            var token = _jwt.GenerateToken(
                paciente.usuario_paciente, // userId
                paciente.email             // email
            );

            // Si todo va bien, devolvemos el token
            return Ok(new{token});
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistroDTO registro)
        {
            if (PacienteExists(registro.usuario_paciente) == true)
            {
                return Conflict("Usuario ya existente.");
            }

            // Crea al usuario con los datos ingresados
            var paciente = new Paciente
            {
                usuario_paciente = registro.usuario_paciente,
                contrasenia_paciente = registro.contrasenia_paciente,
                nombre_completo_paciente = registro.nombre_completo_paciente,
                email = registro.email,
                direccion = registro.direccion,
                fecha_nacimiento = registro.fecha_nacimiento,
                telefono = registro.telefono
            };

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
    }
}
