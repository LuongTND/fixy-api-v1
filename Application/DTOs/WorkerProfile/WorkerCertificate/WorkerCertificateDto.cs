using Application.DTOs.Media;

namespace Application.DTOs.WorkerProfile.WorkerCertificate
{
    public class WorkerCertificateDto
    {
        public Guid Id { get; set; }
        public Guid WorkerProfileId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? IssuedBy { get; set; }
        public DateOnly? IssuedAt { get; set; }
        public ICollection<MediaDto> CertificateImage { get; set; } = new List<MediaDto>();
    }
}
