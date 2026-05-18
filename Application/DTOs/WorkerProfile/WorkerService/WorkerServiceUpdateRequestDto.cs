namespace Application.DTOs.WorkerProfile.WorkerService
{
    public class WorkerServiceUpdateRequestDto
    {
        public Guid CategoryId { get; set; }
        public long BasePrice { get; set; }
        public bool IsPrimary { get; set; }
    }
}
