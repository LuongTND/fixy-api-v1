using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class WorkerMatchingQueue : BaseEntity
    {
        public Guid BookingId { get; set; }
        public Guid WorkerProfileId { get; set; }
        public int AttemptNo { get; set; } = 1;
        public MatchingStatus Status { get; set; }
        public double? DistanceKm { get; set; }
        public double? Score { get; set; }
        public DateTime? OfferedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? RejectReason { get; set; }

        public Booking? Booking { get; set; }
        public WorkerProfile? Worker { get; set; }
    }
}
