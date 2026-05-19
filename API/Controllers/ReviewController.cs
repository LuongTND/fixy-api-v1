using Application.Common;
using Application.DTOs.Review;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/reviews")]
    [ApiController]
    public class ReviewController : ApiController
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("{bookingId}")]
        public async Task<IActionResult> CreateReview(
            Guid bookingId,
            [FromForm] CreateReviewRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var result = await _reviewService.CreateReviewAsync(
                GetUserId(),
                bookingId,
                dto,
                cancellationToken
            );

            return HandleResult(result);
        }

        [Authorize(Roles = "Worker")]
        [HttpPost("{reviewId}/reply")]
        public async Task<IActionResult> ReplyReview(
            Guid reviewId,
            [FromForm] ReplyReviewRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var result = await _reviewService.ReplyReviewAsync(
                GetUserId(),
                reviewId,
                dto,
                cancellationToken
            );

            return HandleResult(result);
        }

        [HttpGet("worker/{workerId}/paged")]
        public async Task<IActionResult> GetWorkerReviewsPaged(
            Guid workerId,
            [FromQuery] PagedQuery query,
            CancellationToken cancellationToken
        )
        {
            var result = await _reviewService.GetWorkerReviewsPagedAsync(
                workerId,
                query,
                cancellationToken
            );

            return HandleResult(result);
        }
    }
}
