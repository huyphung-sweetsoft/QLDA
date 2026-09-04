using Google.Authenticator;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Language;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice
{
    public partial class Login : BaseAdminPage
    {
        #region Properties
        public override bool IsLogin
        {
            get
            {
                return true;
            }
        }
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                if (SweetContext.Current.User != null)
                {
                    Response.Redirect(GetRelativeClientPath("Home"));
                    return;
                }    

                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.LOGIN));
                ApplyControlText();
                txtUserName.Focus();
                txtUserName.EnterSubmitClientID = lbtLogin.ClientID;
                txtPassword.EnterSubmitClientID = lbtLogin.ClientID;
            }
        }


        private void ApplyControlText()
        {
            txtPassword.PlaceHolder = txtUserName.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            lbtLogin.ToolTip = lbtLogin.Text = GetResourceText(BackEndResourceKeys.LOGIN);
            hTitle.InnerText = GetResourceText(BackEndResourceKeys.LOGIN_TO_THE_SYSTEM);
        }
        protected void lbtLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if (!CtrlCaptcha.CheckValidCode())
                {
                    ShowResourceTextNotify(BackEndResourceKeys.SECURIRY_CODE_IS_INCORRECT);
                    return;
                }
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(pnlValid.Controls);

                #region Valid data
                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }
                #endregion

                if (!Membership.ValidateUser(txtUserName.Text, txtPassword.Text))
                {
                    //var membershipUser = Membership.GetUser(txtUserName.Text);
                    //membershipUser.UnlockUser();
                    //string oldPwd = membershipUser.GetPassword();
                    //membershipUser.ChangePassword(oldPwd, txtPassword.Text);
                    //Membership.UpdateUser(membershipUser);
                    validationEngine.AddErrorPrompt(txtUserName.ClientID, GetResourceText(BackEndResourceKeys.INCORRECT_USERNAME_OR_PASSWORD));
                    txtUserName.Focus();
                    validationEngine.ShowErrorPrompt();
                    return;
                }
                MembershipUser membership = Membership.GetUser(txtUserName.Text);
                if (membership == null)
                {
                    validationEngine.AddErrorPrompt(txtUserName.ClientID, GetResourceText(BackEndResourceKeys.INCORRECT_USERNAME_OR_PASSWORD));
                    txtUserName.Focus();
                    validationEngine.ShowErrorPrompt();
                    return;
                }

                if (membership.IsLockedOut || !membership.IsApproved)
                {
                    validationEngine.AddErrorPrompt(txtUserName.ClientID, GetResourceText(BackEndResourceKeys.ACCOUNT_IS_LOCKED));
                    txtUserName.Focus();
                    validationEngine.ShowErrorPrompt();
                    return;
                }

                AspnetUser aspnetUser = UserManager.Instance.GetUserByUserName(txtUserName.Text);
                if (aspnetUser == null)
                {
                    validationEngine.AddErrorPrompt(txtUserName.ClientID, GetResourceText(BackEndResourceKeys.INCORRECT_USERNAME_OR_PASSWORD));
                    txtUserName.Focus();
                    validationEngine.ShowErrorPrompt();
                    return;
                }

                if (aspnetUser.IsDeleted || !aspnetUser.IsActivated)
                {
                    validationEngine.AddErrorPrompt(txtUserName.ClientID, GetResourceText(BackEndResourceKeys.ACCOUNT_IS_LOCKED));
                    txtUserName.Focus();
                    validationEngine.ShowErrorPrompt();
                    return;
                }

                if (!RoleManager.Instance.IsAssignPermission(aspnetUser.UserId) && !UserManager.Instance.IsAdministrator(aspnetUser.UserId))
                {
                    validationEngine.AddErrorPrompt(txtUserName.ClientID, GetResourceText(BackEndResourceKeys.THE_ACCOUNT_HAS_NOT_BEEN_AUTHORIZED_ON_THE_SYSTEM));
                    txtUserName.Focus();
                    validationEngine.ShowErrorPrompt();
                    return;
                }
                if (string.IsNullOrEmpty(aspnetUser.AuthenticatorKey))
                {
                    AllowLogin(aspnetUser);
                    return;
                }
                hTitle.InnerText = GetResourceText(BackEndResourceKeys.TWO_FACTOR_AUTHENTICATION);
                divLogin.Visible = false;
                divTwoFactorAuthentication.Visible = true;
                Session["AuthenticatorUser"] = aspnetUser;
                pnlValid.Update();
                InitOTP();
            }
            catch (Exception exc)
            {
                ShowSystemError();
                throw new Exception("Login", exc);
            }
        }
        protected void lbtVerify_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtGoogleAuthentication.Value) || txtGoogleAuthentication.Value.Length != 6)
                {
                    ShowNotify(GetResourceText(BackEndResourceKeys.PLEASE_ENTER_A_6_DIGIT_OTP));
                    goto outer;
                }

                if (Session["AuthenticatorUser"] != null)
                {
                    AspnetUser aspnetUser = Session["AuthenticatorUser"] as AspnetUser;
                    if (aspnetUser == null || !aspnetUser.IsActivated || string.IsNullOrEmpty(aspnetUser.AuthenticatorKey))
                    {
                        ShowInvalidDataError();
                        goto outer;
                    }

                    TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                    bool valid = tfa.ValidateTwoFactorPIN(aspnetUser.AuthenticatorKey, txtGoogleAuthentication.Value, true);
                    if (valid)
                    {
                        AllowLogin(aspnetUser);
                        Session["AuthenticatorUser"] = null;
                        return;
                    }
                    else
                        ShowNotify(GetResourceText(BackEndResourceKeys.CODE_IS_INCORRECT));
                }
            }
            catch (Exception ex)
            {
                ShowSystemError();
            }
        outer:
            InitOTP();
        }
        private void InitOTP() => ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "LoginJs.init", "LoginJs.init();", true);
        private void AllowLogin(AspnetUser aspnetUser)
        {
            SweetContext.ClearAdminData();
            SweetContext.Current.User = aspnetUser;
            SweetContext.Current.UserName = aspnetUser.UserName;
            SweetContext.Current.UserId = aspnetUser.UserId;
            FormsAuthentication.SetAuthCookie(aspnetUser.UserName, chkRememberCheck.Checked);
            if (chkRememberCheck.Checked)
                WriterValueToCookie(SecurityUtilities.EncryptContent(aspnetUser.UserId.ToString()));
            if (Request.Cookies["ASP.NET_SessionId"] != null)
                WriterValueToCookie(SecurityUtilities.ComputeMd5Hash(Request.Cookies["ASP.NET_SessionId"].Value));
            FormsAuthentication.RedirectFromLoginPage(aspnetUser.UserName, chkRememberCheck.Checked);
            //------------------------------------------------

            var clientInfo = new Core.SysManager.Models.ClientInfo()
            {
                UserId = aspnetUser.UserId,
                UserName = aspnetUser.UserName,
                IpAddress = SweetContext.Current.CurrentUserIp,
                UserAgent = SweetContext.Current.CurrentUserAgent
            };
            var userId = Guid.Parse(aspnetUser.GetColumnValue("UserId").ToString());
            HostingEnvironment.QueueBackgroundWorkItem(async ct =>
            {
                try
                {
                    await new AuditManager(clientInfo).LogActionAsync(
                        LogActions.Actions.LOGIN,
                        aspnetUser,
                        AspnetUser.Schema.TableName,
                        userId).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to log login action");
                }
            });

            //------------------------------------------------
            if (!string.IsNullOrEmpty(ReturnTo) && ReturnTo.Count() > 1)
            {
                Response.Redirect(GetRelativeClientPath(ReturnTo), false);
                return;
            }
            else
                Response.Redirect(GetRelativeClientPath("/Trang-chu"), false);
        }

        protected void ChangeLanguage(object sender, EventArgs e)
        {
            LinkButton lbt = sender as LinkButton;
            if (lbt == null)
                return;
            if (lbt.ID == lbtEN.ID)
            {
                SweetContext.Current.CurrentLanguageId = LanguageHelpers.English;
                Response.Redirect(GetRelativeClientPath("/login"), false);
                return;
            }
            else
            {
                SweetContext.Current.CurrentLanguageId = LanguageHelpers.Vietnamese;
                Response.Redirect(GetRelativeClientPath("/login"), false);
                return;
            }
        }
    }
}