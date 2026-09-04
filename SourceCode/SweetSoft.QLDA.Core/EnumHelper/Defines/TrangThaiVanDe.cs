using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.EnumHelper.Defines
{
    public enum TrangThaiVanDeEnum
    {
        [Description("Đang xử lý")]
        Processing = 0,

        [Description("Đã xử lý")]
        Processed = 1
    }
}
