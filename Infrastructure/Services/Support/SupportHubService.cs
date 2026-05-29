using Application.DTOs.Support;
using Application.Interfaces.Hubs;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services.Support
{
    public class SupportHubService : ISupportHubService
    {
        private readonly IHubContext<SupportHub> _hubContext;
        private readonly ILogger<SupportHubService> _logger;

        public SupportHubService(IHubContext<SupportHub> hubContext, ILogger<SupportHubService> logger)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendSupportMessageAsync(Guid ticketId, SupportMessageDto dto, CancellationToken cancellationToken = default)
        {
            var groupName = SupportHub.GetGroupName(ticketId.ToString());

            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveSupportMessage", dto, cancellationToken);

            _logger.LogInformation("Sent support message for ticket {TicketId} from sender {SenderId}", ticketId, dto.SenderId);
        }
    }
}
