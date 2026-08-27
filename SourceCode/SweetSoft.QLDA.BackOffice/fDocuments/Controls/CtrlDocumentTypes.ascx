<%@ Control Language="C#"
    AutoEventWireup="true"
    CodeBehind="CtrlDocumentTypes.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.fDocuments.Controls.CtrlDocumentTypes" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

                <asp:UpdatePanel
                    runat="server"
                    ID="upMain"
                    UpdateMode="Conditional">

                    <ContentTemplate>

                        <div class="card-header">
                            <div class="d-flex flex-column flex-xl-row gap-3 justify-content-between">

                                <div class="d-flex flex-column flex-xl-row gap-3">

                                    <%-- Bộ lọc nhanh --%>
                                    <asp:Panel
                                        runat="server"
                                        ID="pnlSearchDefault">

                                        <div class="d-flex">

                                            <SweetSoft:BootstrapDropdown
                                                runat="server"
                                                ID="ddlSearchStatus"
                                                Text="Trạng thái"
                                                AllowClear="true"
                                                AutoPostBack="true"
                                                SearchColumn="KichHoat"
                                                CssClass="border-top-left-radius-1 border-bottom-left-radius-1"
                                                OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged">
                                            </SweetSoft:BootstrapDropdown>

                                            <SweetSoft:BootstrapDropdown
                                                runat="server"
                                                ID="ddlSearchNhom"
                                                Text="Nhóm tài liệu"
                                                AllowClear="true"
                                                AutoPostBack="true"
                                                EnableSearch="true"
                                                ValueIsOfTypeGUID="true"
                                                SearchColumn="IdNhomTaiLieu"
                                                SearchPlaceholder="Tìm kiếm nhóm tài liệu..."
                                                NoResultsText="Không tìm thấy nhóm tài liệu"
                                                CssClass="border-top-right-radius-1 border-bottom-right-radius-1"
                                                OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged">
                                            </SweetSoft:BootstrapDropdown>

                                        </div>
                                    </asp:Panel>

                                    <%-- Tìm kiếm từ khóa --%>
                                    <div class="input-group max-w-500">

                                        <a
                                            class="btn btn-info font-mobile-small btn-search-filter"
                                            onclick="CMSMasterJs.ShowOffcanvasSearch();"
                                            href="javascript:;">
                                            <i class="fas fa-filter me-1"></i>
                                            <%= GetResourceText(BackEndResourceKeys.FILTER) %>
                                        </a>

                                        <SweetSoft:ExtraTextBox
                                            runat="server"
                                            ID="txtSearch"
                                            CssClass="border-primary input-search-filter"
                                            PlaceHolder="Nhập tên hoặc mô tả">
                                        </SweetSoft:ExtraTextBox>

                                        <SweetSoft:ExtraButton
                                            runat="server"
                                            ID="btnSearch"
                                            OnClick="btnSearch_Click"
                                            CssClass="btn-outline-primary btn-search-filter"
                                            IsCustomClass="false"
                                            ButtonIcon="Search">
                                        </SweetSoft:ExtraButton>

                                    </div>
                                </div>

                                <SweetSoft:ExtraButton
                                    runat="server"
                                    ID="btnAdd"
                                    OnClick="btnAdd_Click"
                                    ButtonStyle="Info"
                                    ButtonIcon="Add"
                                    Visible="false">
                                </SweetSoft:ExtraButton>

                            </div>

                            <div class="listSearchTagBox mt-2">
                                <SweetSoft:ExtraSearchBox
                                    runat="server"
                                    ID="searchTagBox"
                                    OnTagClosed="searchTagBox_TagClosed">
                                </SweetSoft:ExtraSearchBox>
                            </div>
                        </div>

                        <div class="card-body">

                            <asp:Panel
                                runat="server"
                                ID="pnlForm"
                                Visible="false"
                                CssClass="js-document-type-form validationEngineContainer border rounded p-3 mb-4">

                                <asp:HiddenField
                                    runat="server"
                                    ID="hdfIdLoaiTaiLieu" />

                                <h5 class="text-primary mb-3">
                                    <asp:Literal
                                        runat="server"
                                        ID="litFormTitle" />
                                </h5>

                                <div class="row">

                                    <div class="col-md-6 mb-3">
                                        <label class="form-label label-valid">
                                            <%= GetResourceText(BackEndResourceKeys.DOCUMENT_GROUP) %>
                                        </label>

                                        <SweetSoft:ExtraDropdown
                                            runat="server"
                                            ID="ddlNhomTaiLieu"
                                            Required="true"
                                            ValueIsOfTypeGUID="true"
                                            SimpleInit="true"
                                            PlaceHolder="Chọn nhóm tài liệu">
                                        </SweetSoft:ExtraDropdown>
                                    </div>

                                    <div class="col-md-6 mb-3">
                                        <label class="form-label label-valid">
                                            <%= GetResourceText(BackEndResourceKeys.DOCUMENT_TYPE_NAME) %>
                                        </label>

                                        <SweetSoft:ExtraTextBox
                                            runat="server"
                                            ID="txtTenLoai"
                                            Required="true"
                                            MaxLength="150">
                                        </SweetSoft:ExtraTextBox>
                                    </div>

                                    <div class="col-md-8 mb-3">
                                        <label class="form-label">
                                            <%= GetResourceText(BackEndResourceKeys.DESCRIPTION) %>
                                        </label>

                                        <SweetSoft:ExtraTextBox
                                            runat="server"
                                            ID="txtMoTa"
                                            TextMode="MultiLine"
                                            Rows="3"
                                            MaxLength="500">
                                        </SweetSoft:ExtraTextBox>
                                    </div>

                                    <div class="col-md-2 mb-3">
                                        <label class="form-label label-valid">
                                            <%= GetResourceText(BackEndResourceKeys.DISPLAY_ORDER) %>
                                        </label>

                                        <SweetSoft:ExtraTextBox
                                            runat="server"
                                            ID="txtThuTuHienThi"
                                            Required="true"
                                            TextMode="Number"
                                            Text="0">
                                        </SweetSoft:ExtraTextBox>
                                    </div>

                                    <div class="col-md-2 mb-3">
                                        <label class="form-label">
                                            <%= GetResourceText(BackEndResourceKeys.STATUS) %>
                                        </label>

                                        <div class="mt-2">
                                            <SweetSoft:ExtraCheckbox
                                                runat="server"
                                                ID="chkKichHoat"
                                                Checked="true"
                                                OnText="Kích hoạt"
                                                OffText="Khóa" />
                                        </div>
                                    </div>

                                    <div class="col-md-3 mb-3">
                                        <label class="form-label">
                                            <%= GetResourceText(BackEndResourceKeys.ALLOW_SIGNING) %>
                                        </label>

                                        <div class="mt-2">
                                            <SweetSoft:ExtraCheckbox
                                                runat="server"
                                                ID="chkCanTrinhKy"
                                                OnChange="toggleDocumentSigningMethod();"
                                                OnText="Có"
                                                OffText="Không" />
                                        </div>
                                    </div>

                                    <div
                                        runat="server"
                                        id="divHinhThucKy"
                                        class="col-md-3 mb-3">

                                        <label class="form-label">
                                            <%= GetResourceText(BackEndResourceKeys.DEFAULT_SIGNING_METHOD) %>
                                        </label>

                                        <SweetSoft:ExtraDropdown
                                            runat="server"
                                            ID="ddlHinhThucKy"
                                            SimpleInit="true">
                                        </SweetSoft:ExtraDropdown>
                                    </div>

                                    <div class="col-md-3 mb-3">
                                        <label class="form-label">
                                            <%= GetResourceText(BackEndResourceKeys.ALLOW_SEND_CUSTOMER) %>
                                        </label>

                                        <div class="mt-2">
                                            <SweetSoft:ExtraCheckbox
                                                runat="server"
                                                ID="chkCanGuiKhachHang"
                                                OnText="Có"
                                                OffText="Không" />
                                        </div>
                                    </div>

                                    <div class="col-md-3 mb-3">
                                        <label class="form-label">
                                            <%= GetResourceText(BackEndResourceKeys.ALLOW_PHYSICAL_STORAGE) %>
                                        </label>

                                        <div class="mt-2">
                                            <SweetSoft:ExtraCheckbox
                                                runat="server"
                                                ID="chkCanLuuVatLy"
                                                OnText="Có"
                                                OffText="Không" />
                                        </div>
                                    </div>

                                </div>

                                <div class="d-flex gap-2">
                                    <SweetSoft:ExtraButton
                                        runat="server"
                                        ID="btnSave"
                                        OnClick="btnSave_Click"
                                        OnClientClick="return CMSMasterJs.ValidElement(
                                            '.js-document-type-form');"
                                        ButtonStyle="Primary"
                                        ButtonIcon="Save">
                                    </SweetSoft:ExtraButton>

                                    <SweetSoft:ExtraButton
                                        runat="server"
                                        ID="btnCancel"
                                        OnClick="btnCancel_Click"
                                        ButtonStyle="OutLineSecondary"
                                        ButtonIcon="Close">
                                    </SweetSoft:ExtraButton>
                                </div>

                            </asp:Panel>

                            <div class="table-responsive">
                                <SweetSoft:GridviewExtension
                                    runat="server"
                                    ID="grvData"
                                    AllowSorting="true"
                                    ShowHeader="true"
                                    ShowHeaderWhenEmpty="true"
                                    AutoGenerateColumns="false"
                                    DataKeyNames="IdLoaiTaiLieu"
                                    GridLines="None"
                                    CssClass="table-bordered table-hover"
                                    IsEnableSelectColumn="false"
                                    FocusBtnIcon="fas fa-compress-arrows-alt"
                                    OnNeedDataSource="grvData_NeedDataSource"
                                    OnRowCommand="grvData_RowCommand">

                                    <Columns>

                                        <asp:BoundField
                                            DataField="TenLoai"
                                            HeaderText="Tên loại tài liệu"
                                            SortExpression="TenLoai" />

                                        <asp:BoundField
                                            DataField="TenNhom"
                                            HeaderText="Nhóm tài liệu"
                                            SortExpression="TenNhom" />

                                        <asp:TemplateField
                                            HeaderText="Trình ký"
                                            SortExpression="CanTrinhKy">
                                            <ItemTemplate>
                                                <%# GetSigningText(Eval("CanTrinhKy"), Eval("HinhThucKyMacDinh")) %>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField
                                            HeaderText="Gửi khách"
                                            SortExpression="CanGuiKhachHang"
                                            ItemStyle-CssClass="text-center">
                                            <ItemTemplate>
                                                <%# GetYesNoText(Convert.ToBoolean(Eval("CanGuiKhachHang"))) %>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField
                                            HeaderText="Lưu bản cứng"
                                            SortExpression="CanLuuVatLy"
                                            ItemStyle-CssClass="text-center">
                                            <ItemTemplate>
                                                <%# GetYesNoText(Convert.ToBoolean(Eval("CanLuuVatLy"))) %>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:BoundField
                                            DataField="ThuTuHienThi"
                                            HeaderText="Thứ tự"
                                            SortExpression="ThuTuHienThi"
                                            ItemStyle-CssClass="text-center"
                                            HeaderStyle-Width="80px" />

                                        <asp:TemplateField
                                            HeaderText="Trạng thái"
                                            SortExpression="KichHoat"
                                            ItemStyle-CssClass="text-center"
                                            HeaderStyle-Width="120px">
                                            <ItemTemplate>
                                                <%# GetStatusText(Eval("KichHoat")) %>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField
                                            HeaderText="Thao tác"
                                            ItemStyle-CssClass="text-center"
                                            HeaderStyle-Width="160px">

                                            <ItemTemplate>
                                                <asp:LinkButton
                                                    runat="server"
                                                    ID="btnEditRow"
                                                    CommandName="EDIT_ITEM"
                                                    CommandArgument='<%# Eval("IdLoaiTaiLieu") %>'
                                                    CausesValidation="false"
                                                    Visible='<%# this.IsEdit %>'
                                                    CssClass="btn btn-sm btn-outline-primary me-1"
                                                    Text='<%# GetResourceText(BackEndResourceKeys.EDIT) %>'>
                                                </asp:LinkButton>

                                                <asp:LinkButton
                                                    runat="server"
                                                    ID="btnDeleteRow"
                                                    CommandName="DELETE_ITEM"
                                                    CommandArgument='<%# Eval("IdLoaiTaiLieu") %>'
                                                    CausesValidation="false"
                                                    Visible='<%# this.IsDelete %>'
                                                    CssClass="btn btn-sm btn-outline-danger"
                                                    OnClientClick="return confirm('Bạn có chắc muốn xóa loại tài liệu này?');"
                                                    Text='<%# GetResourceText(BackEndResourceKeys.DELETE) %>'>
                                                </asp:LinkButton>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                    </Columns>

                                    <EmptyDataTemplate>
                                        <div class="text-center p-4">
                                            <%= GetResourceText(BackEndResourceKeys.NO_DATA) %>
                                        </div>
                                    </EmptyDataTemplate>

                                </SweetSoft:GridviewExtension>
                            </div>

                            <SweetSoft:Paging
                                runat="server"
                                ID="ctrlGridviewPaging"
                                OnPageChanged="ctrlGridviewPaging_PageChanged" />

                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>

    <div
        class="offcanvas offcanvas-end offcanvas-form-search"
        id="search-offcanvas"
        aria-hidden="true">

        <div class="offcanvas-header">

            <div class="d-flex flex-column flex-md-row align-items-center gap-3">

                <h5 class="offcanvas-title">
                    <%= GetResourceText(BackEndResourceKeys.ADVANCED_SEARCH) %>
                </h5>

                <div class="d-flex align-items-center gap-1">

                    <SweetSoft:ExtraButton
                        runat="server"
                        ID="btnSearchAdvanced"
                        OnClick="btnSearchAdvanced_Click"
                        CssClass="flex-btn"
                        ButtonStyle="Primary"
                        ButtonIcon="Search">
                    </SweetSoft:ExtraButton>

                    <SweetSoft:ExtraButton
                        runat="server"
                        ID="btnResetSearch"
                        OnClick="btnResetSearch_Click"
                        CssClass="flex-btn"
                        ButtonStyle="OutLineSecondary"
                        ButtonIcon="Refresh">
                    </SweetSoft:ExtraButton>

                </div>
            </div>

            <button
                class="btn-close"
                type="button"
                data-bs-dismiss="offcanvas"
                aria-label="Close">
            </button>

        </div>

        <div class="offcanvas-body pt-0">
            <div class="card shadow-none card-body text-muted mb-0">

                <asp:UpdatePanel
                    runat="server"
                    ID="pnlSearch"
                    UpdateMode="Conditional">

                    <ContentTemplate>

                        <asp:Panel
                            runat="server"
                            ID="pnlSearchPopup">

                            <div class="row">

                                <div class="col-md-6 mb-3">
                                    <label class="form-label">
                                        <%= GetResourceText(BackEndResourceKeys.DOCUMENT_TYPE_NAME) %>
                                    </label>

                                    <SweetSoft:ExtraTextBox
                                        runat="server"
                                        ID="txtSearchTenLoai"
                                        SearchColumn="TenLoai"
                                        PlaceHolder="Nhập tên loại tài liệu">
                                    </SweetSoft:ExtraTextBox>
                                </div>

                                <div class="col-md-6 mb-3">
                                    <label class="form-label">
                                        <%= GetResourceText(BackEndResourceKeys.DESCRIPTION) %>
                                    </label>

                                    <SweetSoft:ExtraTextBox
                                        runat="server"
                                        ID="txtSearchMoTa"
                                        SearchColumn="MoTa"
                                        PlaceHolder="Nhập nội dung mô tả">
                                    </SweetSoft:ExtraTextBox>
                                </div>

                                <div class="col-md-6 mb-3">
                                    <label class="form-label">
                                        <%= GetResourceText(BackEndResourceKeys.ALLOW_SIGNING) %>
                                    </label>

                                    <SweetSoft:ExtraDropdown
                                        runat="server"
                                        ID="ddlSearchCanTrinhKy"
                                        SearchColumn="CanTrinhKy"
                                        SimpleInit="true"
                                        AlowClear="true">
                                    </SweetSoft:ExtraDropdown>
                                </div>

                                <div class="col-md-6 mb-3">
                                    <label class="form-label">
                                        <%= GetResourceText(BackEndResourceKeys.DEFAULT_SIGNING_METHOD) %>
                                    </label>

                                    <SweetSoft:ExtraDropdown
                                        runat="server"
                                        ID="ddlSearchHinhThucKy"
                                        SearchColumn="HinhThucKyMacDinh"
                                        SimpleInit="true"
                                        AlowClear="true">
                                    </SweetSoft:ExtraDropdown>
                                </div>

                                <div class="col-md-6 mb-3">
                                    <label class="form-label">
                                        <%= GetResourceText(BackEndResourceKeys.ALLOW_SEND_CUSTOMER) %>
                                    </label>

                                    <SweetSoft:ExtraDropdown
                                        runat="server"
                                        ID="ddlSearchCanGuiKhachHang"
                                        SearchColumn="CanGuiKhachHang"
                                        SimpleInit="true"
                                        AlowClear="true">
                                    </SweetSoft:ExtraDropdown>
                                </div>

                                <div class="col-md-6 mb-3">
                                    <label class="form-label">
                                        <%= GetResourceText(BackEndResourceKeys.ALLOW_PHYSICAL_STORAGE) %>
                                    </label>

                                    <SweetSoft:ExtraDropdown
                                        runat="server"
                                        ID="ddlSearchCanLuuVatLy"
                                        SearchColumn="CanLuuVatLy"
                                        SimpleInit="true"
                                        AlowClear="true">
                                    </SweetSoft:ExtraDropdown>
                                </div>

                            </div>
                        </asp:Panel>

                    </ContentTemplate>
                </asp:UpdatePanel>

            </div>
        </div>
    </div>

    <script type="text/javascript">
        function toggleDocumentSigningMethod() {
            var checkbox = document.getElementById(
                '<%= chkCanTrinhKy.ClientID %>');
            var signingMethod = document.getElementById(
                '<%= divHinhThucKy.ClientID %>');

            if (!checkbox || !signingMethod) {
                return;
            }

            signingMethod.style.display = checkbox.checked ? '' : 'none';
        }

        if (window.Sys && Sys.Application) {
            Sys.Application.add_load(toggleDocumentSigningMethod);
        }
        else {
            document.addEventListener(
                'DOMContentLoaded',
                toggleDocumentSigningMethod);
        }
    </script>

