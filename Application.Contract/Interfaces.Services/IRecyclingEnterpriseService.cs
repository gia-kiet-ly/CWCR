using Application.Contract.DTOs;

namespace Application.Contract.Interfaces.Services
{
    public interface IRecyclingEnterpriseService
    {
        // ================================
        // CREATE
        // ================================
        Task<RecyclingEnterpriseDto> CreateAsync(
            Guid userId,
            CreateRecyclingEnterpriseDto dto);

        // ================================
        // READ
        // ================================
        Task<RecyclingEnterpriseDto?> GetByIdAsync(Guid id);

        Task<PagedRecyclingEnterpriseDto> GetAllAsync(
            RecyclingEnterpriseFilterDto filter);

        // ================================
        // UPDATE
        // ================================
        Task<bool> UpdateAsync(
            Guid id,
            UpdateRecyclingEnterpriseDto dto);

        Task<bool> UpdateStatusAsync(
            Guid id,
            UpdateEnterpriseStatusDto dto);

        // ================================
        // DELETE
        // ================================
        Task<bool> DeleteAsync(Guid id);
    }
}