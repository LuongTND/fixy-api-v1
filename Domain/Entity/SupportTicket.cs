using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class SupportTicket : BaseAuditableEntity
    {
        public Guid ReporterId { get; set; }
        public Guid? BookingId { get; set; }
        public SupportReporterType ReporterType { get; set; }
        public SupportCategory? Category { get; set; }
        public string Subject { get; set; } = string.Empty;
        public SupportPriority Priority { get; set; } = SupportPriority.Normal;
        public SupportStatus Status { get; set; } = SupportStatus.Open;
        public Guid? AssignedToId { get; set; }
        public DateTime? ResolvedAt { get; set; }

        public User? Reporter { get; set; }
        public Booking? Booking { get; set; }
        public User? AssignedTo { get; set; }
        public ICollection<SupportMessage> Messages { get; set; } = new List<SupportMessage>();
    }
}
