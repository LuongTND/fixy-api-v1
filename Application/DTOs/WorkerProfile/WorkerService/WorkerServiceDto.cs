namespace Application.DTOs.WorkerProfile.WorkerService
{
    public class WorkerServiceDto
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public Guid WorkerProfile { get; set; }
        public long BasePrice { get; set; }
        public bool IsPrimary { get; set; }
    }
}
