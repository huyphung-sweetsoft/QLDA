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
            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.PROJECT_IDENTIFIED),
                GetResourceText(BackEndResourceKeys.PROJECT_NAME),
                GetResourceText(BackEndResourceKeys.CUSTOMER_NAME),
                GetResourceText(BackEndResourceKeys.PROJECT_MANAGER)
            };
            grvData.HeaderTexts = lstTableHeader;
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {

        }

        protected void lbtAdd_Click(object sender, EventArgs e)
        {

        }

        public void InitControls()
        {
            ApplyControlsText();
            grvData.CurrentPageIndex = 1;
            grvData.CurrentPageSize = 10;
            grvData.CurrentSortExpression = "MaDuAn";
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
    }
}