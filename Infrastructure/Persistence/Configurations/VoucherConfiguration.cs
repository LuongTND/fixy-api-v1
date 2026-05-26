using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
    {
        public void Configure(EntityTypeBuilder<Voucher> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Type).HasConversion<string>();
            builder.Property(x => x.Status).HasConversion<string>();
            builder.Property(x => x.Description).HasMaxLength(500);

            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => new { x.Status, x.ExpiresAt }).HasDatabaseName("idx_voucher_status");

            builder.HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // 1-1 relationship with VoucherQuota
            builder.HasOne(x => x.Quota)
                .WithOne(q => q.Voucher)
                .HasForeignKey<VoucherQuota>(q => q.VoucherId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1-N relationship with VoucherRestriction
            builder.HasMany(x => x.Restrictions)
                .WithOne(r => r.Voucher)
                .HasForeignKey(r => r.VoucherId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
