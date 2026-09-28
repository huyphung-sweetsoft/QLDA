using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    public partial class CtrlAddSubTask : BaseAdminUserControl
    {
        public event EventHandler SavedSuccess;

        private Guid ProjectId { get => (Guid)(ViewState["ProjectId"] ?? Guid.Empty); set => ViewState["ProjectId"] = value; }

        public void OpenModal(Guid projectId, Guid parentId)
        {
            this.ProjectId = projectId;
            hfParentId.Value = parentId.ToString();

            TblCongViec parentTask = TaskManager.Instance.FetchById(parentId);
            if (parentTask == null) return;

            bool isClickOnPhase = !parentTask.IdCongViecCha.HasValue;

            if (isClickOnPhase)
            {
                ltrPhaseName.Text = $"[{parentTask.MaCongViec}] {parentTask.TenCongViec}";
                ltrParentTaskName.Text = "<span class='text-muted'>-- Lớp con cấp 1 --</span>";
            }
            else
            {
                string phaseName = TaskManager.Instance.GetRootPhaseName(projectId, parentId);
                ltrPhaseName.Text = string.IsNullOrEmpty(phaseName) ? "-- Không xác định --" : phaseName;
                ltrParentTaskName.Text = $"[{parentTask.MaCongViec}] {parentTask.TenCongViec}";
            }

            string newCode = TaskManager.Instance.GenerateNewTaskCode(projectId, parentId);
            ltrSubTaskCode.Text = newCode;
            txtMaCv.Text = newCode;

            txtTenCongViec.Text = string.Empty;
            txtMoTa.Text = string.Empty;
            txtThoiHan.Text = "1";

            ControlHelpers helpers = new ControlHelpers();
            helpers.BindDependentTasks(ddlPhuThuoc, projectId, currentOrNewCode: newCode);
            FilterDependentTasks(newCode, parentId);
            helpers.BindPriorities(ddlDoUuTien);

            UpdateMinStartDate();

            upAddSubTask.Update();
            mdlAddSubTask.OpenModal(true);
        }
        private void FilterDependentTasks(string newCode, Guid parentId)
        {
            HashSet<Guid> ancestors = new HashSet<Guid>();
            Guid? currentAncestor = parentId;
            while (currentAncestor.HasValue)
            {
                ancestors.Add(currentAncestor.Value);
                var pTask = TaskManager.Instance.FetchById(currentAncestor.Value);
                currentAncestor = pTask?.IdCongViecCha;
            }

            List<ListItem> invalidItems = new List<ListItem>();
            foreach (ListItem item in ddlPhuThuoc.Items)
            {
                if (string.IsNullOrEmpty(item.Value)) continue;

                if (Guid.TryParse(item.Value, out Guid depId))
                {
                    if (ancestors.Contains(depId))
                    {
                        invalidItems.Add(item);
                        continue;
                    }

                    var depTask = TaskManager.Instance.FetchById(depId);
                    if (depTask != null && !string.IsNullOrEmpty(depTask.MaCongViec))
                    {
                        if (CompareWBS(depTask.MaCongViec, newCode) >= 0)
                        {
                            invalidItems.Add(item);
                        }
                    }
                }
            }

            foreach (var item in invalidItems)
            {
                ddlPhuThuoc.Items.Remove(item);
            }
        }
        private int CompareWBS(string code1, string code2)
        {
            if (string.IsNullOrEmpty(code1) || string.IsNullOrEmpty(code2)) return 0;
            var parts1 = code1.Split('.');
            var parts2 = code2.Split('.');
            int minLen = Math.Min(parts1.Length, parts2.Length);

            for (int i = 0; i < minLen; i++)
            {
                if (int.TryParse(parts1[i], out int n1) && int.TryParse(parts2[i], out int n2))
                {
                    if (n1 != n2) return n1.CompareTo(n2);
                }
                else
                {
                    int c = string.Compare(parts1[i], parts2[i], StringComparison.OrdinalIgnoreCase);
                    if (c != 0) return c;
                }
            }
            return parts1.Length.CompareTo(parts2.Length);
        }
        protected void ddlPhuThuoc_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMinStartDate();

            if (DateTime.TryParse(txtNgayBatDau.Text.Trim(), out DateTime startDt) && int.TryParse(txtThoiHan.Text.Trim(), out int duration))
            {
                DateTime endDt = LichBieuChungManager.Instance.CalculateTaskEndDate(startDt, duration);
                txtNgayKetThuc.Text = endDt.ToString("yyyy-MM-dd");
                lblNgayKetThuc.Text = endDt.ToString("dd/MM/yyyy");
            }

            upAddSubTask.Update();
            mdlAddSubTask.OpenModal(true);
        }

        protected void txtNgayBatDau_TextChanged(object sender, EventArgs e)
        {
            UpdateMinStartDate();
            upAddSubTask.Update();
            mdlAddSubTask.OpenModal(true);
        }

        protected void txtThoiHan_TextChanged(object sender, EventArgs e)
        {
            if (DateTime.TryParse(txtNgayBatDau.Text.Trim(), out DateTime startDt) && int.TryParse(txtThoiHan.Text.Trim(), out int duration))
            {
                DateTime endDt = LichBieuChungManager.Instance.CalculateTaskEndDate(startDt, duration);
                txtNgayKetThuc.Text = endDt.ToString("yyyy-MM-dd");
                lblNgayKetThuc.Text = endDt.ToString("dd/MM/yyyy");
            }
            upAddSubTask.Update();
            mdlAddSubTask.OpenModal(true);
        }

        private void UpdateMinStartDate()
        {
            txtNgayBatDau.Attributes.Remove("min");
            if (!Guid.TryParse(hfParentId.Value, out Guid parentId)) return;

            Guid? depId = Guid.TryParse(ddlPhuThuoc.SelectedValue, out Guid did) ? (Guid?)did : null;

            var (minStartLimit, _) = TaskManager.Instance.GetMinStartDate(parentId, depId);

            if (minStartLimit.HasValue)
            {
                string minDateStr = minStartLimit.Value.ToString("yyyy-MM-dd");
                txtNgayBatDau.Attributes["min"] = minDateStr;

                if (!DateTime.TryParse(txtNgayBatDau.Text.Trim(), out DateTime currentStartDate) || currentStartDate.Date < minStartLimit.Value.Date)
                {
                    txtNgayBatDau.Text = minDateStr;
                }
            }
            else
            {
                TblCongViec parentTask = TblCongViec.FetchByID(parentId);
                DateTime defaultStart = parentTask != null && parentTask.NgayBatDau.HasValue ? parentTask.NgayBatDau.Value : DateTime.Today;
                txtNgayBatDau.Text = defaultStart.ToString("yyyy-MM-dd");
            }

            if (DateTime.TryParse(txtNgayBatDau.Text.Trim(), out DateTime startDt) && int.TryParse(txtThoiHan.Text.Trim(), out int duration))
            {
                DateTime endDt = LichBieuChungManager.Instance.CalculateTaskEndDate(startDt, duration);
                txtNgayKetThuc.Text = endDt.ToString("yyyy-MM-dd");
                lblNgayKetThuc.Text = endDt.ToString("dd/MM/yyyy");
            }
        }

        protected void btnSaveSubTask_Click(object sender, EventArgs e)
        {
            try
            {
                DuAnManager.Instance.EnsureCanModifyStructure(this.ProjectId);

                if (!Guid.TryParse(hfParentId.Value, out Guid parentId)) return;
                TblCongViec parentTask = TaskManager.Instance.FetchById(parentId);
                if (parentTask == null) return;

                string tenCv = txtTenCongViec.Text.Trim();
                if (string.IsNullOrEmpty(tenCv))
                {
                    ShowNotify("Vui lòng nhập Tên công việc con!", MSGType.Error);
                    return;
                }

                if (!DateTime.TryParse(txtNgayBatDau.Text.Trim(), out DateTime ngayBd))
                {
                    ShowNotify("Ngày bắt đầu không hợp lệ!", MSGType.Error);
                    return;
                }

                if (!int.TryParse(txtThoiHan.Text.Trim(), out int thoiHan) || thoiHan <= 0)
                {
                    ShowNotify("Thời hạn phải lớn hơn 0!", MSGType.Error);
                    return;
                }

                Guid? idPhuThuoc = Guid.TryParse(ddlPhuThuoc.SelectedValue, out Guid ptId) ? (Guid?)ptId : null;

                var (minStartAllowed, limitReason) = TaskManager.Instance.GetMinStartDate(parentId, idPhuThuoc);
                if (minStartAllowed.HasValue && ngayBd.Date < minStartAllowed.Value.Date)
                {
                    ShowNotify($"Ngày bắt đầu không được nhỏ hơn {minStartAllowed.Value.ToString("dd/MM/yyyy")} ({limitReason})", MSGType.Error);
                    return;
                }

                TblCongViec task = new TblCongViec();
                task.IdCongViec = Guid.NewGuid();
                task.IdDuAn = this.ProjectId;
                task.DaXoa = false;
                task.NgayTao = DateTime.Now;

                task.IdCongViecCha = parentId;
                task.IdCongViecPhuThuoc = idPhuThuoc;
                task.MaCongViec = TaskManager.Instance.GenerateNewTaskCode(this.ProjectId, parentId);

                task.IdGiaiDoanDuAn = parentTask.IdGiaiDoanDuAn;

                task.TenCongViec = tenCv;
                task.MoTa = txtMoTa.Text.Trim();
                task.TrangThai = 0;
                task.VaiTroNhanVien = (int)VaiTroNhanVien.ThucHien;
                task.NgayBatDau = ngayBd;
                task.ThoiHanNgay = thoiHan;
                task.IdDoUuTien = Guid.TryParse(ddlDoUuTien.SelectedValue, out Guid utId) ? (Guid?)utId : null;
                task.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(ngayBd, thoiHan);

                task.Save();
                parentTask.VaiTroNhanVien = (int)VaiTroNhanVien.DieuPhoi;
                parentTask.Save();
                Dictionary<Guid, TblDoUuTien> dictPriorities = TaskManager.Instance.GetDictPriorities();
                TaskManager.Instance.AutoSetParentPriority(this.ProjectId, parentId, dictPriorities);
                TaskManager.Instance.AutoSetParentTime(this.ProjectId, parentId);
                TaskManager.Instance.AutoSetParentStatus(this.ProjectId, parentId);

                mdlAddSubTask.CloseModal();

                string successMsg = Newtonsoft.Json.JsonConvert.SerializeObject("Thêm công việc con thành công!");
                ScriptManager.RegisterStartupScript(upAddSubTask, upAddSubTask.GetType(), "SuccessMsg", $"alert({successMsg});", true);

                if (SavedSuccess != null) SavedSuccess(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ShowNotify("Lỗi hệ thống: " + ex.Message, MSGType.Error);
            }
        }
    }
}