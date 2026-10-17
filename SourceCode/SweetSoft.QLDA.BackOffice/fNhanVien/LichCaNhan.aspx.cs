using Newtonsoft.Json;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.ScheduleManager;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace SweetSoft.QLDA.BackOffice.fNhanVien
{
    public partial class LichCaNhan : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.LichBieu;
        public override bool IsLogin => true;

        private Guid TargetUserId
        {
            get => ViewState["TargetUserId"] != null ? (Guid)ViewState["TargetUserId"] : Guid.Empty;
            set => ViewState["TargetUserId"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!InitSecurityAndTargetUser())
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                    return;
                }
                hfCurrentDate.Value = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                hfViewMode.Value = "month";
                LoadCalendarData();
            }
        }

        #region 1. Bảo mật & phân quyền (giữ nguyên quy tắc đang sử dụng)
        private bool InitSecurityAndTargetUser()
        {
            Guid loggedInUser = SweetContext.Current.UserId;
            string rawQueryId = Request.QueryString["Id"];
            bool isFromProfile = Request.QueryString["from"] == "profile";
            bool hasParentDetail = !string.IsNullOrEmpty(rawQueryId);
            string rollbackUrl = "";

            if (hasParentDetail)
            {
                string plainId = SecurityUtilities.UnprotectUrlParameter(rawQueryId);
                if (Guid.TryParse(plainId, out Guid queryId))
                {
                    TargetUserId = queryId;
                    if (queryId == loggedInUser)
                    {
                        litTitle.Text = GetResourceText(BackEndResourceKeys.MY_PERSONAL_SCHEDULE);
                    }
                    else
                    {
                        if (!this.IsUserRight(ActionKeys.View, ModuleKeys.NhanVien)) return false;
                        AspnetUser targetUser = UserManager.Instance.GetUserById(queryId);
                        litTitle.Text = targetUser != null
                            ? string.Format(GetResourceText(BackEndResourceKeys.SCHEDULE_OF_USER), targetUser.DisplayName)
                            : GetResourceText(BackEndResourceKeys.EMPLOYEE_SCHEDULE);
                    }
                    rollbackUrl = RewriteURLHelper.ViewDetailEmp(queryId);
                }
                else
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), true);
                    return true;
                }
            }
            else
            {
                TargetUserId = loggedInUser;
                litTitle.Text = GetResourceText(BackEndResourceKeys.MY_PERSONAL_SCHEDULE);
            }

            Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.PERSONAL_SCHEDULE);
            var navLinks = new Dictionary<string, string>();
            if (hasParentDetail)
            {
                if (isFromProfile)
                    navLinks.Add(GetRelativeClientPath(RewriteURLHelper.Profile), GetResourceText(BackEndResourceKeys.PROFILE));
                else
                    navLinks.Add(GetRelativeClientPath(RewriteURLHelper.NhanVien), GetResourceText(BackEndResourceKeys.EMPLOYEE_LIST));

                if (isFromProfile && !rollbackUrl.Contains("from=profile"))
                    rollbackUrl += (rollbackUrl.Contains("?") ? "&" : "?") + "from=profile";
                navLinks.Add(GetRelativeClientPath(rollbackUrl), GetResourceText(BackEndResourceKeys.EMPLOYEE_DETAIL));
            }
            Navigation1.keyValuePairUrls = navLinks;
            return true;
        }
        #endregion

        #region 2. Tính kỳ lịch và tải dữ liệu
        // CtrlMeet dùng 5 hoặc 6 hàng tùy tháng; lịch cá nhân áp dụng cách tính tương tự.
        private void GetCalendarPeriod(out DateTime refDate, out DateTime startDate, out DateTime endDate)
        {
            if (!DateTime.TryParseExact(hfCurrentDate.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out refDate)
                || refDate.Year < 2000 || refDate.Year > 2100)
                refDate = DateTime.Today;

            bool weekView = hfViewMode.Value == "week";
            DateTime first = weekView ? refDate.Date : new DateTime(refDate.Year, refDate.Month, 1);
            int offset = ((int)first.DayOfWeek + 6) % 7;
            startDate = first.AddDays(-offset);
            int days = weekView ? 7 : Math.Max(35, ((offset + DateTime.DaysInMonth(refDate.Year, refDate.Month) + 6) / 7) * 7);
            endDate = startDate.AddDays(days - 1); // Manager đang nhận hai đầu mốc bao gồm ngày cuối.
        }

        private void LoadCalendarData()
        {
            GetCalendarPeriod(out DateTime refDate, out DateTime startDate, out DateTime endDate);
            hfCurrentDate.Value = refDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            bool weekView = hfViewMode.Value == "week";
            if (weekView)
            {
                litDateRange.Text = $"{GetResourceText(BackEndResourceKeys.WEEK)} {GetResourceText(BackEndResourceKeys.FROM)} {startDate:dd/MM} – {endDate:dd/MM/yyyy}";
                btnViewWeek.CssClass = "personal-cal-btn active";
                btnViewMonth.CssClass = "personal-cal-btn";
            }
            else
            {
                litDateRange.Text = $"{GetResourceText(BackEndResourceKeys.MONTH)} {refDate:MM/yyyy}";
                btnViewMonth.CssClass = "personal-cal-btn active";
                btnViewWeek.CssClass = "personal-cal-btn";
            }
            var data = LichTrinhManager.Instance.LayLichTrinhNhanVien(TargetUserId, startDate, endDate);
            hfScheduleDataJSON.Value = JsonConvert.SerializeObject(data);
        }
        #endregion

        #region 3. Điều hướng (UpdatePanel)
        protected void btnPrev_Click(object sender, EventArgs e)
        {
            GetCalendarPeriod(out DateTime current, out DateTime start, out DateTime end);
            hfCurrentDate.Value = (hfViewMode.Value == "week" ? current.AddDays(-7) : current.AddMonths(-1)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            LoadCalendarData();
        }
        protected void btnNext_Click(object sender, EventArgs e)
        {
            GetCalendarPeriod(out DateTime current, out DateTime start, out DateTime end);
            hfCurrentDate.Value = (hfViewMode.Value == "week" ? current.AddDays(7) : current.AddMonths(1)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            LoadCalendarData();
        }
        protected void btnToday_Click(object sender, EventArgs e)
        {
            hfCurrentDate.Value = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            LoadCalendarData();
        }
        protected void btnViewMonth_Click(object sender, EventArgs e)
        {
            hfViewMode.Value = "month";
            LoadCalendarData();
        }
        protected void btnViewWeek_Click(object sender, EventArgs e)
        {
            hfViewMode.Value = "week";
            LoadCalendarData();
        }
        #endregion

        #region 4. Modal chi tiết ngày (ExtraModal)
        private static string Esc(string value) => HttpUtility.HtmlEncode(value ?? string.Empty);

        protected void btnOpenModalDay_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfSelectedDateIndex.Value, out int index) || index < 0 || TargetUserId == Guid.Empty) return;
            // Tải lại dữ liệu phía server, không dùng JSON HiddenField mà trình duyệt có thể sửa.
            GetCalendarPeriod(out DateTime refDate, out DateTime start, out DateTime end);
            var days = LichTrinhManager.Instance.LayLichTrinhNhanVien(TargetUserId, start, end);
            if (days == null) return;
            List<ThongTinLichNgay> dayList = days.ToList();
            if (index >= dayList.Count) return;
            ThongTinLichNgay day = dayList[index];
            if (day == null || !Convert.ToBoolean(day.ChoPhepClick) || day.Ngay.Date < start || day.Ngay.Date > end) return;

            mdlDayDetail.Title = GetResourceText(BackEndResourceKeys.SCHEDULE_DETAILS) + " - " + day.Ngay.ToString("dd/MM/yyyy");
            var html = new StringBuilder(2500);
            string status = day.TrangThaiLich ?? "free";

            if (status == "holiday")
            {
                html.Append("<div class='personal-day-notice is-holiday'><i class='fas fa-umbrella-beach'></i><span><strong>")
                    .Append(Esc(GetResourceText(BackEndResourceKeys.HOLIDAY) ?? "Ngày nghỉ lễ"))
                    .Append("</strong><div>").Append(Esc(day.TenNgoaiLe)).Append("</div></span></div>");
            }

            List<TomTatCongViec> tasks = day.DanhSachCongViec != null
                ? day.DanhSachCongViec.Where(t => t != null).ToList()
                : new List<TomTatCongViec>();
            if (tasks.Count == 0)
            {
                if (status == "weekend")
                    html.Append("<div class='personal-day-notice is-weekend'><i class='fas fa-mug-hot'></i><span>Ngày nghỉ, không có công việc được liệt kê.</span></div>");
                else if (status == "busy")
                    html.Append("<div class='personal-day-notice'><i class='far fa-calendar-check'></i><span>Ngày có lịch làm việc, chưa có công việc chi tiết.</span></div>");
                else if (status != "holiday")
                    html.Append("<div class='personal-day-notice'><i class='far fa-calendar'></i><span>Chưa có công việc trong ngày này.</span></div>");
            }
            else
            {
                var groups = tasks.GroupBy(t => string.IsNullOrWhiteSpace(t.TenDuAn)
                    ? (GetResourceText(BackEndResourceKeys.OTHER_PROJECT_UNIDENTIFIED) ?? "Dự án khác")
                    : t.TenDuAn).ToList();
                html.Append("<div class='personal-day-intro'><span class='personal-day-intro-icon'><i class='fas fa-briefcase'></i></span><span><strong class='personal-day-intro-title'>")
                    .Append(tasks.Count).Append(" công việc trong ngày</strong><small class='personal-day-intro-subtitle'>Thuộc ")
                    .Append(groups.Count).Append(" dự án</small></span></div>");
                foreach (var group in groups)
                {
                    html.Append("<section class='personal-day-project'><div class='personal-day-project-head'><span><i class='fas fa-folder-open me-1'></i> ")
                        .Append(Esc(group.Key)).Append("</span><strong class='personal-day-project-count'>")
                        .Append(group.Count()).Append(" công việc</strong></div><div>");
                    foreach (var task in group)
                    {
                        html.Append("<div class='personal-day-task'>");
                        if (!string.IsNullOrWhiteSpace(task.MaCongViec))
                            html.Append("<span class='personal-day-task-code'>").Append(Esc(task.MaCongViec)).Append("</span>");
                        html.Append("<span class='personal-day-task-name'>").Append(Esc(task.TenCongViec)).Append("</span></div>");
                    }
                    html.Append("</div></section>");
                }
            }
            litModalContent.Text = html.ToString();
            upModal.Update();
            mdlDayDetail.OpenModal(true);
        }
        #endregion
    }
}
