using Application.Contract.DTOs;

namespace Application.Contract.Interfaces.Services
{
    public interface IWardService
    {
        Task<WardDto> CreateAsync(CreateWardDto dto);

        Task<WardDto> UpdateAsync(Guid id, UpdateWardDto dto);

        Task<WardDto?> GetByIdAsync(Guid id);

        Task<PagedWardDto> GetPagedAsync(WardFilterDto filter);

        Task<bool> DeleteAsync(Guid id);
        Task<List<WardDto>> GetByDistrictAsync(Guid districtId);
    }
}