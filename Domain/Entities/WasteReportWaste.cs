using Domain.Base;
using Core.Enum;

namespace Domain.Entities
{
    public class WasteReportWaste : BaseEntity
    {
        public Guid WasteReportId { get; set; }
        public WasteReport WasteReport { get; set; } = null!;

        public Guid WasteTypeId { get; set; }
        public WasteType WasteType { get; set; } = null!;

        // optional: ghi chú riêng cho loại rác này
        public string? Note { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}
