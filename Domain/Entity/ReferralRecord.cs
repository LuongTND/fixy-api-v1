using Domain.Common;

namespace Domain.Entity
{
    public class ReferralRecord : BaseEntity
    {
        public Guid ReferrerUserId { get; set; }

        public Guid ReferredUserId { get; set; }

        public string ReferralCode { get; set; } = string.Empty;

        public DateTime ReferredAt { get; set; }

        public bool IsRewardClaimed { get; set; }

        public Guid? RewardVoucherId { get; set; }

        public User? ReferrerUser { get; set; }

        public User? ReferredUser { get; set; }

        public Voucher? RewardVoucher { get; set; }
    }
}
