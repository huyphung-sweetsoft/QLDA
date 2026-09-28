using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.EnumHelper.Defines
{
    public enum DuAnStatus : Byte
    {
        [ERender("Chưa bắt đầu")]
        ChoThucHien = 0,

        [ERender("Đang thực hiện")]
        DangThucHien = 1,

        [ERender("Tạm dừng")]
        TamDung = 2,

        [ERender("Hoàn thành")]
        HoanThanh = 3,

        [ERender("Kết thúc")]
        KetThuc = 4,

    }
}
