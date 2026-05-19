using Application.Common;
using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<bool> ExistsByBookingIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default
        );

        Task<(List<Review>, int)> GetWorkerReviewsPagedAsync(
            Guid workerId,
            PagedQuery query,
            CancellationToken cancellationToken
        );

        Task<Review?> GetDetailAsync(Guid reviewId, CancellationToken cancellationToken = default);
        Task<double> RecalculateAverageRatingAsync(
            Guid workerId,
            CancellationToken cancellationToken = default
        );
        Task<int> RecalculateTotalReviewsAsync(
            Guid workerId,
            CancellationToken cancellationToken = default
        );
    }
}
