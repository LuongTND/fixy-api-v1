using Application.Interfaces.Repositories;
using Domain.Entity;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UserOtpRepository : Repository<UserOtp>, IUserOtpRepository
    {
        public UserOtpRepository(AppDbContext context)
            : base(context) { }

        public async Task<UserOtp?> GetLatestByTargetAsync(
            string target,
            CancellationToken ct = default
        )
        {
            return await _dbSet
                .Where(x => x.Target == target)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<UserOtp?> GetVerifiedOtpAsync(
            string target,
            CancellationToken ct = default
        )
        {
            return await _dbSet
                .Where(x => x.Target == target && x.IsVerified && x.ExpiryDate > DateTime.UtcNow)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<List<UserOtp>> GetUnusedOtpsAsync(
            string target,
            CancellationToken ct = default
        )
        {
            return await _dbSet.Where(x => x.Target == target && !x.IsUsed).ToListAsync(ct);
        }

        public async Task<UserOtp?> GetLatestOtpAsync(
            string target,
            string otpCode,
            CancellationToken ct = default
        )
        {
            return await _dbSet
                .Where(x => x.Target == target && x.OtpCode == otpCode && !x.IsUsed)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<UserOtp?> GetLatestOtpByTargetAsync(string target, CancellationToken ct)
        {
            return await _dbSet
                .Where(x => x.Target == target)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(ct);
        }
    }
}
