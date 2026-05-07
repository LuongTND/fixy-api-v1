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
            builder.Property(x => x.Code).IsRequired();
            builder.Property(x => x.Type).HasConversion<string>();
            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => new { x.IsActive, x.ExpiresAt }).HasDatabaseName("idx_voucher_active");

            builder.HasOne(x => x.Category)
                .WithMany(x => x.Vouchers)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
