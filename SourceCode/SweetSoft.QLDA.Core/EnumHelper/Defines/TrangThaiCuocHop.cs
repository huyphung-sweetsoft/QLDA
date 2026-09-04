using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.EnumHelper.Defines
{
    public enum TrangThaiCuocHopEnum
    {
        [Description("Đã lên lịch")]
        Scheduled = 0,

        [Description("Sắp diễn ra")]
        Upcoming = 1,

        [Description("Đang diễn ra")]
        Ongoing = 2,

        [Description("Kết thúc")]
        Completed = 3
    }
}
