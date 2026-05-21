using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class CustomerProfileRepository : Repository<CustomerProfile>, ICustomerProfileRepository
    {
        public CustomerProfileRepository(AppDbContext context)
            : base(context) { }
    }
}
