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

        [Authorize(Roles = "CUSTOMER")]
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

        [Authorize(Roles = "WORKER")]
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

        [HttpGet("booking/{bookingId:guid}")]
        public async Task<IActionResult> GetByBookingId(
            Guid bookingId,
            CancellationToken cancellationToken
        )
        {
            var result = await _reviewService.GetByBookingIdAsync(bookingId, cancellationToken);

            return HandleResult(result);
        }
    }
}
