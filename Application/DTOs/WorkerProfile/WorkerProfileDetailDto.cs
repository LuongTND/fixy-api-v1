using Application.DTOs.Media;
using Application.DTOs.WorkerProfile.WorkerCertificate;
using Application.DTOs.WorkerProfile.WorkerService;
using Domain.Enum;

namespace Application.DTOs.WorkerProfile
{
    public class WorkerProfileDetailDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? CitizenIdNumber { get; set; }

        public DateTime? CitizenIdIssueDate { get; set; }

        public string? CitizenIdIssuePlace { get; set; }
        public string? Bio { get; set; }
        public int ExperienceYears { get; set; }
        public int MaxDistanceKm { get; set; }
        public WorkerStatus Status { get; set; }
        public List<WorkerCertificateDto> Certificates { get; set; } =
            new List<WorkerCertificateDto>();
        public List<WorkerServiceDto> Services { get; set; } = new List<WorkerServiceDto>();

        public List<MediaDto> IdentificateImages { get; set; } = new List<MediaDto>();
    }
}
