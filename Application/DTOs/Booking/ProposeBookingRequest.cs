namespace Application.DTOs.Booking
{
    public class ProposeBookingRequest
    {
        public long? ProposedPrice { get; set; }
        public DateTime? ProposedTime { get; set; }
        public string? ProposedNote { get; set; }
    }
}
