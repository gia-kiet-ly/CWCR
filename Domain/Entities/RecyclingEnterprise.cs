using Core.Enum;
using Domain.Base;
using Domain.Entities;

public class RecyclingEnterprise : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public EnterpriseStatus Status { get; set; }

    public Guid RepresentativeId { get; set; }
    public ApplicationUser Representative { get; set; } = null!;
}