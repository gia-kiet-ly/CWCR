using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Application.Contract.Interfaces.Infrastructure;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class WardService : IWardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<WardDto> CreateAsync(CreateWardDto dto)
        {
            var repo = _unitOfWork.GetRepository<Ward>();

            var entity = new Ward
            {
                Name = dto.Name,
                Code = dto.Code,
                DistrictId = dto.DistrictId
            };

            await repo.InsertAsync(entity);
            await _unitOfWork.SaveAsync();

            return await GetByIdAsync(entity.Id)
                   ?? throw new Exception("Create failed");
        }

        public async Task<WardDto> UpdateAsync(Guid id, UpdateWardDto dto)
        {
            var repo = _unitOfWork.GetRepository<Ward>();

            var entity = await repo.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
                throw new Exception("Ward not found");

            entity.Name = dto.Name;
            entity.Code = dto.Code;
            entity.DistrictId = dto.DistrictId;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();

            return await GetByIdAsync(id)
                   ?? throw new Exception("Update failed");
        }

        public async Task<WardDto?> GetByIdAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<Ward>();

            var entity = await repo.NoTrackingEntities
                .Include(x => x.District)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            return entity == null ? null : MapToDto(entity);
        }

        public async Task<PagedWardDto> GetPagedAsync(WardFilterDto filter)
        {
            var repo = _unitOfWork.GetRepository<Ward>();

            var query = repo.NoTrackingEntities
                .Include(x => x.District)
                .Where(x => !x.IsDeleted);

            if (filter.DistrictId.HasValue)
                query = query.Where(x => x.DistrictId == filter.DistrictId);

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

            return new PagedWardDto
            {
                TotalCount = paged.TotalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = paged.Items.Select(MapToDto).ToList()
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<Ward>();

            var entity = await repo.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
                return false;

            entity.IsDeleted = true;
            entity.DeletedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        private static WardDto MapToDto(Ward x)
        {
            return new WardDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                DistrictId = x.DistrictId,
                DistrictName = x.District?.Name ?? "",
                CreatedTime = x.CreatedTime
            };
        }
        public async Task<List<WardDto>> GetByDistrictAsync(Guid districtId)
        {
            var repo = _unitOfWork.GetRepository<Ward>();

            var wards = await repo.NoTrackingEntities
                .Include(x => x.District)
                .Where(x => !x.IsDeleted && x.DistrictId == districtId)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return wards.Select(MapToDto).ToList();
        }
    }
}