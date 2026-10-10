using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fEmployeeDashboard.Controls
{
    public partial class CtrlEmployeeDashboard : UserControl
    {
        private string _connectionString;
        private DateTime _weekStart;
        private DateTime _weekEnd;
        private readonly CultureInfo _viCulture = CultureInfo.GetCultureInfo("vi-VN");
        private const int TaskPreviewCount = 5;
        private const int ProjectPreviewCount = 5;
        private const int IssuePreviewCount = 4;
        private const int RiskPreviewCount = 4;

        public string TaskChartGradient { get; set; } = "conic-gradient(#e2e8f0 0% 100%)";
        public string ProjectChartGradient { get; set; } = "conic-gradient(#e2e8f0 0% 100%)";

        public Guid? FilterProjectId
        {
            get { return ViewState["FilterProjectId"] as Guid?; }
            set { ViewState["FilterProjectId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FilterProjectId = null;
                LoadDashboard();
            }
        }

        private void LoadDashboard()
        {
            DateTime requestedDate;
            if (!DateTime.TryParseExact(Request.QueryString["week"], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out requestedDate))
                requestedDate = DateTime.Today;

            _weekStart = GetMonday(requestedDate.Date);
            _weekEnd = _weekStart.AddDays(6);
            SetWeekNavigation();

            try
            {
                _connectionString = ResolveConnectionString();
                Guid userId;
                if (!TryResolveCurrentUserId(out userId))
                {
                    ShowDashboardMessage("Không xác định được tài khoản nhân viên đang đăng nhập.");
                    BindEmptyDashboard();
                    return;
                }

                // SỬA LẠI: Join với TblLoai để lấy tên Phòng Ban và Chức Danh thay vì hiển thị ID GUID
                DataTable profileRows = Query(@"
SELECT TOP (1) u.UserId, u.UserName, u.DisplayName, u.Avatar, u.IsDeleted, u.IsActivated, u.LaNhanVien,
       u.IdPhongBan, pb.TenLoai AS TenPhongBan,
       u.IdChucDanh, cd.TenLoai AS TenChucDanh,
       u.NgayGiaNhap
FROM dbo.aspnet_Users u
LEFT JOIN dbo.TblLoai pb ON u.IdPhongBan = pb.IdLoai
LEFT JOIN dbo.TblLoai cd ON u.IdChucDanh = cd.IdLoai
WHERE u.UserId = @UserId AND ISNULL(u.IsDeleted, 0) = 0;", GuidParameter("@UserId", userId));

                if (profileRows.Rows.Count == 0)
                {
                    ShowDashboardMessage("Không tìm thấy hồ sơ nhân viên.");
                    BindEmptyDashboard();
                    return;
                }

                DataRow profile = profileRows.Rows[0];
                BindProfile(profile);

                List<ProjectItem> projects = LoadProjects(userId);
                List<TaskItem> allTasks = LoadTasks(userId);
                List<IssueItem> issues = LoadIssues(userId);
                List<RiskItem> risks = LoadRisks(userId);
                List<MeetingItem> meetings = LoadMeetings(userId);
                List<ExceptionItem> exceptions = LoadExceptions();
                Dictionary<DayOfWeek, WorkdayConfiguration> workdayConfigurations = LoadWorkdayConfigurations();
                List<CalendarDayItem> days = BuildCalendarDays(allTasks, meetings, exceptions, workdayConfigurations);

                // --- TÍNH TOÁN BIỂU ĐỒ ---
                int totalT = allTasks.Count;
                int overdueT = allTasks.Count(x => x.IsOverdue);
                int dueSoonT = allTasks.Count(x => x.IsDueSoon);
                int normalT = totalT - overdueT - dueSoonT;

                lblTotalTasksChart.Text = totalT.ToString();
                lblTaskOverdueChart.Text = overdueT.ToString();
                lblTaskDueSoonChart.Text = dueSoonT.ToString();
                lblTaskNormalChart.Text = normalT.ToString();
                divTaskChart.Style["background"] = GenerateConicGradient(overdueT, dueSoonT, normalT, totalT);

                int totalP = projects.Count;
                int overdueP = projects.Count(x => x.IsOverdue);
                int dueSoonP = projects.Count(x => x.IsDueSoon);
                int normalP = totalP - overdueP - dueSoonP;

                lblTotalProjectsChart.Text = totalP.ToString();
                lblProjectOverdueChart.Text = overdueP.ToString();
                lblProjectDueSoonChart.Text = dueSoonP.ToString();
                lblProjectNormalChart.Text = normalP.ToString();
                divProjectChart.Style["background"] = GenerateConicGradient(overdueP, dueSoonP, normalP, totalP);

                // --- LỌC TASK CHO BẢNG CÔNG VIỆC ---
                List<TaskItem> panelTasks;
                if (FilterProjectId.HasValue)
                {
                    panelTasks = LoadTasksByProject(userId, FilterProjectId.Value);
                    var selectedProject = projects.FirstOrDefault(p => p.Id == FilterProjectId.Value);
                    if (selectedProject != null)
                    {
                        lblProjectFilterName.Text = "<i class=\"fas fa-folder-open\"></i> " + selectedProject.Name;
                        lblProjectFilterName.Visible = true;
                        btnAttentionTasks.CssClass = "ed-tab-btn";
                    }
                }
                else
                {
                    panelTasks = allTasks;
                    lblProjectFilterName.Visible = false;
                    btnAttentionTasks.CssClass = "ed-tab-btn active";
                }

                // --- BINDING ---
                rptProjects.DataSource = projects.Take(ProjectPreviewCount).ToList();
                rptProjects.DataBind();
                rptProjectsAll.DataSource = projects;
                rptProjectsAll.DataBind();

                rptTasks.DataSource = panelTasks.Take(TaskPreviewCount).ToList();
                rptTasks.DataBind();
                rptTasksAll.DataSource = panelTasks;
                rptTasksAll.DataBind();

                rptIssues.DataSource = issues.Take(IssuePreviewCount).ToList();
                rptIssues.DataBind();
                rptIssuesAll.DataSource = issues;
                rptIssuesAll.DataBind();

                rptRisks.DataSource = risks.Take(RiskPreviewCount).ToList();
                rptRisks.DataBind();
                rptRisksAll.DataSource = risks;
                rptRisksAll.DataBind();

                rptDays.DataSource = days;
                rptDays.DataBind();
                rptCalendarAll.DataSource = days;
                rptCalendarAll.DataBind();

                pnlNoProjects.Visible = projects.Count == 0;
                pnlNoTasks.Visible = panelTasks.Count == 0;
                pnlNoAllTasks.Visible = panelTasks.Count == 0;
                pnlNoIssues.Visible = issues.Count == 0;
                pnlNoRisks.Visible = risks.Count == 0;

                lblProjectCount.Text = projects.Count.ToString();
                lblTaskCount.Text = allTasks.Count.ToString();
                lblOverdueCount.Text = allTasks.Count(x => x.IsOverdue).ToString();
                lblIssueCount.Text = issues.Count.ToString();

                upnlMainLayout.Update();
            }
            catch (Exception ex)
            {
                Trace.Warn("EmployeeDashboard", "Không tải được dashboard nhân viên.", ex);
                ShowDashboardMessage("Lỗi hệ thống: " + ex.GetBaseException().Message);
                BindEmptyDashboard();
            }
        }

        // --- CÁC SỰ KIỆN MỞ EXTRAMODAL ---
        protected void lnkMoreProjects_Click(object sender, EventArgs e)
        {
            upnlModalProjects.Update();
            mdlProjects.OpenModal(true);
        }

        protected void lnkMoreTasks_Click(object sender, EventArgs e)
        {
            upnlModalTasks.Update();
            mdlTasks.OpenModal(true);
        }

        protected void lnkCalendar_Click(object sender, EventArgs e)
        {
            upnlModalCalendar.Update();
            mdlCalendar.OpenModal(true);
        }

        private void PrepareModalQueryContext()
        {
            DateTime requestedDate;
            if (!DateTime.TryParseExact(Request.QueryString["week"], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out requestedDate))
                requestedDate = DateTime.Today;

            _weekStart = GetMonday(requestedDate.Date);
            _weekEnd = _weekStart.AddDays(6);
            _connectionString = ResolveConnectionString();
        }

        protected void lnkMoreIssues_Click(object sender, EventArgs e)
        {
            try
            {
                PrepareModalQueryContext();
                Guid userId;
                if (TryResolveCurrentUserId(out userId))
                {
                    rptIssuesAll.DataSource = LoadIssues(userId);
                    rptIssuesAll.DataBind();
                }
            }
            catch (Exception ex)
            {
                Trace.Warn("EmployeeDashboard", "Không tải được danh sách vấn đề cho popup.", ex);
            }

            upnlModalIssues.Update();
            mdlIssues.OpenModal(true);
        }

        protected void lnkMoreRisks_Click(object sender, EventArgs e)
        {
            try
            {
                PrepareModalQueryContext();
                Guid userId;
                if (TryResolveCurrentUserId(out userId))
                {
                    rptRisksAll.DataSource = LoadRisks(userId);
                    rptRisksAll.DataBind();
                }
            }
            catch (Exception ex)
            {
                Trace.Warn("EmployeeDashboard", "Không tải được danh sách rủi ro cho popup.", ex);
            }

            upnlModalRisks.Update();
            mdlRisks.OpenModal(true);
        }

        // Bắt sự kiện người dùng bấm Nút "Công việc cần chú ý" -> Hủy Lọc
        protected void btnAttentionTasks_Click(object sender, EventArgs e)
        {
            FilterProjectId = null;
            LoadDashboard();
        }

        // Bắt sự kiện người dùng bấm "Tên Dự Án" -> Gán biến Lọc
        protected void rptProjects_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "FILTER_TASKS")
            {
                FilterProjectId = Guid.Parse(e.CommandArgument.ToString());
                LoadDashboard();
            }
        }

        protected void rptProjectsAll_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "FILTER_TASKS")
            {
                FilterProjectId = Guid.Parse(e.CommandArgument.ToString());
                LoadDashboard();
                mdlProjects.CloseModal();
            }
        }

        private string GenerateConicGradient(int overdue, int dueSoon, int normal, int total)
        {
            if (total == 0) return "conic-gradient(#f1f5f9 0% 100%)";

            double pOverdue = (double)overdue / total * 100;
            double pDueSoon = (double)dueSoon / total * 100;

            double endOverdue = pOverdue;
            double endDueSoon = endOverdue + pDueSoon;

            string s1 = endOverdue.ToString(CultureInfo.InvariantCulture);
            string s2 = endDueSoon.ToString(CultureInfo.InvariantCulture);

            return $"conic-gradient(#ef4444 0% {s1}%, #f59e0b {s1}% {s2}%, #3b82f6 {s2}% 100%)";
        }

        private void SetWeekNavigation()
        {
            lblWeekRange.Text = _weekStart.ToString("dd/MM/yyyy", _viCulture) + " – " + _weekEnd.ToString("dd/MM/yyyy", _viCulture);
            lnkPreviousWeek.NavigateUrl = WeekUrl(_weekStart.AddDays(-7));
            lnkNextWeek.NavigateUrl = WeekUrl(_weekStart.AddDays(7));
            lnkCurrentWeek.NavigateUrl = WeekUrl(DateTime.Today);
            bool isCurrentWeek = _weekStart == GetMonday(DateTime.Today);
            lnkCurrentWeek.CssClass = isCurrentWeek ? "ed-week-btn ed-current" : "ed-week-btn";
            lnkCurrentWeek.ToolTip = isCurrentWeek ? "Bạn đang xem tuần hiện tại" : "Quay về tuần hiện tại";
        }

        private string WeekUrl(DateTime date)
        {
            string path = Request.Url.AbsolutePath;
            return path + "?week=" + HttpUtility.UrlEncode(GetMonday(date.Date).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        private void BindProfile(DataRow row)
        {
            string displayName = Value(row, "DisplayName");
            string userName = Value(row, "UserName");
            if (String.IsNullOrWhiteSpace(displayName)) displayName = userName;
            if (String.IsNullOrWhiteSpace(displayName)) displayName = "Nhân viên";

            lblDisplayName.Text = Server.HtmlEncode(displayName);
            lblAvatarFallback.Text = Server.HtmlEncode(GetInitials(displayName));

            string avatar = Value(row, "Avatar");
            if (!String.IsNullOrWhiteSpace(avatar))
            {
                if (avatar.StartsWith("~/", StringComparison.Ordinal))
                    imgAvatar.ImageUrl = ResolveUrl(avatar);
                else if (Uri.IsWellFormedUriString(avatar, UriKind.Absolute) || avatar.StartsWith("/", StringComparison.Ordinal))
                    imgAvatar.ImageUrl = avatar;
                else
                    imgAvatar.ImageUrl = ResolveUrl("~/" + avatar.TrimStart('/', '\\'));
                imgAvatar.Visible = true;
                lblAvatarFallback.Visible = false;
            }
            else
            {
                imgAvatar.Visible = false;
                lblAvatarFallback.Visible = true;
            }

            // Bind trực tiếp Tên phòng ban và Chức danh từ câu SQL JOIN
            string departmentName = Value(row, "TenPhongBan");
            string positionName = Value(row, "TenChucDanh");
            phDepartment.Visible = !String.IsNullOrWhiteSpace(departmentName);
            phPosition.Visible = !String.IsNullOrWhiteSpace(positionName);
            lblDepartment.Text = Server.HtmlEncode(departmentName);
            lblPosition.Text = Server.HtmlEncode(positionName);
        }

        private List<ProjectItem> LoadProjects(Guid userId)
        {
            DataTable table = Query(@"
SELECT d.IdDuAn, d.MaDuAn, d.TenDuAn, d.NgayBatDau, d.NgayDuKienHoanThanh,
       CONVERT(nvarchar(100), d.TrangThai) AS TrangThai,
       (SELECT COUNT(1) FROM dbo.TblCongViec t WHERE t.IdDuAn = d.IdDuAn AND ISNULL(t.DaXoa, 0) = 0) AS TaskTotal,
       (SELECT COUNT(1) FROM dbo.TblCongViec t WHERE t.IdDuAn = d.IdDuAn AND ISNULL(t.DaXoa, 0) = 0 AND ISNULL(t.PhanTramHoanThanh, 0) >= 100) AS TaskCompleted
FROM dbo.TblThanhVienDuAn tv
INNER JOIN dbo.TblDuAn d ON d.IdDuAn = tv.IdDuAn
WHERE tv.IdNhanVien = @UserId AND ISNULL(tv.DaXoa, 0) = 0 AND ISNULL(d.DaXoa, 0) = 0
  AND EXISTS (
      SELECT 1 FROM dbo.TblCongViec_NhanVien tn
      INNER JOIN dbo.TblCongViec t ON t.IdCongViec = tn.IdCongViec
      WHERE tn.IdNhanVien = @UserId AND t.IdDuAn = d.IdDuAn
        AND ISNULL(t.DaXoa, 0) = 0
        AND ISNULL(t.PhanTramHoanThanh, 0) < 100
  )
GROUP BY d.IdDuAn, d.MaDuAn, d.TenDuAn, d.NgayBatDau, d.NgayDuKienHoanThanh, d.TrangThai
ORDER BY CASE WHEN d.NgayDuKienHoanThanh IS NULL THEN 1 ELSE 0 END,
         d.NgayDuKienHoanThanh ASC, d.TenDuAn ASC;", GuidParameter("@UserId", userId));

            List<ProjectItem> items = new List<ProjectItem>();
            foreach (DataRow row in table.Rows)
            {
                int total = IntValue(row["TaskTotal"]);
                int completed = IntValue(row["TaskCompleted"]);
                int progress = total == 0 ? 0 : (int)Math.Round(completed * 100.0 / total, MidpointRounding.AwayFromZero);

                DateTime deadlineDate;
                bool hasDeadline = TryDate(row["NgayDuKienHoanThanh"], out deadlineDate);
                bool projectOverdue = hasDeadline && deadlineDate.Date < DateTime.Today && progress < 100;
                bool projectDueSoon = hasDeadline && deadlineDate.Date >= DateTime.Today && (deadlineDate.Date - DateTime.Today).TotalDays <= 2 && progress < 100;

                items.Add(new ProjectItem
                {
                    Id = GuidValue(row["IdDuAn"]),
                    Code = FirstNonEmpty(Value(row, "MaDuAn"), "Dự án"),
                    Name = FirstNonEmpty(Value(row, "TenDuAn"), "Chưa đặt tên dự án"),
                    Progress = Math.Max(0, Math.Min(100, progress)),
                    TaskSummary = total == 0 ? "Chưa có công việc" : completed + "/" + total + " công việc hoàn thành",
                    Deadline = FormatDate(row["NgayDuKienHoanThanh"], "Hạn: "),
                    IsOverdue = projectOverdue,
                    IsDueSoon = projectDueSoon
                });
            }
            return items;
        }

        private List<TaskItem> LoadTasks(Guid userId)
        {
            DataTable table = Query(@"
SELECT DISTINCT t.IdCongViec, t.IdDuAn, d.MaDuAn, d.TenDuAn, t.MaCongViec, t.TenCongViec,
       t.NgayBatDau, t.NgayKetThuc, t.ThoiHanNgay, t.PhanTramHoanThanh,
       CONVERT(nvarchar(100), t.TrangThai) AS TrangThai
FROM dbo.TblCongViec_NhanVien tn
INNER JOIN dbo.TblCongViec t ON t.IdCongViec = tn.IdCongViec
INNER JOIN dbo.TblDuAn d ON d.IdDuAn = t.IdDuAn
WHERE tn.IdNhanVien = @UserId
  AND ISNULL(t.DaXoa, 0) = 0 AND ISNULL(d.DaXoa, 0) = 0
  AND (
       (t.NgayBatDau IS NOT NULL AND CAST(t.NgayBatDau AS date) <= @WeekEnd
        AND CAST(ISNULL(t.NgayKetThuc, t.NgayBatDau) AS date) >= @WeekStart)
       OR (t.NgayKetThuc IS NOT NULL AND CAST(t.NgayKetThuc AS date) < @WeekStart
           AND ISNULL(t.PhanTramHoanThanh, 0) < 100)
  )
ORDER BY t.NgayKetThuc, t.TenCongViec;",
                GuidParameter("@UserId", userId),
                DateParameter("@WeekStart", _weekStart),
                DateParameter("@WeekEnd", _weekEnd));

            return ParseTasksFromDataTable(table);
        }

        private List<TaskItem> LoadTasksByProject(Guid userId, Guid projectId)
        {
            DataTable table = Query(@"
SELECT DISTINCT t.IdCongViec, t.IdDuAn, d.MaDuAn, d.TenDuAn, t.MaCongViec, t.TenCongViec,
       t.NgayBatDau, t.NgayKetThuc, t.ThoiHanNgay, t.PhanTramHoanThanh,
       CONVERT(nvarchar(100), t.TrangThai) AS TrangThai
FROM dbo.TblCongViec_NhanVien tn
INNER JOIN dbo.TblCongViec t ON t.IdCongViec = tn.IdCongViec
INNER JOIN dbo.TblDuAn d ON d.IdDuAn = t.IdDuAn
WHERE tn.IdNhanVien = @UserId
  AND t.IdDuAn = @ProjectId
  AND ISNULL(t.DaXoa, 0) = 0 AND ISNULL(d.DaXoa, 0) = 0
  AND ISNULL(t.PhanTramHoanThanh, 0) < 100
ORDER BY t.NgayKetThuc, t.TenCongViec;",
                GuidParameter("@UserId", userId),
                GuidParameter("@ProjectId", projectId));

            return ParseTasksFromDataTable(table);
        }

        private List<TaskItem> ParseTasksFromDataTable(DataTable table)
        {
            List<TaskItem> items = new List<TaskItem>();
            foreach (DataRow row in table.Rows)
            {
                DateTime startDate, endDate;
                bool hasStart = TryDate(row["NgayBatDau"], out startDate);
                bool hasEnd = TryDate(row["NgayKetThuc"], out endDate);
                double progress = DoubleValue(row["PhanTramHoanThanh"]);
                progress = Math.Max(0, Math.Min(100, progress));

                bool overdue = hasEnd && endDate.Date < DateTime.Today && progress < 100;
                bool dueSoon = hasEnd && endDate.Date >= DateTime.Today && (endDate.Date - DateTime.Today).TotalDays <= 2 && progress < 100;
                bool completed = progress >= 100;

                string rawStatus = Value(row, "TrangThai");
                string status = FormatTaskStatus(rawStatus, progress, overdue);

                items.Add(new TaskItem
                {
                    Id = GuidValue(row["IdCongViec"]),
                    ProjectId = GuidValue(row["IdDuAn"]),
                    ProjectCode = FirstNonEmpty(Value(row, "MaDuAn"), "DỰ ÁN"),
                    ProjectName = FirstNonEmpty(Value(row, "TenDuAn"), "Chưa đặt tên dự án"),
                    TaskCode = FirstNonEmpty(Value(row, "MaCongViec"), "CÔNG VIỆC"),
                    TaskName = FirstNonEmpty(Value(row, "TenCongViec"), "Chưa đặt tên công việc"),
                    StartDate = hasStart ? (DateTime?)startDate.Date : null,
                    EndDate = hasEnd ? (DateTime?)endDate.Date : null,
                    DateRange = FormatTaskDateRange(hasStart ? (DateTime?)startDate : null, hasEnd ? (DateTime?)endDate : null),
                    Progress = (int)Math.Round(progress, MidpointRounding.AwayFromZero),
                    IsOverdue = overdue,
                    IsDueSoon = dueSoon,
                    IsCompleted = completed,
                    StatusText = status,
                    StatusCss = overdue ? "overdue" : completed ? "done" : "inprogress"
                });
            }

            return items.OrderByDescending(x => x.IsOverdue).ThenBy(x => x.EndDate ?? DateTime.MaxValue).ThenBy(x => x.TaskName).ToList();
        }

        private List<IssueItem> LoadIssues(Guid userId)
        {
            DataTable table = Query(@"
SELECT vd.IdVanDe, vd.MaVanDe, vd.TenVanDe, vd.MucDoAnhHuong, vd.TrangThai,
       vd.NgayTao, d.TenDuAn, t.TenCongViec AS TenCongViecBiAnhHuong
FROM dbo.TblVanDe_NhanVien vn
INNER JOIN dbo.TblVanDe vd ON vd.IdVanDe = vn.IdVanDe
LEFT JOIN dbo.TblDuAn d ON d.IdDuAn = vd.IdDuAn
LEFT JOIN dbo.TblCongViec t ON t.IdCongViec = vd.IdCongViecBiAnhHuong
WHERE vn.IdNhanVien = @UserId AND ISNULL(vd.DaXoa, 0) = 0
ORDER BY ISNULL(vd.NgayCapNhat, vd.NgayTao) DESC, vd.TenVanDe;", GuidParameter("@UserId", userId));

            List<IssueItem> items = new List<IssueItem>();
            foreach (DataRow row in table.Rows)
            {
                items.Add(new IssueItem
                {
                    Code = FirstNonEmpty(Value(row, "MaVanDe"), "VẤN ĐỀ"),
                    Name = FirstNonEmpty(Value(row, "TenVanDe"), "Chưa đặt tên vấn đề"),
                    ProjectName = FirstNonEmpty(Value(row, "TenDuAn"), "Chưa xác định dự án"),
                    Impact = DisplayValue(row["MucDoAnhHuong"], "Chưa xác định mức ảnh hưởng"),
                    StatusText = "Trạng thái: " + DisplayValue(row["TrangThai"], "Chưa cập nhật"),
                    CreatedDate = FormatDate(row["NgayTao"], String.Empty),
                    AffectedTask = FirstNonEmpty(Value(row, "TenCongViecBiAnhHuong"), "Chưa liên kết công việc")
                });
            }
            return items;
        }

        private List<RiskItem> LoadRisks(Guid userId)
        {
            DataTable table = Query(@"
SELECT r.IdRuiRo_DuAn, r.IdDuAn, r.TenRuiRo, r.XacSuatXayRa, r.MucDoAnhHuong, r.DiemRuiRo, r.NgayTao, d.TenDuAn
FROM dbo.TblRuiRo_DuAn r
INNER JOIN dbo.TblDuAn d ON d.IdDuAn = r.IdDuAn
WHERE ISNULL(r.DaXoa, 0) = 0 AND ISNULL(d.DaXoa, 0) = 0
  AND r.IdNhanVienXuLy = @UserId
  AND r.IdDuAn IN (
      SELECT DISTINCT t.IdDuAn
      FROM dbo.TblCongViec_NhanVien tn
      INNER JOIN dbo.TblCongViec t ON t.IdCongViec = tn.IdCongViec
      INNER JOIN dbo.TblDuAn dx ON dx.IdDuAn = t.IdDuAn
      WHERE tn.IdNhanVien = @TaskUserId AND ISNULL(t.DaXoa, 0) = 0 AND ISNULL(dx.DaXoa, 0) = 0
        AND t.NgayBatDau IS NOT NULL
        AND CAST(t.NgayBatDau AS date) <= @WeekEnd
        AND CAST(ISNULL(t.NgayKetThuc, t.NgayBatDau) AS date) >= @WeekStart
  )
ORDER BY TRY_CONVERT(decimal(18, 4), r.DiemRuiRo) DESC, r.NgayTao DESC;",
                GuidParameter("@UserId", userId),
                GuidParameter("@TaskUserId", userId),
                DateParameter("@WeekStart", _weekStart),
                DateParameter("@WeekEnd", _weekEnd));

            List<RiskItem> items = new List<RiskItem>();
            foreach (DataRow row in table.Rows)
            {
                items.Add(new RiskItem
                {
                    Name = FirstNonEmpty(Value(row, "TenRuiRo"), "Chưa đặt tên rủi ro"),
                    ProjectName = FirstNonEmpty(Value(row, "TenDuAn"), "Chưa xác định dự án"),
                    Probability = DisplayValue(row["XacSuatXayRa"], "Chưa cập nhật"),
                    Impact = DisplayValue(row["MucDoAnhHuong"], "Chưa cập nhật"),
                    Score = DisplayValue(row["DiemRuiRo"], "Chưa chấm điểm"),
                    CreatedDate = FormatDate(row["NgayTao"], String.Empty)
                });
            }
            return items;
        }

        private List<MeetingItem> LoadMeetings(Guid userId)
        {
            DataTable table = Query(@"
SELECT DISTINCT lh.IdLichHop, lh.IdDuAn, lh.TenCuocHop, lh.ThoiGianBatDau, lh.ThoiGianKetThuc,
       lh.DiaDiemHop, lh.TrangThai, d.TenDuAn
FROM dbo.TblLichHop_NhanVien lhn
INNER JOIN dbo.TblLichHop lh ON lh.IdLichHop = lhn.IdLichHop
LEFT JOIN dbo.TblDuAn d ON d.IdDuAn = lh.IdDuAn
WHERE lhn.IdNhanVien = @UserId AND ISNULL(lh.DaXoa, 0) = 0
  AND lh.ThoiGianBatDau < @WeekEndExclusive AND ISNULL(lh.ThoiGianKetThuc, lh.ThoiGianBatDau) >= @WeekStart
ORDER BY lh.ThoiGianBatDau;",
                GuidParameter("@UserId", userId),
                DateTimeParameter("@WeekStart", _weekStart),
                DateTimeParameter("@WeekEndExclusive", _weekEnd.AddDays(1)));

            List<MeetingItem> items = new List<MeetingItem>();
            foreach (DataRow row in table.Rows)
            {
                DateTime start, end;
                if (!TryDate(row["ThoiGianBatDau"], out start)) continue;
                if (!TryDate(row["ThoiGianKetThuc"], out end)) end = start;
                items.Add(new MeetingItem
                {
                    Name = FirstNonEmpty(Value(row, "TenCuocHop"), "Cuộc họp"),
                    ProjectName = FirstNonEmpty(Value(row, "TenDuAn"), "Cuộc họp chung"),
                    Place = Value(row, "DiaDiemHop"),
                    Start = start,
                    End = end
                });
            }
            return items;
        }

        private List<ExceptionItem> LoadExceptions()
        {
            DataTable table = Query(@"
SELECT IdNgoaiLe, TenNgoaiLe, NgayBatDau, NgayKetThuc, LaNgayLamViec,
       GioBatDauSang, GioKetThucSang, GioBatDauChieu, GioKetThucChieu, MoTa
FROM dbo.TblLichNgoaiLe
WHERE ISNULL(DaXoa, 0) = 0
  AND CAST(NgayBatDau AS date) <= @WeekEnd
  AND CAST(ISNULL(NgayKetThuc, NgayBatDau) AS date) >= @WeekStart
ORDER BY NgayBatDau, NgayTao;",
                DateParameter("@WeekStart", _weekStart),
                DateParameter("@WeekEnd", _weekEnd));

            List<ExceptionItem> items = new List<ExceptionItem>();
            foreach (DataRow row in table.Rows)
            {
                DateTime start, end;
                if (!TryDate(row["NgayBatDau"], out start)) continue;
                if (!TryDate(row["NgayKetThuc"], out end)) end = start;
                items.Add(new ExceptionItem
                {
                    Name = FirstNonEmpty(Value(row, "TenNgoaiLe"), "Ngoại lệ lịch làm việc"),
                    Description = Value(row, "MoTa"),
                    Start = start.Date,
                    End = end.Date,
                    IsWorkingDay = BoolValue(row["LaNgayLamViec"]),
                    MorningStart = TimeText(row["GioBatDauSang"]),
                    MorningEnd = TimeText(row["GioKetThucSang"]),
                    AfternoonStart = TimeText(row["GioBatDauChieu"]),
                    AfternoonEnd = TimeText(row["GioKetThucChieu"])
                });
            }
            return items;
        }

        private Dictionary<DayOfWeek, WorkdayConfiguration> LoadWorkdayConfigurations()
        {
            DataTable table = Query(@"
SELECT NgayTrongTuan, LaNgayLamViec, GioBatDauSang, GioKetThucSang, GioBatDauChieu, GioKetThucChieu
FROM dbo.TblCauHinhTuanLamViec;");

            List<DataRow> rows = table.Rows.Cast<DataRow>().ToList();
            bool hasZeroValue = rows.Any(x => Value(x, "NgayTrongTuan") == "0");
            Dictionary<DayOfWeek, WorkdayConfiguration> result = new Dictionary<DayOfWeek, WorkdayConfiguration>();
            foreach (DataRow row in rows)
            {
                DayOfWeek day;
                if (!TryParseDayOfWeek(Value(row, "NgayTrongTuan"), hasZeroValue, out day)) continue;
                result[day] = new WorkdayConfiguration
                {
                    IsWorkingDay = BoolValue(row["LaNgayLamViec"]),
                    MorningStart = TimeText(row["GioBatDauSang"]),
                    MorningEnd = TimeText(row["GioKetThucSang"]),
                    AfternoonStart = TimeText(row["GioBatDauChieu"]),
                    AfternoonEnd = TimeText(row["GioKetThucChieu"])
                };
            }
            return result;
        }

        private List<CalendarDayItem> BuildCalendarDays(List<TaskItem> tasks, List<MeetingItem> meetings, List<ExceptionItem> exceptions, Dictionary<DayOfWeek, WorkdayConfiguration> configurations)
        {
            List<CalendarDayItem> days = new List<CalendarDayItem>();
            DateTime today = DateTime.Today;
            for (int i = 0; i < 7; i++)
            {
                DateTime date = _weekStart.AddDays(i).Date;
                WorkdayConfiguration config;
                bool hasConfig = configurations.TryGetValue(date.DayOfWeek, out config);
                if (!hasConfig)
                {
                    config = new WorkdayConfiguration
                    {
                        IsWorkingDay = date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday
                    };
                }

                bool isWorkingDay = config.IsWorkingDay;
                string dayType = isWorkingDay ? "Ngày làm việc" : "Ngày nghỉ";
                string dayTypeCss = isWorkingDay ? String.Empty : "off";
                string workHours = FormatWorkHours(config.MorningStart, config.MorningEnd, config.AfternoonStart, config.AfternoonEnd);
                List<ExceptionItem> dayExceptions = exceptions.Where(x => x.Start.Date <= date && x.End.Date >= date).ToList();
                if (dayExceptions.Count > 0)
                {
                    ExceptionItem exception = dayExceptions[dayExceptions.Count - 1];
                    isWorkingDay = exception.IsWorkingDay;
                    dayType = (isWorkingDay ? "Làm việc ngoại lệ: " : "Nghỉ / ngoại lệ: ") + exception.Name;
                    dayTypeCss = isWorkingDay ? String.Empty : "off";
                    string exceptionHours = FormatWorkHours(
                        FirstNonEmpty(exception.MorningStart, config.MorningStart),
                        FirstNonEmpty(exception.MorningEnd, config.MorningEnd),
                        FirstNonEmpty(exception.AfternoonStart, config.AfternoonStart),
                        FirstNonEmpty(exception.AfternoonEnd, config.AfternoonEnd));
                    workHours = isWorkingDay ? exceptionHours : "Theo lịch ngoại lệ";
                }

                CalendarDayItem dayItem = new CalendarDayItem
                {
                    Date = date,
                    DayName = VietnameseDayName(date.DayOfWeek),
                    DayNumber = date.ToString("dd/MM", _viCulture),
                    IsToday = date == today,
                    CssClass = (date == today ? "is-today " : String.Empty) + (!isWorkingDay ? "is-day-off" : String.Empty),
                    DayType = dayType,
                    DayTypeCss = dayTypeCss,
                    WorkHours = workHours,
                    Events = new List<CalendarEvent>()
                };

                foreach (ExceptionItem exception in dayExceptions)
                {
                    string description = String.IsNullOrWhiteSpace(exception.Description) ? "Lịch ngoại lệ" : exception.Description;
                    dayItem.Events.Add(new CalendarEvent
                    {
                        SortAt = date,
                        TimeLabel = "CẢ NGÀY",
                        Title = exception.Name,
                        Subtitle = description,
                        CssClass = "special"
                    });
                }

                foreach (MeetingItem meeting in meetings)
                {
                    if (meeting.Start.Date <= date && meeting.End.Date >= date)
                    {
                        DateTime displayStart = meeting.Start.Date < date ? date : meeting.Start;
                        DateTime displayEnd = meeting.End.Date > date ? date.AddDays(1).AddTicks(-1) : meeting.End;
                        string time = displayStart.ToString("HH:mm", _viCulture) + "–" + displayEnd.ToString("HH:mm", _viCulture);
                        string subtitle = meeting.ProjectName;
                        if (!String.IsNullOrWhiteSpace(meeting.Place)) subtitle += " · " + meeting.Place;
                        dayItem.Events.Add(new CalendarEvent
                        {
                            SortAt = displayStart,
                            TimeLabel = time,
                            Title = meeting.Name,
                            Subtitle = subtitle,
                            CssClass = "meeting"
                        });
                    }
                }

                foreach (TaskItem task in tasks)
                {
                    DateTime taskStart = task.StartDate ?? task.EndDate ?? DateTime.MinValue;
                    DateTime taskEnd = task.EndDate ?? task.StartDate ?? DateTime.MinValue;
                    if (taskStart != DateTime.MinValue && taskEnd != DateTime.MinValue && taskStart.Date <= date && taskEnd.Date >= date)
                    {
                        string subtitle = task.ProjectCode + " · " + task.ProjectName;
                        if (task.IsOverdue) subtitle += " · Quá hạn";
                        dayItem.Events.Add(new CalendarEvent
                        {
                            SortAt = date.AddHours(23),
                            TimeLabel = task.IsOverdue ? "CẦN ƯU TIÊN" : "CÔNG VIỆC",
                            Title = task.TaskName,
                            Subtitle = subtitle,
                            CssClass = "task"
                        });
                    }
                }

                dayItem.Events = dayItem.Events.OrderBy(x => x.SortAt).ThenBy(x => x.CssClass == "meeting" ? 0 : x.CssClass == "special" ? 1 : 2).ToList();
                dayItem.HasEvents = dayItem.Events.Count > 0;
                days.Add(dayItem);
            }
            return days;
        }

        private bool TryResolveCurrentUserId(out Guid userId)
        {
            userId = Guid.Empty;
            try
            {
                MembershipUser membershipUser = Membership.GetUser();
                if (membershipUser != null && membershipUser.ProviderUserKey != null && Guid.TryParse(Convert.ToString(membershipUser.ProviderUserKey, CultureInfo.InvariantCulture), out userId))
                    return true;
            }
            catch { }

            string[] sessionKeys = { "UserId", "IdNhanVien", "IDNhanVien", "idNhanVien", "CurrentUserId" };
            foreach (string key in sessionKeys)
            {
                object value = Session == null ? null : Session[key];
                if (value != null && Guid.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out userId))
                    return true;
            }

            IPrincipal principal = Context == null ? null : Context.User;
            if (principal != null && principal.Identity != null && principal.Identity.IsAuthenticated && !String.IsNullOrWhiteSpace(principal.Identity.Name))
            {
                DataTable table = Query(@"
SELECT TOP (1) UserId FROM dbo.aspnet_Users
WHERE LoweredUserName = @LoweredUserName OR UserName = @UserName;",
                    TextParameter("@LoweredUserName", principal.Identity.Name.ToLowerInvariant()),
                    TextParameter("@UserName", principal.Identity.Name));
                if (table.Rows.Count > 0 && Guid.TryParse(Value(table.Rows[0], "UserId"), out userId))
                    return true;
            }
            return false;
        }

        private void BindEmptyDashboard()
        {
            rptProjects.DataSource = new List<ProjectItem>();
            rptProjects.DataBind();
            rptProjectsAll.DataSource = new List<ProjectItem>();
            rptProjectsAll.DataBind();
            rptTasks.DataSource = new List<TaskItem>();
            rptTasks.DataBind();
            rptTasksAll.DataSource = new List<TaskItem>();
            rptTasksAll.DataBind();
            rptIssues.DataSource = new List<IssueItem>();
            rptIssues.DataBind();
            rptIssuesAll.DataSource = new List<IssueItem>();
            rptIssuesAll.DataBind();
            rptRisks.DataSource = new List<RiskItem>();
            rptRisks.DataBind();
            rptRisksAll.DataSource = new List<RiskItem>();
            rptRisksAll.DataBind();
            rptDays.DataSource = new List<CalendarDayItem>();
            rptDays.DataBind();
            rptCalendarAll.DataSource = new List<CalendarDayItem>();
            rptCalendarAll.DataBind();
            pnlNoProjects.Visible = true;
            pnlNoTasks.Visible = true;
            pnlNoAllTasks.Visible = true;
            pnlNoIssues.Visible = true;
            pnlNoRisks.Visible = true;
        }

        private void ShowDashboardMessage(string message)
        {
            lblDashboardMessage.Text = Server.HtmlEncode(message);
            pnlDashboardMessage.Visible = true;
        }

        private DataTable Query(string sql, params SqlParameter[] parameters)
        {
            DataTable table = new DataTable();
            using (SqlConnection connection = new SqlConnection(_connectionString ?? ResolveConnectionString()))
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.CommandTimeout = 45;
                if (parameters != null && parameters.Length > 0)
                    command.Parameters.AddRange(parameters);
                adapter.Fill(table);
            }
            return table;
        }

        private string ResolveConnectionString()
        {
            ConnectionStringSettings preferred = ConfigurationManager.ConnectionStrings["SweetSoft.QLDA.BackOffice"];
            if (preferred != null && !String.IsNullOrWhiteSpace(preferred.ConnectionString))
                return preferred.ConnectionString;

            foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
            {
                if (item == null || String.IsNullOrWhiteSpace(item.ConnectionString)) continue;
                if (item.ConnectionString.IndexOf("SweetSoft_QLDA", StringComparison.OrdinalIgnoreCase) >= 0)
                    return item.ConnectionString;
            }

            foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
            {
                if (item == null || String.IsNullOrWhiteSpace(item.ConnectionString)) continue;
                if (String.Equals(item.Name, "LocalSqlServer", StringComparison.OrdinalIgnoreCase)) continue;
                if (String.Equals(item.ProviderName, "System.Data.SqlClient", StringComparison.OrdinalIgnoreCase))
                    return item.ConnectionString;
            }

            throw new ConfigurationErrorsException("Không tìm thấy connection string đến cơ sở dữ liệu SweetSoft_QLDA. Hãy kiểm tra web.config.");
        }

        private static SqlParameter GuidParameter(string name, Guid value)
        {
            return new SqlParameter(name, SqlDbType.UniqueIdentifier) { Value = value };
        }

        private static SqlParameter DateParameter(string name, DateTime value)
        {
            return new SqlParameter(name, SqlDbType.Date) { Value = value.Date };
        }

        private static SqlParameter DateTimeParameter(string name, DateTime value)
        {
            return new SqlParameter(name, SqlDbType.DateTime) { Value = value };
        }

        private static SqlParameter TextParameter(string name, string value)
        {
            return new SqlParameter(name, SqlDbType.NVarChar, 256) { Value = value ?? String.Empty };
        }

        private static DateTime GetMonday(DateTime date)
        {
            int offset = ((int)date.DayOfWeek + 6) % 7;
            return date.Date.AddDays(-offset);
        }

        private static string Value(DataRow row, string column)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(column) || row[column] == DBNull.Value)
                return String.Empty;
            return Convert.ToString(row[column], CultureInfo.InvariantCulture).Trim();
        }

        private static string DisplayValue(object value, string fallback)
        {
            if (value == null || value == DBNull.Value) return fallback;
            string text = Convert.ToString(value, CultureInfo.CurrentCulture).Trim();
            return String.IsNullOrWhiteSpace(text) ? fallback : text;
        }

        private static string FirstNonEmpty(string value, string fallback)
        {
            return String.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static Guid GuidValue(object value)
        {
            Guid result;
            return value != null && Guid.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out result) ? result : Guid.Empty;
        }

        private static int IntValue(object value)
        {
            int result;
            return value != null && value != DBNull.Value && Int32.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out result) ? result : 0;
        }

        private static double DoubleValue(object value)
        {
            double result;
            return value != null && value != DBNull.Value && Double.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out result) ? result : 0;
        }

        private static bool BoolValue(object value)
        {
            if (value == null || value == DBNull.Value) return false;
            if (value is bool) return (bool)value;
            string text = Convert.ToString(value, CultureInfo.InvariantCulture).Trim();
            return text == "1" || text.Equals("true", StringComparison.OrdinalIgnoreCase) || text.Equals("yes", StringComparison.OrdinalIgnoreCase) || text.Equals("có", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryDate(object value, out DateTime date)
        {
            date = DateTime.MinValue;
            if (value == null || value == DBNull.Value) return false;
            if (value is DateTime)
            {
                date = (DateTime)value;
                return true;
            }
            return DateTime.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        private string FormatDate(object value, string prefix)
        {
            DateTime date;
            return TryDate(value, out date) ? prefix + date.ToString("dd/MM/yyyy", _viCulture) : String.Empty;
        }

        private string FormatDateTime(object value)
        {
            DateTime date;
            return TryDate(value, out date) ? date.ToString("dd/MM/yyyy HH:mm", _viCulture) : String.Empty;
        }

        private string FormatTaskDateRange(DateTime? start, DateTime? end)
        {
            if (start.HasValue && end.HasValue)
                return start.Value.ToString("dd/MM/yyyy", _viCulture) + " – " + end.Value.ToString("dd/MM/yyyy", _viCulture);
            if (start.HasValue) return "Bắt đầu: " + start.Value.ToString("dd/MM/yyyy", _viCulture);
            if (end.HasValue) return "Hạn: " + end.Value.ToString("dd/MM/yyyy", _viCulture);
            return "Chưa có thời gian";
        }

        private string FormatTaskStatus(string rawStatus, double progress, bool overdue)
        {
            if (overdue) return "Quá hạn";
            if (!String.IsNullOrWhiteSpace(rawStatus) && !Regex.IsMatch(rawStatus, @"^\d+(\.\d+)?$")) return rawStatus;
            if (progress >= 100) return "Hoàn thành";
            if (progress > 0) return "Đang thực hiện";
            return "Chưa bắt đầu";
        }

        private static string TimeText(object value)
        {
            if (value == null || value == DBNull.Value) return String.Empty;
            if (value is TimeSpan) return ((TimeSpan)value).ToString(@"hh\:mm");
            if (value is DateTime) return ((DateTime)value).ToString("HH:mm", CultureInfo.InvariantCulture);
            string text = Convert.ToString(value, CultureInfo.InvariantCulture).Trim();
            TimeSpan time;
            if (TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out time)) return time.ToString(@"hh\:mm");
            DateTime date;
            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) return date.ToString("HH:mm", CultureInfo.InvariantCulture);
            return text;
        }

        private static string FormatWorkHours(string morningStart, string morningEnd, string afternoonStart, string afternoonEnd)
        {
            List<string> periods = new List<string>();
            if (!String.IsNullOrWhiteSpace(morningStart) || !String.IsNullOrWhiteSpace(morningEnd))
                periods.Add(FirstNonEmpty(morningStart, "--:--") + "–" + FirstNonEmpty(morningEnd, "--:--"));
            if (!String.IsNullOrWhiteSpace(afternoonStart) || !String.IsNullOrWhiteSpace(afternoonEnd))
                periods.Add(FirstNonEmpty(afternoonStart, "--:--") + "–" + FirstNonEmpty(afternoonEnd, "--:--"));
            return periods.Count == 0 ? "Chưa cấu hình giờ làm" : String.Join(" · ", periods);
        }

        private static bool TryParseDayOfWeek(string raw, bool hasZeroValue, out DayOfWeek day)
        {
            day = DayOfWeek.Monday;
            if (String.IsNullOrWhiteSpace(raw)) return false;
            int number;
            if (Int32.TryParse(raw.Trim(), out number))
            {
                if (hasZeroValue)
                {
                    if (number < 0 || number > 6) return false;
                    day = (DayOfWeek)number;
                    return true;
                }
                if (number >= 1 && number <= 7)
                {
                    day = number == 7 ? DayOfWeek.Sunday : (DayOfWeek)number;
                    return true;
                }
                return false;
            }

            string text = NormalizeVietnamese(raw).ToLowerInvariant().Trim();
            if (text == "cn" || text.Contains("chu nhat") || text.Contains("sunday")) { day = DayOfWeek.Sunday; return true; }
            if (text.Contains("thu hai") || text == "thu 2" || text == "monday") { day = DayOfWeek.Monday; return true; }
            if (text.Contains("thu ba") || text == "thu 3" || text == "tuesday") { day = DayOfWeek.Tuesday; return true; }
            if (text.Contains("thu tu") || text == "thu 4" || text == "wednesday") { day = DayOfWeek.Wednesday; return true; }
            if (text.Contains("thu nam") || text == "thu 5" || text == "thursday") { day = DayOfWeek.Thursday; return true; }
            if (text.Contains("thu sau") || text == "thu 6" || text == "friday") { day = DayOfWeek.Friday; return true; }
            if (text.Contains("thu bay") || text == "thu 7" || text == "saturday") { day = DayOfWeek.Saturday; return true; }
            return Enum.TryParse(raw, true, out day);
        }

        private static string NormalizeVietnamese(string text)
        {
            string normalized = text.Normalize(NormalizationForm.FormD);
            StringBuilder result = new StringBuilder();
            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    result.Append(c);
            }
            return result.ToString().Normalize(NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
        }

        private static string VietnameseDayName(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return "Thứ Hai";
                case DayOfWeek.Tuesday: return "Thứ Ba";
                case DayOfWeek.Wednesday: return "Thứ Tư";
                case DayOfWeek.Thursday: return "Thứ Năm";
                case DayOfWeek.Friday: return "Thứ Sáu";
                case DayOfWeek.Saturday: return "Thứ Bảy";
                default: return "Chủ nhật";
            }
        }

        private static string GetInitials(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) return "NV";
            string[] parts = name.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        public class ProjectItem
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public int Progress { get; set; }
            public string TaskSummary { get; set; }
            public string Deadline { get; set; }
            public bool IsOverdue { get; set; }
            public bool IsDueSoon { get; set; }
        }

        public class TaskItem
        {
            public Guid Id { get; set; }
            public Guid ProjectId { get; set; }
            public string ProjectCode { get; set; }
            public string ProjectName { get; set; }
            public string TaskCode { get; set; }
            public string TaskName { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string DateRange { get; set; }
            public int Progress { get; set; }
            public bool IsOverdue { get; set; }
            public bool IsDueSoon { get; set; }
            public bool IsCompleted { get; set; }
            public string StatusText { get; set; }
            public string StatusCss { get; set; }
        }

        public class IssueItem
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public string ProjectName { get; set; }
            public string Impact { get; set; }
            public string StatusText { get; set; }
            public string CreatedDate { get; set; }
            public string AffectedTask { get; set; }
        }

        public class RiskItem
        {
            public string Name { get; set; }
            public string ProjectName { get; set; }
            public string Probability { get; set; }
            public string Impact { get; set; }
            public string Score { get; set; }
            public string CreatedDate { get; set; }
        }

        public class MeetingItem
        {
            public string Name { get; set; }
            public string ProjectName { get; set; }
            public string Place { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }

        public class ExceptionItem
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public bool IsWorkingDay { get; set; }
            public string MorningStart { get; set; }
            public string MorningEnd { get; set; }
            public string AfternoonStart { get; set; }
            public string AfternoonEnd { get; set; }
        }

        public class WorkdayConfiguration
        {
            public bool IsWorkingDay { get; set; }
            public string MorningStart { get; set; }
            public string MorningEnd { get; set; }
            public string AfternoonStart { get; set; }
            public string AfternoonEnd { get; set; }
        }

        public class CalendarDayItem
        {
            public DateTime Date { get; set; }
            public string DayName { get; set; }
            public string DayNumber { get; set; }
            public bool IsToday { get; set; }
            public string CssClass { get; set; }
            public string DayType { get; set; }
            public string DayTypeCss { get; set; }
            public string WorkHours { get; set; }
            public bool HasEvents { get; set; }
            public List<CalendarEvent> Events { get; set; }
            public List<CalendarEvent> PreviewEvents
            {
                get { return Events == null ? new List<CalendarEvent>() : Events.Take(3).ToList(); }
            }
            public int MoreEventsCount
            {
                get { return Events == null ? 0 : Math.Max(0, Events.Count - 3); }
            }
        }

        public class CalendarEvent
        {
            public DateTime SortAt { get; set; }
            public string TimeLabel { get; set; }
            public string Title { get; set; }
            public string Subtitle { get; set; }
            public string CssClass { get; set; }
        }
    }
}