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

        public async Task<IEnumerable<LeaderboardDto>> GetLeaderboardAsync(int topCount = 10)
        {
            var repo = _uow.GetRepository<CitizenPoint>();

            var points = await repo.Entities
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
    }
}