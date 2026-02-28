using Application.Contract.DTOs;
using Application.Contract.Interfaces;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Core.Utils;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class RecyclingStatisticService : IRecyclingStatisticService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RecyclingStatisticService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ================= CREATE =================
        public async Task<RecyclingStatisticDto> CreateAsync(
            CreateRecyclingStatisticDto dto)
        {
            var repo = _unitOfWork.GetRepository<RecyclingStatistic>();
            var enterpriseRepo = _unitOfWork.GetRepository<RecyclingEnterprise>();
            var wasteTypeRepo = _unitOfWork.GetRepository<WasteType>();

            // Check Enterprise
            var enterprise = await enterpriseRepo.NoTrackingEntities
                .FirstOrDefaultAsync(x => x.Id == dto.EnterpriseId && !x.IsDeleted);

            if (enterprise == null)
                throw new Exception("Enterprise not found");

            // Check WasteType
            var wasteType = await wasteTypeRepo.NoTrackingEntities
                .FirstOrDefaultAsync(x => x.Id == dto.WasteTypeId && !x.IsDeleted);

            if (wasteType == null)
                throw new Exception("WasteType not found");

            // Check duplicate theo kỳ
            var existed = await repo.NoTrackingEntities
                .AnyAsync(x =>
                    x.EnterpriseId == dto.EnterpriseId &&
                    x.WasteTypeId == dto.WasteTypeId &&
                    x.RegionCode == dto.RegionCode &&
                    x.Period == dto.Period &&
                    !x.IsDeleted);

            if (existed)
                throw new Exception("Statistic for this period already exists");

            var entity = new RecyclingStatistic
            {
                EnterpriseId = dto.EnterpriseId,
                WasteTypeId = dto.WasteTypeId,
                TotalWeightKg = dto.TotalWeightKg,
                RegionCode = dto.RegionCode,
                Period = dto.Period
            };

            await repo.InsertAsync(entity);
            await _unitOfWork.SaveAsync();

            return MapToDto(entity, enterprise.Name, wasteType.Name);
        }

        // ================= GET BY ID =================
        public async Task<RecyclingStatisticDto?> GetByIdAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<RecyclingStatistic>();

            var entity = await repo.NoTrackingEntities
                .Include(x => x.Enterprise)
                .Include(x => x.WasteType)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            return entity == null
                ? null
                : MapToDto(entity,
                    entity.Enterprise?.Name,
                    entity.WasteType?.Name);
        }

        // ================= GET ALL =================
        public async Task<PagedRecyclingStatisticDto> GetAllAsync(
            RecyclingStatisticFilterDto filter)
        {
            var repo = _unitOfWork.GetRepository<RecyclingStatistic>();

            var query = repo.NoTrackingEntities
                .Include(x => x.Enterprise)
                .Include(x => x.WasteType)
                .Where(x => !x.IsDeleted);

            if (filter.EnterpriseId.HasValue)
                query = query.Where(x => x.EnterpriseId == filter.EnterpriseId);

            if (filter.WasteTypeId.HasValue)
                query = query.Where(x => x.WasteTypeId == filter.WasteTypeId);

            if (!string.IsNullOrWhiteSpace(filter.RegionCode))
                query = query.Where(x => x.RegionCode.Contains(filter.RegionCode));

            if (filter.FromPeriod.HasValue)
                query = query.Where(x => x.Period >= filter.FromPeriod);

            if (filter.ToPeriod.HasValue)
                query = query.Where(x => x.Period <= filter.ToPeriod);

            var pagedData = await repo.GetPagging(
                query.OrderByDescending(x => x.Period),
                filter.PageNumber,
                filter.PageSize);

            return new PagedRecyclingStatisticDto
            {
                TotalCount = pagedData.TotalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = pagedData.Items
                    .Select(x => MapToDto(
                        x,
                        x.Enterprise?.Name,
                        x.WasteType?.Name))
                    .ToList()
            };
        }

        // ================= UPDATE =================
        public async Task<bool> UpdateAsync(
            Guid id,
            UpdateRecyclingStatisticDto dto)
        {
            var repo = _unitOfWork.GetRepository<RecyclingStatistic>();

            var entity = await repo.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
                return false;

            if (dto.TotalWeightKg > 0)
                entity.TotalWeightKg = dto.TotalWeightKg;

            if (!string.IsNullOrWhiteSpace(dto.RegionCode))
                entity.RegionCode = dto.RegionCode;

            entity.LastUpdatedTime = CoreHelper.SystemTimeNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // ================= DELETE (SOFT) =================
        public async Task<bool> DeleteAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<RecyclingStatistic>();

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
        private static RecyclingStatisticDto MapToDto(
            RecyclingStatistic entity,
            string? enterpriseName,
            string? wasteTypeName)
        {
            return new RecyclingStatisticDto
            {
                Id = entity.Id,
                EnterpriseId = entity.EnterpriseId,
                EnterpriseName = enterpriseName ?? string.Empty,
                WasteTypeId = entity.WasteTypeId,
                WasteTypeName = wasteTypeName ?? string.Empty,
                TotalWeightKg = entity.TotalWeightKg,
                RegionCode = entity.RegionCode,
                Period = entity.Period,
                CreatedTime = entity.CreatedTime
            };
        }
    }
}