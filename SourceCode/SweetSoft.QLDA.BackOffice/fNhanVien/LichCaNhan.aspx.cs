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

            // Giống NhanVienDetail: thiết lập tiêu đề tab trình duyệt / metadata của trang.
            string pageTitle = GetResourceText(BackEndResourceKeys.PERSONAL_SCHEDULE);
            if (string.IsNullOrWhiteSpace(pageTitle)) pageTitle = "Lịch cá nhân";
            Navigation1.MainTitle = pageTitle;
            SetMetaTagsOgTags(pageTitle);
            if (string.IsNullOrWhiteSpace(Page.Title)) Page.Title = pageTitle;
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
            var data = LichTrinhManager.Instance.LayLichTrinhNhanVienCoCuocHop(TargetUserId, startDate, endDate);
            BoSungTenDuAnChoLichHop(data);
            hfScheduleDataJSON.Value = JsonConvert.SerializeObject(data);
        }
        #endregion

        // TenDuAn cua DTO hop co the rong du IdDuAn van hop le. Uu tien dung ten
        // san co tu DTO/task; neu thieu chi lookup mot lan cho moi IdDuAn trong ky lich.
        // Khong sua MeetRepository, khong lam thay doi pham vi hop nhan vien duoc xem.
        private static void BoSungTenDuAnChoLichHop(IEnumerable<ThongTinLichNgay> days)
        {
            if (days == null) return;
            var calendarDays = days.Where(d => d != null).ToList();
            var tenDuAn = new Dictionary<Guid, string>();
            var duAnTheoCuocHop = new Dictionary<Guid, Guid>();

            foreach (ThongTinLichNgay day in calendarDays)
            {
                if (day.DanhSachCongViec == null) continue;
                foreach (TomTatCongViec task in day.DanhSachCongViec)
                {
                    if (task != null && task.IdDuAn != Guid.Empty && !string.IsNullOrWhiteSpace(task.TenDuAn))
                        tenDuAn[task.IdDuAn] = task.TenDuAn;
                }
            }

            foreach (ThongTinLichNgay day in calendarDays)
            {
                if (day.DanhSachCuocHop == null) continue;
                foreach (TomTatCuocHop meeting in day.DanhSachCuocHop)
                {
                    if (meeting == null) continue;
                    Guid idDuAn = meeting.IdDuAn;
                    // Du phong DTO tu repository chua gan IdDuAn: tra cuu theo IdLichHop.
                    // Chi truy van mot lan cho moi cuoc hop, ke ca hop keo dai nhieu ngay.
                    if (idDuAn == Guid.Empty && meeting.IdLichHop != Guid.Empty)
                    {
                        if (!duAnTheoCuocHop.TryGetValue(meeting.IdLichHop, out idDuAn))
                        {
                            TblLichHop hop = TblLichHop.FetchByID(meeting.IdLichHop);
                            idDuAn = Guid.Empty;
                            if (hop != null && hop.DaXoa != true)
                                Guid.TryParse(Convert.ToString(hop.IdDuAn), out idDuAn);
                            duAnTheoCuocHop[meeting.IdLichHop] = idDuAn;
                        }
                        if (idDuAn != Guid.Empty) meeting.IdDuAn = idDuAn;
                    }
                    if (idDuAn == Guid.Empty) continue;
                    if (!string.IsNullOrWhiteSpace(meeting.TenDuAn))
                    {
                        tenDuAn[idDuAn] = meeting.TenDuAn;
                        continue;
                    }
                    string ten;
                    if (!tenDuAn.TryGetValue(idDuAn, out ten))
                    {
                        TblDuAn project = DuAnManager.Instance.GetDuAnById(idDuAn);
                        ten = project != null ? project.TenDuAn : string.Empty;
                        tenDuAn[idDuAn] = ten ?? string.Empty;
                    }
                    if (!string.IsNullOrWhiteSpace(ten)) meeting.TenDuAn = ten;
                }
            }
        }

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
            var days = LichTrinhManager.Instance.LayLichTrinhNhanVienCoCuocHop(TargetUserId, start, end);
            if (days == null) return;
            BoSungTenDuAnChoLichHop(days);
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
            List<TomTatCuocHop> meetings = day.DanhSachCuocHop != null
                ? day.DanhSachCuocHop.Where(m => m != null)
                    .OrderBy(m => m.ThoiGianBatDau).ToList()
                : new List<TomTatCuocHop>();
            html.Append("<div class='personal-day-dual' role='group' aria-label='Công việc và lịch họp trong ngày'>")
                .Append("<section class='personal-day-pane is-task' aria-label='Danh sách công việc'>")
                .Append("<div class='personal-day-section-head is-task'><span class='personal-day-section-head-title'><i class='fas fa-briefcase'></i> Công việc trong ngày</span><strong class='personal-day-pane-total'>")
                .Append(tasks.Count).Append(" công việc</strong></div><div class='personal-day-pane-scroll' tabindex='0'>");
            if (tasks.Count > 0)
            {
                var groups = tasks.GroupBy(t => string.IsNullOrWhiteSpace(t.TenDuAn)
                    ? (GetResourceText(BackEndResourceKeys.OTHER_PROJECT_UNIDENTIFIED) ?? "Dự án khác")
                    : t.TenDuAn).ToList();
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

            else
            {
                html.Append("<div class='personal-day-pane-empty'><i class='far fa-calendar-check'></i><strong>Không có công việc</strong><small>Chưa có công việc được liệt kê trong ngày.</small></div>");
            }
            html.Append("</div></section>")
                .Append("<section class='personal-day-pane is-meeting' aria-label='Danh sách cuộc họp'>")
                .Append("<div class='personal-day-section-head is-meeting'><span class='personal-day-section-head-title'><i class='far fa-calendar-alt'></i> Lịch họp trong ngày</span><strong class='personal-day-pane-total'>")
                .Append(meetings.Count).Append(" cuộc họp</strong></div><div class='personal-day-pane-scroll' tabindex='0'>");

            // Họp được nhóm theo dự án, rồi xếp theo giờ bắt đầu trong từng dự án.
            // Địa chỉ họp trực tuyến không được xuất ra dưới dạng URL/link/nút tham gia ở Lịch cá nhân.
            if (meetings.Count > 0)
            {
                var meetingGroups = meetings.GroupBy(m => string.IsNullOrWhiteSpace(m.TenDuAn)
                    ? "Chưa xác định dự án" : m.TenDuAn).ToList();
                DateTime now = DateTime.Now;
                foreach (var group in meetingGroups)
                {
                    html.Append("<section class='personal-day-meeting-project'><div class='personal-day-meeting-project-head'>")
                        .Append("<span class='personal-day-meeting-project-name'><i class='fas fa-folder-open'></i> ")
                        .Append(Esc(group.Key)).Append("</span><span class='personal-day-meeting-project-count'>")
                        .Append(group.Count()).Append(" cuộc họp</span></div>");
                    foreach (TomTatCuocHop meeting in group.OrderBy(m => m.ThoiGianBatDau))
                    {
                        int state = now >= meeting.ThoiGianKetThuc ? 3
                            : now >= meeting.ThoiGianBatDau ? 2
                            : meeting.ThoiGianBatDau <= now.AddMinutes(15) ? 1 : 0;
                        string[] statusLabels = { "Đã lên lịch", "Sắp diễn ra", "Đang diễn ra", "Đã kết thúc" };
                        DateTime from = meeting.ThoiGianBatDau > day.Ngay.Date
                            ? meeting.ThoiGianBatDau : day.Ngay.Date;
                        DateTime until = meeting.ThoiGianKetThuc < day.Ngay.Date.AddDays(1)
                            ? meeting.ThoiGianKetThuc : day.Ngay.Date.AddDays(1);
                        string endTime = until == day.Ngay.Date.AddDays(1)
                            ? "24:00" : until.ToString("HH:mm");
                        string host = !string.IsNullOrWhiteSpace(meeting.TenNguoiChuTri)
                            ? meeting.TenNguoiChuTri : meeting.LaChuTri
                                ? "Nhân viên đang xem lịch" : "Chưa xác định người chủ trì";

                        html.Append("<article class='personal-day-meeting-entry is-status-").Append(state).Append("'>")
                            .Append("<div class='personal-day-meeting-clock'><span>").Append(from.ToString("HH:mm"))
                            .Append("</span><small>đến</small><span>").Append(endTime)
                            .Append("</span></div><div class='personal-day-meeting-content'>")
                            .Append("<div class='personal-day-meeting-heading'><div class='personal-day-meeting-name'>");
                        if (meeting.LaChuTri)
                            html.Append("<i class='fas fa-crown personal-meeting-host' title='Bạn chủ trì'></i> ");
                        html.Append(Esc(string.IsNullOrWhiteSpace(meeting.TenCuocHop) ? "Cuộc họp chưa có tên" : meeting.TenCuocHop))
                            .Append("</div><span class='personal-day-meeting-status'>")
                            .Append(statusLabels[state]);
                        if (state == 2)
                            html.Append("<i class='personal-meeting-pulse' aria-hidden='true'></i>");
                        html.Append("</span></div><div class='personal-day-meeting-meta-grid'>")
                            .Append("<div class='personal-day-meeting-detail'><i class='fas fa-user-tie'></i><span><strong>Chủ trì: </strong>")
                            .Append(Esc(host)).Append("</span></div>")
                            .Append("<div class='personal-day-meeting-detail'><i class='fas ")
                            .Append(meeting.LaHopOnline ? "fa-video" : "fa-users")
                            .Append("'></i><span><strong>Hình thức: </strong>")
                            .Append(meeting.LaHopOnline ? "Trực tuyến" : "Trực tiếp").Append("</span></div>")
                            .Append("<div class='personal-day-meeting-detail is-location")
                            .Append(meeting.LaHopOnline ? " is-online" : "")
                            .Append("'><i class='fas ")
                            .Append(meeting.LaHopOnline ? "fa-info-circle" : "fa-map-marker-alt")
                            .Append("'></i><span>");
                        if (meeting.LaHopOnline)
                            html.Append("Thông tin phòng họp được xem tại trang Lịch họp.");
                        else
                            html.Append("<strong>Địa điểm: </strong>")
                                .Append(Esc(string.IsNullOrWhiteSpace(meeting.DiaDiemHop)
                                    ? "Chưa cập nhật địa điểm" : meeting.DiaDiemHop));
                        html.Append("</span></div></div></div></article>");
                    }
                    html.Append("</section>");
                }
            }
            else
            {
                html.Append("<div class='personal-day-pane-empty is-meeting'><i class='far fa-calendar-alt'></i><strong>Không có lịch họp</strong><small>Không có cuộc họp được ghi nhận trong ngày.</small></div>");
            }
            html.Append("</div></section></div>");
            litModalContent.Text = html.ToString();
            upModal.Update();
            mdlDayDetail.OpenModal(true);
        }
        #endregion
    }
}
