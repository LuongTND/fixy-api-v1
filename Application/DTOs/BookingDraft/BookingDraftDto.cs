using Domain.Enum;

namespace Application.DTOs.BookingDraft
{
    public class BookingDraftDto
    {
        public Guid DraftId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public Guid CategoryId { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<Guid> MediaIds { get; set; } = new();
        public Guid? AddressId { get; set; }
        public string? Address { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public BookingScheduledType ScheduledType { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public Guid? WorkerId { get; set; }
        public bool AutoMatch { get; set; }
    }
}
