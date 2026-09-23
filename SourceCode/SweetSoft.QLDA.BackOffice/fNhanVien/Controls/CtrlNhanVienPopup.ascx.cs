using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Web.Security;
using System.Web.UI;
using SweetSoft.QLDA.Core.Infrastructure;

namespace SweetSoft.QLDA.BackOffice.fNhanVien.Controls
{ 
    public partial class CtrlNhanVienPopup : BaseAdminUserControl
    {
        public EventHandler SavedHandlerCallback;

        private Guid DetailUserId
        {
            get => ViewState["DetailUserId"] != null ? (Guid)ViewState["DetailUserId"] : Guid.Empty;
            set => ViewState["DetailUserId"] = value;
        }

        protected bool IsEditMode => DetailUserId != Guid.Empty;

        private Guid TempAvatarSessionId
        {
            get
            {
                if (ViewState["TempAvatarSessionId"] == null)
                    ViewState["TempAvatarSessionId"] = Guid.NewGuid();
                return (Guid)ViewState["TempAvatarSessionId"];
            }
        }

        protected void Page_Load(object sender, EventArgs e) { }

        public void InitControls()
        {
            ApplyControlsText();
            new ControlHelpers().BindRoles(ddlRole);
            new ControlHelpers().BindChucDanh(ddlChucDanh);
            new ControlHelpers().BindPhongBan(ddlPhongBan);
        }

        private void ApplyControlsText()
        {
            txtFullName.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            txtUserName.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            txtEmail.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            txtPhone.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            txtCCCD.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            txtDiaChi.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            txtPassword.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            txtConfirmPassword.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);

            ddlRole.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            ddlPhongBan.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            ddlChucDanh.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);

