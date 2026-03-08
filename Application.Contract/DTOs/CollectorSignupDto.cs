using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.DTOs
{
    public class CreateCollectorDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string Password { get; set; } = null!;

        [MaxLength(150)]
        public string? FullName { get; set; }
    }

    public class CreateCollectorResponseDto
    {
        public Guid UserId { get; set; }
        public Guid CollectorProfileId { get; set; }
        public Guid EnterpriseId { get; set; }

        public string Email { get; set; } = null!;
        public string? FullName { get; set; }

        public bool IsActive { get; set; }
        public bool IsProfileCompleted { get; set; }

        public string Role { get; set; } = null!;
    }

    public class CollectorItemDto
    {
        public Guid UserId { get; set; }
        public Guid CollectorProfileId { get; set; }
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public bool IsActive { get; set; }
        public bool IsProfileCompleted { get; set; }
        public Guid EnterpriseId { get; set; }
    }
}
