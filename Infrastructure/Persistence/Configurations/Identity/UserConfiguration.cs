using Domain.Entity.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Identity
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();

            builder.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();

            builder.HasIndex(x => x.PhoneNumber).IsUnique();

            builder.Property(x => x.Email).HasMaxLength(255);

            builder.HasIndex(x => x.Email).IsUnique();

            builder.Property(x => x.PasswordHash).IsRequired();
            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
