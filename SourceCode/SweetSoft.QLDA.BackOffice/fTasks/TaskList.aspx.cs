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
            CtrlTask1.ConfigHeSoHandlerCallback = ConfigHeSo_Callback;
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
            bool isPhase = TaskManager.Instance.CheckPhase(task);
            bool hasChildren = TaskManager.Instance.CheckHasChildTasks(CurrentProjectId, task);
            mdlEditTask.Title = isPhase ? "Cập nhật Giai đoạn (Phase)" : "Cập nhật Công việc con";
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
                BindPhaseDependencies(task);
            }
            else
            {
                rowBreadcrumb.Visible = true;
                colCongViecCha.Visible = true;
                colPhuThuoc.Visible = true;
                _controlHelpers.BindParentTasks(ddlEditCongViecCha, CurrentProjectId, task.IdCongViec, task.IdCongViecCha);
                _controlHelpers.BindDependentTasks(ddlEditPhuThuoc, CurrentProjectId, task.IdCongViec, task.IdCongViecPhuThuoc, task.MaCongViec, chiLayGiaiDoan: false);
                if (!task.IdCongViecCha.HasValue)
                {
                    List<ListItem> invalidItems = new List<ListItem>();
                    foreach (ListItem item in ddlEditPhuThuoc.Items)
                    {
                        if (Guid.TryParse(item.Value, out Guid depId))
                        {
                            var depTask = TaskManager.Instance.FetchById(depId);
                            if (depTask != null && depTask.IdCongViecCha.HasValue) invalidItems.Add(item);
                        }
                    }
                    foreach (var item in invalidItems) ddlEditPhuThuoc.Items.Remove(item);
                }
            }
            if (isPhase || hasChildren)
            {
                divRollUpNotice.Visible = true;
                txtEditNgayBatDau.Enabled = this.IsEdit;
                txtEditThoiHan.Enabled = false;
                ddlEditDoUuTien.Enabled = false;
                ddlEditTrangThai.Enabled = false;
            }
            else
            {
                divRollUpNotice.Visible = false;
                txtEditNgayBatDau.Enabled = this.IsEdit;
                txtEditThoiHan.Enabled = this.IsEdit;
                ddlEditDoUuTien.Enabled = this.IsEdit;
                ddlEditTrangThai.Enabled = this.IsEdit;
                if (task.TrangThai == 1 || task.TrangThai == 2 || task.TrangThai == 3)
                {
                    ListItem itemChuaBatDau = ddlEditTrangThai.Items.FindByValue("0");
                    if (itemChuaBatDau != null) itemChuaBatDau.Enabled = false;
                }
            }
            ddlEditCongViecCha.Enabled = false;
            UpdateMinStartDate();
            upModal.Update();
            mdlEditTask.OpenModal(true, IsPostBack ? 0 : 1000);
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

            if (task == null || task.DaXoa == true ||task.IdDuAn != CurrentProjectId) return;
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
        #endregion

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
            string tenCv = txtEditTenCv.Text.Trim();
            if (string.IsNullOrEmpty(tenCv)) { ShowNotify(GetResourceText(BackEndResourceKeys.CAN_NOT_BE_BLANK), MSGType.Error); return; }
            if (!DateTime.TryParse(txtEditNgayBatDau.Text.Trim(), out DateTime ngayBd))
            {
                ShowNotify(GetResourceText(BackEndResourceKeys.CAN_NOT_BE_BLANK), MSGType.Error);
                return;
            }
            if (isPhase)
            {
                DataTable phaseTable = TaskManager.Instance.FetchPhasesByProjectId(CurrentProjectId);
                List<TblCongViec> phases = GetOrderedProjectPhases(phaseTable);
                int currentPhaseIndex = phases.FindIndex(x => x.IdCongViec == task.IdCongViec);

                if (currentPhaseIndex <= 0)
                {
                    task.IdCongViecPhuThuoc = null;
                }
                else
                {
                    Guid? selectedDependencyId = Guid.TryParse(ddlEditPhuThuoc.SelectedValue, out Guid phaseDepId)
                        ? (Guid?)phaseDepId
                        : null;

                    if (!selectedDependencyId.HasValue ||
                        !phases.Take(currentPhaseIndex).Any(x => x.IdCongViec == selectedDependencyId.Value))
                    {
                        ShowNotify("Phụ thuộc của Giai đoạn phải là một Giai đoạn đứng trước nó.", MSGType.Warning);
                        return;
                    }

                    task.IdCongViecPhuThuoc = selectedDependencyId;
                }
            }

            var (minStartAllowed, limitReason) = TaskManager.Instance.GetMinStartDate(task.IdCongViecCha, task.IdCongViecPhuThuoc);
            if (minStartAllowed.HasValue && ngayBd.Date < minStartAllowed.Value.Date)
            {
                ShowNotify(string.Format(GetResourceText(BackEndResourceKeys.INVALID_START_DATE_LIMIT), minStartAllowed.Value.ToString("dd/MM/yyyy"), limitReason), MSGType.Error);
                return;
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
                task.IdCongViecPhuThuoc = Guid.TryParse(ddlEditPhuThuoc.SelectedValue, out Guid ptId) ? (Guid?)ptId : null;
            }
            task.TenCongViec = tenCv;
            task.MoTa = txtEditMoTa.Text.Trim();
            task.NgayCapNhat = DateTime.Now;
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

        private void BindPhaseDependencies(TblCongViec currentPhase)
        {
            ddlEditPhuThuoc.Items.Clear();

            List<TblCongViec> phases = GetOrderedProjectPhases(TaskManager.Instance.FetchPhasesByProjectId(CurrentProjectId));
            int currentIndex = phases.FindIndex(x => x.IdCongViec == currentPhase.IdCongViec);

            if (currentIndex <= 0)
            {
                ddlEditPhuThuoc.Items.Add(new ListItem("Giai đoạn đầu tiên nên không có phụ thuộc", ""));
                ddlEditPhuThuoc.SelectedIndex = 0;
                ddlEditPhuThuoc.Enabled = false;
                return;
            }

            for (int i = 0; i < currentIndex; i++)
            {
                TblCongViec previousPhase = phases[i];
                ddlEditPhuThuoc.Items.Add(new ListItem(
                    $"[{previousPhase.MaCongViec}] {previousPhase.TenCongViec}",
                    previousPhase.IdCongViec.ToString()));
            }

            if (currentPhase.IdCongViecPhuThuoc.HasValue)
            {
                ListItem selectedItem = ddlEditPhuThuoc.Items.FindByValue(currentPhase.IdCongViecPhuThuoc.Value.ToString());
                if (selectedItem != null)
                    ddlEditPhuThuoc.SelectedValue = selectedItem.Value;
                else
                    ddlEditPhuThuoc.SelectedIndex = ddlEditPhuThuoc.Items.Count - 1;
            }
            else
            {
                ddlEditPhuThuoc.SelectedIndex = ddlEditPhuThuoc.Items.Count - 1;
            }

            ddlEditPhuThuoc.Enabled = this.IsEdit;
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
                    // Phase không có công việc cha. Dependency của Phase luôn là một Phase
                    // đứng trước nó và lấy trực tiếp từ dropdown để min ngày cập nhật ngay
                    // khi người dùng đổi dependency.
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

        private void ShowNotify(string message, MSGType msgType)
        {
            string safeMsg = message.Replace("'", "\\'").Replace("\r\n", "\\n").Replace("\n", "\\n");
            mdlEditTask.OpenModal(true);
            ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), $"alert('{safeMsg}');", true);
        }
        #region Logic Cấu Hình Hệ Số Đóng Góp (Popup)

        private void ConfigHeSo_Callback(object sender, EventArgs e)
        {
            if (!this.IsEdit)
            {
                ShowNotify(GetResourceText(BackEndResourceKeys.NO_PERMISSION_EDIT), MSGType.Error);
                return;
            }

            // Lấy 3 dòng hệ số của RIÊNG PROJECT NÀY
            DataTable dtHeSo = HeSoDongGopManager.Instance.GetHeSoCuaDuAn(CurrentProjectId);

            // Chốt chặn an toàn: Nếu dự án cũ chưa có hệ số -> Khởi tạo cho nó luôn
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
                // Quét qua Repeater để lấy ID và Hệ số mới
                foreach (RepeaterItem item in rptHeSoDongGop.Items)
                {
                    if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                    {
                        HiddenField hdfIdDoUuTien = (HiddenField)item.FindControl("hdfIdDoUuTien");
                        HiddenField hdfIdHeSoDongGop = (HiddenField)item.FindControl("hdfIdHeSoDongGop");
                        TextBox txtHeSo = (TextBox)item.FindControl("txtHeSo");

                        // [CHỮA BỆNH Ở ĐÂY]: Quy đổi mọi dấu phẩy thành dấu chấm
                        string val = txtHeSo.Text.Trim().Replace(",", ".");

                        // Dùng InvariantCulture để ép server luôn hiểu dấu chấm là thập phân
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

                // Lưu Update đè lên 3 dòng của Project này
                bool isSaved = HeSoDongGopManager.Instance.SaveHeSoCuaDuAn(CurrentProjectId, lstUpdate);
                if (isSaved)
                {
                    ShowNotify("Cập nhật hệ số đóng góp thành công!", MSGType.Success);
                    mdlHeSoDongGop.CloseModal();

                    // Nạp lại danh sách công việc ở UI -> Tự động truy vấn lại -> Cập nhật toàn bộ điểm Task!
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
