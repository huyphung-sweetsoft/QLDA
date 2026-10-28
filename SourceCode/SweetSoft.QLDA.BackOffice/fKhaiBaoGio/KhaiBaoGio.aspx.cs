using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Transactions;
using SweetSoft.QLDA.BackOffice.fTasks.Controls;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
namespace SweetSoft.QLDA.BackOffice.fKhaiBaoGio
{
    /// <summary>
    /// Trang bấm giờ làm việc trong ngày của nhân viên (Stopwatch Time Tracking):
    /// - Bỏ chia tách nhập tay theo ca sáng/chiều; nhân viên nhấn Bắt đầu -> Dừng -> Tiếp tục cho từng Task.
    /// - Tại một thời điểm chỉ có duy nhất 1 Task được chạy đồng hồ.
    /// - Tự động trừ giờ nghỉ trưa (dựa vào khoảng giữa Ca sáng và Ca chiều trong CauHinhTuanLamViec).
    /// - Tự động dừng đồng hồ và chốt sổ sau giờ kết thúc ca chiều (VD: 17:00).
    /// </summary>
    public partial class KhaiBaoGio : BaseAdminPage
    {
        // Giong TaskList: tab du an can dung ModuleKey dung de CtrlProjectTabs active.
        // Duong V1 ngoai du an van giu module EmployeeDashboard.
        public override ModuleKeys PAGE_FUNCTION_CODE =>
            CurrentProjectId != Guid.Empty ? ModuleKeys.KhaiBaoGio : ModuleKeys.EmployeeDashboard;

        /// <summary>
        /// Bỏ qua kiểm tra quyền Menu tự động của BaseAdminPage để nhân viên không bị lỗi 403.
        /// Trang này kiểm soát quyền truy cập trực tiếp bằng cờ LaNhanVien.
        /// </summary>
        public override bool IsLogin => true;

        /// <summary>
        /// Trang trong du an va V2 duoc kich hoat => chi render / xu ly V2.
        /// Ngoai du an van giu V1 de khong pha duong legacy chua chuyen doi.
        /// </summary>
        protected bool LaGiaoDienKbg20 =>
            CurrentProjectId != Guid.Empty && KhaiBaoGioManager.KhaiBaoGio20DaKichHoat();

        private static readonly string[] Weekdays =
            { "Chủ nhật", "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7" };

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        #region Vòng đời trang

