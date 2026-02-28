using System.ComponentModel.DataAnnotations;

namespace Application.Contract.DTOs
{
    // ================= CREATE =================
    public class CreateEnterpriseServiceAreaDto
    {
        [Required]
        public Guid EnterpriseId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RegionCode { get; set; } = string.Empty;
    }

    // ================= UPDATE =================
    public class UpdateEnterpriseServiceAreaDto
    {
        [Required]
        [MaxLength(50)]
        public string RegionCode { get; set; } = string.Empty;
    }

    // ================= RESPONSE =================
    public class EnterpriseServiceAreaDto
    {
        public Guid Id { get; set; }

        public Guid EnterpriseId { get; set; }
        public string EnterpriseName { get; set; } = string.Empty;

        public string RegionCode { get; set; } = string.Empty;

        public DateTimeOffset CreatedTime { get; set; }
    }

    // ================= FILTER =================
    public class EnterpriseServiceAreaFilterDto
    {
        public Guid? EnterpriseId { get; set; }
        public string? RegionCode { get; set; }

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
    public class PagedEnterpriseServiceAreaDto
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public List<EnterpriseServiceAreaDto> Items { get; set; }
            = new();
    }
}