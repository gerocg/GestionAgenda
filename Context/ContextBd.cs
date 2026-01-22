using GestionAgenda.Controllers;
using GestionAgenda.Modelo;
using Microsoft.EntityFrameworkCore;

namespace GestionAgenda.Context
{
    public class ContextBd : DbContext
    {
        public ContextBd(DbContextOptions<ContextBd> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<UsuarioRol> UsuarioRoles { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Profesional> Profesionales { get; set; }
        public DbSet<HistorialClinico> Historiales { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<Archivo> Archivos { get; set; }
        public DbSet<Recordatorio> Recordatorios { get; set; }
        public DbSet<ConfiguracionCalendario> ConfiguracionCalendario { get; set; }
        public DbSet<BloqueoHorario> BloqueosHorarios { get; set; }
        public DbSet<EstadoChat> EstadoChats { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UsuarioRol>()
                .HasKey(ur => new { ur.UsuarioId, ur.RolId });

            modelBuilder.Entity<Paciente>()
                .HasOne(p => p.Usuario)
                .WithOne(u => u.Paciente)
                .HasForeignKey<Paciente>(p => p.UsuarioId);

            modelBuilder.Entity<Profesional>()
                .HasOne(p => p.Usuario)
                .WithOne(u => u.Profesional)
                .HasForeignKey<Profesional>(p => p.UsuarioId);
            
            modelBuilder.Entity<HistorialClinico>()
                .HasOne(h => h.Paciente)
                .WithOne(p => p.HistorialClinico)
                .HasForeignKey<HistorialClinico>(h => h.PacienteId);

            modelBuilder.Entity<Rol>().HasData(
                new Rol { Id = 1, Nombre = "Paciente" },
                new Rol { Id = 2, Nombre = "Profesional" },
                new Rol { Id = 3, Nombre = "Admin" }
            );

            modelBuilder.Entity<ConfiguracionCalendario>().HasData(
                new ConfiguracionCalendario
                {
                    Id = 1,
                    HoraInicio = new TimeSpan(8, 0, 0),
                    HoraFin = new TimeSpan(18, 0, 0),
                    IntervaloBase = 30,
                    DuracionCita = 30
                }
            );

            modelBuilder.Entity<EstadoChat>()
                .Property(e => e.fecha)
                .HasColumnType("DATE");

            modelBuilder.Entity<EstadoChat>()
                .Property(e => e.hora)
                .HasColumnType("TIME");

        }


    }
}
