using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fProjectTemplate.Controls
{
    public partial class CtrlProjectTemplate : BaseAdminUserControl
    {
        public EventHandler NewTemplateHandlerCallback;

        protected bool IsView => this.CURRENT_PAGE.IsView;
        protected bool IsEdit => this.CURRENT_PAGE.IsEdit;
        protected bool IsDelete => this.CURRENT_PAGE.IsDelete;
        protected bool IsAdd => this.CURRENT_PAGE.IsAdd;

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButton();
        }

        private void RegisterAsyncButton()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            if (script != null)
            {
                script.RegisterAsyncPostBackControl(lbtSearchSingle);
            }
        }

        public void InitControls()
        {
            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
            lbtAdd.Visible = this.IsAdd;

            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            Rebind();
            pnlButtons.Update();
        }

        public void Rebind()
        {
            grvData.CurrentPageIndex = 1;
            grvData.Rebind();
        }

        protected void grvData_NeedDataSource(object sender, ExtraGridEventArg e)
        {
            try
            {
                GridviewExtension grid = sender as GridviewExtension;
                if (grid == null) return;

                string keyword = txtSearchSingle.Text.Trim();

                SqlQuery query = new Select()
                    .From(TblMauDuAn.Schema)
                    .Where(TblMauDuAn.Columns.DaXoa).IsEqualTo(false);

                if (!string.IsNullOrEmpty(keyword))
                {
                    query.AndExpression(TblMauDuAn.Columns.TenMau).Like($"%{keyword}%")
                         .Or(TblMauDuAn.Columns.MoTa).Like($"%{keyword}%");
                }

                query.OrderDesc(TblMauDuAn.Columns.NgayTao);
                query.Paged(grid.CurrentPageIndex, grid.CurrentPageSize);

                DataTable dt = query.ExecuteDataSet().Tables[0];
                int totalRows = query.GetRecordCount();

                if (dt == null || dt.Rows.Count == 0)
                {
                    grvData.DataSource = null;
                    grvData.DataBind();
                    ctrlGridviewPaging.Visible = false;
                }
                else
                {
                    ctrlGridviewPaging.Visible = true;
                    grvData.VirtualItemCount = totalRows;
                    grvData.DataSource = dt;
                    grvData.DataBind();

                    ctrlGridviewPaging.PageIndex = grid.CurrentPageIndex;
                    ctrlGridviewPaging.PageSize = grid.CurrentPageSize;
                    ctrlGridviewPaging.TotalItems = totalRows;
                    ctrlGridviewPaging.InitLoad();
                }

                upMain.Update();
            }
            catch (Exception ex)
            {
                ShowNotify("Lỗi tải dữ liệu: " + ex.Message, MSGType.Error);
            }
        }

        protected void btnSearch_ServerClick(object sender, EventArgs e)
        {
            Rebind();
        }

        protected void ctrlGridviewPaging_PageChanged(object sender, GridviewCustomPageChangeArgs e)
        {
            grvData.CurrentPageSize = e.CurrentPageSize;
            grvData.CurrentPageIndex = e.CurrentPageNumber;
            grvData.Rebind();
        }

        protected void lbtAdd_Click(object sender, EventArgs e)
        {
            if (!this.IsAdd)
            {
                ShowAccessDeniedNotify();
                return;
            }

            NewTemplateHandlerCallback?.Invoke(Guid.Empty, EventArgs.Empty);
        }

        protected void grvData_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ITEM_EDIT" || e.CommandName == "ITEM_DETAIL")
            {
                if (!this.IsEdit && !this.IsView)
                {
                    ShowAccessDeniedNotify();
                    return;
                }

                Guid idMau = Guid.Parse(e.CommandArgument.ToString());
                Response.Redirect(RewriteURLHelper.ProjectTemplateDetail(idMau), false);
            }
            else if (e.CommandName == "ITEM_DELETE")
            {
                if (!this.IsDelete)
                {
                    ShowAccessDeniedNotify();
                    return;
                }

                Guid idMau = Guid.Parse(e.CommandArgument.ToString());
                TblMauDuAn mauDel = TblMauDuAn.FetchByID(idMau);

                if (mauDel == null || mauDel.DaXoa == true)
                {
                    ShowInvalidNotFoundData();
                    return;
                }

                ConfirmResult result = new ConfirmResult { CommandName = "ITEM_DELETE", Value = mauDel };
                this.CURRENT_PAGE.CurrentConfirmResult = result;

                MessageBox msg = new MessageBox("Xóa mẫu dự án", $"Bạn có chắc chắn muốn xóa mẫu: {mauDel.TenMau}?", MSGButton.DeleteCancel, MSGIcon.Error);
                OpenMessageBox(msg, result, false, false);
            }
        }

        public void ConfirmRequest(ConfirmResult e)
        {
            if (e != null && e.Submit && e.CommandName != null && e.CommandName.Contains("TEMPLATE_DELETE"))
            {
                TblMauDuAn mau = e.Value as TblMauDuAn;
                if (mau == null)
                {
                    ShowInvalidNotFoundData();
                    return;
                }

                try
                {
                    mau.DaXoa = true;
                    mau.Save();

                    ShowSuccessDeleteData();
                    Rebind();
                }
                catch (Exception exc)
                {
                    ShowNotify(exc.Message, MSGType.Error);
                }
            }
        }
    }
}
