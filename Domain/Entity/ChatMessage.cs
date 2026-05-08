using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class ChatMessage : BaseEntity
    {
        public Guid BookingId { get; set; }
        public Guid SenderId { get; set; }
        public ChatMessageType Type { get; set; }
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public bool IsRead { get; set; }

        public Booking? Booking { get; set; }
        public User? Sender { get; set; }
    }
}
