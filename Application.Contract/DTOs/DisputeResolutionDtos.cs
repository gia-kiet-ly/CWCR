using System.ComponentModel.DataAnnotations;

namespace Application.Contract.DTOs
{
    // =============================
    // CREATE (Admin)
    // =============================
    public class CreateDisputeResolutionDto
    {
        [Required]
        public Guid ComplaintId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string ResolutionNote { get; set; } = default!;
    }

    // =============================
    // RESPONSE
    // =============================
    public class DisputeResolutionResponseDto
    {
        public Guid Id { get; set; }

        public Guid ComplaintId { get; set; }

        public Guid HandlerId { get; set; }

        public string ResolutionNote { get; set; } = default!;

        public DateTimeOffset ResolvedAt { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
    }

    // =============================
    // FILTER + PAGING (Admin)
    // =============================
    public class DisputeResolutionFilterDto
    {
        public Guid? ComplaintId { get; set; }

        public Guid? HandlerId { get; set; }

        public DateTimeOffset? FromDate { get; set; }

        public DateTimeOffset? ToDate { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }

    // =============================
    // PAGED RESULT
    // =============================
    public class PagedDisputeResolutionDto
    {
        public int TotalCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public List<DisputeResolutionResponseDto> Items { get; set; } = new();
    }
}