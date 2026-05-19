using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class WorkerProfileConfiguration : IEntityTypeConfiguration<WorkerProfile>
    {
        public void Configure(EntityTypeBuilder<WorkerProfile> builder)
        {
            builder.HasKey(x => x.Id);

            // =========================
            // Enum Conversion
            // =========================

            builder.Property(x => x.Status).HasConversion<string>();

            builder.Property(x => x.Badge).HasConversion<string>();

            builder.Property(x => x.FeaturedPackage).HasConversion<string>();

            // =========================
            // Profile
            // =========================

            builder.Property(x => x.Bio).HasMaxLength(2000);

            builder.Property(x => x.ExperienceYears).HasDefaultValue(0);

            // =========================
            // Working Area
            // =========================

            builder.Property(x => x.MaxDistanceKm).HasDefaultValue(10);

            // =========================
            // Realtime Status
            // =========================

            builder.Property(x => x.IsOnline).HasDefaultValue(false);

            builder.Property(x => x.IsBusy).HasDefaultValue(false);

            builder.Property(x => x.IsAcceptingJobs).HasDefaultValue(true);

            // =========================
            // Statistics
            // =========================

            builder.Property(x => x.RatingAvg).HasPrecision(4, 2).HasDefaultValue(0);

            builder.Property(x => x.TotalReviews).HasDefaultValue(0);

            builder.Property(x => x.TotalOrders).HasDefaultValue(0);

            // =========================
            // Indexes
            // =========================

            builder.HasIndex(x => x.UserId).IsUnique();

            builder
                .HasIndex(x => new { x.Status, x.RatingAvg })
                .HasDatabaseName("idx_worker_search");

            builder
                .HasIndex(x => new
                {
                    x.Status,
                    x.IsOnline,
                    x.IsAcceptingJobs,
                })
                .HasDatabaseName("idx_worker_available");

            builder.HasIndex(x => x.FeaturedUntil).HasDatabaseName("idx_worker_featured");

            // =========================
            // Relationships
            // =========================

            builder
                .HasOne(x => x.User)
                .WithOne(x => x.WorkerProfile)
                .HasForeignKey<WorkerProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.ApprovedBy)
                .WithMany()
                .HasForeignKey(x => x.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
