using Application.Contract.DTOs;

namespace Application.Contract.Interfaces
{
    public interface IWasteReportService
    {
        // =============================
        // CREATE REPORT
        // =============================
        Task<WasteReportResponseDto> CreateAsync(
            CreateWasteReportDto dto,
            Guid citizenId);

        // =============================
        // UPDATE REPORT (Citizen edit after reject)
        // =============================
        Task<WasteReportResponseDto> UpdateAsync(
            Guid reportId,
            UpdateWasteReportDto dto);

        // =============================
        // GET REPORT BY ID
        // =============================
        Task<WasteReportResponseDto?> GetByIdAsync(
            Guid reportId);

        // =============================
        // FILTER + PAGINATION
        // =============================
        Task<PagedWasteReportDto> GetPagedAsync(
            WasteReportFilterDto filter);

        // =============================
        // SOFT DELETE
        // =============================
        Task<bool> DeleteAsync(
            Guid reportId);

        // =============================
        // REDISPATCH (Citizen manually retry)
        // =============================
        Task RedispatchAsync(
            Guid reportId);

        // =============================
        // GET REJECT HISTORY
        // =============================
        Task<List<RejectHistoryDto>> GetRejectHistoryAsync(
            Guid reportId,
            Guid citizenId);

        // =============================
        // GET COLLECTION PROOF
        // =============================
        Task<CitizenCollectionProofDto?> GetProofForCitizenAsync(
            Guid reportId,
            Guid citizenId);
    }
}