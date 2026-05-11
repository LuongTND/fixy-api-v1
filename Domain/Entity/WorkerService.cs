using Domain.Common;

namespace Domain.Entity
{
    public class WorkerService : BaseEntity
    {
        public Guid WorkerId { get; set; }
        public Guid CategoryId { get; set; }
        public long BasePrice { get; set; }
        public bool IsPrimary { get; set; }

        public WorkerProfile? Worker { get; set; }
        public ServiceCategory? Category { get; set; }
    }
}
