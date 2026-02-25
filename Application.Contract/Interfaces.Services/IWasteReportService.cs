using Application.Contract.DTOs;

namespace Application.Contract.Interfaces
{
    public interface IWasteReportService
    {
        // =============================
        // CREATE
        // =============================
        Task<WasteReportResponseDto> CreateAsync(
            CreateWasteReportDto dto,
            Guid citizenId);

        // =============================
        // UPDATE
        // =============================
        Task<WasteReportResponseDto> UpdateAsync(
            Guid reportId,
            UpdateWasteReportDto dto);

        // =============================
        // GET BY ID
        // =============================
        Task<WasteReportResponseDto?> GetByIdAsync(
            Guid reportId);

        // =============================
        // FILTER + PAGING
        // =============================
        Task<PagedWasteReportDto> GetPagedAsync(
            WasteReportFilterDto filter);

        // =============================
        // DELETE
        // =============================
        Task<bool> DeleteAsync(
            Guid reportId);
    }
}