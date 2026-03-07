using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Application.Contract.Interfaces.Infrastructure;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class DistrictService : IDistrictService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DistrictService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DistrictDto> CreateAsync(CreateDistrictDto dto)
        {
            var repo = _unitOfWork.GetRepository<District>();

            var entity = new District
            {
                Name = dto.Name,
                Code = dto.Code,
                ProvinceCode = dto.ProvinceCode
            };

            await repo.InsertAsync(entity);
            await _unitOfWork.SaveAsync();

            return MapToDto(entity);
        }

        public async Task<DistrictDto> UpdateAsync(Guid id, UpdateDistrictDto dto)
        {
            var repo = _unitOfWork.GetRepository<District>();

            var entity = await repo.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
                throw new Exception("District not found");

            entity.Name = dto.Name;
            entity.Code = dto.Code;
            entity.ProvinceCode = dto.ProvinceCode;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();

            return MapToDto(entity);
        }

        public async Task<DistrictDto?> GetByIdAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<District>();

            var entity = await repo.NoTrackingEntities
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            return entity == null ? null : MapToDto(entity);
        }

        public async Task<PagedDistrictDto> GetPagedAsync(DistrictFilterDto filter)
        {
            var repo = _unitOfWork.GetRepository<District>();

            var query = repo.NoTrackingEntities
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.ProvinceCode))
                query = query.Where(x => x.ProvinceCode == filter.ProvinceCode);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(keyword));
            }

            var paged = await repo.GetPagging(
                query.OrderBy(x => x.Name),
                filter.PageNumber,
                filter.PageSize);

            return new PagedDistrictDto
            {
                TotalCount = paged.TotalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = paged.Items.Select(MapToDto).ToList()
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<District>();

            var entity = await repo.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
                return false;

            entity.IsDeleted = true;
            entity.DeletedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        private static DistrictDto MapToDto(District x)
        {
            return new DistrictDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                ProvinceCode = x.ProvinceCode,
                CreatedTime = x.CreatedTime
            };
        }
        public async Task<List<DistrictDto>> GetByProvinceAsync(string provinceCode)
        {
            var repo = _unitOfWork.GetRepository<District>();

            var districts = await repo.NoTrackingEntities
                .Where(x => !x.IsDeleted && x.ProvinceCode == provinceCode)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return districts.Select(MapToDto).ToList();
        }
    }

}