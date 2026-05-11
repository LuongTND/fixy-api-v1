using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;

namespace Infrastructure.Persistence.Repositories
{
    public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(AppDbContext context)
            : base(context) { }

        public async Task<bool> ExistsAsync(
            Guid userId,
            Guid roleId,
            CancellationToken ct = default
        )
        {
            return await _dbSet.AnyAsync(x => x.UserId == userId && x.RoleId == roleId, ct);
        }
    }
}
