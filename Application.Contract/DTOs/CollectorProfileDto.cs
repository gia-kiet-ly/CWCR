using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.DTOs
{
    public class CollectorProfileDto
    {
        public Guid Id { get; set; }

        public Guid CollectorId { get; set; }
        public string? CollectorName { get; set; }
        public string? CollectorEmail { get; set; }

        public Guid EnterpriseId { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
    }

    public class CreateCollectorProfileDto
    {
        [Required]
        public Guid CollectorId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateCollectorProfileDto
    {
        public bool IsActive { get; set; }
    }

    public class CollectorProfileFilterDto
    {
        public string? Keyword { get; set; }   // search name/email
        public bool? IsActive { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
