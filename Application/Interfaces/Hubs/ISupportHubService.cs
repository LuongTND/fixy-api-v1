using Application.DTOs.Support;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Hubs
{
    public interface ISupportHubService
    {
        Task SendSupportMessageAsync(Guid ticketId, SupportMessageDto dto, CancellationToken cancellationToken = default);
    }
}
