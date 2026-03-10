using Core.Enum;
using Domain.Base;

namespace Domain.Entities
{
    public class CitizenPointHistory : BaseEntity
    {
        public Guid CitizenId { get; set; }
        public ApplicationUser Citizen { get; set; } = null!;

        public Guid? WasteReportId { get; set; }
        public WasteReport? WasteReport { get; set; }

        public int Points { get; set; }

        public CitizenPointReason Reason { get; set; }

        public string? Description { get; set; }
    }
}