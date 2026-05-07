using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class WorkerScheduleConfiguration : IEntityTypeConfiguration<WorkerSchedule>
    {
        public void Configure(EntityTypeBuilder<WorkerSchedule> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.DayOfWeek).HasConversion<string>();
            builder.HasIndex(x => new { x.WorkerId, x.DayOfWeek }).IsUnique();

            builder.HasOne(x => x.Worker)
                .WithMany(x => x.Schedules)
                .HasForeignKey(x => x.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
