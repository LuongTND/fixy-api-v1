namespace Application.DTOs.Dashboard
{
    public class TopServiceDto
    {
        public string ServiceName { get; set; } = default!;

        public int TotalBookings { get; set; }
        public double Percentage { get; set; }
    }
}
