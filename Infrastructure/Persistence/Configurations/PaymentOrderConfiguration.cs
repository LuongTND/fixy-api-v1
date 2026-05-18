using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class PaymentOrderConfiguration : IEntityTypeConfiguration<PaymentOrder>
    {
        public void Configure(EntityTypeBuilder<PaymentOrder> builder)
        {
            builder.ToTable("PaymentOrders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount).IsRequired();

            builder.Property(x => x.DiscountAmount).HasDefaultValue(0);

            builder.Property(x => x.FinalAmount).IsRequired();

            builder.Property(x => x.Method).IsRequired().HasConversion<string>();

            builder.Property(x => x.Status).IsRequired().HasConversion<string>();

            builder.Property(x => x.Type).IsRequired().HasConversion<string>();

            builder.Property(x => x.GatewayRef).HasMaxLength(255);

            builder.Property(x => x.ExternalTransactionId).HasMaxLength(255);

            builder.Property(x => x.GatewayResponse).HasColumnType("nvarchar(max)");

            builder.HasIndex(x => x.UserId);

            builder.HasIndex(x => x.Status);

            builder.HasIndex(x => x.Type);

            builder
                .HasIndex(x => x.ExternalTransactionId)
                .IsUnique()
                .HasFilter("[ExternalTransactionId] IS NOT NULL");

            // USER
            builder
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // BOOKING 1 - 1
            builder
                .HasOne(x => x.Booking)
                .WithOne(x => x.PaymentOrder)
                .HasForeignKey<PaymentOrder>(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // INVOICE 1 - 1
            builder
                .HasOne(x => x.Invoice)
                .WithOne(x => x.PaymentOrder)
                .HasForeignKey<Invoice>(x => x.PaymentOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // WORKER EARNING 1 - 1
            builder
                .HasOne(x => x.WorkerEarning)
                .WithOne(x => x.PaymentOrder)
                .HasForeignKey<WorkerEarning>(x => x.PaymentOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
