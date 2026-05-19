using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public CustomerReviewInfoDto Customer { get; set; } = default!;
    }
}
