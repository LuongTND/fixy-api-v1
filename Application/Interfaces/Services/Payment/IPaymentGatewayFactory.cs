using Domain.Enum;

namespace Application.Interfaces.Services.Payment
{
    public interface IPaymentGatewayFactory
    {
        IPaymentGateway Get(PaymentMethod method);
    }
}
