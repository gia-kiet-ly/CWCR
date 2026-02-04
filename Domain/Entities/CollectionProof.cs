using Domain.Base;

namespace Domain.Entities
{
    public class CollectionProof : BaseEntity
    {
        public Guid AssignmentId { get; set; }
        public CollectorAssignment Assignment { get; set; }
        public CollectionRequest CollectionRequest { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string? Note { get; set; }
    }
}
