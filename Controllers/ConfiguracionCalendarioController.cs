using GestionAgenda.Context;
using GestionAgenda.DTOs;
using GestionAgenda.Modelo;
using GestionAgenda.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionAgenda.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfiguracionCalendarioController : ControllerBase
    {
        private readonly ContextBd _context;
        
        public ConfiguracionCalendarioController(ContextBd context)
        {
            _context = context;
        }

        [Authorize(Roles = "Profesional,Admin,Paciente")]
        [HttpGet]
        public async Task<ActionResult<ConfiguracionCalendario>> Get()
        {
            return await _context.ConfiguracionCalendario.FirstAsync();
        }

        [Authorize(Roles = "Profesional,Admin")]
        [HttpPut]
        public async Task<IActionResult> Update(ConfiguracionCalendarioDTO dto)
        {
            var config = await _context.ConfiguracionCalendario.FirstAsync();

            config.HoraInicio = dto.HoraInicio;
            config.HoraFin = dto.HoraFin;
            config.IntervaloBase = dto.IntervaloBase;
            config.DuracionCita = dto.DuracionCita;

            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
