using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WorkerServiceRepository : Repository<WorkerService>, IWorkerServiceRepository
    {
        public WorkerServiceRepository(AppDbContext context)
            : base(context) { }

        public async Task<WorkerService?> GetByWorkerAndCategoryWithOptionsAsync(
            Guid workerProfileId,
            Guid categoryId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                .Include(x => x.Options)
                .FirstOrDefaultAsync(
                    x => x.WorkerProfileId == workerProfileId && x.CategoryId == categoryId,
                    cancellationToken
                );
        }

        public async Task<WorkerService?> GetByCategoryWithOptionsAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                .Include(x => x.Options)
                .FirstOrDefaultAsync(
                    x => x.CategoryId == categoryId,
                    cancellationToken
                );
        }

        public async Task<(long? MinPrice, long? MaxPrice)> GetPriceRangeAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default
        )
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(x => x.CategoryId == categoryId)
                .Select(x => x.BasePrice);

            if (!await query.AnyAsync(cancellationToken))
            {
                return (null, null);
            }

            var minPrice = await query.MinAsync(cancellationToken);
            var maxPrice = await query.MaxAsync(cancellationToken);

            return (minPrice, maxPrice);
        }
    }
}

