using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.MailManager;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using static SweetSoft.QLDA.Controls.EnumHelper;

namespace SweetSoft.QLDA.BackOffice.fEmailTemplate.Controls
{
    public partial class CtrlTemplates : BaseAdminUserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButton();
        }
        public void InitControl()
        {
            ApplyControlsText();
            lbtDeleteMultiple.Visible = this.CURRENT_PAGE.IsDelete;
            lbtAdd.NavigateUrl = RewriteURLHelper.EmailTemplateNew;
            lbtAdd.Visible = this.CURRENT_PAGE.IsEdit;
            ControlHelpers controlHelpers = new ControlHelpers();
            controlHelpers.BindUsers(ddlSearchCreatedUser, true);
            controlHelpers.BindUsers(ddlSearchUpdatedUser, true);
            controlHelpers.BindStatus(ddlSearchStatus, true);
            BindDDL();

            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
            MasterTemplate master = Page.Master as MasterTemplate;
            master.LoadSessionLastSearch(searchTagBox, pnlSearchPopup, grvData, txtSearchSingle);
            InitGridData();
        }
        public void BindDDL()
        {
            ddlSearchTemplateKey.Items.Clear();
            ddlSearchTemplateKey.DataTextField = "Text";
            ddlSearchTemplateKey.DataValueField = "Value";
            ddlSearchTemplateKey.DataSource = EmailTemplateKeys.GetListItems();
            ddlSearchTemplateKey.DataBind();
            ddlSearchTemplateKey.Items.Insert(0, new ListItem(GetResourceText(BackEndResourceKeys.ALL), ""));
            ddlSearchTemplateKey.SelectedIndex = -1;
        }
        private void RegisterAsyncButton()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            script.RegisterAsyncPostBackControl(lbtSearchSingle);
            script.RegisterAsyncPostBackControl(lbtCancel);
            script.RegisterAsyncPostBackControl(lbtSearchAdvanced);
        }
        private void ApplyControlsText()
        {
            txtSearchSingle.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            txtSearchName.SearchTagItemText = GetResourceText(BackEndResourceKeys.NAME);
            ddlSearchCreatedUser.SearchTagItemText = GetResourceText(BackEndResourceKeys.CREATED_BY);
            ddlSearchUpdatedUser.SearchTagItemText = GetResourceText(BackEndResourceKeys.UPDATED_BY);
            dtSearchCreatedDate.SearchTagItemText = GetResourceText(BackEndResourceKeys.CREATED_DATE);
            dtSearchUpdatedDate.SearchTagItemText = GetResourceText(BackEndResourceKeys.UPDATED_DATE);
            ddlSearchStatus.SearchTagItemText = GetResourceText(BackEndResourceKeys.STATUS);
            ddlSearchTemplateKey.SearchTagItemText = GetResourceText(BackEndResourceKeys.TEMPLATE_KEY);
            //------------------------------------------------
            txtSearchSingle.PlaceHolder = txtSearchName.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            ddlSearchCreatedUser.PlaceHolder = ddlSearchStatus.PlaceHolder = ddlSearchUpdatedUser.PlaceHolder
                = ddlSearchTemplateKey.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            dtSearchCreatedDate.PlaceHolder = dtSearchUpdatedDate.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_DATE);
            //------------------------------------------------
            lbtAdd.ToolTip = lbtAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);
            lbtSearchAdvanced.ToolTip = lbtSearchAdvanced.Text = GetResourceText(BackEndResourceKeys.SEARCH);
            lbtCancel.ToolTip = lbtCancel.Text = GetResourceText(BackEndResourceKeys.REFRESH);
            lbtDeleteMultiple.ToolTip = lbtDeleteMultiple.Text = GetResourceText(BackEndResourceKeys.BULK_DELETE);
            //-----------------------------------------------------------
            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.TEMPLATE_NAME),
                GetResourceText(BackEndResourceKeys.TEMPLATE_KEY),
                "Đối tượng áp dụng",
                GetResourceText(BackEndResourceKeys.STATUS),
                GetResourceText(BackEndResourceKeys.CREATED_BY),
                GetResourceText(BackEndResourceKeys.CREATED_DATE),
                GetResourceText(BackEndResourceKeys.UPDATED_BY),
                GetResourceText(BackEndResourceKeys.UPDATED_DATE),
                GetResourceText(BackEndResourceKeys.ACTION),
            };
            grvData.HeaderTexts = lstTableHeader;
        }

        #region Search + Init gridview
        public void InitGridData()
        {
            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = "CreatedDate";
            grvData.CurrentSortDerection = "DESC";
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
                    dt = EmailTemplateManager.SearchPaging(txtSearchSingle.Text, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                else
                {
                    Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
                    ControlHelpers controlHelpers = new ControlHelpers();
                    keyValueSearchs = controlHelpers.GetControlValues(pnlSearchPopup);
                    dt = EmailTemplateManager.SearchPaging(keyValueSearchs, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                }
                if (dt == null || dt.Rows.Count == 0)
                {
                    grvData.DataSource = null;
                    grvData.DataBind();
                    ctrlGridviewPaging.Visible = liDeleteMultiple.Visible = false;
                }
                else
                {
                    ctrlGridviewPaging.Visible = liDeleteMultiple.Visible = true;
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
                ShowSystemError();
                throw new Exception("EmailTemplates", exc);
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
                    Guid emailTemplateId = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out emailTemplateId))
                        return;
                    Response.Redirect(RewriteURLHelper.EmailTemplateDetail(emailTemplateId));
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

                    emailTemplateId = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out emailTemplateId))
                        return;

                    TblEmailTemplate tblEmailTemplate = EmailTemplateManager.GetEmailTemplateById(emailTemplateId);
                    if (tblEmailTemplate == null)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }

                    ConfirmResult result = new ConfirmResult();
                    result.Value = tblEmailTemplate;
                    result.CommandName = "EMAIL_TEMPLATE_DELETE";
                    this.CURRENT_PAGE.CurrentConfirmResult = result;
                    MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION)
                        , string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THE_DATA), "email template")
                        , MSGButton.DeleteCancel, MSGIcon.Error);
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
            new ControlHelpers().ClearControlValues(pnlSearchPopup.Controls);
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
                ShowSystemError();
                throw new Exception("EmailTemplates", exc);
            }
        }
        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null)
            {
                if (e.Submit && e.CommandName != null)
                {
                    if (e.CommandName.Equals("EMAIL_TEMPLATE_DELETE"))
                    {
                        if (!this.CURRENT_PAGE.IsDelete)
                        {
                            ShowAccessDeniedNotify();
                            return;
                        }

                        TblEmailTemplate tblEmailTemplate = e.Value as TblEmailTemplate;
                        if (tblEmailTemplate == null)
                        {
                            ShowInvalidNotFoundData();
                            return;
                        }

                        try
                        {
                            tblEmailTemplate.IsDeleted = true;
                            tblEmailTemplate.Save();
                            ShowSuccessDeleteData();
                            grvData.CurrentPageIndex = 1;
                            grvData.Rebind();
                        }
                        catch (Exception exc)
                        {
                            ShowSystemError();
                            throw new Exception("EmailTemplates", exc);
                        }
                    }
                    else if (e.CommandName.Equals("EMAIL_TEMPLATE_DELETE_MULTIPLE"))
                    {
                        if (!this.CURRENT_PAGE.IsDelete)
                        {
                            ShowAccessDeniedNotify();
                            return;
                        }

                        grvData.HandleGetSelectedColumns(grvData.UniqueID, Request.Params);
                        List<GridviewExtension.DataTable> lstDataSelected = grvData.SelectedColumns;
                        if (lstDataSelected == null || lstDataSelected.Count == 0)
                        {
                            ShowInvalidNotFoundData();
                            return;
                        }

                        foreach (var item in lstDataSelected)
                        {
                            if (item.Id == Guid.Empty)
                                continue;

                            TblEmailTemplate tblEmailTemplate = EmailTemplateManager.GetEmailTemplateById(item.Id);
                            if (tblEmailTemplate == null)
                                continue;
                            tblEmailTemplate.IsDeleted = true;
                            tblEmailTemplate.Save();
                            ShowSuccessDeleteData();
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
        protected void lbtDeleteMultiple_Click(object sender, EventArgs e)
        {
            try
            {
                if (!this.CURRENT_PAGE.IsDelete)
                {
                    ShowAccessDeniedNotify();
                    return;
                }

                grvData.HandleGetSelectedColumns(grvData.UniqueID, Request.Params);
                List<GridviewExtension.DataTable> lstDataSelected = grvData.SelectedColumns;
                if (lstDataSelected == null || lstDataSelected.Count == 0)
                {
                    ShowInvalidNotFoundData();
                    return;
                }

                StringBuilder str = new StringBuilder();
                foreach (var item in lstDataSelected)
                {
                    str.AppendFormat(GridviewExtension.TEMPLATE_DELETE_ITEM, GetResourceText(BackEndResourceKeys.TEMPLATE_NAME), item.Name, item.Id);
                }
                string templateDelete = string.Format(GridviewExtension.TEMPLATE_WRAPPER_DELETE_MULTIPLE
                    , GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THESE_DATA), str);
                ConfirmResult result = new ConfirmResult();
                result.CommandName = "EMAIL_TEMPLATE_DELETE_MULTIPLE";
                //result.Value = lstDataSelected;
                this.CURRENT_PAGE.CurrentConfirmResult = result;
                MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION)
                    , templateDelete, MSGButton.AcceptCancel, MSGIcon.Error);
                OpenMessageBox(msg, result, false, false);
            }
            catch (Exception ex)
            {
                ShowSystemError();
                throw new Exception("Delete multiple", ex);
            }
        }
        #endregion
    }
}