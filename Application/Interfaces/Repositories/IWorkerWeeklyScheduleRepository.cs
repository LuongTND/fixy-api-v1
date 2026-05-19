using Domain.Entity;
using Domain.Enum;

namespace Application.Interfaces.Repositories
{
    public interface IWorkerWeeklyScheduleRepository : IRepository<WorkerWeeklySchedule>
    {
        Task<bool> AnyByWorkerAsync(
            Guid workerProfileId,
            CancellationToken cancellationToken = default
        );
        Task<List<WorkerWeeklySchedule>> GetByWorkerProfileIdAsync(
            Guid workerProfileId,
            CancellationToken cancellationToken = default
        );

        Task<WorkerWeeklySchedule?> GetByWorkerAndDayAsync(
            Guid workerProfileId,
            WeekDay dayOfWeek,
            CancellationToken cancellationToken = default
        );
    }
}
