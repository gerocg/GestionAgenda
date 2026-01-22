namespace GestionAgenda.DTOs
{
    public class EstadoChatDTO
    {
        public long chatId { get; set; }
        public String? telefono { get; set; }
        public DateOnly? fecha { get; set; }
        public TimeOnly? hora { get; set; }
        public DateTime updatedAt { get; set; }
    }
}
