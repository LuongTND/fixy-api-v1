using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        DbSet<User> Users { get; }

        DbSet<Role> Roles { get; }

        DbSet<UserRole> UserRoles { get; }

        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<UserOtp> Otps { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        int SaveChanges();
    }
}
