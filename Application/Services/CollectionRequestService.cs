using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Core.Enum;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Application.Services
{
    public class CollectionRequestService : ICollectionRequestService
    {
        private readonly IUnitOfWork _uow;

        public CollectionRequestService(IUnitOfWork uow)
        {
            _uow = uow;
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

                var bestEnterpriseId = await FindBestEnterpriseTop1Async(
                    report.RegionCode,
                    item.WasteTypeId);

                if (bestEnterpriseId == null)
                    continue;

                var request = new CollectionRequest
                {
                    WasteReportWasteId = item.Id,
                    EnterpriseId = bestEnterpriseId.Value,
                    Status = CollectionRequestStatus.Offered,
                    PriorityScore = 100
                };

                await reqRepo.InsertAsync(request);
            }

            await _uow.SaveAsync();
        }

        /// <summary>
        /// Dispatch logic:
        /// RegionCode contains District.Code OR Ward.Code
        /// </summary>
        private async Task<Guid?> FindBestEnterpriseTop1Async(
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

            // match district
            var districtIds = await districtRepo.NoTrackingEntities
                .Where(x => !x.IsDeleted &&
                    EF.Functions.Like(regionCode, "%" + x.Code + "%"))
                .Select(x => x.Id)
                .ToListAsync();

            // match ward
            var wardIds = await wardRepo.NoTrackingEntities
                .Where(x => !x.IsDeleted &&
                    EF.Functions.Like(regionCode, "%" + x.Code + "%"))
                .Select(x => x.Id)
                .ToListAsync();

            if (!districtIds.Any() && !wardIds.Any())
                return null;

            // enterprise trong khu vực
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

            // enterprise có khả năng xử lý waste type
            var candidates =
                from eid in enterprisesInArea.Distinct()
                join c in capRepo.NoTrackingEntities
                    on eid equals c.EnterpriseId
                where !c.IsDeleted && c.WasteTypeId == wasteTypeId
                select new
                {
                    EnterpriseId = eid,
                    c.DailyCapacityKg
                };

            var bestEnterpriseId = await candidates
                .OrderByDescending(x => x.DailyCapacityKg)
                .Select(x => x.EnterpriseId)
                .FirstOrDefaultAsync();

            if (bestEnterpriseId == Guid.Empty)
                return null;

            return bestEnterpriseId;
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
            {
                query = query.Where(r => r.Status == filter.Status.Value);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedTime)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedCollectionRequestDto
            {
                TotalCount = total,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = items.Select(MapToDto).ToList()
            };
        }

        // =============================
        // ACCEPT
        // =============================
        public async Task<bool> AcceptAsync(Guid enterpriseId, Guid requestId)
        {
            var repo = _uow.GetRepository<CollectionRequest>();

            var info = await repo.NoTrackingEntities
                .Where(x => !x.IsDeleted && x.Id == requestId && x.EnterpriseId == enterpriseId)
                .Select(x => new
                {
                    x.Id,
                    x.EnterpriseId,
                    x.WasteReportWasteId,
                    x.Status
                })
                .FirstOrDefaultAsync();

            if (info == null || info.Status != CollectionRequestStatus.Offered)
                return false;

            const string Table = "CollectionRequests";

            var sql = $@"
UPDATE {Table}
SET Status = {{0}}, LastUpdatedTime = {{1}}
WHERE Id = {{2}}
AND EnterpriseId = {{3}}
AND IsDeleted = 0
AND Status = {{4}}
AND NOT EXISTS (
SELECT 1
FROM {Table}
WHERE WasteReportWasteId = {{5}}
AND IsDeleted = 0
AND Status IN ({{6}}, {{7}}, {{8}})
);";

            var rows = await _uow.ExecuteSqlRawAsync(
                sql,
                CollectionRequestStatus.Accepted.ToString(),
                DateTimeOffset.UtcNow,
                info.Id,
                info.EnterpriseId,
                CollectionRequestStatus.Offered.ToString(),
                info.WasteReportWasteId,
                CollectionRequestStatus.Accepted.ToString(),
                CollectionRequestStatus.Assigned.ToString(),
                CollectionRequestStatus.Completed.ToString()
            );

            return rows > 0;
        }

        // =============================
        // REJECT
        // =============================
        public async Task<bool> RejectAsync(Guid enterpriseId, Guid requestId, string? reason)
        {
            var repo = _uow.GetRepository<CollectionRequest>();

            var entity = await repo.Entities.FirstOrDefaultAsync(x =>
                x.Id == requestId &&
                !x.IsDeleted &&
                x.EnterpriseId == enterpriseId);

            if (entity == null || entity.Status != CollectionRequestStatus.Offered)
                return false;

            entity.Status = CollectionRequestStatus.Rejected;
            entity.LastUpdatedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
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