using Domain.Base;
using Domain.Entities;

public class PointRule : BaseEntity
{
    public Guid EnterpriseId { get; set; }
    public RecyclingEnterprise Enterprise { get; set; } = null!;

    public Guid WasteTypeId { get; set; }
    public WasteType WasteType { get; set; } = null!;

    public int BasePoint { get; set; }

    public decimal QualityMultiplier { get; set; }

    public int FastCollectionBonus { get; set; }

    public bool IsActive { get; set; }
}