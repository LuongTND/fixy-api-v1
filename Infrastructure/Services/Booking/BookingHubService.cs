using Application.DTOs.Booking;
using Application.Interfaces.Hubs;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Booking
{
    public class BookingHubService : IBookingHubService
    {
        private readonly IHubContext<BookingHub> _hubContext;
        private readonly ILogger<BookingHubService> _logger;

        public BookingHubService(IHubContext<BookingHub> hubContext, ILogger<BookingHubService> logger)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Tracking status real-time updates for a booking
        public async Task SendStatusUpdateAsync(Guid bookingId,BookingStatusUpdateDto dto,CancellationToken cancellationToken = default)
        {
            var groupName = BookingHub.GetGroupName(bookingId.ToString());

            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveStatusUpdate", dto, cancellationToken);

            _logger.LogInformation("Sent status update for booking {BookingId}: {Status}",bookingId,dto.Status);
        }

        // Tracking worker location real-time updates for a booking
        public async Task SendLocationUpdateAsync(Guid bookingId,WorkerLocationUpdateDto dto,CancellationToken cancellationToken = default)
        {
            var groupName = BookingHub.GetGroupName(bookingId.ToString());

            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveLocationUpdate", dto, cancellationToken);

            _logger.LogDebug("Sent location update for booking {BookingId}: ({Lat}, {Lng})",bookingId,dto.Lat,dto.Lng);
        }
    }
}
