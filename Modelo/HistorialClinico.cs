using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GestionAgenda.Modelo
{
    public class HistorialClinico
    {
        [Key]
        public int id_hc {  get; set; }
        public Paciente paciente { get; set; }
    }
}
