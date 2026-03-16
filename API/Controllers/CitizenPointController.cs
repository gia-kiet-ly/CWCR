using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Domain.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitizenPointController : ControllerBase
    {
        private readonly ICitizenPointService _service;

        public CitizenPointController(ICitizenPointService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lấy tổng điểm hiện tại của công dân.
        /// </summary>
        [HttpGet("{citizenId:guid}")]
        [Authorize(Roles = "Citizen,Admin")] // cả admin và citizen được phép
        public async Task<IActionResult> GetPoint(Guid citizenId)
        {
            var point = await _service.GetPointAsync(citizenId);
            if (point == null)
                throw new BaseException.NotFoundException(
                    "citizen_point_not_found",
                    $"Không tìm thấy điểm của công dân {citizenId}.");

            return Ok(BaseResponse<CitizenPointDto>.OkResponse(point));
        }

        /// <summary>
        /// Lấy lịch sử điểm của công dân, có phân trang.
        /// </summary>
        [HttpGet("{citizenId:guid}/history")]
        [Authorize(Roles = "Citizen,Admin")]
        public async Task<IActionResult> GetHistory(Guid citizenId, int pageNumber = 1, int pageSize = 20)
        {
            var history = await _service.GetHistoryAsync(citizenId, pageNumber, pageSize);
            return Ok(BaseResponse<IEnumerable<CitizenPointHistoryDto>>.OkResponse(history));
        }

        /// <summary>
        /// ⭐ NEW: Lấy bảng xếp hạng với filter Ward/District và Time-based
        /// </summary>
        [HttpGet("leaderboard")]
        [Authorize(Roles = "Citizen,Admin")]
        public async Task<IActionResult> GetLeaderboard([FromQuery] LeaderboardFilterDto filter)
        {
            var leaderboard = await _service.GetLeaderboardAsync(filter);
            return Ok(BaseResponse<IEnumerable<LeaderboardDto>>.OkResponse(leaderboard));
        }

        /// <summary>
        /// ⭐ NEW: Xem vị trí xếp hạng của bản thân
        /// </summary>
        [HttpGet("my-rank")]
        [Authorize(Roles = "Citizen")]
        public async Task<IActionResult> GetMyRank([FromQuery] LeaderboardPeriod period = LeaderboardPeriod.AllTime)
        {
            var citizenId = GetCurrentUserId();
            var myRank = await _service.GetMyRankAsync(citizenId, period);
            return Ok(BaseResponse<MyRankDto>.OkResponse(myRank));
        }

        /// <summary>
        /// Cộng điểm cho công dân từ một waste report đã được xác minh.
        /// </summary>
        [HttpPost("award/{wasteReportId:guid}")]
        [Authorize(Roles = "Admin")] // chỉ admin mới có quyền award
        public async Task<IActionResult> AwardPointsForVerifiedReport(Guid wasteReportId)
        {
            if (wasteReportId == Guid.Empty)
                throw new BaseException.BadRequestException(
                    "invalid_request",
                    "WasteReportId là bắt buộc.");

            var point = await _service.AwardPointsForVerifiedReportAsync(wasteReportId);
            return Ok(BaseResponse<CitizenPointDto>.OkResponse(point, "Cộng điểm thành công."));
        }

        /// <summary>
        /// Cập nhật điểm của công dân (dành cho admin).
        /// </summary>
        [HttpPut("{citizenId:guid}")]
        [Authorize(Roles = "Admin")] // chỉ admin mới update
        public async Task<IActionResult> UpdatePoint(Guid citizenId, [FromBody] UpdateCitizenPointRequest request)
        {
            if (request == null)
                throw new BaseException.BadRequestException(
                    "invalid_request",
                    "Yêu cầu body là bắt buộc.");

            var updatedPoint = await _service.UpdatePointAsync(citizenId, request);
            return Ok(BaseResponse<CitizenPointDto>.OkResponse(updatedPoint, "Cập nhật điểm thành công."));
        }
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new BaseException.UnauthorizedException(
                    "invalid_user_claim",
                    "User id claim is missing or invalid.");
            }

            return userId;
        }
    }
}