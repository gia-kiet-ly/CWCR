using System;
using System.Collections.Generic;

namespace Application.Contract.DTOs
{
    // =============================
    // COLLECTION REQUEST - RESPONSE
    // =============================
    public class CollectionRequestResponseDto
    {
        public Guid Id { get; set; }

        public Guid WasteReportWasteId { get; set; }
        public Guid WasteReportId { get; set; }

        public Guid EnterpriseId { get; set; }
        public string Status { get; set; } = default!;
        public int? PriorityScore { get; set; }

        // Waste info
        public Guid WasteTypeId { get; set; }
        public string? WasteTypeName { get; set; }
        public string? Note { get; set; }
        public List<string> ImageUrls { get; set; } = new();

        // Report location
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? RegionCode { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
    }

    // =============================
    // FILTER + PAGING (ENTERPRISE INBOX)
    // =============================
    public class CollectionRequestFilterDto
    {
        // Offered/Accepted/Rejected/Assigned/Completed
        public string? Status { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PagedCollectionRequestDto
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<CollectionRequestResponseDto> Items { get; set; } = new();
    }

    // =============================
    // ACTIONS
    // =============================
    public class AcceptCollectionRequestDto
    {
        public Guid RequestId { get; set; }
    }

    public class RejectCollectionRequestDto
    {
        public Guid RequestId { get; set; }
        public string? Reason { get; set; }
    }
}