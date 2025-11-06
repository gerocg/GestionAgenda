using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionAgenda.Context;
using GestionAgenda.Modelo;
using GestionAgenda.DTOs;

namespace GestionAgenda.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacientesController : ControllerBase
    {
        private readonly ContextBd _context;

        public PacientesController(ContextBd context)
        {
            _context = context;
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
