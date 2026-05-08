using Domain.Common;

namespace Domain.Entity
{
    public class WorkerServiceArea : BaseEntity
    {
        public Guid WorkerId { get; set; }
        public string Province { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;

        public WorkerProfile? Worker { get; set; }
    }
}
