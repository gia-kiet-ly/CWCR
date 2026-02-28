using Application.Contract.DTOs;
using Application.Contract.Interfaces;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Core.Utils;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class EnterpriseWasteCapabilityService
        : IEnterpriseWasteCapabilityService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EnterpriseWasteCapabilityService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ================= CREATE =================
        public async Task<EnterpriseWasteCapabilityDto> CreateAsync(
            CreateEnterpriseWasteCapabilityDto dto)
        {
            if (dto.DailyCapacityKg <= 0)
                throw new Exception("Daily capacity must be greater than 0");

            var capabilityRepo = _unitOfWork.GetRepository<EnterpriseWasteCapability>();
            var enterpriseRepo = _unitOfWork.GetRepository<RecyclingEnterprise>();
            var wasteTypeRepo = _unitOfWork.GetRepository<WasteType>();

            // Check enterprise tồn tại
            var enterprise = await enterpriseRepo.NoTrackingEntities
                .FirstOrDefaultAsync(x => x.Id == dto.EnterpriseId && !x.IsDeleted);

            if (enterprise == null)
                throw new Exception("Enterprise not found");

            // Check waste type tồn tại
            var wasteType = await wasteTypeRepo.NoTrackingEntities
                .FirstOrDefaultAsync(x => x.Id == dto.WasteTypeId && !x.IsDeleted);

            if (wasteType == null)
                throw new Exception("Waste type not found");

            // Không cho duplicate WasteType trong cùng Enterprise
            var existed = await capabilityRepo.NoTrackingEntities
                .AnyAsync(x =>
                    x.EnterpriseId == dto.EnterpriseId &&
                    x.WasteTypeId == dto.WasteTypeId &&
                    !x.IsDeleted);

            if (existed)
                throw new Exception("Capability already exists");

            var entity = new EnterpriseWasteCapability
            {
                EnterpriseId = dto.EnterpriseId,
                WasteTypeId = dto.WasteTypeId,
                DailyCapacityKg = dto.DailyCapacityKg
            };

            await capabilityRepo.InsertAsync(entity);
            await _unitOfWork.SaveAsync();

            return MapToDto(entity, enterprise.Name, wasteType.Name);
        }

        // ================= GET BY ID =================
        public async Task<EnterpriseWasteCapabilityDto?> GetByIdAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<EnterpriseWasteCapability>();

            var entity = await repo.NoTrackingEntities
                .Include(x => x.Enterprise)
                .Include(x => x.WasteType)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            return entity == null
                ? null
                : MapToDto(entity, entity.Enterprise?.Name, entity.WasteType?.Name);
        }

        // ================= GET ALL (FILTER + PAGING) =================
        public async Task<PagedEnterpriseWasteCapabilityDto> GetAllAsync(
            EnterpriseWasteCapabilityFilterDto filter)
        {
            var repo = _unitOfWork.GetRepository<EnterpriseWasteCapability>();

            var query = repo.NoTrackingEntities
                .Include(x => x.Enterprise)
                .Include(x => x.WasteType)
                .Where(x => !x.IsDeleted);

            if (filter.EnterpriseId.HasValue)
                query = query.Where(x => x.EnterpriseId == filter.EnterpriseId);

            if (filter.WasteTypeId.HasValue)
                query = query.Where(x => x.WasteTypeId == filter.WasteTypeId);

            if (filter.MinCapacity.HasValue)
                query = query.Where(x => x.DailyCapacityKg >= filter.MinCapacity);

            if (filter.MaxCapacity.HasValue)
                query = query.Where(x => x.DailyCapacityKg <= filter.MaxCapacity);

            var pagedData = await repo.GetPagging(
                query.OrderByDescending(x => x.CreatedTime),
                filter.PageNumber,
                filter.PageSize);

            return new PagedEnterpriseWasteCapabilityDto
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
            UpdateEnterpriseWasteCapabilityDto dto)
        {
            if (dto.DailyCapacityKg <= 0)
                return false;

            var repo = _unitOfWork.GetRepository<EnterpriseWasteCapability>();

            var entity = await repo.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
                return false;

            entity.DailyCapacityKg = dto.DailyCapacityKg;
            entity.LastUpdatedTime = CoreHelper.SystemTimeNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // ================= DELETE (SOFT) =================
        public async Task<bool> DeleteAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<EnterpriseWasteCapability>();

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
        private static EnterpriseWasteCapabilityDto MapToDto(
            EnterpriseWasteCapability entity,
            string? enterpriseName,
            string? wasteTypeName)
        {
            return new EnterpriseWasteCapabilityDto
            {
                Id = entity.Id,
                EnterpriseId = entity.EnterpriseId,
                EnterpriseName = enterpriseName ?? string.Empty,
                WasteTypeId = entity.WasteTypeId,
                WasteTypeName = wasteTypeName ?? string.Empty,
                DailyCapacityKg = entity.DailyCapacityKg,
                CreatedTime = entity.CreatedTime
            };
        }
    }
}