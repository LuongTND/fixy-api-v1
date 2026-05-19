using Domain.Enum;

namespace Application.DTOs.Booking
{
    public class BookingDetailDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? WorkerId { get; set; }
        public Guid CategoryId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ScheduledType { get; set; } = string.Empty;
        public DateTime? ScheduledAt { get; set; }
        public long? EstimatedPrice { get; set; }
        public long? FinalPrice { get; set; }
        public string? CancelReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public long? WorkerProposedPrice { get; set; }
        public DateTime? WorkerProposedTime { get; set; }
        public string? WorkerProposedNote { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
