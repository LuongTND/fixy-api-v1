using Application.DTOs.Media;
using Application.DTOs.WorkerProfile.WorkerCertificate;
using Application.DTOs.WorkerProfile.WorkerService;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.WorkerProfile
{
    public class WorkerRegisterRequestDto
    {
        public string Target { get; set; } = string.Empty!;
        public string? Bio { get; set; }
        public int ExperienceYears { get; set; }
        public int MaxDistanceKm { get; set; }

        // Identification
        public string CitizenIdNumber { get; set; } = default!;

        public DateTime CitizenIdIssueDate { get; set; }

        public string CitizenIdIssuePlace { get; set; } = default!;
        public List<IFormFile> IdentificationUploads { get; set; } = new List<IFormFile>();

        public List<WorkerServiceRegisterRequestDto> WorkerService { get; set; } =
            new List<WorkerServiceRegisterRequestDto>();
        public List<WorkerCertificateUploadRequestDto> CertificateUploads { get; set; } =
            new List<WorkerCertificateUploadRequestDto>();
    }
}
