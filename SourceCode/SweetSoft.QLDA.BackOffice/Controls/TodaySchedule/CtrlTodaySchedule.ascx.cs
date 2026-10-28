using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fMeets.Controls;
using SweetSoft.QLDA.BackOffice.fTasks.Controls;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.SysManager;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
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
        protected string SelectedDateClientId
        {
            get { Control c = FindDescendant(this, "hfScheduleSelectedDate"); return c == null ? "" : c.ClientID; }
        }
        protected string TodayOpenKindClientId { get { Control c = FindDescendant(this, "hfTodayOpenKind"); return c == null ? "" : c.ClientID; } }
        protected string TodayOpenIdClientId { get { Control c = FindDescendant(this, "hfTodayOpenId"); return c == null ? "" : c.ClientID; } }
        protected string TodayOpenButtonClientId { get { Control c = FindDescendant(this, "btnTodayOpenDetail"); return c == null ? "" : c.ClientID; } }
        protected string TodayTaskModalClientId { get { Control c = FindDescendant(this, "mdlTaskView"); return c == null ? "" : c.ClientID; } }
        protected string TodayMeetModalClientId { get { Control c = FindDescendant(this, "mdlMeetView"); return c == null ? "" : c.ClientID; } }

        // Async postback riêng để không thay HTML/scroll của popup lịch trình.
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ScriptManager manager = ScriptManager.GetCurrent(Page);
            Control openButton = FindDescendant(this, "btnTodayOpenDetail");
            if (manager != null && openButton != null) manager.RegisterAsyncPostBackControl(openButton);
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
                if (string.Equals(sql, TasksSql, StringComparison.Ordinal))
                {
                    cmd.Parameters.Add("@OnlyOpen", SqlDbType.Bit).Value = today.Date == DateTime.Today;
                }
                adapter.Fill(table);
            }
            return table;
        }
        private const string TasksSql = @"
SELECT DISTINCT t.IdCongViec, t.IdDuAn, t.MaCongViec, t.TenCongViec, t.NgayBatDau, t.NgayKetThuc,
       d.TenDuAn, ISNULL(t.PhanTramHoanThanh,0) AS PhanTramHoanThanh, ISNULL(t.TrangThai,0) AS TrangThai
FROM dbo.TblCongViec_NhanVien tn
INNER JOIN dbo.TblCongViec t ON t.IdCongViec = tn.IdCongViec
INNER JOIN dbo.TblDuAn d ON d.IdDuAn = t.IdDuAn
WHERE tn.IdNhanVien = @UserId AND ISNULL(t.DaXoa,0) = 0 AND ISNULL(d.DaXoa,0) = 0
  AND (@OnlyOpen = 0 OR (ISNULL(t.PhanTramHoanThanh,0) < 100 AND ISNULL(t.TrangThai,0) IN (0,1)))
  AND ((t.NgayBatDau IS NOT NULL AND t.NgayBatDau < @Tomorrow
        AND (t.NgayKetThuc IS NULL OR t.NgayKetThuc >= @Today))
      )
ORDER BY t.NgayKetThuc, t.TenCongViec;";
        // Các task đã quá hạn và chưa hoàn thành chỉ được truy vấn trong lịch hôm nay.
        private const string OverdueTasksSql = @"
SELECT DISTINCT t.IdCongViec, t.IdDuAn, t.MaCongViec, t.TenCongViec, t.NgayBatDau, t.NgayKetThuc,
       d.TenDuAn, ISNULL(t.PhanTramHoanThanh,0) AS PhanTramHoanThanh, ISNULL(t.TrangThai,0) AS TrangThai
FROM dbo.TblCongViec_NhanVien tn
INNER JOIN dbo.TblCongViec t ON t.IdCongViec = tn.IdCongViec
INNER JOIN dbo.TblDuAn d ON d.IdDuAn = t.IdDuAn
WHERE tn.IdNhanVien = @UserId
  AND ISNULL(t.DaXoa,0) = 0 AND ISNULL(d.DaXoa,0) = 0
  AND t.NgayKetThuc < @Today AND ISNULL(t.TrangThai,0) NOT IN (2,3)
ORDER BY t.NgayKetThuc ASC, t.TenCongViec;";
        private const string MeetingsSql = @"
