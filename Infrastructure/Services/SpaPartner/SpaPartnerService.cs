using Application.Common;
using Application.DTOs.SpaPartner;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Media;
using Application.Interfaces.Services.SpaPartner;
using AutoMapper;
using Domain.Entity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.SpaPartner
{
    public class SpaPartnerService : ISpaPartnerService
    {
        private readonly ISpaPartnerRepository _spaPartnerRepository;
        private readonly IRepository<SpaPartnerReview> _reviewRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBlobService _blobService;
        private readonly ILogger<SpaPartnerService> _logger;

        public SpaPartnerService(
            ISpaPartnerRepository spaPartnerRepository,
            IRepository<SpaPartnerReview> reviewRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IBlobService blobService,
            ILogger<SpaPartnerService> logger
        )
        {
            _spaPartnerRepository = spaPartnerRepository ?? throw new ArgumentNullException(nameof(spaPartnerRepository));
            _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OperationResult<PagedResponse<SpaPartnerDto>>> SearchAsync(
            SearchSpaPartnerQuery query,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var (items, distances, totalCount) = await _spaPartnerRepository.SearchAsync(query, cancellationToken);
                var dtos = _mapper.Map<List<SpaPartnerDto>>(items);

                // Populate distance & filter matched services if category selected
                foreach (var dto in dtos)
                {
                    if (distances.TryGetValue(dto.Id, out var dist))
                    {
                        dto.DistanceKm = dist;
                    }

                    if (query.SpaServiceCategoryId.HasValue)
                    {
                        dto.MatchedServices = dto.MatchedServices
                            .Where(s => s.SpaServiceCategoryId == query.SpaServiceCategoryId.Value)
                            .ToList();
                    }
                }

                var response = new PagedResponse<SpaPartnerDto>
                {
                    Items = dtos,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalCount = totalCount
                };

                return OperationResult<PagedResponse<SpaPartnerDto>>.Success(response, "Spa partners retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching spa partners");
                return OperationResult<PagedResponse<SpaPartnerDto>>.Failure("Failed to search spa partners");
            }
        }

        public async Task<OperationResult<SpaPartnerDetailDto>> GetDetailAsync(
            Guid id,
            double? customerLat,
            double? customerLng,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var spa = await _spaPartnerRepository.GetDetailByIdAsync(id, cancellationToken);
                if (spa == null)
                {
                    return OperationResult<SpaPartnerDetailDto>.Failure("Spa partner not found");
                }

                var detailDto = _mapper.Map<SpaPartnerDetailDto>(spa);

                if (customerLat.HasValue && customerLng.HasValue && spa.Lat.HasValue && spa.Lng.HasValue)
                {
                    var dist = CalculateHaversineDistance(customerLat.Value, customerLng.Value, spa.Lat.Value, spa.Lng.Value);
                    detailDto.DistanceKm = Math.Round(dist, 1);
                }

                return OperationResult<SpaPartnerDetailDto>.Success(detailDto, "Spa partner detail retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detail for spa partner {Id}", id);
                return OperationResult<SpaPartnerDetailDto>.Failure("Failed to retrieve spa partner detail");
            }
        }

        public async Task<OperationResult<List<SpaPartnerDto>>> GetNearbyAsync(
            double lat,
            double lng,
            double radiusKm,
            int limit,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var (items, distances) = await _spaPartnerRepository.GetNearbyAsync(lat, lng, radiusKm, limit, cancellationToken);
                var dtos = _mapper.Map<List<SpaPartnerDto>>(items);

                foreach (var dto in dtos)
                {
                    if (distances.TryGetValue(dto.Id, out var dist))
                    {
                        dto.DistanceKm = dist;
                    }
                }

                return OperationResult<List<SpaPartnerDto>>.Success(dtos, "Nearby spa partners retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting nearby spa partners");
                return OperationResult<List<SpaPartnerDto>>.Failure("Failed to retrieve nearby spa partners");
            }
        }

        public async Task<OperationResult<SpaPartnerReviewDto>> CreateReviewAsync(
            Guid spaId,
            Guid customerProfileId,
            CreateSpaPartnerReviewDto dto,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var spa = await _spaPartnerRepository.GetByIdAsync(spaId, cancellationToken);
                if (spa == null)
                {
                    return OperationResult<SpaPartnerReviewDto>.Failure("Spa partner not found");
                }

                var review = new SpaPartnerReview
                {
                    SpaPartnerId = spaId,
                    CustomerProfileId = customerProfileId,
                    Rating = Math.Clamp(dto.Rating, 1, 5),
                    Comment = dto.Comment?.Trim(),
                    IsVisible = true
                };

                await _reviewRepository.AddAsync(review, cancellationToken);

                // Update rating avg and review count on SpaPartner
                var currentTotal = spa.TotalReviews;
                var currentRatingTotal = spa.RatingAvg * currentTotal;
                spa.TotalReviews = currentTotal + 1;
                spa.RatingAvg = Math.Round((currentRatingTotal + review.Rating) / spa.TotalReviews, 1);

                _spaPartnerRepository.Update(spa);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var reviewDto = _mapper.Map<SpaPartnerReviewDto>(review);
                return OperationResult<SpaPartnerReviewDto>.Success(reviewDto, "Review submitted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for spa partner {SpaId}", spaId);
                return OperationResult<SpaPartnerReviewDto>.Failure("Failed to submit review");
            }
        }

        public async Task<OperationResult<PagedResponse<SpaPartnerReviewDto>>> GetReviewsAsync(
            Guid spaId,
            PagedQuery query,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var reviews = await _reviewRepository.FindAsync(r => r.SpaPartnerId == spaId && r.IsVisible, cancellationToken);
                var totalCount = reviews.Count;

                var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
                var pageSize = query.PageSize < 1 ? 10 : query.PageSize;

                var pagedReviews = reviews
                    .OrderByDescending(r => r.CreatedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var dtos = _mapper.Map<List<SpaPartnerReviewDto>>(pagedReviews);

                var response = new PagedResponse<SpaPartnerReviewDto>
                {
                    Items = dtos,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };

                return OperationResult<PagedResponse<SpaPartnerReviewDto>>.Success(response, "Reviews retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for spa partner {SpaId}", spaId);
                return OperationResult<PagedResponse<SpaPartnerReviewDto>>.Failure("Failed to retrieve reviews");
            }
        }

        public async Task<OperationResult<SpaPartnerDetailDto>> CreateAsync(
            CreateSpaPartnerDto dto,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var spa = _mapper.Map<Domain.Entity.SpaPartner>(dto);

                if (dto.LogoFile != null)
                {
                    spa.LogoUrl = await _blobService.UploadImageAsync(dto.LogoFile);
                }

                if (dto.CoverImageFile != null)
                {
                    spa.CoverImageUrl = await _blobService.UploadImageAsync(dto.CoverImageFile);
                }

                await _spaPartnerRepository.AddAsync(spa, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var detailDto = _mapper.Map<SpaPartnerDetailDto>(spa);
                return OperationResult<SpaPartnerDetailDto>.Success(detailDto, "Spa partner created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating spa partner");
                return OperationResult<SpaPartnerDetailDto>.Failure("Failed to create spa partner");
            }
        }

        public async Task<OperationResult<SpaPartnerDetailDto>> UpdateAsync(
            Guid id,
            UpdateSpaPartnerDto dto,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var spa = await _spaPartnerRepository.GetByIdAsync(id, cancellationToken);
                if (spa == null)
                {
                    return OperationResult<SpaPartnerDetailDto>.Failure("Spa partner not found");
                }

                if (!string.IsNullOrWhiteSpace(dto.Name)) spa.Name = dto.Name.Trim();
                if (dto.Description != null) spa.Description = dto.Description;
                if (!string.IsNullOrWhiteSpace(dto.Address)) spa.Address = dto.Address.Trim();
                if (!string.IsNullOrWhiteSpace(dto.City)) spa.City = dto.City.Trim();
                if (dto.Lat.HasValue) spa.Lat = dto.Lat.Value;
                if (dto.Lng.HasValue) spa.Lng = dto.Lng.Value;
                if (dto.Phone != null) spa.Phone = dto.Phone;
                if (dto.Email != null) spa.Email = dto.Email;
                if (dto.OpeningHours != null) spa.OpeningHours = dto.OpeningHours;
                if (dto.IsActive.HasValue) spa.IsActive = dto.IsActive.Value;
                if (dto.SortOrder.HasValue) spa.SortOrder = dto.SortOrder.Value;

                if (dto.LogoFile != null)
                {
                    spa.LogoUrl = await _blobService.UploadImageAsync(dto.LogoFile, spa.LogoUrl);
                }

                if (dto.CoverImageFile != null)
                {
                    spa.CoverImageUrl = await _blobService.UploadImageAsync(dto.CoverImageFile, spa.CoverImageUrl);
                }

                _spaPartnerRepository.Update(spa);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var detailDto = _mapper.Map<SpaPartnerDetailDto>(spa);
                return OperationResult<SpaPartnerDetailDto>.Success(detailDto, "Spa partner updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating spa partner {Id}", id);
                return OperationResult<SpaPartnerDetailDto>.Failure("Failed to update spa partner");
            }
        }

        public async Task<OperationResult> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var spa = await _spaPartnerRepository.GetByIdAsync(id, cancellationToken);
                if (spa == null)
                {
                    return OperationResult.Failure("Spa partner not found");
                }

                _spaPartnerRepository.Remove(spa);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return OperationResult.Success("Spa partner deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting spa partner {Id}", id);
                return OperationResult.Failure("Failed to delete spa partner");
            }
        }

        private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            var dLat = (Math.PI / 180) * (lat2 - lat1);
            var dLon = (Math.PI / 180) * (lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos((Math.PI / 180) * lat1) * Math.Cos((Math.PI / 180) * lat2) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}
