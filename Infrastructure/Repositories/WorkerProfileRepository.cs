using Application.Common;
using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WorkerProfileRepository : Repository<WorkerProfile>, IWorkerProfileRepository
    {
        public WorkerProfileRepository(AppDbContext context)
            : base(context) { }

        public async Task<(
            List<WorkerProfile> Items,
            int TotalCount
        )> GetPagedWorkerRegisterRequestAsync(
            PagedQuery query,
            CancellationToken cancellationToken = default
        )
        {
            var queryDb = _dbSet.Include(x => x.User).AsNoTracking();

            var totalCount = await queryDb.CountAsync(cancellationToken);

            var items = await queryDb
                .OrderByDescending(x => x.CreatedDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<WorkerProfile?> GetWorkerProfileDetailByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                .Include(x => x.User)
                .Include(x => x.Certificates)
                .Include(x => x.Services)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
