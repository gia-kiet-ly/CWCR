using Core.Enum;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Contract.DTOs
{
    public class CreateOrUpdateEnterpriseProfileRequestDto
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

        public Guid? EnvironmentLicenseFileId { get; set; }
    }

    public class EnterpriseProfileResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string Name { get; set; } = default!;
        public string TaxCode { get; set; } = default!;
        public string Address { get; set; } = default!;
        public string LegalRepresentative { get; set; } = default!;
        public string RepresentativePosition { get; set; } = default!;

        public Guid? EnvironmentLicenseFileId { get; set; }

        public string ApprovalStatus { get; set; } = default!;
        public string OperationalStatus { get; set; } = default!;

        public DateTime? SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public string? RejectionReason { get; set; }

        public DateTimeOffset CreatedTime { get; set; }

        public List<EnterpriseDocumentResponseDto> Documents { get; set; } = new();
    }

    public class UploadEnterpriseDocumentRequestDto
    {
        [Required]
        public EnterpriseDocumentType DocumentType { get; set; }

        [Required]
        public IFormFile File { get; set; } = default!;
    }

    public class EnterpriseDocumentResponseDto
    {
        public Guid Id { get; set; }
        public Guid RecyclingEnterpriseId { get; set; }

        public string DocumentType { get; set; } = default!;
        public string OriginalFileName { get; set; } = default!;
        public string StoredFileName { get; set; } = default!;
        public string FileUrl { get; set; } = default!;
        public string? ContentType { get; set; }
        public long? FileSize { get; set; }
        public DateTime UploadedAt { get; set; }

        public bool IsDeleted { get; set; }
    }

    public class SetEnvironmentLicenseRequestDto
    {
        [Required]
        public Guid DocumentId { get; set; }
    }

    public class SubmitEnterpriseProfileRequestDto
    {
        public string? Note { get; set; }
    }

    public class SubmitEnterpriseProfileResponseDto
    {
        public Guid EnterpriseId { get; set; }
        public string ApprovalStatus { get; set; } = default!;
        public DateTime SubmittedAt { get; set; }
        public string Message { get; set; } = default!;
    }

    public class RecyclingEnterpriseFilterDto
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? ApprovalStatus { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
    }

    public class PagedRecyclingEnterpriseDto
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public List<EnterpriseProfileResponseDto> Items { get; set; } = new();
    }
}