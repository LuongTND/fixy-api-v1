using Application.Common;
using Application.DTOs.ServiceCategory;

namespace Application.Interfaces.Services.ServiceCategory
{
    public interface IServiceCategoryService
    {
        Task<OperationResult<List<ServiceCategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<OperationResult<ServiceCategoryDto>> GetByIdAsync(Guid id,CancellationToken cancellationToken = default);
        Task<OperationResult<ServiceCategoryDto>> CreateAsync(CreateServiceCategoryDto dto,CancellationToken cancellationToken = default);
        Task<OperationResult<ServiceCategoryDto>> UpdateAsync(Guid id,UpdateServiceCategoryDto dto,CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
