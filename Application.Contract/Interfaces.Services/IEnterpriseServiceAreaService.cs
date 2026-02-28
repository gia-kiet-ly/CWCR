using Application.Contract.DTOs;

namespace Application.Contract.Interfaces.Services
{
    public interface IEnterpriseServiceAreaService
    {
        // ================= CREATE =================
        Task<EnterpriseServiceAreaDto> CreateAsync(
            CreateEnterpriseServiceAreaDto dto);

        // ================= GET BY ID =================
        Task<EnterpriseServiceAreaDto?> GetByIdAsync(Guid id);

        // ================= GET ALL (FILTER + PAGING) =================
        Task<PagedEnterpriseServiceAreaDto> GetAllAsync(
            EnterpriseServiceAreaFilterDto filter);

        // ================= UPDATE =================
        Task<bool> UpdateAsync(
            Guid id,
            UpdateEnterpriseServiceAreaDto dto);

        // ================= DELETE (SOFT) =================
        Task<bool> DeleteAsync(Guid id);
    }
}