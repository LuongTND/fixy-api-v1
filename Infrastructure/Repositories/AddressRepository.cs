using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class AddressRepository : Repository<Address>, IAddressRepository
    {
        public AddressRepository(AppDbContext context)
            : base(context) { }

        public async Task<List<Address>> GetByUserIdAsync(
            Guid userId,
            CancellationToken ct = default
        )
        {
            return await _dbSet.Where(x => x.CustomerId == userId && !x.IsDeleted).ToListAsync(ct);
        }

        public async Task<Address?> GetByIdAsync(
            Guid addressId,
            Guid userId,
            CancellationToken ct = default
        )
        {
            return await _dbSet.FirstOrDefaultAsync(
                x => x.Id == addressId && x.CustomerId == userId && !x.IsDeleted,
                ct
            );
        }

        public async Task<Address> GetDefaultByUserIdAsync(
            Guid userId,
            CancellationToken ct = default
        )
        {
            return await _dbSet.FirstAsync(
                x => x.CustomerId == userId && x.IsDefault && !x.IsDeleted,
                ct
            );
        }
    }
}
