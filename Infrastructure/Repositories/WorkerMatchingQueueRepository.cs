using Application.Interfaces.Repositories;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WorkerMatchingQueueRepository : Repository<WorkerMatchingQueue>, IWorkerMatchingQueueRepository
    {
        public WorkerMatchingQueueRepository(AppDbContext context) : base(context) { }

        public async Task<WorkerMatchingQueue?> GetOfferedEntryAsync(Guid bookingId, Guid workerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(q =>
                    q.BookingId == bookingId &&
                    q.WorkerId == workerId &&
                    q.Status == MatchingStatus.Offered,
                    cancellationToken);
        }

        public async Task<List<WorkerMatchingQueue>> GetExpiredEntriesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Include(q => q.Booking)
                .Where(q =>
                    q.Status == MatchingStatus.Offered &&
                    q.ExpiresAt != null &&
                    q.ExpiresAt < now)
                .ToListAsync(cancellationToken);
        }

        public async Task<WorkerMatchingQueue?> GetNextCandidateAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(q =>
                    q.BookingId == bookingId &&
                    q.Status == MatchingStatus.Pending)
                .OrderByDescending(q => q.Score)
                .ThenBy(q => q.DistanceKm)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<WorkerMatchingQueue>> GetQueueForBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(q => q.Worker)
                    .ThenInclude(w => w!.User)
                .Where(q => q.BookingId == bookingId)
                .OrderByDescending(q => q.Score)
                .ThenBy(q => q.DistanceKm)
                .ToListAsync(cancellationToken);
        }
    }
}
