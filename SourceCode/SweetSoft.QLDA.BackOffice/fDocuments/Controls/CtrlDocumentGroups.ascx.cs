using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using static SweetSoft.QLDA.Controls.EnumHelper;

namespace SweetSoft.QLDA.BackOffice.fDocuments.Controls
{
    public partial class CtrlDocumentGroups : BaseAdminUserControl
    {
        protected bool IsAdd
        {
            get { return CURRENT_PAGE.IsAdd; }
        }

        protected bool IsEdit
        {
            get { return CURRENT_PAGE.IsEdit; }
        }

        protected bool IsDelete
        {
            get { return CURRENT_PAGE.IsDelete; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButtons();
        }

        public void InitControls()
        {
            ApplyControlsText();
            BindDropdowns();
            LoadSearchState();
            ResetForm();
            InitGridData();
        }

        private void RegisterAsyncButtons()
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
            if (scriptManager == null)
                return;

            scriptManager.RegisterAsyncPostBackControl(btnSearch);
            scriptManager.RegisterAsyncPostBackControl(btnSearchAdvanced);
            scriptManager.RegisterAsyncPostBackControl(btnResetSearch);
        }

        private void ApplyControlsText()
        {
            txtSearch.SearchTagItemText = GetResourceText(
                BackEndResourceKeys.KEYWORD);
            ddlSearchStatus.SearchTagItemText = GetResourceText(
                BackEndResourceKeys.STATUS);
            txtSearchTenNhom.SearchTagItemText = GetResourceText(
                BackEndResourceKeys.DOCUMENT_GROUP_NAME);
            txtSearchMoTa.SearchTagItemText = GetResourceText(
                BackEndResourceKeys.DESCRIPTION);

            btnSearch.ToolTip = btnSearch.Text = GetResourceText(
                BackEndResourceKeys.SEARCH);
            btnAdd.ToolTip = btnAdd.Text = GetResourceText(
                BackEndResourceKeys.ADD_NEW);
            btnSave.ToolTip = btnSave.Text = GetResourceText(
                BackEndResourceKeys.SAVE);
            btnCancel.ToolTip = btnCancel.Text = GetResourceText(
                BackEndResourceKeys.CANCEL);
            btnSearchAdvanced.ToolTip = btnSearchAdvanced.Text =
                GetResourceText(BackEndResourceKeys.SEARCH);
            btnResetSearch.ToolTip = btnResetSearch.Text =
                GetResourceText(BackEndResourceKeys.REFRESH);

            ddlSearchStatus.Text = GetResourceText(
                BackEndResourceKeys.STATUS);
            txtSearch.PlaceHolder = txtSearchTenNhom.PlaceHolder =
                txtSearchMoTa.PlaceHolder = GetResourceText(
                    BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            chkKichHoat.OnText = GetResourceText(
                BackEndResourceKeys.ACTIVE);
            chkKichHoat.OffText = GetResourceText(
                BackEndResourceKeys.INACTIVE);

            grvData.HeaderTexts = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.DOCUMENT_GROUP_NAME),
                GetResourceText(BackEndResourceKeys.DESCRIPTION),
                GetResourceText(BackEndResourceKeys.DISPLAY_ORDER),
                GetResourceText(BackEndResourceKeys.STATUS),
                GetResourceText(BackEndResourceKeys.CREATED_DATE),
                GetResourceText(BackEndResourceKeys.ACTION)
            };

            txtSearch.EnterSubmitClientID = btnSearch.ClientID;
            btnAdd.Visible = this.IsAdd;
        }

        private void BindDropdowns()
        {
            ControlHelpers controlHelpers = new ControlHelpers();
            controlHelpers.BindStatus(ddlSearchStatus);
        }

