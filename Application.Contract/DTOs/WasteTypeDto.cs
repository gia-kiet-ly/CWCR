using Core.Enum;
using System.ComponentModel.DataAnnotations;

namespace Application.Contract.DTOs
{
    // =============================
    // CREATE
    // =============================
    public class CreateWasteTypeDto
    {
        // FIX: Thêm [Required] và [MaxLength] cho Name
        [Required(ErrorMessage = "Tên loại rác là bắt buộc.")]
        [MaxLength(200, ErrorMessage = "Tên loại rác không được vượt quá 200 ký tự.")]
        public string Name { get; set; } = default!;

        [MaxLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Category là bắt buộc.")]
        public WasteCategory Category { get; set; }
    }

    // =============================
    // UPDATE
    // =============================
    public class UpdateWasteTypeDto
    {
        [MaxLength(200, ErrorMessage = "Tên loại rác không được vượt quá 200 ký tự.")]
        public string? Name { get; set; }

        [MaxLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
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