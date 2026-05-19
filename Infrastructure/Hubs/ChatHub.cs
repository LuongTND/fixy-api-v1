using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public async Task JoinChatGroup(string bookingId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(bookingId));
        }

        public async Task LeaveChatGroup(string bookingId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(bookingId));
        }

        public static string GetGroupName(string bookingId) => $"chat:{bookingId}";
    }
}
