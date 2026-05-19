using Domain.Common;

namespace Domain.Entity
{
    public class WorkerScheduleException : BaseEntity
    {
        public Guid WorkerProfileId { get; set; }

        public DateOnly Date { get; set; }

        // nghỉ cả ngày
        public bool IsDayOff { get; set; }

        // custom giờ riêng ngày đó
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        public string? Reason { get; set; }

        public WorkerProfile? WorkerProfile { get; set; }
    }
}
