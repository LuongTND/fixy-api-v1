using Application.Interfaces.Services.Payment;
using Domain.Enum;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.Payment
{
    public class PaymentGatewayFactory : IPaymentGatewayFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public PaymentGatewayFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPaymentGateway Get(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Vnpay => _serviceProvider.GetRequiredService<VnPayService>(),

                _ => throw new Exception("Payment method not supported"),
            };
        }
    }
}
