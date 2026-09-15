using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.EnumHelper.Defines
{
    public enum TrangThaiChiPhi
    {
        [Description("Chưa duyệt")]
        NotApproved = 0,

        [Description("Đã duyệt")]
        Approved = 1,

        [Description("Từ chối")]
        Rejected = 2
    }
}
