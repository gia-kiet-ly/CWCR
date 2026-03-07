using Domain.Base;
namespace Domain.Entities;

public class EnterpriseServiceArea : BaseEntity
{
    public Guid EnterpriseId { get; set; }
    public RecyclingEnterprise Enterprise { get; set; }

    public Guid DistrictId { get; set; }
    public District District { get; set; }

    public Guid? WardId { get; set; }
    public Ward? Ward { get; set; }
}