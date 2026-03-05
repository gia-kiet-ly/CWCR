using System.ComponentModel.DataAnnotations;

namespace Application.Contract.DTOs
{
    // =============================
    // CREATE
    // =============================
    public class CreateRecyclingEnterpriseDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = default!;

        [Required, MaxLength(50)]
        public string TaxCode { get; set; } = default!;

        [Required, MaxLength(300)]
        public string Address { get; set; } = default!;

        [Required, MaxLength(200)]
        public string LegalRepresentative { get; set; } = default!;

        [Required, MaxLength(100)]
        public string RepresentativePosition { get; set; } = default!;

        [Required]
        public Guid EnvironmentLicenseFileId { get; set; }
    }

    // =============================
    // UPDATE
    // =============================
    public class UpdateRecyclingEnterpriseDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = default!;

        [Required, MaxLength(50)]
        public string TaxCode { get; set; } = default!;

        [Required, MaxLength(300)]
        public string Address { get; set; } = default!;

        [Required, MaxLength(200)]
        public string LegalRepresentative { get; set; } = default!;

        [Required, MaxLength(100)]
        public string RepresentativePosition { get; set; } = default!;

        public Guid EnvironmentLicenseFileId { get; set; }
    }

    // =============================
    // UPDATE STATUS (ADMIN)
    // =============================
    public class UpdateEnterpriseStatusDto
    {
        [Required]
        public string Status { get; set; } = default!;
    }

    // =============================
    // RESPONSE
    // =============================
    public class RecyclingEnterpriseDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Name { get; set; } = default!;

        public string TaxCode { get; set; } = default!;

        public string Address { get; set; } = default!;

        public string LegalRepresentative { get; set; } = default!;

        public string RepresentativePosition { get; set; } = default!;

        public Guid EnvironmentLicenseFileId { get; set; }

        public string ApprovalStatus { get; set; } = default!;

        public string OperationalStatus { get; set; } = default!;

        public DateTimeOffset CreatedTime { get; set; }
    }

    // =============================
    // FILTER + PAGING
    // =============================
    public class RecyclingEnterpriseFilterDto
    {
        public string? Name { get; set; }

        public string? Address { get; set; }

        // ApprovalStatus
        public string? Status { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }

    // =============================
    // PAGED RESULT
    // =============================
    public class PagedRecyclingEnterpriseDto
    {
        public int TotalCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public List<RecyclingEnterpriseDto> Items { get; set; } = new();
    }
}