using System.ComponentModel.DataAnnotations;

namespace Application.Contract.DTOs
{
    // ================= CREATE =================
    public class CreateRecyclingStatisticDto
    {
        [Required]
        public Guid EnterpriseId { get; set; }

        [Required]
        public Guid WasteTypeId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TotalWeightKg { get; set; }

        [Required]
        [MaxLength(50)]
        public string RegionCode { get; set; } = string.Empty;

        [Required]
        public DateOnly Period { get; set; }
    }

    // ================= UPDATE =================
    public class UpdateRecyclingStatisticDto
    {
        [Range(0.01, double.MaxValue)]
        public decimal TotalWeightKg { get; set; }

        [MaxLength(50)]
        public string? RegionCode { get; set; }
    }

    // ================= RESPONSE =================
    public class RecyclingStatisticDto
    {
        public Guid Id { get; set; }

        public Guid EnterpriseId { get; set; }
        public string EnterpriseName { get; set; } = string.Empty;

        public Guid WasteTypeId { get; set; }
        public string WasteTypeName { get; set; } = string.Empty;

        public decimal TotalWeightKg { get; set; }

        public string RegionCode { get; set; } = string.Empty;

        public DateOnly Period { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
    }

    // ================= FILTER =================
    public class RecyclingStatisticFilterDto
    {
        public Guid? EnterpriseId { get; set; }
        public Guid? WasteTypeId { get; set; }
        public string? RegionCode { get; set; }

        public DateOnly? FromPeriod { get; set; }
        public DateOnly? ToPeriod { get; set; }

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
    public class PagedRecyclingStatisticDto
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public List<RecyclingStatisticDto> Items { get; set; }
            = new();
    }
}