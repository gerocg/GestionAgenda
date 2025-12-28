using System.ComponentModel.DataAnnotations;

namespace GestionAgenda.Modelo
{
    public class Recordatorio
    {
        [Key]
        public int Id {  get; set; }
        public Paciente Paciente {  get; set; }
        public Profesional Profesional {  get; set; }
        public string Mensaje { get; set; }
        public DateTime FechaEntrega { get; set; }
    }
}
