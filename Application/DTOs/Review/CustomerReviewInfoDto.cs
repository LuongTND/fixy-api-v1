namespace Application.DTOs.Review
{
    public class CustomerReviewInfoDto
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = default!;

        public string? AvatarUrl { get; set; }
    }
}
