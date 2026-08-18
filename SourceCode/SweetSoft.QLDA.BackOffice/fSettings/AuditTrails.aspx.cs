using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using static SweetSoft.QLDA.Controls.EnumHelper;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;

namespace SweetSoft.QLDA.BackOffice
{
    public partial class AuditTrails : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get
            {
                return ModuleKeys.AuditLog;
            }
        }
        private AuditManager _auditManager;
        protected void Page_Load(object sender, EventArgs e)
        {
            ScriptManager script = ScriptManager.GetCurrent(this.Page);
            script.RegisterAsyncPostBackControl(lbtConfirm);
            script.RegisterAsyncPostBackControl(lbtSearchAdvanced);
            script.RegisterAsyncPostBackControl(lbtCancel);
            if(_auditManager == null)
                _auditManager = new AuditManager(new Core.SysManager.Models.ClientInfo()
                {
                    UserId = SweetContext.Current.UserId,
                    IpAddress = SweetContext.Current.CurrentUserIp,
                    UserAgent = SweetContext.Current.CurrentUserAgent
                });
            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);
                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.SYSTEM_LOG));
                Navigation1.keyValuePairUrls = new Dictionary<string, string>()
                {
                    {RewriteURLHelper.AuditLogs, GetResourceText(BackEndResourceKeys.SYSTEM_LOG) }
                };
                ApplyControlsText();
                BindingExtraControls.BindDropdownEnum<LogActions.Actions>(ddlSearchAction, true);
                ControlHelpers controlHelpers = new ControlHelpers();
                controlHelpers.BindYears(ddlYear);
                controlHelpers.BindUsers(ddlSearchUser, true);
                txtSearchSingle.EnterSubmitClientID = lbtSearchSingle.ClientID;
                MasterTemplate master = Page.Master as MasterTemplate;
                master.LoadSessionLastSearch(searchTagBox, pnlSearchPopup, grvData, txtSearchSingle);
                InitGridData();
            }
        }
        private void ApplyControlsText()
        {
            Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.SYSTEM_LOG);
            txtSearchSingle.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            txtSearchIPAddress.SearchTagItemText = GetResourceText(BackEndResourceKeys.IP_ADDRESS);
            txtSearchDate.SearchTagItemText = GetResourceText(BackEndResourceKeys.DATE);
            ddlSearchAction.SearchTagItemText = GetResourceText(BackEndResourceKeys.ACTION);
            ddlSearchUser.SearchTagItemText = GetResourceText(BackEndResourceKeys.ACCOUNT);
            //---------------------------------------------------------------------
            txtSearchIPAddress.PlaceHolder = txtSearchSingle.PlaceHolder
                = GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            txtSearchDate.PlaceHolder = txtTimeDelete.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_DATE);
            ddlSearchAction.PlaceHolder = ddlSearchUser.PlaceHolder
                = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            //---------------------------------------------------------------------
            lbtSearchAdvanced.ToolTip = lbtSearchAdvanced.Text = GetResourceText(BackEndResourceKeys.SEARCH);
            lbtCancel.ToolTip = lbtCancel.Text = GetResourceText(BackEndResourceKeys.REFRESH);
            lbtConfirm.ToolTip = lbtConfirm.Text = GetResourceText(BackEndResourceKeys.CONFIRM);
            //-----------------------------------------------------------
            List<string> lstTableHeader = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.IP_ADDRESS),
                GetResourceText(BackEndResourceKeys.DATE),
                GetResourceText(BackEndResourceKeys.ACCOUNT),
                GetResourceText(BackEndResourceKeys.ACTION),
                GetResourceText(BackEndResourceKeys.FUNCTION),
                "Changes",
                GetResourceText(BackEndResourceKeys.BROWSER),
            };
            grvData.HeaderTexts = lstTableHeader;
        }
        #region Search + Init gridview
        private void InitGridData()
        {
            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = TblAuditTemp.Columns.ChangedAt;
            grvData.CurrentSortDerection = "DESC";
            grvData.Rebind();
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
                //--------------------------
                DataTable dt = null;
                if (grid.GridSearchType == GridSearchType.Single)
                    dt = _auditManager.SearchAuditLogsAsync(DateTime.UtcNow.Year, txtSearchSingle.Text, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                else
                {
                    Dictionary<string, object> keyValueSearchs = new Dictionary<string, object>();
                    ControlHelpers controlHelpers = new ControlHelpers();
                    keyValueSearchs = controlHelpers.GetControlValues(pnlSearchPopup);
                    dt = _auditManager.SearchAuditLogsAsync(DateTime.UtcNow.Year, keyValueSearchs, $"{grid.CurrentSortExpression} {grid.CurrentSortDerection}", rowIndex, pageSize, out totalRows);
                }
                if (dt == null || dt.Rows.Count == 0)
                {
                    grvData.DataSource = null;
                    grvData.DataBind();
                    ctrlGridviewPaging.Visible = false;
                }
                else
                {
                    if (dt.Rows.Count > 0)
                        ctrlGridviewPaging.Visible = true;
                    else
                        ctrlGridviewPaging.Visible = false;
                    grvData.VirtualItemCount = totalRows;
                    grvData.DataSource = dt;
                    grvData.DataBind();
                    ctrlGridviewPaging.PageIndex = grvData.CurrentPageIndex;
                    ctrlGridviewPaging.PageSize = grvData.CurrentPageSize;
                    ctrlGridviewPaging.TotalItems = totalRows;
                    ctrlGridviewPaging.InitLoad();
                }
                if (IsPostBack)
                    upMain.Update();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void grvData_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName.Equals("deleteitem"))
            {
                int rowIndex = 0;
                if (e.CommandSource.GetType() != typeof(GridviewExtension))
                    rowIndex = ((GridViewRow)((LinkButton)(e.CommandSource)).NamingContainer).RowIndex;
                else
                    rowIndex = Convert.ToInt32(e.CommandArgument);
                int ID = 0;
                int.TryParse(grvData.DataKeys[rowIndex].Value.ToString(), out ID);
                //Kiểm tra quyền
                if (!this.IsDelete)
                {
                    ShowAccessDeniedNotify();
                    return;
                }

                //TblAuditTemp obj = auditManager.get(ID);
                //if (obj == null)
                //{
                //    ShowInvalidNotFoundData();
                //    return;
                //}

                //ConfirmResult result = new ConfirmResult();
                //result.CommandName = "log_delete";
                //result.Value = obj;
                //CurrentConfirmResult = result;
                //MessageBox msg = new MessageBox(GetResourceText(BackEndResourceKeys.NOTIFICATION)
                //    , string.Format(GetResourceText(BackEndResourceKeys.CONFIRM_DELETE_OF_SYSTEM_LOG), obj.LogName)
                //    , MSGButton.AcceptCancel, MSGIcon.Error);
                //OpenMessageBox(msg, result, false, false);
            }
        }

        protected void ctrlGridviewPaging_PageChanged(object sender, GridviewCustomPageChangeArgs e)
        {
            grvData.CurrentPageSize = e.CurrentPageSize;
            grvData.CurrentPageIndex = e.CurrentPageNumber;
            grvData.Rebind();
        }
        #endregion
        #region Button
        protected void btnSearch_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            master.btnSearchSingle_Click(searchTagBox, grvData, txtSearchSingle);
            upSearchTagBox.Update();

        }
        protected void btnSearchAdvanced_ServerClick(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            master.btnSearchAdvanced_Click(searchTagBox, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            txtSearchIPAddress.Text = string.Empty;
            ddlSearchUser.SelectedIndex = ddlSearchAction.SelectedIndex = -1;
            txtSearchDate.ClearDate();
            pnlSearch.Update();
            MasterTemplate master = Page.Master as MasterTemplate;
            master.btnSearchAdvanced_Click(searchTagBox, pnlSearchPopup, grvData);
            upSearchTagBox.Update();
        }
        protected void searchTagBox_TagClosed(object sender, SearchTagItem tag)
        {
            try
            {
                MasterTemplate master = Page.Master as MasterTemplate;
                GridSearchType? searchType;
                master.searchTagBox_TagClosed(searchTagBox, tag, pnlSearchPopup, grvData, txtSearchSingle, out searchType);
                pnlSearch.Update();
                string script = string.Format("$('#{0}').val('');", txtSearchSingle.ClientID);
                ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "UpdateTxtSearch", script, true);
            }
            catch (Exception exc)
            {
               ShowNotify(exc.Message, MSGType.Error);
            }
        }
        #endregion
        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null)
            {
                if (e.Submit && e.CommandName != null)
                {
                    if (e.CommandName.Equals("log_delete"))
                    {
                        TblAuditTemp obj = e.Value as TblAuditTemp;
                        if (obj == null)
                        {
                            ShowInvalidNotFoundData();
                            return;
                        }

                        try
                        {
                            //LogManager.Delete(obj.Id);
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

        protected void lbtConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                if (txtTimeDelete.StartValue == DateTime.MinValue)
                {
                    validationEngine.AddErrorPrompt(txtTimeDelete.ClientID, $"* {GetResourceText(BackEndResourceKeys.SELECT_DATE)}");
                    validationEngine.ShowErrorPrompt();
                }

                //AuditTrailsManager.Delete(txtStartDate.StartValue, txtStartDate.EndValue);
                ScriptManager.RegisterClientScriptBlock(this.Page, GetType(), "LogJs.CloseModal", "LogJs.CloseModal();", true);
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