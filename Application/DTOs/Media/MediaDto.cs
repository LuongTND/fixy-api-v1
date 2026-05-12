using Domain.Enum;

namespace Application.DTOs.Media
{
    public class MediaDto
    {
        public Guid Id { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? OriginalName { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public int? FileSizeKb { get; set; }
        public MediaCategory Category { get; set; }
    }
}
