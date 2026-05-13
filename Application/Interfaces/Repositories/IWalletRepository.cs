using Domain.Entity;
using Domain.Enum;

namespace Application.Interfaces.Repositories
{
    public interface IWalletRepository : IRepository<Wallet>
    {
        Task<Wallet?> GetByUserIdAsync(
            Guid userId,
            WalletOwnerType type,
            CancellationToken cancellationToken = default
        );

        Task<Wallet?> GetWithTransactionsAsync(
            Guid walletId,
            CancellationToken cancellationToken = default
        );
    }
}
