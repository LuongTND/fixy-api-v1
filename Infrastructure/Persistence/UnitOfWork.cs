using Application.Interfaces;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public DbSet<User> Users => _context.Users;

        public DbSet<Role> Roles => _context.Roles;

        public DbSet<UserRole> UserRoles => _context.UserRoles;

        public DbSet<RefreshToken> RefreshTokens => _context.RefreshTokens;

        public DbSet<UserOtp> Otps => _context.UserOtps;
        public DbSet<Address> Addresses => _context.Addresses;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
