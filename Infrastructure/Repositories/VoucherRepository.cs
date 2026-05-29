using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class VoucherRepository : Repository<Voucher>, IVoucherRepository
    {
        public VoucherRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<Voucher?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(v => v.Quota)
                .Include(v => v.Restrictions)
                .Include(v => v.Campaign)
                .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        }

        public async Task<Voucher?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(v => v.Quota)
                .Include(v => v.Restrictions)
                .Include(v => v.Campaign)
                .FirstOrDefaultAsync(v => v.Code == code, cancellationToken);
        }

        public async Task<bool> IsCodeExistsAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(v => v.Code == code, cancellationToken);
        }

        public async Task<(List<Voucher> Items, int TotalCount)> GetPagedAsync(
            string? searchTerm,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(v =>
                    v.Code.Contains(searchTerm) ||
                    (v.Description != null && v.Description.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(v => v.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(v => v.Quota)
                .Include(v => v.Restrictions)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<List<Voucher>> GetActiveVouchersAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow.AddHours(7); // Vietnamese Time (GMT+7)
            return await _dbSet
                .Where(v => v.Status == Domain.Enum.VoucherStatus.Active 
                            && v.IsDeleted == false 
                            && v.StartsAt <= now 
                            && v.ExpiresAt >= now
                            && (v.CampaignId == null || (v.Campaign != null 
                                                         && v.Campaign.Status == Domain.Enum.CampaignStatus.Active 
                                                         && v.Campaign.StartsAt <= now 
                                                         && v.Campaign.ExpiresAt >= now)))
                .Include(v => v.Quota)
                .Include(v => v.Restrictions)
                .Include(v => v.Campaign)
                .ToListAsync(cancellationToken);
        }
    }
}
