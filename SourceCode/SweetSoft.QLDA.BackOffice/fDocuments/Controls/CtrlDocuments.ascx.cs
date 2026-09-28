using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.fFilesBox;
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
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static SweetSoft.QLDA.Controls.EnumHelper;

namespace SweetSoft.QLDA.BackOffice.fDocuments.Controls
{
    public partial class CtrlDocuments : BaseAdminUserControl
    {
        private const string DeleteConfirmCommand =
            "DOCUMENT_DELETE";

        /// <summary>
        /// A non-empty value switches this shared control into project mode.
        /// The page hosting the control assigns it on every request.
        /// </summary>
        public Guid ProjectId
        {
            get
            {
                object value = ViewState["ProjectId"];
                if (value is Guid)
                    return (Guid)value;

                Guid result;
                return Guid.TryParse(Convert.ToString(value), out result)
                    ? result
                    : Guid.Empty;
            }
            set { ViewState["ProjectId"] = value; }
        }

        private bool IsProjectContext
        {
            get { return ProjectId != Guid.Empty; }
        }

        private string GetAddDocumentText()
        {
            return "Thêm hồ sơ";
        }

        private TblTaiLieu GetDocumentByCurrentScope(Guid idTaiLieu)
        {
            return IsProjectContext
                ? DocumentManager.Instance.GetProjectDocumentById(
                    idTaiLieu,
                    ProjectId)
                : DocumentManager.Instance.GetAccessibleDocument(idTaiLieu);
        }

        private string GetDocumentDetailUrl(Guid idTaiLieu, Guid? documentProjectId)
        {
            return documentProjectId.HasValue
                ? RewriteURLHelper.ProjectDocumentDetail(documentProjectId.Value, idTaiLieu)
                : RewriteURLHelper.DocumentDetail(idTaiLieu);
        }

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

        protected bool CanAccessRow(object id, string action)
        {
            if (string.Equals(action, "Update", StringComparison.OrdinalIgnoreCase))
            {
                return DocumentManager.Instance.CanAccessDocument(
                    (Guid)id,
                    SweetSoft.QLDA.Core.Managers.DocumentPermissionKeys.UpdateInfo);
            }
            return DocumentManager.Instance.CanAccessDocument((Guid)id,
                (SweetSoft.QLDA.Core.Functions.ActionKeys)Enum.Parse(typeof(SweetSoft.QLDA.Core.Functions.ActionKeys),action));
        }
        protected bool IsAdd
        {
            get { return CURRENT_PAGE.IsAdd; }
        }

        protected bool IsView
        {
            get { return CURRENT_PAGE.IsView; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncButtons();
            Page.Form.Enctype = "multipart/form-data";
            ScriptManager scriptManager = ScriptManager.GetCurrent(Page);
            if (scriptManager != null)
                scriptManager.RegisterPostBackControl(btnSave);
        }

        protected override void OnPreRender(EventArgs e)
        {
            ConfigureGridLayout();
            ApplyQuickFilterState();
            base.OnPreRender(e);
        }

        public void InitControls()
        {
            ApplyControlsText();
            BindDropdowns();
            LoadSearchState();
            if (IsProjectContext)
                SelectedDocumentScope = DocumentScopeKeys.Project;
            ResetForm();
            InitGridData();
        }

        private void ConfigureGridLayout()
        {
            pnlDocumentGrid.CssClass = IsProjectContext
                ? "card-body p-0 document-project-context"
                : "card-body p-0";
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
                GetAddDocumentText();
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
            ddlCreateProject.PlaceHolder = "Chọn dự án";
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
                chkCanGuiKhachHang.OnText =
                chkCanLuuVatLy.OnText =
                    GetResourceText(BackEndResourceKeys.YES);
            chkCanTrinhKy.OffText =
                chkCanGuiKhachHang.OffText =
                chkCanLuuVatLy.OffText =
                    GetResourceText(BackEndResourceKeys.NO);

            grvData.HeaderTexts = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.DOCUMENT_CODE),
                GetResourceText(BackEndResourceKeys.DOCUMENT_NAME),
                GetResourceText(BackEndResourceKeys.DOCUMENT_SCOPE),
                "Loại hồ sơ",
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
            ddlSearchNhomTaiLieu.Items.Clear();
            ddlNhomTaiLieu.Items.Clear();
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
            BindCreateProjects();
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

            ControlHelpers controlHelpers = new ControlHelpers();
            controlHelpers.BindDocumentTypes(
                ddlSearchLoaiTaiLieu,
                (Guid?)null);
        }

