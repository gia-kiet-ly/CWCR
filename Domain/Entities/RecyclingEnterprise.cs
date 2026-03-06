using Core.Enum;
using Domain.Base;
using Domain.Entities;

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

    // File giấy phép môi trường
    public Guid EnvironmentLicenseFileId { get; set; }

    // --- Trạng thái ---
    public EnterpriseApprovalStatus ApprovalStatus { get; set; } = EnterpriseApprovalStatus.PendingApproval;

    public EnterpriseStatus OperationalStatus { get; set; } = EnterpriseStatus.Active;

    // --- Navigation ---
    public ICollection<EnterpriseWasteCapability> WasteCapabilities { get; set; } = new List<EnterpriseWasteCapability>();

    public ICollection<EnterpriseServiceArea> ServiceAreas { get; set; } = new List<EnterpriseServiceArea>();

    public ICollection<CollectorProfile> Collectors { get; set; } = new List<CollectorProfile>();

    public ICollection<CollectionRequest> CollectionRequests { get; set; } = new List<CollectionRequest>();

    public ICollection<PointRule> PointRules { get; set; } = new List<PointRule>();

    public ICollection<RecyclingStatistic> RecyclingStatistics { get; set; } = new List<RecyclingStatistic>();
}