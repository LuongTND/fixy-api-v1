using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class WorkerFeaturedOrder : BaseEntity
    {
        public Guid WorkerId { get; set; }
        public FeaturedPackage PackageName { get; set; }
        public int DurationDays { get; set; }
        public long Amount { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public string? PaymentRef { get; set; }

        public WorkerProfile? Worker { get; set; }
    }
}
