using Domain.Base;

namespace Domain.Entities
{
    public class CollectorProfile : BaseEntity
    {
        public Guid CollectorId { get; set; }
        public ApplicationUser Collector { get; set; } = null!;

        public Guid EnterpriseId { get; set; }
        public RecyclingEnterprise Enterprise { get; set; } = null!;

        public bool IsActive { get; set; }

        // Optional: list assignment của collector
        public ICollection<CollectorAssignment> Assignments { get; set; } = new List<CollectorAssignment>();
    }
}
