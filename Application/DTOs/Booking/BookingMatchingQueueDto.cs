namespace Application.DTOs.Booking
{
    public class BookingMatchingQueueDto
    {
        public Guid WorkerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public double RatingAvg { get; set; }
        public double? DistanceKm { get; set; }
        public double? Score { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? OfferedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? RejectReason { get; set; }
    }
}
