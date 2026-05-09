using System.Net;
using System.Net.Mail;
using Application.Interfaces.Services.Email;
using Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<SmtpSettings> settings, ILogger<SmtpEmailService> logger)
        {
            _settings = (settings ?? throw new ArgumentNullException(nameof(settings))).Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendOtpEmailAsync(string toEmail, string otp, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending OTP email. To={ToEmail}", toEmail);

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.User, _settings.Password)
            };

            var from = new MailAddress(_settings.FromEmail, _settings.FromName);
            var to = new MailAddress(toEmail);
            using var message = new MailMessage(from, to)
            {
                Subject = "Your OTP code",
                Body = $"Your OTP code is: {otp}. It expires in 3 minutes.",
                IsBodyHtml = false
            };

            using var registration = cancellationToken.Register(() => client.SendAsyncCancel());
            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("OTP email sent. To={ToEmail}", toEmail);
        }
    }
}
