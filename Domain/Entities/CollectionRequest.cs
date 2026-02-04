using Core.Enum;
using Domain.Base;

namespace Domain.Entities
{
    public class CollectionRequest : BaseEntity
    {
        public Guid ReportId { get; set; }
        public WasteReport Report { get; set; }

        public Guid EnterpriseId { get; set; }
        public RecyclingEnterprise Enterprise { get; set; }
        public ApplicationUser Citizen { get; set; } = null!;
        public WasteType WasteType { get; set; } = null!;
        public CollectionRequestStatus Status { get; set; }
        public int? PriorityScore { get; set; }
    }

}
