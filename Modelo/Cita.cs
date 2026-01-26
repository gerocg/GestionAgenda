using GestionAgenda.Enums;
using System.ComponentModel.DataAnnotations;

namespace GestionAgenda.Modelo
{
    public class Cita
    {
        [Key]
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public Paciente Paciente { get; set; }
        public int ProfesionalId { get; set; }
        public Profesional Profesional { get; set; }
        public DateTime FechaAgendada { get; set; }
        public string Tratamiento { get; set; }
        public string? Observaciones { get; set; }
        public EstadoCita Estado { get; set; } = EstadoCita.Confirmada;
        public int DuracionMinutos { get; set; }
        public ICollection<Archivo> Archivos { get; set; }
        public bool Recordatorio24hEnviado { get; set; }
        public bool Recordatorio2hEnviado { get; set; }

    }
}