        private void LoadSearchState()
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master == null)
                return;

            master.LoadSessionLastSearch(
                searchTagBox,
                pnlSearchPopup,
                grvData,
                txtSearch);
        }

        private void InitGridData()
        {
            grvData.CurrentPageSize = Convert.ToInt32(
                SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression =
                TblNhomTaiLieu.Columns.ThuTuHienThi;
            grvData.CurrentSortDerection = "ASC";
            grvData.Rebind();
        }

        private void RebindGridFromFirstPage()
        {
            grvData.CurrentPageIndex = 1;
            grvData.Rebind();
        }

        protected void grvData_NeedDataSource(
            object sender,
            ExtraGridEventArg e)
        {
            try
            {
                GridviewExtension grid = sender as GridviewExtension;
                if (grid == null)
                {
                    ShowInvalidDataError();
                    return;
                }

                int totalRows;
                int rowOffset =
                    (grid.CurrentPageIndex - 1) * grid.CurrentPageSize;
                int endRow = rowOffset + grid.CurrentPageSize;

                ControlHelpers controlHelpers = new ControlHelpers();
                Dictionary<string, object> searchParameters =
                    controlHelpers.GetControlValues(pnlSearchDefault);
                string orderBy = grid.CurrentSortExpression
                    + " "
                    + grid.CurrentSortDerection;

                DataTable data;
                if (grid.GridSearchType == GridSearchType.Single)
                {
                    data = DocumentGroupManager.Instance.SearchDocumentGroups(
                        txtSearch.Text,
                        searchParameters,
                        orderBy,
                        rowOffset,
                        endRow,
                        out totalRows);
                }
                else
                {
                    Dictionary<string, object> advancedParameters =
                        controlHelpers.GetControlValues(pnlSearchPopup);

                    foreach (KeyValuePair<string, object> parameter
                        in advancedParameters)
                    {
                        searchParameters[parameter.Key] = parameter.Value;
                    }

                    data = DocumentGroupManager.Instance.SearchDocumentGroups(
                        searchParameters,
                        orderBy,
                        rowOffset,
                        endRow,
                        out totalRows);
                }

                bool hasData = data != null && data.Rows.Count > 0;
                grid.VirtualItemCount = totalRows;
                grid.DataSource = hasData ? data : null;
                grid.DataBind();

                ctrlGridviewPaging.Visible = hasData;
                if (hasData)
                {
                    ctrlGridviewPaging.PageIndex = grid.CurrentPageIndex;
                    ctrlGridviewPaging.PageSize = grid.CurrentPageSize;
                    ctrlGridviewPaging.TotalItems = totalRows;
                    ctrlGridviewPaging.InitLoad();
                }

                upMain.Update();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void ctrlGridviewPaging_PageChanged(
            object sender,
            GridviewCustomPageChangeArgs e)
        {
            grvData.CurrentPageSize = e.CurrentPageSize;
            grvData.CurrentPageIndex = e.CurrentPageNumber;
            grvData.Rebind();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master == null)
            {
                RebindGridFromFirstPage();
                return;
            }

            master.btnSearchSingle_Click(
                searchTagBox,
                pnlSearchDefault,
                grvData,
                txtSearch);
        }

        protected void bootstrapDropdown_SelectedValueChanged(
            object sender,
            EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master == null)
            {
                RebindGridFromFirstPage();
                return;
            }

            if (grvData.GridSearchType == GridSearchType.Single)
            {
                master.btnSearchSingle_Click(
                    searchTagBox,
                    pnlSearchDefault,
                    grvData,
                    txtSearch);
            }
            else
            {
                master.btnSearchAdvanced_Click(
                    searchTagBox,
                    pnlSearchDefault,
                    pnlSearchPopup,
                    grvData);
            }
        }

        protected void btnSearchAdvanced_Click(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master == null)
            {
                RebindGridFromFirstPage();
                return;
            }

            master.btnSearchAdvanced_Click(
                searchTagBox,
                pnlSearchDefault,
                pnlSearchPopup,
                grvData);
        }

        protected void btnResetSearch_Click(object sender, EventArgs e)
        {
            ControlHelpers controlHelpers = new ControlHelpers();
            controlHelpers.ClearControlValues(pnlSearchPopup.Controls);
            pnlSearch.Update();

            MasterTemplate master = Page.Master as MasterTemplate;
            if (master == null)
            {
                RebindGridFromFirstPage();
                return;
            }

            master.btnSearchAdvanced_Click(
                searchTagBox,
                pnlSearchDefault,
                pnlSearchPopup,
                grvData);
        }

        protected void searchTagBox_TagClosed(
            object sender,
            SearchTagItem tag)
        {
            try
            {
                MasterTemplate master = Page.Master as MasterTemplate;
                if (master == null)
                    return;

                GridSearchType? searchType;
                master.searchTagBox_TagClosed(
                    searchTagBox,
                    tag,
                    pnlSearchDefault,
                    pnlSearchPopup,
                    grvData,
                    txtSearch,
                    out searchType);
                pnlSearch.Update();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        private void ResetForm()
        {
            hdfIdNhomTaiLieu.Value = string.Empty;
            txtTenNhom.Text = string.Empty;
            txtMoTa.Text = string.Empty;
            txtThuTuHienThi.Text = "0";
            chkKichHoat.Checked = true;
            pnlForm.Visible = false;
        }

        private void ShowAddForm()
        {
            ResetForm();
            litFormTitle.Text = GetResourceText(
                    BackEndResourceKeys.ADD_NEW)
                + " "
                + GetResourceText(
                    BackEndResourceKeys.DOCUMENT_GROUP);
            pnlForm.Visible = true;
            upMain.Update();
        }

        private void ShowEditForm(TblNhomTaiLieu item)
        {
            hdfIdNhomTaiLieu.Value = item.IdNhomTaiLieu.ToString();
            txtTenNhom.Text = item.TenNhom;
            txtMoTa.Text = item.MoTa;
            txtThuTuHienThi.Text = item.ThuTuHienThi.ToString();
            chkKichHoat.Checked = item.KichHoat;
            litFormTitle.Text = GetResourceText(
                    BackEndResourceKeys.EDIT)
                + " "
                + GetResourceText(
                    BackEndResourceKeys.DOCUMENT_GROUP);
            pnlForm.Visible = true;
            upMain.Update();
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            if (!this.IsAdd)
            {
                ShowAccessDeniedNotify();
                return;
            }

            ShowAddForm();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ResetForm();
            upMain.Update();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Guid idNhomTaiLieu = Guid.Empty;
            if (!string.IsNullOrEmpty(hdfIdNhomTaiLieu.Value)
                && !Guid.TryParse(
                    hdfIdNhomTaiLieu.Value,
                    out idNhomTaiLieu))
            {
                ShowInvalidDataError();
                return;
            }

            bool isNew = idNhomTaiLieu == Guid.Empty;
            if (isNew && !this.IsAdd)
            {
                ShowAccessDeniedNotify();
                return;
            }

            if (!isNew && !this.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            int thuTuHienThi;
            if (!int.TryParse(txtThuTuHienThi.Text, out thuTuHienThi))
            {
                ShowNotify(
                    "Thứ tự hiển thị không hợp lệ.",
                    MSGType.Warning);
                return;
            }

            try
            {
                DocumentGroupManager.Instance.Save(
                    idNhomTaiLieu,
                    txtTenNhom.Text,
                    txtMoTa.Text,
                    thuTuHienThi,
                    chkKichHoat.Checked);

                if (isNew)
                    CURRENT_PAGE.ShowSuccessAddNewData();
                else
                    ShowSuccessSaveData();

                ResetForm();
                RebindGridFromFirstPage();
            }
            catch (ArgumentException exc)
            {
                ShowNotify(exc.Message, MSGType.Warning);
            }
            catch (InvalidOperationException exc)
            {
                ShowNotify(exc.Message, MSGType.Warning);
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected void grvData_RowCommand(
            object sender,
            GridViewCommandEventArgs e)
        {
            if (e.CommandName != "EDIT_ITEM"
                && e.CommandName != "DELETE_ITEM")
            {
                return;
            }

            Guid idNhomTaiLieu;
            if (!Guid.TryParse(
                Convert.ToString(e.CommandArgument),
                out idNhomTaiLieu))
            {
                ShowInvalidDataError();
                return;
            }

            if (e.CommandName == "EDIT_ITEM")
            {
                if (!this.IsEdit)
                {
                    ShowAccessDeniedNotify();
                    return;
                }

                TblNhomTaiLieu item =
                    DocumentGroupManager.Instance.GetById(idNhomTaiLieu);
                if (item == null)
                {
                    ShowInvalidNotFoundData();
                    return;
                }

                ShowEditForm(item);
                return;
            }

            if (!this.IsDelete)
            {
                ShowAccessDeniedNotify();
                return;
            }

            try
            {
                bool deleted = DocumentGroupManager.Instance.Delete(
                    idNhomTaiLieu);
                if (!deleted)
                {
                    ShowInvalidNotFoundData();
                    return;
                }

                ShowSuccessDeleteData();
                ResetForm();
                RebindGridFromFirstPage();
            }
            catch (InvalidOperationException exc)
            {
                ShowNotify(exc.Message, MSGType.Warning);
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected string GetStatusText(object value)
        {
            return CURRENT_PAGE.GetStatusText(value);
        }
    }
}
