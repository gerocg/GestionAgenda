using System.ComponentModel.DataAnnotations;

namespace GestionAgenda.Modelo
{
    public class Recordatorio
    {
        [Key]
        public int Id {  get; set; }
        public Paciente Paciente {  get; set; }
        public DateTime fechaEnvio {  get; set; }
        public string Mensaje { get; set; }
        public Cita cita { get; set; }
        public bool Enviado { get; set; }
        public DateTime FechaEnviado { get; set; }
        public long chatId { get; set; }
    }
}
