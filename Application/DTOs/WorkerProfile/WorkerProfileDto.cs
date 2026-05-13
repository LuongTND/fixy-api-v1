using Domain.Enum;

namespace Application.DTOs.WorkerProfile
{
    public class WorkerProfileDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
