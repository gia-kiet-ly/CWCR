using Application.Contract.DTOs;
using Application.Contract.Paggings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.Interfaces.Services
{
    public interface ICollectionProofService
    {
        // Collector
        Task<CollectionProofDto> CreateAsync(Guid collectorUserId, CreateCollectionProofDto dto);
        Task<CollectionProofDto?> GetByIdCollectorAsync(Guid collectorUserId, Guid id);
        Task<PaginatedList<CollectionProofDto>> GetPagedCollectorAsync(Guid collectorUserId, CollectionProofFilterDto filter);

        // Enterprise
        Task<CollectionProofDto?> GetByIdEnterpriseAsync(Guid enterpriseId, Guid id);
        Task<PaginatedList<CollectionProofDto>> GetPagedEnterpriseAsync(Guid enterpriseId, CollectionProofFilterDto filter);
        Task<bool> ReviewAsync(Guid enterpriseId, Guid reviewerUserId, Guid id, ReviewCollectionProofDto dto);
    }
}
