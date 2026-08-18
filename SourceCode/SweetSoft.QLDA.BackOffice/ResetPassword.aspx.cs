//-----------------------PROGRAMER LOGS---------------------------
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice
{
    public partial class ResetPassword : BaseAdminPage
    {
        public override bool IsLogin
        {
            get
            {
                return true;
            }
        }
        private string _resetKeyForCache { get; set; }
        private string ResetKey
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(_resetKeyForCache))
                        _resetKeyForCache = CommonHelpers.QueryString("rk");
                    return _resetKeyForCache;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButton();
            if (!IsPostBack)
            {
                if (SweetContext.Current.User != null)
                    Response.Redirect(GetRelativeClientPath(), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.CHANGE_PASSWORD));
                txtPassword.EnterSubmitClientID = txtConfirmPassword.EnterSubmitClientID = lbtConfirm.ClientID;
                ApplyControlsText();
                AspnetUser user = null;
                if (!IsValidResetKey(out user))
                    return;
            }
        }

        private void ApplyControlsText()
        {
            txtConfirmPassword.PlaceHolder = txtPassword.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            lbtConfirm.ToolTip = lbtConfirm.Text = GetResourceText(BackEndResourceKeys.CONFIRM);
        }

        private void RegisterAsyncButton()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            script.RegisterAsyncPostBackControl(lbtConfirm);
        }

        private bool IsValidResetKey(out AspnetUser user)
        {
            string errorMsg = GetResourceText(BackEndResourceKeys.THE_REQUESTED_LINK_IS_INCORRECT_OR_HAS_EXPIRED);
            bool isValid = false;
            user = null;
            if (string.IsNullOrEmpty(ResetKey))
                goto outer;

            string[] result = SecurityUtilities.DecryptContent(ResetKey).Split('|');
            if (result == null || result.Length != 2 || string.IsNullOrEmpty(result[0])
                || !RegexUtilities.IsValidEmail(result[0]) || string.IsNullOrEmpty(result[1]))
                goto outer;

            DateTime expirationTime = DateTime.MinValue;
            if (!DateTime.TryParse(result[1], out expirationTime) || expirationTime < DateTime.UtcNow)
                goto outer;

            user = UserManager.Instance.GetUserByEmail(result[0]);
            if (user == null || HttpUtility.UrlDecode(user.ResetPasswordKey) != ResetKey)
                goto outer;
            else if (!user.IsActivated)
            {
                errorMsg = GetResourceText(BackEndResourceKeys.ACCOUNT_IS_LOCKED);
                goto outer;
            }

            isValid = true;
        outer:
            if (!isValid)
            {
                divAlert.InnerText = errorMsg;
                divAlert.Attributes["class"] = "alert-danger text-center my-4 js-alert rounded-2 p-2";
                pnlValid.Visible = false;
                ShowInvalidDataError();
            }
            return isValid;
        }

        protected void lbtConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Captcha1.CheckValidCode())
                    return;
                #region Valid
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(pnlValid.Controls);

                if (!string.IsNullOrEmpty(txtPassword.Text) || !string.IsNullOrEmpty(txtConfirmPassword.Text))
                {
                    if (txtPassword.Text != txtConfirmPassword.Text)
                        validationEngine.AddErrorPrompt(txtConfirmPassword.ClientID, GetResourceText(BackEndResourceKeys.RE_ENTER_INCORRECT_PASSWORD));
                }

                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }
                #endregion

                AspnetUser user = null;
                bool isValidResetKey = IsValidResetKey(out user);
                if (!isValidResetKey)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "errorToken", "errorToken();", true);
                    return;
                }
                MembershipUser membershipUser = Membership.GetUser(user.UserName);
                if (membershipUser == null)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "errorToken", "errorToken();", true);
                    return;
                }
                string oldPass = membershipUser.ResetPassword();
                if (!membershipUser.ChangePassword(oldPass, txtPassword.Text))
                {
                    ShowNotify(GetResourceText(BackEndResourceKeys.UNABLE_TO_UPDATE_PASSWORD_FOR_ACCOUNT));
                    return;
                }
                Membership.UpdateUser(membershipUser);
                user.ResetPasswordKey = string.Empty;
                user.Save();
                txtPassword.Text = txtConfirmPassword.Text = string.Empty;
                pnlValid.Update();
                ShowNotify(GetResourceText(BackEndResourceKeys.YOUR_PASSWORD_HAS_BEEN_UPDATED));
                return;
            }
            catch (Exception exc)
            {
                ShowSystemError();
                throw new Exception("Reset password", exc);
            }
        }
    }
}