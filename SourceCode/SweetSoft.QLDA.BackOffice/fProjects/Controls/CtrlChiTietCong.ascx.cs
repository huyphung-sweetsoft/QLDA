using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.DataAccess;
using SweetSoft.QLDA.Controls;

namespace SweetSoft.QLDA.BackOffice.fProjects.Controls
{
    public partial class CtrlChiTietCong : BaseAdminUserControl
    {
        // Chỉ bản popup trong CtrlViewTaskDetail sử dụng luồng mở riêng.
        public bool EmbeddedInTaskView { get; set; }
        public string TaskModalClientId { get { return mdlPersonalDetail.ClientID; } }
        public string TaskTypeClientId { get { return hfDetailType.ClientID; } }
        public string TaskStaffClientId { get { return hfDetailStaff.ClientID; } }
        public string TaskLoadClientId { get { return btnLoadDetail.ClientID; } }
        public string TaskBodyHtmlId { get { return EmbeddedInTaskView ? "tvdLaborPersonalDetailModalBody" : "pdPersonalDetailModalBody"; } }

        public Guid ProjectId
        {
            get { return ViewState["PdProjectId"] is Guid ? (Guid)ViewState["PdProjectId"] : Guid.Empty; }
            set { ViewState["PdProjectId"] = value; }
        }

        protected string ModalClientId { get { return mdlPersonalDetail.ClientID; } }
        protected string RequestTypeId { get { return hfDetailType.ClientID; } }
        protected string RequestStaffId { get { return hfDetailStaff.ClientID; } }
        protected string LoadButtonId { get { return btnLoadDetail.ClientID; } }

        private bool CanReadProject(Guid projectId, Guid userId)
        {
            if (projectId == Guid.Empty || userId == Guid.Empty) return false;
            if (CURRENT_PAGE.IsAdministrator) return true;
            Guid? pmId = DuAnManager.Instance.LayIdNhanVienQuanLy(projectId);
            if (pmId.HasValue && pmId.Value == userId) return true;
            return ThanhVienDuAnManager.Instance.GetByNhanVienAndDuAn(userId, projectId) != null;
        }

        private bool CanReadTeam(Guid projectId)
        {
            if (CURRENT_PAGE.IsAdministrator) return true;
            Guid? pmId = DuAnManager.Instance.LayIdNhanVienQuanLy(projectId);
            return pmId.HasValue && pmId.Value == SweetContext.Current.UserId;
        }

        private bool IsAuthorizedTeamMember(Guid memberId, Guid projectId)
        {
            if (memberId == Guid.Empty || projectId == Guid.Empty) return false;
            Guid? pmId = DuAnManager.Instance.LayIdNhanVienQuanLy(projectId);
            if (pmId.HasValue && pmId.Value == memberId) return true;
            return (ThanhVienDuAnManager.Instance.GetAllActiveMemberIds(projectId) ?? new List<Guid>()).Contains(memberId);
        }

        protected void btnLoadDetail_Click(object sender, EventArgs e)
        {
            Guid projectId = ProjectId;
            Guid actor = SweetContext.Current.UserId;
            string kind = (hfDetailType.Value ?? "").Trim();
            string[] personal = { "pdPersonalTaskDetail", "pdPersonalIssueDetail", "pdPersonalHourDetail" };
            string[] teams = { "pdTeamFullDetail", "pdTeamDetail-task", "pdTeamDetail-issue" };
            ResetDetailView();
            bool isTeam = teams.Contains(kind);
            bool isPersonal = personal.Contains(kind);
            if (!isTeam && !isPersonal || !CanReadProject(projectId, actor) || isTeam && !CanReadTeam(projectId))
            {
                Response.StatusCode = 403;
                Toggle("pnlPdMessage", true); litDetailHtml.Text = "Bạn không có quyền xem bảng công này.";
                upDetail.Update();
                return;
            }
            Guid member = actor;
            if (isTeam)
            {
                if (!Guid.TryParse(hfDetailStaff.Value, out member) || !IsAuthorizedTeamMember(member, projectId))
                {
                    Toggle("pnlPdMessage", true); litDetailHtml.Text = "Nhân viên không thuộc dự án.";
                    upDetail.Update();
                    return;
                }
            }
            try
            {
                using (var connection = new SqlConnection(PersonalConnectionString()))
                {
                    connection.Open();
                    if (isTeam) BindTeamDetail(connection, projectId, member, kind);
                    else BindPersonalDetail(connection, projectId, actor, kind);
                }
            }
            catch (Exception exc)
            {
                SysLogger.LogError(exc, "Lỗi tải popup bảng công dự án");
                Toggle("pnlPdMessage", true); litDetailHtml.Text = "Không tải được dữ liệu chi tiết. Vui lòng thử lại.";
            }
            upDetail.Update();
            // Callback runs after AJAX renders Literal: no hidden HTML sources on the page.
            string callback = EmbeddedInTaskView
                ? "if(window.QLDATaskLaborLayer){window.QLDATaskLaborLayer.received();}"
                : "if(window.QLDAProjectDetailPopup){window.QLDAProjectDetailPopup.received();}";
            ScriptManager.RegisterStartupScript(upDetail, upDetail.GetType(),
                EmbeddedInTaskView ? "tvdTaskLaborLoaded" : "pdDetailLoaded", callback, true);
        }

