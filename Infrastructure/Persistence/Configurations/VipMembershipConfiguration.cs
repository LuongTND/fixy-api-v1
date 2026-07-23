using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class VipMembershipConfiguration : IEntityTypeConfiguration<VipMembership>
    {
        public void Configure(EntityTypeBuilder<VipMembership> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DiscountPercent)
                .HasColumnType("decimal(5,2)");

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
