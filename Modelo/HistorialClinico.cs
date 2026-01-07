using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GestionAgenda.Modelo
{
    public class HistorialClinico
    {
        [Key]
        public int Id {  get; set; }
        public int PacienteId { get; set; }
        public Paciente Paciente { get; set; }
        public string? ObservacionesGenerales { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