        private void BindDocumentFormTypes(Guid? idNhomTaiLieu)
        {
            ddlLoaiTaiLieu.Items.Clear();

            new ControlHelpers().BindDocumentTypes(
                ddlLoaiTaiLieu,
                (Guid?)null);
            ddlLoaiTaiLieu.Enabled = true;
        }

        private void BindProjects()
        {
            if (IsProjectContext)
            {
                ddlSearchDuAn.Items.Clear();
                return;
            }

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

        private void BindCreateProjects()
        {
            ddlCreateProject.Items.Clear();
            if (DocumentManager.Instance.CanCreateCompanyDocument())
            {
                ddlCreateProject.Items.Add(new ListItem(
                    "Hồ sơ công ty (không chọn dự án)", string.Empty));
            }

            if (IsProjectContext)
                return;

            foreach (TblDuAn project in DocumentManager.Instance
                .GetProjectsAvailableForDocumentCreation())
            {
                string text = string.IsNullOrWhiteSpace(project.MaDuAn)
                    ? project.TenDuAn
                    : project.MaDuAn + " · " + project.TenDuAn;
                ddlCreateProject.Items.Add(new ListItem(
                    text, project.IdDuAn.ToString()));
            }
        }

        private void ApplyQuickFilterParameters(
            Dictionary<string, object> parameters)
        {
            if (parameters == null)
                return;
            SelectedDocumentGroupId = null;

            if (IsProjectContext)
            {
                parameters[DocumentRepository.DocumentScopeParameter] =
                    DocumentScopeKeys.Project;
                parameters[TblTaiLieu.Columns.IdDuAn] =
                    ProjectId.ToString();
                parameters[DocumentRepository.DocumentGroupParameter] =
                    SelectedDocumentGroupId.HasValue
                        ? SelectedDocumentGroupId.Value.ToString()
                        : string.Empty;
                return;
            }

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
            pnlSearchScope.Visible = !IsProjectContext;
            ddlSearchPhamVi.SelectedValue =
                IsProjectContext
                || SelectedDocumentScope == DocumentScopeKeys.All
                    ? string.Empty
                    : SelectedDocumentScope;

            pnlProjectSelector.Visible =
                !IsProjectContext
                && SelectedDocumentScope == DocumentScopeKeys.Project;

            ddlSearchNhomTaiLieu.SelectedValue =
                SelectedDocumentGroupId.HasValue
                    ? SelectedDocumentGroupId.Value.ToString()
                    : string.Empty;

            ddlSearchLoaiTaiLieu.Enabled = true;
            ddlSearchLoaiTaiLieu.Text = "Loại hồ sơ";
            ddlSearchLoaiTaiLieu.CssClass = IsProjectContext
                ? "border-top-left-radius-1 border-bottom-left-radius-1"
                : string.Empty;
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
                upMain.Update();
                return;
            }

            master.btnSearchAdvanced_Click(
                searchTagBox,
                pnlSearchDefault,
                pnlSearchPopup,
                grvData);
            upMain.Update();
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
            upMain.Update();
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

                DataTable data;
                if (IsProjectContext)
                {
                    data = DocumentManager.Instance.SearchProjectDocuments(
                        ProjectId,
                        grid.GridSearchType == GridSearchType.Single
                            ? txtSearch.Text
                            : string.Empty,
                        searchParameters,
                        orderBy,
                        rowOffset,
                        endRow,
                        out totalRows);
                }
                else
                {
                    data = grid.GridSearchType == GridSearchType.Single
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

        private string DecodeDocumentContent()
        {
            string encoded = hdfDocumentContent.Value ?? string.Empty;
            if (encoded.Length > 1100000)
                throw new ArgumentException("Nội dung hồ sơ quá dài.");
            try { return new System.Text.UTF8Encoding(false, true).GetString(Convert.FromBase64String(encoded)); }
            catch (FormatException) { throw new ArgumentException("Nội dung hồ sơ không hợp lệ. Vui lòng mở lại form."); }
            catch (System.Text.DecoderFallbackException) { throw new ArgumentException("Mã hóa nội dung hồ sơ không hợp lệ."); }
        }

        private void ResetForm()
        {
            hdfIdTaiLieu.Value = string.Empty;
            pnlCreateProject.Visible = !IsProjectContext;
            pnlCreateUnavailable.Visible = false;
            btnSave.Enabled = true;
            pnlInitialFileUpload.Visible = true;
            SelectDropdownValue(ddlCreateProject, string.Empty);
            txtMaTaiLieu.Text = string.Empty;
            txtTenTaiLieu.Text = string.Empty;
            txtMoTa.Text = string.Empty;
            hdfDocumentContent.Value = string.Empty;
            SelectDropdownValue(ddlNhomTaiLieu, string.Empty);
            BindDocumentFormTypes(null);
            SelectDropdownValue(ddlNguoiPhuTrach, string.Empty);
            SelectDropdownValue(
                ddlHinhThucKy,
                DocumentSigningMethodKeys.Paper);
            chkCanTrinhKy.Checked = false;
            chkCanGuiKhachHang.Checked = false;
            chkCanLuuVatLy.Checked = false;
            pnlInitialContent.Visible = true;
            rbInitialUpload.Checked = true;
            rbInitialTemplate.Checked = false;
            BindInitialTemplates(Guid.Empty);
            ApplyInitialSourceState();
        }

        private void ShowAddForm()
        {
            BindCreateProjects();
            ResetForm();
            pnlCreateUnavailable.Visible = !IsProjectContext
                && ddlCreateProject.Items.Count == 0;
            btnSave.Enabled = !pnlCreateUnavailable.Visible;
            dlDetail.Title = GetAddDocumentText();
            dlDetail.OpenModal(true);
        }

        private void ShowEditForm(TblTaiLieu item)
        {
            hdfIdTaiLieu.Value = item.IdTaiLieu.ToString();
            pnlCreateProject.Visible = false;
            pnlInitialFileUpload.Visible = false;
            txtMaTaiLieu.Text = item.MaTaiLieu;
            txtTenTaiLieu.Text = item.TenTaiLieu;
            txtMoTa.Text = item.MoTa;
            hdfDocumentContent.Value = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
                DocumentManager.Instance.GetDocumentContent(item.IdTaiLieu)));
            BindDocumentFormTypes(null);
            SelectDropdownValue(
                ddlLoaiTaiLieu,
                item.IdLoaiTaiLieu.ToString());
            SelectDropdownValue(
                ddlNguoiPhuTrach,
                item.IdNhanVienPhuTrach.HasValue
                    ? item.IdNhanVienPhuTrach.Value.ToString()
                    : string.Empty);
            chkCanTrinhKy.Checked = item.CanTrinhKy;
            chkCanGuiKhachHang.Checked = item.CanGuiKhachHang;
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
                chkCanGuiKhachHang.Checked = false;
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
            chkCanGuiKhachHang.Checked = documentType.CanGuiKhachHang;
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
            rbInitialTemplate.Checked = false;
            rbInitialUpload.Checked = true;
            ApplyInitialSourceState();
        }

