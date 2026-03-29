using Domain.Entities;
using Infrastructure.Repo;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.DataSeeds
{
    public static class AdminUserSeeder
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
        {
            // ================= ADMIN =================
            await EnsureUserAsync(userManager,
                email: "admin@system.com",
                fullName: "System Administrator",
                password: "String@123",
                role: SystemRoles.Administrator);

            // ================= ENTERPRISE =================
            await EnsureUserAsync(userManager,
                email: "enterprise@system.com",
                fullName: "Default Enterprise",
                password: "String@123",
                role: SystemRoles.RecyclingEnterprise);

            await EnsureUserAsync(userManager,
                email: "enterprise2@system.com",
                fullName: "Default Enterprise 2",
                password: "String@123",
                role: SystemRoles.RecyclingEnterprise);

            // ================= CITIZEN =================
            await EnsureUserAsync(userManager,
                email: "citizen@system.com",
                fullName: "Default Citizen",
                password: "String@123",
                role: SystemRoles.Citizen);
        }

        private static async Task EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string fullName,
            string password,
            string role)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null)
                return;

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = email,
                FullName = fullName,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create user {email}: {errors}");
            }

            var addToRoleResult = await userManager.AddToRoleAsync(user, role);
            if (!addToRoleResult.Succeeded)
            {
                var errors = string.Join("; ", addToRoleResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to assign role {role} to {email}: {errors}");
            }
        }
    }
}