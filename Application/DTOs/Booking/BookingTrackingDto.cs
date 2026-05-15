namespace Application.DTOs.Booking
{
    public class WorkerTrackingInfoDto
    {
        public Guid WorkerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public double RatingAvg { get; set; }
    }

    /// <summary>
    /// Latest known location of a worker for a given booking.
    /// Returned by GET /api/bookings/{id}/tracking when the customer first opens the tracking screen.
    /// </summary>
    public class BookingTrackingDto
    {
        public Guid BookingId { get; set; }
        public string Status { get; set; } = string.Empty;
        public double? WorkerLat { get; set; }
        public double? WorkerLng { get; set; }
        public DateTime? LocationUpdatedAt { get; set; }
        public WorkerTrackingInfoDto? WorkerInfo { get; set; }
    }
}
