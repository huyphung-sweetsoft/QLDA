<%@ Control Language="C#"
    AutoEventWireup="true"
    CodeBehind="CtrlDocuments.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.fDocuments.Controls.CtrlDocuments" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx"
    TagPrefix="SweetSoft"
    TagName="FilesBox" %>

<asp:UpdatePanel
    runat="server"
    ID="upMain"
    UpdateMode="Conditional">

    <ContentTemplate>

        <div class="card-header">
            <div class="d-flex flex-column flex-xl-row gap-3 justify-content-between">

                <div class="d-flex flex-column flex-xl-row gap-3">

                    <asp:Panel
                        runat="server"
                        ID="pnlSearchDefault">

                        <div class="d-flex">

                            <SweetSoft:BootstrapDropdown
                                runat="server"
                                ID="ddlSearchTrangThai"
                                Text="Trạng thái hồ sơ"
                                AllowClear="true"
                                AutoPostBack="true"
                                SearchColumn="TrangThaiTaiLieu"
                                CssClass="border-top-left-radius-1 border-bottom-left-radius-1"
                                OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged">
                            </SweetSoft:BootstrapDropdown>

                            <SweetSoft:BootstrapDropdown
                                runat="server"
                                ID="ddlSearchLoaiTaiLieu"
                                Text="Loại tài liệu"
                                AllowClear="true"
                                AutoPostBack="true"
                                EnableSearch="true"
                                ValueIsOfTypeGUID="true"
                                SearchColumn="IdLoaiTaiLieu"
                                CssClass="border-top-right-radius-1 border-bottom-right-radius-1"
                                OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged">
                            </SweetSoft:BootstrapDropdown>

                        </div>
                    </asp:Panel>

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
                            CssClass="border-primary input-search-filter">
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
                CssClass="js-document-form validationEngineContainer border rounded p-3 mb-4">

                <asp:HiddenField
                    runat="server"
                    ID="hdfIdTaiLieu" />

                <h5 class="text-primary mb-3">
                    <asp:Literal
                        runat="server"
                        ID="litFormTitle" />
                </h5>

                <div class="row">

                    <div class="col-md-4 mb-3">
                        <label class="form-label">
                            <%= GetResourceText(BackEndResourceKeys.DOCUMENT_CODE) %>
                        </label>

                        <SweetSoft:ExtraTextBox
                            runat="server"
                            ID="txtMaTaiLieu"
                            MaxLength="100">
                        </SweetSoft:ExtraTextBox>
                    </div>

                    <div class="col-md-8 mb-3">
                        <label class="form-label label-valid">
                            <%= GetResourceText(BackEndResourceKeys.DOCUMENT_NAME) %>
                        </label>

                        <SweetSoft:ExtraTextBox
                            runat="server"
                            ID="txtTenTaiLieu"
                            Required="true"
                            MaxLength="255">
                        </SweetSoft:ExtraTextBox>
                    </div>

                    <div class="col-md-6 mb-3">
                        <label class="form-label label-valid">
                            <%= GetResourceText(BackEndResourceKeys.DOCUMENT_TYPE) %>
                        </label>

                        <SweetSoft:ExtraDropdown
                            runat="server"
                            ID="ddlLoaiTaiLieu"
                            Required="true"
                            ValueIsOfTypeGUID="true"
                            SimpleInit="true"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="ddlLoaiTaiLieu_SelectedIndexChanged">
                        </SweetSoft:ExtraDropdown>
                    </div>

                    <div class="col-md-6 mb-3">
                        <label class="form-label">
                            <%= GetResourceText(BackEndResourceKeys.RESPONSIBLE_EMPLOYEE) %>
                        </label>

                        <SweetSoft:ExtraDropdown
                            runat="server"
                            ID="ddlNguoiPhuTrach"
                            ValueIsOfTypeGUID="true"
                            SimpleInit="true"
                            AlowClear="true">
                        </SweetSoft:ExtraDropdown>
                    </div>

                    <div class="col-md-12 mb-3">
                        <label class="form-label">
                            <%= GetResourceText(BackEndResourceKeys.DESCRIPTION) %>
                        </label>

                        <SweetSoft:ExtraTextBox
                            runat="server"
                            ID="txtMoTa"
                            TextMode="MultiLine"
                            Rows="3"
                            MaxLength="1000">
                        </SweetSoft:ExtraTextBox>
                    </div>

                    <div class="col-12 mb-3">
                        <div class="card border shadow-none mb-0">
                            <div class="card-body pb-2">
                                <div class="d-flex flex-column flex-lg-row justify-content-between gap-2 mb-3">
                                    <h6 class="text-primary mb-0">
                                        <%= GetResourceText(BackEndResourceKeys.DOCUMENT_TYPE_RULE_NOTICE) %>
                                    </h6>

                                    <SweetSoft:ExtraButton
                                        runat="server"
                                        ID="btnRestoreTypeDefaults"
                                        OnClick="btnRestoreTypeDefaults_Click"
                                        CausesValidation="false"
                                        ButtonStyle="OutLineSecondary"
                                        ButtonIcon="Refresh">
                                    </SweetSoft:ExtraButton>
                                </div>

                                <div class="row">
                                    <div class="col-md-4 mb-3">
                                        <label class="form-label">
                                            <%= GetResourceText(BackEndResourceKeys.ALLOW_SIGNING) %>
                                        </label>
                                        <div class="mt-2">
                                            <SweetSoft:ExtraCheckbox
                                                runat="server"
                                                ID="chkCanTrinhKy"
                                                OnChange="toggleDocumentFormSigningMethod();"
                                                OnText="Có"
                                                OffText="Không" />
                                        </div>
                                    </div>

                                    <div
                                        runat="server"
                                        id="divHinhThucKy"
                                        class="col-md-4 mb-3">
                                        <label class="form-label">
                                            <%= GetResourceText(BackEndResourceKeys.SIGNING_METHOD) %>
                                        </label>
                                        <SweetSoft:ExtraDropdown
                                            runat="server"
                                            ID="ddlHinhThucKy"
                                            SimpleInit="true">
                                        </SweetSoft:ExtraDropdown>
                                    </div>

                                    <div class="col-md-4 mb-3">
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

                                    <div class="col-md-4 mb-3">
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
                            </div>
                        </div>
                    </div>

                </div>

                <asp:Panel
                    runat="server"
                    ID="pnlUploadPlaceholder"
                    CssClass="alert alert-secondary py-2">
                    <%= GetResourceText(BackEndResourceKeys.SAVE_DOCUMENT_BEFORE_UPLOAD) %>
                </asp:Panel>

                <asp:Panel
                    runat="server"
                    ID="pnlVersionFiles"
                    Visible="false"
                    CssClass="border rounded p-3 mb-3">
                    <h6 class="text-primary mb-2">
                        <%= GetResourceText(BackEndResourceKeys.DOCUMENT_VERSION_FILES) %>
                    </h6>
                    <div class="alert alert-info py-2">
                        <%= GetResourceText(BackEndResourceKeys.DOCUMENT_VERSION_NOTICE) %>
                    </div>

                    <SweetSoft:FilesBox
                        runat="server"
                        ID="fbVersions"
                        IsMultiple="true" />

                    <asp:Panel
                        runat="server"
                        ID="pnlVersionHistory"
                        Visible="false"
                        CssClass="table-responsive mt-3">
                        <table class="table table-sm table-bordered align-middle mb-0">
                            <thead>
                                <tr>
                                    <th style="width: 110px;">
                                        <%= GetResourceText(BackEndResourceKeys.VERSION) %>
                                    </th>
                                    <th>
                                        <%= GetResourceText(BackEndResourceKeys.FILE_NAME) %>
                                    </th>
                                    <th style="width: 130px;">
                                        <%= GetResourceText(BackEndResourceKeys.STATUS) %>
                                    </th>
                                    <th style="width: 170px;">
                                        <%= GetResourceText(BackEndResourceKeys.CREATED_DATE) %>
                                    </th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater
                                    runat="server"
                                    ID="rptVersions">
                                    <ItemTemplate>
                                        <tr>
                                            <td><%#: Eval("SoPhienBan") %></td>
                                            <td>
                                                <asp:HyperLink
                                                    runat="server"
                                                    NavigateUrl='<%# GetFileUrl(Eval("FileUrl")) %>'
                                                    Text='<%# GetVersionFileName(Eval("TenFileGoc"), Eval("TenFile")) %>'
                                                    Target="_blank"
                                                    CssClass="text-primary text-decoration-underline" />
                                            </td>
                                            <td>
                                                <asp:Label
                                                    runat="server"
                                                    Visible='<%# Convert.ToBoolean(Eval("LaPhienBanHienTai")) %>'
                                                    Text='<%# GetResourceText(BackEndResourceKeys.CURRENT_VERSION) %>'
                                                    CssClass="badge bg-success" />
                                            </td>
                                            <td><%# ConvertDateTimeToString(Eval("NgayTao")) %></td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </asp:Panel>
                </asp:Panel>

                <div class="d-flex gap-2">
                    <SweetSoft:ExtraButton
                        runat="server"
                        ID="btnSave"
                        OnClick="btnSave_Click"
                        OnClientClick="return CMSMasterJs.ValidElement(
                            '.js-document-form');"
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
                    DataKeyNames="IdTaiLieu"
                    GridLines="None"
                    CssClass="table-bordered table-hover align-middle"
                    IsEnableSelectColumn="false"
                    FocusBtnIcon="fas fa-compress-arrows-alt"
                    OnNeedDataSource="grvData_NeedDataSource"
                    OnRowCommand="grvData_RowCommand">

                    <Columns>

                        <asp:BoundField
                            DataField="MaTaiLieu"
                            HeaderText="Mã hồ sơ"
                            SortExpression="MaTaiLieu"
                            HeaderStyle-Width="150px" />

                        <asp:BoundField
                            DataField="TenTaiLieu"
                            HeaderText="Tên hồ sơ"
                            SortExpression="TenTaiLieu" />

                        <asp:TemplateField
                            HeaderText="Loại tài liệu"
                            SortExpression="TenLoai">
                            <ItemTemplate>
                                <%#: GetDocumentTypeText(Eval("TenNhom"), Eval("TenLoai")) %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField
                            HeaderText="Người phụ trách"
                            SortExpression="TenNhanVienPhuTrach">
                            <ItemTemplate>
                                <%#: GetResponsibleEmployeeText(Eval("TenNhanVienPhuTrach")) %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField
                            HeaderText="Trạng thái hồ sơ"
                            SortExpression="TrangThaiTaiLieu"
                            HeaderStyle-Width="145px">
                            <ItemTemplate>
                                <span class='<%# GetDocumentStatusCss(Eval("TrangThaiTaiLieu")) %>'>
                                    <%#: GetDocumentStatusText(Eval("TrangThaiTaiLieu")) %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField
                            HeaderText="Trình ký"
                            HeaderStyle-Width="130px">
                            <ItemTemplate>
                                <%#: GetSigningText(Eval("CanTrinhKy"), Eval("HinhThucKy")) %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField
                            HeaderText="Gửi khách"
                            SortExpression="TrangThaiGuiKhach"
                            HeaderStyle-Width="125px">
                            <ItemTemplate>
                                <%#: GetCustomerStatusText(Eval("CanGuiKhachHang"), Eval("TrangThaiGuiKhach")) %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField
                            HeaderText="Lưu bản cứng"
                            SortExpression="TrangThaiLuuTru"
                            HeaderStyle-Width="125px">
                            <ItemTemplate>
                                <%#: GetPhysicalStorageStatusText(Eval("CanLuuVatLy"), Eval("TrangThaiLuuTru")) %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField
                            HeaderText="File chính thức">
                            <ItemTemplate>
                                <asp:HyperLink
                                    runat="server"
                                    Visible='<%# HasOfficialFile(Eval("IdFileBanChinhThuc")) %>'
                                    NavigateUrl='<%# GetFileUrl(Eval("FileChinhThucUrl")) %>'
                                    Text='<%# GetOfficialFileName(Eval("TenFileChinhThucGoc"), Eval("TenFileChinhThuc")) %>'
                                    Target="_blank"
                                    CssClass="text-primary text-decoration-underline" />
                                <asp:Label
                                    runat="server"
                                    Visible='<%# !HasOfficialFile(Eval("IdFileBanChinhThuc")) %>'
                                    Text='<%# GetResourceText(BackEndResourceKeys.FILE_NOT_UPLOADED) %>'
                                    CssClass="badge bg-secondary" />
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField
                            HeaderText="Ngày tạo"
                            SortExpression="NgayTao"
                            HeaderStyle-Width="140px">
                            <ItemTemplate>
                                <%# ConvertDateTimeToString(Eval("NgayTao")) %>
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
                                    CommandArgument='<%# Eval("IdTaiLieu") %>'
                                    CausesValidation="false"
                                    Visible='<%# this.IsEdit %>'
                                    CssClass="btn btn-sm btn-outline-primary me-1"
                                    Text='<%# GetResourceText(BackEndResourceKeys.EDIT) %>'>
                                </asp:LinkButton>

                                <asp:LinkButton
                                    runat="server"
                                    ID="btnDeleteRow"
                                    CommandName="DELETE_ITEM"
                                    CommandArgument='<%# Eval("IdTaiLieu") %>'
                                    CausesValidation="false"
                                    Visible='<%# this.IsDelete %>'
                                    CssClass="btn btn-sm btn-outline-danger"
                                    OnClientClick="return confirm('Bạn có chắc muốn xóa hồ sơ này?');"
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
                                    <%= GetResourceText(BackEndResourceKeys.DOCUMENT_CODE) %>
                                </label>
                                <SweetSoft:ExtraTextBox
                                    runat="server"
                                    ID="txtSearchMaTaiLieu"
                                    SearchColumn="MaTaiLieu">
                                </SweetSoft:ExtraTextBox>
                            </div>

                            <div class="col-md-6 mb-3">
                                <label class="form-label">
                                    <%= GetResourceText(BackEndResourceKeys.DOCUMENT_NAME) %>
                                </label>
                                <SweetSoft:ExtraTextBox
                                    runat="server"
                                    ID="txtSearchTenTaiLieu"
                                    SearchColumn="TenTaiLieu">
                                </SweetSoft:ExtraTextBox>
                            </div>

                            <div class="col-md-6 mb-3">
                                <label class="form-label">
                                    <%= GetResourceText(BackEndResourceKeys.DOCUMENT_GROUP) %>
                                </label>
                                <SweetSoft:ExtraDropdown
                                    runat="server"
                                    ID="ddlSearchNhomTaiLieu"
                                    SearchColumn="IdNhomTaiLieu"
                                    ValueIsOfTypeGUID="true"
                                    SimpleInit="true"
                                    AlowClear="true">
                                </SweetSoft:ExtraDropdown>
                            </div>

                            <div class="col-md-6 mb-3">
                                <label class="form-label">
                                    <%= GetResourceText(BackEndResourceKeys.RESPONSIBLE_EMPLOYEE) %>
                                </label>
                                <SweetSoft:ExtraDropdown
                                    runat="server"
                                    ID="ddlSearchNguoiPhuTrach"
                                    SearchColumn="IdNhanVienPhuTrach"
                                    ValueIsOfTypeGUID="true"
                                    SimpleInit="true"
                                    AlowClear="true">
                                </SweetSoft:ExtraDropdown>
                            </div>

                            <div class="col-md-12 mb-3">
                                <label class="form-label">
                                    <%= GetResourceText(BackEndResourceKeys.DESCRIPTION) %>
                                </label>
                                <SweetSoft:ExtraTextBox
                                    runat="server"
                                    ID="txtSearchMoTa"
                                    SearchColumn="MoTa">
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
                                    <%= GetResourceText(BackEndResourceKeys.SIGNING_METHOD) %>
                                </label>
                                <SweetSoft:ExtraDropdown
                                    runat="server"
                                    ID="ddlSearchHinhThucKy"
                                    SearchColumn="HinhThucKy"
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
                                    <%= GetResourceText(BackEndResourceKeys.CUSTOMER_SEND_STATUS) %>
                                </label>
                                <SweetSoft:ExtraDropdown
                                    runat="server"
                                    ID="ddlSearchTrangThaiGuiKhach"
                                    SearchColumn="TrangThaiGuiKhach"
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

                            <div class="col-md-6 mb-3">
                                <label class="form-label">
                                    <%= GetResourceText(BackEndResourceKeys.PHYSICAL_STORAGE_STATUS) %>
                                </label>
                                <SweetSoft:ExtraDropdown
                                    runat="server"
                                    ID="ddlSearchTrangThaiLuuTru"
                                    SearchColumn="TrangThaiLuuTru"
                                    SimpleInit="true"
                                    AlowClear="true">
                                </SweetSoft:ExtraDropdown>
                            </div>

                            <div class="col-md-6 mb-3">
                                <label class="form-label">
                                    <%= GetResourceText(BackEndResourceKeys.OFFICIAL_FILE) %>
                                </label>
                                <SweetSoft:ExtraDropdown
                                    runat="server"
                                    ID="ddlSearchHasOfficialFile"
                                    SearchColumn="HasOfficialFile"
                                    SimpleInit="true"
                                    AlowClear="true">
                                </SweetSoft:ExtraDropdown>
                            </div>

                            <div class="col-md-6 mb-3">
                                <label class="form-label">
                                    <%= GetResourceText(BackEndResourceKeys.CREATED_DATE) %>
                                </label>
                                <SweetSoft:ExtraDateTime
                                    runat="server"
                                    ID="dtSearchNgayTao"
                                    SearchColumn="NgayTao"
                                    SingleDatePicker="false"
                                    IsPredefinedDateRanges="true"
                                    AutoUpdateInput="false" />
                            </div>

                        </div>
                    </asp:Panel>

                </ContentTemplate>
            </asp:UpdatePanel>

        </div>
    </div>
</div>

<script type="text/javascript">
    function toggleDocumentFormSigningMethod() {
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
        Sys.Application.add_load(toggleDocumentFormSigningMethod);
    }
    else {
        document.addEventListener(
            'DOMContentLoaded',
            toggleDocumentFormSigningMethod);
    }
</script>