        protected static string EncodePdAttr(object value) { return HttpUtility.HtmlAttributeEncode(Convert.ToString(value) ?? string.Empty).Replace("'", "&#39;"); }

        // View models cho Repeater: HTML/CSS ở .ascx, phép tính & truy vấn ở C#.
        public sealed class PdSummaryRow { public string Name { get; set; } public string Hours { get; set; } public bool IsTotal { get; set; } }
        public sealed class PdIssueCard { public string Name { get; set; } public string Status { get; set; } public string StatusCss { get; set; } public string Hours { get; set; } public string Created { get; set; } }
        public sealed class PdLaborRow
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public string TaskId { get; set; }
            public string IssueId { get; set; }
            public bool IsIssue { get; set; }
            public string SelfRegular { get; set; }
            public string SelfOvertime { get; set; }
            public string ApprovedRegular { get; set; }
            public string ApprovedOvertime { get; set; }
            public string Result { get; set; }
            public string Approval { get; set; }
            public string Status { get; set; }
            public string StatusCss { get; set; }
            public bool LateFinished { get; set; }
            public string HoursCompact { get; set; }
            public string DueText { get; set; }
            public string BarCss { get; set; }
            public string ProgressTooltip { get; set; }
            public int Percent { get; set; }
        }
        public sealed class PdMeetingRow { public string MeetId { get; set; } public string Name { get; set; } public string Start { get; set; } public string Duration { get; set; } public string Joined { get; set; } public string Result { get; set; } }
        public sealed class PdSection
        {
            public string Kind { get; set; }
            public string Title { get; set; }
            public bool ShowTitle { get; set; }
            public bool IsMeeting { get; set; }
            public bool IsTeamSection { get; set; }
            public List<PdLaborRow> LaborRows { get; set; }
            public List<PdMeetingRow> MeetingRows { get; set; }
        }

