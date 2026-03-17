using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.DTOs
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }

        public Guid? WardId { get; set; }
        public string? WardName { get; set; }

        public Guid? DistrictId { get; set; }
        public string? DistrictName { get; set; }

        public string Role { get; set; } = default!;
        public bool EmailConfirmed { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdateUserProfileDto
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public Guid? WardId { get; set; }
        public Guid? DistrictId { get; set; }
    }
}
