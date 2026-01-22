namespace GestionAgenda.DTOs
{
    public class PreconfirmacionDto
    {
        public PacienteDTO Paciente { get; set; }
        public DateOnly Fecha { get; set; }
        public TimeOnly Hora { get; set; }
    }
}
