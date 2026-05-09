using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired();
            builder.HasIndex(x => x.Name).IsUnique();

            var seedTime = new DateTime(2026, 5, 8, 0, 0, 0, DateTimeKind.Utc);

            builder.HasData(
                new Role
                {
                    Id = new Guid("b1f5f9b2-2e2c-4ed8-8c1b-6b4d2e8df3a1"),
                    Name = "Customer",
                    Description = "Customer role",
                    IsActive = true,
                    CreatedDate = seedTime
                },
                new Role
                {
                    Id = new Guid("6bb3f0d4-0c2e-4a2e-9f5e-30f6f5f6a0b2"),
                    Name = "Worker",
                    Description = "Worker role",
                    IsActive = true,
                    CreatedDate = seedTime
                },
                new Role
                {
                    Id = new Guid("a0a2e2f1-3b6a-4f39-9f9d-1d2a1d2c3b4c"),
                    Name = "Admin",
                    Description = "Administrator role",
                    IsActive = true,
                    CreatedDate = seedTime
                });
        }
    }
}
