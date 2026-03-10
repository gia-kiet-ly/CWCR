using Core.Enum;
using Domain.Base;

namespace Domain.Entities
{
    public class RecyclingEnterprise : BaseEntity
    {
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string TaxCode { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string LegalRepresentative { get; set; } = null!;
        public string RepresentativePosition { get; set; } = null!;

        public Guid? EnvironmentLicenseFileId { get; set; }

        public EnterpriseApprovalStatus ApprovalStatus { get; set; } = EnterpriseApprovalStatus.PendingApproval;
        public EnterpriseStatus OperationalStatus { get; set; } = EnterpriseStatus.Active;

        public DateTime? SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public ApplicationUser? ReviewedByUser { get; set; }
        public string? RejectionReason { get; set; }

        public ICollection<EnterpriseWasteCapability> WasteCapabilities { get; set; } = new List<EnterpriseWasteCapability>();
        public ICollection<EnterpriseServiceArea> ServiceAreas { get; set; } = new List<EnterpriseServiceArea>();
        public ICollection<CollectorProfile> Collectors { get; set; } = new List<CollectorProfile>();
        public ICollection<CollectionRequest> CollectionRequests { get; set; } = new List<CollectionRequest>();
        public ICollection<RecyclingStatistic> RecyclingStatistics { get; set; } = new List<RecyclingStatistic>();
        public ICollection<EnterpriseDocument> Documents { get; set; } = new List<EnterpriseDocument>();
    }
}