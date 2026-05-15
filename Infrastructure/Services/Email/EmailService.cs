using Application.Interfaces.Services.Email;
using Application.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;

        public EmailService(IOptions<SmtpSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            Console.WriteLine("===== SMTP =====");
            Console.WriteLine($"Host: {_settings.Host}");
            Console.WriteLine($"Port: {_settings.Port}");
            Console.WriteLine($"User: {_settings.Username}");
            Console.WriteLine($"Pass: {_settings.Password}");
            Console.WriteLine("================");
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("Vua Tho", _settings.From));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();

            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(_settings.Username, _settings.Password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
