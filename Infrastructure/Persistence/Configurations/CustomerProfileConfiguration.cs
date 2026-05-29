using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
    {
        public void Configure(EntityTypeBuilder<CustomerProfile> builder)
        {
            builder.HasKey(x => x.Id);

            // =========================
            // Indexes
            // =========================

            builder.HasIndex(x => x.UserId).IsUnique();

            // =========================
            // Relationships
            // =========================

            builder
                .HasOne(x => x.User)
                .WithOne(x => x.CustomerProfile)
                .HasForeignKey<CustomerProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(x => x.Addresses)
                .WithOne(x => x.CustomerProfile)
                .HasForeignKey(x => x.CustomerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(x => x.Bookings)
                .WithOne(x => x.CustomerProfile)
                .HasForeignKey(x => x.CustomerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(x => x.Reviews)
                .WithOne(x => x.CustomerProfile)
                .HasForeignKey(x => x.CustomerProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
