using System.ComponentModel.DataAnnotations;

namespace GestionAgenda.DTOs
{
    public class PacienteDTO
    {
        public int Id { get; set; }
        public String NombreCompleto { get; set; }
        public String Email { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
    }
}
