namespace Application.DTOs.BookingDraft
{
    public class BookingDraftCreatedDto
    {
        public Guid DraftId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
