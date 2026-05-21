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

            // =========================
            // Basic Info
            // =========================

            builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();

            builder.Property(x => x.Phone).HasMaxLength(20);

            builder.Property(x => x.Email).HasMaxLength(255);

            builder.Property(x => x.PasswordHash).IsRequired();

            builder.Property(x => x.AvatarUrl).HasMaxLength(1000);
            builder.Property(x => x.Gender).HasConversion<string>();

            // =========================
            // Citizen ID
            // =========================

            builder.Property(x => x.CitizenIdNumber).HasMaxLength(50);

            builder.Property(x => x.CitizenIdIssuePlace).HasMaxLength(255);

            // =========================
            // OAuth
            // =========================

            builder.Property(x => x.OAuthProvider).HasConversion<string>();

            builder.Property(x => x.OAuthId).HasMaxLength(255);

            // =========================
            // TOTP
            // =========================

            builder.Property(x => x.TotpSecret).HasMaxLength(500);

            // =========================
            // Indexes
            // =========================

            builder.HasIndex(x => x.Email).IsUnique();

            builder.HasIndex(x => x.Phone).IsUnique();

            builder.HasIndex(x => x.CitizenIdNumber).IsUnique();

            // =========================
            // Relationships
            // =========================

            builder
                .HasOne(x => x.CustomerProfile)
                .WithOne(x => x.User)
                .HasForeignKey<CustomerProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.WorkerProfile)
                .WithOne(x => x.User)
                .HasForeignKey<WorkerProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
