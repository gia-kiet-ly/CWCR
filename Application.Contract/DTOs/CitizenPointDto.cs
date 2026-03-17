using Core.Enum;

namespace Application.Contract.DTOs
{
    public class CitizenPointDto
    {
        public Guid Id { get; set; }

        public Guid CitizenId { get; set; }

        public string? CitizenName { get; set; }

        public int TotalPoints { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
    }

    public class UpdateCitizenPointRequest
    {
        public int TotalPoints { get; set; }
    }

    // ================= ENUM: Time Period =================
    public enum LeaderboardPeriod
    {
        AllTime = 0,    // Toàn thời gian
        Daily = 1,      // Hôm nay
        Weekly = 2,     // Tuần này
        Monthly = 3,    // Tháng này
        Yearly = 4      // Năm này
    }

    // ================= FILTER REQUEST =================
    public class LeaderboardFilterDto
    {
        public Guid? WardId { get; set; }
        public Guid? DistrictId { get; set; }
        public LeaderboardPeriod Period { get; set; } = LeaderboardPeriod.AllTime;
        public int TopCount { get; set; } = 10;
    }

    // ================= LEADERBOARD RESPONSE =================
    public class LeaderboardDto
    {
        public int Rank { get; set; }
        public Guid CitizenId { get; set; }
        public string CitizenName { get; set; } = string.Empty;
        public int TotalPoints { get; set; }

        // Location info
        public Guid? WardId { get; set; }
        public string? WardName { get; set; }
        public Guid? DistrictId { get; set; }
        public string? DistrictName { get; set; }
    }

    // ================= MY RANK RESPONSE =================
    public class MyRankDto
    {
        public Guid CitizenId { get; set; }
        public string CitizenName { get; set; } = string.Empty;
        public int TotalPoints { get; set; }

        // Rankings
        public int? GlobalRank { get; set; }        // Xếp hạng toàn hệ thống
        public int GlobalTotalUsers { get; set; }    // Tổng số user trong hệ thống

        public int? WardRank { get; set; }          // Xếp hạng trong Ward
        public int WardTotalUsers { get; set; }      // Tổng số user trong Ward

        public int? DistrictRank { get; set; }      // Xếp hạng trong District
        public int DistrictTotalUsers { get; set; }  // Tổng số user trong District

        // Location
        public Guid? WardId { get; set; }
        public string? WardName { get; set; }
        public Guid? DistrictId { get; set; }
        public string? DistrictName { get; set; }

        // Additional info
        public int PointsToNextRank { get; set; }   // Điểm cần để lên hạng (global)
        public string? NextRankCitizenName { get; set; }
    }

    public class CitizenPointHistoryDto
    {
        public Guid Id { get; set; }

        public Guid CitizenId { get; set; }

        public string? CitizenName { get; set; }

        public Guid? WasteReportId { get; set; }

        public int Points { get; set; }

        public CitizenPointReason Reason { get; set; }

        public string? Description { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
    }
}