using Domain.Entities;
using Infrastructure.Repo;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataSeeds
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(RoleManager<ApplicationRole> roleManager)
        {
            string[] roles =
            {
            SystemRoles.Citizen,
            SystemRoles.RecyclingEnterprise,
            SystemRoles.Collector,
            SystemRoles.Administrator
        };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Id = Guid.NewGuid(),
                        Name = roleName,
                        NormalizedName = roleName.ToUpper(),
                        Description = $"System role: {roleName}"
                    });
                }
            }
        }
    }
}
