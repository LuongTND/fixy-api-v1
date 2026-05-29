using Domain.Enum;

namespace Application.DTOs.Support
{
    public class UpdateTicketStatusRequest
    {
        public SupportStatus Status { get; set; }
        public SupportPriority? Priority { get; set; }
        public SupportCategory? Category { get; set; }
    }
}
