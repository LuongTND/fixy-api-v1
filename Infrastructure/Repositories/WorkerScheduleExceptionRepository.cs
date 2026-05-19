using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class WorkerScheduleExceptionRepository
        : Repository<WorkerScheduleException>,
            IWorkerScheduleExceptionRepository
    {
        public WorkerScheduleExceptionRepository(AppDbContext context)
            : base(context) { }

        public async Task<List<WorkerScheduleException>> GetByWorkerProfileIdAsync(
            Guid workerProfileId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                .Where(x => x.WorkerProfileId == workerProfileId)
                .OrderByDescending(x => x.Date)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<WorkerScheduleException?> GetByWorkerAndDateAsync(
            Guid workerProfileId,
            DateOnly date,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet.FirstOrDefaultAsync(
                x => x.WorkerProfileId == workerProfileId && x.Date == date,
                cancellationToken
            );
        }

        public async Task<List<WorkerScheduleException>> GetByDateRangeAsync(
            Guid workerProfileId,
            DateOnly fromDate,
            DateOnly toDate,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                .Where(x =>
                    x.WorkerProfileId == workerProfileId && x.Date >= fromDate && x.Date <= toDate
                )
                .OrderBy(x => x.Date)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
