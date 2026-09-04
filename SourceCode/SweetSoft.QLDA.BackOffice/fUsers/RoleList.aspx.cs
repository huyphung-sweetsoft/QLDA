using ImageResizer.Configuration.Logging;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static SweetSoft.QLDA.Controls.EnumHelper;

namespace SweetSoft.QLDA.BackOffice.fUsers
{
    public partial class RoleList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.Role;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButton();
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.USER_GROUP));
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    {RewriteURLHelper.LichBieu, GetResourceText(BackEndResourceKeys.DASHBOARD) }
                };
                ApplyControlsText();
                AssignSearchColumns();
                ControlHelpers controlHelpers = new ControlHelpers();
                controlHelpers.BindUsers(ddlSearchCreatedBy, true);
                controlHelpers.BindUsers(ddlSearchUpdatedBy, true);
                controlHelpers.BindStatus(ddlSearchStatus, true);
                txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
                MasterTemplate master = Page.Master as MasterTemplate;
                master.LoadSessionLastSearch(searchTagBox, pnlSearchPopup, grvData, txtSearchSingle);
                InitGridData();
                lbtAdd.Visible = this.IsAdd;
            }
        }
        private void RegisterAsyncButton()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            script.RegisterAsyncPostBackControl(lbtSearchSingle);
            script.RegisterAsyncPostBackControl(lbtSearchAdvanced);
            script.RegisterAsyncPostBackControl(lbtCancel);
        }
        private void ApplyControlsText()
        {
            Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.USER_GROUP);
            txtSearchSingle.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            txtSearchRoleName.SearchTagItemText = GetResourceText(BackEndResourceKeys.NAME);
            txtSearchSummary.SearchTagItemText = GetResourceText(BackEndResourceKeys.SUMMARY);
            ddlSearchStatus.SearchTagItemText = GetResourceText(BackEndResourceKeys.STATUS);
            ddlSearchCreatedBy.SearchTagItemText = GetResourceText(BackEndResourceKeys.CREATED_BY);
            ddlSearchUpdatedBy.SearchTagItemText = GetResourceText(BackEndResourceKeys.UPDATED_BY);
            txtSearchCreatedDate.SearchTagItemText = GetResourceText(BackEndResourceKeys.CREATED_DATE);
            txtSearchUpdatedDate.SearchTagItemText = GetResourceText(BackEndResourceKeys.UPDATED_DATE);
            //------------------------------------------------
            lbtAdd.ToolTip = lbtAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);
            lbtCancel.ToolTip = lbtCancel.Text = GetResourceText(BackEndResourceKeys.REFRESH);
            lbtSearchAdvanced.ToolTip = lbtSearchAdvanced.Text = GetResourceText(BackEndResourceKeys.SEARCH);
            //------------------------------------------------
            txtSearchRoleName.PlaceHolder = txtSearchSingle.PlaceHolder
                = txtSearchSummary.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            ddlSearchStatus.PlaceHolder = ddlSearchCreatedBy.PlaceHolder = ddlSearchUpdatedBy.PlaceHolder
                = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            txtSearchCreatedDate.PlaceHolder = txtSearchUpdatedDate.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_DATE);
            //------------------------------------------------
            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.NAME),
                GetResourceText(BackEndResourceKeys.SUMMARY),
                GetResourceText(BackEndResourceKeys.STATUS),
                GetResourceText(BackEndResourceKeys.CREATED_DATE),
                GetResourceText(BackEndResourceKeys.ACTION),
            };
            grvData.HeaderTexts = lstTableHeader;
        }
        private void AssignSearchColumns()
        {
            txtSearchRoleName.SearchColumn = AspnetRole.Columns.RoleName;
            ddlSearchStatus.SearchColumn = AspnetRole.Columns.IsActivated;
            ddlSearchCreatedBy.SearchColumn = AspnetRole.Columns.CreatedBy;
            ddlSearchUpdatedBy.SearchColumn = AspnetRole.Columns.UpdatedBy;
            txtSearchCreatedDate.SearchColumn = AspnetRole.Columns.CreatedDate;
            txtSearchUpdatedDate.SearchColumn = AspnetRole.Columns.UpdatedDate;

        }
        #region Search + Init gridview
        private void InitGridData()
        {
            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = AspnetRole.Columns.RoleName;
            grvData.CurrentSortDerection = "ASC";
            grvData.Rebind();
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
                //--------------------------
                DataTable dt = null;
                if (grid.GridSearchType == GridSearchType.Single)
                    dt = RoleManager.Instance.SearchRoles(txtSearchSingle.Text, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                else
                {
                    Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
                    ControlHelpers controlHelpers = new ControlHelpers();
                    keyValueSearchs = controlHelpers.GetControlValues(pnlSearchPopup);
                    dt = RoleManager.Instance.SearchRoles(keyValueSearchs, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                }
                if (dt == null || dt.Rows.Count == 0)
                {
                    lbtDeleteMultiple.Visible=false;
                    grvData.DataSource = null;
                    grvData.DataBind();
                    ctrlGridviewPaging.Visible = false;
                }
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        ctrlGridviewPaging.Visible = true;
                        lbtDeleteMultiple.Visible = this.IsDelete;
                    }    
                    else
                        ctrlGridviewPaging.Visible = lbtDeleteMultiple.Visible = false;
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
                    if (!this.IsEdit && !this.IsView)
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
                    Guid roleId = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out roleId))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    Response.Redirect(RewriteURLHelper.RoleDetail(roleId));
                    break;
                case "ITEM_DELETE":
                    if (!this.IsDelete)
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

                    roleId = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out roleId))
                        return;

                    AspnetRole role = RoleManager.Instance.GetRoleById(roleId);
                    if (role == null)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }

                    ConfirmResult result = new ConfirmResult();
                    result.CommandName = "ROLE_DELETE";
                    result.Value = role;
                    CurrentConfirmResult = result;
                    MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION)
                        , string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THE_DATA), role.RoleName)
                        , MSGButton.AcceptCancel, MSGIcon.Error);
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
        #endregion

        #region Button
        protected void btnSearch_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            master.btnSearchSingle_Click(searchTagBox, grvData, txtSearchSingle);
            upSearchTagBox.Update();

        }
        protected void btnSearchAdvanced_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            master.btnSearchAdvanced_Click(searchTagBox, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            new ControlHelpers().ClearControlValues(pnlSearch.Controls);
            pnlSearch.Update();
            MasterTemplate master = Page.Master as MasterTemplate;
            master.btnSearchAdvanced_Click(searchTagBox, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }
        protected void searchTagBox_TagClosed(object sender, SearchTagItem tag)
        {
            try
            {
                MasterTemplate master = Page.Master as MasterTemplate;
                GridSearchType? searchType;
                master.searchTagBox_TagClosed(searchTagBox, tag, pnlSearchPopup, grvData, txtSearchSingle, out searchType);
                pnlSearch.Update();
                string script = string.Format("$('#{0}').val('');", txtSearchSingle.ClientID);
                ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "UpdateTxtSearch", script, true);
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }
        protected void lbtDeleteMultiple_Click(object sender, EventArgs e)
        {
            try
            {
                if (!this.IsDelete)
                {
                    ShowAccessDeniedNotify();
                    return;
                }

                grvData.HandleGetSelectedColumns(grvData.UniqueID, Request.Params);
                List<GridviewExtension.DataTable> lstDataSelected = grvData.SelectedColumns;
                if (lstDataSelected == null || lstDataSelected.Count == 0)
                {
                    this.NoDataSelectedForDeletion();
                    return;
                }

                StringBuilder str = new StringBuilder();
                foreach (var item in lstDataSelected)
                {
                    str.AppendFormat(GridviewExtension.TEMPLATE_DELETE_ITEM, GetResourceText(BackEndResourceKeys.USER_GROUP), item.Name, item.Id);
                }
                string templateDelete = string.Format(GridviewExtension.TEMPLATE_WRAPPER_DELETE_MULTIPLE
                    , GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THESE_DATA), str);
                ConfirmResult result = new ConfirmResult();
                result.CommandName = "ROLE_DELETE_MULTIPLE";
                result.Value = lstDataSelected;
                this.CurrentConfirmResult = result;
                MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION)
                    , templateDelete, MSGButton.AcceptCancel, MSGIcon.Error);
                OpenMessageBox(msg, result, false, false);
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }
        #endregion
        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null)
            {
                if (e.Submit && e.CommandName != null)
                {
                    if (e.CommandName.Equals("ROLE_DELETE"))
                    {
                        AspnetRole role = e.Value as AspnetRole;
                        if (role == null)
                        {
                            ShowInvalidNotFoundData();
                            return;
                        }

                        try
                        {
                            RoleManager.Instance.Delete(role);
                            ShowSuccessDeleteData();
                            grvData.CurrentPageIndex = 1;
                            grvData.Rebind();
                        }
                        catch (Exception exc)
                        {
                            ShowNotify(exc.Message, MSGType.Error);
                        }
                    }
                    else if (e.CommandName.Equals("ROLE_DELETE_MULTIPLE"))
                    {
                        if (!this.IsDelete)
                        {
                            ShowAccessDeniedNotify();
                            return;
                        }

                        grvData.HandleGetSelectedColumns(grvData.UniqueID, Request.Params);
                        List<GridviewExtension.DataTable> lstDataSelected = grvData.SelectedColumns;
                        if (lstDataSelected == null || lstDataSelected.Count == 0)
                        {
                            this.NoDataSelectedForDeletion();
                            return;
                        }
                        RoleManager roleManager = RoleManager.Instance;
                        foreach (var item in lstDataSelected)
                        {
                            if (item.Id == Guid.Empty)
                                continue;

                            AspnetRole role = roleManager.GetRoleById(item.Id);
                            if (role == null)
                                continue;
                            roleManager.Delete(role);
                        }

                        ShowSuccessDeleteData();
                        grvData.CurrentPageIndex = 1;
                        grvData.Rebind();
                    }
                }
                else
                {
                    ShowInvalidNotFoundData();
                    return;
                }
            }
        }

        protected void lbtAdd_Click(object sender, EventArgs e)
        {
            if (!this.IsAdd)
            {
                ShowAccessDeniedNotify();
                return;
            }
            Response.Redirect(RewriteURLHelper.AddRole);
        }
    }
}