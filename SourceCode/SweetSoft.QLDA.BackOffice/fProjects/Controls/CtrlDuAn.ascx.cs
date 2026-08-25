using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fProjects.Controls
{
    public partial class CtrlDuAn : BaseAdminUserControl
    {
        public EventHandler NewProjectHandlerCallBack;
        public EventHandler EditProjectHandlerCallBack;

        protected bool IsEdit
        {
            get
            {
                if (this.CURRENT_PAGE.IsUserRight(ActionKeys.Update, ModuleKeys.Project))
                    return true;
                return false;
            }
        }

        protected bool IsView
        {
            get
            {
                return this.CURRENT_PAGE.IsView; 
            }
        }

        protected bool IsDelete
        {
            get
            {
                if (this.CURRENT_PAGE.IsUserRight(ActionKeys.Delete, ModuleKeys.Project))
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
            script.RegisterPostBackControl(btnExport);
        }

        private void ApplyControlsText()
        {
            txtSearchSingle.SearchTagItemKey = GetResourceText(BackEndResourceKeys.KEYWORD);
            //-------------------------------------------------
            lbtAdd.ToolTip = lbtAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);
            btnExport.ToolTip = btnExport.Text = GetResourceText(BackEndResourceKeys.EXPORT_EXCEL);
            //-------------------------------------------------
            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.PROJECT_CODE),
                GetResourceText(BackEndResourceKeys.PROJECT_NAME),
                GetResourceText(BackEndResourceKeys.CUSTOMER),
                GetResourceText(BackEndResourceKeys.PROJECT_MANAGER)
            };
            grvData.HeaderTexts = lstTableHeader;
        }

        public void InitControls()
        {
            ApplyControlsText();

            lbtAdd.Visible = this.CURRENT_PAGE.IsAdd;
            
            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = TblDuAn.Columns.MaDuAn;
            grvData.CurrentSortDerection = "ASC";

            tagOther.Visible = true;
            grvData.Rebind();
            pnlButtons.Update();
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
                dt = DuAnManager.Instance.SearchDuAns(string.Empty, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);

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
                        ctrlGridviewPaging.Visible = btnExport.Visible = true;
                    grvData.VirtualItemCount = totalRows;
                    grvData.DataSource = dt;
                    grvData.DataBind();
                    ctrlGridviewPaging.PageIndex = grvData.CurrentPageIndex;
                    ctrlGridviewPaging.PageSize = grvData.CurrentPageSize;
                    ctrlGridviewPaging.TotalItems = totalRows;
                    ctrlGridviewPaging.InitLoad();
                }
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
                case "ITEM_EDIT":
                    if (!this.CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    //------------------------------------------
                    int rowIndex = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndex = ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);
                    Guid idDuAn = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out idDuAn))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    if (EditProjectHandlerCallBack != null)
                    {
                        EditProjectHandlerCallBack(idDuAn, EventArgs.Empty);
                    }
                    break;
                case "ITEM_DETAIL":
                    if (!this.CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify(); 
                        return;
                    }
                    //------------------------------------------
                    rowIndex = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndex = ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out idDuAn))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    Response.Redirect(RewriteURLHelper.ProjectDetail(idDuAn));
                    Context.ApplicationInstance.CompleteRequest();
                    break;
                case "ITEM_DELETE":
                    if (!this.CURRENT_PAGE.IsDelete)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    rowIndex = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndex = ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out idDuAn))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    TblDuAn duAn = DuAnManager.Instance.GetDuAnById(idDuAn);
                    if (duAn == null)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }

                    ConfirmResult result = new ConfirmResult();
                    result.CommandName = "PROJECT_DELETE";
                    result.Value = duAn;
                    this.CURRENT_PAGE.CurrentConfirmResult = result;
                    MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION)
                        , string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THE_DATA), duAn.TenDuAn)
                        , MSGButton.DeleteCancel, MSGIcon.Error);
                    OpenMessageBox(msg, result, false, false);
                    break;
            }
        }


        protected void btnSearch_ServerClick(object sender, EventArgs e)
        {
            // MasterTemplate master = Page.Master as MasterTemplate;
            // master.btnSearchSingle_Click(searchTagBox, grvData, txtSearchSingle);
            // upSearchTagBox.Update();

            // Temporary rebind until search logic is fully implemented
            Rebind();
        }

        protected void ctrlGridviewPaging_PageChanged(object sender, GridviewCustomPageChangeArgs e)
        {
            grvData.CurrentPageSize = e.CurrentPageSize;
            grvData.CurrentPageIndex = e.CurrentPageNumber;
            grvData.Rebind();
        }

        public void Rebind()
        {
            grvData.CurrentPageIndex = 1;
            grvData.Rebind();
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            //if (!this.CURRENT_PAGE.IsExportExcel)
            //{
            //    ShowAccessDeniedNotify();
            //    return;
            //}

            //int totalRows = 0;
            //int rowIndex = (grvData.CurrentPageIndex - 1) * grvData.CurrentPageSize;
            //int pageSize = rowIndex + grvData.CurrentPageSize;
            ////----------------------------------------------
            //DataTable dt = null;
            //if (grvData.GridSearchType == GridSearchType.Single)
            //{

            //}
        }

        protected void lbtAdd_Click(object sender, EventArgs e)
        {
            if (!this.CURRENT_PAGE.IsAdd)
            {
                ShowAccessDeniedNotify();
                return;
            }
            if (NewProjectHandlerCallBack != null)
                NewProjectHandlerCallBack(Guid.Empty, EventArgs.Empty);
        }


        protected void searchTagBox_TagClosed(object sender, SearchTagItem tag)
        {
            //try
            //{
            //    MasterTemplate master = Page.Master as MasterTemplate;
            //    GridSearchType? searchType;
            //    master.searchTagBox_TagClosed(searchTagBox, tag, pnlSearchPopup, grvData, txtSearchSingle, out searchType);
            //    pnlSearch.Update();
            //    string script = string.Format("$('#{0}').val('');", txtSearchSingle.ClientID);
            //    ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "UpdateTxtSearch", script, true);
            //}
            //catch (Exception exc)
            //{
            //    ShowNotify(exc.Message, MSGType.Error);
            //}
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null)
            {
                if (e.Submit && e.CommandName != null)
                {
                    if (e.CommandName.Contains("PROJECT_DELETE"))
                    {
                        TblDuAn duAn = e.Value as TblDuAn;
                        if (duAn == null)
                        {
                            ShowInvalidNotFoundData();
                            return;
                        }
                        try
                        {
                            DuAnManager.Instance.Delete(duAn);
                            ShowSuccessDeleteData();
                            grvData.CurrentPageIndex = 1;
                            grvData.Rebind();
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
    }
}