using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fMeets.Controls
{
    public partial class CtrlMeet : BaseAdminUserControl
    {
        public EventHandler NewMeetingHandlerCallback;
        public EventHandler EditMeetingHandlerCallback;
        public EventHandler OpenMeetingFilesHandlerCallback;

        public Guid ProjectId
        {
            get
            {
                if (ViewState["ProjectId"] == null)
                {
                    if (this.Page is BaseAdminPage basePage && basePage.CurrentProjectId != Guid.Empty)
                        return basePage.CurrentProjectId;
                    if (Request.QueryString["ProjectId"] != null && Guid.TryParse(Request.QueryString["ProjectId"], out Guid qId))
                        return qId;
                    return Guid.Empty;
                }
                return (Guid)ViewState["ProjectId"];
            }
            set => ViewState["ProjectId"] = value;
        }

        // Quyền mở hai giao diện: PM, Administrator hoặc nhân sự có cả quyền Thêm và Sửa.
        protected bool IsProjectPM
        {
            get
            {
                if (ProjectId == Guid.Empty) return false;
                try
                {
                    Guid? pmId = DuAnManager.Instance.LayIdNhanVienQuanLy(ProjectId);
                    return pmId.HasValue && pmId.Value == SweetContext.Current.UserId;
                }
                catch { return false; }
            }
        }
        protected bool CanSwitchMeetingView => IsProjectPM || CURRENT_PAGE.IsAdministrator || (CURRENT_PAGE.IsAdd && CURRENT_PAGE.IsEdit);
        protected bool CanReadMeetings => CURRENT_PAGE.IsView || CURRENT_PAGE.IsAdd || CURRENT_PAGE.IsEdit || CURRENT_PAGE.IsDelete || IsProjectPM;
        protected bool IsView => this.CURRENT_PAGE.IsView;
        protected bool IsEdit => this.CURRENT_PAGE.IsEdit;
        protected bool IsDelete => this.CURRENT_PAGE.IsDelete;

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButton();
        }

        // Calendar navigation is server-backed, including periods beyond the current month.
        private DateTime CalendarAnchor
        {
            get { return ViewState["CalendarAnchor"] is DateTime ? (DateTime)ViewState["CalendarAnchor"] : DateTime.Today; }
            set { ViewState["CalendarAnchor"] = value.Date; }
        }
        private string CalendarMode
        {
            get { return Convert.ToString(ViewState["CalendarMode"]) == "week" ? "week" : "month"; }
            set { ViewState["CalendarMode"] = value == "week" ? "week" : "month"; }
        }
        private bool CalendarMine
        {
            get { return ViewState["CalendarMine"] is bool ? (bool)ViewState["CalendarMine"] : !CanSwitchMeetingView; }
            set { ViewState["CalendarMine"] = value; }
        }
        // Find recursively so adding markup does not require the user to hand-edit *.designer.cs.
        private static Control CalendarFindControl(Control parent, string id)
        {
            if (parent.ID == id) return parent;
            foreach (Control child in parent.Controls)
            {
                Control match = CalendarFindControl(child, id);
                if (match != null) return match;
            }
            return null;
        }
        protected HiddenField CalendarCreateHidden => CalendarFindControl(this, "hdfCalendarCreateDate") as HiddenField;
        protected LinkButton CalendarCreateButton => CalendarFindControl(this, "btnCreateCalendarMeeting") as LinkButton;

        protected void btnCreateCalendarMeeting_Click(object sender, EventArgs e)
        {
            if (!CURRENT_PAGE.IsAdd || ProjectId == Guid.Empty) { ShowAccessDeniedNotify(); return; }
            DateTime selected;
            if (!DateTime.TryParseExact(CalendarCreateHidden != null ? CalendarCreateHidden.Value : null, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out selected)) { ShowInvalidDataError(); return; }
            // Không nhận ngày nằm ngoài khoảng hợp lý để tránh giá trị bị sửa trên client.
            if (selected.Year < 2000 || selected.Year > 2100) { ShowInvalidDataError(); return; }
            NewMeetingHandlerCallback?.Invoke(selected.Date, EventArgs.Empty);
        }

        protected void btnCalendarMine_Click(object sender, EventArgs e)
        {
            CalendarMine = !CalendarMine;
            upMain.Update();
        }
        protected void btnCalendarNavigate_Click(object sender, EventArgs e)
        {
            var button = sender as LinkButton;
            string action = button == null ? "today" : button.CommandArgument;
            if (action == "today") CalendarAnchor = DateTime.Today;
            else if (CalendarMode == "week") CalendarAnchor = CalendarAnchor.AddDays(action == "prev" ? -7 : 7);
            else CalendarAnchor = CalendarAnchor.AddMonths(action == "prev" ? -1 : 1);
            upMain.Update();
        }
        protected void btnCalendarMode_Click(object sender, EventArgs e)
        {
            LinkButton button = sender as LinkButton;
            CalendarMode = button == null ? "month" : button.CommandArgument;
            upMain.Update();
        }
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            var shell = FindControl("meetViewShell") as HtmlGenericControl;
            if (shell != null)
            {
                shell.Attributes["data-can-list"] = CanSwitchMeetingView ? "1" : "0";
                shell.Attributes["data-project"] = ProjectId.ToString("N");
                if (!CanSwitchMeetingView) shell.Attributes["class"] = "meet-view-shell meet-mode-calendar";
            }
            // Thanh công cụ duy nhất phục vụ cả Danh sách và Calendar.
            var actionHost = upMain.ContentTemplateContainer.FindControl("tagOther") as HtmlGenericControl;
            var switcher = actionHost != null ? actionHost.FindControl("meetViewSwitcher") as HtmlGenericControl : null;
            if (switcher != null) switcher.Visible = CanSwitchMeetingView;
            var mineButton = upMain.ContentTemplateContainer.FindControl("btnCalendarMine") as LinkButton;
            if (mineButton != null) mineButton.CssClass = "meet-cal-period-btn" + (CalendarMine ? " is-active" : "");
            PlaceHolder host = upMain.ContentTemplateContainer.FindControl("phMeetingCalendar") as PlaceHolder;
            if (host != null)
            {
                host.Controls.Clear();
                try { host.Controls.Add(new LiteralControl(BuildMeetingCalendarHtml())); }
                catch (Exception)
                {
                    host.Controls.Add(new LiteralControl("<div class='meet-cal-empty-state'>Không tải được dữ liệu lịch họp. Vui lòng thử lại.</div>"));
                }
            }
        }
        private static DateTime CalendarStartOfWeek(DateTime date)
        {
            return date.Date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
        }
        private static TimeSpan CalendarTime(object input, TimeSpan fallback)
        {
            if (input == null || input == DBNull.Value) return fallback;
            if (input is TimeSpan) return (TimeSpan)input;
            if (input is DateTime) return ((DateTime)input).TimeOfDay;
            TimeSpan ts;
            if (TimeSpan.TryParse(Convert.ToString(input), out ts)) return ts;
            DateTime date;
            if (DateTime.TryParse(Convert.ToString(input), out date)) return date.TimeOfDay;
            return fallback;
        }
        private static string CalendarEsc(string text) { return HttpUtility.HtmlEncode(text ?? string.Empty); }
        private static string CalendarPreview(string html)
        {
            string plain = HttpUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(html ?? "", "<[^>]*>", " "));
            plain = System.Text.RegularExpressions.Regex.Replace(plain, @"\s+", " ").Trim();
            return plain.Length > 170 ? plain.Substring(0, 167) + "…" : plain;
        }
        private static string CalendarDisplayTime(DateTime start, DateTime end, DateTime day)
        {
            DateTime dayEnd = day.AddDays(1);
            DateTime from = start < day ? day : start;
            DateTime to = end > dayEnd ? dayEnd : end;
            return from.ToString("HH:mm") + "–" + (to == dayEnd ? "24:00" : to.ToString("HH:mm"));
        }
        private bool _calendarAttendeesLoaded;
        private sealed class CalendarAttendees
        {
            public int Count { get; set; }
            public bool IncludesCurrentUser { get; set; }
        }
        // Dùng bảng quan hệ nhân viên–cuộc họp. Các tham số WHERE đều là GUID phía server.
        private Dictionary<Guid, CalendarAttendees> LoadCalendarAttendees()
        {
            _calendarAttendeesLoaded = false;
            var result = new Dictionary<Guid, CalendarAttendees>();
            string query = "SELECT p.IdLichHop, COUNT(*) AS SoNguoi, MAX(CASE WHEN p.IdNhanVien='" + SweetContext.Current.UserId.ToString("D") + "' THEN 1 ELSE 0 END) AS CoToi " +
                "FROM dbo.TblLichHop_NhanVien p INNER JOIN dbo.TblLichHop m ON m.IdLichHop=p.IdLichHop " +
                "WHERE m.IdDuAn='" + ProjectId.ToString("D") + "' AND m.DaXoa=0 GROUP BY p.IdLichHop";
            try
            {
                using (IDataReader reader = new InlineQuery().ExecuteReader(query))
                {
                    while (reader.Read())
                    {
                        Guid id;
                        if (!Guid.TryParse(Convert.ToString(reader["IdLichHop"]), out id)) continue;
                        result[id] = new CalendarAttendees
                        {
                            Count = Convert.ToInt32(reader["SoNguoi"]),
                            IncludesCurrentUser = Convert.ToInt32(reader["CoToi"]) > 0
                        };
                    }
                }
                _calendarAttendeesLoaded = true;
            }
            catch (Exception)
            {
                // Không lọc cá nhân khi không truy vấn được quan hệ tham gia.
            }
            return result;
        }
        // Người tạo cuộc họp được xem là người chủ trì theo dữ liệu TblLichHop.IdNguoiTao.
        // Tra cứu theo danh sách GUID từ database, không nhận SQL tự do từ trình duyệt.
        private Dictionary<Guid, string> LoadCalendarChairNames(IEnumerable<TblLichHop> meetings)
        {
            var result = new Dictionary<Guid, string>();
            List<Guid> ids = meetings.Where(m => m.IdNguoiTao != Guid.Empty)
                .Select(m => m.IdNguoiTao).Distinct().ToList();
            if (ids.Count == 0) return result;
            try
            {
                string safeIds = string.Join(",", ids.Select(id => "'" + id.ToString("D") + "'"));
                string sql = "SELECT UserId, DisplayName, UserName FROM dbo.aspnet_Users WHERE UserId IN (" + safeIds + ")";
                using (IDataReader reader = new InlineQuery().ExecuteReader(sql))
                {
                    while (reader.Read())
                    {
                        Guid id;
                        if (!Guid.TryParse(Convert.ToString(reader["UserId"]), out id)) continue;
                        string name = Convert.ToString(reader["DisplayName"]);
                        if (string.IsNullOrWhiteSpace(name)) name = Convert.ToString(reader["UserName"]);
                        if (!string.IsNullOrWhiteSpace(name)) result[id] = name.Trim();
                    }
                }
            }
            catch (Exception) { /* Vẫn hiển thị cuộc họp nếu không tải được tên người chủ trì. */ }
            return result;
        }
        private static string CalendarAttr(string value) { return CalendarEsc(value).Replace("'", "&#39;"); }
        // Đồng bộ với UpdateMeetingStatuses: 0=đã lên lịch, 1=sắp diễn ra, 2=đang diễn ra, 3=đã kết thúc.
        // Tính từ giờ server để thẻ có màu đúng ngay khi Calendar được render, kể cả trước lần refresh 60 giây tiếp theo.
        private static byte CalendarStatusAt(TblLichHop meeting, DateTime now)
        {
            if (now >= meeting.ThoiGianKetThuc) return 3;
            if (now >= meeting.ThoiGianBatDau) return 2;
            if (meeting.ThoiGianBatDau <= now.AddMinutes(15)) return 1;
            return 0;
        }
        private string BuildCalendarEventButton(TblLichHop meeting, DateTime day, Dictionary<Guid, CalendarAttendees> attendees, Dictionary<Guid, string> chairs)
        {
            string title = CalendarEsc(meeting.TenCuocHop);
            string place = CalendarEsc(meeting.DiaDiemHop);
            string description = CalendarEsc(CalendarPreview(meeting.NoiDungCuocHop));
            byte statusCode = CalendarStatusAt(meeting, DateTime.Now);
            string status = CalendarEsc(GetTrangThaiCuocHopText(statusCode));
            string time = CalendarDisplayTime(meeting.ThoiGianBatDau, meeting.ThoiGianKetThuc, day);
            bool ownerIsMe = meeting.IdNguoiTao == SweetContext.Current.UserId;
            CalendarAttendees participant;
            attendees.TryGetValue(meeting.IdLichHop, out participant);
            string chairText = "Chưa xác định người chủ trì";
            string chairClass = "";
            if (ownerIsMe) { chairText = "Bạn là người chủ trì"; chairClass = " is-self"; }
            else if (meeting.IdNguoiTao != Guid.Empty)
            {
                string chairName;
                if (chairs.TryGetValue(meeting.IdNguoiTao, out chairName)) chairText = chairName + " là người chủ trì";
            }
            var html = new StringBuilder(800);
            html.Append("<button type='button' class='meet-cal-event is-status-" + statusCode + (ownerIsMe ? " is-hosted" : "") + "' data-meet-status='" + statusCode + "' onclick='return SweetMeetCalendar.openDetail(&quot;" + meeting.IdLichHop.ToString("D") + "&quot;);' aria-label='" + CalendarAttr(status + ". " + (ownerIsMe ? "Bạn chủ trì: " : "Xem cuộc họp: ") + meeting.TenCuocHop) + "'>");
            if (ownerIsMe) html.Append("<i class='fas fa-crown meet-cal-host-icon' title='Bạn là người chủ trì' aria-hidden='true'></i>");
            if (statusCode == 1 || statusCode == 2) html.Append("<span class='meet-cal-status-dot' aria-hidden='true'></span>");
            html.Append("<span class='meet-cal-event-time'>" + CalendarEsc(time.Split('–')[0]) + "</span><span class='meet-cal-event-text'>" + title + "</span>");
            html.Append("<span class='meet-cal-popover'><span class='meet-cal-tooltip-head'><strong class='meet-cal-tooltip-title'>" + title + "</strong><span class='meet-cal-tooltip-status is-status-" + statusCode + "'>" + status + "<i class='meet-cal-tooltip-status-dot' aria-hidden='true'></i></span></span><span class='meet-cal-chair" + chairClass + "'><i class='fas fa-user-tie'></i> " + CalendarEsc(chairText) + "</span>");
            if (participant != null) html.Append("<span><i class='fas fa-users'></i> " + participant.Count + " người tham gia" + (participant.IncludesCurrentUser ? " · <b>Bạn tham gia</b>" : "") + "</span>");
            html.Append("<span><i class='far fa-clock'></i> " + CalendarEsc(meeting.ThoiGianBatDau.ToString("dd/MM HH:mm")) + " – " + CalendarEsc(meeting.ThoiGianKetThuc.ToString("dd/MM HH:mm")) + "</span>");
            html.Append("<span><i class='fas fa-map-marker-alt'></i> " + (string.IsNullOrWhiteSpace(place) ? "Chưa có địa điểm" : place) + "</span>");
            if (!string.IsNullOrWhiteSpace(description)) html.Append("<small>" + description + "</small>");
            html.Append("</span></button>");
            return html.ToString();
        }
        // Mỗi ngày tối đa hai cuộc họp hiển thị trực tiếp. Toàn bộ lịch họp được giữ
        // trong vùng ẩn để ExtraModal có thể sử dụng mà không gọi API mới.
        private void RenderCalendarDayMeetings(StringBuilder html, List<TblLichHop> meetings, DateTime date, Dictionary<Guid, CalendarAttendees> attendees, Dictionary<Guid, string> chairs)
        {
            List<TblLichHop> ordered = meetings.OrderBy(m => m.ThoiGianBatDau).ThenBy(m => m.TenCuocHop).ToList();
            html.Append("<div class='meet-cal-day-items'>");
            if (ordered.Count == 0)
                html.Append("<span class='meet-cal-no-meetings'><i class='far fa-calendar' aria-hidden='true'></i> Trống</span>");
            else
            {
                foreach (TblLichHop meeting in ordered.Take(2))
                    html.Append(BuildCalendarEventButton(meeting, date, attendees, chairs));
                if (ordered.Count > 2)
                    html.Append("<button type='button' class='meet-cal-view-more' aria-label='Xem thêm " + (ordered.Count - 2) + " cuộc họp ngày " + date.ToString("dd/MM/yyyy") + "'><i class='fas fa-plus-circle' aria-hidden='true'></i> " + (ordered.Count - 2) + " cuộc họp nữa</button>");
            }
            html.Append("</div>");
            // Nội dung nguồn để sử dụng trong ExtraModal; ẩn hoàn toàn khỏi ô ngày.
            html.Append("<div class='meet-cal-day-all' hidden>");
            foreach (TblLichHop meeting in ordered)
                html.Append(BuildCalendarEventButton(meeting, date, attendees, chairs));
            html.Append("</div>");
        }
        private string BuildMeetingCalendarHtml()
        {
            if (!CanReadMeetings || ProjectId == Guid.Empty)
                return "<div class='meet-cal-empty-state'>Không có quyền xem lịch họp hoặc chưa chọn dự án.</div>";
            DateTime anchor = CalendarAnchor;
            bool weekView = CalendarMode == "week";
            DateTime firstOfMonth = new DateTime(anchor.Year, anchor.Month, 1);
            DateTime start = weekView ? CalendarStartOfWeek(anchor) : CalendarStartOfWeek(firstOfMonth);
            int offset = ((int)firstOfMonth.DayOfWeek + 6) % 7;
            int cells = weekView ? 7 : Math.Max(35, ((offset + DateTime.DaysInMonth(anchor.Year, anchor.Month) + 6) / 7) * 7);
            DateTime end = start.AddDays(cells);
            var meetings = MeetManager.Instance.GetMeetingsByProject(ProjectId) ?? new List<TblLichHop>();
            meetings = meetings.Where(m => m != null && m.DaXoa != true && m.ThoiGianKetThuc > m.ThoiGianBatDau && m.ThoiGianBatDau < end && m.ThoiGianKetThuc > start).ToList();
            Dictionary<Guid, CalendarAttendees> attendees = LoadCalendarAttendees();
            if (CalendarMine && _calendarAttendeesLoaded)
            {
                Guid userId = SweetContext.Current.UserId;
                meetings = meetings.Where(m => (m.IdNguoiTao == userId) ||
                    (attendees.ContainsKey(m.IdLichHop) && attendees[m.IdLichHop].IncludesCurrentUser)).ToList();
            }
            Dictionary<Guid, string> chairs = LoadCalendarChairNames(meetings);
            var configs = new SubSonic.Select().From(TblCauHinhTuanLamViec.Schema).ExecuteTypedList<TblCauHinhTuanLamViec>();
            var configByDay = configs.GroupBy(c => Convert.ToInt32(c.NgayTrongTuan)).ToDictionary(g => g.Key, g => g.First());
            var holidays = new SubSonic.Select().From(TblLichNgoaiLe.Schema)
                .Where(TblLichNgoaiLe.Columns.DaXoa).IsEqualTo(false)
                .And(TblLichNgoaiLe.Columns.NgayBatDau).IsLessThan(end)
                .And(TblLichNgoaiLe.Columns.NgayKetThuc).IsGreaterThanOrEqualTo(start)
                .ExecuteTypedList<TblLichNgoaiLe>()
                .Where(x => !x.LaNgayLamViec).ToList();
            var html = new StringBuilder(23000);
            string label = weekView ? "Tuần " + start.ToString("dd/MM") + " – " + end.AddDays(-1).ToString("dd/MM/yyyy") : "Tháng " + anchor.ToString("MM/yyyy");
            html.Append("<div class='meet-cal-period-data' data-cal-period='" + CalendarEsc(label) + "' style='display:none'></div>");
            if (CalendarMine && !_calendarAttendeesLoaded)
                html.Append("<div class='meet-cal-warning'>Không đọc được danh sách nhân viên tham gia. Tạm hiển thị lịch chung của dự án.</div>");
            html.Append("<div class='meet-cal-scroll'><div class='meet-calendar-grid" + (weekView ? " is-week" : " is-month") + "'>");
            foreach (string dayName in new[] { "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy", "Chủ Nhật" }) html.Append("<div class='meet-cal-dow'>" + dayName + "</div>");
            for (int dayIndex = 0; dayIndex < cells; dayIndex++)
            {
                DateTime date = start.AddDays(dayIndex);
                TblCauHinhTuanLamViec dayConfig;
                bool hasConfig = configByDay.TryGetValue((int)date.DayOfWeek, out dayConfig);
                bool isWorkingDay = hasConfig ? dayConfig.LaNgayLamViec : date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
                TblLichNgoaiLe holiday = holidays.FirstOrDefault(h => date.Date >= h.NgayBatDau.Date && date.Date <= h.NgayKetThuc.Date);
                bool isOff = holiday != null || !isWorkingDay;
                var dayMeetings = meetings.Where(m => m.ThoiGianBatDau < date.AddDays(1) && m.ThoiGianKetThuc > date).ToList();
                html.Append("<div class='meet-cal-day" + (isOff ? " is-off" : "") + (dayMeetings.Count > 0 ? " has-meetings" : "") + (date.Date == DateTime.Today ? " is-today" : "") + (!weekView && date.Month != anchor.Month ? " is-outside" : "") + "' data-day-label='" + date.ToString("dd/MM/yyyy") + "' data-meet-count='" + dayMeetings.Count + "'" + (dayMeetings.Count > 0 ? " tabindex='0' role='button' aria-label='Xem " + dayMeetings.Count + " cuộc họp ngày " + date.ToString("dd/MM/yyyy") + "'" : "") + ">");
                html.Append("<div class='meet-cal-day-top'><span class='meet-cal-day-number'>" + date.Day + "</span>");
                html.Append("<span class='meet-cal-day-tools'>");
                if (holiday != null) html.Append("<span class='meet-cal-day-badge' title='" + CalendarEsc(holiday.TenNgoaiLe) + "'><i class='fas fa-umbrella-beach'></i> " + CalendarEsc(holiday.TenNgoaiLe) + "</span>");
                else if (isOff) html.Append("<span class='meet-cal-day-badge'><i class='fas fa-mug-hot'></i> Nghỉ</span>");
                if (CURRENT_PAGE.IsAdd) html.Append("<button type='button' class='meet-cal-create' title='Tạo cuộc họp' aria-label='Tạo cuộc họp ngày " + date.ToString("dd/MM/yyyy") + "' onclick='event.stopPropagation();return SweetMeetCalendar.createMeeting(&quot;" + date.ToString("yyyy-MM-dd") + "&quot;);'><i class='fas fa-plus' aria-hidden='true'></i></button>");
                html.Append("</span></div>");
                // Calendar gọn: một ngày chỉ có thẻ họp, không hiển thị ca sáng/chiều.
                RenderCalendarDayMeetings(html, dayMeetings, date, attendees, chairs);
                html.Append("</div>");
            }
            html.Append("</div></div>");
            return html.ToString();
        }

        private void RegisterAsyncButton()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            if (script != null)
            {
                script.RegisterAsyncPostBackControl(lbtSearchSingle);
                script.RegisterAsyncPostBackControl(lbtSearchAdvanced);
                script.RegisterAsyncPostBackControl(lbtCancel);
                script.RegisterAsyncPostBackControl(btnRefreshMeetingStatuses);
                if (CalendarCreateButton != null) script.RegisterAsyncPostBackControl(CalendarCreateButton);
            }
        }

        public void InitControls()
        {
            ApplyControlsText();
            AssignSearchColumns();
            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
            lbtAdd.Visible = this.CURRENT_PAGE.IsAdd;

            MasterTemplate master = Page.Master as MasterTemplate;
            if (master != null)
                master.LoadSessionLastSearch(searchTagBox, pnlSearchPopup, grvData, txtSearchSingle);

            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = "MaCuocHop";
            grvData.CurrentSortDerection = "ASC";
            Rebind();
            pnlSearch.Update();
            pnlButtons.Update();
        }

        private void AssignSearchColumns()
        {
            txtSearchTenCuocHop.SearchColumn = "TenCuocHop";
        }

        public void Rebind()
        {
            UpdateMeetingStatuses();
            grvData.CurrentPageIndex = 1;
            grvData.Rebind();
        }

        private bool UpdateMeetingStatuses()
        {
            if (this.ProjectId == Guid.Empty)
                return false;

            DateTime now = DateTime.Now;
            bool hasChanged = false;

            List<TblLichHop> meetings = MeetManager.Instance.GetMeetingsByProject(this.ProjectId);

            if (meetings == null || meetings.Count == 0)
                return false;

            foreach (TblLichHop meeting in meetings)
            {
                if (meeting == null || meeting.DaXoa == true)
                    continue;

                if (meeting.ThoiGianBatDau == DateTime.MinValue ||
                    meeting.ThoiGianKetThuc == DateTime.MinValue)
                    continue;

                DateTime startTime = meeting.ThoiGianBatDau;
                DateTime endTime = meeting.ThoiGianKetThuc;

                byte newStatus;

                // 3 - Kết thúc: thời điểm hiện tại đã tới hoặc vượt quá thời gian kết thúc.
                if (now >= endTime)
                {
                    newStatus = 3;
                }
                // 2 - Đang diễn ra: hiện tại nằm trong khoảng bắt đầu -> kết thúc.
                else if (now >= startTime && now < endTime)
                {
                    newStatus = 2;
                }
                // 1 - Sắp diễn ra: còn tối đa 15 phút trước giờ bắt đầu.
                else if (now < startTime && startTime <= now.AddMinutes(15))
                {
                    newStatus = 1;
                }
                // 0 - Đã lên lịch: còn hơn 15 phút mới bắt đầu.
                else
                {
                    newStatus = 0;
                }

                int currentStatus = Convert.ToInt32(meeting.TrangThai);

                if (currentStatus != newStatus)
                {
                    meeting.TrangThai = newStatus;
                    meeting.Save();
                    hasChanged = true;
                }
            }

            return hasChanged;
        }

        protected void btnRefreshMeetingStatuses_Click(object sender, EventArgs e)
        {
            try
            {
                // Sử dụng postback đã có để mở chi tiết từ calendar qua đúng server.
                Guid calendarMeetId;
                if (Guid.TryParse(Request.Form["meetCalendarSelectedId"], out calendarMeetId) && calendarMeetId != Guid.Empty)
                {
                    if (!CanReadMeetings) { ShowAccessDeniedNotify(); return; }
                    TblLichHop selectedMeet = TblLichHop.FetchByID(calendarMeetId);
                    if (selectedMeet == null || selectedMeet.DaXoa == true || selectedMeet.IdDuAn != ProjectId)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }
                    CtrlViewMeetDetail1.OpenModal(calendarMeetId);
                    upMain.Update();
                    return;
                }
                bool hasChanged = UpdateMeetingStatuses();

                if (hasChanged)
                {
                    // Async UpdatePanel: đổi màu badge của GridView + Calendar nhưng giữ tháng/tuần,
                    // trang hiện tại, chế độ xem và bộ lọc người dùng trong ViewState/sessionStorage.
                    grvData.Rebind();
                    upMain.Update();
                }
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        private void ApplyControlsText()
        {
            txtSearchSingle.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            txtSearchTenCuocHop.SearchTagItemText = "Tên cuộc họp";
            lbtAdd.ToolTip = lbtAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);

            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.MEETING_NAME),
                "Người tham gia",
                GetResourceText(BackEndResourceKeys.START_TIME),
                GetResourceText(BackEndResourceKeys.END_TIME),
                GetResourceText(BackEndResourceKeys.MEETING_ROOM),
                GetResourceText(BackEndResourceKeys.STATUS),
                GetResourceText(BackEndResourceKeys.ACTION)
            };
            grvData.HeaderTexts = lstTableHeader;
        }

        protected void grvData_NeedDataSource(object sender, ExtraGridEventArg e)
        {
            try
            {
                GridviewExtension grid = sender as GridviewExtension;
                if (grid == null)
                {
                    this.ShowInvalidDataError();
                    return;
                }

                int totalRows = 0;
                int rowIndex = (grid.CurrentPageIndex - 1) * grid.CurrentPageSize;
                int pageSize = rowIndex + grid.CurrentPageSize;
                DataTable dt = null;

                if (grid.GridSearchType == GridSearchType.Single)
                {
                    dt = MeetManager.Instance.SearchMeeting(this.ProjectId, txtSearchSingle.Text, null, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                }
                else
                {
                    Dictionary<string, object> keyValueSearchs = new ControlHelpers().GetControlValues(pnlSearchPopup);
                    dt = MeetManager.Instance.SearchMeeting(this.ProjectId, "", keyValueSearchs, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                }

                if (dt == null || dt.Rows.Count == 0)
                {
                    grvData.DataSource = null;
                    grvData.DataBind();
                    ctrlGridviewPaging.Visible = false;
                }
                else
                {
                    ctrlGridviewPaging.Visible = true;
                    grvData.VirtualItemCount = totalRows;
                    grvData.DataSource = dt;
                    grvData.DataBind();
                    ctrlGridviewPaging.PageIndex = grvData.CurrentPageIndex;
                    ctrlGridviewPaging.PageSize = grvData.CurrentPageSize;
                    ctrlGridviewPaging.TotalItems = totalRows;
                    ctrlGridviewPaging.InitLoad();
                }

                upMain.Update();
                pnlButtons.Update();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void grvData_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "VIEW_MEMBERS":
                    if (!this.CURRENT_PAGE.IsEdit && !this.CURRENT_PAGE.IsView)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }

                    int rowIndexMeet = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndexMeet = ((GridViewRow)((WebControl)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndexMeet = Convert.ToInt32(e.CommandArgument);

                    Guid idCuocHop = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndexMeet].Value.ToString(), out idCuocHop))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    TblLichHop meet = TblLichHop.FetchByID(idCuocHop);
                    if (meet == null) return;

                    Guid? hostId = meet.IdNguoiTao;
                    DateTime startDate = meet.ThoiGianBatDau;
                    DateTime endDate = meet.ThoiGianKetThuc;

                    ((CtrlXemNhanVienMeet)CtrlXemNhanVienMeet1).OpenModal(idCuocHop, startDate, endDate, meet.TenCuocHop, hostId);
                    break;

                case "MEETING_FILES":
                    if (!this.IsView)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }

                    Guid meetingIdForFiles;
                    if (!Guid.TryParse(Convert.ToString(e.CommandArgument), out meetingIdForFiles) || meetingIdForFiles == Guid.Empty)
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    if (OpenMeetingFilesHandlerCallback != null)
                    {
                        OpenMeetingFilesHandlerCallback(meetingIdForFiles, EventArgs.Empty);
                    }
                    break;

                case "ITEM_VIEW":
                    if (!this.CURRENT_PAGE.IsView)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }

                    int rowIndexView = e.CommandSource.GetType() != typeof(GridviewExtension)
                        ? ((GridViewRow)((LinkButton)e.CommandSource).NamingContainer).RowIndex
                        : Convert.ToInt32(e.CommandArgument);

                    Guid meetIdView = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndexView].Value.ToString(), out meetIdView))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    CtrlViewMeetDetail1.OpenModal(meetIdView);
                    break;

                case "ITEM_DETAIL":
                    if (!this.CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }

                    int rowIndexEdit = e.CommandSource.GetType() != typeof(GridviewExtension)
                        ? ((GridViewRow)((LinkButton)e.CommandSource).NamingContainer).RowIndex
                        : Convert.ToInt32(e.CommandArgument);

                    Guid meetIdEdit = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndexEdit].Value.ToString(), out meetIdEdit))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    if (EditMeetingHandlerCallback != null)
                        EditMeetingHandlerCallback(meetIdEdit, EventArgs.Empty);
                    break;
                case "ITEM_DELETE":
                    if (!this.CURRENT_PAGE.IsDelete)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    int rowIndexDel = (e.CommandSource.GetType() != typeof(GridviewExtension))
                        ? ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex
                        : Convert.ToInt32(e.CommandArgument);
                    Guid meetIdDel = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndexDel].Value.ToString(), out meetIdDel))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    TblLichHop meetDel = TblLichHop.FetchByID(meetIdDel);
                    if (meetDel == null || meetDel.DaXoa == true)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }

                    ConfirmResult result = new ConfirmResult { CommandName = "MEETING_DELETE", Value = meetDel };
                    this.CURRENT_PAGE.CurrentConfirmResult = result;
                    MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION), $"Bạn có chắc chắn xóa cuộc họp: {meetDel.TenCuocHop}?", MSGButton.DeleteCancel, MSGIcon.Error);
                    OpenMessageBox(msg, result, false, false);
                    break;
            }
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null && e.Submit && e.CommandName != null && e.CommandName.Contains("MEETING_DELETE"))
            {
                TblLichHop meet = e.Value as TblLichHop;
                if (meet == null)
                {
                    ShowInvalidNotFoundData();
                    return;
                }

                try
                {
                    MeetManager.Instance.DeleteMeet(meet);
                    ShowSuccessDeleteData();
                    grvData.CurrentPageIndex = 1;
                    grvData.Rebind();
                }
                catch (Exception exc)
                {
                    ShowNotify(exc.Message, MSGType.Error);
                }
            }
        }

        protected void ctrlGridviewPaging_PageChanged(object sender, GridviewCustomPageChangeArgs e)
        {
            grvData.CurrentPageSize = e.CurrentPageSize;
            grvData.CurrentPageIndex = e.CurrentPageNumber;
            grvData.Rebind();
        }

        protected void btnSearch_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master != null)
                master.btnSearchSingle_Click(searchTagBox, grvData, txtSearchSingle);
            upSearchTagBox.Update();
        }

        protected void searchTagBox_TagClosed(object sender, SearchTagItem tag)
        {
            try
            {
                MasterTemplate master = Page.Master as MasterTemplate;
                if (master != null)
                {
                    GridSearchType? searchType;
                    master.searchTagBox_TagClosed(searchTagBox, tag, null, pnlSearchPopup, grvData, txtSearchSingle, out searchType);
                }
                upSearchTagBox.Update();
                string script = string.Format("$('#{0}').val('');", txtSearchSingle.ClientID);
                ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "UpdateTxtSearch", script, true);
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void btnSearchAdvanced_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master != null)
                master.btnSearchAdvanced_Click(searchTagBox, null, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            if (pnlSearchPopup != null)
                new ControlHelpers().ClearControlValues(pnlSearchPopup.Controls);
            pnlSearch.Update();

            MasterTemplate master = Page.Master as MasterTemplate;
            if (master != null)
                master.btnSearchAdvanced_Click(searchTagBox, null, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }

        protected void lbtAdd_Click(object sender, EventArgs e)
        {
            if (!this.CURRENT_PAGE.IsAdd)
            {
                ShowAccessDeniedNotify();
                return;
            }
            NewMeetingHandlerCallback?.Invoke(Guid.Empty, EventArgs.Empty);
        }

        protected string GetTrangThaiCuocHopText(object value)
        {
            if (value == null || value == DBNull.Value) return "—";
            TrangThaiCuocHopEnum status = (TrangThaiCuocHopEnum)Convert.ToInt32(value);
            return GetResourceText(MeetManager.Instance.GetValueForTrangThaiCuoHop(status));
        }

        public string GetAssigneeDisplay(object tenNhanVienObj, object avatarsObj)
        {
            string names = tenNhanVienObj?.ToString() ?? "";
            string avatars = avatarsObj?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(names)) return "";

            string[] nameArray = names.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            string[] avatarArray = avatars.Split(new string[] { "," }, StringSplitOptions.None);

            string html = "";
            string[] colors = { "#f59e0b", "#3b82f6", "#10b981", "#8b5cf6", "#ec4899" };

            int maxDisplay = 2;
            int count = nameArray.Length;

            for (int i = 0; i < Math.Min(count, maxDisplay); i++)
            {
                string name = nameArray[i].Trim();
                string avatar = (i < avatarArray.Length) ? avatarArray[i].Trim() : "";
                string color = colors[i % colors.Length];
                bool isDefaultAvatar = string.IsNullOrEmpty(avatar) || avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);

                if (!isDefaultAvatar)
                {
                    string avatarUrl = avatar.StartsWith("~") ? Page.ResolveUrl(avatar) : avatar;
                    string safeName = System.Web.HttpUtility.HtmlAttributeEncode(name ?? string.Empty);
                    string safeInitials = System.Web.HttpUtility.HtmlEncode(GetInitials(name));
                    string fallbackHtml = $"<div class='avatar-circle' style='background-color: {color};' title='{safeName}'>{safeInitials}</div>";
                    string fallbackJs = System.Web.HttpUtility.JavaScriptStringEncode(fallbackHtml);
                    string safeAvatarUrl = System.Web.HttpUtility.HtmlAttributeEncode(avatarUrl);
                    html += $"<img src='{safeAvatarUrl}' class='avatar-circle' style='object-fit: cover;' title='{safeName}' onerror=\"this.onerror=null;this.outerHTML='{fallbackJs}';\" />";
                }
                else
                {
                    string initials = GetInitials(name);
                    html += $"<div class='avatar-circle' style='background-color: {color};' title='{name}'>{initials}</div>";
                }
            }

            if (count > maxDisplay)
                html += $"<div class='avatar-circle avatar-more' title='Và {count - maxDisplay} người khác'>+{count - maxDisplay}</div>";

            return html;
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "";
            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[parts.Length - 2].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        protected void bootstrapDropdown_SelectedValueChanged(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master != null)
                master.btnSearchSingle_Click(searchTagBox, pnlSearchDefaultStatus, grvData, txtSearchSingle);

            upSearchTagBox.Update();
            if (pnlSearchDropdowns != null)
                pnlSearchDropdowns.Update();
        }
    }
}
