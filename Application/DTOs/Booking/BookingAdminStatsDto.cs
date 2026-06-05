namespace Application.DTOs.Booking
{
    public class BookingAdminStatsDto
    {
        public int TotalBookings { get; set; }
        public int InProgressBookings { get; set; }
        public int CompletedBookings { get; set; }
        public long TotalValue { get; set; }
    }
}