        private void ApplyInitialSourceState()
        {
            pnlInitialContent.Visible = false;
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
            if (IsProjectContext)
            {
                SelectedDocumentScope = DocumentScopeKeys.Project;
                ApplyActiveSearch();
                return;
            }

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

        private void ShowSaveWarning(string message)
        {
            string fileHint = fuInitialFiles.HasFiles
                ? " Nếu đã chọn file, vui lòng chọn lại trước khi lưu."
                : string.Empty;
            ShowNotify(message + fileHint, MSGType.Warning);
            dlDetail.OpenModal(true);
        }

        private string UploadInitialFiles(
            Guid documentId,
            IList<HttpPostedFile> files,
            SecureFileUploadHandler uploader)
        {
            if (files.Count == 0)
                return null;

            var uploadedIds = new List<Guid>();
            string warning = null;
            foreach (HttpPostedFile file in files)
            {
                SecureFileUploadHandler.UploadResult result;
                try
                {
                    result = uploader.UploadDocumentVersionFile(documentId, file);
                }
                catch (Exception exc)
                {
                    SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(
                        exc, "Initial document file upload failed");
                    warning = "Một file chưa tải lên do lỗi máy chủ.";
                    break;
                }
                if (!result.Success || !result.FileId.HasValue)
                {
                    warning = "Một file chưa tải lên: " + result.Message;
                    break;
                }

                uploadedIds.Add(result.FileId.Value);
            }

            if (uploadedIds.Count > 0)
            {
                try
                {
                    DocumentManager.Instance.SaveDocumentFileSet(
                        documentId, null, uploadedIds);
                }
                catch (Exception exc)
                {
                    SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(
                        exc, "Save initial document file set failed");
                    return "Hồ sơ đã được tạo nhưng các file chưa được gắn vào hồ sơ. "
                        + "Vui lòng mở hồ sơ để tải lại file.";
                }
            }

            return warning == null
                ? null
                : warning + " Hồ sơ đã được tạo; vui lòng mở hồ sơ để tải lại file còn thiếu.";
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Guid idTaiLieu = Guid.Empty;
            if (!string.IsNullOrEmpty(hdfIdTaiLieu.Value)
                && !Guid.TryParse(hdfIdTaiLieu.Value, out idTaiLieu))
            {
                ShowSaveWarning("Thông tin hồ sơ không hợp lệ.");
                return;
            }

            bool isNew = idTaiLieu == Guid.Empty;
            if (isNew && !this.IsAdd)
            {
                ShowAccessDeniedNotify();
                return;
            }

            if (!isNew && !DocumentManager.Instance.CanAccessDocument(
                idTaiLieu,
                SweetSoft.QLDA.Core.Managers.DocumentPermissionKeys.UpdateInfo))
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
                ShowSaveWarning("Vui lòng chọn loại hồ sơ.");
                return;
            }

            TblLoaiTaiLieu selectedDocumentType =
                DocumentManager.Instance
                    .GetDocumentTypeDefaults(idLoaiTaiLieu);
            if (selectedDocumentType == null)
            {
                ShowSaveWarning("Loại hồ sơ không tồn tại hoặc đã khóa.");
                return;
            }

            Guid? selectedProjectId;
            if (isNew)
            {
                selectedProjectId = IsProjectContext
                    ? (Guid?)ProjectId
                    : null;
            }
            else
            {
                TblTaiLieu existingDocument = GetDocumentByCurrentScope(idTaiLieu);
                if (existingDocument == null)
                {
                    ShowInvalidNotFoundData();
                    return;
                }

                selectedProjectId = existingDocument.IdDuAn;
            }

            if (isNew && !IsProjectContext
                && !string.IsNullOrWhiteSpace(ddlCreateProject.SelectedValue))
            {
                Guid projectId;
                if (!Guid.TryParse(ddlCreateProject.SelectedValue, out projectId)
                    || projectId == Guid.Empty
                    || ddlCreateProject.Items.FindByValue(
                        ddlCreateProject.SelectedValue) == null)
                {
                    ShowSaveWarning("Vui lòng chọn dự án hợp lệ.");
                    return;
                }

                selectedProjectId = projectId;
            }

            if (isNew && !selectedProjectId.HasValue
                && !DocumentManager.Instance.CanCreateCompanyDocument())
            {
                ShowSaveWarning("Bạn chưa có quyền tạo hồ sơ công ty. Vui lòng chọn dự án mà bạn làm PM.");
                return;
            }

            var initialFiles = new List<HttpPostedFile>();
            SecureFileUploadHandler uploader = null;
            if (isNew)
            {
                initialFiles = fuInitialFiles.PostedFiles
                    .Cast<HttpPostedFile>()
                    .Where(file => file != null
                        && !string.IsNullOrWhiteSpace(file.FileName))
                    .ToList();
                if (initialFiles.Count > 10)
                {
                    ShowSaveWarning("Chỉ được chọn tối đa 10 file cho một hồ sơ.");
                    return;
                }

                if (initialFiles.Count > 0)
                {
                    bool canUploadInitialFiles = selectedProjectId.HasValue
                        ? DocumentManager.Instance.CanAccessProjectDocument(
                            selectedProjectId.Value,
                            SweetSoft.QLDA.Core.Functions.ActionKeys.Update)
                        : DocumentManager.Instance.CanUpdateCompanyDocument();
                    if (!canUploadInitialFiles)
                    {
                        ShowSaveWarning("Bạn cần quyền Cập nhật ở phạm vi đã chọn để gắn file khi tạo hồ sơ.");
                        return;
                    }
                    uploader = new SecureFileUploadHandler();
                    foreach (HttpPostedFile file in initialFiles)
                    {
                        SecureFileUploadHandler.UploadResult validation =
                            uploader.ValidateDocumentVersionFile(file);
                        if (!validation.Success)
                        {
                            ShowSaveWarning("File đã chọn không hợp lệ: "
                                + validation.Message);
                            return;
                        }
                    }
                }
            }

            Guid? idNhanVienPhuTrach = null;
            string employeeValue = ddlNguoiPhuTrach.SelectedValue;
            if (!string.IsNullOrWhiteSpace(employeeValue))
            {
                Guid employeeId;
                if (!Guid.TryParse(employeeValue, out employeeId))
                {
                    ShowSaveWarning("Người phụ trách không hợp lệ.");
                    return;
                }

                idNhanVienPhuTrach = employeeId;
            }

            try
            {
                TblTaiLieu savedItem = selectedProjectId.HasValue
                    ? DocumentManager.Instance.SaveProjectDocument(
                        selectedProjectId.Value,
                        idTaiLieu,
                        idLoaiTaiLieu,
                        idNhanVienPhuTrach,
                        txtMaTaiLieu.Text,
                        txtTenTaiLieu.Text,
                        txtMoTa.Text,
                        chkCanTrinhKy.Checked,
                        ddlHinhThucKy.SelectedValue,
                        chkCanGuiKhachHang.Checked,
                        chkCanLuuVatLy.Checked, DecodeDocumentContent())
                    : DocumentManager.Instance.SaveCompanyDocument(
                        idTaiLieu,
                        idLoaiTaiLieu,
                        idNhanVienPhuTrach,
                        txtMaTaiLieu.Text,
                        txtTenTaiLieu.Text,
                        txtMoTa.Text,
                        chkCanTrinhKy.Checked,
                        ddlHinhThucKy.SelectedValue,
                        chkCanGuiKhachHang.Checked,
                        chkCanLuuVatLy.Checked, DecodeDocumentContent());

                if (isNew)
                {
                    string uploadWarning = UploadInitialFiles(
                        savedItem.IdTaiLieu, initialFiles, uploader);
                    if (!string.IsNullOrEmpty(uploadWarning))
                        Session["DocumentInitialUploadWarning_"
                            + savedItem.IdTaiLieu.ToString("N")] = uploadWarning;
                    else
                        Session["DocumentCreationSuccess_"
                            + savedItem.IdTaiLieu.ToString("N")] =
                            initialFiles.Count == 0
                                ? "Đã tạo hồ sơ."
                                : "Đã tạo hồ sơ và gắn "
                                    + initialFiles.Count + " file.";
                    Response.Redirect(
                        (selectedProjectId.HasValue
                            ? RewriteURLHelper.ProjectDocumentDetail(
                                selectedProjectId.Value,
                                savedItem.IdTaiLieu)
                            : RewriteURLHelper.DocumentDetail(
                                savedItem.IdTaiLieu))
                        + "?tab=versions",
                        false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
                else
                    ShowSuccessSaveData();

                ResetForm();
                dlDetail.CloseModal(true);
                RebindGridFromFirstPage();
            }
            catch (ArgumentException exc)
            {
                ShowSaveWarning(exc.Message);
            }
            catch (InvalidOperationException exc)
            {
                ShowSaveWarning(exc.Message);
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
                dlDetail.OpenModal(true);
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

                TblTaiLieu viewItem = DocumentManager.Instance.GetAccessibleDocument(idTaiLieu);
                if (viewItem == null)
                {
                    ShowInvalidNotFoundData();
                    return;
                }

                Response.Redirect(
                    GetDocumentDetailUrl(idTaiLieu, viewItem.IdDuAn));
                return;
            }

            if (e.CommandName == "EDIT_ITEM")
            {
                if (!DocumentManager.Instance.CanAccessDocument(
                        idTaiLieu,
                        SweetSoft.QLDA.Core.Managers.DocumentPermissionKeys.UpdateInfo))
                {
                    ShowAccessDeniedNotify();
                    return;
                }

                TblTaiLieu item = GetDocumentByCurrentScope(idTaiLieu);
                if (item == null)
                {
                    ShowInvalidNotFoundData();
                    return;
                }

                ShowEditForm(item);
                return;
            }

            if (!DocumentManager.Instance.CanAccessDocument(
                    idTaiLieu,
                    SweetSoft.QLDA.Core.Managers.DocumentPermissionKeys.Delete))
            {
                ShowAccessDeniedNotify();
                return;
            }

            TblTaiLieu deleteItem = GetDocumentByCurrentScope(idTaiLieu);
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
                if (!DocumentManager.Instance.CanAccessDocument(
                    idTaiLieu,
                    SweetSoft.QLDA.Core.Managers.DocumentPermissionKeys.Delete))
                {
                    ShowAccessDeniedNotify();
                    return;
                }

                TblTaiLieu deleteItem = DocumentManager.Instance
                    .GetAccessibleDocument(idTaiLieu);
                if (deleteItem == null)
                {
                    ShowInvalidNotFoundData();
                    return;
                }

                bool deleted = deleteItem.IdDuAn.HasValue
                    ? DocumentManager.Instance.DeleteProjectDocument(
                        idTaiLieu,
                        deleteItem.IdDuAn.Value)
                    : DocumentManager.Instance.DeleteCompanyDocument(idTaiLieu);
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
            return documentTypeName;
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
