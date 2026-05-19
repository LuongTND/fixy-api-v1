namespace Application.DTOs.Booking
{
    public class RespondProposalRequest
    {
        public bool Accept { get; set; }
        public string? RejectReason { get; set; }
    }
}
