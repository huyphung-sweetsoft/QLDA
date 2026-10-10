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
    public partial class CtrlProjectTemplateDetail : BaseAdminUserControl
    {
        public EventHandler AddPhaseTemplateHandlerCallback;
        public EventHandler AddSubTaskTemplateHandlerCallback;
        public EventHandler EditTaskTemplateHandlerCallback; // MỚI: Bắt sự kiện Edit

        public Guid TemplateId
        {
            get => ViewState["TemplateId"] != null ? (Guid)ViewState["TemplateId"] : Guid.Empty;
            set => ViewState["TemplateId"] = value;
        }

        protected bool IsView => this.CURRENT_PAGE.IsView;
        protected bool IsEdit => this.CURRENT_PAGE.IsEdit;
        protected bool IsAdd => this.CURRENT_PAGE.IsAdd;
        protected bool IsDelete => this.CURRENT_PAGE.IsDelete;

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButton();
        }

        private void RegisterAsyncButton()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            if (script != null)
                script.RegisterAsyncPostBackControl(lbtSearchSingle);
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

                if (TemplateId == Guid.Empty)
                {
                    grvData.DataSource = null;
                    grvData.DataBind();
                    ctrlGridviewPaging.Visible = false;
                    return;
                }

                int pageIndex = grid.CurrentPageIndex <= 0 ? 1 : grid.CurrentPageIndex;
                int pageSize = grid.CurrentPageSize > 0 ? grid.CurrentPageSize : Convert.ToInt32(SweetContext.Current.CurrentPageSize);
                string keyword = txtSearchSingle.Text.Trim();

                SqlQuery query = new Select()
                    .From(TblChiTietMau.Schema)
                    .Where(TblChiTietMau.Columns.IdMau).IsEqualTo(TemplateId);

                if (!string.IsNullOrEmpty(keyword))
                {
                    query.AndExpression(TblChiTietMau.Columns.MaCongViec).Like($"%{keyword}%")
                         .Or(TblChiTietMau.Columns.TenCongViec).Like($"%{keyword}%");
                }

                query.OrderAsc(TblChiTietMau.Columns.MaCongViec);
                query.Paged(pageIndex, pageSize);

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

                    ctrlGridviewPaging.PageIndex = pageIndex;
                    ctrlGridviewPaging.PageSize = pageSize;
                    ctrlGridviewPaging.TotalItems = totalRows;
                    ctrlGridviewPaging.InitLoad();
                }

                upMain.Update();
            }
            catch (Exception ex)
            {
                ShowNotify("Lỗi tải danh sách công việc mẫu: " + ex.Message, MSGType.Error);
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
            AddPhaseTemplateHandlerCallback?.Invoke(Guid.Empty, EventArgs.Empty);
        }

        protected void grvData_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "ITEM_DETAIL":
                    if (!this.IsView && !this.IsEdit) { ShowAccessDeniedNotify(); return; }
                    ShowNotify("Màn hình chi tiết công việc chưa được triển khai.", MSGType.Info);
                    break;
                case "ITEM_ADD_CHILD":
                    if (!this.IsAdd) { ShowAccessDeniedNotify(); return; }
                    Guid parentId = Guid.Parse(e.CommandArgument.ToString());
                    AddSubTaskTemplateHandlerCallback?.Invoke(parentId, EventArgs.Empty);
                    break;
                case "ITEM_EDIT":
                    if (!this.IsEdit) { ShowAccessDeniedNotify(); return; }
                    Guid editId = Guid.Parse(e.CommandArgument.ToString());
                    EditTaskTemplateHandlerCallback?.Invoke(editId, EventArgs.Empty);
                    break;
                case "ITEM_DELETE":
                    if (!this.IsDelete) { ShowAccessDeniedNotify(); return; }
                    ShowNotify("Xóa công việc mẫu chưa được triển khai.", MSGType.Warning);
                    break;
            }
        }

        protected string GetTaskIndent(object value)
        {
            string code = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(code)) return "0";
            int level = code.Split('.').Length;
            return Math.Max(0, (level - 1) * 25).ToString();
        }

        protected string GetTaskBranch(object value)
        {
            string code = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(code)) return string.Empty;
            int level = code.Split('.').Length;
            return level <= 1 ? string.Empty : "└─ ";
        }

        protected string GetTaskNameCssClass(object value)
        {
            string code = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(code) || !code.Contains("."))
                return "template-task-name-link phase-title";
            return "template-task-name-link subtask-title";
        }

        protected string GetTaskPrefixCssClass(object value)
        {
            string code = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(code) || !code.Contains("."))
                return "task-code-prefix phase-prefix";
            return "task-code-prefix subtask-prefix";
        }

        protected string GetPhuThuocCode(object idPhuThuocObj)
        {
            if (idPhuThuocObj == null || idPhuThuocObj == DBNull.Value) return string.Empty;
            if (Guid.TryParse(idPhuThuocObj.ToString(), out Guid idPhuThuoc))
            {
                TblChiTietMau pt = TblChiTietMau.FetchByID(idPhuThuoc);
                if (pt != null)
                {
                    return pt.MaCongViec;
                }
            }
            return string.Empty;
        }

        protected string GetPhuThuocName(object idPhuThuocObj)
        {
            if (idPhuThuocObj == null || idPhuThuocObj == DBNull.Value) return string.Empty;
            if (Guid.TryParse(idPhuThuocObj.ToString(), out Guid idPhuThuoc))
            {
                TblChiTietMau pt = TblChiTietMau.FetchByID(idPhuThuoc);
                if (pt != null)
                {
                    return pt.TenCongViec;
                }
            }
            return string.Empty;
        }
    }
}