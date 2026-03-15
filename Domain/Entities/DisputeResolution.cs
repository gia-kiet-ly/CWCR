using Domain.Base;

namespace Domain.Entities
{
    public class DisputeResolution : BaseEntity
    {
        public Guid ComplaintId { get; set; }
        public Complaint Complaint { get; set; } = null!;
        public Guid EnterpriseId { get; set; }
        public ApplicationUser Enterprise { get; set; } = null!;

        public string ResponseNote { get; set; } = null!;
        public DateTimeOffset ResolvedAt { get; set; }
    }
}
