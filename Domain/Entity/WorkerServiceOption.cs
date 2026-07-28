using Domain.Common;

namespace Domain.Entity
{
    public class WorkerServiceOption : BaseEntity
    {
        public Guid WorkerServiceId { get; set; }
        public int DurationMinutes { get; set; }
        public long Price { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public WorkerService? WorkerService { get; set; }
    }
}
