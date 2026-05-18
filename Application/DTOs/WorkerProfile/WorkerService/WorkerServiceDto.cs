namespace Application.DTOs.WorkerProfile.WorkerService
{
    public class WorkerServiceDto
    {
        public Guid Id { get; set; }
        public Guid WorkerProfileId { get; set; }

        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public long BasePrice { get; set; }
        public bool IsPrimary { get; set; }
    }
}
