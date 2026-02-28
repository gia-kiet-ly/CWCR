using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Enum
{
    public enum EnterpriseStatus
    {
        Pending = 0,      // Chờ admin duyệt
        Active = 1,       // Đang hoạt động
        Suspended = 2,    // Bị tạm khóa
        Rejected = 3      // Bị từ chối
    }
}
