using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.EnumHelper.Defines
{
    public enum MeetingSpeechesStatus
    {
        [ERender("Không xác định")]
        Unknown,
        [ERender("Đang chờ (Đã giơ tay)")]
        Pending,
        [ERender("Từ chối")]
        Rejected,
        [ERender("Hoàn thành - Đã phát biểu")]
        Success
    }
}
