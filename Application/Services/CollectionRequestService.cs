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

        public CollectionRequestService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // =============================
        // SYSTEM: DISPATCH (MATCHING TOP1)
        // =============================
        public async Task CreateTop1RequestsForReportAsync(Guid wasteReportId)
        {
            var reportRepo = _uow.GetRepository<WasteReport>();
            var reqRepo = _uow.GetRepository<CollectionRequest>();

            // Load report + wastes (items)
            var report = await reportRepo.NoTrackingEntities
                .Include(r => r.Wastes)
                .FirstOrDefaultAsync(r => r.Id == wasteReportId && !r.IsDeleted);

            if (report == null)
                throw new Exception("WasteReport not found");

            if (string.IsNullOrWhiteSpace(report.RegionCode) || report.RegionCode == "UNKNOWN")
            {
                // MVP: nếu chưa resolve được region code thì bỏ qua dispatch
                // (sau này bạn có thể cho retry job)
                return;
            }

            // For each item -> create 1 offered request for best enterprise
            foreach (var item in report.Wastes)
            {
                // Idempotent: nếu item đã có request rồi thì skip
                var existed = await reqRepo.NoTrackingEntities.AnyAsync(x =>
                    x.WasteReportWasteId == item.Id && !x.IsDeleted);

                if (existed) continue;

                var bestEnterpriseId = await FindBestEnterpriseTop1Async(report.RegionCode, item.WasteTypeId);

                if (bestEnterpriseId == null)
                {
                    // MVP: không có enterprise phù hợp -> skip
                    continue;
                }

                var request = new CollectionRequest
                {
                    WasteReportWasteId = item.Id,
                    EnterpriseId = bestEnterpriseId.Value,
                    Status = CollectionRequestStatus.Offered,
                    PriorityScore = 100 // MVP: hardcode; sau này bạn tính theo distance/capacity/priority
                };

                await reqRepo.InsertAsync(request);
                await _uow.SaveAsync();
            }
        }

        /// <summary>
        /// TOP1 matching:
        /// 1) RegionCode match trước (EnterpriseServiceArea.RegionCode exact match MVP)
        /// 2) Capability match sau (EnterpriseWasteCapability.WasteTypeId)
        /// 3) Chọn top1 theo DailyCapacityKg (MVP)
        /// </summary>
        private async Task<Guid?> FindBestEnterpriseTop1Async(string regionCode, Guid wasteTypeId)
        {
            var enterpriseRepo = _uow.GetRepository<RecyclingEnterprise>();
            var areaRepo = _uow.GetRepository<EnterpriseServiceArea>();
            var capRepo = _uow.GetRepository<EnterpriseWasteCapability>();

            // Region filter FIRST
            var enterprisesInArea =
                from e in enterpriseRepo.NoTrackingEntities
                join a in areaRepo.NoTrackingEntities on e.Id equals a.EnterpriseId
                where !e.IsDeleted && !a.IsDeleted
                      && e.ApprovalStatus == EnterpriseApprovalStatus.Approved
                      && e.OperationalStatus == EnterpriseStatus.Active
                      && a.RegionCode == regionCode
                select e.Id;

            // Capability filter SECOND
            var candidates =
                from eid in enterprisesInArea
                join c in capRepo.NoTrackingEntities on eid equals c.EnterpriseId
                where !c.IsDeleted && c.WasteTypeId == wasteTypeId
                select new { EnterpriseId = eid, c.DailyCapacityKg };

            // Top1: pick highest capacity
            var best = await candidates
                .OrderByDescending(x => x.DailyCapacityKg)
                .Select(x => x.EnterpriseId)
                .FirstOrDefaultAsync();

            return best == Guid.Empty ? null : best;
        }

        // =============================
        // ENTERPRISE: INBOX
        // =============================
        public async Task<PagedCollectionRequestDto> GetPagedForEnterpriseAsync(Guid enterpriseId, CollectionRequestFilterDto filter)
        {
            var repo = _uow.GetRepository<CollectionRequest>();

            var query = repo.NoTrackingEntities
                .Include(r => r.WasteReportWaste)
                    .ThenInclude(w => w.WasteType)
                .Include(r => r.WasteReportWaste)
                    .ThenInclude(w => w.Images)
                .Include(r => r.WasteReportWaste)
                    .ThenInclude(w => w.WasteReport)
                .Where(r => !r.IsDeleted && r.EnterpriseId == enterpriseId);

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                Enum.TryParse<CollectionRequestStatus>(filter.Status, true, out var st))
            {
                query = query.Where(r => r.Status == st);
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
        // ENTERPRISE: ACTIONS
        // =============================
        public async Task<bool> AcceptAsync(Guid enterpriseId, Guid requestId)
        {
            var repo = _uow.GetRepository<CollectionRequest>();

            // 1) Get WasteReportWasteId (no tracking)
            var info = await repo.NoTrackingEntities
                .Where(x => !x.IsDeleted && x.Id == requestId && x.EnterpriseId == enterpriseId)
                .Select(x => new { x.Id, x.EnterpriseId, x.WasteReportWasteId })
                .FirstOrDefaultAsync();

            if (info == null) return false;

            // ✅ IMPORTANT: set đúng table name trong DB
            const string Table = "CollectionRequests";

            // 2) Atomic accept:
            // - Chỉ accept nếu request đang Offered
            // - Và chưa có request "win" (Accepted/Assigned/Completed) cho cùng WasteReportWasteId
            var sql = $@"
UPDATE {Table}
SET Status = {{0}}, LastUpdatedTime = {{1}}
WHERE Id = {{2}}
  AND EnterpriseId = {{3}}
  AND IsDeleted = 0
  AND Status = {{4}}
  AND NOT EXISTS (
      SELECT 1
      FROM {Table} cr
      WHERE cr.IsDeleted = 0
        AND cr.WasteReportWasteId = {{5}}
        AND cr.Status IN ({{6}}, {{7}}, {{8}})
  );";

            var now = DateTimeOffset.UtcNow;

            var rows = await _uow.ExecuteSqlRawAsync(
                sql,
                (int)CollectionRequestStatus.Accepted,
                now,
                info.Id,
                info.EnterpriseId,
                (int)CollectionRequestStatus.Offered,
                info.WasteReportWasteId,
                (int)CollectionRequestStatus.Accepted,
                (int)CollectionRequestStatus.Assigned,
                (int)CollectionRequestStatus.Completed
            );

            return rows > 0;
        }

        public async Task<bool> RejectAsync(Guid enterpriseId, Guid requestId, string? reason)
        {
            var repo = _uow.GetRepository<CollectionRequest>();

            var entity = await repo.Entities.FirstOrDefaultAsync(x =>
                x.Id == requestId && !x.IsDeleted && x.EnterpriseId == enterpriseId);

            if (entity == null) return false;

            if (entity.Status != CollectionRequestStatus.Offered)
                return false;

            entity.Status = CollectionRequestStatus.Rejected;
            entity.LastUpdatedTime = DateTimeOffset.UtcNow;

            // MVP: entity chưa có field Reason -> bỏ qua
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
                Status = r.Status.ToString(),
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

                CreatedTime = r.CreatedTime
            };
        }
    }
}