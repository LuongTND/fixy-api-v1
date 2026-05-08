using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class WorkerSchedule : BaseEntity
    {
        public Guid WorkerId { get; set; }
        public WeekDay DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string? BlockedDates { get; set; }

        public WorkerProfile? Worker { get; set; }
    }
}
