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
            builder
                .HasIndex(x => new
                {
                    x.WorkerProfileId,
                    x.IsVisible,
                    x.CreatedDate,
                })
                .HasDatabaseName("idx_review_worker");
            builder.HasIndex(x => x.BookingId).IsUnique();

            builder
                .HasOne(x => x.Booking)
                .WithOne(x => x.Review)
                .HasForeignKey<Review>(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.Customer)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.CustomerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.Worker)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.WorkerProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
