using SweetCMS.Controls.Helpers;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fProjects.Controls;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fProjects
{
    public partial class DuAnDetail : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.Project;
            }
        }

        protected bool IsContractView
        {
            get
            {
                if (this.IsUserRight(ActionKeys.View, ModuleKeys.Contract))
                    return true;
                return false;
            }
        }

        private Guid IdHopDongThucHien
        {
            get
            {
                if (ViewState["IdHopDongThucHien"] != null)
                    return (Guid)ViewState["IdHopDongThucHien"];

                return Guid.Empty;
            }
            set
            {
                ViewState["IdHopDongThucHien"] = value;
            }
        }

        private Guid QueryId
        {
            get
            {
                try
                {
                    string temp = CommonHelpers.QueryString("ProjectId");
                    if (string.IsNullOrEmpty(temp))
                        return Guid.Empty;
                    return Guid.Parse(SecurityUtilities.UnprotectUrlParameter(temp));
                }
                catch
                {
                    return Guid.Empty;
                }
            }
        }

        private bool CanModifyProject
        {
            get
            {
                return this.IsEdit &&
                       CurrentStatusValue != (byte)DuAnStatus.HoanThanh &&
                       CurrentStatusValue != (byte)DuAnStatus.KetThuc;
            }
        }

        private AuditManager _auditManager;
        protected byte CurrentStatusValue { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlGiaiDoanDuAn1.IdDuAn = QueryId;
            CtrlLichSuDuAn1.IdDuAn = QueryId;
            pnlContract.Visible = this.IsContractView;
            CtrlDuAnForm1.SaveCompleted += CtrlDuAnForm1_SaveCompleted;
            CtrlChonNhanVien1.OnConfirmSelection += CtrlChonNhanVien1_OnConfirmSelection;
            if (_auditManager == null)
                _auditManager = new AuditManager(new Core.SysManager.Models.ClientInfo()
                {
                    UserId = SweetContext.Current.UserId,
                    IpAddress = SweetContext.Current.CurrentUserIp,
                    UserAgent = SweetContext.Current.CurrentUserAgent
                });
            CtrlProjectTabs1.ProjectId = QueryId;
            if (!IsPostBack)
            {
                if (ThanhVienDuAnManager.Instance.GetByNhanVienAndDuAn(SweetContext.Current.UserId, QueryId) == null && !this.IsAdministrator)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.PROJECT_LIST));
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    {RewriteURLHelper.Projects, GetResourceText(BackEndResourceKeys.PROJECT_LIST) }, {"javascript:", GetResourceText(BackEndResourceKeys.DETAIL) }
                };
                if (this.QueryId != Guid.Empty)
                {
                    BindData();
                    CtrlGiaiDoanDuAn1.InitControls();
                }
            }
        }

        protected override void BindData()
        {
            try
            {
                DataTable dt = DuAnManager.Instance.GetDetailDuAnById(QueryId);
                if (dt == null || dt.Rows.Count == 0)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), false);
                    return;
                }
                DataRow row = dt.Rows[0];
                BindProjectInformation(row);
                BindProjectTeam();
                BindRecentProjectHistory();
                BindProjectProgress();
                BindPersonalProjectView();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        // =========================================================================================
        // PHẦN VIỆC CỦA TÔI - chỉ đọc thông tin, giới hạn cả @ProjectId và @UserId.
        // Không phụ thuộc vào các bảng KhaiBaoGio: giờ công tạm hiển thị 0 theo yêu cầu.
        // =========================================================================================
        private static Control FindProjectDetailControl(Control root, string id)
        {
            if (root == null) return null;
            if (root.ID == id) return root;
            foreach (Control child in root.Controls)
            {
                Control found = FindProjectDetailControl(child, id);
                if (found != null) return found;
            }
            return null;
        }

        private static string EncodePersonal(object value)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(value) ?? string.Empty);
        }

        private static string PersonalText(DataRow row, string column)
        {
            return row != null && row.Table.Columns.Contains(column) && row[column] != DBNull.Value
                ? Convert.ToString(row[column]) : string.Empty;
        }

        private static DateTime? PersonalDate(DataRow row, string column)
        {
            DateTime result;
            return DateTime.TryParse(PersonalText(row, column), out result) ? (DateTime?)result : null;
        }

        private static int PersonalInt(DataRow row, string column)
        {
            int result;
            return int.TryParse(PersonalText(row, column), out result) ? result : 0;
        }

        private static string PersonalConnectionString()
        {
            ConnectionStringSettings named = ConfigurationManager.ConnectionStrings["SweetSoft.QLDA.BackOffice"];
            if (named != null && !string.IsNullOrWhiteSpace(named.ConnectionString)) return named.ConnectionString;
            foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.ConnectionString) &&
                    item.ConnectionString.IndexOf("SweetSoft_QLDA", StringComparison.OrdinalIgnoreCase) >= 0)
                    return item.ConnectionString;
            }
            throw new ConfigurationErrorsException("Chưa tìm thấy connection string của database SweetSoft_QLDA.");
        }

        private static DataTable PersonalQuery(SqlConnection connection, string sql, Guid projectId, Guid userId)
        {
            DataTable table = new DataTable();
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.CommandTimeout = 30;
                command.Parameters.Add("@ProjectId", SqlDbType.UniqueIdentifier).Value = projectId;
                command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                adapter.Fill(table);
            }
            return table;
        }

        private static readonly string PersonalTasksSql = @"
SELECT DISTINCT t.IdCongViec,t.MaCongViec,t.TenCongViec,t.NgayBatDau,t.NgayKetThuc,
       t.TrangThai,t.PhanTramHoanThanh
FROM dbo.TblCongViec_NhanVien assign
INNER JOIN dbo.TblCongViec t ON t.IdCongViec=assign.IdCongViec
WHERE assign.IdNhanVien=@UserId AND t.IdDuAn=@ProjectId AND ISNULL(t.DaXoa,0)=0
ORDER BY t.NgayKetThuc,t.TenCongViec;";

        private static readonly string PersonalMeetingsSql = @"
SELECT DISTINCT m.IdLichHop,m.TenCuocHop,m.ThoiGianBatDau,m.ThoiGianKetThuc
FROM dbo.TblLichHop m
WHERE m.IdDuAn=@ProjectId AND ISNULL(m.DaXoa,0)=0 AND
      (m.IdNguoiTao=@UserId OR EXISTS
        (SELECT 1 FROM dbo.TblLichHop_NhanVien attendees
         WHERE attendees.IdLichHop=m.IdLichHop AND attendees.IdNhanVien=@UserId));";

        private static readonly string PersonalIssuesSql = @"
SELECT DISTINCT v.IdVanDe,v.TenVanDe,v.TrangThai,v.MucDoAnhHuong,
       v.NgayTao,v.IdCongViecBiAnhHuong
