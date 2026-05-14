using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeds
{
    public static class UserSeeder
    {
        public static readonly Guid AdminUserId = Guid.Parse(
            "f1a7d8c1-3e21-4a8c-9b11-2d7f4c5e1000"
        );

        public static void Seed(ModelBuilder builder)
        {
            // Seed Admin User
            builder.Entity<User>().HasData(new User
            {
                Id = AdminUserId,
                FullName = "System Admin",
                Email = "admin@fixy.com",
                IsEmailVerified = true,
                Phone = "0000000000",
                IsPhoneVerified = true,
                // Password: Admin@123
                PasswordHash = "$2a$11$2Sn0EJfhBql3sJvuD/UDeODXtuVKzfkeAO8EWTplBPNMFi/P1Gz/.",
                IsActive = true,
                CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            });

            // Assign Admin Role to Admin User
            builder.Entity<UserRole>().HasData(new UserRole
            {
                UserId = AdminUserId,
                RoleId = RoleSeeder.AdminRoleId,
                AssignedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}
