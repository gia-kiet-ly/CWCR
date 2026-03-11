using Core.Enum;
using Domain.Base;

namespace Domain.Entities
{
    public class CollectionRequest : BaseEntity
    {
        public Guid WasteReportWasteId { get; set; }
        public WasteReportWaste WasteReportWaste { get; set; } = null!;

        public Guid EnterpriseId { get; set; }
        public RecyclingEnterprise Enterprise { get; set; } = null!;

        public CollectionRequestStatus Status { get; set; }

        // điểm ưu tiên khi dispatch enterprise
        public int? PriorityScore { get; set; }

        // ===== Reject info =====
        public RejectReason? RejectReason { get; set; }
        public string? RejectNote { get; set; }

        public ICollection<CollectorAssignment> Assignments { get; set; } = new List<CollectorAssignment>();
    }
}