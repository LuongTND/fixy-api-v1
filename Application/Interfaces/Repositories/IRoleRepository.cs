using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
        Task<Role> GetCustomerRoleAsync(CancellationToken ct);
    }
}
