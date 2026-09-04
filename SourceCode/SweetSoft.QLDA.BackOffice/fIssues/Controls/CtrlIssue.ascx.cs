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

        private readonly IssueManager _issueManager = IssueManager.Instance;

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
            grvData.CurrentSortExpression = "TenVanDe";
            grvData.CurrentSortDerection = "ASC";
            grvData.Rebind();
            pnlSearch.Update();
            pnlButtons.Update();
        }

        private void AssignSearchColumns()
        {
            txtSearchTenVanDe.SearchColumn = "TenVanDe";
        }

        public void Rebind()
        {
            grvData.CurrentPageIndex = 1;
            grvData.Rebind();
        }

        private void ApplyControlsText()
        {
            txtSearchSingle.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            txtSearchTenVanDe.SearchTagItemText = "Tên vấn đề"; 
            lbtAdd.ToolTip = lbtAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);

            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.ISSUE_CODE),
                GetResourceText(BackEndResourceKeys.ISSUE_NAME),
                GetResourceText(BackEndResourceKeys.IMPACT),
                GetResourceText(BackEndResourceKeys.STATUS),
                GetResourceText(BackEndResourceKeys.ORIGIN),
                GetResourceText(BackEndResourceKeys.CREATED_BY),
                GetResourceText(BackEndResourceKeys.ACTION),
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

                if (grid.GridSearchType == GridSearchType.Single)
                {
                    dt = _issueManager.SearchIssue(
                        this.ProjectId,
                        txtSearchSingle.Text,
                        null,
                        $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}",
                        rowIndex,
                        pageSize,
                        out totalRows
                    );
                }
                else
                {
                    Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
                    ControlHelpers controlHelpers = new ControlHelpers();
                    if (pnlSearchPopup != null)
                    {
                        keyValueSearchs = controlHelpers.GetControlValues(pnlSearchPopup);
                    }
                    dt = _issueManager.SearchIssue(
                        this.ProjectId,
                        "",
                        keyValueSearchs,
                        $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}",
                        rowIndex,
                        pageSize,
                        out totalRows
                    );
                }
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

                    Guid issueId = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out issueId))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    if (EditIssueHandlerCallback != null)
                        EditIssueHandlerCallback(issueId, EventArgs.Empty);
                    break;

                case "ITEM_DELETE":
                    if (!this.CURRENT_PAGE.IsDelete)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }

                    int rowIndexDel = 0;
                    if (e.CommandSource.GetType() != typeof(GridviewExtension))
                        rowIndexDel = ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex;
                    else
                        rowIndexDel = Convert.ToInt32(e.CommandArgument);

                    Guid issueIdDel = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndexDel].Value.ToString(), out issueIdDel))
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

                    ConfirmResult result = new ConfirmResult();
                    result.CommandName = "ISSUE_DELETE";
                    result.Value = issueDel;
                    this.CURRENT_PAGE.CurrentConfirmResult = result;

                    MessageBox msg = new MessageBox(
                        GetResourceText(BackEndResourceKeys.NOTIFICATION),
                        string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THE_DATA), issueDel.TenVanDe),
                        MSGButton.DeleteCancel,
                        MSGIcon.Error
                    );
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
                            _issueManager.DeleteIssue(issue);
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
                master.btnSearchSingle_Click(searchTagBox, grvData, txtSearchSingle);
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
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void btnSearchAdvanced_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master != null)
                master.btnSearchAdvanced_Click(searchTagBox, null, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            if (pnlSearchPopup != null)
                new ControlHelpers().ClearControlValues(pnlSearchPopup.Controls);
            pnlSearch.Update();

            MasterTemplate master = Page.Master as MasterTemplate;
            if (master != null)
                master.btnSearchAdvanced_Click(searchTagBox, null, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
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
            return GetResourceText(_issueManager.GetValueForMucDoAnhHuong(mucDo));
        }

        protected string GetTrangThaiVanDeText(object value)
        {
            if (value == null || value == DBNull.Value) return "—";
            TrangThaiVanDeEnum status = (TrangThaiVanDeEnum)Convert.ToInt32(value);
            return GetResourceText(_issueManager.GetValueForTrangThaiVanDe(status));
        }
        protected string GetNguonGocVanDeText(object value)
        {
            if (value == null || value == DBNull.Value) return "—";
            NguonGocVanDeEnum origin = (NguonGocVanDeEnum)Convert.ToInt32(value);
            return GetResourceText(_issueManager.GetValueForNguonGocVanDe(origin));
        }
    }
}