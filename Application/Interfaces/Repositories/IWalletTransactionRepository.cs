using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IWalletTransactionRepository : IRepository<WalletTransaction>
    {
        Task<bool> ExistsExternalTransactionAsync(
            string externalId,
            CancellationToken cancellationToken = default
        );

        Task<List<WalletTransaction>> GetByWalletIdAsync(
            Guid walletId,
            CancellationToken cancellationToken = default
        );

        Task<List<WalletTransaction>> GetByWalletAndDateAsync(
            Guid walletId,
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default
        );
    }
}
