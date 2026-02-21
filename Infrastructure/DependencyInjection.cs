using Application.Contract.Interfaces.Infrastructure;
using Infrastructure.DbContext;
using Infrastructure.Repo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
        {
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

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            return services;
        }
    }
}
