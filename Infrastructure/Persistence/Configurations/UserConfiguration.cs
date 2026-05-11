using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PasswordHash).IsRequired();
            builder.Property(x => x.OAuthProvider).HasConversion<string>();
            builder.HasIndex(x => x.Phone).HasDatabaseName("idx_users_phone");
            builder.HasIndex(x => x.Email).HasDatabaseName("idx_users_email");
            builder.HasIndex(x => x.Phone).IsUnique();
            builder.HasIndex(x => x.Email).IsUnique();
        }
    }
}
