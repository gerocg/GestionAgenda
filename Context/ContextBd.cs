using GestionAgenda.Modelo;
using Microsoft.EntityFrameworkCore;

namespace GestionAgenda.Context
{
    public class ContextBd : DbContext
    {
        public ContextBd(DbContextOptions<ContextBd> options) : base(options) { }

        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Profesional> Profesionales { get; set; }
        public DbSet<HistorialClinico> Historiales { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<Archivo> Archivos { get; set; }
        public DbSet<Recordatorio> Recordatorios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Paciente>()
            .HasIndex(p => p.usuario_paciente)
            .IsUnique();

            modelBuilder.Entity<Paciente>()
        .HasOne(p => p.historial_clinico)
        .WithOne()
        .HasForeignKey<Paciente>(p => p.historial_clinicoid_hc)
        .OnDelete(DeleteBehavior.Cascade);
        }


    }
}
