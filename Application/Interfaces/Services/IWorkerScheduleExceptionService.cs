using Application.Common;
using Domain.Entity;

namespace Infrastructure.Services
{
    public interface IWorkerScheduleExceptionService
    {
        Task<OperationResult<List<WorkerScheduleException>>> GetScheduleExceptionsAsync(
            Guid workerProfileId,
            CancellationToken cancellationToken = default
        );
        Task<OperationResult> AddDayOffAsync(
            Guid workerProfileId,
            DateOnly date,
            string? reason,
            CancellationToken cancellationToken = default
        );

        Task<OperationResult> RemoveDayOffAsync(
            Guid workerProfileId,
            DateOnly date,
            CancellationToken cancellationToken = default
        );

        Task<OperationResult<bool>> IsWorkerAvailableAsync(
            Guid workerProfileId,
            DateTime bookingTime,
            CancellationToken cancellationToken = default
        );
    }
}
