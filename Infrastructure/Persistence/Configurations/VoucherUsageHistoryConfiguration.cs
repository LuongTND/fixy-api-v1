using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class VoucherUsageHistoryConfiguration : IEntityTypeConfiguration<VoucherUsageHistory>
    {
        public void Configure(EntityTypeBuilder<VoucherUsageHistory> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FailReason).HasMaxLength(500);

            builder.HasIndex(x => new { x.VoucherId, x.UserId }).HasDatabaseName("idx_usage_voucher_user");
            builder.HasIndex(x => x.BookingId).HasDatabaseName("idx_usage_booking");

            builder.HasOne(x => x.Voucher)
                .WithMany(x => x.UsageHistories)
                .HasForeignKey(x => x.VoucherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Booking)
                .WithMany()
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
