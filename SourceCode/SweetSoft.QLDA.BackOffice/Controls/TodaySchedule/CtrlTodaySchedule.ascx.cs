using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.SysManager;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.Controls.TodaySchedule
{
    // Lịch trình chung được tải THEO YÊU CẦU khi người dùng bấm Q/menu, không truy vấn ở mỗi trang.
    public partial class CtrlTodaySchedule : UserControl
    {
        private static Control FindDescendant(Control root, string id)
        {
            if (root == null) return null;
            if (root.ID == id) return root;
            foreach (Control child in root.Controls)
            {
                Control match = FindDescendant(child, id);
                if (match != null) return match;
            }
            return null;
        }
        protected string TodayModalClientId
        {
            get { Control c = FindDescendant(this, "mdlTodaySchedule"); return c == null ? "" : c.ClientID; }
        }
        protected string TodayRefreshClientId
        {
            get { Control c = FindDescendant(this, "btnTodayReload"); return c == null ? "" : c.ClientID; }
        }
        private static string E(object value) { return HttpUtility.HtmlEncode(Convert.ToString(value) ?? string.Empty); }
        private static string ResolveConnectionString()
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["SweetSoft.QLDA.BackOffice"];
            if (settings != null && !string.IsNullOrWhiteSpace(settings.ConnectionString)) return settings.ConnectionString;
            foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.ConnectionString) && item.ConnectionString.IndexOf("SweetSoft_QLDA", StringComparison.OrdinalIgnoreCase) >= 0) return item.ConnectionString;
            }
            foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.ConnectionString) && !string.Equals(item.Name, "LocalSqlServer", StringComparison.OrdinalIgnoreCase) && string.Equals(item.ProviderName, "System.Data.SqlClient", StringComparison.OrdinalIgnoreCase)) return item.ConnectionString;
            }
            throw new ConfigurationErrorsException("Không tìm thấy connection string SweetSoft_QLDA.");
        }
        private static DataTable Query(string sql, Guid userId, DateTime today, DateTime tomorrow)
        {
            DataTable table = new DataTable();
            using (SqlConnection conn = new SqlConnection(ResolveConnectionString()))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandTimeout = 30;
                cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                cmd.Parameters.Add("@Today", SqlDbType.DateTime).Value = today;
                cmd.Parameters.Add("@Tomorrow", SqlDbType.DateTime).Value = tomorrow;
                adapter.Fill(table);
            }
            return table;
        }
        private const string TasksSql = @"
SELECT DISTINCT t.IdCongViec, t.MaCongViec, t.TenCongViec, t.NgayBatDau, t.NgayKetThuc,
       d.TenDuAn, ISNULL(t.PhanTramHoanThanh,0) AS PhanTramHoanThanh
FROM dbo.TblCongViec_NhanVien tn
INNER JOIN dbo.TblCongViec t ON t.IdCongViec = tn.IdCongViec
INNER JOIN dbo.TblDuAn d ON d.IdDuAn = t.IdDuAn
WHERE tn.IdNhanVien = @UserId AND ISNULL(t.DaXoa,0) = 0 AND ISNULL(d.DaXoa,0) = 0
  AND ISNULL(t.PhanTramHoanThanh,0) < 100 AND ISNULL(t.TrangThai,0) IN (0,1)
  AND ((t.NgayBatDau IS NOT NULL AND t.NgayBatDau < @Tomorrow
        AND (t.NgayKetThuc IS NULL OR t.NgayKetThuc >= @Today))
       OR (t.NgayKetThuc IS NOT NULL AND t.NgayKetThuc < @Today))
ORDER BY t.NgayKetThuc, t.TenCongViec;";
        private const string MeetingsSql = @"
SELECT DISTINCT h.IdLichHop, h.TenCuocHop, h.ThoiGianBatDau, h.ThoiGianKetThuc,
       h.DiaDiemHop, h.LaHopOnline, d.TenDuAn
FROM dbo.TblLichHop h
LEFT JOIN dbo.TblDuAn d ON d.IdDuAn = h.IdDuAn
WHERE ISNULL(h.DaXoa,0) = 0 AND h.ThoiGianBatDau < @Tomorrow
  AND h.ThoiGianKetThuc >= @Today
  AND (h.IdNguoiTao = @UserId OR EXISTS
       (SELECT 1 FROM dbo.TblLichHop_NhanVien hn
        WHERE hn.IdLichHop = h.IdLichHop AND hn.IdNhanVien = @UserId))
