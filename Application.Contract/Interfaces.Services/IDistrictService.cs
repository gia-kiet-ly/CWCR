using Application.Contract.DTOs;

namespace Application.Contract.Interfaces.Services
{
    public interface IDistrictService
    {
        Task<DistrictDto> CreateAsync(CreateDistrictDto dto);

        Task<DistrictDto> UpdateAsync(Guid id, UpdateDistrictDto dto);

        Task<DistrictDto?> GetByIdAsync(Guid id);

        Task<PagedDistrictDto> GetPagedAsync(DistrictFilterDto filter);

        Task<bool> DeleteAsync(Guid id);
        Task<List<DistrictDto>> GetByProvinceAsync(string provinceCode);
    }
}