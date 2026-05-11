using Application.Interfaces;
using Application.Interfaces.Repositories;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Repositories;

namespace Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            Users = new UserRepository(context);
            Roles = new RoleRepository(context);
            UserRoles = new UserRoleRepository(context);
            RefreshTokens = new RefreshTokenRepository(context);
            Otps = new UserOtpRepository(context);
            Addresses = new AddressRepository(context);
        }

        public IUserRepository Users { get; }
        public IRoleRepository Roles { get; }
        public IUserRoleRepository UserRoles { get; }
        public IRefreshTokenRepository RefreshTokens { get; }
        public IUserOtpRepository Otps { get; }
        public IAddressRepository Addresses { get; }

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
