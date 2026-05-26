using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class VoucherRestrictionConfiguration : IEntityTypeConfiguration<VoucherRestriction>
    {
        public void Configure(EntityTypeBuilder<VoucherRestriction> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type).HasConversion<string>();
            builder.Property(x => x.Value).IsRequired().HasMaxLength(255);

            builder.HasIndex(x => new { x.VoucherId, x.Type }).HasDatabaseName("idx_restriction_voucher_type");
        }
    }
}
