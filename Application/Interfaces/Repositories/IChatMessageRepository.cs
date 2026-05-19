using Application.Common;
using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IChatMessageRepository : IRepository<ChatMessage>
    {
        Task<(List<ChatMessage> Items, int TotalCount)> GetChatHistoryAsync(Guid bookingId, PagedQuery query, CancellationToken cancellationToken = default);
        Task MarkMessagesAsReadAsync(Guid bookingId, Guid currentUserId, CancellationToken cancellationToken = default);
    }
}
