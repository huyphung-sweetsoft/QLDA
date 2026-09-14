
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Services;

namespace SweetSoft.QLDA.BackOffice
{
    /// <summary>
    /// Summary description for WebService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class WebService : System.Web.Services.WebService
    {
        private void HandlePaging(ref int pageIndex, ref int pageSize)
        {
            pageIndex = ((pageIndex - 1) * pageSize + 1);
            pageSize = (((pageIndex - 1) * pageSize) + pageSize);
        }

        //---------------------------------------------
        [WebMethod(EnableSession = true)]
        public object GetUsers(string keyword, string page, string page_limit)
        {
            int pageIndex = 0;
            int pageSize = 0;
            int.TryParse(page, out pageIndex);
            int.TryParse(page_limit, out pageSize);
            if (pageIndex > 0 && pageSize > 0)
            {
                int totalRows = 0;
                HandlePaging(ref pageIndex, ref pageSize);
                DataTable dt = UserManager.Instance.SearchUsers(keyword, Guid.Empty, " DisplayName ASC ", pageIndex, pageSize, out totalRows);
                if (dt == null || dt.Rows.Count <= 0)
                    return null;
                List<object> dic = new List<object>();
                foreach (DataRow item in dt.Rows)
                {
                    dic.Add(new
                    {
                        id = item["UserId"],
                        text = item["DisplayName"]
                    });
                }
                return new
                {
                    data = dic,
                    total = totalRows
                };
            }
            return null;
        }

        //---------------------------------------------  Notifications  ---------------------------------------------

        /// <summary>
        /// Lấy danh sách thông báo của user đang đăng nhập (lazy load 10 item/lần).
        /// </summary>
        /// <param name="page">Trang hiện tại, bắt đầu từ 1</param>
        [WebMethod(EnableSession = true)]
        public object GetNotifications(int page = 1)
        {
            try
            {
                const int PageSize = 10;
                Guid userId = SweetContext.Current.UserId;

                if (userId == Guid.Empty)
                    return new { success = false, message = "Unauthorized" };

                // Chuyển page -> startRow / endRow theo pattern ROW_NUMBER của project
                int startRow = (page - 1) * PageSize + 1;
                int endRow   = startRow + PageSize - 1;

                int totalRecord = 0;
                DataTable dt = ThongBaoManager.Instance.GetByUser(userId, startRow, endRow, out totalRecord);

                List<object> items = new List<object>();

                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        Guid? idDuAn = row["IdDuAn"] == DBNull.Value ? (Guid?)null : (Guid)row["IdDuAn"];
                        Guid? idCongViec = row["IdCongViec"] == DBNull.Value ? (Guid?)null : (Guid)row["IdCongViec"];
                        string loaiThongBao = row["LoaiThongBao"] == DBNull.Value ? null : (string)row["LoaiThongBao"];
                        
                        items.Add(new
                        {
                            id             = row["IdThongBao"],
                            tieuDe         = row["TieuDe"],
                            noiDung        = row["NoiDung"] == DBNull.Value ? null : (string)row["NoiDung"],
                            loaiThongBao   = loaiThongBao,
                            duongDanLienKet= SweetSoft.QLDA.BackOffice.Common.NotificationHelper.BuildNotificationLink(loaiThongBao, idDuAn, idCongViec),
                            daDoc          = (bool)row["DaDoc"],
                            ngayTao        = Convert.ToDateTime(row["NgayTao"]).ToString("dd/MM/yyyy HH:mm")
                        });
                    }
                }

                bool hasMore = (page * PageSize) < totalRecord;

                return new
                {
                    success = true,
                    data    = items,
                    total   = totalRecord,
                    hasMore = hasMore
                };
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }

        /// <summary>
        /// Đếm số thông báo chưa đọc — dùng cho badge icon chuông (polling 30s).
        /// </summary>
        [WebMethod(EnableSession = true)]
        public object GetUnreadCount()
        {
            try
            {
                Guid userId = SweetContext.Current.UserId;

                if (userId == Guid.Empty)
                    return new { success = false, count = 0 };

                int count = ThongBaoManager.Instance.CountUnread(userId);

                return new { success = true, count = count };
            }
            catch (Exception ex)
            {
                return new { success = false, count = 0, message = ex.Message };
            }
        }

        /// <summary>
        /// Polling: Lấy 10 thông báo mới nhất và số đếm chưa đọc để cập nhật tự động.
        /// </summary>
        [WebMethod(EnableSession = true)]
        public object GetLatestNotifications()
        {
            try
            {
                Guid userId = SweetContext.Current.UserId;

                if (userId == Guid.Empty)
                    return new { success = false, message = "Unauthorized" };

                int totalRecord = 0;
                DataTable dt = ThongBaoManager.Instance.GetByUser(userId, 1, 10, out totalRecord);

                List<object> items = new List<object>();

                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        Guid? idDuAn = row["IdDuAn"] == DBNull.Value ? (Guid?)null : (Guid)row["IdDuAn"];
                        Guid? idCongViec = row["IdCongViec"] == DBNull.Value ? (Guid?)null : (Guid)row["IdCongViec"];
                        string loaiThongBao = row["LoaiThongBao"] == DBNull.Value ? null : (string)row["LoaiThongBao"];
                        
                        items.Add(new
                        {
                            id             = row["IdThongBao"],
                            tieuDe         = row["TieuDe"],
                            noiDung        = row["NoiDung"] == DBNull.Value ? null : (string)row["NoiDung"],
                            loaiThongBao   = loaiThongBao,
                            duongDanLienKet= SweetSoft.QLDA.BackOffice.Common.NotificationHelper.BuildNotificationLink(loaiThongBao, idDuAn, idCongViec),
                            daDoc          = (bool)row["DaDoc"],
                            ngayTao        = Convert.ToDateTime(row["NgayTao"]).ToString("dd/MM/yyyy HH:mm")
                        });
                    }
                }

                int unreadCount = ThongBaoManager.Instance.CountUnread(userId);

                return new
                {
                    success = true,
                    data    = items,
                    unreadCount = unreadCount
                };
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }

        /// <summary>
        /// Đánh dấu một thông báo là đã đọc theo Id.
        /// </summary>
        /// <param name="id">IdThongBao (Guid dạng string)</param>
        [WebMethod(EnableSession = true)]
        public object MarkNotificationAsRead(string id)
        {
            try
            {
                Guid userId = SweetContext.Current.UserId;

                if (userId == Guid.Empty)
                    return new { success = false, message = "Unauthorized" };

                Guid idThongBao;
                if (!Guid.TryParse(id, out idThongBao))
                    return new { success = false, message = "Invalid id" };

                ThongBaoManager.Instance.MarkAsRead(idThongBao);

                return new { success = true };
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }

        /// <summary>
        /// Đánh dấu tất cả thông báo của user hiện tại là đã đọc.
        /// </summary>
        [WebMethod(EnableSession = true)]
        public object MarkAllNotificationsAsRead()
        {
            try
            {
                Guid userId = SweetContext.Current.UserId;

                if (userId == Guid.Empty)
                    return new { success = false, message = "Unauthorized" };

                ThongBaoManager.Instance.MarkAllAsRead(userId);

                return new { success = true };
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }
    }
}
