using Domain.Common;

namespace Domain.Entity
{
    public class UserFcmToken : BaseAuditableEntity
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string? DeviceType { get; set; } // e.g., "Web", "Mobile"
        public string? Browser { get; set; }    // e.g., "Chrome", "Safari", "Firefox"

        // Navigation Property
        public User? User { get; set; }
    }
}
