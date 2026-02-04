using Domain.Base;

namespace Domain.Entities
{
    public class CitizenPoint : BaseEntity
    {
        public Guid CitizenId { get; set; }
        public ApplicationUser Citizen { get; set; }

        public Guid ReportId { get; set; }
        public WasteReport Report { get; set; }

        public int Point { get; set; }
        public string Reason { get; set; } = null!;
    }
}
