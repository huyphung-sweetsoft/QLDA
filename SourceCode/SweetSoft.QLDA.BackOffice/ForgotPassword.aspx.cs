using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.MailManager;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Threading.Tasks;
using System.Web.UI;

//-------------------------------------Logs-------------------------------------

namespace SweetSoft.QLDA.BackOffice
{
    public partial class ForgotPassword : BaseAdminPage
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
            if (!IsPostBack)
            {
                if (SweetContext.Current.User != null)
                    Response.Redirect(GetRelativeClientPath("/"), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.FORGOT_PASSWORD));
                txtEmail.EnterSubmitClientID = lbtConfirm.ClientID;
                txtEmail.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
                lbtConfirm.ToolTip = lbtConfirm.Text = GetResourceText(BackEndResourceKeys.CONFIRM);
            }
        }
        private void RegisterAsyncButton()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            script.RegisterAsyncPostBackControl(lbtConfirm);
        }
        protected void lbtConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Captcha1.CheckValidCode())
                    return;
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                #region Valid data
                validationEngine.CheckValidControls(pnlValid.Controls);
                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }
                #endregion

                AspnetUser user = UserManager.Instance.GetUserByEmail(txtEmail.Text);
                if (user == null)
                {
                    validationEngine.AddErrorPrompt(txtEmail.ClientID, GetResourceText(BackEndResourceKeys.EMAIL_DOES_NOT_EXISTS));
                    txtEmail.Focus();
                    validationEngine.ShowErrorPrompt();
                    return;
                }
                if (!user.IsActivated)
                {
                    validationEngine.AddErrorPrompt(txtEmail.ClientID, GetResourceText(BackEndResourceKeys.ACCOUNT_IS_LOCKED));
                    txtEmail.Focus();
                    validationEngine.ShowErrorPrompt();
                    return;
                }
                //------------------------------------------
                int timeOut = 5;
                user.Email = txtEmail.Text;
                string resetKey = $"{user.Email}|{DateTime.UtcNow.AddMinutes(timeOut)}";
                user.ResetPasswordKey = SecurityUtilities.EncryptContent(resetKey);
                user.Save();
                string resetPasswordLink = $"{CommonHelpers.GetHostPath()}reset-password?rk={user.ResetPasswordKey}";
                //------------------------------------------
                var appContext= SweetContext.Current;
                string companyName = SettingManager.Instance.GetSettingValue(SettingKeys.CompanyName);
                string companyEmail = SettingManager.Instance.GetSettingValue(SettingKeys.CompanyEmail);
                Task.Run(async () =>
                {
                   await new EmailManager(appContext).SendEmailWithTemplateAsync(null, EmailType.System, user.UserId, user.Email
                    , EmailTemplateKeys.AdminTemplate.TemplateForgotPassword
                    , EmailFormatTypes.Admin
                    , new System.Collections.Generic.Dictionary<string, string>
                    {
                        { EmailKeys.USER_NAME, user.UserName },
                        { EmailKeys.FULL_NAME, user.DisplayName },
                        { EmailKeys.RESET_PASSWORD_LINK, resetPasswordLink },
                        { EmailKeys.EXPIRY_TIME, timeOut.ToString() },
                        { EmailKeys.COMPANY_NAME, companyName },
                        { EmailKeys.CURRENT_YEAR, DateTime.UtcNow.Year.ToString() },
                        { EmailKeys.SUPPORT_EMAIL, companyEmail },
                    }
                );
                });
                //------------------------------------------
                txtEmail.Text = string.Empty;
                pnlValid.Update();
                ShowNotify(string.Format(GetResourceText(BackEndResourceKeys.AN_EMAIL_HAS_BEEN_SENT_TO_EMAIL), user.Email));
            }
            catch (Exception exc)
            {
                ShowSystemError();
            }
        }
    }
}