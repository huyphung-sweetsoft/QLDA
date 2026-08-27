using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using static SweetSoft.QLDA.Controls.EnumHelper;

namespace SweetSoft.QLDA.BackOffice.fDocuments.Controls
{
    public partial class CtrlDocumentTemplates : BaseAdminUserControl
    {
        public const string TemplateFileBeforeSaveCallbackKey =
            "DOCUMENT_TEMPLATE_FILE_BEFORE_SAVE";
        public const string TemplateFileSavedCallbackKey =
            "DOCUMENT_TEMPLATE_FILE_SAVED";

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
            txtSearch.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.KEYWORD);
            ddlSearchStatus.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.STATUS);
            ddlSearchLoaiTaiLieu.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.DOCUMENT_TYPE);
            txtSearchTenMau.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.DOCUMENT_TEMPLATE_NAME);
            txtSearchPhienBanMau.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.TEMPLATE_VERSION);
            txtSearchMoTa.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.DESCRIPTION);
            ddlSearchLaMauMacDinh.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.DEFAULT_TEMPLATE);
            ddlSearchHasFile.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.TEMPLATE_FILE);

            btnSearch.ToolTip = btnSearch.Text =
                GetResourceText(BackEndResourceKeys.SEARCH);
            btnAdd.ToolTip = btnAdd.Text =
                GetResourceText(BackEndResourceKeys.ADD_NEW);
            btnSave.ToolTip = btnSave.Text =
                GetResourceText(BackEndResourceKeys.SAVE);
            btnCancel.ToolTip = btnCancel.Text =
                GetResourceText(BackEndResourceKeys.CANCEL);
            btnSearchAdvanced.ToolTip = btnSearchAdvanced.Text =
                GetResourceText(BackEndResourceKeys.SEARCH);
            btnResetSearch.ToolTip = btnResetSearch.Text =
                GetResourceText(BackEndResourceKeys.REFRESH);

            ddlSearchStatus.Text =
                GetResourceText(BackEndResourceKeys.STATUS);
            ddlSearchLoaiTaiLieu.Text =
                GetResourceText(BackEndResourceKeys.DOCUMENT_TYPE);
            ddlSearchLoaiTaiLieu.SearchPlaceholder =
                GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            ddlSearchLoaiTaiLieu.NoResultsText =
                GetResourceText(BackEndResourceKeys.NO_DATA);

            txtSearch.PlaceHolder =
                txtSearchTenMau.PlaceHolder =
                txtSearchPhienBanMau.PlaceHolder =
                txtSearchMoTa.PlaceHolder =
                GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            ddlLoaiTaiLieu.PlaceHolder =
                GetResourceText(BackEndResourceKeys.SELECT_DOCUMENT_TYPE);
            ddlSearchLaMauMacDinh.PlaceHolder =
                ddlSearchHasFile.PlaceHolder =
                GetResourceText(BackEndResourceKeys.SELECT_VALUE);

            chkKichHoat.OnText =
                GetResourceText(BackEndResourceKeys.ACTIVE);
            chkKichHoat.OffText =
                GetResourceText(BackEndResourceKeys.INACTIVE);
            chkLaMauMacDinh.OnText =
                GetResourceText(BackEndResourceKeys.YES);
            chkLaMauMacDinh.OffText =
                GetResourceText(BackEndResourceKeys.NO);

            grvData.HeaderTexts = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.DOCUMENT_TEMPLATE_NAME),
                GetResourceText(BackEndResourceKeys.DOCUMENT_TYPE),
                GetResourceText(BackEndResourceKeys.TEMPLATE_VERSION),
                GetResourceText(BackEndResourceKeys.DEFAULT_TEMPLATE),
                GetResourceText(BackEndResourceKeys.TEMPLATE_FILE),
                GetResourceText(BackEndResourceKeys.STATUS),
                GetResourceText(BackEndResourceKeys.ACTION)
            };

            txtSearch.EnterSubmitClientID = btnSearch.ClientID;
            btnAdd.Visible = this.IsAdd;
        }

        private void BindDropdowns()
        {
            ControlHelpers controlHelpers = new ControlHelpers();
            controlHelpers.BindStatus(ddlSearchStatus);
            controlHelpers.BindDocumentTypes(ddlSearchLoaiTaiLieu);
            controlHelpers.BindDocumentTypes(ddlLoaiTaiLieu);
            controlHelpers.BindStatusYesNo(ddlSearchLaMauMacDinh, true);
            controlHelpers.BindStatusYesNo(ddlSearchHasFile, true);
        }

        private void LoadSearchState()
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master != null)
            {
                master.LoadSessionLastSearch(
                    searchTagBox,
                    pnlSearchPopup,
                    grvData,
                    txtSearch);
            }
        }

        private void InitGridData()
        {
            grvData.CurrentPageSize =
                Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression =
                TblMauTaiLieu.Columns.NgayTao;
            grvData.CurrentSortDerection = "DESC";
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
                    (grid.CurrentPageIndex - 1)
                    * grid.CurrentPageSize;
                int endRow = rowOffset + grid.CurrentPageSize;
                ControlHelpers controlHelpers = new ControlHelpers();
                Dictionary<string, object> searchParameters =
                    controlHelpers.GetControlValues(pnlSearchDefault);
                string orderBy =
                    grid.CurrentSortExpression
                    + " "
                    + grid.CurrentSortDerection;

                DataTable data;
                if (grid.GridSearchType == GridSearchType.Single)
                {
                    data = DocumentTemplateManager.Instance
                        .SearchDocumentTemplates(
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

                    data = DocumentTemplateManager.Instance
                        .SearchDocumentTemplates(
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
                    ctrlGridviewPaging.PageIndex =
                        grid.CurrentPageIndex;
                    ctrlGridviewPaging.PageSize =
                        grid.CurrentPageSize;
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

        private void ResetForm()
        {
            hdfIdMauTaiLieu.Value = string.Empty;
            SelectDropdownValue(ddlLoaiTaiLieu, string.Empty);
            txtTenMau.Text = string.Empty;
            txtPhienBanMau.Text = "1.0";
            txtMoTa.Text = string.Empty;
            chkLaMauMacDinh.Checked = false;
            chkKichHoat.Checked = true;
            pnlTemplateFile.Visible = false;
            fbTemplate.ClearData();
            pnlForm.Visible = false;
        }

        private void ShowAddForm()
        {
            ResetForm();
            litFormTitle.Text =
                GetResourceText(BackEndResourceKeys.ADD_NEW)
                + " "
                + GetResourceText(BackEndResourceKeys.DOCUMENT_TEMPLATE);
            pnlForm.Visible = true;
            upMain.Update();
        }

        private void ShowEditForm(TblMauTaiLieu item)
        {
            hdfIdMauTaiLieu.Value = item.IdMauTaiLieu.ToString();
            SelectDropdownValue(
                ddlLoaiTaiLieu,
                item.IdLoaiTaiLieu.ToString());
            txtTenMau.Text = item.TenMau;
            txtPhienBanMau.Text = item.PhienBanMau;
            txtMoTa.Text = item.MoTa;
            chkLaMauMacDinh.Checked = item.LaMauMacDinh;
            chkKichHoat.Checked = item.KichHoat;
            litFormTitle.Text =
                GetResourceText(BackEndResourceKeys.EDIT)
                + " "
                + GetResourceText(BackEndResourceKeys.DOCUMENT_TEMPLATE);

            fbTemplate.IsMultiple = false;
            fbTemplate.BeforeSaveDataCallbackKey =
                TemplateFileBeforeSaveCallbackKey;
            fbTemplate.SaveDataCallbackKey =
                TemplateFileSavedCallbackKey;
            fbTemplate.LoadFile(
                item.IdMauTaiLieu,
                FileUploadTypes.DocumentTemplate);
            pnlTemplateFile.Visible = true;
            pnlForm.Visible = true;
            upMain.Update();
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
            Guid idMauTaiLieu = Guid.Empty;
            if (!string.IsNullOrEmpty(hdfIdMauTaiLieu.Value)
                && !Guid.TryParse(hdfIdMauTaiLieu.Value, out idMauTaiLieu))
            {
                ShowInvalidDataError();
                return;
            }

            bool isNew = idMauTaiLieu == Guid.Empty;
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

            Guid idLoaiTaiLieu;
            if (!Guid.TryParse(
                    ddlLoaiTaiLieu.SelectedValue,
                    out idLoaiTaiLieu)
                || idLoaiTaiLieu == Guid.Empty)
            {
                ShowNotify("Vui lòng chọn loại tài liệu.", MSGType.Warning);
                return;
            }

            try
            {
                TblMauTaiLieu savedItem =
                    DocumentTemplateManager.Instance.Save(
                        idMauTaiLieu,
                        idLoaiTaiLieu,
                        txtTenMau.Text,
                        txtPhienBanMau.Text,
                        txtMoTa.Text,
                        chkLaMauMacDinh.Checked,
                        chkKichHoat.Checked);

                if (isNew)
                {
                    CURRENT_PAGE.ShowSuccessAddNewData();
                    ShowEditForm(savedItem);
                }
                else
                {
                    ShowSuccessSaveData();
                    ResetForm();
                }

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

            Guid idMauTaiLieu;
            if (!Guid.TryParse(
                    Convert.ToString(e.CommandArgument),
                    out idMauTaiLieu))
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

                TblMauTaiLieu item =
                    DocumentTemplateManager.Instance.GetById(idMauTaiLieu);
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
                bool deleted =
                    DocumentTemplateManager.Instance.Delete(idMauTaiLieu);
                if (!deleted)
                {
                    ShowInvalidNotFoundData();
                    return;
                }

                ShowSuccessDeleteData();
                ResetForm();
                RebindGridFromFirstPage();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        public void HandleFileCallback(string key)
        {
            Guid idMauTaiLieu;
            if (!Guid.TryParse(
                    hdfIdMauTaiLieu.Value,
                    out idMauTaiLieu)
                || idMauTaiLieu == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            if (!this.IsEdit)
            {
                ShowAccessDeniedNotify();
                return;
            }

            try
            {
                if (string.Equals(
                        key,
                        TemplateFileBeforeSaveCallbackKey,
                        StringComparison.Ordinal))
                {
                    DocumentTemplateManager.Instance
                        .ClearTemplateFile(idMauTaiLieu);
                    return;
                }

                if (string.Equals(
                        key,
                        TemplateFileSavedCallbackKey,
                        StringComparison.Ordinal))
                {
                    DocumentTemplateManager.Instance
                        .SyncTemplateFile(idMauTaiLieu);
                    grvData.Rebind();
                    upMain.Update();
                }
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        protected string GetDocumentTypeText(
            object groupNameValue,
            object documentTypeNameValue)
        {
            string groupName = Convert.ToString(groupNameValue);
            string documentTypeName = Convert.ToString(documentTypeNameValue);
            return string.IsNullOrEmpty(groupName)
                ? documentTypeName
                : groupName + " / " + documentTypeName;
        }

        protected bool HasTemplateFile(object value)
        {
            return value != null
                && value != DBNull.Value
                && !string.IsNullOrWhiteSpace(Convert.ToString(value));
        }

        protected string GetTemplateFileName(
            object originalFileNameValue,
            object fileNameValue)
        {
            string originalFileName =
                Convert.ToString(originalFileNameValue);
            return string.IsNullOrWhiteSpace(originalFileName)
                ? Convert.ToString(fileNameValue)
                : originalFileName;
        }

        protected string GetFileUrl(object value)
        {
            return FileHelpers.IsValidPath(Convert.ToString(value));
        }

        protected string GetYesNoText(bool value)
        {
            return GetResourceText(
                value
                    ? BackEndResourceKeys.YES
                    : BackEndResourceKeys.NO);
        }

        protected string GetStatusText(object value)
        {
            return CURRENT_PAGE.GetStatusText(value);
        }

        private static void SelectDropdownValue(
            ListControl dropdown,
            string value)
        {
            ListItem item = dropdown.Items.FindByValue(value);
            if (item != null)
                dropdown.SelectedValue = item.Value;
            else if (dropdown.Items.Count > 0)
                dropdown.SelectedValue = dropdown.Items[0].Value;
        }
    }
}
