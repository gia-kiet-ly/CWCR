using Core.Enum;
using Domain.Base;

namespace Domain.Entities
{
    public class CollectionProof : BaseEntity
    {
        public Guid AssignmentId { get; set; }
        public CollectorAssignment Assignment { get; set; } = null!;

        public string ImageUrl { get; set; } = null!;
        public string PublicId { get; set; } = null!;
        public string? Note { get; set; }

        // ✅ NEW: review bởi enterprise
        public ProofReviewStatus ReviewStatus { get; set; } = ProofReviewStatus.Pending;

        public Guid? ReviewedBy { get; set; }          // userId (Enterprise representative / admin)
        public DateTimeOffset? ReviewedAt { get; set; }
        public string? ReviewNote { get; set; }        // lý do reject / ghi chú approve
    }
}
