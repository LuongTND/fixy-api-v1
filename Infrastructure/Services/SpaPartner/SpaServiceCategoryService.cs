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
    public class SpaServiceCategoryService : ISpaServiceCategoryService
    {
        private readonly ISpaServiceCategoryRepository _spaServiceCategoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBlobService _blobService;
        private readonly ILogger<SpaServiceCategoryService> _logger;

        public SpaServiceCategoryService(
            ISpaServiceCategoryRepository spaServiceCategoryRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IBlobService blobService,
            ILogger<SpaServiceCategoryService> logger
        )
        {
            _spaServiceCategoryRepository = spaServiceCategoryRepository ?? throw new ArgumentNullException(nameof(spaServiceCategoryRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OperationResult<List<SpaServiceCategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var categories = await _spaServiceCategoryRepository.GetActiveCategoriesWithSpaCountAsync(cancellationToken);
                var dtos = _mapper.Map<List<SpaServiceCategoryDto>>(categories);

                return OperationResult<List<SpaServiceCategoryDto>>.Success(dtos, "Spa service categories retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting spa service categories");
                return OperationResult<List<SpaServiceCategoryDto>>.Failure("Failed to retrieve spa service categories");
            }
        }

        public async Task<OperationResult<SpaServiceCategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var category = await _spaServiceCategoryRepository.GetByIdAsync(id, cancellationToken);
                if (category == null)
                {
                    return OperationResult<SpaServiceCategoryDto>.Failure("Spa service category not found");
                }

                var dto = _mapper.Map<SpaServiceCategoryDto>(category);
                return OperationResult<SpaServiceCategoryDto>.Success(dto, "Spa service category retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting spa service category {Id}", id);
                return OperationResult<SpaServiceCategoryDto>.Failure("Failed to retrieve spa service category");
            }
        }

        public async Task<OperationResult<SpaServiceCategoryDto>> CreateAsync(CreateSpaServiceCategoryDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var category = _mapper.Map<SpaServiceCategory>(dto);

                if (dto.ImageFile != null)
                {
                    category.ImageUrl = await _blobService.UploadImageAsync(dto.ImageFile);
                }

                await _spaServiceCategoryRepository.AddAsync(category, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var resultDto = _mapper.Map<SpaServiceCategoryDto>(category);
                return OperationResult<SpaServiceCategoryDto>.Success(resultDto, "Spa service category created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating spa service category");
                return OperationResult<SpaServiceCategoryDto>.Failure("Failed to create spa service category");
            }
        }

        public async Task<OperationResult<SpaServiceCategoryDto>> UpdateAsync(Guid id, CreateSpaServiceCategoryDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var category = await _spaServiceCategoryRepository.GetByIdAsync(id, cancellationToken);
                if (category == null)
                {
                    return OperationResult<SpaServiceCategoryDto>.Failure("Spa service category not found");
                }

                if (!string.IsNullOrWhiteSpace(dto.Name)) category.Name = dto.Name.Trim();
                if (dto.Description != null) category.Description = dto.Description;
                if (!string.IsNullOrWhiteSpace(dto.Code)) category.Code = dto.Code.Trim().ToLower();
                if (dto.SortOrder.HasValue) category.SortOrder = dto.SortOrder.Value;
                if (dto.IsActive.HasValue) category.IsActive = dto.IsActive.Value;

                if (dto.ImageFile != null)
                {
                    category.ImageUrl = await _blobService.UploadImageAsync(dto.ImageFile, category.ImageUrl);
                }

                _spaServiceCategoryRepository.Update(category);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var resultDto = _mapper.Map<SpaServiceCategoryDto>(category);
                return OperationResult<SpaServiceCategoryDto>.Success(resultDto, "Spa service category updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating spa service category {Id}", id);
                return OperationResult<SpaServiceCategoryDto>.Failure("Failed to update spa service category");
            }
        }

        public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var category = await _spaServiceCategoryRepository.GetByIdAsync(id, cancellationToken);
                if (category == null)
                {
                    return OperationResult.Failure("Spa service category not found");
                }

                _spaServiceCategoryRepository.Remove(category);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return OperationResult.Success("Spa service category deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting spa service category {Id}", id);
                return OperationResult.Failure("Failed to delete spa service category");
            }
        }
    }
}
