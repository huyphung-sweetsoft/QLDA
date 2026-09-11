using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using static SweetSoft.QLDA.Core.Managers.TaskManager;

namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    public partial class CtrlTask : BaseAdminUserControl
    {
        public EventHandler NewTaskHandlerCallback;
        public EventHandler EditTaskHandlerCallback;
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
        protected bool IsDelete => this.CURRENT_PAGE.IsDelete;

        protected Dictionary<Guid, string> _dictTaskCodes = new Dictionary<Guid, string>();
        private static Dictionary<Guid, TblDoUuTien> _dictPriorities = new Dictionary<Guid, TblDoUuTien>();
        private readonly TaskManager _taskManager = TaskManager.Instance;
        protected readonly ControlHelpers _controlHelpers = new ControlHelpers();

        protected void Page_Load(object sender, EventArgs e)
        {
            // BẮT SỰ KIỆN GÁN THÀNH CÔNG TỪ MODAL MỚI
            ((CtrlChonNhanVienTask)CtrlChonNhanVienTask1).OnAssignConfirmed += CtrlChonNhanVienTask1_OnAssignConfirmed;
            if (!IsPostBack)
            {
                InitControls();
            }
        }

        public void InitControls()
        {
            ApplyControlsText();
            if (_dictPriorities.Count == 0)
            {
                _dictPriorities = _taskManager.GetDictPriorities();
            }
            lbtAdd.Visible = this.CURRENT_PAGE.IsAdd;
            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
            Rebind();
        }
        private void CtrlChonNhanVienTask1_OnAssignConfirmed(object sender, EventArgs e)
        {
            Rebind(); 
            upMain.Update();
            ShowNotify("Phân công nhân sự thành công!", MSGType.Success);
        }
        public void Rebind()
        {
            DataTable dtTasks = new DataTable();
            _dictTaskCodes.Clear();
            int overdueCount = 0;
            string searchValue=txtSearchSingle.Text.Trim();
            (dtTasks, _dictTaskCodes, overdueCount) = _taskManager.GetDictTasksAndCountOverdue(this.ProjectId, searchValue);

            lblOverdueCount.InnerText = overdueCount.ToString();
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
            lbtAdd.ToolTip = lbtAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);
            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.TASK_NAME),
                GetResourceText(BackEndResourceKeys.OWNER),
                GetResourceText(BackEndResourceKeys.DURATION),
                GetResourceText(BackEndResourceKeys.START_DATE),
                GetResourceText(BackEndResourceKeys.END_DATE),
                GetResourceText(BackEndResourceKeys.PRIORITY),
                GetResourceText(BackEndResourceKeys.STATUS),
                GetResourceText(BackEndResourceKeys.DEPENDENT),
                GetResourceText(BackEndResourceKeys.ACTION)
            };
            grvData.HeaderTexts = lstTableHeader;
        }

        #region Gridview Events
        protected void grvData_NeedDataSource(object sender, ExtraGridEventArg e)
        {
            Rebind();
        }

        protected void grvData_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "ASSIGN_TASK":
                    if (!this.CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    int rowIndexAssign = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndexAssign = ((GridViewRow)((WebControl)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndexAssign = Convert.ToInt32(e.CommandArgument);
                    Guid taskIdAssign = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndexAssign].Value.ToString(), out taskIdAssign))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    TblCongViec task = _taskManager.FetchById(taskIdAssign);
                    if (task == null)
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    if (!task.NgayBatDau.HasValue || !task.NgayKetThuc.HasValue)
                    {
                        ShowNotify("Vui lòng thiết lập 'Ngày bắt đầu' và 'Thời hạn' cho công việc trước khi phân công nhân sự!", MSGType.Warning);
                        return;
                    }
                    Guid? pmId = DuAnManager.Instance.LayIdNhanVienQuanLy(this.ProjectId);
                    ((CtrlChonNhanVienTask)CtrlChonNhanVienTask1).OpenPicker(this.ProjectId, taskIdAssign, task.NgayBatDau.Value, task.NgayKetThuc.Value, task.TenCongViec, pmId);
                    break;
                case "ITEM_DETAIL":
                    if (!this.CURRENT_PAGE.IsEdit && !this.CURRENT_PAGE.IsView)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    int rowIndex = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndex = ((GridViewRow)((WebControl)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);
                    Guid taskId = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out taskId))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    if (EditTaskHandlerCallback != null)
                        EditTaskHandlerCallback(taskId, EventArgs.Empty);
                    break;
                case "ITEM_DELETE":
                    if (!this.CURRENT_PAGE.IsDelete)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    rowIndex = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndex = ((GridViewRow)((WebControl)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);
                    taskId = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out taskId))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    TblCongViec taskToDelete = _taskManager.FetchById(taskId);
                    if (taskToDelete == null)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }
                    hfDeletingTaskId.Value = taskToDelete.IdCongViec.ToString();
                    ConfirmResult result = new ConfirmResult();
                    result.CommandName = "TASK_DELETE";
                    this.CURRENT_PAGE.CurrentConfirmResult = result;

                    MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION)
                        , string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THE_DATA), taskToDelete.TenCongViec)
                        , MSGButton.DeleteCancel, MSGIcon.Error);
                    OpenMessageBox(msg, result, false, false);
                    break;
            }
        }
        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null && e.Submit)
            {
                Guid taskId = Guid.Empty;
                if (!Guid.TryParse(hfDeletingTaskId.Value, out taskId))
                {
                    ShowInvalidDataError();
                    return;
                }
                TblCongViec task = _taskManager.FetchById(taskId);
                if (task == null)
                {
                    ShowInvalidNotFoundData();
                    return;
                }
                try
                {
                    _taskManager.DeleteTask(task);

                    if (task.IdCongViecCha.HasValue)
                    {
                        if (_dictPriorities == null || _dictPriorities.Count == 0)
                            _dictPriorities = _taskManager.GetDictPriorities();
                        _taskManager.AutoSetParentPriority(this.ProjectId, task.IdCongViecCha.Value, _dictPriorities);
                        _taskManager.AutoSetParentTime(this.ProjectId, task.IdCongViecCha.Value);
                        _taskManager.AutoSetParentStatus(this.ProjectId, task.IdCongViecCha.Value);
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
        protected void grvData_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView rowView = (DataRowView)e.Row.DataItem;
                string maCv = rowView[ColMaCv]?.ToString() ?? "";
                int level = maCv.Split('.').Length;
                bool isOverdue = _taskManager.CheckOverdue(rowView.Row);

                e.Row.Attributes["data-code"] = maCv;
                e.Row.Attributes["data-level"] = level.ToString();
                e.Row.Attributes["data-overdue"] = isOverdue ? "1" : "0";
                if (isOverdue)
                {
                    e.Row.CssClass += " row-overdue-bg";
                }
            }
        }
        #endregion

        #region Buttons
        protected void lbtAdd_Click(object sender, EventArgs e)
        {
            if (!this.CURRENT_PAGE.IsAdd)
            {
                ShowAccessDeniedNotify();
                return;
            }
            if (NewTaskHandlerCallback != null)
                NewTaskHandlerCallback(Guid.Empty, EventArgs.Empty);
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
        // Thay thế hàm GetAssigneeDisplay cũ
        public string GetAssigneeDisplay(object tenNhanVienObj, object avatarsObj)
        {
            string names = tenNhanVienObj?.ToString() ?? "";
            string avatars = avatarsObj?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(names)) return ""; // Không hiện gì nếu chưa có ai

            // Cắt chuỗi từ SQL
            string[] nameArray = names.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            string[] avatarArray = avatars.Split(new string[] { "," }, StringSplitOptions.None);

            string html = "";
            string[] colors = { "#f59e0b", "#3b82f6", "#10b981", "#8b5cf6", "#ec4899" };

            int maxDisplay = 2; // Số lượng Avatar hiển thị tối đa trước khi gộp thành "+N"
            int count = nameArray.Length;

            for (int i = 0; i < Math.Min(count, maxDisplay); i++)
            {
                string name = nameArray[i].Trim();
                string avatar = (i < avatarArray.Length) ? avatarArray[i].Trim() : "";
                string color = colors[i % colors.Length];
                // Kiểm tra xem avatar có phải là ảnh mặc định không (bắt cả trường hợp có dấu ~ hoặc khác hoa/thường)
                bool isDefaultAvatar = avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);
                // Nếu DB có lưu link Avatar 
                if (!string.IsNullOrEmpty(avatar) && !isDefaultAvatar)
                {
                    // ResolveUrl giúp chuyển đổi đường dẫn ảo (~) thành đường dẫn thực của Website
                    string avatarUrl = avatar.StartsWith("~") ? Page.ResolveUrl(avatar) : avatar;

                    // Xây dựng chuỗi Fallback (Hiện chữ) nếu ảnh bị hỏng/không load được
                    string fallbackHtml = $"<div class=\\'avatar-circle\\' style=\\'background-color: {color};\\' title=\\'{name}\\'>{GetInitials(name)}</div>";

                    // Vẽ thẻ img, sử dụng object-fit: cover để ảnh ko bị méo khi bo tròn
                    html += $"<img src='{avatarUrl}' class='avatar-circle' style='object-fit: cover;' title='{name}' onerror=\"this.onerror=null; this.outerHTML='{fallbackHtml}';\" />";
                }
                else
                {
                    // Nhân viên chưa có Avatar -> Hiện chữ cái
                    string initials = GetInitials(name);
                    html += $"<div class='avatar-circle' style='background-color: {color};' title='{name}'>{initials}</div>";
                }
            }

            if (count > maxDisplay)
            {
                html += $"<div class='avatar-circle avatar-more' title='Và {count - maxDisplay} người khác'>+{count - maxDisplay}</div>";
            }

            return html;
        }

        // Hàm lấy 1 hoặc 2 chữ cái đầu của tên (Giữ nguyên như bản cũ)
        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "";
            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[parts.Length - 2].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        public string GetTaskStatusBadge(object status)
        {
            return _controlHelpers.GetTaskStatusBadge(status);
        }
        #endregion
    }
}