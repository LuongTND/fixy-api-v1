using Application.Interfaces.Repositories;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PaymentOrderRepository : Repository<PaymentOrder>, IPaymentOrderRepository
    {
        public PaymentOrderRepository(AppDbContext context)
            : base(context) { }

        public async Task<PaymentOrder?> GetBookingPaymentOrderAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet.FirstOrDefaultAsync(
                x => x.BookingId == bookingId && x.Type == PaymentOrderType.BookingPayment,
                cancellationToken
            );
        }
    }
}
