using System.ComponentModel.DataAnnotations;

namespace GestionAgenda.Modelo
{
    public class Cita
    {
        [Key]
        public int id_cita { get; set; }
        public int idHistorialClinico { get; set; }
        public HistorialClinico hc { get; set; }
        public int idProfesional { get; set; }
        public Profesional profesional { get; set; }
        public DateTime fechaAgendado { get; set; }
        public DateTime fechaRealizada { get; set; }
        public String comentario { get; set; }
    }
}
