using Application.Common;
using Application.DTOs.Wallet;
using Domain.Entity;
using Domain.Enum;

namespace Application.Interfaces.Services
{
    public interface IWalletService
    {
        Task<OperationResult<WalletOverviewDto>> GetWalletOverviewAsync(
            Guid userId,
            WalletOwnerType type,
            CancellationToken cancellationToken
        );

        Task<OperationResult<PagedResponse<WalletTransactionDto>>> GetWalletTransactionsAsync(
            Guid userId,
            WalletOwnerType type,
            PagedQuery query,
            CancellationToken cancellationToken
        );
        Task<OperationResult<WalletTransaction>> TopUpAsync(
            Guid userId,
            long amount,
            string externalId,
            CancellationToken cancellationToken
        );

        Task<OperationResult<WalletTransaction>> PayBookingAsync(
            Guid userId,
            Guid bookingId,
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
