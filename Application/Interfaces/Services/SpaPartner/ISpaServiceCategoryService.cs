using Application.Common;
using Application.DTOs.SpaPartner;

namespace Application.Interfaces.Services.SpaPartner
{
    public interface ISpaServiceCategoryService
    {
        Task<OperationResult<List<SpaServiceCategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<OperationResult<SpaServiceCategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<OperationResult<SpaServiceCategoryDto>> CreateAsync(CreateSpaServiceCategoryDto dto, CancellationToken cancellationToken = default);
        Task<OperationResult<SpaServiceCategoryDto>> UpdateAsync(Guid id, CreateSpaServiceCategoryDto dto, CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
