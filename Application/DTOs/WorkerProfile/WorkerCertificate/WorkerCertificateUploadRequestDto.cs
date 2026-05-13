using Microsoft.AspNetCore.Http;

namespace Application.DTOs.WorkerProfile.WorkerCertificate
{
    public class WorkerCertificateUploadRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string? IssuedBy { get; set; }
        public DateOnly? IssuedAt { get; set; }
        public List<IFormFile> MediaUploads { get; set; } = new List<IFormFile>();
    }
}
