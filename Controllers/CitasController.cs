using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionAgenda.Context;
using GestionAgenda.Modelo;

namespace GestionAgenda.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitasController : ControllerBase
    {
        private readonly ContextBd _context;

        public CitasController(ContextBd context)
        {
            _context = context;
        }

        // GET: api/Citas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cita>>> GetCitas()
        {
            return await _context.Citas.ToListAsync();
        }

        [HttpGet("filtrar")]
        public async Task<ActionResult<IEnumerable<Cita>>> Filtrar(
            [FromQuery] int? profesional,
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            var query = _context.Citas
                .Include(c => c.profesional)
                .AsQueryable();

            if (profesional.HasValue)
                query = query.Where(c => c.idProfesional == profesional.Value);

            if (desde.HasValue)
                query = query.Where(c => c.fechaAgendado >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(c => c.fechaAgendado <= hasta.Value);

            return Ok(await query.ToListAsync());
        }

        [HttpPost("subir-archivo/{citaId}")]
        public async Task<IActionResult> SubirArchivo(int citaId, IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("Archivo vacío");

            // 1. Definir carpeta de guardado
            var carpeta = Path.Combine("uploads", citaId.ToString());
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            // 2. Guardar archivo físicamente
            var rutaArchivo = Path.Combine(carpeta, archivo.FileName);
            using (var stream = new FileStream(rutaArchivo, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // 3. Guardar referencia en la base de datos
            var adjunto = new Archivo
            {
                idCita = citaId,
                nombreARchivo = archivo.FileName,
                tipoArchivo = archivo.ContentType,
                rutaArchivo = "/" + rutaArchivo.Replace("\\", "/"),
                fechaSubida = DateTime.UtcNow
            };

            _context.Archivos.Add(adjunto);
            await _context.SaveChangesAsync();

            return Ok("Archivo subido correctamente");
        }

        // GET: api/Citas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cita>> GetCita(int id)
        {
            var cita = await _context.Citas.FindAsync(id);

            if (cita == null)
            {
                return NotFound();
            }

            return cita;
        }

        // PUT: api/Citas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCita(int id, Cita cita)
        {
            if (id != cita.id_cita)
            {
                return BadRequest();
            }

            _context.Entry(cita).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CitaExists(id))
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

        // POST: api/Citas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Cita>> PostCita(Cita cita)
        {
            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCita", new { id = cita.id_cita }, cita);
        }

        // DELETE: api/Citas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCita(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null)
            {
                return NotFound();
            }

            _context.Citas.Remove(cita);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CitaExists(int id)
        {
            return _context.Citas.Any(e => e.id_cita == id);
        }
    }
}