FROM dbo.TblVanDe_NhanVien assign
INNER JOIN dbo.TblVanDe v ON v.IdVanDe=assign.IdVanDe
WHERE assign.IdNhanVien=@UserId AND v.IdDuAn=@ProjectId AND ISNULL(v.DaXoa,0)=0
ORDER BY v.NgayTao DESC,v.TenVanDe;";

        private static DateTime PersonalMeetingEnd(DataRow row)
        {
            return PersonalDate(row, "ThoiGianKetThuc") ?? PersonalDate(row, "ThoiGianBatDau") ?? DateTime.MinValue;
        }

        // Giờ công họp tạm tính theo thời lượng lịch họp (phút / 60), chưa phải giờ tham gia thực tế.
        private static decimal PersonalMeetingHours(DataRow row)
        {
            DateTime? start = PersonalDate(row, "ThoiGianBatDau");
            DateTime? end = PersonalDate(row, "ThoiGianKetThuc");
            if (!start.HasValue || !end.HasValue || end.Value <= start.Value) return 0m;
            return (decimal)(end.Value - start.Value).TotalMinutes / 60m;
        }

        // Số giờ ở badge: "Giờ công: 1". Thời lượng cuộc họp: "(1 giờ)".
        private static string PersonalHourValue(decimal hours)
        {
            return hours.ToString("0.##", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        }

        private static string PersonalHourText(decimal hours)
        {
            return PersonalHourValue(hours) + " giờ";
        }

        private static string PersonalMeetingDayText(DateTime? start, DateTime? end)
        {
            if (!start.HasValue) return "Chưa xác định ngày";
            if (end.HasValue && start.Value.Date != end.Value.Date)
                return "Ngày " + start.Value.ToString("dd/MM/yyyy") + " – " + end.Value.ToString("dd/MM/yyyy");
            return "Ngày " + start.Value.ToString("dd/MM/yyyy");
        }

        private static string PersonalMeetingTimeText(DateTime? start, DateTime? end)
        {
            if (!start.HasValue) return "Chưa xác định giờ";
            string timeRange = start.Value.ToString("HH:mm") + " – "
                + (end.HasValue ? end.Value.ToString("HH:mm") : "Chưa xác định");
            if (end.HasValue && end.Value > start.Value)
            {
                decimal durationHours = (decimal)(end.Value - start.Value).TotalMinutes / 60m;
                timeRange += " (" + PersonalHourText(durationHours) + ")";
            }
            return timeRange;
        }

        private void BindPersonalProjectView()
        {
            Literal literal = FindProjectDetailControl(this, "litProjectPersonal") as Literal;
            if (literal == null) return;
            Guid userId = SweetContext.Current.UserId;
            if (QueryId == Guid.Empty || userId == Guid.Empty)
            {
                literal.Text = "<div class='pd-personal-empty'>Không xác định được người dùng hoặc dự án.</div>";
                return;
            }
            try
            {
                using (SqlConnection connection = new SqlConnection(PersonalConnectionString()))
                {
                    connection.Open();
                    DataTable tasks = PersonalQuery(connection, PersonalTasksSql, QueryId, userId);
                    DataTable meetings = PersonalQuery(connection, PersonalMeetingsSql, QueryId, userId);
                    DataTable issues = PersonalQuery(connection, PersonalIssuesSql, QueryId, userId);
                    literal.Text = BuildPersonalHtml(tasks, meetings, issues);
                }
            }
            catch (Exception exception)
            {
                SysLogger.LogError(exception, "Không thể tải Phần việc của tôi trong DuAnDetail");
                literal.Text = "<div class='pd-personal-empty'><i class='fas fa-exclamation-circle me-2'></i>Không tải được danh sách cá nhân của dự án. Vui lòng thử tải lại trang.</div>";
            }
        }

        private string PersonalIssueStatus(int value)
        {
            try
            {
                TrangThaiVanDeEnum status = (TrangThaiVanDeEnum)value;
                string key = IssueManager.Instance.GetValueForTrangThaiVanDe(status);
                string name = GetResourceText(key);
                return string.IsNullOrWhiteSpace(name) ? "Chưa xác định" : name;
            }
            catch { return "Chưa xác định"; }
        }


        private string BuildPersonalHtml(DataTable tasks, DataTable meetings, DataTable issues)
        {
            DateTime now = DateTime.Now;
            StringBuilder builder = new StringBuilder(32000);
            var taskRows = tasks.Rows.Cast<DataRow>()
                .OrderBy(r => { int s = PersonalInt(r, "TrangThai"); return s == 1 ? 0 : (s == 0 ? 1 : (s == 2 ? 3 : 2)); })
                .ThenBy(r => PersonalDate(r, "NgayKetThuc") ?? DateTime.MaxValue)
                .ThenBy(r => PersonalText(r, "MaCongViec"), StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            var notEnded = meetings.Rows.Cast<DataRow>().Where(r => PersonalMeetingEnd(r) > now)
                .OrderBy(r => Math.Abs(((PersonalDate(r, "ThoiGianBatDau") ?? now) - now).TotalMinutes))
                .ThenBy(r => PersonalDate(r, "ThoiGianBatDau") ?? DateTime.MaxValue).ToList();
            var ended = meetings.Rows.Cast<DataRow>().Where(r => PersonalMeetingEnd(r) <= now)
                .OrderByDescending(r => PersonalDate(r, "ThoiGianBatDau") ?? DateTime.MinValue).ToList();
            var issueRows = issues.Rows.Cast<DataRow>().OrderByDescending(r => PersonalDate(r, "NgayTao") ?? DateTime.MinValue).ToList();
            decimal taskHours = 0m, issueHours = 0m; // Các mục này hiện chưa kết nối dữ liệu chấm công.
            decimal meetingHours = meetings.Rows.Cast<DataRow>().Sum(PersonalMeetingHours);
            decimal totalHours = taskHours + meetingHours + issueHours;

            builder.Append("<div class='pd-personal-grid'>");

            // Công việc
            builder.Append("<section class='pd-personal-card pd-personal-tasks' id='pdPersonalTaskCard'><div class='pd-personal-card-head'><div class='pd-head-main'><i class='fas fa-tasks'></i><strong>Công việc được giao</strong></div><div class='pd-head-actions'><small>")
                   .Append(taskRows.Count).Append(" công việc</small>")
                   .Append("<button type='button' class='pd-compact-filter' data-pd-toggle-filter='task' aria-pressed='false' title='Chỉ hiển thị công việc chưa hoàn thành'><span data-pd-filter-label>Tất cả</span><i class='fas fa-exchange-alt'></i></button>")
                   .Append("</div></div><div class='pd-card-body'><div class='pd-personal-list'>");
            if (taskRows.Count == 0) builder.Append("<div class='pd-personal-empty'>Bạn chưa được giao công việc nào trong dự án này.</div>");
            foreach (DataRow task in taskRows) AppendPersonalTaskCard(builder, task, now);
            if (taskRows.Count > 0) builder.Append("<div class='pd-personal-empty pd-filter-empty' hidden>Không có công việc chưa hoàn thành.</div>");
            builder.Append("</div></div><div class='pd-personal-card-footer'><button type='button' class='pd-detail-btn' data-pd-detail-source='pdPersonalTaskDetail' data-pd-detail-title='Chi tiết công việc được giao' data-pd-detail-sub='Danh sách công việc được phân công trực tiếp cho bạn trong dự án.'><i class='fas fa-expand-alt'></i>Xem chi tiết</button></div></section>");

            // Cuộc họp
            builder.Append("<section class='pd-personal-card pd-personal-meetings' id='pdPersonalMeetingCard'><div class='pd-personal-card-head'><div class='pd-head-main'><i class='far fa-calendar-alt'></i><strong>Cuộc họp của tôi</strong></div><div class='pd-head-actions'><small>")
                   .Append(meetings.Rows.Count).Append(" cuộc họp</small>")
                   .Append("<button type='button' class='pd-compact-filter' data-pd-toggle-filter='meeting' aria-pressed='false' title='Chỉ hiển thị cuộc họp chưa kết thúc'><span data-pd-filter-label>Tất cả</span><i class='fas fa-exchange-alt'></i></button>")
                   .Append("</div></div><div class='pd-card-body'><div class='pd-personal-list'>");
            if (meetings.Rows.Count == 0) builder.Append("<div class='pd-personal-empty'>Bạn chưa có cuộc họp nào trong dự án này.</div>");
            else
            {
                foreach (DataRow meet in notEnded) AppendPersonalMeeting(builder, meet, now, false);
                foreach (DataRow meet in ended) AppendPersonalMeeting(builder, meet, now, true);
            }
            if (meetings.Rows.Count > 0) builder.Append("<div class='pd-personal-empty pd-filter-empty' hidden>Không có cuộc họp chưa kết thúc.</div>");
            builder.Append("</div></div><div class='pd-personal-card-footer'><button type='button' class='pd-detail-btn' data-pd-detail-source='pdPersonalMeetingDetail' data-pd-detail-title='Chi tiết cuộc họp của tôi' data-pd-detail-sub='Các cuộc họp bạn chủ trì hoặc tham gia trong dự án, ưu tiên hiển thị cuộc họp chưa kết thúc.'><i class='fas fa-expand-alt'></i>Xem chi tiết</button></div></section>");

            // Vấn đề
            builder.Append("<section class='pd-personal-card pd-personal-issues' id='pdPersonalIssueCard'><div class='pd-personal-card-head'><div class='pd-head-main'><i class='fas fa-exclamation-circle'></i><strong>Vấn đề được giao</strong></div><div class='pd-head-actions'><small>")
                   .Append(issueRows.Count).Append(" vấn đề</small>")
                   .Append("<button type='button' class='pd-compact-filter' data-pd-toggle-filter='issue' aria-pressed='false' title='Chỉ hiển thị vấn đề chưa giải quyết'><span data-pd-filter-label>Tất cả</span><i class='fas fa-exchange-alt'></i></button>")
                   .Append("</div></div><div class='pd-card-body'><div class='pd-personal-list'>");
            if (issueRows.Count == 0) builder.Append("<div class='pd-personal-empty'>Chưa có vấn đề được phân công cho bạn.</div>");
            foreach (DataRow issue in issueRows) AppendPersonalIssueCard(builder, issue);
            if (issueRows.Count > 0) builder.Append("<div class='pd-personal-empty pd-filter-empty' hidden>Không có vấn đề chưa giải quyết.</div>");
            builder.Append("</div></div><div class='pd-personal-card-footer'><button type='button' class='pd-detail-btn' data-pd-detail-source='pdPersonalIssueDetail' data-pd-detail-title='Chi tiết vấn đề được giao' data-pd-detail-sub='Danh sách vấn đề bạn đang phụ trách trong dự án này.'><i class='fas fa-expand-alt'></i>Xem chi tiết</button></div></section>");

            // Giờ công
            builder.Append("<section class='pd-personal-card pd-personal-time'><div class='pd-personal-card-head'><div class='pd-head-main'><i class='far fa-clock'></i><strong>Giờ công của bạn</strong></div><div class='pd-head-actions'><small>Tạm tính</small></div></div><div class='pd-card-body'><div class='pd-personal-list'><div class='pd-personal-summary'>");
            AppendSummaryRow(builder, "Tổng giờ làm công việc", taskHours, false);
            AppendSummaryRow(builder, "Tổng giờ công cuộc họp", meetingHours, false);
            AppendSummaryRow(builder, "Tổng giờ công xử lý vấn đề", issueHours, false);
            AppendSummaryRow(builder, "Tổng giờ công", totalHours, true);
            builder.Append("</div></div></div><div class='pd-personal-card-footer'><button type='button' class='pd-detail-btn' data-pd-detail-source='pdPersonalHourDetail' data-pd-detail-title='Chi tiết giờ công của bạn' data-pd-detail-sub='Giờ họp tạm tính từ lịch họp; giờ công việc và xử lý vấn đề đang chờ kết nối dữ liệu.'><i class='fas fa-expand-alt'></i>Xem chi tiết</button></div></section>");
            builder.Append("</div>");

            // Hidden sources for popup detail.
            builder.Append("<div style='display:none'>");
            builder.Append("<div id='pdPersonalTaskDetail'><div class='pd-detail-list'>");
            if (taskRows.Count == 0) builder.Append("<div class='pd-personal-empty'>Bạn chưa được giao công việc nào trong dự án này.</div>");
            foreach (DataRow task in taskRows) AppendPersonalTaskDetail(builder, task, now);
            builder.Append("</div></div>");

            builder.Append("<div id='pdPersonalMeetingDetail'><div class='pd-detail-list'>");
            if (meetings.Rows.Count == 0) builder.Append("<div class='pd-personal-empty'>Bạn chưa có cuộc họp nào trong dự án này.</div>");
            foreach (DataRow meet in notEnded) AppendPersonalMeetingDetail(builder, meet, now, false);
            foreach (DataRow meet in ended) AppendPersonalMeetingDetail(builder, meet, now, true);
            builder.Append("</div></div>");

            builder.Append("<div id='pdPersonalIssueDetail'><div class='pd-detail-list'>");
            if (issueRows.Count == 0) builder.Append("<div class='pd-personal-empty'>Chưa có vấn đề được phân công cho bạn.</div>");
            foreach (DataRow issue in issueRows) AppendPersonalIssueDetail(builder, issue);
            builder.Append("</div></div>");

            builder.Append("<div id='pdPersonalHourDetail'><div class='pd-detail-grid'>");
            AppendSummaryRow(builder, "Tổng giờ làm công việc", taskHours, false);
            AppendSummaryRow(builder, "Tổng giờ công cuộc họp", meetingHours, false);
            AppendSummaryRow(builder, "Tổng giờ công xử lý vấn đề", issueHours, false);
            AppendSummaryRow(builder, "Tổng giờ công", totalHours, true);
            builder.Append("</div></div></div>");
            return builder.ToString();
        }

        private static void AppendSummaryRow(StringBuilder builder, string title, decimal hours, bool isTotal)
        {
            builder.Append("<div class='pd-summary-row")
                   .Append(isTotal ? " is-total" : string.Empty)
                   .Append("'><strong>")
                   .Append(HttpUtility.HtmlEncode(title))
                   .Append("</strong><span>")
                   .Append(PersonalHourText(hours))
                   .Append("</span></div>");
        }

        private static void AppendPersonalTaskCard(StringBuilder builder, DataRow task, DateTime now)
        {
            int status = PersonalInt(task, "TrangThai");
            int progress = Math.Max(0, Math.Min(100, PersonalInt(task, "PhanTramHoanThanh")));
            bool done = status == 2 || progress >= 100;
            DateTime? dueDate = PersonalDate(task, "NgayKetThuc");
            bool overdue = !done && dueDate.HasValue && dueDate.Value.Date < now.Date;
            string statusName = done ? "Đã hoàn thành" : status == 1 ? "Đang thực hiện" : status == 0 ? "Chưa bắt đầu" : "Đang xử lý";
            string statusCss = done ? "is-done" : status == 1 ? "is-progress" : string.Empty;
            builder.Append("<article class='pd-personal-item")
                   .Append(done ? " pd-task-done" : string.Empty)
                   .Append("' data-pd-record='1'><div class='pd-personal-task-header'><div class='pd-personal-task-title'><div class='pd-personal-item-title pd-personal-task-heading'><span class='pd-personal-task-code'>")
                   .Append(EncodePersonal(task["MaCongViec"]))
                   .Append("</span><span class='pd-personal-task-name'>")
                   .Append(EncodePersonal(task["TenCongViec"]))
                   .Append("</span><span class='pd-personal-tag ")
                   .Append(statusCss)
                   .Append("'>")
                   .Append(EncodePersonal(statusName))
                   .Append("</span></div><div class='pd-personal-due'>")
                   .Append(dueDate.HasValue ? "<span><i class='far fa-calendar-alt me-1'></i>Hạn " + dueDate.Value.ToString("dd/MM/yyyy") + "</span>" : "<span><i class='far fa-calendar-alt me-1'></i>Chưa có hạn</span>");
            if (overdue) builder.Append("<span class='pd-personal-tag is-overdue'>Quá hạn</span>");
            builder.Append("<span>").Append(progress).Append("%</span></div></div><span class='pd-personal-hours'><i class='far fa-clock'></i>Giờ công: 0</span></div><div class='pd-personal-progress'><span style='width:")
                   .Append(progress).Append("%'></span></div></article>");
        }

        private static void AppendPersonalMeeting(StringBuilder builder, DataRow meet, DateTime now, bool ended)
        {
            DateTime? start = PersonalDate(meet, "ThoiGianBatDau");
            DateTime? finish = PersonalDate(meet, "ThoiGianKetThuc");
            bool inProgress = !ended && start.HasValue && start.Value <= now;
            builder.Append("<article class='pd-personal-item")
                   .Append(ended ? " pd-meeting-ended" : string.Empty)
                   .Append("' data-pd-record='1'><div class='pd-personal-task-header'><div class='pd-personal-task-title'>")
                   .Append("<div class='pd-personal-item-title pd-personal-task-heading'><span class='pd-personal-task-name'>")
                   .Append(EncodePersonal(meet["TenCuocHop"]))
                   .Append("</span><span class='pd-personal-tag ")
                   .Append(ended ? string.Empty : inProgress ? "is-progress" : "is-upcoming")
                   .Append("'>")
                   .Append(ended ? "Đã kết thúc" : inProgress ? "Đang diễn ra" : "Sắp diễn ra")
                   .Append("</span></div><div class='pd-personal-schedule'>")
                   .Append("<div class='pd-personal-schedule-line'><i class='far fa-calendar-alt'></i><span>")
                   .Append(PersonalMeetingDayText(start, finish))
                   .Append("</span></div><div class='pd-personal-schedule-line'><i class='far fa-clock'></i><span>")
                   .Append(PersonalMeetingTimeText(start, finish))
                   .Append("</span></div></div></div><span class='pd-personal-hours'><i class='far fa-clock'></i>Giờ công: ")
                   .Append(PersonalHourValue(PersonalMeetingHours(meet)))
                   .Append("</span></div></article>");
        }

        private void AppendPersonalIssueCard(StringBuilder builder, DataRow issue)
        {
            int status = PersonalInt(issue, "TrangThai");
            bool processed = status == (int)TrangThaiVanDeEnum.Processed;
            bool processing = status == (int)TrangThaiVanDeEnum.Processing;
            DateTime? created = PersonalDate(issue, "NgayTao");
            builder.Append("<article class='pd-personal-item")
                   .Append(processed ? " pd-issue-processed" : string.Empty)
                   .Append("' data-pd-record='1'><div class='pd-personal-task-header'><div class='pd-personal-task-title'>")
                   .Append("<div class='pd-personal-item-title pd-personal-task-heading'><span class='pd-personal-task-name'>")
                   .Append(EncodePersonal(issue["TenVanDe"]))
                   .Append("</span><span class='pd-personal-tag ")
                   .Append(processed ? "is-done" : processing ? "is-progress" : string.Empty)
                   .Append("'>")
                   .Append(EncodePersonal(PersonalIssueStatus(status)))
                   .Append("</span></div><div class='pd-personal-meta'>")
                   .Append("<span><i class='far fa-calendar-alt me-1'></i>")
                   .Append(created.HasValue ? "Ngày tạo " + created.Value.ToString("dd/MM/yyyy") : "Chưa có ngày tạo")
                   .Append("</span></div></div><span class='pd-personal-hours'><i class='far fa-clock'></i>Giờ công: 0</span></div></article>");
        }

        private static void AppendPersonalTaskDetail(StringBuilder builder, DataRow task, DateTime now)
        {
            int status = PersonalInt(task, "TrangThai");
            int progress = Math.Max(0, Math.Min(100, PersonalInt(task, "PhanTramHoanThanh")));
            bool done = status == 2 || progress >= 100;
            DateTime? dueDate = PersonalDate(task, "NgayKetThuc");
            bool overdue = !done && dueDate.HasValue && dueDate.Value.Date < now.Date;
            string statusName = done ? "Đã hoàn thành" : status == 1 ? "Đang thực hiện" : status == 0 ? "Chưa bắt đầu" : "Đang xử lý";
            builder.Append("<div class='pd-detail-item'><div class='pd-detail-row'><div class='pd-personal-item-title'><span class='pd-personal-task-code'>")
                   .Append(EncodePersonal(task["MaCongViec"]))
                   .Append("</span>")
                   .Append(EncodePersonal(task["TenCongViec"]))
                   .Append("</div><div class='pd-personal-meeting-flags'><span class='pd-personal-tag ")
                   .Append(done ? "is-done" : status == 1 ? "is-progress" : string.Empty)
                   .Append("'>")
                   .Append(statusName)
                   .Append("</span><span class='pd-personal-hours'><i class='far fa-clock'></i>Giờ công: 0</span></div></div><div class='pd-detail-subrow'><span><i class='far fa-calendar-alt me-1'></i>")
                   .Append(dueDate.HasValue ? "Hạn " + dueDate.Value.ToString("dd/MM/yyyy") : "Chưa có hạn")
                   .Append("</span>");
            if (overdue) builder.Append("<span class='pd-personal-tag is-overdue'>Quá hạn</span>");
            builder.Append("<span>Tiến độ ").Append(progress).Append("%</span></div><div class='pd-personal-progress'><span style='width:")
                   .Append(progress).Append("%'></span></div></div>");
        }

        private static void AppendPersonalMeetingDetail(StringBuilder builder, DataRow meet, DateTime now, bool ended)
        {
            DateTime? start = PersonalDate(meet, "ThoiGianBatDau");
            DateTime? finish = PersonalDate(meet, "ThoiGianKetThuc");
            bool inProgress = !ended && start.HasValue && start.Value <= now;
            builder.Append("<div class='pd-detail-item'><div class='pd-detail-row'><div class='pd-personal-item-title'>")
                   .Append(EncodePersonal(meet["TenCuocHop"]))
                   .Append("</div><div class='pd-personal-meeting-flags'><span class='pd-personal-tag ")
                   .Append(ended ? string.Empty : inProgress ? "is-progress" : "is-upcoming")
                   .Append("'>")
                   .Append(ended ? "Đã kết thúc" : inProgress ? "Đang diễn ra" : "Sắp diễn ra")
                   .Append("</span><span class='pd-personal-hours'><i class='far fa-clock'></i>Giờ công: ")
                   .Append(PersonalHourValue(PersonalMeetingHours(meet)))
                   .Append("</span></div></div><div class='pd-personal-schedule'>")
                   .Append("<div class='pd-personal-schedule-line'><i class='far fa-calendar-alt'></i><span>")
                   .Append(PersonalMeetingDayText(start, finish))
                   .Append("</span></div><div class='pd-personal-schedule-line'><i class='far fa-clock'></i><span>")
                   .Append(PersonalMeetingTimeText(start, finish))
                   .Append("</span></div></div></div>");
        }

        private void AppendPersonalIssueDetail(StringBuilder builder, DataRow issue)
        {
            int status = PersonalInt(issue, "TrangThai");
            bool processed = status == (int)TrangThaiVanDeEnum.Processed;
            bool processing = status == (int)TrangThaiVanDeEnum.Processing;
            DateTime? created = PersonalDate(issue, "NgayTao");
            builder.Append("<div class='pd-detail-item'><div class='pd-detail-row'><div class='pd-personal-item-title'>")
                   .Append(EncodePersonal(issue["TenVanDe"]))
                   .Append("</div><div class='pd-personal-meeting-flags'><span class='pd-personal-tag ")
                   .Append(processed ? "is-done" : processing ? "is-progress" : string.Empty)
                   .Append("'>")
                   .Append(EncodePersonal(PersonalIssueStatus(status)))
                   .Append("</span><span class='pd-personal-hours'><i class='far fa-clock'></i>Giờ công: 0</span></div></div>")
                   .Append("<div class='pd-detail-subrow'><span><i class='far fa-calendar-alt me-1'></i>")
                   .Append(created.HasValue ? "Ngày tạo " + created.Value.ToString("dd/MM/yyyy") : "Chưa có ngày tạo")
                   .Append("</span></div></div>");
        }

        protected void rptRecentProjectHistory_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            DataRowView row = e.Item.DataItem as DataRowView;

            if (row == null)
                return;

            Label lblHistoryContent = e.Item.FindControl("lblHistoryContent") as Label;

            Label lblHistoryTime = e.Item.FindControl("lblHistoryTime") as Label;

            if (lblHistoryContent != null)
            {
                lblHistoryContent.Text = HttpUtility.HtmlEncode(BuildHistoryContent(row.Row));
            }

            if (lblHistoryTime != null)
            {
                lblHistoryTime.Text = FormatHistoryTime(row.Row);
            }
        }

        protected void lbtViewAllHistory_Click(object sender, EventArgs e)
        {
            CtrlLichSuDuAn1.IdDuAn = QueryId;

            CtrlLichSuDuAn1.OpenDrawer();
        }

        protected void lbtViewContract_Click(
    object sender,
    EventArgs e)
        {
            if (!this.IsView)
            {
                ShowAccessDeniedNotify();
                return;
            }

            if (this.IdHopDongThucHien ==
                Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            TblHopDongThucHien hopDong = HopDongThucHienManager.Instance.GetHopDongById(this.IdHopDongThucHien);

            if (hopDong == null)
            {
                ShowInvalidNotFoundData();
                return;
            }

            BindContractInformation(hopDong);

            Response.Redirect(RewriteURLHelper.ContractDetail(this.IdHopDongThucHien));
        }


        private void BindContractInformation(TblHopDongThucHien hopDong)
        {
            txtContractNumber.Text = hopDong.SoHopDong;
            txtContractName.Text = hopDong.TenHopDong;

            TblKhachHang khachHang = KhachHangManager.Instance.GetKhachHangById(hopDong.IdKhachHang);
            txtContractCustomer.Text = khachHang == null ? "Chưa có" : khachHang.TenKhachHang;
            txtContractValue.Text = hopDong.GiaTriHopDong.HasValue
                ? FormatHelpers.ConvertDecimalToStringByLanguage(hopDong.GiaTriHopDong.Value, "vi-VN")
                : string.Empty;
            txtContractSignDate.Text = hopDong.NgayKy.HasValue
                ? hopDong.NgayKy.Value.ToString("yyyy-MM-dd")
                : string.Empty;
            txtContractEffectiveDate.Text = hopDong.NgayHieuLuc.HasValue
                ? hopDong.NgayHieuLuc.Value.ToString("yyyy-MM-dd")
                : string.Empty;
            txtContractExpiryDate.Text = hopDong.NgayHetHan.HasValue
                ? hopDong.NgayHetHan.Value.ToString("yyyy-MM-dd")
                : string.Empty;
            txtContractDescription.Text = hopDong.MoTa;
        }

        protected void lbtEditProject_Click(object sender, EventArgs e)
        {
            Guid idDuAn = this.QueryId;

            if (idDuAn == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            CtrlDuAnForm1.OpenEdit(idDuAn);
        }

        protected void lbtThemThanhVien_Click(object sender, EventArgs e)
        {
            TblDuAn duAn = DuAnManager.Instance.GetDuAnById(this.QueryId);

            if (duAn == null || duAn.DaXoa)
            {
                ShowInvalidDataError();
                return;
            }

            if (duAn.NgayBatDau == null || duAn.NgayDuKienHoanThanh == null)
            {
                ShowNotify(GetResourceText(BackEndResourceKeys.PLEASE_SELECT_START_AND_END_DATE), MSGType.Error);
                return;
            }

            CtrlChonNhanVien1.StartDate = duAn.NgayBatDau;
            CtrlChonNhanVien1.EndDate = duAn.NgayDuKienHoanThanh;
            CtrlChonNhanVien1.SelectedUserIds = DuAnManager.Instance.GetMemberIds(duAn.IdDuAn);
            CtrlChonNhanVien1.IdNhanVienQuanLy = duAn.IdNhanVienQuanLy;
            CtrlChonNhanVien1.OpenPicker();
        }

        protected void CtrlChonNhanVien1_OnConfirmSelection(List<Guid> selectedIds)
        {
            try
            {
                TblDuAn duAn = DuAnManager.Instance.GetDuAnById(this.QueryId);
                if (duAn == null || duAn.DaXoa)
                {
                    ShowInvalidDataError();
                    return;
                }

                DuAnManager.Instance.CreateOrUpdate(duAn, selectedIds);

                BindData();
                upProjectDetail.Update();
            }
            catch (Exception ex)
            {
                ShowNotify(ex.Message, MSGType.Error);
            }
        }

        private string BuildHistoryContent(DataRow row)
        {
            string resourceKey = GetColumnText(row, "Description");

            string actor = GetColumnText(row, "ChangedBy");

            string tableName = GetColumnText(row, "TableName");

            string title = GetColumnText(row, "Title");

            if (string.IsNullOrWhiteSpace(actor) || string.Equals(actor, "[System]", StringComparison.OrdinalIgnoreCase))
            {
                actor = "Hệ thống";
            }

            if (string.IsNullOrWhiteSpace(title))
                title = "Chưa xác định";

            string entityName = GetHistoryEntityName(tableName);

            string template = string.IsNullOrWhiteSpace(resourceKey)
                    ? null
                    : GetResourceText(resourceKey);

            if (string.IsNullOrWhiteSpace(template) || string.Equals(template, resourceKey, StringComparison.OrdinalIgnoreCase))
            {
                return BuildDefaultHistoryContent(row, actor, entityName, title);
            }

            bool isContainerAction = string.Equals(resourceKey, BackEndResourceKeys.HISTORY_ADDED_TO_CONTAINER, StringComparison.OrdinalIgnoreCase) || string.Equals(resourceKey, BackEndResourceKeys.HISTORY_REMOVED_FROM_CONTAINER, StringComparison.OrdinalIgnoreCase);
            if (isContainerAction)
            {
                string containerName = GetResourceText(BackEndResourceKeys.PROJECT);
                return string.Format(template, actor,          // {0}: Administrator
                    title,          // {1}: Nguyễn Thị A
                    containerName);
            }

            try
            {
                return string.Format(template, actor, entityName, title);
            }
            catch (FormatException)
            {
                return BuildDefaultHistoryContent(row, actor, entityName, title);
            }
        }

        private string GetHistoryEntityName(string tableName)
        {
            switch (tableName)
            {
                case nameof(TblDuAn): return "dự án";

                case nameof(TblGiaiDoanDuAn): return "giai đoạn";

                case nameof(TblCongViec): return "công việc";

                case nameof(TblThanhVienDuAn): return "thành viên dự án";

                case nameof(TblHopDongThucHien): return "hợp đồng thực hiện";

                case nameof(TblRuiRoDuAn): return "rủi ro";

                case nameof(TblChiPhi): return "khoản chi";

                default: return "thông tin";
            }
        }

        private string BuildDefaultHistoryContent(DataRow row, string actor, string entityName, string title)
        {
            string actionType = GetColumnText(row, "ActionType");

            switch (actionType)
            {
                case "CREATE": return string.Format("{0} đã thêm {1} \"{2}\".", actor, entityName, title);

                case "UPDATE": return string.Format("{0} đã cập nhật {1} \"{2}\".", actor, entityName, title);

                case "DELETE": return string.Format("{0} đã xóa {1} \"{2}\".", actor, entityName, title);

                default: return string.Format("{0} đã thao tác trên {1} \"{2}\".", actor, entityName, title);
            }
        }

        private string FormatHistoryTime(DataRow row)
        {
            if (!row.Table.Columns.Contains("ChangedAt") || row["ChangedAt"] == null || row["ChangedAt"] == DBNull.Value)
            {
                return string.Empty;
            }

            DateTime changedAt = Convert.ToDateTime(row["ChangedAt"]);

            return changedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        }

        private string GetColumnText(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == null || row[columnName] == DBNull.Value)
            {
                return string.Empty;
            }

            return Convert.ToString(row[columnName]);
        }

        private void BindProjectProgress()
        {
            var tienDo = DuAnManager.Instance.GetDuAnTienDo(QueryId);

            if (tienDo.TienDoThoiGian.HasValue)
            {
                int phanTramThoiGian = (int)Math.Round(tienDo.TienDoThoiGian.Value);
                lblTienDoThoiGian.InnerText = phanTramThoiGian + "%";
                divTienDoThoiGian.Attributes["aria-valuenow"] = phanTramThoiGian.ToString();
                divTienDoThoiGianBar.Style["width"] = phanTramThoiGian + "%";
            }
            else
            {
                lblTienDoThoiGian.InnerText = "—";
                divTienDoThoiGian.Attributes["aria-valuenow"] = "0";
                divTienDoThoiGianBar.Style["width"] = "0%";
            }

            if (tienDo.TienDoCongViec.HasValue)
            {
                int phanTramCongViec = (int)Math.Round(tienDo.TienDoCongViec.Value);
                lblTienDoCongViec.InnerText = phanTramCongViec + "%";
                divTienDoCongViec.Attributes["aria-valuenow"] = phanTramCongViec.ToString();
                divTienDoCongViecBar.Style["width"] = phanTramCongViec + "%";
            }
            else
            {
                lblTienDoCongViec.InnerText = "—";
                divTienDoCongViec.Attributes["aria-valuenow"] = "0";
                divTienDoCongViecBar.Style["width"] = "0%";
            }
        }

        private void BindProjectTeam()
        {
            // 1. Get PM ID
            Guid? pmId = DuAnManager.Instance.LayIdNhanVienQuanLy(QueryId);

            if (pmId.HasValue && pmId.Value != Guid.Empty)
            {
                var pm = UserManager.Instance.GetUserById(pmId.Value);
                if (pm != null)
                {
                    lblNhanVienQuanLy.Text = pm.DisplayName;
                    imgAvatarPM.Src = !string.IsNullOrEmpty(pm.Avatar) && !pm.Avatar.Contains("no-file.png") ? pm.Avatar : "/Styles/images/user-icon.png";
                }
                else
                {
                    lblNhanVienQuanLy.Text = "Không xác định";
                    imgAvatarPM.Src = "/Styles/images/user-icon.png";
                }
            }
            else
            {
                lblNhanVienQuanLy.Text = "Chưa có Quản lý";
                imgAvatarPM.Src = "/Styles/images/user-icon.png";
            }

            // 2. Get Members
            List<Guid> memberIds = SweetSoft.QLDA.Core.Managers.ThanhVienDuAnManager.Instance.GetAllActiveMemberIds(QueryId);

            // Remove PM from members list if present
            if (pmId.HasValue)
            {
                memberIds.RemoveAll(id => id == pmId.Value);
            }

            // Distinct IDs to avoid duplicates
            memberIds = memberIds.Distinct().ToList();

            if (memberIds.Count == 0)
            {
                ltrThanhVienGroup.Text = "";
                return;
            }

            // Get User objects
            List<SweetSoft.QLDA.DataAccess.AspnetUser> members = new List<SweetSoft.QLDA.DataAccess.AspnetUser>();
            foreach (var id in memberIds)
            {
                var user = SweetSoft.QLDA.Core.Managers.UserManager.Instance.GetUserById(id);
                if (user != null)
                {
                    members.Add(user);
                }
            }

            // Render Avatar Group
            int maxVisible = 6;
            int hiddenCount = members.Count > maxVisible ? members.Count - maxVisible : 0;
            var visibleMembers = members.Take(maxVisible).ToList();

            StringBuilder sb = new StringBuilder();
            sb.Append("<div class=\"d-flex align-items-center flex-wrap gap-2\">");
            sb.Append("<div class=\"avatar-group\">");
            sb.Append("<div class=\"avatar-stack-container\">");

            foreach (var member in visibleMembers)
            {
                bool hasAvatar = !string.IsNullOrEmpty(member.Avatar) && !member.Avatar.Contains("no-file.png") && !member.Avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);
                string title = member.DisplayName;

                if (hasAvatar)
                {
                    string avatarUrl = member.Avatar.StartsWith("~")
                        ? Page.ResolveUrl(member.Avatar)
                        : member.Avatar;

                    string safeAvatarUrl = HttpUtility.HtmlAttributeEncode(avatarUrl);
                    string safeTitle = HttpUtility.HtmlAttributeEncode(title);

                    sb.Append(
                        $"<img src=\"{safeAvatarUrl}\" " +
                        $"class=\"avatar-circle\" " +
                        $"style=\"object-fit: cover;\" " +
                        $"title=\"{safeTitle}\" />"
                    );
                }
                else
                {
                    sb.Append(GetFallbackAvatarHtml(title));
                }
            }

            sb.Append("</div>"); // close avatar-stack-container

            if (hiddenCount > 0)
            {
                sb.Append($"<div class=\"avatar-circle avatar-more\">+{hiddenCount}</div>");
            }

            sb.Append("</div>"); // close avatar-group
            sb.Append("</div>"); // close d-flex gap-2

            ltrThanhVienGroup.Text = sb.ToString();
        }

        private string GetFallbackAvatarHtml(string fullName)
        {
            string initials = GetInitials(fullName);
            string bgColor = GetAvatarBackground(fullName);
            string safeName = HttpUtility.HtmlAttributeEncode(fullName);

            return $"<div class=\"avatar-circle\" " +
                   $"style=\"background-color: {bgColor};\" " +
                   $"title=\"{safeName}\">{initials}</div>";
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "?";

            string[] words = fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 1)
            {
                return words[0].Substring(0, 1).ToUpper();
            }
            else if (words.Length > 1)
            {
                return (words[0].Substring(0, 1) + words[words.Length - 1].Substring(0, 1)).ToUpper();
            }
            return "?";
        }

        private string GetAvatarBackground(string fullName)
        {
            string[] colors = { "#f59e0b", "#3b82f6", "#10b981", "#8b5cf6", "#ec4899", "#ef4444", "#06b6d4" };
            if (string.IsNullOrWhiteSpace(fullName)) return colors[0];

            int hash = Math.Abs(fullName.GetHashCode());
            return colors[hash % colors.Length];
        }

        private void BindProjectInformation(DataRow row)
        {
            Navigation1.MainTitle = GetDisplayText(row, "MaDuAn");
            lblTenDuAn.Text = GetDisplayText(row, "TenDuAn");
            lblKhachHang.Text = GetDisplayText(row, "TenKhachHang");
            lblLoaiDuAn.Text = GetDisplayText(row, "TenLoaiDuAn");
            lblSoHopDong.Text = GetDisplayText(row, "SoHopDong");
            lblGiaTriHopDong.Text = FormatMoney(row, "GiaTriHopDong");
            lblNgayKy.Text = FormatDate(row, "NgayKy");
            lblNgayBatDau.Text = FormatDate(row, "NgayBatDau");
            lblNgayHoanThanhDuKien.Text = FormatDate(row, "NgayDuKienHoanThanh");
            lblNgayKetThucThucTe.Text = FormatDate(row, "NgayHoanThanhThucTe");
            ltrMoTa.Text = GetHtmlText(row, "MoTa");
            lblNhanVienQuanLy.Text = GetDisplayText(row, "DisplayName");

            string avatarUrl = Convert.ToString(row["Avatar"]);
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                imgAvatarPM.Src = avatarUrl;
            }
            else
            {
                imgAvatarPM.Src = "~/Styles/images/user-icon.png";
            }

            byte trangThai = Convert.ToByte(row["TrangThai"]);
            switch ((DuAnStatus)trangThai)
            {
                case DuAnStatus.TamDung:
                    lblNgayKetThucThucTeTitle.Text = "Ngày tạm dừng";
                    break;
                case DuAnStatus.HoanThanh:
                    lblNgayKetThucThucTeTitle.Text = "Ngày hoàn thành thực tế";
                    break;
                case DuAnStatus.KetThuc:
                    lblNgayKetThucThucTeTitle.Text = "Ngày kết thúc";
                    break;
                default:
                    lblNgayKetThucThucTeTitle.Text = "Ngày hoàn thành thực tế";
                    break;
            }
            lblTrangThai.Text = Convert.ToString(EnumHelpers.GetERenderText(typeof(DuAnStatus), (DuAnStatus)trangThai));
            lblTrangThaiHead.Text = Convert.ToString(EnumHelpers.GetERenderText(typeof(DuAnStatus), (DuAnStatus)trangThai));
            CurrentStatusValue = trangThai;
            DuAnStatus statusEnum = (DuAnStatus)trangThai;
            //ltrCurrentStatusName.Text = EnumHelpers.GetERenderText(typeof(DuAnStatus), statusEnum);
            //iCurrentStatusIcon.Attributes["class"] = "fas fa-circle me-2 small " + GetStatusCssClass(statusEnum);
            BindStatusActions(statusEnum);

            Guid idHopDongThucHien = Guid.Empty;
            if (row.Table.Columns.Contains("IdHopDongThucHien") && row["IdHopDongThucHien"] != DBNull.Value)
            {
                Guid.TryParse(Convert.ToString(row["IdHopDongThucHien"]), out idHopDongThucHien);
            }

            this.IdHopDongThucHien = idHopDongThucHien;

            lbtViewContract.Visible = idHopDongThucHien != Guid.Empty;

            lblNoContract.Visible = idHopDongThucHien == Guid.Empty;

            lbtEditProject.Visible = CanModifyProject;

            lbtThemThanhVien.Visible = CanModifyProject;
        }

        private string GetDisplayText(DataRow row, string columnName)
        {
            string value = Convert.ToString(row[columnName]);
            if (string.IsNullOrEmpty(value))
            {
                return "Chưa có";
            }
            return HttpUtility.HtmlEncode(value);
        }

        private string GetHtmlText(DataRow row, string columnName)
        {
            string value = Convert.ToString(row[columnName]);
            if (string.IsNullOrEmpty(value))
            {
                return "<p class='text-muted'>Chưa có nội dung mô tả</p>";
            }
            return HttpUtility.HtmlDecode(value);
        }

        private string FormatDate(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == null || row[columnName] == DBNull.Value)
            {
                return "Chưa có";
            }
            DateTime value = Convert.ToDateTime(row[columnName]);
            return DateTimeHelper.ConvertDateTime(value, false);
        }

        private string FormatMoney(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == null || row[columnName] == DBNull.Value)
            {
                return "Chưa có";
            }
            decimal value = Convert.ToDecimal(row[columnName]);
            return FormatHelpers.ConvertDecimalToStringByLanguage(value, "vi-VN");
        }

        private int CalculateProgressByDay(DataRow row)
        {
            if (!row.Table.Columns.Contains("NgayBatDau") || row["NgayBatDau"] == DBNull.Value)
                return 0;
            if (!row.Table.Columns.Contains("NgayDuKienHoanThanh") || row["NgayDuKienHoanThanh"] == DBNull.Value)
                return 0;
            DateTime ngayBatDau = Convert.ToDateTime(row["NgayBatDau"]);
            DateTime ngayDuKien = Convert.ToDateTime(row["NgayDuKienHoanThanh"]);
            DateTime ngayHienTai = DateTime.Now;
            double tongNgay = (ngayDuKien - ngayBatDau).TotalDays;
            if (tongNgay <= 0) return 100;
            double ngayDaQua = (ngayHienTai - ngayBatDau).TotalDays;
            int phanTram = (int)Math.Round((ngayDaQua / tongNgay) * 100);
            return Math.Max(0, Math.Min(100, phanTram));
        }

        private void BindRecentProjectHistory()
        {
            if (rptRecentProjectHistory == null)
            {
                throw new InvalidOperationException("Không tìm thấy control rptRecentProjectHistory trong DuAnDetail.aspx.");
            }

            if (pnlEmptyRecentHistory == null)
            {
                throw new InvalidOperationException("Không tìm thấy control pnlEmptyRecentHistory trong DuAnDetail.aspx.");
            }

            DataTable history = _auditManager.GetRecentProjectHistory(QueryId, 5);

            bool hasData = history != null && history.Rows.Count > 0;

            rptRecentProjectHistory.Visible = hasData;

            pnlEmptyRecentHistory.Visible = !hasData;

            if (!hasData)
            {
                rptRecentProjectHistory.DataSource = null;

                rptRecentProjectHistory.DataBind();
                return;
            }

            rptRecentProjectHistory.DataSource = history;

            rptRecentProjectHistory.DataBind();
        }

        protected void lbtStatusDangThucHien_Click(object sender, EventArgs e)
        {
            ChangeProjectStatus(DuAnStatus.DangThucHien);
        }

        protected void lbtStatusTamDung_Click(object sender, EventArgs e)
        {
            if (!this.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            OpenStatusChangeModal(DuAnStatus.TamDung.ToString());
        }

        protected void lbtStatusHoanThanh_Click(object sender, EventArgs e)
        {
            ChangeProjectStatus(DuAnStatus.HoanThanh);
        }

        protected void lbtStatusKetThuc_Click(object sender, EventArgs e)
        {
            if (!this.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            OpenStatusChangeModal(DuAnStatus.KetThuc.ToString());
        }

        private void ChangeProjectStatus(DuAnStatus newStatus)
        {
            if (!this.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            if (QueryId == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            try
            {
                DuAnManager.Instance.UpdateProjectStatus(QueryId, newStatus, txtStatusReason.Text);
                Response.Redirect(Request.RawUrl, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                ShowNotify(ex.Message, MSGType.Error);
            }
        }

        private void BindStatusActions(DuAnStatus currentStatus)
        {
            lbtStatusDangThucHien.Visible = false;
            lbtStatusTamDung.Visible = false;
            lbtStatusHoanThanh.Visible = false;
            lbtStatusKetThuc.Visible = false;

            if (!this.IsEdit) return;

            switch (currentStatus)
            {
                case DuAnStatus.ChuaBatDau:
                    lbtStatusDangThucHien.Visible = true;
                    lbtStatusKetThuc.Visible = true;
                    break;

                case DuAnStatus.DangThucHien:
                    lbtStatusTamDung.Visible = true;
                    lbtStatusHoanThanh.Visible = true;
                    lbtStatusKetThuc.Visible = true;
                    break;

                case DuAnStatus.TamDung:
                    lbtStatusDangThucHien.Visible = true;
                    lbtStatusKetThuc.Visible = true;
                    break;
            }
        }

        protected string GetStatusCssClass(DuAnStatus status)
        {
            switch (status)
            {
                case DuAnStatus.ChuaBatDau: return "text-warning";
                case DuAnStatus.DangThucHien: return "text-info";
                case DuAnStatus.TamDung: return "text-secondary";
                case DuAnStatus.HoanThanh: return "text-success";
                case DuAnStatus.KetThuc: return "text-dark";
                default: return "text-primary";
            }
        }

        private void CtrlDuAnForm1_SaveCompleted(object sender, EventArgs e)
        {
            BindData();
            upProjectDetail.Update();
        }

        private void OpenStatusChangeModal(string targetStatus)
        {
            if (QueryId == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            TblDuAn duAn = DuAnManager.Instance.GetDuAnById(QueryId);
            if (duAn == null || duAn.DaXoa)
            {
                ShowInvalidNotFoundData();
                return;
            }

            hfStatusTarget.Value = targetStatus;
            hfStatusAtModalOpen.Value = duAn.TrangThai.ToString();
            txtStatusReason.Text = string.Empty;

            bool isPause = targetStatus == DuAnStatus.TamDung.ToString();
            mdlStatusChange.Title = isPause ? "Xác nhận tạm dừng dự án" : "Xác nhận kết thúc dự án";
            lblStatusDescription.InnerText = isPause
                ? "Dự án sẽ chuyển sang trạng thái Tạm dừng. Vui lòng nhập lý do."
                : "Dự án sẽ chuyển sang trạng thái Kết thúc. Vui lòng nhập lý do.";

            upStatusChange.Update();
            mdlStatusChange.OpenModal(true);
        }

        protected void lbtConfirmStatusChange_Click(object sender, EventArgs e)
        {
            if (!this.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            if (QueryId == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            string target = hfStatusTarget.Value;
            DuAnStatus newStatus;

            if (!Enum.TryParse(target, out newStatus) ||
                (newStatus != DuAnStatus.TamDung && newStatus != DuAnStatus.KetThuc))
            {
                ShowInvalidDataError();
                return;
            }

            string reason = (txtStatusReason.Text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(reason))
            {
                ShowNotify("Vui lòng nhập lý do.", MSGType.Warning);
                mdlStatusChange.OpenModal(true);
                return;
            }

            if (reason.Length > 1000)
            {
                ShowNotify("Lý do không được vượt quá 1000 ký tự.", MSGType.Warning);
                mdlStatusChange.OpenModal(true);
                return;
            }

            try
            {
                TblDuAn duAn = DuAnManager.Instance.GetDuAnById(QueryId);

                if (duAn == null || duAn.DaXoa)
                {
                    ShowInvalidNotFoundData();
                    return;
                }

                byte statusAtModalOpen;
                if (!byte.TryParse(hfStatusAtModalOpen.Value, out statusAtModalOpen) ||
                    duAn.TrangThai != statusAtModalOpen)
                {
                    ShowNotify("Trạng thái dự án đã thay đổi. Vui lòng tải lại trang.", MSGType.Warning);
                    return;
                }

                // Chỉ cho phép hai thao tác mà modal này phục vụ.
                if (newStatus == DuAnStatus.TamDung && duAn.TrangThai != (byte)DuAnStatus.DangThucHien)
                {
                    ShowNotify("Chỉ có thể tạm dừng dự án đang thực hiện.", MSGType.Warning);
                    return;
                }

                if (newStatus == DuAnStatus.KetThuc &&
                    duAn.TrangThai != (byte)DuAnStatus.ChuaBatDau &&
                    duAn.TrangThai != (byte)DuAnStatus.DangThucHien &&
                    duAn.TrangThai != (byte)DuAnStatus.TamDung)
                {
                    ShowNotify("Không thể kết thúc dự án ở trạng thái hiện tại.", MSGType.Warning);
                    return;
                }

                // TODO: Lưu reason cùng lần cập nhật trạng thái ở DuAnManager.
                DuAnManager.Instance.UpdateProjectStatus(QueryId, newStatus, reason);

                BindData();
                upProjectDetail.Update();
                mdlStatusChange.CloseModal();
            }
            catch (Exception ex)
            {
                ShowNotify(ex.Message, MSGType.Error);
                mdlStatusChange.OpenModal(true);
            }
        }
    }
}