ORDER BY h.ThoiGianBatDau;";
        protected void btnTodayReload_Click(object sender, EventArgs e)
        {
            var literal = FindDescendant(this, "litTodaySchedule") as Literal;
            var update = FindDescendant(this, "upTodaySchedule") as UpdatePanel;
            try
            {
                Guid userId = SweetContext.Current.UserId;
                if (userId == Guid.Empty)
                    throw new InvalidOperationException("Phiên đăng nhập không hợp lệ.");
                DateTime today = DateTime.Today;
                DataTable tasks = Query(TasksSql, userId, today, today.AddDays(1));
                DataTable meetings = Query(MeetingsSql, userId, today, today.AddDays(1));
                if (literal != null) literal.Text = BuildHtml(tasks, meetings, today);
            }
            catch (Exception ex)
            {
                SysLogger.LogError(ex, "Không thể tải lịch trình hôm nay");
                if (literal != null) literal.Text = "<div class='today-schedule-fail'>Không tải được lịch trình. Vui lòng thử lại hoặc liên hệ quản trị viên.</div>";
            }
            // Dùng luôn API của ExtraModal đang có trong dự án; JavaScript show() là fallback
            // đồng thời định vị mascot phía ngoài modal sau khi nội dung AJAX đã cập nhật.
            ExtraModal modal = FindDescendant(this, "mdlTodaySchedule") as ExtraModal;
            if (modal != null) modal.OpenModal(true);
            if (update != null)
            {
                update.Update();
                ScriptManager.RegisterStartupScript(update, update.GetType(), "TodayScheduleShow" + Guid.NewGuid().ToString("N"),
                    "if(window.TodayScheduleUI){window.TodayScheduleUI.show();}", true);
            }
        }
        private static string BuildHtml(DataTable tasks, DataTable meetings, DateTime today)
        {
            StringBuilder sb = new StringBuilder(12000);
            sb.Append("<div class='today-schedule-content' data-tasks='").Append(tasks.Rows.Count).Append("' data-meetings='").Append(meetings.Rows.Count).Append("'>");
            sb.Append("<div class='today-schedule-intro'><span class='today-schedule-intro-icon'><i class='far fa-calendar-check'></i></span><div><strong>")
              .Append(E(today.ToString("dddd", new System.Globalization.CultureInfo("vi-VN"))))
              .Append(", ").Append(today.ToString("dd/MM/yyyy")).Append("</strong><span>Những việc bạn được phân công và cuộc họp của bạn trong ngày</span></div></div>");
            sb.Append("<div class='today-schedule-grid'><section class='today-schedule-block'><div class='today-schedule-block-head'><strong><i class='fas fa-clipboard-list'></i> Công việc hôm nay</strong><small>")
  .Append(tasks.Rows.Count).Append(" công việc</small></div><div class='today-schedule-list'>");
            if (tasks.Rows.Count == 0) sb.Append("<div class='today-schedule-empty'><i class='far fa-check-circle'></i>Không có công việc đang chờ trong lịch hôm nay.</div>");
            foreach (DataRow task in tasks.Rows)
            {
                DateTime end = today; bool hasEnd = task["NgayKetThuc"] != DBNull.Value && DateTime.TryParse(Convert.ToString(task["NgayKetThuc"]), out end);
                bool overdue = hasEnd && end.Date < today;
                sb.Append("<article class='today-schedule-item'><div class='today-schedule-item-top'><span class='today-schedule-code'>").Append(E(task["MaCongViec"]))
                  .Append("</span><span class='today-schedule-time'>").Append(hasEnd ? "Hạn " + end.ToString("dd/MM") : "Đang thực hiện").Append("</span></div>")
                  .Append("<span class='today-schedule-name'>").Append(E(task["TenCongViec"])).Append("</span><div class='today-schedule-meta'><i class='fas fa-folder-open'></i>")
                  .Append(E(task["TenDuAn"])).Append("</div><span class='today-schedule-pill")
                  .Append(overdue ? " is-overdue'>Quá hạn – cần tiếp tục" : "'>Đang trong lịch làm việc").Append("</span></article>");
            }
            sb.Append("</div></section><section class='today-schedule-block'><div class='today-schedule-block-head'><strong><i class='far fa-calendar-alt'></i> Lịch họp</strong><small>")
              .Append(meetings.Rows.Count).Append(" cuộc họp</small></div><div class='today-schedule-list'>");
            if (meetings.Rows.Count == 0) sb.Append("<div class='today-schedule-empty'><i class='far fa-calendar'></i>Hôm nay bạn chưa có cuộc họp nào.</div>");
            foreach (DataRow meet in meetings.Rows)
            {
                DateTime start = Convert.ToDateTime(meet["ThoiGianBatDau"]);
                DateTime end = meet["ThoiGianKetThuc"] == DBNull.Value ? start : Convert.ToDateTime(meet["ThoiGianKetThuc"]);
                bool online = meet["LaHopOnline"] != DBNull.Value && Convert.ToBoolean(meet["LaHopOnline"]);
                string place = Convert.ToString(meet["DiaDiemHop"]);
                sb.Append("<article class='today-schedule-item'><div class='today-schedule-item-top'><span class='today-schedule-code'>")
                  .Append(online ? "HỌP ONLINE" : "HỌP TRỰC TIẾP").Append("</span><span class='today-schedule-time'>")
                  .Append(start.ToString("HH:mm")).Append(" – ").Append(end.ToString("HH:mm")).Append("</span></div>")
                  .Append("<span class='today-schedule-name'>").Append(E(meet["TenCuocHop"])).Append("</span>")
                  .Append("<div class='today-schedule-meta'><i class='fas fa-map-marker-alt'></i>").Append(E(string.IsNullOrWhiteSpace(place) ? "Chưa có địa điểm" : place)).Append("</div>")
                  .Append("<div class='today-schedule-meta'><i class='fas fa-folder-open'></i>").Append(E(meet["TenDuAn"])).Append("</div></article>");
            }
            sb.Append("</div></section></div><p class='today-schedule-note'>Công việc quá hạn chưa hoàn thành cũng được nhắc để tránh bỏ sót. Danh sách cập nhật mỗi lần bạn mở lịch trình.</p></div>");
            return sb.ToString();
        }
    }
}
