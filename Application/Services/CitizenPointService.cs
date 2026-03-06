using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Domain.Entities;
using Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Core.Enum;

namespace Application.Services
{
    public class CitizenPointService : ICitizenPointService
    {
        private readonly ApplicationDbContext _db;

        public CitizenPointService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<CitizenPointDto?> GetPointAsync(Guid citizenId)
        {
            var point = await _db.CitizenPoints
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

        public async Task<IEnumerable<CitizenPointHistoryDto>> GetHistoryAsync(Guid citizenId, int pageNumber = 1, int pageSize = 20)
        {
            return await _db.CitizenPointHistories
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
                    CreatedTime = h.CreatedTime.DateTime
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaderboardDto>> GetLeaderboardAsync(int topCount = 10)
        {
            var points = await _db.CitizenPoints
                .Include(p => p.Citizen)
                .OrderByDescending(p => p.TotalPoints)
                .Take(topCount)
                .ToListAsync();

            int rank = 1;
            return points.Select(p => new LeaderboardDto
            {
                Rank = rank++,
                CitizenId = p.CitizenId,
                CitizenName = p.Citizen.FullName,
                TotalPoints = p.TotalPoints
            });
        }

        public async Task<CitizenPointDto> CalculatePointAsync(CalculateCitizenPointRequest request)
        {
            // Lấy WasteReport cùng citizen và wastes
            var report = await _db.WasteReports
                .Include(r => r.Citizen)
                .Include(r => r.Wastes)
                .ThenInclude(w => w.WasteType)
                .FirstOrDefaultAsync(r => r.Id == request.WasteReportId);

            if (report == null)
                throw new Exception("WasteReport not found");

            // Lấy hoặc tạo CitizenPoint
            var citizenPoint = await _db.CitizenPoints
                .FirstOrDefaultAsync(p => p.CitizenId == report.CitizenId);

            if (citizenPoint == null)
            {
                citizenPoint = new CitizenPoint
                {
                    CitizenId = report.CitizenId,
                    TotalPoints = 0
                };
                _db.CitizenPoints.Add(citizenPoint);
            }

            int totalPointsToAdd = 0;

            foreach (var waste in report.Wastes)
            {
                // Lấy point rule cho wasteType (chỉ active)
                var rule = await _db.PointRules
                    .FirstOrDefaultAsync(r => r.WasteTypeId == waste.WasteTypeId && r.IsActive);

                if (rule == null) continue;

                // Tính điểm
                int points = (int)Math.Round(rule.BasePoint * rule.QualityMultiplier) + rule.FastCollectionBonus;

                totalPointsToAdd += points;

                // Thêm history
                _db.CitizenPointHistories.Add(new CitizenPointHistory
                {
                    CitizenId = report.CitizenId,
                    WasteReportId = report.Id,
                    Points = points,
                    Reason = CitizenPointReason.ValidReport // hoặc tùy logic bạn muốn gán
                });
            }

            citizenPoint.TotalPoints += totalPointsToAdd;
            citizenPoint.LastUpdatedTime = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            return new CitizenPointDto
            {
                Id = citizenPoint.Id,
                CitizenId = citizenPoint.CitizenId,
                CitizenName = report.Citizen.FullName,
                TotalPoints = citizenPoint.TotalPoints,
                CreatedTime = citizenPoint.CreatedTime
            };
        }

        public async Task<CitizenPointDto> UpdatePointAsync(Guid citizenId, UpdateCitizenPointRequest request)
        {
            var citizenPoint = await _db.CitizenPoints
                .Include(p => p.Citizen)
                .FirstOrDefaultAsync(p => p.CitizenId == citizenId);

            if (citizenPoint == null)
                throw new Exception("CitizenPoint not found");

            citizenPoint.TotalPoints = request.TotalPoints;
            citizenPoint.LastUpdatedTime = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            return new CitizenPointDto
            {
                Id = citizenPoint.Id,
                CitizenId = citizenPoint.CitizenId,
                CitizenName = citizenPoint.Citizen.FullName,
                TotalPoints = citizenPoint.TotalPoints,
                CreatedTime = citizenPoint.CreatedTime
            };
        }
    }
}