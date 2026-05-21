using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(x => x.Id);

            // =========================
            // Properties
            // =========================

            builder.Property(x => x.Rating).IsRequired();

            builder.Property(x => x.Comment).HasMaxLength(2000);

            builder.Property(x => x.WorkerReply).HasMaxLength(2000);

            builder.Property(x => x.IsVisible).HasDefaultValue(true);

            // =========================
            // Relationships
            // =========================

            // 1 booking chỉ có 1 review
            builder
                .HasOne(x => x.Booking)
                .WithOne(x => x.Review)
                .HasForeignKey<Review>(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(x => x.CustomerProfile)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.CustomerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.WorkerProfile)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.WorkerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // Indexes
            // =========================

            builder.HasIndex(x => x.BookingId).IsUnique();

            builder
                .HasIndex(x => new { x.WorkerProfileId, x.Rating })
                .HasDatabaseName("idx_review_worker_rating");

            builder
                .HasIndex(x => new { x.CustomerProfileId, x.CreatedDate })
                .HasDatabaseName("idx_review_customer");

            builder.HasIndex(x => x.IsVisible).HasDatabaseName("idx_review_visible");
        }
    }
}
