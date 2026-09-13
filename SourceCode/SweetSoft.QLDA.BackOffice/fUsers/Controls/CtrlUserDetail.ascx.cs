using SubSonic; // BẮT BUỘC THÊM ĐỂ CHẠY LỆNH UPDATE/DELETE SANDBOX
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Web.Security;
using System.Web.UI;
using SweetSoft.QLDA.Core.Infrastructure;
namespace SweetSoft.QLDA.BackOffice.fUsers.Controls
{
    public enum UserPopupMode { SystemUser, Employee }

    public partial class CtrlUserDetail : BaseAdminUserControl
    {
        public EventHandler SavedHandlerCallback;

        public UserPopupMode CurrentMode
        {
            get => ViewState["CurrentMode"] != null ? (UserPopupMode)ViewState["CurrentMode"] : UserPopupMode.SystemUser;
            set => ViewState["CurrentMode"] = value;
        }

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

        // =========================================================================
        // TRÍCH XUẤT AVATAR TỪ PAYLOAD FILESBOX (TRÁNH DESYNC)
        // =========================================================================
        private string ExtractAvatarFromFilesBox(string currentAvatar)
        {
            string newAvatar = currentAvatar;
            bool isDeleted = false;

            foreach (string key in Request.Params.AllKeys)
            {
                if (string.IsNullOrEmpty(key)) continue;

                if (key.EndsWith("txtArFileRemove") && !string.IsNullOrEmpty(Request.Params[key]))
                    isDeleted = true;

                if (key.Contains("filePath$"))
                    newAvatar = Request.Params[key];
            }

            if (isDeleted && newAvatar == currentAvatar) return "";
            return newAvatar;
        }

        public void InitControls()
        {
            new ControlHelpers().BindRoles(ddlRole);
            new ControlHelpers().BindChucDanh(ddlChucDanh);
            new ControlHelpers().BindPhongBan(ddlPhongBan);
        }

        private void SetUIByMode()
        {
            if (CurrentMode == UserPopupMode.SystemUser)
            {
                boxEmployeeInfo.Visible = false;
                divToggleAccount.Visible = false;
                boxAccountInfo.Style["display"] = "block";

                txtUserName.Required = true;
            }
            else
            {
                boxEmployeeInfo.Visible = true;
                divToggleAccount.Visible = true;

                if (!chkEnableAccount.Checked)
                {
                    boxAccountInfo.Style["display"] = "none";
                    txtUserName.Required = false;
                }
                else
                {
                    boxAccountInfo.Style["display"] = "block";
                    txtUserName.Required = true;
                }
            }
        }

        public void AddNew()
        {
            this.DetailUserId = Guid.Empty;

            txtUserName.Text = txtPhone.Text = txtFullName.Text = txtEmail.Text = string.Empty;
            txtPassword.Text = txtConfirmPassword.Text = txtCCCD.Text = txtDiaChi.Text = string.Empty;
            txtNgaySinh.Text = txtNgayGiaNhap.Text = string.Empty;

            ddlGioiTinh.SelectedIndex = ddlRole.SelectedIndex = ddlPhongBan.SelectedIndex = ddlChucDanh.SelectedIndex = 0;
            chkStatus.Checked = true;

            chkEnableAccount.Checked = CurrentMode == UserPopupMode.SystemUser;
            chkEnableAccount.Disabled = false;

            txtUserName.Enabled = true;

            divChangePassword.Visible = false;
            divPassword.Visible = false;
            divPassword.Attributes["data-edit"] = "false";

            fbImage.SingleFilePath = "/Styles/images/user-icon.png";
            fbImage.SingleFilePathType = FileTypes.Internal;
            fbImage.IsMultiple = false;
            fbImage.LoadFile(TempAvatarSessionId, FileUploadTypes.UserAvatar); // DÙNG ID ẢO
            SetUIByMode();

            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
            lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.OpenModal(true);
        }

        public void Edit(Guid userId)
        {
            if (userId == Guid.Empty) return;

            AspnetUser user = UserManager.Instance.GetUserById(userId);
            if (user == null || user.IsDeleted) return;

            this.DetailUserId = user.UserId;

            txtFullName.Text = user.DisplayName;
            txtPhone.Text = user.MobileAlias;
            chkStatus.Checked = user.IsActivated;

            fbImage.SingleFilePath = string.IsNullOrEmpty(user.Avatar) ? "/Styles/images/user-icon.png" : user.Avatar;
            fbImage.SingleFilePathType = FileTypes.Internal;
            fbImage.IsMultiple = false;
            fbImage.LoadFile(TempAvatarSessionId, FileUploadTypes.UserAvatar); // DÙNG ID ẢO

            MembershipUser memUser = Membership.GetUser(user.UserName);
            if (memUser != null && !memUser.Email.Contains("no-email.com"))
                txtEmail.Text = memUser.Email;

            if (CurrentMode == UserPopupMode.Employee)
            {
                txtCCCD.Text = user.IdCCCD;
                txtDiaChi.Text = user.DiaChi;
                if (user.NgaySinh.HasValue) txtNgaySinh.Text = user.NgaySinh.Value.ToString("yyyy-MM-dd");
                if (user.NgayGiaNhap.HasValue) txtNgayGiaNhap.Text = user.NgayGiaNhap.Value.ToString("yyyy-MM-dd");
                if (user.IdPhongBan.HasValue) ddlPhongBan.SelectedValue = user.IdPhongBan.Value.ToString();
                if (user.IdChucDanh.HasValue) ddlChucDanh.SelectedValue = user.IdChucDanh.Value.ToString();
                if (!string.IsNullOrEmpty(user.GioiTinh)) ddlGioiTinh.SelectedValue = user.GioiTinh;
            }

            bool hasAccount = !IsGhostAccount(user.UserName);

            if (!hasAccount)
            {
                txtUserName.Text = string.Empty;
                chkEnableAccount.Checked = false;
                chkEnableAccount.Disabled = false;

                txtUserName.Enabled = true;

                divChangePassword.Visible = false;
                divPassword.Visible = false;
            }
            else
            {
                txtUserName.Text = user.UserName;
                chkEnableAccount.Checked = true;
                chkEnableAccount.Disabled = true;

                txtUserName.Enabled = false;

                divChangePassword.Visible = true;
                divPassword.Visible = true;
                divPassword.Attributes["data-edit"] = "true";
            }

            AspnetRole role = RoleManager.Instance.GetRoleByUserId(user.UserId);
            if (role != null) ddlRole.SelectedValue = role.RoleId.ToString();
            else ddlRole.SelectedIndex = 0;

            SetUIByMode();

            ScriptManager.RegisterStartupScript(this.Page, GetType(), "HidePwd", "$('[data-selector=\"password\"]').removeClass('show');", true);

            dlDetail.Title = GetResourceText(BackEndResourceKeys.ACCOUNT_INFORMATION);
            lbtSubmit.Text = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.OpenModal(true, 1000);
        }

        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(dlDetail.Controls);
                AspnetUser dto = IsEditMode ? UserManager.Instance.GetUserById(DetailUserId) : new AspnetUser();

