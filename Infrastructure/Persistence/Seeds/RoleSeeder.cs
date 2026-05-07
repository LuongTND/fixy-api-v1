using Domain.Entity.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeds
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Roles.AnyAsync())
            {
                return;
            }

            var roles = new List<Role>
            {
                new() { Name = "Admin", Code = "ADMIN" },
                new() { Name = "Worker", Code = "WORKER" },
                new() { Name = "Customer", Code = "CUSTOMER" },
            };

            await context.Roles.AddRangeAsync(roles);

            await context.SaveChangesAsync();
        }
    }
}
