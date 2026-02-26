using Application.Contract.DTOs;
using Application.Contract.Interfaces;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class WasteTypeService : IWasteTypeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WasteTypeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ================= CREATE =================
        public async Task<WasteTypeResponseDto> CreateAsync(CreateWasteTypeDto dto)
        {
            var repo = _unitOfWork.GetRepository<WasteType>();

            // Check duplicate Name (không tính record đã delete)
            var exists = await repo.NoTrackingEntities
                .AnyAsync(x => !x.IsDeleted &&
                               x.Name.ToLower() == dto.Name.ToLower());

            if (exists)
                throw new Exception("WasteType name already exists");

            var entity = new WasteType
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                Category = dto.Category,
                IsActive = true
            };

            await repo.InsertAsync(entity);
            await _unitOfWork.SaveAsync();

            return MapToDto(entity);
        }

        // ================= UPDATE =================
        public async Task<WasteTypeResponseDto> UpdateAsync(Guid id, UpdateWasteTypeDto dto)
        {
            var repo = _unitOfWork.GetRepository<WasteType>();

            var entity = await repo.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted)
                throw new Exception("WasteType not found");

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                var duplicate = await repo.NoTrackingEntities
                    .AnyAsync(x => !x.IsDeleted &&
                                   x.Id != id &&
                                   x.Name.ToLower() == dto.Name.ToLower());

                if (duplicate)
                    throw new Exception("WasteType name already exists");

                entity.Name = dto.Name.Trim();
            }

            if (dto.Description != null)
                entity.Description = dto.Description;

            if (dto.Category.HasValue)
                entity.Category = dto.Category.Value;

            if (dto.IsActive.HasValue)
                entity.IsActive = dto.IsActive.Value;

            entity.LastUpdatedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();

            return MapToDto(entity);
        }

        // ================= GET BY ID =================
        public async Task<WasteTypeResponseDto?> GetByIdAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<WasteType>();

            var entity = await repo.NoTrackingEntities
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            return entity == null ? null : MapToDto(entity);
        }

        // ================= PAGING / FILTER =================
        public async Task<PagedWasteTypeDto> GetPagedAsync(WasteTypeFilterDto filter)
        {
            var repo = _unitOfWork.GetRepository<WasteType>();

            var query = repo.NoTrackingEntities
                .Where(x => !x.IsDeleted);

            if (filter.Category.HasValue)
                query = query.Where(x => x.Category == filter.Category.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(x => x.IsActive == filter.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            var paged = await repo.GetPagging(
                query.OrderByDescending(x => x.CreatedTime),
                filter.PageNumber,
                filter.PageSize);

            return new PagedWasteTypeDto
            {
                TotalCount = paged.TotalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = paged.Items.Select(MapToDto).ToList()
            };
        }

        // ================= DELETE (SOFT) =================
        public async Task<bool> DeleteAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<WasteType>();

            var entity = await repo.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted)
                return false;

            entity.IsDeleted = true;
            entity.DeletedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // ================= MAPPER =================
        private static WasteTypeResponseDto MapToDto(WasteType entity)
        {
            return new WasteTypeResponseDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Category = entity.Category,
                IsActive = entity.IsActive,
                CreatedTime = entity.CreatedTime
            };
        }
    }
}