using Application.Contract.Interfaces;
using Application.Contract.Interfaces.Services;
using Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationLayer(
    this IServiceCollection services,
    IConfiguration configuration)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IWasteReportService, WasteReportService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IWasteTypeService, WasteTypeService>();
            services.AddScoped<IWasteImageService,WasteImageService>();
            return services;
        }
    }
}
