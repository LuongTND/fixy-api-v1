namespace Application.DTOs.Booking
{
    public class BookingStatusUpdateDto
    {
        public Guid BookingId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? Message { get; set; }
    }
}
