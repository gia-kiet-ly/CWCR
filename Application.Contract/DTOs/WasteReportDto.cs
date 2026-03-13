using Core.Enum;
using System.ComponentModel.DataAnnotations;

namespace Application.Contract.DTOs
{
    // =============================
    // CREATE
    // =============================
    public class CreateWasteReportDto
    {
        public string? Description { get; set; }

        // GPS có thể tự fill
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public List<CreateWasteItemDto> Wastes { get; set; } = new();
    }

    public class CreateWasteItemDto
    {
        public Guid WasteTypeId { get; set; }

        public int Quantity { get; set; }

        public string? Note { get; set; }

        public List<string> Images { get; set; } = new();
    }

    // =============================
    // UPDATE
    // =============================
    public class UpdateWasteReportDto
    {
        public string? Description { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // Cho phép chỉnh sửa waste items khi bị reject
        public List<UpdateWasteItemDto>? Wastes { get; set; }
    }

    public class UpdateWasteItemDto
    {
        public Guid WasteTypeId { get; set; }

        public int Quantity { get; set; }

        public string? Note { get; set; }

        // Replace lại toàn bộ images nếu citizen upload lại
        public List<string>? Images { get; set; }
    }

    // =============================
    // REJECT HISTORY
    // =============================
    public class RejectHistoryDto
    {
        public Guid RequestId { get; set; }

        public Guid EnterpriseId { get; set; }

        public string? EnterpriseName { get; set; }

        public RejectReason? RejectReason { get; set; }

        public string? RejectReasonName { get; set; }

        public string? RejectNote { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
    }

    // =============================
    // RESPONSE
    // =============================
    public class WasteReportResponseDto
    {
        public Guid Id { get; set; }

        public Guid CitizenId { get; set; }

        public string? Description { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public string Status { get; set; } = default!;

        public DateTimeOffset CreatedTime { get; set; }

        public List<WasteItemResponseDto> Wastes { get; set; } = new();
    }

    public class WasteItemResponseDto
    {
        public Guid WasteTypeId { get; set; }

        public string? WasteTypeName { get; set; }

        public int Quantity { get; set; }

        public string? Note { get; set; }

        public List<string> ImageUrls { get; set; } = new();
    }

    // =============================
    // FILTER + PAGING
    // =============================
    public class WasteReportFilterDto
    {
        public Guid? CitizenId { get; set; }

        public string? Status { get; set; }

        public string? Keyword { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }

    // =============================
    // PAGED RESULT
    // =============================
    public class PagedWasteReportDto
    {
        public int TotalCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public List<WasteReportResponseDto> Items { get; set; } = new();
    }

    // =============================
    // CITIZEN COLLECTION PROOF
    // =============================
    public class CitizenCollectionProofDto
    {
        public Guid ProofId { get; set; }

        public DateTimeOffset CreatedTime { get; set; }

        public string? Notes { get; set; }

        public ProofReviewStatus ReviewStatus { get; set; }

        public List<string> Images { get; set; } = new();
    }
}