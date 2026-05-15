namespace Application.DTOs.Booking
{
    public class WorkerLocationUpdateDto
    {
        public Guid BookingId { get; set; }
        public Guid WorkerId { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
