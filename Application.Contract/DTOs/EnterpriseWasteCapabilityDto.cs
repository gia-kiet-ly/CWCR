using System.ComponentModel.DataAnnotations;

namespace Application.Contract.DTOs
{
    // ================= CREATE =================
    public class CreateEnterpriseWasteCapabilityDto
    {
        [Required]
        public Guid EnterpriseId { get; set; }

        [Required]
        public Guid WasteTypeId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal DailyCapacityKg { get; set; }
    }

    // ================= UPDATE =================
    public class UpdateEnterpriseWasteCapabilityDto
    {
        [Range(0.01, double.MaxValue)]
        public decimal DailyCapacityKg { get; set; }
    }

    // ================= RESPONSE =================
    public class EnterpriseWasteCapabilityDto
    {
        public Guid Id { get; set; }

        public Guid EnterpriseId { get; set; }
        public string EnterpriseName { get; set; } = string.Empty;

        public Guid WasteTypeId { get; set; }
        public string WasteTypeName { get; set; } = string.Empty;

        public decimal DailyCapacityKg { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
    }

    // ================= FILTER =================
    public class EnterpriseWasteCapabilityFilterDto
    {
        public Guid? EnterpriseId { get; set; }
        public Guid? WasteTypeId { get; set; }

        public decimal? MinCapacity { get; set; }
        public decimal? MaxCapacity { get; set; }

        // Paging
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
    public class PagedEnterpriseWasteCapabilityDto
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public List<EnterpriseWasteCapabilityDto> Items { get; set; }
            = new();
    }
}