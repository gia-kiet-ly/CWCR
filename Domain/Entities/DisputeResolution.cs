using Domain.Base;

namespace Domain.Entities
{
    public class DisputeResolution : BaseEntity
    {
        public Guid ComplaintId { get; set; }
        public Complaint Complaint { get; set; } = null!;

        public Guid HandlerId { get; set; }
        public ApplicationUser Handler { get; set; } = null!;

        public string ResolutionNote { get; set; } = null!;
        public DateTimeOffset ResolvedAt { get; set; }
    }
}
