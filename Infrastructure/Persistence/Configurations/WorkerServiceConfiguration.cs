using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class WorkerServiceConfiguration : IEntityTypeConfiguration<WorkerService>
    {
        public void Configure(EntityTypeBuilder<WorkerService> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.WorkerProfileId, x.CategoryId }).IsUnique();

            builder
                .HasOne(x => x.Worker)
                .WithMany(x => x.Services)
                .HasForeignKey(x => x.WorkerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.Category)
                .WithMany(x => x.WorkerServices)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
