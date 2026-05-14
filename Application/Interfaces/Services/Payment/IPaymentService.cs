using Application.Common;
using Domain.Enum;

namespace Application.Interfaces.Services.Payment
{
    public interface IPaymentService
    {
        Task<OperationResult<string>> CreateTopUpPaymentUrlAsync(
            Guid userId,
            long amount,
            PaymentMethod method,
            CancellationToken cancellationToken
        );

        Task<OperationResult<bool>> HandleVnPayCallbackAsync(
            Dictionary<string, string> response,
            CancellationToken cancellationToken
        );
    }
}
