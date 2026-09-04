using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.EnumHelper.Defines
{
    public enum MucDoAnhHuonEnum
    {
        [Description("Rất thấp")]
        VeryLow = 1,

        [Description("Thấp")]
        Low = 2,

        [Description("Trung bình")]
        Medium = 3,

        [Description("Cao")]
        High = 4,

        [Description("Rất cao")]
        VeryHigh = 5
    }
}
