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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fCosts.Controls
{
    public partial class CtrlCost : BaseAdminUserControl
    {
        public EventHandler NewCostHandlerCallback;
        public EventHandler EditCostHandlerCallback;
        public EventHandler OpenCostFilesHandlerCallback;
        private ControlHelpers _controlHelpers = new ControlHelpers();

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

        protected bool IsPM
        {
            get
            {
                try
                {
                    Guid? pmId = DuAnManager.Instance.LayIdNhanVienQuanLy(this.ProjectId);
                    return pmId.HasValue && pmId.Value == SweetContext.Current.UserId;
                }
                catch
                {
                    return false;
                }
            }
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
                script.RegisterAsyncPostBackControl(ddlSearchTrangThaiChiPhi);
            }
        }

        public void InitControls()
        {
            ApplyControlsText();
            AssignSearchColumns();
            txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
            lbtAdd.Visible = this.CURRENT_PAGE.IsAdd;
            _controlHelpers.BindProjectMembers(ddlSearchNhanVienYeuCau, this.ProjectId, null);
            _controlHelpers.BindTrangThaiChiPhi(ddlSearchTrangThaiChiPhi);
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master != null)
                master.LoadSessionLastSearch(searchTagBox, pnlSearchPopup, grvData, txtSearchSingle);

            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = "NgayTao";
            grvData.CurrentSortDerection = "DESC";
            grvData.Rebind();
            pnlSearch.Update();
            pnlButtons.Update();
        }

        private void AssignSearchColumns()
        {
            txtSearchTenKhoanChi.SearchColumn = "TenKhoanChi";
            txtSearchSoTienMin.SearchColumn = "SoTienMin";
            txtSearchSoTienMax.SearchColumn = "SoTienMax";
        }

        public void Rebind()
        {
            grvData.CurrentPageIndex = 1;
            grvData.Rebind();
        }

        private void ApplyControlsText()
        {
            txtSearchSingle.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            txtSearchTenKhoanChi.SearchTagItemText = GetResourceText(BackEndResourceKeys.COST_NAME);
            ddlSearchNhanVienYeuCau.Attributes["SearchTagItemText"] = GetResourceText(BackEndResourceKeys.REQUESTER);
            ddlSearchTrangThaiChiPhi.SearchTagItemText = GetResourceText(BackEndResourceKeys.STATUS);
            txtSearchSoTienMin.SearchTagItemText = GetResourceText(BackEndResourceKeys.LOWEST_TOTAL_AMOUNT);
            txtSearchSoTienMax.SearchTagItemText = GetResourceText(BackEndResourceKeys.HIGHEST_TOTAL_AMOUNT);
            lbtAdd.ToolTip = lbtAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);
            lbtCancel.ToolTip = GetResourceText(BackEndResourceKeys.CANCEL);
            txtSearchSingle.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            lbtSearchAdvanced.Text = GetResourceText(BackEndResourceKeys.APPLY);
            lbtCancel.Text = GetResourceText(BackEndResourceKeys.REFRESH);

            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.COST_CODE),
                GetResourceText(BackEndResourceKeys.COST_NAME),
                GetResourceText(BackEndResourceKeys.PRICE),
                GetResourceText(BackEndResourceKeys.QUANTITY),
                GetResourceText(BackEndResourceKeys.TOTAL_AMOUNT),
                GetResourceText(BackEndResourceKeys.REQUESTER),
                GetResourceText(BackEndResourceKeys.DATE_CREATED),
                GetResourceText(BackEndResourceKeys.STATUS),
                GetResourceText(BackEndResourceKeys.ACTION)
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
                int startRow = (grid.CurrentPageIndex - 1) * grid.CurrentPageSize;
                int endRow = startRow + grid.CurrentPageSize;

                Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();

                if (pnlSearchPopup != null)
                {
                    var advParams = new ControlHelpers().GetControlValues(pnlSearchPopup);
                    foreach (var item in advParams) keyValueSearchs[item.Key] = item.Value;
                }

                if (pnlSearchDefaultStatus != null)
                {
                    var defaultParams = new ControlHelpers().GetControlValues(pnlSearchDefaultStatus);
                    foreach (var item in defaultParams) keyValueSearchs[item.Key] = item.Value;
                }

                DataTable dt = CostManager.Instance.SearchCost(
                        this.ProjectId,
                    txtSearchSingle.Text,
                        keyValueSearchs,
                        $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}",
                        startRow, endRow, out totalRows);

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
                case "OPEN_APPROVE_MODAL":
                    if (!this.IsPM)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }

                    int rowIndexApprove = (e.CommandSource.GetType() != typeof(GridviewExtension)) ?
                        ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex : Convert.ToInt32(e.CommandArgument);

                    if (Guid.TryParse(grvData.DataKeys[rowIndexApprove].Value.ToString(), out Guid costIdApprove))
                    {
                        hdfApproveCostId.Value = costIdApprove.ToString();
                        txtRejectReason.Text = string.Empty;

                        ScriptManager.RegisterStartupScript(this, GetType(), "ResetApproveUI", "$('#divRejectReason').hide(); $('#grpActionButtons').show();", true);
                        mdlFastApprove.OpenModal(true);
                    }
                    break;

                case "COST_FILES":
                    if (!this.IsView)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }

                    Guid costIdForFiles;
                    if (!Guid.TryParse(Convert.ToString(e.CommandArgument), out costIdForFiles) || costIdForFiles == Guid.Empty)
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    if (OpenCostFilesHandlerCallback != null)
                    {
                        OpenCostFilesHandlerCallback(costIdForFiles, EventArgs.Empty);
                    }
                    break;

                case "ITEM_DETAIL":
                    if (!this.CURRENT_PAGE.IsEdit)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }

                    int rowIndex = (e.CommandSource.GetType() != typeof(GridviewExtension)) ?
                        ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex : Convert.ToInt32(e.CommandArgument);

                    Guid costId = Guid.Empty;
                    if (Guid.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out costId) && EditCostHandlerCallback != null)
                        EditCostHandlerCallback(costId, EventArgs.Empty);
                    break;

                case "ITEM_DELETE":
                    if (!this.CURRENT_PAGE.IsDelete)
                    {
                        ShowAccessDeniedNotify();
                        return;
                    }

                    int rowIndexDel = (e.CommandSource.GetType() != typeof(GridviewExtension)) ?
                        ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex : Convert.ToInt32(e.CommandArgument);

                    Guid costIdDel = Guid.Empty;
                    if (Guid.TryParse(grvData.DataKeys[rowIndexDel].Value.ToString(), out costIdDel))
                    {
                        TblChiPhi costDel = TblChiPhi.FetchByID(costIdDel);
                        if (costDel != null && (costDel.DaXoa == false || costDel.DaXoa == null))
                        {
                            ConfirmResult result = new ConfirmResult { CommandName = "COST_DELETE", Value = costDel };
                            this.CURRENT_PAGE.CurrentConfirmResult = result;

                            MessageBox msg = new MessageBox(
                                GetResourceText(BackEndResourceKeys.NOTIFICATION),
                                string.Format(GetResourceText(BackEndResourceKeys.PLEASE_CONFIRM_TO_DELETE_THE_DATA), costDel.TenKhoanChi),
                                MSGButton.DeleteCancel, MSGIcon.Error);
                            OpenMessageBox(msg, result, false, false);
                        }
                    }
                    break;
            }
        }

        protected void btnQuickApprove_Click(object sender, EventArgs e)
        {
            if (Guid.TryParse(hdfApproveCostId.Value, out Guid costId))
            {
                try
                {
                    TblChiPhi costToApprove = TblChiPhi.FetchByID(costId);
                    if (costToApprove != null)
                    {
                        ConfirmResult result = new ConfirmResult { CommandName = "COST_APPROVE", Value = costToApprove };
                        this.CURRENT_PAGE.CurrentConfirmResult = result;

                        MessageBox msg = new MessageBox(
                            GetResourceText(BackEndResourceKeys.NOTIFICATION),
                            string.Format("Bạn có chắc chắn muốn DUYỆT khoản chi <b>{0}</b> này không?", costToApprove.TenKhoanChi),
                            MSGButton.AcceptCancel,
                            MSGIcon.Success
                        );

                        mdlFastApprove.CloseModal();
                        OpenMessageBox(msg, result, false, false);
                    }
                }
                catch (Exception exc)
                {
                    ShowNotify(exc.Message, MSGType.Error);
                }
            }
        }

        protected void btnConfirmReject_Click(object sender, EventArgs e)
        {
            if (Guid.TryParse(hdfApproveCostId.Value, out Guid costId))
            {
                string reason = txtRejectReason.Text.Trim();
                if (string.IsNullOrEmpty(reason))
                {
                    ShowNotify("Vui lòng nhập lý do từ chối!", MSGType.Warning);
                    ScriptManager.RegisterStartupScript(this, GetType(), "KeepRejectUI", "$('#grpActionButtons').hide(); $('#divRejectReason').show();", true);
                    mdlFastApprove.OpenModal(true);
                    return;
                }

                try
                {
                    TblChiPhi cost = TblChiPhi.FetchByID(costId);
                    if (cost != null)
                    {
                        cost.TrangThai = 2;
                        cost.LyDoTuChoi = reason;
                        cost.Save();

                        ShowNotify("Đã từ chối khoản chi!", MSGType.Success);
                        mdlFastApprove.CloseModal();
                        Rebind();
                    }
                }
                catch (Exception exc)
                {
                    ShowNotify(exc.Message, MSGType.Error);
                }
            }
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null && e.Submit && e.CommandName != null)
            {
                if (e.CommandName.Contains("COST_DELETE"))
                {
                    TblChiPhi cost = e.Value as TblChiPhi;
                    if (cost != null)
                    {
                        try
                        {
                            CostManager.Instance.DeleteCost(cost);
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
                else if (e.CommandName.Contains("COST_APPROVE"))
                {
                    TblChiPhi cost = e.Value as TblChiPhi;
                    if (cost != null)
                    {
                        try
                        {
                            cost.TrangThai = 1;
                            cost.LyDoTuChoi = null;
                            cost.Save();

                            ShowNotify("Đã phê duyệt khoản chi thành công!", MSGType.Success);
                            grvData.Rebind();
                        }
                        catch (Exception exc)
                        {
                            ShowNotify(exc.Message, MSGType.Error);
                        }
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

        protected void btnSearch_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master != null)
                master.btnSearchSingle_Click(searchTagBox, grvData, txtSearchSingle);
            upSearchTagBox.Update();
        }

        protected void bootstrapDropdown_SelectedValueChanged(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master != null)
            {
                master.btnSearchSingle_Click(searchTagBox, pnlSearchDefaultStatus, grvData, txtSearchSingle);
            }
            upSearchTagBox.Update();
            pnlSearchDropdowns.Update();
        }

        protected void searchTagBox_TagClosed(object sender, SearchTagItem tag)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master != null)
            {
                GridSearchType? searchType;
                master.searchTagBox_TagClosed(searchTagBox, tag, pnlSearchDefaultStatus, pnlSearchPopup, grvData, txtSearchSingle, out searchType);
            }
            pnlSearch.Update();
            if (pnlSearchDropdowns != null) pnlSearchDropdowns.Update();
            upSearchTagBox.Update();
            ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "UpdateTxtSearch", $"$('#{txtSearchSingle.ClientID}').val('');", true);
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
            if (NewCostHandlerCallback != null)
                NewCostHandlerCallback(Guid.Empty, EventArgs.Empty);
        }

        public string GetTrangThaiChiPhiText(object value)
        {
            if (value == null || value == DBNull.Value) return "—";
            int statusCode = Convert.ToInt32(value);
            TrangThaiChiPhi status = (TrangThaiChiPhi)statusCode;
            string text = GetResourceText(CostManager.Instance.GetValueForTrangThaiChiPhi(status));
            string cssClass = "badge-status badge-status-pending";
            if (statusCode == 1)
            {
                cssClass = "badge-status badge-status-approved";
            }
            else if (statusCode == 2)
            {
                cssClass = "badge-status badge-status-rejected";
            }
            return $"<span class='{cssClass}'>{text}</span>";
        }
    }
}
