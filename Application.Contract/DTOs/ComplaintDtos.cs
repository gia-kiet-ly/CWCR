using System.ComponentModel.DataAnnotations;

namespace Application.Contract.DTOs
{
    // =============================
    // CREATE
    // =============================
    public class CreateComplaintDto
    {
        [Required]
        public Guid ReportId { get; set; }

        // Optional
        public Guid? CollectionRequestId { get; set; }

        [Required, MaxLength(50)]
        public string Type { get; set; } = default!;   // map sang enum ComplaintType

        [Required, MinLength(10), MaxLength(1000)]
        public string Content { get; set; } = default!;
    }

    // =============================
    // UPDATE (Admin)
    // =============================
    public class UpdateComplaintStatusDto
    {
        [Required, MaxLength(50)]
        public string Status { get; set; } = default!; // map sang enum ComplaintStatus
    }

    // =============================
    // RESPONSE
    // =============================
    public class ComplaintResponseDto
    {
        public Guid Id { get; set; }

        public Guid ComplainantId { get; set; }

        public Guid ReportId { get; set; }

        public Guid? CollectionRequestId { get; set; }

        public string Type { get; set; } = default!;

        public string Status { get; set; } = default!;

        public string Content { get; set; } = default!;

        public DateTimeOffset CreatedTime { get; set; }

        public List<DisputeResolutionResponseDto> Resolutions { get; set; } = new();
    }

    // =============================
    // FILTER + PAGING
    // =============================
    public class ComplaintFilterDto
    {
        public Guid? ComplainantId { get; set; }

        public string? Status { get; set; }

        public string? Type { get; set; }

        public Guid? ReportId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "PageNumber phải >= 1.")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize phải từ 1 đến 100.")]
        public int PageSize { get; set; } = 10;
    }

    // =============================
    // PAGED RESULT
    // =============================
    public class PagedComplaintDto
    {
        public int TotalCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public List<ComplaintResponseDto> Items { get; set; } = new();
    }
}