using Domain.Entities;
using Infrastructure.Repo;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.DataSeeds
{
    public static class AdminUserSeeder
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager)
        {
            const string adminEmail = "admin@system.com";
            const string adminPassword = "String@123";

            var existingUser = await userManager.FindByEmailAsync(adminEmail);
            if (existingUser != null)
                return;

            var adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = adminEmail,
                UserName = adminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create admin user: {errors}");
            }

            var addToRoleResult = await userManager.AddToRoleAsync(adminUser, SystemRoles.Administrator);

            if (!addToRoleResult.Succeeded)
            {
                var errors = string.Join("; ", addToRoleResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to assign admin role: {errors}");
            }
        }
    }
}