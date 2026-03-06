using Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class LeaderboardDto
    {
        public int Rank { get; set; }

        public Guid CitizenId { get; set; }

        public string? CitizenName { get; set; }

        public int TotalPoints { get; set; }
    }

    public class CalculateCitizenPointRequest
    {
        public Guid WasteReportId { get; set; }
    }

    public class CitizenPointHistoryDto
    {
        public Guid Id { get; set; }

        public Guid CitizenId { get; set; }

        public string? CitizenName { get; set; }

        public Guid? WasteReportId { get; set; }

        public int Points { get; set; }

        public CitizenPointReason Reason { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
    }
}
