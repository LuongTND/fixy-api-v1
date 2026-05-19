using Domain.Enum;

namespace Application.DTOs.WorkerProfile.WorkerSchedule
{
    public class UpdateWeeklyScheduleRequestDto
    {
        public WeekDay DayOfWeek { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public bool IsActive { get; set; }
    }
}
