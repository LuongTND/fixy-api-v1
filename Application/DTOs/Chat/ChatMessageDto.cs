namespace Application.DTOs.Chat
{
    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderRole { get; set; } = string.Empty; // "WORKER" hoặc "CUSTOMER"
        public string Type { get; set; } = "Text"; // Text, Image, Voice
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
