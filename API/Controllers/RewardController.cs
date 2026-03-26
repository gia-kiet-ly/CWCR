using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Infrastructure.Repo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/rewards")]
    public class RewardController : ControllerBase
    {
        private readonly IRewardService _service;

        public RewardController(IRewardService service)
        {
            _service = service;
        }

        private Guid GetUserId()
            => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // ===================== CITIZEN =====================

        // Citizen: xem danh sách quà active & còn stock
        [HttpGet]
        [Authorize(Roles = SystemRoles.Citizen)]
        public async Task<IActionResult> GetActiveRewards()
        {
            var list = await _service.GetActiveRewardsAsync();
            return Ok(list);
        }

        // Citizen: đổi quà
        [HttpPost("{rewardId:guid}/redeem")]
        [Authorize(Roles = SystemRoles.Citizen)]
        public async Task<IActionResult> Redeem(Guid rewardId)
        {
            var citizenId = GetUserId();
            var result = await _service.RedeemAsync(citizenId, rewardId);
            return Ok(result);
        }

        // Citizen: lịch sử đổi
        [HttpGet("me/redemptions")]
        [Authorize(Roles = SystemRoles.Citizen)]
        public async Task<IActionResult> GetMyRedemptions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var citizenId = GetUserId();
            var list = await _service.GetMyRedemptionsAsync(citizenId, pageNumber, pageSize);
            return Ok(list);
        }

        // ===================== ADMIN =====================

        // Admin: list tất cả quà (kể cả inactive/out-of-stock)
        [HttpGet("admin")]
        [Authorize(Roles = SystemRoles.Administrator)]
        public async Task<IActionResult> AdminGetAll()
        {
            var list = await _service.GetAllAdminAsync();
            return Ok(list);
        }

        // Admin: xem chi tiết 1 quà
        [HttpGet("admin/{id:guid}")]
        [Authorize(Roles = SystemRoles.Administrator)]
        public async Task<IActionResult> AdminGetById(Guid id)
        {
            var item = await _service.GetByIdAdminAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // Admin: tạo quà
        [HttpPost("admin")]
        [Authorize(Roles = SystemRoles.Administrator)]
        public async Task<IActionResult> AdminCreate([FromBody] CreateRewardDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return Ok(created);
        }

        // Admin: update quà (name/pointCost/stock/isActive...)
        [HttpPut("admin/{id:guid}")]
        [Authorize(Roles = SystemRoles.Administrator)]
        public async Task<IActionResult> AdminUpdate(Guid id, [FromBody] UpdateRewardDto dto)
        {
            var ok = await _service.UpdateAsync(id, dto);
            return ok ? Ok() : NotFound();
        }

        // Admin: tăng/giảm stock (atomic)
        // body: { "delta": 10 } hoặc { "delta": -3 }
        [HttpPut("admin/{id:guid}/stock")]
        [Authorize(Roles = SystemRoles.Administrator)]
        public async Task<IActionResult> AdminAdjustStock(Guid id, [FromBody] AdjustRewardStockDto dto)
        {
            var ok = await _service.AdjustStockAsync(id, dto.Delta);
            return ok ? Ok() : BadRequest("Invalid stock update (would become negative) or reward not found.");
        }
    }
}