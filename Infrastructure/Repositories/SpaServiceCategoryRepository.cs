using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SpaServiceCategoryRepository : Repository<SpaServiceCategory>, ISpaServiceCategoryRepository
    {
        public SpaServiceCategoryRepository(AppDbContext context)
            : base(context) { }

        public async Task<List<SpaServiceCategory>> GetActiveCategoriesWithSpaCountAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.SpaPartnerServices)
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