        private static T Ui<T>(Control root, string id) where T : Control
        {
            if (root == null) return null;
            if (root.ID == id) return root as T;
            foreach (Control child in root.Controls)
            {
                T result = Ui<T>(child, id);
                if (result != null) return result;
            }
            return null;
        }
        private void Toggle(string id, bool show)
        {
            Control control = Ui<Control>(upDetail, id);
            if (control != null) control.Visible = show;
        }
        private void BindRows<T>(string id, IEnumerable<T> rows)
        {
            Repeater repeater = Ui<Repeater>(upDetail, id);
            if (repeater != null) { repeater.DataSource = rows; repeater.DataBind(); }
        }
        private void SetText(string id, string text)
        {
            Literal literal = Ui<Literal>(upDetail, id);
            if (literal != null) literal.Text = EncodePersonal(text);
        }
        private void ResetDetailView()
        {
            litDetailHtml.Text = string.Empty;
            Toggle("pnlPdMessage", false);
            Toggle("pnlPdOverview", false); Toggle("pnlPdSummary", false); Toggle("pnlPdIssueCards", false); Toggle("pnlPdFootnote", false);
            BindRows("rptPdSections", new PdSection[0]);
            BindRows("rptPdIssueCards", new PdIssueCard[0]);
            BindRows("rptPdSummaries", new PdSummaryRow[0]);
        }
        private static string GuidColumn(DataRow row, string column)
        {
            return row.Table.Columns.Contains(column) && row[column] != DBNull.Value ? Convert.ToString(row[column]) : string.Empty;
        }
        private static PdLaborRow LaborView(DataRow row, bool issue)
        {
            PersonalTaskProgressInfo progress = !issue ? GetPersonalTaskProgress(row, DateTime.Now) : null;
            DateTime? due = !issue ? PersonalDate(row, "NgayKetThuc") : null;
            bool approved = DsResultNote(row) == "PM đã chốt";
            return new PdLaborRow
            {
                Name = PersonalText(row, issue ? "TenVanDe" : "TenCongViec"),
                Code = issue ? string.Empty : PersonalText(row, "MaCongViec"),
                TaskId = issue ? string.Empty : GuidColumn(row, "IdCongViec"),
                IssueId = issue ? GuidColumn(row, "IdVanDe") : string.Empty,
                IsIssue = issue,
                SelfRegular = DsOptionalMinute(row, "DsThuongTuKhai"),
                SelfOvertime = DsOptionalMinute(row, "DsTangCaTuKhai"),
                ApprovedRegular = approved ? DsOptionalMinute(row, "DsThuongPm") : "—",
                ApprovedOvertime = approved ? DsOptionalMinute(row, "DsTangCaPm") : "—",
                Result = DsHour(row) > 0m ? PersonalHourValue(DsHour(row)) + " giờ công" : "Chưa ghi nhận công",
                Approval = DsResultNote(row),
                Status = progress != null ? progress.Status : string.Empty,
                StatusCss = progress != null ? progress.StatusCss : string.Empty,
                LateFinished = progress != null && progress.LateFinished,
                HoursCompact = DsCompact(row),
                DueText = due.HasValue ? "Hạn " + due.Value.ToString("dd/MM/yyyy") : "Chưa có hạn",
                BarCss = progress != null ? progress.BarCss : string.Empty,
                ProgressTooltip = progress != null ? progress.Tooltip : string.Empty,
                Percent = progress != null ? progress.Percent : 0
            };
        }
        private static PdSection LaborSection(string kind, IEnumerable<DataRow> rows, decimal hours, bool team)
        {
            List<DataRow> list = rows.ToList();
            string label = kind == "task" ? "Công việc" : "Vấn đề";
            string noun = kind == "task" ? "công việc" : "vấn đề";
            return new PdSection
            {
                Kind = kind,
                IsMeeting = false,
                ShowTitle = team,
                IsTeamSection = team,
                Title = team ? label + " (" + list.Count + " " + noun + " – Tổng giờ công: " + PersonalHourText(hours) + ")" : string.Empty,
                LaborRows = list.Select(row => LaborView(row, kind == "issue")).ToList(),
                MeetingRows = new List<PdMeetingRow>()
            };
        }
        private static PdSection MeetingSection(IEnumerable<DataRow> rows, decimal hours)
        {
            List<DataRow> list = rows.ToList();
            return new PdSection
            {
                Kind = "meeting",
                ShowTitle = true,
                IsMeeting = true,
                IsTeamSection = true,
                Title = "Cuộc họp (" + list.Count + " cuộc họp – Tổng giờ công: " + PersonalHourText(hours) + ")",
                LaborRows = new List<PdLaborRow>(),
                MeetingRows = list.Select(row => new PdMeetingRow
                {
                    MeetId = GuidColumn(row, "IdLichHop"),
                    Name = PersonalText(row, "TenCuocHop"),
                    Start = PersonalDate(row, "ThoiGianBatDau").HasValue ? PersonalDate(row, "ThoiGianBatDau").Value.ToString("dd/MM/yyyy HH:mm") : "—",
                    Duration = PersonalHourText(PersonalMeetingHours(row)),
                    Joined = PersonalInt(row, "XacNhanThamGia") == 1 ? "Có tham gia" : "Không tham gia",
                    Result = TeamLaborHourText(TeamMeetingLaborHours(row))
                }).ToList()
            };
        }
        private void BindPersonalDetail(SqlConnection conn, Guid projectId, Guid userId, string kind)
        {
            bool needTasks = kind == "pdPersonalTaskDetail" || kind == "pdPersonalHourDetail";
            bool needIssues = kind == "pdPersonalIssueDetail" || kind == "pdPersonalHourDetail";
            DataTable tasks = needTasks ? LoadPersonalTasks(conn, projectId, userId) : new DataTable();
            DataTable issues = needIssues ? PersonalQuery(conn, PersonalIssuesSql, projectId, userId) : new DataTable();
            DataTable meetings = kind == "pdPersonalHourDetail" ? PersonalQuery(conn, PersonalMeetingsSql, projectId, userId) : new DataTable();
            if (needTasks || needIssues)
            {
                if (needTasks) AttachDoiSoat(tasks, LoadDoiSoat(conn, projectId, userId, false), "IdCongViec");
                if (needIssues) AttachDoiSoat(issues, LoadDoiSoat(conn, projectId, userId, true), "IdVanDe");
            }
            List<DataRow> taskRows = tasks.Rows.Cast<DataRow>().OrderBy(r => { int s = PersonalInt(r, "TrangThai"); return s == 1 ? 0 : s == 0 ? 1 : s == 2 ? 3 : 2; })
                .ThenBy(r => PersonalDate(r, "NgayKetThuc") ?? DateTime.MaxValue).ThenBy(r => PersonalText(r, "MaCongViec"), StringComparer.CurrentCultureIgnoreCase).ToList();
            List<DataRow> issueRows = issues.Rows.Cast<DataRow>().OrderByDescending(r => PersonalDate(r, "NgayTao") ?? DateTime.MinValue).ToList();
            List<PdSection> sections = new List<PdSection>();
            if (kind == "pdPersonalTaskDetail")
            {
                if (taskRows.Count == 0)
                {
                    Toggle("pnlPdMessage", true);
                    litDetailHtml.Text = "Bạn chưa được giao công việc nào trong dự án này.";
                }
                sections.Add(LaborSection("task", taskRows, DsSum(taskRows), false));
            }
            else if (kind == "pdPersonalIssueDetail")
            {
                if (issueRows.Count == 0)
                {
                    Toggle("pnlPdMessage", true);
                    litDetailHtml.Text = "Chưa có vấn đề được phân công cho bạn.";
                }
                else
                {
                    Toggle("pnlPdIssueCards", true);
                    BindRows("rptPdIssueCards", issueRows.Select(issue => {
                        int status = PersonalInt(issue, "TrangThai");
                        DateTime? created = PersonalDate(issue, "NgayTao");
                        return new PdIssueCard
                        {
                            Name = PersonalText(issue, "TenVanDe"),
                            Status = PersonalIssueStatus(status),
                            StatusCss = status == (int)TrangThaiVanDeEnum.Processed ? "is-done" : status == (int)TrangThaiVanDeEnum.Processing ? "is-progress" : "",
                            Hours = DsCompact(issue),
                            Created = created.HasValue ? "Ngày tạo " + created.Value.ToString("dd/MM/yyyy") : "Chưa có ngày tạo"
                        };
                    }).ToList());
                }
                sections.Add(LaborSection("issue", issueRows, DsSum(issueRows), false));
            }
            else
            {
                decimal taskHours = DsSum(taskRows), issueHours = DsSum(issueRows);
                decimal meetingHours = meetings.Rows.Cast<DataRow>().Sum(PersonalMeetingHours);
                decimal totalHours = DsUniqueSum(taskRows, issueRows) + meetingHours;
                Toggle("pnlPdSummary", true);
                BindRows("rptPdSummaries", new[] {
                    new PdSummaryRow {Name="Tổng giờ làm công việc",Hours=PersonalHourText(taskHours)},
                    new PdSummaryRow {Name="Tổng giờ công cuộc họp",Hours=PersonalHourText(meetingHours)},
                    new PdSummaryRow {Name="Tổng giờ công xử lý vấn đề",Hours=PersonalHourText(issueHours)},
                    new PdSummaryRow {Name="Tổng giờ công",Hours=PersonalHourText(totalHours),IsTotal=true} });
                sections.Add(LaborSection("task", taskRows, taskHours, false));
                sections.Add(LaborSection("issue", issueRows, issueHours, false));
                // Tiêu đề bảng cá nhân giống bản gốc (không thay phần popup/JS).
                sections[0].Title = "Công việc: PM chốt / tự khai"; sections[0].ShowTitle = true;
                sections[1].Title = "Vấn đề liên kết"; sections[1].ShowTitle = true;
                Toggle("pnlPdFootnote", true);
                SetText("litPdFootnote", "Một công việc có thể đồng thời liên kết vấn đề; tổng giờ cuối không cộng trùng. Cuộc họp dùng quy tắc thời lượng hiện tại.");
            }
            BindRows("rptPdSections", sections);
        }
        private void BindTeamDetail(SqlConnection conn, Guid projectId, Guid userId, string kind)
        {
            bool full = kind == "pdTeamFullDetail";
            bool needTasks = full || kind == "pdTeamDetail-task";
            bool needIssues = full || kind == "pdTeamDetail-issue";
            DataTable tasks = needTasks ? LoadPersonalTasks(conn, projectId, userId) : new DataTable();
            DataTable issues = needIssues ? PersonalQuery(conn, PersonalIssuesSql, projectId, userId) : new DataTable();
            DataTable meetings = full ? PersonalQuery(conn, TeamHoursEndedMeetingsSql, projectId, userId) : new DataTable();
            if (needTasks || needIssues)
            {
                if (needTasks) AttachDoiSoat(tasks, LoadDoiSoat(conn, projectId, userId, false), "IdCongViec");
                if (needIssues) AttachDoiSoat(issues, LoadDoiSoat(conn, projectId, userId, true), "IdVanDe");
            }
            var taskRows = tasks.Rows.Cast<DataRow>().ToList();
            var meetingRows = meetings.Rows.Cast<DataRow>().OrderByDescending(r => PersonalDate(r, "ThoiGianBatDau") ?? DateTime.MinValue).ToList();
            var issueRows = issues.Rows.Cast<DataRow>().OrderByDescending(r => PersonalDate(r, "NgayTao") ?? DateTime.MinValue).ToList();
            decimal taskHours = DsSum(taskRows), issueHours = DsSum(issueRows), meetingHours = meetingRows.Sum(TeamMeetingLaborHours);
            decimal totalHours = DsUniqueSum(taskRows, issueRows) + meetingHours;
            List<PdSection> sections = new List<PdSection>();
            if (full)
            {
                AspnetUser user = UserManager.Instance.GetUserById(userId);
                string name = user == null ? "Thành viên" : (string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName : user.DisplayName);
                Toggle("pnlPdOverview", true);
                SetText("litPdTeamName", name); SetText("litPdJumpTask", PersonalHourText(taskHours)); SetText("litPdJumpMeeting", PersonalHourText(meetingHours));
                SetText("litPdJumpIssue", PersonalHourText(totalHours - taskHours - meetingHours)); SetText("litPdJumpTotal", TeamLaborHourText(totalHours));
                sections.Add(LaborSection("task", taskRows, taskHours, true));
                sections.Add(MeetingSection(meetingRows, meetingHours));
                sections.Add(LaborSection("issue", issueRows, issueHours, true));
                Toggle("pnlPdFootnote", true);
                SetText("litPdFootnote", "Giờ PM chốt chỉ lấy từ bản đối soát hiện tại. Công vấn đề trong phép cộng là phần chưa trùng với Công việc; ở bảng Vấn đề vẫn hiện đủ giờ của công việc liên kết để đối chiếu. Cuộc họp giữ logic tham gia theo CtrlMeet.");
            }
            else
            {
                string section = kind.Substring("pdTeamDetail-".Length);
                sections.Add(section == "task" ? LaborSection("task", taskRows, taskHours, true) : LaborSection("issue", issueRows, issueHours, true));
            }
            BindRows("rptPdSections", sections);
        }

