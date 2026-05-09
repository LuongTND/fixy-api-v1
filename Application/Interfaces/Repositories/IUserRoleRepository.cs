using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IUserRoleRepository : IRepository<UserRole>
    {
        Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    }
}
