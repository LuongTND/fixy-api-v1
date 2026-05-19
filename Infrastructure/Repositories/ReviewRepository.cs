using Application.Common;
using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        public ReviewRepository(AppDbContext context)
            : base(context) { }

        public async Task<bool> ExistsByBookingIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet.AnyAsync(x => x.BookingId == bookingId, cancellationToken);
        }

        public async Task<(List<Review>, int)> GetWorkerReviewsPagedAsync(
            Guid workerId,
            PagedQuery query,
            CancellationToken cancellationToken
        )
        {
            var queryDb = _dbSet
                .Include(x => x.Customer)
                    .ThenInclude(x => x!.User)
                .AsNoTracking()
                .Where(x => x.WorkerId == workerId && x.IsVisible);

            // Sort
            queryDb = query.SortBy?.ToLower() switch
            {
                "rating" => query.SortDescending
                    ? queryDb.OrderByDescending(x => x.Rating)
                    : queryDb.OrderBy(x => x.Rating),

                "createddate" => query.SortDescending
                    ? queryDb.OrderByDescending(x => x.CreatedDate)
                    : queryDb.OrderBy(x => x.CreatedDate),

                _ => queryDb.OrderByDescending(x => x.CreatedDate),
            };

            var totalCount = await queryDb.CountAsync(cancellationToken);

            var items = await queryDb
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<Review?> GetDetailAsync(
            Guid reviewId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                .Include(x => x.Customer)
                    .ThenInclude(x => x!.User)
                .FirstOrDefaultAsync(x => x.Id == reviewId, cancellationToken);
        }

        public async Task<double> RecalculateAverageRatingAsync(
            Guid workerId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                    .Where(x => x.WorkerId == workerId && x.IsVisible)
                    .Select(x => (double?)x.Rating)
                    .AverageAsync(cancellationToken)
                ?? 0;
        }

        public async Task<int> RecalculateTotalReviewsAsync(
            Guid workerId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet.CountAsync(
                x => x.WorkerId == workerId && x.IsVisible,
                cancellationToken
            );
        }
    }
}
