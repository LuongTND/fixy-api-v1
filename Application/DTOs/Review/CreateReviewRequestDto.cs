using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Review
{
    public class CreateReviewRequestDto
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}
