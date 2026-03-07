using Domain.Base;

namespace Domain.Entities
{
    public class District : BaseEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public string ProvinceCode { get; set; }

        public ICollection<Ward> Wards { get; set; }
    }
}