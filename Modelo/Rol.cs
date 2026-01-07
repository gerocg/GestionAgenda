using System.ComponentModel.DataAnnotations;

namespace GestionAgenda.Modelo
{
    public class Rol
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        public ICollection<UsuarioRol> UsuarioRoles { get; set; }
    }
}
