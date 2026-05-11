using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class User : BaseEntity, ISoftDelete
    {
        public string FullName { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public bool IsPhoneVerified { get; set; }
        public string? Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public OAuthProvider? OAuthProvider { get; set; }
        public string? OAuthId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }
        public string? TotpSecret { get; set; }
        public bool TotpEnabled { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }

        public ICollection<UserOtp> UserOtps { get; set; } = new List<UserOtp>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public WorkerProfile? WorkerProfile { get; set; }
        public CustomerProfile? CustomerProfile { get; set; }
        public Wallet? Wallet { get; set; }
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public NotificationSetting? NotificationSetting { get; set; }
    }
}
