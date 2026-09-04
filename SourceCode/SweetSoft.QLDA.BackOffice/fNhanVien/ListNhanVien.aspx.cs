using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fNhanVien.Controls;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
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
using static SweetSoft.QLDA.Controls.EnumHelper;

namespace SweetSoft.QLDA.BackOffice.fNhanVien
{
    public partial class ListNhanVien : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.NhanVien;
            }
        }

        private Guid IdNhanVien
        {
            get
            {
                if (ViewState["IdNhanVien"] != null)
                    return (Guid)ViewState["IdNhanVien"];
                return Guid.Empty;
            }
            set
            {
                ViewState["IdNhanVien"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlNhanViens1.NewNhanVienHandlerCallback += NewNhanVienAction;
            CtrlNhanViens1.EditNhanVienHandlerCallback += EditNhanVienAction;

            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);

                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.EMPLOYEE_MANAGEMENT));

                if (Navigation1 != null)
                {
                    Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.EMPLOYEE_LIST);
                    Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                    {
                        { RewriteURLHelper.NhanVien, GetResourceText(BackEndResourceKeys.EMPLOYEE_LIST) }
                    };
                }

                CtrlNhanViens1.InitControls();

                string idQuery = CommonHelpers.QueryString("id");
                if (!string.IsNullOrEmpty(idQuery))
                {
                    Guid tempId = Guid.Empty;
                    if (Guid.TryParse(SecurityUtilities.UnprotectUrlParameter(idQuery), out tempId))
                    {
                        EditNhanVienAction(tempId, EventArgs.Empty);
                    }
                }
            }
        }

        #region Modal
        private void RefreshUserInfo()
        {
            ControlHelpers controlHelpers = new ControlHelpers();
            controlHelpers.BindPhongBan(ddlPhongBan);
            controlHelpers.BindChucDanh(ddlChucDanh);
         

            lbtSubmit.Visible = false;
            txtTenNhanVien.Text = txtCCCD.Text = txtEmail.Text = txtPhone.Text = txtDiaChi.Text = "";
            ddlGioiTinh.SelectedIndex = 0;
            // Xóa dòng cũ: txtNgaySinh.DateValue = null;
            txtNgaySinh.Text = "";
            txtNgayGiaNhap.Text = "";
            ddlPhongBan.SelectedIndex = ddlChucDanh.SelectedIndex = 0;

            txtEmail.Enabled = true;
            this.IdNhanVien = Guid.Empty;

            fbImage.SingleFilePath = "/Styles/images/user-icon.png";
            fbImage.SingleFilePathType = FileTypes.Internal;
            fbImage.IsMultiple = false;
            fbImage.LoadFile(this.IdNhanVien, FileUploadTypes.UserAvatar);
        }

        private void EditNhanVienAction(object sender, EventArgs e)
        {
            if (sender == null) { ShowInvalidDataError(); return; }
            Guid idNhanVien = (Guid)sender;
            if (idNhanVien == Guid.Empty) { ShowInvalidDataError(); return; }

            RefreshUserInfo();
            lbtSubmit.Visible = this.IsEdit;

            TblNhanVien nhanVien = NhanVienManager.Instance.GetNhanVienById(idNhanVien);
            // Đã sửa lỗi kiểm tra DaXoa ở đây cho an toàn tuyệt đối
            if (nhanVien == null || nhanVien.DaXoa)
            {
                Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), false);
                return;
            }

            this.IdNhanVien = nhanVien.IdNhanVien;

            fbImage.LoadFile(nhanVien.UserId.HasValue ? nhanVien.UserId.Value : this.IdNhanVien, FileUploadTypes.UserAvatar);

            txtTenNhanVien.Text = nhanVien.TenNhanVien;
            txtCCCD.Text = nhanVien.IdCCCD;
            txtDiaChi.Text = nhanVien.DiaChi;
            if (!string.IsNullOrEmpty(nhanVien.GioiTinh) && ddlGioiTinh.Items.FindByValue(nhanVien.GioiTinh) != null)
                ddlGioiTinh.SelectedValue = nhanVien.GioiTinh;
            if (nhanVien.IdPhongBan.HasValue) ddlPhongBan.SelectedValue = nhanVien.IdPhongBan.Value.ToString();
            if (nhanVien.IdChucDanh.HasValue) ddlChucDanh.SelectedValue = nhanVien.IdChucDanh.Value.ToString();

            // ĐÃ SỬA LỖI GÁN NHẦM NGÀY GIA NHẬP THÀNH NGÀY SINH Ở ĐÂY:
            // Xóa 2 dòng kiểm tra HasValue cũ và thay bằng:
            if (nhanVien.NgaySinh.HasValue)
                txtNgaySinh.Text = nhanVien.NgaySinh.Value.ToString("yyyy-MM-dd");
            else
                txtNgaySinh.Text = "";

            if (nhanVien.NgayGiaNhap.HasValue)
                txtNgayGiaNhap.Text = nhanVien.NgayGiaNhap.Value.ToString("yyyy-MM-dd");
            else
                txtNgayGiaNhap.Text = "";

            if (nhanVien.UserId.HasValue)
            {
                AspnetUser user = UserManager.Instance.GetUserById(nhanVien.UserId.Value);
                if (user != null)
                {
                    txtPhone.Text = user.MobileAlias;

                    System.Web.Security.MembershipUser membershipUser = System.Web.Security.Membership.GetUser(user.UserName);
                    if (membershipUser != null && !membershipUser.Email.Contains("no-email.com"))
                    {
                        txtEmail.Text = membershipUser.Email;
                        txtEmail.Enabled = false;
                    }

               
                }
            }

            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.Title = "Cập nhật thông tin nhân viên";

            if (!IsPostBack) dlDetail.OpenModal(true, 1000);
            else dlDetail.OpenModal(true);
        }

        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(dlDetail.Controls);

                if (!string.IsNullOrEmpty(txtPhone.Text) && !RegexUtilities.IsValidPhone(txtPhone.Text))
                    validationEngine.AddErrorPrompt(txtPhone.ClientID, GetResourceText(BackEndResourceKeys.INVALID_PHONE_NUMBER));

                if (!string.IsNullOrEmpty(txtCCCD.Text))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(txtCCCD.Text, @"^\d+$"))
                        validationEngine.AddErrorPrompt(txtCCCD.ClientID, "CCCD chỉ được chứa số.");
                    else if (txtCCCD.Text.Length != 12)
                        validationEngine.AddErrorPrompt(txtCCCD.ClientID, "CCCD phải đúng 12 số.");
                }

                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }

                bool isAdd = true;
                TblNhanVien nhanVien;

                if (this.IdNhanVien != Guid.Empty)
                {
                    if (!this.IsEdit) { ShowAccessDeniedNotify(); return; }
                    isAdd = false;

                    // MẤU CHỐT: Lấy lại dữ liệu cũ từ Database để không bị ghi đè mất Data
                    nhanVien = NhanVienManager.Instance.GetNhanVienById(this.IdNhanVien);
                    if (nhanVien == null) { ShowInvalidDataError(); return; }
                }
                else
                {
                    if (!this.IsAdd) { ShowAccessDeniedNotify(); return; }
                    nhanVien = new TblNhanVien();
                }

                // -- ĐÓNG GÓI DỮ LIỆU TỪ FORM --
                nhanVien.TenNhanVien = txtTenNhanVien.Text.Trim();
                nhanVien.IdCCCD = txtCCCD.Text.Trim();
                nhanVien.Email = string.IsNullOrEmpty(txtEmail.Text) ? $"{DateTime.UtcNow.Ticks}@no-email.com" : txtEmail.Text.Trim();
                nhanVien.PhoneNumber = txtPhone.Text.Trim();
                nhanVien.GioiTinh = ddlGioiTinh.SelectedValue;
                nhanVien.DiaChi = txtDiaChi.Text.Trim();

                // Chấp nhận lấy cả giá trị rỗng để cho phép xóa ảnh
                if (fbImage != null)
                    nhanVien.AnhDaiDien = fbImage.SingleFilePath;

                Guid idTemp = Guid.Empty;
                if (this.GetValue(ddlPhongBan, out idTemp) && idTemp != Guid.Empty) nhanVien.IdPhongBan = idTemp;
                if (this.GetValue(ddlChucDanh, out idTemp) && idTemp != Guid.Empty) nhanVien.IdChucDanh = idTemp;


                // Đóng gói Ngày sinh và Ngày gia nhập từ TextBox
                DateTime tempDate;
                if (DateTime.TryParse(txtNgaySinh.Text, out tempDate))
                    nhanVien.NgaySinh = tempDate;
                else
                    nhanVien.NgaySinh = null;

                if (DateTime.TryParse(txtNgayGiaNhap.Text, out tempDate))
                    nhanVien.NgayGiaNhap = tempDate;
                else
                    nhanVien.NgayGiaNhap = null;

                nhanVien = NhanVienManager.Instance.CreateOrUpdate(nhanVien);

                if (nhanVien != null)
                {
                    if (isAdd) ShowNotify(GetResourceText(BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY));
                    else ShowSuccessSaveData();
                }
                else
                {
                    ShowInvalidDataError();
                    return;
                }
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
                return;
            }

            dlDetail.CloseModal();
            CtrlNhanViens1.Rebind();
        }
        #endregion

        #region Confirm Request
        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlNhanViens1.ConfirmRequest(e);
        }

        public override void CloseRequest(ConfirmResult e)
        {
            base.CloseRequest(e);
        }
        #endregion

        #region Button
        private void NewNhanVienAction(object sender, EventArgs e)
        {
            RefreshUserInfo();
            lbtSubmit.Visible = this.IsAdd;
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
            dlDetail.OpenModal(true);
        }
        #endregion
    }
}