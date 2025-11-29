using System.ComponentModel.DataAnnotations;

namespace GestionAgenda.Modelo
{
    public class Paciente
    {
        [Key]
        public int id_paciente { get; set; }
        public String usuario_paciente { get; set; }
        public String contrasenia_paciente { get; set; }
        public String nombre_completo_paciente { get; set; }
        public String email { get; set; }
        public DateTime fecha_nacimiento { get; set; }
        public String telefono { get; set; }
        public int historial_clinicoid_hc { get; set; }
        public HistorialClinico historial_clinico { get; set; }

        public Paciente() { 
            this.historial_clinico = new HistorialClinico();
        }
    }
}
