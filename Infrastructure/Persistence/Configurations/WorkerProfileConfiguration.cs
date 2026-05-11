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
            builder.Property(x => x.Status).HasConversion<string>();
            builder.Property(x => x.Badge).HasConversion<string>();
            builder.Property(x => x.FeaturedPackage).HasConversion<string>();
            builder.HasIndex(x => x.UserId).IsUnique();
            builder.HasIndex(x => new { x.Lat, x.Lng }).HasDatabaseName("idx_worker_geo");
            builder
                .HasIndex(x => new { x.Status, x.RatingAvg })
                .HasDatabaseName("idx_worker_search");
            builder
                .HasIndex(x => new { x.Status, x.IsOnline })
                .HasDatabaseName("idx_worker_available");
            builder.HasIndex(x => x.FeaturedUntil).HasDatabaseName("idx_worker_featured");

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
