//-----------------------PROGRAMER LOGS---------------------------
using Google.Authenticator;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Caches;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Web.Security;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice
{
    public partial class Profile : BaseAdminPage
    {
        public override bool IsLogin
        {
            get
            {
                return true;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButton();
            CtrlTwoFAForChangePassword.CallbackSuccess += CtrlTwoFAForChangePassword_CallbackSuccess;
            CtrlToggleTwoFA.CallbackSuccess += CtrlToggleTwoFA_CallbackSuccess;
            if (!IsPostBack)
            {
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.PROFILE));
                this.FolderPath = "~/uploads/profile";
                this.FolderName = GetResourceText(BackEndResourceKeys.PROFILE);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    {RewriteURLHelper.Profile, GetResourceText(BackEndResourceKeys.PROFILE) }
                };
                ApplyControlsText();
                BindData();
            }
        }

        private void ApplyControlsText()
        {
            Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.PROFILE);
            txtConfirmPassword.PlaceHolder = txtFullName.PlaceHolder
                = txtEmail.PlaceHolder = txtNewPassword.PlaceHolder
                = txtOldPassword.PlaceHolder = txtPhone.PlaceHolder
                = txtUserName.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            lbtChangePassword.ToolTip = lbtChangePassword.Text = GetResourceText(BackEndResourceKeys.CONFIRM);
            lbtUpdate.ToolTip = lbtUpdate.Text = GetResourceText(BackEndResourceKeys.UPDATE);
            //chkEnableNotification.OnText = GetResourceText(BackEndResourceKeys.ON);
            //chkEnableNotification.OffText = GetResourceText(BackEndResourceKeys.OFF);
        }

        private void RegisterAsyncButton()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            script.RegisterAsyncPostBackControl(lbtUpdate);
            script.RegisterAsyncPostBackControl(lbtChangePassword);
        }
        protected override void BindData()
        {
            try
            {
                AspnetUser user = UserManager.Instance.GetUserById(SweetContext.Current.UserId);
                if (user == null)
                    return;
                txtUserName.Text = user.UserName;
                txtFullName.Text = user.DisplayName;
                txtPhone.Text = user.MobileAlias; 
                //---------------------------------------
                //fbImage.SingleFilePath = "/Styles/images/user-icon.png";
                //fbImage.SingleFilePathType = FileTypes.Internal;
                //fbImage.IsMultiple = false;
                //fbImage.LoadFile(user.UserId, FileUploadTypes.UserAvatar);
                //---------------------------------------
                MembershipUser membershipUser = Membership.GetUser(user.UserName);
                if (membershipUser != null)
                    txtEmail.Text = membershipUser.Email;
                //----------------------------------
                BindTwoFactorAuthentication(user.UserName, user.AuthenticatorKey);
            }
            catch (Exception exc)
            {
                ShowSystemError();
                throw new Exception("Profile", exc);
            }
        }

        private void BindTwoFactorAuthentication(string userName, string authenticatorKey)
        {
            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            string appName = $"QLPHLH-STC";
            Guid guid = Guid.NewGuid();
            string secrectKey = Convert.ToString(guid).Replace("-", "");
            var setupInfo = tfa.GenerateSetupCode(appName, userName, secrectKey, false, 5);
            bool statusTwoFA = !string.IsNullOrEmpty(authenticatorKey);
            imgScretKey.Alt = spSecretKey.InnerText = setupInfo.ManualEntryKey;
            imgScretKey.Src = setupInfo.QrCodeSetupImageUrl;
            spStatusTwoFA.InnerHtml = GetStatusTextOnOff(statusTwoFA);
            string accountSecretKey = authenticatorKey;
            if (string.IsNullOrEmpty(accountSecretKey))
                accountSecretKey = setupInfo.ManualEntryKey;
            if (statusTwoFA)
            {
                divInstructionsIntegrateTwoFA.Visible = false;
                divResetTwoFA.Visible = true;
            }
            else
            {
                divInstructionsIntegrateTwoFA.Visible = true;
                divResetTwoFA.Visible = false;
            }
            CtrlToggleTwoFA.InitControl(accountSecretKey);
            pnlTwoFactor.Update();
        }

        protected void lbtUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                #region Valid
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(pnlValid.Controls);
                if (!string.IsNullOrEmpty(txtPhone.Text) && !RegexUtilities.IsValidPhone(txtPhone.Text))
                    validationEngine.AddErrorPrompt(txtPhone.ClientID, GetResourceText(BackEndResourceKeys.INVALID_PHONE_NUMBER));

                if (!string.IsNullOrEmpty(txtEmail.Text))
                {
                    if (!RegexUtilities.IsValidEmail(txtEmail.Text))
                        validationEngine.AddErrorPrompt(txtEmail.ClientID, GetResourceText(BackEndResourceKeys.INVALID_EMAIL));
                }
                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }
                #endregion
                UserManager userManager = UserManager.Instance;
                AspnetUser user = SweetContext.Current.User;
                if(user == null || user.IsDeleted)
                {
                    string userName = SweetContext.Current.UserName;
                    FormsAuthentication.SignOut();
                    SweetContext.ClearAdminData();
                    AppCache.Remove(string.Format("ASP.NET_LockedId_{0}", userName));
                    ExpireAllCookies();
                    Response.Redirect(RewriteURLHelper.Login, false);
                    return;
                }
                user.UserId = SweetContext.Current.UserId;
                if (userManager.IsEmailExist(user.UserId, txtEmail.Text))
                {
                    validationEngine.AddErrorPrompt(txtEmail.ClientID, GetResourceText(BackEndResourceKeys.EMAIL_ALREADY_EXISTS));
                    validationEngine.ShowErrorPrompt();
                    return;
                }
                user.Email = txtEmail.Text;
                user.DisplayName = txtFullName.Text;
                user.MobileAlias = txtPhone.Text;
                userManager.CreateOrUpdate(user);
                ShowSuccessSaveData();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void lbtChangePassword_Click(object sender, EventArgs e)
        {
            try
            {
                #region Valid
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                if (string.IsNullOrEmpty(txtOldPassword.Text))
                    validationEngine.AddErrorPrompt(txtOldPassword.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));

                if (string.IsNullOrEmpty(txtNewPassword.Text))
                    validationEngine.AddErrorPrompt(txtNewPassword.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));

                if (string.IsNullOrEmpty(txtConfirmPassword.Text))
                    validationEngine.AddErrorPrompt(txtConfirmPassword.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));

                if (txtNewPassword.Text != txtConfirmPassword.Text)
                    validationEngine.AddErrorPrompt(txtConfirmPassword.ClientID, GetResourceText(BackEndResourceKeys.NEW_PASSWORD_DOES_NOT_MATCH));

                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }
                #endregion
                UserManager userManager = UserManager.Instance;
                AspnetUser user = userManager.GetUserById(SweetContext.Current.UserId);
                if (user == null)
                {
                    ShowInvalidNotFoundData();
                    return;
                }
                MembershipUser membershipUser = Membership.GetUser(user.UserName);
                if (membershipUser == null)
                {
                    ShowInvalidNotFoundData();
                    return;
                }

                string oldPass = string.Empty;
                if (user.UserId != null && userManager.IsAdministrator(user.UserId))
                {
                    if (!Membership.ValidateUser(txtUserName.Text, txtOldPassword.Text.Trim()))
                    {
                        validationEngine.AddErrorPrompt(txtOldPassword.ClientID, GetResourceText(BackEndResourceKeys.OLD_PASSWORD_IS_INCORRECT));
                        validationEngine.ShowErrorPrompt();
                        return;
                    }
                    oldPass = txtOldPassword.Text.Trim();
                }
                else
                {
                    oldPass = membershipUser.GetPassword();
                    if (oldPass != txtOldPassword.Text.Trim())
                    {
                        validationEngine.AddErrorPrompt(txtOldPassword.ClientID, GetResourceText(BackEndResourceKeys.OLD_PASSWORD_IS_INCORRECT));
                        validationEngine.ShowErrorPrompt();
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(user.AuthenticatorKey))
                {
                    CtrlTwoFAForChangePassword.InitControl(user.AuthenticatorKey);
                    divChangePassword.Visible = false;
                    divTwoFAForChangePassword.Visible = true;
                    upChangePassword.Update();
                    Session["AspnetMembershipId"] = user.UserId;
                    Session["AspnetMembershipOldPwd"] = oldPass;
                }
                else
                {
                    if (!membershipUser.ChangePassword(oldPass, txtNewPassword.Text.Trim()))
                    {
                        ShowNotify(GetResourceText(BackEndResourceKeys.UNABLE_TO_UPDATE_PASSWORD_FOR_ACCOUNT));
                        return;
                    }

                    Membership.UpdateUser(membershipUser);
                    ShowSuccessSaveData();
                }
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void CtrlTwoFAForChangePassword_CallbackSuccess(object sender, EventArgs e)
        {
            try
            {
                if (Session["AspnetMembershipId"] == null || (Guid)Session["AspnetMembershipId"] == Guid.Empty)
                {
                    ShowInvalidDataError();
                    return;
                }
                UserManager userManager = UserManager.Instance;
                AspnetUser user = userManager.GetUserById((Guid)Session["AspnetMembershipId"]);
                if (user == null)
                {
                    ShowInvalidDataError();
                    return;
                }
                MembershipUser membershipUser = Membership.GetUser(user.UserName);
                if (membershipUser == null)
                {
                    ShowInvalidNotFoundData();
                    return;
                }
                string oldPwd = (string)Session["AspnetMembershipOldPwd"] ?? string.Empty;
                if (!membershipUser.ChangePassword(oldPwd, txtNewPassword.Text.Trim()))
                {
                    ShowNotify(GetResourceText(BackEndResourceKeys.UNABLE_TO_UPDATE_PASSWORD_FOR_ACCOUNT));
                    return;
                }

                Membership.UpdateUser(membershipUser);
                ShowSuccessSaveData();
                Session["AspnetMembershipId"] = null;
                Session["AspnetMembershipOldPwd"] = string.Empty;
                divChangePassword.Visible = true;
                divTwoFAForChangePassword.Visible = false;
                upChangePassword.Update();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        private void CtrlToggleTwoFA_CallbackSuccess(object sender, EventArgs e)
        {
            UserManager userManager = UserManager.Instance;
            AspnetUser user = userManager.GetUserById(SweetContext.Current.UserId);
            if (user == null)
            {
                ShowInvalidDataError();
                return;
            }
            string message = string.Empty;
            if (string.IsNullOrEmpty(user.AuthenticatorKey))
            {
                user.AuthenticatorKey = CtrlToggleTwoFA.AccountSecretKey;
                message = GetResourceText(BackEndResourceKeys.AUTHENTICATOR_APP_IS_REGISTERED);
            }
            else
            {
                user.AuthenticatorKey = string.Empty;
                message = GetResourceText(BackEndResourceKeys.AUTHENTICATOR_APP_HAS_BEEN_RESET);
            }
            user.Save();
            //----------------------------------------------
            ShowNotify(message);
            BindTwoFactorAuthentication(user.UserName, user.AuthenticatorKey);
        }
    }
}