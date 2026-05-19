namespace Application.DTOs.Review
{
    public class CreateReviewRequestDto
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
