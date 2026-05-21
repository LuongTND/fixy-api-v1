using Application.Common;
using Application.DTOs.Media;
using Application.DTOs.Review;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Media;
using Domain.Entity;
using Domain.Enum;

namespace Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IWorkerProfileRepository _workerProfileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IWorkerProfileRepository _workerRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IBlobService _blobService;
        private readonly IUnitOfWork _unitOfWork;

        public ReviewService(
            IReviewRepository reviewRepository,
            IWorkerProfileRepository workerProfileRepository,
            IUserRepository userRepository,
            IBookingRepository bookingRepository,
            IWorkerProfileRepository workerRepository,
            IMediaRepository mediaRepository,
            IBlobService blobService,
            IUnitOfWork unitOfWork
        )
        {
            _reviewRepository = reviewRepository;
            _bookingRepository = bookingRepository;
            _workerProfileRepository = workerProfileRepository;
            _userRepository = userRepository;
            _workerRepository = workerRepository;
            _mediaRepository = mediaRepository;
            _blobService = blobService;
            _unitOfWork = unitOfWork;
        }

        public async Task<OperationResult> CreateReviewAsync(
            Guid customerUserId,
            Guid bookingId,
            CreateReviewRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            // =========================
            // Validation
            // =========================

            if (dto.Rating is < 1 or > 5)
            {
                return OperationResult.Failure("Rating must be between 1 and 5.");
            }

            if (dto.Images.Count > 5)
            {
                return OperationResult.Failure("You only can upload 5 images.");
            }

            // =========================
            // Booking
            // =========================

            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                return OperationResult.Failure("Booking not found.");
            }

            if (booking.Status != BookingStatus.Completed)
            {
                return OperationResult.Failure("Only completed bookings can be reviewed.");
            }

            if (booking.WorkerProfileId == null)
            {
                return OperationResult.Failure("This booking does not have an assigned worker.");
            }

            // =========================
            // Customer
            // =========================

            var customerUser = await _userRepository.GetWithCustomerProfileByIdAsync(
                customerUserId,
                cancellationToken
            );

            if (customerUser == null || customerUser.CustomerProfile == null)
            {
                return OperationResult.Failure("Customer not found.");
            }

            var customerProfileId = customerUser.CustomerProfile.Id;

            if (booking.CustomerProfileId != customerProfileId)
            {
                return OperationResult.Failure("Forbidden.");
            }

            // =========================
            // Worker
            // =========================

            var workerProfile = await _workerRepository.GetByIdAsync(
                booking.WorkerProfileId.Value,
                cancellationToken
            );

            if (workerProfile == null)
            {
                return OperationResult.Failure("Worker not found.");
            }

            // =========================
            // Duplicate Review
            // =========================

            var alreadyReviewed = await _reviewRepository.ExistsByBookingIdAsync(
                bookingId,
                cancellationToken
            );

            if (alreadyReviewed)
            {
                return OperationResult.Failure("This booking has already been reviewed.");
            }

            // =========================
            // Create Review
            // =========================

            var uploadedUrls = new List<string>();

            try
            {
                var review = new Review
                {
                    BookingId = booking.Id,

                    CustomerProfileId = customerProfileId,

                    WorkerProfileId = workerProfile.Id,

                    Rating = dto.Rating,

                    Comment = dto.Comment,
                };

                await _reviewRepository.AddAsync(review, cancellationToken);

                // =========================
                // Upload Images
                // =========================

                foreach (var image in dto.Images)
                {
                    var imageUrl = await _blobService.UploadImageAsync(image);

                    uploadedUrls.Add(imageUrl);

                    await _mediaRepository.AddAsync(
                        new Media
                        {
                            OwnerId = review.Id,

                            UploadedById = customerUserId,

                            OwnerType = MediaOwnerType.Review,

                            Category = MediaCategory.Review,

                            FileUrl = imageUrl,
                        },
                        cancellationToken
                    );
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // =========================
                // Recalculate Worker Rating
                // =========================

                workerProfile.RatingAvg = await _reviewRepository.RecalculateAverageRatingAsync(
                    workerProfile.Id,
                    cancellationToken
                );

                workerProfile.TotalReviews = await _reviewRepository.RecalculateTotalReviewsAsync(
                    workerProfile.Id,
                    cancellationToken
                );

                _workerRepository.Update(workerProfile);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return OperationResult.Success("Review created successfully.");
            }
            catch
            {
                foreach (var url in uploadedUrls)
                {
                    await _blobService.DeleteImageAsync(url);
                }

                throw;
            }
        }

        public async Task<OperationResult> ReplyReviewAsync(
            Guid workerId,
            Guid reviewId,
            ReplyReviewRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);

            if (review == null)
            {
                return OperationResult.Failure("Review not found.");
            }
            var workerProfile = await _workerProfileRepository.GetWorkerProfileByUserIdAsync(
                workerId
            );
            if (workerProfile == null)
            {
                return OperationResult.Failure("Worker profile not found.");
            }
            if (review.WorkerProfileId != workerProfile.Id)
            {
                return OperationResult.Failure("Forbidden.");
            }

            review.WorkerReply = dto.Reply;

            review.RepliedAt = DateTime.UtcNow;

            _reviewRepository.Update(review);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult.Success("Reply added successfully.");
        }

        public async Task<OperationResult<ReviewDto>> GetByBookingIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken
        )
        {
            var review = await _reviewRepository.GetByBookingIdAsync(bookingId, cancellationToken);

            if (review == null)
            {
                return OperationResult<ReviewDto>.Failure("Review not found.");
            }

            var reviewImages = await _mediaRepository.GetReviewImagesByReviewIdsAsync(
                new List<Guid> { review.Id },
                cancellationToken
            );

            var dto = new ReviewDto
            {
                Id = review.Id,

                BookingId = review.BookingId,

                Rating = review.Rating,

                Comment = review.Comment,

                WorkerReply = review.WorkerReply,

                CreatedAt = review.CreatedDate,

                RepliedAt = review.RepliedAt,

                Customer = new CustomerReviewInfoDto
                {
                    Id = review.CustomerProfileId,

                    FullName = review.CustomerProfile?.User?.FullName ?? string.Empty,

                    AvatarUrl = review.CustomerProfile?.User?.AvatarUrl,
                },

                Images = reviewImages
                    .Select(i => new MediaDto
                    {
                        Id = i.Id,

                        OwnerId = i.OwnerId,

                        FileUrl = i.FileUrl,
                    })
                    .ToList(),
            };

            return OperationResult<ReviewDto>.Success(dto);
        }

        public async Task<OperationResult<PagedResponse<ReviewDto>>> GetWorkerReviewsPagedAsync(
            Guid workerProfileId,
            PagedQuery query,
            CancellationToken cancellationToken
        )
        {
            var worker = await _workerRepository.GetByIdAsync(workerProfileId, cancellationToken);

            if (worker == null)
            {
                return OperationResult<PagedResponse<ReviewDto>>.Failure("Worker not found.");
            }

            var (reviews, totalCount) = await _reviewRepository.GetWorkerReviewsPagedAsync(
                workerProfileId,
                query,
                cancellationToken
            );

            var reviewIds = reviews.Select(x => x.Id).ToList();

            var reviewImages = await _mediaRepository.GetReviewImagesByReviewIdsAsync(
                reviewIds,
                cancellationToken
            );

            var reviewImageLookup = reviewImages.ToLookup(x => x.OwnerId);

            var items = reviews
                .Select(x => new ReviewDto
                {
                    Id = x.Id,

                    BookingId = x.BookingId,

                    Rating = x.Rating,

                    Comment = x.Comment,

                    WorkerReply = x.WorkerReply,

                    CreatedAt = x.CreatedDate,

                    RepliedAt = x.RepliedAt,

                    Customer = new CustomerReviewInfoDto
                    {
                        Id = x.CustomerProfileId,

                        FullName = x.CustomerProfile!.User!.FullName,

                        AvatarUrl = x.CustomerProfile.User!.AvatarUrl,
                    },

                    Images = reviewImageLookup[x.Id]
                        .Select(i => new MediaDto
                        {
                            Id = i.Id,

                            OwnerId = i.OwnerId,

                            FileUrl = i.FileUrl,
                        })
                        .ToList(),
                })
                .ToList();

            var result = new PagedResponse<ReviewDto>
            {
                Items = items,

                PageNumber = query.PageNumber,

                PageSize = query.PageSize,

                TotalCount = totalCount,
            };

            return OperationResult<PagedResponse<ReviewDto>>.Success(result);
        }
    }
}
