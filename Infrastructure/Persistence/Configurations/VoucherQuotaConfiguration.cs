using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class VoucherQuotaConfiguration : IEntityTypeConfiguration<VoucherQuota>
    {
        public void Configure(EntityTypeBuilder<VoucherQuota> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.VoucherId).IsUnique();
        }
    }
}
