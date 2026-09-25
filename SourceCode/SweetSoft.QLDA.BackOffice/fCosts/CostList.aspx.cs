using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fCosts
{
    public partial class CostList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.Cost;
        private ControlHelpers _control = new ControlHelpers();

        private Guid CostId
        {
            get => ViewState["CostId"] != null ? (Guid)ViewState["CostId"] : Guid.Empty;
            set => ViewState["CostId"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlProjectTabs1.ProjectId = CurrentProjectId;
            CtrlCost1.NewCostHandlerCallback += NewCostAction;
            CtrlCost1.EditCostHandlerCallback += EditCostAction;
            CtrlCost1.OpenCostFilesHandlerCallback += OpenCostFilesAction;
            fbCostFiles.CurrentFileIdResolver = (recordId, refType) =>
                ProjectRecordFileAccess.GetLinkedFileId(recordId, refType.ToString());
            fbCostFiles.FileMutationValidator = (recordId, refType, fileId) =>
                ProjectRecordFileAccess.CanAccess(SweetContext.Current.UserId,
                    recordId, refType.ToString(), true)
                && ProjectRecordFileAccess.BelongsToRecord(recordId,
                    refType.ToString(), fileId);

            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);

                if (CurrentProjectId == Guid.Empty)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Projects), true);
                    return;
                }

                string title = GetResourceText(BackEndResourceKeys.COST_LIST) ?? "Danh sách chi phí";
                SetMetaTagsOgTags(title);
                Navigation1.MainTitle = title;
                Navigation1.keyValuePairUrls = new Dictionary<string, string>
                {
                    { GetRelativeClientPath(RewriteURLHelper.Projects), GetResourceText(BackEndResourceKeys.PROJECT_LIST) },
                    { "javascript:;", title }
                };

                ApplyControlsText();
                CtrlCost1.InitControls();
            }
        }

        private void ApplyControlsText()
        {
            dlDetail.Title = "Thông tin chi phí";
            dlDetail.CloseText = GetResourceText(BackEndResourceKeys.CLOSE);
            txtTenKhoanChi.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            _control.BindTrangThaiChiPhi(ddlTrangThai);
        }

        private void SetupUIByRole(bool isNew, Guid? idNhanVienDeNghi = null)
        {
            Guid? pmId = DuAnManager.Instance.LayIdNhanVienQuanLy(CurrentProjectId);
            bool isPM = pmId.HasValue && pmId.Value == SweetContext.Current.UserId;

            if (isPM)
            {
                txtNhanVienYeuCau.Visible = false;
                ddlNhanVienYeuCau.Visible = true;

                _control.BindProjectMembers(ddlNhanVienYeuCau, CurrentProjectId, null);

                if (idNhanVienDeNghi.HasValue)
                    ddlNhanVienYeuCau.SelectedValue = idNhanVienDeNghi.Value.ToString();
                else
                    ddlNhanVienYeuCau.SelectedValue = SweetContext.Current.UserId.ToString();
            }
            else
            {
                txtNhanVienYeuCau.Visible = true;
                ddlNhanVienYeuCau.Visible = false;

                if (idNhanVienDeNghi.HasValue)
                {
                    var user = UserManager.Instance.GetUserById(idNhanVienDeNghi.Value);
                    txtNhanVienYeuCau.Text = user != null ? user.DisplayName : "—";
                }
                else
                {
                    txtNhanVienYeuCau.Text = SweetContext.Current.UserName;
                }
            }

            if (isPM)
            {
                ddlTrangThai.Enabled = true;
            }
            else
            {
                ddlTrangThai.Enabled = false;
                if (isNew)
                {
                    ddlTrangThai.SelectedValue = "0";
                }
            }
        }

        private void OpenCostFilesAction(object sender, EventArgs e)
        {
            Guid idChiPhi = sender is Guid ? (Guid)sender : Guid.Empty;
            if (idChiPhi == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            TblChiPhi cost = TblChiPhi.FetchByID(idChiPhi);
            if (cost == null || cost.DaXoa == true || cost.IdDuAn != CurrentProjectId)
            {
                ShowInvalidDataError();
                return;
            }
            if (!ProjectRecordFileAccess.CanAccess(
                SweetContext.Current.UserId, idChiPhi,
                FileUploadTypes.CostAttachment.ToString(), false))
            {
                ShowAccessDeniedNotify();
                return;
            }

            fbCostFiles.IsMultiple = false;
            fbCostFiles.IsEnabled = ProjectRecordFileAccess.CanAccess(
                SweetContext.Current.UserId, idChiPhi,
                FileUploadTypes.CostAttachment.ToString(), true);
            fbCostFiles.LoadFile(idChiPhi, FileUploadTypes.CostAttachment);
            dlCostFiles.OpenModal(true);
        }

        private void NewCostAction(object sender, EventArgs e)
        {
            RefreshCostInfo();
            lbtSubmit.Visible = this.IsAdd;
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);

            txtNgayTao.Text = DateTime.Now.ToString("dd/MM/yyyy");
            txtNguoiTao.Text = SweetContext.Current.UserName;

            SetupUIByRole(true, null);

            dlDetail.OpenModal(true);
        }

        private void EditCostAction(object sender, EventArgs e)
        {
            if (sender == null || (Guid)sender == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            Guid costId = (Guid)sender;
            RefreshCostInfo();
            lbtSubmit.Visible = this.IsEdit;

            TblChiPhi cost = TblChiPhi.FetchByID(costId);
            if (cost == null || cost.DaXoa == true)
            {
                Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), false);
                return;
            }

            this.CostId = cost.IdChiPhi;
            txtTenKhoanChi.Text = cost.TenKhoanChi;
            txtMoTaChiTiet.Text = cost.MoTaChiTiet;

            txtDonGia.Text = cost.DonGia?.ToString("N0");
            txtSoLuong.Text = cost.SoLuong?.ToString();
            txtTongTien.Text = cost.SoTien.ToString("N0");

            if (cost.NgayTao != null)
                txtNgayTao.Text = cost.NgayTao.Date.ToString("dd/MM/yyyy");

            if (cost.TrangThai != null)
            {
                ddlTrangThai.SelectedValue = cost.TrangThai.ToString();
                if (cost.TrangThai == 2)
                {
                    txtLyDoTuChoi.Text = cost.LyDoTuChoi;
                    divLyDoTuChoi.Style["display"] = "block"; 
                }
                else
                {
                    divLyDoTuChoi.Style["display"] = "none";
                }
            }

            SetupUIByRole(false, cost.IdNhanVienDeNghi);

            if (cost.IdNguoiTao.HasValue)
            {
                var creator = UserManager.Instance.GetUserById(cost.IdNguoiTao.Value);
                txtNguoiTao.Text = creator != null ? creator.DisplayName : "—";
            }
            else
            {
                txtNguoiTao.Text = "—";
            }

            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.EDIT) ?? "Cập nhật";
            dlDetail.OpenModal(true, IsPostBack ? 0 : 1000);
        }

        private void RefreshCostInfo()
        {
            txtTenKhoanChi.Text = txtDonGia.Text = txtSoLuong.Text = txtTongTien.Text = txtMoTaChiTiet.Text = txtNgayTao.Text = txtNguoiTao.Text = "";
            txtLyDoTuChoi.Text = "";
            divLyDoTuChoi.Style["display"] = "none"; // CẬP NHẬT: Ẩn mặc định

            ddlNhanVienYeuCau.Items.Clear();
            if (ddlTrangThai.Items.Count > 0) ddlTrangThai.SelectedIndex = 0;
            lbtSubmit.Visible = false;
            this.CostId = Guid.Empty;
        }

        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(dlDetail.Controls);
                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }

                bool isNew = (this.CostId == Guid.Empty);
                TblChiPhi costDto = isNew ? new TblChiPhi() : TblChiPhi.FetchByID(this.CostId);

                costDto.IdDuAn = CtrlCost1.ProjectId;
                costDto.TenKhoanChi = txtTenKhoanChi.Text.Trim();
                costDto.MoTaChiTiet = !string.IsNullOrEmpty(txtMoTaChiTiet.Text.Trim()) ? txtMoTaChiTiet.Text.Trim() : null;

                string donGiaText = txtDonGia.Text.Trim().Replace(",", "");
                if (decimal.TryParse(donGiaText, out decimal donGia))
                    costDto.DonGia = donGia;
                if (int.TryParse(txtSoLuong.Text.Trim(), out int soLuong))
                    costDto.SoLuong = soLuong;

                costDto.SoTien = (costDto.DonGia ?? 0) * (costDto.SoLuong ?? 0);

                if (int.TryParse(ddlTrangThai.SelectedValue, out int trangThai))
                {
                    costDto.TrangThai = (byte)trangThai;

                    if (trangThai == 2)
                    {
                        if (string.IsNullOrWhiteSpace(txtLyDoTuChoi.Text))
                        {
                            ShowNotify("Vui lòng nhập lý do từ chối!", MSGType.Warning);
                            // Ghi đè C# state tránh slideUp mất
                            divLyDoTuChoi.Style["display"] = "block";
                            dlDetail.OpenModal(true);
                            return;
                        }
                        costDto.LyDoTuChoi = txtLyDoTuChoi.Text.Trim();
                    }
                    else
                    {
                        costDto.LyDoTuChoi = null;
                    }
                }

                Guid? pmId = DuAnManager.Instance.LayIdNhanVienQuanLy(CurrentProjectId);
                bool isPM = pmId.HasValue && pmId.Value == SweetContext.Current.UserId;

                if (isPM)
                {
                    if (Guid.TryParse(ddlNhanVienYeuCau.SelectedValue, out Guid selectedId))
                        costDto.IdNhanVienDeNghi = selectedId;
                }
                else
                {
                    if (isNew) costDto.IdNhanVienDeNghi = SweetContext.Current.UserId;
                }

                if (isNew)
                {
                    costDto.IdNguoiTao = SweetContext.Current.UserId;
                    costDto.NgayTao = DateTime.Now;
                    costDto.DaXoa = false;
                }

                TblChiPhi savedCost = CostManager.Instance.CreateOrUpdate(costDto);

                if (savedCost == null)
                {
                    ShowInvalidDataError();
                    return;
                }
                ShowSuccessSaveData();
                dlDetail.CloseModal();
                CtrlCost1.Rebind();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }
    }
}
