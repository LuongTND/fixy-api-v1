using Application.Common;
using Application.DTOs.Media;
using Application.DTOs.Review;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Media;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Repositories;

namespace Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IWorkerProfileRepository _workerRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IBlobService _blobService;
        private readonly IUnitOfWork _unitOfWork;

        public ReviewService(
            IReviewRepository reviewRepository,
            IBookingRepository bookingRepository,
            IWorkerProfileRepository workerRepository,
            IMediaRepository mediaRepository,
            IBlobService blobService,
            IUnitOfWork unitOfWork
        )
        {
            _reviewRepository = reviewRepository;
            _bookingRepository = bookingRepository;
            _workerRepository = workerRepository;
            _mediaRepository = mediaRepository;
            _blobService = blobService;
            _unitOfWork = unitOfWork;
        }

        public async Task<OperationResult> CreateReviewAsync(
            Guid customerId,
            Guid bookingId,
            CreateReviewRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            if (dto.Rating is < 1 or > 5)
            {
                return OperationResult.Failure("Rating must be between 1 and 5.");
            }
            if (dto.Images.Count > 5)
            {
                return OperationResult.Failure("You only can upload 5 images.");
            }
            var booking = await _bookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
            {
                return OperationResult.Failure("Booking not found.");
            }

            if (booking.CustomerId != customerId)
            {
                return OperationResult.Failure("Forbidden.");
            }

            if (booking.Status != BookingStatus.Completed)
            {
                return OperationResult.Failure("Only completed bookings can be reviewed.");
            }

            if (booking.WorkerId == null)
            {
                return OperationResult.Failure("This booking does not have an assigned worker.");
            }

            var alreadyReviewed = await _reviewRepository.ExistsByBookingIdAsync(
                bookingId,
                cancellationToken
            );

            if (alreadyReviewed)
            {
                return OperationResult.Failure("This booking has already been reviewed.");
            }

            var uploadedUrls = new List<string>();

            try
            {
                var review = new Review
                {
                    BookingId = booking.Id,
                    CustomerId = booking.CustomerId,
                    WorkerId = booking.WorkerId.Value,
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                };

                await _reviewRepository.AddAsync(review);

                // upload images
                foreach (var image in dto.Images)
                {
                    var imageUrl = await _blobService.UploadImageAsync(image);

                    uploadedUrls.Add(imageUrl);

                    await _mediaRepository.AddAsync(
                        new Media
                        {
                            OwnerId = review.Id,
                            UploadedById = customerId,
                            OwnerType = MediaOwnerType.Review,
                            Category = MediaCategory.Review,
                            FileUrl = imageUrl,
                        },
                        cancellationToken
                    );
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var worker = await _workerRepository.GetByIdAsync(booking.WorkerId.Value);

                if (worker != null)
                {
                    worker.RatingAvg = await _reviewRepository.RecalculateAverageRatingAsync(
                        worker.Id,
                        cancellationToken
                    );

                    worker.TotalReviews = await _reviewRepository.RecalculateTotalReviewsAsync(
                        worker.Id,
                        cancellationToken
                    );

                    _workerRepository.Update(worker);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

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
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
            {
                return OperationResult.Failure("Review not found.");
            }
            if (review.WorkerId != workerId)
            {
                return OperationResult.Failure("Forbidden.");
            }
            review.WorkerReply = dto.Reply;
            review.RepliedAt = DateTime.UtcNow;
            _reviewRepository.Update(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Reply added successfully.");
        }

        public async Task<OperationResult<PagedResponse<ReviewDto>>> GetWorkerReviewsPagedAsync(
            Guid workerId,
            PagedQuery query,
            CancellationToken cancellationToken
        )
        {
            var worker = await _workerRepository.GetByIdAsync(workerId);

            if (worker == null)
            {
                return OperationResult<PagedResponse<ReviewDto>>.Failure("Worker not found.");
            }

            var (reviews, totalCount) = await _reviewRepository.GetWorkerReviewsPagedAsync(
                workerId,
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
                        Id = x.CustomerId,

                        FullName = x.Customer!.User!.FullName,

                        AvatarUrl = x.Customer.User.AvatarUrl,
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
