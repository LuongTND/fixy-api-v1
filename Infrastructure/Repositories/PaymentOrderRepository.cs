using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class PaymentOrderRepository : Repository<PaymentOrder>, IPaymentOrderRepository
    {
        public PaymentOrderRepository(AppDbContext context)
            : base(context) { }
    }
}
