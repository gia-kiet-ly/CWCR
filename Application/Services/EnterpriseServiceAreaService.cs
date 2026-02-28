using Application.Contract.DTOs;
using Application.Contract.Interfaces;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Core.Utils;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class EnterpriseServiceAreaService
        : IEnterpriseServiceAreaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EnterpriseServiceAreaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ================= CREATE =================
        public async Task<EnterpriseServiceAreaDto> CreateAsync(
            CreateEnterpriseServiceAreaDto dto)
        {
            var areaRepo = _unitOfWork.GetRepository<EnterpriseServiceArea>();
            var enterpriseRepo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            // Check enterprise tồn tại
            var enterprise = await enterpriseRepo.NoTrackingEntities
                .FirstOrDefaultAsync(x => x.Id == dto.EnterpriseId && !x.IsDeleted);

            if (enterprise == null)
                throw new Exception("Enterprise not found");

            // Không cho duplicate RegionCode trong cùng Enterprise
            var existed = await areaRepo.NoTrackingEntities
                .AnyAsync(x =>
                    x.EnterpriseId == dto.EnterpriseId &&
                    x.RegionCode == dto.RegionCode &&
                    !x.IsDeleted);

            if (existed)
                throw new Exception("Service area already exists");

            var entity = new EnterpriseServiceArea
            {
                EnterpriseId = dto.EnterpriseId,
                RegionCode = dto.RegionCode
            };

            await areaRepo.InsertAsync(entity);
            await _unitOfWork.SaveAsync();

            return MapToDto(entity, enterprise.Name);
        }

        // ================= GET BY ID =================
        public async Task<EnterpriseServiceAreaDto?> GetByIdAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<EnterpriseServiceArea>();

            var entity = await repo.NoTrackingEntities
                .Include(x => x.Enterprise)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            return entity == null
                ? null
                : MapToDto(entity, entity.Enterprise?.Name);
        }

        // ================= GET ALL =================
        public async Task<PagedEnterpriseServiceAreaDto> GetAllAsync(
            EnterpriseServiceAreaFilterDto filter)
        {
            var repo = _unitOfWork.GetRepository<EnterpriseServiceArea>();

            var query = repo.NoTrackingEntities
                .Include(x => x.Enterprise)
                .Where(x => !x.IsDeleted);

            if (filter.EnterpriseId.HasValue)
                query = query.Where(x => x.EnterpriseId == filter.EnterpriseId);

            if (!string.IsNullOrWhiteSpace(filter.RegionCode))
                query = query.Where(x => x.RegionCode.Contains(filter.RegionCode));

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
                    .Select(x => MapToDto(
                        x,
                        x.Enterprise?.Name))
                    .ToList()
            };
        }

        // ================= UPDATE =================
        public async Task<bool> UpdateAsync(
            Guid id,
            UpdateEnterpriseServiceAreaDto dto)
        {
            var repo = _unitOfWork.GetRepository<EnterpriseServiceArea>();

            var entity = await repo.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
                return false;

            // Check duplicate RegionCode trong cùng Enterprise
            var existed = await repo.NoTrackingEntities
                .AnyAsync(x =>
                    x.Id != id &&
                    x.EnterpriseId == entity.EnterpriseId &&
                    x.RegionCode == dto.RegionCode &&
                    !x.IsDeleted);

            if (existed)
                throw new Exception("Service area already exists");

            entity.RegionCode = dto.RegionCode;
            entity.LastUpdatedTime = CoreHelper.SystemTimeNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // ================= DELETE (SOFT) =================
        public async Task<bool> DeleteAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<EnterpriseServiceArea>();

            var entity = await repo.GetByIdAsync(id);

            if (entity == null)
                return false;

            entity.IsDeleted = true;
            entity.DeletedTime = CoreHelper.SystemTimeNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // ================= MAPPER =================
        private static EnterpriseServiceAreaDto MapToDto(
            EnterpriseServiceArea entity,
            string? enterpriseName)
        {
            return new EnterpriseServiceAreaDto
            {
                Id = entity.Id,
                EnterpriseId = entity.EnterpriseId,
                EnterpriseName = enterpriseName ?? string.Empty,
                RegionCode = entity.RegionCode,
                CreatedTime = entity.CreatedTime
            };
        }
    }
}