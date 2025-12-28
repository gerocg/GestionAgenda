using System.ComponentModel.DataAnnotations;

namespace GestionAgenda.Modelo
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string NombreCompleto { get; set; }
        public Paciente? Paciente { get; set; }
        public Profesional? Profesional { get; set; }
        public List<UsuarioRol> UsuarioRoles { get; set; }
        public bool RequiereCambioContrasena { get; set; } = false;
    }
}
