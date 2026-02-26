using Application.Contract.Interfaces.Infrastructure;
using CloudinaryDotNet;
using Infrastructure.DbContext;
using Infrastructure.Repo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureLayer(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // =============================
            // Database
            // =============================
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                if (!connectionString!.Contains("Max Pool Size", StringComparison.OrdinalIgnoreCase))
                {
                    var separator = connectionString.EndsWith(";") ? "" : ";";
                    connectionString = $"{connectionString}{separator}Max Pool Size=250;Min Pool Size=10;";
                }

                options.UseSqlServer(connectionString);
            });

            // =============================
            // Repositories & Infrastructure Services
            // =============================
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            // =============================
            // Cloudinary (FIX lỗi DI ImageService)
            // =============================
            services.AddSingleton(sp =>
            {
                var cloudSection = configuration.GetSection("Cloudinary");

                var account = new Account(
                    cloudSection["CloudName"],
                    cloudSection["ApiKey"],
                    cloudSection["ApiSecret"]
                );

                return new Cloudinary(account);
            });

            return services;
        }
    }
}