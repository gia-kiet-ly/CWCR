using Application.Contract.DTOs;

namespace Application.Contract.Interfaces.Services
{
    public interface IEnterpriseWasteCapabilityService
    {
        // ================= CREATE =================
        Task<EnterpriseWasteCapabilityDto> CreateAsync(
            CreateEnterpriseWasteCapabilityDto dto);

        // ================= GET BY ID =================
        Task<EnterpriseWasteCapabilityDto?> GetByIdAsync(Guid id);

        // ================= GET ALL (FILTER + PAGING) =================
        Task<PagedEnterpriseWasteCapabilityDto> GetAllAsync(
            EnterpriseWasteCapabilityFilterDto filter);

        // ================= UPDATE =================
        Task<bool> UpdateAsync(
            Guid id,
            UpdateEnterpriseWasteCapabilityDto dto);

        // ================= DELETE (SOFT) =================
        Task<bool> DeleteAsync(Guid id);
    }
}