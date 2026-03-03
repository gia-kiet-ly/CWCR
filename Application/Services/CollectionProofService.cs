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
    public class CollectionProofService : ICollectionProofService
    {
        private readonly IUnitOfWork _uow;

        public CollectionProofService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // ================= COLLECTOR: CREATE =================
        public async Task<CollectionProofDto> CreateAsync(Guid collectorUserId, CreateCollectionProofDto dto)
        {
            var assignmentRepo = _uow.GetRepository<CollectorAssignment>();
            var proofRepo = _uow.GetRepository<CollectionProof>();

            // ownership check: assignment phải thuộc collector đang login
            var assignment = await assignmentRepo.NoTrackingEntities
                .Include(a => a.Collector) // CollectorProfile
                .FirstOrDefaultAsync(a => a.Id == dto.AssignmentId && !a.IsDeleted);

            if (assignment == null)
                throw new Exception("Assignment not found.");

            if (assignment.Collector.CollectorId != collectorUserId)
                throw new Exception("You do not own this assignment.");

            // Optional: chặn upload proof nếu assignment chưa Collected
            // (tuỳ bạn, MVP nên chặn)
            if (assignment.Status != AssignmentStatus.Collected)
                throw new Exception("Assignment must be Collected before uploading proof.");

            var entity = new CollectionProof
            {
                AssignmentId = dto.AssignmentId,
                ImageUrl = dto.ImageUrl,
                PublicId = dto.PublicId,
                Note = dto.Note,
                ReviewStatus = ProofReviewStatus.Pending
            };

            await proofRepo.InsertAsync(entity);
            await _uow.SaveAsync();

            return Map(entity);
        }

        // ================= COLLECTOR: GET BY ID =================
        public async Task<CollectionProofDto?> GetByIdCollectorAsync(Guid collectorUserId, Guid id)
        {
            var repo = _uow.GetRepository<CollectionProof>();

            var entity = await repo.NoTrackingEntities
                .Include(p => p.Assignment)
                    .ThenInclude(a => a.Collector)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (entity == null) return null;

            if (entity.Assignment.Collector.CollectorId != collectorUserId)
                return null;

            return Map(entity);
        }

        // ================= COLLECTOR: GET PAGED =================
        public async Task<PaginatedList<CollectionProofDto>> GetPagedCollectorAsync(Guid collectorUserId, CollectionProofFilterDto filter)
        {
            var repo = _uow.GetRepository<CollectionProof>();

            var query = repo.NoTrackingEntities
                .Include(p => p.Assignment)
                    .ThenInclude(a => a.Collector)
                .Where(p => !p.IsDeleted && p.Assignment.Collector.CollectorId == collectorUserId);

            if (filter.AssignmentId.HasValue)
                query = query.Where(x => x.AssignmentId == filter.AssignmentId.Value);

            if (!string.IsNullOrWhiteSpace(filter.ReviewStatus) &&
                Enum.TryParse<ProofReviewStatus>(filter.ReviewStatus, true, out var st))
                query = query.Where(x => x.ReviewStatus == st);

            var total = await query.CountAsync();
            var page = PaginationHelper.ValidateAndAdjustPageNumber(filter.PageNumber, total, filter.PageSize);

            var list = await query
                .OrderByDescending(x => x.CreatedTime)
                .Skip((page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = list.Select(Map).ToList();
            return new PaginatedList<CollectionProofDto>(dtos, total, page, filter.PageSize);
        }

        // ================= ENTERPRISE: GET BY ID =================
        public async Task<CollectionProofDto?> GetByIdEnterpriseAsync(Guid enterpriseId, Guid id)
        {
            var repo = _uow.GetRepository<CollectionProof>();

            var entity = await repo.NoTrackingEntities
                .Include(p => p.Assignment)
                    .ThenInclude(a => a.Request)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (entity == null) return null;

            // enterprise ownership: proof -> assignment -> request -> enterprise
            if (entity.Assignment.Request.EnterpriseId != enterpriseId)
                return null;

            return Map(entity);
        }

        // ================= ENTERPRISE: GET PAGED =================
        public async Task<PaginatedList<CollectionProofDto>> GetPagedEnterpriseAsync(Guid enterpriseId, CollectionProofFilterDto filter)
        {
            var repo = _uow.GetRepository<CollectionProof>();

            var query = repo.NoTrackingEntities
                .Include(p => p.Assignment)
                    .ThenInclude(a => a.Request)
                .Where(p => !p.IsDeleted && p.Assignment.Request.EnterpriseId == enterpriseId);

            if (filter.AssignmentId.HasValue)
                query = query.Where(x => x.AssignmentId == filter.AssignmentId.Value);

            if (!string.IsNullOrWhiteSpace(filter.ReviewStatus) &&
                Enum.TryParse<ProofReviewStatus>(filter.ReviewStatus, true, out var st))
                query = query.Where(x => x.ReviewStatus == st);

            var total = await query.CountAsync();
            var page = PaginationHelper.ValidateAndAdjustPageNumber(filter.PageNumber, total, filter.PageSize);

            var list = await query
                .OrderByDescending(x => x.CreatedTime)
                .Skip((page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = list.Select(Map).ToList();
            return new PaginatedList<CollectionProofDto>(dtos, total, page, filter.PageSize);
        }

        // ================= ENTERPRISE: REVIEW =================
        public async Task<bool> ReviewAsync(Guid enterpriseId, Guid reviewerUserId, Guid id, ReviewCollectionProofDto dto)
        {
            var repo = _uow.GetRepository<CollectionProof>();

            var entity = await repo.Entities
                .Include(p => p.Assignment)
                    .ThenInclude(a => a.Request)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (entity == null) return false;

            if (entity.Assignment.Request.EnterpriseId != enterpriseId)
                return false;

            if (!Enum.TryParse<ProofReviewStatus>(dto.Status, true, out var newStatus))
                return false;

            // chỉ cho review Approved/Rejected
            if (newStatus == ProofReviewStatus.Pending)
                return false;

            // Optional: chỉ review khi đang Pending
            if (entity.ReviewStatus != ProofReviewStatus.Pending)
                return false;

            entity.ReviewStatus = newStatus;
            entity.ReviewedBy = reviewerUserId;
            entity.ReviewedAt = DateTimeOffset.UtcNow;
            entity.ReviewNote = dto.ReviewNote;
            entity.LastUpdatedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _uow.SaveAsync();

            // Optional nâng cấp sau:
            // nếu Approved -> update CollectionRequest.Status = Collected (hoặc Completed)
            // và cộng CitizenPoint

            return true;
        }

        private static CollectionProofDto Map(CollectionProof x)
        {
            return new CollectionProofDto
            {
                Id = x.Id,
                AssignmentId = x.AssignmentId,
                ImageUrl = x.ImageUrl,
                PublicId = x.PublicId,
                Note = x.Note,
                ReviewStatus = x.ReviewStatus.ToString(),
                ReviewedBy = x.ReviewedBy,
                ReviewedAt = x.ReviewedAt,
                ReviewNote = x.ReviewNote,
                CreatedTime = x.CreatedTime
            };
        }
    }
}
