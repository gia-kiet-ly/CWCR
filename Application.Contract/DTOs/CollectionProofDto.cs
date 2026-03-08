using Core.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.DTOs
{
    public class CollectionProofDto
    {
        public Guid Id { get; set; }
        public Guid AssignmentId { get; set; }

        public string ImageUrl { get; set; } = default!;
        public string PublicId { get; set; } = default!;
        public string? Note { get; set; }

        public ProofReviewStatus ReviewStatus { get; set; }
        public Guid? ReviewedBy { get; set; }
        public DateTimeOffset? ReviewedAt { get; set; }
        public string? ReviewNote { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }

        // Assignment info
        public Guid CollectorId { get; set; }
        public AssignmentStatus AssignmentStatus { get; set; }
        public DateTimeOffset? CollectedAt { get; set; }
        public string? CollectedNote { get; set; }

        // Request info
        public Guid RequestId { get; set; }
        public Guid EnterpriseId { get; set; }
        public CollectionRequestStatus RequestStatus { get; set; }
        public int? PriorityScore { get; set; }

        // Waste info
        public Guid WasteReportWasteId { get; set; }
        public Guid WasteTypeId { get; set; }
        public string? WasteTypeName { get; set; }
        public string? RequestNote { get; set; }
        public List<string> ImageUrls { get; set; } = new();

        // Location
        public Guid WasteReportId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? RegionCode { get; set; }
    }

    // Collector upload proof
    public class CreateCollectionProofDto
    {
        [Required]
        public Guid AssignmentId { get; set; }

        [Required]
        public string ImageUrl { get; set; } = default!;

        [Required]
        public string PublicId { get; set; } = default!;

        public string? Note { get; set; }
    }

    // Enterprise review proof
    public class ReviewCollectionProofDto
    {
        [Required]
        public ProofReviewStatus Status { get; set; }

        public string? ReviewNote { get; set; }
    }

    public class CollectionProofFilterDto
    {
        public Guid? AssignmentId { get; set; }
        public ProofReviewStatus? ReviewStatus { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
