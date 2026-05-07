using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class Voucher : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public VoucherType Type { get; set; }
        public long Value { get; set; }
        public long MinOrderValue { get; set; }
        public long? MaxDiscount { get; set; }
        public Guid? CategoryId { get; set; }
        public int? MaxUsage { get; set; }
        public int UsedCount { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid CreatedById { get; set; }

        public ServiceCategory? Category { get; set; }
        public User? CreatedBy { get; set; }
        public ICollection<BookingVoucher> BookingVouchers { get; set; } = new List<BookingVoucher>();
    }
}
