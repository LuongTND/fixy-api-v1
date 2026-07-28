using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class WorkerServiceOptionConfiguration : IEntityTypeConfiguration<WorkerServiceOption>
    {
        public void Configure(EntityTypeBuilder<WorkerServiceOption> builder)
        {
            builder.ToTable("WorkerServiceOptions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DurationMinutes)
                .IsRequired();

            builder.Property(x => x.Price)
                .IsRequired();

            builder.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.HasOne(x => x.WorkerService)
                .WithMany(x => x.Options)
                .HasForeignKey(x => x.WorkerServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
