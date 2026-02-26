using Core.Enum;
using Domain.Base;

namespace Domain.Entities
{
    public class WasteType : BaseEntity
    {
        public string Name { get; set; } = default!;

        public WasteCategory Category { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<WasteReportWaste> WasteReportWastes { get; set; }
            = new List<WasteReportWaste>();
    }
}