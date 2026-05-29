namespace Application.DTOs.Dashboard
{
    public class BookingTrendDto
    {
        public string Label { get; set; } = default!;

        public int CompletedCount { get; set; }

        public int CancelledCount { get; set; }
    }
}
