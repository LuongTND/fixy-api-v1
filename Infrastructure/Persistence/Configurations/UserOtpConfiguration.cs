using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserOtpConfiguration : IEntityTypeConfiguration<UserOtp>
    {
        public void Configure(EntityTypeBuilder<UserOtp> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.OtpHash).IsRequired();
            builder.Property(x => x.Type).HasConversion<string>();
            builder.Property(x => x.ExpiresAt).IsRequired();
            builder.HasIndex(x => new { x.UserId, x.Type, x.IsUsed }).HasDatabaseName("idx_otp_lookup");
            builder.HasIndex(x => x.ExpiresAt).HasDatabaseName("idx_otp_expires");

            builder.HasOne(x => x.User)
                .WithMany(x => x.UserOtps)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
