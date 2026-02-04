using Domain.Base;

namespace Domain.Entities
{
    public class EnterpriseWasteCapability : BaseEntity
    {
        public Guid EnterpriseId { get; set; }
        public RecyclingEnterprise Enterprise { get; set; }

        public Guid WasteTypeId { get; set; }
        public WasteType WasteType { get; set; }

        public decimal DailyCapacityKg { get; set; }
    }
}
