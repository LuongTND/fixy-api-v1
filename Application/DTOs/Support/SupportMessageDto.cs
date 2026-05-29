using System;

namespace Application.DTOs.Support
{
    public class SupportMessageDto
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderRole { get; set; } = string.Empty; // e.g. Customer, Worker, Admin
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
