using Application.Common.Models.Email;

namespace Application.Interfaces.Services.Email
{
    public interface IEmailQueue
    {
        ValueTask QueueEmailAsync(EmailMessage message);

        ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken);
    }
}
