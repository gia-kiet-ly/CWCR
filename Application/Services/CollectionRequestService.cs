using Application.Constants;
using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Core.Enum;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class CollectionRequestService : ICollectionRequestService
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notificationService;
        private readonly IRegionCodeResolver _regionCodeResolver;

        public CollectionRequestService(
            IUnitOfWork uow,
            INotificationService notificationService,
            IRegionCodeResolver regionCodeResolver)

        {
            _uow = uow;
            _notificationService = notificationService;
            _regionCodeResolver = regionCodeResolver;
        }

        // =============================
        // SYSTEM: DISPATCH
        // =============================
        public async Task CreateTop1RequestsForReportAsync(Guid wasteReportId)
        {
            var reportRepo = _uow.GetRepository<WasteReport>();
            var reqRepo = _uow.GetRepository<CollectionRequest>();

            var report = await reportRepo.NoTrackingEntities
                .Include(r => r.Wastes)
                .FirstOrDefaultAsync(r => r.Id == wasteReportId && !r.IsDeleted);

            if (report == null)
                throw new Exception("WasteReport not found");

            foreach (var item in report.Wastes)
            {
                var existed = await reqRepo.NoTrackingEntities
                    .AnyAsync(x => x.WasteReportWasteId == item.Id && !x.IsDeleted);

                if (existed) continue;

                var best = await FindBestEnterpriseTop1Async(
                    item.Id,
                    report.RegionCode,
                    item.WasteTypeId);

                if (best == null)
                    continue;

                var request = new CollectionRequest
                {
                    WasteReportWasteId = item.Id,
                    EnterpriseId = best.Value.enterpriseId,
                    Status = CollectionRequestStatus.Offered,
                    PriorityScore = (int)best.Value.capacity
                };

                await reqRepo.InsertAsync(request);
            }

            await _uow.SaveAsync();
        }

        // =============================
        // DISPATCH LOGIC
        // =============================
        private async Task<(Guid enterpriseId, decimal capacity)?> FindBestEnterpriseTop1Async(
            Guid wasteItemId,
            string regionCode,
            Guid wasteTypeId)
        {
            if (string.IsNullOrWhiteSpace(regionCode))
                return null;

            var enterpriseRepo = _uow.GetRepository<RecyclingEnterprise>();
            var areaRepo = _uow.GetRepository<EnterpriseServiceArea>();
            var capRepo = _uow.GetRepository<EnterpriseWasteCapability>();
            var districtRepo = _uow.GetRepository<District>();
            var wardRepo = _uow.GetRepository<Ward>();
            var reqRepo = _uow.GetRepository<CollectionRequest>();

            var requestedEnterpriseIds = await reqRepo.NoTrackingEntities
                .Where(x =>
                    x.WasteReportWasteId == wasteItemId &&
                    !x.IsDeleted)
                .Select(x => x.EnterpriseId)
                .ToListAsync();

            var districtIds = await districtRepo.NoTrackingEntities
                .Where(x => !x.IsDeleted &&
                    EF.Functions.Like(regionCode, "%" + x.Code + "%"))
                .Select(x => x.Id)
                .ToListAsync();

            var wardIds = await wardRepo.NoTrackingEntities
                .Where(x => !x.IsDeleted &&
                    EF.Functions.Like(regionCode, "%" + x.Code + "%"))
                .Select(x => x.Id)
                .ToListAsync();

            if (!districtIds.Any() && !wardIds.Any())
                return null;

            var enterprisesInArea =
                from e in enterpriseRepo.NoTrackingEntities
                join a in areaRepo.NoTrackingEntities
                    on e.Id equals a.EnterpriseId
                where !e.IsDeleted
                      && !a.IsDeleted
                      && e.ApprovalStatus == EnterpriseApprovalStatus.Approved
                      && e.OperationalStatus == EnterpriseStatus.Active
                      && (
                            districtIds.Contains(a.DistrictId)
                            || (a.WardId != null && wardIds.Contains(a.WardId.Value))
                         )
                select e.Id;

            var candidates =
                from eid in enterprisesInArea.Distinct()
                join c in capRepo.NoTrackingEntities
                    on eid equals c.EnterpriseId
                where !c.IsDeleted
                      && c.WasteTypeId == wasteTypeId
                      && !requestedEnterpriseIds.Contains(eid)
                      && (c.DailyCapacityKg - c.AssignedTodayKg) > 0
                select new
                {
                    EnterpriseId = eid,
                    RemainingCapacity = (c.DailyCapacityKg - c.AssignedTodayKg)
                };

            var best = await candidates
                .OrderByDescending(x => x.RemainingCapacity)
                .FirstOrDefaultAsync();

            if (best == null)
                return null;

            return (best.EnterpriseId, best.RemainingCapacity);
        }

        // =============================
        // ENTERPRISE: INBOX
        // =============================
        public async Task<PagedCollectionRequestDto> GetPagedForEnterpriseAsync(
            Guid enterpriseId,
            CollectionRequestFilterDto filter)
        {
            var repo = _uow.GetRepository<CollectionRequest>();

            var query = repo.NoTrackingEntities
                .Include(r => r.Assignments)
                .Include(r => r.WasteReportWaste)
                    .ThenInclude(w => w.WasteType)
                .Include(r => r.WasteReportWaste)
                    .ThenInclude(w => w.Images)
                .Include(r => r.WasteReportWaste)
                    .ThenInclude(w => w.WasteReport)
                .Where(r => !r.IsDeleted && r.EnterpriseId == enterpriseId);

            if (filter.Status.HasValue)
                query = query.Where(r => r.Status == filter.Status.Value);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedTime)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = items.Select(MapToDto).ToList();

            // ✅ Thêm address (KHÔNG đụng logic cũ)
            foreach (var dto in dtos)
            {
                if (dto.Latitude.HasValue && dto.Longitude.HasValue)
                {
                    var (address, _) = await _regionCodeResolver
                        .ResolveFullAsync(dto.Latitude.Value, dto.Longitude.Value);

                    dto.Address = address;
                }
            }

            return new PagedCollectionRequestDto
            {
                TotalCount = total,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = dtos
            };
        }

        // =============================
        // ACCEPT
        // =============================
        public async Task<bool> AcceptAsync(Guid enterpriseId, Guid requestId)
        {
            var repo = _uow.GetRepository<CollectionRequest>();
            var capRepo = _uow.GetRepository<EnterpriseWasteCapability>();

            var entity = await repo.Entities
                .Include(x => x.WasteReportWaste)
                    .ThenInclude(w => w.WasteReport)
                .FirstOrDefaultAsync(x =>
                    x.Id == requestId &&
                    !x.IsDeleted &&
                    x.EnterpriseId == enterpriseId);

            if (entity == null || entity.Status != CollectionRequestStatus.Offered)
                return false;

            entity.Status = CollectionRequestStatus.Accepted;
            entity.LastUpdatedTime = DateTimeOffset.UtcNow;

            entity.WasteReportWaste.WasteReport.Status = WasteReportStatus.Accepted;
            entity.WasteReportWaste.WasteReport.LastUpdatedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);

            var capability = await capRepo.Entities.FirstOrDefaultAsync(x =>
                x.EnterpriseId == enterpriseId &&
                x.WasteTypeId == entity.WasteReportWaste.WasteTypeId &&
                !x.IsDeleted);

            if (capability != null)
            {
                capability.AssignedTodayKg += entity.WasteReportWaste.Quantity;
                capRepo.Update(capability);
            }

            await _uow.SaveAsync();

            // Notification
            var citizenId = entity.WasteReportWaste.WasteReport.CitizenId;

            await _notificationService.CreateAsync(
                citizenId,
                NotificationConstants.Types.COLLECTION_ACCEPTED,
                NotificationConstants.ReferenceTypes.COLLECTION_REQUEST,
                entity.Id
            );

            return true;
        }

        // =============================
        // REJECT
        // =============================
        public async Task<bool> RejectAsync(Guid enterpriseId, Guid requestId, RejectReason reason, string? note)
        {
            var repo = _uow.GetRepository<CollectionRequest>();
            var reportRepo = _uow.GetRepository<WasteReport>();

            var entity = await repo.Entities
                .Include(x => x.WasteReportWaste)
                    .ThenInclude(w => w.WasteReport)
                .FirstOrDefaultAsync(x =>
                    x.Id == requestId &&
                    !x.IsDeleted &&
                    x.EnterpriseId == enterpriseId);

            if (entity == null || entity.Status != CollectionRequestStatus.Offered)
                return false;

            entity.Status = CollectionRequestStatus.Rejected;
            entity.RejectReason = reason;
            entity.RejectNote = note;
            entity.LastUpdatedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _uow.SaveAsync();

            // Notification reject
            var citizenId = entity.WasteReportWaste.WasteReport.CitizenId;

            await _notificationService.CreateAsync(
                citizenId,
                NotificationConstants.Types.COLLECTION_REJECTED,
                NotificationConstants.ReferenceTypes.COLLECTION_REQUEST,
                entity.Id
            );

            var wasteItemId = entity.WasteReportWasteId;

            var rejectCount = await repo.NoTrackingEntities
                .CountAsync(x =>
                    x.WasteReportWasteId == wasteItemId &&
                    !x.IsDeleted &&
                    x.Status == CollectionRequestStatus.Rejected);

            if (rejectCount >= 3)
            {
                var report = entity.WasteReportWaste.WasteReport;

                report.Status = WasteReportStatus.NoEnterpriseAvailable;
                report.LastUpdatedTime = DateTimeOffset.UtcNow;

                reportRepo.Update(report);
                await _uow.SaveAsync();

                // Notification no enterprise
                await _notificationService.CreateAsync(
                    report.CitizenId,
                    NotificationConstants.Types.WASTE_REPORT_NO_ENTERPRISE,
                    NotificationConstants.ReferenceTypes.WASTE_REPORT,
                    report.Id
                );

                return true;
            }

            if (reason == RejectReason.WrongWasteType ||
                reason == RejectReason.ImageNotClear)
            {
                var report = entity.WasteReportWaste.WasteReport;

                report.Status = WasteReportStatus.Rejected;
                report.LastUpdatedTime = DateTimeOffset.UtcNow;

                reportRepo.Update(report);
                await _uow.SaveAsync();

                return true;
            }

            var nextEnterprise = await FindBestEnterpriseTop1Async(
                wasteItemId,
                entity.WasteReportWaste.WasteReport.RegionCode,
                entity.WasteReportWaste.WasteTypeId);

            if (nextEnterprise == null)
                return true;

            var newRequest = new CollectionRequest
            {
                WasteReportWasteId = wasteItemId,
                EnterpriseId = nextEnterprise.Value.enterpriseId,
                Status = CollectionRequestStatus.Offered,
                PriorityScore = (int)nextEnterprise.Value.capacity
            };

            await repo.InsertAsync(newRequest);
            await _uow.SaveAsync();

            return true;
        }

        // =============================
        // MAPPER
        // =============================
        private static CollectionRequestResponseDto MapToDto(CollectionRequest r)
        {
            var item = r.WasteReportWaste;
            var report = item.WasteReport;

            return new CollectionRequestResponseDto
            {
                Id = r.Id,
                WasteReportWasteId = r.WasteReportWasteId,
                WasteReportId = report.Id,
                EnterpriseId = r.EnterpriseId,
                Status = r.Status,
                PriorityScore = r.PriorityScore,

                RejectReason = r.RejectReason,
                RejectReasonName = r.RejectReason?.ToString(),
                RejectNote = r.RejectNote,

                WasteTypeId = item.WasteTypeId,
                WasteTypeName = item.WasteType?.Name,
                Note = item.Note,

                ImageUrls = item.Images
                    .Where(i => !i.IsDeleted)
                    .Select(i => i.ImageUrl)
                    .ToList(),

                Latitude = report.Latitude,
                Longitude = report.Longitude,
                RegionCode = report.RegionCode,

                HasAssignment = r.Assignments.Any(a => !a.IsDeleted),

                CreatedTime = r.CreatedTime,
                LastUpdatedTime = r.LastUpdatedTime
            };
        }
    }
}