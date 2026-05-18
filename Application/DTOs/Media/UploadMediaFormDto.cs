using Domain.Enum;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Media
{
    public class UploadMediaFormDto
    {
        public MediaCategory Category { get; set; }
        public MediaOwnerType OwnerType { get; set; }
        public Guid? OwnerId { get; set; }
        public List<IFormFile> Files { get; set; } = new();
    }
}
