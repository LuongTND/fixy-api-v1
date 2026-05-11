using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeds
{
    public static class RoleSeeder
    {
        public static readonly Guid AdminRoleId = Guid.Parse(
            "a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"
        );

        public static readonly Guid CustomerRoleId = Guid.Parse(
            "b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"
        );

        public static readonly Guid StaffRoleId = Guid.Parse(
            "c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"
        );

        public static void Seed(ModelBuilder builder)
        {
            builder
                .Entity<Role>()
                .HasData(
                    new Role
                    {
                        Id = AdminRoleId,
                        Name = "ADMIN",
                        CreatedDate = DateTime.UtcNow,
                    },
                    new Role
                    {
                        Id = CustomerRoleId,
                        Name = "CUSTOMER",
                        CreatedDate = DateTime.UtcNow,
                    },
                    new Role
                    {
                        Id = StaffRoleId,
                        Name = "STAFF",
                        CreatedDate = DateTime.UtcNow,
                    }
                );
        }
    }
}
