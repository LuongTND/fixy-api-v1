using Domain.Common;

namespace Domain.Entity
{
    public class Invoice : BaseEntity
    {
        public Guid BookingId { get; set; }
        public Guid PaymentOrderId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public long Amount { get; set; }
        public long TaxAmount { get; set; }
        public long TotalAmount { get; set; }
        public DateTime IssuedAt { get; set; }
        public string? FileUrl { get; set; }

        public Booking? Booking { get; set; }
        public PaymentOrder? PaymentOrder { get; set; }
    }
}
