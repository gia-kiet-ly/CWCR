using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.DTOs
{
    public class DashboardChartItemDto
    {
        public string Label { get; set; } = default!;
        public int Count { get; set; }
    }

    public class DashboardValueItemDto
    {
        public string Label { get; set; } = default!;
        public decimal Value { get; set; }
    }

    public class AdminDashboardDto
    {
        public AdminSummaryDto Summary { get; set; } = new();
        public List<DashboardChartItemDto> UsersByRole { get; set; } = new();
        public List<DashboardChartItemDto> ReportsByMonth { get; set; } = new();
        public List<DashboardChartItemDto> RequestsByStatus { get; set; } = new();
        public List<DashboardChartItemDto> EnterprisesByStatus { get; set; } = new();
    }

    public class AdminSummaryDto
    {
        public int TotalUsers { get; set; }
        public int TotalCitizens { get; set; }
        public int TotalEnterprises { get; set; }
        public int TotalCollectors { get; set; }
        public int TotalWasteReports { get; set; }
        public int TotalCollectionRequests { get; set; }
        public int TotalCompletedAssignments { get; set; }
        public int PendingEnterpriseApprovals { get; set; }
        public int PendingProofReviews { get; set; }
        public decimal TotalRewardedPoints { get; set; }
    }

    public class CitizenDashboardDto
    {
        public CitizenSummaryDto Summary { get; set; } = new();
        public List<DashboardChartItemDto> ReportsByMonth { get; set; } = new();
        public List<DashboardChartItemDto> ReportsByStatus { get; set; } = new();
        public List<DashboardValueItemDto> PointsByMonth { get; set; } = new();
        public List<DashboardChartItemDto> ReportsByWasteType { get; set; } = new();
    }

    public class CitizenSummaryDto
    {
        public int MyTotalReports { get; set; }
        public int MyPendingReports { get; set; }
        public int MyCollectedReports { get; set; }
        public decimal MyCurrentPoints { get; set; }
        public decimal MyPointsThisMonth { get; set; }
    }

    public class EnterpriseDashboardDto
    {
        public EnterpriseSummaryDto Summary { get; set; } = new();
        public List<DashboardChartItemDto> RequestsByMonth { get; set; } = new();
        public List<DashboardChartItemDto> RequestsByStatus { get; set; } = new();
        public List<DashboardChartItemDto> ProofsByReviewStatus { get; set; } = new();
        public List<DashboardChartItemDto> RequestsByWasteType { get; set; } = new();
    }

    public class EnterpriseSummaryDto
    {
        public int TotalRequestsReceived { get; set; }
        public int PendingRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int PendingProofReviews { get; set; }
        public int ApprovedProofs { get; set; }
        public int RejectedProofs { get; set; }
        public decimal CompletionRate { get; set; }
    }

    public class CollectorDashboardDto
    {
        public CollectorSummaryDto Summary { get; set; } = new();
        public List<DashboardChartItemDto> AssignmentsByMonth { get; set; } = new();
        public List<DashboardChartItemDto> AssignmentsByStatus { get; set; } = new();
        public List<DashboardChartItemDto> ProofsByReviewStatus { get; set; } = new();
        public List<DashboardChartItemDto> AssignmentsByRegion { get; set; } = new();
    }

    public class CollectorSummaryDto
    {
        public int MyTotalAssignments { get; set; }
        public int MyActiveAssignments { get; set; }
        public int MyCompletedAssignments { get; set; }
        public int MyPendingProofReviews { get; set; }
        public int MyRejectedProofs { get; set; }
        public decimal MyCompletionRate { get; set; }
    }
}
