using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class VipMembership : BaseEntity
    {
        public Guid UserId { get; set; }

        public VipTier Tier { get; set; } = VipTier.Silver;

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public long TotalSpent { get; set; }

        public decimal DiscountPercent { get; set; }

        public User? User { get; set; }
    }
}
