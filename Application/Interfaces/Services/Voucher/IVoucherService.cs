using Application.Common;
using Application.DTOs.Voucher;

namespace Application.Interfaces.Services.Voucher
{
    public interface IVoucherService
    {
        // Admin CRUD
        Task<OperationResult<VoucherDto>> CreateAsync(CreateVoucherDto dto, CancellationToken cancellationToken = default);
        Task<OperationResult<VoucherDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<OperationResult<PagedResponse<VoucherDto>>> GetPagedAsync(VoucherQuery query, CancellationToken cancellationToken = default);
        Task<OperationResult<VoucherDto>> UpdateAsync(Guid id, UpdateVoucherDto dto, CancellationToken cancellationToken = default);
        Task<OperationResult<VoucherDto>> UpdateStatusAsync(Guid id, UpdateVoucherStatusDto dto, CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        // Customer
        Task<OperationResult<ApplyVoucherResponse>> ApplyVoucherAsync(ApplyVoucherRequest request, Guid userId, CancellationToken cancellationToken = default);
        Task<OperationResult> ReleaseVoucherAsync(Guid bookingId, CancellationToken cancellationToken = default);
    }
}
