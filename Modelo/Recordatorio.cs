using System.ComponentModel.DataAnnotations;

namespace GestionAgenda.Modelo
{
    public class Recordatorio
    {
        [Key]
        public int id_recordatorio {  get; set; }
        public Paciente paciente {  get; set; }
        public Profesional profesional {  get; set; }
        public String mensaje { get; set; }
        public DateTime fechaEntrega { get; set; }
    }
}
