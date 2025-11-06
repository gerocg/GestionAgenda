using System.ComponentModel.DataAnnotations;

namespace GestionAgenda.Modelo
{
    public class Paciente
    {
        [Key]
        public String usuario_paciente { get; set; }
        public String contrasenia_paciente { get; set; }
        public String nombre_completo_paciente { get; set; }
        public String email { get; set; }
        public String direccion { get; set; }
        public DateTime fecha_nacimiento { get; set; }
        public ICollection<Telefono> telefonos { get; set; }
    }
}
