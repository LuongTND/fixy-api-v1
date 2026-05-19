using System.Text.Json;
using Application.Common;
using Application.Common.Interfaces;
using Application.DTOs.Booking;
using Application.DTOs.Worker;
using Application.Interfaces.Hubs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Worker;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Worker
{
    public class WorkerLocationService : IWorkerLocationService
    {
        private readonly IDistributedCache _cache;
        private readonly IBookingRepository _bookingRepository;
        private readonly IBookingHubService _bookingHubService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IWorkerProfileRepository _workerProfileRepository;
        private readonly ILogger<WorkerLocationService> _logger;

        // Redis key TTL: location data expires after 2 hours of inactivity
        private static readonly TimeSpan LocationTtl = TimeSpan.FromHours(2);

        public WorkerLocationService(
            IDistributedCache cache,
            IBookingRepository bookingRepository,
            IBookingHubService bookingHubService,
            ICurrentUserService currentUserService,
            IWorkerProfileRepository workerProfileRepository,
            ILogger<WorkerLocationService> logger
        )
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _bookingHubService = bookingHubService ?? throw new ArgumentNullException(nameof(bookingHubService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _workerProfileRepository = workerProfileRepository ?? throw new ArgumentNullException(nameof(workerProfileRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<OperationResult> UpdateLocationAsync(
            Guid userId,
            UpdateWorkerLocationRequest request,
            CancellationToken cancellationToken = default
        )
        {
            // 0. Resolve WorkerProfileId from UserId
            var workerProfile = await _workerProfileRepository.FirstOrDefaultAsync(wp => wp.UserId == userId, cancellationToken);
            if (workerProfile == null)
            {
                return OperationResult.Failure("Worker profile not found");
            }

            var workerId = workerProfile.Id;

            // 1. Persist latest location to Redis
            var locationData = new WorkerLocationUpdateDto
            {
                WorkerId = workerId,
                Lat = request.Lat,
                Lng = request.Lng,
                UpdatedAt = DateTime.UtcNow
            };

            await SaveToRedisAsync(workerId, locationData, cancellationToken);

            // 2. Find the active booking for this worker (Traveling or InProgress)
            var activeBooking = await _bookingRepository.GetActiveBookingByWorkerIdAsync(workerId, cancellationToken);

            if (activeBooking == null)
            {
                // Worker sent a location update but is not on an active booking — still saved to Redis but not broadcast
                _logger.LogDebug("Worker {WorkerId} sent location update but has no active booking to broadcast to.", workerId);
                return OperationResult.Success("Location saved");
            }

            // 3. Broadcast to all clients watching this booking
            locationData.BookingId = activeBooking.Id;

            await _bookingHubService.SendLocationUpdateAsync(activeBooking.Id, locationData, cancellationToken);

            _logger.LogDebug(
                "Worker {WorkerId} location broadcast to booking {BookingId}: ({Lat}, {Lng})",
                workerId, activeBooking.Id, request.Lat, request.Lng
            );

            return OperationResult.Success("Location updated and broadcast");
        }

        /// <inheritdoc />
        public async Task<WorkerLocationUpdateDto?> GetLastLocationAsync(Guid workerId, CancellationToken cancellationToken = default)
        {
            var key = BuildLocationKey(workerId);
            var payload = await _cache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrWhiteSpace(payload))
            {
                return null;
            }

            return JsonSerializer.Deserialize<WorkerLocationUpdateDto>(payload);
        }

        /// <inheritdoc />
        public async Task<OperationResult<BookingTrackingDto>> GetBookingTrackingAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default
        )
        {
            var booking = await _bookingRepository.GetBookingWithWorkerAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                return OperationResult<BookingTrackingDto>.Failure("Booking not found");
            }

            var trackingDto = new BookingTrackingDto
            {
                BookingId = bookingId,
                Status = booking.Status.ToString()
            };

            // Populate worker info if assigned
            if (booking.Worker != null && booking.Worker.User != null)
            {
                trackingDto.WorkerInfo = new WorkerTrackingInfoDto
                {
                    WorkerId = booking.WorkerId ?? Guid.Empty,
                    FullName = booking.Worker.User.FullName,
                    Phone = booking.Worker.User.Phone,
                    RatingAvg = booking.Worker.RatingAvg,
                    // AvatarUrl could be fetched from media repository or a profile field, 
                    // assuming for now it's not directly in User entity or handled elsewhere
                };
            }

            // Try to load the worker's last known location from Redis
            if (booking.WorkerId.HasValue)
            {
                var lastLocation = await GetLastLocationAsync(booking.WorkerId.Value, cancellationToken);
                if (lastLocation != null)
                {
                    trackingDto.WorkerLat = lastLocation.Lat;
                    trackingDto.WorkerLng = lastLocation.Lng;
                    trackingDto.LocationUpdatedAt = lastLocation.UpdatedAt;
                }
            }

            return OperationResult<BookingTrackingDto>.Success(trackingDto, "Tracking info retrieved");
        }

        // -------------------------
        // Private helpers
        // -------------------------

        private async Task SaveToRedisAsync(Guid workerId, WorkerLocationUpdateDto data, CancellationToken cancellationToken)
        {
            var key = BuildLocationKey(workerId);
            var payload = JsonSerializer.Serialize(data);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = LocationTtl
            };

            await _cache.SetStringAsync(key, payload, options, cancellationToken);
        }

        /// <summary>Redis key pattern: worker:{workerId}:location</summary>
        private static string BuildLocationKey(Guid workerId) => $"worker:{workerId}:location";
    }
}
