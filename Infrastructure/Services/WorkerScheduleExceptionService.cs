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
        private readonly IWorkerProfileRepository _workerProfileRepository;
        private readonly IUnitOfWork _unitOfWork;

        public WorkerScheduleExceptionService(
            IWorkerScheduleExceptionRepository workerScheduleExceptionRepository,
            IWorkerWeeklyScheduleRepository workerWeeklyScheduleRepository,
            IWorkerProfileRepository workerProfileRepository,
            IUnitOfWork unitOfWork
        )
        {
            _workerScheduleExceptionRepository = workerScheduleExceptionRepository;
            _workerWeeklyScheduleRepository = workerWeeklyScheduleRepository;
            _workerProfileRepository = workerProfileRepository;
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
            string? reason = null,
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
            // 0. Priority 1 (Realtime Working Status override)
            var worker = await _workerProfileRepository.GetByIdAsync(workerProfileId, cancellationToken);
            if (worker == null)
            {
                return OperationResult<bool>.Failure("Worker profile not found");
            }
            if (worker.IsBusy)
            {
                return OperationResult<bool>.Success(false, "Kỹ thuật viên hiện đang bận trong ca làm việc khác.");
            }

            if (!worker.IsOnline || !worker.IsAcceptingJobs)
            {
                // If worker profile is approved but IsOnline and IsAcceptingJobs were uninitialized (both false), auto-enable them
                if (worker.Status == WorkerStatus.Approved && !worker.IsOnline && !worker.IsAcceptingJobs)
                {
                    worker.IsOnline = true;
                    worker.IsAcceptingJobs = true;
                    _workerProfileRepository.Update(worker);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    return OperationResult<bool>.Success(false, "Kỹ thuật viên hiện đang tắt trạng thái nhận việc.");
                }
            }
            // Convert bookingTime to local Vietnam time (+07:00) before extracting date and time
            var bookingTimeUtc = bookingTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(bookingTime, DateTimeKind.Utc)
                : bookingTime.ToUniversalTime();
            var localZone = TimeZoneInfo.FindSystemTimeZoneById(
                System.OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh"
            );
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(bookingTimeUtc, localZone);

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
                    return OperationResult<bool>.Success(false, "Kỹ thuật viên nghỉ làm vào ngày này");
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
                            ? "Kỹ thuật viên khả dụng"
                            : "Kỹ thuật viên đang ngoài khung giờ làm việc đặc biệt"
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
                return OperationResult<bool>.Failure("Chưa thiết lập lịch làm việc cho Kỹ thuật viên");
            }

            if (!schedule.IsActive)
            {
                return OperationResult<bool>.Success(false, "Kỹ thuật viên không làm việc vào ngày này");
            }

            if (schedule.StartTime == null || schedule.EndTime == null)
            {
                return OperationResult<bool>.Failure("Khung giờ làm việc không hợp lệ");
            }

            var currentTime = TimeOnly.FromDateTime(localTime);

            var isAvailable = currentTime >= schedule.StartTime && currentTime <= schedule.EndTime;

            return OperationResult<bool>.Success(
                isAvailable,
                isAvailable ? "Kỹ thuật viên khả dụng" : "Kỹ thuật viên đang ngoài khung giờ làm việc"
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
