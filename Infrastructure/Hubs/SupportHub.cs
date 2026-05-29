using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Infrastructure.Hubs
{
    [Authorize]
    public class SupportHub : Hub
    {
        public async Task JoinTicketGroup(string ticketId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(ticketId));
        }

        public async Task LeaveTicketGroup(string ticketId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(ticketId));
        }

        public static string GetGroupName(string ticketId) => $"support:{ticketId}";
    }
}
