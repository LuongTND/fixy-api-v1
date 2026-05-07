using Domain.Common;

namespace Domain.Entity.Identity
{
    public class RefreshToken : BaseAuditableEntity
    {
        public string Token { get; set; } = default!;

        public DateTime ExpiryDate { get; set; }

        public bool IsRevoked { get; set; }

        public DateTime? RevokedDate { get; set; }

        public string? DeviceId { get; set; }
        public Guid UserId { get; set; }

        public User User { get; set; } = default!;
    }
}