        protected void Page_Load(object sender, EventArgs e)
        {
            Guid userId;
            if (!TryGetCurrentUserId(out userId))
            {
                Response.Redirect(GetRelativeClientPath("/login"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            bool dungV2 = LaGiaoDienKbg20;

            // Chặn hẳn UpdatePanel V1 trên server: không render và không tải dữ liệu.
            // Controls vẫn tồn tại trong control tree để bảo toàn WebForms/designer V1.
            upnlMain.Visible = !dungV2;
            btnTaskAction.Visible = !dungV2;
            btnRefresh.Visible = !dungV2;

            if (dungV2)
            {
                AspnetUser employee = UserManager.Instance.GetUserById(userId);
                pnlNotEmployee.Visible = employee == null || !employee.LaNhanVien;
            }

            // Tieu chuan TaskList: gan project tabs tren moi request, breadcrumb chi lan dau.
            CauHinhDieuHuongDuAn(!IsPostBack);

            // Giống TaskList: nút workflow dùng AJAX, không reload toàn bộ trang.
            // Nút ở cpMain, popup ở cpModalMain: đăng ký bằng ScriptManager để
            // tránh trigger cross naming-container trong markup.
            if (dungV2 && !pnlNotEmployee.Visible)
            {
                var workflowTrigger = TimControl<LinkButton>(Page, "btnKbg20Workflow");
                var sm = ScriptManager.GetCurrent(Page);
                if (workflowTrigger != null && sm != null)
                    sm.RegisterAsyncPostBackControl(workflowTrigger);
            }

            // Chỉ hook các Ctrl Task khi đang thực sự dùng UI V2.
            if (dungV2 && Kbg20TichHopTask.DuocKichHoat && !pnlNotEmployee.Visible)
                CaiDatKbg20TaskHooks(userId);

            if (IsPostBack) return;

            // V2 lấy dữ liệu bằng KhaiBaoGio20.ashx; KHÔNG gọi GetTodayView/BindAll của V1.
            if (dungV2) return;

            InitHeader(userId);
            BindAll(null, null);
        }

        /// <summary>
        /// Cau hinh Navigation / CtrlProjectTabs o Page_Load, nhu TaskList.
        /// Tim control qua cpMain de khong phu thuoc file .aspx.designer.cs
        /// (ban giao 2D khong kem designer cua solution nguoi dung).
        /// </summary>
        private void CauHinhDieuHuongDuAn(bool lanDau)
        {
            if (CurrentProjectId == Guid.Empty) return;

            Control content = Master != null ? Master.FindControl("cpMain") : null;
            if (content == null)
                throw new InvalidOperationException("Khong tim thay ContentPlaceHolder cpMain cua MasterTemplate.");

            var projectTabs = content.FindControl("ProjectTabsKbg")
                as SweetSoft.QLDA.BackOffice.fProjects.Controls.CtrlProjectTabs;
            if (projectTabs == null)
                throw new InvalidOperationException("Khong tim thay ProjectTabsKbg trong KhaiBaoGio.aspx.");

            // CtrlProjectTabs.BindTabs() chay o OnPreRender, can duoc gan tren moi postback.
            projectTabs.ProjectId = CurrentProjectId;

            if (!lanDau) return;

            var navigation = content.FindControl("NavigationKbg")
                as SweetSoft.QLDA.BackOffice.Controls.Breadcrumb.CtrlBreadcrumb;
            if (navigation == null)
                throw new InvalidOperationException("Khong tim thay NavigationKbg trong KhaiBaoGio.aspx.");

            // Theo cung cau truc TaskList: Danh sach du an -> Chuc nang hien tai.
            navigation.MainTitle = "Tính giờ công việc";
            navigation.keyValuePairUrls = new Dictionary<string, string>
            {
                { GetRelativeClientPath(RewriteURLHelper.Projects), GetResourceText(BackEndResourceKeys.PROJECT_LIST) },
                { "javascript:;", "Tính giờ công việc" }
            };
        }

        /// <summary>
        /// Tao token cho lenh POST cua KhaiBaoGio20.ashx.
        /// Phai khop CheckToken: MachineKey / purpose / user id / ngay UTC.
        /// Khong phai token V1 va khong thay doi cach xac thuc cua API.
        /// </summary>
        protected string Kbg20RequestToken()
        {
            AspnetUser user = SweetContext.Current != null ? SweetContext.Current.User : null;
            if (user == null || user.UserId == Guid.Empty)
                throw new UnauthorizedAccessException("Khong xac dinh duoc nhan vien dang nhap de tao token tinh gio.");

            string payload = user.UserId.ToString("N") + "|"
                           + DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            byte[] encrypted = MachineKey.Protect(
                System.Text.Encoding.UTF8.GetBytes(payload), "KBG20", "WebActionV1");
            if (encrypted == null || encrypted.Length == 0)
                throw new InvalidOperationException("Khong tao duoc token bao ve thao tac tinh gio.");

            return Convert.ToBase64String(encrypted);
        }

        private void InitHeader(Guid userId)
        {
            lnkDashboard.HRef = GetRelativeClientPath(RewriteURLHelper.EmployeeDashboard);

            // Chỉ hiện nút sang trang Thống kê tổng quan nếu tài khoản là Admin hoặc được cấp quyền xem Dashboard
            bool canViewDashboard = UserManager.Instance.IsAdministrator(userId)
                                 || this.IsUserRight(ActionKeys.View, ModuleKeys.EmployeeDashboard);
            lnkDashboard.Visible = canViewDashboard;

            DateTime now = DateTime.Now;
            ltrDateText.Text = HttpUtility.HtmlEncode(
                Weekdays[(int)now.DayOfWeek] + ", " + now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));

            string name = string.Empty;
            AspnetUser user = UserManager.Instance.GetUserById(userId);
            if (user != null)
            {
                name = !string.IsNullOrWhiteSpace(user.DisplayName) ? user.DisplayName : user.UserName;
            }

            ltrName.Text = HttpUtility.HtmlEncode(name);
            ltrInitial.Text = HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(name)
                ? "?" : name.Trim().Substring(name.Trim().LastIndexOf(' ') + 1, 1).ToUpperInvariant());
        }

        private bool TryGetCurrentUserId(out Guid userId)
        {
            userId = Guid.Empty;

            // Mỗi request chỉ tra cứu 1 lần (trước đây gọi 2-3 lần, mỗi lần là 1 câu UPDATE LastActivityDate).
            object cached = Context.Items["ks_uid"];
            if (cached is Guid)
            {
                userId = (Guid)cached;
                return userId != Guid.Empty;
            }

            try
            {
                MembershipUser mu = Membership.GetUser();
                if (mu != null && mu.ProviderUserKey is Guid)
                {
                    userId = (Guid)mu.ProviderUserKey;
                    Context.Items["ks_uid"] = userId;
                    return true;
                }
            }
            catch (Exception)
            {
            }

            try
            {
                if (SweetContext.Current != null && SweetContext.Current.UserId != Guid.Empty)
                {
                    userId = SweetContext.Current.UserId;
                    Context.Items["ks_uid"] = userId;
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        #endregion

        #region Dựng dữ liệu giao diện

        private void BindAll(string notice, string noticeKind)
        {
            Guid userId;
            if (!TryGetCurrentUserId(out userId))
            {
                SetStatus("error", "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
                return;
            }

            AspnetUser user = UserManager.Instance.GetUserById(userId);
            if (user == null || !user.LaNhanVien)
            {
                pnlNotEmployee.Visible = true;
                upnlMain.Visible = false;
                return;
            }

            DateTime now = DateTime.Now;

            // Việc chốt sổ cuối ngày do job Hangfire "KhaiBaoGio-Chot-So" (KhaiBaoGioCloseJob) đảm nhiệm,
            // trang không gọi nữa để request của người dùng không phải gánh các câu UPDATE/INSERT hàng loạt.

            KhaiBaoGioView view = KhaiBaoGioManager.Instance.GetTodayView(userId, now);
            RenderView(view, notice, noticeKind);
        }

        private void RenderView(KhaiBaoGioView view, string notice, string noticeKind)
        {
            CaView dayShift = view.DayShift;
            CaView open = view.OpenShift;

            pnlDayOff.Visible = !view.IsWorkingDay;
            pnlShifts.Visible = view.IsWorkingDay;
            pnlTasks.Visible = view.IsWorkingDay;

            if (!view.IsWorkingDay)
            {
                string text = "<b>Hôm nay là ngày nghỉ</b><br />" + HttpUtility.HtmlEncode(view.DayOffReason);
                if (view.NextWorkingDay.HasValue)
                {
                    DateTime next = view.NextWorkingDay.Value;
                    text += "<br />Ngày làm việc tiếp theo: " + HttpUtility.HtmlEncode(
                        Weekdays[(int)next.DayOfWeek] + ", " + next.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                }
                ltrDayOffText.Text = text;
                pnlFooter.Visible = false;
                RegisterClientConfig(view);
                return;
            }

            rptShifts.DataSource = view.Shifts;
            rptShifts.DataBind();
            rptTasks.DataSource = view.Tasks;
            rptTasks.DataBind();
            pnlNoTasks.Visible = view.Tasks.Count == 0;
            pnlFooter.Visible = true;

            if (open != null)
            {
                ltrTasksTitle.Text = "Danh sách công việc hôm nay";
                ltrTasksSub.Text = view.RunningTaskId.HasValue
                    ? "Đang bấm giờ 1 công việc. Nhấn Dừng nếu bạn muốn tạm nghỉ hoặc chuyển sang công việc khác."
                    : "Nhấn Bắt đầu (hoặc Tiếp tục) ở công việc bạn đang làm để hệ thống tính giờ thực tế.";
            }
            else
            {
                ltrTasksTitle.Text = "Danh sách công việc hôm nay";
                ltrTasksSub.Text = HttpUtility.HtmlEncode(DayStatusText(dayShift));
            }

            if (!string.IsNullOrEmpty(notice))
            {
                SetStatus(string.IsNullOrEmpty(noticeKind) ? "error" : noticeKind, notice);
            }
            else if (open != null)
            {
                if (view.RunningTaskId.HasValue)
                {
                    TaskRowView runningTask = view.Tasks.FirstOrDefault(t => t.IdCongViec == view.RunningTaskId.Value);
                    string code = runningTask != null ? runningTask.MaCongViec : "";
                    SetStatus("pending", "Đồng hồ đang chạy cho công việc [" + code + "]…");
                }
                else if (open.LuuLanCuoiLuc.HasValue)
                {
                    SetStatus("ok", "Cập nhật lần cuối lúc " + open.LuuLanCuoiLuc.Value.ToString("HH:mm:ss", CultureInfo.InvariantCulture)
                                  + " · Tổng hôm nay: " + KhaiBaoGioManager.FormatDecimalHours(open.TongPhut));
                }
                else
                {
                    SetStatus(string.Empty, "Chưa có công việc nào đang chạy. Nhấn Bắt đầu để tính giờ.");
                }
            }
            else
            {
                SetStatus(string.Empty, DayStatusText(dayShift));
            }

            RegisterClientConfig(view);
        }

        private static string DayStatusText(CaView dayShift)
        {
            if (dayShift == null) return "Hôm nay không có ca làm việc.";
            if (dayShift.State == TrangThaiCaView.ChuaMo)
                return "Chưa đến giờ làm việc hôm nay (mở lúc " + dayShift.GioBatDau + ").";
            return "Ngày làm việc hôm nay đã kết thúc lúc " + dayShift.GioKetThuc
                 + " · Tổng thời gian ghi nhận: " + KhaiBaoGioManager.FormatDecimalHours(dayShift.TongPhut) + ".";
        }

        private void SetStatus(string kind, string text)
        {
            lblSaveStatus.Text = HttpUtility.HtmlEncode(text);
            lblSaveStatus.CssClass = "ks-save-status" + (string.IsNullOrEmpty(kind) ? string.Empty : " is-" + kind);
        }

        /// <summary>Gửi cấu hình thời gian và các khung ca xuống JavaScript để đồng hồ chạy mượt từng giây và tự trừ nghỉ trưa.</summary>
        private void RegisterClientConfig(KhaiBaoGioView view)
        {
            DateTime now = view.ServerNow;
            CaView dayShift = view.DayShift;
            List<ThongTinCa> rawShifts = LichBieuChungManager.Instance.GetShiftsForDate(view.Today);

            List<object> segments = new List<object>();
            List<long> boundaries = new List<long>();

            if (rawShifts != null)
            {
                foreach (ThongTinCa s in rawShifts.OrderBy(x => x.BatDau))
                {
                    segments.Add(new { start = ToMs(s.BatDau), end = ToMs(s.KetThuc) });
                    if (s.BatDau > now) boundaries.Add(ToMs(s.BatDau));
                    if (s.KetThuc > now) boundaries.Add(ToMs(s.KetThuc));
                }
            }

            var cfg = new
            {
                serverNow = ToMs(now),
                isDayOpen = view.OpenShift != null,
                hasRunningTask = view.RunningTaskId.HasValue,
                capacityMin = dayShift != null ? dayShift.SoPhutCa : 0,
                capacitySec = dayShift != null ? dayShift.SoPhutCa * 60 : 0,
                lunchStart = (dayShift != null && dayShift.NghiTruaBatDau.HasValue) ? ToMs(dayShift.NghiTruaBatDau.Value) : 0L,
                lunchEnd = (dayShift != null && dayShift.NghiTruaKetThuc.HasValue) ? ToMs(dayShift.NghiTruaKetThuc.Value) : 0L,
                segments = segments,
                boundaries = boundaries.Distinct().OrderBy(x => x).ToList(),
                midnight = ToMs(view.Today.AddDays(1)),
                journal = view.JournalSessions.Select(x => new
                {
                    taskId = x.IdCongViec.ToString("D"),
                    start = ToMs(x.BatDauLuc),
                    end = x.KetThucLuc.HasValue ? (long?)ToMs(x.KetThucLuc.Value) : null,
                    seconds = x.SoGiayThucLam,
                    reason = x.LoaiKetThuc
                }).ToList(),
                actionBtnId = btnTaskAction.UniqueID,
                refreshId = btnRefresh.UniqueID
            };

            string json = new JavaScriptSerializer().Serialize(cfg);
            string script = "(function w(){ if (window.KhaiBaoGioJs) { KhaiBaoGioJs.init(" + json + "); } else { setTimeout(w, 50); } })();";
            ScriptManager.RegisterStartupScript(upnlMain, GetType(), "ksInit", script, true);
        }

        protected static long ToMs(DateTime local)
        {
            DateTime utc = DateTime.SpecifyKind(local, DateTimeKind.Local).ToUniversalTime();
            return (long)(utc - Epoch).TotalMilliseconds;
        }

        #endregion

        #region Sự kiện bấm nút Bắt đầu / Tiếp tục / Dừng & Làm mới

        protected void btnTaskAction_Click(object sender, EventArgs e)
        {
            // Không cho POST V1 Start/Stop khi dự án đang chạy V2.
            if (LaGiaoDienKbg20) return;

            // Đọc dữ liệu trước khi xóa để tránh giữ lại yêu cầu cũ
            // trong trạng thái xử lý phía server.
            string rawTaskId = hdfActionTaskId.Value;
            string action = (hdfActionType.Value ?? string.Empty)
                .Trim()
                .ToLowerInvariant();

            hdfActionTaskId.Value = string.Empty;
            hdfActionType.Value = string.Empty;

            Guid userId;

            if (!TryGetCurrentUserId(out userId))
            {
                SetStatus(
                    "error",
                    "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
                );

                upnlMain.Update();
                return;
            }

            Guid taskId = Guid.Empty;

            try
            {
                if (!Guid.TryParse(rawTaskId, out taskId) ||
                    taskId == Guid.Empty)
                {
                    RefreshAll(
                        "Không xác định được công việc cần thao tác.",
                        "error"
                    );

                    return;
                }

                DateTime now = DateTime.Now;

                // Start/Stop tự kiểm tra giờ làm việc; việc chốt sổ hàng loạt do BindAll gọi (có giới hạn 1 lần/phút).
                // Không gọi CloseExpiredShifts ở đây nữa vì mỗi lần bấm sẽ chạy lại các câu UPDATE/INSERT toàn hệ thống.

                SaveResult result;

                if (action == "start" || action == "resume")
                {
                    result = KhaiBaoGioManager.Instance
                        .StartOrResumeTask(userId, taskId, now);
                }
                else if (action == "stop")
                {
                    result = KhaiBaoGioManager.Instance
                        .StopTask(userId, taskId, now);
                }
                else
                {
                    result = SaveResult.Fail("Thao tác không hợp lệ.");
                }

                RefreshAll(
                    result.Message,
                    result.Success ? "ok" : "error"
                );

                // Thông báo các tab: đồng hồ nổi phải cập nhật ngay khi Start/Stop thành công.
                // Đăng ký script vào UpdatePanel để chạy sau partial postback WebForms.
                if (result.Success)
                {
                    ScriptManager.RegisterStartupScript(upnlMain, GetType(), "kbgFloatingChanged",
                        "if(window.KbgFloatingTimer){window.KbgFloatingTimer.notifyChanged();}", true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(
    "KhaiBaoGio.btnTaskAction_Click failed. " +
    "UserId=" + userId +
    "; TaskId=" + taskId +
    "; Action=" + action +
    "; Exception=" + ex
);
                // Để lỗi được chuyển về PageRequestManager.
                // JavaScript sẽ thông báo và tải lại trang để đồng bộ dữ liệu.
                throw;
            }
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            if (LaGiaoDienKbg20) return;
            try
            {
                // Chốt sổ do job Hangfire đảm nhiệm; làm mới chỉ đọc lại dữ liệu.
                RefreshAll(null, null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(
    "KhaiBaoGio.btnRefresh_Click failed. Exception=" + ex
);

                throw;
            }
        }

        private void RefreshAll(string notice, string noticeKind)
        {
            BindAll(notice, noticeKind);
            upnlMain.Update();
        }

        #endregion

        #region Repeater & Helpers hiển thị HTML

        protected void rptTasks_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            TaskRowView t = (TaskRowView)e.Item.DataItem;

            PlaceHolder phActionBtn = (PlaceHolder)e.Item.FindControl("phActionBtn");
            PlaceHolder phReadOnly = (PlaceHolder)e.Item.FindControl("phReadOnly");

            bool canInteract = t.Editable && t.TrangThaiDongHo != TrangThaiDongHoTask.DaKhoa;
            phActionBtn.Visible = canInteract;
            phReadOnly.Visible = !canInteract;

            PlaceHolder phGroup = (PlaceHolder)e.Item.FindControl("phGroup");
            phGroup.Visible = t.ShowGroupHeader;
            if (t.ShowGroupHeader)
            {
                Literal ltr = (Literal)e.Item.FindControl("ltrGroup");
                ltr.Text = HttpUtility.HtmlEncode(t.GroupTitle + " (" + t.GroupCount + ")");
            }
        }

        protected string RenderTaskActionButton(TaskRowView t)
        {
            string id = t.IdCongViec.ToString("D");

            if (t.TrangThaiDongHo == TrangThaiDongHoTask.DangChay)
            {
                return "<button type=\"button\" class=\"ks-action-btn btn-stop\" data-task-id=\"" + id + "\" data-action=\"stop\">"
                     + "<span class=\"ks-pulse-dot\"></span>Dừng</button>";
            }

            if (t.TrangThaiDongHo == TrangThaiDongHoTask.TamDung)
            {
                string disabled = t.AllowStartOrResume ? "" : " disabled=\"disabled\" title=\"Đang có công việc khác đang chạy\"";
                return "<button type=\"button\" class=\"ks-action-btn btn-resume\" data-task-id=\"" + id + "\" data-action=\"resume\"" + disabled + ">"
                     + "<i class=\"fas fa-play\"></i>Tiếp tục</button>";
            }

            string disStart = t.AllowStartOrResume ? "" : " disabled=\"disabled\" title=\"Đang có công việc khác đang chạy\"";
            return "<button type=\"button\" class=\"ks-action-btn btn-start\" data-task-id=\"" + id + "\" data-action=\"start\"" + disStart + ">"
                 + "<i class=\"fas fa-play\"></i>Bắt đầu</button>";
        }

        protected string ShiftCss(CaView c)
        {
            switch (c.State)
            {
                case TrangThaiCaView.DangMo: return "is-open";
                case TrangThaiCaView.DaChotCoKhaiBao: return "is-done";
                case TrangThaiCaView.DaChotKhongKhaiBao: return "is-missed";
                default: return "is-pending";
            }
        }

        protected string ShiftChipText(CaView c)
        {
            switch (c.State)
            {
                case TrangThaiCaView.DangMo: return "Đang trong giờ làm việc";
                case TrangThaiCaView.DaChotCoKhaiBao: return "Đã chốt sổ cuối ngày";
                case TrangThaiCaView.DaChotKhongKhaiBao: return "Không ghi nhận giờ làm";
                case TrangThaiCaView.DaChotKhongCoTask: return "Không có công việc";
                default: return "Chưa đến giờ làm việc";
            }
        }

        protected string ShiftLunchHtml(CaView c)
        {
            if (!string.IsNullOrWhiteSpace(c.GioNghiTrua))
            {
                return "<i class=\"fas fa-mug-hot me-1\"></i>Nghỉ giữa giờ: <b>"
                     + HttpUtility.HtmlEncode(c.GioNghiTrua)
                     + "</b> (hệ thống tự động trừ khi tính công) · Định mức ngày: <b>"
                     + KhaiBaoGioManager.FormatReadableMinutes(c.SoPhutCa) + "</b>";
            }
            return "Làm việc 1 ca trong ngày · Định mức: <b>" + KhaiBaoGioManager.FormatReadableMinutes(c.SoPhutCa) + "</b>";
        }

        protected string ShiftMetaHtml(CaView c)
        {
            switch (c.State)
            {
                case TrangThaiCaView.ChuaMo:
                    return "Mở sau <b class=\"ks-count\" data-target=\"" + ToMs(c.BatDau) + "\">--:--:--</b>";
                case TrangThaiCaView.DangMo:
                    return "Kết thúc giờ làm việc sau <b class=\"ks-count\" data-target=\"" + ToMs(c.KetThuc) + "\">--:--:--</b>";
                case TrangThaiCaView.DaChotCoKhaiBao:
                    return "Đã hoàn tất ngày làm việc";
                case TrangThaiCaView.DaChotKhongKhaiBao:
                    return "Hết ngày làm việc nhưng không bấm giờ";
                default:
                    return "Không có công việc cần bấm giờ";
            }
        }

        protected string ShiftPercent(CaView c)
        {
            int capSec = c.SoPhutCa * 60;
            if (capSec <= 0) return "0";
            int pct = (int)Math.Min(100, c.TongGiayHienTai * 100L / capSec);
            return pct.ToString(CultureInfo.InvariantCulture);
        }

        protected string TaskCss(TaskRowView t)
        {
            List<string> cls = new List<string>();
            if (t.Nhom == NhomTaskView.QuaHan) cls.Add("is-overdue");
            if (t.Nhom == NhomTaskView.DaKhaiKhongConHoatDong) cls.Add("is-extra");
            if (t.TrangThaiDongHo == TrangThaiDongHoTask.DangChay) cls.Add("is-running");
            return string.Join(" ", cls.ToArray());
        }

        protected string TaskTodaySummaryHtml(TaskRowView t)
        {
            if (t.TrangThaiDongHo == TrangThaiDongHoTask.DangChay)
            {
                return "Đang chạy · " + KhaiBaoGioManager.FormatReadableMinutes(t.Phut);
            }
            if (t.TongGiayHienTai > 0 || t.Phut > 0)
            {
                return "Hôm nay: " + KhaiBaoGioManager.FormatReadableMinutes(t.Phut);
            }
            return "Hôm nay: 0 phút";
        }

        protected string TaskMetaHtml(TaskRowView t)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // Trạng thái tiến độ công việc là thông tin riêng, KHÔNG thay đổi khi bấm đồng hồ.
            if (t.IsOverdue)
                sb.Append("<span class=\"ks-badge is-overdue\">Quá hạn · trễ " + t.TreNgayLamViec + " ngày</span>");
            if (t.TrangThai == 0)
                sb.Append("<span class=\"ks-badge is-todo\" title=\"Trạng thái nghiệp vụ công việc\">CV: Chưa bắt đầu</span>");
            else
                sb.Append("<span class=\"ks-badge\" title=\"Trạng thái nghiệp vụ công việc\">CV: Đang thực hiện</span>");
            if (t.TrangThaiDongHo == TrangThaiDongHoTask.DangChay)
                sb.Append("<span class=\"ks-badge is-clock-running\">Đồng hồ: Đang chạy</span>");
            else if (t.TrangThaiDongHo == TrangThaiDongHoTask.TamDung)
                sb.Append("<span class=\"ks-badge is-clock-paused\">Đồng hồ: Đã dừng</span>");

            if (t.NgayBatDau.HasValue || t.NgayKetThuc.HasValue)
            {
                string from = t.NgayBatDau.HasValue ? t.NgayBatDau.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "?";
                string to = t.NgayKetThuc.HasValue ? t.NgayKetThuc.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "?";
                sb.Append("<span><i class=\"far fa-calendar\"></i> " + from + " – " + to + "</span>");
            }

            if (!t.IsOverdue && t.TongNgayLamViec > 0)
            {
                bool dueToday = t.NgayKetThuc.HasValue && t.NgayKetThuc.Value.Date == DateTime.Today;
                sb.Append(dueToday
                    ? "<span><b>Hạn hôm nay</b></span>"
                    : "<span>Ngày " + t.NgayThuMay + "/" + t.TongNgayLamViec + "</span>");
            }

            if (!string.IsNullOrWhiteSpace(t.TenDoUuTien))
                sb.Append("<span>Ưu tiên: " + HttpUtility.HtmlEncode(t.TenDoUuTien) + "</span>");

            if (t.SoLanBamHomNay > 0)
                sb.Append("<span><i class=\"fas fa-stopwatch\"></i> " + t.SoLanBamHomNay + " lần bấm hôm nay</span>");

            return sb.ToString();
        }

        protected string TaskBlockReasonHtml(TaskRowView t)
        {
            if (!t.Editable || t.TrangThaiDongHo == TrangThaiDongHoTask.DangChay || t.AllowStartOrResume)
                return string.Empty;
            return "<div class=\"ks-blocked-reason\">Dừng công việc đang chạy trước khi chuyển sang công việc này.</div>";
        }

        protected string TaskRecordHintHtml(TaskRowView t)
        {
            if (t.TongGiayHienTai <= 0 && t.Phut <= 0)
                return t.SoLanBamHomNay > 0
                    ? "<span class=\"ks-hint-label\">Đã bấm giờ nhưng chưa phát sinh thời gian được tính</span>"
                    : "<span class=\"ks-hint-label\">Chưa ghi nhận thời gian hôm nay</span>";
            return "Ghi nhận hôm nay: " + (t.IsOverdue ? "Trễ hạn" : "Đúng hạn");
        }

        protected string TaskReadOnlyHtml(TaskRowView t)
        {
            if (t.Nhom == NhomTaskView.DaKhaiKhongConHoatDong)
                return "Công việc không còn hoạt động; giờ đã ghi nhận được giữ nguyên.";
            return "Đã khóa bấm giờ (ngoài khung giờ làm việc hôm nay).";
        }

        protected string TaskHistoryHtml(TaskRowView t)
        {
            int total = t.LichSuDungHanPhut + t.LichSuTrePhut;
            if (total <= 0) return string.Empty;
            return HttpUtility.HtmlEncode("Lịch sử các ngày trước: " + KhaiBaoGioManager.FormatReadableMinutes(total)
                   + " (đúng hạn " + KhaiBaoGioManager.FormatMinutes(t.LichSuDungHanPhut)
                   + " · trễ " + KhaiBaoGioManager.FormatMinutes(t.LichSuTrePhut) + ")");
        }

        #endregion
    
        // KBG20 2D - KHONG viet lai handler cua partner; chi host cac Ctrl goc.
        private static T TimControl<T>(Control root,string id) where T : Control
        {
            if(root==null)return null;
            if(root.ID==id)return root as T;
            foreach(Control child in root.Controls)
            {
                var hit=TimControl<T>(child,id);
                if(hit!=null)return hit;
            }
            return null;
        }
        private bool LaPmDuAn(Guid uid)
        {
            Guid? pm=DuAnManager.Instance.LayIdNhanVienQuanLy(CurrentProjectId);
            return pm.HasValue && pm.Value==uid;
        }
        private static Guid NhanVienDangNhap()
        {
            var u=SweetContext.Current==null?null:SweetContext.Current.User;
            return u==null?Guid.Empty:u.UserId;
        }
        private void CaiDatKbg20TaskHooks(Guid uid)
        {
            var start=TimControl<CtrlStartTask>(Page,"KbgCtrlStartTask");
            var complete=TimControl<CtrlFastCompleteTask>(Page,"KbgCtrlFastCompleteTask");
            var review=TimControl<CtrlFastReviewTask>(Page,"KbgCtrlFastReviewTask");
            if(start!=null)
            {
                start.IdDuAnNguonKbg20=CurrentProjectId;
                start.LuuBatDauQuaKbg20=(task,luuTask) =>
                {
                    XacThucKbg20Workflow(task,uid,false);
                    bool overtime=string.Equals(Convert.ToString(ViewState["Kbg20FlowType"]),"overtime",StringComparison.OrdinalIgnoreCase);
                    bool overdue=ViewState["Kbg20FlowOverdue"] is bool && (bool)ViewState["Kbg20FlowOverdue"];
                    Kbg20TichHopTask.StartTaskVaGio(CurrentProjectId,task,uid,Guid.NewGuid(),
                        overdue,true,overtime,luuTask);
                };
                start.ActionStarted+=(sender,args)=>DangKyRefeshKbg20();
            }
            if(complete!=null)
            {
                complete.IdDuAnNguonKbg20=CurrentProjectId;
                complete.LaPmNguonKbg20=LaPmDuAn(uid);
                // Ban than CtrlFastCompleteTask goi Kbg20TichHopTask.LuuHoanThanh neu co flag.
                complete.ActionCompleted+=(sender,args)=>DangKyRefeshKbg20();
            }
            if(review!=null)
            {
                review.IsPM=LaPmDuAn(uid);
                review.ActionCompleted+=(sender,args)=>DangKyRefeshKbg20();
            }
        }
        private void DangKyRefeshKbg20()
        {
            // Ctrl Task has called ExtraModal.CloseModal() before raising the event.
            // Do NOT reload the page: it interrupts the close animation and resets KBG20 UI.
            ScriptManager.RegisterStartupScript(Page,Page.GetType(),"Kbg20WorkflowRefresh_"+Guid.NewGuid().ToString("N"),
                "window.setTimeout(function(){if(typeof window.Kbg20RefreshAfterWorkflow==='function')" +
                "window.Kbg20RefreshAfterWorkflow();},260);",true);
        }
        private TblCongViec XacThucKbg20Workflow(Guid taskId,Guid userId,bool laPm)
        {
            if(!Kbg20TichHopTask.DuocKichHoat || taskId==Guid.Empty || userId==Guid.Empty || CurrentProjectId==Guid.Empty)
                throw new UnauthorizedAccessException("Khong duoc phep thao tac.");
            var task=TaskManager.Instance.FetchById(taskId);
            if(task==null || task.IdDuAn!=CurrentProjectId || task.DaXoa==true)
                throw new UnauthorizedAccessException("Task khong thuoc du an hoac da xoa.");
            if(!laPm && !TaskManager.Instance.GetAssignedNhanVienIds(taskId).Contains(userId))
                throw new UnauthorizedAccessException("Ban khong duoc phan cong truc tiep vao task nay.");
            return task;
        }
        private static List<StartTaskDependencyWarning> CanhBaoPhuThuocTuTaskList(TblCongViec task)
        {
            var list=new List<StartTaskDependencyWarning>();
            if(task.IdCongViecPhuThuoc.HasValue)
            {
                var dep=TaskManager.Instance.FetchById(task.IdCongViecPhuThuoc.Value);
                if(dep!=null && dep.TrangThai!=2 && dep.TrangThai!=3)
                    list.Add(new StartTaskDependencyWarning{DependencyTask=dep,ParentTask=null,TargetTask=task});
            }
            Guid? id=task.IdCongViecCha;
            var seen=new HashSet<Guid>();
            while(id.HasValue && seen.Add(id.Value))
            {
                var parent=TaskManager.Instance.FetchById(id.Value);
                if(parent==null)break;
                if(parent.IdCongViecPhuThuoc.HasValue)
                {
                    var dep=TaskManager.Instance.FetchById(parent.IdCongViecPhuThuoc.Value);
                    if(dep!=null && dep.TrangThai!=2 && dep.TrangThai!=3)
                        list.Add(new StartTaskDependencyWarning{DependencyTask=dep,ParentTask=parent,TargetTask=task});
                }
                id=parent.IdCongViecCha;
            }
            return list;
        }
        protected void btnKbg20Workflow_Click(object sender,EventArgs e)
        {
            try
            {
                var uid=NhanVienDangNhap();
                Guid id;
                if(!Guid.TryParse(Request.Form["kbg20FlowTask"],out id))throw new InvalidOperationException("Ma cong viec khong hop le.");
                string action=Request.Form["kbg20FlowAction"];
                bool pm=LaPmDuAn(uid);
                // PM duoc xac nhan hoan thanh truc tiep du khong duoc phan cong.
                // Hanh dong start va gui duyet cua nhan vien van doi hoi phan cong.
                var task=XacThucKbg20Workflow(id,uid,pm && (action=="review" || action=="complete"));
                if(action=="start")
                {
                    if(task.TrangThai!=0)throw new InvalidOperationException("Task khong con o trang thai Chua bat dau.");
                    if(TaskManager.Instance.CheckHasChildTasks(CurrentProjectId,task))
                        throw new InvalidOperationException("Task cha co con, nut Start tu TaskList khong ap dung truc tiep.");
                    var ctrl=TimControl<CtrlStartTask>(Page,"KbgCtrlStartTask");
                    if(ctrl==null)throw new InvalidOperationException("Khong tim thay CtrlStartTask.");
                    ViewState["Kbg20FlowType"]=Request.Form["kbg20FlowType"];
                    ViewState["Kbg20FlowOverdue"]=string.Equals(Request.Form["kbg20FlowOverdue"],"1",StringComparison.Ordinal);
                    ctrl.OpenModal(id,task.MaCongViec,task.TenCongViec,CanhBaoPhuThuocTuTaskList(task));
                    CapNhatPanelPopupPartner();
                }
                else if(action=="complete")
                {
                    if(task.TrangThai!=1)throw new InvalidOperationException("Chi gui hoan thanh task dang thuc hien.");
                    var ctrl=TimControl<CtrlFastCompleteTask>(Page,"KbgCtrlFastCompleteTask");
                    if(ctrl==null)throw new InvalidOperationException("Khong tim thay CtrlFastCompleteTask.");
                    ctrl.OpenModal(id,task.MaCongViec,task.TenCongViec);
                    CapNhatPanelPopupPartner();
                }
                else if(action=="review")
                {
                    if(!pm)throw new UnauthorizedAccessException("Chi PM cua du an moi co quyen duyet.");
                    var ctrl=TimControl<CtrlFastReviewTask>(Page,"KbgCtrlFastReviewTask");
                    if(ctrl==null)throw new InvalidOperationException("Khong tim thay CtrlFastReviewTask.");
                    ctrl.OpenModal(CurrentProjectId,id,task.MaCongViec,task.TenCongViec);
                    CapNhatPanelPopupPartner();
                }
                else throw new InvalidOperationException("Thao tac chua duoc ho tro.");
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.TraceError("KBG20 workflow: khong the mo popup Task partner: {0}", ex);
                ShowNotify(ex.Message, SweetSoft.QLDA.BackOffice.Common.MSGType.Warning);
                // Không để nút chỉ reload mà im lặng nếu ShowNotify không hiện trên AJAX.
                ScriptManager.RegisterStartupScript(Page, GetType(),
                    "Kbg20WorkflowError_" + Guid.NewGuid().ToString("N"),
                    "(function(){var m=" + new JavaScriptSerializer().Serialize(ex.Message).Replace("<", @"\u003c") +
                    ";var el=document.getElementById('kbg20Message');" +
                    "if(el){el.textContent=m;el.style.display='block';}" +
                    "else{window.alert(m);}})();", true);
            }
        }

        // Popup partner dat o cpModalMain, khac naming container voi nut mo.
        // Chu dong cap nhat host panel de HTML/hidden fields va script OpenModal
        // duoc gui ve trong cung mot AJAX response.
        private void CapNhatPanelPopupPartner()
        {
            var panel = TimControl<UpdatePanel>(Page, "upnlKbg20PartnerModals");
            if (panel == null)
                throw new InvalidOperationException("Khong tim thay panel chua cac Ctrl Task trong cpModalMain.");
            panel.Update();
        }
    }
}
