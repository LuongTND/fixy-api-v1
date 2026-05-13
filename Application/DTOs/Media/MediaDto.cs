using Domain.Enum;

namespace Application.DTOs.Media
{
    public class MediaDto
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public MediaOwnerType OwnerType { get; set; }
        public MediaCategory Category { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string FilePublicId { get; set; } = string.Empty;
    }
}
