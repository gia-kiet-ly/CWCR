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
            var collectorProfileRepo = _uow.GetRepository<CollectorProfile>();

            // 1) Request phải thuộc enterprise
            var request = await requestRepo.Entities
                .FirstOrDefaultAsync(r =>
                    r.Id == dto.RequestId &&
                    r.EnterpriseId == enterpriseId &&
                    !r.IsDeleted);

            if (request == null)
                throw new Exception("Request not found or not belong to enterprise.");

            // 2) Chỉ assign khi request đã Accepted
            if (request.Status != CollectionRequestStatus.Accepted)
                throw new Exception("Only accepted request can be assigned.");

            // 3) Không cho assign trùng
            var existedAssignment = await assignmentRepo.NoTrackingEntities
                .AnyAsync(a => a.RequestId == dto.RequestId && !a.IsDeleted);

            if (existedAssignment)
                throw new Exception("Request already has an assignment.");

            // 4) Collector phải là user thuộc enterprise này thông qua CollectorProfile
            var collectorProfile = await collectorProfileRepo.NoTrackingEntities
                .FirstOrDefaultAsync(c =>
                    c.CollectorId == dto.CollectorId &&
                    c.EnterpriseId == enterpriseId &&
                    c.IsActive &&
                    !c.IsDeleted);

            if (collectorProfile == null)
                throw new Exception("Collector not found or not belong to enterprise.");

            var entity = new CollectorAssignment
            {
                RequestId = dto.RequestId,
                CollectorId = dto.CollectorId, // ApplicationUser.Id
                Status = AssignmentStatus.Assigned
            };

            await assignmentRepo.InsertAsync(entity);

            request.Status = CollectionRequestStatus.Assigned;
            request.LastUpdatedTime = DateTimeOffset.UtcNow;
            requestRepo.Update(request);

            await _uow.SaveAsync();

            return await GetByIdEnterpriseAsync(enterpriseId, entity.Id)
                   ?? throw new Exception("Create assignment failed.");
        }

        // ================= ENTERPRISE: GET BY ID =================
        public async Task<CollectorAssignmentDto?> GetByIdEnterpriseAsync(Guid enterpriseId, Guid id)
        {
            var repo = _uow.GetRepository<CollectorAssignment>();

            var entity = await repo.NoTrackingEntities
                .Include(a => a.Request)
                    .ThenInclude(r => r.WasteReportWaste)
                        .ThenInclude(w => w.WasteType)
                .Include(a => a.Request)
                    .ThenInclude(r => r.WasteReportWaste)
                        .ThenInclude(w => w.Images)
                .Include(a => a.Request)
                    .ThenInclude(r => r.WasteReportWaste)
                        .ThenInclude(w => w.WasteReport)
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
                    .ThenInclude(r => r.WasteReportWaste)
                        .ThenInclude(w => w.WasteType)
                .Include(a => a.Request)
                    .ThenInclude(r => r.WasteReportWaste)
                        .ThenInclude(w => w.Images)
                .Include(a => a.Request)
                    .ThenInclude(r => r.WasteReportWaste)
                        .ThenInclude(w => w.WasteReport)
                .Where(a => !a.IsDeleted && a.Request.EnterpriseId == enterpriseId);

            if (filter.RequestId.HasValue)
                query = query.Where(x => x.RequestId == filter.RequestId.Value);

            if (filter.CollectorId.HasValue)
                query = query.Where(x => x.CollectorId == filter.CollectorId.Value);

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

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
                .Include(a => a.Request)
                    .ThenInclude(r => r.WasteReportWaste)
                        .ThenInclude(w => w.WasteType)
                .Include(a => a.Request)
                    .ThenInclude(r => r.WasteReportWaste)
                        .ThenInclude(w => w.Images)
                .Include(a => a.Request)
                    .ThenInclude(r => r.WasteReportWaste)
                        .ThenInclude(w => w.WasteReport)
                .FirstOrDefaultAsync(a => a.Id == id &&
                                          !a.IsDeleted &&
                                          a.CollectorId == collectorUserId);

            return entity == null ? null : Map(entity);
        }

        // ================= COLLECTOR: GET PAGED =================
        public async Task<PaginatedList<CollectorAssignmentDto>> GetPagedCollectorAsync(Guid collectorUserId, AssignmentFilterDto filter)
        {
            var repo = _uow.GetRepository<CollectorAssignment>();

            var query = repo.NoTrackingEntities
                .Include(a => a.Request)
                    .ThenInclude(r => r.WasteReportWaste)
                        .ThenInclude(w => w.WasteType)
                .Include(a => a.Request)
                    .ThenInclude(r => r.WasteReportWaste)
                        .ThenInclude(w => w.Images)
                .Include(a => a.Request)
                    .ThenInclude(r => r.WasteReportWaste)
                        .ThenInclude(w => w.WasteReport)
                .Where(a => !a.IsDeleted && a.CollectorId == collectorUserId);

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            if (filter.RequestId.HasValue)
                query = query.Where(x => x.RequestId == filter.RequestId.Value);

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
                .FirstOrDefaultAsync(a => a.Id == id &&
                                          !a.IsDeleted &&
                                          a.CollectorId == collectorUserId);

            if (entity == null) return false;

            var newStatus = dto.Status;

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
            var request = x.Request;
            var item = request?.WasteReportWaste;
            var report = item?.WasteReport;

            return new CollectorAssignmentDto
            {
                Id = x.Id,
                RequestId = x.RequestId,
                CollectorId = x.CollectorId,

                Status = x.Status,
                CollectedAt = x.CollectedAt,
                CollectedNote = x.CollectedNote,
                CreatedTime = x.CreatedTime,
                LastUpdatedTime = x.LastUpdatedTime,

                EnterpriseId = request?.EnterpriseId ?? Guid.Empty,
                RequestStatus = request?.Status ?? CollectionRequestStatus.Offered,
                PriorityScore = request?.PriorityScore,

                WasteReportWasteId = request?.WasteReportWasteId ?? Guid.Empty,
                WasteTypeId = item?.WasteTypeId ?? Guid.Empty,
                WasteTypeName = item?.WasteType?.Name,
                Note = item?.Note,
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