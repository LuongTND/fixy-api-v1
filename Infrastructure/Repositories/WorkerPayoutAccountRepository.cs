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
                .Include(x => x.WorkerProfile)
                .Where(x => x.WorkerProfile!.UserId == workerId)
                .OrderByDescending(x => x.IsDefault)
                .ToListAsync(cancellationToken);
        }

        public async Task<WorkerPayoutAccount?> GetDefaultAsync(
            Guid workerId,
            CancellationToken cancellationToken
        )
        {
            return await _dbSet
                .Include(x => x.WorkerProfile)
                .FirstOrDefaultAsync(
                    x => x.WorkerProfile!.UserId == workerId && x.IsDefault,
                    cancellationToken
                );
        }
    }
}
