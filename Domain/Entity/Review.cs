using Domain.Common;

namespace Domain.Entity
{
    public class Review : BaseAuditableEntity
    {
        // =========================
        // Ownership
        // =========================

        public Guid BookingId { get; set; }

        public Guid CustomerProfileId { get; set; }

        public Guid WorkerProfileId { get; set; }

        // =========================
        // Review Content
        // =========================

        public int Rating { get; set; }

        public string? Comment { get; set; }

        // =========================
        // Worker Reply
        // =========================

        public string? WorkerReply { get; set; }

        public DateTime? RepliedAt { get; set; }

        // =========================
        // Visibility
        // =========================

        public bool IsVisible { get; set; } = true;

        // =========================
        // Navigation
        // =========================

        public Booking? Booking { get; set; }

        public CustomerProfile? CustomerProfile { get; set; }

        public WorkerProfile? WorkerProfile { get; set; }
    }
}
