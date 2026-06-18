using Application.DTOs.User;
using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<(List<User>, int)> GetPagedUsersAsync(
            UserManagementQuery query,
            CancellationToken cancellationToken
        );
        Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default);

        Task<User?> GetByIdWithProfilesAsync(Guid id, CancellationToken ct = default);

        Task<User?> GetByTargetAsync(string target, CancellationToken ct = default);

        Task<User?> GetByTargetWithRoleAsync(string target, CancellationToken ct = default);

        Task<User?> GetWithCustomerProfileByIdAsync(Guid userId, CancellationToken ct = default);

        Task<User?> GetWithWorkerProfileByIdAsync(Guid userId, CancellationToken ct = default);

        Task<User?> GetByOAuthIdAsync(Domain.Enum.OAuthProvider provider,string oauthId, CancellationToken ct = default);
    }
}
