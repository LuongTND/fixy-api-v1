using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CustomerProfileRepository : Repository<CustomerProfile>, ICustomerProfileRepository
    {
        public CustomerProfileRepository(AppDbContext context)
            : base(context) { }

        public async Task<CustomerProfile?> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        }
    }
}
