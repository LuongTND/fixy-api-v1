using Domain.Common;

namespace Domain.Entity
{
    public class Review : BaseAuditableEntity
    {
        public Guid BookingId { get; set; }
        public Guid CustomerProfileId { get; set; }
        public Guid WorkerProfileId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? WorkerReply { get; set; }
        public DateTime? RepliedAt { get; set; }
        public bool IsVisible { get; set; } = true;

        public Booking? Booking { get; set; }
        public CustomerProfile? Customer { get; set; }
        public WorkerProfile? Worker { get; set; }
    }
}
