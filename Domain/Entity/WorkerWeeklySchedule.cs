using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class WorkerWeeklySchedule : BaseEntity
    {
        public Guid WorkerProfileId { get; set; }

        public WeekDay DayOfWeek { get; set; }

        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        public bool IsActive { get; set; } = true;

        public WorkerProfile? WorkerProfile { get; set; }
    }
}
