namespace GestionAgenda.Modelo
{
    public class ConfiguracionCalendario
    {
            public int Id { get; set; }
            public TimeSpan HoraInicio { get; set; }
            public TimeSpan HoraFin { get; set; }
            public int IntervaloBase { get; set; }
            public int DuracionCita { get; set; }
    }
}
