using Application.Contract.Interfaces;
using Application.Contract.Interfaces.Services;
using Application.Contract.Services;
using Application.Services;
using Application.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationLayer(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
            services.AddScoped<IEnterpriseApprovalService, EnterpriseApprovalService>();

            services.AddScoped<IWasteReportService, WasteReportService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IWasteTypeService, WasteTypeService>();
            services.AddScoped<IWasteImageService, WasteImageService>();
            services.AddScoped<IRecyclingEnterpriseService, RecyclingEnterpriseService>();
            services.AddScoped<IEnterpriseWasteCapabilityService, EnterpriseWasteCapabilityService>();
            services.AddScoped<IEnterpriseServiceAreaService, EnterpriseServiceAreaService>();
            services.AddScoped<IRecyclingStatisticService, RecyclingStatisticService>();
            services.AddScoped<ICollectorProfileService, CollectorProfileService>();
            services.AddScoped<ICollectorAssignmentService, CollectorAssignmentService>();
            services.AddScoped<ICollectionProofService, CollectionProofService>();
            services.AddScoped<IComplaintService, ComplaintService>();
            services.AddScoped<IDisputeResolutionService, DisputeResolutionService>();
            services.AddScoped<ICollectionRequestService, CollectionRequestService>();
            services.AddScoped<ICitizenPointService, CitizenPointService>();

            return services;
        }
    }
}