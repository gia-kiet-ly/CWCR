using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Core.Enum;
using Domain.Base;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class RewardService : IRewardService
    {
        private readonly IUnitOfWork _uow;

        public RewardService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<RewardDto>> GetActiveRewardsAsync()
        {
            var repo = _uow.GetRepository<Reward>();

            var list = await repo.NoTrackingEntities
                .Where(x => x.IsActive && !x.IsDeleted && x.Stock > 0)
                .OrderBy(x => x.PointCost)
                .ThenByDescending(x => x.CreatedTime)
                .Select(x => new RewardDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    ImageUrl = x.ImageUrl,
                    PointCost = x.PointCost,
                    Stock = x.Stock,
                    IsActive = x.IsActive,
                    CreatedTime = x.CreatedTime
                })
                .ToListAsync();

            return list;
        }

        public async Task<RewardDto> CreateAsync(CreateRewardDto dto)
        {
            var repo = _uow.GetRepository<Reward>();

            var entity = new Reward
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                PointCost = dto.PointCost,
                Stock = dto.Stock,
                IsActive = true
            };

            await repo.InsertAsync(entity);
            await _uow.SaveAsync();

            return new RewardDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                ImageUrl = entity.ImageUrl,
                PointCost = entity.PointCost,
                Stock = entity.Stock,
                IsActive = entity.IsActive,
                CreatedTime = entity.CreatedTime
            };
        }

        public async Task<RewardRedemptionDto> RedeemAsync(Guid citizenId, Guid rewardId)
        {
            if (citizenId == Guid.Empty)
                throw new BaseException.BadRequestException("invalid_request", "CitizenId is required.");

            if (rewardId == Guid.Empty)
                throw new BaseException.BadRequestException("invalid_request", "RewardId is required.");

            var rewardRepo = _uow.GetRepository<Reward>();
            var pointRepo = _uow.GetRepository<CitizenPoint>();
            var historyRepo = _uow.GetRepository<CitizenPointHistory>();
            var redemptionRepo = _uow.GetRepository<RewardRedemption>();

            await _uow.BeginTransactionAsync();
            try
            {
                // 1) Lấy reward trước để biết PointCost
                var reward = await rewardRepo.Entities
                    .FirstOrDefaultAsync(x => x.Id == rewardId && !x.IsDeleted);

                if (reward == null || !reward.IsActive)
                    throw new BaseException.NotFoundException("reward_not_found", "Reward not found or inactive.");

                var cost = reward.PointCost;

                // 2) Trừ stock ATOMIC bằng SQL để chống 2 người đổi cùng lúc
                // Nếu rowsAffected = 0 => hết stock hoặc inactive
                var stockRows = await _uow.ExecuteSqlRawAsync(
                    "UPDATE Rewards SET Stock = Stock - 1 WHERE Id = {0} AND IsDeleted = 0 AND IsActive = 1 AND Stock > 0",
                    rewardId);

                if (stockRows <= 0)
                    throw new BaseException.BadRequestException("out_of_stock", "Reward is out of stock.");

                // 3) Lấy / tạo CitizenPoint
                var citizenPoint = await pointRepo.Entities
                    .FirstOrDefaultAsync(x => x.CitizenId == citizenId);

                if (citizenPoint == null)
                {
                    citizenPoint = new CitizenPoint
                    {
                        CitizenId = citizenId,
                        TotalPoints = 0
                    };
                    await pointRepo.InsertAsync(citizenPoint);
                    await _uow.SaveAsync(); // để có record trước khi SQL update (optional)
                }

                // 4) Trừ điểm ATOMIC: chỉ trừ nếu đủ điểm
                var pointRows = await _uow.ExecuteSqlRawAsync(
                    "UPDATE CitizenPoints SET TotalPoints = TotalPoints - {0}, LastUpdatedTime = SYSUTCDATETIME() WHERE CitizenId = {1} AND TotalPoints >= {0}",
                    cost, citizenId);

                if (pointRows <= 0)
                {
                    // hoàn stock lại (vì đã -1 stock ở trên)
                    await _uow.ExecuteSqlRawAsync(
                        "UPDATE Rewards SET Stock = Stock + 1 WHERE Id = {0}",
                        rewardId);

                    throw new BaseException.BadRequestException("not_enough_points", "Not enough points to redeem this reward.");
                }

                // 5) Lưu history (âm điểm) + redemption record (lịch sử đổi)
                await historyRepo.InsertAsync(new CitizenPointHistory
                {
                    CitizenId = citizenId,
                    WasteReportId = null,
                    Points = -cost,
                    Reason = CitizenPointReason.RewardRedeemed,
                    Description = $"Redeemed reward: {reward.Name} (-{cost})"
                });

                var redemption = new RewardRedemption
                {
                    CitizenId = citizenId,
                    RewardId = rewardId,
                    PointCostSnapshot = cost
                };

                await redemptionRepo.InsertAsync(redemption);

                await _uow.SaveAsync();
                await _uow.CommitTransactionAsync();

                return new RewardRedemptionDto
                {
                    Id = redemption.Id,
                    RewardId = reward.Id,
                    RewardName = reward.Name,
                    PointCostSnapshot = cost,
                    CreatedTime = redemption.CreatedTime
                };
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<List<RewardRedemptionDto>> GetMyRedemptionsAsync(Guid citizenId, int pageNumber = 1, int pageSize = 20)
        {
            var repo = _uow.GetRepository<RewardRedemption>();

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var list = await repo.NoTrackingEntities
                .Where(x => x.CitizenId == citizenId && !x.IsDeleted)
                .Include(x => x.Reward)
                .OrderByDescending(x => x.CreatedTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RewardRedemptionDto
                {
                    Id = x.Id,
                    RewardId = x.RewardId,
                    RewardName = x.Reward.Name,
                    PointCostSnapshot = x.PointCostSnapshot,
                    CreatedTime = x.CreatedTime
                })
                .ToListAsync();

            return list;
        }

        public async Task<List<RewardDto>> GetAllAdminAsync()
        {
            var repo = _uow.GetRepository<Reward>();

            return await repo.NoTrackingEntities
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedTime)
                .Select(x => new RewardDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    ImageUrl = x.ImageUrl,
                    PointCost = x.PointCost,
                    Stock = x.Stock,
                    IsActive = x.IsActive,
                    CreatedTime = x.CreatedTime
                })
                .ToListAsync();
        }

        public async Task<RewardDto?> GetByIdAdminAsync(Guid id)
        {
            var repo = _uow.GetRepository<Reward>();

            var x = await repo.NoTrackingEntities
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (x == null) return null;

            return new RewardDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                PointCost = x.PointCost,
                Stock = x.Stock,
                IsActive = x.IsActive,
                CreatedTime = x.CreatedTime
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateRewardDto dto)
        {
            var repo = _uow.GetRepository<Reward>();
            var entity = await repo.Entities.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (entity == null) return false;

            if (!string.IsNullOrWhiteSpace(dto.Name))
                entity.Name = dto.Name.Trim();

            if (dto.Description != null)
                entity.Description = dto.Description;

            if (dto.ImageUrl != null)
                entity.ImageUrl = dto.ImageUrl;

            if (dto.PointCost.HasValue)
                entity.PointCost = dto.PointCost.Value;

            if (dto.Stock.HasValue)
                entity.Stock = dto.Stock.Value;

            if (dto.IsActive.HasValue)
                entity.IsActive = dto.IsActive.Value;

            entity.LastUpdatedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _uow.SaveAsync();
            return true;
        }

        public async Task<bool> AdjustStockAsync(Guid id, int delta)
        {
            // delta có thể âm/dương
            // atomic: chỉ update nếu stock sau update >= 0
            var rows = await _uow.ExecuteSqlRawAsync(
                "UPDATE Rewards SET Stock = Stock + {0}, LastUpdatedTime = SYSUTCDATETIME() WHERE Id = {1} AND IsDeleted = 0 AND (Stock + {0}) >= 0",
                delta, id);

            return rows > 0;
        }
    }
}