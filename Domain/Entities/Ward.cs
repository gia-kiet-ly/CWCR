using Domain.Base;

namespace Domain.Entities
{
    public class Ward : BaseEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public Guid DistrictId { get; set; }
        public District District { get; set; }
    }
}