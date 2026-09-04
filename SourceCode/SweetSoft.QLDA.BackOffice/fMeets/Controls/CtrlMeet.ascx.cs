using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
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

namespace SweetSoft.QLDA.BackOffice.fMeets.Controls
{
    public partial class CtrlMeet : BaseAdminUserControl
    {
        public EventHandler NewMeetingHandlerCallback;
        public EventHandler EditMeetingHandlerCallback;

        public Guid ProjectId
        {
            get
            {
                if (ViewState["ProjectId"] == null)
                {
                    if (this.Page is BaseAdminPage basePage && basePage.CurrentProjectId != Guid.Empty)
                        return basePage.CurrentProjectId;
                    if (Request.QueryString["ProjectId"] != null && Guid.TryParse(Request.QueryString["ProjectId"], out Guid qId))
                        return qId;
                    return Guid.Empty;
                }
                return (Guid)ViewState["ProjectId"];
            }
            set => ViewState["ProjectId"] = value;
        }

        protected bool IsView => this.CURRENT_PAGE.IsView;
        protected bool IsEdit => this.CURRENT_PAGE.IsEdit;
        protected bool IsDelete => this.CURRENT_PAGE.IsDelete;

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
                script.RegisterAsyncPostBackControl(lbtSearchAdvanced);
                script.RegisterAsyncPostBackControl(lbtCancel);
            }
        }

        public void InitControls()
        {
            ApplyControlsText();
            AssignSearchColumns();
            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
            lbtAdd.Visible = this.CURRENT_PAGE.IsAdd;

            MasterTemplate master = Page.Master as MasterTemplate;
            if (master != null)
                master.LoadSessionLastSearch(searchTagBox, pnlSearchPopup, grvData, txtSearchSingle);

            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = "TenCuocHop";
            grvData.CurrentSortDerection = "ASC";
            grvData.Rebind();
            pnlSearch.Update();
            pnlButtons.Update();
        }

        private void AssignSearchColumns()
        {
            txtSearchTenCuocHop.SearchColumn = "TenCuocHop";
        }

        public void Rebind()
        {
            grvData.CurrentPageIndex = 1;
            grvData.Rebind();
        }

        private void ApplyControlsText()
        {
            txtSearchSingle.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            txtSearchTenCuocHop.SearchTagItemText = "Tên cuộc họp";
            lbtAdd.ToolTip = lbtAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);

            // Cập nhật lại số lượng và tên cột
            List<string> lstTableHeader = new List<string>
            {
                "Mã họp", "Tên cuộc họp", "Bắt đầu", "Kết thúc", "Địa điểm", "Trạng thái", "Thao tác"
            };
            grvData.HeaderTexts = lstTableHeader;
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
                DataTable dt = null;

                /* Bác viết lại hàm truy vấn Manager gọi vào đây để trả dữ liệu (nhớ bỏ Chủ Trì ra khỏi list nhé)
                if (grid.GridSearchType == GridSearchType.Single)
                {
                    dt = MeetingManager.Instance.SearchMeeting(this.ProjectId, txtSearchSingle.Text, null, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                }
                else
                {
                    Dictionary<string, object> keyValueSearchs = new ControlHelpers().GetControlValues(pnlSearchPopup);
                    dt = MeetingManager.Instance.SearchMeeting(this.ProjectId, "", keyValueSearchs, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                }
                */

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
                    if (!this.CURRENT_PAGE.IsEdit) { ShowAccessDeniedNotify(); return; }
                    int rowIndex = (e.CommandSource.GetType() != typeof(GridviewExtension)) ? ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex : Convert.ToInt32(e.CommandArgument);
                    Guid meetId = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out meetId)) { ShowInvalidDataError(); return; }
                    EditMeetingHandlerCallback?.Invoke(meetId, EventArgs.Empty);
                    break;

                case "ITEM_DELETE":
                    if (!this.CURRENT_PAGE.IsDelete) { ShowAccessDeniedNotify(); return; }
                    int rowIndexDel = (e.CommandSource.GetType() != typeof(GridviewExtension)) ? ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex : Convert.ToInt32(e.CommandArgument);
                    Guid meetIdDel = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndexDel].Value.ToString(), out meetIdDel)) { ShowInvalidDataError(); return; }

                    TblLichHop meetDel = TblLichHop.FetchByID(meetIdDel);
                    if (meetDel == null || meetDel.DaXoa == true) { ShowInvalidNotFoundData(); return; }

                    ConfirmResult result = new ConfirmResult { CommandName = "MEETING_DELETE", Value = meetDel };
                    this.CURRENT_PAGE.CurrentConfirmResult = result;
                    MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION), $"Bạn có chắc chắn xóa cuộc họp: {meetDel.TenCuocHop}?", MSGButton.DeleteCancel, MSGIcon.Error);
                    OpenMessageBox(msg, result, false, false);
                    break;
            }
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null && e.Submit && e.CommandName != null && e.CommandName.Contains("MEETING_DELETE"))
            {
                TblLichHop meet = e.Value as TblLichHop;
                if (meet == null) { ShowInvalidNotFoundData(); return; }

                try
                {
                    // Đã thay đổi cột cập nhật thành IdNguoiCapNhat kiểu Guid 
                    string sql = $"UPDATE TblLichHop SET DaXoa = 1, IdNguoiCapNhat = '{SweetContext.Current.UserId}', NgayCapNhat = GETDATE() WHERE IdLichHop = '{meet.IdLichHop}'";
                    using (var reader = new InlineQuery().ExecuteReader(sql)) { }

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

        protected void ctrlGridviewPaging_PageChanged(object sender, GridviewCustomPageChangeArgs e)
        {
            grvData.CurrentPageSize = e.CurrentPageSize;
            grvData.CurrentPageIndex = e.CurrentPageNumber;
            grvData.Rebind();
        }

        protected void btnSearch_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            master?.btnSearchSingle_Click(searchTagBox, grvData, txtSearchSingle);
            upSearchTagBox.Update();
        }

        protected void searchTagBox_TagClosed(object sender, SearchTagItem tag)
        {
            try
            {
                MasterTemplate master = Page.Master as MasterTemplate;
                if (master != null)
                {
                    GridSearchType? searchType;
                    master.searchTagBox_TagClosed(searchTagBox, tag, null, pnlSearchPopup, grvData, txtSearchSingle, out searchType);
                }
                upSearchTagBox.Update();
                string script = string.Format("$('#{0}').val('');", txtSearchSingle.ClientID);
                ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "UpdateTxtSearch", script, true);
            }
            catch (Exception exc) { ShowNotify(exc.Message, MSGType.Error); }
        }

        protected void btnSearchAdvanced_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            master?.btnSearchAdvanced_Click(searchTagBox, null, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            if (pnlSearchPopup != null) new ControlHelpers().ClearControlValues(pnlSearchPopup.Controls);
            pnlSearch.Update();
            MasterTemplate master = Page.Master as MasterTemplate;
            master?.btnSearchAdvanced_Click(searchTagBox, null, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }

        protected void lbtAdd_Click(object sender, EventArgs e)
        {
            if (!this.CURRENT_PAGE.IsAdd) { ShowAccessDeniedNotify(); return; }
            NewMeetingHandlerCallback?.Invoke(Guid.Empty, EventArgs.Empty);
        }
    }
}