using Core.Enum;
using Domain.Base;

namespace Domain.Entities
{
    public class WasteType : BaseEntity
    {
        public int BusinessCode { get; set; }
        public WasteCategory Name { get; set; }
        public string? Description { get; set; }
    }
}