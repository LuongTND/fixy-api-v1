using Application.Common;
using Domain.Entity;
using Domain.Enum;

namespace Application.Interfaces.Services
{
    public interface IWorkerWeeklyScheduleService
    {
        Task<OperationResult<List<WorkerWeeklySchedule>>> GetWeeklySchedulesAsync(
            Guid workerProfileId,
            CancellationToken cancellationToken = default
        );
        Task<OperationResult> CreateDefaultScheduleAsync(
            Guid workerProfileId,
            CancellationToken cancellationToken = default
        );

        Task<OperationResult> UpdateWeeklyScheduleAsync(
            Guid workerProfileId,
            WeekDay dayOfWeek,
            TimeOnly? startTime,
            TimeOnly? endTime,
            bool isActive,
            CancellationToken cancellationToken = default
        );
    }
}
