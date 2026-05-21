using Application.Common;
using Application.DTOs.Review;

namespace Application.Interfaces.Services
{
    public interface IReviewService
    {
        Task<OperationResult> CreateReviewAsync(
            Guid customerId,
            Guid bookingId,
            CreateReviewRequestDto dto,
            CancellationToken cancellationToken
        );

        Task<OperationResult> ReplyReviewAsync(
            Guid workerId,
            Guid reviewId,
            ReplyReviewRequestDto dto,
            CancellationToken cancellationToken
        );
        Task<OperationResult<PagedResponse<ReviewDto>>> GetWorkerReviewsPagedAsync(
            Guid workerId,
            PagedQuery query,
            CancellationToken cancellationToken
        );
        Task<OperationResult<ReviewDto>> GetByBookingIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken
        );
    }
}
