using Application.Common;
using Application.Common.Interfaces;
using Application.DTOs.Booking;
using Application.DTOs.Media;
using Application.DTOs.Support;
using Application.Interfaces;
using Application.Interfaces.Hubs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Booking;
using AutoMapper;
using Domain.Entity;
using Domain.Enum;
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
        private readonly ICustomerProfileRepository _customerProfileRepository;
        private readonly IWorkerProfileRepository _workerProfileRepository;
        private readonly IWorkerMatchingService _workerMatchingService;
        private readonly ILogger<BookingService> _logger;

        private static readonly Dictionary<BookingStatus, BookingStatus> AllowedTransitions = new()
        {
            { BookingStatus.Pending, BookingStatus.PendingPayment },
            { BookingStatus.PendingPayment, BookingStatus.Confirmed },
            { BookingStatus.Confirmed, BookingStatus.Traveling },
            { BookingStatus.Traveling, BookingStatus.Arrived },
            { BookingStatus.Arrived, BookingStatus.InProgress },
            { BookingStatus.InProgress, BookingStatus.Completed },
        };

        public BookingService(
            IBookingRepository bookingRepository,
            IUnitOfWork unitOfWork,
            IBookingHubService bookingHubService,
            ICurrentUserService currentUserService,
            IMediaRepository mediaRepository,
            ISupportTicketRepository supportTicketRepository,
            IWorkerMatchingQueueRepository matchingQueueRepository,
            ICustomerProfileRepository customerProfileRepository,
            IWorkerProfileRepository workerProfileRepository,
            IWorkerMatchingService workerMatchingService,
            IMapper mapper,
            ILogger<BookingService> logger
        )
        {
            _bookingRepository =
                bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _workerProfileRepository =
                workerProfileRepository
                ?? throw new ArgumentNullException(nameof(workerProfileRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _bookingHubService =
                bookingHubService ?? throw new ArgumentNullException(nameof(bookingHubService));
            _currentUserService =
                currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _mediaRepository =
                mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
            _supportTicketRepository =
                supportTicketRepository
                ?? throw new ArgumentNullException(nameof(supportTicketRepository));
            _matchingQueueRepository =
                matchingQueueRepository
                ?? throw new ArgumentNullException(nameof(matchingQueueRepository));
            _workerMatchingService =
                workerMatchingService
                ?? throw new ArgumentNullException(nameof(workerMatchingService));
            _customerProfileRepository = customerProfileRepository;
            _workerProfileRepository = workerProfileRepository;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // =========================================================
        // HELPERS
        // =========================================================

        private async Task<CustomerProfile?> GetCurrentCustomerProfileAsync(
            CancellationToken cancellationToken
        )
        {
            if (!Guid.TryParse(_currentUserService.UserId, out var userId))
            {
                return null;
            }

            return await _customerProfileRepository.FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken
            );
        }

        private async Task<WorkerProfile?> GetCurrentWorkerProfileAsync(
            CancellationToken cancellationToken
        )
        {
            if (!Guid.TryParse(_currentUserService.UserId, out var userId))
            {
                return null;
            }

            return await _workerProfileRepository.FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken
            );
        }

        // =========================================================
        // GET BY ID
        // =========================================================

        public async Task<OperationResult<BookingDetailDto>> GetByIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default
        )
        {
            var booking = await _bookingRepository.GetBookingWithWorkerAsync(
                bookingId,
                cancellationToken
            );

            if (booking == null)
            {
                return OperationResult<BookingDetailDto>.Failure("Booking not found");
            }

            var dto = _mapper.Map<BookingDetailDto>(booking);
            var requestImages = await _mediaRepository.GetBookingRequestImagesAsync(
                bookingId,
                cancellationToken
            );
            var completeImages = await _mediaRepository.GetBookingCompletionImagesAsync(
                bookingId,
                cancellationToken
            );
            dto.RequestImages = requestImages
                .Select(x => new MediaDto
                {
                    Id = x.Id,
                    OwnerId = x.OwnerId,
                    FileUrl = x.FileUrl,
                })
                .ToList();
            dto.CompleteImages = completeImages
                .Select(x => new MediaDto
                {
                    Id = x.Id,
                    OwnerId = x.OwnerId,
                    FileUrl = x.FileUrl,
                })
                .ToList();
            return OperationResult<BookingDetailDto>.Success(dto, "Get booking successfully");
        }

        //=================================
        // ACCEPT
        // =========================================================

        public async Task<OperationResult> AcceptAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default
        )
        {
            var workerProfile = await GetCurrentWorkerProfileAsync(cancellationToken);

            if (workerProfile == null)
            {
                return OperationResult.Failure("Worker profile not found");
            }

            return await TransitionStatusAsync(
                bookingId,
                BookingStatus.Pending,
                BookingStatus.PendingPayment,
                "Worker accepted the booking. Awaiting payment.",
                async (booking) =>
                {
                    booking.WorkerProfileId = workerProfile.Id;
                    booking.FinalPrice = booking.EstimatedPrice;

                    // Update matching queue entry to Accepted to prevent timeout false-positive
                    var queueEntry = await _matchingQueueRepository.GetOfferedEntryAsync(
                        bookingId,
                        workerProfile.Id,
                        cancellationToken
                    );
                    if (queueEntry != null)
                    {
                        queueEntry.Status = MatchingStatus.Accepted;
                        queueEntry.RespondedAt = DateTime.UtcNow;
                        _matchingQueueRepository.Update(queueEntry);
                    }
                },
                cancellationToken
            );
        }

        // =========================================================
        // PAYMENT CONFIRM
        // =========================================================

        public async Task<OperationResult> ConfirmPaymentAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default
        )
        {
            return await TransitionStatusAsync(
                bookingId,
                BookingStatus.PendingPayment,
                BookingStatus.Confirmed,
                "Payment successful. Booking confirmed.",
                null,
                cancellationToken
            );
        }

        // =========================================================
        // START TRAVEL
        // =========================================================

        public async Task<OperationResult> StartTravelAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default
        )
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

        // =========================================================
        // ARRIVE
        // =========================================================

        public async Task<OperationResult> ArriveAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default
        )
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

        // =========================================================
        // START WORK
        // =========================================================

        public async Task<OperationResult> StartWorkAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default
        )
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

        // =========================================================
        // COMPLETE
        // =========================================================

        public async Task<OperationResult> CompleteAsync(
            Guid bookingId,
            CompleteBookingRequest request,
            CancellationToken cancellationToken = default
        )
        {
            return await TransitionStatusAsync(
                bookingId,
                BookingStatus.InProgress,
                BookingStatus.Completed,
                "Booking completed",
                async booking =>
                {
                    booking.CompletedAt = DateTime.UtcNow;

                    if (request.MediaIds != null && request.MediaIds.Any())
                    {
                        var medias = await _mediaRepository.FindAsync(
                            x => request.MediaIds.Contains(x.Id),
                            cancellationToken
                        );

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

        // =========================================================
        // DECLINE
        // =========================================================

        public async Task<OperationResult> DeclineAsync(
            Guid bookingId,
            DeclineBookingRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var workerProfile = await GetCurrentWorkerProfileAsync(cancellationToken);

            if (workerProfile == null)
            {
                return OperationResult.Failure("Worker profile not found");
            }

            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                return OperationResult.Failure("Booking not found");
            }

            if (booking.WorkerProfileId != workerProfile.Id)
            {
                return OperationResult.Failure("Forbidden");
            }
            var queueEntry = await _matchingQueueRepository.GetOfferedEntryAsync(
                bookingId,
                workerProfile.Id,
                cancellationToken
            );
            if (queueEntry == null)
            {
                return OperationResult.Failure("No active offer found for this worker and booking");
            }

            // Mark as rejected
            queueEntry.Status = MatchingStatus.Rejected;
            queueEntry.RejectReason = request.RejectReason;
            queueEntry.RespondedAt = DateTime.UtcNow;
            _matchingQueueRepository.Update(queueEntry);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Worker {WorkerId} declined booking {BookingId}. Reason: {Reason}. Auto-forwarding to next worker.",
                workerProfile.Id,
                bookingId,
                request.RejectReason
            );

            // Auto-forward to next worker in the matching queue
            await _workerMatchingService.OfferToNextWorkerAsync(bookingId, cancellationToken);

            return OperationResult.Success("Booking declined successfully");
        }

        // =========================================================
        // PROPOSE
        // =========================================================

        public async Task<OperationResult> ProposeAsync(
            Guid bookingId,
            ProposeBookingRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var workerProfile = await GetCurrentWorkerProfileAsync(cancellationToken);

            if (workerProfile == null)
            {
                return OperationResult.Failure("Worker profile not found");
            }

            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                return OperationResult.Failure("Booking not found");
            }

            if (booking.WorkerProfileId != workerProfile.Id)
            {
                return OperationResult.Failure("Forbidden");
            }

            booking.WorkerProposedPrice = request.ProposedPrice;
            booking.WorkerProposedTime = request.ProposedTime;
            booking.WorkerProposedNote = request.ProposedNote;
            booking.UpdatedDate = DateTime.UtcNow;

            _bookingRepository.Update(booking);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult.Success("Proposal submitted successfully");
        }

        // =========================================================
        // RESPOND PROPOSAL
        // =========================================================

        public async Task<OperationResult> RespondProposalAsync(
            Guid bookingId,
            RespondProposalRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var customerProfile = await GetCurrentCustomerProfileAsync(cancellationToken);

            if (customerProfile == null)
            {
                return OperationResult.Failure("Customer profile not found");
            }

            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                return OperationResult.Failure("Booking not found");
            }

            // IMPORTANT
            if (booking.CustomerProfileId != customerProfile.Id)
            {
                return OperationResult.Failure("Forbidden");
            }

            if (request.Accept)
            {
                booking.FinalPrice = booking.WorkerProposedPrice ?? booking.EstimatedPrice;

                booking.ScheduledAt = booking.WorkerProposedTime ?? booking.ScheduledAt;

                booking.Status = BookingStatus.PendingPayment;
            }
            else
            {
                booking.WorkerProfileId = null;

                booking.WorkerProposedPrice = null;
                booking.WorkerProposedTime = null;
                booking.WorkerProposedNote = null;

                booking.Status = BookingStatus.Matching;
            }

            booking.UpdatedDate = DateTime.UtcNow;

            _bookingRepository.Update(booking);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult.Success(
                request.Accept ? "Proposal accepted" : "Proposal rejected"
            );
        }

        // =========================================================
        // REPORT ISSUE
        // =========================================================

        public async Task<OperationResult<SupportTicketDto>> ReportIssueAsync(
            Guid bookingId,
            ReportBookingIssueRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var workerProfile = await GetCurrentWorkerProfileAsync(cancellationToken);

            if (workerProfile == null)
            {
                return OperationResult<SupportTicketDto>.Failure("Worker profile not found");
            }

            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                return OperationResult<SupportTicketDto>.Failure("Booking not found");
            }

            if (booking.WorkerProfileId != workerProfile.Id)
            {
                return OperationResult<SupportTicketDto>.Failure("Forbidden");
            }

            var ticket = new SupportTicket
            {
                BookingId = bookingId,
                ReporterId = workerProfile.Id,
                ReporterType = SupportReporterType.Worker,
                Subject = request.Subject,
                Category = request.Category,
                Priority = request.Priority,
                Status = SupportStatus.Open,
                CreatedDate = DateTime.UtcNow,
                Messages = new List<SupportMessage>
                {
                    new()
                    {
                        SenderId = workerProfile.Id,
                        Content = request.Description,
                        CreatedDate = DateTime.UtcNow,
                    },
                },
            };

            await _supportTicketRepository.AddAsync(ticket, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<SupportTicketDto>(ticket);

            return OperationResult<SupportTicketDto>.Success(dto, "Issue reported successfully");
        }

        // =========================================================
        // GET WORKER BOOKINGS
        // =========================================================

        public async Task<OperationResult<PagedResponse<BookingDetailDto>>> GetWorkerBookingsAsync(
            WorkerBookingsQuery query,
            CancellationToken cancellationToken = default
        )
        {
            var workerProfile = await GetCurrentWorkerProfileAsync(cancellationToken);

            if (workerProfile == null)
            {
                return OperationResult<PagedResponse<BookingDetailDto>>.Failure(
                    "Worker profile not found"
                );
            }

            var (items, totalCount) = await _bookingRepository.GetWorkerBookingsAsync(
                workerProfile.Id,
                query,
                cancellationToken
            );

            var dtos = _mapper.Map<List<BookingDetailDto>>(items);

            return OperationResult<PagedResponse<BookingDetailDto>>.Success(
                new PagedResponse<BookingDetailDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                },
                "Worker bookings retrieved successfully"
            );
        }

        // =========================================================
        // GET CUSTOMER BOOKINGS
        // =========================================================

        public async Task<
            OperationResult<PagedResponse<BookingDetailDto>>
        > GetCustomerBookingsAsync(
            CustomerBookingsQuery query,
            CancellationToken cancellationToken = default
        )
        {
            var customerProfile = await GetCurrentCustomerProfileAsync(cancellationToken);

            if (customerProfile == null)
            {
                return OperationResult<PagedResponse<BookingDetailDto>>.Failure(
                    "Customer profile not found"
                );
            }

            var (items, totalCount) = await _bookingRepository.GetCustomerBookingsAsync(
                customerProfile.Id,
                query,
                cancellationToken
            );
            var dtos = _mapper.Map<List<BookingDetailDto>>(items);

            var response = new PagedResponse<BookingDetailDto>
            {
                Items = dtos,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
            };

            return OperationResult<PagedResponse<BookingDetailDto>>.Success(
                response,
                "Customer bookings retrieved successfully"
            );
        }

        public async Task<OperationResult<List<BookingMatchingQueueDto>>> GetMatchingQueueAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default
        )
        {
            var matchingQueue = await _matchingQueueRepository.GetQueueForBookingAsync(
                bookingId,
                cancellationToken
            );

            var dtos = matchingQueue
                .Select(q => new BookingMatchingQueueDto
                {
                    WorkerId = q.WorkerId,
                    FullName = q.Worker?.User?.FullName ?? string.Empty,
                    Phone = q.Worker?.User?.Phone ?? string.Empty,
                    AvatarUrl = q.Worker?.User?.AvatarUrl,
                    RatingAvg = q.Worker?.RatingAvg ?? 0,
                    DistanceKm = q.DistanceKm,
                    Score = q.Score,
                    Status = q.Status.ToString(),
                    OfferedAt = q.OfferedAt,
                    ExpiresAt = q.ExpiresAt,
                    RejectReason = q.RejectReason,
                })
                .ToList();

            return OperationResult<List<BookingMatchingQueueDto>>.Success(
                dtos,
                "Matching queue retrieved successfully"
            );
        }

        /// <summary>
        /// Attempts to offer the booking to the next candidate worker in the matching queue.
        /// If no candidates remain, the booking status is set back to Matching.
        /// </summary>
        private async Task OfferToNextWorkerAsync(
            BookingEntity booking,
            CancellationToken cancellationToken
        )
        {
            var nextCandidate = await _matchingQueueRepository.GetNextCandidateAsync(
                booking.Id,
                cancellationToken
            );

            if (nextCandidate != null)
            {
                nextCandidate.Status = MatchingStatus.Offered;
                nextCandidate.OfferedAt = DateTime.UtcNow;
                nextCandidate.ExpiresAt = DateTime.UtcNow.AddMinutes(15); // Default timeout, will be read from PlatformConfig in the future
                _matchingQueueRepository.Update(nextCandidate);

                _logger.LogInformation(
                    "Offered booking {BookingId} to next worker {WorkerId}",
                    booking.Id,
                    nextCandidate.WorkerId
                );
            }
            else
            {
                // No more candidates — set booking back to Matching so the system can re-search
                booking.Status = BookingStatus.Matching;
                booking.UpdatedDate = DateTime.UtcNow;
                _bookingRepository.Update(booking);

                _logger.LogWarning(
                    "No more candidates for booking {BookingId}. Status set to Matching.",
                    booking.Id
                );
            }
        }

        // =========================================================
        // TRANSITION
        // =========================================================

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
                return OperationResult.Failure("Booking not found");
            }

            if (booking.Status != expectedCurrentStatus)
            {
                return OperationResult.Failure(
                    $"Invalid status transition. Current status: {booking.Status}"
                );
            }

            if (
                !AllowedTransitions.TryGetValue(expectedCurrentStatus, out var allowedNext)
                || allowedNext != newStatus
            )
            {
                return OperationResult.Failure("Transition is not allowed");
            }

            booking.Status = newStatus;
            booking.UpdatedDate = DateTime.UtcNow;

            if (onTransition != null)
            {
                await onTransition(booking);
            }

            _bookingRepository.Update(booking);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _bookingHubService.SendStatusUpdateAsync(
                booking.Id,
                new BookingStatusUpdateDto
                {
                    BookingId = booking.Id,
                    Status = booking.Status.ToString(),
                    UpdatedAt = DateTime.UtcNow,
                    Message = message,
                },
                cancellationToken
            );

            return OperationResult.Success(message);
        }
    }
}
