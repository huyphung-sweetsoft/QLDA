using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.ResourceTexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Google.Authenticator;

namespace SweetSoft.QLDA.BackOffice.Controls.TwoFA
{
    public partial class CtrlTwoFactorAuthenticator : BaseAdminUserControl
    {
        #region Properties
        public EventHandler CallbackSuccess;
        public string AccountSecretKey
        {
            get
            {
                if (ViewState["AccountSecretKey"] != null)
                    return (string)ViewState["AccountSecretKey"];
                return string.Empty;
            }
            set
            {
                ViewState["AccountSecretKey"] = value;
            }
        }
        #endregion

        #region Script + Styles
        protected virtual RegisterCSSAndJS RegisterCSSAndJS
        {
            get
            {
                List<string> jsLinks = new List<string>();
                jsLinks.Add(this.CURRENT_PAGE.GetRelativeClientPath("/Controls/TwoFA/handle-two-factor-authenticator.js"));
                return new RegisterCSSAndJS("cpHeadVendor", "cpVendorScript", null, jsLinks);
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RegisterCSSAndJS.Register();
        }
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                ScriptManager script = ScriptManager.GetCurrent(Page);
                script.RegisterAsyncPostBackControl(lbtVerify);
                if (IsPostBack)
                    return;
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }

        public void InitControl(string accountSecretKey)
        {
            this.AccountSecretKey = accountSecretKey;
            lbtVerify.ToolTip = lbtVerify.Text = GetResourceText(BackEndResourceKeys.VERIFY);
            ScriptManager.RegisterClientScriptBlock(Page, GetType(), $"{this.ClientID}init", $@"if(typeof {this.ClientID} === 'undefined') {{ document.addEventListener(""DOMContentLoaded"", () => {{ {this.ClientID} = new TwoFactorAuthenticator('{txtGoogleAuthentication.ClientID}', '{divTwoFA.ClientID}'); {this.ClientID}.init(); }}); }} else {this.ClientID}.init();", true);
            lbtVerify.OnClientClick = $"return {this.ClientID}.verifyOTP();";
        }

        protected void ReloadInputOTP() => ScriptManager.RegisterClientScriptBlock(Page, GetType(), "init", $"{this.ClientID}.init();", true);

        private (bool, string) IsValidateTwoFactorPIN()
        {
            string errorMsg = string.Empty;
            if (string.IsNullOrEmpty(txtGoogleAuthentication.Value) || txtGoogleAuthentication.Value.Length != 6)
            {
                errorMsg = GetResourceText(BackEndResourceKeys.PLEASE_ENTER_A_6_DIGIT_OTP);
                goto outer;
            }

            if(string.IsNullOrEmpty(this.AccountSecretKey))
            {
                errorMsg = GetResourceText(BackEndResourceKeys.INACCURATE_DATA_PLEASE_RELOAD_THE_PAGE_AND_TRY_AGAIN);
                goto outer;
            }

            bool isValidate = new TwoFactorAuthenticator().ValidateTwoFactorPIN(this.AccountSecretKey, txtGoogleAuthentication.Value, true);
            if (!isValidate)
                errorMsg = GetResourceText(BackEndResourceKeys.INCORRECT_PIN);
            return (isValidate, errorMsg);
        outer:
            return (false, errorMsg);
        }

        protected void lbtVerify_Click(object sender, EventArgs e)
        {
            try
            {
                var (isVerify, errorMsg) = IsValidateTwoFactorPIN();
                if (isVerify)
                {
                    if (CallbackSuccess != null)
                    {
                        CallbackSuccess(sender, e);
                        txtGoogleAuthentication.Value = string.Empty;
                        upHiddenField.Update();
                    }
                }
                else
                {
                    ShowNotify(errorMsg, MSGType.Error);
                    ReloadInputOTP();
                }    
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }
    }
}