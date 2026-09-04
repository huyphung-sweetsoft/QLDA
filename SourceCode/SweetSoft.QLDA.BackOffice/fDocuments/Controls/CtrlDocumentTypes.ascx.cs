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
    public partial class CtrlDocumentTypes : BaseAdminUserControl
    {
        private const string DeleteConfirmCommand =
            "DOCUMENT_TYPE_DELETE";

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

        private void RegisterAsyncButtons()
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
            if (scriptManager == null)
                return;
            scriptManager.RegisterAsyncPostBackControl(btnSearch);
            scriptManager.RegisterAsyncPostBackControl(btnSearchAdvanced);
            scriptManager.RegisterAsyncPostBackControl(btnResetSearch);
        }

        public void InitControls()
        {
            ApplyControlsText();
            BindDropdowns();
            LoadSearchState();
            ResetForm();
            InitGridData();
        }

        private void ApplyControlsText()
        {
            txtSearch.SearchTagItemText = GetResourceText(BackEndResourceKeys.KEYWORD);
            ddlSearchStatus.SearchTagItemText = GetResourceText(BackEndResourceKeys.STATUS);
            ddlSearchNhom.SearchTagItemText = GetResourceText(BackEndResourceKeys.DOCUMENT_GROUP);
            txtSearchTenLoai.SearchTagItemText = GetResourceText(BackEndResourceKeys.DOCUMENT_TYPE_NAME);
            txtSearchMoTa.SearchTagItemText = GetResourceText(BackEndResourceKeys.DESCRIPTION);
            ddlSearchCanTrinhKy.SearchTagItemText = GetResourceText(BackEndResourceKeys.ALLOW_SIGNING);
            ddlSearchHinhThucKy.SearchTagItemText = GetResourceText(BackEndResourceKeys.DEFAULT_SIGNING_METHOD);
            ddlSearchCanGuiKhachHang.SearchTagItemText = GetResourceText(BackEndResourceKeys.ALLOW_SEND_CUSTOMER);
            ddlSearchCanLuuVatLy.SearchTagItemText = GetResourceText(BackEndResourceKeys.ALLOW_PHYSICAL_STORAGE);
            btnSearch.ToolTip = btnSearch.Text = GetResourceText(BackEndResourceKeys.SEARCH);
            btnAdd.ToolTip = btnAdd.Text = GetResourceText(BackEndResourceKeys.ADD_NEW);
            btnSave.ToolTip = btnSave.Text = GetResourceText(BackEndResourceKeys.SAVE);
            btnCancel.ToolTip = btnCancel.Text = GetResourceText(BackEndResourceKeys.CANCEL);
            btnSearchAdvanced.ToolTip = btnSearchAdvanced.Text = GetResourceText(BackEndResourceKeys.SEARCH);
            btnResetSearch.ToolTip = btnResetSearch.Text = GetResourceText(BackEndResourceKeys.REFRESH);
            ddlSearchStatus.Text = GetResourceText(BackEndResourceKeys.STATUS);
            ddlSearchNhom.Text = GetResourceText(BackEndResourceKeys.DOCUMENT_GROUP);
            ddlSearchNhom.SearchPlaceholder = GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            ddlSearchNhom.NoResultsText = GetResourceText(BackEndResourceKeys.NO_DATA);
            txtSearch.PlaceHolder = txtSearchTenLoai.PlaceHolder = txtSearchMoTa.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            ddlNhomTaiLieu.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_DOCUMENT_GROUP);
            ddlHinhThucKy.PlaceHolder = ddlSearchCanTrinhKy.PlaceHolder = ddlSearchHinhThucKy.PlaceHolder = ddlSearchCanGuiKhachHang.PlaceHolder = ddlSearchCanLuuVatLy.PlaceHolder = GetResourceText(BackEndResourceKeys.SELECT_VALUE);
            chkKichHoat.OnText = GetResourceText(BackEndResourceKeys.ACTIVE);
            chkKichHoat.OffText = GetResourceText(BackEndResourceKeys.INACTIVE);
            chkCanTrinhKy.OnText = chkCanGuiKhachHang.OnText = chkCanLuuVatLy.OnText = GetResourceText(BackEndResourceKeys.YES);
            chkCanTrinhKy.OffText = chkCanGuiKhachHang.OffText = chkCanLuuVatLy.OffText = GetResourceText(BackEndResourceKeys.NO);
            List<string> tableHeaders = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.DOCUMENT_TYPE_NAME),
                GetResourceText(BackEndResourceKeys.DOCUMENT_GROUP),
                GetResourceText(BackEndResourceKeys.ALLOW_SIGNING),
                GetResourceText(BackEndResourceKeys.ALLOW_SEND_CUSTOMER),
                GetResourceText(BackEndResourceKeys.ALLOW_PHYSICAL_STORAGE),
                GetResourceText(BackEndResourceKeys.DISPLAY_ORDER),
                GetResourceText(BackEndResourceKeys.STATUS),
                GetResourceText(BackEndResourceKeys.ACTION)
            };
            grvData.HeaderTexts = tableHeaders;
            txtSearch.EnterSubmitClientID = btnSearch.ClientID;
            btnAdd.Visible = this.IsAdd;
        }

        private void BindDropdowns()
        {
            ControlHelpers controlHelpers = new ControlHelpers();
            // Bộ lọc nhanh: BootstrapDropdown.
            controlHelpers.BindStatus(ddlSearchStatus);
            controlHelpers.BindDocumentGroups(ddlSearchNhom);
            // Dropdown trong form: ExtraDropdown.
            controlHelpers.BindDocumentGroups(ddlNhomTaiLieu);
            controlHelpers.BindDocumentSigningMethods(ddlHinhThucKy);
            controlHelpers.BindStatusYesNo(ddlSearchCanTrinhKy, true);
            controlHelpers.BindDocumentSigningMethods(ddlSearchHinhThucKy, true);
            controlHelpers.BindStatusYesNo(ddlSearchCanGuiKhachHang, true);
            controlHelpers.BindStatusYesNo(ddlSearchCanLuuVatLy, true);
        }

        private void LoadSearchState()
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master == null)
                return;
            master.LoadSessionLastSearch(searchTagBox, pnlSearchPopup, grvData, txtSearch);
        }

        private void InitGridData()
        {
            grvData.CurrentPageSize = Convert.ToInt32(SweetContext.Current.CurrentPageSize);
            grvData.CurrentSortExpression = TblLoaiTaiLieu.Columns.ThuTuHienThi;
            grvData.CurrentSortDerection = "ASC";
            grvData.Rebind();
        }

        private void RebindGridFromFirstPage()
        {
            grvData.CurrentPageIndex = 1;
            grvData.Rebind();
        }

        protected void grvData_NeedDataSource(object sender, ExtraGridEventArg e)
        {
            try
            {
                GridviewExtension grid = sender as GridviewExtension;
                if (grid == null)
                {
                    ShowInvalidDataError();
                    return;
                }

                int totalRows = 0;
                int rowOffset = (grid.CurrentPageIndex - 1) * grid.CurrentPageSize;
                int endRow = rowOffset + grid.CurrentPageSize;
                ControlHelpers controlHelpers = new ControlHelpers();
                Dictionary<string, object> searchParameters = controlHelpers.GetControlValues(pnlSearchDefault);
                string orderBy = grid.CurrentSortExpression + " " + grid.CurrentSortDerection;
                DataTable data;
                if (grid.GridSearchType == GridSearchType.Single)
                {
                    data = DocumentTypeManager.Instance.SearchDocumentTypes(txtSearch.Text, searchParameters, orderBy, rowOffset, endRow, out totalRows);
                }
                else
                {
                    Dictionary<string, object> advancedParameters = controlHelpers.GetControlValues(pnlSearchPopup);
                    foreach (KeyValuePair<string, object> parameter in advancedParameters)
                    {
                        searchParameters[parameter.Key] = parameter.Value;
                    }

                    data = DocumentTypeManager.Instance.SearchDocumentTypes(searchParameters, orderBy, rowOffset, endRow, out totalRows);
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

        protected void ctrlGridviewPaging_PageChanged(object sender, GridviewCustomPageChangeArgs e)
        {
            grvData.CurrentPageSize = e.CurrentPageSize;
            grvData.CurrentPageIndex = e.CurrentPageNumber;
            grvData.Rebind();
        }

        private void ResetForm()
        {
            hdfIdLoaiTaiLieu.Value = string.Empty;
            SelectDropdownValue(ddlNhomTaiLieu, string.Empty);
            txtTenLoai.Text = string.Empty;
            txtMoTa.Text = string.Empty;
            txtThuTuHienThi.Text = "0";
            chkCanTrinhKy.Checked = false;
            SelectDropdownValue(ddlHinhThucKy, DocumentSigningMethodKeys.Paper);
            chkCanGuiKhachHang.Checked = false;
            chkCanLuuVatLy.Checked = false;
            chkKichHoat.Checked = true;
        }

        private void ShowAddForm()
        {
            ResetForm();
            dlDetail.Title =
                GetResourceText(BackEndResourceKeys.ADD_NEW)
                + " "
                + GetResourceText(BackEndResourceKeys.DOCUMENT_TYPE);
            dlDetail.OpenModal(true);
        }

        private void ShowEditForm(TblLoaiTaiLieu item)
        {
            hdfIdLoaiTaiLieu.Value = item.IdLoaiTaiLieu.ToString();
            SelectDropdownValue(ddlNhomTaiLieu, item.IdNhomTaiLieu.ToString());
            txtTenLoai.Text = item.TenLoai;
            txtMoTa.Text = item.MoTa;
            txtThuTuHienThi.Text = item.ThuTuHienThi.ToString();
            chkCanTrinhKy.Checked = item.CanTrinhKy;
            SelectDropdownValue(ddlHinhThucKy, string.IsNullOrEmpty(item.HinhThucKyMacDinh) ? DocumentSigningMethodKeys.Paper : item.HinhThucKyMacDinh);
            chkCanGuiKhachHang.Checked = item.CanGuiKhachHang;
            chkCanLuuVatLy.Checked = item.CanLuuVatLy;
            chkKichHoat.Checked = item.KichHoat;
            dlDetail.Title =
                GetResourceText(BackEndResourceKeys.EDIT)
                + " "
                + GetResourceText(BackEndResourceKeys.DOCUMENT_TYPE);
            dlDetail.OpenModal(true);
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master == null)
            {
                RebindGridFromFirstPage();
                return;
            }

            master.btnSearchSingle_Click(searchTagBox, pnlSearchDefault, grvData, txtSearch);
        }

        protected void bootstrapDropdown_SelectedValueChanged(object sender, EventArgs e)
        {
            MasterTemplate master = Page.Master as MasterTemplate;
            if (master == null)
            {
                RebindGridFromFirstPage();
                return;
            }

            if (grvData.GridSearchType == GridSearchType.Single)
            {
                master.btnSearchSingle_Click(searchTagBox, pnlSearchDefault, grvData, txtSearch);
            }
            else
            {
                master.btnSearchAdvanced_Click(searchTagBox, pnlSearchDefault, pnlSearchPopup, grvData);
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

            master.btnSearchAdvanced_Click(searchTagBox, pnlSearchDefault, pnlSearchPopup, grvData);
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

            master.btnSearchAdvanced_Click(searchTagBox, pnlSearchDefault, pnlSearchPopup, grvData);
        }

        protected void searchTagBox_TagClosed(object sender, SearchTagItem tag)
        {
            try
            {
                MasterTemplate master = Page.Master as MasterTemplate;
                if (master == null)
                    return;
                GridSearchType? searchType;
                master.searchTagBox_TagClosed(searchTagBox, tag, pnlSearchDefault, pnlSearchPopup, grvData, txtSearch, out searchType);
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

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Guid idLoaiTaiLieu = Guid.Empty;
            if (!string.IsNullOrEmpty(hdfIdLoaiTaiLieu.Value) && !Guid.TryParse(hdfIdLoaiTaiLieu.Value, out idLoaiTaiLieu))
            {
                ShowInvalidDataError();
                return;
            }

            bool isNew = idLoaiTaiLieu == Guid.Empty;
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
            if (!Guid.TryParse(ddlNhomTaiLieu.SelectedValue, out idNhomTaiLieu) || idNhomTaiLieu == Guid.Empty)
            {
                ShowNotify("Vui lòng chọn nhóm tài liệu.", MSGType.Warning);
                return;
            }

            int thuTuHienThi;
            if (!int.TryParse(txtThuTuHienThi.Text, out thuTuHienThi))
            {
                ShowNotify("Thứ tự hiển thị không hợp lệ.", MSGType.Warning);
                return;
            }

            try
            {
                DocumentTypeManager.Instance.Save(idLoaiTaiLieu, idNhomTaiLieu, txtTenLoai.Text, txtMoTa.Text, chkCanTrinhKy.Checked, ddlHinhThucKy.SelectedValue, chkCanGuiKhachHang.Checked, chkCanLuuVatLy.Checked, thuTuHienThi, chkKichHoat.Checked);
                if (isNew)
                    CURRENT_PAGE.ShowSuccessAddNewData();
                else
                    ShowSuccessSaveData();
                ResetForm();
                dlDetail.CloseModal(true);
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

        protected void grvData_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "EDIT_ITEM" && e.CommandName != "DELETE_ITEM")
            {
                return;
            }

            Guid idLoaiTaiLieu;
            if (!Guid.TryParse(Convert.ToString(e.CommandArgument), out idLoaiTaiLieu))
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

                TblLoaiTaiLieu item = DocumentTypeManager.Instance.GetById(idLoaiTaiLieu);
                if (item == null)
                {
                    ShowInvalidNotFoundData();
                    return;
                }

                ShowEditForm(item);
                return;
            }

            if (e.CommandName == "DELETE_ITEM")
            {
                if (!this.IsDelete)
                {
                    ShowAccessDeniedNotify();
                    return;
                }

                TblLoaiTaiLieu deleteItem =
                    DocumentTypeManager.Instance.GetById(idLoaiTaiLieu);
                if (deleteItem == null)
                {
                    ShowInvalidNotFoundData();
                    return;
                }

                ConfirmResult result = new ConfirmResult
                {
                    CommandName = DeleteConfirmCommand,
                    Value = idLoaiTaiLieu.ToString()
                };
                CURRENT_PAGE.CurrentConfirmResult = result;

                MessageBox message = new MessageBox(
                    GetResourceText(BackEndResourceKeys.NOTIFICATION),
                    string.Format(
                        GetResourceText(
                            BackEndResourceKeys
                                .PLEASE_CONFIRM_TO_DELETE_THE_DATA),
                        deleteItem.TenLoai),
                    MSGButton.DeleteCancel,
                    MSGIcon.Error);
                OpenMessageBox(message, result, false, false);
            }
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

            Guid idLoaiTaiLieu;
            if (!Guid.TryParse(
                    Convert.ToString(e.Value),
                    out idLoaiTaiLieu))
            {
                ShowInvalidDataError();
                return;
            }

            try
            {
                bool deleted =
                    DocumentTypeManager.Instance.Delete(idLoaiTaiLieu);
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

        protected string GetSigningText(object canTrinhKyValue, object hinhThucKyValue)
        {
            bool canTrinhKy = canTrinhKyValue != null && canTrinhKyValue != DBNull.Value && Convert.ToBoolean(canTrinhKyValue);
            if (!canTrinhKy)
                return GetYesNoText(false);
            string hinhThucKy = Convert.ToString(hinhThucKyValue);
            if (string.Equals(hinhThucKy, DocumentSigningMethodKeys.DigitalExternal, StringComparison.OrdinalIgnoreCase))
            {
                return GetResourceText(BackEndResourceKeys.EXTERNAL_DIGITAL_SIGNING);
            }

            return GetResourceText(BackEndResourceKeys.PAPER_SIGNING);
        }

        protected string GetYesNoText(bool value)
        {
            return GetResourceText(value ? BackEndResourceKeys.YES : BackEndResourceKeys.NO);
        }

        protected string GetStatusText(object value)
        {
            return CURRENT_PAGE.GetStatusText(value);
        }

        private static void SelectDropdownValue(ListControl dropdown, string value)
        {
            ListItem item = dropdown.Items.FindByValue(value);
            if (item != null)
                dropdown.SelectedValue = item.Value;
            else if (dropdown.Items.Count > 0)
                dropdown.SelectedValue = dropdown.Items[0].Value;
        }
    }
}
