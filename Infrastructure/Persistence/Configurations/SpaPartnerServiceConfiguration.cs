using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class SpaPartnerServiceConfiguration : IEntityTypeConfiguration<SpaPartnerService>
    {
        public void Configure(EntityTypeBuilder<SpaPartnerService> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasMaxLength(250).IsRequired();

            builder.HasOne(x => x.SpaPartner)
                .WithMany(x => x.Services)
                .HasForeignKey(x => x.SpaPartnerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SpaServiceCategory)
                .WithMany(x => x.SpaPartnerServices)
                .HasForeignKey(x => x.SpaServiceCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
