using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.MailManager;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Web.Security;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fUsers.Controls
{
    public partial class CtrlUserPopup : BaseAdminUserControl
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
        }
        private void ApplyControlsText()
        {
            txtFullName.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            txtUserName.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            txtEmail.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            txtPhone.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            ddlRole.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
        }
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

        private bool IsGhostAccount(string userName)
        {
            return string.IsNullOrEmpty(userName) || userName.StartsWith("EMP_");
        }
        private void RefreshAccountInfor()
        {
            this.DetailUserId = Guid.Empty;
            ViewState["TempAvatarSessionId"] = Guid.NewGuid();
            txtUserName.Text =
                txtPhone.Text =
                txtFullName.Text =
                txtEmail.Text = string.Empty;
            ddlRole.SelectedIndex = 0;
            txtUserName.Enabled = true;
            divPasswordAutoInfo.Visible = true;
            divResetPasswordBtn.Visible = false;
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
            MembershipUser memUser = Membership.GetUser(user.UserName);
            if (memUser != null && !memUser.Email.Contains("no-email.com"))
                txtEmail.Text = memUser.Email;

            bool hasAccount = !IsGhostAccount(user.UserName);

            if (!hasAccount)
            {
                txtUserName.Text = string.Empty;
                txtUserName.Enabled = true;
                divPasswordAutoInfo.Visible = true;
                divResetPasswordBtn.Visible = false;
            }
            else
            {
                txtUserName.Text = user.UserName;
                txtUserName.Enabled = false;
                divPasswordAutoInfo.Visible = false;
                divResetPasswordBtn.Visible = true;
            }
            AspnetRole role = RoleManager.Instance.GetRoleByUserId(user.UserId);
            if (role != null) ddlRole.SelectedValue = role.RoleId.ToString();
            else ddlRole.SelectedIndex = 0;

            // Load avatar đúng 1 lần ở Edit
            fbImage.SingleFilePath = string.IsNullOrEmpty(user.Avatar)
                ? "/Styles/images/user-icon.png"
                : user.Avatar;

            fbImage.SingleFilePathType = FileTypes.Internal;
            fbImage.IsMultiple = false;
            fbImage.LoadFile(TempAvatarSessionId, FileUploadTypes.UserAvatar);

            ScriptManager.RegisterStartupScript(
                this.Page,
                GetType(),
                "HidePwd",
                "$('[data-selector=\"password\"]').removeClass('show');",
                true);

            dlDetail.Title = GetResourceText(BackEndResourceKeys.UPDATE);
            lbtSubmit.Text = lbtSubmit.ToolTip = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.OpenModal(true);
        }
        // BẮT SỰ KIỆN KHI BẤM NÚT "CẤP LẠI MẬT KHẨU" TRONG POPUP
        protected void lbtResetPassword_Click(object sender, EventArgs e)
        {
            if (this.DetailUserId == Guid.Empty) return;

            AspnetUser user = UserManager.Instance.GetUserById(this.DetailUserId);
            if (user == null || IsGhostAccount(user.UserName)) return;

            ConfirmResult result = new ConfirmResult();
            result.CommandName = "USER_RESET_PASSWORD";
            result.Value = user;
            this.CURRENT_PAGE.CurrentConfirmResult = result;

            MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION)
                , string.Format("Bạn có chắc chắn muốn cấp lại mật khẩu cho tài khoản {0}?", user.DisplayName)
                , MSGButton.Send, MSGIcon.Warning);
            OpenMessageBox(msg, result, false, false);
        }

        // BẮT SỰ KIỆN "ĐỒNG Ý" TỪ HỘP THOẠI TRÊN
        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null && e.Submit && e.CommandName != null)
            {
                if (e.CommandName.Contains("USER_RESET_PASSWORD"))
                {
                    AspnetUser user = e.Value as AspnetUser;
                    if (user == null) return;

                    try
                    {
                        bool isDefault = false;
                        if (System.Web.Configuration.WebConfigurationManager.AppSettings["IsUsedDefaultPassword"] != null)
                            isDefault = bool.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings["IsUsedDefaultPassword"]);

                        string newPassword = isDefault ? this.CURRENT_PAGE.DefaultPassword : SecurityUtilities.CreateAlphaNumericString(8);

                        // Đánh thẳng vào Membership
                        MembershipUser membershipUser = Membership.GetUser(user.UserName);
                        if (membershipUser == null)
                        {
                            this.CURRENT_PAGE.ShowNotify("Lỗi: Không tìm thấy tài khoản để cấp lại.", MSGType.Error);
                            return;
                        }

                        string oldPass = membershipUser.ResetPassword();
                        membershipUser.ChangePassword(oldPass, newPassword);
                        Membership.UpdateUser(membershipUser);

                        // Gửi Email độc lập
                        Guid userIdToSend = user.UserId;
                        string emailToSend = membershipUser.Email;
                        string displayNameToSend = user.DisplayName;
                        string userNameToSend = user.UserName;

                        System.Threading.Tasks.Task.Run(async () =>
                        {
                            try
                            {
                                await System.Threading.Tasks.Task.Delay(1500);
                                var emailManager = new SweetSoft.QLDA.Core.MailManager.EmailManager(null);
                                var placeholdersBody = new System.Collections.Generic.Dictionary<string, string>
                                {
                                    { "[[COMPANY_NAME]]", "SweetSoft" },
                                    { "[[FULL_NAME]]", displayNameToSend },
                                    { "[[USER_NAME]]", userNameToSend },
                                    { "[[PASSWORD]]", newPassword },
                                    { "[[EMAIL]]", emailToSend },
                                    { "[[SUPPORT_EMAIL]]", "hotro@sweetsoft.vn" },
                                    { "[[LOGIN_URL]]", "https://qlda.dev.sweetsoft.co/Login" }
                                };

                                await emailManager.SendEmailWithTemplateAsync(
                                    refId: userIdToSend,
                                    refType: EmailType.Notification,
                                    customerId: userIdToSend,
                                    toEmail: emailToSend,
                                    templateKey: "TemplateAccountInformation",
                                    formatType: EmailFormatTypes.Admin,
                                    placeholdersBody: placeholdersBody,
                                    attachments: null,
                                    useBackgroundThread: false
                                );
                            }
                            catch (Exception ex)
                            {
                                SysLogger.LogError(ex, $"Lỗi gửi email cấp lại mật khẩu: {userNameToSend}");
                            }
                        });

                        dlDetail.CloseModal();
                        SavedHandlerCallback?.Invoke(this, EventArgs.Empty);
                        this.CURRENT_PAGE.ShowNotify(GetResourceText(BackEndResourceKeys.THE_NEW_PASSWORD_HAS_BEEN_SENT_TO_THE_ACCOUNT_S_EMAIL_ADDRESS), MSGType.Success);
                    }
                    catch (Exception exc)
                    {
                        this.CURRENT_PAGE.ShowNotify(exc.Message.Replace("'", "\\'").Replace("\n", ""), MSGType.Error);
                    }
                }
            }
        }
        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(dlDetail.Controls);

                AspnetUser dto = IsEditMode ? UserManager.Instance.GetUserById(DetailUserId) : new AspnetUser();
                if (dto == null)
                {
                    ShowInvalidDataError();
                    return;
                }
                dto.LaNhanVien = false; // Mode System Account

                bool hasAccount = false;
                if (IsEditMode && dto != null)
                    hasAccount = !IsGhostAccount(dto.UserName);

                dto.DisplayName = txtFullName.Text.Trim();
                dto.Email = string.IsNullOrEmpty(txtEmail.Text) ? $"{DateTime.UtcNow.Ticks}@no-email.com" : txtEmail.Text.Trim();
                dto.MobileAlias = txtPhone.Text.Trim();
                if (!hasAccount)
                {
                    dto.UserName = txtUserName.Text.Trim();
                    dto.Password = null; // UserManager sẽ tự sinh pass khi Add New
                }
                else
                {
                    dto.UserName = txtUserName.Text.Trim();
                    dto.Password = null; // Khi Update, không đổi pass ở đây nữa
                }
                // Nếu Thêm mới -> Mặc định tài khoản mở (true). Nếu Cập nhật -> Giữ nguyên trạng thái cũ (dto đã mang sẵn data từ DB).
                if (!IsEditMode) dto.IsActivated = true;
                string oldAvatar = IsEditMode ? dto.Avatar : "";
                dto.Avatar = ExtractAvatarFromFilesBox(oldAvatar);

                if (Guid.TryParse(ddlRole.SelectedValue, out Guid rId) && rId != Guid.Empty)
                    dto.RoleId = rId;
                else
                    dto.RoleId = Guid.Empty;

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

                if (result.Avatar != oldAvatar && !string.IsNullOrEmpty(result.Avatar))
                {
                    // [GIẢI PHÁP]: DỌN RÁC 3 BƯỚC BẰNG SUBSONIC
                    // 1. Chỉ giữ lại ảnh cuối cùng được submit
                    new SubSonic.Update(TblUploadFile.Schema)
                        .Set(TblUploadFile.Columns.RefId).EqualTo(result.UserId)
                        .Where(TblUploadFile.Columns.RefId).IsEqualTo(TempAvatarSessionId)
                        .And(TblUploadFile.Columns.FileUrl).IsEqualTo(result.Avatar)
                        .Execute();

                    // 2. Xóa ảnh cũ trước đó của User
                    new SubSonic.Delete().From(TblUploadFile.Schema)
                        .Where(TblUploadFile.Columns.RefId).IsEqualTo(result.UserId)
                        .And(TblUploadFile.Columns.FileUrl).IsEqualTo(oldAvatar)
                        .Execute();

                    // 3. Xóa vĩnh viễn các ảnh rác của phiên ảo (upload hụt)
                    new SubSonic.Delete().From(TblUploadFile.Schema)
                        .Where(TblUploadFile.Columns.RefId).IsEqualTo(TempAvatarSessionId)
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
                if (SavedHandlerCallback != null)
                {
                    SavedHandlerCallback(this, EventArgs.Empty);
                }
            }
            catch (Exception exc)
            {
                string safeErrorMsg = exc.Message.Replace("'", "\\'").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ");
                ShowNotify(safeErrorMsg, MSGType.Error);
            }
        }
    }
}