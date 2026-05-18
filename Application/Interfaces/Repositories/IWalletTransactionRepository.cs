using Application.Common;
using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IWalletTransactionRepository : IRepository<WalletTransaction>
    {
        Task<bool> ExistsExternalTransactionAsync(
            string externalId,
            CancellationToken cancellationToken = default
        );

        Task<List<WalletTransaction>> GetRecentByWalletIdAsync(
            Guid walletId,
            int take = 10,
            CancellationToken cancellationToken = default
        );

        Task<(List<WalletTransaction>, int)> GetPagedByWalletIdAsync(
            Guid walletId,
            PagedQuery query,
            CancellationToken cancellationToken = default
        );
    }
}
