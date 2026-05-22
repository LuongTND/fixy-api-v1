using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class VoucherUsageHistoryRepository : Repository<VoucherUsageHistory>, IVoucherUsageHistoryRepository
    {
        public VoucherUsageHistoryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<int> GetUsageCountByUserAsync(
            Guid voucherId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(
                h => h.VoucherId == voucherId && h.UserId == userId && h.IsSuccess,
                cancellationToken);
        }

        public async Task<List<VoucherUsageHistory>> GetByVoucherIdAsync(
            Guid voucherId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(h => h.VoucherId == voucherId)
                .OrderByDescending(h => h.AppliedAt)
                .Include(h => h.User)
                .Include(h => h.Booking)
                .ToListAsync(cancellationToken);
        }
    }
}
