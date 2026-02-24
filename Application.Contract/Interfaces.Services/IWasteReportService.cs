using Application.Contract.DTOs;

namespace Application.Contract.Interfaces.Services
{
    public interface IWasteReportService
    {
        Task<Guid> CreateAsync(CreateWasteReportDto dto);
        Task UpdateAsync(Guid id, UpdateWasteReportDto dto);
        Task DeleteAsync(Guid id);

        Task<WasteReportResponseDto?> GetByIdAsync(Guid id);

        Task<PagedWasteReportDto> GetPagedAsync(WasteReportFilterDto filter);
    }
}