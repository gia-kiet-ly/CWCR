using Domain.Base;

namespace Domain.Entities
{
    public class DisputeResolution : BaseEntity
    {
        public Guid ComplaintId { get; set; }

        public Guid AdminId { get; set; }
        public ApplicationUser Admin { get; set; }
        public Complaint Complaint { get; set; } = null!;
        public ApplicationUser Handler { get; set; } = null!;
        public string ResolutionNote { get; set; } = null!;
        public DateTimeOffset ResolvedAt { get; set; }
    }
}
