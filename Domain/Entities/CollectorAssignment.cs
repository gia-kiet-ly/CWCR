using Core.Enum;
using Domain.Base;

namespace Domain.Entities
{
    public class CollectorAssignment : BaseEntity
    {
        public Guid RequestId { get; set; }
        public CollectionRequest Request { get; set; }

        public Guid CollectorId { get; set; }
        public CollectorProfile Collector { get; set; }

        public AssignmentStatus Status { get; set; }
        public CollectionRequest CollectionRequest { get; set; } = null!;
    }

}
