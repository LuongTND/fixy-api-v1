using Application.Common;
using Application.DTOs.Booking;
using Application.Interfaces;
using Application.Interfaces.Hubs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Booking;
using Application.Interfaces.Services.Worker;
using Application.Settings;
using Domain.Entity;
using Domain.Enum;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Booking
{
    public class WorkerMatchingService : IWorkerMatchingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IWorkerProfileRepository _workerProfileRepository;
        private readonly IWorkerServiceRepository _workerServiceRepository;
        private readonly IWorkerMatchingQueueRepository _matchingQueueRepository;
        private readonly IWorkerLocationService _workerLocationService;
        private readonly IBookingHubService _bookingHubService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly WorkerMatchingSettings _matchingSettings;
        private readonly ILogger<WorkerMatchingService> _logger;

        public WorkerMatchingService(
            IBookingRepository bookingRepository,
            IWorkerProfileRepository workerProfileRepository,
            IWorkerServiceRepository workerServiceRepository,
            IWorkerMatchingQueueRepository matchingQueueRepository,
            IWorkerLocationService workerLocationService,
            IBookingHubService bookingHubService,
            IUnitOfWork unitOfWork,
            IOptions<WorkerMatchingSettings> matchingOptions,
            ILogger<WorkerMatchingService> logger
        )
        {
            _bookingRepository =
                bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _workerProfileRepository =
                workerProfileRepository
                ?? throw new ArgumentNullException(nameof(workerProfileRepository));
            _workerServiceRepository =
                workerServiceRepository
                ?? throw new ArgumentNullException(nameof(workerServiceRepository));
            _matchingQueueRepository =
                matchingQueueRepository
                ?? throw new ArgumentNullException(nameof(matchingQueueRepository));
            _workerLocationService =
                workerLocationService
                ?? throw new ArgumentNullException(nameof(workerLocationService));
            _bookingHubService =
                bookingHubService ?? throw new ArgumentNullException(nameof(bookingHubService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _matchingSettings =
                matchingOptions?.Value ?? throw new ArgumentNullException(nameof(matchingOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<OperationResult> ProcessAutoMatchAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default
        )
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            if (booking == null)
            {
                return OperationResult.Failure("Booking not found");
            }

            // 1. Find all eligible workers for this category
            var eligibleWorkers = await _workerServiceRepository.FindAsync(
                ws => ws.CategoryId == booking.CategoryId,
                cancellationToken
            );

            if (eligibleWorkers.Count == 0)
            {
                _logger.LogWarning(
                    "No workers registered for category {CategoryId}",
                    booking.CategoryId
                );
                return OperationResult.Failure("No workers available for this service category");
            }

            var eligibleWorkerProfileIds = eligibleWorkers
                .Select(ws => ws.WorkerProfileId)
                .Distinct()
                .ToList();

            // 2. Find worker profiles that are online, not busy, accepting jobs, and approved
            var activeWorkers = await _workerProfileRepository.FindAsync(
                wp =>
                    eligibleWorkerProfileIds.Contains(wp.Id)
                    && wp.Status == WorkerStatus.Approved,
                    //&& wp.IsOnline
                    //&& !wp.IsBusy
                    //&& wp.IsAcceptingJobs
                cancellationToken
            );

            if (activeWorkers.Count == 0)
            {
                _logger.LogWarning(
                    "No active workers available for booking {BookingId}",
                    bookingId
                );
                return OperationResult.Failure("No workers are currently available");
            }

            // 3. Calculate distance for each worker and filter by MaxDistanceKm
            var candidates = new List<(WorkerProfile Worker, double DistanceKm)>();

            foreach (var worker in activeWorkers)
            {
                // Try to get real-time location from Redis first
                double? workerLat = null;
                double? workerLng = null;

                var lastLocation = await _workerLocationService.GetLastLocationAsync(
                    worker.Id,
                    cancellationToken
                );
                if (lastLocation != null)
                {
                    workerLat = lastLocation.Lat;
                    workerLng = lastLocation.Lng;
                }
                else
                {
                    // Fall back to DB-stored location
                    workerLat = worker.CurrentLat;
                    workerLng = worker.CurrentLng;
                }

                if (!workerLat.HasValue || !workerLng.HasValue)
                {
                    continue; // Skip workers without any known location
                }

                var distanceKm = CalculateHaversineDistance(
                    booking.Lat,
                    booking.Lng,
                    workerLat.Value,
                    workerLng.Value
                );

                // Distance filter is intentionally disabled for now.
                candidates.Add((worker, distanceKm));
            }

            if (candidates.Count == 0)
            {
                _logger.LogWarning("No workers within range for booking {BookingId}", bookingId);
                return OperationResult.Failure("No workers available within service range");
            }

            // 4. Sort by RatingAvg DESC, then by DistanceKm ASC (tie-breaker)
            var sortedCandidates = candidates
                .OrderByDescending(c => c.Worker.RatingAvg)
                .ThenBy(c => c.DistanceKm)
                .ToList();

            // 5. Populate the WorkerMatchingQueues table
            var queueEntries = new List<WorkerMatchingQueue>();
            foreach (var (worker, distanceKm) in sortedCandidates)
            {
                queueEntries.Add(
                    new WorkerMatchingQueue
                    {
                        BookingId = bookingId,
                        WorkerProfileId = worker.Id,
                        AttemptNo = 1,
                        Status = MatchingStatus.Pending,
                        DistanceKm = Math.Round(distanceKm, 2),
                        Score = worker.RatingAvg,
                    }
                );
            }

            await _matchingQueueRepository.AddRangeAsync(queueEntries, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Auto-match populated {Count} candidates for booking {BookingId}",
                queueEntries.Count,
                bookingId
            );

            // 6. Offer to the first (best-ranked) worker
            return await OfferToNextWorkerAsync(bookingId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<OperationResult> OfferToNextWorkerAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default
        )
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            if (booking == null)
            {
                return OperationResult.Failure("Booking not found");
            }

            var nextCandidate = await _matchingQueueRepository.GetNextCandidateAsync(
                bookingId,
                cancellationToken
            );

            if (nextCandidate != null)
            {
                // Update queue entry → Offered
                nextCandidate.Status = MatchingStatus.Offered;
                nextCandidate.OfferedAt = DateTime.UtcNow;
                nextCandidate.ExpiresAt = DateTime.UtcNow.AddMinutes(
                    _matchingSettings.OfferTimeoutMinutes
                );
                _matchingQueueRepository.Update(nextCandidate);

                // Update booking → assign this worker, set status to Pending
                booking.WorkerProfileId = nextCandidate.WorkerProfileId;
                booking.Status = BookingStatus.Pending;
                booking.UpdatedDate = DateTime.UtcNow;
                _bookingRepository.Update(booking);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Booking {BookingId}: Offered to worker {WorkerId} (Rating: {Rating}, Distance: {Distance}km)",
                    bookingId,
                    nextCandidate.WorkerProfileId,
                    nextCandidate.Score,
                    nextCandidate.DistanceKm
                );

                // Notify via SignalR
                await _bookingHubService.SendStatusUpdateAsync(
                    bookingId,
                    new BookingStatusUpdateDto
                    {
                        BookingId = bookingId,
                        Status = BookingStatus.Pending.ToString(),
                        UpdatedAt = DateTime.UtcNow,
                        Message = "A worker has been found and notified. Waiting for response.",
                    },
                    cancellationToken
                );

                return OperationResult.Success("Worker found and notified");
            }
            else
            {
                // No more candidates — set booking back to Matching
                booking.WorkerProfileId = null;
                booking.Status = BookingStatus.Matching;
                booking.UpdatedDate = DateTime.UtcNow;
                _bookingRepository.Update(booking);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    "Booking {BookingId}: No more candidates available. Status set to Matching.",
                    bookingId
                );

                await _bookingHubService.SendStatusUpdateAsync(
                    bookingId,
                    new BookingStatusUpdateDto
                    {
                        BookingId = bookingId,
                        Status = BookingStatus.Matching.ToString(),
                        UpdatedAt = DateTime.UtcNow,
                        Message =
                            "No workers available at this time. The system is still searching.",
                    },
                    cancellationToken
                );

                return OperationResult.Failure("No workers available at this time");
            }
        }

        /// <summary>
        /// Calculates the great-circle distance between two GPS coordinates using the Haversine formula.
        /// </summary>
        /// <returns>Distance in kilometers.</returns>
        private static double CalculateHaversineDistance(
            double lat1,
            double lng1,
            double lat2,
            double lng2
        )
        {
            const double EarthRadiusKm = 6371.0;

            var dLat = DegreesToRadians(lat2 - lat1);
            var dLng = DegreesToRadians(lng2 - lng1);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(DegreesToRadians(lat1))
                    * Math.Cos(DegreesToRadians(lat2))
                    * Math.Sin(dLng / 2)
                    * Math.Sin(dLng / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }

        private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
    }
}
