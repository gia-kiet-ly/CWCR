using System.ComponentModel.DataAnnotations;

namespace Application.Contract.DTOs
{
    // ================= CREATE =================
    public class CreateWardDto
    {
        [Required]
        public Guid DistrictId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;
    }

    // ================= UPDATE =================
    public class UpdateWardDto
    {
        [Required]
        public Guid DistrictId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;
    }

    // ================= RESPONSE =================
    public class WardDto
    {
        public Guid Id { get; set; }

        public Guid DistrictId { get; set; }

        public string DistrictName { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public DateTimeOffset CreatedTime { get; set; }
    }

    // ================= FILTER =================
    public class WardFilterDto
    {
        public Guid? DistrictId { get; set; }

        public string? Keyword { get; set; }

        private int _pageNumber = 1;
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value <= 0 ? 1 : value;
        }

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value <= 0 ? 10 : value;
        }
    }

    // ================= PAGED RESULT =================
    public class PagedWardDto
    {
        public int TotalCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public List<WardDto> Items { get; set; } = new();
    }
}