using SweetSoft.QLDA.Core.EnumHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Functions
{
    [Flags]
    public enum ActionKeys
    {
        [ERender("Không xác định")]
        None = 0,
        [ERender("Chỉ xem")]
        View = 1 << 0,
        [ERender("Tạo mới")]
        Create = 1 << 1,
        [ERender("Cập nhật")]
        Update = 1 << 2,
        [ERender("Xóa")]
        Delete = 1 << 3,
        [ERender("Xuất Excel")]
        Export = 1 << 4,
        [ERender("Tất cả")]
        All = View | Create | Update | Delete | Export 
    }
}
