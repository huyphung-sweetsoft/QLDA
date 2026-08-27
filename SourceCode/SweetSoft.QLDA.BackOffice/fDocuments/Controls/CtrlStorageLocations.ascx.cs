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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using static SweetSoft.QLDA.Controls.EnumHelper;

namespace SweetSoft.QLDA.BackOffice.fDocuments.Controls
{
    public partial class CtrlStorageLocations : BaseAdminUserControl
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
            BindStorageLevels();
            BindEmployees();
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
            ddlSearchCapLuuTru.SearchTagItemText = GetResourceText(
                BackEndResourceKeys.STORAGE_LEVEL);
            txtSearchMaNoiLuuTru.SearchTagItemText = GetResourceText(
                BackEndResourceKeys.DOCUMENT_STORAGE_LOCATION_CODE);
            txtSearchTenNoiLuuTru.SearchTagItemText = GetResourceText(
                BackEndResourceKeys.DOCUMENT_STORAGE_LOCATION_NAME);
            ddlSearchNguoiPhuTrach.SearchTagItemText = GetResourceText(
                BackEndResourceKeys.RESPONSIBLE_EMPLOYEE);
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
            ddlSearchCapLuuTru.Text = GetResourceText(
                BackEndResourceKeys.STORAGE_LEVEL);
            txtSearch.PlaceHolder = GetResourceText(
                BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            txtSearchMaNoiLuuTru.PlaceHolder = GetResourceText(
                BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            txtSearchTenNoiLuuTru.PlaceHolder = GetResourceText(
                BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            txtSearchMoTa.PlaceHolder = GetResourceText(
                BackEndResourceKeys.ENTER_SEARCH_KEYWORDS);
            ddlSearchNguoiPhuTrach.PlaceHolder = GetResourceText(
                BackEndResourceKeys.SELECT_VALUE);
            ddlCapLuuTru.PlaceHolder = GetResourceText(
                BackEndResourceKeys.SELECT_STORAGE_LEVEL);
            ddlNoiLuuTruCha.PlaceHolder = GetResourceText(
                BackEndResourceKeys.SELECT_PARENT_STORAGE_LOCATION);
            ddlNguoiPhuTrach.PlaceHolder = GetResourceText(
                BackEndResourceKeys.SELECT_VALUE);
            chkKichHoat.OnText = GetResourceText(
                BackEndResourceKeys.ACTIVE);
            chkKichHoat.OffText = GetResourceText(
                BackEndResourceKeys.INACTIVE);

            grvData.HeaderTexts = new List<string>
            {
                GetResourceText(BackEndResourceKeys.INDEX),
                GetResourceText(BackEndResourceKeys.DOCUMENT_STORAGE_LOCATION_NAME),
                GetResourceText(BackEndResourceKeys.DOCUMENT_STORAGE_LOCATION_CODE),
                GetResourceText(BackEndResourceKeys.STORAGE_LEVEL),
                "Đường dẫn lưu trữ",
                GetResourceText(BackEndResourceKeys.RESPONSIBLE_EMPLOYEE),
                GetResourceText(BackEndResourceKeys.DISPLAY_ORDER),
                GetResourceText(BackEndResourceKeys.STATUS),
                GetResourceText(BackEndResourceKeys.ACTION)
            };

            txtSearch.EnterSubmitClientID = btnSearch.ClientID;
            btnAdd.Visible = this.IsAdd;
        }

        private void BindStorageLevels()
        {
            ddlCapLuuTru.Items.Clear();
            ddlCapLuuTru.Items.Add(
                new ListItem("Văn phòng", DocumentStorageLevelKeys.Office));
            ddlCapLuuTru.Items.Add(
                new ListItem("Phòng", DocumentStorageLevelKeys.Room));
            ddlCapLuuTru.Items.Add(
                new ListItem("Tủ", DocumentStorageLevelKeys.Cabinet));
            ddlCapLuuTru.Items.Add(
                new ListItem("Kệ", DocumentStorageLevelKeys.Shelf));

            ddlSearchCapLuuTru.Items.Clear();
            ddlSearchCapLuuTru.DefaultSearchValue = "null";
            ddlSearchCapLuuTru.AddItem(
                "Văn phòng",
                DocumentStorageLevelKeys.Office);
            ddlSearchCapLuuTru.AddItem(
                "Phòng",
                DocumentStorageLevelKeys.Room);
            ddlSearchCapLuuTru.AddItem(
                "Tủ",
                DocumentStorageLevelKeys.Cabinet);
            ddlSearchCapLuuTru.AddItem(
                "Kệ",
                DocumentStorageLevelKeys.Shelf);
            ddlSearchCapLuuTru.ClearSelection();

            ControlHelpers controlHelpers = new ControlHelpers();
            controlHelpers.BindStatus(ddlSearchStatus);
        }

        private void BindEmployees()
        {
            List<TblNhanVien> employees =
                DocumentStorageLocationManager.Instance
                    .GetAvailableEmployees()
                ?? new List<TblNhanVien>();

            ddlNguoiPhuTrach.Items.Clear();
            ddlNguoiPhuTrach.Items.Add(
                new ListItem("-- Không chọn --", string.Empty));

            ddlSearchNguoiPhuTrach.Items.Clear();
            ddlSearchNguoiPhuTrach.DefaultSearchValue = string.Empty;
            ddlSearchNguoiPhuTrach.AlowClear = true;
            ddlSearchNguoiPhuTrach.Items.Add(
                new ListItem("Tất cả nhân viên", string.Empty));

            foreach (TblNhanVien employee in employees)
            {
                ListItem item = new ListItem(
                    employee.TenNhanVien,
                    employee.IdNhanVien.ToString());
                ddlNguoiPhuTrach.Items.Add(item);
                ddlSearchNguoiPhuTrach.Items.Add(
                    new ListItem(item.Text, item.Value));
            }

            ddlSearchNguoiPhuTrach.SelectedIndex = -1;
        }

        private void BindParentLocations(
            string childLevel,
            Guid currentId,
            Guid? selectedParentId)
        {
            ddlNoiLuuTruCha.Items.Clear();
            ddlNoiLuuTruCha.Items.Add(
                new ListItem("-- Không có --", string.Empty));

            if (string.Equals(
                childLevel,
                DocumentStorageLevelKeys.Office,
                StringComparison.OrdinalIgnoreCase))
            {
                ddlNoiLuuTruCha.Enabled = false;
                return;
            }

            ddlNoiLuuTruCha.Enabled = true;
            List<TblNoiLuuTru> allItems =
                DocumentStorageLocationManager.Instance.GetAll();
            Dictionary<Guid, TblNoiLuuTru> lookup = allItems.ToDictionary(
                item => item.IdNoiLuuTru);
            List<TblNoiLuuTru> parents =
                DocumentStorageLocationManager.Instance.GetAvailableParents(
                    childLevel,
                    currentId);

            foreach (TblNoiLuuTru parent in parents)
            {
                ddlNoiLuuTruCha.Items.Add(
                    new ListItem(
                        BuildStoragePath(
                            parent.IdNoiLuuTru,
                            lookup),
                        parent.IdNoiLuuTru.ToString()));
            }

            if (selectedParentId.HasValue
                && ddlNoiLuuTruCha.Items.FindByValue(
                    selectedParentId.Value.ToString()) == null)
            {
                TblNoiLuuTru selectedParent =
                    DocumentStorageLocationManager.Instance.GetById(
                        selectedParentId.Value);

                if (selectedParent != null)
                {
                    if (!lookup.ContainsKey(selectedParent.IdNoiLuuTru))
                    {
                        lookup.Add(
                            selectedParent.IdNoiLuuTru,
                            selectedParent);
                    }

                    ddlNoiLuuTruCha.Items.Add(
                        new ListItem(
                            BuildStoragePath(
                                selectedParent.IdNoiLuuTru,
                                lookup)
                            + " (đang khóa)",
                            selectedParent.IdNoiLuuTru.ToString()));
                }
            }

            if (selectedParentId.HasValue)
            {
                SelectDropdownValue(
                    ddlNoiLuuTruCha,
                    selectedParentId.Value.ToString());
            }
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
            grvData.CurrentSortExpression = "TreeOrder";
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
                    data = DocumentStorageLocationManager.Instance
                        .SearchStorageLocations(
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

                    data = DocumentStorageLocationManager.Instance
                        .SearchStorageLocations(
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
            hdfIdNoiLuuTru.Value = string.Empty;
            txtMaNoiLuuTru.Text = string.Empty;
            txtTenNoiLuuTru.Text = string.Empty;
            txtMoTa.Text = string.Empty;
            txtThuTuHienThi.Text = "0";
            SelectDropdownValue(
                ddlCapLuuTru,
                DocumentStorageLevelKeys.Office);
            SelectDropdownValue(ddlNguoiPhuTrach, string.Empty);
            BindParentLocations(
                DocumentStorageLevelKeys.Office,
                Guid.Empty,
                null);
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
                    BackEndResourceKeys.DOCUMENT_STORAGE_LOCATION);
            pnlForm.Visible = true;
            upMain.Update();
        }

        private void ShowEditForm(TblNoiLuuTru item)
        {
            hdfIdNoiLuuTru.Value = item.IdNoiLuuTru.ToString();
            txtMaNoiLuuTru.Text = item.MaNoiLuuTru;
            txtTenNoiLuuTru.Text = item.TenNoiLuuTru;
            txtMoTa.Text = item.MoTa;
            txtThuTuHienThi.Text = item.ThuTuHienThi.ToString();
            SelectDropdownValue(ddlCapLuuTru, item.CapLuuTru);
            BindParentLocations(
                item.CapLuuTru,
                item.IdNoiLuuTru,
                item.IdNoiLuuTruCha);
            SelectDropdownValue(
                ddlNguoiPhuTrach,
                item.IdNhanVienPhuTrach.HasValue
                    ? item.IdNhanVienPhuTrach.Value.ToString()
                    : string.Empty);
            chkKichHoat.Checked = item.KichHoat;
            litFormTitle.Text = GetResourceText(
                    BackEndResourceKeys.EDIT)
                + " "
                + GetResourceText(
                    BackEndResourceKeys.DOCUMENT_STORAGE_LOCATION);
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

        protected void ddlCapLuuTru_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            BindParentLocations(
                ddlCapLuuTru.SelectedValue,
                GetCurrentStorageLocationId(),
                null);
            upMain.Update();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Guid idNoiLuuTru = GetCurrentStorageLocationId();
            bool isNew = idNoiLuuTru == Guid.Empty;

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
                Guid? idNoiLuuTruCha = GetNullableGuid(
                    ddlNoiLuuTruCha,
                    "Nơi lưu trữ cha không hợp lệ.");
                Guid? idNhanVienPhuTrach = GetNullableGuid(
                    ddlNguoiPhuTrach,
                    "Người phụ trách không hợp lệ.");

                DocumentStorageLocationManager.Instance.Save(
                    idNoiLuuTru,
                    idNoiLuuTruCha,
                    txtMaNoiLuuTru.Text,
                    txtTenNoiLuuTru.Text,
                    ddlCapLuuTru.SelectedValue,
                    idNhanVienPhuTrach,
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

            Guid idNoiLuuTru;
            if (!Guid.TryParse(
                Convert.ToString(e.CommandArgument),
                out idNoiLuuTru))
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

                TblNoiLuuTru item =
                    DocumentStorageLocationManager.Instance.GetById(
                        idNoiLuuTru);
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
                    DocumentStorageLocationManager.Instance.Delete(
                        idNoiLuuTru);
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

        protected string GetIndentedStorageName(
            object nameValue,
            object depthValue)
        {
            string name = Convert.ToString(nameValue);
            int depth;
            if (!int.TryParse(Convert.ToString(depthValue), out depth))
                depth = 0;

            string indentation = depth > 0
                ? new string('\u00A0', depth * 4) + "↳ "
                : string.Empty;

            return indentation + name;
        }

        protected string GetStorageLevelText(object levelValue)
        {
            string level = Convert.ToString(levelValue);

            if (string.Equals(
                level,
                DocumentStorageLevelKeys.Office,
                StringComparison.OrdinalIgnoreCase))
            {
                return "Văn phòng";
            }

            if (string.Equals(
                level,
                DocumentStorageLevelKeys.Room,
                StringComparison.OrdinalIgnoreCase))
            {
                return "Phòng";
            }

            if (string.Equals(
                level,
                DocumentStorageLevelKeys.Cabinet,
                StringComparison.OrdinalIgnoreCase))
            {
                return "Tủ";
            }

            if (string.Equals(
                level,
                DocumentStorageLevelKeys.Shelf,
                StringComparison.OrdinalIgnoreCase))
            {
                return "Kệ";
            }

            return level;
        }

        protected string GetResponsibleEmployeeName(object employeeNameValue)
        {
            string employeeName = Convert.ToString(employeeNameValue);
            return string.IsNullOrWhiteSpace(employeeName)
                ? "—"
                : employeeName;
        }

        protected string GetStatusText(object value)
        {
            return CURRENT_PAGE.GetStatusText(value);
        }

        private Guid GetCurrentStorageLocationId()
        {
            Guid id;
            return Guid.TryParse(hdfIdNoiLuuTru.Value, out id)
                ? id
                : Guid.Empty;
        }

        private static Guid? GetNullableGuid(
            ListControl dropdown,
            string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(dropdown.SelectedValue))
                return null;

            Guid id;
            if (!Guid.TryParse(dropdown.SelectedValue, out id))
                throw new ArgumentException(errorMessage);

            return id;
        }

        private static string BuildStoragePath(
            Guid id,
            IDictionary<Guid, TblNoiLuuTru> lookup)
        {
            List<string> names = new List<string>();
            HashSet<Guid> visited = new HashSet<Guid>();
            Guid? cursor = id;

            while (cursor.HasValue)
            {
                if (!visited.Add(cursor.Value))
                    break;

                TblNoiLuuTru item;
                if (!lookup.TryGetValue(cursor.Value, out item))
                    break;

                names.Insert(0, item.TenNoiLuuTru);
                cursor = item.IdNoiLuuTruCha;
            }

            return string.Join(" / ", names);
        }

        private static void SelectDropdownValue(
            ListControl dropdown,
            string value)
        {
            ListItem item = dropdown.Items.FindByValue(
                value ?? string.Empty);

            if (item != null)
                dropdown.SelectedValue = item.Value;
            else if (dropdown.Items.Count > 0)
                dropdown.SelectedIndex = 0;
        }
    }
}
