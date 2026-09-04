using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
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
    public partial class CtrlDocuments : BaseAdminUserControl
    {
        private const string DeleteConfirmCommand =
            "DOCUMENT_DELETE";

        private string SelectedDocumentScope
        {
            get
            {
                string value = Convert.ToString(
                    ViewState["SelectedDocumentScope"]);
                if (value == DocumentScopeKeys.Company
                    || value == DocumentScopeKeys.Project)
                {
                    return value;
                }

                return DocumentScopeKeys.All;
            }
            set
            {
                ViewState["SelectedDocumentScope"] = value;
            }
        }

        private Guid? SelectedDocumentGroupId
        {
            get
            {
                Guid value;
                return Guid.TryParse(
                    Convert.ToString(
                        ViewState["SelectedDocumentGroupId"]),
                    out value)
                    && value != Guid.Empty
                        ? (Guid?)value
                        : null;
            }
            set
            {
                ViewState["SelectedDocumentGroupId"] =
                    value.HasValue
                        ? value.Value.ToString()
                        : string.Empty;
            }
        }

        protected bool IsAdd
        {
            get { return CURRENT_PAGE.IsAdd; }
        }

        protected bool IsView
        {
            get { return CURRENT_PAGE.IsView; }
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

        protected override void OnPreRender(EventArgs e)
        {
            ApplyQuickFilterState();
            base.OnPreRender(e);
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
            scriptManager.RegisterAsyncPostBackControl(ddlSearchPhamVi);
            scriptManager.RegisterAsyncPostBackControl(
                ddlSearchNhomTaiLieu);
            scriptManager.RegisterAsyncPostBackControl(ddlSearchDuAn);
            scriptManager.RegisterAsyncPostBackControl(
                ddlSearchLoaiTaiLieu);
            scriptManager.RegisterAsyncPostBackControl(
                ddlSearchTrangThai);
            scriptManager.RegisterAsyncPostBackControl(ddlNhomTaiLieu);
            scriptManager.RegisterAsyncPostBackControl(ddlLoaiTaiLieu);
            scriptManager.RegisterAsyncPostBackControl(rbInitialUpload);
            scriptManager.RegisterAsyncPostBackControl(rbInitialTemplate);
            scriptManager.RegisterAsyncPostBackControl(
                btnRestoreTypeDefaults);
        }

        private void ApplyControlsText()
        {
            txtSearch.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.KEYWORD);
            ddlSearchTrangThai.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.DOCUMENT_STATUS);
            ddlSearchLoaiTaiLieu.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.DOCUMENT_TYPE);
            ddlSearchDuAn.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.PROJECT);
            ddlSearchPhamVi.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.DOCUMENT_SCOPE);
            ddlSearchNhomTaiLieu.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.DOCUMENT_GROUP);
            txtSearchMaTaiLieu.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.DOCUMENT_CODE);
            txtSearchTenTaiLieu.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.DOCUMENT_NAME);
            ddlSearchNguoiPhuTrach.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.RESPONSIBLE_EMPLOYEE);
            txtSearchMoTa.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.DESCRIPTION);
            ddlSearchCanTrinhKy.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.ALLOW_SIGNING);
            ddlSearchHinhThucKy.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.SIGNING_METHOD);
            ddlSearchCanGuiKhachHang.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.ALLOW_SEND_CUSTOMER);
            ddlSearchTrangThaiGuiKhach.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.CUSTOMER_SEND_STATUS);
            ddlSearchCanLuuVatLy.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.ALLOW_PHYSICAL_STORAGE);
            ddlSearchTrangThaiLuuTru.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.PHYSICAL_STORAGE_STATUS);
            ddlSearchHasOfficialFile.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.OFFICIAL_FILE);
            dtSearchNgayTao.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.CREATED_DATE);

            btnSearch.ToolTip = btnSearch.Text =
                GetResourceText(BackEndResourceKeys.SEARCH);
            btnAdd.ToolTip = btnAdd.Text =
                GetResourceText(BackEndResourceKeys.ADD_COMPANY_DOCUMENT);
            btnSave.ToolTip = btnSave.Text =
                GetResourceText(BackEndResourceKeys.SAVE);
            btnCancel.ToolTip = btnCancel.Text =
                GetResourceText(BackEndResourceKeys.CANCEL);
            dlDetail.CloseText =
                GetResourceText(BackEndResourceKeys.CLOSE);
            btnSearchAdvanced.ToolTip = btnSearchAdvanced.Text =
                GetResourceText(BackEndResourceKeys.SEARCH);
            btnResetSearch.ToolTip = btnResetSearch.Text =
                GetResourceText(BackEndResourceKeys.REFRESH);
            btnRestoreTypeDefaults.ToolTip =
                btnRestoreTypeDefaults.Text =
                    GetResourceText(
                        BackEndResourceKeys
                            .RESTORE_DOCUMENT_TYPE_DEFAULTS);

            ddlSearchTrangThai.Text =
                GetResourceText(BackEndResourceKeys.DOCUMENT_STATUS);
            ddlSearchLoaiTaiLieu.Text =
                GetResourceText(BackEndResourceKeys.DOCUMENT_TYPE);
            ddlSearchLoaiTaiLieu.SearchPlaceholder =
                GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            ddlSearchLoaiTaiLieu.NoResultsText =
                GetResourceText(BackEndResourceKeys.NO_DATA);
            ddlSearchLoaiTaiLieu.ClearText =
                GetResourceText(BackEndResourceKeys.ALL);
            ddlSearchDuAn.Text =
                GetResourceText(BackEndResourceKeys.ALL_PROJECTS);
            ddlSearchDuAn.ClearText =
                GetResourceText(BackEndResourceKeys.ALL_PROJECTS);
            ddlSearchDuAn.SearchPlaceholder =
                GetResourceText(BackEndResourceKeys.SELECT_PROJECT);
            ddlSearchDuAn.NoResultsText =
                GetResourceText(BackEndResourceKeys.NO_DATA);
            ddlSearchPhamVi.Text =
                GetResourceText(BackEndResourceKeys.ALL_DOCUMENTS);
            ddlSearchNhomTaiLieu.Text =
                GetResourceText(BackEndResourceKeys.SELECT_DOCUMENT_GROUP);
            ddlSearchNhomTaiLieu.ClearText =
                GetResourceText(BackEndResourceKeys.ALL);
            ddlSearchNhomTaiLieu.SearchPlaceholder =
                GetResourceText(BackEndResourceKeys.SELECT_DOCUMENT_GROUP);
            ddlSearchNhomTaiLieu.NoResultsText =
                GetResourceText(BackEndResourceKeys.NO_DATA);
            txtSearch.PlaceHolder =
                txtSearchMaTaiLieu.PlaceHolder =
                txtSearchTenTaiLieu.PlaceHolder =
                txtSearchMoTa.PlaceHolder =
                GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            ddlLoaiTaiLieu.PlaceHolder =
                GetResourceText(BackEndResourceKeys.SELECT_DOCUMENT_TYPE);
            ddlNhomTaiLieu.PlaceHolder =
                GetResourceText(BackEndResourceKeys.SELECT_DOCUMENT_GROUP);
            ddlNguoiPhuTrach.PlaceHolder =
                GetResourceText(
                    BackEndResourceKeys.SELECT_RESPONSIBLE_EMPLOYEE);
            ddlHinhThucKy.PlaceHolder =
                GetResourceText(BackEndResourceKeys.SIGNING_METHOD);
            ddlInitialTemplate.PlaceHolder =
                GetResourceText(
                    BackEndResourceKeys.SELECT_DOCUMENT_TEMPLATE);
            rbInitialUpload.Text =
                GetResourceText(BackEndResourceKeys.UPLOAD_NEW_FILE);
            rbInitialTemplate.Text =
                GetResourceText(
                    BackEndResourceKeys.USE_DOCUMENT_TEMPLATE);

            chkCanTrinhKy.OnText =
                chkCanLuuVatLy.OnText =
                    GetResourceText(BackEndResourceKeys.YES);
            chkCanTrinhKy.OffText =
                chkCanLuuVatLy.OffText =
                    GetResourceText(BackEndResourceKeys.NO);

            grvData.HeaderTexts = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.DOCUMENT_CODE),
                GetResourceText(BackEndResourceKeys.DOCUMENT_NAME),
                GetResourceText(BackEndResourceKeys.DOCUMENT_SCOPE),
                GetResourceText(BackEndResourceKeys.DOCUMENT_TYPE),
                GetResourceText(BackEndResourceKeys.RESPONSIBLE_EMPLOYEE),
                GetResourceText(BackEndResourceKeys.DOCUMENT_STATUS),
                GetResourceText(BackEndResourceKeys.ALLOW_SIGNING),
                GetResourceText(BackEndResourceKeys.ALLOW_SEND_CUSTOMER),
                GetResourceText(BackEndResourceKeys.ALLOW_PHYSICAL_STORAGE),
                GetResourceText(BackEndResourceKeys.OFFICIAL_FILE),
                GetResourceText(BackEndResourceKeys.CREATED_DATE),
                GetResourceText(BackEndResourceKeys.ACTION)
            };

            txtSearch.EnterSubmitClientID = btnSearch.ClientID;
            btnAdd.Visible = this.IsAdd;
        }

        private void BindDropdowns()
        {
            ControlHelpers controlHelpers = new ControlHelpers();
            BindDocumentScopes();
            controlHelpers.BindDocumentGroups(ddlSearchNhomTaiLieu);
            controlHelpers.BindDocumentGroups(ddlNhomTaiLieu);
            controlHelpers.BindDocumentStatuses(ddlSearchTrangThai);
            BindQuickDocumentTypes();
            BindDocumentFormTypes(null);
            controlHelpers.BindDocumentSigningMethods(ddlHinhThucKy);
            controlHelpers.BindStatusYesNo(ddlSearchCanTrinhKy, true);
            controlHelpers.BindDocumentSigningMethods(
                ddlSearchHinhThucKy,
                true);
            controlHelpers.BindStatusYesNo(
                ddlSearchCanGuiKhachHang,
                true);
            controlHelpers.BindDocumentCustomerStatuses(
                ddlSearchTrangThaiGuiKhach,
                true);
            controlHelpers.BindStatusYesNo(
                ddlSearchCanLuuVatLy,
                true);
            controlHelpers.BindDocumentPhysicalStorageStatuses(
                ddlSearchTrangThaiLuuTru,
                true);
            controlHelpers.BindStatusYesNo(
                ddlSearchHasOfficialFile,
                true);
            BindProjects();
            BindEmployees();
        }

        private void BindDocumentScopes()
        {
            ddlSearchPhamVi.Items.Clear();
            ddlSearchPhamVi.AddItem(
                GetResourceText(BackEndResourceKeys.ALL_DOCUMENTS),
                string.Empty);
            ddlSearchPhamVi.AddItem(
                GetResourceText(BackEndResourceKeys.COMPANY_DOCUMENTS),
                DocumentScopeKeys.Company);
            ddlSearchPhamVi.AddItem(
                GetResourceText(BackEndResourceKeys.PROJECT_DOCUMENTS),
                DocumentScopeKeys.Project);
            ddlSearchPhamVi.SelectedValue =
                SelectedDocumentScope == DocumentScopeKeys.All
                    ? string.Empty
                    : SelectedDocumentScope;
        }

        private void BindQuickDocumentTypes()
        {
            ddlSearchLoaiTaiLieu.Items.Clear();
            ddlSearchLoaiTaiLieu.ClearSelection();

            if (!SelectedDocumentGroupId.HasValue)
                return;

            ControlHelpers controlHelpers = new ControlHelpers();
            controlHelpers.BindDocumentTypes(
                ddlSearchLoaiTaiLieu,
                SelectedDocumentGroupId);
        }

        private void BindDocumentFormTypes(Guid? idNhomTaiLieu)
        {
            ddlLoaiTaiLieu.Items.Clear();

            if (!idNhomTaiLieu.HasValue
                || idNhomTaiLieu.Value == Guid.Empty)
            {
                ddlLoaiTaiLieu.Items.Add(
                    new ListItem(
                        GetResourceText(
                            BackEndResourceKeys
                                .SELECT_DOCUMENT_GROUP_FIRST),
                        string.Empty));
                ddlLoaiTaiLieu.SelectedIndex = 0;
                ddlLoaiTaiLieu.Enabled = false;
                return;
            }

            new ControlHelpers().BindDocumentTypes(
                ddlLoaiTaiLieu,
                idNhomTaiLieu);
            ddlLoaiTaiLieu.Enabled = true;
        }

        private void BindProjects()
        {
            List<TblDuAn> projects =
                DocumentManager.Instance.GetAvailableProjects()
                ?? new List<TblDuAn>();

            ddlSearchDuAn.Items.Clear();
            foreach (TblDuAn project in projects)
            {
                string text = string.IsNullOrWhiteSpace(project.MaDuAn)
                    ? project.TenDuAn
                    : project.MaDuAn + " · " + project.TenDuAn;
                ddlSearchDuAn.AddItem(
                    text,
                    project.IdDuAn.ToString());
            }

            ddlSearchDuAn.ClearSelection();
        }

        private void ApplyQuickFilterParameters(
            Dictionary<string, object> parameters)
        {
            if (parameters == null)
                return;

            parameters[DocumentRepository.DocumentScopeParameter] =
                SelectedDocumentScope;
            parameters[DocumentRepository.DocumentGroupParameter] =
                SelectedDocumentGroupId.HasValue
                    ? SelectedDocumentGroupId.Value.ToString()
                    : string.Empty;

            if (SelectedDocumentScope != DocumentScopeKeys.Project)
            {
                parameters[TblTaiLieu.Columns.IdDuAn] = string.Empty;
            }
        }

        private void ApplyQuickFilterState()
        {
            ddlSearchPhamVi.SelectedValue =
                SelectedDocumentScope == DocumentScopeKeys.All
                    ? string.Empty
                    : SelectedDocumentScope;

            pnlProjectSelector.Visible =
                SelectedDocumentScope == DocumentScopeKeys.Project;

            ddlSearchNhomTaiLieu.SelectedValue =
                SelectedDocumentGroupId.HasValue
                    ? SelectedDocumentGroupId.Value.ToString()
                    : string.Empty;

            bool hasSelectedGroup = SelectedDocumentGroupId.HasValue;
            ddlSearchLoaiTaiLieu.Enabled = hasSelectedGroup;
            ddlSearchLoaiTaiLieu.Text = hasSelectedGroup
                ? GetResourceText(BackEndResourceKeys.DOCUMENT_TYPE)
                : GetResourceText(
                    BackEndResourceKeys.SELECT_DOCUMENT_GROUP_FIRST);
        }

        private void ApplyActiveSearch()
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
                return;
            }

            master.btnSearchAdvanced_Click(
                searchTagBox,
                pnlSearchDefault,
                pnlSearchPopup,
                grvData);
        }

        private void BindEmployees()
        {
            List<AspnetUser> employees =
                DocumentManager.Instance.GetAvailableEmployees()
                ?? new List<AspnetUser>();

            ddlNguoiPhuTrach.Items.Clear();
            ddlNguoiPhuTrach.Items.Add(new ListItem(
                "-- "
                + GetResourceText(
                    BackEndResourceKeys.SELECT_RESPONSIBLE_EMPLOYEE)
                + " --",
                string.Empty));

            ddlSearchNguoiPhuTrach.Items.Clear();
            ddlSearchNguoiPhuTrach.DefaultSearchValue = string.Empty;
            ddlSearchNguoiPhuTrach.AlowClear = true;
            ddlSearchNguoiPhuTrach.Items.Add(new ListItem(
                GetResourceText(BackEndResourceKeys.ALL),
                string.Empty));

            foreach (AspnetUser employee in employees)
            {
                string value = employee.UserId.ToString();
                ddlNguoiPhuTrach.Items.Add(
                    new ListItem(employee.DisplayName, value));
                ddlSearchNguoiPhuTrach.Items.Add(
                    new ListItem(employee.DisplayName, value));
            }

            ddlSearchNguoiPhuTrach.SelectedIndex = -1;
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
            grvData.CurrentSortExpression = TblTaiLieu.Columns.NgayTao;
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

                if (grid.GridSearchType != GridSearchType.Single)
                {
                    Dictionary<string, object> advancedParameters =
                        controlHelpers.GetControlValues(pnlSearchPopup);
                    foreach (KeyValuePair<string, object> parameter
                        in advancedParameters)
                    {
                        searchParameters[parameter.Key] = parameter.Value;
                    }
                }

                ApplyQuickFilterParameters(searchParameters);

                DataTable data = grid.GridSearchType == GridSearchType.Single
                    ? DocumentManager.Instance.SearchDocuments(
                        txtSearch.Text,
                        searchParameters,
                        orderBy,
                        rowOffset,
                        endRow,
                        out totalRows)
                    : DocumentManager.Instance.SearchDocuments(
                        searchParameters,
                        orderBy,
                        rowOffset,
                        endRow,
                        out totalRows);

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

        private void ResetForm()
        {
            hdfIdTaiLieu.Value = string.Empty;
            txtMaTaiLieu.Text = string.Empty;
            txtTenTaiLieu.Text = string.Empty;
            txtMoTa.Text = string.Empty;
            SelectDropdownValue(ddlNhomTaiLieu, string.Empty);
            BindDocumentFormTypes(null);
            SelectDropdownValue(ddlNguoiPhuTrach, string.Empty);
            SelectDropdownValue(
                ddlHinhThucKy,
                DocumentSigningMethodKeys.Paper);
            chkCanTrinhKy.Checked = false;
            chkCanLuuVatLy.Checked = false;
            pnlInitialContent.Visible = true;
            rbInitialUpload.Checked = true;
            rbInitialTemplate.Checked = false;
            BindInitialTemplates(Guid.Empty);
            ApplyInitialSourceState();
        }

        private void ShowAddForm()
        {
            ResetForm();
            dlDetail.Title =
                GetResourceText(
                    BackEndResourceKeys.ADD_COMPANY_DOCUMENT);
            dlDetail.OpenModal(true);
        }

        private void ShowEditForm(TblTaiLieu item)
        {
            hdfIdTaiLieu.Value = item.IdTaiLieu.ToString();
            txtMaTaiLieu.Text = item.MaTaiLieu;
            txtTenTaiLieu.Text = item.TenTaiLieu;
            txtMoTa.Text = item.MoTa;
            TblLoaiTaiLieu documentType = DocumentManager.Instance
                .GetDocumentTypeDefaults(item.IdLoaiTaiLieu);
            Guid? documentGroupId = documentType == null
                ? (Guid?)null
                : documentType.IdNhomTaiLieu;
            SelectDropdownValue(
                ddlNhomTaiLieu,
                documentGroupId.HasValue
                    ? documentGroupId.Value.ToString()
                    : string.Empty);
            BindDocumentFormTypes(documentGroupId);
            SelectDropdownValue(
                ddlLoaiTaiLieu,
                item.IdLoaiTaiLieu.ToString());
            SelectDropdownValue(
                ddlNguoiPhuTrach,
                item.IdNhanVienPhuTrach.HasValue
                    ? item.IdNhanVienPhuTrach.Value.ToString()
                    : string.Empty);
            chkCanTrinhKy.Checked = item.CanTrinhKy;
            chkCanLuuVatLy.Checked = item.CanLuuVatLy;
            SelectDropdownValue(
                ddlHinhThucKy,
                string.IsNullOrWhiteSpace(item.HinhThucKy)
                    ? DocumentSigningMethodKeys.Paper
                    : item.HinhThucKy);
            dlDetail.Title =
                GetResourceText(BackEndResourceKeys.EDIT)
                + " "
                + GetResourceText(BackEndResourceKeys.DOCUMENT);
            pnlInitialContent.Visible = false;
            dlDetail.OpenModal(true);
        }

        private void ApplySelectedTypeDefaults()
        {
            Guid idLoaiTaiLieu;
            if (!Guid.TryParse(
                    ddlLoaiTaiLieu.SelectedValue,
                    out idLoaiTaiLieu)
                || idLoaiTaiLieu == Guid.Empty)
            {
                chkCanTrinhKy.Checked = false;
                chkCanLuuVatLy.Checked = false;
                BindInitialTemplates(Guid.Empty);
                SelectDropdownValue(
                    ddlHinhThucKy,
                    DocumentSigningMethodKeys.Paper);
                return;
            }

            TblLoaiTaiLieu documentType = DocumentManager.Instance
                .GetDocumentTypeDefaults(idLoaiTaiLieu);
            if (documentType == null)
            {
                ShowNotify(
                    "Loại tài liệu không tồn tại hoặc đã bị xóa.",
                    MSGType.Warning);
                return;
            }

            chkCanTrinhKy.Checked = documentType.CanTrinhKy;
            chkCanLuuVatLy.Checked = documentType.CanLuuVatLy;
            BindInitialTemplates(idLoaiTaiLieu);
            SelectDropdownValue(
                ddlHinhThucKy,
                string.IsNullOrWhiteSpace(
                    documentType.HinhThucKyMacDinh)
                    ? DocumentSigningMethodKeys.Paper
                    : documentType.HinhThucKyMacDinh);
        }

        private void BindInitialTemplates(Guid idLoaiTaiLieu)
        {
            ddlInitialTemplate.Items.Clear();
            ddlInitialTemplate.Items.Add(
                new ListItem(
                    GetResourceText(
                        BackEndResourceKeys.SELECT_DOCUMENT_TEMPLATE),
                    string.Empty));

            DataTable templates = idLoaiTaiLieu == Guid.Empty
                ? new DataTable()
                : DocumentTemplateManager.Instance
                    .GetAvailableTemplatesByType(idLoaiTaiLieu);
            bool hasTemplates = templates != null
                && templates.Rows.Count > 0;
            if (hasTemplates)
            {
                foreach (DataRow row in templates.Rows)
                {
                    string text = Convert.ToString(row["TenMau"]);
                    string version = Convert.ToString(
                        row["PhienBanMau"]);
                    if (!string.IsNullOrWhiteSpace(version))
                        text += " · " + version;
                    if (Convert.ToBoolean(row["LaMauMacDinh"]))
                    {
                        text += " · "
                            + GetResourceText(
                                BackEndResourceKeys.DEFAULT_TEMPLATE);
                    }

                    ddlInitialTemplate.Items.Add(
                        new ListItem(
                            text,
                            Convert.ToString(row["IdMauTaiLieu"])));
                }

                ddlInitialTemplate.SelectedIndex = 1;
            }
            else
            {
                ddlInitialTemplate.SelectedIndex = 0;
            }

            ddlInitialTemplate.Enabled = hasTemplates;
            pnlNoInitialTemplates.Visible = !hasTemplates;
            ApplyInitialSourceState();
        }

        private void ApplyInitialSourceState()
        {
            pnlInitialUploadInfo.Visible = rbInitialUpload.Checked;
            pnlInitialTemplate.Visible = rbInitialTemplate.Checked;
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

        protected void ddlSearchPhamVi_SelectedValueChanged(
            object sender,
            EventArgs e)
        {
            string scope = Convert.ToString(
                    ddlSearchPhamVi.SelectedValue)
                .ToUpperInvariant();
            if (scope != DocumentScopeKeys.Company
                && scope != DocumentScopeKeys.Project)
            {
                scope = DocumentScopeKeys.All;
            }

            SelectedDocumentScope = scope;
            if (scope != DocumentScopeKeys.Project)
                ddlSearchDuAn.ClearSelection();

            ApplyActiveSearch();
        }

        protected void ddlSearchDuAn_SelectedValueChanged(
            object sender,
            EventArgs e)
        {
            if (SelectedDocumentScope != DocumentScopeKeys.Project)
            {
                ddlSearchDuAn.ClearSelection();
                return;
            }

            ApplyActiveSearch();
        }

        protected void ddlSearchNhomTaiLieu_SelectedValueChanged(
            object sender,
            EventArgs e)
        {
            Guid groupId;
            SelectedDocumentGroupId = Guid.TryParse(
                    ddlSearchNhomTaiLieu.SelectedValue,
                    out groupId)
                && groupId != Guid.Empty
                    ? (Guid?)groupId
                    : null;

            BindQuickDocumentTypes();

            ApplyActiveSearch();
        }

        protected void bootstrapDropdown_SelectedValueChanged(
            object sender,
            EventArgs e)
        {
            ApplyActiveSearch();
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

                if (tag.Id == ddlSearchPhamVi.ID
                    || tag.Key == ddlSearchPhamVi.ClientID)
                {
                    SelectedDocumentScope = DocumentScopeKeys.All;
                    ddlSearchDuAn.ClearSelection();
                }

                if (tag.Id == ddlSearchNhomTaiLieu.ID
                    || tag.Key == ddlSearchNhomTaiLieu.ClientID)
                {
                    SelectedDocumentGroupId = null;
                    BindQuickDocumentTypes();
                }

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
            dlDetail.CloseModal(true);
        }

        protected void ddlLoaiTaiLieu_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            ApplySelectedTypeDefaults();
            dlDetail.UpdateContentModal();
        }

        protected void ddlNhomTaiLieu_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            Guid idNhomTaiLieu;
            Guid? selectedGroupId = Guid.TryParse(
                    ddlNhomTaiLieu.SelectedValue,
                    out idNhomTaiLieu)
                && idNhomTaiLieu != Guid.Empty
                    ? (Guid?)idNhomTaiLieu
                    : null;

            BindDocumentFormTypes(selectedGroupId);
            ApplySelectedTypeDefaults();
            dlDetail.UpdateContentModal();
        }

        protected void initialSource_CheckedChanged(
            object sender,
            EventArgs e)
        {
            ApplyInitialSourceState();
            dlDetail.UpdateContentModal();
        }

        protected void btnRestoreTypeDefaults_Click(
            object sender,
            EventArgs e)
        {
            ApplySelectedTypeDefaults();
            dlDetail.UpdateContentModal();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Guid idTaiLieu = Guid.Empty;
            if (!string.IsNullOrEmpty(hdfIdTaiLieu.Value)
                && !Guid.TryParse(hdfIdTaiLieu.Value, out idTaiLieu))
            {
                ShowInvalidDataError();
                return;
            }

            bool isNew = idTaiLieu == Guid.Empty;
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

            Guid idNhomTaiLieu;
            if (!Guid.TryParse(
                    ddlNhomTaiLieu.SelectedValue,
                    out idNhomTaiLieu)
                || idNhomTaiLieu == Guid.Empty)
            {
                ShowNotify(
                    GetResourceText(
                        BackEndResourceKeys.SELECT_DOCUMENT_GROUP),
                    MSGType.Warning);
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

            TblLoaiTaiLieu selectedDocumentType =
                DocumentManager.Instance
                    .GetDocumentTypeDefaults(idLoaiTaiLieu);
            if (selectedDocumentType == null
                || selectedDocumentType.IdNhomTaiLieu
                    != idNhomTaiLieu)
            {
                ShowNotify(
                    "Loại tài liệu không thuộc nhóm đã chọn.",
                    MSGType.Warning);
                return;
            }

            Guid? idNhanVienPhuTrach = null;
            string employeeValue = ddlNguoiPhuTrach.SelectedValue;
            if (!string.IsNullOrWhiteSpace(employeeValue))
            {
                Guid employeeId;
                if (!Guid.TryParse(employeeValue, out employeeId))
                {
                    ShowInvalidDataError();
                    return;
                }

                idNhanVienPhuTrach = employeeId;
            }

            Guid idMauTaiLieu = Guid.Empty;
            if (isNew && rbInitialTemplate.Checked)
            {
                if (!Guid.TryParse(
                        ddlInitialTemplate.SelectedValue,
                        out idMauTaiLieu)
                    || idMauTaiLieu == Guid.Empty)
                {
                    ShowNotify(
                        GetResourceText(
                            BackEndResourceKeys.SELECT_DOCUMENT_TEMPLATE),
                        MSGType.Warning);
                    return;
                }
            }

            try
            {
                TblTaiLieu savedItem =
                    DocumentManager.Instance.SaveCompanyDocument(
                    idTaiLieu,
                    idLoaiTaiLieu,
                    idNhanVienPhuTrach,
                    txtMaTaiLieu.Text,
                    txtTenTaiLieu.Text,
                    txtMoTa.Text,
                    chkCanTrinhKy.Checked,
                    ddlHinhThucKy.SelectedValue,
                    false,
                    chkCanLuuVatLy.Checked);

                if (isNew)
                {
                    if (idMauTaiLieu != Guid.Empty)
                    {
                        try
                        {
                            DocumentManager.Instance
                                .CreateInitialVersionFromTemplate(
                                    savedItem.IdTaiLieu,
                                    idMauTaiLieu);
                        }
                        catch
                        {
                            DocumentManager.Instance
                                .DeleteCompanyDocument(
                                    savedItem.IdTaiLieu);
                            throw;
                        }
                    }

                    CURRENT_PAGE.ShowSuccessAddNewData();
                    Response.Redirect(
                        RewriteURLHelper.DocumentDetail(
                            savedItem.IdTaiLieu)
                        + "?tab=versions",
                        false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
                else
                    ShowSuccessSaveData();

                ShowEditForm(savedItem);
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
            if (e.CommandName != "VIEW_ITEM"
                && e.CommandName != "EDIT_ITEM"
                && e.CommandName != "DELETE_ITEM")
            {
                return;
            }

            Guid idTaiLieu;
            if (!Guid.TryParse(
                    Convert.ToString(e.CommandArgument),
                    out idTaiLieu))
            {
                ShowInvalidDataError();
                return;
            }

            if (e.CommandName == "VIEW_ITEM")
            {
                if (!this.IsView)
                {
                    ShowAccessDeniedNotify();
                    return;
                }

                TblTaiLieu viewItem =
                    DocumentManager.Instance.GetCompanyDocumentById(idTaiLieu);
                if (viewItem == null)
                {
                    ShowInvalidNotFoundData();
                    return;
                }

                Response.Redirect(
                    RewriteURLHelper.DocumentDetail(idTaiLieu));
                return;
            }

            if (e.CommandName == "EDIT_ITEM")
            {
                if (!this.IsEdit)
                {
                    ShowAccessDeniedNotify();
                    return;
                }

                TblTaiLieu item =
                    DocumentManager.Instance.GetCompanyDocumentById(idTaiLieu);
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

            TblTaiLieu deleteItem =
                DocumentManager.Instance.GetCompanyDocumentById(idTaiLieu);
            if (deleteItem == null)
            {
                ShowInvalidNotFoundData();
                return;
            }

            ConfirmResult result = new ConfirmResult
            {
                CommandName = DeleteConfirmCommand,
                Value = idTaiLieu.ToString()
            };
            CURRENT_PAGE.CurrentConfirmResult = result;

            MessageBox message = new MessageBox(
                GetResourceText(BackEndResourceKeys.NOTIFICATION),
                string.Format(
                    GetResourceText(
                        BackEndResourceKeys
                            .PLEASE_CONFIRM_TO_DELETE_THE_DATA),
                    deleteItem.TenTaiLieu),
                MSGButton.DeleteCancel,
                MSGIcon.Error);
            OpenMessageBox(message, result, false, false);
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e == null
                || !e.Submit
                || !string.Equals(
                    e.CommandName,
                    DeleteConfirmCommand,
                    StringComparison.Ordinal))
            {
                return;
            }

            if (!this.IsDelete)
            {
                ShowAccessDeniedNotify();
                return;
            }

            Guid idTaiLieu;
            if (!Guid.TryParse(
                    Convert.ToString(e.Value),
                    out idTaiLieu))
            {
                ShowInvalidDataError();
                return;
            }

            try
            {
                bool deleted =
                    DocumentManager.Instance.DeleteCompanyDocument(idTaiLieu);
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

        protected bool IsCompanyDocument(object projectIdValue)
        {
            return projectIdValue == null
                || projectIdValue == DBNull.Value
                || string.IsNullOrWhiteSpace(
                    Convert.ToString(projectIdValue));
        }

        protected string GetDocumentScopeText(
            object projectIdValue,
            object projectCodeValue,
            object projectNameValue)
        {
            if (IsCompanyDocument(projectIdValue))
            {
                return GetResourceText(
                    BackEndResourceKeys.COMPANY_DOCUMENTS);
            }

            string projectCode = Convert.ToString(projectCodeValue);
            string projectName = Convert.ToString(projectNameValue);
            if (string.IsNullOrWhiteSpace(projectCode))
            {
                return string.IsNullOrWhiteSpace(projectName)
                    ? GetResourceText(BackEndResourceKeys.PROJECT)
                    : projectName;
            }

            return string.IsNullOrWhiteSpace(projectName)
                ? projectCode
                : projectCode + " · " + projectName;
        }

        protected string GetDocumentScopeCss(object projectIdValue)
        {
            return IsCompanyDocument(projectIdValue)
                ? "badge bg-secondary document-scope-badge"
                : "badge bg-info text-dark document-scope-badge";
        }

        protected string GetDocumentScopeIcon(object projectIdValue)
        {
            return IsCompanyDocument(projectIdValue)
                ? "fas fa-building me-1"
                : "fas fa-project-diagram me-1";
        }

        protected string GetResponsibleEmployeeText(object value)
        {
            string result = Convert.ToString(value);
            return string.IsNullOrWhiteSpace(result) ? "—" : result;
        }

        protected string GetDocumentStatusText(object value)
        {
            string status = Convert.ToString(value);
            if (status == DocumentStatusKeys.Drafting)
                return GetResourceText(BackEndResourceKeys.DRAFTING);
            if (status == DocumentStatusKeys.PendingSignature)
                return GetResourceText(BackEndResourceKeys.PENDING_SIGNATURE);
            if (status == DocumentStatusKeys.ChangesRequested)
                return GetResourceText(BackEndResourceKeys.CHANGES_REQUESTED);
            if (status == DocumentStatusKeys.Signed)
                return GetResourceText(BackEndResourceKeys.SIGNED);
            if (status == DocumentStatusKeys.Completed)
                return GetResourceText(BackEndResourceKeys.COMPLETED);
            return status;
        }

        protected string GetDocumentStatusCss(object value)
        {
            string status = Convert.ToString(value);
            if (status == DocumentStatusKeys.Signed
                || status == DocumentStatusKeys.Completed)
            {
                return "badge bg-success";
            }

            if (status == DocumentStatusKeys.PendingSignature)
                return "badge bg-info";
            if (status == DocumentStatusKeys.ChangesRequested)
                return "badge bg-warning text-dark";
            return "badge bg-secondary";
        }

        protected string GetSigningText(
            object requiredValue,
            object signingMethodValue)
        {
            if (!Convert.ToBoolean(requiredValue))
                return GetResourceText(BackEndResourceKeys.NO);

            string signingMethod = Convert.ToString(signingMethodValue);
            return signingMethod == DocumentSigningMethodKeys.DigitalExternal
                ? GetResourceText(
                    BackEndResourceKeys.EXTERNAL_DIGITAL_SIGNING)
                : GetResourceText(BackEndResourceKeys.PAPER_SIGNING);
        }

        protected string GetCustomerStatusText(
            object requiredValue,
            object statusValue)
        {
            if (!Convert.ToBoolean(requiredValue))
                return GetResourceText(BackEndResourceKeys.NO);

            string status = Convert.ToString(statusValue);
            if (status == DocumentCustomerStatusKeys.NotSent)
                return GetResourceText(BackEndResourceKeys.NOT_SENT);
            if (status == DocumentCustomerStatusKeys.Sent)
                return GetResourceText(BackEndResourceKeys.SENT);
            if (status == DocumentCustomerStatusKeys.WaitingForReturn)
                return GetResourceText(BackEndResourceKeys.WAITING_FOR_RETURN);
            if (status == DocumentCustomerStatusKeys.ReceivedBack)
                return GetResourceText(BackEndResourceKeys.RECEIVED_BACK);
            return status;
        }

        protected string GetPhysicalStorageStatusText(
            object requiredValue,
            object statusValue)
        {
            if (!Convert.ToBoolean(requiredValue))
                return GetResourceText(BackEndResourceKeys.NO);

            string status = Convert.ToString(statusValue);
            if (status == DocumentPhysicalStorageStatusKeys.NotStored)
                return GetResourceText(BackEndResourceKeys.NOT_STORED);
            if (status == DocumentPhysicalStorageStatusKeys.Stored)
                return GetResourceText(BackEndResourceKeys.STORED);
            if (status == DocumentPhysicalStorageStatusKeys.CheckedOut)
                return GetResourceText(BackEndResourceKeys.CHECKED_OUT);
            return status;
        }

        protected bool HasOfficialFile(object value)
        {
            return value != null
                && value != DBNull.Value
                && !string.IsNullOrWhiteSpace(Convert.ToString(value));
        }

        protected string GetOfficialFileName(
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
