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

            // =========================
            // Enum Conversion
            // =========================

            builder.Property(x => x.Status).HasConversion<string>();

            builder.Property(x => x.ScheduledType).HasConversion<string>();

            // =========================
            // Properties
            // =========================

            builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();

            builder.Property(x => x.Address).HasMaxLength(500).IsRequired();

            builder.Property(x => x.WorkerProposedNote).HasMaxLength(1000);

            builder.Property(x => x.CancelReason).HasMaxLength(1000);

            // =========================
            // Relationships
            // =========================

            builder
                .HasOne(x => x.CustomerProfile)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.CustomerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.WorkerProfile)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.WorkerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.CancelledBy)
                .WithMany()
                .HasForeignKey(x => x.CancelledById)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.ReorderFrom)
                .WithMany(x => x.Reorders)
                .HasForeignKey(x => x.ReorderFromId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // Indexes
            // =========================

            builder
                .HasIndex(x => new { x.CustomerProfileId, x.Status })
                .HasDatabaseName("idx_booking_customer");

            builder
                .HasIndex(x => new { x.WorkerProfileId, x.Status })
                .HasDatabaseName("idx_booking_worker");

            builder.HasIndex(x => x.ScheduledAt).HasDatabaseName("idx_booking_schedule");

            builder
                .HasIndex(x => new { x.Status, x.CreatedDate })
                .HasDatabaseName("idx_booking_status");
        }
    }
}
