using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper;
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

namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    public partial class CtrlTask : BaseAdminUserControl
    {
        public EventHandler NewTaskHandlerCallback;
        public EventHandler<Guid> NewSubTaskHandlerCallback;
        public EventHandler EditTaskHandlerCallback;
        public EventHandler ConfigHeSoHandlerCallback;
        public EventHandler<Guid> ReminderHandlerCallback;
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
        protected bool IsView => this.CURRENT_PAGE.IsView;
        protected bool IsEdit => this.CURRENT_PAGE.IsEdit;
        protected bool IsAdd => this.CURRENT_PAGE.IsAdd;
        protected bool IsDelete => this.CURRENT_PAGE.IsDelete;
        protected bool IsPM => this.CURRENT_PAGE.IsPM;
        protected bool IsAdministrator => this.CURRENT_PAGE.IsAdministrator;
        protected Dictionary<Guid, string> _dictTaskCodes = new Dictionary<Guid, string>();
        private static Dictionary<Guid, TblDoUuTien> _dictPriorities = new Dictionary<Guid, TblDoUuTien>();
        protected readonly ControlHelpers _controlHelpers = new ControlHelpers();

        protected void Page_Load(object sender, EventArgs e)
        {
            ((CtrlChonNhanVienTask)CtrlChonNhanVienTask1).OnAssignConfirmed += CtrlChonNhanVienTask1_OnAssignConfirmed;
            if (!IsPostBack)
                InitControls();
            CtrlSwapPhase1.SwapSuccessCallback = () => { this.Rebind(); };
            if (CtrlFastCompleteTask1 != null)
                CtrlFastCompleteTask1.ActionCompleted += (s, ev) => { Rebind(); };
            if (CtrlStartTask1 != null)
                CtrlStartTask1.ActionStarted += (s, ev) => { Rebind(); };
            CtrlAddPhase1.SavedSuccess += (s, ev) => { Rebind(); };
            CtrlAddSubTask1.SavedSuccess += (s, ev) => { Rebind(); };
        }
        public void InitControls()
        {
            ApplyControlsText();
            if (_dictPriorities.Count == 0)
                _dictPriorities = TaskManager.Instance.GetDictPriorities();
            lbtAdd.Visible = this.CURRENT_PAGE.IsAdd;
            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
            Rebind();
        }
        private void CtrlChonNhanVienTask1_OnAssignConfirmed(object sender, EventArgs e)
        {
            Rebind();
            upMain.Update();
            ShowNotify(GetResourceText(BackEndResourceKeys.ASSIGN_TASK_SUCCESS), MSGType.Success);
        }
        public void Rebind()
        {
            DataTable dtTasks = new DataTable();
            _dictTaskCodes.Clear();
            int overdueCount = 0;
            string searchValue = txtSearchSingle.Text.Trim();
            (dtTasks, _dictTaskCodes, overdueCount) = TaskManager.Instance.GetDictTasksAndCountOverdue(this.ProjectId, searchValue, IsPM || IsAdministrator);
            if (dtTasks != null && dtTasks.Rows.Count > 0)
            {
                if (!dtTasks.Columns.Contains("ReminderCount"))
                    dtTasks.Columns.Add("ReminderCount", typeof(int));

                foreach (DataRow row in dtTasks.Rows)
                    row["ReminderCount"] = 0;

                Guid currentUserId = SweetContext.Current != null? SweetContext.Current.UserId  : Guid.Empty;

                if (this.ProjectId != Guid.Empty && currentUserId != Guid.Empty)
                {
                    DataTable reminderSummary =
                        NhacViecLichCongViecManager.Instance.GetPendingTaskSummary(
                            this.ProjectId,
                            currentUserId);
                    if (reminderSummary != null && reminderSummary.Rows.Count > 0)
                    {
                        Dictionary<Guid, int> reminderCounts =
                            new Dictionary<Guid, int>();
                        foreach (DataRow reminderRow in reminderSummary.Rows)
                        {
                            if (!Guid.TryParse(
                                    reminderRow["IdCongViec"]?.ToString(),
                                    out Guid taskId))
                                continue;
                            int count = 0;
                            if (reminderRow["ReminderCount"] != DBNull.Value)
                                count = Convert.ToInt32(reminderRow["ReminderCount"]);
                            reminderCounts[taskId] = count;
                        }
                        foreach (DataRow taskRow in dtTasks.Rows)
                        {
                            if (!Guid.TryParse(
                                    taskRow["IdCongViec"]?.ToString(),
                                    out Guid taskId))
                                continue;

                            if (reminderCounts.TryGetValue(taskId, out int count))
                                taskRow["ReminderCount"] = count;
                        }
                    }
                }
            }
            lblOverdueCount.InnerText = overdueCount.ToString();
            if (dtTasks == null || dtTasks.Rows.Count == 0)
            {
                grvData.Visible = false;
                pnlNoTask.Visible = true;
            }
            else
            {
                grvData.Visible = true;
                pnlNoTask.Visible = false;
            }
            grvData.DataSource = dtTasks;
            grvData.DataBind();
            upMain.Update();
        }
        public void btnSearch_ServerClick(object sender, EventArgs e)
        {
            Rebind();
        }
        private void ApplyControlsText()
        {
            txtSearchSingle.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            lbtAdd.ToolTip = lbtAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);
            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.TASK_NAME),
                GetResourceText(BackEndResourceKeys.ASSIGNEE),
                GetResourceText(BackEndResourceKeys.DURATION),
                GetResourceText(BackEndResourceKeys.START_DATE),
                GetResourceText(BackEndResourceKeys.EXPECTED_COMPLETION_DATE),
                GetResourceText(BackEndResourceKeys.ACTUAL_COMPLETION_DATE),
                GetResourceText(BackEndResourceKeys.STATUS),
                GetResourceText(BackEndResourceKeys.DEPENDENT),
                GetResourceText(BackEndResourceKeys.ACTION)
            };
            grvData.HeaderTexts = lstTableHeader;
        }

        #region Security helper
        private Guid GetCurrentUserId()
        {
            string[] sessionKeys = { "UserId", "UserID", "ID", "AccountId", "account_id" };
            foreach (string key in sessionKeys)
            {
                object value = Session[key];
                if (value != null && Guid.TryParse(value.ToString(), out Guid userId))
                    return userId;
            }
            return Guid.Empty;
        }
        private bool IsCurrentUserAssignedToTask(Guid taskId)
        {
            Guid currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
                return false;
            List<Guid> assignedIds = TaskManager.Instance.GetAssignedNhanVienIds(taskId);
            return assignedIds.Contains(currentUserId);
        }
        #endregion

        #region Gridview Events
        protected void grvData_NeedDataSource(object sender, ExtraGridEventArg e)
        {
            Rebind();
        }
        protected void grvData_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "REMINDER":
                    if (Guid.TryParse(e.CommandArgument?.ToString(), out Guid reminderTaskId))
                    {
                        ReminderHandlerCallback?.Invoke(this, reminderTaskId);
                    }
                    break;
                case "ASSIGN_TASK":
                    if (!this.CURRENT_PAGE.IsEdit && !this.CURRENT_PAGE.IsView)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    int rowIndexAssign = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndexAssign = ((GridViewRow)((WebControl)e.CommandSource).NamingContainer).RowIndex;
                    else
                        rowIndexAssign = Convert.ToInt32(e.CommandArgument);
                    Guid taskIdAssign = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndexAssign].Value.ToString(), out taskIdAssign))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    TblCongViec task = TaskManager.Instance.FetchById(taskIdAssign);
                    if (task == null)
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    if (!task.NgayBatDau.HasValue || !task.NgayKetThuc.HasValue)
                    {
                        ShowNotify(GetResourceText(BackEndResourceKeys.REQUIRE_TASK_DATES_BEFORE_ASSIGN), MSGType.Warning);
                        return;
                    }
                    Guid? pmId = DuAnManager.Instance.LayIdNhanVienQuanLy(this.ProjectId);
                    if (this.CURRENT_PAGE.IsEdit)
                    {
                        CtrlChonNhanVienTask1.ViewOnly = task.TrangThai == 2 || task.TrangThai == 3;
                        ((CtrlChonNhanVienTask)CtrlChonNhanVienTask1).OpenPicker(
                            this.ProjectId,
                            taskIdAssign,
                            task.NgayBatDau.Value,
                            task.NgayKetThuc.Value,
                            task.TenCongViec,
                            pmId,
                            task.IdCongViecCha);
                    }
                    else
                    {
                        ((CtrlXemNhanVienTask)CtrlXemNhanVienTask1).OpenModal(
                            taskIdAssign,
                            task.NgayBatDau.Value,
                            task.NgayKetThuc.Value,
                            task.TenCongViec,
                            pmId);
                    }
                    break;

                case "VIEW_SCHEDULE":
                    if (!this.CURRENT_PAGE.IsView)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    int rowIndexSched = (e.CommandSource.GetType() != typeof(GridviewExtension))
                        ? ((GridViewRow)((WebControl)e.CommandSource).NamingContainer).RowIndex
                        : Convert.ToInt32(e.CommandArgument);
                    Guid taskIdSched = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndexSched].Value.ToString(), out taskIdSched))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    TblCongViec taskSched = TaskManager.Instance.FetchById(taskIdSched);
                    if (taskSched == null)
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    if (!taskSched.NgayBatDau.HasValue || !taskSched.NgayKetThuc.HasValue)
                    {
                        ShowNotify("Công việc này chưa có Ngày bắt đầu và Ngày kết thúc để vẽ lịch biểu!", MSGType.Warning);
                        return;
                    }
                    List<TblCongViec> leafTasks = TaskManager.Instance.GetLeafTasksForSchedule(taskIdSched);
                    var dict = new Dictionary<string, object>();
                    DateTime current = taskSched.NgayBatDau.Value.Date;
                    DateTime end = taskSched.NgayKetThuc.Value.Date;
                    string txtTask = GetResourceText(BackEndResourceKeys.TASK).ToLower();
                    string txtFree = GetResourceText(BackEndResourceKeys.FREE);
                    string txtHoliday = GetResourceText(BackEndResourceKeys.HOLIDAY);
                    string txtWeekend = GetResourceText(BackEndResourceKeys.WEEKEND);
                    while (current <= end)
                    {
                        string status = "";
                        var activeTasks = new List<object>();
                        string displayText = "";
                        TblLichNgoaiLe exceptionDay = LichBieuChungManager.Instance.GetExceptionByDate(current);
                        bool isWeekend = current.DayOfWeek == DayOfWeek.Sunday || current.DayOfWeek == DayOfWeek.Saturday;
                        if (exceptionDay != null)
                        {
                            status = "holiday";
                            displayText = exceptionDay.TenNgoaiLe;
                        }
                        else if (isWeekend)
                        {
                            status = "weekend";
                            displayText = txtWeekend;
                        }
                        else
                        {
                            var tasksToday = leafTasks.Where(t => t.NgayBatDau.Value.Date <= current.Date && t.NgayKetThuc.Value.Date >= current.Date).ToList();
                            if (tasksToday.Count > 0)
                            {
                                status = "busy";
                                foreach (var t in tasksToday)
                                    activeTasks.Add(new { code = t.MaCongViec, name = t.TenCongViec });
                                displayText = tasksToday.Count == 1 ? $"[{tasksToday[0].MaCongViec}]" : $"{tasksToday.Count} {txtTask}";
                            }
                            else
                            {
                                status = "free";
                                displayText = txtFree;
                            }
                        }
                        dict.Add(current.ToString("yyyy-MM-dd"), new { status = status, text = displayText, tasks = activeTasks });
                        current = current.AddDays(1);
                    }
                    string jsonSchedule = Newtonsoft.Json.JsonConvert.SerializeObject(dict);
                    mdlTaskSchedule.Title = GetResourceText(BackEndResourceKeys.SCHEDULE_DETAILS) ?? "Chi tiết lịch biểu";
                    string startStr = taskSched.NgayBatDau.Value.ToString("dd/MM/yyyy");
                    string endStr = taskSched.NgayKetThuc.Value.ToString("dd/MM/yyyy");
                    ltrScheduleTaskName.Text = $"[{taskSched.MaCongViec}] {taskSched.TenCongViec} : {startStr} - {endStr}";
                    hdfSingleTaskScheduleJson.Value = jsonSchedule;
                    mdlTaskSchedule.OpenModal(true);
                    ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "RenderTaskScheduleJS",
                        "setTimeout(function() { CMSMasterJs.RenderSingleTaskSchedule(); }, 200);", true);
                    break;
                case "ITEM_ADD_CHILD":
                    if (!this.CURRENT_PAGE.IsAdd)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    int rowIndexAdd = (e.CommandSource.GetType() != typeof(GridviewExtension))
                        ? ((GridViewRow)((WebControl)e.CommandSource).NamingContainer).RowIndex
                        : Convert.ToInt32(e.CommandArgument);
                    Guid parentTaskId = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndexAdd].Value.ToString(), out parentTaskId))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    TblCongViec parentTask = TaskManager.Instance.FetchById(parentTaskId);
                    if (parentTask == null)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }
                    if (!this.IsPM && !this.IsAdministrator && !IsCurrentUserAssignedToTask(parentTaskId))
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    CtrlAddSubTask1.OpenModal(this.ProjectId, parentTaskId);
                    break;
                case "ITEM_VIEW":
                    if (!this.CURRENT_PAGE.IsView && !this.CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }

                    Guid taskIdView = Guid.Empty;
                    if (e.CommandArgument != null && Guid.TryParse(e.CommandArgument.ToString(), out taskIdView))
                    {
                        CtrlViewTaskDetail1.OpenModal(taskIdView);
                    }
                    else
                    {
                        int rowIndexView = 0;
                        if (e.CommandSource.GetType() != typeof(GridviewExtension))
                            rowIndexView = ((GridViewRow)((WebControl)e.CommandSource).NamingContainer).RowIndex;
                        else
                            rowIndexView = Convert.ToInt32(e.CommandArgument);

                        if (Guid.TryParse(grvData.DataKeys[rowIndexView].Value.ToString(), out taskIdView))
                        {
                            CtrlViewTaskDetail1.OpenModal(taskIdView);
                        }
                    }
                    break;
                case "ITEM_DETAIL":
                    if (!this.CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    int rowIndex = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndex = ((GridViewRow)((WebControl)e.CommandSource).NamingContainer).RowIndex;
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);
                    Guid taskId = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out taskId))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    if (EditTaskHandlerCallback != null)
                    {
                        EditTaskHandlerCallback(taskId, EventArgs.Empty);
                    }
                    break;
                case "FAST_COMPLETE":
                    if (!this.CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    int rowIndexComplete = (e.CommandSource.GetType() != typeof(GridviewExtension))
                        ? ((GridViewRow)((WebControl)e.CommandSource).NamingContainer).RowIndex
                        : Convert.ToInt32(e.CommandArgument);
                    if (Guid.TryParse(grvData.DataKeys[rowIndexComplete].Value.ToString(), out Guid taskIdComplete))
                    {
                        TblCongViec taskToComplete = TaskManager.Instance.FetchById(taskIdComplete);
                        if (taskToComplete != null && taskToComplete.IdCongViecCha.HasValue)
                            CtrlFastCompleteTask1.OpenModal(taskIdComplete, taskToComplete.MaCongViec, taskToComplete.TenCongViec);
                    }
                    break;

                case "START_TASK":
                    if (!this.CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    int rowIndexStart = (e.CommandSource.GetType() != typeof(GridviewExtension))
                        ? ((GridViewRow)((WebControl)e.CommandSource).NamingContainer).RowIndex
                        : Convert.ToInt32(e.CommandArgument);
                    if (Guid.TryParse(grvData.DataKeys[rowIndexStart].Value.ToString(), out Guid taskIdStart))
                    {
                        TblCongViec taskToStart = TaskManager.Instance.FetchById(taskIdStart);
                        if (taskToStart != null && !CheckIsFatherTask(taskToStart.IdCongViec))
                        {
                            if (taskToStart.IdCongViecPhuThuoc.HasValue)
                            {
                                TblCongViec dependentTask = TaskManager.Instance.FetchById(taskToStart.IdCongViecPhuThuoc.Value);
                                if (dependentTask != null && dependentTask.TrangThai != 2 && dependentTask.TrangThai != 3)
                                {
                                    ShowNotify($"Không thể bắt đầu! Công việc này phụ thuộc vào [{dependentTask.MaCongViec}] nhưng công việc đó chưa hoàn thành.", MSGType.Warning);
                                    return;
                                }
                            }
                            Guid? checkParentId = taskToStart.IdCongViecCha;
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
                                            ShowNotify($"Không thể bắt đầu! Giai đoạn cha [{pTask.MaCongViec}] đang bị kẹt phụ thuộc vào [{pDepTask.MaCongViec}] chưa hoàn thành.", MSGType.Warning);
                                            return;
                                        }
                                    }
                                    checkParentId = pTask.IdCongViecCha;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            CtrlStartTask1.OpenModal(taskIdStart, taskToStart.MaCongViec, taskToStart.TenCongViec);
                        }
                    }
                    break;

                case "ITEM_DELETE":
                    if (!this.CURRENT_PAGE.IsDelete)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    rowIndex = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndex = ((GridViewRow)((WebControl)e.CommandSource).NamingContainer).RowIndex;
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);
                    taskId = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out taskId))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    TblCongViec taskToDelete = TaskManager.Instance.FetchById(taskId);
                    if (taskToDelete == null)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }
                    hfDeletingTaskId.Value = taskToDelete.IdCongViec.ToString();
                    ConfirmResult result = new ConfirmResult();
                    result.CommandName = "TASK_DELETE";
                    this.CURRENT_PAGE.CurrentConfirmResult = result;
                    MessageBox msg = new MessageBox(
                        GetResourceText(BackEndResourceKeys.NOTIFICATION),
                        string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THE_DATA), taskToDelete.TenCongViec),
                        MSGButton.DeleteCancel,
                        MSGIcon.Error);
                    OpenMessageBox(msg, result, false, false);
                    break;
            }
        }
        protected void grvData_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView rowView = (DataRowView)e.Row.DataItem;
                string maCv = rowView["MaCongViec"]?.ToString() ?? "";
                int level = maCv.TrimEnd('.').Split('.').Length;
                int finalSeverity = 0;
                if (rowView["NgayKetThuc"] != DBNull.Value && rowView["TrangThai"] != DBNull.Value)
                {
                    DateTime ngayKt = Convert.ToDateTime(rowView["NgayKetThuc"]);
                    byte tThai = Convert.ToByte(rowView["TrangThai"]);
                    if (tThai != 2 && tThai != 3)
                    {
                        double daysLeft = (ngayKt.Date - DateTime.Now.Date).TotalDays;
                        if (daysLeft < 0)
                            finalSeverity = 2;
                        else if (daysLeft >= 0 && daysLeft <= 2)
                            finalSeverity = 1;
                    }
                }
                e.Row.Attributes["data-code"] = maCv;
                e.Row.Attributes["data-level"] = level.ToString();
                e.Row.Attributes["data-overdue"] = finalSeverity == 2 ? "1" : "0";
                bool isRootPhase = level == 1 && (rowView["IdCongViecCha"] == DBNull.Value || string.IsNullOrEmpty(rowView["IdCongViecCha"]?.ToString()));
                if (isRootPhase && this.IsEdit)
                {
                    e.Row.Attributes["data-phase-row"] = "1";
                    e.Row.Attributes["data-phase-id"] = rowView["IdCongViec"].ToString();
                    e.Row.Attributes["draggable"] = "true";
                    e.Row.Attributes["title"] = "Kéo giai đoạn để đổi vị trí";
                    e.Row.CssClass += " phase-draggable-row";
                }
                if (finalSeverity == 2)
                    e.Row.CssClass += " row-overdue-bg";
                else if (finalSeverity == 1)
                    e.Row.CssClass += " row-warning-bg";
                LinkButton lbtAssign = (LinkButton)e.Row.FindControl("lbtAssign");
                if (lbtAssign != null)
                    lbtAssign.Visible = this.IsEdit || this.IsView;
            }
        }
        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlSwapPhase1.ConfirmRequest(e);
            if (e != null && e.Submit &&
                e.CommandName != "CONFIRM_SWAP_PHASES" &&
                e.CommandName != "CONFIRM_REORDER_PHASES")
            {
                Guid taskId = Guid.Empty;
                if (!Guid.TryParse(hfDeletingTaskId.Value, out taskId))
                {
                    ShowInvalidDataError();
                    return;
                }
                TblCongViec task = TaskManager.Instance.FetchById(taskId);
                if (task == null)
                {
                    ShowInvalidNotFoundData();
                    return;
                }
                try
                {
                    TaskManager.Instance.DeleteTask(task);
                    if (task.IdCongViecCha.HasValue)
                    {
                        if (_dictPriorities == null || _dictPriorities.Count == 0)
                            _dictPriorities = TaskManager.Instance.GetDictPriorities();
                        TaskManager.Instance.AutoSetParentPriority(this.ProjectId, task.IdCongViecCha.Value, _dictPriorities);
                        TaskManager.Instance.AutoSetParentTime(this.ProjectId, task.IdCongViecCha.Value);
                        TaskManager.Instance.AutoSetParentStatus(this.ProjectId, task.IdCongViecCha.Value);
                    }
                    hfDeletingTaskId.Value = string.Empty;
                    ShowSuccessDeleteData();
                    grvData.CurrentPageIndex = 1;
                    Rebind();
                }
                catch (Exception exc)
                {
                    ShowNotify(exc.Message, MSGType.Error);
                }
            }
        }
        #endregion

        #region Buttons
        protected void lbtAdd_Click(object sender, EventArgs e)
        {
            CtrlAddPhase1.OpenModal(this.ProjectId);
        }
        #endregion

        #region Formatters
        public string GetFormattedTaskName(object macv, object tencv)
        {
            return _controlHelpers.GetFormattedTaskName(macv, tencv);
        }
        public string GetTaskPriorityBadge(object tenDoUuTien, object diemDoUuTien)
        {
            return _controlHelpers.GetTaskPriorityBadge(tenDoUuTien, diemDoUuTien);
        }
        public string GetPhuThuoc(object idPhuThuocObj)
        {
            if (idPhuThuocObj != null && Guid.TryParse(idPhuThuocObj.ToString(), out Guid predId))
            {
                if (_dictTaskCodes.ContainsKey(predId) && !string.IsNullOrEmpty(_dictTaskCodes[predId]))
                    return _dictTaskCodes[predId];
            }
            return "—";
        }
        public string FormatDateTime(object date)
        {
            return _controlHelpers.FormatDateTime(date);
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
                bool isDefaultAvatar = avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);
                if (!string.IsNullOrEmpty(avatar) && !isDefaultAvatar)
                {
                    string avatarUrl = avatar.StartsWith("~") ? Page.ResolveUrl(avatar) : avatar;
                    string fallbackHtml = $"<div class=\\'avatar-circle\\' style=\\'background-color: {color};\\' title=\\'{name}\\'>{GetInitials(name)}</div>";
                    html += $"<img src='{avatarUrl}' class='avatar-circle' style='object-fit: cover;' title='{name}' onerror=\"this.onerror=null; this.outerHTML='{fallbackHtml}';\" />";
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
        public string GetTaskStatusTextOnly(object statusObj)
        {
            int status = Convert.ToInt32(statusObj);
            TrangThaiCongViec enumStatus = (TrangThaiCongViec)status;
            string resourceKey = TaskManager.Instance.GetValueForTrangThaiCongViec(enumStatus);
            return GetResourceText(resourceKey);
        }
        public bool CheckIsFatherTask(object idCongViecObj)
        {
            if (idCongViecObj == null || !Guid.TryParse(idCongViecObj.ToString(), out Guid id)) return false;
            TblCongViec task = TaskManager.Instance.FetchById(id);
            if (task == null) return false;
            return TaskManager.Instance.CheckHasChildTasks(this.ProjectId, task);
        }
        public string GetTaskStatusBadge(object statusObj)
        {
            int status = Convert.ToInt32(statusObj);
            TrangThaiCongViec enumStatus = (TrangThaiCongViec)status;
            string resourceKey = TaskManager.Instance.GetValueForTrangThaiCongViec(enumStatus);
            string statusText = GetResourceText(resourceKey);
            switch (status)
            {
                case 1:
                    return $"<span class=\"badge-pill-custom badge-status-doing\">{statusText}</span>";
                case 2:
                    return $"<span class=\"badge-pill-custom badge-status-done\">{statusText}</span>";
                case 3:
                    string lateText = GetResourceText(BackEndResourceKeys.OVERDUE);
                    return $"<span class=\"badge-pill-custom badge-status-done\">{statusText}</span>" +
                           $"<span style='display:block; font-size:11px; color:#dc2626; font-weight:bold; margin-top:2px;'>({lateText})</span>";
                case 0:
                default:
                    return $"<span class=\"badge-pill-custom badge-status-todo\">{statusText}</span>";
            }
        }
        protected void lbtApplyPhaseReorder_Click(object sender, EventArgs e)
        {
            if (!this.CURRENT_PAGE.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }
            if (!Guid.TryParse(hfDragPhaseId.Value, out Guid phaseId) || !Guid.TryParse(hfDragTargetPhaseId.Value, out Guid targetPhaseId))
            {
                hfDragPhaseId.Value = string.Empty;
                hfDragTargetPhaseId.Value = string.Empty;
                hfDragDropPosition.Value = string.Empty;
                ShowInvalidDataError();
                return;
            }
            string dropPosition = hfDragDropPosition.Value;
            if (!string.Equals(dropPosition, "before", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(dropPosition, "after", StringComparison.OrdinalIgnoreCase))
                dropPosition = "before";
            try
            {
                CtrlSwapPhase1.OpenReorderOptions(this.ProjectId, phaseId, targetPhaseId, dropPosition);
                hfDragPhaseId.Value = string.Empty;
                hfDragTargetPhaseId.Value = string.Empty;
                hfDragDropPosition.Value = string.Empty;
            }
            catch (Exception ex)
            {
                hfDragPhaseId.Value = string.Empty;
                hfDragTargetPhaseId.Value = string.Empty;
                hfDragDropPosition.Value = string.Empty;
                ShowNotify(ex.Message, MSGType.Error);
            }
        }
        protected void lbtConfigHeSo_Click(object sender, EventArgs e)
        {
            // Bắn sự kiện ra Trang Chính (TaskList.aspx) để nó mở Popup
            if (ConfigHeSoHandlerCallback != null)
                ConfigHeSoHandlerCallback(sender, e);
        }
        #endregion
    }
}