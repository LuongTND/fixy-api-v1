using Domain.Common;

namespace Domain.Entity
{
    public class WorkerService : BaseEntity
    {
        public Guid WorkerProfileId { get; set; }
        public Guid CategoryId { get; set; }
        public long BasePrice { get; set; }
        public bool IsPrimary { get; set; }

        public int DurationMinutes { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public WorkerProfile? Worker { get; set; }
        public ServiceCategory? Category { get; set; }
    }
}
