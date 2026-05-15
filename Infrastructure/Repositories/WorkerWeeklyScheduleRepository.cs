using Application.Interfaces.Repositories;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WorkerWeeklyScheduleRepository
        : Repository<WorkerWeeklySchedule>,
            IWorkerWeeklyScheduleRepository
    {
        public WorkerWeeklyScheduleRepository(AppDbContext context)
            : base(context) { }

        public async Task<bool> AnyByWorkerAsync(
            Guid workerProfileId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet.AnyAsync(
                x => x.WorkerProfileId == workerProfileId,
                cancellationToken
            );
        }

        public async Task<List<WorkerWeeklySchedule>> GetByWorkerProfileIdAsync(
            Guid workerProfileId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                .Where(x => x.WorkerProfileId == workerProfileId)
                .OrderBy(x => x.DayOfWeek)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<WorkerWeeklySchedule?> GetByWorkerAndDayAsync(
            Guid workerProfileId,
            WeekDay dayOfWeek,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet.FirstOrDefaultAsync(
                x => x.WorkerProfileId == workerProfileId && x.DayOfWeek == dayOfWeek,
                cancellationToken
            );
        }
    }
}
