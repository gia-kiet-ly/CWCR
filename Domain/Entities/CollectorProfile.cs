using Domain.Base;

namespace Domain.Entities
{
    public class CollectorProfile : BaseEntity
    {
        public Guid CollectorId { get; set; }
        public ApplicationUser Collector { get; set; }

        public Guid EnterpriseId { get; set; }
        public RecyclingEnterprise Enterprise { get; set; }

        public bool IsActive { get; set; }
    }
}
