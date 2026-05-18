using Domain.Enum;

namespace Application.DTOs.Booking
{
    public class ReportBookingIssueRequest
    {
        public SupportCategory Category { get; set; } = SupportCategory.Other;
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public SupportPriority Priority { get; set; } = SupportPriority.Normal;
    }
}
