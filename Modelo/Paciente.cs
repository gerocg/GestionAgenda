using System.ComponentModel.DataAnnotations;

namespace GestionAgenda.Modelo
{
    public class Paciente
    {
        [Key]
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public HistorialClinico HistorialClinico { get; set; } = new HistorialClinico();
    }
}
