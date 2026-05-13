using Application.Common;
using Domain.Entity;
using Domain.Enum;

namespace Application.Interfaces.Services
{
    public interface IWalletService
    {
        Task<OperationResult<Wallet>> GetWalletAsync(
            Guid userId,
            WalletOwnerType type,
            CancellationToken cancellationToken
        );
        Task<OperationResult<List<WalletTransaction>>> GetWalletTransactionAsync(
            Guid userId,
            WalletOwnerType type,
            CancellationToken cancellationToken
        );
        Task<OperationResult<WalletTransaction>> TopUpAsync(
            Guid userId,
            long amount,
            string externalId,
            CancellationToken cancellationToken
        );

        Task<OperationResult<WalletTransaction>> PayAsync(
            Guid walletId,
            long amount,
            string referenceId,
            CancellationToken cancellationToken
        );

        Task<OperationResult<WalletTransaction>> RefundAsync(
            Guid walletId,
            long amount,
            string referenceId,
            CancellationToken cancellationToken
        );

        Task<OperationResult<WalletTransaction>> WithdrawAsync(
            Guid walletId,
            long amount,
            string referenceId,
            CancellationToken cancellationToken
        );
    }
}
