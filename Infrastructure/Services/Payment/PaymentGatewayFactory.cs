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

                PaymentMethod.Momo => _serviceProvider.GetRequiredService<MoMoService>(),
                PaymentMethod.PayOS => _serviceProvider.GetRequiredService<PayOSService>(),

                _ => throw new Exception("Payment method not supported"),
            };
        }
    }
}
