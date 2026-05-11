using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByTargetAsync(string target, CancellationToken ct = default);
        Task<User?> GetByTargetWithRoleAsync(string target, CancellationToken ct);
    }
}