                if (CurrentMode == UserPopupMode.SystemUser)
                {
                    dto.LaNhanVien = false;
                }
                else
                {
                    dto.LaNhanVien = true;
                    if (string.IsNullOrEmpty(txtCCCD.Text.Trim()))
                        validationEngine.AddErrorPrompt(txtCCCD.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));
                }

                bool hasAccount = false;
                if (IsEditMode && dto != null)
                    hasAccount = !IsGhostAccount(dto.UserName);

                dto.DisplayName = txtFullName.Text.Trim();
                dto.Email = string.IsNullOrEmpty(txtEmail.Text) ? $"{DateTime.UtcNow.Ticks}@no-email.com" : txtEmail.Text.Trim();
                dto.MobileAlias = txtPhone.Text.Trim();
                dto.IsActivated = chkStatus.Checked;

                // THAY THẾ CÁCH LẤY AVATAR CŨ (TRÁNH LỖI KHI DÙNG ID ẢO)
                string oldAvatar = IsEditMode ? dto.Avatar : "";
                dto.Avatar = ExtractAvatarFromFilesBox(oldAvatar);

                if (dto.LaNhanVien)
                {
                    dto.IdCCCD = txtCCCD.Text.Trim();
                    dto.DiaChi = txtDiaChi.Text.Trim();
                    dto.GioiTinh = ddlGioiTinh.SelectedValue;
                    if (DateTime.TryParse(txtNgaySinh.Text, out DateTime tempDOB)) dto.NgaySinh = tempDOB;
                    if (DateTime.TryParse(txtNgayGiaNhap.Text, out DateTime tempJoin)) dto.NgayGiaNhap = tempJoin;

                    if (Guid.TryParse(ddlPhongBan.SelectedValue, out Guid idPB) && idPB != Guid.Empty) dto.IdPhongBan = idPB;
                    if (Guid.TryParse(ddlChucDanh.SelectedValue, out Guid idCD) && idCD != Guid.Empty) dto.IdChucDanh = idCD;
                }

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
                    if (Guid.TryParse(ddlRole.SelectedValue, out Guid rId) && rId != Guid.Empty)
                        dto.RoleId = rId;
                    else
                        dto.RoleId = Guid.Empty;
                }

                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }

                var result = UserManager.Instance.CreateOrUpdate(dto);
                if (result == null)
                {
                    ShowInvalidDataError();
                    return;
                }

                // =========================================================
                // CHỐT HẠ SANDBOX: CHUYỂN QUYỀN SỞ HỮU TỪ ID ẢO SANG ID THẬT
                // =========================================================
                if (result.Avatar != oldAvatar && !string.IsNullOrEmpty(result.Avatar))
                {
                    new SubSonic.Update(TblUploadFile.Schema)
                        .Set(TblUploadFile.Columns.RefId).EqualTo(result.UserId)
                        .Where(TblUploadFile.Columns.RefId).IsEqualTo(TempAvatarSessionId)
                        .Execute();

                    new SubSonic.Delete().From(TblUploadFile.Schema)
                        .Where(TblUploadFile.Columns.RefId).IsEqualTo(result.UserId)
                        .And(TblUploadFile.Columns.FileUrl).IsEqualTo(oldAvatar)
                        .Execute();
                }
                if (result.UserId == SweetContext.Current.UserId)
                {
                    SweetContext.Current.User = null;
                    var master = this.Page.Master as SweetSoft.QLDA.BackOffice.MasterPages.MasterTemplate;
                    if (master != null)
                    {
                        master.SetUserInfomation(result.DisplayName, result.Avatar);
                    }
                }
                ShowNotify(IsEditMode ? GetResourceText(BackEndResourceKeys.DATA_HAS_BEEN_UPDATED_SUCCESSFULLY) : GetResourceText(BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY), MSGType.Success);
                dlDetail.CloseModal();
                SavedHandlerCallback?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception exc)
            {
                string safeErrorMsg = exc.Message
                                         .Replace("'", "\\'")
                                         .Replace("\"", "\\\"")
                                         .Replace("\r", "")
                                         .Replace("\n", " ");

                ShowNotify(safeErrorMsg, MSGType.Error);
            }
        }

        private bool IsGhostAccount(string userName)
        {
            return string.IsNullOrEmpty(userName) || userName.StartsWith("EMP_");
        }
    }
}