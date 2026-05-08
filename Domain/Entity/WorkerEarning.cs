using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class WorkerEarning : BaseAuditableEntity
    {
        public Guid WorkerId { get; set; }
        public Guid BookingId { get; set; }
        public Guid PaymentOrderId { get; set; }
        public long GrossAmount { get; set; }
        public double CommissionRate { get; set; }
        public long CommissionAmount { get; set; }
        public long NetAmount { get; set; }
        public WorkerEarningStatus Status { get; set; } = WorkerEarningStatus.Pending;
        public Guid? PayoutRequestId { get; set; }
        public DateTime? ReleasedAt { get; set; }

        public WorkerProfile? Worker { get; set; }
        public Booking? Booking { get; set; }
        public PaymentOrder? PaymentOrder { get; set; }
        public PayoutRequest? PayoutRequest { get; set; }
    }
}
