using ImageResizer.Configuration.Logging;
using Newtonsoft.Json;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fUsers
{
    public partial class RoleDetail : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.Role;
            }
        }
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
        #region Function
        protected void Page_Load(object sender, EventArgs e)
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            script.RegisterAsyncPostBackControl(lbtSubmit);
            script.RegisterAsyncPostBackControl(lbtDelete);
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.USER_GROUP));
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.DETAIL);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    {RewriteURLHelper.Roles, GetResourceText(BackEndResourceKeys.USER_GROUP) },
                    {"javascript:;", GetResourceText(BackEndResourceKeys.DETAIL) }
                };
                ApplyControlsText();
                if (Session["Message_Notify"] != null)
                {
                    ShowNotify((string)Session["Message_Notify"]);
                    Session["Message_Notify"] = null;
                }
                //-----------------------------------------
                if (this.QueryId != Guid.Empty)
                {
                    lbtSubmit.Visible = this.IsEdit;
                    lbtDelete.Visible = this.IsDelete;
                    BindData();
                }
                else
                {
                    CtrlPermission1.RoleId = Guid.Empty;
                    CtrlPermission1.IsDisabled = !this.IsAdd;
                    CtrlPermission1.InitPermission();
                    lbtSubmit.Visible = this.IsAdd;
                    //-----------------------------------------
                }
            }
        }

        private void ApplyControlsText()
        {
            lbtBack.ToolTip = lbtBack.Text = GetResourceText(BackEndResourceKeys.BACK_TO_LIST);
            lbtDelete.ToolTip = lbtDelete.Text = GetResourceText(BackEndResourceKeys.DELETE);
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            //--------------------------------------------------------
            txtRoleName.PlaceHolder = txtSummary.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);
            chkStatus.OnText = GetResourceText(BackEndResourceKeys.ACTIVE);
            chkStatus.OffText = GetResourceText(BackEndResourceKeys.INACTIVE);
        }

        protected override void BindData()
        {
            try
            {
                AspnetRole role = RoleManager.Instance.GetRoleById(this.QueryId);
                if (role == null || role.IsDeleted)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), false);
                    return;
                }
                tagUsers.Visible = true;
                Navigation1.MainTitle = $"{GetResourceText(BackEndResourceKeys.USER_GROUP)}: [{role.RoleName}]";
                txtRoleName.Text = role.RoleName;
                txtSummary.Text = role.Description;
                chkStatus.Checked = role.IsActivated;
                //-----------------------------------------
                CtrlPermission1.IsDisabled = !this.IsEdit;
                CtrlPermission1.RoleId = role.RoleId;
                CtrlPermission1.InitPermission();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }
        #endregion

        #region Button
        protected void btnSave_ServerClick(object sender, EventArgs e)
        {
            try
            {
                #region Valid
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(pnlValid.Controls);
                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }
                #endregion

                AspnetRole aspnetRole = new AspnetRole();
                #region Checking add or update, result: 'isAddNew'
                if (this.QueryId != Guid.Empty)
                    aspnetRole.RoleId = this.QueryId;
                #endregion

                aspnetRole.RoleName = txtRoleName.Text.Trim();
                aspnetRole.LoweredRoleName = Helpers.NormalizeFileName(aspnetRole.RoleName);
                if (aspnetRole.RoleId == Guid.Empty && System.Web.Security.Roles.RoleExists(aspnetRole.LoweredRoleName))
                {
                    validationEngine.AddErrorPrompt(txtRoleName.ClientID, "* Tên nhóm người dùng đã tồn tại");
                    validationEngine.ShowErrorPrompt();
                    return;
                }
                aspnetRole.Description = txtSummary.Text.Trim();
                aspnetRole.IsActivated = chkStatus.Checked;
                aspnetRole.UpdatedDate = DateTime.UtcNow;
                aspnetRole.UpdatedBy = SweetContext.Current.UserName;
                if (aspnetRole.RoleId == Guid.Empty)
                {
                    aspnetRole.ApplicationId = SweetContext.Current.ApplicationId;
                    aspnetRole.CreatedDate = DateTime.UtcNow;
                    aspnetRole.CreatedBy = SweetContext.Current.UserName;
                    aspnetRole = RoleManager.Instance.CreateOrUpdate(aspnetRole);
                    CtrlPermission1.RoleId = aspnetRole.RoleId;
                    if (!CtrlPermission1.SavePermission())
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    if (aspnetRole != null)
                    {
                        Session["Message_Notify"] = GetResourceText(BackEndResourceKeys.NEW_DATA_ADDED_SUCCESSFULLY);
                        Response.Redirect(RewriteURLHelper.RoleDetail(aspnetRole.RoleId), false);
                        return;
                    }
                }
                else
                {
                    aspnetRole = RoleManager.Instance.CreateOrUpdate(aspnetRole);
                    if (aspnetRole == null)
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    if (!CtrlPermission1.SavePermission())
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    ShowSuccessSaveData();
                }
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }
        protected void lbtDelete_Click(object sender, EventArgs e)
        {
            try
            {
                #region Check permissions
                if (!this.IsDelete)
                {
                    ShowAccessDeniedNotify();
                    return;
                }
                #endregion
                AspnetRole aspnetRole = RoleManager.Instance.GetRoleById(this.QueryId);
                if (aspnetRole == null || aspnetRole.IsDeleted)
                {
                    ShowInvalidNotFoundData();
                    return;
                }
                ConfirmResult result = new ConfirmResult();
                result.CommandName = "ROLE_DELETE";
                result.Value = aspnetRole;
                CurrentConfirmResult = result;
                MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION)
                    , string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THE_DATA), aspnetRole.RoleName)
                    , MSGButton.AcceptCancel, MSGIcon.Error);
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
                        case "ROLE_DELETE":
                            AspnetRole aspnetRole = e.Value as AspnetRole;
                            if (aspnetRole == null)
                            {
                                ShowInvalidNotFoundData();
                                return;
                            }

                            try
                            {
                                RoleManager.Instance.Delete(aspnetRole);
                                Response.Redirect(RewriteURLHelper.Roles, false);
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

        public override void LoadTab(string tabKey)
        {
            switch (tabKey)
            {
                case "user-list":
                    CtrlUsers1.RoleId = this.QueryId;
                    CtrlUsers1.InitControls();
                    break;
            }
        }
    }
}