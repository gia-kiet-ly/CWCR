using Core.Enum;
using Domain.Base;

namespace Domain.Entities
{
    public class CollectorAssignment : BaseEntity
    {
        public Guid RequestId { get; set; }
        public CollectionRequest Request { get; set; } = null!;

        public Guid CollectorId { get; set; }   // ApplicationUser.Id
        public ApplicationUser Collector { get; set; } = null!;

        public AssignmentStatus Status { get; set; }
        public string? CollectedNote { get; set; }
        public DateTimeOffset? CollectedAt { get; set; }

        public ICollection<CollectionProof> Proofs { get; set; } = new List<CollectionProof>();
    }

}
