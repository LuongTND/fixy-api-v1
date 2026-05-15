namespace Application.DTOs.Booking
{
    public class CompleteBookingRequest
    {
        public List<Guid> MediaIds { get; set; } = new();
    }
}
