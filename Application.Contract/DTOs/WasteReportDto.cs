using System;
using System.Collections.Generic;

namespace Application.Contract.DTOs
{
    // =============================
    // CREATE
    // =============================
    public class CreateWasteReportDto
    {
        public Guid CitizenId { get; set; }

        public string? Description { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
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

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public string Status { get; set; } = default!;
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