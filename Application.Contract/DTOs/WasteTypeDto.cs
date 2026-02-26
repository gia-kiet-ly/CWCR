using Core.Enum;

namespace Application.Contract.DTOs
{
    // =============================
    // CREATE
    // =============================
    public class CreateWasteTypeDto
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public WasteCategory Category { get; set; }
    }

    // =============================
    // UPDATE
    // =============================
    public class UpdateWasteTypeDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public WasteCategory? Category { get; set; }
        public bool? IsActive { get; set; }
    }

    // =============================
    // RESPONSE
    // =============================
    public class WasteTypeResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public WasteCategory Category { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
    }

    // =============================
    // FILTER
    // =============================
    public class WasteTypeFilterDto
    {
        public WasteCategory? Category { get; set; }
        public bool? IsActive { get; set; }
        public string? Keyword { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // =============================
    // PAGED RESULT
    // =============================
    public class PagedWasteTypeDto
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<WasteTypeResponseDto> Items { get; set; } = new();
    }
}