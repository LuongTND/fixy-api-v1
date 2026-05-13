using Application.DTOs.Media;

namespace Application.DTOs.WorkerProfile.WorkerCertificate
{
    public class WorkerCertificateUploadRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string? IssuedBy { get; set; }
        public DateOnly? IssuedAt { get; set; }
        public ICollection<UploadMediaRequestDto> MediaUploads { get; set; } =
            new List<UploadMediaRequestDto>();
    }
}
