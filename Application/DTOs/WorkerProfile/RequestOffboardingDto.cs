namespace Application.DTOs.WorkerProfile
{
    public class RequestOffboardingDto
    {
        public Guid PayoutAccountId { get; set; }

        public string? Reason { get; set; }
    }
}
