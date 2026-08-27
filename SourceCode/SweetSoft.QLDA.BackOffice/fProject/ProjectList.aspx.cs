using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fUsers
{
    public partial class ProjectList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get { return ModuleKeys.Project; }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButton();
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.PROJECT_LIST));
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.PROJECT_LIST);
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    { "javascript:;", GetResourceText(BackEndResourceKeys.PROJECT_LIST) }
                };
                txtSearchSingle.EnterSubmitClientID=lbtSearchSingle.ClientID;
                InitGridData();
            }
        }
        private void RegisterAsyncButton()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            if(script != null)
            {
                script.RegisterAsyncPostBackControl(lbtSearchSingle);
            }
        }
        protected void btnSearch_ServerClick(object sender, EventArgs e)
        {
            grvData.CurrentPageIndex = 1; 
            grvData.Rebind();           
        }

        private void InitGridData()
        {
            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = TblDuAn.Columns.MaDuAn;
            grvData.CurrentSortDerection = "ASC";
            grvData.Rebind();
        }

        protected void grvData_NeedDataSource(object sender, ExtraGridEventArg e)
        {
            try
            {
                GridviewExtension grid = sender as GridviewExtension;
                if (grid == null) return;

                int pageIndex = grid.CurrentPageIndex > 0 ? grid.CurrentPageIndex : 1;

                Select select = new Select();
                select.From(TblDuAn.Schema);
                select.Where(TblDuAn.DaXoaColumn).IsEqualTo(false);
                string keyword = txtSearchSingle.Text.Trim();
                if (!string.IsNullOrEmpty(keyword))
                {
                    select.AndExpression(TblDuAn.MaDuAnColumn.ColumnName).Like("%" + keyword + "%")
                          .Or(TblDuAn.TenDuAnColumn.ColumnName).Like("%" + keyword + "%");
                }
                int totalRows = select.GetRecordCount();
                grid.VirtualItemCount = totalRows;

                select.Paged(pageIndex, grid.CurrentPageSize);
                select.OrderAsc(TblDuAn.MaDuAnColumn.ColumnName);

                var listData = select.ExecuteTypedList<TblDuAn>();
                grid.DataSource = listData;

                ctrlGridviewPaging.PageIndex = grid.CurrentPageIndex;
                ctrlGridviewPaging.PageSize = grid.CurrentPageSize;
                ctrlGridviewPaging.TotalItems = totalRows;
                ctrlGridviewPaging.InitLoad();

                upMain.Update();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }
        protected void ctrlGridviewPaging_PageChanged(object sender, EventArgs e)
        {
            grvData.CurrentPageIndex = ctrlGridviewPaging.PageIndex;
            grvData.Rebind();
        }

        protected void grvData_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "ITEM_DETAIL":
                    if (!this.IsView)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }

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
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.ProjectTasks(idDuAn)));
                    break;

                case "ITEM_DELETE":
                    if (!this.IsDelete)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }

                    rowIndex = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndex = ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);

                    idDuAn = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out idDuAn))
                        return;

                    TblDuAn duAn = TblDuAn.FetchByID(idDuAn);
                    if (duAn == null)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }

                    duAn.DaXoa = true;
                    duAn.Save();

                    ShowNotify("Xóa dự án thành công!", MSGType.Success);
                    grvData.Rebind();
                    break;
            }
        }
    }
}