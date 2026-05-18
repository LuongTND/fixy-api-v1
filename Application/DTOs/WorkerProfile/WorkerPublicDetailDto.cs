using Application.DTOs.Media;
using Application.DTOs.WorkerProfile.WorkerCertificate;
using Application.DTOs.WorkerProfile.WorkerService;

namespace Application.DTOs.WorkerProfile
{
    public class WorkerPublicDetailDto
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public int ExperienceYears { get; set; }

        public double RatingAvg { get; set; }

        public string? Bio { get; set; }

        public List<WorkerServiceDto> Services { get; set; } = [];

        public List<MediaDto> PortfolioImages { get; set; } = [];

        public List<WorkerCertificateDto> Certificates { get; set; } = [];
    }
}
