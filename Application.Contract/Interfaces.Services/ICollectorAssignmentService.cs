using Application.Contract.DTOs;
using Application.Contract.Paggings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.Interfaces.Services
{
    public interface ICollectorAssignmentService
    {
        // Enterprise
        Task<CollectorAssignmentDto> CreateAsync(Guid enterpriseId, CreateAssignmentDto dto);
        Task<CollectorAssignmentDto?> GetByIdEnterpriseAsync(Guid enterpriseId, Guid id);
        Task<PaginatedList<CollectorAssignmentDto>> GetPagedEnterpriseAsync(Guid enterpriseId, AssignmentFilterDto filter);

        // Collector
        Task<CollectorAssignmentDto?> GetByIdCollectorAsync(Guid collectorUserId, Guid id);
        Task<PaginatedList<CollectorAssignmentDto>> GetPagedCollectorAsync(Guid collectorUserId, AssignmentFilterDto filter);
        Task<bool> UpdateStatusCollectorAsync(Guid collectorUserId, Guid id, UpdateAssignmentStatusDto dto);
    }
}
