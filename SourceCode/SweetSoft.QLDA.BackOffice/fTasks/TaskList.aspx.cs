using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fTasks.Controls;
using SweetSoft.QLDA.BackOffice.fUsers.Controls;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using static SweetSoft.QLDA.Core.Managers.TaskManager;

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
            CtrlTask1.NewTaskHandlerCallback = NewTask_Callback;
            CtrlTask1.NewSubTaskHandlerCallback = NewSubTask_Callback;
            CtrlTask1.EditTaskHandlerCallback = EditTask_Callback;

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
                {
                    _dictPriorities = TaskManager.Instance.GetDictPriorities();
                }
            }
        }

        #region Callbacks mở Popup
        private void NewTask_Callback(object sender, EventArgs e)
        {
            _controlHelpers.ClearControlValues(upModal.Controls);
            hfEditTaskId.Value = string.Empty;
            mdlEditTask.Title = "Thêm giai đoạn mới";

            string maCV = TaskManager.Instance.GenerateNewTaskCode(CurrentProjectId, null);
            txtEditMaCv.Text = maCV;
            txtEditGiaiDoan.Text = "Giai đoạn mới (Gốc)";
            txtEditTenCv.Text = string.Empty;
            txtEditMoTa.Text = string.Empty;
            txtEditThoiHan.Text = "1";
            txtEditNgayBatDau.Text = DateTime.Today.ToString("yyyy-MM-dd");
            txtEditNgayKetThuc.Text = DateTime.Today.ToString("yyyy-MM-dd");

            _controlHelpers.BindParentTasks(ddlEditCongViecCha, CurrentProjectId);

            _controlHelpers.BindDependentTasks(ddlEditPhuThuoc, CurrentProjectId, currentOrNewCode: maCV, chiLayGiaiDoan: true);

            _controlHelpers.BindPriorities(ddlEditDoUuTien);
            _controlHelpers.BindTaskStatus(ddlEditTrangThai, 0);

            ddlEditTrangThai.SelectedValue = "0";
            ddlEditDoUuTien.SelectedIndex = 0;

            SetFormControlsState(isPhase: true, hasChildren: false);
            ddlEditCongViecCha.Enabled = false;

            UpdateMinStartDate();

            upModal.Update();
            mdlEditTask.OpenModal(true);
        }

        private void NewSubTask_Callback(object sender, Guid parentId)
        {
            _controlHelpers.ClearControlValues(upModal.Controls);
            hfEditTaskId.Value = string.Empty;
            mdlEditTask.Title = "Thêm công việc con";

            TblCongViec parentTask = TaskManager.Instance.FetchById(parentId);
            if (parentTask == null) return;

            string maCV = TaskManager.Instance.GenerateNewTaskCode(CurrentProjectId, parentId);
            txtEditMaCv.Text = maCV;
            txtEditGiaiDoan.Text = TaskManager.Instance.GetRootPhaseName(CurrentProjectId, parentId);
            txtEditTenCv.Text = string.Empty;
            txtEditMoTa.Text = string.Empty;
            txtEditThoiHan.Text = "1";
            txtEditNgayBatDau.Text = parentTask.NgayBatDau.HasValue ? parentTask.NgayBatDau.Value.ToString("yyyy-MM-dd") : DateTime.Today.ToString("yyyy-MM-dd");
            txtEditNgayKetThuc.Text = txtEditNgayBatDau.Text;

            _controlHelpers.BindParentTasks(ddlEditCongViecCha, CurrentProjectId);
            _controlHelpers.BindDependentTasks(ddlEditPhuThuoc, CurrentProjectId, currentOrNewCode: maCV);
            _controlHelpers.BindPriorities(ddlEditDoUuTien);
            _controlHelpers.BindTaskStatus(ddlEditTrangThai, 0);

            if (ddlEditCongViecCha.Items.FindByValue(parentId.ToString()) != null)
            {
                ddlEditCongViecCha.SelectedValue = parentId.ToString();
            }
            ddlEditTrangThai.SelectedValue = "0";

            SetFormControlsState(isPhase: false, hasChildren: false);

            ddlEditCongViecCha.Enabled = false;

            UpdateMinStartDate();

            upModal.Update();
            mdlEditTask.OpenModal(true);
        }
        private void EditTask_Callback(object sender, EventArgs e)
        {
            Guid taskId = (Guid)sender;
            TblCongViec task = TaskManager.Instance.FetchById(taskId);
            if (task == null || task.DaXoa == true) return;

            hfEditTaskId.Value = task.IdCongViec.ToString();
            mdlEditTask.Title = this.IsEdit ? GetResourceText(BackEndResourceKeys.EDIT) : GetResourceText(BackEndResourceKeys.DETAIL);
            txtEditMaCv.Text = task.MaCongViec;
            txtEditTenCv.Text = task.TenCongViec;
            txtEditGiaiDoan.Text = TaskManager.Instance.GetRootPhaseName(CurrentProjectId, task.IdCongViecCha);
            txtEditMoTa.Text = task.MoTa;
            txtEditThoiHan.Text = task.ThoiHanNgay.HasValue ? task.ThoiHanNgay.ToString() : "";
            txtEditNgayBatDau.Text = task.NgayBatDau.HasValue ? task.NgayBatDau.Value.ToString("yyyy-MM-dd") : "";
            txtEditNgayKetThuc.Text = task.NgayKetThuc.HasValue ? task.NgayKetThuc.Value.ToString("yyyy-MM-dd") : "";

            bool isPhase = TaskManager.Instance.CheckPhase(task);
            bool hasChildren = TaskManager.Instance.CheckHasChildTasks(CurrentProjectId, task);

            SetFormControlsState(isPhase, hasChildren);

            _controlHelpers.BindParentTasks(ddlEditCongViecCha, CurrentProjectId, task.IdCongViec, task.IdCongViecCha);
            bool laGiaiDoanGoc = !task.IdCongViecCha.HasValue;
            _controlHelpers.BindDependentTasks(ddlEditPhuThuoc, CurrentProjectId, task.IdCongViec, task.IdCongViecPhuThuoc, task.MaCongViec, chiLayGiaiDoan: laGiaiDoanGoc);

            if (!task.IdCongViecCha.HasValue)
            {
                List<ListItem> invalidItems = new List<ListItem>();
                foreach (ListItem item in ddlEditPhuThuoc.Items)
                {
                    if (Guid.TryParse(item.Value, out Guid depId))
                    {
                        var depTask = TaskManager.Instance.FetchById(depId);
                        if (depTask != null && depTask.IdCongViecCha.HasValue)
                        {
                            invalidItems.Add(item);
                        }
                    }
                }
                foreach (var item in invalidItems) ddlEditPhuThuoc.Items.Remove(item);
            }

            _controlHelpers.BindPriorities(ddlEditDoUuTien, task.IdDoUuTien);
            _controlHelpers.BindTaskStatus(ddlEditTrangThai, task.TrangThai);
            UpdateMinStartDate();

            byte trangThaiTask = task.TrangThai;

            ddlEditCongViecCha.Enabled = false;
            ddlEditPhuThuoc.Enabled = false;

            if (trangThaiTask == 1 || trangThaiTask == 2)
            {
                txtEditNgayBatDau.Enabled = false;

                ListItem itemChuaBatDau = ddlEditTrangThai.Items.FindByValue("0");
                if (itemChuaBatDau != null)
                {
                    itemChuaBatDau.Enabled = false;
                }
            }

            upModal.Update();
            mdlEditTask.OpenModal(true);
        }
        #endregion

        #region Postbacks & Save
        protected void btnSaveTask_Click(object sender, EventArgs e)
        {
            try
            {
                DuAnManager.Instance.EnsureCanModifyStructure(CurrentProjectId);
            }
            catch (Exception ex)
            {
                ShowNotify(ex.Message, MSGType.Error);
                return;
            }

            bool isAddNew = string.IsNullOrEmpty(hfEditTaskId.Value);
            if (isAddNew && !this.IsAdd) { ShowNotify(GetResourceText(BackEndResourceKeys.NO_PERMISSION_ADD), MSGType.Error); return; }
            if (!isAddNew && !this.IsEdit) { ShowNotify(GetResourceText(BackEndResourceKeys.NO_PERMISSION_EDIT), MSGType.Error); return; }

            TblCongViec task;
            bool isPhase = false, isFatherTask = false;
            byte? oldStatus = null;

            if (isAddNew)
            {
                task = new TblCongViec { IdCongViec = Guid.NewGuid(), IdDuAn = CurrentProjectId, DaXoa = false, NgayTao = DateTime.Now };
            }
            else
            {
                if (!Guid.TryParse(hfEditTaskId.Value, out Guid taskId)) return;
                task = TaskManager.Instance.FetchById(taskId);
                if (task == null) return;
                isPhase = TaskManager.Instance.CheckPhase(task);
                isFatherTask = TaskManager.Instance.CheckHasChildTasks(CurrentProjectId, task);
                oldStatus = task.TrangThai;
            }

            string tenCv = txtEditTenCv.Text.Trim();
            if (string.IsNullOrEmpty(tenCv)) { ShowNotify(GetResourceText(BackEndResourceKeys.CAN_NOT_BE_BLANK), MSGType.Error); return; }

            Guid? idCha = null;
            Guid? idPhuThuoc = null;

            if (isAddNew)
            {
                idCha = Guid.TryParse(ddlEditCongViecCha.SelectedValue, out Guid cId) ? (Guid?)cId : null;
                idPhuThuoc = Guid.TryParse(ddlEditPhuThuoc.SelectedValue, out Guid ptId) ? (Guid?)ptId : null;

                isPhase = !idCha.HasValue;
                isFatherTask = false;
            }
            else
            {
                idCha = task.IdCongViecCha;
                idPhuThuoc = task.IdCongViecPhuThuoc;
            }

            task.IdCongViecCha = idCha;
            task.IdCongViecPhuThuoc = idPhuThuoc;
            if (isAddNew) task.MaCongViec = TaskManager.Instance.GenerateNewTaskCode(CurrentProjectId, idCha);

            DateTime ngayBd;
            if (!isAddNew && (oldStatus == 1 || oldStatus == 2))
            {
                ngayBd = task.NgayBatDau ?? DateTime.Today;
            }
            else
            {
                if (!DateTime.TryParse(txtEditNgayBatDau.Text.Trim(), out ngayBd))
                {
                    ShowNotify(GetResourceText(BackEndResourceKeys.CAN_NOT_BE_BLANK), MSGType.Error);
                    return;
                }
                var (minStartAllowed, limitReason) = TaskManager.Instance.GetMinStartDate(task.IdCongViecCha, task.IdCongViecPhuThuoc);
                if (minStartAllowed.HasValue && ngayBd.Date < minStartAllowed.Value.Date)
                {
                    ShowNotify(string.Format(GetResourceText(BackEndResourceKeys.INVALID_START_DATE_LIMIT), minStartAllowed.Value.ToString("dd/MM/yyyy"), limitReason), MSGType.Error);
                    return;
                }
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

                if (newStatus != 0 && idPhuThuoc.HasValue)
                {
                    TblCongViec dependentTask = TaskManager.Instance.FetchById(idPhuThuoc.Value);
                    if (dependentTask != null && dependentTask.TrangThai != 2)
                    {
                        ShowNotify($"Không thể thực hiện! Công việc này phụ thuộc vào [{dependentTask.MaCongViec}] nhưng công việc đó chưa hoàn thành.", MSGType.Warning);
                        return;
                    }
                }

                if (newStatus != 0)
                {
                    Guid? checkParentId = idCha;
                    while (checkParentId.HasValue)
                    {
                        TblCongViec pTask = TaskManager.Instance.FetchById(checkParentId.Value);
                        if (pTask != null)
                        {
                            if (pTask.IdCongViecPhuThuoc.HasValue)
                            {
                                TblCongViec pDepTask = TaskManager.Instance.FetchById(pTask.IdCongViecPhuThuoc.Value);
                                if (pDepTask != null && pDepTask.TrangThai != 2)
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

                if (!isAddNew && (oldStatus == 1 || oldStatus == 2) && newStatus == 0)
                {
                    ShowNotify("Không thể chuyển công việc đang làm hoặc đã hoàn thành về trạng thái Chưa bắt đầu!", MSGType.Warning);
                    return;
                }

                task.TrangThai = newStatus;

                if (isAddNew && newStatus != 0 || !isAddNew && oldStatus != newStatus)
                {
                    try
                    {
                        DuAnManager.Instance.EnsureCanUpdateProgress(CurrentProjectId);
                    }
                    catch (Exception ex)
                    {
                        ShowNotify(ex.Message, MSGType.Error);
                        return;
                    }
                }

                if (!isFatherTask) task.IdDoUuTien = Guid.TryParse(ddlEditDoUuTien.SelectedValue, out Guid idUt) ? (Guid?)idUt : null;

                task.ThoiHanNgay = thoiHan;
                task.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(ngayBd, thoiHan);

                if (newStatus == 2)
                {
                    if (oldStatus != 2 || !task.NgayHoanThanhThucTe.HasValue)
                    {
                        task.NgayHoanThanhThucTe = DateTime.Now;
                    }
                }
                else
                {
                    task.NgayHoanThanhThucTe = null;
                }
            }
            else
            {
                if (isAddNew)
                {
                    task.TrangThai = 0;
                    task.ThoiHanNgay = 1;
                    task.IdDoUuTien = Guid.TryParse(ddlEditDoUuTien.SelectedValue, out Guid idUt) ? (Guid?)idUt : null;
                    task.NgayKetThuc = ngayBd;
                }
            }

            task.TenCongViec = tenCv;
            task.MoTa = txtEditMoTa.Text.Trim();
            task.NgayCapNhat = DateTime.Now;
            task.Save();

            if (isAddNew)
            {
                if (task.IdCongViecCha.HasValue)
                {
                    TaskManager.Instance.AutoSetParentPriority(CurrentProjectId, task.IdCongViecCha.Value, _dictPriorities);
                    TaskManager.Instance.AutoSetParentTime(CurrentProjectId, task.IdCongViecCha.Value);
                    TaskManager.Instance.AutoSetParentStatus(CurrentProjectId, task.IdCongViecCha.Value);
                }
            }
            else
            {
                if (isFatherTask)
                {
                    TaskManager.Instance.AutoSetFirstChildStartTime(CurrentProjectId, task.IdCongViec, task.NgayBatDau.Value);
                }
                else
                {
                    TaskManager.Instance.AutoSetDependentTime(CurrentProjectId, task.IdCongViec);
                    if (task.IdCongViecCha.HasValue)
                    {
                        TaskManager.Instance.AutoSetParentPriority(CurrentProjectId, task.IdCongViecCha.Value, _dictPriorities);
                        TaskManager.Instance.AutoSetParentTime(CurrentProjectId, task.IdCongViecCha.Value);
                        TaskManager.Instance.AutoSetParentStatus(CurrentProjectId, task.IdCongViecCha.Value);
                    }
                }
            }

            ShowNotify(isAddNew ? GetResourceText(BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY) : GetResourceText(BackEndResourceKeys.DATA_HAS_BEEN_UPDATED_SUCCESSFULLY), MSGType.Success);
            CtrlTask1.Rebind();
            mdlEditTask.CloseModal();
        }

        protected void ddlEditCongViecChaSelected(object sender, EventArgs e)
        {
            Guid? parentId = Guid.TryParse(ddlEditCongViecCha.SelectedValue, out Guid pid) ? (Guid?)pid : null;
            bool isAddNew = string.IsNullOrEmpty(hfEditTaskId.Value);
            txtEditGiaiDoan.Text = TaskManager.Instance.GetRootPhaseName(CurrentProjectId, parentId);
            string targetCode = isAddNew ? TaskManager.Instance.GenerateNewTaskCode(CurrentProjectId, parentId) : txtEditMaCv.Text.Trim();
            if (isAddNew) txtEditMaCv.Text = targetCode;
            Guid? currentExcludeId = isAddNew ? (Guid?)null : (Guid.TryParse(hfEditTaskId.Value, out Guid tid) ? (Guid?)tid : null);
            _controlHelpers.BindDependentTasks(ddlEditPhuThuoc, CurrentProjectId, currentExcludeId, currentOrNewCode: targetCode);
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
        private void SetFormControlsState(bool isPhase, bool hasChildren)
        {
            bool canEdit = this.IsEdit;

            txtEditTenCv.Enabled = canEdit;
            txtEditMoTa.Enabled = canEdit;
            txtEditNgayBatDau.Enabled = canEdit;
            ddlEditCongViecCha.Enabled = canEdit;
            ddlEditPhuThuoc.Enabled = canEdit;

            ddlEditTrangThai.Enabled = canEdit && !isPhase && !hasChildren;
            txtEditThoiHan.Enabled = canEdit && !isPhase && !hasChildren;
            ddlEditDoUuTien.Enabled = canEdit && !isPhase && !hasChildren;
        }

        private void UpdateMinStartDate()
        {
            txtEditNgayBatDau.Attributes.Remove("min");
            Guid? parentId = Guid.TryParse(ddlEditCongViecCha.SelectedValue, out Guid pid) ? (Guid?)pid : null;
            Guid? depId = Guid.TryParse(ddlEditPhuThuoc.SelectedValue, out Guid did) ? (Guid?)did : null;
            var (minStartLimit, _) = TaskManager.Instance.GetMinStartDate(parentId, depId);

            if (minStartLimit.HasValue)
            {
                string minDateStr = minStartLimit.Value.ToString("yyyy-MM-dd");
                txtEditNgayBatDau.Attributes["min"] = minDateStr;
                if (DateTime.TryParse(txtEditNgayBatDau.Text.Trim(), out DateTime currentStartDate))
                {
                    if (currentStartDate.Date < minStartLimit.Value.Date)
                    {
                        txtEditNgayBatDau.Text = minDateStr;
                    }
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

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlTask1.ConfirmRequest(e);
        }
        #endregion
    }
}