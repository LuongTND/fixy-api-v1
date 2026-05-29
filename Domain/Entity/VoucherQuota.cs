using Domain.Common;

namespace Domain.Entity
{
    public class VoucherQuota : BaseEntity
    {
        public Guid VoucherId { get; set; }
        public int? MaxUsage { get; set; }
        public int UsedCount { get; set; }
        public int? MaxUsagePerUser { get; set; }

        // Navigation Property
        public Voucher? Voucher { get; set; }
    }
}
