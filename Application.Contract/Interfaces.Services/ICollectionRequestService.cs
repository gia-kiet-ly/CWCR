using Application.Contract.DTOs;

namespace Application.Contract.Interfaces.Services
{
    public interface ICollectionRequestService
    {
        // =============================
        // SYSTEM: DISPATCH (MATCHING TOP1)
        // =============================
        Task CreateTop1RequestsForReportAsync(Guid wasteReportId);

        // =============================
        // ENTERPRISE: INBOX
        // =============================
        Task<PagedCollectionRequestDto> GetPagedForEnterpriseAsync(Guid enterpriseId, CollectionRequestFilterDto filter);

        // =============================
        // ENTERPRISE: ACTIONS
        // =============================
        Task<bool> AcceptAsync(Guid enterpriseId, Guid requestId);
        Task<bool> RejectAsync(Guid enterpriseId, Guid requestId, string? reason);
    }
}