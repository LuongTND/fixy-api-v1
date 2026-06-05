using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entity;
using Domain.Enum;

namespace Infrastructure.Services
{
    public class WorkerScheduleExceptionService : IWorkerScheduleExceptionService
    {
        private readonly IWorkerScheduleExceptionRepository _workerScheduleExceptionRepository;
        private readonly IWorkerWeeklyScheduleRepository _workerWeeklyScheduleRepository;
        private readonly IUnitOfWork _unitOfWork;

        public WorkerScheduleExceptionService(
            IWorkerScheduleExceptionRepository workerScheduleExceptionRepository,
            IWorkerWeeklyScheduleRepository workerWeeklyScheduleRepository,
            IUnitOfWork unitOfWork
        )
        {
            _workerScheduleExceptionRepository = workerScheduleExceptionRepository;
            _workerWeeklyScheduleRepository = workerWeeklyScheduleRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<
            OperationResult<List<WorkerScheduleException>>
        > GetScheduleExceptionsAsync(
            Guid workerProfileId,
            CancellationToken cancellationToken = default
        )
        {
            var exceptions = await _workerScheduleExceptionRepository.GetByWorkerProfileIdAsync(
                workerProfileId,
                cancellationToken
            );

            return OperationResult<List<WorkerScheduleException>>.Success(
                exceptions,
                "Get schedule exceptions successfully"
            );
        }

        public async Task<OperationResult> AddDayOffAsync(
            Guid workerProfileId,
            DateOnly date,
            string? reason,
            CancellationToken cancellationToken = default
        )
        {
            var localZone = TimeZoneInfo.FindSystemTimeZoneById(
                System.OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh"
            );
            var localToday = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localZone));
            if (date < localToday)
            {
                return OperationResult.Failure("Cannot add day off in the past");
            }

            var existed = await _workerScheduleExceptionRepository.GetByWorkerAndDateAsync(
                workerProfileId,
                date,
                cancellationToken
            );

            if (existed != null)
            {
                return OperationResult.Failure("Day off already exists");
            }

            var exception = new WorkerScheduleException
            {
                WorkerProfileId = workerProfileId,
                Date = date,
                IsDayOff = true,
                Reason = reason,
            };

            await _workerScheduleExceptionRepository.AddAsync(exception);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Add day off successfully");
        }

        public async Task<OperationResult<bool>> IsWorkerAvailableAsync(
            Guid workerProfileId,
            DateTime bookingTime,
            CancellationToken cancellationToken = default
        )
        {
            // Convert bookingTime to local Vietnam time (+07:00) before extracting date and time
            var utcTime = bookingTime.ToUniversalTime();
            var localZone = TimeZoneInfo.FindSystemTimeZoneById(
                System.OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh"
            );
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, localZone);

            var date = DateOnly.FromDateTime(localTime);

            var exception = await _workerScheduleExceptionRepository.GetByWorkerAndDateAsync(
                workerProfileId,
                date,
                cancellationToken
            );

            // nghỉ đặc biệt
            if (exception != null)
            {
                if (exception.IsDayOff)
                {
                    return OperationResult<bool>.Success(false, "Worker is on day off");
                }

                // custom giờ riêng
                if (exception.StartTime != null && exception.EndTime != null)
                {
                    var currentTimeEx = TimeOnly.FromDateTime(localTime);

                    var isAvailableEx =
                        currentTimeEx >= exception.StartTime && currentTimeEx <= exception.EndTime;

                    return OperationResult<bool>.Success(
                        isAvailableEx,
                        isAvailableEx
                            ? "Worker is available"
                            : "Worker is outside custom working hours"
                    );
                }
            }

            var dayOfWeek = localTime.DayOfWeek switch
            {
                DayOfWeek.Monday => WeekDay.Mon,
                DayOfWeek.Tuesday => WeekDay.Tue,
                DayOfWeek.Wednesday => WeekDay.Wed,
                DayOfWeek.Thursday => WeekDay.Thu,
                DayOfWeek.Friday => WeekDay.Fri,
                DayOfWeek.Saturday => WeekDay.Sat,
                _ => WeekDay.Sun,
            };

            var schedule = await _workerWeeklyScheduleRepository.GetByWorkerAndDayAsync(
                workerProfileId,
                dayOfWeek,
                cancellationToken
            );

            if (schedule == null)
            {
                return OperationResult<bool>.Failure("Schedule not found");
            }

            if (!schedule.IsActive)
            {
                return OperationResult<bool>.Success(false, "Worker is not working this day");
            }

            if (schedule.StartTime == null || schedule.EndTime == null)
            {
                return OperationResult<bool>.Failure("Schedule time is invalid");
            }

            var currentTime = TimeOnly.FromDateTime(localTime);

            var isAvailable = currentTime >= schedule.StartTime && currentTime <= schedule.EndTime;

            return OperationResult<bool>.Success(
                isAvailable,
                isAvailable ? "Worker is available" : "Worker is outside working hours"
            );
        }

        public async Task<OperationResult> RemoveDayOffAsync(
            Guid workerProfileId,
            DateOnly date,
            CancellationToken cancellationToken = default
        )
        {
            var exception = await _workerScheduleExceptionRepository.GetByWorkerAndDateAsync(
                workerProfileId,
                date,
                cancellationToken
            );

            if (exception == null)
            {
                return OperationResult.Failure("Day off not found");
            }

            _workerScheduleExceptionRepository.Remove(exception);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Remove day off successfully");
        }
    }
}
