using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class WorkerScheduleExceptionConfiguration
        : IEntityTypeConfiguration<WorkerScheduleException>
    {
        public void Configure(EntityTypeBuilder<WorkerScheduleException> builder)
        {
            builder.ToTable("WorkerScheduleExceptions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Date).IsRequired();

            builder.Property(x => x.IsDayOff).HasDefaultValue(false);

            // nullable vì có thể:
            // - nghỉ cả ngày
            // - custom giờ riêng
            builder.Property(x => x.StartTime);

            builder.Property(x => x.EndTime);

            builder.Property(x => x.Reason).HasMaxLength(500);

            // 1 worker chỉ có 1 exception/ngày
            builder.HasIndex(x => new { x.WorkerProfileId, x.Date }).IsUnique();

            builder
                .HasOne(x => x.WorkerProfile)
                .WithMany(x => x.ScheduleExceptions)
                .HasForeignKey(x => x.WorkerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.WorkerProfileId);

            builder.HasIndex(x => x.Date);
        }
    }
}
