using Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class EnterpriseProfile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = default!;

        public string EnterpriseName { get; set; } = default!;
        public string TaxCode { get; set; } = default!;
        public string Address { get; set; } = default!;
        public string LegalRepresentative { get; set; } = default!;
        public string RepresentativePosition { get; set; } = default!;
        public Guid EnvironmentLicenseFileId { get; set; }

        public EnterpriseApprovalStatus Status { get; set; } = EnterpriseApprovalStatus.PendingApproval;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
