using GestionAgenda.Context;
using GestionAgenda.DTOs;
using GestionAgenda.Interfaces;
using GestionAgenda.Modelo;
using System;

namespace GestionAgenda.Services
{
    public class PacienteService : IPacienteService
    {
        private readonly ContextBd _context;

        public PacienteService(ContextBd context)
        {
            _context = context;
        }

        public Paciente GetById(int id)
        {
            return _context.Pacientes.FirstOrDefault(p => p.id_paciente == id);
        }

        public void Update(int id, PacienteDTO dto)
        {
            var paciente = _context.Pacientes.Find(id);
            if (paciente == null) return;

            paciente.nombre_completo_paciente = dto.nombre_completo_paciente;
            paciente.email = dto.email;
            paciente.telefono = dto.telefono;
            paciente.fecha_nacimiento = dto.fecha_nacimiento;

            _context.SaveChanges();
        }
    }
}
