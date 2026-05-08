using Domain.Entity.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Identity
{
    public class OtpVerificationConfiguration : IEntityTypeConfiguration<OtpVerification>
    {
        public void Configure(EntityTypeBuilder<OtpVerification> builder)
        {
            builder.ToTable("OtpVerifications");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Target).HasMaxLength(255).IsRequired();

            builder.Property(x => x.OtpCode).HasMaxLength(10).IsRequired();

            builder.Property(x => x.IsUsed).HasDefaultValue(false);

            builder.Property(x => x.IsVerified).HasDefaultValue(false);

            builder.HasIndex(x => x.Target);
        }
    }
}
