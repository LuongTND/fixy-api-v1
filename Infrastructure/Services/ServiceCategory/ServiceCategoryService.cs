using Application.Common;
using Application.DTOs.ServiceCategory;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Media;
using Application.Interfaces.Services.ServiceCategory;
using AutoMapper;
using Domain.Entity;
using Microsoft.Extensions.Logging;

namespace Application.Service
{
    public class ServiceCategoryService : IServiceCategoryService
    {
        private readonly IServiceCategoryRepository _serviceCategoryRepository;
        private readonly IWorkerServiceRepository _workerServiceRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBlobService _blobService;
        private readonly ILogger<ServiceCategoryService> _logger;

        public ServiceCategoryService(
            IServiceCategoryRepository serviceCategoryRepository,
            IWorkerServiceRepository workerServiceRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IBlobService blobService,
            ILogger<ServiceCategoryService> logger
        )
        {
            _serviceCategoryRepository = serviceCategoryRepository ?? throw new ArgumentNullException(nameof(serviceCategoryRepository));
            _workerServiceRepository = workerServiceRepository ?? throw new ArgumentNullException(nameof(workerServiceRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OperationResult<List<ServiceCategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var categories = await _serviceCategoryRepository.GetAllAsync(cancellationToken);

            var data = _mapper.Map<List<ServiceCategoryDto>>(categories);

            return OperationResult<List<ServiceCategoryDto>>.Success(data, "Service categories retrieved successfully");
        }

        public async Task<OperationResult<ServiceCategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var category = await _serviceCategoryRepository.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (category == null)
            {
                _logger.LogWarning("Service category not found. Id: {ServiceCategoryId}", id);
                return OperationResult<ServiceCategoryDto>.Failure("Service category not found");
            }

            return OperationResult<ServiceCategoryDto>.Success(_mapper.Map<ServiceCategoryDto>(category), "Service category retrieved successfully");
        }

        public async Task<OperationResult<ServiceCategoryPriceDto>> GetPriceAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var categoryExists = await _serviceCategoryRepository.ExistsByIdAsync(id, cancellationToken);

            if (!categoryExists)
            {
                _logger.LogWarning("Service category not found. Id: {ServiceCategoryId}", id);
                return OperationResult<ServiceCategoryPriceDto>.Failure("Service category not found");
            }

            var (minPrice, maxPrice) = await _workerServiceRepository.GetPriceRangeAsync(id, cancellationToken);

            if (!minPrice.HasValue || !maxPrice.HasValue)
            {
                return OperationResult<ServiceCategoryPriceDto>.Failure("Service category price not found");
            }

            return OperationResult<ServiceCategoryPriceDto>.Success(
                new ServiceCategoryPriceDto
                {
                    CategoryId = id,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                },
                "Service category price retrieved successfully"
            );
        }

        public async Task<OperationResult<ServiceCategoryDto>> CreateAsync(CreateServiceCategoryDto dto, CancellationToken cancellationToken = default)
        {
            var parentCheck = await ValidateParentAsync(dto.ParentId, null, cancellationToken);

            if (!parentCheck.IsSuccess)
            {
                return OperationResult<ServiceCategoryDto>.Failure(parentCheck.Message ?? string.Empty);
            }

            var category = _mapper.Map<ServiceCategory>(dto);

            if (dto.ImageFile != null)
            {
                category.ImageUrl = await _blobService.UploadImageAsync(dto.ImageFile);
            }

            await _serviceCategoryRepository.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Service category created. Id: {ServiceCategoryId}", category.Id);
            return OperationResult<ServiceCategoryDto>.Success(_mapper.Map<ServiceCategoryDto>(category), "Service category created successfully");
        }

        public async Task<OperationResult<ServiceCategoryDto>> UpdateAsync(Guid id, UpdateServiceCategoryDto dto, CancellationToken cancellationToken = default)
        {
            var category = await _serviceCategoryRepository.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (category == null)
            {
                _logger.LogWarning("Service category not found. Id: {ServiceCategoryId}", id);
                return OperationResult<ServiceCategoryDto>.Failure("Service category not found");
            }

            var parentCheck = await ValidateParentAsync(dto.ParentId, id, cancellationToken);

            if (!parentCheck.IsSuccess)
            {
                return OperationResult<ServiceCategoryDto>.Failure(parentCheck.Message ?? string.Empty);
            }

            _mapper.Map(dto, category);

            if (dto.ImageFile != null)
            {
                category.ImageUrl = await _blobService.UploadImageAsync(dto.ImageFile, category.ImageUrl);
            }

            category.UpdatedDate = DateTime.UtcNow;
            _serviceCategoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Service category updated. Id: {ServiceCategoryId}", category.Id);
            return OperationResult<ServiceCategoryDto>.Success(_mapper.Map<ServiceCategoryDto>(category), "Service category updated successfully");
        }

        public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var category = await _serviceCategoryRepository.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (category == null)
            {
                _logger.LogWarning("Service category not found. Id: {ServiceCategoryId}", id);
                return OperationResult.Failure("Service category not found");
            }

            var hasChildren = await _serviceCategoryRepository.HasChildrenAsync(id, cancellationToken);

            if (hasChildren)
            {
                _logger.LogWarning("Cannot delete service category with children. Id: {ServiceCategoryId}", id);
                return OperationResult.Failure("Cannot delete a category that has children");
            }

            _serviceCategoryRepository.Remove(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Service category deleted. Id: {ServiceCategoryId}", id);
            return OperationResult.Success("Service category deleted successfully");
        }

        private async Task<OperationResult> ValidateParentAsync(Guid? parentId, Guid? currentId, CancellationToken cancellationToken)
        {
            if (!parentId.HasValue)
            {
                return OperationResult.Success();
            }

            if (currentId.HasValue && parentId.Value == currentId.Value)
            {
                return OperationResult.Failure("Parent category is invalid");
            }

            var parentExists = await _serviceCategoryRepository.ExistsByIdAsync(parentId.Value, cancellationToken);

            if (!parentExists)
            {
                return OperationResult.Failure("Parent category not found");
            }

            return OperationResult.Success();
        }

    }
}
