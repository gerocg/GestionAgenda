using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GestionAgenda.Modelo
{
    public class Archivo
    {
        [Key]
        public int id_archivo {  get; set; }
        public Cita cita { get; set; }
        public String rutaArchivo { get; set; }
    }
}
