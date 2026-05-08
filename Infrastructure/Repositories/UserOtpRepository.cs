using Application.Interfaces.Repositories;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserOtpRepository : Repository<UserOtp>, IUserOtpRepository
    {
        public UserOtpRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<UserOtp>> GetActiveOtpsAsync(Guid userId, UserOtpType type, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.UserId == userId && x.Type == type && !x.IsUsed)
                .ToListAsync(cancellationToken);
        }

        public async Task<UserOtp?> GetLatestActiveOtpAsync(Guid userId, UserOtpType type, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.UserId == userId && x.Type == type && !x.IsUsed)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
