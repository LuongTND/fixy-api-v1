using System.Reflection;
using Domain.Common;
using Domain.Entity;
using Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserOtp> UserOtps { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<WorkerProfile> WorkerProfiles { get; set; }
        public DbSet<WorkerCertificate> WorkerCertificates { get; set; }
        public DbSet<WorkerWeeklySchedule> WorkerSchedules { get; set; }
        public DbSet<WorkerScheduleException> WorkerScheduleExceptions { get; set; }

        public DbSet<WorkerService> WorkerServices { get; set; }
        public DbSet<WorkerPayoutAccount> WorkerPayoutAccounts { get; set; }
        public DbSet<WorkerFeaturedOrder> WorkerFeaturedOrders { get; set; }

        public DbSet<CustomerProfile> CustomerProfiles { get; set; }
        public DbSet<Address> Addresses { get; set; }

        public DbSet<ServiceCategory> ServiceCategories { get; set; }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<WorkerMatchingQueue> WorkerMatchingQueues { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<VoucherCampaign> VoucherCampaigns { get; set; }
        public DbSet<BookingVoucher> BookingVouchers { get; set; }
        public DbSet<VoucherUsageHistory> VoucherUsageHistories { get; set; }
        public DbSet<VoucherQuota> VoucherQuotas { get; set; }
        public DbSet<VoucherRestriction> VoucherRestrictions { get; set; }


        public DbSet<ChatMessage> ChatMessages { get; set; }

        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<PaymentOrder> PaymentOrders { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<WorkerEarning> WorkerEarnings { get; set; }
        public DbSet<PayoutRequest> PayoutRequests { get; set; }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationSetting> NotificationSettings { get; set; }

        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<SupportMessage> SupportMessages { get; set; }

        public DbSet<PlatformConfig> PlatformConfigs { get; set; }
        public DbSet<EventLog> EventLogs { get; set; }
        public DbSet<Media> Media { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            RoleSeeder.Seed(modelBuilder);
            UserSeeder.Seed(modelBuilder);
            // Apply all configurations from assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Global query filters
            ConfigureGlobalFilters(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        private void ConfigureGlobalFilters(ModelBuilder modelBuilder)
        {
            // Apply soft delete filter
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(
                        entityType.ClrType,
                        "e"
                    );
                    var property = System.Linq.Expressions.Expression.Property(
                        parameter,
                        nameof(ISoftDelete.IsDeleted)
                    );
                    var filter = System.Linq.Expressions.Expression.Lambda(
                        System.Linq.Expressions.Expression.Equal(
                            property,
                            System.Linq.Expressions.Expression.Constant(false)
                        ),
                        parameter
                    );
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }
            }
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default
        )
        {
            // Set audit fields
            foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedDate = DateTime.UtcNow;
                        break;
                }
            }

            // Set soft delete fields
            foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedDate = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
