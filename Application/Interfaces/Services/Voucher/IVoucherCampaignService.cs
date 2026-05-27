using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.VoucherCampaign;

namespace Application.Interfaces.Services.Voucher
{
    public interface IVoucherCampaignService
    {
        Task<OperationResult<CampaignDto>> CreateAsync(CreateCampaignDto dto, CancellationToken cancellationToken = default);
        Task<OperationResult<CampaignDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<OperationResult<PagedResponse<CampaignDto>>> GetPagedAsync(CampaignQuery query, CancellationToken cancellationToken = default);
        Task<OperationResult<CampaignDto>> UpdateAsync(Guid id, UpdateCampaignDto dto, CancellationToken cancellationToken = default);
        Task<OperationResult<CampaignDto>> UpdateStatusAsync(Guid id, UpdateCampaignStatusDto dto, CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
