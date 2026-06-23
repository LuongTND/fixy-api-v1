using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserFcmTokenConfiguration : IEntityTypeConfiguration<UserFcmToken>
    {
        public void Configure(EntityTypeBuilder<UserFcmToken> builder)
        {
            builder.ToTable("UserFcmTokens");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.DeviceType)
                .HasMaxLength(50);

            builder.Property(x => x.Browser)
                .HasMaxLength(50);

            // Exception keys are linked to table users.
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes for quick queries based on User ID.
            builder.HasIndex(x => x.UserId);

            // A unique index is used on the token to avoid duplication within the system.
            builder.HasIndex(x => x.Token).IsUnique();
        }
    }
}
