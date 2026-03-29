using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Core.Enum;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public DashboardService(
            IUnitOfWork uow,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _uow = uow;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ======================== ADMIN ========================
        public async Task<AdminDashboardDto> GetAdminDashboardAsync()
        {
            var dto = new AdminDashboardDto();

            var wasteReportRepo = _uow.GetRepository<WasteReport>();
            var requestRepo = _uow.GetRepository<CollectionRequest>();
            var assignmentRepo = _uow.GetRepository<CollectorAssignment>();
            var proofRepo = _uow.GetRepository<CollectionProof>();
            var enterpriseRepo = _uow.GetRepository<RecyclingEnterprise>();
            var pointHistoryRepo = _uow.GetRepository<CitizenPointHistory>();

            dto.Summary.TotalUsers = await _userManager.Users
                .CountAsync(x => !x.IsDeleted);

            dto.Summary.TotalWasteReports = await wasteReportRepo.Entities
                .CountAsync(x => !x.IsDeleted);

            dto.Summary.TotalCollectionRequests = await requestRepo.Entities
                .CountAsync(x => !x.IsDeleted);

            dto.Summary.TotalCompletedAssignments = await assignmentRepo.Entities
                .CountAsync(x => !x.IsDeleted && x.Status == AssignmentStatus.Completed);

            dto.Summary.PendingEnterpriseApprovals = await enterpriseRepo.Entities
                .CountAsync(x => !x.IsDeleted && x.ApprovalStatus == EnterpriseApprovalStatus.PendingApproval);

            dto.Summary.PendingProofReviews = await proofRepo.Entities
                .CountAsync(x => !x.IsDeleted && x.ReviewStatus == ProofReviewStatus.Pending);

            dto.Summary.TotalRewardedPoints = await pointHistoryRepo.Entities
                .Where(x => !x.IsDeleted)
                .SumAsync(x => (decimal?)x.Points) ?? 0;

            var roles = await _roleManager.Roles.ToListAsync();
            var users = await _userManager.Users
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            foreach (var role in roles)
            {
                var count = 0;
                foreach (var user in users)
                {
                    if (await _userManager.IsInRoleAsync(user, role.Name!))
                        count++;
                }
                dto.UsersByRole.Add(new DashboardChartItemDto
                {
                    Label = role.Name!,
                    Count = count
                });
            }

            dto.Summary.TotalCitizens = dto.UsersByRole.FirstOrDefault(x => x.Label == "Citizen")?.Count ?? 0;
            dto.Summary.TotalEnterprises = dto.UsersByRole.FirstOrDefault(x => x.Label == "Enterprise")?.Count ?? 0;
            dto.Summary.TotalCollectors = dto.UsersByRole.FirstOrDefault(x => x.Label == "Collector")?.Count ?? 0;

            dto.ReportsByMonth = await wasteReportRepo.Entities
                .Where(x => !x.IsDeleted)
                .GroupBy(x => new { x.CreatedTime.Year, x.CreatedTime.Month })
                .Select(g => new DashboardChartItemDto
                {
                    Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Count = g.Count()
                })
                .OrderBy(x => x.Label)
                .ToListAsync();

            dto.RequestsByStatus = await requestRepo.Entities
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.Status)
                .Select(g => new DashboardChartItemDto
                {
                    Label = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            dto.EnterprisesByStatus = await enterpriseRepo.Entities
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.ApprovalStatus)
                .Select(g => new DashboardChartItemDto
                {
                    Label = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            return dto;
        }

        // ======================== CITIZEN ========================
        public async Task<CitizenDashboardDto> GetCitizenDashboardAsync(Guid citizenUserId)
        {
            var dto = new CitizenDashboardDto();

            var wasteReportRepo = _uow.GetRepository<WasteReport>();
            var wasteReportWasteRepo = _uow.GetRepository<WasteReportWaste>();
            var citizenPointRepo = _uow.GetRepository<CitizenPoint>();
            var pointHistoryRepo = _uow.GetRepository<CitizenPointHistory>();

            dto.Summary.MyTotalReports = await wasteReportRepo.Entities
                .CountAsync(x => !x.IsDeleted && x.CitizenId == citizenUserId);

            dto.Summary.MyPendingReports = await wasteReportRepo.Entities
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.CitizenId == citizenUserId &&
                    x.Status == WasteReportStatus.Pending);

            dto.Summary.MyCollectedReports = await wasteReportRepo.Entities
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.CitizenId == citizenUserId &&
                    (x.Status == WasteReportStatus.Collected ||
                     x.Status == WasteReportStatus.Verified));

            dto.Summary.MyCurrentPoints = await citizenPointRepo.Entities
                .Where(x => !x.IsDeleted && x.CitizenId == citizenUserId)
                .Select(x => (decimal?)x.TotalPoints)
                .FirstOrDefaultAsync() ?? 0;

            var now = DateTimeOffset.UtcNow;
            dto.Summary.MyPointsThisMonth = await pointHistoryRepo.Entities
                .Where(x =>
                    !x.IsDeleted &&
                    x.CitizenId == citizenUserId &&
                    x.CreatedTime.Year == now.Year &&
                    x.CreatedTime.Month == now.Month)
                .SumAsync(x => (decimal?)x.Points) ?? 0;

            dto.ReportsByMonth = await wasteReportRepo.Entities
                .Where(x => !x.IsDeleted && x.CitizenId == citizenUserId)
                .GroupBy(x => new { x.CreatedTime.Year, x.CreatedTime.Month })
                .Select(g => new DashboardChartItemDto
                {
                    Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Count = g.Count()
                })
                .OrderBy(x => x.Label)
                .ToListAsync();

            dto.ReportsByStatus = await wasteReportRepo.Entities
                .Where(x => !x.IsDeleted && x.CitizenId == citizenUserId)
                .GroupBy(x => x.Status)
                .Select(g => new DashboardChartItemDto
                {
                    Label = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            dto.PointsByMonth = await pointHistoryRepo.Entities
                .Where(x => !x.IsDeleted && x.CitizenId == citizenUserId)
                .GroupBy(x => new { x.CreatedTime.Year, x.CreatedTime.Month })
                .Select(g => new DashboardValueItemDto
                {
                    Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Value = g.Sum(x => x.Points)
                })
                .OrderBy(x => x.Label)
                .ToListAsync();

            dto.ReportsByWasteType = await wasteReportWasteRepo.Entities
                .Include(x => x.WasteType)
                .Include(x => x.WasteReport)
                .Where(x =>
                    !x.IsDeleted &&
                    !x.WasteReport.IsDeleted &&
                    x.WasteReport.CitizenId == citizenUserId)
                .GroupBy(x => x.WasteType.Name)
                .Select(g => new DashboardChartItemDto
                {
                    Label = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return dto;
        }

        // ======================== ENTERPRISE ========================
        public async Task<EnterpriseDashboardDto> GetEnterpriseDashboardAsync(Guid enterpriseUserId)
        {
            var dto = new EnterpriseDashboardDto();

            var enterpriseRepo = _uow.GetRepository<RecyclingEnterprise>();
            var requestRepo = _uow.GetRepository<CollectionRequest>();
            var proofRepo = _uow.GetRepository<CollectionProof>();
            var capRepo = _uow.GetRepository<EnterpriseWasteCapability>();

            var enterprise = await enterpriseRepo.Entities
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.UserId == enterpriseUserId);

            if (enterprise == null)
                return dto;

            var enterpriseId = enterprise.Id;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var todayStart = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
            var todayEnd = todayStart.AddDays(1);

            // ======= Summary cũ =======
            dto.Summary.TotalRequestsReceived = await requestRepo.Entities
                .CountAsync(x => !x.IsDeleted && x.EnterpriseId == enterpriseId);

            dto.Summary.PendingRequests = await requestRepo.Entities
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.EnterpriseId == enterpriseId &&
                    x.Status == CollectionRequestStatus.Offered);

            dto.Summary.CompletedRequests = await requestRepo.Entities
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.EnterpriseId == enterpriseId &&
                    x.Status == CollectionRequestStatus.Completed);

            dto.Summary.PendingProofReviews = await proofRepo.Entities
                .Include(x => x.Assignment)
                    .ThenInclude(a => a.Request)
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.Assignment.Request.EnterpriseId == enterpriseId &&
                    x.ReviewStatus == ProofReviewStatus.Pending);

            dto.Summary.ApprovedProofs = await proofRepo.Entities
                .Include(x => x.Assignment)
                    .ThenInclude(a => a.Request)
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.Assignment.Request.EnterpriseId == enterpriseId &&
                    x.ReviewStatus == ProofReviewStatus.Approved);

            dto.Summary.RejectedProofs = await proofRepo.Entities
                .Include(x => x.Assignment)
                    .ThenInclude(a => a.Request)
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.Assignment.Request.EnterpriseId == enterpriseId &&
                    x.ReviewStatus == ProofReviewStatus.Rejected);

            dto.Summary.CompletionRate =
                dto.Summary.TotalRequestsReceived == 0
                    ? 0
                    : Math.Round(
                        (decimal)dto.Summary.CompletedRequests * 100 / dto.Summary.TotalRequestsReceived,
                        2);

            // ======= 🆕 Capa hôm nay (từ EnterpriseWasteCapability) =======
            var capabilities = await capRepo.Entities
                .Include(x => x.WasteType)
                .Where(x => !x.IsDeleted && x.EnterpriseId == enterpriseId)
                .ToListAsync();

            dto.Summary.TodayTotalCapacity = capabilities.Sum(x => x.DailyCapacityKg);

            // Nếu LastResetDate != hôm nay → AssignedTodayCount thực tế = 0
            dto.Summary.TodayAssignedCount = capabilities
                .Sum(x => x.LastResetDate == today ? x.AssignedTodayCount : 0);

            dto.Summary.TodayRemainingCapacity =
                dto.Summary.TodayTotalCapacity - dto.Summary.TodayAssignedCount;

            // CapacityByWasteType chart (bar chart: mỗi loại rác có Daily vs Assigned)
            dto.CapacityByWasteType = capabilities.Select(x => new CapacityByWasteTypeDto
            {
                WasteTypeName = x.WasteType?.Name ?? string.Empty,
                DailyCapacity = x.DailyCapacityKg,
                AssignedToday = x.LastResetDate == today ? x.AssignedTodayCount : 0,
                Remaining = x.DailyCapacityKg - (x.LastResetDate == today ? x.AssignedTodayCount : 0)
            }).ToList();

            // ======= 🆕 Rác đã thu thực tế (từ CollectionProof Approved) =======

            // Hôm nay — SUM Quantity của WasteReportWaste thuộc proof approved hôm nay
            dto.Summary.TodayCollectedQuantity = await proofRepo.Entities
                .Include(x => x.Assignment)
                    .ThenInclude(a => a.Request)
                        .ThenInclude(r => r.WasteReportWaste)
                .Where(x =>
                    !x.IsDeleted &&
                    x.Assignment.Request.EnterpriseId == enterpriseId &&
                    x.ReviewStatus == ProofReviewStatus.Approved &&
                    x.ReviewedAt >= todayStart &&
                    x.ReviewedAt < todayEnd)
                .SumAsync(x => (decimal?)x.Assignment.Request.WasteReportWaste.Quantity) ?? 0;

            // All time
            dto.Summary.TotalCollectedQuantityAllTime = await proofRepo.Entities
                .Include(x => x.Assignment)
                    .ThenInclude(a => a.Request)
                        .ThenInclude(r => r.WasteReportWaste)
                .Where(x =>
                    !x.IsDeleted &&
                    x.Assignment.Request.EnterpriseId == enterpriseId &&
                    x.ReviewStatus == ProofReviewStatus.Approved)
                .SumAsync(x => (decimal?)x.Assignment.Request.WasteReportWaste.Quantity) ?? 0;

            // Số lượng thu gom theo tháng (chart trend)
            dto.CollectedQuantityByMonth = await proofRepo.Entities
                .Include(x => x.Assignment)
                    .ThenInclude(a => a.Request)
                        .ThenInclude(r => r.WasteReportWaste)
                .Where(x =>
                    !x.IsDeleted &&
                    x.Assignment.Request.EnterpriseId == enterpriseId &&
                    x.ReviewStatus == ProofReviewStatus.Approved &&
                    x.ReviewedAt.HasValue)
                .GroupBy(x => new { x.ReviewedAt!.Value.Year, x.ReviewedAt.Value.Month })
                .Select(g => new DashboardValueItemDto
                {
                    Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Value = g.Sum(x => (decimal)x.Assignment.Request.WasteReportWaste.Quantity)
                })
                .OrderBy(x => x.Label)
                .ToListAsync();

            // Số lượng thu gom theo WasteType (chart)
            dto.CollectedQuantityByWasteType = await proofRepo.Entities
                .Include(x => x.Assignment)
                    .ThenInclude(a => a.Request)
                        .ThenInclude(r => r.WasteReportWaste)
                            .ThenInclude(w => w.WasteType)
                .Where(x =>
                    !x.IsDeleted &&
                    x.Assignment.Request.EnterpriseId == enterpriseId &&
                    x.ReviewStatus == ProofReviewStatus.Approved)
                .GroupBy(x => x.Assignment.Request.WasteReportWaste.WasteType.Name)
                .Select(g => new DashboardValueItemDto
                {
                    Label = g.Key,
                    Value = g.Sum(x => (decimal)x.Assignment.Request.WasteReportWaste.Quantity)
                })
                .OrderByDescending(x => x.Value)
                .ToListAsync();

            // ======= Charts cũ =======
            dto.RequestsByMonth = await requestRepo.Entities
                .Where(x => !x.IsDeleted && x.EnterpriseId == enterpriseId)
                .GroupBy(x => new { x.CreatedTime.Year, x.CreatedTime.Month })
                .Select(g => new DashboardChartItemDto
                {
                    Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Count = g.Count()
                })
                .OrderBy(x => x.Label)
                .ToListAsync();

            dto.RequestsByStatus = await requestRepo.Entities
                .Where(x => !x.IsDeleted && x.EnterpriseId == enterpriseId)
                .GroupBy(x => x.Status)
                .Select(g => new DashboardChartItemDto
                {
                    Label = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            dto.ProofsByReviewStatus = await proofRepo.Entities
                .Include(x => x.Assignment)
                    .ThenInclude(a => a.Request)
                .Where(x =>
                    !x.IsDeleted &&
                    x.Assignment.Request.EnterpriseId == enterpriseId)
                .GroupBy(x => x.ReviewStatus)
                .Select(g => new DashboardChartItemDto
                {
                    Label = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            dto.RequestsByWasteType = await requestRepo.Entities
                .Include(x => x.WasteReportWaste)
                    .ThenInclude(w => w.WasteType)
                .Where(x => !x.IsDeleted && x.EnterpriseId == enterpriseId)
                .GroupBy(x => x.WasteReportWaste.WasteType.Name)
                .Select(g => new DashboardChartItemDto
                {
                    Label = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return dto;
        }

        // ======================== COLLECTOR ========================
        public async Task<CollectorDashboardDto> GetCollectorDashboardAsync(Guid collectorUserId)
        {
            var dto = new CollectorDashboardDto();

            var assignmentRepo = _uow.GetRepository<CollectorAssignment>();
            var proofRepo = _uow.GetRepository<CollectionProof>();

            dto.Summary.MyTotalAssignments = await assignmentRepo.Entities
                .CountAsync(x => !x.IsDeleted && x.CollectorId == collectorUserId);

            dto.Summary.MyActiveAssignments = await assignmentRepo.Entities
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.CollectorId == collectorUserId &&
                    (x.Status == AssignmentStatus.Assigned ||
                     x.Status == AssignmentStatus.OnTheWay ||
                     x.Status == AssignmentStatus.Collected));

            dto.Summary.MyCompletedAssignments = await assignmentRepo.Entities
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.CollectorId == collectorUserId &&
                    x.Status == AssignmentStatus.Completed);

            dto.Summary.MyPendingProofReviews = await proofRepo.Entities
                .Include(x => x.Assignment)
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.Assignment.CollectorId == collectorUserId &&
                    x.ReviewStatus == ProofReviewStatus.Pending);

            dto.Summary.MyRejectedProofs = await proofRepo.Entities
                .Include(x => x.Assignment)
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.Assignment.CollectorId == collectorUserId &&
                    x.ReviewStatus == ProofReviewStatus.Rejected);

            dto.Summary.MyCompletionRate =
                dto.Summary.MyTotalAssignments == 0
                    ? 0
                    : Math.Round(
                        (decimal)dto.Summary.MyCompletedAssignments * 100 / dto.Summary.MyTotalAssignments,
                        2);

            dto.AssignmentsByMonth = await assignmentRepo.Entities
                .Where(x => !x.IsDeleted && x.CollectorId == collectorUserId)
                .GroupBy(x => new { x.CreatedTime.Year, x.CreatedTime.Month })
                .Select(g => new DashboardChartItemDto
                {
                    Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Count = g.Count()
                })
                .OrderBy(x => x.Label)
                .ToListAsync();

            dto.AssignmentsByStatus = await assignmentRepo.Entities
                .Where(x => !x.IsDeleted && x.CollectorId == collectorUserId)
                .GroupBy(x => x.Status)
                .Select(g => new DashboardChartItemDto
                {
                    Label = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            dto.ProofsByReviewStatus = await proofRepo.Entities
                .Include(x => x.Assignment)
                .Where(x =>
                    !x.IsDeleted &&
                    x.Assignment.CollectorId == collectorUserId)
                .GroupBy(x => x.ReviewStatus)
                .Select(g => new DashboardChartItemDto
                {
                    Label = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            dto.AssignmentsByRegion = await assignmentRepo.Entities
                .Include(x => x.Request)
                    .ThenInclude(r => r.WasteReportWaste)
                        .ThenInclude(w => w.WasteReport)
                .Where(x => !x.IsDeleted && x.CollectorId == collectorUserId)
                .GroupBy(x => x.Request.WasteReportWaste.WasteReport.RegionCode)
                .Select(g => new DashboardChartItemDto
                {
                    Label = g.Key ?? "Unknown",
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return dto;
        }
    }
}