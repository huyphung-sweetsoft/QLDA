using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.EnumHelper.Defines
{
    public enum XacSuatRuiRoEnum
    {
        [Description("Rất thấp")]
        VeryLow = 10,

        [Description("Thấp")]
        Low = 25,

        [Description("Trung bình")]
        Medium = 50,

        [Description("Cao")]
        High = 75,

        [Description("Rất cao")]
        VeryHigh = 90
    }
}
