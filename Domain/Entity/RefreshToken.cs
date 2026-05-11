using Domain.Common;

namespace Domain.Entity
{
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? RevokedReason { get; set; }
        public Guid? ReplacedById { get; set; }

        public User? User { get; set; }
        public RefreshToken? ReplacedBy { get; set; }
    }
}
