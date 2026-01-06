namespace GestionAgenda.DTOs
{
    public class BloqueoHorarioDTO
    {
        public DateTimeOffset FechaDesde { get; set; }
        public DateTimeOffset FechaHasta { get; set; }
        public string? Motivo { get; set; }
    }
}
