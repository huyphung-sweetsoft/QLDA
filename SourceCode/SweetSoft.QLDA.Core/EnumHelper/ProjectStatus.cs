using System.ComponentModel;

namespace SweetSoft.QLDA.Core.EnumHelper
{
    public enum ProjectStatus : byte
    {
        [Description("Chưa bắt đầu")]
        NotStarted = 0,

        [Description("Đang thực hiện")]
        InProgress = 1,

        [Description("Đã hoàn thành")]
        Completed = 2,

        [Description("Tạm dừng")]
        Paused = 3,

        [Description("Đã hủy")]
        Cancelled = 4
    }
}