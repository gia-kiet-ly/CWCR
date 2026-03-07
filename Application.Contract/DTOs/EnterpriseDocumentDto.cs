using System.ComponentModel.DataAnnotations;

namespace Application.Contract.DTOs
{
    public class ApproveEnterpriseRequestDto
    {
        [Required]
        public Guid EnterpriseId { get; set; }
    }

    public class RejectEnterpriseRequestDto
    {
        [Required]
        public Guid EnterpriseId { get; set; }

        [Required, MaxLength(1000)]
        public string Reason { get; set; } = default!;
    }

    public class EnterpriseApprovalResponseDto
    {
        public Guid EnterpriseId { get; set; }
        public string ApprovalStatus { get; set; } = default!;
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public string? RejectionReason { get; set; }
        public string Message { get; set; } = default!;
    }

    public class EnterpriseApprovalListItemDto
    {
        public Guid EnterpriseId { get; set; }
        public Guid UserId { get; set; }

        public string CompanyName { get; set; } = default!;
        public string TaxCode { get; set; } = default!;
        public string Address { get; set; } = default!;
        public string LegalRepresentative { get; set; } = default!;

        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;

        public string ApprovalStatus { get; set; } = default!;
        public DateTime? SubmittedAt { get; set; }
    }

    public class EnterpriseApprovalDetailDto
    {
        public EnterpriseProfileResponseDto Enterprise { get; set; } = default!;
    }
}
