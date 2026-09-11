using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.BackOffice.fCosts
{
    public partial class CostList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.Cost;
        private ControlHelpers _controls = new ControlHelpers();
        private Guid CostId
        {
            get => ViewState["CostId"] != null ? (Guid)ViewState["CostId"] : Guid.Empty;
            set => ViewState["CostId"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlCost1.NewCostHandlerCallback += NewCostAction;
            CtrlCost1.EditCostHandlerCallback += EditCostAction;

            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);

                if (CurrentProjectId == Guid.Empty)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Projects), true);
                    return;
                }

                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.COST_LIST));
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.COST_LIST);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>
                {
                    { GetRelativeClientPath(RewriteURLHelper.Projects), GetResourceText(BackEndResourceKeys.PROJECT_LIST) },
                    { "javascript:;", GetResourceText(BackEndResourceKeys.COST_LIST) }
                };

                ApplyControlsText();
                CtrlCost1.InitControls();
            }
        }

        private void ApplyControlsText()
        {
            dlDetail.CloseText = GetResourceText(BackEndResourceKeys.CLOSE);
            txtTenKhoanChi.PlaceHolder = txtDonGia.PlaceHolder =
            txtMoTaChiTiet.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);

            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
        }

        private void NewCostAction(object sender, EventArgs e)
        {
            RefreshCostInfo();
            lbtSubmit.Visible = this.IsAdd;
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
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

            if (cost.IdNhanVienDeNghi != null)
            {
                txtNhanVienYeuCau.Text = SweetContext.Current.UserName;
            }
            if (cost.TrangThai != null)
                ddlTrangThai.SelectedValue = cost.TrangThai.ToString();
            ddlTrangThai.Enabled = this.IsEdit;

            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.EDIT);

            dlDetail.OpenModal(true, IsPostBack ? 0 : 1000);
        }

        private void RefreshCostInfo()
        {
            lbtSubmit.Visible = false;
            txtTenKhoanChi.Text = txtMoTaChiTiet.Text = "";
            _controls.BindTrangThaiChiPhi(ddlTrangThai);
            txtNhanVienYeuCau.Text = SweetContext.Current.UserName;
            txtNgayTao.Text = DateTime.Now.ToString("dd/MM/yyyy");

            txtDonGia.Text = "0";
            txtSoLuong.Text = "1";
            txtTongTien.Text = "0";
            if (ddlTrangThai.Items.Count > 0)
                ddlTrangThai.SelectedValue = "0";
            ddlTrangThai.Enabled = this.IsEdit;

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

                TblChiPhi costDto = new TblChiPhi();
                bool isNew = (this.CostId == Guid.Empty);

                if (!isNew)
                {
                    costDto.IdChiPhi = this.CostId;
                }

                costDto.IdDuAn = CtrlCost1.ProjectId;
                costDto.TenKhoanChi = txtTenKhoanChi.Text.Trim();
                costDto.MoTaChiTiet = !string.IsNullOrEmpty(txtMoTaChiTiet.Text.Trim()) ? txtMoTaChiTiet.Text.Trim() : null;

                string donGiaText = txtDonGia.Text.Trim().Replace(",", "");
                if (decimal.TryParse(donGiaText, out decimal donGia))
                    costDto.DonGia = donGia;

                if (int.TryParse(txtSoLuong.Text.Trim(), out int soLuong))
                    costDto.SoLuong = soLuong;

                if (int.TryParse(ddlTrangThai.SelectedValue, out int trangThai))
                {
                    costDto.TrangThai = (byte)trangThai;
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