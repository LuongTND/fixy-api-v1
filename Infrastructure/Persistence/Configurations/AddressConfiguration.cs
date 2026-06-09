using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.HasKey(x => x.Id);

            // =========================
            // Properties
            // =========================

            builder.Property(x => x.Label).HasMaxLength(100);

            builder.Property(x => x.City).HasMaxLength(100).IsRequired();

            builder.Property(x => x.Ward).HasMaxLength(100).IsRequired();

            builder.Property(x => x.Detail).HasMaxLength(300).IsRequired();

            // =========================
            // Relationships
            // =========================

            // CustomerProfile 1-n Address
            builder
                .HasOne(x => x.CustomerProfile)
                .WithMany(x => x.Addresses)
                .HasForeignKey(x => x.CustomerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // WorkerProfile 1-1 Address
            builder
                .HasOne(x => x.WorkerProfile)
                .WithOne(x => x.Address)
                .HasForeignKey<Address>(x => x.WorkerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // Indexes
            // =========================

            // Worker chỉ có 1 address
            builder.HasIndex(x => x.WorkerProfileId).IsUnique();

            // Customer chỉ có 1 default address
            builder
                .HasIndex(x => new { x.CustomerProfileId, x.IsDefault })
                .HasDatabaseName("idx_customer_default_address");
        }
    }
}
