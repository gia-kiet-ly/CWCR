using Domain.Base;

namespace Domain.Entities
{
    public class SystemAuditLog : BaseEntity
    {
        public Guid? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public string Action { get; set; } = null!;
        public string Entity { get; set; } = null!;
        public Guid EntityId { get; set; }
    }
}
