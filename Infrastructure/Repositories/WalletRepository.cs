using Application.Interfaces.Repositories;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WalletRepository : Repository<Wallet>, IWalletRepository
    {
        public WalletRepository(AppDbContext context)
            : base(context) { }

        public async Task<Wallet?> GetByUserIdAsync(
            Guid userId,
            WalletOwnerType type,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet.FirstOrDefaultAsync(
                x => x.UserId == userId && x.OwnerType == type,
                cancellationToken
            );
        }

        public async Task<Wallet?> GetWithTransactionsAsync(
            Guid walletId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                .Include(x => x.Transactions)
                .FirstOrDefaultAsync(x => x.Id == walletId, cancellationToken);
        }
    }
}
