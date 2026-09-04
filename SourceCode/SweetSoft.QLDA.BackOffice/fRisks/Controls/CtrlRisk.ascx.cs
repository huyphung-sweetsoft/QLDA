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

namespace SweetSoft.QLDA.BackOffice.fRisks.Controls
{
    public partial class CtrlRisk : BaseAdminUserControl
    {
        public EventHandler NewRiskHandlerCallback;
        public EventHandler EditRiskHandlerCallback;
        private readonly RiskManager _riskManager = RiskManager.Instance;
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
            script.RegisterAsyncPostBackControl(lbtSearchSingle);
            script.RegisterAsyncPostBackControl(lbtSearchAdvanced);
            script.RegisterAsyncPostBackControl(lbtCancel);
        }   
        public void InitControls()
        {
            ApplyControlsText();
            AssignSearchColumns();
            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
            lbtAdd.Visible = this.CURRENT_PAGE.IsAdd;
            MasterTemplate master = Page.Master as MasterTemplate;
            master.LoadSessionLastSearch(searchTagBox, pnlSearchPopup, grvData, txtSearchSingle);
            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = TblRuiRoDuAn.Columns.TenRuiRo; 
            grvData.CurrentSortDerection = "ASC";
            grvData.Rebind();
            pnlSearch.Update();
            pnlButtons.Update();
        }
        private void AssignSearchColumns()
        {
            txtSearchTenRuiRo.SearchColumn = TblRuiRoDuAn.Columns.TenRuiRo;
        }
        public void Rebind()
        {
            grvData.CurrentPageIndex = 1;
            grvData.Rebind();
        }
        private void ApplyControlsText()
        {
            txtSearchSingle.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            txtSearchTenRuiRo.SearchTagItemText = GetResourceText(BackEndResourceKeys.RISK_NAME);
            lbtAdd.ToolTip = lbtAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);
            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.RISK_NAME),
                GetResourceText(BackEndResourceKeys.IMPACT),
                GetResourceText(BackEndResourceKeys.PROBABILITY),
                GetResourceText(BackEndResourceKeys.RISK_LEVEL),
                GetResourceText(BackEndResourceKeys.MONITOR),
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
                    dt = _riskManager.SearchRisk(
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

                    dt = _riskManager.SearchRisk(
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

                    Guid riskId = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out riskId))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    if (EditRiskHandlerCallback != null)
                        EditRiskHandlerCallback(riskId, EventArgs.Empty);
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

                    Guid riskIdDel = Guid.Empty;
                    if (!Guid.TryParse(grvData.DataKeys[rowIndexDel].Value.ToString(), out riskIdDel))
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    TblRuiRoDuAn riskDel = TblRuiRoDuAn.FetchByID(riskIdDel);
                    if (riskDel == null || riskDel.DaXoa == true)
                    {
                        ShowInvalidNotFoundData();
                        return;
                    }

                    ConfirmResult result = new ConfirmResult();
                    result.CommandName = "RISK_DELETE";
                    result.Value = riskDel;
                    this.CURRENT_PAGE.CurrentConfirmResult = result;

                    MessageBox msg = new MessageBox(
                        GetResourceText(BackEndResourceKeys.NOTIFICATION),
                        string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THE_DATA), riskDel.TenRuiRo),
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
                    if (e.CommandName.Contains("RISK_DELETE"))
                    {
                        TblRuiRoDuAn risk = e.Value as TblRuiRoDuAn;
                        if (risk == null)
                        {
                            ShowInvalidNotFoundData();
                            return;
                        }

                        try
                        {
                            risk.DaXoa = true;
                            risk.NgayCapNhat = DateTime.Now;
                            risk.Save();
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
            master.btnSearchSingle_Click(searchTagBox, grvData, txtSearchSingle);
            upSearchTagBox.Update();
        }
        protected void searchTagBox_TagClosed(object sender, SearchTagItem tag)
        {
            try
            {
                MasterTemplate master = Page.Master as MasterTemplate;
                GridSearchType? searchType;
                master.searchTagBox_TagClosed(searchTagBox, tag, null, pnlSearchPopup, grvData, txtSearchSingle, out searchType);
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
            master.btnSearchAdvanced_Click(searchTagBox, null, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            new ControlHelpers().ClearControlValues(pnlSearchPopup.Controls);
            pnlSearch.Update();
            MasterTemplate master = Page.Master as MasterTemplate;
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
            if (NewRiskHandlerCallback != null) 
                NewRiskHandlerCallback(Guid.Empty, EventArgs.Empty);
        }
        protected string GetMucDoAnhHuongText(object value)
        {
            if (value == null || value == DBNull.Value) return "—";
            MucDoAnhHuonEnum mucDo = (MucDoAnhHuonEnum)Convert.ToInt32(value);
            return GetResourceText(_riskManager.GetValueForMucDoAnhHuong(mucDo));
        }
        protected string GetMucDoRuiRoText(object value)
        {
            if (value == null || value == DBNull.Value) return "—";

            decimal score = Convert.ToDecimal(value);
            string textMucDo = "";
            if (score < 1.0m) textMucDo = GetResourceText(BackEndResourceKeys.VERY_LOW);
            else if (score >= 1.0m && score < 2.0m) textMucDo = GetResourceText(BackEndResourceKeys.LOW);
            else if (score >= 2.0m && score < 3.5m) textMucDo = GetResourceText(BackEndResourceKeys.MEDIUM);
            else if (score >= 3.5m && score < 4.5m) textMucDo = GetResourceText(BackEndResourceKeys.HIGH);
            else textMucDo = GetResourceText(BackEndResourceKeys.VERY_HIGH);
            return $"{textMucDo}";
        }
    }
}