using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default);

        Task<User?> GetByIdWithProfilesAsync(Guid id, CancellationToken ct = default);

        Task<User?> GetByTargetAsync(string target, CancellationToken ct = default);

        Task<User?> GetByTargetWithRoleAsync(string target, CancellationToken ct = default);

        Task<User?> GetWithCustomerProfileByIdAsync(Guid userId, CancellationToken ct = default);

        Task<User?> GetWithWorkerProfileByIdAsync(Guid userId, CancellationToken ct = default);
    }
}
