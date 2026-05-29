using System;
using System.Collections.Generic;

namespace Application.DTOs.Support
{
    public class SupportTicketDetailsDto : SupportTicketDto
    {
        public string ReporterName { get; set; } = string.Empty;
        public string ReporterPhone { get; set; } = string.Empty;
        public string ReporterAvatarUrl { get; set; } = string.Empty;
        public string ReporterType { get; set; } = string.Empty; // e.g. Customer, Worker
        public string? AssignedToName { get; set; }
        
        // Simplified Booking Info if associated
        public BookingSupportDto? Booking { get; set; }

        public List<SupportMessageDto> Messages { get; set; } = new();
    }
}
