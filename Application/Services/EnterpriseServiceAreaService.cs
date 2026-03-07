using Application.Contract.DTOs;
using Application.Contract.Interfaces;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Core.Utils;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class EnterpriseServiceAreaService : IEnterpriseServiceAreaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EnterpriseServiceAreaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ================= CREATE =================
        public async Task<EnterpriseServiceAreaDto> CreateAsync(
            Guid userId,
            CreateEnterpriseServiceAreaDto dto)
        {
            var areaRepo = _unitOfWork.GetRepository<EnterpriseServiceArea>();
            var enterpriseRepo = _unitOfWork.GetRepository<RecyclingEnterprise>();
            var districtRepo = _unitOfWork.GetRepository<District>();
            var wardRepo = _unitOfWork.GetRepository<Ward>();

            var enterprise = await enterpriseRepo.NoTrackingEntities
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    !x.IsDeleted);

            if (enterprise == null)
                throw new Exception("Enterprise not found");

            var district = await districtRepo.NoTrackingEntities
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.DistrictId &&
                    !x.IsDeleted);

            if (district == null)
                throw new Exception("District not found");

            var ward = await wardRepo.NoTrackingEntities
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.WardId &&
                    !x.IsDeleted);

            if (ward == null)
                throw new Exception("Ward not found");

            // check ward belongs to district
            if (ward.DistrictId != dto.DistrictId)
                throw new Exception("Ward does not belong to the selected district");

            // check duplicate
            var existed = await areaRepo.NoTrackingEntities
                .AnyAsync(x =>
                    x.EnterpriseId == enterprise.Id &&
                    x.DistrictId == dto.DistrictId &&
                    x.WardId == dto.WardId &&
                    !x.IsDeleted);

            if (existed)
                throw new Exception("Service area already exists");

            var entity = new EnterpriseServiceArea
            {
                EnterpriseId = enterprise.Id,
                DistrictId = dto.DistrictId,
                WardId = dto.WardId,
                CreatedTime = CoreHelper.SystemTimeNow
            };

            await areaRepo.InsertAsync(entity);
            await _unitOfWork.SaveAsync();

            return await GetByIdAsync(entity.Id)
                   ?? throw new Exception("Create failed");
        }

        // ================= GET BY ID =================
        public async Task<EnterpriseServiceAreaDto?> GetByIdAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<EnterpriseServiceArea>();

            var entity = await repo.NoTrackingEntities
                .Include(x => x.Enterprise)
                .Include(x => x.District)
                .Include(x => x.Ward)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

            return entity == null
                ? null
                : MapToDto(entity);
        }

        // ================= GET ALL =================
        public async Task<PagedEnterpriseServiceAreaDto> GetAllAsync(
            EnterpriseServiceAreaFilterDto filter)
        {
            var repo = _unitOfWork.GetRepository<EnterpriseServiceArea>();

            var query = repo.NoTrackingEntities
                .Include(x => x.Enterprise)
                .Include(x => x.District)
                .Include(x => x.Ward)
                .Where(x => !x.IsDeleted);

            if (filter.EnterpriseId.HasValue)
                query = query.Where(x =>
                    x.EnterpriseId == filter.EnterpriseId);

            if (filter.DistrictId.HasValue)
                query = query.Where(x =>
                    x.DistrictId == filter.DistrictId);

            if (filter.WardId.HasValue)
                query = query.Where(x =>
                    x.WardId == filter.WardId);

            var pagedData = await repo.GetPagging(
                query.OrderByDescending(x => x.CreatedTime),
                filter.PageNumber,
                filter.PageSize);

            return new PagedEnterpriseServiceAreaDto
            {
                TotalCount = pagedData.TotalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = pagedData.Items
                    .Select(MapToDto)
                    .ToList()
            };
        }

        // ================= UPDATE =================
        public async Task<bool> UpdateAsync(
            Guid id,
            UpdateEnterpriseServiceAreaDto dto)
        {
            var repo = _unitOfWork.GetRepository<EnterpriseServiceArea>();
            var wardRepo = _unitOfWork.GetRepository<Ward>();

            var entity = await repo.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
                return false;

            var ward = await wardRepo.NoTrackingEntities
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.WardId &&
                    !x.IsDeleted);

            if (ward == null)
                throw new Exception("Ward not found");

            if (ward.DistrictId != dto.DistrictId)
                throw new Exception("Ward does not belong to the selected district");

            var existed = await repo.NoTrackingEntities
                .AnyAsync(x =>
                    x.Id != id &&
                    x.EnterpriseId == entity.EnterpriseId &&
                    x.DistrictId == dto.DistrictId &&
                    x.WardId == dto.WardId &&
                    !x.IsDeleted);

            if (existed)
                throw new Exception("Service area already exists");

            entity.DistrictId = dto.DistrictId;
            entity.WardId = dto.WardId;
            entity.LastUpdatedTime = CoreHelper.SystemTimeNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // ================= DELETE =================
        public async Task<bool> DeleteAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<EnterpriseServiceArea>();

            var entity = await repo.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
                return false;

            entity.IsDeleted = true;
            entity.DeletedTime = CoreHelper.SystemTimeNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // ================= MAPPER =================
        private static EnterpriseServiceAreaDto MapToDto(
            EnterpriseServiceArea entity)
        {
            return new EnterpriseServiceAreaDto
            {
                Id = entity.Id,

                EnterpriseId = entity.EnterpriseId,
                EnterpriseName = entity.Enterprise?.Name ?? string.Empty,

                DistrictId = entity.DistrictId,
                DistrictName = entity.District?.Name ?? string.Empty,
                DistrictCode = entity.District?.Code ?? string.Empty,

                WardId = entity.WardId,
                WardName = entity.Ward?.Name ?? string.Empty,
                WardCode = entity.Ward?.Code ?? string.Empty,

                CreatedTime = entity.CreatedTime
            };
        }
    }
}