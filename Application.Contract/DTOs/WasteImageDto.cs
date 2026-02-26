using Core.Enum;
using Microsoft.AspNetCore.Http;

namespace Application.Contract.DTOs
{
    // =============================
    // CREATE (Upload Image)
    // =============================
    public class CreateWasteImageDto
    {
        public Guid WasteReportWasteId { get; set; }
        public IFormFile File { get; set; } = default!;
        public WasteImageType ImageType { get; set; }
    }

    // =============================
    // RESPONSE
    // =============================
    public class WasteImageResponseDto
    {
        public Guid Id { get; set; }
        public Guid WasteReportWasteId { get; set; }

        public string ImageUrl { get; set; } = default!;
        public WasteImageType ImageType { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
    }

    // =============================
    // FILTER
    // =============================
    public class WasteImageFilterDto
    {
        public Guid? WasteReportWasteId { get; set; }
        public WasteImageType? ImageType { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // =============================
    // PAGED RESULT
    // =============================
    public class PagedWasteImageDto
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public List<WasteImageResponseDto> Items { get; set; } = new();
    }
}