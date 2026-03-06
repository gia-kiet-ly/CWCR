using Application.Contract.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Contract.Interfaces.Services
{
    public interface ICitizenPointService
    {
        // Lấy tổng điểm hiện tại của citizen.
        Task<CitizenPointDto?> GetPointAsync(Guid citizenId);

        // Lấy danh sách lịch sử điểm, có phân trang.
        Task<IEnumerable<CitizenPointHistoryDto>> GetHistoryAsync(Guid citizenId, int pageNumber = 1, int pageSize = 20);

        // Lấy top N citizen dựa trên TotalPoints.
        Task<IEnumerable<LeaderboardDto>> GetLeaderboardAsync(int topCount = 10);

        // Dựa trên WasteReportId để tính điểm dựa theo Weight và PointRule.
        Task<CitizenPointDto> CalculatePointAsync(CalculateCitizenPointRequest request);

        // Cho phép update điểm manually (admin action).
        Task<CitizenPointDto> UpdatePointAsync(Guid citizenId, UpdateCitizenPointRequest request);
    }
}