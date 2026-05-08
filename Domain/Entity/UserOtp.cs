using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class UserOtp : BaseEntity
    {
        public Guid UserId { get; set; }
        public string OtpHash { get; set; } = string.Empty;
        public UserOtpType Type { get; set; }
        public int AttemptCount { get; set; }
        public DateTime? LockedUntil { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
        public string? IpAddress { get; set; }

        public User? User { get; set; }
    }
}
