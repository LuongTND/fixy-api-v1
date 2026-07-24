using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ReferralRecordConfiguration : IEntityTypeConfiguration<ReferralRecord>
    {
        public void Configure(EntityTypeBuilder<ReferralRecord> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReferralCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasOne(x => x.ReferrerUser)
                .WithMany()
                .HasForeignKey(x => x.ReferrerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReferredUser)
                .WithMany()
                .HasForeignKey(x => x.ReferredUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RewardVoucher)
                .WithMany()
                .HasForeignKey(x => x.RewardVoucherId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
