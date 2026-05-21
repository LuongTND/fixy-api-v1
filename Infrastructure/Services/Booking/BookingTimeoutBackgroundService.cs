using Application.DTOs.Booking;
using Application.Interfaces;
using Application.Interfaces.Hubs;
using Application.Interfaces.Repositories;
using Domain.Enum;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Booking
{
    /// <summary>
    /// Background service that periodically scans the WorkerMatchingQueue
    /// for expired offers (workers who didn't respond within the timeout)
    /// and automatically re-routes the booking to the next available worker.
    /// </summary>
    public class BookingTimeoutBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingTimeoutBackgroundService> _logger;

        /// <summary>
        /// How often the service checks for expired offers.
        /// </summary>
        private static readonly TimeSpan ScanInterval = TimeSpan.FromSeconds(30);

        public BookingTimeoutBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<BookingTimeoutBackgroundService> logger
        )
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingTimeoutBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpiredOffersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing expired booking offers.");
                }

                await Task.Delay(ScanInterval, stoppingToken);
            }

            _logger.LogInformation("BookingTimeoutBackgroundService stopped.");
        }

        private async Task ProcessExpiredOffersAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var matchingQueueRepository =
                scope.ServiceProvider.GetRequiredService<IWorkerMatchingQueueRepository>();
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var bookingHubService = scope.ServiceProvider.GetRequiredService<IBookingHubService>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var expiredEntries = await matchingQueueRepository.GetExpiredEntriesAsync(
                cancellationToken
            );

            if (expiredEntries.Count == 0)
            {
                return;
            }

            _logger.LogInformation(
                "Found {Count} expired booking offer(s). Processing...",
                expiredEntries.Count
            );

            foreach (var entry in expiredEntries)
            {
                try
                {
                    // Mark as timed out
                    entry.Status = MatchingStatus.Timeout;
                    entry.RespondedAt = DateTime.UtcNow;
                    matchingQueueRepository.Update(entry);

                    // Try to offer to the next candidate
                    var nextCandidate = await matchingQueueRepository.GetNextCandidateAsync(
                        entry.BookingId,
                        cancellationToken
                    );

                    if (nextCandidate != null)
                    {
                        nextCandidate.Status = MatchingStatus.Offered;
                        nextCandidate.OfferedAt = DateTime.UtcNow;
                        nextCandidate.ExpiresAt = DateTime.UtcNow.AddMinutes(15); // Default timeout
                        matchingQueueRepository.Update(nextCandidate);

                        _logger.LogInformation(
                            "Booking {BookingId}: Worker {OldWorkerId} timed out. Offered to next worker {NewWorkerId}.",
                            entry.BookingId,
                            entry.WorkerId,
                            nextCandidate.WorkerId
                        );
                    }
                    else
                    {
                        // No more candidates — set booking back to Matching
                        if (entry.Booking != null)
                        {
                            entry.Booking.Status = BookingStatus.Matching;
                            entry.Booking.WorkerProfileId = null;
                            entry.Booking.UpdatedDate = DateTime.UtcNow;
                            bookingRepository.Update(entry.Booking);
                        }

                        _logger.LogWarning(
                            "Booking {BookingId}: Worker {WorkerId} timed out. No more candidates available.",
                            entry.BookingId,
                            entry.WorkerId
                        );
                    }

                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    // Notify via SignalR
                    await bookingHubService.SendStatusUpdateAsync(
                        entry.BookingId,
                        new BookingStatusUpdateDto
                        {
                            BookingId = entry.BookingId,
                            Status = nextCandidate != null ? "Matching" : "Matching",
                            UpdatedAt = DateTime.UtcNow,
                            Message =
                                nextCandidate != null
                                    ? "Previous worker did not respond in time. Finding another worker..."
                                    : "No workers available. The system is searching for new candidates.",
                        },
                        cancellationToken
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error processing expired offer for booking {BookingId}, worker {WorkerId}.",
                        entry.BookingId,
                        entry.WorkerId
                    );
                }
            }
        }
    }
}
