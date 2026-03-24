using Application.Contract.DTOs;
using Application.Contract.Paggings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.Interfaces.Services
{
    public interface ICollectorProfileService
    {
        Task<CollectorProfileDto> CreateAsync(Guid enterpriseId, CreateCollectorProfileDto dto);

        Task<CollectorProfileDto?> GetByIdAsync(Guid enterpriseId, Guid id);

        Task<IEnumerable<CollectorProfileDto>> GetAllAsync(Guid enterpriseId);

        Task<PaginatedList<CollectorProfileDto>> GetPagedAsync(
            Guid enterpriseId,
            CollectorProfileFilterDto filter);

        Task<bool> UpdateAsync(Guid enterpriseId, Guid id, UpdateCollectorProfileDto dto);

        Task<bool> DeleteAsync(Guid enterpriseId, Guid id);
        Task<CollectorProfileDto> GetMyCollectorProfileAsync(Guid userId);
    }
}
