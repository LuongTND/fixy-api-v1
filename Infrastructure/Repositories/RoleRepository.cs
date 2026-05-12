using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        public RoleRepository(AppDbContext context)
            : base(context) { }

        public async Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Name == name, ct);
        }

        public async Task<Role> GetCustomerRoleAsync(CancellationToken ct)
        {
            return await _dbSet.FirstAsync(x => x.Name == "CUSTOMER", ct);
        }
    }
}
