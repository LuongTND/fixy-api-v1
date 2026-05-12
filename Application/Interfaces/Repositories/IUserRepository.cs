using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByIdWithRoleAndAddressAsync(Guid id, CancellationToken ct = default);
        Task<User?> GetByTargetAsync(string target, CancellationToken ct = default);
        Task<User?> GetByTargetWithRoleAsync(string target, CancellationToken ct = default);
    }
}
