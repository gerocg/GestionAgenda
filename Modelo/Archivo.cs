using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GestionAgenda.Modelo
{
    public class Archivo
    {
        [Key]
        public int Id {  get; set; }
        public int CitaId { get; set; }
        public Cita Cita { get; set; }
        public string RutaArchivo { get; set; }
        public string NombreArchivo { get; set; }
        public string TipoArchivo { get; set; }
        public DateTime FechaSubida { get; set; }

    }
}
