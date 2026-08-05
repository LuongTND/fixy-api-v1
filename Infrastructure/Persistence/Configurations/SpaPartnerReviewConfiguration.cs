using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class SpaPartnerReviewConfiguration : IEntityTypeConfiguration<SpaPartnerReview>
    {
        public void Configure(EntityTypeBuilder<SpaPartnerReview> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Comment).HasMaxLength(1000);

            builder.HasOne(x => x.SpaPartner)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.SpaPartnerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.CustomerProfile)
                .WithMany()
                .HasForeignKey(x => x.CustomerProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
