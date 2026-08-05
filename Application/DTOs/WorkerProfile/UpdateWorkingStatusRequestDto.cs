namespace Application.DTOs.WorkerProfile
{
    public class UpdateWorkingStatusRequestDto
    {
        public bool? IsOnline { get; set; }
        public bool? IsAcceptingJobs { get; set; }
    }
}
