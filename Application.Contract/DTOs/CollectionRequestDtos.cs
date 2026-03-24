using Core.Enum;
using System.ComponentModel.DataAnnotations;

namespace Application.Contract.DTOs
{
    public class CollectionRequestResponseDto
    {
        public Guid Id { get; set; }

        public Guid WasteReportWasteId { get; set; }
        public Guid WasteReportId { get; set; }

        public Guid EnterpriseId { get; set; }

        public CollectionRequestStatus Status { get; set; }

        public int? PriorityScore { get; set; }

        // ===== Reject Info =====
        public RejectReason? RejectReason { get; set; }
        public string? RejectReasonName { get; set; }
        public string? RejectNote { get; set; }

        // ===== Waste info =====
        public Guid WasteTypeId { get; set; }
        public string? WasteTypeName { get; set; }
        public string? Note { get; set; }

        public List<string> ImageUrls { get; set; } = new();

        // ===== Report location =====
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Address { get; set; }
        public string? RegionCode { get; set; }


        // ===== Extra info =====
        public bool HasAssignment { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
    }


    public class CollectionRequestFilterDto
    {
        public CollectionRequestStatus? Status { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "PageNumber phải >= 1.")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize phải từ 1 đến 100.")]
        public int PageSize { get; set; } = 10;
    }


    public class PagedCollectionRequestDto
    {
        public int TotalCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public List<CollectionRequestResponseDto> Items { get; set; } = new();
    }


    public class AcceptCollectionRequestDto
    {
        [Required]
        public Guid RequestId { get; set; }
    }


    public class RejectCollectionRequestDto
    {
        [Required]
        public Guid RequestId { get; set; }

        [Required]
        public RejectReason Reason { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }

}