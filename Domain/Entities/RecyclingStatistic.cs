using Domain.Base;

namespace Domain.Entities
{
    public class RecyclingStatistic : BaseEntity
    {
        public Guid EnterpriseId { get; set; }
        public RecyclingEnterprise Enterprise { get; set; }

        public Guid WasteTypeId { get; set; }
        public WasteType WasteType { get; set; }

        public decimal TotalWeightKg { get; set; }
        public string RegionCode { get; set; } = null!;

        public DateOnly Period { get; set; }
    }
}
