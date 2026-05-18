using Microsoft.AspNetCore.Http;

namespace Application.DTOs.WorkerProfile.WorkerService
{
    public class UpdateIdentificationImagesRequestDto
    {
        public List<IFormFile> Images { get; set; } = [];
    }
}
