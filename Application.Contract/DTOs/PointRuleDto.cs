using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.DTOs
{
    public class CreatePointRuleRequest
    {
        [Required]
        public Guid WasteTypeId { get; set; }

        [Range(1, 10000, ErrorMessage = "BasePoint phải từ 1 đến 10000.")]
        public int BasePoint { get; set; }
    }

    public class UpdatePointRuleRequest
    {
        [Range(1, 10000, ErrorMessage = "BasePoint phải từ 1 đến 10000.")]
        public int BasePoint { get; set; }

        public bool IsActive { get; set; }
    }

    public class PointRuleResponse
    {
        public Guid Id { get; set; }

        public Guid WasteTypeId { get; set; }

        public int BasePoint { get; set; }

        public bool IsActive { get; set; }
    }
}
