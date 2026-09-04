using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.EnumHelper.Defines
{
    public enum NguonGocVanDeEnum
    {
        [Description("Khác")]
        Other = 0,

        [Description("Phát sinh từ công việc")]
        TaskIssue = 1,

        [Description("Phản hồi từ khách hàng")]
        CustomerFeedback = 2,
    }
}
