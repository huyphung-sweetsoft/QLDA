using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.EnumHelper
{
    public enum TrangThaiCongViec
    {
        [Description("Chưa bắt đầu")]
        ChuaBatDau = 0,

        [Description("Đang thực hiện")]
        DangThucHien = 1,

        [Description("Hoàn thành")]
        HoanThanh = 2,

        [Description("Hoàn thành trễ")]
        HoanThanhTre = 3
    }
}
