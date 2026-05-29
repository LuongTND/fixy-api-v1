using Application.DTOs.User;
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

        public async Task<(List<User>, int)> GetPagedUsersAsync(
            UserManagementQuery query,
            CancellationToken cancellationToken = default
        )
        {
            var usersQuery = _context
                .Users.Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();

                usersQuery = usersQuery.Where(x =>
                    x.FullName.ToLower().Contains(search)
                    || (x.Email != null && x.Email.ToLower().Contains(search))
                    || (x.Phone != null && x.Phone.ToLower().Contains(search))
                );
            }

            // FILTER ROLE
            if (!string.IsNullOrWhiteSpace(query.Role))
            {
                var role = query.Role.Trim().ToLower();

                usersQuery = usersQuery.Where(x =>
                    x.UserRoles.Any(r => r.Role!.Name.ToLower() == role)
                );
            }

            // FILTER ACTIVE
            if (query.IsActive.HasValue)
            {
                usersQuery = usersQuery.Where(x => x.IsActive == query.IsActive.Value);
            }

            // SORT
            usersQuery = usersQuery.OrderByDescending(x => x.CreatedDate);

            var totalCount = await usersQuery.CountAsync(cancellationToken);

            var items = await usersQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

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
