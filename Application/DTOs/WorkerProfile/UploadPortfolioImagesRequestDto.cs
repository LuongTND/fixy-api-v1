using Microsoft.AspNetCore.Http;

namespace Application.DTOs.WorkerProfile
{
    public class UploadPortfolioImagesRequestDto
    {
        public List<IFormFile> Images { get; set; } = [];
    }
}
