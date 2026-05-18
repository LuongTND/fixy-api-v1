using Domain.Entity;
using Domain.Enum;

namespace Application.Interfaces.Repositories
{
    public interface IWorkerMatchingQueueRepository : IRepository<WorkerMatchingQueue>
    {
        /// <summary>
        /// Get the current offered queue entry for a specific worker and booking.
        /// </summary>
        Task<WorkerMatchingQueue?> GetOfferedEntryAsync(Guid bookingId, Guid workerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all expired queue entries (Status == Offered and ExpiresAt < now).
        /// </summary>
        Task<List<WorkerMatchingQueue>> GetExpiredEntriesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the next candidate worker in the queue for a booking (Status == Pending, ordered by Score descending).
        /// </summary>
        Task<WorkerMatchingQueue?> GetNextCandidateAsync(Guid bookingId, CancellationToken cancellationToken = default);
    }
}
