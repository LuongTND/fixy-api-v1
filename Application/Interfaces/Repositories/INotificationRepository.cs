using Application.Interfaces.Repositories;
using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<(List<Notification> Items, int TotalCount)> GetPagedByUserIdAsync(Guid userId,int page,int pageSize,bool? isRead = null,CancellationToken cancellationToken = default);

        Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
