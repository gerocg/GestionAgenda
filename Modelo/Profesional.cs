using System.ComponentModel.DataAnnotations;

namespace GestionAgenda.Modelo
{
    public class Profesional
    {
        [Key]
        public String usuario_profesional {  get; set; }
        public String contrasenia_profesional { get; set; }
        public String nombre_completo_profesional { get; set; }
        public String profesion {  get; set; }
    }
}
