using Application.Common;

namespace Application.Interfaces.Services.Booking
{
    public interface IWorkerMatchingService
    {
        /// <summary>
        /// Search for tradespeople in the area, sort by RatingAvg, and create a matching queue for Booking.
        /// Then, automatically send job offers to the first tradesperson in the queue.
        /// </summary>
        Task<OperationResult> ProcessAutoMatchAsync(Guid bookingId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Forward the job offer to the next worker in the queue.
        /// Called when the previous worker declines or the response time expires.
        /// </summary>
        Task<OperationResult> OfferToNextWorkerAsync(Guid bookingId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a matching queue entry for a manually assigned worker and set booking status to Pending.
        /// </summary>
        Task<OperationResult> ProcessDirectAssignAsync(Guid bookingId, Guid workerProfileId, CancellationToken cancellationToken = default);
    }
}
