using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WorkerPayoutAccountRepository
        : Repository<WorkerPayoutAccount>,
            IWorkerPayoutAccountRepository
    {
        public WorkerPayoutAccountRepository(AppDbContext context)
            : base(context) { }

        public async Task<List<WorkerPayoutAccount>> GetByWorkerIdAsync(
            Guid workerId,
            CancellationToken cancellationToken
        )
        {
            return await _dbSet
                .Where(x => x.WorkerId == workerId)
                .OrderByDescending(x => x.IsDefault)
                .ToListAsync(cancellationToken);
        }

        public async Task<WorkerPayoutAccount?> GetDefaultAsync(
            Guid workerId,
            CancellationToken cancellationToken
        )
        {
            return await _dbSet.FirstOrDefaultAsync(
                x => x.WorkerId == workerId && x.IsDefault,
                cancellationToken
            );
        }
    }
}
