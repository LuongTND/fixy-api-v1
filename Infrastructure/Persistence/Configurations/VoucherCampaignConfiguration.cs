using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class VoucherCampaignConfiguration : IEntityTypeConfiguration<VoucherCampaign>
    {
        public void Configure(EntityTypeBuilder<VoucherCampaign> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.Status).HasConversion<string>();
            builder.Property(x => x.AutoTriggerEvent).HasConversion<string>();

            // 1-N relationship with Voucher
            builder.HasMany(c => c.Vouchers)
                .WithOne(v => v.Campaign)
                .HasForeignKey(v => v.CampaignId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
