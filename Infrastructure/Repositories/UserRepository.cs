using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context)
            : base(context) { }

        public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<User?> GetByIdWithProfilesAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(x => x.CustomerProfile)
                .Include(x => x.WorkerProfile)
                .Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<User?> GetByTargetAsync(string target, CancellationToken ct = default)
        {
            return await _dbSet.FirstOrDefaultAsync(
                x => x.Email == target || x.Phone == target,
                ct
            );
        }

        public async Task<User?> GetByTargetWithRoleAsync(
            string target,
            CancellationToken ct = default
        )
        {
            return await _dbSet
                .Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == target || x.Phone == target, ct);
        }

        public async Task<User?> GetWithCustomerProfileByIdAsync(
            Guid userId,
            CancellationToken ct = default
        )
        {
            return await _dbSet
                .Include(x => x.CustomerProfile)
                .FirstOrDefaultAsync(x => x.Id == userId, ct);
        }

        public async Task<User?> GetWithWorkerProfileByIdAsync(
            Guid userId,
            CancellationToken ct = default
        )
        {
            return await _dbSet
                .Include(x => x.WorkerProfile)
                .FirstOrDefaultAsync(x => x.Id == userId, ct);
        }
    }
}
