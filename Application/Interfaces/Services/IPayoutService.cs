using Application.Common;
using Application.DTOs.Payout;

namespace Application.Interfaces.Services
{
    public interface IPayoutService
    {
        Task<OperationResult<PayoutRequestDto>> CreateRequestAsync(
            Guid workerId,
            Guid payoutAccountId,
            long amount,
            CancellationToken cancellationToken
        );

        Task<OperationResult> ApproveAsync(
            Guid payoutRequestId,
            Guid reviewerId,
            CancellationToken cancellationToken
        );

        Task<OperationResult> RejectAsync(
            Guid payoutRequestId,
            Guid reviewerId,
            string reason,
            CancellationToken cancellationToken
        );
        Task<OperationResult<PagedResponse<PayoutRequestDto>>> GetAllAsync(
            PagedQuery query,
            CancellationToken cancellationToken
        );
        Task<OperationResult<PagedResponse<PayoutRequestDto>>> GetMyRequestsAsync(
            Guid workerId,
            PagedQuery query,
            CancellationToken cancellationToken
        );
    }
}
