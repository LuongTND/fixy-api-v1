using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
        Task<RefreshToken?> GetByTokenHashWithUserSessionAsync(string tokenHash, CancellationToken cancellationToken = default);
        Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
