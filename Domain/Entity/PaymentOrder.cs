using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class PaymentOrder : BaseAuditableEntity
    {
        public Guid BookingId { get; set; }
        public Guid CustomerId { get; set; }
        public long Amount { get; set; }
        public long DiscountAmount { get; set; }
        public long FinalAmount { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? GatewayRef { get; set; }
        public string? GatewayResponse { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }

        public Booking? Booking { get; set; }
        public User? Customer { get; set; }
        public Invoice? Invoice { get; set; }
        public WorkerEarning? WorkerEarning { get; set; }
    }
}
