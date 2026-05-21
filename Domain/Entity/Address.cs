using Domain.Common;

namespace Domain.Entity
{
    public class Address : BaseEntity, ISoftDelete
    {
        // =========================
        // Ownership
        // =========================

        // Customer: multiple addresses
        public Guid? CustomerProfileId { get; set; }

        // Worker: single address
        public Guid? WorkerProfileId { get; set; }

        // =========================
        // Address Info
        // =========================

        public string? Label { get; set; }

        public string City { get; set; } = default!;

        public string District { get; set; } = default!;

        public string Ward { get; set; } = default!;

        public string Detail { get; set; } = default!;

        public double? Lat { get; set; }

        public double? Lng { get; set; }

        public bool IsDefault { get; set; }

        // =========================
        // Soft Delete
        // =========================

        public bool IsDeleted { get; set; }

        public DateTime? DeletedDate { get; set; }

        public string? DeletedBy { get; set; }

        // =========================
        // Navigation
        // =========================

        public CustomerProfile? CustomerProfile { get; set; }

        public WorkerProfile? WorkerProfile { get; set; }
    }
}
