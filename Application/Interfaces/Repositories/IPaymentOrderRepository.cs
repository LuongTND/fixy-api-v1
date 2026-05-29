using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IPaymentOrderRepository : IRepository<PaymentOrder>
    {
        Task<PaymentOrder?> GetBookingPaymentOrderAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default
        );
        Task<PaymentOrder?> GetByGatewayOrderCodeAsync(
            long gatewayOrderCode,
            CancellationToken cancellationToken
        );
    }
}
