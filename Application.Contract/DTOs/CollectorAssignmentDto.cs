using Core.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.DTOs
{
    public class CollectorAssignmentDto
    {
        public Guid Id { get; set; }
        public Guid RequestId { get; set; }
        public Guid CollectorId { get; set; }

        public AssignmentStatus Status { get; set; }
        public string? CollectedNote { get; set; }
        public DateTimeOffset? CollectedAt { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }

        // Request info
        public Guid EnterpriseId { get; set; }
        public CollectionRequestStatus RequestStatus { get; set; }
        public int? PriorityScore { get; set; }

        // Waste info
        public Guid WasteReportWasteId { get; set; }
        public Guid WasteTypeId { get; set; }
        public string? WasteTypeName { get; set; }
        public string? Note { get; set; }
        public List<string> ImageUrls { get; set; } = new();

        // Location
        public Guid WasteReportId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? RegionCode { get; set; }
    }

    // Enterprise assign
    public class CreateAssignmentDto
    {
        public Guid RequestId { get; set; }

        public Guid CollectorId { get; set; }   // ApplicationUser.Id
    }

    // Collector update status
    public class UpdateAssignmentStatusDto
    {
        [Required]
        public AssignmentStatus Status { get; set; }

        public string? CollectedNote { get; set; }
    }

    public class AssignmentFilterDto
    {
        public AssignmentStatus? Status { get; set; }
        public Guid? CollectorId { get; set; }
        public Guid? RequestId { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
