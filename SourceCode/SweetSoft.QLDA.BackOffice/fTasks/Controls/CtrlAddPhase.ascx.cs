using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    public partial class CtrlAddPhase : BaseAdminUserControl
    {
        public event EventHandler SavedSuccess;

        private Guid ProjectId
        {
            get => ViewState["ProjectId"] != null ? (Guid)ViewState["ProjectId"] : Guid.Empty;
            set => ViewState["ProjectId"] = value;
        }

        public void OpenModal(Guid projectId)
        {
            this.ProjectId = projectId;

            string newCode = TaskManager.Instance.GenerateNewTaskCode(projectId, null);
            ltrPhaseCode.Text = newCode;
            txtTenGiaiDoan.Text = string.Empty;
            txtMoTa.Text = string.Empty;
            txtThoiHan.Text = "1";

            ControlHelpers helpers = new ControlHelpers();
            helpers.BindDependentTasks(ddlPhuThuoc, projectId, currentOrNewCode: newCode, chiLayGiaiDoan: true);

            UpdateMinStartDate();

            upAddPhase.Update();
            mdlAddPhase.OpenModal(true);
        }

        protected void ddlPhuThuoc_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMinStartDate();
            upAddPhase.Update();
            mdlAddPhase.OpenModal(true);
        }

        protected void txtNgayBatDau_TextChanged(object sender, EventArgs e)
        {
            UpdateMinStartDate();
            upAddPhase.Update();
            mdlAddPhase.OpenModal(true);
        }
        private void UpdateMinStartDate()
        {
            txtNgayBatDau.Attributes.Remove("min");

            // Phase thì không có IdCha, nên parentId = null
            Guid? depId = Guid.TryParse(ddlPhuThuoc.SelectedValue, out Guid did) ? (Guid?)did : null;

            var (minStartLimit, _) = TaskManager.Instance.GetMinStartDate(null, depId);

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
                if (string.IsNullOrEmpty(txtNgayBatDau.Text))
                {
                    txtNgayBatDau.Text = DateTime.Today.ToString("yyyy-MM-dd");
                }
            }

            if (DateTime.TryParse(txtNgayBatDau.Text.Trim(), out DateTime startDt) && int.TryParse(txtThoiHan.Text.Trim(), out int duration))
            {
                UpdateEndDateDisplay(startDt, duration);
            }
        }

        private void UpdateEndDateDisplay(DateTime startDate, int durationDays)
        {
            DateTime endDt = LichBieuChungManager.Instance.CalculateTaskEndDate(startDate, durationDays);
            txtNgayKetThuc.Text = endDt.ToString("yyyy-MM-dd");
            lblNgayKetThuc.Text = endDt.ToString("dd/MM/yyyy");
        }

        protected void btnSavePhase_Click(object sender, EventArgs e)
        {
            try
            {
                DuAnManager.Instance.EnsureCanModifyStructure(this.ProjectId);

                string tenGiaiDoan = txtTenGiaiDoan.Text.Trim();
                if (string.IsNullOrEmpty(tenGiaiDoan))
                {
                    ShowNotify("Vui lòng nhập Tên giai đoạn!", MSGType.Error);
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

                var (minStartAllowed, limitReason) = TaskManager.Instance.GetMinStartDate(null, idPhuThuoc);
                if (minStartAllowed.HasValue && ngayBd.Date < minStartAllowed.Value.Date)
                {
                    ShowNotify($"Ngày bắt đầu không được nhỏ hơn {minStartAllowed.Value.ToString("dd/MM/yyyy")} ({limitReason})", MSGType.Error);
                    return;
                }
                TblGiaiDoanDuAn phase = new TblGiaiDoanDuAn();
                phase.IdGiaiDoanDuAn = Guid.NewGuid();
                phase.DaXoa = false;

                TblCongViec task = new TblCongViec();
                task.IdCongViec = Guid.NewGuid();
                phase.IdDuAn = task.IdDuAn = this.ProjectId;
                task.IdCongViecCha = null;

                task.IdCongViecPhuThuoc = idPhuThuoc;
                task.IdGiaiDoanDuAn = phase.IdGiaiDoanDuAn;
                task.MaCongViec = TaskManager.Instance.GenerateNewTaskCode(this.ProjectId, null);
                if (int.TryParse(task.MaCongViec, out int thuTu))
                {
                    phase.ThuTuGiaiDoan = thuTu;
                }
                phase.TenGiaiDoanTuyChinh = task.TenCongViec = tenGiaiDoan;
                task.MoTa = txtMoTa.Text.Trim();
                phase.NgayBatDau = task.NgayBatDau = ngayBd;
                task.ThoiHanNgay = thoiHan;
                phase.NgayDuKienHoanThanh = task.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(ngayBd, thoiHan);
                task.TrangThai = 0;
                task.VaiTroNhanVien = (int)VaiTroNhanVien.ThucHien;
                task.DaXoa = false;
                task.NgayTao = DateTime.Now;
                phase.Save();
                task.Save();

                mdlAddPhase.CloseModal();

                string successMsg = Newtonsoft.Json.JsonConvert.SerializeObject("Thêm giai đoạn thành công!");
                ScriptManager.RegisterStartupScript(upAddPhase, upAddPhase.GetType(), "SuccessMsg", $"alert({successMsg});", true);

                if (SavedSuccess != null) SavedSuccess(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ShowNotify("Lỗi hệ thống: " + ex.Message, MSGType.Error);
            }
        }
    }
}