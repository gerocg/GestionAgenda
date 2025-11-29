namespace GestionAgenda.DTOs
{
    public class RegistroDTO
    {
        public String nombre_completo_paciente { get; set; }
        public String contrasenia_paciente { get; set; }
        public String email { get; set; }
        public DateTime fecha_nacimiento { get; set; }
        public String telefono { get; set; }
    }
}
