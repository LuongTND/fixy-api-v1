using Domain.Enum;

namespace Application.DTOs.Media
{
    public class UploadMediaRequestDto
    {
        public MediaOwnerType OwnerType { get; set; }
        public MediaCategory Category { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string FilePublicId { get; set; } = string.Empty;
    }
}
