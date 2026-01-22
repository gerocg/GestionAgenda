namespace GestionAgenda.DTOs
{
    public class CitaReporteDTO
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public int DuracionMinutos { get; set; }
        public string Estado { get; set; }
        public string NombrePaciente { get; set; }
        public string Tratamiento { get; set; }
        public string Observaciones { get; set; }
    }

    public class ResumenReporteCitasDTO
    {
        public int Total { get; set; }
        public int Realizadas { get; set; }
        public int Canceladas { get; set; }
        public int Inasistencias { get; set; }
    }

}
