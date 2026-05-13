using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FullName).IsRequired().HasMaxLength(100);

            builder.Property(x => x.Phone).HasMaxLength(20);

            builder.Property(x => x.Email).HasMaxLength(255);

            builder.Property(x => x.PasswordHash).HasMaxLength(500);

            builder.Property(x => x.CitizenIdNumber).HasMaxLength(20);
            builder.Property(x => x.CitizenIdIssuePlace).HasMaxLength(255);
            builder.Property(x => x.CitizenIdIssueDate);
            builder.Property(x => x.IsCitizenIdVerified);
            builder.Property(x => x.CitizenIdVerifiedAt);

            builder.Property(x => x.OAuthId).HasMaxLength(255);

            builder.Property(x => x.TotpSecret).HasMaxLength(500);

            builder.Property(x => x.DateOfBirth);

            // enum -> string
            builder.Property(x => x.Gender).HasConversion<string>().HasMaxLength(20);

            builder.HasIndex(x => x.Email).IsUnique();

            builder.HasIndex(x => x.Phone).IsUnique();

            builder.HasQueryFilter(x => !x.IsDeleted);

            // User - WorkerProfile
            builder
                .HasOne(x => x.WorkerProfile)
                .WithOne(x => x.User)
                .HasForeignKey<WorkerProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - CustomerProfile
            builder
                .HasOne(x => x.CustomerProfile)
                .WithOne(x => x.User)
                .HasForeignKey<CustomerProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - Wallet
            builder
                .HasOne(x => x.Wallet)
                .WithOne(x => x.User)
                .HasForeignKey<Wallet>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - NotificationSetting
            builder
                .HasOne(x => x.NotificationSetting)
                .WithOne(x => x.User)
                .HasForeignKey<NotificationSetting>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - RefreshTokens
            builder
                .HasMany(x => x.RefreshTokens)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - UserRoles
            builder
                .HasMany(x => x.UserRoles)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - Notifications
            builder
                .HasMany(x => x.Notifications)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // User - Address
            builder
                .HasMany(x => x.Addresses)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
