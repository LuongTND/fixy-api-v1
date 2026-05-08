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

            builder.Property(x => x.Target).HasMaxLength(255).IsRequired();

            builder.Property(x => x.OtpCode).HasMaxLength(10).IsRequired();

            builder.Property(x => x.IsUsed).HasDefaultValue(false);

            builder.Property(x => x.IsVerified).HasDefaultValue(false);

            builder.HasIndex(x => x.Target);
        }
    }
}
