using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.DTOs
{
    public class CreatePointRuleRequest
    {
        public Guid WasteTypeId { get; set; }

        public int BasePoint { get; set; }
    }

    public class UpdatePointRuleRequest
    {
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
