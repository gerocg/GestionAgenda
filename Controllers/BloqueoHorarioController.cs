using GestionAgenda.Context;
using GestionAgenda.DTOs;
using GestionAgenda.Modelo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionAgenda.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BloqueoHorarioController : ControllerBase
    {
        private readonly ContextBd _context;

        public BloqueoHorarioController(ContextBd context)
        {
            _context = context;
        }

        [Authorize(Roles = "Profesional,Admin,Paciente")]
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var query = _context.BloqueosHorarios.AsQueryable();

            if (desde.HasValue) query = query.Where(b => b.FechaHasta >= desde.Value);

            if (hasta.HasValue) query = query.Where(b => b.FechaDesde <= hasta.Value);

            if (!desde.HasValue && !hasta.HasValue) query = query.Where(b => b.FechaHasta >= DateTime.Today);

            return Ok(await query.OrderBy(b => b.FechaDesde).ToListAsync());
        }

        [Authorize(Roles = "Profesional,Admin")]
        [HttpPost]
        public async Task<IActionResult> Crear(BloqueoHorarioDTO bloqueo)
        {
            if (bloqueo.FechaDesde >= bloqueo.FechaHasta) return BadRequest("Rango inválido");

            if (bloqueo.FechaDesde <= DateTime.Now) return BadRequest("No se permiten bloqueos en el pasado");

            bool solapa = await _context.BloqueosHorarios.AnyAsync(b => bloqueo.FechaDesde < b.FechaHasta && bloqueo.FechaHasta > b.FechaDesde);

            if (solapa) return Conflict("El bloqueo se solapa con otro existente");

            var bloqueoNuevo = new BloqueoHorario
            {
                FechaDesde = bloqueo.FechaDesde,
                FechaHasta = bloqueo.FechaHasta,
                Motivo = bloqueo.Motivo
            };
            
            _context.BloqueosHorarios.Add(bloqueoNuevo);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "Profesional,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var bloqueo = await _context.BloqueosHorarios.FindAsync(id);
            if (bloqueo == null) return NotFound();

            _context.BloqueosHorarios.Remove(bloqueo);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
