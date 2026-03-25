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
    public class CitizenPointService : ICitizenPointService
    {
        private readonly IUnitOfWork _uow;

        public CitizenPointService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // ================= GET POINT =================
        public async Task<CitizenPointDto?> GetPointAsync(Guid citizenId)
        {
            var repo = _uow.GetRepository<CitizenPoint>();

            var point = await repo.Entities
                .Include(p => p.Citizen)
                .FirstOrDefaultAsync(p => p.CitizenId == citizenId);

            if (point == null) return null;

            return new CitizenPointDto
            {
                Id = point.Id,
                CitizenId = point.CitizenId,
                CitizenName = point.Citizen.FullName,
                TotalPoints = point.TotalPoints,
                CreatedTime = point.CreatedTime
            };
        }

        // ================= GET HISTORY =================
        public async Task<IEnumerable<CitizenPointHistoryDto>> GetHistoryAsync(Guid citizenId, int pageNumber = 1, int pageSize = 20)
        {
            var repo = _uow.GetRepository<CitizenPointHistory>();

            return await repo.Entities
                .Include(h => h.Citizen)
                .Where(h => h.CitizenId == citizenId)
                .OrderByDescending(h => h.CreatedTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new CitizenPointHistoryDto
                {
                    Id = h.Id,
                    CitizenId = h.CitizenId,
                    CitizenName = h.Citizen.FullName,
                    WasteReportId = h.WasteReportId,
                    Points = h.Points,
                    Reason = h.Reason,
                    Description = h.Description,
                    CreatedTime = h.CreatedTime
                })
                .ToListAsync();
        }

        // ================= LEADERBOARD (BY CITIZEN LOCATION) =================
        public async Task<IEnumerable<LeaderboardDto>> GetLeaderboardAsync(LeaderboardFilterDto filter)
        {
            var pointRepo = _uow.GetRepository<CitizenPoint>();
            var historyRepo = _uow.GetRepository<CitizenPointHistory>();

            // Time-based filtering
            if (filter.Period == LeaderboardPeriod.AllTime)
            {
                // Dùng TotalPoints từ CitizenPoint
                var query = pointRepo.Entities
                    .Include(p => p.Citizen)
                        .ThenInclude(c => c.Ward)
                    .Include(p => p.Citizen)
                        .ThenInclude(c => c.District)
                    .AsQueryable();

                // Filter by Ward
                if (filter.WardId.HasValue)
                {
                    query = query.Where(p => p.Citizen.WardId == filter.WardId.Value);
                }
                // Filter by District
                else if (filter.DistrictId.HasValue)
                {
                    query = query.Where(p => p.Citizen.DistrictId == filter.DistrictId.Value);
                }

                var points = await query
                    .OrderByDescending(p => p.TotalPoints)
                    .Take(filter.TopCount)
                    .ToListAsync();

                int rank = 1;
                return points.Select(p => new LeaderboardDto
                {
                    Rank = rank++,
                    CitizenId = p.CitizenId,
                    CitizenName = p.Citizen.FullName ?? "Unknown",
                    TotalPoints = p.TotalPoints,
                    WardId = p.Citizen.WardId,
                    WardName = p.Citizen.Ward?.Name,
                    DistrictId = p.Citizen.DistrictId,
                    DistrictName = p.Citizen.District?.Name
                });
            }
            else
            {
                // Tính điểm theo period từ history
                var (startDate, endDate) = GetDateRangeForPeriod(filter.Period);

                var pointsInPeriod = await historyRepo.Entities
                    .Where(h => h.CreatedTime >= startDate && h.CreatedTime <= endDate)
                    .GroupBy(h => h.CitizenId)
                    .Select(g => new
                    {
                        CitizenId = g.Key,
                        Points = g.Sum(h => h.Points)
                    })
                    .ToListAsync();

                var citizenIds = pointsInPeriod.Select(p => p.CitizenId).ToList();

                // Get citizen info
                var query = pointRepo.Entities
                    .Include(p => p.Citizen)
                        .ThenInclude(c => c.Ward)
                    .Include(p => p.Citizen)
                        .ThenInclude(c => c.District)
                    .Where(p => citizenIds.Contains(p.CitizenId))
                    .AsQueryable();

                // Filter by Ward
                if (filter.WardId.HasValue)
                {
                    query = query.Where(p => p.Citizen.WardId == filter.WardId.Value);
                }
                // Filter by District
                else if (filter.DistrictId.HasValue)
                {
                    query = query.Where(p => p.Citizen.DistrictId == filter.DistrictId.Value);
                }

                var citizens = await query.ToListAsync();

                // Join với points in period
                var leaderboard = citizens
                    .Select(c => new
                    {
                        Citizen = c,
                        Points = pointsInPeriod.FirstOrDefault(p => p.CitizenId == c.CitizenId)?.Points ?? 0
                    })
                    .OrderByDescending(x => x.Points)
                    .Take(filter.TopCount)
                    .ToList();

                int rank = 1;
                return leaderboard.Select(x => new LeaderboardDto
                {
                    Rank = rank++,
                    CitizenId = x.Citizen.CitizenId,
                    CitizenName = x.Citizen.Citizen.FullName ?? "Unknown",
                    TotalPoints = x.Points,
                    WardId = x.Citizen.Citizen.WardId,
                    WardName = x.Citizen.Citizen.Ward?.Name,
                    DistrictId = x.Citizen.Citizen.DistrictId,
                    DistrictName = x.Citizen.Citizen.District?.Name
                });
            }
        }

        // ================= GET MY RANK =================
        public async Task<MyRankDto> GetMyRankAsync(Guid citizenId, LeaderboardPeriod period = LeaderboardPeriod.AllTime)
        {
            var pointRepo = _uow.GetRepository<CitizenPoint>();
            var historyRepo = _uow.GetRepository<CitizenPointHistory>();

            var citizenPoint = await pointRepo.Entities
                .Include(p => p.Citizen)
                    .ThenInclude(c => c.Ward)
                        .ThenInclude(w => w.District)
                .Include(p => p.Citizen)
                    .ThenInclude(c => c.District)
                .FirstOrDefaultAsync(p => p.CitizenId == citizenId);

            if (citizenPoint == null)
            {
                throw new Exception($"CitizenPoint not found for citizenId: {citizenId}");
            }

            int myPoints;

            // Calculate points based on period
            if (period == LeaderboardPeriod.AllTime)
            {
                myPoints = citizenPoint.TotalPoints;
            }
            else
            {
                var (startDate, endDate) = GetDateRangeForPeriod(period);
                myPoints = await historyRepo.Entities
                    .Where(h => h.CitizenId == citizenId && h.CreatedTime >= startDate && h.CreatedTime <= endDate)
                    .SumAsync(h => h.Points);
            }

            // Global Rank
            var (globalRank, globalTotal) = await CalculateRankAsync(citizenId, myPoints, null, null, period);

            // Ward Rank
            int? wardRank = null;
            int wardTotal = 0;
            if (citizenPoint.Citizen.WardId.HasValue)
            {
                (wardRank, wardTotal) = await CalculateRankAsync(
                    citizenId, myPoints, citizenPoint.Citizen.WardId.Value, null, period);
            }

            // District Rank
            int? districtRank = null;
            int districtTotal = 0;
            if (citizenPoint.Citizen.DistrictId.HasValue)
            {
                (districtRank, districtTotal) = await CalculateRankAsync(
                    citizenId, myPoints, null, citizenPoint.Citizen.DistrictId.Value, period);
            }

            // Points to next rank
            var (pointsToNext, nextRankName) = await CalculatePointsToNextRankAsync(citizenId, myPoints, period);

            return new MyRankDto
            {
                CitizenId = citizenPoint.CitizenId,
                CitizenName = citizenPoint.Citizen.FullName ?? "Unknown",
                TotalPoints = myPoints,
                GlobalRank = globalRank,
                GlobalTotalUsers = globalTotal,
                WardRank = wardRank,
                WardTotalUsers = wardTotal,
                DistrictRank = districtRank,
                DistrictTotalUsers = districtTotal,
                WardId = citizenPoint.Citizen.WardId,
                WardName = citizenPoint.Citizen.Ward?.Name,
                DistrictId = citizenPoint.Citizen.DistrictId,
                DistrictName = citizenPoint.Citizen.District?.Name,
                PointsToNextRank = pointsToNext,
                NextRankCitizenName = nextRankName
            };
        }

        // ================= HELPER METHODS =================
        private (DateTimeOffset startDate, DateTimeOffset endDate) GetDateRangeForPeriod(LeaderboardPeriod period)
        {
            var now = DateTimeOffset.UtcNow;

            return period switch
            {
                LeaderboardPeriod.Daily => (now.Date, now),
                LeaderboardPeriod.Weekly => (now.AddDays(-7), now),
                LeaderboardPeriod.Monthly => (new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero), now),
                LeaderboardPeriod.Yearly => (new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero), now),
                _ => (DateTimeOffset.MinValue, now)
            };
        }

        private async Task<(int? rank, int total)> CalculateRankAsync(
            Guid citizenId,
            int myPoints,
            Guid? wardId,
            Guid? districtId,
            LeaderboardPeriod period)
        {
            var pointRepo = _uow.GetRepository<CitizenPoint>();
            var historyRepo = _uow.GetRepository<CitizenPointHistory>();

            if (period == LeaderboardPeriod.AllTime)
            {
                var query = pointRepo.Entities.Include(p => p.Citizen).AsQueryable();

                if (wardId.HasValue)
                    query = query.Where(p => p.Citizen.WardId == wardId.Value);
                else if (districtId.HasValue)
                    query = query.Where(p => p.Citizen.DistrictId == districtId.Value);

                var total = await query.CountAsync();
                var rank = await query.CountAsync(p => p.TotalPoints > myPoints) + 1;

                return (rank, total);
            }
            else
            {
                var (startDate, endDate) = GetDateRangeForPeriod(period);

                var pointsInPeriod = await historyRepo.Entities
                    .Where(h => h.CreatedTime >= startDate && h.CreatedTime <= endDate)
                    .GroupBy(h => h.CitizenId)
                    .Select(g => new
                    {
                        CitizenId = g.Key,
                        Points = g.Sum(h => h.Points)
                    })
                    .ToListAsync();

                var citizenQuery = pointRepo.Entities.Include(p => p.Citizen).AsQueryable();

                if (wardId.HasValue)
                    citizenQuery = citizenQuery.Where(p => p.Citizen.WardId == wardId.Value);
                else if (districtId.HasValue)
                    citizenQuery = citizenQuery.Where(p => p.Citizen.DistrictId == districtId.Value);

                var citizenIds = await citizenQuery.Select(p => p.CitizenId).ToListAsync();

                var filteredPoints = pointsInPeriod
                    .Where(p => citizenIds.Contains(p.CitizenId))
                    .ToList();

                var total = filteredPoints.Count;
                var rank = filteredPoints.Count(p => p.Points > myPoints) + 1;

                return (rank, total);
            }
        }

        private async Task<(int pointsToNext, string? nextRankName)> CalculatePointsToNextRankAsync(
            Guid citizenId,
            int myPoints,
            LeaderboardPeriod period)
        {
            var pointRepo = _uow.GetRepository<CitizenPoint>();
            var historyRepo = _uow.GetRepository<CitizenPointHistory>();

            if (period == LeaderboardPeriod.AllTime)
            {
                var nextPerson = await pointRepo.Entities
                    .Include(p => p.Citizen)
                    .Where(p => p.TotalPoints > myPoints && p.CitizenId != citizenId)
                    .OrderBy(p => p.TotalPoints)
                    .FirstOrDefaultAsync();

                if (nextPerson == null)
                    return (0, null);

                return (nextPerson.TotalPoints - myPoints, nextPerson.Citizen.FullName);
            }
            else
            {
                var (startDate, endDate) = GetDateRangeForPeriod(period);

                var pointsInPeriod = await historyRepo.Entities
                    .Where(h => h.CreatedTime >= startDate && h.CreatedTime <= endDate)
                    .GroupBy(h => h.CitizenId)
                    .Select(g => new
                    {
                        CitizenId = g.Key,
                        Points = g.Sum(h => h.Points)
                    })
                    .Where(p => p.Points > myPoints && p.CitizenId != citizenId)
                    .OrderBy(p => p.Points)
                    .FirstOrDefaultAsync();

                if (pointsInPeriod == null)
                    return (0, null);

                var nextCitizen = await pointRepo.Entities
                    .Include(p => p.Citizen)
                    .FirstOrDefaultAsync(p => p.CitizenId == pointsInPeriod.CitizenId);

                return (pointsInPeriod.Points - myPoints, nextCitizen?.Citizen.FullName);
            }
        }

        // ================= AWARD POINTS =================
        public async Task<CitizenPointDto> AwardPointsForVerifiedReportAsync(Guid wasteReportId)
        {
            var reportRepo = _uow.GetRepository<WasteReport>();
            var pointRepo = _uow.GetRepository<CitizenPoint>();
            var historyRepo = _uow.GetRepository<CitizenPointHistory>();
            var ruleRepo = _uow.GetRepository<PointRule>();

            var report = await reportRepo.Entities
                .Include(r => r.Citizen)
                .Include(r => r.Wastes)
                    .ThenInclude(w => w.WasteType)
                .FirstOrDefaultAsync(r => r.Id == wasteReportId);

            if (report == null)
                throw new Exception("WasteReport not found.");

            if (report.Status != WasteReportStatus.Verified)
                throw new Exception("WasteReport is not verified yet.");

            if (report.IsPointCalculated)
                throw new Exception("Points have already been calculated for this report.");

            if (report.Wastes == null || !report.Wastes.Any())
                throw new Exception("WasteReport does not contain any waste items.");

            var citizenPoint = await pointRepo.Entities
                .Include(p => p.Citizen)
                .FirstOrDefaultAsync(p => p.CitizenId == report.CitizenId);

            if (citizenPoint == null)
            {
                citizenPoint = new CitizenPoint
                {
                    CitizenId = report.CitizenId,
                    TotalPoints = 0
                };

                await pointRepo.InsertAsync(citizenPoint);
            }

            int totalPointsToAdd = 0;
            var rules = await ruleRepo.Entities
                .Where(r => r.IsActive)
                .ToListAsync();

            foreach (var waste in report.Wastes)
            {
                if (waste.Quantity <= 0)
                    continue;

                var rule = rules.FirstOrDefault(r => r.WasteTypeId == waste.WasteTypeId);
                if (rule == null)
                    continue;

                int itemPoints = rule.BasePoint * waste.Quantity;
                totalPointsToAdd += itemPoints;

                await historyRepo.InsertAsync(new CitizenPointHistory
                {
                    CitizenId = report.CitizenId,
                    WasteReportId = report.Id,
                    Points = itemPoints,
                    Reason = CitizenPointReason.WasteCollectionCompleted,
                    Description = $"{waste.WasteType.Name}: {rule.BasePoint} x {waste.Quantity} = {itemPoints}"
                });
            }

            if (totalPointsToAdd <= 0)
                throw new Exception("No valid point rule found for this report.");

            citizenPoint.TotalPoints += totalPointsToAdd;
            citizenPoint.LastUpdatedTime = DateTimeOffset.UtcNow;

            report.IsPointCalculated = true;
            report.PointCalculatedAt = DateTime.UtcNow;
            report.LastUpdatedTime = DateTimeOffset.UtcNow;

            await _uow.SaveAsync();

            return new CitizenPointDto
            {
                Id = citizenPoint.Id,
                CitizenId = citizenPoint.CitizenId,
                CitizenName = report.Citizen.FullName,
                TotalPoints = citizenPoint.TotalPoints,
                CreatedTime = citizenPoint.CreatedTime
            };
        }

        // ================= UPDATE POINTS =================
        public async Task<CitizenPointDto> UpdatePointAsync(Guid citizenId, UpdateCitizenPointRequest request)
        {
            var pointRepo = _uow.GetRepository<CitizenPoint>();
            var historyRepo = _uow.GetRepository<CitizenPointHistory>();

            var citizenPoint = await pointRepo.Entities
                .Include(p => p.Citizen)
                .FirstOrDefaultAsync(p => p.CitizenId == citizenId);

            if (citizenPoint == null)
                throw new Exception("CitizenPoint not found.");

            int oldPoints = citizenPoint.TotalPoints;
            int newPoints = request.TotalPoints;
            int diff = newPoints - oldPoints;

            citizenPoint.TotalPoints = newPoints;
            citizenPoint.LastUpdatedTime = DateTimeOffset.UtcNow;

            if (diff != 0)
            {
                await historyRepo.InsertAsync(new CitizenPointHistory
                {
                    CitizenId = citizenId,
                    WasteReportId = null,
                    Points = diff,
                    Reason = CitizenPointReason.ManualAdjustment,
                    Description = $"Manual adjustment: {oldPoints} -> {newPoints}"
                });
            }

            await _uow.SaveAsync();

            return new CitizenPointDto
            {
                Id = citizenPoint.Id,
                CitizenId = citizenPoint.CitizenId,
                CitizenName = citizenPoint.Citizen.FullName,
                TotalPoints = citizenPoint.TotalPoints,
                CreatedTime = citizenPoint.CreatedTime
            };
        }

        public async Task AwardPointsForResolvedComplaintAsync(Guid complaintId)
        {
            var complaintRepo = _uow.GetRepository<Complaint>();
            var pointRepo = _uow.GetRepository<CitizenPoint>();
            var historyRepo = _uow.GetRepository<CitizenPointHistory>();

            var complaint = await complaintRepo.GetByIdAsync(complaintId);
            if (complaint == null)
                throw new Exception("Complaint not found.");

            // Chống cộng điểm 2 lần
            var alreadyAwarded = await historyRepo.Entities
                .AnyAsync(h =>
                    h.CitizenId == complaint.ComplainantId &&
                    h.Reason == CitizenPointReason.ComplaintResolved &&
                    h.Description!.Contains(complaintId.ToString()));

            if (alreadyAwarded)
                return;

            var citizenPoint = await pointRepo.Entities
                .FirstOrDefaultAsync(p => p.CitizenId == complaint.ComplainantId);

            if (citizenPoint == null)
            {
                citizenPoint = new CitizenPoint
                {
                    CitizenId = complaint.ComplainantId,
                    TotalPoints = 0
                };
                await pointRepo.InsertAsync(citizenPoint);
            }

            citizenPoint.TotalPoints += 10;
            citizenPoint.LastUpdatedTime = DateTimeOffset.UtcNow;

            await historyRepo.InsertAsync(new CitizenPointHistory
            {
                CitizenId = complaint.ComplainantId,
                WasteReportId = null,
                Points = 10,
                Reason = CitizenPointReason.ComplaintResolved,
                Description = $"Complaint resolved: {complaintId}"
            });

            await _uow.SaveAsync();
        }
    }
}