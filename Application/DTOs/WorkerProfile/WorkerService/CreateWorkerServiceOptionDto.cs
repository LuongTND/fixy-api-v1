namespace Application.DTOs.WorkerProfile.WorkerService
{
    public class CreateWorkerServiceOptionDto
    {
        public int DurationMinutes { get; set; }
        public long Price { get; set; }
        public int? SortOrder { get; set; }
        public bool? IsActive { get; set; }
    }
}
