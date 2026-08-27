using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.FileManager;
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
        private const string DocumentVersionSavedCallbackKey =
            "DocumentVersionSaved";

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
            scriptManager.RegisterAsyncPostBackControl(ddlLoaiTaiLieu);
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
            txtSearchMaTaiLieu.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.DOCUMENT_CODE);
            txtSearchTenTaiLieu.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.DOCUMENT_NAME);
            ddlSearchNhomTaiLieu.SearchTagItemText =
                GetResourceText(BackEndResourceKeys.DOCUMENT_GROUP);
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
                GetResourceText(BackEndResourceKeys.ADD_NEW);
            btnSave.ToolTip = btnSave.Text =
                GetResourceText(BackEndResourceKeys.SAVE);
            btnCancel.ToolTip = btnCancel.Text =
                GetResourceText(BackEndResourceKeys.CANCEL);
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

            txtSearch.PlaceHolder =
                txtSearchMaTaiLieu.PlaceHolder =
                txtSearchTenTaiLieu.PlaceHolder =
                txtSearchMoTa.PlaceHolder =
                GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            ddlLoaiTaiLieu.PlaceHolder =
                GetResourceText(BackEndResourceKeys.SELECT_DOCUMENT_TYPE);
            ddlNguoiPhuTrach.PlaceHolder =
                GetResourceText(
                    BackEndResourceKeys.SELECT_RESPONSIBLE_EMPLOYEE);
            ddlHinhThucKy.PlaceHolder =
                GetResourceText(BackEndResourceKeys.SIGNING_METHOD);

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
            controlHelpers.BindDocumentStatuses(ddlSearchTrangThai);
            controlHelpers.BindDocumentTypes(ddlSearchLoaiTaiLieu);
            controlHelpers.BindDocumentTypes(ddlLoaiTaiLieu);
            controlHelpers.BindDocumentSigningMethods(ddlHinhThucKy);
            controlHelpers.BindDocumentGroups(ddlSearchNhomTaiLieu, true);
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
            BindEmployees();
        }

        private void BindEmployees()
        {
            List<TblNhanVien> employees =
                DocumentManager.Instance.GetAvailableEmployees()
                ?? new List<TblNhanVien>();

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

            foreach (TblNhanVien employee in employees)
            {
                string value = employee.IdNhanVien.ToString();
                ddlNguoiPhuTrach.Items.Add(
                    new ListItem(employee.TenNhanVien, value));
                ddlSearchNguoiPhuTrach.Items.Add(
                    new ListItem(employee.TenNhanVien, value));
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

                DataTable data;
                if (grid.GridSearchType == GridSearchType.Single)
                {
                    data = DocumentManager.Instance.SearchCompanyDocuments(
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

                    data = DocumentManager.Instance.SearchCompanyDocuments(
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

        private void ResetForm()
        {
            hdfIdTaiLieu.Value = string.Empty;
            txtMaTaiLieu.Text = string.Empty;
            txtTenTaiLieu.Text = string.Empty;
            txtMoTa.Text = string.Empty;
            SelectDropdownValue(ddlLoaiTaiLieu, string.Empty);
            SelectDropdownValue(ddlNguoiPhuTrach, string.Empty);
            SelectDropdownValue(
                ddlHinhThucKy,
                DocumentSigningMethodKeys.Paper);
            chkCanTrinhKy.Checked = false;
            chkCanGuiKhachHang.Checked = false;
            chkCanLuuVatLy.Checked = false;
            pnlUploadPlaceholder.Visible = true;
            pnlVersionFiles.Visible = false;
            pnlVersionHistory.Visible = false;
            rptVersions.DataSource = null;
            rptVersions.DataBind();
            fbVersions.ClearData();
            pnlForm.Visible = false;
        }

        private void ShowAddForm()
        {
            ResetForm();
            litFormTitle.Text =
                GetResourceText(BackEndResourceKeys.ADD_NEW)
                + " "
                + GetResourceText(BackEndResourceKeys.DOCUMENT);
            pnlForm.Visible = true;
            upMain.Update();
        }

        private void ShowEditForm(TblTaiLieu item)
        {
            hdfIdTaiLieu.Value = item.IdTaiLieu.ToString();
            txtMaTaiLieu.Text = item.MaTaiLieu;
            txtTenTaiLieu.Text = item.TenTaiLieu;
            txtMoTa.Text = item.MoTa;
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
            litFormTitle.Text =
                GetResourceText(BackEndResourceKeys.EDIT)
                + " "
                + GetResourceText(BackEndResourceKeys.DOCUMENT);

            fbVersions.IsMultiple = true;
            fbVersions.IsEnabled = this.IsEdit;
            fbVersions.SaveDataCallbackKey =
                DocumentVersionSavedCallbackKey;
            fbVersions.LoadFile(
                item.IdTaiLieu,
                FileUploadTypes.DocumentVersion);
            BindVersionHistory(item.IdTaiLieu);
            pnlUploadPlaceholder.Visible = false;
            pnlVersionFiles.Visible = true;
            pnlForm.Visible = true;
            upMain.Update();
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
            SelectDropdownValue(
                ddlHinhThucKy,
                string.IsNullOrWhiteSpace(
                    documentType.HinhThucKyMacDinh)
                    ? DocumentSigningMethodKeys.Paper
                    : documentType.HinhThucKyMacDinh);
        }

        private void BindVersionHistory(Guid idTaiLieu)
        {
            DataTable versions =
                DocumentManager.Instance.GetDocumentVersions(idTaiLieu);
            bool hasVersions = versions != null && versions.Rows.Count > 0;
            rptVersions.DataSource = hasVersions ? versions : null;
            rptVersions.DataBind();
            pnlVersionHistory.Visible = hasVersions;
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

        protected void ddlLoaiTaiLieu_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            ApplySelectedTypeDefaults();
            upMain.Update();
        }

        protected void btnRestoreTypeDefaults_Click(
            object sender,
            EventArgs e)
        {
            ApplySelectedTypeDefaults();
            upMain.Update();
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

            Guid idLoaiTaiLieu;
            if (!Guid.TryParse(
                    ddlLoaiTaiLieu.SelectedValue,
                    out idLoaiTaiLieu)
                || idLoaiTaiLieu == Guid.Empty)
            {
                ShowNotify("Vui lòng chọn loại tài liệu.", MSGType.Warning);
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
                    chkCanGuiKhachHang.Checked,
                    chkCanLuuVatLy.Checked);

                if (isNew)
                    CURRENT_PAGE.ShowSuccessAddNewData();
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
            if (e.CommandName != "EDIT_ITEM"
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

        protected string GetVersionFileName(
            object originalFileNameValue,
            object fileNameValue)
        {
            string originalFileName =
                Convert.ToString(originalFileNameValue);
            return string.IsNullOrWhiteSpace(originalFileName)
                ? Convert.ToString(fileNameValue)
                : originalFileName;
        }

        public void HandleFileCallback(string key)
        {
            if (!string.Equals(
                    key,
                    DocumentVersionSavedCallbackKey,
                    StringComparison.Ordinal))
            {
                return;
            }

            Guid idTaiLieu;
            if (!Guid.TryParse(hdfIdTaiLieu.Value, out idTaiLieu)
                || idTaiLieu == Guid.Empty)
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
                DocumentManager.Instance.SyncDocumentVersions(idTaiLieu);
                fbVersions.LoadFile(
                    idTaiLieu,
                    FileUploadTypes.DocumentVersion);
                BindVersionHistory(idTaiLieu);
                grvData.Rebind();
                upMain.Update();
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
