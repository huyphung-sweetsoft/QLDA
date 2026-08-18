using OfficeOpenXml;
using OfficeOpenXml.Style;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.ExcelManager;
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
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using static SweetSoft.QLDA.Controls.EnumHelper;

namespace SweetSoft.QLDA.BackOffice.fUsers.Controls
{
    public partial class CtrlUsers : BaseAdminUserControl
    {
        public EventHandler NewUserHandlerCallback;
        public EventHandler EditUserHandlerCallback;
        public EventHandler SendMailHandlerCallback;
        public Guid RoleId
        {
            get
            {
                if (ViewState["RoleId"] == null)
                    return Guid.Empty;
                return (Guid)ViewState["RoleId"];
            }
            set
            {
                ViewState["RoleId"] = value;
            }
        }
        protected bool IsView
        {
            get
            {
                if (this.RoleId != Guid.Empty)
                    return false;
                return this.CURRENT_PAGE.IsView;
            }
        }
        protected bool IsEdit
        {
            get
            {
                if (this.CURRENT_PAGE.IsUserRight(ActionKeys.Update, ModuleKeys.User))
                    return true;
                return false;
            }
        }
        protected bool IsDelete
        {
            get
            {
                if (this.CURRENT_PAGE.IsUserRight(ActionKeys.Delete, ModuleKeys.User))
                    return true;
                return false;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButton();
        }

        private void RegisterAsyncButton()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            script.RegisterAsyncPostBackControl(lbtSearchSingle);
            script.RegisterAsyncPostBackControl(lbtCancel);
            script.RegisterAsyncPostBackControl(lbtSearchAdvanced);
            script.RegisterPostBackControl(btnExport);
        }
        private void ApplyControlsText()
        {
            txtSearchSingle.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            txtSearchUserName.SearchTagItemText = GetResourceText(BackEndResourceKeys.USER_NAME);
            txtSearchFullName.SearchTagItemText = GetResourceText(BackEndResourceKeys.DISPLAY_NAME);
            txtSearchEmail.SearchTagItemText = "Email";
            txtSearchPhone.SearchTagItemText = GetResourceText(BackEndResourceKeys.PHONE_NUMBER);
            txtSearchCreatedDate.SearchTagItemText = GetResourceText(BackEndResourceKeys.CREATED_DATE);
            ddlSearchStatus.SearchTagItemText = GetResourceText(BackEndResourceKeys.STATUS);
            ddlSearchRole.SearchTagItemText = GetResourceText(BackEndResourceKeys.USER_GROUP);
            //------------------------------------------------
            lbtAdd.ToolTip = lbtAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);
            lbtCancel.ToolTip = lbtCancel.Text = GetResourceText(BackEndResourceKeys.REFRESH);
            lbtSearchAdvanced.ToolTip = lbtSearchAdvanced.Text = GetResourceText(BackEndResourceKeys.SEARCH);
            btnExport.ToolTip = btnExport.Text = GetResourceText(BackEndResourceKeys.EXPORT_EXCEL);
            //------------------------------------------------
            txtSearchFullName.PlaceHolder = txtSearchEmail.PlaceHolder
                = txtSearchPhone.PlaceHolder = txtSearchSingle.PlaceHolder
                = txtSearchUserName.PlaceHolder 
                = GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            txtSearchCreatedDate.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_DATE);
            //------------------------------------------------
            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.ACCOUNT),
                "Email",
                GetResourceText(BackEndResourceKeys.PHONE_NUMBER),
                GetResourceText(BackEndResourceKeys.USER_GROUP),
                GetResourceText(BackEndResourceKeys.STATUS),
                "2FA",
                GetResourceText(BackEndResourceKeys.LAST_LOGIN_DATE),
                GetResourceText(BackEndResourceKeys.ACTION),
            };
            grvData.HeaderTexts = lstTableHeader;
        }
        #region Search + Init gridview
        public void Rebind()
        {
            grvData.CurrentPageIndex = 1;
            grvData.Rebind();
        }
        public void InitControls()
        {
            ApplyControlsText();
            AssignSearchColumns();
            ControlHelpers controlHelpers = new ControlHelpers();
            controlHelpers.BindStatus(ddlSearchStatus);
            controlHelpers.BindRoles(ddlSearchRole);
            if (this.RoleId != Guid.Empty)
                ddlSearchRole.SelectedValue = this.RoleId.ToString();
            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
            lbtAdd.Visible = this.CURRENT_PAGE.IsAdd && this.RoleId == Guid.Empty;
            grvData.Columns[5].Visible 
                = grvData.Columns[8].Visible
                = tagOther.Visible  
                = this.RoleId == null || this.RoleId == Guid.Empty;
            MasterTemplate master = Page.Master as MasterTemplate;
            master.LoadSessionLastSearch(searchTagBox, pnlSearchPopup, grvData, txtSearchSingle);
            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = AspnetUser.Columns.UserName;
            grvData.CurrentSortDerection = "ASC";
            grvData.Rebind();
            pnlButtons.Update();
            pnlSearch.Update();
        }
        private void AssignSearchColumns()
        {
            txtSearchUserName.SearchColumn = AspnetUser.Columns.UserName;
            txtSearchFullName.SearchColumn = AspnetUser.Columns.DisplayName;
            txtSearchEmail.SearchColumn = AspnetMembership.Columns.Email;
            txtSearchPhone.SearchColumn = AspnetUser.Columns.MobileAlias;
            ddlSearchStatus.SearchColumn = AspnetUser.Columns.IsActivated;
            ddlSearchRole.SearchColumn = AspnetRole.Columns.RoleId;
            txtSearchCreatedDate.SearchColumn = AspnetUser.Columns.LastActivityDate;
            ddlSearchRole.Enabled = this.RoleId == Guid.Empty;
        }
        protected void grvData_NeedDataSource(object sender, ExtraGridEventArg e)
        {
            try
            {
                GridviewExtension grid = sender as GridviewExtension;
                if (grid == null)
                {
                    this.ShowInvalidDataError();
                    return;
                }

                int totalRows = 0;
                int rowIndex = (grid.CurrentPageIndex - 1) * grid.CurrentPageSize;
                int pageSize = rowIndex + grid.CurrentPageSize;
                //--------------------------------------------
                DataTable dt = null;
                if (grid.GridSearchType == GridSearchType.Single)
                {
                    Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
                    ControlHelpers controlHelpers = new ControlHelpers();
                    keyValueSearchs = controlHelpers.GetControlValues(pnlSearchDefault);
                    // Add RoleId to search criteria
                    if(this.RoleId != Guid.Empty)
                    {
                        if (!keyValueSearchs.ContainsKey("RoleId"))
                            keyValueSearchs.Add("RoleId", this.RoleId);
                        else
                            keyValueSearchs["RoleId"] = this.RoleId;
                    }
                    dt = UserManager.Instance.SearchUsers(txtSearchSingle.Text, keyValueSearchs, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                }    
                else
                {
                    Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
                    ControlHelpers controlHelpers = new ControlHelpers();
                    var temp = controlHelpers.GetControlValues(pnlSearchDefault);
                    keyValueSearchs.AddIfNotExists(temp);
                    temp = controlHelpers.GetControlValues(pnlSearchPopup);
                    keyValueSearchs.AddIfNotExists(temp);
                    // Add RoleId to search criteria
                    if(this.RoleId != Guid.Empty)
                    {
                        if (!keyValueSearchs.ContainsKey("RoleId"))
                            keyValueSearchs.Add("RoleId", this.RoleId);
                        else
                            keyValueSearchs["RoleId"] = this.RoleId;
                    }
                    dt = UserManager.Instance.SearchUsers(keyValueSearchs, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                }
                if (dt == null || dt.Rows.Count == 0)
                {
                    grvData.DataSource = null;
                    grvData.DataBind();
                    ctrlGridviewPaging.Visible = btnExport.Visible = false;
                }
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        ctrlGridviewPaging.Visible = true;
                        btnExport.Visible = this.CURRENT_PAGE.IsExportExcel;
                    }
                    else
                        ctrlGridviewPaging.Visible = btnExport.Visible = false;
                    grvData.VirtualItemCount = totalRows;
                    grvData.DataSource = dt;
                    grvData.DataBind();
                    ctrlGridviewPaging.PageIndex = grvData.CurrentPageIndex;
                    ctrlGridviewPaging.PageSize = grvData.CurrentPageSize;
                    ctrlGridviewPaging.TotalItems = totalRows;
                    ctrlGridviewPaging.InitLoad();
                }
                //-------------------------------------------------
                upMain.Update();
                pnlButtons.Update();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void grvData_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "ITEM_DETAIL":
                    if (!this.CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    //--------------------------------------------
                    int rowIndex = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndex = ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);
                    Guid userId = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out userId))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    if (UserManager.Instance.IsAdministrator(userId) && !SweetContext.Current.IsAdministrator)
                    {
                        ShowNotify(GetResourceText(BackEndResourceKeys.THE_ACCOUNT_DOES_NOT_HAVE_PERMISSION_TO_PERFORM_THIS_ACTION));
                        return;
                    }
                    if (EditUserHandlerCallback != null && (this.RoleId == null || this.RoleId == Guid.Empty))
                        EditUserHandlerCallback(userId, EventArgs.Empty);
                    else
                        Response.Redirect(RewriteURLHelper.ViewUser(userId));
                    break;
                case "ITEM_DELETE":
                    if (!this.CURRENT_PAGE.IsDelete)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    //--------------------------------------------
                    rowIndex = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndex = ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);

                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out userId))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    if (UserManager.Instance.IsAdministrator(userId) && !SweetContext.Current.IsAdministrator)
                    {
                        ShowNotify(GetResourceText(BackEndResourceKeys.THE_ACCOUNT_DOES_NOT_HAVE_PERMISSION_TO_PERFORM_THIS_ACTION));
                        return;
                    }

                    AspnetUser user = UserManager.Instance.GetUserById(userId);
                    if (user == null)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }
                    ConfirmResult result = new ConfirmResult();
                    result.CommandName = "USER_DELETE";
                    result.Value = user;
                    this.CURRENT_PAGE.CurrentConfirmResult = result;
                    MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION)
                        , string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THE_DATA), user.DisplayName)
                        , MSGButton.DeleteCancel, MSGIcon.Error);
                    OpenMessageBox(msg, result, false, false);
                    break;
                case "RESET_PASSWORD":
                    if (!this.CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    //--------------------------------------------
                    rowIndex = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndex = ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);

                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out userId))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    if (UserManager.Instance.IsAdministrator(userId) && !SweetContext.Current.IsAdministrator)
                    {
                        ShowNotify(GetResourceText(BackEndResourceKeys.THE_ACCOUNT_DOES_NOT_HAVE_PERMISSION_TO_PERFORM_THIS_ACTION));
                        return;
                    }

                    user = UserManager.Instance.GetUserById(userId);
                    if (user == null)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }

                    result = new ConfirmResult();
                    result.CommandName = "USER_RESET_PASSWORD";
                    result.Value = user;
                    this.CURRENT_PAGE.CurrentConfirmResult = result;
                    msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION)
                        , string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_RESET_PASSWORD_FOR_ACCOUNT), user.DisplayName), MSGButton.Send, MSGIcon.Warning);
                    OpenMessageBox(msg, result, false, false);
                    break;
            }
        }

        protected void ctrlGridviewPaging_PageChanged(object sender, GridviewCustomPageChangeArgs e)
        {
            grvData.CurrentPageSize = e.CurrentPageSize;
            grvData.CurrentPageIndex = e.CurrentPageNumber;
            grvData.Rebind();
        }
        protected void bootstrapDropdown_SelectedValueChanged(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (grvData.GridSearchType == GridSearchType.Single)
                master.btnSearchSingle_Click(searchTagBox, pnlSearchDefault, grvData, txtSearchSingle);
            else
                master.btnSearchAdvanced_Click(searchTagBox, pnlSearchDefault, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }
        #endregion

        #region Button
        protected void lbtAdd_Click(object sender, EventArgs e)
        {
            if (!this.CURRENT_PAGE.IsAdd)
            {
                ShowAccessDeniedNotify();
                return;
            }
            if (NewUserHandlerCallback != null)
                NewUserHandlerCallback(Guid.Empty, EventArgs.Empty);
        }
        protected void btnSearch_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            master.btnSearchSingle_Click(searchTagBox, grvData, txtSearchSingle);
            upSearchTagBox.Update();

        }
        protected void btnSearchAdvanced_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            master.btnSearchAdvanced_Click(searchTagBox, pnlSearchDefault, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            new ControlHelpers().ClearControlValues(pnlSearch.Controls);
            pnlSearch.Update();
            MasterTemplate master = Page.Master as MasterTemplate;
            master.btnSearchAdvanced_Click(searchTagBox, pnlSearchDefault, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }
        protected void searchTagBox_TagClosed(object sender, SearchTagItem tag)
        {
            try
            {
                MasterTemplate master = Page.Master as MasterTemplate;
                GridSearchType? searchType;
                master.searchTagBox_TagClosed(searchTagBox, tag, pnlSearchDefault, pnlSearchPopup, grvData, txtSearchSingle, out searchType);
                upnlSearchDefault.Update();
                pnlSearch.Update();
                string script = string.Format("$('#{0}').val('');", txtSearchSingle.ClientID);
                ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "UpdateTxtSearch", script, true);
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }
        protected void btnExport_Click(object sender, EventArgs e)
        {
            if (!this.CURRENT_PAGE.IsExportExcel)
            {
                ShowAccessDeniedNotify();
                return;
            }

            #region Get data 
            int totalRows = 0;
            int rowIndex = (grvData.CurrentPageIndex - 1) * grvData.CurrentPageSize;
            int pageSize = rowIndex + grvData.CurrentPageSize;
            //-----------------------------------------------
            DataTable dt = null;
            if (grvData.GridSearchType == GridSearchType.Single)
            {
                Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
                ControlHelpers controlHelpers = new ControlHelpers();
                keyValueSearchs = controlHelpers.GetControlValues(pnlSearchDefault);
                // Add RoleId to search criteria
                if(this.RoleId != Guid.Empty)
                {
                    if (!keyValueSearchs.ContainsKey("RoleId"))
                        keyValueSearchs.Add("RoleId", this.RoleId);
                    else
                        keyValueSearchs["RoleId"] = this.RoleId;
                }
                dt = UserManager.Instance.SearchUsers(txtSearchSingle.Text, keyValueSearchs, $"{grvData.CurrentSortExpression} {grvData.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
            }
            else
            {
                Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
                ControlHelpers controlHelpers = new ControlHelpers();
                var temp = controlHelpers.GetControlValues(pnlSearchDefault);
                keyValueSearchs.AddIfNotExists(temp);
                temp = controlHelpers.GetControlValues(pnlSearchPopup);
                keyValueSearchs.AddIfNotExists(temp);
                // Add RoleId to search criteria
                if (this.RoleId != Guid.Empty)
                {
                    if (!keyValueSearchs.ContainsKey("RoleId"))
                        keyValueSearchs.Add("RoleId", this.RoleId);
                    else
                        keyValueSearchs["RoleId"] = this.RoleId;
                }
                dt = UserManager.Instance.SearchUsers(keyValueSearchs, $"{grvData.CurrentSortExpression} {grvData.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
            }
            #endregion
            ExcelExportCore excelExportCore = new ExcelExportCore();
            string subject = GetResourceText(BackEndResourceKeys.USER_LIST);
            var options = new ExcelExportOptions
            {
                SheetName = subject,
                ColumnStyles = new Dictionary<string, Action<ExcelRange>>()
                {

                    { "LastActivityDate", range =>
                        {
                            range.Style.Numberformat.Format = "dd-mmm-yyyy";
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        }
                    },
                },
                IsFixedHeader = true,
                EnableZebraStripe = true,
                ImageType = OfficeOpenXml.Drawing.ePictureType.Png,
                LogoHeight = 80,
                LogoWidth = 250,
                LogoCols = 2,
                IsLogoCenter = true,
                ColumnNames = new List<string>()
                {
                    GetResourceText(BackEndResourceKeys.USER_NAME),
                    GetResourceText(BackEndResourceKeys.FULL_NAME),
                    "Địa chỉ email",
                    GetResourceText(BackEndResourceKeys.PHONE_NUMBER),
                    GetResourceText(BackEndResourceKeys.USER_GROUP),
                    GetResourceText(BackEndResourceKeys.STATUS),
                    GetResourceText(BackEndResourceKeys.CREATED_DATE)
                },
                ShowColumns = new HashSet<string>()
                {
                    "UserName",
                    "DisplayName",
                    "Email",
                    "MobileAlias",
                    "RoleName",
                    "IsActivated",
                    "LastActivityDate",
                },
                ConditionalMappingTexts = new List<ConditionalMappingText>
                {
                     new ConditionalMappingText
                    {
                        ColumnName = "IsActivated",
                        ValueMappings = new Dictionary<string, string>
                        {
                            { "True", GetResourceText(BackEndResourceKeys.ACTIVE) },
                            { "False", GetResourceText(BackEndResourceKeys.INACTIVE)},
                        },
                        DefaultText = GetResourceText(BackEndResourceKeys.ACTIVE)
                    },
                }
            };
            byte[] bytes = excelExportCore.ExportExcel(dt, subject, options);
            string filename = string.Format("{1} {0:dd-MM-yyyy HH-mm}.xlsx", DateTime.Now, Helpers.NormalizeFileName(subject));
            Response.Clear();

            MemoryStream ms = new MemoryStream(bytes);
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;filename=" + filename);
            Response.Buffer = true;
            ms.WriteTo(Response.OutputStream);
            Response.Flush();
            Response.End();
        }
        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null)
            {
                if (e.Submit && e.CommandName != null)
                {
                    if (e.CommandName.Contains("USER_DELETE"))
                    {
                        AspnetUser user = e.Value as AspnetUser;
                        if (user == null)
                        {
                            ShowInvalidNotFoundData();
                            return;
                        }

                        try
                        {
                            UserManager.Instance.Delete(user);
                            ShowSuccessDeleteData();
                            grvData.CurrentPageIndex = 1;
                            grvData.Rebind();
                        }
                        catch (Exception exc)
                        {
                            ShowNotify(exc.Message, MSGType.Error);
                        }
                    }
                    else if (e.CommandName.Contains("USER_RESET_PASSWORD"))
                    {
                        AspnetUser user = e.Value as AspnetUser;
                        if (user == null)
                        {
                            ShowInvalidNotFoundData();
                            return;
                        }

                        try
                        {
                            bool isDefault = false;
                            if (WebConfigurationManager.AppSettings["IsUsedDefaultPassword"] != null)
                                isDefault = bool.Parse(WebConfigurationManager.AppSettings["IsUsedDefaultPassword"]);

                            string password = string.Empty;
                            if (isDefault)
                                password = this.CURRENT_PAGE.DefaultPassword;
                            else
                                password = SecurityUtilities.CreateAlphaNumericString(8);

                            MembershipUser membershipUser = Membership.GetUser(user.UserName);
                            if (membershipUser == null)
                            {
                                ShowInvalidDataError();
                                return;
                            }
                            string oldPass = membershipUser.ResetPassword();
                            if (!membershipUser.ChangePassword(oldPass, password))
                            {
                                ShowNotify(GetResourceText(BackEndResourceKeys.UNABLE_TO_UPDATE_PASSWORD_FOR_ACCOUNT));
                                return;
                            }
                            Membership.UpdateUser(membershipUser);
                            user.Email = membershipUser.Email;
                            user.ResetPasswordKey = string.Empty;
                            user.Save();
                            if (user != null)
                            {
                                if(SendMailHandlerCallback != null)
                                    SendMailHandlerCallback(new { User = user, Password = password }, EventArgs.Empty);
                                ShowNotify(GetResourceText(BackEndResourceKeys.THE_NEW_PASSWORD_HAS_BEEN_SENT_TO_THE_ACCOUNT_S_EMAIL_ADDRESS));
                            }
                        }
                        catch (Exception exc)
                        {
                            ShowNotify(exc.Message, MSGType.Error);
                        }
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