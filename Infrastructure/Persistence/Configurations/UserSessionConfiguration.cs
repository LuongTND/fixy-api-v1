using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
    {
        public void Configure(EntityTypeBuilder<UserSession> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.IpAddress).IsRequired();
            builder.Property(x => x.DeviceType).HasConversion<string>();
            builder.Property(x => x.TerminatedBy).HasConversion<string>();
            builder.HasIndex(x => new { x.UserId, x.IsActive }).HasDatabaseName("idx_session_user_active");
            builder.HasIndex(x => x.DeviceId).HasDatabaseName("idx_session_device");
            builder.HasIndex(x => x.LastActiveAt).HasDatabaseName("idx_session_last_active");

            builder.HasOne(x => x.User)
                .WithMany(x => x.UserSessions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
