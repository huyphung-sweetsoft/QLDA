using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fCustomers.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fCustomers
{
    public partial class KhachHangList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.Customer;
            }
        }

        private Guid IdKhachHang
        {
            get
            {
                if (ViewState["IdKhachHang"] != null)
                    return (Guid)ViewState["IdKhachHang"];
                return Guid.Empty;
            }
            set
            {
                ViewState["IdKhachHang"] = value;
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlKhachHang.NewCustomerHandlerCallBack += NewCustomerAction;
            CtrlKhachHang.EditCustomerHandlerCallBack += EditCustomerAction;

            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.CUSTOMER_LIST));
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.CUSTOMER_LIST);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    {RewriteURLHelper.Customers, GetResourceText(BackEndResourceKeys.CUSTOMER_LIST) }
                };
                CtrlKhachHang.InitControls();
                ApplyControlsText();
            }
        }

        private void ApplyControlsText()
        {
            ddlLoaiKhachHang.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);

            txtTenKhachHang.PlaceHolder = txtIdSoThue.PlaceHolder
                = txtSoDienThoai.PlaceHolder
                = txtEmail.PlaceHolder
                = txtNguoiLienHe.PlaceHolder
                = txtSDTLienHe.PlaceHolder
                = txtEmailLienHe.PlaceHolder
                = "";

            chkStatus.OnText = GetResourceText(BackEndResourceKeys.ACTIVE);
            chkStatus.OffText = GetResourceText(BackEndResourceKeys.INACTIVE);
        }

        private void RefreshCustomerInfo()
        {
            new ControlHelpers().BindLoaiKhachHang(ddlLoaiKhachHang);
            lbtSubmit.Visible = false;
            //-----------------------------------------------------------
            txtTenKhachHang.Text = txtIdSoThue.Text
                = txtSoDienThoai.Text
                = txtEmail.Text
                = txtNguoiLienHe.Text
                = txtSDTLienHe.Text
                = txtEmailLienHe.Text
                = "";

            chkStatus.Checked = true;
            this.IdKhachHang = Guid.Empty;
        }

        private void NewCustomerAction(object sender, EventArgs e)
        {
            RefreshCustomerInfo();
            lbtSubmit.Visible = this.IsAdd;
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
            dlDetail.OpenModal(true);
        }

        private void EditCustomerAction(object sender, EventArgs e)
        {
            if (sender == null)
            {
                ShowInvalidDataError();
                return;
            }
            Guid idKhachHang = (Guid)sender;
            if (idKhachHang == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }
            RefreshCustomerInfo();
            lbtSubmit.Visible = this.IsEdit;
            TblKhachHang khachHang = KhachHangManager.Instance.GetKhachHangById(idKhachHang);
            if (khachHang == null || khachHang.DaXoa)
            {
                Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), false);
                return;
            }

            this.IdKhachHang = khachHang.IdKhachHang;
            txtTenKhachHang.Text = khachHang.TenKhachHang;
            txtIdSoThue.Text = khachHang.IdSoThue;
            txtEmail.Text = khachHang.Email;
            txtSoDienThoai.Text = khachHang.SoDienThoai;
            txtDiaChi.Text = khachHang.DiaChi;
            txtNguoiLienHe.Text = khachHang.TenNguoiLienHe;
            txtSDTLienHe.Text = khachHang.DienThoaiLienHe;
            txtEmailLienHe.Text = khachHang.EmailLienHe;
            txtMoTa.Text = khachHang.GhiChu;
            ddlLoaiKhachHang.SelectedValue = khachHang.IdLoaiKhachHang.ToString();
            chkStatus.Checked = khachHang.KichHoat;

            lbtSubmit.Visible = this.IsEdit;
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.UPDATE);

            dlDetail.Title = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.OpenModal(true);
        }

        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                #region Valid
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(dlDetail.Controls);

                Guid idLoaiKhachHang = Guid.Empty;

                if (!string.IsNullOrEmpty(txtSoDienThoai.Text) && !RegexUtilities.IsValidPhone(txtSoDienThoai.Text))
                    validationEngine.AddErrorPrompt(txtSoDienThoai.ClientID, GetResourceText(BackEndResourceKeys.INVALID_PHONE_NUMBER));

                if (!string.IsNullOrEmpty(txtSDTLienHe.Text) && !RegexUtilities.IsValidPhone(txtSDTLienHe.Text))
                    validationEngine.AddErrorPrompt(txtSDTLienHe.ClientID, GetResourceText(BackEndResourceKeys.INVALID_PHONE_NUMBER));

                if (!string.IsNullOrEmpty(txtEmail.Text) && !RegexUtilities.IsValidEmail(txtEmail.Text))
                    validationEngine.AddErrorPrompt(txtEmail.ClientID, GetResourceText(BackEndResourceKeys.INVALID_EMAIL));

                if (!string.IsNullOrEmpty(txtEmailLienHe.Text) && !RegexUtilities.IsValidEmail(txtEmailLienHe.Text))
                    validationEngine.AddErrorPrompt(txtEmailLienHe.ClientID, GetResourceText(BackEndResourceKeys.INVALID_EMAIL));

                if (!GetValue(ddlLoaiKhachHang, out idLoaiKhachHang) || idLoaiKhachHang == Guid.Empty)
                {
                    validationEngine.AddErrorPrompt(ddlLoaiKhachHang.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE));
                }

                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }
                #endregion

                bool isAdd = true;
                KhachHangManager organizationKhachHangManager = KhachHangManager.Instance;
                TblKhachHang khachHang = new TblKhachHang();
                if (this.IdKhachHang != Guid.Empty)
                {
                    khachHang.IdKhachHang = this.IdKhachHang;
                    if (!this.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    isAdd = false;
                }
                else if (!this.IsAdd)
                {
                    ShowAccessDeniedNotify();
                    return;
                }
                khachHang.TenKhachHang = txtTenKhachHang.Text.Trim();
                khachHang.SoDienThoai = txtSoDienThoai.Text.Trim();
                khachHang.IdSoThue = txtIdSoThue.Text.Trim();
                khachHang.Email = txtEmail.Text.Trim();
                khachHang.DiaChi = txtDiaChi.Text.Trim();
                khachHang.TenNguoiLienHe = txtNguoiLienHe.Text.Trim();
                khachHang.DienThoaiLienHe = txtSDTLienHe.Text.Trim();
                khachHang.EmailLienHe = txtEmailLienHe.Text.Trim();
                khachHang.GhiChu = txtMoTa.Text.Trim();
                khachHang.KichHoat = chkStatus.Checked;

                if (this.GetValue(ddlLoaiKhachHang, out idLoaiKhachHang) && idLoaiKhachHang != Guid.Empty)
                    khachHang.IdLoaiKhachHang = idLoaiKhachHang;

                if (isAdd)
                {
                    khachHang = organizationKhachHangManager.CreateOrUpdate(khachHang);
                    if (khachHang == null)
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    ShowNotify(GetResourceText(BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY));
                }
                else
                {
                    khachHang = organizationKhachHangManager.CreateOrUpdate(khachHang);
                    if (khachHang == null)
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    ShowSuccessSaveData();
                }
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
                return;
            }
            dlDetail.CloseModal();
            CtrlKhachHang.Rebind();
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlKhachHang.ConfirmRequest(e);
        }
    }
}