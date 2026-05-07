using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class Booking : BaseAuditableEntity, ISoftDelete
    {
        public Guid CustomerId { get; set; }
        public Guid? WorkerId { get; set; }
        public Guid CategoryId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Lng { get; set; }
        public BookingScheduledType ScheduledType { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public long? EstimatedPrice { get; set; }
        public long? FinalPrice { get; set; }
        public long? WorkerProposedPrice { get; set; }
        public DateTime? WorkerProposedTime { get; set; }
        public string? WorkerProposedNote { get; set; }
        public string? CancelReason { get; set; }
        public Guid? CancelledById { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid? ReorderFromId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }

        public CustomerProfile? Customer { get; set; }
        public WorkerProfile? Worker { get; set; }
        public ServiceCategory? Category { get; set; }
        public User? CancelledBy { get; set; }
        public Booking? ReorderFrom { get; set; }
        public ICollection<Booking> Reorders { get; set; } = new List<Booking>();
        public ICollection<WorkerMatchingQueue> MatchingQueue { get; set; } = new List<WorkerMatchingQueue>();
        public PaymentOrder? PaymentOrder { get; set; }
        public Invoice? Invoice { get; set; }
        public BookingVoucher? BookingVoucher { get; set; }
        public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
        public Review? Review { get; set; }
        public WorkerEarning? WorkerEarning { get; set; }
        public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
    }
}
