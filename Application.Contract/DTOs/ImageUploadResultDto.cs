using Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.DTOs
{
    public class ImageUploadResultDto
    {
        public string Url { get; set; } = default!;
        public string PublicId { get; set; } = default!;
        // Gợi ý từ Vision AI (null = không phải ảnh rác)
        public WasteCategory? SuggestedCategory { get; set; }
    }
}
