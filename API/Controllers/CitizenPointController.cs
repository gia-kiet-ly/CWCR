using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Domain.Base;
using Microsoft.AspNetCore.Mvc;

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
        /// <param name="citizenId">ID của công dân.</param>
        /// <returns>Thông tin điểm hiện tại của công dân.</returns>
        /// <response code="200">Trả về thông tin điểm của công dân</response>
        /// <response code="404">Không tìm thấy điểm của công dân</response>
        [HttpGet("{citizenId:guid}")]
        public async Task<IActionResult> GetPoint(Guid citizenId)
        {
            var point = await _service.GetPointAsync(citizenId);
            if (point == null)
                throw new BaseException.NotFoundException("citizen_point_not_found", $"Không tìm thấy điểm của công dân {citizenId}.");

            return Ok(BaseResponse<CitizenPointDto>.OkResponse(point));
        }

        /// <summary>
        /// Lấy lịch sử điểm của công dân, có phân trang.
        /// </summary>
        /// <param name="citizenId">ID của công dân.</param>
        /// <param name="pageNumber">Số trang (mặc định 1)</param>
        /// <param name="pageSize">Số bản ghi trên mỗi trang (mặc định 20)</param>
        /// <returns>Danh sách lịch sử điểm của công dân.</returns>
        /// <response code="200">Trả về danh sách lịch sử điểm</response>
        [HttpGet("{citizenId:guid}/history")]
        public async Task<IActionResult> GetHistory(Guid citizenId, int pageNumber = 1, int pageSize = 20)
        {
            var history = await _service.GetHistoryAsync(citizenId, pageNumber, pageSize);
            return Ok(BaseResponse<IEnumerable<CitizenPointHistoryDto>>.OkResponse(history));
        }

        /// <summary>
        /// Lấy bảng xếp hạng công dân theo tổng điểm.
        /// </summary>
        /// <param name="topCount">Số công dân top đầu cần lấy (mặc định 10)</param>
        /// <returns>Danh sách bảng xếp hạng.</returns>
        /// <response code="200">Trả về bảng xếp hạng</response>
        [HttpGet("leaderboard")]
        public async Task<IActionResult> GetLeaderboard(int topCount = 10)
        {
            var leaderboard = await _service.GetLeaderboardAsync(topCount);
            return Ok(BaseResponse<IEnumerable<LeaderboardDto>>.OkResponse(leaderboard));
        }

        /// <summary>
        /// Tính điểm cho công dân dựa trên báo cáo rác thải.
        /// </summary>
        /// <param name="request">Yêu cầu chứa WasteReportId</param>
        /// <returns>Thông tin điểm sau khi tính</returns>
        /// <response code="200">Tính điểm thành công</response>
        /// <response code="400">Yêu cầu không hợp lệ</response>
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculatePoint([FromBody] CalculateCitizenPointRequest request)
        {
            if (request == null || request.WasteReportId == Guid.Empty)
                throw new BaseException.BadRequestException("invalid_request", "WasteReportId là bắt buộc.");

            var point = await _service.CalculatePointAsync(request);
            return Ok(BaseResponse<CitizenPointDto>.OkResponse(point, "Tính điểm thành công."));
        }

        /// <summary>
        /// Cập nhật điểm của công dân (dành cho admin).
        /// </summary>
        /// <param name="citizenId">ID của công dân</param>
        /// <param name="request">Yêu cầu chứa tổng điểm mới</param>
        /// <returns>Thông tin điểm đã cập nhật</returns>
        /// <response code="200">Cập nhật điểm thành công</response>
        /// <response code="400">Yêu cầu không hợp lệ</response>
        [HttpPut("{citizenId:guid}")]
        public async Task<IActionResult> UpdatePoint(Guid citizenId, [FromBody] UpdateCitizenPointRequest request)
        {
            if (request == null)
                throw new BaseException.BadRequestException("invalid_request", "Yêu cầu body là bắt buộc.");

            var updatedPoint = await _service.UpdatePointAsync(citizenId, request);
            return Ok(BaseResponse<CitizenPointDto>.OkResponse(updatedPoint, "Cập nhật điểm thành công."));
        }
    }
}