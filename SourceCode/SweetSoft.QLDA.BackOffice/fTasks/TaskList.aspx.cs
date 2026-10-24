using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fTasks.Controls;
using SweetSoft.QLDA.BackOffice.fUsers.Controls;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fTasks
{
    public partial class TaskList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.Task;
        private static Dictionary<Guid, TblDoUuTien> _dictPriorities = new Dictionary<Guid, TblDoUuTien>();
        protected readonly ControlHelpers _controlHelpers = new ControlHelpers();

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlProjectTabs1.ProjectId = CurrentProjectId;
            CtrlTask1.EditTaskHandlerCallback = EditTask_Callback;
            CtrlTask1.ReminderHandlerCallback = Reminder_Callback;
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                if (CurrentProjectId == Guid.Empty)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Projects), true);
                    return;
                }
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.TASK_LIST));
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.TASK_LIST);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>
                {
                    { GetRelativeClientPath(RewriteURLHelper.Projects), GetResourceText(BackEndResourceKeys.PROJECT_LIST) },
                    { "javascript:;", GetResourceText(BackEndResourceKeys.TASK_LIST) }
                };
                if (_dictPriorities.Count == 0)
                    _dictPriorities = TaskManager.Instance.GetDictPriorities();

                Guid taskId;
                if (Guid.TryParse(Request.QueryString["taskId"], out taskId))
                {
                    TblCongViec task = TaskManager.Instance.FetchById(taskId);
                    if (task != null
                        && task.DaXoa != true
                        && task.IdDuAn == CurrentProjectId)
                    {
                        EditTask_Callback(taskId, EventArgs.Empty);
                    }
                }
            }
        }

        #region Logic hiển thị Popup Edit
        private void EditTask_Callback(object sender, EventArgs e)
        {
            Guid taskId = (Guid)sender;
            TblCongViec task = TaskManager.Instance.FetchById(taskId);
            if (task == null || task.DaXoa == true) return;

            hfEditTaskId.Value = task.IdCongViec.ToString();
            // Lịch dự kiến chỉ thuộc công việc đang mở; không tái sử dụng JSON của task trước.
            hdfLocalScheduleJson.Value = string.Empty;
            bool isPhase = TaskManager.Instance.CheckPhase(task);
            bool hasChildren = TaskManager.Instance.CheckHasChildTasks(CurrentProjectId, task);

            mdlEditTask.Title = isPhase ? "Cập nhật Giai đoạn" : "Cập nhật Công việc con";

            txtEditMaCv.Text = task.MaCongViec;
            txtEditTenCv.Text = task.TenCongViec;
            txtEditGiaiDoan.Text = TaskManager.Instance.GetRootPhaseName(CurrentProjectId, task.IdCongViecCha);
            txtEditMoTa.Text = task.MoTa;
            txtEditThoiHan.Text = task.ThoiHanNgay.HasValue ? task.ThoiHanNgay.ToString() : "";
            txtEditNgayBatDau.Text = task.NgayBatDau.HasValue ? task.NgayBatDau.Value.ToString("yyyy-MM-dd") : "";
            txtEditNgayKetThuc.Text = task.NgayKetThuc.HasValue ? task.NgayKetThuc.Value.ToString("yyyy-MM-dd") : "";

            lblEditMaCv.Text = task.MaCongViec;
            lblEditGiaiDoan.Text = string.IsNullOrEmpty(txtEditGiaiDoan.Text) ? "Giai đoạn gốc" : txtEditGiaiDoan.Text;
            lblNgayKetThuc.Text = task.NgayKetThuc.HasValue ? task.NgayKetThuc.Value.ToString("dd/MM/yyyy") : "--/--/----";

            _controlHelpers.BindPriorities(ddlEditDoUuTien, task.IdDoUuTien);
            _controlHelpers.BindTaskStatus(ddlEditTrangThai, task.TrangThai);
            boxTrangThai.Attributes["class"] = "header-dropdown-box status-box-" + task.TrangThai;

            string prioClass = "priority-default";
            if (ddlEditDoUuTien.SelectedItem != null)
            {
                string textPrio = ddlEditDoUuTien.SelectedItem.Text.ToLower();
                if (textPrio.Contains("cao")) prioClass = "priority-high";
                else if (textPrio.Contains("trung bình")) prioClass = "priority-med";
                else if (textPrio.Contains("thấp")) prioClass = "priority-low";
            }
            boxUuTien.Attributes["class"] = "header-dropdown-box " + prioClass;

            if (isPhase)
            {
                rowBreadcrumb.Visible = false;
                colCongViecCha.Visible = false;
                colPhuThuoc.Visible = true;
                BindDependentTasks(null, task.MaCongViec, task.IdCongViecPhuThuoc);
            }
            else
            {
                rowBreadcrumb.Visible = true;
                colCongViecCha.Visible = true;
                colPhuThuoc.Visible = true;
                _controlHelpers.BindParentTasks(ddlEditCongViecCha, CurrentProjectId, task.IdCongViec, task.IdCongViecCha);
                _controlHelpers.BindDependentTasks(ddlEditPhuThuoc, CurrentProjectId, task.IdCongViec, task.IdCongViecPhuThuoc, task.MaCongViec, chiLayGiaiDoan: false);
                BindDependentTasks(task.IdCongViecCha, task.MaCongViec, task.IdCongViecPhuThuoc);
            }

            bool isCompleted = (task.TrangThai == 2 || task.TrangThai == 3);
            bool isDoing = (task.TrangThai == 1);

            // Client chỉ dùng để chọn đúng thông báo/tooltip, không tự suy diễn từ trạng thái
            // dropdown có thể đã thay đổi nhưng chưa được lưu.
            txtEditNgayBatDau.Attributes["data-ql-task-phase"] = isPhase ? "1" : "0";
            txtEditNgayBatDau.Attributes["data-ql-task-children"] = hasChildren ? "1" : "0";
            txtEditNgayBatDau.Attributes["data-ql-task-original-status"] = task.TrangThai.ToString();

            if (isPhase || hasChildren)
            {
                // Không hiện cảnh báo tổng hợp cho công việc cha không phải giai đoạn:
                // task này có thể còn được đổi ngày bắt đầu khi chưa bắt đầu.
                // Chỉ giai đoạn có con mới lấy lịch tổng hợp từ các công việc con.
                // Giai đoạn chưa có con được sửa thời hạn, trừ khi đã hoàn thành.
                divRollUpNotice.Visible = isPhase && hasChildren;

                txtEditThoiHan.Enabled = isPhase && !hasChildren && !isCompleted && this.IsEdit;
                ddlEditDoUuTien.Enabled = false;
                ddlEditTrangThai.Enabled = false;

                if (isCompleted)
                {
                    txtEditNgayBatDau.Enabled = false;
                    txtEditMoTa.Enabled = false;
                    ddlEditPhuThuoc.Enabled = false;

                    txtEditTenCv.Enabled = this.IsEdit;
                }
                else
                {
                    txtEditNgayBatDau.Enabled = isDoing ? false : this.IsEdit;
                    txtEditMoTa.Enabled = this.IsEdit;
                    txtEditTenCv.Enabled = this.IsEdit;
                }
            }
            else
            {
                divRollUpNotice.Visible = false;

                if (isCompleted)
                {
                    txtEditNgayBatDau.Enabled = false;
                    txtEditThoiHan.Enabled = false;
                    ddlEditDoUuTien.Enabled = false;
                    ddlEditPhuThuoc.Enabled = false;
                    txtEditMoTa.Enabled = false;

                    txtEditTenCv.Enabled = this.IsEdit;
                    ddlEditTrangThai.Enabled = this.IsEdit;
                }
                else
                {
                    txtEditTenCv.Enabled = this.IsEdit;

                    txtEditNgayBatDau.Enabled = isDoing ? false : this.IsEdit;

                    txtEditThoiHan.Enabled = this.IsEdit;
                    ddlEditDoUuTien.Enabled = this.IsEdit;
                    ddlEditPhuThuoc.Enabled = this.IsEdit;
                    txtEditMoTa.Enabled = this.IsEdit;
                    ddlEditTrangThai.Enabled = this.IsEdit;
                }

                if (task.TrangThai == 1 || task.TrangThai == 2 || task.TrangThai == 3)
                {
                    ListItem itemChuaBatDau = ddlEditTrangThai.Items.FindByValue("0");
                    if (itemChuaBatDau != null) itemChuaBatDau.Enabled = false;
                }
            }

            ddlEditCongViecCha.Enabled = false;
            UpdateMinStartDate();
            // Chỉ cho chọn ngày trên lịch. HTML readonly vẫn post được giá trị người dùng chọn.
            txtEditNgayBatDau.Attributes["readonly"] = "readonly";
            // Chuẩn bị dữ liệu lịch trước khi mở popup: nút Lịch chỉ chạy client, không postback gây giật chiều cao.
            if (task.NgayBatDau.HasValue)
            {
                try { hdfLocalScheduleJson.Value = BuildLocalTaskScheduleJson(task); }
                catch (Exception ex) { hdfLocalScheduleJson.Value = string.Empty; System.Diagnostics.Trace.TraceError("Lịch dự kiến: " + ex); }
            }
            upModal.Update();
            mdlEditTask.OpenModal(true, IsPostBack ? 0 : 1000);
        }
        #endregion
        // Lịch ban đầu: tải sẵn ngày làm việc trong tháng đang mở, kể cả ngày kề tháng.
        // Danh sách leaf task phục vụ highlight, không cần truy vấn lại khi chọn ngày.
        private string BuildLocalTaskScheduleJson(TblCongViec taskSched)
        {
            if (!taskSched.NgayBatDau.HasValue) return "{}";
            DateTime start = taskSched.NgayBatDau.Value.Date;
            DateTime end = taskSched.NgayKetThuc.HasValue ? taskSched.NgayKetThuc.Value.Date : start;
            DateTime first = new DateTime(start.Year, start.Month, 1);
            int offset = ((int)first.DayOfWeek + 6) % 7;
            DateTime begin = first.AddDays(-offset);
            var days = new Dictionary<string, object>();
            for (int i = 0; i < 42; i++)
            {
                DateTime day = begin.AddDays(i);
                days[day.ToString("yyyy-MM-dd")] = BuildLocalCalendarDay(day);
            }
            var children = TaskManager.Instance.GetLeafTasksForSchedule(taskSched.IdCongViec);
            var tasks = children.Where(t => t.NgayBatDau.HasValue && t.NgayKetThuc.HasValue)
                .Select(t => new {
                    code = t.MaCongViec,
                    name = t.TenCongViec,
                    start = t.NgayBatDau.Value.ToString("yyyy-MM-dd"),
                    end = t.NgayKetThuc.Value.ToString("yyyy-MM-dd")
                }).ToList();
            return Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                start = start.ToString("yyyy-MM-dd"),
                end = end.ToString("yyyy-MM-dd"),
                days,
                tasks
            });
        }

        private static object BuildLocalCalendarDay(DateTime day)
        {
            var manager = LichBieuChungManager.Instance;
            bool working = manager.CheckIsWorkingDay(day);
            TblLichNgoaiLe ex = manager.GetExceptionByDate(day);
            bool weekend = day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday;
            string name = ex != null ? (ex.TenNgoaiLe ?? "") : "";
            bool namedWeekend = name.IndexOf("cuối tuần", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("chủ nhật", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("thứ bảy", StringComparison.OrdinalIgnoreCase) >= 0;
            string status = working ? "free"
                : (weekend && (ex == null || namedWeekend) ? "weekend" : "holiday");
            return new
            {
                working,
                status,
                text = working ? "Trống" : (status == "weekend" ? "Cuối tuần" :
                    (string.IsNullOrWhiteSpace(name) ? "Ngày nghỉ" : name))
            };
        }

        // AJAX chỉ trả về lịch làm việc và kết quả tính ngày, không trả dữ liệu dự án/task.
        // Không sao chép logic trừ cuối tuần trên JS: tính duy nhất từ LichBieuChungManager.
        [System.Web.Services.WebMethod]
        public static object LayLichDuKienCongViec(string ngayBatDau, int thoiHan,
            string ngayKetThucChon, int nam, int thang)
        {
            var ctx = System.Web.HttpContext.Current;
            if (ctx == null || ctx.User == null || ctx.User.Identity == null ||
                !ctx.User.Identity.IsAuthenticated || SweetContext.Current == null ||
                SweetContext.Current.UserId == Guid.Empty)
                return new { success = false, message = "Phiên đăng nhập đã hết hạn." };
            if (!DateTime.TryParseExact(ngayBatDau, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime start)
                || start.Year < 2000 || start.Year > 2100 || thoiHan < 1 || thoiHan > 3500
                || nam < 2000 || nam > 2100 || thang < 1 || thang > 12)
                return new { success = false, message = "Ngày hoặc thời hạn không hợp lệ." };
            try
            {
                var manager = LichBieuChungManager.Instance;
                int duration = thoiHan;
                if (!string.IsNullOrWhiteSpace(ngayKetThucChon))
                {
                    if (!DateTime.TryParseExact(ngayKetThucChon, "yyyy-MM-dd",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out DateTime chosen)
                        || chosen.Date < start.Date || chosen.Date > start.AddDays(3650)
                        || !manager.CheckIsWorkingDay(chosen))
                        return new { success = false, message = "Ngày kết thúc phải là ngày làm việc và không trước ngày bắt đầu." };
                    duration = 0;
                    for (DateTime d = start; d <= chosen; d = d.AddDays(1))
                        if (manager.CheckIsWorkingDay(d)) duration++;
                    if (duration < 1 || duration > 3500)
                        return new { success = false, message = "Khoảng thời gian không hợp lệ." };
                }
                // Cùng một hàm được dùng tại btnSaveTask_Click.
                DateTime calculatedEnd = manager.CalculateTaskEndDate(start, duration);
                DateTime first = new DateTime(nam, thang, 1);
                int offset = ((int)first.DayOfWeek + 6) % 7;
                DateTime begin = first.AddDays(-offset);
                var days = new Dictionary<string, object>();
                for (int i = 0; i < 42; i++)
                {
                    DateTime day = begin.AddDays(i);
                    days[day.ToString("yyyy-MM-dd")] = BuildLocalCalendarDay(day);
                }
                return new
                {
                    success = true,
                    duration,
                    endDate = calculatedEnd.ToString("yyyy-MM-dd"),
                    days
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Xem truoc lich cong viec: " + ex);
                return new { success = false, message = "Không thể tính lịch theo cấu hình ngày làm việc." };
            }
        }

        private void Reminder_Callback(object sender, Guid taskId)
        {
            if (taskId == Guid.Empty)
                return;

            Guid currentUserId = SweetContext.Current != null
                ? SweetContext.Current.UserId
                : Guid.Empty;

            if (currentUserId == Guid.Empty)
                return;

            TblCongViec task = TaskManager.Instance.FetchById(taskId);

            if (task == null || task.DaXoa == true || task.IdDuAn != CurrentProjectId) return;
            hdfReminderTaskId.Value = taskId.ToString();
            hdfSelectedReminderIds.Value = string.Empty;
            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "ResetReminderButton",
                "setTimeout(function(){ var b=document.getElementById('" + btnProcessReminders.ClientID + "'); if(b){ b.style.pointerEvents='none'; b.style.opacity='0.55'; b.style.cursor='default'; } }, 0);",
                true
            );
            List<TblNhacViecLichCongViec> reminders =
                NhacViecLichCongViecManager.Instance.GetPendingByTask(
                    taskId,
                    currentUserId);

            ltrReminderTaskName.Text =
                $"[{task.MaCongViec}] {task.TenCongViec}";

            rptTaskReminders.DataSource = reminders;
            rptTaskReminders.DataBind();

            pnlNoReminder.Visible =
                reminders == null || reminders.Count == 0;

            mdlTaskReminder.Title = "Nhắc việc - " + task.MaCongViec;

            upTaskReminder.Update();
            mdlTaskReminder.OpenModal(true);
        }

        protected void btnProcessReminders_Click(object sender, EventArgs e)
        {

            if (!Guid.TryParse(hdfReminderTaskId.Value, out Guid taskId) ||
                taskId == Guid.Empty)
                return;
            TblCongViec task = TaskManager.Instance.FetchById(taskId);

            if (task == null ||
                task.DaXoa == true ||
                task.IdDuAn != CurrentProjectId)
                return;
            Guid currentUserId = SweetContext.Current != null
                ? SweetContext.Current.UserId
                : Guid.Empty;
            if (currentUserId == Guid.Empty)
                return;
            List<Guid> reminderIds = new List<Guid>();
            string rawIds = hdfSelectedReminderIds.Value ?? string.Empty;
            foreach (string value in rawIds.Split(','))
            {
                if (Guid.TryParse(value, out Guid reminderId) &&
                    reminderId != Guid.Empty)
                {
                    reminderIds.Add(reminderId);
                }
            }
            reminderIds = reminderIds
                .Distinct()
                .ToList();
            if (reminderIds.Count == 0)
                return;
            List<TblNhacViecLichCongViec> pendingReminders =
                NhacViecLichCongViecManager.Instance.GetPendingByTask(
                    taskId,
                    currentUserId);

            HashSet<Guid> validReminderIds =
                new HashSet<Guid>(
                    pendingReminders.Select(x => x.IdNhacViec));
            reminderIds = reminderIds
                .Where(id => validReminderIds.Contains(id))
                .Distinct()
                .ToList();
            if (reminderIds.Count == 0)
                return;
            int processedCount = NhacViecLichCongViecManager.Instance.MarkAsProcessed(reminderIds);
            if (processedCount > 0)
            {
                ShowNotify("Đã xử lý nhắc việc thành công!", MSGType.Success);
            }
            else
            {
                ShowNotify("Không thể xử lý nhắc việc.", MSGType.Error);
            }
            hdfSelectedReminderIds.Value = string.Empty;
            List<TblNhacViecLichCongViec> reminders =
                NhacViecLichCongViecManager.Instance.GetPendingByTask(
                    taskId,
                    currentUserId);
            rptTaskReminders.DataSource = reminders;
            rptTaskReminders.DataBind();
            pnlNoReminder.Visible =
                reminders == null || reminders.Count == 0;
            upTaskReminder.Update();
            CtrlTask1.Rebind();
            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "ResetReminderBtnAfterProcess",
                "setTimeout(function(){ var b=document.getElementById('" + btnProcessReminders.ClientID + "'); if(b){ b.style.pointerEvents='none'; b.style.opacity='0.55'; b.style.cursor='default'; } }, 0);",
                true
            );
            if (reminders != null && reminders.Count > 0)
            {
                mdlTaskReminder.OpenModal(true);
            }
            else
            {
                mdlTaskReminder.CloseModal();
            }
        }

        #region Logic Lưu Edit
        protected void btnSaveTask_Click(object sender, EventArgs e)
        {
            if (!this.IsEdit) { ShowNotify(GetResourceText(BackEndResourceKeys.NO_PERMISSION_EDIT), MSGType.Error); return; }
            if (!Guid.TryParse(hfEditTaskId.Value, out Guid taskId)) return;

            try { DuAnManager.Instance.EnsureCanModifyStructure(CurrentProjectId); }
            catch (Exception ex) { ShowNotify(ex.Message, MSGType.Error); return; }

            TblCongViec task = TaskManager.Instance.FetchById(taskId);
            if (task == null) return;

            bool isPhase = TaskManager.Instance.CheckPhase(task);
            bool isFatherTask = TaskManager.Instance.CheckHasChildTasks(CurrentProjectId, task);
            byte oldStatus = task.TrangThai;
            // Snapshot truoc khi sua object SubSonic, phuc vu doi chieu sau khi lay AppLock.
            DateTime? ngayBatDauCu = task.NgayBatDau;
            DateTime? ngayKetThucCu = task.NgayKetThuc;
            Guid? phuThuocCu = task.IdCongViecPhuThuoc;
            string tenCv = txtEditTenCv.Text.Trim();

            if (string.IsNullOrEmpty(tenCv)) { ShowNotify(GetResourceText(BackEndResourceKeys.CAN_NOT_BE_BLANK), MSGType.Error); return; }
            if (!DateTime.TryParse(txtEditNgayBatDau.Text.Trim(), out DateTime ngayBd))
            {
                ShowNotify(GetResourceText(BackEndResourceKeys.CAN_NOT_BE_BLANK), MSGType.Error);
                return;
            }

            // Dropdown có lựa chọn "-- Không phụ thuộc --" (Value rỗng).
            // Đọc giá trị mới TRƯỚC các kiểm tra ngày và trạng thái, tránh dùng phụ thuộc cũ.
            Guid? selectedDependencyId = Guid.TryParse(ddlEditPhuThuoc.SelectedValue, out Guid depId)
                ? (Guid?)depId
                : null;

            if (isPhase && selectedDependencyId.HasValue)
            {
                // Chỉ kiểm tra thứ tự khi THỰC SỰ chọn một giai đoạn phụ thuộc.
                // Chọn "Không phụ thuộc" là hợp lệ kể cả với giai đoạn thứ 2 trở đi.
                DataTable phaseTable = TaskManager.Instance.FetchPhasesByProjectId(CurrentProjectId);
                List<TblCongViec> phases = GetOrderedProjectPhases(phaseTable);
                int currentPhaseIndex = phases.FindIndex(x => x.IdCongViec == task.IdCongViec);

                if (currentPhaseIndex <= 0 ||
                    !phases.Take(currentPhaseIndex).Any(x => x.IdCongViec == selectedDependencyId.Value))
                {
                    ShowNotify("Phụ thuộc của Giai đoạn phải là một Giai đoạn đứng trước nó.", MSGType.Warning);
                    return;
                }
            }

            // Dùng phụ thuộc vừa chọn cho mọi kiểm tra và cho task.Save() về sau.
            // Với "Không phụ thuộc", giá trị sẽ là NULL trong CSDL.
            task.IdCongViecPhuThuoc = selectedDependencyId;

            var (minStartAllowed, limitReason) = TaskManager.Instance.GetMinStartDate(task.IdCongViecCha, task.IdCongViecPhuThuoc);
            if (minStartAllowed.HasValue && ngayBd.Date < minStartAllowed.Value.Date)
            {
                ShowNotify(string.Format(GetResourceText(BackEndResourceKeys.INVALID_START_DATE_LIMIT), minStartAllowed.Value.ToString("dd/MM/yyyy"), limitReason), MSGType.Error);
                return;
            }

            // Giai đoạn chưa có công việc con được sửa thời hạn như một công việc lá.
            // Không cho phép PostBack tùy ý sửa thời hạn của giai đoạn có con hoặc đã hoàn thành.
            if (isPhase && !isFatherTask && oldStatus != 2 && oldStatus != 3)
            {
                if (!int.TryParse(txtEditThoiHan.Text.Trim(), out int phaseDuration)
                    || phaseDuration < 1 || phaseDuration > 3500)
                {
                    ShowNotify("Thời hạn giai đoạn phải từ 1 đến 3500 ngày làm việc.", MSGType.Error);
                    return;
                }

                task.ThoiHanNgay = phaseDuration;
                task.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(ngayBd, phaseDuration);
            }

            task.NgayBatDau = ngayBd;

            if (!isPhase)
            {
                if (!int.TryParse(txtEditThoiHan.Text.Trim(), out int thoiHan) || thoiHan <= 0)
                {
                    ShowNotify(GetResourceText(BackEndResourceKeys.TASK_DURATION_MUST_BE_POSITIVE), MSGType.Error);
                    return;
                }

                byte newStatus = Convert.ToByte(ddlEditTrangThai.SelectedValue);

                if (newStatus != 0 && task.IdCongViecPhuThuoc.HasValue)
                {
                    TblCongViec dependentTask = TaskManager.Instance.FetchById(task.IdCongViecPhuThuoc.Value);
                    if (dependentTask != null && (dependentTask.TrangThai != 2 && dependentTask.TrangThai != 3))
                    {
                        ShowNotify($"Không thể thực hiện! Công việc này phụ thuộc vào [{dependentTask.MaCongViec}] nhưng công việc đó chưa hoàn thành.", MSGType.Warning);
                        return;
                    }
                }

                if (newStatus != 0)
                {
                    Guid? checkParentId = task.IdCongViecCha;
                    while (checkParentId.HasValue)
                    {
                        TblCongViec pTask = TaskManager.Instance.FetchById(checkParentId.Value);
                        if (pTask != null)
                        {
                            if (pTask.IdCongViecPhuThuoc.HasValue)
                            {
                                TblCongViec pDepTask = TaskManager.Instance.FetchById(pTask.IdCongViecPhuThuoc.Value);
                                if (pDepTask != null && pDepTask.TrangThai != 2 && pDepTask.TrangThai != 3)
                                {
                                    ShowNotify($"Không thể thực hiện! Giai đoạn/Công việc cha [{pTask.MaCongViec}] đang bị kẹt phụ thuộc vào [{pDepTask.MaCongViec}] chưa hoàn thành.", MSGType.Warning);
                                    return;
                                }
                            }
                            checkParentId = pTask.IdCongViecCha;
                        }
                        else break;
                    }
                }

                if ((oldStatus == 1 || oldStatus == 2 || oldStatus == 3) && newStatus == 0)
                {
                    ShowNotify("Không thể chuyển công việc đang làm hoặc đã hoàn thành về trạng thái Chưa bắt đầu!", MSGType.Warning);
                    return;
                }

                task.TrangThai = newStatus;

                if (oldStatus != newStatus)
                {
                    try { DuAnManager.Instance.EnsureCanUpdateProgress(CurrentProjectId); }
                    catch (Exception ex) { ShowNotify(ex.Message, MSGType.Error); return; }
                }

                if (!isFatherTask) task.IdDoUuTien = Guid.TryParse(ddlEditDoUuTien.SelectedValue, out Guid idUt) ? (Guid?)idUt : null;

                task.ThoiHanNgay = thoiHan;
                task.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(ngayBd, thoiHan);

                if (newStatus == 2 || newStatus == 3)
                {
                    if ((oldStatus != 2 && oldStatus != 3) || !task.NgayHoanThanhThucTe.HasValue)
                        task.NgayHoanThanhThucTe = DateTime.Now;
                    if (task.NgayKetThuc.HasValue && task.NgayHoanThanhThucTe.HasValue)
                    {
                        if (task.NgayHoanThanhThucTe.Value.Date > task.NgayKetThuc.Value.Date) task.TrangThai = 3;
                        else task.TrangThai = 2;
                    }
                }
                else
                    task.NgayHoanThanhThucTe = null;

            }

            task.TenCongViec = tenCv;
            task.MoTa = txtEditMoTa.Text.Trim();
            task.NgayCapNhat = DateTime.Now;
            // GIU NGUYEN tat ca AutoSet* cua partner (tham so true khong doi).
            // Chi boc mot giao dich bao ve quanh phan save + lan truyen du lieu.
            Action luuVaDongBoLichPartner = () =>
            {
                task.Save();

                if (isFatherTask)
                {
                    TaskManager.Instance.AutoSetFirstChildStartTime(CurrentProjectId, task.IdCongViec, task.NgayBatDau.Value, true);
                }
                else
                {
                    TaskManager.Instance.AutoSetDependentTime(CurrentProjectId, task.IdCongViec, true);
                    if (task.IdCongViecCha.HasValue)
                    {
                        TaskManager.Instance.AutoSetParentPriority(CurrentProjectId, task.IdCongViecCha.Value, _dictPriorities);
                        TaskManager.Instance.AutoSetParentTime(CurrentProjectId, task.IdCongViecCha.Value, true);
                        TaskManager.Instance.AutoSetParentStatus(CurrentProjectId, task.IdCongViecCha.Value);
                    }
                }
            };

            try
            {
                if (KhaiBaoGioManager.KhaiBaoGio20DaKichHoat())
                {
                    // ReadCommitted thay vi Serializable mac dinh de tranh giu range lock qua lon.
                    var tuyChon = new TransactionOptions
                    {
                        IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                        Timeout = TimeSpan.FromMinutes(2)
                    };
                    using (var giaoDich = new TransactionScope(TransactionScopeOption.Required, tuyChon))
                    using (var dongBo = new KhaiBaoGio20Manager().MoDongBoKeHoachTrongTransactionScope(CurrentProjectId))
                    {
                        dongBo.KiemTraBanGhiGoc(task.IdCongViec, ngayBatDauCu,
                            ngayKetThucCu, phuThuocCu, oldStatus);
                        dongBo.ChotGioTheoLichCu();       // Lich CU, dung moc hieu luc.
                        luuVaDongBoLichPartner();          // KHONG thay doi AutoSetDependentTime.
                        dongBo.KiemTraVaApDungLichMoi();  // Lich MOI, khong tinh nguoc.
                        giaoDich.Complete();
                    }
                }
                else
                {
                    // Flag V2 tat: giu nguyen luong cu, khong can ket noi KBG20.
                    luuVaDongBoLichPartner();
                }
            }
            catch (Exception ex)
            {
                string maThamChieu = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
                SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(ex,
                    "KBG20 DongBoKeHoach TaskList | Ma=" + maThamChieu + " | Task=" + task.IdCongViec);
                // Chi hien ro loi NGHIEP VU duoc kiem soat; khong dua SQL exception ra trinh duyet.
                bool loiNghiepVu = ex is InvalidOperationException &&
                    (ex.Message.StartsWith("Du lieu cong viec vua duoc nguoi khac cap nhat") ||
                     ex.Message.StartsWith("Khong the doi ngay bat dau cua cong viec") ||
                     ex.Message.StartsWith("Khong the chuyen task [") ||
                     ex.Message.StartsWith("Cong viec ["));
                ShowNotify(loiNghiepVu ? ex.Message :
                    "Không thể lưu thay đổi kế hoạch an toàn. Mã tham chiếu: " + maThamChieu,
                    MSGType.Error);
                return;
            }

            ShowNotify(GetResourceText(BackEndResourceKeys.DATA_HAS_BEEN_UPDATED_SUCCESSFULLY), MSGType.Success);
            CtrlTask1.Rebind();
            mdlEditTask.CloseModal();
        }

        private List<TblCongViec> GetOrderedProjectPhases(DataTable phaseTable)
        {
            List<TblCongViec> phases = new List<TblCongViec>();
            if (phaseTable == null) return phases;

            foreach (DataRow row in phaseTable.Rows)
            {
                if (row[TaskManager.ColIdCongViec] == DBNull.Value ||
                    !Guid.TryParse(row[TaskManager.ColIdCongViec].ToString(), out Guid phaseId))
                    continue;

                TblCongViec phase = TaskManager.Instance.FetchById(phaseId);
                if (phase != null && phase.IdDuAn == CurrentProjectId && !phase.IdCongViecCha.HasValue && phase.DaXoa != true)
                    phases.Add(phase);
            }

            return phases
                .OrderBy(x =>
                {
                    string rootCode = string.IsNullOrWhiteSpace(x.MaCongViec) ? "" : x.MaCongViec.Split('.')[0];
                    return int.TryParse(rootCode, out int number) ? number : int.MaxValue;
                })
                .ThenBy(x => x.MaCongViec)
                .ToList();
        }

        private int CompareWBS(string code1, string code2)
        {
            string[] p1 = (code1 ?? "").Split('.');
            string[] p2 = (code2 ?? "").Split('.');
            for (int i = 0; i < Math.Min(p1.Length, p2.Length); i++)
            {
                int.TryParse(p1[i], out int n1);
                int.TryParse(p2[i], out int n2);
                if (n1 != n2) return n1.CompareTo(n2);
            }
            return p1.Length.CompareTo(p2.Length);
        }

        private void BindDependentTasks(Guid? parentId, string currentTaskCode, Guid? currentDependentId = null)
        {
            ddlEditPhuThuoc.Items.Clear();
            ddlEditPhuThuoc.Items.Add(new ListItem("-- Không phụ thuộc --", string.Empty));

            SqlQuery query = new Select()
                .From(TblCongViec.Schema)
                .Where(TblCongViec.Columns.IdDuAn).IsEqualTo(CurrentProjectId)
                .And(TblCongViec.Columns.DaXoa).IsEqualTo(false);

            DataTable dt = query.ExecuteDataSet().Tables[0];

            if (dt != null && dt.Rows.Count > 0)
            {
                var rows = dt.AsEnumerable().ToList();
                rows.Sort((row1, row2) => CompareWBS(row1["MaCongViec"]?.ToString().Trim(), row2["MaCongViec"]?.ToString().Trim()));

                string parentCode = "";
                if (parentId.HasValue)
                {
                    var pRow = rows.Find(r => r["IdCongViec"].ToString().Equals(parentId.Value.ToString(), StringComparison.OrdinalIgnoreCase));
                    if (pRow != null) parentCode = pRow["MaCongViec"]?.ToString().Trim() ?? "";
                }

                foreach (DataRow row in rows)
                {
                    string maCongViec = row["MaCongViec"]?.ToString().Trim() ?? string.Empty;
                    string tenCongViec = row["TenCongViec"]?.ToString().Trim() ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(maCongViec)) continue;

                    if (Guid.TryParse(Convert.ToString(row["IdCongViec"]), out Guid idCongViec))
                    {
                        // Bỏ qua các công việc nằm phía sau (hoặc chính nó)
                        if (CompareWBS(maCongViec, currentTaskCode) >= 0)
                        {
                            continue;
                        }

                        if (!parentId.HasValue)
                        {
                            // Nếu đang là root (giai đoạn), chỉ phụ thuộc root khác
                            if (maCongViec.Contains(".")) continue;
                        }
                        else
                        {
                            // Nếu đang là con, không phụ thuộc vào chính cha của mình
                            if (maCongViec == parentCode || parentCode.StartsWith(maCongViec + "."))
                            {
                                continue;
                            }
                        }

                        int level = maCongViec.Split('.').Length;
                        string prefix = level > 1 ? new String('-', (level - 1) * 2) + " " : "";

                        ddlEditPhuThuoc.Items.Add(new ListItem($"{prefix}[{maCongViec}] {tenCongViec}", idCongViec.ToString()));
                    }
                }
            }

            // Phục hồi lại Option đã chọn trước đó
            if (currentDependentId.HasValue)
            {
                var item = ddlEditPhuThuoc.Items.FindByValue(currentDependentId.Value.ToString());
                if (item != null)
                {
                    ddlEditPhuThuoc.SelectedValue = item.Value;
                }
            }
        }
        protected void ddlEditCongViecChaSelected(object sender, EventArgs e)
        {
            Guid? parentId = Guid.TryParse(ddlEditCongViecCha.SelectedValue, out Guid pid) ? (Guid?)pid : null;
            txtEditGiaiDoan.Text = TaskManager.Instance.GetRootPhaseName(CurrentProjectId, parentId);
            Guid? currentExcludeId = Guid.TryParse(hfEditTaskId.Value, out Guid tid) ? (Guid?)tid : null;
            _controlHelpers.BindDependentTasks(ddlEditPhuThuoc, CurrentProjectId, currentExcludeId, currentOrNewCode: txtEditMaCv.Text.Trim());
            UpdateMinStartDate();
            upModal.Update();
            mdlEditTask.OpenModal(true);
        }

        protected void ddlEditPhuThuocSelected(object sender, EventArgs e)
        {
            UpdateMinStartDate();
            upModal.Update();
            mdlEditTask.OpenModal(true);
        }
        #endregion

        #region Helpers
        private void UpdateMinStartDate()
        {
            txtEditNgayBatDau.Attributes.Remove("min");

            Guid? parentId = Guid.TryParse(ddlEditCongViecCha.SelectedValue, out Guid pid)
                ? (Guid?)pid
                : null;

            Guid? depId = Guid.TryParse(ddlEditPhuThuoc.SelectedValue, out Guid did)
                ? (Guid?)did
                : null;

            if (Guid.TryParse(hfEditTaskId.Value, out Guid taskId))
            {
                TblCongViec currentTask = TaskManager.Instance.FetchById(taskId);

                if (currentTask != null && TaskManager.Instance.CheckPhase(currentTask))
                {
                    parentId = null;
                    depId = Guid.TryParse(ddlEditPhuThuoc.SelectedValue, out Guid phaseDepId)
                        ? (Guid?)phaseDepId
                        : null;
                }
            }

            var (minStartLimit, _) = TaskManager.Instance.GetMinStartDate(parentId, depId);

            if (minStartLimit.HasValue)
            {
                string minDateStr = minStartLimit.Value.ToString("yyyy-MM-dd");
                txtEditNgayBatDau.Attributes["min"] = minDateStr;

                if (DateTime.TryParse(txtEditNgayBatDau.Text.Trim(), out DateTime currentStartDate))
                {
                    if (currentStartDate.Date < minStartLimit.Value.Date)
                        txtEditNgayBatDau.Text = minDateStr;
                }
                else
                {
                    txtEditNgayBatDau.Text = minDateStr;
                }
            }
        }
        #region Logic Cấu Hình Hệ Số Đóng Góp (Popup)

        private void ConfigHeSo_Callback(object sender, EventArgs e)
        {
            if (!this.IsEdit)
            {
                ShowNotify(GetResourceText(BackEndResourceKeys.NO_PERMISSION_EDIT), MSGType.Error);
                return;
            }

            DataTable dtHeSo = HeSoDongGopManager.Instance.GetHeSoCuaDuAn(CurrentProjectId);

            if (dtHeSo == null || dtHeSo.Rows.Count == 0)
            {
                HeSoDongGopManager.Instance.InitializeProjectCoefficients(CurrentProjectId);
                dtHeSo = HeSoDongGopManager.Instance.GetHeSoCuaDuAn(CurrentProjectId);
            }

            rptHeSoDongGop.DataSource = dtHeSo;
            rptHeSoDongGop.DataBind();
            upHeSoDongGop.Update();
            mdlHeSoDongGop.OpenModal(true);
        }

        protected void btnSaveHeSo_Click(object sender, EventArgs e)
        {
            if (!this.IsEdit) return;

            try
            {
                List<TblHeSoDongGop> lstUpdate = new List<TblHeSoDongGop>();
                foreach (RepeaterItem item in rptHeSoDongGop.Items)
                {
                    if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                    {
                        HiddenField hdfIdDoUuTien = (HiddenField)item.FindControl("hdfIdDoUuTien");
                        HiddenField hdfIdHeSoDongGop = (HiddenField)item.FindControl("hdfIdHeSoDongGop");
                        TextBox txtHeSo = (TextBox)item.FindControl("txtHeSo");

                        string val = txtHeSo.Text.Trim().Replace(",", ".");

                        if (Guid.TryParse(hdfIdDoUuTien.Value, out Guid idDoUuTien) &&
                            decimal.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal heSo))
                        {
                            Guid.TryParse(hdfIdHeSoDongGop.Value, out Guid idHeSoDongGop);

                            lstUpdate.Add(new TblHeSoDongGop
                            {
                                IdHeSoDongGop = idHeSoDongGop,
                                IdDoUuTien = idDoUuTien,
                                HeSoDongGop = heSo
                            });
                        }
                    }
                }

                bool isSaved = HeSoDongGopManager.Instance.SaveHeSoCuaDuAn(CurrentProjectId, lstUpdate);
                if (isSaved)
                {
                    ShowNotify("Cập nhật hệ số đóng góp thành công!", MSGType.Success);
                    mdlHeSoDongGop.CloseModal();

                    CtrlTask1.Rebind();
                }
                else
                {
                    ShowNotify("Không thể lưu hệ số đóng góp.", MSGType.Error);
                }
            }
            catch (Exception ex)
            {
                ShowNotify(ex.Message, MSGType.Error);
            }
        }

        #endregion

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlTask1.ConfirmRequest(e);
        }
        #endregion
    }
}