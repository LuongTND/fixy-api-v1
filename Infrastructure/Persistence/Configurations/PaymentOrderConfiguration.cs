using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class PaymentOrderConfiguration : IEntityTypeConfiguration<PaymentOrder>
    {
        public void Configure(EntityTypeBuilder<PaymentOrder> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Method).HasConversion<string>();
            builder.Property(x => x.Status).HasConversion<string>();
            builder.HasIndex(x => x.BookingId).IsUnique();
            builder.HasIndex(x => new { x.CustomerId, x.Status }).HasDatabaseName("idx_po_customer");
            builder.HasIndex(x => x.GatewayRef).HasDatabaseName("idx_po_gateway");

            builder.HasOne(x => x.Booking)
                .WithOne(x => x.PaymentOrder)
                .HasForeignKey<PaymentOrder>(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
