using Domain.Entity.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        DbSet<User> Users { get; }

        DbSet<Role> Roles { get; }

        DbSet<UserRole> UserRoles { get; }

        DbSet<RefreshToken> RefreshTokens { get; }

        DbSet<OtpVerification> OtpVerifications { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        int SaveChanges();
    }
}
