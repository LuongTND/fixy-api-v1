using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class WorkerProfile : BaseEntity
    {
        public Guid UserId { get; set; }

        //Profile
        public string? Bio { get; set; }
        public int ExperienceYears { get; set; }

        //Working Area
        public int MaxDistanceKm { get; set; }

        //Realtime status
        public bool IsOnline { get; set; }
        public bool IsBusy { get; set; }
        public bool IsAcceptingJobs { get; set; }

        //Real time location
        public double? CurrentLat { get; set; }
        public double? CurrentLng { get; set; }
        public DateTime? LastLocationAt { get; set; }

        // Bussiness
        public WorkerStatus Status { get; set; } = WorkerStatus.Pending;
        public string? RejectReason { get; set; }

        // Statistic
        public WorkerBadge Badge { get; set; } = WorkerBadge.New;
        public double RatingAvg { get; set; }
        public int TotalOrders { get; set; }

        //Feature
        public DateTime? FeaturedUntil { get; set; }
        public FeaturedPackage? FeaturedPackage { get; set; }

        // Approval
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedById { get; set; }

        public User? User { get; set; }
        public User? ApprovedBy { get; set; }

        public ICollection<WorkerCertificate> Certificates { get; set; } =
            new List<WorkerCertificate>();
        public ICollection<WorkerService> Services { get; set; } = new List<WorkerService>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        //Worker Schdule
        public ICollection<WorkerWeeklySchedule> WeeklySchedules { get; set; } =
            new List<WorkerWeeklySchedule>();
        public ICollection<WorkerScheduleException> ScheduleExceptions { get; set; } =
            new List<WorkerScheduleException>();

        /// <summary>
        /// Chua dung den
        /// </summary>
        public ICollection<WorkerPayoutAccount> PayoutAccounts { get; set; } =
            new List<WorkerPayoutAccount>();
        public ICollection<WorkerFeaturedOrder> FeaturedOrders { get; set; } =
            new List<WorkerFeaturedOrder>();
        public ICollection<WorkerEarning> Earnings { get; set; } = new List<WorkerEarning>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
