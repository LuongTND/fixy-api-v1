using System.Threading.Channels;
using Application.Common.Models.Email;
using Application.Interfaces.Services.Email;

namespace Infrastructure.Services.Email
{
    public class EmailQueue : IEmailQueue
    {
        private readonly Channel<EmailMessage> _queue;

        public EmailQueue()
        {
            _queue = Channel.CreateUnbounded<EmailMessage>();
        }

        public async ValueTask QueueEmailAsync(EmailMessage message)
        {
            await _queue.Writer.WriteAsync(message);
        }

        public async ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
