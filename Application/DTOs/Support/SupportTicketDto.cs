using Domain.Enum;

namespace Application.DTOs.Support
{
    public class SupportTicketDto
    {
        public Guid Id { get; set; }
        public Guid ReporterId { get; set; }
        public Guid? BookingId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Priority { get; set; } = SupportPriority.Normal.ToString();
        public string Status { get; set; } = SupportStatus.Open.ToString();
        public DateTime CreatedDate { get; set; }
    }
}
