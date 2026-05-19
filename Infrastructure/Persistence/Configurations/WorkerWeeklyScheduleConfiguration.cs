using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class WorkerWeeklyScheduleConfiguration : IEntityTypeConfiguration<WorkerWeeklySchedule>
    {
        public void Configure(EntityTypeBuilder<WorkerWeeklySchedule> builder)
        {
            builder.ToTable("WorkerWeeklySchedules");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DayOfWeek).IsRequired();

            // nullable vì có ngày OFF
            builder.Property(x => x.StartTime);

            builder.Property(x => x.EndTime);

            builder.Property(x => x.IsActive).HasDefaultValue(true);

            // 1 worker chỉ có 1 schedule cho mỗi thứ
            builder.HasIndex(x => new { x.WorkerProfileId, x.DayOfWeek }).IsUnique();

            builder
                .HasOne(x => x.WorkerProfile)
                .WithMany(x => x.WeeklySchedules)
                .HasForeignKey(x => x.WorkerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // optional nhưng nên có để query nhanh
            builder.HasIndex(x => x.WorkerProfileId);
        }
    }
}
