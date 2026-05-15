using Application.Common;
using Application.Common.Interfaces;
using Application.DTOs.Booking;
using Application.Interfaces;
using Application.Interfaces.Hubs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Booking;
using Domain.Enum;
using Microsoft.Extensions.Logging;
using AutoMapper;
using BookingEntity = Domain.Entity.Booking;

namespace Infrastructure.Services.Booking
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBookingHubService _bookingHubService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly IMediaRepository _mediaRepository;
        private readonly ILogger<BookingService> _logger;

        /// <summary>
        /// Defines valid state transitions: CurrentStatus -> AllowedNextStatus.
        /// </summary>
        private static readonly Dictionary<BookingStatus, BookingStatus> AllowedTransitions = new()
        {
            { BookingStatus.Pending,    BookingStatus.Confirmed },
            { BookingStatus.Confirmed,  BookingStatus.Traveling },
            { BookingStatus.Traveling,  BookingStatus.Arrived },
            { BookingStatus.Arrived,    BookingStatus.InProgress },
            { BookingStatus.InProgress, BookingStatus.Completed },
        };

        public BookingService(
            IBookingRepository bookingRepository,
            IUnitOfWork unitOfWork,
            IBookingHubService bookingHubService,
            ICurrentUserService currentUserService,
            IMediaRepository mediaRepository,
            IMapper mapper,
            ILogger<BookingService> logger
        )
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _bookingHubService = bookingHubService ?? throw new ArgumentNullException(nameof(bookingHubService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OperationResult<BookingDetailDto>> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                return OperationResult<BookingDetailDto>.Failure("Booking not found");
            }

            var dto = _mapper.Map<BookingDetailDto>(booking);

            return OperationResult<BookingDetailDto>.Success(dto, "Booking retrieved successfully");
        }

        public async Task<OperationResult> AcceptAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            return await TransitionStatusAsync(
                bookingId,
                BookingStatus.Pending,
                BookingStatus.Confirmed,
                "Worker accepted the booking",
                (booking) =>
                {
                    // Assign the current user (worker) to the booking if not already assigned
                    if (booking.WorkerId == null && Guid.TryParse(_currentUserService.UserId, out var workerId))
                    {
                        booking.WorkerId = workerId;
                    }
                    return Task.CompletedTask;
                },
                cancellationToken
            );
        }

        public async Task<OperationResult> StartTravelAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            return await TransitionStatusAsync(
                bookingId,
                BookingStatus.Confirmed,
                BookingStatus.Traveling,
                "Worker is on the way",
                null,
                cancellationToken
            );
        }

        public async Task<OperationResult> ArriveAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            return await TransitionStatusAsync(
                bookingId,
                BookingStatus.Traveling,
                BookingStatus.Arrived,
                "Worker has arrived",
                null,
                cancellationToken
            );
        }

        public async Task<OperationResult> StartWorkAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            return await TransitionStatusAsync(
                bookingId,
                BookingStatus.Arrived,
                BookingStatus.InProgress,
                "Work in progress",
                null,
                cancellationToken
            );
        }

        public async Task<OperationResult> CompleteAsync(Guid bookingId, CompleteBookingRequest request, CancellationToken cancellationToken = default)
        {
            return await TransitionStatusAsync(
                bookingId,
                BookingStatus.InProgress,
                BookingStatus.Completed,
                "Booking completed",
                async (booking) =>
                {
                    booking.CompletedAt = DateTime.UtcNow;

                    // Associate media if any
                    if (request.MediaIds != null && request.MediaIds.Any())
                    {
                        var medias = await _mediaRepository.FindAsync(m => request.MediaIds.Contains(m.Id), cancellationToken);
                        foreach (var media in medias)
                        {
                            media.OwnerId = bookingId;
                            media.OwnerType = MediaOwnerType.Booking;
                            media.Category = MediaCategory.Completion;
                            _mediaRepository.Update(media);
                        }
                    }
                },
                cancellationToken
            );
        }

        /// <summary>
        /// Core state machine: validates the current status, transitions to the next status,
        /// saves to DB, and broadcasts the change via SignalR.
        /// </summary>
        private async Task<OperationResult> TransitionStatusAsync(
            Guid bookingId,
            BookingStatus expectedCurrentStatus,
            BookingStatus newStatus,
            string message,
            Func<BookingEntity, Task>? onTransition,
            CancellationToken cancellationToken
        )
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                _logger.LogWarning("Booking not found. Id: {BookingId}", bookingId);
                return OperationResult.Failure("Booking not found");
            }

            // Validate: current status must match expected
            if (booking.Status != expectedCurrentStatus)
            {
                _logger.LogWarning(
                    "Invalid status transition for booking {BookingId}. Current: {CurrentStatus}, Expected: {ExpectedStatus}, Target: {TargetStatus}",
                    bookingId, booking.Status, expectedCurrentStatus, newStatus
                );
                return OperationResult.Failure(
                    $"Cannot transition from '{booking.Status}' to '{newStatus}'. Expected current status: '{expectedCurrentStatus}'."
                );
            }

            // Validate: ensure this transition is allowed in the state machine
            if (!AllowedTransitions.TryGetValue(expectedCurrentStatus, out var allowedNext) || allowedNext != newStatus)
            {
                return OperationResult.Failure($"Transition from '{expectedCurrentStatus}' to '{newStatus}' is not allowed.");
            }

            // Apply transition
            booking.Status = newStatus;
            booking.UpdatedDate = DateTime.UtcNow;
            if (onTransition != null)
            {
                await onTransition(booking);
            }

            _bookingRepository.Update(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Booking {BookingId} transitioned from {OldStatus} to {NewStatus}",
                bookingId, expectedCurrentStatus, newStatus
            );

            // Broadcast status change via SignalR
            await _bookingHubService.SendStatusUpdateAsync(
                bookingId,
                new BookingStatusUpdateDto
                {
                    BookingId = bookingId,
                    Status = newStatus.ToString(),
                    UpdatedAt = DateTime.UtcNow,
                    Message = message
                },
                cancellationToken
            );

            return OperationResult.Success(message);
        }
    }
}
