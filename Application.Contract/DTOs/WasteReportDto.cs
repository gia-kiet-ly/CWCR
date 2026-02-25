using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Contract.DTOs
{
    // =============================
    // CREATE
    // =============================
    public class CreateWasteReportDto
    {
        public string? Description { get; set; }

        // Để null, sau này GPS sẽ tự fill
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public List<CreateWasteItemDto> Wastes { get; set; } = new();
    }

    public class CreateWasteItemDto
    {
        public Guid WasteTypeId { get; set; }

        public string? Note { get; set; }

        // Upload nhiều ảnh
        public List<IFormFile> Images { get; set; } = new();
    }

    // =============================
    // UPDATE
    // =============================
    public class UpdateWasteReportDto
    {
        public string? Description { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public string? Status { get; set; }
    }

    // =============================
    // RESPONSE
    // =============================
    public class WasteReportResponseDto
    {
        public Guid Id { get; set; }

        public Guid CitizenId { get; set; }

        public string? Description { get; set; }

        // Response nên trả giá trị thực tế (không nullable)
        // Nếu DB cho phép null thì bạn đổi thành decimal?
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
}