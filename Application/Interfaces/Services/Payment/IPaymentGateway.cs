using Domain.Entity;

namespace Application.Interfaces.Services.Payment
{
    public interface IPaymentGateway
    {
        Task<string> CreatePaymentUrlAsync(PaymentOrder order, CancellationToken cancellationToken);

        bool VerifySignature(Dictionary<string, string> response);
    }
}