        // Các hàm truy vấn / tính / render lấy nguyên logic đối soát V15.
        private sealed class PdDoiSoat
        {
            public Guid IdDoiTuong;
            public decimal? ThuongTuKhai, TangCaTuKhai;
            public decimal ThuongPm, TangCaPm;
            public bool DaPmChot;
            public decimal PhutPm { get { return DaPmChot ? ThuongPm + TangCaPm : 0m; } }
        }

        private sealed class PersonalTaskProgressInfo
        {
            public int Percent;
            public string Status;
            public string StatusCss;
            public string BarCss;
            public string Tooltip;
            public bool Done;
            public bool LateFinished;
        }

        private static readonly string PersonalTasksSql = @"
SELECT DISTINCT t.IdCongViec,t.MaCongViec,t.TenCongViec,t.NgayBatDau,t.NgayKetThuc,
       t.TrangThai,t.PhanTramHoanThanh, {COMPLETION_DATE} AS NgayHoanThanhThucTe
FROM dbo.TblCongViec_NhanVien assign
INNER JOIN dbo.TblCongViec t ON t.IdCongViec=assign.IdCongViec
WHERE assign.IdNhanVien=@UserId AND t.IdDuAn=@ProjectId AND ISNULL(t.DaXoa,0)=0
ORDER BY t.NgayKetThuc,t.TenCongViec;";

