using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GestionAgenda.Modelo
{
    public class HistorialClinico
    {
        [Key]
        public int id_hc {  get; set; }
        public ICollection<Cita> citas { get; set; } = new List<Cita>();
    }
}
