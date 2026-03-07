using System.ComponentModel.DataAnnotations;

namespace Application.Contract.DTOs
{
    // ================= CREATE =================
    public class CreateDistrictDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ProvinceCode { get; set; } = string.Empty;
    }

    // ================= UPDATE =================
    public class UpdateDistrictDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ProvinceCode { get; set; } = string.Empty;
    }

    // ================= RESPONSE =================
    public class DistrictDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string ProvinceCode { get; set; } = string.Empty;

        public DateTimeOffset CreatedTime { get; set; }
    }

    // ================= FILTER =================
    public class DistrictFilterDto
    {
        public string? ProvinceCode { get; set; }
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
    public class PagedDistrictDto
    {
        public int TotalCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public List<DistrictDto> Items { get; set; } = new();
    }
}