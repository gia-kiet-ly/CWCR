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
        Task<IEnumerable<CitizenPointHistoryDto>> GetHistoryAsync(
            Guid citizenId,
            int pageNumber = 1,
            int pageSize = 20);

        /// <summary>
        /// Get leaderboard với filter Ward/District và Time-based
        /// </summary>
        Task<IEnumerable<LeaderboardDto>> GetLeaderboardAsync(LeaderboardFilterDto filter);

        /// <summary>
        /// Xem vị trí xếp hạng của bản thân
        /// </summary>
        Task<MyRankDto> GetMyRankAsync(Guid citizenId, LeaderboardPeriod period = LeaderboardPeriod.AllTime);

        // Cộng điểm cho citizen dựa trên WasteReport đã được xác minh.
        Task<CitizenPointDto> AwardPointsForVerifiedReportAsync(Guid wasteReportId);

        // Cho phép update điểm manually (admin action).
        Task<CitizenPointDto> UpdatePointAsync(Guid citizenId, UpdateCitizenPointRequest request);
        // thêm vào interface
        Task AwardPointsForResolvedComplaintAsync(Guid complaintId);
    }
}