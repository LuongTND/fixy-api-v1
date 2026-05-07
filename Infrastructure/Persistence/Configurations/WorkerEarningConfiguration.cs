using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class WorkerEarningConfiguration : IEntityTypeConfiguration<WorkerEarning>
    {
        public void Configure(EntityTypeBuilder<WorkerEarning> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Status).HasConversion<string>();
            builder.HasIndex(x => new { x.WorkerId, x.Status }).HasDatabaseName("idx_earn_worker");
            builder.HasIndex(x => x.PaymentOrderId).HasDatabaseName("idx_earn_payment");

            builder.HasOne(x => x.Worker)
                .WithMany(x => x.Earnings)
                .HasForeignKey(x => x.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Booking)
                .WithOne(x => x.WorkerEarning)
                .HasForeignKey<WorkerEarning>(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PaymentOrder)
                .WithOne(x => x.WorkerEarning)
                .HasForeignKey<WorkerEarning>(x => x.PaymentOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PayoutRequest)
                .WithMany(x => x.WorkerEarnings)
                .HasForeignKey(x => x.PayoutRequestId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
