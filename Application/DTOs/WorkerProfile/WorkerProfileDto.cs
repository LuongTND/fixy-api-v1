using Application.DTOs.WorkerProfile.WorkerService;
using Domain.Enum;

namespace Application.DTOs.WorkerProfile
{
    public class WorkerProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public int ExperienceYears { get; set; }
        public double RatingAvg { get; set; }
        public int TotalReviews { get; set; }
        public int TotalOrders { get; set; }

        public string? Gender { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public bool IsBusy { get; set; }
        public double? DistanceKm { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public List<WorkerServiceDto> Services { get; set; } = new List<WorkerServiceDto>();
    }
}
