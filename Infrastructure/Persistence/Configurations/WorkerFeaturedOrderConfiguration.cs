using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class WorkerFeaturedOrderConfiguration : IEntityTypeConfiguration<WorkerFeaturedOrder>
    {
        public void Configure(EntityTypeBuilder<WorkerFeaturedOrder> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PackageName).HasConversion<string>();
            builder.HasIndex(x => new { x.WorkerId, x.EndsAt }).HasDatabaseName("idx_featured_orders_worker");

            builder.HasOne(x => x.Worker)
                .WithMany(x => x.FeaturedOrders)
                .HasForeignKey(x => x.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
