using Domain.Enum;

namespace Application.DTOs.Support
{
    public class CreateSupportTicketRequest
    {
        public Guid? BookingId { get; set; }
        public SupportCategory Category { get; set; } = SupportCategory.Other;
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public SupportPriority Priority { get; set; } = SupportPriority.Normal;
    }
}