        // Giữ TẤT CẢ lịch họp để hiển thị, nhưng giờ công chỉ ghi khi đã kết thúc và đã tham gia.
        // Override của PM có độ ưu tiên cao nhất, cùng hợp đồng với Công thành viên / CtrlMeet.
        private static readonly string PersonalMeetingsSql = @"
SELECT DISTINCT m.IdLichHop,m.TenCuocHop,m.ThoiGianBatDau,m.ThoiGianKetThuc,
       ISNULL((SELECT MAX(CAST(g.DaThamGiaOverride AS INT))
               FROM dbo.TblLichHopGioCong g
               WHERE g.IdLichHop=m.IdLichHop AND g.IdNhanVien=@UserId),
              ISNULL((SELECT MAX(CAST(ISNULL(p.XacNhanThamGia,0) AS INT))
                      FROM dbo.TblLichHop_NhanVien p
                      WHERE p.IdLichHop=m.IdLichHop AND p.IdNhanVien=@UserId),0)) AS XacNhanThamGia
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
        private static readonly string TeamHoursEndedMeetingsSql = @"
SELECT m.IdLichHop, m.TenCuocHop, m.ThoiGianBatDau, m.ThoiGianKetThuc,
       ISNULL(
         (SELECT MAX(CAST(g.DaThamGia AS INT))
          FROM dbo.TblLichHopGioCong g
          WHERE g.IdLichHop=m.IdLichHop AND g.IdNhanVien=@UserId),
         ISNULL(
           (SELECT MAX(CAST(ISNULL(p.XacNhanThamGia,0) AS INT))
            FROM dbo.TblLichHop_NhanVien p
            WHERE p.IdLichHop=m.IdLichHop AND p.IdNhanVien=@UserId),
           0)
       ) AS XacNhanThamGia
FROM dbo.TblLichHop m
WHERE m.IdDuAn=@ProjectId
  AND ISNULL(m.DaXoa,0)=0
  AND m.ThoiGianKetThuc<=GETDATE()
  AND (m.IdNguoiTao=@UserId OR EXISTS
      (SELECT 1 FROM dbo.TblLichHop_NhanVien p
       WHERE p.IdLichHop=m.IdLichHop AND p.IdNhanVien=@UserId))
ORDER BY m.ThoiGianBatDau DESC, m.IdLichHop DESC;";

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

