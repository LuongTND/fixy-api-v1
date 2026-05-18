namespace Application.DTOs.WorkerProfile
{
    public class WorkerPrivateDetailDto : WorkerPublicDetailDto
    {
        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
    }
}
