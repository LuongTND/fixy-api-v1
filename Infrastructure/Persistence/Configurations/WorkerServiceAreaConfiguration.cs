using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class WorkerServiceAreaConfiguration : IEntityTypeConfiguration<WorkerServiceArea>
    {
        public void Configure(EntityTypeBuilder<WorkerServiceArea> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Province).IsRequired();
            builder.Property(x => x.District).IsRequired();
            builder.HasIndex(x => new { x.WorkerId, x.Province, x.District }).IsUnique();

            builder.HasOne(x => x.Worker)
                .WithMany(x => x.ServiceAreas)
                .HasForeignKey(x => x.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
