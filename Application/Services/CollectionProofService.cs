using Application.Constants;
using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Application.Contract.Paggings;
using Core.Enum;
using Core.Utils;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class CollectionProofService : ICollectionProofService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICitizenPointService _citizenPointService;
        private readonly INotificationService _notificationService;
        private readonly IRegionCodeResolver _regionCodeResolver;

        public CollectionProofService(
            IUnitOfWork uow,
            ICitizenPointService citizenPointService,
            INotificationService notificationService,
            IRegionCodeResolver regionCodeResolver)
        {
            _uow = uow;
            _citizenPointService = citizenPointService;
            _notificationService = notificationService;
            _regionCodeResolver = regionCodeResolver;
        }

        // ================= BASE QUERY =================
        private IQueryable<CollectionProof> BuildFullQuery(IGenericRepository<CollectionProof> repo)
        {
            return repo.NoTrackingEntities
                .Include(p => p.Assignment)
                    .ThenInclude(a => a.Request)
                        .ThenInclude(r => r.WasteReportWaste)
                            .ThenInclude(w => w.WasteType)
                .Include(p => p.Assignment)
                    .ThenInclude(a => a.Request)
                        .ThenInclude(r => r.WasteReportWaste)
                            .ThenInclude(w => w.Images)
                .Include(p => p.Assignment)
                    .ThenInclude(a => a.Request)
                        .ThenInclude(r => r.WasteReportWaste)
                            .ThenInclude(w => w.WasteReport);
        }

        // ================= COLLECTOR: CREATE =================
        public async Task<CollectionProofDto> CreateAsync(Guid collectorUserId, CreateCollectionProofDto dto)
        {
            var assignmentRepo = _uow.GetRepository<CollectorAssignment>();
            var proofRepo = _uow.GetRepository<CollectionProof>();

            var assignment = await assignmentRepo.NoTrackingEntities
                .FirstOrDefaultAsync(a => a.Id == dto.AssignmentId && !a.IsDeleted);

            if (assignment == null)
                throw new Exception("Assignment not found.");

            if (assignment.CollectorId != collectorUserId)
                throw new Exception("You do not own this assignment.");

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

            // Notification
            var assignmentFull = await assignmentRepo.Entities
                .Include(a => a.Request)
                .FirstOrDefaultAsync(a => a.Id == dto.AssignmentId);

            if (assignmentFull?.Request != null)
            {
                await _notificationService.CreateAsync(
                    assignmentFull.Request.EnterpriseId,
                    NotificationConstants.Types.PROOF_UPLOADED,
                    NotificationConstants.ReferenceTypes.COLLECTION_PROOF,
                    entity.Id
                );
            }

            var created = await BuildFullQuery(proofRepo)
                .FirstAsync(p => p.Id == entity.Id && !p.IsDeleted);

            return Map(created);
        }

        // ================= COLLECTOR: GET BY ID =================
        public async Task<CollectionProofDto?> GetByIdCollectorAsync(Guid collectorUserId, Guid id)
        {
            var repo = _uow.GetRepository<CollectionProof>();

            var entity = await BuildFullQuery(repo)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (entity == null) return null;

            if (entity.Assignment?.CollectorId != collectorUserId)
                return null;

            return Map(entity);
        }

        // ================= COLLECTOR: GET PAGED =================
        public async Task<PaginatedList<CollectionProofDto>> GetPagedCollectorAsync(Guid collectorUserId, CollectionProofFilterDto filter)
        {
            var repo = _uow.GetRepository<CollectionProof>();

            var query = BuildFullQuery(repo)
                .Where(p => !p.IsDeleted && p.Assignment.CollectorId == collectorUserId);

            if (filter.AssignmentId.HasValue)
                query = query.Where(x => x.AssignmentId == filter.AssignmentId.Value);

            if (filter.ReviewStatus.HasValue)
                query = query.Where(x => x.ReviewStatus == filter.ReviewStatus.Value);

            var total = await query.CountAsync();

            var page = PaginationHelper.ValidateAndAdjustPageNumber(
                filter.PageNumber, total, filter.PageSize);

            var list = await query
                .OrderByDescending(x => x.CreatedTime)
                .Skip((page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PaginatedList<CollectionProofDto>(
                list.Select(Map).ToList(),
                total,
                page,
                filter.PageSize
            );
        }

        // ================= ENTERPRISE: GET BY ID =================
        public async Task<CollectionProofDto?> GetByIdEnterpriseAsync(Guid enterpriseId, Guid id)
        {
            var repo = _uow.GetRepository<CollectionProof>();

            var entity = await BuildFullQuery(repo)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (entity == null) return null;

            if (entity.Assignment?.Request?.EnterpriseId != enterpriseId)
                return null;

            return Map(entity);
        }

        // ================= ENTERPRISE: GET PAGED =================
        public async Task<PaginatedList<CollectionProofDto>> GetPagedEnterpriseAsync(Guid enterpriseId, CollectionProofFilterDto filter)
        {
            var repo = _uow.GetRepository<CollectionProof>();

            var query = BuildFullQuery(repo)
                .Where(p => !p.IsDeleted &&
                            p.Assignment.Request.EnterpriseId == enterpriseId);

            if (filter.AssignmentId.HasValue)
                query = query.Where(x => x.AssignmentId == filter.AssignmentId.Value);

            if (filter.ReviewStatus.HasValue)
                query = query.Where(x => x.ReviewStatus == filter.ReviewStatus.Value);

            var total = await query.CountAsync();

            var page = PaginationHelper.ValidateAndAdjustPageNumber(
                filter.PageNumber, total, filter.PageSize);

            var list = await query
                .OrderByDescending(x => x.CreatedTime)
                .Skip((page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PaginatedList<CollectionProofDto>(
                list.Select(Map).ToList(),
                total,
                page,
                filter.PageSize
            );
        }

        // ================= ENTERPRISE: REVIEW =================
        public async Task<bool> ReviewAsync(Guid enterpriseId, Guid reviewerUserId, Guid id, ReviewCollectionProofDto dto)
        {
            var repo = _uow.GetRepository<CollectionProof>();

            var entity = await repo.Entities
                .Include(p => p.Assignment)
                    .ThenInclude(a => a.Request)
                        .ThenInclude(r => r.WasteReportWaste)
                            .ThenInclude(w => w.WasteReport)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (entity == null) return false;

            if (entity.Assignment?.Request?.EnterpriseId != enterpriseId)
                return false;

            if (dto.Status == ProofReviewStatus.Pending)
                return false;

            if (entity.ReviewStatus != ProofReviewStatus.Pending)
                return false;

            entity.ReviewStatus = dto.Status;
            entity.ReviewedBy = reviewerUserId;
            entity.ReviewedAt = DateTimeOffset.UtcNow;
            entity.ReviewNote = dto.ReviewNote;
            entity.LastUpdatedTime = DateTimeOffset.UtcNow;

            Guid? wasteReportId = null;

            if (dto.Status == ProofReviewStatus.Approved)
            {
                entity.Assignment.Status = AssignmentStatus.Completed;
                entity.Assignment.LastUpdatedTime = DateTimeOffset.UtcNow;

                entity.Assignment.Request.Status = CollectionRequestStatus.Completed;
                entity.Assignment.Request.LastUpdatedTime = DateTimeOffset.UtcNow;

                var wasteReport = entity.Assignment.Request.WasteReportWaste?.WasteReport
                    ?? throw new Exception("WasteReport not found.");

                wasteReport.Status = WasteReportStatus.Verified;
                wasteReport.LastUpdatedTime = DateTimeOffset.UtcNow;
                wasteReportId = wasteReport.Id;

                // ← THÊM DÒNG NÀY
                var wasteReportRepo = _uow.GetRepository<WasteReport>();
                wasteReportRepo.Update(wasteReport);
            }

            repo.Update(entity);
            await _uow.SaveAsync();

            // Notification Collector
            await _notificationService.CreateAsync(
                entity.Assignment.CollectorId,
                NotificationConstants.Types.PROOF_VERIFIED,
                NotificationConstants.ReferenceTypes.COLLECTION_PROOF,
                entity.Id
            );

            if (dto.Status == ProofReviewStatus.Approved && wasteReportId.HasValue)
            {
                await _citizenPointService.AwardPointsForVerifiedReportAsync(wasteReportId.Value);

                var wasteReport = entity.Assignment.Request.WasteReportWaste?.WasteReport;

                if (wasteReport != null)
                {
                    await _notificationService.CreateAsync(
                        wasteReport.CitizenId,
                        NotificationConstants.Types.PROOF_VERIFIED,
                        NotificationConstants.ReferenceTypes.WASTE_REPORT,
                        wasteReport.Id
                    );
                }
            }

            return true;
        }

        private static CollectionProofDto Map(CollectionProof x)
        {
            var assignment = x.Assignment;
            var request = assignment?.Request;
            var item = request?.WasteReportWaste;
            var report = item?.WasteReport;

            return new CollectionProofDto
            {
                Id = x.Id,
                AssignmentId = x.AssignmentId,
                ImageUrl = x.ImageUrl,
                PublicId = x.PublicId,
                Note = x.Note,

                ReviewStatus = x.ReviewStatus,
                ReviewedBy = x.ReviewedBy,
                ReviewedAt = x.ReviewedAt,
                ReviewNote = x.ReviewNote,
                CreatedTime = x.CreatedTime,
                LastUpdatedTime = x.LastUpdatedTime,

                CollectorId = assignment?.CollectorId ?? Guid.Empty,
                AssignmentStatus = assignment?.Status ?? AssignmentStatus.Assigned,
                CollectedAt = assignment?.CollectedAt,
                CollectedNote = assignment?.CollectedNote,

                RequestId = request?.Id ?? Guid.Empty,
                EnterpriseId = request?.EnterpriseId ?? Guid.Empty,
                RequestStatus = request?.Status ?? CollectionRequestStatus.Offered,
                PriorityScore = request?.PriorityScore,

                WasteReportWasteId = request?.WasteReportWasteId ?? Guid.Empty,
                WasteTypeId = item?.WasteTypeId ?? Guid.Empty,
                WasteTypeName = item?.WasteType?.Name,
                RequestNote = item?.Note,

                ImageUrls = item?.Images?
                    .Where(i => !i.IsDeleted)
                    .Select(i => i.ImageUrl)
                    .ToList() ?? new List<string>(),

                WasteReportId = report?.Id ?? Guid.Empty,
                Latitude = report?.Latitude,
                Longitude = report?.Longitude,
                RegionCode = report?.RegionCode
            };
        }
    }
}