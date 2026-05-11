using Domain.Common;

namespace Domain.Entity
{
    public class SupportMessage : BaseEntity
    {
        public Guid TicketId { get; set; }
        public Guid SenderId { get; set; }
        public string Content { get; set; } = string.Empty;

        public SupportTicket? Ticket { get; set; }
        public User? Sender { get; set; }
    }
}
