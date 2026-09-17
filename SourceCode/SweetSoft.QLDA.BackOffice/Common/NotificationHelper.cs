using System;
using SweetSoft.QLDA.Core.Managers;

namespace SweetSoft.QLDA.BackOffice.Common
{
    public static class NotificationHelper
    {
        public static string BuildNotificationLink(string loaiThongBao, Guid? idDuAn, Guid? idCongViec)
        {
            switch (loaiThongBao)
            {
                case ThongBaoTypes.CongViec:
                    if (idDuAn.HasValue && idDuAn.Value != Guid.Empty)
                    {
                        return RewriteURLHelper.ProjectTasks(idDuAn.Value);
                    }
                    break;

                case ThongBaoTypes.DuAn:
                    if (idDuAn.HasValue && idDuAn.Value != Guid.Empty)
                    {
                        return RewriteURLHelper.ProjectDetail(idDuAn.Value);
                    }
                    break;
                    
                // Có thể mở rộng cho Lịch họp, Vấn đề, Tài liệu sau khi có route tương ứng
            }

            return string.Empty;
        }
    }
}
