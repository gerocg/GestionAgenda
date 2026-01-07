using GestionAgenda.Modelo;

namespace GestionAgenda.DTOs
{
    public class CitaDTO
    {
        public int? PacienteId { get; set; }
        public DateTime FechaHora { get; set; }
        public int Duracion { get; set; }
        public string Tratamiento { get; set; }
        public string? Observaciones { get; set; }
    }
}
