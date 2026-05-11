using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class WorkerProfile : BaseEntity
    {
        public Guid UserId { get; set; }
        public string? Bio { get; set; }
        public int ExperienceYears { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public int MaxDistanceKm { get; set; }
        public WorkerStatus Status { get; set; } = WorkerStatus.Pending;
        public string? RejectReason { get; set; }
        public WorkerBadge Badge { get; set; } = WorkerBadge.New;
        public double RatingAvg { get; set; }
        public int TotalOrders { get; set; }
        public bool IsOnline { get; set; }
        public double? CurrentLat { get; set; }
        public double? CurrentLng { get; set; }
        public DateTime? LastLocationAt { get; set; }
        public DateTime? FeaturedUntil { get; set; }
        public FeaturedPackage? FeaturedPackage { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedById { get; set; }

        public User? User { get; set; }
        public User? ApprovedBy { get; set; }
        public Address? Address { get; set; }
        public ICollection<WorkerCertificate> Certificates { get; set; } =
            new List<WorkerCertificate>();
        public ICollection<WorkerServiceArea> ServiceAreas { get; set; } =
            new List<WorkerServiceArea>();
        public ICollection<WorkerSchedule> Schedules { get; set; } = new List<WorkerSchedule>();
        public ICollection<WorkerService> Services { get; set; } = new List<WorkerService>();
        public ICollection<WorkerPayoutAccount> PayoutAccounts { get; set; } =
            new List<WorkerPayoutAccount>();
        public ICollection<WorkerFeaturedOrder> FeaturedOrders { get; set; } =
            new List<WorkerFeaturedOrder>();
        public ICollection<WorkerEarning> Earnings { get; set; } = new List<WorkerEarning>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
