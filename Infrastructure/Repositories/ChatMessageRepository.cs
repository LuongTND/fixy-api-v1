using Application.Common;
using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ChatMessageRepository : Repository<ChatMessage>, IChatMessageRepository
    {
        public ChatMessageRepository(AppDbContext context)
            : base(context) { }

        public async Task<(List<ChatMessage> Items, int TotalCount)> GetChatHistoryAsync(
            Guid bookingId,
            PagedQuery query,
            CancellationToken cancellationToken = default
        )
        {
            var totalCount = await _dbSet.CountAsync(
                m => m.BookingId == bookingId,
                cancellationToken
            );
            var skip = (query.PageNumber - 1) * query.PageSize;

            var messages = await _dbSet
                .Include(m => m.Sender)
                .Include(m => m.Booking)
                    .ThenInclude(b => b!.WorkerProfile)
                .Where(m => m.BookingId == bookingId)
                .OrderByDescending(m => m.CreatedDate)
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return (messages, totalCount);
        }

        public async Task MarkMessagesAsReadAsync(
            Guid bookingId,
            Guid currentUserId,
            CancellationToken cancellationToken = default
        )
        {
            var unreadMessages = await _dbSet
                .Where(m => m.BookingId == bookingId && m.SenderId != currentUserId && !m.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }
        }
    }
}
