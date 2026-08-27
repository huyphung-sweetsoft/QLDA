using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fUsers.Controls;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Web.UI;
using static SweetSoft.QLDA.Core.Managers.TaskManager;

namespace SweetSoft.QLDA.BackOffice.fProjects
{
    public partial class TaskList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.Task;
        private static Dictionary<Guid, TblDoUuTien> _dictPriorities = new Dictionary<Guid, TblDoUuTien>();
        private readonly TaskManager _taskManager = TaskManager.Instance;
        protected readonly ControlHelpers _controlHelpers = new ControlHelpers();

        protected void Page_Load(object sender, EventArgs e)
        {

            CtrlTask1.NewTaskHandlerCallback = NewTask_Callback;
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
                    _dictPriorities = _taskManager.GetDictPriorities();
                }
            }
        }

        #region Callbacks mở Popup
        private void NewTask_Callback(object sender, EventArgs e)
        {
            _controlHelpers.ClearControlValues(upModal.Controls);
            hfEditTaskId.Value = string.Empty;
            litModalTitle.Text = "Thêm mới công việc";

            string maCV = _taskManager.GenerateNewTaskCode(CurrentProjectId, null);
            txtEditMaCv.Text = maCV;
            txtEditGiaiDoan.Text = _taskManager.GetRootPhaseName(CurrentProjectId, null);
            txtEditTenCv.Text = string.Empty;
            txtEditMoTa.Text = string.Empty;
            txtEditThoiHan.Text = "1";
            txtEditNgayBatDau.Text = DateTime.Today.ToString("yyyy-MM-dd");
            txtEditNgayKetThuc.Text = DateTime.Today.ToString("yyyy-MM-dd");
            ddlEditTrangThai.SelectedValue = "0";

            SetFormControlsState(isPhase: false, hasChildren: false);

            _controlHelpers.BindParentTasks(ddlEditCongViecCha, CurrentProjectId);
            _controlHelpers.BindDependentTasks(ddlEditPhuThuoc, CurrentProjectId, currentOrNewCode: maCV);
            _controlHelpers.BindPriorities(ddlEditDoUuTien);
            _controlHelpers.BindProjectMembers(ddlEditNhanVien, CurrentProjectId);
            _controlHelpers.BindTaskStatus(ddlEditTrangThai, 0);
            UpdateMinStartDate();

            upModal.Update();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenAddModal", "openEditModal();", true);
        }

        private void EditTask_Callback(object sender, EventArgs e)
        {
            Guid taskId = (Guid)sender;
            TblCongViec task = _taskManager.FetchById(taskId);
            if (task == null || task.DaXoa == true) return;

            hfEditTaskId.Value = task.IdCongViec.ToString();
            litModalTitle.Text = this.IsEdit ? "Cập nhật thông tin công việc" : "Chi tiết công việc";
            txtEditMaCv.Text = task.MaCongViec;
            txtEditTenCv.Text = task.TenCongViec;
            txtEditGiaiDoan.Text = _taskManager.GetRootPhaseName(CurrentProjectId, task.IdCongViecCha);
            txtEditMoTa.Text = task.MoTa;
            txtEditThoiHan.Text = task.ThoiHanNgay.HasValue ? task.ThoiHanNgay.ToString() : "";
            txtEditNgayBatDau.Text = task.NgayBatDau.HasValue ? task.NgayBatDau.Value.ToString("yyyy-MM-dd") : "";
            txtEditNgayKetThuc.Text = task.NgayKetThuc.HasValue ? task.NgayKetThuc.Value.ToString("yyyy-MM-dd") : "";

            bool isPhase = _taskManager.CheckPhase(task);
            bool hasChildren = _taskManager.CheckHasChildTasks(CurrentProjectId, task);
            SetFormControlsState(isPhase, hasChildren);

            _controlHelpers.BindParentTasks(ddlEditCongViecCha, CurrentProjectId, task.IdCongViec, task.IdCongViecCha);
            _controlHelpers.BindDependentTasks(ddlEditPhuThuoc, CurrentProjectId, task.IdCongViec, task.IdCongViecPhuThuoc, task.MaCongViec);
            _controlHelpers.BindPriorities(ddlEditDoUuTien, task.IdDoUuTien);
            //Sua nay nua
            _controlHelpers.BindProjectMembers(ddlEditNhanVien, CurrentProjectId, null);
            _controlHelpers.BindTaskStatus(ddlEditTrangThai, task.TrangThai);
            if (!isPhase) UpdateMinStartDate();

            upModal.Update();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenEditModal", "openEditModal();", true);
        }
        #endregion

        #region Postbacks & Save
        protected void btnSaveTask_Click(object sender, EventArgs e)
        {
            bool isAddNew = string.IsNullOrEmpty(hfEditTaskId.Value);
            if (isAddNew && !this.IsAdd) { ShowAlert("Bạn không có quyền thêm mới công việc!"); return; }
            if (!isAddNew && !this.IsEdit) { ShowAlert("Bạn không có quyền chỉnh sửa công việc!"); return; }

            TblCongViec task;
            bool isPhase = false, isFatherTask = false;

            if (isAddNew)
            {
                task = new TblCongViec { IdCongViec = Guid.NewGuid(), IdDuAn = CurrentProjectId, DaXoa = false, NgayTao = DateTime.Now };
            }
            else
            {
                if (!Guid.TryParse(hfEditTaskId.Value, out Guid taskId)) return;
                task = _taskManager.FetchById(taskId);
                if (task == null) return;
                isPhase = _taskManager.CheckPhase(task);
                isFatherTask = _taskManager.CheckHasChildTasks(CurrentProjectId, task);
            }

            string tenCv = txtEditTenCv.Text.Trim();
            if (string.IsNullOrEmpty(tenCv)) { ShowAlert("Tên công việc không được để trống!"); return; }

            Guid? idCha = Guid.TryParse(ddlEditCongViecCha.SelectedValue, out Guid cId) ? (Guid?)cId : null;
            Guid? idPhuThuoc = Guid.TryParse(ddlEditPhuThuoc.SelectedValue, out Guid ptId) ? (Guid?)ptId : null;

            task.IdCongViecCha = idCha;
            task.IdCongViecPhuThuoc = idPhuThuoc;
            if (isAddNew) task.MaCongViec = _taskManager.GenerateNewTaskCode(CurrentProjectId, idCha);

            if (!isPhase)
            {
                if (!int.TryParse(txtEditThoiHan.Text.Trim(), out int thoiHan) || thoiHan <= 0)
                {
                    ShowAlert("Thời hạn công việc phải là số nguyên dương lớn hơn 0!");
                    return;
                }
                if (!DateTime.TryParse(txtEditNgayBatDau.Text.Trim(), out DateTime ngayBd))
                {
                    ShowAlert("Vui lòng chọn ngày bắt đầu công việc!");
                    return;
                }

                var (minStartAllowed, limitReason) = _taskManager.GetMinStartDate(task.IdCongViecCha, task.IdCongViecPhuThuoc);
                if (minStartAllowed.HasValue && ngayBd.Date < minStartAllowed.Value.Date)
                {
                    ShowAlert($"Ngày bắt đầu không hợp lệ! Phải từ ngày {minStartAllowed.Value:dd/MM/yyyy} trở đi (do {limitReason}).");
                    return;
                }
                //Phan nay can sua vi them bang moi
                //task.IdNhanVienPhuTrach = Guid.TryParse(ddlEditNhanVien.SelectedValue, out Guid idNv) ? (Guid?)idNv : null;
                task.TrangThai = Convert.ToByte(ddlEditTrangThai.SelectedValue);
                if (!isFatherTask) task.IdDoUuTien = Guid.TryParse(ddlEditDoUuTien.SelectedValue, out Guid idUt) ? (Guid?)idUt : null;

                task.ThoiHanNgay = thoiHan;
                task.NgayBatDau = ngayBd;
                task.NgayKetThuc = ngayBd.AddDays(thoiHan - 1);
            }

            task.TenCongViec = tenCv;
            task.MoTa = txtEditMoTa.Text.Trim();
            task.NgayCapNhat = DateTime.Now;
            task.Save();
            if (isAddNew)
            {
                if (task.IdCongViecCha.HasValue)
                {
                    _taskManager.AutoSetParentPriority(CurrentProjectId, task.IdCongViecCha.Value, _dictPriorities);
                    _taskManager.AutoSetParentTime(CurrentProjectId, task.IdCongViecCha.Value);
                    _taskManager.AutoSetParentStatus(CurrentProjectId, task.IdCongViecCha.Value);
                }
            }
            else
            {
                if (isFatherTask)
                {
                    _taskManager.AutoSetFirstChildStartTime(CurrentProjectId, task.IdCongViec, task.NgayBatDau.Value);
                }
                else
                {
                    _taskManager.AutoSetDependentTime(CurrentProjectId, task.IdCongViec);
                    if (task.IdCongViecCha.HasValue)
                    {
                        _taskManager.AutoSetParentPriority(CurrentProjectId, task.IdCongViecCha.Value, _dictPriorities);
                        _taskManager.AutoSetParentTime(CurrentProjectId, task.IdCongViecCha.Value);
                        _taskManager.AutoSetParentStatus(CurrentProjectId, task.IdCongViecCha.Value);
                    }
                }
            }
            ShowNotify(isAddNew ? "Thêm mới công việc thành công!" : "Cập nhật công việc thành công!", MSGType.Success);
            CtrlTask1.Rebind();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "CloseEditModal", "closeEditModal();", true);
        }

        protected void ddlEditCongViecChaSelected(object sender, EventArgs e)
        {
            Guid? parentId = Guid.TryParse(ddlEditCongViecCha.SelectedValue, out Guid pid) ? (Guid?)pid : null;
            bool isAddNew = string.IsNullOrEmpty(hfEditTaskId.Value);
            txtEditGiaiDoan.Text = _taskManager.GetRootPhaseName(CurrentProjectId, parentId);
            string targetCode = isAddNew ? _taskManager.GenerateNewTaskCode(CurrentProjectId, parentId) : txtEditMaCv.Text.Trim();
            if (isAddNew) txtEditMaCv.Text = targetCode;
            Guid? currentExcludeId = isAddNew ? (Guid?)null : (Guid.TryParse(hfEditTaskId.Value, out Guid tid) ? (Guid?)tid : null);
            _controlHelpers.BindDependentTasks(ddlEditPhuThuoc, CurrentProjectId, currentExcludeId, currentOrNewCode: targetCode);
            UpdateMinStartDate();
            upModal.Update();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "KeepModalOpen", "openEditModal();", true);
        }

        protected void ddlEditPhuThuocSelected(object sender, EventArgs e)
        {
            UpdateMinStartDate();
            upModal.Update();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "KeepModalOpen", "openEditModal();", true);
        }
        #endregion

        #region Helpers
        private void SetFormControlsState(bool isPhase, bool hasChildren)
        {
            txtEditTenCv.Enabled = true;
            txtEditMoTa.Enabled = true;
            ddlEditNhanVien.Enabled = !isPhase;
            ddlEditTrangThai.Enabled = !isPhase;
            txtEditThoiHan.Enabled = !isPhase && !hasChildren;
            txtEditNgayBatDau.Enabled = true;
            ddlEditDoUuTien.Enabled = !isPhase && !hasChildren;
        }

        private void UpdateMinStartDate()
        {
            txtEditNgayBatDau.Attributes.Remove("min");
            Guid? parentId = Guid.TryParse(ddlEditCongViecCha.SelectedValue, out Guid pid) ? (Guid?)pid : null;
            Guid? depId = Guid.TryParse(ddlEditPhuThuoc.SelectedValue, out Guid did) ? (Guid?)did : null;
            var (minStartLimit, _) = _taskManager.GetMinStartDate(parentId, depId);
            if (minStartLimit.HasValue)
            {
                txtEditNgayBatDau.Attributes["min"] = minStartLimit.Value.ToString("yyyy-MM-dd");
            }
        }

        private void ShowAlert(string message)
        {
            string safeMsg = message.Replace("'", "\\'").Replace("\r\n", "\\n").Replace("\n", "\\n");
            ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(),
                $"openEditModal(); alert('{safeMsg}');", true);
        }
        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlTask1.ConfirmRequest(e);
        }
        #endregion
    }
}