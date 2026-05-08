using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class UserSession : BaseEntity
    {
        public Guid UserId { get; set; }
        public string? DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public SessionDeviceType? DeviceType { get; set; }
        public string? Os { get; set; }
        public string? AppVersion { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? Location { get; set; }
        public DateTime LastActiveAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? TerminatedAt { get; set; }
        public SessionTerminatedBy? TerminatedBy { get; set; }

        public User? User { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
