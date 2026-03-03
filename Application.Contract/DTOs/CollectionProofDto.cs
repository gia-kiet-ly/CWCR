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

        public string ReviewStatus { get; set; } = default!; // Pending/Approved/Rejected
        public Guid? ReviewedBy { get; set; }
        public DateTimeOffset? ReviewedAt { get; set; }
        public string? ReviewNote { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
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
        public string Status { get; set; } = default!; // Approved/Rejected

        public string? ReviewNote { get; set; }
    }

    public class CollectionProofFilterDto
    {
        public Guid? AssignmentId { get; set; }
        public string? ReviewStatus { get; set; } // Pending/Approved/Rejected

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
