using GestionAgenda.Modelo;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace GestionAgenda.Services
{
    public class CitasEmailService
    {
        private readonly IConfiguration _config;

        public CitasEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarConfirmacion(Cita cita, string email, string nombre)
        {
            await EnviarMail(email, nombre,
                "Cita confirmada",
                $"""
                <h3>Hola {nombre}</h3>
                <p>Tu cita fue confirmada correctamente.</p>
                <ul>
                    <li><b>Fecha:</b> {cita.FechaAgendada:dd/MM/yyyy}</li>
                    <li><b>Hora:</b> {cita.FechaAgendada:HH:mm}</li>
                    <li><b>Duración:</b> {cita.DuracionMinutos} minutos</li>
                </ul>
                """
            );
        }

        public async Task EnviarModificacion(Cita cita, string email, string nombre)
        {
            await EnviarMail(email, nombre,
                "Cita modificada",
                $"""
                <h3>Hola {nombre}</h3>
                <p>Tu cita fue modificada.</p>
                <ul>
                    <li><b>Nueva fecha:</b> {cita.FechaAgendada:dd/MM/yyyy}</li>
                    <li><b>Nueva hora:</b> {cita.FechaAgendada:HH:mm}</li>
                </ul>
                """
            );
        }

        public async Task EnviarCancelacion(Cita cita, string email, string nombre)
        {
            await EnviarMail(email, nombre,
                "Cita cancelada",
                $"""
                <h3>Hola {nombre}</h3>
                <p>Su cita fue cancelada.</p>
                <p><b>Fecha:</b> {cita.FechaAgendada:dd/MM/yyyy}
                <p><b>Hora:</b> {cita.FechaAgendada:HH:mm}</p>
                """
            );
        }

        public async Task EnviarRecordatorio(Cita cita, string email, string nombre)
        {
            await EnviarMail(email, nombre,
                "Recordatorio de cita",
                $"""
                <h3>Hola {nombre}</h3>
                <p>Te recordamos que tienes una cita programada.</p>
                <ul>
                    <li><b>Fecha:</b> {cita.FechaAgendada:dd/MM/yyyy}</li>
                    <li><b>Hora:</b> {cita.FechaAgendada:HH:mm}</li>
                    <li><b>Duración:</b> {cita.DuracionMinutos} minutos</li>
                </ul>
                """
            );
        }

        private async Task EnviarMail(string email, string nombre, string asunto, string bodyHtml)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Gestión Agenda", "no-reply@tuapp.com"));
            message.To.Add(new MailboxAddress(nombre, email));
            message.Subject = asunto;

            message.Body = new TextPart("html") { Text = bodyHtml };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(
                _config["Smtp:User"],
                _config["Smtp:Pass"]
            );
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
    }
}
