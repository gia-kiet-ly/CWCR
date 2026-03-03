using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.DTOs
{
    public class CollectorAssignmentDto
    {
        public Guid Id { get; set; }

        public Guid RequestId { get; set; }

        public Guid CollectorProfileId { get; set; }

        public string Status { get; set; } = default!;

        public string? CollectedNote { get; set; }

        public DateTimeOffset? CollectedAt { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
    }

    // Enterprise assign
    public class CreateAssignmentDto
    {
        public Guid RequestId { get; set; }

        // ⚠️ đổi rõ tên cho khỏi nhầm
        public Guid CollectorProfileId { get; set; }
    }

    // Collector update status
    public class UpdateAssignmentStatusDto
    {
        public string Status { get; set; } = default!; // Assigned | OnTheWay | Collected
        public string? CollectedNote { get; set; }     // optional
    }

    public class AssignmentFilterDto
    {
        public string? Status { get; set; }       // filter theo status
        public Guid? CollectorId { get; set; }    // enterprise lọc theo collector
        public Guid? RequestId { get; set; }      // optional

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
