using Application.Interfaces.Services.Email;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Email
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IEmailQueue _emailQueue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmailBackgroundService> _logger;

        public EmailBackgroundService(
            IEmailQueue emailQueue,
            IServiceScopeFactory scopeFactory,
            ILogger<EmailBackgroundService> logger
        )
        {
            _emailQueue = emailQueue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var email = await _emailQueue.DequeueAsync(stoppingToken);

                    using var scope = _scopeFactory.CreateScope();

                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    await emailService.SendEmailAsync(email.To, email.Subject, email.Body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Send email failed");
                }
            }
        }
    }
}
