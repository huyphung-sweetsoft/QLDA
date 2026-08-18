using OfficeOpenXml;
using OfficeOpenXml.Style;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.ExcelManager;
using SweetSoft.QLDA.Core.FileManager;
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
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Interop;
using static SweetSoft.QLDA.Controls.EnumHelper;

namespace SweetSoft.QLDA.BackOffice.fUsers
{
    public partial class UserList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.User;
            }
        }
        private Guid UserId
        {
            get
            {
                if (ViewState["UserId"] != null)
                    return (Guid)ViewState["UserId"];
                return Guid.Empty;
            }
            set
            {
                ViewState["UserId"] = value;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlUsers1.NewUserHandlerCallback += NewUserAction;
            CtrlUsers1.EditUserHandlerCallback += EditUserAction;
            CtrlUsers1.SendMailHandlerCallback += SendMail;
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.USER_LIST));

                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.USER_LIST);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    {RewriteURLHelper.Users, GetResourceText(BackEndResourceKeys.USER_LIST) }
                };
                ApplyControlsText();
                CtrlUsers1.InitControls();
                string userQueryId = CommonHelpers.QueryString("userId");
                if (string.IsNullOrEmpty(userQueryId)) return;
                Guid tempId = Guid.Empty;
                if (!Guid.TryParse(SecurityUtilities.UnprotectUrlParameter(userQueryId), out tempId))
                    return;
                EditUserAction(tempId, EventArgs.Empty);
            }
        }
        private void ApplyControlsText()
        {
            ddlRole.PlaceHolder
                = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            //------------------------------------------------
            dlDetail.CloseText = GetResourceText(BackEndResourceKeys.CLOSE);
            //--------------------------------------------------------
            txtConfirmPassword.PlaceHolder = txtFullName.PlaceHolder = txtEmail.PlaceHolder
                = txtPassword.PlaceHolder
                = txtPhone.PlaceHolder
                = txtUserName.PlaceHolder = txtPhone.PlaceHolder
                = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);

            chkStatus.OnText = GetResourceText(BackEndResourceKeys.ACTIVE);
            chkStatus.OffText = GetResourceText(BackEndResourceKeys.INACTIVE);
            //------------------------------------------------
        }

        #region Modal
        private void RefreshUserInfo()
        {
            new ControlHelpers().BindRoles(ddlRole);
            lbtSubmit.Visible = false;
            //---------------------------------------------
            txtUserName.Enabled = true;
            txtUserName.Text = txtPhone.Text
                = txtFullName.Text = txtEmail.Text
                = txtPassword.Text = txtConfirmPassword.Text
                = "";
            ddlRole.SelectedIndex = 0;
            chkStatus.Checked = true;
            chkChangePassword.Checked = false;
            this.UserId = Guid.Empty;
            divImage.Visible = false;
            //---------------------------------------------
            fbImage.SingleFilePath = "/Styles/images/user-icon.png";
            fbImage.SingleFilePathType = FileTypes.Internal;
            fbImage.IsMultiple = false;
            fbImage.LoadFile(this.UserId, FileUploadTypes.UserAvatar);
            string script = @"
                window.addEventListener('load', function() {
                    if (typeof CMSMasterJs !== 'undefined' && typeof CMSMasterJs.HideChangePwd === 'function') {
                        CMSMasterJs.HideChangePwd();
                    }
                });
            ";
            ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "HideChangePwd", script, true);
        }
        private void EditUserAction(object sender, EventArgs e)
        {
            if (sender == null)
            {
                ShowInvalidDataError();
                return;
            }
            Guid userId = (Guid)sender;
            if (userId == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }
            RefreshUserInfo();
            lbtSubmit.Visible = this.IsEdit;
            AspnetUser user = UserManager.Instance.GetUserById(userId);
            if (user == null || user.IsDeleted)
            {
                Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), false);
                return;
            }
            this.UserId = user.UserId;
            fbImage.LoadFile(this.UserId, FileUploadTypes.UserAvatar);
            divImage.Visible = true;
            //--------------------------------------------
            txtUserName.Text = user.UserName;
            txtUserName.Enabled = false;
            txtFullName.Text = user.DisplayName;
            txtPhone.Text = user.MobileAlias;
            divPassword.Attributes["data-edit"] = "true";
            divChangePassword.Visible = true;
            chkStatus.Checked = user.IsActivated;
            //---------------------------------------------
            MembershipUser membershipUser = Membership.GetUser(user.UserName);
            if (membershipUser != null)
            {
                if (!membershipUser.Email.Contains("no-email.com"))
                    txtEmail.Text = membershipUser.Email;
            }
            //---------------------------------------------
            AspnetRole role = RoleManager.Instance.GetRoleByUserId(user.UserId);
            if (role != null)
                ddlRole.SelectedValue = role.RoleId.ToString();
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.ACCOUNT_INFORMATION);
            if (!IsPostBack)
                dlDetail.OpenModal(true, 1000);
            else
                dlDetail.OpenModal(true);
        }
        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                #region Valid
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(dlDetail.Controls);

                if (!string.IsNullOrEmpty(txtPhone.Text) && !RegexUtilities.IsValidPhone(txtPhone.Text))
                    validationEngine.AddErrorPrompt(txtPhone.ClientID, GetResourceText(BackEndResourceKeys.INVALID_PHONE_NUMBER));

                if (chkChangePassword.Checked)
                {
                    if (string.IsNullOrEmpty(txtPassword.Text.Trim()))
                        validationEngine.AddErrorPrompt(txtPassword.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));
                    else if (txtPassword.Text.Trim().Length < 6)
                        validationEngine.AddErrorPrompt(txtPassword.ClientID, GetResourceText(BackEndResourceKeys.PASSWORD_MUST_HAVE_MINIMUM_OF_6_CHARACTERS));

                    if (string.IsNullOrEmpty(txtConfirmPassword.Text.Trim()))
                        validationEngine.AddErrorPrompt(txtConfirmPassword.ClientID, GetResourceText(BackEndResourceKeys.PLEASE_ENTER_THE_VALUE));
                    else if (txtConfirmPassword.Text.Trim() != txtPassword.Text.Trim())
                        validationEngine.AddErrorPrompt(txtConfirmPassword.ClientID, GetResourceText(BackEndResourceKeys.RE_ENTER_INCORRECT_PASSWORD));
                }

                if (!string.IsNullOrEmpty(txtPassword.Text) || !string.IsNullOrEmpty(txtConfirmPassword.Text))
                {
                    if ((txtPassword.Text != txtConfirmPassword.Text))
                        validationEngine.AddErrorPrompt(txtConfirmPassword.ClientID, GetResourceText(BackEndResourceKeys.RE_ENTER_INCORRECT_PASSWORD));
                }

                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }
                #endregion
                bool isAdd = true;
                UserManager organizationUserManager = UserManager.Instance;
                AspnetUser user = new AspnetUser();
                if (this.UserId != Guid.Empty)
                {
                    user.UserId = this.UserId;
                    if (!this.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    isAdd = false;
                }
                else if (!this.IsAdd)
                {
                    ShowAccessDeniedNotify();
                    return;
                }
                user.UserName = txtUserName.Text;
                user.LoweredUserName = user.UserName;
                user.DisplayName = txtFullName.Text;
                user.Email = txtEmail.Text;
                user.MobileAlias = txtPhone.Text;
                user.IsActivated = chkStatus.Checked;
                string password = string.Empty;
                if (!string.IsNullOrEmpty(txtPassword.Text))
                    password = txtPassword.Text;
                if (string.IsNullOrEmpty(txtEmail.Text))
                    txtEmail.Text = $"{DateTime.UtcNow.Ticks}@no-email.com";
                Guid roleId = Guid.Empty;
                if (this.GetValue(ddlRole, out roleId) && roleId != Guid.Empty)
                    user.RoleId = roleId;
                if (isAdd)
                {
                    if (string.IsNullOrEmpty(password))
                        password = SecurityUtilities.CreateAlphaNumericString(8);
                    user.Password = password;
                    user = organizationUserManager.CreateOrUpdate(user);
                    if (user == null)
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    #region Add
                    //------------------------------------------
                    if (user.Email != null)
                    {
                        if (!user.Email.Contains("no-email.com") && RegexUtilities.IsValidEmail((user.Email)))
                            SendMail(new
                            {
                                User = user,
                                Password = password,
                            }, EventArgs.Empty);
                    }
                    ShowNotify(GetResourceText(BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY));
                    #endregion
                }
                else
                {
                    bool isNewPass = false;
                    if (!string.IsNullOrEmpty(password))
                    {
                        user.Password = password;
                        isNewPass = true;
                    }

                    user = organizationUserManager.CreateOrUpdate(user);
                    if (user != null)
                    {
                        //------------------------------------------
                        if (isNewPass && user.Email != null)
                        {
                            if (!user.Email.Contains("no-email.com") && RegexUtilities.IsValidEmail((user.Email)))
                            {
                                SendMail(new
                                {
                                    User = user,
                                    Password = txtPassword.Text,
                                }, EventArgs.Empty);
                            }
                        }
                    }
                    ShowSuccessSaveData();
                }
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
                return;
            }
            dlDetail.CloseModal();
            CtrlUsers1.Rebind();
        }
        private void SendMail(object sender, EventArgs e)
        {
            if (sender == null)
                return;
            dynamic dic = (dynamic)sender;
            if (dic == null)
                return;
            AspnetUser user = dic.User;
            if (user == null)
                return;
            if (string.IsNullOrEmpty(user.Email))
            {
                MembershipUser membershipUser = Membership.GetUser(user.UserName);
                if (membershipUser != null)
                    user.Email = membershipUser.Email;
            }
            if (string.IsNullOrEmpty(user.Email))
                return;
            string password = dic.Password;
            string companyName = SettingManager.Instance.GetSettingValue(SettingKeys.CompanyName);
            string companyEmail = SettingManager.Instance.GetSettingValue(SettingKeys.CompanyEmail);
            var appContext = SweetContext.Current;
            string hostPath = CommonHelpers.GetHostPath().TrimEnd('/');
            Task.Run(async () =>
            {
                await new EmailManager(appContext).SendEmailWithTemplateAsync(
                 user.UserId,
                 EmailType.System,
                 user.UserId,
                 user.Email,
                 EmailTemplateKeys.AdminTemplate.TemplateAccountInformation,
                 EmailFormatTypes.Admin,
                 new Dictionary<string, string>
                 {
                    { EmailKeys.USER_NAME, user.UserName },
                    { EmailKeys.FULL_NAME, user.DisplayName },
                    { EmailKeys.PASSWORD, password },
                    { EmailKeys.EMAIL, user.Email },
                    { EmailKeys.PHONE_NUMBER, user.MobileAlias },
                    { EmailKeys.LOGIN_URL, $"{hostPath}/login" },
                    { EmailKeys.COMPANY_NAME, companyName },
                    { EmailKeys.CURRENT_YEAR, DateTime.UtcNow.Year.ToString() },
                    { EmailKeys.SUPPORT_EMAIL, companyEmail },

                 }
             );
            });
        }
        #endregion

        #region Button
        private void NewUserAction(object sender, EventArgs e)
        {
            RefreshUserInfo();
            lbtSubmit.Visible = this.IsAdd;
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
            dlDetail.OpenModal(true);
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlUsers1.ConfirmRequest(e);
        }
        #endregion

    }
}