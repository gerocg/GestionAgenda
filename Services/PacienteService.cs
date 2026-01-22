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

        public static string NormalizarTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return string.Empty;

            var soloNumeros = new string(telefono.Where(char.IsDigit).ToArray());

            if (soloNumeros.StartsWith("0"))
                soloNumeros = soloNumeros.Substring(1);

            if (soloNumeros.Length == 8)
                soloNumeros = "598" + soloNumeros;

            if (soloNumeros.StartsWith("598") && soloNumeros.Length == 11)
                return soloNumeros;

            return soloNumeros;
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
