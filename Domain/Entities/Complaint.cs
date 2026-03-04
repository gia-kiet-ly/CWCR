using Core.Enum;
using Domain.Base;

namespace Domain.Entities
{
    public class Complaint : BaseEntity
    {
        public Guid ComplainantId { get; set; }
        public ApplicationUser Complainant { get; set; } = null!;

        public Guid ReportId { get; set; }
        public WasteReport Report { get; set; } = null!;

        public Guid? CollectionRequestId { get; set; }
        public CollectionRequest? CollectionRequest { get; set; }

        public ComplaintType Type { get; set; }
        public ComplaintStatus Status { get; set; }

        public string Content { get; set; } = null!;

        public ICollection<DisputeResolution> Resolutions { get; set; }
            = new List<DisputeResolution>();
    }

}
