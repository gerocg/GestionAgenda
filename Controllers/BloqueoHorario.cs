using GestionAgenda.Modelo;

namespace GestionAgenda.Controllers
{
    public class BloqueoHorario
    {
        public int Id { get; set; }
        public DateTimeOffset FechaDesde { get; set; }
        public DateTimeOffset FechaHasta { get; set; }
        public string? Motivo { get; set; }
    }
}
