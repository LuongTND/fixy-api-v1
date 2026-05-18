using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Hubs
{
    [Authorize]
    public class BookingHub : Hub
    {
        private readonly ILogger<BookingHub> _logger;

        public BookingHub(ILogger<BookingHub> logger)
        {
            _logger = logger;
        }

        public async Task JoinBookingGroup(string bookingId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(bookingId));

            _logger.LogInformation("Connection {ConnectionId} joined booking group {BookingId}",Context.ConnectionId,bookingId);
        }

        public async Task LeaveBookingGroup(string bookingId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(bookingId));

            _logger.LogInformation("Connection {ConnectionId} left booking group {BookingId}",Context.ConnectionId,bookingId);
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected to BookingHub. ConnectionId: {ConnectionId}, UserId: {UserId}",Context.ConnectionId,Context.UserIdentifier);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected from BookingHub. ConnectionId: {ConnectionId}",Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
        public static string GetGroupName(string bookingId) => $"booking:{bookingId}";
    }
}
