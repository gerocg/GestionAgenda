using GestionAgenda.Context;
using GestionAgenda.DTOs;
using GestionAgenda.Interfaces;
using GestionAgenda.Modelo;
using Microsoft.EntityFrameworkCore;
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
            return _context.Pacientes.Include(p => p.Usuario).Include(p => p.HistorialClinico).FirstOrDefault(p => p.UsuarioId == id);
        }

        public void Update(int id, PacienteDTO dto)
        {
            var paciente = _context.Pacientes.Include(p => p.Usuario).FirstOrDefault(p => p.UsuarioId == id);

            if (paciente == null) return;

            // Datos del paciente
            paciente.FechaNacimiento = dto.FechaNacimiento;
            paciente.Telefono = dto.Telefono;
            paciente.Direccion = dto.Direccion;

            // Datos del usuario
            paciente.Usuario.NombreCompleto = dto.NombreCompleto;
            paciente.Usuario.Email = dto.Email;

            _context.SaveChanges();
        }
    }
}
