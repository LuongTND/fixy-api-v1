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

        public async Task<List<Address>> GetByCustomerProfileIdAsync(
            Guid customerProfileId,
            CancellationToken ct = default
        )
        {
            return await _dbSet
                .Where(x => x.CustomerProfileId == customerProfileId && !x.IsDeleted)
                .OrderByDescending(x => x.IsDefault)
                .ToListAsync(ct);
        }

        public async Task<Address?> GetWorkerAddressAsync(
            Guid workerProfileId,
            CancellationToken ct = default
        )
        {
            return await _dbSet.FirstOrDefaultAsync(
                x => x.WorkerProfileId == workerProfileId && !x.IsDeleted,
                ct
            );
        }

        public async Task<Address?> GetDefaultByCustomerProfileIdAsync(
            Guid customerProfileId,
            CancellationToken ct = default
        )
        {
            return await _dbSet.FirstOrDefaultAsync(
                x => x.CustomerProfileId == customerProfileId && x.IsDefault && !x.IsDeleted,
                ct
            );
        }
    }
}
