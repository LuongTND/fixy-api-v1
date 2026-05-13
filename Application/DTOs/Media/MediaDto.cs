using Domain.Enum;

namespace Application.DTOs.Media
{
    public class MediaDto
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string OwnerType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
    }
}
