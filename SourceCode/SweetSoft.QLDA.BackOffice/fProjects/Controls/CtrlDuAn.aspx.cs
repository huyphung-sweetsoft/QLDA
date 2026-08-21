using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fProjects.Controls
{
    public partial class CtrlDuAn : BaseAdminUserControl
    {
        public EventHandler NewProjectHandleCallBack;
        public EventHandler EditProjectHangleCallBack;

        protected bool IsView
        {
            get { return this.CURRENT_PAGE.IsView; }
        }

        protected bool IsEdit
        {
            get
            {
                if (this.CURRENT_PAGE.IsUserRight(ActionKeys.Update, ModuleKeys.Project))
                    return true;
                return false;
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
            txtSearchSingle.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            ddlSearchStatus.SearchTagItemText = GetResourceText(BackEndResourceKeys.STATUS);
            ddlSearchProjectType.SearchTagItemText = GetResourceText(BackEndResourceKeys.PROJECT_TYPE);
            lbtAdd.ToolTip = lbtAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);
            btnExport.ToolTip = btnExport.Text = GetResourceText(BackEndResourceKeys.EXPORT_EXCEL);
            txtSearchSingle.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.PROJECT_NAME),
                GetResourceText(BackEndResourceKeys.CUSTOMER),
                GetResourceText(BackEndResourceKeys.PROJECT_MANAGER),
                GetResourceText(BackEndResourceKeys.STATUS),
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
            controlHelpers.BindDuAnStatus(ddlSearchStatus);
            controlHelpers.BindLoaiDuAn(ddlSearchProjectType);
            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
            lbtAdd.Visible = this.CURRENT_PAGE.IsAdd;
            tagOther.Visible = true;
            MasterTemplate master = Page.Master as MasterTemplate;
            master.LoadSessionLastSearch(searchTagBox, pnlSearchDefault, grvData, txtSearchSingle);
            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = "MaDuAn";
            grvData.CurrentSortDerection = "ASC";
            grvData.Rebind();
            pnlButtons.Update();
        }

        private void AssignSearchColumns()
        {
            ddlSearchStatus.SearchColumn = "TrangThai";
            ddlSearchProjectType.SearchColumn = "MaLoaiDuAn";
        }
        #endregion

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
                Dictionary<string, object> keyValueSearchs = new ControlHelpers().GetControlValues(pnlSearchDefault);
                DataTable dt = DuAnManager.Instance.SearchDuAn(txtSearchSingle.Text, keyValueSearchs,
                    $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);

                if (dt == null || dt.Rows.Count == 0)
                {
                    grvData.DataSource = null;
                    grvData.DataBind();
                    ctrlGridviewPaging.Visible = btnExport.Visible = false;
                }
                else
                {
                    ctrlGridviewPaging.Visible = true;
                    btnExport.Visible = this.CURRENT_PAGE.IsExportExcel;
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
                case "ITEM_DETAIL":
                    if (!this.CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    int rowIndex = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndex = ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);
                    Guid id = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out id))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    if (EditProjectHangleCallBack != null)
                        EditProjectHangleCallBack(id, EventArgs.Empty);
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
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out id))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    var duAn = DuAnManager.Instance.GetProjectById(id);
                    if (duAn == null)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }
                    ConfirmResult result = new ConfirmResult();
                    result.CommandName = "DUAN_DELETE";
                    result.Value = duAn;
                    this.CURRENT_PAGE.CurrentConfirmResult = result;
                    MessageBox msg = new MessageBox(
                        GetResourceText(BackEndResourceKeys.NOTIFICATION),
                        string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THE_DATA), duAn.TenDuAn),
                        MSGButton.DeleteCancel, MSGIcon.Error);
                    OpenMessageBox(msg, result, false, false);
                    break;
            }
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null && e.Submit && e.CommandName != null)
            {
                if (e.CommandName.Contains("DUAN_DELETE"))
                {
                    var duAn = e.Value as SweetSoft.QLDA.DataAccess.TblDuAn;
                    if (duAn == null) { ShowInvalidNotFoundData(); return; }
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
                master.btnSearchAdvanced_Click(searchTagBox, pnlSearchDefault, grvData);
            upSearchTagBox.Update();
        }

        protected void lbtAdd_Click(object sender, EventArgs e)
        {
            if (!this.CURRENT_PAGE.IsAdd) { ShowAccessDeniedNotify(); return; }
            if (NewProjectHandleCallBack != null)
                NewProjectHandleCallBack(Guid.Empty, EventArgs.Empty);
        }

        protected void btnSearch_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            master.btnSearchSingle_Click(searchTagBox, grvData, txtSearchSingle);
            upSearchTagBox.Update();
        }

        protected void searchTagBox_TagClosed(object sender, SearchTagItem tag)
        {
            try
            {
                MasterTemplate master = Page.Master as MasterTemplate;
                GridSearchType? searchType;
                master.searchTagBox_TagClosed(searchTagBox, tag, pnlSearchDefault, null, grvData, txtSearchSingle, out searchType);
                upnlSearchDefault.Update();
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
            if (!this.CURRENT_PAGE.IsExportExcel) { ShowAccessDeniedNotify(); return; }
            // TODO: Implement export excel for DuAn
            ShowNotify("Chức năng xuất Excel đang được phát triển.", MSGType.Info);
        }
    }
}