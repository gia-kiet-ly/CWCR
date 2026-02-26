using Application.Contract.DTOs;

namespace Application.Contract.Interfaces
{
    public interface IWasteImageService
    {
        // ================= UPLOAD =================
        Task<WasteImageResponseDto> CreateAsync(CreateWasteImageDto dto);

        // ================= GET BY ID =================
        Task<WasteImageResponseDto?> GetByIdAsync(Guid id);

        // ================= PAGING / FILTER =================
        Task<PagedWasteImageDto> GetPagedAsync(WasteImageFilterDto filter);

        // ================= DELETE (Cloud + DB) =================
        Task<bool> DeleteAsync(Guid id);
    }
}