using Domain.Base;

namespace Domain.Entities
{
    public class EnterpriseServiceArea : BaseEntity
    {
        public Guid EnterpriseId { get; set; }
        public RecyclingEnterprise Enterprise { get; set; }

        public string RegionCode { get; set; } = null!;
    }
}
