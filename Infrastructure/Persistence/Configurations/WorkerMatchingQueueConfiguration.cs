using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class WorkerMatchingQueueConfiguration : IEntityTypeConfiguration<WorkerMatchingQueue>
    {
        public void Configure(EntityTypeBuilder<WorkerMatchingQueue> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Status).HasConversion<string>();
            builder.HasIndex(x => new { x.BookingId, x.Status }).HasDatabaseName("idx_mq_booking");
            builder
                .HasIndex(x => new
                {
                    x.WorkerProfileId,
                    x.Status,
                    x.OfferedAt,
                })
                .HasDatabaseName("idx_mq_worker");
            builder.HasIndex(x => x.ExpiresAt).HasDatabaseName("idx_mq_expires");

            builder
                .HasOne(x => x.Booking)
                .WithMany(x => x.MatchingQueue)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.Worker)
                .WithMany()
                .HasForeignKey(x => x.WorkerProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
