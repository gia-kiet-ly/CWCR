using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public string? AvatarUrl { get; set; }
        public string? AvatarPublicId { get; set; }

        public Guid? WardId { get; set; }
        public Ward? Ward { get; set; }

        public Guid? DistrictId { get; set; }
        public District? District { get; set; }

        public ICollection<CollectorAssignment> Assignments { get; set; } = new List<CollectorAssignment>();

        public CollectorProfile? CollectorProfile { get; set; }
    }
}
