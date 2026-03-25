using Enrich.DAL.Data;
using Enrich.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Enrich.Web.Seeders
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            var roles = new[] { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "admin@enrich.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new User
                {
                    UserName = "SuperAdmin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                };

                var createPowerUser = await userManager.CreateAsync(newAdmin, "AdminPassword123!");
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            await SeedCategoriesAsync(scope.ServiceProvider);
        }

        private static async Task SeedCategoriesAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();

            var defaultCategories = new[]
            {
                "General", "Business", "Technology", "Science", "Medicine",
                "Law", "Politics", "Art", "Music", "Sports",
                "Travel", "Food", "Nature", "Education", "Finance"
            };

            foreach (var name in defaultCategories)
            {
                var exists = await db.Categories.AnyAsync(c => c.Name == name);
                if (!exists)
                {
                    db.Categories.Add(new Category { Name = name });
                }
            }

            await db.SaveChangesAsync();
        }
    }
}