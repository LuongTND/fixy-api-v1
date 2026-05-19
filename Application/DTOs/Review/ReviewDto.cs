using Application.DTOs.Media;

namespace Application.DTOs.Review
{
    public class ReviewDto
    {
        public Guid Id { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public string? WorkerReply { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? RepliedAt { get; set; }

        public Guid BookingId { get; set; }
        public List<MediaDto> Images { get; set; } = new List<MediaDto>();

        public CustomerReviewInfoDto Customer { get; set; } = default!;
    }
}
