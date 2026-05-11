using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TokenHash).IsRequired();
            builder.HasIndex(x => x.TokenHash).IsUnique().HasDatabaseName("idx_rt_hash");
            builder
                .HasIndex(x => new { x.UserId, x.IsRevoked })
                .HasDatabaseName("idx_rt_user_active");
            builder.HasIndex(x => x.ExpiresAt).HasDatabaseName("idx_rt_expires");

            builder
                .HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.ReplacedBy)
                .WithMany()
                .HasForeignKey(x => x.ReplacedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
