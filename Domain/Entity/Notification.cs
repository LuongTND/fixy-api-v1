using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class Notification : BaseAuditableEntity
    {
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public string? Code { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? DeepLink { get; set; }
        public string? Meta { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }

        public User? User { get; set; }
    }
}
