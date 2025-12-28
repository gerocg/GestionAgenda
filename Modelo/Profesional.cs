using System.ComponentModel.DataAnnotations;

namespace GestionAgenda.Modelo
{
    public class Profesional
    {
        [Key]
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        public string Profesion { get; set; }
    }
}
