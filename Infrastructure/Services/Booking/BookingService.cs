using Application.Common;
using Application.Common.Interfaces;
using Application.DTOs.Booking;
using Application.DTOs.Support;
using Application.Interfaces;
using Application.Interfaces.Hubs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Booking;
using AutoMapper;
using Domain.Enum;
using Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
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
        private readonly ISupportTicketRepository _supportTicketRepository;
        private readonly IWorkerMatchingQueueRepository _matchingQueueRepository;
        private readonly IWorkerProfileRepository _workerProfileRepository;
        private readonly ILogger<BookingService> _logger;

        /// <summary>
        /// Defines valid state transitions: CurrentStatus -> AllowedNextStatus.
        /// </summary>
        private static readonly Dictionary<BookingStatus, BookingStatus> AllowedTransitions = new()
        {
            { BookingStatus.Pending,         BookingStatus.PendingPayment },
            { BookingStatus.PendingPayment,  BookingStatus.Confirmed },
            { BookingStatus.Confirmed,       BookingStatus.Traveling },
            { BookingStatus.Traveling,       BookingStatus.Arrived },
            { BookingStatus.Arrived,         BookingStatus.InProgress },
            { BookingStatus.InProgress,      BookingStatus.Completed },
        };

        public BookingService(
            IBookingRepository bookingRepository,
            IUnitOfWork unitOfWork,
            IBookingHubService bookingHubService,
            ICurrentUserService currentUserService,
            IMediaRepository mediaRepository,
            ISupportTicketRepository supportTicketRepository,
            IWorkerMatchingQueueRepository matchingQueueRepository,
            IWorkerProfileRepository workerProfileRepository,
            IMapper mapper,
            ILogger<BookingService> logger
        )
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _workerProfileRepository = workerProfileRepository ?? throw new ArgumentNullException(nameof(workerProfileRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _bookingHubService = bookingHubService ?? throw new ArgumentNullException(nameof(bookingHubService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
            _supportTicketRepository = supportTicketRepository ?? throw new ArgumentNullException(nameof(supportTicketRepository));
            _matchingQueueRepository = matchingQueueRepository ?? throw new ArgumentNullException(nameof(matchingQueueRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OperationResult<BookingDetailDto>> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            var booking = await _bookingRepository.GetBookingWithWorkerAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                return OperationResult<BookingDetailDto>.Failure("Booking not found");
            }

            var dto = _mapper.Map<BookingDetailDto>(booking);

            return OperationResult<BookingDetailDto>.Success(dto, "Booking retrieved successfully");
        }

        public async Task<OperationResult> AcceptAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(_currentUserService.UserId, out var userId))
            {
                return OperationResult.Failure("User not authenticated");
            }

            var workerProfile = await _workerProfileRepository.FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
            if (workerProfile == null)
            {
                return OperationResult.Failure("Worker profile not found");
            }

            return await TransitionStatusAsync(
                bookingId,
                BookingStatus.Pending,
                BookingStatus.PendingPayment,
                "Worker accepted the booking. Awaiting payment.",
                (booking) =>
                {
                    // Assign the worker profile to the booking if not already assigned
                    if (booking.WorkerId == null)
                    {
                        booking.WorkerId = workerProfile.Id;
                    }
                    booking.FinalPrice = booking.EstimatedPrice;
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

        public async Task<OperationResult<SupportTicketDto>> ReportIssueAsync(Guid bookingId, ReportBookingIssueRequest request, CancellationToken cancellationToken = default)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            if (booking == null)
            {
                return OperationResult<SupportTicketDto>.Failure("Booking not found");
            }

            if (!Guid.TryParse(_currentUserService.UserId, out var reporterId))
            {
                return OperationResult<SupportTicketDto>.Failure("User not authenticated");
            }

            var ticket = new Domain.Entity.SupportTicket
            {
                BookingId = bookingId,
                ReporterId = reporterId,
                ReporterType = SupportReporterType.Worker,
                Category = request.Category,
                Subject = request.Subject,
                Priority = request.Priority,
                Status = SupportStatus.Open,
                CreatedDate = DateTime.UtcNow,
                Messages = new List<Domain.Entity.SupportMessage>
                {
                    new Domain.Entity.SupportMessage
                    {
                        SenderId = reporterId,
                        Content = request.Description,
                        CreatedDate = DateTime.UtcNow
                    }
                }
            };

            await _supportTicketRepository.AddAsync(ticket, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Issue reported for booking {BookingId} by worker {WorkerId}", bookingId, reporterId);

            var ticketDto = _mapper.Map<SupportTicketDto>(ticket);
            return OperationResult<SupportTicketDto>.Success(ticketDto, "Issue reported successfully. Support team will contact you.");
        }

        public async Task<OperationResult> DeclineAsync(Guid bookingId, DeclineBookingRequest request, CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(_currentUserService.UserId, out var userId))
            {
                return OperationResult.Failure("User not authenticated");
            }

            var workerProfile = await _workerProfileRepository.FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
            if (workerProfile == null)
            {
                return OperationResult.Failure("Worker profile not found");
            }
            var workerId = workerProfile.Id;

            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            if (booking == null)
            {
                return OperationResult.Failure("Booking not found");
            }

            // Find the offered queue entry for this worker
            var queueEntry = await _matchingQueueRepository.GetOfferedEntryAsync(bookingId, workerId, cancellationToken);
            if (queueEntry == null)
            {
                return OperationResult.Failure("No active offer found for this worker and booking");
            }

            // Mark as rejected
            queueEntry.Status = MatchingStatus.Rejected;
            queueEntry.RejectReason = request.RejectReason;
            queueEntry.RespondedAt = DateTime.UtcNow;
            _matchingQueueRepository.Update(queueEntry);

            // Đưa đơn hàng quay lại trạng thái tìm thợ (Matching) để Khách hàng chọn lại thợ khác
            booking.WorkerId = null;
            booking.Status = BookingStatus.Matching;
            booking.UpdatedDate = DateTime.UtcNow;
            _bookingRepository.Update(booking);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Worker {WorkerId} declined booking {BookingId}. Reason: {Reason}. Reverted booking to Matching status.",
                workerId, bookingId, request.RejectReason);

            // Notify via SignalR
            await _bookingHubService.SendStatusUpdateAsync(
                bookingId,
                new BookingStatusUpdateDto
                {
                    BookingId = bookingId,
                    Status = BookingStatus.Matching.ToString(),
                    UpdatedAt = DateTime.UtcNow,
                    Message = "Worker declined the offer. Reverted to matching for customer selection."
                },
                cancellationToken
            );

            return OperationResult.Success("Booking declined successfully");
        }

        public async Task<OperationResult> ProposeAsync(Guid bookingId, ProposeBookingRequest request, CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(_currentUserService.UserId, out var userId))
            {
                return OperationResult.Failure("User not authenticated");
            }
            var workerProfile = await _workerProfileRepository.FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
            if (workerProfile == null)
            {
                return OperationResult.Failure("Worker profile not found");
            }
            var workerId = workerProfile.Id;

            if (request.ProposedPrice == null && request.ProposedTime == null)
            {
                return OperationResult.Failure("At least one of ProposedPrice or ProposedTime must be provided");
            }

            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            if (booking == null)
            {
                return OperationResult.Failure("Booking not found");
            }

            // Verify the worker is indeed authorized to propose for this booking and it is in Pending status
            if (booking.WorkerId != workerId || booking.Status != BookingStatus.Pending)
            {
                return OperationResult.Failure("You are not authorized to propose changes for this booking or the booking is not pending");
            }

            // Store the worker's counter-proposal on the booking
            booking.WorkerProposedPrice = request.ProposedPrice;
            booking.WorkerProposedTime = request.ProposedTime;
            booking.WorkerProposedNote = request.ProposedNote;
            booking.UpdatedDate = DateTime.UtcNow;
            _bookingRepository.Update(booking);

            // Pause the timeout if this was an auto-match queue entry (optional)
            var queueEntry = await _matchingQueueRepository.GetOfferedEntryAsync(bookingId, workerId, cancellationToken);
            if (queueEntry != null)
            {
                queueEntry.ExpiresAt = null;
                _matchingQueueRepository.Update(queueEntry);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Worker {WorkerId} proposed changes for booking {BookingId}. Price: {Price}, Time: {Time}",
                workerId, bookingId, request.ProposedPrice, request.ProposedTime);

            // Notify customer via SignalR
            await _bookingHubService.SendStatusUpdateAsync(
                bookingId,
                new BookingStatusUpdateDto
                {
                    BookingId = bookingId,
                    Status = booking.Status.ToString(),
                    UpdatedAt = DateTime.UtcNow,
                    Message = "Worker has proposed alternative price/time. Please review."
                },
                cancellationToken
            );

            return OperationResult.Success("Proposal submitted successfully");
        }

        public async Task<OperationResult> RespondProposalAsync(Guid bookingId, RespondProposalRequest request, CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(_currentUserService.UserId, out var customerId))
            {
                return OperationResult.Failure("User not authenticated");
            }

            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            if (booking == null)
            {
                return OperationResult.Failure("Booking not found");
            }

            // Verify this is the customer's booking
            if (booking.CustomerId != customerId)
            {
                return OperationResult.Failure("You are not authorized to respond to this booking's proposal");
            }

            // Verify there is a pending proposal
            if (booking.WorkerProposedPrice == null && booking.WorkerProposedTime == null)
            {
                return OperationResult.Failure("No pending proposal found for this booking");
            }

            if (booking.WorkerId == null)
            {
                return OperationResult.Failure("No worker assigned to this booking");
            }

            var queueEntry = await _matchingQueueRepository.GetOfferedEntryAsync(bookingId, booking.WorkerId.Value, cancellationToken);

            if (request.Accept)
            {
                // Customer accepts the proposal
                booking.FinalPrice = booking.WorkerProposedPrice ?? booking.EstimatedPrice;
                booking.ScheduledAt = booking.WorkerProposedTime ?? booking.ScheduledAt;
                booking.Status = BookingStatus.PendingPayment;
                booking.UpdatedDate = DateTime.UtcNow;
                _bookingRepository.Update(booking);

                if (queueEntry != null)
                {
                    queueEntry.Status = MatchingStatus.Accepted;
                    queueEntry.RespondedAt = DateTime.UtcNow;
                    _matchingQueueRepository.Update(queueEntry);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Customer {CustomerId} accepted proposal for booking {BookingId}",
                    customerId, bookingId);

                await _bookingHubService.SendStatusUpdateAsync(
                    bookingId,
                    new BookingStatusUpdateDto
                    {
                        BookingId = bookingId,
                        Status = BookingStatus.PendingPayment.ToString(),
                        UpdatedAt = DateTime.UtcNow,
                        Message = "Customer accepted the proposal. Awaiting payment."
                    },
                    cancellationToken
                );

                return OperationResult.Success("Proposal accepted. Awaiting payment.");
            }
            else
            {
                // Customer rejects the proposal — clear proposal fields and revert booking to Matching state
                booking.WorkerProposedPrice = null;
                booking.WorkerProposedTime = null;
                booking.WorkerProposedNote = null;
                booking.WorkerId = null;
                booking.Status = BookingStatus.Matching;
                booking.UpdatedDate = DateTime.UtcNow;
                _bookingRepository.Update(booking);

                if (queueEntry != null)
                {
                    queueEntry.Status = MatchingStatus.Rejected;
                    queueEntry.RejectReason = request.RejectReason ?? "Customer rejected the proposal";
                    queueEntry.RespondedAt = DateTime.UtcNow;
                    _matchingQueueRepository.Update(queueEntry);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Customer {CustomerId} rejected proposal for booking {BookingId}. Reason: {Reason}. Reverted booking to Matching status.",
                    customerId, bookingId, request.RejectReason);

                await _bookingHubService.SendStatusUpdateAsync(
                    bookingId,
                    new BookingStatusUpdateDto
                    {
                        BookingId = bookingId,
                        Status = BookingStatus.Matching.ToString(),
                        UpdatedAt = DateTime.UtcNow,
                        Message = "Proposal rejected. Reverted to matching for customer selection."
                    },
                    cancellationToken
                );

                return OperationResult.Success("Proposal rejected. Reverted to matching for customer selection.");
            }
        }

        /// <summary>
        /// Attempts to offer the booking to the next candidate worker in the matching queue.
        /// If no candidates remain, the booking status is set back to Matching.
        /// </summary>
        private async Task OfferToNextWorkerAsync(BookingEntity booking, CancellationToken cancellationToken)
        {
            var nextCandidate = await _matchingQueueRepository.GetNextCandidateAsync(booking.Id, cancellationToken);

            if (nextCandidate != null)
            {
                nextCandidate.Status = MatchingStatus.Offered;
                nextCandidate.OfferedAt = DateTime.UtcNow;
                nextCandidate.ExpiresAt = DateTime.UtcNow.AddMinutes(15); // Default timeout, will be read from PlatformConfig in the future
                _matchingQueueRepository.Update(nextCandidate);

                _logger.LogInformation("Offered booking {BookingId} to next worker {WorkerId}",
                    booking.Id, nextCandidate.WorkerId);
            }
            else
            {
                // No more candidates — set booking back to Matching so the system can re-search
                booking.Status = BookingStatus.Matching;
                booking.UpdatedDate = DateTime.UtcNow;
                _bookingRepository.Update(booking);

                _logger.LogWarning("No more candidates for booking {BookingId}. Status set to Matching.",
                    booking.Id);
            }
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

        public async Task<OperationResult<PagedResponse<BookingDetailDto>>> GetWorkerBookingsAsync(
            WorkerBookingsQuery query,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_currentUserService.UserId) || !Guid.TryParse(_currentUserService.UserId, out var userId))
            {
                return OperationResult<PagedResponse<BookingDetailDto>>.Failure("User ID not found in token");
            }

            var workerProfile = await _workerProfileRepository.FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
            if (workerProfile == null)
            {
                return OperationResult<PagedResponse<BookingDetailDto>>.Failure("Worker profile not found");
            }

            var (items, totalCount) = await _bookingRepository.GetWorkerBookingsAsync(workerProfile.Id, query, cancellationToken);

            var dtos = _mapper.Map<List<BookingDetailDto>>(items);

            var response = new PagedResponse<BookingDetailDto>
            {
                Items = dtos,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };

            return OperationResult<PagedResponse<BookingDetailDto>>.Success(response, "Worker bookings retrieved successfully");
        }

        public async Task<OperationResult<PagedResponse<BookingDetailDto>>> GetCustomerBookingsAsync(
            CustomerBookingsQuery query,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_currentUserService.UserId) || !Guid.TryParse(_currentUserService.UserId, out var userId))
            {
                return OperationResult<PagedResponse<BookingDetailDto>>.Failure("User ID not found in token");
            }

            var (items, totalCount) = await _bookingRepository.GetCustomerBookingsAsync(userId, query, cancellationToken);

            var dtos = _mapper.Map<List<BookingDetailDto>>(items);

            var response = new PagedResponse<BookingDetailDto>
            {
                Items = dtos,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };

            return OperationResult<PagedResponse<BookingDetailDto>>.Success(response, "Customer bookings retrieved successfully");
        }
    }
}
