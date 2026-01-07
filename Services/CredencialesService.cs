using GestionAgenda.Modelo;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using MimeKit;
using System.Security.Cryptography;

namespace GestionAgenda.Services
{
    public class CredencialesService
    {
        private readonly IConfiguration _config;

        public CredencialesService(IConfiguration config)
        {
            _config = config;
        }
        public string GenerarContraseniaTemporal(int length = 10)
        {
            const string valid = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789@$";
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);

            return new string(bytes.Select(b => valid[b % valid.Length]).ToArray());
        }

        public void SetPassword(Usuario usuario, string password)
        {
            var hasher = new PasswordHasher<Usuario>();
            usuario.PasswordHash = hasher.HashPassword(usuario, password);
        }

        public async Task EnviarContraseniaTemporal(string email, string nombre, string password)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Gestion Agenda", "no-reply@tuapp.com"));
            message.To.Add(new MailboxAddress(nombre, email));
            message.Subject = "Acceso a Gestion Agenda";

            message.Body = new TextPart("html")
            {
                Text = $"""
                        <h3>Hola {nombre}</h3>
                        <p>Tu contraseña temporal es:</p>
                        <h2>{password}</h2>
                        <p>Ingresá al sistema y se te pedirá que la cambies.</p>
                    """
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                "smtp.gmail.com",
                587,
                SecureSocketOptions.StartTls
            );
            await smtp.AuthenticateAsync(
               _config["Smtp:User"],
               _config["Smtp:Pass"]
            );
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
    }
}