        // KBG20: Task va Issue co DOI SOAT RIENG. Khong dung IdCongViecBiAnhHuong de lay gio Issue.
        // Chi gio do PM chot (event 4 Task; event 4/6 Issue) moi duoc tinh vao tong.
        private static Dictionary<Guid, PdDoiSoat> LoadDoiSoat(SqlConnection conn, Guid projectId, Guid userId, bool isIssue)
        {
            // Tach hai cau SQL co dieu kien co dinh, KHONG noi ten bang/column tu input.
            // LaBanHienTai=1 va latest theo doi tuong de khong cong ca ban lich su.
            const string sqlTask = @"WITH HienTai AS (
    SELECT s.IdCongViec AS IdDoiTuong, s.SoPhutThuongTuKhai, s.SoPhutTangCaTuKhai,
           s.SoPhutThuongPmChot, s.SoPhutTangCaPmChot, s.IdPmChot, s.LoaiSuKien,
           ROW_NUMBER() OVER (PARTITION BY s.IdCongViec
             ORDER BY s.LanDoiSoat DESC, s.PhienBan DESC, s.NgayTao DESC, s.IdDoiSoat DESC) AS rn
    FROM dbo.TblKhaiBaoGioDoiSoat s
    INNER JOIN dbo.TblCongViec t ON t.IdCongViec=s.IdCongViec
       AND t.IdDuAn=s.IdDuAn AND ISNULL(t.DaXoa,0)=0
    WHERE s.IdDuAn=@ProjectId AND s.IdNhanVien=@UserId
      AND s.IdCongViec IS NOT NULL AND s.IdVanDe IS NULL AND s.LaBanHienTai=1
) SELECT * FROM HienTai WHERE rn=1;";
            const string sqlIssue = @"WITH HienTai AS (
    SELECT s.IdVanDe AS IdDoiTuong, s.SoPhutThuongTuKhai, s.SoPhutTangCaTuKhai,
           s.SoPhutThuongPmChot, s.SoPhutTangCaPmChot, s.IdPmChot, s.LoaiSuKien,
           ROW_NUMBER() OVER (PARTITION BY s.IdVanDe
             ORDER BY s.LanDoiSoat DESC, s.PhienBan DESC, s.NgayTao DESC, s.IdDoiSoat DESC) AS rn
    FROM dbo.TblKhaiBaoGioDoiSoat s
    INNER JOIN dbo.TblVanDe v ON v.IdVanDe=s.IdVanDe
       AND v.IdDuAn=s.IdDuAn AND ISNULL(v.DaXoa,0)=0
    WHERE s.IdDuAn=@ProjectId AND s.IdNhanVien=@UserId
      AND s.IdVanDe IS NOT NULL AND s.IdCongViec IS NULL AND s.LaBanHienTai=1
) SELECT * FROM HienTai WHERE rn=1;";
            var result = new Dictionary<Guid, PdDoiSoat>();
            DataTable table = PersonalQuery(conn, isIssue ? sqlIssue : sqlTask, projectId, userId);
            foreach (DataRow r in table.Rows)
            {
                Guid id = (Guid)r["IdDoiTuong"];
                int evt = r["LoaiSuKien"] == DBNull.Value ? 0 : Convert.ToInt32(r["LoaiSuKien"]);
                bool hasPmMinutes = r["SoPhutThuongPmChot"] != DBNull.Value &&
                                    r["SoPhutTangCaPmChot"] != DBNull.Value;
                // Task: event 4 va IdPmChot. Issue: event 4 chot, 6 dieu chinh;
                // manager Issue luu IdNguoiThaoTacVanDe thay vi IdPmChot.
                bool approved = hasPmMinutes && (isIssue
                    ? evt == 4 || evt == 6
                    : evt == 4 && r["IdPmChot"] != DBNull.Value && (Guid)r["IdPmChot"] != Guid.Empty);
                result[id] = new PdDoiSoat
                {
                    IdDoiTuong = id,
                    DaPmChot = approved,
                    ThuongTuKhai = r["SoPhutThuongTuKhai"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["SoPhutThuongTuKhai"]),
                    TangCaTuKhai = r["SoPhutTangCaTuKhai"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["SoPhutTangCaTuKhai"]),
                    ThuongPm = approved ? Convert.ToDecimal(r["SoPhutThuongPmChot"]) : 0m,
                    TangCaPm = approved ? Convert.ToDecimal(r["SoPhutTangCaPmChot"]) : 0m
                };
            }
            return result;
        }

        private static void AttachDoiSoat(DataTable table, Dictionary<Guid, PdDoiSoat> records, string idColumn)
        {
            string[] names = { "DsThuongTuKhai", "DsTangCaTuKhai", "DsThuongPm", "DsTangCaPm", "DsPhutPm", "DsDaChot" };
            foreach (string name in names)
                if (!table.Columns.Contains(name)) table.Columns.Add(name, name == "DsDaChot" ? typeof(bool) : typeof(decimal));
            foreach (DataRow row in table.Rows)
            {
                Guid id = row.Table.Columns.Contains(idColumn) && row[idColumn] != DBNull.Value
                    ? (Guid)row[idColumn] : Guid.Empty;
                PdDoiSoat item;
                // Chua chot KHAC voi PM chot bang 0 phut. Khong lay gio tu khai lam gio PM.
                row["DsDaChot"] = records.TryGetValue(id, out item) && item.DaPmChot;
                row["DsPhutPm"] = item != null ? item.PhutPm : 0m;
                if (item == null) continue;
                if (item.DaPmChot)
                {
                    row["DsThuongPm"] = item.ThuongPm;
                    row["DsTangCaPm"] = item.TangCaPm;
                }
                if (item.ThuongTuKhai.HasValue) row["DsThuongTuKhai"] = item.ThuongTuKhai.Value;
                if (item.TangCaTuKhai.HasValue) row["DsTangCaTuKhai"] = item.TangCaTuKhai.Value;
            }
        }
        private static decimal DsMinute(DataRow row, string field)
        {
            return row.Table.Columns.Contains(field) && row[field] != DBNull.Value ? Convert.ToDecimal(row[field]) : 0m;
        }
        private static decimal DsHour(DataRow row) { return DsMinute(row, "DsPhutPm") / 60m; }
        private static string DsOptionalMinute(DataRow row, string column)
        {
            return !row.Table.Columns.Contains(column) || row[column] == DBNull.Value
                ? "Chưa ghi nhận công" : Convert.ToDecimal(row[column]) > 0m
                    ? Convert.ToDecimal(row[column]).ToString("0.##", CultureInfo.GetCultureInfo("vi-VN")) + " phút"
                    : "Chưa ghi nhận công";
        }
        private static string DsResultNote(DataRow row)
        {
            return row.Table.Columns.Contains("DsDaChot") && row["DsDaChot"] != DBNull.Value && Convert.ToBoolean(row["DsDaChot"])
                ? "PM đã chốt" : "PM chưa chốt";
        }
        private static string DsCompact(DataRow row)
        {
            return DsHour(row) > 0m ? "Giờ công: " + PersonalHourValue(DsHour(row)) : "Chưa ghi nhận công";
        }
        private static decimal DsSum(List<DataRow> rows)
        {
            // Task/Issue la hai doi tuong co phien gio doc lap; cong dong thoi van cong du ca hai.
            return rows.Sum(row => DsMinute(row, "DsPhutPm")) / 60m;
        }

        private static DataTable LoadPersonalTasks(SqlConnection connection, Guid projectId, Guid userId)
        {
            string actualDateExpr = "CAST(NULL AS DATETIME)";
            const string columnSql = @"SELECT TOP (1) COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='TblCongViec'
  AND COLUMN_NAME IN ('NgayHoanThanhThucTe', 'NgayKetThucThucTe', 'NgayHoanThanh', 'ThoiGianHoanThanh')
ORDER BY CASE COLUMN_NAME
    WHEN 'NgayHoanThanhThucTe' THEN 0
    WHEN 'NgayKetThucThucTe' THEN 1
    WHEN 'NgayHoanThanh' THEN 2
    ELSE 3 END";
            using (SqlCommand command = new SqlCommand(columnSql, connection))
            {
                object column = command.ExecuteScalar();
                if (column != null && column != DBNull.Value)
                {
                    string selected = Convert.ToString(column);
                    string[] allowed = { "NgayHoanThanhThucTe", "NgayKetThucThucTe", "NgayHoanThanh", "ThoiGianHoanThanh" };
                    if (allowed.Contains(selected)) actualDateExpr = "t.[" + selected + "]";
                }
            }
            return PersonalQuery(connection, PersonalTasksSql.Replace("{COMPLETION_DATE}", actualDateExpr), projectId, userId);
        }

        private static decimal PersonalMeetingHours(DataRow row)
        {
            DateTime? start = PersonalDate(row, "ThoiGianBatDau");
            DateTime? end = PersonalDate(row, "ThoiGianKetThuc");
            if (!start.HasValue || !end.HasValue || end.Value <= start.Value) return 0m;
            return (decimal)(end.Value - start.Value).TotalMinutes / 60m;
        }

        private static decimal PersonalMeetingLaborHours(DataRow row, DateTime now)
        {
            DateTime? end = PersonalDate(row, "ThoiGianKetThuc");
            // Cuoc hop dang dien ra/chua den ngay va nhan vien chua xac nhan deu khong tinh cong.
            return end.HasValue && end.Value <= now && PersonalInt(row, "XacNhanThamGia") == 1
                ? Math.Round(PersonalMeetingHours(row), 2, MidpointRounding.AwayFromZero) : 0m;
        }
        private static string PersonalHourValue(decimal hours)
        {
            return hours.ToString("0.##", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        }

        private static string PersonalHourText(decimal hours)
        {
            return hours > 0m ? PersonalHourValue(hours) + " giờ" : "Chưa ghi nhận công";
        }
        private static int PersonalWeekdaysBetween(DateTime from, DateTime to)
        {
            if (to.Date <= from.Date) return 0;
            DateTime first = from.Date;
            DateTime last = to.Date;
            int days = (int)Math.Min((last - first).TotalDays, 36525d);
            int fullWeeks = days / 7;
            int result = fullWeeks * 5;
            DateTime remainStart = first.AddDays(fullWeeks * 7);
            for (int i = 0; i < days % 7; i++)
            {
                DayOfWeek d = remainStart.AddDays(i).DayOfWeek;
                if (d != DayOfWeek.Saturday && d != DayOfWeek.Sunday) result++;
            }
            return result;
        }

        private static PersonalTaskProgressInfo GetPersonalTaskProgress(DataRow task, DateTime now)
        {
            int status = PersonalInt(task, "TrangThai");
            DateTime? begin = PersonalDate(task, "NgayBatDau");
            DateTime? due = PersonalDate(task, "NgayKetThuc");
            DateTime? completed = PersonalDate(task, "NgayHoanThanhThucTe");
            var info = new PersonalTaskProgressInfo();
            info.Done = status == 2 || status == 3;
            info.LateFinished = status == 3;
            info.Status = status == 0 ? "Chưa bắt đầu" : status == 1 ? "Đang thực hiện"
                : status == 2 ? "Hoàn thành" : status == 3 ? "Hoàn thành" : status == 4 ? "Chờ duyệt" : "Chưa xác định";
            info.StatusCss = status == 0 ? "pd-task-state-0" : status == 1 ? "pd-task-state-1"
                : status == 2 ? "pd-task-state-2" : status == 3 ? "pd-task-state-3" : "pd-task-state-4";
            info.BarCss = status == 0 ? "pd-task-bar-0" : status == 1 ? "pd-task-bar-1"
                : status == 2 ? "pd-task-bar-2" : status == 3 ? "pd-task-bar-3" : "pd-task-bar-4";

            if (info.Done || status == 4) info.Percent = 100;
            else if (status == 0) info.Percent = 0;
            else if (status == 1 && begin.HasValue && due.HasValue)
            {
                int total = PersonalWeekdaysBetween(begin.Value, due.Value);
                int elapsed = PersonalWeekdaysBetween(begin.Value, now.Date < due.Value.Date ? now.Date : due.Value.Date);
                info.Percent = total > 0 ? Math.Max(0, Math.Min(100, (int)Math.Round(100d * elapsed / total, MidpointRounding.AwayFromZero)))
                    : (now.Date >= due.Value.Date ? 100 : 0);
            }
            else info.Percent = Math.Max(0, Math.Min(100, PersonalInt(task, "PhanTramHoanThanh")));

            // Tooltips show the task state only; deadline information is for running tasks.
            if (info.Done) info.Tooltip = "Hoàn thành";
            else if (status == 4) info.Tooltip = "Chờ duyệt";
            else if (status == 0) info.Tooltip = "Chưa bắt đầu";
            else if (status != 1) info.Tooltip = "Chưa xác định";
            else if (!due.HasValue) info.Tooltip = "Đang thực hiện. Chưa có hạn dự kiến";
            else
            {
                int daysLeft = (due.Value.Date - now.Date).Days;
                if (daysLeft < 0)
                {
                    info.Tooltip = "Đang thực hiện. Trễ hạn " + (-daysLeft) + " ngày";
                    info.StatusCss += " pd-task-severity-late";
                    info.BarCss = "pd-task-bar-late";
                }
                else if (daysLeft == 0)
                {
                    info.Tooltip = "Đang thực hiện. Đến hạn hôm nay";
                    info.StatusCss += " pd-task-severity-soon";
                    info.BarCss = "pd-task-bar-soon";
                }
                else if (daysLeft <= 2)
                {
                    info.Tooltip = "Đang thực hiện. Sắp hạn, còn " + daysLeft + " ngày";
                    info.StatusCss += " pd-task-severity-soon";
                    info.BarCss = "pd-task-bar-soon";
                }
                else info.Tooltip = "Đang thực hiện. Còn " + daysLeft + " ngày đến hạn";
            }
            return info;
        }

        private static decimal TeamMeetingLaborHours(DataRow meeting)
        {
            return PersonalInt(meeting, "XacNhanThamGia") == 1
                ? Math.Round(PersonalMeetingHours(meeting), 2, MidpointRounding.AwayFromZero)
                : 0m;
        }

        private static string TeamLaborHourText(decimal hours)
        {
            return hours > 0m ? PersonalHourValue(hours) + " giờ công" : "Chưa ghi nhận công";
        }

        private static string TeamMeetingDurationText(DataRow meeting)
        {
            decimal duration = Math.Round(PersonalMeetingHours(meeting), 2, MidpointRounding.AwayFromZero);
            return " (" + PersonalHourValue(duration) + " giờ)";
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

    }
}
