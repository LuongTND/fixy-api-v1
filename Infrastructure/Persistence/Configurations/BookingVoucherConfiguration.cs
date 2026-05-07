using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class BookingVoucherConfiguration : IEntityTypeConfiguration<BookingVoucher>
    {
        public void Configure(EntityTypeBuilder<BookingVoucher> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.BookingId).IsUnique();

            builder.HasOne(x => x.Booking)
                .WithOne(x => x.BookingVoucher)
                .HasForeignKey<BookingVoucher>(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Voucher)
                .WithMany(x => x.BookingVouchers)
                .HasForeignKey(x => x.VoucherId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
