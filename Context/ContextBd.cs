using GestionAgenda.Modelo;
using Microsoft.EntityFrameworkCore;

namespace GestionAgenda.Context
{
    public class ContextBd : DbContext
    {
        public ContextBd(DbContextOptions<ContextBd> options) : base (options) { }
        
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Profesional> Profesionales { get; set; }
        public DbSet<HistorialClinico> Historiales { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<Archivo> Archivos { get; set; }
        public DbSet<Recordatorio> Recordatorios { get; set; }
        public DbSet<Telefono> Telefonos { get; set; }


    }
}
