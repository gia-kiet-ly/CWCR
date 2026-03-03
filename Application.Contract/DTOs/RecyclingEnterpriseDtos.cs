using System.ComponentModel.DataAnnotations;

namespace Application.Contract.DTOs
{
    public class CreateRecyclingEnterpriseDto
    {
        [Required]
        public Guid UserId { get; set; }          // ✅ NEW (bắt buộc)

        [Required, MaxLength(200)]
        public string Name { get; set; } = default!;

        [Required, MaxLength(300)]
        public string Address { get; set; } = default!;

        public Guid? RepresentativeId { get; set; } // ✅ đổi Guid -> Guid?
    }

    public class UpdateRecyclingEnterpriseDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = default!;

        [Required, MaxLength(300)]
        public string Address { get; set; } = default!;

        public Guid? RepresentativeId { get; set; } // ✅ cho update luôn
    }

    // ✅ giữ nguyên tên class để khỏi sửa controller/service interface nhiều
    // Status này sẽ hiểu là ApprovalStatus
    public class UpdateEnterpriseStatusDto
    {
        [Required]
        public string Status { get; set; } = default!;
    }

    public class RecyclingEnterpriseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Address { get; set; } = default!;

        // ✅ giữ field Status để tương thích, map = ApprovalStatus
        public string Status { get; set; } = default!;

        public Guid? RepresentativeId { get; set; } // ✅ đổi Guid -> Guid?
        public string? RepresentativeName { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
    }

    public class RecyclingEnterpriseFilterDto
    {
        public string? Name { get; set; }
        public string? Address { get; set; }

        // ✅ filter theo ApprovalStatus
        public string? Status { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}