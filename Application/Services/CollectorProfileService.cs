using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Application.Contract.Paggings;
using Core.Utils;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CollectorProfileService : ICollectorProfileService
    {
        private readonly IUnitOfWork _uow;

        public CollectorProfileService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<CollectorProfileDto> CreateAsync(Guid enterpriseId, CreateCollectorProfileDto dto)
        {
            var repo = _uow.GetRepository<CollectorProfile>();

            var entity = new CollectorProfile
            {
                EnterpriseId = enterpriseId,
                CollectorId = dto.CollectorId,
                IsActive = dto.IsActive
            };

            await repo.InsertAsync(entity);
            await _uow.SaveAsync();

            return await GetByIdAsync(enterpriseId, entity.Id)
                ?? throw new Exception("Create failed");
        }

        public async Task<CollectorProfileDto?> GetByIdAsync(Guid enterpriseId, Guid id)
        {
            var repo = _uow.GetRepository<CollectorProfile>();

            var entity = await repo.NoTrackingEntities
                .Include(x => x.Collector)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EnterpriseId == enterpriseId &&
                    !x.IsDeleted);

            return entity == null ? null : Map(entity);
        }

        public async Task<IEnumerable<CollectorProfileDto>> GetAllAsync(Guid enterpriseId)
        {
            var repo = _uow.GetRepository<CollectorProfile>();

            var list = await repo.NoTrackingEntities
                .Include(x => x.Collector)
                .Where(x => x.EnterpriseId == enterpriseId && !x.IsDeleted)
                .ToListAsync();

            return list.Select(Map);
        }

        public async Task<PaginatedList<CollectorProfileDto>> GetPagedAsync(
            Guid enterpriseId,
            CollectorProfileFilterDto filter)
        {
            var repo = _uow.GetRepository<CollectorProfile>();

            var query = repo.NoTrackingEntities
                .Include(x => x.Collector)
                .Where(x => x.EnterpriseId == enterpriseId && !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                query = query.Where(x =>
                    x.Collector.UserName!.Contains(filter.Keyword) ||
                    x.Collector.Email!.Contains(filter.Keyword));
            }

            if (filter.IsActive.HasValue)
                query = query.Where(x => x.IsActive == filter.IsActive.Value);

            var totalCount = await query.CountAsync();

            var pageNumber = PaginationHelper.ValidateAndAdjustPageNumber(
                filter.PageNumber,
                totalCount,
                filter.PageSize);

            var paged = await query
                .OrderByDescending(x => x.CreatedTime)
                .Skip((pageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = paged.Select(Map).ToList();

            return new PaginatedList<CollectorProfileDto>(
                result,
                totalCount,
                pageNumber,
                filter.PageSize);
        }

        public async Task<bool> UpdateAsync(Guid enterpriseId, Guid id, UpdateCollectorProfileDto dto)
        {
            var repo = _uow.GetRepository<CollectorProfile>();

            var entity = await repo.GetByIdAsync(id);

            if (entity == null ||
                entity.IsDeleted ||
                entity.EnterpriseId != enterpriseId)
                return false;

            entity.IsActive = dto.IsActive;
            entity.LastUpdatedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _uow.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid enterpriseId, Guid id)
        {
            var repo = _uow.GetRepository<CollectorProfile>();

            var entity = await repo.GetByIdAsync(id);

            if (entity == null || entity.EnterpriseId != enterpriseId)
                return false;

            entity.IsDeleted = true;
            entity.DeletedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _uow.SaveAsync();

            return true;
        }

        private static CollectorProfileDto Map(CollectorProfile x)
        {
            return new CollectorProfileDto
            {
                Id = x.Id,
                CollectorId = x.CollectorId,
                CollectorName = x.Collector?.UserName,
                CollectorEmail = x.Collector?.Email,
                EnterpriseId = x.EnterpriseId,
                IsActive = x.IsActive,
                CreatedTime = x.CreatedTime
            };
        }
    }
}
