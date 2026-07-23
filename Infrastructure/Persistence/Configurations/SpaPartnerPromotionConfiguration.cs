using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class SpaPartnerPromotionConfiguration : IEntityTypeConfiguration<SpaPartnerPromotion>
    {
        public void Configure(EntityTypeBuilder<SpaPartnerPromotion> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).HasMaxLength(250).IsRequired();

            builder.Property(x => x.DiscountPercent)
                .HasColumnType("decimal(5,2)");

            builder.HasOne(x => x.SpaPartner)
                .WithMany(x => x.Promotions)
                .HasForeignKey(x => x.SpaPartnerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
