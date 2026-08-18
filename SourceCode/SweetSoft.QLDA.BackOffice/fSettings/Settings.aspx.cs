using Newtonsoft.Json;
using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls.Helpers;
using SweetSoft.QLDA.Core.Caches;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.MailManager;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

//-----------------------------PROGRAMER LOGS-------------------------------

namespace SweetSoft.QLDA.BackOffice
{
    public partial class Settings : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.Setting;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButton();
            this.FolderPath = "~/uploads/settings";
            this.FolderName = GetResourceText(BackEndResourceKeys.SYSTEM_CONFIGURATION);
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.SYSTEM_CONFIGURATION));
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.SYSTEM_CONFIGURATION);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    {RewriteURLHelper.Settings, GetResourceText(BackEndResourceKeys.SYSTEM_CONFIGURATION) }
                };

                ApplyControlsText();
                FillTimeZoneDropDown();
                BindTimeZoneSystem();
                lbtSubmit.Visible = this.IsEdit;
                BindData();
            }
        }

        private void ApplyControlsText()
        {
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            lbtClearCache.ToolTip = lbtClearCache.Text = GetResourceText(BackEndResourceKeys.CLEAR_CACHE);
            lbtTest.ToolTip = lbtTest.Text = GetResourceText(BackEndResourceKeys.SEND_MAIL);
            txtAccount.PlaceHolder = txtAdminEmail.PlaceHolder = txtCompanyAddress.PlaceHolder
                = txtCompanyEmail.PlaceHolder = txtCompanyFax.PlaceHolder = txtCompanyHotline.PlaceHolder
                = txtCompanyName.PlaceHolder = txtCompanyPhone.PlaceHolder
                = txtEmailErrorReport.PlaceHolder = txtLinkAddress.PlaceHolder = txtMessengerUrl.PlaceHolder
                = txtNumberItemOfGrid.PlaceHolder
                = txtPort.PlaceHolder = txtSenderEmail.PlaceHolder = txtServer.PlaceHolder = txtSiteTitle.PlaceHolder
                = txtTaxCode.PlaceHolder = txtTestEmail.PlaceHolder
                = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            ddlSelectTimeZone.PlaceHolder
                = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            chkPreventSelection.OnText = chkPreventRightClick.OnText = chkSaveLog.OnText = chkUsingSLL.OnText
                = GetResourceText(BackEndResourceKeys.ON);
            chkPreventSelection.OffText = chkPreventRightClick.OffText = chkSaveLog.OffText = chkUsingSLL.OffText
                = GetResourceText(BackEndResourceKeys.OFF);
            ControlHelpers controlHelpers = new ControlHelpers();
            controlHelpers.BindUsers(ddlDefaultProcessor, false, "UserId");
        }

        #region TimeZone
        private void BindTimeZoneSystem()
        {
            lbServerTime.Text = string.Format("{0:g}", DateTime.Now);
            TimeZoneInfo localZone = TimeZoneInfo.Local;
            lbServerTimeZone.Text = localZone.DisplayName.Substring(0, 12);
        }
        private void FillTimeZoneDropDown()
        {
            try
            {
                ddlSelectTimeZone.Items.Clear();
                ReadOnlyCollection<TimeZoneInfo> tz = TimeZoneInfo.GetSystemTimeZones();
                if (tz == null || tz.Count <= 0)
                    return;
                foreach (TimeZoneInfo timeZone in tz)
                {
                    ddlSelectTimeZone.Items.Add(new ListItem(timeZone.DisplayName, timeZone.Id));
                }
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }
        protected void ddlTimeZone_SelectedIndexChanged(object sender, EventArgs e)
        {
            string myTimeZone = ddlSelectTimeZone.SelectedValue;
            double differentTime = DateTimeHelper.GetDifferentHour(myTimeZone);
            lbDifferent.Text = differentTime.ToString() + " " + GetResourceText(BackEndResourceKeys.HOUR);
            pnlTimeZone.Update();
        }
        #endregion
        private void RegisterAsyncButton()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            script.RegisterAsyncPostBackControl(lbtSubmit);
            script.RegisterAsyncPostBackControl(lbtClearCache);
            script.RegisterAsyncPostBackControl(lbtTest);
            script.RegisterAsyncPostBackControl(ddlSelectTimeZone);
        }
        protected override void BindData()
        {
            try
            {
                SettingManager settingManager = new SettingManager();
                string langCode = SweetContext.Current.CurrentLanguageCode;
                #region Overview
                TblSetting setting = settingManager.GetSettingByName(string.Format("{0}_{1}", SettingKeys.TitleOfWebsite, langCode));
                if (setting != null)
                    txtSiteTitle.Text = setting.SettingValue;
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.AdministratorEmail);
                if (setting != null)
                    txtAdminEmail.Text = setting.SettingValue;
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.ErrorReceiverEmail);
                if (setting != null)
                    txtEmailErrorReport.Text = setting.SettingValue;
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.DataGridItemsPerPage);
                if (setting != null)
                    txtNumberItemOfGrid.Text = setting.SettingValue;
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.DefaultProcessor);
                if (setting != null)
                    ddlDefaultProcessor.SelectedValue = setting.SettingValue;
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.PreventRightClick);
                if (setting != null)
                    chkPreventRightClick.Checked = setting.SettingValue.ToUpper() == "TRUE";
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.PreventSelection);
                if (setting != null)
                    chkPreventSelection.Checked = setting.SettingValue.ToUpper() == "TRUE";
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.SaveLog);
                if (setting != null)
                    chkSaveLog.Checked = setting.SettingValue.ToUpper() == "TRUE";
                #endregion

                #region TimeZone
                setting = settingManager.GetSettingByName(SettingKeys.MyTimeZone);
                if (setting != null && !string.IsNullOrEmpty(setting.SettingValue))
                {
                    ddlSelectTimeZone.SelectedValue = setting.SettingValue;
                    double differentTime = DateTimeHelper.GetDifferentHour(setting.SettingValue);
                    lbDifferent.Text = differentTime.ToString() + " " + GetResourceText(BackEndResourceKeys.HOUR);
                }
                #endregion

                #region SMTP
                setting = settingManager.GetSettingByName(SettingKeys.SmtpMailServerAddress);
                if (setting != null)
                    txtServer.Text = setting.SettingValue;
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.SmtpSenderEmail);
                if (setting != null)
                    txtSenderEmail.Text = setting.SettingValue;
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.SmtpSenderAccount);
                if (setting != null)
                    txtAccount.Text = setting.SettingValue;
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.SmtpPort);
                if (setting != null)
                    txtPort.Text = setting.SettingValue;
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.SmtpUsingSSL);
                if (setting != null)
                    chkUsingSLL.Checked = setting.SettingValue.ToUpper() == "TRUE";
                #endregion

                #region Contacts
                setting = settingManager.GetSettingByName(SettingKeys.CompanyName);
                if (setting != null)
                    txtCompanyName.Text = setting.SettingValue;
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.CompanyEmail);
                if (setting != null)
                    txtCompanyEmail.Text = setting.SettingValue;
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.CompanyPhone);
                if (setting != null)
                    txtCompanyPhone.Text = setting.SettingValue;
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(string.Format("{0}_{1}", SettingKeys.CompanyAddress, langCode));
                if (setting != null)
                    txtCompanyAddress.Text = setting.SettingValue;
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.CompanyHotline);
                if (setting != null)
                    txtCompanyHotline.Text = setting.SettingValue;
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.CompanyFax);
                if (setting != null)
                    txtCompanyFax.Text = setting.SettingValue;
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.InternalAnnouncement);
                if (setting != null)
                    txtInternalAnnouncement.Text = setting.SettingValue;
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.LinkAddress);
                if (setting != null)
                    txtLinkAddress.Text = setting.SettingValue;
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.TaxCode);
                if (setting != null)
                    txtTaxCode.Text = setting.SettingValue;
                //--------------------------------------------------------------
                setting = settingManager.GetSettingByName(SettingKeys.MessengerUrl);
                if (setting != null)
                    txtMessengerUrl.Text = setting.SettingValue;
                //--------------------------------------------------------------
                #endregion
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (!this.IsEdit)
                {
                    ShowAccessDeniedNotify();
                    return;
                }

                SettingManager settingManager = new SettingManager();
                string langCode = SweetContext.Current.CurrentLanguageCode;
                #region Overview
                settingManager.SaveSetting(string.Format("{0}_{1}", SettingKeys.TitleOfWebsite, langCode), txtSiteTitle.Text);
                //----------------------------------------------------------
                settingManager.SaveSetting(SettingKeys.AdministratorEmail, txtAdminEmail.Text);
                //----------------------------------------------------------
                settingManager.SaveSetting(SettingKeys.ErrorReceiverEmail, txtEmailErrorReport.Text);
                //----------------------------------------------------------
                settingManager.SaveSetting(SettingKeys.DataGridItemsPerPage, txtNumberItemOfGrid.Text);
                //----------------------------------------------------------
                settingManager.SaveSetting(SettingKeys.DefaultProcessor, ddlDefaultProcessor.SelectedValue);
                //----------------------------------------------------------
                settingManager.SaveSetting(SettingKeys.PreventRightClick, chkPreventRightClick.Checked.ToString());
                //----------------------------------------------------------
                settingManager.SaveSetting(SettingKeys.PreventSelection, chkPreventSelection.Checked.ToString());
                //----------------------------------------------------------
                settingManager.SaveSetting(SettingKeys.SaveLog, chkSaveLog.Checked.ToString());
                //----------------------------------------------------------
                #endregion

                #region TimeZone
                //----------------------------------------------------------        
                settingManager.SaveSetting(SettingKeys.MyTimeZone, ddlSelectTimeZone.SelectedValue);
                //----------------------------------------------------------        
                #endregion

                #region SMTP
                settingManager.SaveSetting(SettingKeys.SmtpMailServerAddress, txtServer.Text);
                //----------------------------------------------------------
                settingManager.SaveSetting(SettingKeys.SmtpSenderEmail, txtSenderEmail.Text);
                //----------------------------------------------------------
                settingManager.SaveSetting(SettingKeys.SmtpSenderAccount, txtAccount.Text);
                //----------------------------------------------------------
                if (!string.IsNullOrEmpty(txtPassword.Text))
                    settingManager.SaveSetting(SettingKeys.SmtpSenderPassword, SecurityUtilities.EncryptContent(txtPassword.Text));
                //----------------------------------------------------------
                settingManager.SaveSetting(SettingKeys.SmtpPort, txtPort.Text);
                //----------------------------------------------------------
                settingManager.SaveSetting(SettingKeys.SmtpUsingSSL, chkUsingSLL.Checked.ToString());
                //----------------------------------------------------------
                #endregion

                #region Contacts
                settingManager.SaveSetting(SettingKeys.CompanyName, txtCompanyName.Text);
                //----------------------------------------------------------   
                settingManager.SaveSetting(SettingKeys.CompanyEmail, txtCompanyEmail.Text);
                //----------------------------------------------------------
                settingManager.SaveSetting(SettingKeys.CompanyPhone, txtCompanyPhone.Text);
                //----------------------------------------------------------
                settingManager.SaveSetting(string.Format("{0}_{1}", SettingKeys.CompanyAddress, langCode), txtCompanyAddress.Text);
                //----------------------------------------------------------
                settingManager.SaveSetting(SettingKeys.CompanyHotline, txtCompanyHotline.Text);
                //----------------------------------------------------------
                settingManager.SaveSetting(SettingKeys.CompanyFax, txtCompanyFax.Text);
                //----------------------------------------------------------
                settingManager.SaveSetting(SettingKeys.InternalAnnouncement, txtInternalAnnouncement.Text);
                //----------------------------------------------------------
                settingManager.SaveSetting(SettingKeys.LinkAddress, txtLinkAddress.Text);
                //----------------------------------------------------------
                settingManager.SaveSetting(SettingKeys.TaxCode, txtTaxCode.Text);
                //----------------------------------------------------------
                settingManager.SaveSetting(SettingKeys.MessengerUrl, txtMessengerUrl.Text);
                //----------------------------------------------------------
                #endregion


                AppCache.Clear();
                settingManager.ClearCacheForAPI();
                ShowSuccessSaveData();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void lbtTest_Click(object sender, EventArgs e)
        {
            try
            {
                if (!RegexUtilities.IsValidEmail(txtTestEmail.Text))
                {
                    ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                    validationEngine.AddErrorPrompt(txtTestEmail.ClientID, GetResourceText(BackEndResourceKeys.INVALID_EMAIL));
                    validationEngine.ShowErrorPrompt();
                    return;
                }

                SettingManager settingManager = new SettingManager();
                string fromEmail = settingManager.GetSettingValue(SettingKeys.AdministratorEmail);
                string subject = string.Format("Test email sent by [{0}] on System Management - {1}", SweetContext.Current.UserName, SweetContext.Current.SystemName);

                // Message body content
                string strMessage = string.Format("<p style='margin: 0; line-height: 1.5;'>SMTP settings are correct!</p>");
                strMessage += string.Format("<p style='margin: 0; line-height: 1.5;'>(This email was sent from {0})</p>", SweetContext.Current.SiteUrl);
                Guid userId = SweetContext.Current.UserId;
                var appContext = SweetContext.Current;
                Task.Run(async () =>
                {
                    await new EmailManager(appContext).SendEmailAsync(new EmailRequest()
                    {
                        CustomerId = userId,
                        RefType = EmailType.System,
                        Sender = EmailManager.BackendSenderName,
                        Subject = subject,
                        Content = strMessage,
                        FromEmail = fromEmail,
                        ToEmail = txtTestEmail.Text
                    }, false);
                });
                ShowNotify(string.Format(GetResourceText(BackEndResourceKeys.AN_EMAIL_HAS_BEEN_SENT_TO_EMAIL), txtTestEmail.Text));
            }
            catch (Exception exc)
            {
                ShowNotify($"Error: {exc.Message}", MSGType.Error);
                //ShowSystemError();
                //throw new Exception("System settings", exc);
            }
        }

        protected void lbtClearCache_Click(object sender, EventArgs e)
        {
            AppCache.Clear();
            SettingManager settingManager = new SettingManager();
            settingManager.ClearCacheForAPI();
            ShowNotify(GetResourceText(BackEndResourceKeys.CACHE_CLEARED));
        }
    }
}