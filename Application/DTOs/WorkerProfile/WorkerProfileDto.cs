using Application.DTOs.WorkerProfile.WorkerService;
using Domain.Enum;

namespace Application.DTOs.WorkerProfile
{
    public class WorkerProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public int ExperienceYears { get; set; }
        public double RatingAvg { get; set; }
        public string? Gender { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<WorkerServiceDto> Services { get; set; } = new List<WorkerServiceDto>();
    }
}
