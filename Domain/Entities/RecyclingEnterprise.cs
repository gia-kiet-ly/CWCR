using Core.Enum;
using Domain.Base;
using Domain.Entities;

public class RecyclingEnterprise : BaseEntity
{
    // --- Liên kết account đại diện/doanh nghiệp ---
    // Account đăng ký hồ sơ doanh nghiệp (owner)
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    // (Optional) người đại diện vận hành trên hệ thống (có thể trùng UserId)
    public Guid? RepresentativeId { get; set; }
    public ApplicationUser? Representative { get; set; }

    // --- Thông tin doanh nghiệp ---
    public string Name { get; set; } = null!;                 // EnterpriseName
    public string TaxCode { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string LegalRepresentative { get; set; } = null!;
    public string RepresentativePosition { get; set; } = null!;

    // Giấy phép môi trường / file đính kèm (bạn sẽ map sang file entity sau)
    public Guid EnvironmentLicenseFileId { get; set; }

    // --- Trạng thái ---
    // Trạng thái duyệt hồ sơ (Pending/Approved/Rejected)
    public EnterpriseApprovalStatus ApprovalStatus { get; set; } = EnterpriseApprovalStatus.PendingApproval;

    // Trạng thái vận hành (Active/Inactive/Suspended...)
    public EnterpriseStatus OperationalStatus { get; set; } = EnterpriseStatus.Active;

    // --- Navigation theo nghiệp vụ hiện tại của bạn ---
    public ICollection<EnterpriseWasteCapability> WasteCapabilities { get; set; } = new List<EnterpriseWasteCapability>();
    public ICollection<EnterpriseServiceArea> ServiceAreas { get; set; } = new List<EnterpriseServiceArea>();
    public ICollection<CollectorProfile> Collectors { get; set; } = new List<CollectorProfile>();
    public ICollection<CollectionRequest> CollectionRequests { get; set; } = new List<CollectionRequest>();
    public ICollection<PointRule> PointRules { get; set; } = new List<PointRule>();
    public ICollection<RecyclingStatistic> RecyclingStatistics { get; set; } = new List<RecyclingStatistic>();
}