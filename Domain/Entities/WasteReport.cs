using Core.Enum;
using Domain.Base;

namespace Domain.Entities
{
    public class WasteReport : BaseEntity
    {
        public Guid CitizenId { get; set; }
        public ApplicationUser Citizen { get; set; }

        public string? Description { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public WasteReportStatus Status { get; set; }

        public ICollection<WasteReportWaste> Wastes { get; set; }
            = new List<WasteReportWaste>();
    }

}