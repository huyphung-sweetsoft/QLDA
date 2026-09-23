using SweetSoft.QLDA.BackOffice.Common;
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

namespace SweetSoft.QLDA.BackOffice.fCustomers.Controls
{
    public partial class CtrlKhachHangForm : BaseAdminUserControl
    {
        public event EventHandler SaveCompleted;
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

        protected bool IsAdd
        {
            get
            {
                if (this.CURRENT_PAGE.IsUserRight(ActionKeys.Create, ModuleKeys.Customer))
                    return true;
                return false;
            }
        }

        protected bool IsEdit
        {
            get
            {
                if (this.CURRENT_PAGE.IsUserRight(ActionKeys.Update, ModuleKeys.Customer))
                    return true;
                return false;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
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

                if (!Guid.TryParse(ddlLoaiKhachHang.SelectedValue, out idLoaiKhachHang) || idLoaiKhachHang == Guid.Empty)
                    validationEngine.AddErrorPrompt(ddlLoaiKhachHang.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_SELECT_THE_VALUE));

                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }

                bool isAdd = true;
                KhachHangManager organizationKhachHangManager = KhachHangManager.Instance;
                TblKhachHang khachHang = new TblKhachHang();

                if (IdKhachHang != Guid.Empty)
                {
                    khachHang.IdKhachHang = IdKhachHang;
                    if (!IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    isAdd = false;
                }
                else if (!IsAdd)
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
                khachHang.IdLoaiKhachHang = idLoaiKhachHang;

                khachHang = organizationKhachHangManager.CreateOrUpdate(khachHang);
                if (khachHang == null)
                {
                    ShowInvalidDataError();
                    return;
                }

                if (isAdd)
                    ShowNotify(GetResourceText(BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY));
                else
                    ShowSuccessSaveData();

                dlDetail.CloseModal();
                SaveCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
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

            chkStatus.OnText = "Đang hợp tác";
            chkStatus.OffText = "Ngừng hợp tác";
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
                = txtDiaChi.Text
                = txtMoTa.Text
                = "";

            chkStatus.Checked = true;
            this.IdKhachHang = Guid.Empty;
        }

        public void OpenAdd()
        {
            ApplyControlsText();
            RefreshCustomerInfo();
            lbtSubmit.Visible = IsAdd;
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
            dlDetail.OpenModal(true);
        }

        public void OpenEdit(Guid idKhachHang)
        {
            if (idKhachHang == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            ApplyControlsText();
            RefreshCustomerInfo();

            TblKhachHang khachHang = KhachHangManager.Instance.GetKhachHangById(idKhachHang);
            if (khachHang == null || khachHang.DaXoa)
            {
                Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), false);
                return;
            }

            IdKhachHang = khachHang.IdKhachHang;
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

            lbtSubmit.Visible = IsEdit;
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.OpenModal(true);
        }
    }
}