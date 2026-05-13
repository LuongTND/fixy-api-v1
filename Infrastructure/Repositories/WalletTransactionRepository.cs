using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;

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
        return await _dbSet.AnyAsync(x => x.ExternalTransactionId == externalId, cancellationToken);
    }

    public async Task<List<WalletTransaction>> GetByWalletIdAsync(
        Guid walletId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet
            .Where(x => x.WalletId == walletId)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<WalletTransaction>> GetByWalletAndDateAsync(
        Guid walletId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet
            .Where(x => x.WalletId == walletId && x.CreatedDate >= from && x.CreatedDate <= to)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }
}
