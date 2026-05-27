using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class VoucherCampaignRepository : Repository<VoucherCampaign>, IVoucherCampaignRepository
    {
        public VoucherCampaignRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<VoucherCampaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Vouchers)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<(List<VoucherCampaign> Items, int TotalCount)> GetPagedAsync(
            string? searchTerm,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.Name.Contains(searchTerm) ||
                    (c.Description != null && c.Description.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.Vouchers)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
