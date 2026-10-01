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
                    return RewriteURLHelper.Projects;
                case ThongBaoTypes.DuAn:
                    if (idDuAn.HasValue && idDuAn.Value != Guid.Empty)
                    {
                        return RewriteURLHelper.ProjectDetail(idDuAn.Value);
                    }
                    return RewriteURLHelper.Projects;
                case ThongBaoTypes.LichHop:
                    if (idDuAn.HasValue && idDuAn.Value != Guid.Empty)
                    {
                        return RewriteURLHelper.ProjectMeets(idDuAn.Value);
                    }
                    return RewriteURLHelper.Projects;
                case ThongBaoTypes.TaiLieu:
                    if (idDuAn.HasValue && idDuAn.Value != Guid.Empty)
                    {
                        if(idCongViec.HasValue && idCongViec.Value != Guid.Empty)
                            return RewriteURLHelper.ProjectDocumentDetail(idDuAn.Value, idCongViec.Value);
                        return RewriteURLHelper.ProjectDocuments(idDuAn.Value);
                    }
                    else if (idCongViec.HasValue && idCongViec.Value != Guid.Empty)
                    {
                        return RewriteURLHelper.DocumentDetail(idCongViec.Value);
                    }
                    return RewriteURLHelper.Documents;
                case ThongBaoTypes.RuiRo:
                    if (idDuAn.HasValue && idDuAn.Value != Guid.Empty)
                    {
                        return RewriteURLHelper.ProjectRisks(idDuAn.Value);
                    }
                    return RewriteURLHelper.Projects;
                    
                // Có thể mở rộng cho Lịch họp, Vấn đề, Tài liệu sau khi có route tương ứng
            }

            return string.Empty;
        }
    }
}
