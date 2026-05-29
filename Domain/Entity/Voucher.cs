using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class Voucher : BaseEntity, ISoftDelete
    {
        public string Code { get; set; } = string.Empty;
        public VoucherType Type { get; set; }
        public long Value { get; set; }
        public long MinOrderValue { get; set; }
        public long? MaxDiscount { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public VoucherStatus Status { get; set; } = VoucherStatus.Draft;
        public string? Description { get; set; }
        public Guid CreatedById { get; set; }

        // ISoftDelete Implementation
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }

        // Navigation Properties
        public Guid? CampaignId { get; set; }
        public virtual VoucherCampaign? Campaign { get; set; }

        public User? CreatedBy { get; set; }
        public VoucherQuota? Quota { get; set; }
        public ICollection<VoucherRestriction> Restrictions { get; set; } = new List<VoucherRestriction>();
        public ICollection<BookingVoucher> BookingVouchers { get; set; } = new List<BookingVoucher>();
        public ICollection<VoucherUsageHistory> UsageHistories { get; set; } = new List<VoucherUsageHistory>();
    }
}
