using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IWorkerScheduleExceptionRepository : IRepository<WorkerScheduleException>
    {
        Task<List<WorkerScheduleException>> GetByWorkerProfileIdAsync(
            Guid workerProfileId,
            CancellationToken cancellationToken = default
        );

        Task<WorkerScheduleException?> GetByWorkerAndDateAsync(
            Guid workerProfileId,
            DateOnly date,
            CancellationToken cancellationToken = default
        );

        Task<List<WorkerScheduleException>> GetByDateRangeAsync(
            Guid workerProfileId,
            DateOnly fromDate,
            DateOnly toDate,
            CancellationToken cancellationToken = default
        );
    }
}
