using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Enum
{
    public enum CollectionRequestStatus
    {
        Offered = 0,     // ✅ hệ thống match và "đẩy" cho enterprise, chờ phản hồi

        Accepted = 1,    // enterprise nhận
        Rejected = 2,    // enterprise từ chối

        Assigned = 3,    // đã gán collector (từ enterprise)
        Completed = 4,   // hoàn tất thu gom

        // Optional cho phase sau:
         Expired = 5,
         Cancelled = 6
    }
}