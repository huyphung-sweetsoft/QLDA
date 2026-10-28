using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fIssues.Controls
{
    public partial class CtrlIssue : BaseAdminUserControl
    {
        public EventHandler NewIssueHandlerCallback;
        public EventHandler EditIssueHandlerCallback;
        public EventHandler GuiDuyetIssueCallback;
        public EventHandler DuyetIssueCallback;
        public EventHandler TinhCongIssueCallback;
        private readonly ControlHelpers _controlHelpers = new ControlHelpers();
        private bool? _kbg20Enabled;
        protected bool LaKbg20Issue
        {
            get
            {
                if(!_kbg20Enabled.HasValue)_kbg20Enabled=KhaiBaoGioManager.KhaiBaoGio20DaKichHoat();
                return _kbg20Enabled.Value;
            }
        }
        private bool? _coQuyenQuanLyIssue;
        // Cùng nguồn quyền thực tế của GioXuLyVanDeManager (TblDuAn.IdNhanVienQuanLy),
        // không chỉ dựa trên cờ IsPM của trang vốn có thể không phản ánh PM dự án.
        protected bool CoQuyenQuanLyIssue
        {
            get
            {
                if (_coQuyenQuanLyIssue.HasValue) return _coQuyenQuanLyIssue.Value;
                if (this.CURRENT_PAGE.IsAdministrator) return (_coQuyenQuanLyIssue = true).Value;
                Guid viewer = SweetContext.Current == null ? Guid.Empty : SweetContext.Current.UserId;
                if (viewer == Guid.Empty || ProjectId == Guid.Empty) return (_coQuyenQuanLyIssue = false).Value;
                try
                {
                    Guid? pm = DuAnManager.Instance.LayIdNhanVienQuanLy(ProjectId);
                    return (_coQuyenQuanLyIssue = pm.HasValue && pm.Value == viewer).Value;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError("Không xác định được PM Issue: " + ex);
                    return (_coQuyenQuanLyIssue = false).Value;
                }
            }
        }
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
        protected void Page_Load(object sender, EventArgs e) { RegisterAsyncButton(); }
        private void RegisterAsyncButton()
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            if (script != null)
            {
                script.RegisterAsyncPostBackControl(lbtSearchSingle);
                script.RegisterAsyncPostBackControl(ddlSearchMucDoAnhHuong);
                script.RegisterAsyncPostBackControl(ddlSearchTrangThai);
                script.RegisterAsyncPostBackControl(ddlSearchNguonGoc);
            }
        }
        public void InitControls()
        {
            ApplyControlsText();
            AssignSearchColumns();
            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
            lbtAdd.Visible = this.CURRENT_PAGE.IsAdd;
            _controlHelpers.BindMucDoAnhHuong(ddlSearchMucDoAnhHuong);
            _controlHelpers.BindTrangThaiVanDe(ddlSearchTrangThai);
            _controlHelpers.BindNguonGocVanDe(ddlSearchNguonGoc);
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master != null)
                master.LoadSessionLastSearch(searchTagBox, null, grvData, txtSearchSingle);
            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = "MaVanDe";
            grvData.CurrentSortDerection = "ASC";
            grvData.Rebind();
            pnlSearchDropdowns.Update();
            pnlButtons.Update();
        }
        private void AssignSearchColumns() { txtSearchSingle.SearchColumn = "MaVanDe"; }
        public void Rebind() { grvData.CurrentPageIndex = 1; grvData.Rebind(); }
        private void ApplyControlsText()
        {
            txtSearchSingle.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            txtSearchSingle.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            ddlSearchMucDoAnhHuong.SearchTagItemText = GetResourceText(BackEndResourceKeys.IMPACT);
            ddlSearchMucDoAnhHuong.Text = GetResourceText(BackEndResourceKeys.IMPACT);
            ddlSearchTrangThai.SearchTagItemText = GetResourceText(BackEndResourceKeys.STATUS);
            ddlSearchTrangThai.Text = GetResourceText(BackEndResourceKeys.STATUS);
            ddlSearchNguonGoc.SearchTagItemText = GetResourceText(BackEndResourceKeys.ORIGIN);
            ddlSearchNguonGoc.Text = GetResourceText(BackEndResourceKeys.ORIGIN);
            lbtAdd.ToolTip = lbtAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);
            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.ISSUE_NAME),
                GetResourceText(BackEndResourceKeys.IMPACT),
                GetResourceText(BackEndResourceKeys.STATUS),
                GetResourceText(BackEndResourceKeys.ORIGIN),
                GetResourceText(BackEndResourceKeys.CREATED_BY),
                GetResourceText(BackEndResourceKeys.CREATED_DATE),
                GetResourceText(BackEndResourceKeys.ACTION)
            };
            grvData.HeaderTexts = lstTableHeader;
        }
        protected void bootstrapDropdown_SelectedValueChanged(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master != null)
                master.btnSearchSingle_Click(searchTagBox, pnlSearchDefaultStatus, grvData, txtSearchSingle);
            upSearchTagBox.Update();
            if (pnlSearchDropdowns != null) pnlSearchDropdowns.Update();
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
                Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
                if (pnlSearchDefaultStatus != null)
                {
                    var defaultParams = _controlHelpers.GetControlValues(pnlSearchDefaultStatus);
                    foreach (var item in defaultParams) keyValueSearchs[item.Key] = item.Value;
                }
                DataTable dt = IssueManager.Instance.SearchIssue(ProjectId, txtSearchSingle.Text, keyValueSearchs, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
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
            catch (Exception exc) { ShowNotify(exc.Message, MSGType.Error); }
        }
        protected void grvData_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "FAST_COMPLETE_ISSUE":
                case "REVIEW_COMPLETE_ISSUE":
                    {
                        Guid requestedId;
                        if (!Guid.TryParse(Convert.ToString(e.CommandArgument), out requestedId) || requestedId == Guid.Empty || ProjectId == Guid.Empty)
                        { ShowInvalidDataError(); return; }
                        TblVanDe requested = TblVanDe.FetchByID(requestedId);
                        if (requested == null || requested.DaXoa == true || requested.IdDuAn != ProjectId)
                        { ShowInvalidNotFoundData(); return; }
                        if (!LaKbg20Issue) { ShowNotify("Tính giờ vấn đề chưa được kích hoạt.", MSGType.Warning); return; }
                        if (e.CommandName == "REVIEW_COMPLETE_ISSUE")
                        {
                            if (!CoQuyenQuanLyIssue || requested.TrangThai != 0 || requested.TrangThaiDuyetGio != 1)
                            { ShowNotify("Vấn đề không còn chờ duyệt hoặc bạn không có quyền PM.", MSGType.Warning); Rebind(); return; }
                            if (DuyetIssueCallback != null) DuyetIssueCallback(requestedId, EventArgs.Empty);
                        }
                        else
                        {
                            if (requested.TrangThai != 0 || (requested.TrangThaiDuyetGio != 0 && requested.TrangThaiDuyetGio != 3))
                            { ShowNotify("Vấn đề không còn trong trạng thái được gửi duyệt.", MSGType.Warning); Rebind(); return; }
                            // CtrlFastCompleteIssue sẽ đọc lại người được giao/giờ đã báo tại server.
                            if (GuiDuyetIssueCallback != null) GuiDuyetIssueCallback(requestedId, EventArgs.Empty);
                        }
                        return;
                    }
                case "ISSUE_REPORT":
                    {
                        Guid reportIssueId;
                        if (!Guid.TryParse(Convert.ToString(e.CommandArgument), out reportIssueId) || reportIssueId == Guid.Empty || ProjectId == Guid.Empty)
                        { ShowInvalidDataError(); return; }
                        TblVanDe issue = TblVanDe.FetchByID(reportIssueId);
                        if (issue == null || issue.IdDuAn != ProjectId || issue.DaXoa == true)
                        { ShowInvalidNotFoundData(); return; }
                        string target=RewriteURLHelper.ProjectDetail(ProjectId).TrimEnd('/')
                            + "/TimeTracking?vanDeId=" + reportIssueId.ToString("D") + "&gvAction=report";
                        ScriptManager.RegisterStartupScript(this,this.GetType(),"IssueReport"+reportIssueId.ToString("N"),
                            "window.location.href='"+HttpUtility.JavaScriptStringEncode(target)+"';",true);
                        return;
                    }
                case "ISSUE_SEND_COMPLETE":
                case "ISSUE_REVIEW_COMPLETE":
                case "ISSUE_PM_HOURS":
                    {
                        Guid selected;
                        if (ProjectId == Guid.Empty || !Guid.TryParse(Convert.ToString(e.CommandArgument), out selected) || selected == Guid.Empty)
                        { ShowInvalidDataError(); return; }
                        TblVanDe candidate = TblVanDe.FetchByID(selected);
                        if (candidate == null || candidate.DaXoa == true || candidate.IdDuAn != ProjectId)
                        { ShowInvalidNotFoundData(); return; }
                        if (!KhaiBaoGioManager.KhaiBaoGio20DaKichHoat())
                        { ShowNotify("Tính giờ Issue chưa được kích hoạt trên dự án.", MSGType.Warning); return; }
                        if ((e.CommandName == "ISSUE_REVIEW_COMPLETE" || e.CommandName == "ISSUE_PM_HOURS") && !CoQuyenQuanLyIssue)
                        { ShowAccessDeniedNotify(); return; }
                        if (e.CommandName == "ISSUE_PM_HOURS" && (candidate.TrangThai != 1 || candidate.TrangThaiDuyetGio != 2))
                        { ShowNotify("Chỉ tính/chốt giờ sau khi vấn đề được PM duyệt hoàn thành.", MSGType.Warning); Rebind(); return; }
                        if (e.CommandName == "ISSUE_SEND_COMPLETE")
                        { if (GuiDuyetIssueCallback != null) GuiDuyetIssueCallback(selected, EventArgs.Empty); }
                        else if (e.CommandName == "ISSUE_REVIEW_COMPLETE")
                        { if (DuyetIssueCallback != null) DuyetIssueCallback(selected, EventArgs.Empty); }
                        else if (TinhCongIssueCallback != null) TinhCongIssueCallback(selected, EventArgs.Empty);
                        return;
                    }
                case "ITEM_COMPLETE_PROCESS":
                    // Khi KBG20 bat: hoan thanh Issue phai qua gui duyet -> PM phe duyet.
                    // Khong cho cap nhat thang TrangThai=1, vi thao tac nay bo qua dong ho nhom.
                    if (KhaiBaoGioManager.KhaiBaoGio20DaKichHoat())
                    {
                        int k = e.CommandSource.GetType() != typeof(GridviewExtension)
                            ? ((GridViewRow)((WebControl)e.CommandSource).NamingContainer).RowIndex
                            : Convert.ToInt32(e.CommandArgument);
                        Guid id;
                        if (!Guid.TryParse(Convert.ToString(grvData.DataKeys[k].Value), out id))
                        { ShowInvalidDataError(); return; }
                        var issue = TblVanDe.FetchByID(id);
                        if (issue == null || issue.IdDuAn != ProjectId || issue.DaXoa == true)
                        { ShowInvalidNotFoundData(); return; }
                        if (issue.TrangThaiDuyetGio == 1)
                        { if (!CoQuyenQuanLyIssue) { ShowAccessDeniedNotify(); return; } if (DuyetIssueCallback != null) DuyetIssueCallback(id, EventArgs.Empty); }
                        else
                        { if (GuiDuyetIssueCallback != null) GuiDuyetIssueCallback(id, EventArgs.Empty); }
                        return;
                    }
                    if (!IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    int rowIndexProcess = e.CommandSource.GetType() != typeof(GridviewExtension) ? ((GridViewRow)((WebControl)e.CommandSource).NamingContainer).RowIndex : Convert.ToInt32(e.CommandArgument);
                    if (Guid.TryParse(grvData.DataKeys[rowIndexProcess].Value.ToString(), out Guid issueIdProcess))
                    {
                        TblVanDe issueProcess = TblVanDe.FetchByID(issueIdProcess);
                        if (issueProcess == null || issueProcess.DaXoa == true)
                        {
                            ShowInvalidNotFoundData();
                            return;
                        }
                        if (issueProcess.TrangThai != 0)
                        {
                            return;
                        }
                        ConfirmResult resultProcess = new ConfirmResult { CommandName = "ISSUE_COMPLETE_PROCESS", Value = issueProcess };
                        this.CURRENT_PAGE.CurrentConfirmResult = resultProcess;
                        MessageBox msgProcess = new MessageBox(
                            GetResourceText(BackEndResourceKeys.NOTIFICATION),
                            string.Format("Bạn có chắc chắn muốn đánh dấu vấn đề <b>{0}</b> là <b>Đã xử lý</b> không?", issueProcess.TenVanDe),
                            MSGButton.AcceptCancel,
                            MSGIcon.Success);
                        OpenMessageBox(msgProcess, resultProcess, false, false);
                    }
                    break;
                case "ITEM_DETAIL":
                    if (!IsEdit && !IsView)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    int rowIndex = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndex = ((GridViewRow)((WebControl)e.CommandSource).NamingContainer).RowIndex;
                    else
                        rowIndex = Convert.ToInt32(e.CommandArgument);
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out Guid issueId))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    if (IsEdit)
                    {
                        if (EditIssueHandlerCallback != null)
                            EditIssueHandlerCallback(issueId, EventArgs.Empty);
                    }
                    else
                    {
                        CtrlViewIssueDetail viewControl = FindControl("CtrlViewIssueDetail1") as CtrlViewIssueDetail;
                        if (viewControl != null)
                            viewControl.OpenModal(issueId);
                    }
                    break;
                case "ITEM_VIEW_DETAIL":
                    if (!IsEdit && !IsView)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    int rowIndexView = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndexView = ((GridViewRow)((WebControl)e.CommandSource).NamingContainer).RowIndex;
                    else
                        rowIndexView = Convert.ToInt32(e.CommandArgument);
                    if (!Guid.TryParse(grvData.DataKeys[rowIndexView].Value.ToString(), out Guid issueIdView))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    CtrlViewIssueDetail viewDetailControl = FindControl("CtrlViewIssueDetail1") as CtrlViewIssueDetail;
                    if (viewDetailControl != null)
                        viewDetailControl.OpenModal(issueIdView);
                    break;
                case "ITEM_DELETE":
                    if (!IsDelete)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }
                    int rowIndexDel = e.CommandSource.GetType() != typeof(GridviewExtension) ? ((GridViewRow)((WebControl)e.CommandSource).NamingContainer).RowIndex : Convert.ToInt32(e.CommandArgument);
                    if (!Guid.TryParse(grvData.DataKeys[rowIndexDel].Value.ToString(), out Guid issueIdDel))
                    {
                        ShowInvalidDataError();
                        return;
                    }
                    TblVanDe issueDel = TblVanDe.FetchByID(issueIdDel);
                    if (issueDel == null || issueDel.DaXoa == true)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }
                    ConfirmResult result = new ConfirmResult { CommandName = "ISSUE_DELETE", Value = issueDel };
                    this.CURRENT_PAGE.CurrentConfirmResult = result;
                    MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION), string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THE_DATA), issueDel.TenVanDe), MSGButton.DeleteCancel, MSGIcon.Error);
                    OpenMessageBox(msg, result, false, false);
                    break;
            }
        }
        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null)
            {
                if (e.Submit && e.CommandName != null)
                {
                    if (e.CommandName.Contains("ISSUE_DELETE"))
                    {
                        TblVanDe issue = e.Value as TblVanDe;
                        if (issue == null)
                        {
                            ShowInvalidNotFoundData();
                            return;
                        }
                        try
                        {
                            IssueManager.Instance.DeleteIssue(issue);
                            ShowSuccessDeleteData();
                            grvData.CurrentPageIndex = 1;
                            grvData.Rebind();
                        }
                        catch (Exception exc) { ShowNotify(exc.Message, MSGType.Error); }
                    }
                    else if (e.CommandName.Contains("ISSUE_COMPLETE_PROCESS"))
                    {
                        if (KhaiBaoGioManager.KhaiBaoGio20DaKichHoat())
                        {
                            ShowNotify("Không thể bỏ qua bước gửi duyệt/PM phê duyệt giờ xử lý Vấn đề. Hãy mở Tính giờ công việc > Vấn đề.", MSGType.Error);
                            return;
                        }
                        TblVanDe issue = e.Value as TblVanDe;
                        if (issue == null)
                        {
                            ShowInvalidNotFoundData();
                            return;
                        }
                        try
                        {
                            issue.TrangThai = 1;
                            issue.NgayCapNhat = DateTime.Now;
                            issue.NguoiCapNhat = SweetContext.Current != null ? SweetContext.Current.UserName : "System";
                            issue.Save();
                            ShowNotify("Đã cập nhật vấn đề sang trạng thái Đã xử lý!", MSGType.Success);
                            grvData.Rebind();
                        }
                        catch (Exception exc) { ShowNotify(exc.Message, MSGType.Error); }
                    }
                }
                else
                {
                    ShowInvalidNotFoundData();
                    return;
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
            if (master != null)
                master.btnSearchSingle_Click(searchTagBox, pnlSearchDefaultStatus, grvData, txtSearchSingle);
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
                    master.searchTagBox_TagClosed(searchTagBox, tag, pnlSearchDefaultStatus, null, grvData, txtSearchSingle, out searchType);
                }
                upSearchTagBox.Update();
                if (pnlSearchDropdowns != null) pnlSearchDropdowns.Update();
                string script = string.Format("$('#{0}').val('');", txtSearchSingle.ClientID);
                ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "UpdateTxtSearch", script, true);
            }
            catch (Exception exc) { ShowNotify(exc.Message, MSGType.Error); }
        }
        protected void lbtAdd_Click(object sender, EventArgs e)
        {
            if (!this.CURRENT_PAGE.IsAdd)
            {
                ShowAccessDeniedNotify();
                return;
            }
            if (NewIssueHandlerCallback != null)
                NewIssueHandlerCallback(Guid.Empty, EventArgs.Empty);
        }
        protected string GetMucDoAnhHuongText(object value)
        {
            if (value == null || value == DBNull.Value) return "—";
            MucDoAnhHuonEnum mucDo = (MucDoAnhHuonEnum)Convert.ToInt32(value);
            return GetResourceText(IssueManager.Instance.GetValueForMucDoAnhHuong(mucDo));
        }
        protected bool DuocGuiDuyetIssue(object status, object approval, object assigned, object reported)
        {
            return LaKbg20Issue && IsView && Convert.ToInt32(status) == 0
                && (GetIssueApprovalValue(approval) == 0 || GetIssueApprovalValue(approval) == 3)
                && Convert.ToInt32(assigned) == 1;
        }
        protected bool DuocDuyetIssue(object status, object approval)
        {
            return LaKbg20Issue && CoQuyenQuanLyIssue
                && Convert.ToInt32(status) == 0 && GetIssueApprovalValue(approval) == 1;
        }
        protected bool CoNutTrangThaiIssue(object status, object approval, object assigned, object reported)
        {
            return DuocGuiDuyetIssue(status, approval, assigned, reported) || DuocDuyetIssue(status, approval);
        }
        protected string GetIssueStatusBadge(object status, object approval)
        {
            if (status == null || status == DBNull.Value) return "—";
            if (Convert.ToInt32(status) == 1)
                return "<span class='badge-pill-custom badge-status-done'>Đã xử lý</span>";
            if (GetIssueApprovalValue(approval) == 1)
                return "<span class='badge-pill-custom badge-status-review'>Chờ duyệt</span>";
            return "<span class='badge-pill-custom badge-status-doing'>Đang xử lý</span>";
        }
        protected void grvData_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            DataRowView data = e.Row.DataItem as DataRowView;
            if (data == null) return;
            WebControl button = e.Row.FindControl("lbtDuyetCong") as WebControl;
            if (button == null) return;
            bool canManage = CoQuyenQuanLyIssue;
            bool enabledModule = LaKbg20Issue;
            bool approved = Convert.ToInt32(data["TrangThai"]) == 1 && GetIssueApprovalValue(data["TrangThaiDuyetGio"]) == 2;
            bool hasHours = data.Row.Table.Columns.Contains("CoDuLieuGioVanDe") && Convert.ToInt32(data["CoDuLieuGioVanDe"]) == 1;
            bool finalized = data.Row.Table.Columns.Contains("DaChotGioVanDe") && Convert.ToInt32(data["DaChotGioVanDe"]) == 1;
            bool missing = data.Row.Table.Columns.Contains("CoNguoiChuaBaoGio") && Convert.ToInt32(data["CoNguoiChuaBaoGio"]) == 1;
            bool canOpen = canManage && enabledModule && approved && hasHours && (finalized || !missing);
            button.Enabled = canOpen;
            button.CssClass = "btn-grid-action btn-qlpm-review text-decoration-none" + (!canOpen ? " is-disabled" : finalized ? " is-final" : "");
            string tip = !canManage ? "Chỉ PM dự án hoặc quản trị viên được tính công."
                : !enabledModule ? "Tính giờ vấn đề chưa được kích hoạt."
                : !approved ? "Chỉ tính công sau khi vấn đề đã được PM duyệt hoàn thành."
                : !hasHours ? "Vấn đề chưa có giờ công để đối soát."
                : finalized ? "Đã chốt giờ công — nhấn để xem hoặc điều chỉnh."
                : missing ? "Còn nhân viên chưa báo hoàn thành phần việc; chưa thể chốt giờ công."
                : "Đủ điều kiện — nhấn để tính và chốt giờ công.";
            button.ToolTip = tip;
            button.Attributes["aria-disabled"] = canOpen ? "false" : "true";
            var wrap = e.Row.FindControl("qlpmActionWrap") as System.Web.UI.HtmlControls.HtmlGenericControl;
            if (wrap != null) wrap.Attributes["title"] = tip;
            Label mark = e.Row.FindControl("lblPmPending") as Label;
            if (mark != null)
            {
                mark.Visible = canOpen;
                mark.Text = finalized ? "✓" : "!";
                mark.CssClass = finalized ? "btn-qlpm-final-mark" : "btn-qlpm-pending";
                mark.ToolTip = tip;
            }
        }

        protected int GetIssueApprovalValue(object value)
        { return value==null || value==DBNull.Value ? 0 : Convert.ToInt32(value); }

        protected string GetTrangThaiVanDeText(object value)
        {
            if (value == null || value == DBNull.Value) return "—";
            int status = Convert.ToInt32(value);
            if (status == 0) return "Đang xử lý";
            if (status == 1) return "Đã xử lý";
            return "—";
        }
        protected string GetNguonGocVanDeText(object value)
        {
            if (value == null || value == DBNull.Value) return "—";
            NguonGocVanDeEnum origin = (NguonGocVanDeEnum)Convert.ToInt32(value);
            return GetResourceText(IssueManager.Instance.GetValueForNguonGocVanDe(origin));
        }
        protected string GetFormattedDate(object value)
        {
            return _controlHelpers.FormatDateTime(value);
        }
    }
}
