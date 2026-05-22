using Application.Common;
using Application.DTOs.Payout;
using Domain.Entity;

namespace Application.Interfaces.Services
{
    public interface IWorkerPayoutAccountService
    {
        Task<OperationResult<WorkerPayoutAccount>> CreateAsync(
            Guid workerId,
            CreateWorkerPayoutAccountDto dto,
            CancellationToken cancellationToken
        );

        Task<OperationResult<List<WorkerPayoutAccountDto>>> GetMyAccountsAsync(
            Guid workerId,
            CancellationToken cancellationToken
        );

        Task<OperationResult> SetDefaultAsync(
            Guid workerId,
            Guid payoutAccountId,
            CancellationToken cancellationToken
        );

        Task<OperationResult> DeleteAsync(
            Guid workerId,
            Guid payoutAccountId,
            CancellationToken cancellationToken
        );
    }
}
