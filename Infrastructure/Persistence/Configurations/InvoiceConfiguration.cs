using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.InvoiceNo).IsRequired();
            builder.HasIndex(x => x.InvoiceNo).IsUnique().HasDatabaseName("idx_invoice_no");
            builder.HasIndex(x => x.BookingId).IsUnique();

            builder.HasOne(x => x.Booking)
                .WithOne(x => x.Invoice)
                .HasForeignKey<Invoice>(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PaymentOrder)
                .WithOne(x => x.Invoice)
                .HasForeignKey<Invoice>(x => x.PaymentOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
