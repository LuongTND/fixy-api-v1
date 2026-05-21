using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class User : BaseEntity, ISoftDelete
    {
        // =========================
        // Basic Info
        // =========================

        public string FullName { get; set; } = string.Empty;

        public DateOnly? DateOfBirth { get; set; }

        public Gender Gender { get; set; }

        public string? Phone { get; set; }

        public bool IsPhoneVerified { get; set; }

        public string? Email { get; set; }

        public bool IsEmailVerified { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        // =========================
        // Identification
        // =========================

        public string? CitizenIdNumber { get; set; }

        public DateTime? CitizenIdIssueDate { get; set; }

        public string? CitizenIdIssuePlace { get; set; }

        public bool IsCitizenIdVerified { get; set; } = false;

        public DateTime? CitizenIdVerifiedAt { get; set; }

        // =========================
        // OAuth
        // =========================

        public OAuthProvider? OAuthProvider { get; set; }

        public string? OAuthId { get; set; }

        // =========================
        // Security
        // =========================

        public bool IsActive { get; set; } = true;

        public string? TotpSecret { get; set; }

        public bool TotpEnabled { get; set; }

        // =========================
        // Soft Delete
        // =========================

        public bool IsDeleted { get; set; }

        public DateTime? DeletedDate { get; set; }

        public string? DeletedBy { get; set; }

        // =========================
        // Navigation
        // =========================

        public ICollection<UserOtp> UserOtps { get; set; } = new List<UserOtp>();

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        public NotificationSetting? NotificationSetting { get; set; }

        public Wallet? Wallet { get; set; }

        // =========================
        // Role Profiles
        // =========================

        public CustomerProfile? CustomerProfile { get; set; }

        public WorkerProfile? WorkerProfile { get; set; }
    }
}
