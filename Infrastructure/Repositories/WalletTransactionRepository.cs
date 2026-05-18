using Application.Common;
using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WalletTransactionRepository
        : Repository<WalletTransaction>,
            IWalletTransactionRepository
    {
        public WalletTransactionRepository(AppDbContext context)
            : base(context) { }

        public async Task<bool> ExistsExternalTransactionAsync(
            string externalId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet.AnyAsync(
                x => x.ExternalTransactionId == externalId,
                cancellationToken
            );
        }

        public async Task<List<WalletTransaction>> GetRecentByWalletIdAsync(
            Guid walletId,
            int take = 10,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                .Where(x => x.WalletId == walletId)
                .OrderByDescending(x => x.CreatedDate)
                .Take(take)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<WalletTransaction>, int)> GetPagedByWalletIdAsync(
            Guid walletId,
            PagedQuery query,
            CancellationToken cancellationToken = default
        )
        {
            var transactions = _dbSet.Where(x => x.WalletId == walletId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                transactions = transactions.Where(x =>
                    (
                        x.ExternalTransactionId != null
                        && x.ExternalTransactionId.Contains(query.SearchTerm)
                    ) || (x.ReferenceId != null && x.ReferenceId.Contains(query.SearchTerm))
                );
            }

            transactions = query.SortDescending
                ? transactions.OrderByDescending(x => x.CreatedDate)
                : transactions.OrderBy(x => x.CreatedDate);

            var totalCount = await transactions.CountAsync(cancellationToken);

            var items = await transactions
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
