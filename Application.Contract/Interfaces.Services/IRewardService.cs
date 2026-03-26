using Application.Contract.DTOs;

namespace Application.Contract.Interfaces.Services
{
    public interface IRewardService
    {
        // citizen
        Task<List<RewardDto>> GetActiveRewardsAsync();
        Task<RewardRedemptionDto> RedeemAsync(Guid citizenId, Guid rewardId);
        Task<List<RewardRedemptionDto>> GetMyRedemptionsAsync(Guid citizenId, int pageNumber = 1, int pageSize = 20);

        // admin
        Task<RewardDto> CreateAsync(CreateRewardDto dto);
        Task<List<RewardDto>> GetAllAdminAsync();                 // xem toàn bộ (kể cả inactive/out-of-stock)
        Task<RewardDto?> GetByIdAdminAsync(Guid id);
        Task<bool> UpdateAsync(Guid id, UpdateRewardDto dto);      // update info/stock/active
        Task<bool> AdjustStockAsync(Guid id, int delta);           // cộng/trừ stock (atomic)
    }
}