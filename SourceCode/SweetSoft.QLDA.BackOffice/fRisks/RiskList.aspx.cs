using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fRisks
{
    public partial class RiskList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.Risk;
        private Guid RiskId
        {
            get
            {
                if (ViewState["RiskId"] != null)
                    return (Guid)ViewState["RiskId"];
                return Guid.Empty;
            }
            set => ViewState["RiskId"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlRisk1.NewRiskHandlerCallback += NewRiskAction;
            CtrlRisk1.EditRiskHandlerCallback += EditRiskAction;
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                if (CurrentProjectId == Guid.Empty)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Projects), true);
                    return;
                }
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.RISK_LIST));
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.RISK_LIST);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>
                {
                    { GetRelativeClientPath(RewriteURLHelper.Projects), GetResourceText(BackEndResourceKeys.PROJECT_LIST) },
                    { "javascript:;", GetResourceText(BackEndResourceKeys.RISK_LIST) }
                };
                ApplyControlsText();
                CtrlRisk1.InitControls();
            }
        }

        private void ApplyControlsText()
        {
            ddlNhanVien.PlaceHolder = ddlMucDoAnhHuong.PlaceHolder=
            ddlXacSuat.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            dlDetail.CloseText = GetResourceText(BackEndResourceKeys.CLOSE);
            txtTenRuiRo.PlaceHolder = txtMucDoRuiRo.PlaceHolder
                = txtKeHoachPhongNgua.PlaceHolder
                = txtKeHoachUngPho.PlaceHolder
                = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
        }

        private void NewRiskAction(object sender, EventArgs e)
        {
            RefreshRiskInfo();
            lbtSubmit.Visible = this.IsAdd;
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
            dlDetail.OpenModal(true);
        }
        private void EditRiskAction(object sender, EventArgs e)
        {
            if (sender == null)
            {
                ShowInvalidDataError();
                return;
            }
            Guid riskId = (Guid)sender;
            if (riskId == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }
            RefreshRiskInfo();
            lbtSubmit.Visible = this.IsEdit;
            TblRuiRoDuAn risk = TblRuiRoDuAn.FetchByID(riskId);
            if (risk == null || risk.DaXoa == true)
            {
                Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), false);
                return;
            }
            this.RiskId = risk.IdRuiRoDuAn;
            txtTenRuiRo.Text = risk.TenRuiRo;

            if (risk.IdNhanVienXuLy != null)
                ddlNhanVien.SelectedValue = risk.IdNhanVienXuLy.ToString();

            if (risk.XacSuatXayRa != null)
                ddlXacSuat.SelectedValue = risk.XacSuatXayRa.ToString();

            if (risk.MucDoAnhHuong != null)
                ddlMucDoAnhHuong.SelectedValue = risk.MucDoAnhHuong.ToString();

            if (risk.DiemRuiRo != null)
                txtMucDoRuiRo.Text = risk.DiemRuiRo.ToString();

            txtKeHoachPhongNgua.Text = risk.KeHoachPhongNgua != GetResourceText(BackEndResourceKeys.NOT_ENTERED) ? risk.KeHoachPhongNgua : "";
            txtKeHoachUngPho.Text = risk.KeHoachUngPho != GetResourceText(BackEndResourceKeys.NOT_ENTERED) ? risk.KeHoachUngPho : "";

            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.EDIT) ?? "Thông tin rủi ro";
            if (!IsPostBack)
                dlDetail.OpenModal(true, 1000); 
            else
                dlDetail.OpenModal(true);
        }

        private void RefreshRiskInfo()
        {
            ControlHelpers controlHelpers = new ControlHelpers();
            controlHelpers.BindNhanVienDuAn(ddlNhanVien, CtrlRisk1.ProjectId);
            controlHelpers.BindMucDoAnhHuong(ddlMucDoAnhHuong);
            controlHelpers.BindXacSuatRuiRo(ddlXacSuat);
            lbtSubmit.Visible = false;
            txtTenRuiRo.Text = txtMucDoRuiRo.Text =
            txtKeHoachPhongNgua.Text = txtKeHoachUngPho.Text = "";
            if (ddlNhanVien.Items.Count > 0)
                ddlNhanVien.SelectedIndex = 0;
            this.RiskId = Guid.Empty;
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
                TblRuiRoDuAn risk = null;
                bool isNew = (this.RiskId == Guid.Empty);
                if (isNew)
                {
                    risk = new TblRuiRoDuAn();
                    risk.IdRuiRoDuAn = Guid.NewGuid();
                    risk.IdDuAn = CtrlRisk1.ProjectId;
                    risk.DaXoa = false;
                    risk.NgayTao = DateTime.Now;
                    risk.NguoiTao = SweetContext.Current.UserName; 
                    risk.NgayCapNhat = DateTime.Now;
                    risk.NguoiCapNhat = SweetContext.Current.UserName; 
                }
                else
                {
                    risk = TblRuiRoDuAn.FetchByID(this.RiskId);
                    if (risk == null)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }
                    risk.NgayCapNhat = DateTime.Now;
                    risk.NguoiCapNhat = SweetContext.Current.UserName;
                }
                risk.TenRuiRo = txtTenRuiRo.Text.Trim();

                Guid idNhanVien = Guid.Empty;
                if (this.GetValue(ddlNhanVien, out idNhanVien) && idNhanVien != Guid.Empty)
                    risk.IdNhanVienXuLy = idNhanVien;
                else
                    risk.IdNhanVienXuLy = null;
                int xacSuat = 0;
                if (this.GetValue(ddlXacSuat, out xacSuat))
                {
                    risk.XacSuatXayRa = xacSuat;
                }
                int mucDoAnhHuong = 0;
                if (this.GetValue(ddlMucDoAnhHuong, out mucDoAnhHuong))
                    risk.MucDoAnhHuong = mucDoAnhHuong;

                decimal score = ((decimal)xacSuat / 100m) * mucDoAnhHuong;

                risk.DiemRuiRo = (float)score; 
                risk.KeHoachPhongNgua = !string.IsNullOrEmpty(txtKeHoachPhongNgua.Text.Trim()) ? txtKeHoachPhongNgua.Text.Trim() : GetResourceText(BackEndResourceKeys.NOT_ENTERED);
                risk.KeHoachUngPho = !string.IsNullOrEmpty(txtKeHoachUngPho.Text.Trim()) ? txtKeHoachUngPho.Text.Trim() : GetResourceText(BackEndResourceKeys.NOT_ENTERED);

                risk.Save();
                ShowSuccessSaveData();
                dlDetail.CloseModal();
                CtrlRisk1.Rebind();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlRisk1.ConfirmRequest(e);
        }
        private void TinhToanMucDoRuiRo()
        {
            decimal xacSuat = 0;
            decimal.TryParse(ddlXacSuat.SelectedValue, out xacSuat);
            int mucDoAnhHuong = 0;
            int.TryParse(ddlMucDoAnhHuong.SelectedValue, out mucDoAnhHuong);
            decimal score = (xacSuat / 100m) * mucDoAnhHuong;
            string textMucDoRuiRo = "";
            if (xacSuat >= 75m || mucDoAnhHuong >= 4)
            {
                if (score >= 4.5m)
                    textMucDoRuiRo = GetResourceText(BackEndResourceKeys.VERY_HIGH);
                else
                    textMucDoRuiRo = GetResourceText(BackEndResourceKeys.HIGH);
            }
            else
            {
                if (score < 1.0m)
                    textMucDoRuiRo = GetResourceText(BackEndResourceKeys.VERY_LOW);
                else if (score >= 1.0m && score < 2.0m)
                    textMucDoRuiRo = GetResourceText(BackEndResourceKeys.LOW);
                else if (score >= 2.0m && score < 3.5m)
                    textMucDoRuiRo = GetResourceText(BackEndResourceKeys.MEDIUM);
                else if (score >= 3.5m && score < 4.5m)
                    textMucDoRuiRo = GetResourceText(BackEndResourceKeys.HIGH);
                else
                    textMucDoRuiRo = GetResourceText(BackEndResourceKeys.VERY_HIGH);
            }
            if (score > 0)
            {
                txtMucDoRuiRo.Text = $"{textMucDoRuiRo} ({score.ToString("0.##")})";
            }
            else
            {
                txtMucDoRuiRo.Text = "--";
            }
        }

        protected void ddlXacSuat_SelectedIndexChanged(object sender, EventArgs e)
        {
            TinhToanMucDoRuiRo();
        }

        protected void ddlMucDoAnhHuong_SelectedIndexChanged(object sender, EventArgs e)
        {
            TinhToanMucDoRuiRo();
        }
    }
}