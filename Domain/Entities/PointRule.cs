using Domain.Base;

namespace Domain.Entities
{
    public class PointRule : BaseEntity
    {
        public Guid WasteTypeId { get; set; }
        public WasteType WasteType { get; set; } = null!;

        public int BasePoint { get; set; }

        public bool IsActive { get; set; } = true;
    }
}