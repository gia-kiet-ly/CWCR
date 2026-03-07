using Core.Enum;
using Domain.Base;

namespace Domain.Entities
{
    public class RecyclingEnterprise : BaseEntity
    {
        // --- Account sở hữu doanh nghiệp ---
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        // --- Thông tin doanh nghiệp ---
        public string Name { get; set; } = null!;
        public string TaxCode { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string LegalRepresentative { get; set; } = null!;
        public string RepresentativePosition { get; set; } = null!;

        // File giấy phép môi trường chính
        public Guid? EnvironmentLicenseFileId { get; set; }

        // --- Trạng thái duyệt ---
        public EnterpriseApprovalStatus ApprovalStatus { get; set; } = EnterpriseApprovalStatus.PendingApproval;

        // --- Trạng thái hoạt động ---
        public EnterpriseStatus OperationalStatus { get; set; } = EnterpriseStatus.Active;

        // --- Audit approve/reject ---
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public ApplicationUser? ReviewedByUser { get; set; }
        public string? RejectionReason { get; set; }

        // --- Navigation ---
        public ICollection<EnterpriseWasteCapability> WasteCapabilities { get; set; } = new List<EnterpriseWasteCapability>();
        public ICollection<EnterpriseServiceArea> ServiceAreas { get; set; } = new List<EnterpriseServiceArea>();
        public ICollection<CollectorProfile> Collectors { get; set; } = new List<CollectorProfile>();
        public ICollection<CollectionRequest> CollectionRequests { get; set; } = new List<CollectionRequest>();
        public ICollection<PointRule> PointRules { get; set; } = new List<PointRule>();
        public ICollection<RecyclingStatistic> RecyclingStatistics { get; set; } = new List<RecyclingStatistic>();

        // Hồ sơ/tài liệu doanh nghiệp upload lên để admin duyệt
        public ICollection<EnterpriseDocument> Documents { get; set; } = new List<EnterpriseDocument>();
    }
}