SELECT DISTINCT h.IdLichHop, h.IdDuAn, h.TenCuocHop, h.ThoiGianBatDau, h.ThoiGianKetThuc,
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
                DateTime selectedDay = DateTime.Today;
                HiddenField chosen = FindDescendant(this, "hfScheduleSelectedDate") as HiddenField;
                DateTime parsed;
                if (chosen != null && !string.IsNullOrWhiteSpace(chosen.Value))
                {
                    if (!DateTime.TryParseExact(chosen.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out parsed) || parsed.Year < 1900 || parsed.Year > 2100)
                        throw new InvalidOperationException("Ngày xem lịch trình không hợp lệ.");
                    selectedDay = parsed.Date;
                }
                DataTable tasks = Query(TasksSql, userId, selectedDay, selectedDay.AddDays(1));
                DataTable meetings = Query(MeetingsSql, userId, selectedDay, selectedDay.AddDays(1));
                DataTable overdue = selectedDay == DateTime.Today
                    ? Query(OverdueTasksSql, userId, selectedDay, selectedDay.AddDays(1)) : new DataTable();
                if (literal != null) literal.Text = BuildHtml(tasks, meetings, overdue, selectedDay);
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

        // Chỉ nhận ID từ request, KHÔNG tin data-* phía client: xác minh bản ghi và
        // người đang đăng nhập có quan hệ với công việc/cuộc họp/dự án đó.
        private bool CanOpenScheduleObject(string kind, Guid id, Guid userId)
        {
            string sql;
            switch (kind)
            {
                case "task":
                    sql = @"SELECT CASE WHEN EXISTS (
                        SELECT 1 FROM dbo.TblCongViec t
                        JOIN dbo.TblCongViec_NhanVien tn ON tn.IdCongViec=t.IdCongViec
                        JOIN dbo.TblDuAn d ON d.IdDuAn=t.IdDuAn
                        WHERE t.IdCongViec=@Id AND tn.IdNhanVien=@UserId
                          AND ISNULL(t.DaXoa,0)=0 AND ISNULL(d.DaXoa,0)=0
                    ) THEN 1 ELSE 0 END";
                    break;
                case "meet":
                    sql = @"SELECT CASE WHEN EXISTS (
                        SELECT 1 FROM dbo.TblLichHop h
                        WHERE h.IdLichHop=@Id AND ISNULL(h.DaXoa,0)=0
                          AND (h.IdNguoiTao=@UserId OR EXISTS
                            (SELECT 1 FROM dbo.TblLichHop_NhanVien hn
                             WHERE hn.IdLichHop=h.IdLichHop AND hn.IdNhanVien=@UserId))
                    ) THEN 1 ELSE 0 END";
                    break;
                case "project":
                    sql = @"SELECT CASE WHEN EXISTS (
                        SELECT 1 FROM dbo.TblDuAn d WHERE d.IdDuAn=@Id AND ISNULL(d.DaXoa,0)=0
                          AND (EXISTS (SELECT 1 FROM dbo.TblCongViec t
                              JOIN dbo.TblCongViec_NhanVien tn ON tn.IdCongViec=t.IdCongViec
                              WHERE t.IdDuAn=d.IdDuAn AND ISNULL(t.DaXoa,0)=0 AND tn.IdNhanVien=@UserId)
                            OR EXISTS (SELECT 1 FROM dbo.TblLichHop h
                              WHERE h.IdDuAn=d.IdDuAn AND ISNULL(h.DaXoa,0)=0
                                AND (h.IdNguoiTao=@UserId OR EXISTS
                                  (SELECT 1 FROM dbo.TblLichHop_NhanVien hn
                                   WHERE hn.IdLichHop=h.IdLichHop AND hn.IdNhanVien=@UserId))))
                    ) THEN 1 ELSE 0 END";
                    break;
                default: return false;
            }
            using (SqlConnection conn = new SqlConnection(ResolveConnectionString()))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
                cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }

        protected void btnTodayOpenDetail_Click(object sender, EventArgs e)
        {
            HiddenField hfKind = FindDescendant(this, "hfTodayOpenKind") as HiddenField;
            HiddenField hfId = FindDescendant(this, "hfTodayOpenId") as HiddenField;
            string kind = hfKind == null ? string.Empty : (hfKind.Value ?? string.Empty).Trim().ToLowerInvariant();
            Guid id;
            if (hfId == null || !Guid.TryParse(hfId.Value, out id) || id == Guid.Empty)
                return;
            try
            {
                Guid userId = SweetContext.Current.UserId;
                if (userId == Guid.Empty || !CanOpenScheduleObject(kind, id, userId)) return;
                if (kind == "project")
                {
                    Response.Redirect(RewriteURLHelper.ProjectDetail(id));
                    return;
                }
                if (kind == "task")
                {
                    CtrlViewTaskDetail detail = FindDescendant(this, "CtrlTodayTaskDetail") as CtrlViewTaskDetail;
                    if (detail != null) detail.OpenModal(id);
                }
                else if (kind == "meet")
                {
                    CtrlViewMeetDetail detail = FindDescendant(this, "CtrlTodayMeetDetail") as CtrlViewMeetDetail;
                    if (detail != null) detail.OpenModal(id);
                }
            }
            catch (System.Threading.ThreadAbortException) { throw; } // Response.Redirect kết thúc request theo WebForms.
            catch (Exception ex) { SysLogger.LogError(ex, "Không thể mở chi tiết từ lịch trình hôm nay"); }
        }
        // Mỗi bảng giữ chung thiết kế và cùng số dòng mỗi trang như kiểu phân trang của CtrlMeet.
        // Phân trang ở client nên không postback và không truy vấn lại SQL khi bấm chuyển trang.
        private const int SchedulePageSize = 3;

        private static void AppendScheduleStart(StringBuilder sb, string kind, string title, string icon, int count, string itemLabel)
        {
            sb.Append("<section class='today-schedule-block today-schedule-block--").Append(kind)
              .Append("' data-today-block data-today-page='1' data-today-count='").Append(count)
              .Append("' data-today-size='").Append(SchedulePageSize).Append("'>")
              .Append("<div class='today-schedule-block-head'><strong><i class='").Append(icon)
              .Append("'></i> ").Append(E(title)).Append("</strong><small>").Append(count).Append(' ')
              .Append(E(itemLabel)).Append("</small></div><div class='today-schedule-list'>");
        }

        private static void AppendScheduleEnd(StringBuilder sb, int count)
        {
            sb.Append("</div><nav class='today-schedule-page' aria-label='Chuyển trang danh sách'>")
              .Append("<button type='button' data-today-page-action='prev' aria-label='Trang trước' disabled><i class='fas fa-chevron-left'></i></button>")
              .Append("<span data-today-page-label>")
              .Append(count == 0 ? "0 mục" : "1–" + Math.Min(SchedulePageSize, count) + " / " + count)
              .Append("</span><button type='button' data-today-page-action='next' aria-label='Trang sau'");
            if (count <= SchedulePageSize) sb.Append(" disabled");
            sb.Append("><i class='fas fa-chevron-right'></i></button></nav></section>");
        }

        // Business status is independent of due-date warning: use the actual TrangThaiCongViec enum.
        private static string ScheduleTaskStatus(int status, out string css)
        {
            switch (status)
            {
                case 0: css = "is-default"; return "Chưa bắt đầu";
                case 1: css = "is-in-progress"; return "Đang thực hiện";
                case 2: css = "is-completed"; return "Hoàn thành";
                case 3: css = "is-completed"; return "Hoàn thành trễ";
                case 4: css = "is-review"; return "Chờ duyệt";
                default: css = "is-default"; return "Chưa xác định";
            }
        }

        // Task status = TrangThaiCongViec (business); deadline warning is separate.
        // The deadline badge explains actual days relative to TODAY, not the viewed calendar day.
        private static void AppendTask(StringBuilder sb, DataRow task, DateTime selectedDay, int index)
        {
            DateTime end = selectedDay;
            bool hasEnd = task["NgayKetThuc"] != DBNull.Value && DateTime.TryParse(Convert.ToString(task["NgayKetThuc"]), out end);
            string taskName = Convert.ToString(task["TenCongViec"]);
            string code = Convert.ToString(task["MaCongViec"]);
            int state = task["TrangThai"] == DBNull.Value ? 0 : Convert.ToInt32(task["TrangThai"]);
            string stateCss;
            string stateText = ScheduleTaskStatus(state, out stateCss);
            bool finished = state == 2 || state == 3;
            int daysRemaining = hasEnd ? (end.Date - DateTime.Today).Days : int.MaxValue;
            bool isLate = !finished && hasEnd && daysRemaining < 0;
            bool isDueSoon = !finished && hasEnd && daysRemaining >= 0 && daysRemaining <= 2;
            string notice = isLate ? "Quá hạn" : isDueSoon ? "Sắp hạn" : string.Empty;
            string noticeCss = isLate ? "is-overdue" : "is-due-soon";
            string tooltip = isLate ? "Đã trễ hạn " + (-daysRemaining) + " ngày" :
                             daysRemaining == 0 ? "Hạn hoàn thành là hôm nay" :
                             "Còn " + daysRemaining + " ngày đến hạn";

            sb.Append("<article class='today-schedule-item' data-today-row-index='").Append(index).Append("'");
            if (index >= SchedulePageSize) sb.Append(" hidden");
            sb.Append("><div class='today-schedule-task-top'><button type='button' class='today-schedule-task-title today-schedule-link' data-today-open-kind='task' data-today-open-id='")
              .Append(E(task["IdCongViec"])).Append("' title='Xem chi tiết: ")
              .Append(HttpUtility.HtmlAttributeEncode(code + ". " + taskName))
              .Append("'><span class='today-schedule-task-code'>").Append(E(code)).Append(".</span> ")
              .Append("<strong>").Append(E(taskName)).Append("</strong></button>")
              .Append("<span class='today-schedule-task-status ").Append(stateCss)
              .Append("'>").Append(E(stateText)).Append("</span></div>")
              .Append("<div class='today-schedule-task-details'>");
            if (hasEnd)
                sb.Append("<span class='today-schedule-deadline").Append(isLate ? " is-overdue" : "")
                  .Append("'><i class='far fa-calendar-alt' aria-hidden='true'></i> Hạn ")
                  .Append(end.ToString("dd/MM/yyyy")).Append("</span>");
            else sb.Append("<span class='today-schedule-deadline'></span>");
            if (!string.IsNullOrEmpty(notice))
                sb.Append("<span class='today-schedule-task-alert ").Append(noticeCss)
                  .Append("' tabindex='0' data-today-tooltip='")
                  .Append(HttpUtility.HtmlAttributeEncode(tooltip))
                  .Append("' aria-label='").Append(HttpUtility.HtmlAttributeEncode(notice + ". " + tooltip))
                  .Append("'><i class='fas fa-exclamation-circle' aria-hidden='true'></i> ")
                  .Append(notice).Append("</span>");
            sb.Append("</div><div class='today-schedule-meta'><i class='fas fa-folder-open'></i>");
            AppendProjectLink(sb, task["IdDuAn"], task["TenDuAn"]);
            sb.Append("</div></article>");
        }

        private static void AppendProjectLink(StringBuilder sb, object projectId, object projectName)
        {
            Guid id;
            string name = Convert.ToString(projectName);
            if (projectId != DBNull.Value && Guid.TryParse(Convert.ToString(projectId), out id) && id != Guid.Empty)
            {
                sb.Append("<button type='button' class='today-schedule-project-link today-schedule-link' data-today-open-kind='project' data-today-open-id='")
                  .Append(E(id)).Append("' title='Đến chi tiết dự án'>")
                  .Append(E(name)).Append("</button>");
            }
            else sb.Append("<span>").Append(E(string.IsNullOrWhiteSpace(name) ? "Không thuộc dự án" : name)).Append("</span>");
        }

        private static string BuildHtml(DataTable tasks, DataTable meetings, DataTable overdue, DateTime selectedDay)
        {
            StringBuilder sb = new StringBuilder(15000);
            bool isToday = selectedDay.Date == DateTime.Today;
            int overdueCount = isToday ? overdue.Rows.Count : 0;
            sb.Append("<div class='today-schedule-content' data-schedule-date='").Append(selectedDay.ToString("yyyy-MM-dd"))
              .Append("' data-tasks='").Append(tasks.Rows.Count)
              .Append("' data-overdue='").Append(overdueCount)
              .Append("' data-meetings='").Append(meetings.Rows.Count).Append("'>")
              .Append("<div class='today-schedule-intro'><span class='today-schedule-intro-icon'><i class='far fa-calendar-check'></i></span><div><strong>")
              .Append(E(selectedDay.ToString("dddd", CultureInfo.GetCultureInfo("vi-VN"))))
              .Append(", ").Append(selectedDay.ToString("dd/MM/yyyy"))
              .Append("</strong><span>Những công việc được phân công và cuộc họp trong ngày được chọn</span></div></div>")
              .Append("<div class='today-schedule-grid").Append(isToday ? " has-overdue" : "")
              .Append("'>");

            if (isToday)
            {
                AppendScheduleStart(sb, "overdue", "Công việc quá hạn chưa hoàn thành", "fas fa-exclamation-triangle", overdueCount, "công việc");
                if (overdueCount == 0) sb.Append("<div class='today-schedule-empty'><i class='far fa-check-circle'></i>Không có công việc quá hạn chưa hoàn thành.</div>");
                for (int i = 0; i < overdueCount; i++) AppendTask(sb, overdue.Rows[i], selectedDay, i);
                AppendScheduleEnd(sb, overdueCount);
            }

            AppendScheduleStart(sb, "tasks", isToday ? "Công việc hôm nay" : "Công việc trong ngày", "fas fa-clipboard-list", tasks.Rows.Count, "công việc");
            if (tasks.Rows.Count == 0) sb.Append("<div class='today-schedule-empty'><i class='far fa-check-circle'></i>Không có công việc trong ngày này.</div>");
            for (int i = 0; i < tasks.Rows.Count; i++) AppendTask(sb, tasks.Rows[i], selectedDay, i);
            AppendScheduleEnd(sb, tasks.Rows.Count);

            AppendScheduleStart(sb, "meetings", "Lịch họp", "far fa-calendar-alt", meetings.Rows.Count, "cuộc họp");
            if (meetings.Rows.Count == 0) sb.Append("<div class='today-schedule-empty'><i class='far fa-calendar'></i>Ngày này bạn chưa có cuộc họp nào.</div>");
            for (int i = 0; i < meetings.Rows.Count; i++)
            {
                DataRow meet = meetings.Rows[i];
                DateTime start = Convert.ToDateTime(meet["ThoiGianBatDau"]);
                DateTime end = meet["ThoiGianKetThuc"] == DBNull.Value ? start : Convert.ToDateTime(meet["ThoiGianKetThuc"]);
                bool online = meet["LaHopOnline"] != DBNull.Value && Convert.ToBoolean(meet["LaHopOnline"]);
                string place = Convert.ToString(meet["DiaDiemHop"]);
                sb.Append("<article class='today-schedule-item' data-today-row-index='").Append(i).Append("'");
                if (i >= SchedulePageSize) sb.Append(" hidden");
                sb.Append("><div class='today-schedule-item-top'><span class='today-schedule-code'>")
                  .Append(online ? "HỌP ONLINE" : "HỌP TRỰC TIẾP").Append("</span><span class='today-schedule-time'>")
                  .Append(start.ToString("HH:mm")).Append(" – ").Append(end.ToString("HH:mm"))
                  .Append("</span></div><button type='button' class='today-schedule-name today-schedule-link' data-today-open-kind='meet' data-today-open-id='")
                  .Append(E(meet["IdLichHop"])).Append("' title='Xem chi tiết: ")
                  .Append(HttpUtility.HtmlAttributeEncode(Convert.ToString(meet["TenCuocHop"])))
                  .Append("'>").Append(E(meet["TenCuocHop"])).Append("</button>")
                  .Append("<div class='today-schedule-meta'><i class='fas fa-map-marker-alt'></i><span>")
                  .Append(E(string.IsNullOrWhiteSpace(place) ? "Chưa có địa điểm" : place))
                  .Append("</span></div><div class='today-schedule-meta'><i class='fas fa-folder-open'></i>");
                AppendProjectLink(sb, meet["IdDuAn"], meet["TenDuAn"]);
                sb.Append("</div></article>");
            }
            AppendScheduleEnd(sb, meetings.Rows.Count);
            sb.Append("</div><p class='today-schedule-note'>")
              .Append(isToday ? "Việc quá hạn chưa hoàn thành được tách riêng để bạn dễ ưu tiên xử lý. " : "Lịch của ngày được chọn. ")
              .Append("Danh sách được cập nhật mỗi lần mở lịch trình.</p></div>");
            return sb.ToString();
        }

    }
}
