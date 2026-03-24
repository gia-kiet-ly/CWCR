using Application.Contract.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<AdminDashboardDto> GetAdminDashboardAsync();
        Task<CitizenDashboardDto> GetCitizenDashboardAsync(Guid citizenUserId);
        Task<EnterpriseDashboardDto> GetEnterpriseDashboardAsync(Guid enterpriseUserId);
        Task<CollectorDashboardDto> GetCollectorDashboardAsync(Guid collectorUserId);
    }
}
