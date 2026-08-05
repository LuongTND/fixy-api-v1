using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class SpaPartnerGalleryConfiguration : IEntityTypeConfiguration<SpaPartnerGallery>
    {
        public void Configure(EntityTypeBuilder<SpaPartnerGallery> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Caption).HasMaxLength(250);

            builder.HasOne(x => x.SpaPartner)
                .WithMany(x => x.Gallery)
                .HasForeignKey(x => x.SpaPartnerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
