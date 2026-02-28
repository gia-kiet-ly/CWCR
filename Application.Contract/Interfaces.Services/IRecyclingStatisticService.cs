using Application.Contract.DTOs;

namespace Application.Contract.Interfaces.Services
{
    public interface IRecyclingStatisticService
    {
        // ================= CREATE =================
        Task<RecyclingStatisticDto> CreateAsync(
            CreateRecyclingStatisticDto dto);

        // ================= GET BY ID =================
        Task<RecyclingStatisticDto?> GetByIdAsync(Guid id);

        // ================= GET ALL (FILTER + PAGING) =================
        Task<PagedRecyclingStatisticDto> GetAllAsync(
            RecyclingStatisticFilterDto filter);

        // ================= UPDATE =================
        Task<bool> UpdateAsync(
            Guid id,
            UpdateRecyclingStatisticDto dto);

        // ================= DELETE (SOFT) =================
        Task<bool> DeleteAsync(Guid id);
    }
}