            chkStatus.OnText = GetResourceText(BackEndResourceKeys.WORKING);
            chkStatus.OffText = GetResourceText(BackEndResourceKeys.RESIGNED);
        }

        private string ExtractAvatarFromFilesBox(string currentAvatar)
        {
            string newAvatar = currentAvatar;
            bool isDeleted = false;

            foreach (string key in Request.Params.AllKeys)
            {
                if (string.IsNullOrEmpty(key)) continue;
                if (key.EndsWith("txtArFileRemove") && !string.IsNullOrEmpty(Request.Params[key])) isDeleted = true;
                if (key.Contains("filePath$")) newAvatar = Request.Params[key];
            }

            if (isDeleted && newAvatar == currentAvatar) return "";
            return newAvatar;
        }

        private bool IsGhostAccount(string userName)
        {
            return string.IsNullOrEmpty(userName) || userName.StartsWith("EMP_");
        }

        private void RefreshAccountInfor()
        {
            this.DetailUserId = Guid.Empty;
            ViewState["TempAvatarSessionId"] = Guid.NewGuid();

            txtUserName.Text = txtPhone.Text = txtFullName.Text = txtEmail.Text = string.Empty;
            txtPassword.Text = txtConfirmPassword.Text = txtCCCD.Text = txtDiaChi.Text = string.Empty;
            txtNgaySinh.Text = txtNgayGiaNhap.Text = string.Empty;

            ddlGioiTinh.SelectedIndex = ddlRole.SelectedIndex = ddlPhongBan.SelectedIndex = ddlChucDanh.SelectedIndex = 0;
            chkStatus.Checked = true;
            chkChangePassword.Checked = false;

            chkEnableAccount.Checked = false;
            chkEnableAccount.Disabled = false;
            txtUserName.Enabled = true;
            txtUserName.Required = false;

            boxAccountInfo.Style["display"] = "none";
            divChangePassword.Visible = false;
            divPassword.Visible = false;
            divPassword.Attributes["data-edit"] = "false";
        }

        private void LoadDefaultAvatar()
        {
            fbImage.SingleFilePath = "/Styles/images/user-icon.png";
            fbImage.SingleFilePathType = FileTypes.Internal;
            fbImage.IsMultiple = false;
            fbImage.LoadFile(TempAvatarSessionId, FileUploadTypes.UserAvatar);
        }

        public void AddNew()
        {
            RefreshAccountInfor();
            LoadDefaultAvatar();

            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
            lbtSubmit.Text = lbtSubmit.ToolTip = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.OpenModal(true);
        }

        public void Edit(Guid userId)
        {
            if (userId == Guid.Empty) return;
            RefreshAccountInfor();

            AspnetUser user = UserManager.Instance.GetUserById(userId);
            if (user == null || user.IsDeleted) return;

            this.DetailUserId = user.UserId;

            txtFullName.Text = user.DisplayName;
            txtPhone.Text = user.MobileAlias;
            chkStatus.Checked = user.IsActivated;
            txtCCCD.Text = user.IdCCCD;
            txtDiaChi.Text = user.DiaChi;

            if (user.NgaySinh.HasValue) txtNgaySinh.Text = user.NgaySinh.Value.ToString("yyyy-MM-dd");
            if (user.NgayGiaNhap.HasValue) txtNgayGiaNhap.Text = user.NgayGiaNhap.Value.ToString("yyyy-MM-dd");
            if (user.IdPhongBan.HasValue) ddlPhongBan.SelectedValue = user.IdPhongBan.Value.ToString();
            if (user.IdChucDanh.HasValue) ddlChucDanh.SelectedValue = user.IdChucDanh.Value.ToString();
            if (!string.IsNullOrEmpty(user.GioiTinh)) ddlGioiTinh.SelectedValue = user.GioiTinh;

            MembershipUser memUser = Membership.GetUser(user.UserName);
            if (memUser != null && !memUser.Email.Contains("no-email.com"))
                txtEmail.Text = memUser.Email;

            bool hasAccount = !IsGhostAccount(user.UserName);
            if (!hasAccount)
            {
                txtUserName.Text = string.Empty;
                chkEnableAccount.Checked = false;
                chkEnableAccount.Disabled = false;
                txtUserName.Enabled = true;
                txtUserName.Required = false;
                boxAccountInfo.Style["display"] = "none";
                divChangePassword.Visible = false;
                divPassword.Visible = false;
                divPassword.Attributes["data-edit"] = "false";
            }
            else
            {
                txtUserName.Text = user.UserName;
                chkEnableAccount.Checked = true;
                chkEnableAccount.Disabled = true;
                txtUserName.Enabled = false;
                txtUserName.Required = true;
                boxAccountInfo.Style["display"] = "block";
                divChangePassword.Visible = true;
                divPassword.Visible = true;
                divPassword.Attributes["data-edit"] = "true";
            }

            AspnetRole role = RoleManager.Instance.GetRoleByUserId(user.UserId);
            if (role != null) ddlRole.SelectedValue = role.RoleId.ToString();
            else ddlRole.SelectedIndex = 0;

            fbImage.SingleFilePath = string.IsNullOrEmpty(user.Avatar) ? "/Styles/images/user-icon.png" : user.Avatar;
            fbImage.SingleFilePathType = FileTypes.Internal;
            fbImage.IsMultiple = false;
            fbImage.LoadFile(TempAvatarSessionId, FileUploadTypes.UserAvatar);

            ScriptManager.RegisterStartupScript(this.Page, GetType(), "HidePwd", "$('[data-selector=\"password\"]').removeClass('show');", true);

            dlDetail.Title = GetResourceText(BackEndResourceKeys.UPDATE);
            lbtSubmit.Text = lbtSubmit.ToolTip = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.OpenModal(true);
        }

        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(dlDetail.Controls);

                // =========================================================
                // LOGIC CHỐT CHẶN: KIỂM TRA TRÙNG EMAIL (2 BƯỚC RÕ RÀNG)
                // =========================================================
                string inputEmail = string.IsNullOrEmpty(txtEmail.Text) ? string.Empty : txtEmail.Text.Trim();
                if (!string.IsNullOrEmpty(inputEmail))
                {
                    // BƯỚC 1: Kiểm tra xem có trùng với tài khoản QUẢN TRỊ nào không
                    if (UserManager.Instance.IsEmailExistInAdminGroup(DetailUserId, inputEmail))
                    {
                        validationEngine.AddErrorPrompt(txtEmail.ClientID, "Email bị trùng với một tài khoản quản trị");
                        validationEngine.ShowErrorPrompt();
                        return; // Ngắt luồng ngay tại đây
                    }

                    // BƯỚC 2: Kiểm tra xem có trùng với BẤT KỲ tài khoản nào khác không
                    // (Lúc này nếu trùng thì chắc chắn là trùng với Nhân viên khác, vì Admin đã lọt qua Bước 1)
                    if (UserManager.Instance.IsEmailExist(DetailUserId, inputEmail))
                    {
                        // Sử dụng Resource đa ngôn ngữ gốc của hệ thống ông cho đồng bộ
                        validationEngine.AddErrorPrompt(txtEmail.ClientID, GetResourceText(BackEndResourceKeys.EMAIL_ALREADY_EXISTS));
                        validationEngine.ShowErrorPrompt();
                        return; // Ngắt luồng ngay tại đây
                    }
                }

                AspnetUser dto = IsEditMode ? UserManager.Instance.GetUserById(DetailUserId) : new AspnetUser();
                if (dto == null) { ShowInvalidDataError(); return; }

                dto.LaNhanVien = true;

                if (string.IsNullOrEmpty(txtCCCD.Text.Trim()))
                    validationEngine.AddErrorPrompt(txtCCCD.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));

                bool hasAccount = false;
                if (IsEditMode) hasAccount = !IsGhostAccount(dto.UserName);

                dto.DisplayName = txtFullName.Text.Trim();
                dto.Email = inputEmail;
                dto.MobileAlias = txtPhone.Text.Trim();
                dto.IsActivated = chkStatus.Checked;

                string oldAvatar = IsEditMode ? dto.Avatar : "";
                dto.Avatar = ExtractAvatarFromFilesBox(oldAvatar);

                dto.IdCCCD = txtCCCD.Text.Trim();
                dto.DiaChi = txtDiaChi.Text.Trim();
                dto.GioiTinh = ddlGioiTinh.SelectedValue;

                // GIỮ NGUYÊN LOGIC GÁN NGÀY THÁNG/DROPDOWN CỦA CTRLUSERDETAIL.ASCX.CS GỐC
                if (DateTime.TryParse(txtNgaySinh.Text, out DateTime tempDOB)) dto.NgaySinh = tempDOB;
                if (DateTime.TryParse(txtNgayGiaNhap.Text, out DateTime tempJoin)) dto.NgayGiaNhap = tempJoin;
                if (Guid.TryParse(ddlPhongBan.SelectedValue, out Guid idPB) && idPB != Guid.Empty) dto.IdPhongBan = idPB;
                if (Guid.TryParse(ddlChucDanh.SelectedValue, out Guid idCD) && idCD != Guid.Empty) dto.IdChucDanh = idCD;

                if (!chkEnableAccount.Checked)
                {
                    dto.UserName = null;
                    dto.Password = null;
                    dto.RoleId = Guid.Empty;
                }
                else
                {
                    if (!hasAccount)
                    {
                        dto.UserName = txtUserName.Text.Trim();
                        dto.Password = null;
                    }
                    else
                    {
                        dto.UserName = txtUserName.Text.Trim();
                        if (chkChangePassword.Checked)
                        {
                            if (string.IsNullOrEmpty(txtPassword.Text.Trim()))
                                validationEngine.AddErrorPrompt(txtPassword.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));
                            else if (txtPassword.Text.Trim().Length < 6)
                                validationEngine.AddErrorPrompt(txtPassword.ClientID, GetResourceText(BackEndResourceKeys.PASSWORD_MUST_HAVE_MINIMUM_OF_6_CHARACTERS));

                            if (txtConfirmPassword.Text.Trim() != txtPassword.Text.Trim())
                                validationEngine.AddErrorPrompt(txtConfirmPassword.ClientID, GetResourceText(BackEndResourceKeys.RE_ENTER_INCORRECT_PASSWORD));

                            dto.Password = txtPassword.Text;
                        }
                        else
                        {
                            dto.Password = null;
                        }
                    }
                    if (Guid.TryParse(ddlRole.SelectedValue, out Guid rId) && rId != Guid.Empty) dto.RoleId = rId;
                    else dto.RoleId = Guid.Empty;
                }

                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }

                var result = UserManager.Instance.CreateOrUpdate(dto);
                if (result == null) { ShowInvalidDataError(); return; }

                if (result.Avatar != oldAvatar && !string.IsNullOrEmpty(result.Avatar))
                {
                    new SubSonic.Update(TblUploadFile.Schema)
                        .Set(TblUploadFile.Columns.RefId).EqualTo(result.UserId)
                        .Where(TblUploadFile.Columns.RefId).IsEqualTo(TempAvatarSessionId)
                        .And(TblUploadFile.Columns.FileUrl).IsEqualTo(result.Avatar)
                        .Execute();

                    new SubSonic.Delete().From(TblUploadFile.Schema)
                        .Where(TblUploadFile.Columns.RefId).IsEqualTo(result.UserId)
                        .And(TblUploadFile.Columns.FileUrl).IsEqualTo(oldAvatar)
                        .Execute();

                    new SubSonic.Delete().From(TblUploadFile.Schema)
                        .Where(TblUploadFile.Columns.RefId).IsEqualTo(TempAvatarSessionId)
                        .Execute();
                }

                if (result.UserId == SweetContext.Current.UserId)
                {
                    SweetContext.Current.User = null;
                    if (this.Page.Master is SweetSoft.QLDA.BackOffice.MasterPages.MasterTemplate master)
                        master.SetUserInfomation(result.DisplayName, result.Avatar);
                }

                ShowNotify(IsEditMode ? GetResourceText(BackEndResourceKeys.DATA_HAS_BEEN_UPDATED_SUCCESSFULLY) : GetResourceText(BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY), MSGType.Success);
                dlDetail.CloseModal();
                SavedHandlerCallback?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message.Replace("'", "\\'").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " "), MSGType.Error);
            }
        }
    }
}