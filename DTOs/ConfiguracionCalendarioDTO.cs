namespace GestionAgenda.DTOs
{
    public class ConfiguracionCalendarioDTO
    {
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public int IntervaloBase { get; set; }
        public int DuracionCita { get; set; }
    }
}
