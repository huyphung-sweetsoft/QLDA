using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.MailManager;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fEmailTemplate.Controls
{
    public partial class CtrlEmailTemplateDetail : BaseAdminUserControl
    {
        private Guid QueryId
        {
            get
            {
                try
                {
                    string temp = CommonHelpers.QueryString("Id");
                    if (string.IsNullOrEmpty(temp))
                        return Guid.Empty;
                    return Guid.Parse(SecurityUtilities.UnprotectUrlParameter(temp));
                }
                catch
                {
                    return Guid.Empty;
                }
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            script.RegisterAsyncPostBackControl(lbtSubmit);
            script.RegisterAsyncPostBackControl(lbtDelete);
            if (!IsPostBack)
            {
                ApplyControlsText();
                if (Session["Message_Notify"] != null)
                {
                    ShowNotify((string)Session["Message_Notify"]);
                    Session["Message_Notify"] = null;
                }
                //-----------------------------------------
                BindDDL();
                lbtSubmit.Visible = this.CURRENT_PAGE.IsEdit;
                if (this.QueryId != Guid.Empty)
                {
                    lbtDelete.Visible = this.CURRENT_PAGE.IsDelete;
                    BindingData();
                }
            }
        }
        public void BindDDL()
        {
            ddlTemplateKey.Items.Clear();
            ddlTemplateKey.DataTextField = "Text";
            ddlTemplateKey.DataValueField = "Value";
            ddlTemplateKey.DataSource = EmailTemplateKeys.GetListItems();
            ddlTemplateKey.DataBind();
            ddlTemplateKey.Items.Insert(0, new ListItem(GetResourceText(BackEndResourceKeys.SELECT_VALUE), ""));
            ddlTemplateKey.SelectedIndex = -1;
            //---------------------------------------
            BindingExtraControls.BindDropdownEnum<EmailFormatTypes>(ddlEmailFormatType);
        }
        private void ApplyControlsText()
        {
            lbtBack.NavigateUrl = RewriteURLHelper.EmailTemplates;
            lbtBack.ToolTip = lbtBack.Text = GetResourceText(BackEndResourceKeys.BACK_TO_LIST);
            lbtDelete.ToolTip = lbtDelete.Text = GetResourceText(BackEndResourceKeys.DELETE);
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            //--------------------------------------------------------
            txtSubject.PlaceHolder = txtBCCEmail.PlaceHolder 
                = txtCCEmail.PlaceHolder = txtName.PlaceHolder
                = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            ddlTemplateKey.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            chkStatus.OnText = GetResourceText(BackEndResourceKeys.ACTIVE);
            chkStatus.OffText = GetResourceText(BackEndResourceKeys.INACTIVE);
        }

        private void BindingData()
        {
            try
            {
                TblEmailTemplate objEmailTemplate = EmailTemplateManager.GetEmailTemplateById(this.QueryId);
                if (objEmailTemplate == null || objEmailTemplate.IsDeleted)
                {
                    Response.Redirect(GetRelativeClientPath("/404"), false);
                    return;
                }
                lbtSubmit.Text = lbtSubmit.ToolTip = GetResourceText(BackEndResourceKeys.UPDATE);
                txtName.Text = objEmailTemplate.Name;
                txtSubject.Text = objEmailTemplate.Subject;
                txtBody.Text = Server.HtmlDecode(objEmailTemplate.Body);
                txtCCEmail.Text = objEmailTemplate.CCEmail;
                txtBCCEmail.Text = objEmailTemplate.BCCEmail;
                chkStatus.Checked = objEmailTemplate.IsActivated;
                ddlTemplateKey.SelectedValue = objEmailTemplate.TemplateKey;
                ddlEmailFormatType.SelectedValue = objEmailTemplate.EmailType.ToString();
                divSystem.Visible = true;
                lbCreateBy.Text = this.CURRENT_PAGE.DisplayName(objEmailTemplate.CreatedUser);
                lbCreatedDate.Text = ConvertDateTimeToString(objEmailTemplate.CreatedDate);
                lbUpdatedBy.Text = this.CURRENT_PAGE.DisplayName(objEmailTemplate.UpdatedUser);
                lbUpdatedDate.Text = ConvertDateTimeToString(objEmailTemplate.UpdatedDate);
                pnlValid.Update();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        #region Button
        protected void btnSave_ServerClick(object sender, EventArgs e)
        {
            try
            {
                #region Valid
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(pnlValid.Controls);

                CheckValidListEmail(txtCCEmail, validationEngine);
                CheckValidListEmail(txtBCCEmail, validationEngine);

                //if (!Helpers.IsValidType<EmailTemplateKeys>(ddlTemplateKey.SelectedValue))
                //    validationEngine.AddErrorPrompt(ddlTemplateKey.ClientID, GetResourceText(BackEndResourceKeys.TEMPLATE_KEY_DOES_NOT_EXIST));

                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }

                #endregion

                bool isAddNew = true;
                TblEmailTemplate objEmailTemplate = new TblEmailTemplate();

                #region Checking add or update, result: 'isAddNew'
                if (this.QueryId != Guid.Empty)
                {
                    if (!this.CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    objEmailTemplate = EmailTemplateManager.GetEmailTemplateById(this.QueryId);
                    if (objEmailTemplate == null)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }
                    else
                        isAddNew = false;
                }
                else
                {
                    if (!this.CURRENT_PAGE.IsAdd)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                }
                #endregion
                objEmailTemplate.Name= txtName.Text.Trim();
                objEmailTemplate.Subject = txtSubject.Text.Trim();
                objEmailTemplate.Body = Server.HtmlEncode(txtBody.Text);
                objEmailTemplate.TemplateKey = ddlTemplateKey.SelectedValue;
                objEmailTemplate.IsActivated = chkStatus.Checked;
                objEmailTemplate.CCEmail = txtCCEmail.Text;
                objEmailTemplate.BCCEmail = txtBCCEmail.Text;
                byte formatType = 0;
                if (this.CURRENT_PAGE.GetValue(ddlEmailFormatType, out formatType))
                    objEmailTemplate.EmailType = formatType;
                if (isAddNew)
                {
                    objEmailTemplate.Id = UUIDv7.NewGuid();
                    objEmailTemplate.CreatedDate = DateTime.UtcNow;
                    objEmailTemplate.CreatedUser = SweetContext.Current.UserName;
                    objEmailTemplate.Save();
                    Session["Message_Notify"] = GetResourceText(BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY);
                    Response.Redirect(RewriteURLHelper.EmailTemplateDetail(objEmailTemplate.Id), false);
                    return;
                }
                else
                {
                    objEmailTemplate.UpdatedDate = DateTime.UtcNow;
                    objEmailTemplate.UpdatedUser = SweetContext.Current.UserName;
                    objEmailTemplate.Save();
                }
                ShowSuccessSaveData();
                BindingData();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        private void CheckValidListEmail(ExtraTextBox extraTextBox, ValidationEngine validationEngine)
        {
            string error = string.Empty;
            if (!string.IsNullOrEmpty(extraTextBox.Text))
            {
                List<string> arrEmail = extraTextBox.Text.Split(',').ToList();
                if (arrEmail != null && arrEmail.Count > 0)
                {
                    foreach (var item in arrEmail)
                    {
                        if (!RegexUtilities.IsValidEmail(item.Trim()))
                        {
                            if (!string.IsNullOrEmpty(error))
                                error += ", ";
                            error += item;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(error))
                validationEngine.AddErrorPrompt(extraTextBox.ClientID, $"{GetResourceText(BackEndResourceKeys.THE_FOLLOWING_WORDS_ARE_NOT_IN_THE_CORRECT_EMAI_FORMAT)}: {error}");
        }

        protected void lbtDelete_Click(object sender, EventArgs e)
        {
            try
            {
                #region Check permissions
                if (!this.CURRENT_PAGE.IsDelete)
                {
                    ShowAccessDeniedNotify();
                    return;
                }
                #endregion
                TblEmailTemplate objEmailTemplate = EmailTemplateManager.GetEmailTemplateById(this.QueryId);
                if (objEmailTemplate == null || objEmailTemplate.IsDeleted)
                {
                    ShowInvalidNotFoundData();
                    return;
                }
                ConfirmResult result = new ConfirmResult();
                result.CommandName = "EMAIL_TEMPLATE_DELETE";
                result.Value = objEmailTemplate;
                this.CURRENT_PAGE.CurrentConfirmResult = result;
                MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION)
                    , string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THE_DATA), "email template")
                    , MSGButton.DeleteCancel, MSGIcon.Error);
                OpenMessageBox(msg, result, false, false);
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }
        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null)
            {
                if (e.Submit && e.CommandName != null)
                {
                    switch (e.CommandName)
                    {
                        case "EMAIL_TEMPLATE_DELETE":
                            TblEmailTemplate objEmailTemplate = e.Value as TblEmailTemplate;
                            if (objEmailTemplate == null)
                            {
                                ShowInvalidNotFoundData();
                                return;
                            }

                            try
                            {
                                objEmailTemplate.IsDeleted = true;
                                objEmailTemplate.Save();
                                Response.Redirect(RewriteURLHelper.EmailTemplates, false);
                                return;
                            }
                            catch (Exception exc)
                            {
                                ShowNotify(exc.Message, MSGType.Error);
                            }
                            break;
                    }
                }
                else
                {
                    ShowInvalidNotFoundData();
                    return;
                }
            }
        }
        #endregion
    }
}