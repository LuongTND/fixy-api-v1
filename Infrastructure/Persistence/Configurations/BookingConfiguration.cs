using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Description).IsRequired();
            builder.Property(x => x.Address).IsRequired();
            builder.Property(x => x.ScheduledType).HasConversion<string>();
            builder.Property(x => x.Status).HasConversion<string>();
            builder.HasIndex(x => new { x.CustomerId, x.Status, x.CreatedDate }).HasDatabaseName("idx_booking_customer");
            builder.HasIndex(x => new { x.WorkerId, x.Status }).HasDatabaseName("idx_booking_worker");
            builder.HasIndex(x => new { x.Status, x.ScheduledAt }).HasDatabaseName("idx_booking_scheduled");

            builder.HasOne(x => x.Customer)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Worker)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Category)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CancelledBy)
                .WithMany()
                .HasForeignKey(x => x.CancelledById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReorderFrom)
                .WithMany(x => x.Reorders)
                .HasForeignKey(x => x.ReorderFromId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
