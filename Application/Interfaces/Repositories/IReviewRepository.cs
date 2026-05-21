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
        Task<Review?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken);
        Task<(List<Review>, int)> GetWorkerReviewsPagedAsync(
            Guid workerProfileId,
            PagedQuery query,
            CancellationToken cancellationToken
        );

        Task<double> RecalculateAverageRatingAsync(
            Guid workerProfileId,
            CancellationToken cancellationToken = default
        );

        Task<int> RecalculateTotalReviewsAsync(
            Guid workerProfileId,
            CancellationToken cancellationToken = default
        );
    }
}
