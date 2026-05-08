using Domain.Common;

namespace Domain.Entity
{
    public class BookingVoucher : BaseEntity
    {
        public Guid BookingId { get; set; }
        public Guid VoucherId { get; set; }
        public long DiscountAmount { get; set; }
        public DateTime AppliedAt { get; set; }

        public Booking? Booking { get; set; }
        public Voucher? Voucher { get; set; }
    }
}
