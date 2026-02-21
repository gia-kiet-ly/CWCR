using Domain.Entities;
using Infrastructure.DbContext;
using Microsoft.AspNetCore.Identity;
using Application;
using Infrastructure;

namespace API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddConfig(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddApplicationLayer(configuration)
                .AddInfrastructureLayer(configuration);

            // 👉 Identity đặt ở API giống bài cũ
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            return services;
        }
    }
}
