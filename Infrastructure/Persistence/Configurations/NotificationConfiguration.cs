using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type).HasConversion<string>();
            builder.Property(x => x.Title).IsRequired();
            builder.Property(x => x.Body).IsRequired();
            builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedDate }).HasDatabaseName("idx_notif_unread");
            builder.HasIndex(x => new { x.UserId, x.Type, x.CreatedDate }).HasDatabaseName("idx_notif_type");

            builder.HasOne(x => x.User)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
