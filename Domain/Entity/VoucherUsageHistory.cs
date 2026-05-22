using Domain.Common;

namespace Domain.Entity
{
    public class VoucherUsageHistory : BaseEntity
    {
        public Guid VoucherId { get; set; }
        public Guid UserId { get; set; }
        public Guid? BookingId { get; set; }
        public long DiscountAmount { get; set; }
        public DateTime AppliedAt { get; set; }
        public bool IsSuccess { get; set; }
        public string? FailReason { get; set; }

        public Voucher? Voucher { get; set; }
        public User? User { get; set; }
        public Booking? Booking { get; set; }
    }
}
