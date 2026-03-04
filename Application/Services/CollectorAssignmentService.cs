using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Application.Contract.Paggings;
using Core.Enum;
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
    public class CollectorAssignmentService : ICollectorAssignmentService
    {
        private readonly IUnitOfWork _uow;

        public CollectorAssignmentService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // ================= ENTERPRISE: CREATE ASSIGNMENT =================
        public async Task<CollectorAssignmentDto> CreateAsync(Guid enterpriseId, CreateAssignmentDto dto)
        {
            var requestRepo = _uow.GetRepository<CollectionRequest>();
            var assignmentRepo = _uow.GetRepository<CollectorAssignment>();
            var collectorRepo = _uow.GetRepository<CollectorProfile>();

            // 1) Request phải thuộc enterprise
            var request = await requestRepo.NoTrackingEntities
                .FirstOrDefaultAsync(r =>
                    r.Id == dto.RequestId &&
                    r.EnterpriseId == enterpriseId &&
                    !r.IsDeleted);

            if (request == null)
                throw new Exception("Request not found or not belong to enterprise.");

            // 2) CollectorProfile phải thuộc enterprise
            var collectorProfile = await collectorRepo.NoTrackingEntities
                .FirstOrDefaultAsync(c =>
                    c.Id == dto.CollectorProfileId &&
                    c.EnterpriseId == enterpriseId &&
                    !c.IsDeleted);

            if (collectorProfile == null)
                throw new Exception("CollectorProfile not found or not belong to enterprise.");

            var entity = new CollectorAssignment
            {
                RequestId = dto.RequestId,
                CollectorId = dto.CollectorProfileId,  // ✅ dùng CollectorProfileId
                Status = AssignmentStatus.Assigned
            };

            await assignmentRepo.InsertAsync(entity);
            await _uow.SaveAsync();

            // ✅ NEW (không đụng logic cũ): update CollectionRequest status -> Assigned
            // chỉ update khi request đã Accepted
            var requestTracked = await requestRepo.GetByIdAsync(dto.RequestId);
            if (requestTracked != null && !requestTracked.IsDeleted)
            {
                if (requestTracked.Status == CollectionRequestStatus.Accepted)
                {
                    requestTracked.Status = CollectionRequestStatus.Assigned;
                    requestTracked.LastUpdatedTime = DateTimeOffset.UtcNow;
                    requestRepo.Update(requestTracked);
                    await _uow.SaveAsync();
                }
            }

            return await GetByIdEnterpriseAsync(enterpriseId, entity.Id)
                   ?? throw new Exception("Create assignment failed");
        }

        // ================= ENTERPRISE: GET BY ID =================
        public async Task<CollectorAssignmentDto?> GetByIdEnterpriseAsync(Guid enterpriseId, Guid id)
        {
            var repo = _uow.GetRepository<CollectorAssignment>();

            var entity = await repo.NoTrackingEntities
                .Include(a => a.Request)
                .FirstOrDefaultAsync(a => a.Id == id &&
                                          !a.IsDeleted &&
                                          a.Request.EnterpriseId == enterpriseId);

            return entity == null ? null : Map(entity);
        }

        // ================= ENTERPRISE: GET PAGED =================
        public async Task<PaginatedList<CollectorAssignmentDto>> GetPagedEnterpriseAsync(Guid enterpriseId, AssignmentFilterDto filter)
        {
            var repo = _uow.GetRepository<CollectorAssignment>();

            var query = repo.NoTrackingEntities
                .Include(a => a.Request)
                .Where(a => !a.IsDeleted && a.Request.EnterpriseId == enterpriseId);

            if (filter.RequestId.HasValue)
                query = query.Where(x => x.RequestId == filter.RequestId.Value);

            if (filter.CollectorId.HasValue)
                query = query.Where(x => x.CollectorId == filter.CollectorId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                Enum.TryParse<AssignmentStatus>(filter.Status, true, out var st))
                query = query.Where(x => x.Status == st);

            var total = await query.CountAsync();
            var page = PaginationHelper.ValidateAndAdjustPageNumber(filter.PageNumber, total, filter.PageSize);

            var list = await query
                .OrderByDescending(x => x.CreatedTime)
                .Skip((page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = list.Select(Map).ToList();

            return new PaginatedList<CollectorAssignmentDto>(dtos, total, page, filter.PageSize);
        }

        // ================= COLLECTOR: GET BY ID =================
        public async Task<CollectorAssignmentDto?> GetByIdCollectorAsync(Guid collectorUserId, Guid id)
        {
            var repo = _uow.GetRepository<CollectorAssignment>();

            var entity = await repo.NoTrackingEntities
                .Include(a => a.Collector)
                .FirstOrDefaultAsync(a => a.Id == id &&
                                          !a.IsDeleted &&
                                          a.Collector.CollectorId == collectorUserId);

            return entity == null ? null : Map(entity);
        }

        // ================= COLLECTOR: GET PAGED =================
        public async Task<PaginatedList<CollectorAssignmentDto>> GetPagedCollectorAsync(Guid collectorUserId, AssignmentFilterDto filter)
        {
            var repo = _uow.GetRepository<CollectorAssignment>();

            var query = repo.NoTrackingEntities
                .Include(a => a.Collector)
                .Where(a => !a.IsDeleted && a.Collector.CollectorId == collectorUserId);

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                Enum.TryParse<AssignmentStatus>(filter.Status, true, out var st))
                query = query.Where(x => x.Status == st);

            var total = await query.CountAsync();
            var page = PaginationHelper.ValidateAndAdjustPageNumber(filter.PageNumber, total, filter.PageSize);

            var list = await query
                .OrderByDescending(x => x.CreatedTime)
                .Skip((page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = list.Select(Map).ToList();
            return new PaginatedList<CollectorAssignmentDto>(dtos, total, page, filter.PageSize);
        }

        // ================= COLLECTOR: UPDATE STATUS =================
        public async Task<bool> UpdateStatusCollectorAsync(Guid collectorUserId, Guid id, UpdateAssignmentStatusDto dto)
        {
            var repo = _uow.GetRepository<CollectorAssignment>();

            var entity = await repo.Entities
                .Include(a => a.Collector) // CollectorProfile
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (entity == null) return false;

            // Check ownership qua CollectorProfile
            if (entity.Collector.CollectorId != collectorUserId)
                return false;

            if (!Enum.TryParse<AssignmentStatus>(dto.Status, true, out var newStatus))
                return false;

            var allowed =
                (entity.Status == AssignmentStatus.Assigned && newStatus == AssignmentStatus.OnTheWay) ||
                (entity.Status == AssignmentStatus.OnTheWay && newStatus == AssignmentStatus.Collected);

            if (!allowed) return false;

            entity.Status = newStatus;

            if (newStatus == AssignmentStatus.Collected)
            {
                entity.CollectedAt = DateTimeOffset.UtcNow;
                entity.CollectedNote = dto.CollectedNote;
            }

            entity.LastUpdatedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _uow.SaveAsync();

            return true;
        }

        private static CollectorAssignmentDto Map(CollectorAssignment x)
        {
            return new CollectorAssignmentDto
            {
                Id = x.Id,
                RequestId = x.RequestId,
                CollectorProfileId = x.CollectorId,  // rename đúng nghĩa
                Status = x.Status.ToString(),
                CollectedAt = x.CollectedAt,
                CollectedNote = x.CollectedNote,
                CreatedTime = x.CreatedTime
            };
        }
    }
}