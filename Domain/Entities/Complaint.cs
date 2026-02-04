using Core.Enum;
using Domain.Base;

namespace Domain.Entities
{
    public class Complaint : BaseEntity
    {
        public Guid CitizenId { get; set; }
        public ApplicationUser Citizen { get; set; }

        public Guid ReportId { get; set; }
        public WasteReport Report { get; set; }

        public ComplaintType Type { get; set; }
        public ComplaintStatus Status { get; set; }
        public ICollection<DisputeResolution> Resolutions { get; set; }
        public ApplicationUser Complainant { get; set; } = null!;
        public CollectionRequest? CollectionRequest { get; set; }
        public string Content { get; set; } = null!;
    }

}
