using Application.DTOs.Media;
using Application.DTOs.WorkerProfile.WorkerCertificate;
using Application.DTOs.WorkerProfile.WorkerService;

namespace Application.DTOs.WorkerProfile
{
    public class WorkerRegisterRequestDto
    {
        public string Target { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Password { get; set; } = default!;

        public string? Bio { get; set; }
        public int ExperienceYears { get; set; }
        public int MaxDistanceKm { get; set; }

        // Identification
        public string CitizenIdNumber { get; set; } = default!;

        public DateTime CitizenIdIssueDate { get; set; }

        public string CitizenIdIssuePlace { get; set; } = default!;
        public ICollection<UploadMediaRequestDto> IdentificationUploads { get; set; } =
            new List<UploadMediaRequestDto>();

        public ICollection<WorkerServiceRegisterRequestDto> WorkerService { get; set; } =
            new List<WorkerServiceRegisterRequestDto>();
        public ICollection<WorkerCertificateUploadRequestDto> CerificateUploads { get; set; } =
            new List<WorkerCertificateUploadRequestDto>();
    }
}
