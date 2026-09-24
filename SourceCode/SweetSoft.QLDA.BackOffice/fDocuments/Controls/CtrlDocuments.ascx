<%@ Control Language="C#"
    AutoEventWireup="true"
    CodeBehind="CtrlDocuments.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.fDocuments.Controls.CtrlDocuments" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<asp:UpdatePanel
    runat="server"
    ID="upMain"
    UpdateMode="Conditional">

    <ContentTemplate>

        <div class="card-header">
            <div class="d-flex flex-column flex-xl-row gap-3">
                <asp:Panel runat="server" ID="pnlSearchDefault">
                    <div class="d-flex">
                        <asp:Panel runat="server" ID="pnlSearchScope" CssClass="d-flex">
                            <SweetSoft:BootstrapDropdown runat="server" ID="ddlSearchPhamVi"
                                AutoPostBack="true" SearchColumn="DocumentScope"
                                CssClass="border-top-left-radius-1 border-bottom-left-radius-1"
                                OnSelectedValueChanged="ddlSearchPhamVi_SelectedValueChanged" />
                        </asp:Panel>
                        <asp:Panel runat="server" ID="pnlProjectSelector" CssClass="d-flex">
                            <SweetSoft:BootstrapDropdown runat="server" ID="ddlSearchDuAn"
                                AllowClear="true" AutoPostBack="true" EnableSearch="true"
                                ValueIsOfTypeGUID="true" SearchColumn="IdDuAn"
                                OnSelectedValueChanged="ddlSearchDuAn_SelectedValueChanged" />
                        </asp:Panel>
                        <SweetSoft:BootstrapDropdown runat="server" ID="ddlSearchNhomTaiLieu"
                            Visible="false" AllowClear="true" AutoPostBack="true" EnableSearch="true"
                            ValueIsOfTypeGUID="true" SearchColumn="IdNhomTaiLieu"
                            OnSelectedValueChanged="ddlSearchNhomTaiLieu_SelectedValueChanged" />
                        <SweetSoft:BootstrapDropdown runat="server" ID="ddlSearchLoaiTaiLieu"
                            AllowClear="true" AutoPostBack="true" EnableSearch="true"
                            ValueIsOfTypeGUID="true" SearchColumn="IdLoaiTaiLieu"
                            OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged" />
                        <SweetSoft:BootstrapDropdown runat="server" ID="ddlSearchTrangThai"
                            AllowClear="true" AutoPostBack="true" SearchColumn="TrangThaiTaiLieu"
                            CssClass="border-top-right-radius-1 border-bottom-right-radius-1"
                            OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged" />
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

                <div class="d-flex justify-content-end gap-3 w-full flex-wrap">
                    <SweetSoft:ExtraButton
                        runat="server"
                        ID="btnAdd"
                        OnClick="btnAdd_Click"
                        ButtonStyle="Info"
                        ButtonIcon="Add"
                        Visible="false">
                    </SweetSoft:ExtraButton>
                </div>
            </div>

            <div class="listSearchTagBox">
                <SweetSoft:ExtraSearchBox
                    runat="server"
                    ID="searchTagBox"
                    OnTagClosed="searchTagBox_TagClosed">
                </SweetSoft:ExtraSearchBox>
            </div>
        </div>

        <asp:Panel runat="server" ID="pnlDocumentGrid" CssClass="card-body p-0">
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
                    CssClass="table-bordered table-hover document-list-grid"
                    IsEnableSelectColumn="false"
                    FocusBtnIcon="fas fa-compress-arrows-alt"
                    OnNeedDataSource="grvData_NeedDataSource"
                    OnRowCommand="grvData_RowCommand">

                    <Columns>

                        <asp:TemplateField
                            HeaderText="Mã hồ sơ"
                            SortExpression="MaTaiLieu"
                            HeaderStyle-Width="150px"
                            HeaderStyle-CssClass="document-list-code-column text-center"
                            ItemStyle-CssClass="document-list-code-column">
                            <ItemTemplate>
                                <span class="fw-bold text-primary"><%#: Eval("MaTaiLieu") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField
                            DataField="TenTaiLieu"
                            HeaderText="Tên hồ sơ"
                            SortExpression="TenTaiLieu"
                            HeaderStyle-CssClass="document-list-name-column text-center"
                            ItemStyle-CssClass="document-list-name-column" />

                        <asp:TemplateField
                            HeaderText="Phạm vi"
                            SortExpression="TenDuAn"
                            HeaderStyle-Width="180px"
                            HeaderStyle-CssClass="document-list-scope-column text-center"
                            ItemStyle-CssClass="document-list-scope-column">
                            <ItemTemplate>
                                <span class="text-body">
                                    <i class='<%# GetDocumentScopeIcon(Eval("IdDuAn")) %>'></i>
                                    <%#: GetDocumentScopeText(Eval("IdDuAn"), Eval("MaDuAn"), Eval("TenDuAn")) %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField
                            HeaderText="Loại tài liệu"
                            SortExpression="TenLoai"
                            HeaderStyle-CssClass="document-list-type-column text-center"
                            ItemStyle-CssClass="document-list-type-column">
                            <ItemTemplate>
                                <%#: GetDocumentTypeText(Eval("TenNhom"), Eval("TenLoai")) %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField
                            HeaderText="Người phụ trách"
                            SortExpression="TenNhanVienPhuTrach"
                            HeaderStyle-CssClass="document-list-responsible-column text-center"
                            ItemStyle-CssClass="document-list-responsible-column">
                            <ItemTemplate>
                                <%#: GetResponsibleEmployeeText(Eval("TenNhanVienPhuTrach")) %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField
                            HeaderText="Trạng thái hồ sơ"
                            SortExpression="TrangThaiTaiLieu"
                            HeaderStyle-Width="145px"
                            HeaderStyle-CssClass="document-list-status-column text-center"
                            ItemStyle-CssClass="document-list-status-column text-center">
                            <ItemTemplate>
                                <span class='<%# GetDocumentStatusCss(Eval("TrangThaiTaiLieu")) %>'>
                                    <%#: GetDocumentStatusText(Eval("TrangThaiTaiLieu")) %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField
                            HeaderText="Trình ký"
                            HeaderStyle-Width="130px"
                            HeaderStyle-CssClass="document-list-secondary-column"
                            ItemStyle-CssClass="document-list-secondary-column">
                            <ItemTemplate>
                                <%#: GetSigningText(Eval("CanTrinhKy"), Eval("HinhThucKy")) %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField
                            HeaderText="Gửi khách"
                            SortExpression="TrangThaiGuiKhach"
                            HeaderStyle-Width="125px"
                            HeaderStyle-CssClass="document-list-secondary-column"
                            ItemStyle-CssClass="document-list-secondary-column">
                            <ItemTemplate>
                                <%#: GetCustomerStatusText(Eval("CanGuiKhachHang"), Eval("TrangThaiGuiKhach")) %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField
                            HeaderText="Lưu bản cứng"
                            SortExpression="TrangThaiLuuTru"
                            HeaderStyle-Width="125px"
                            HeaderStyle-CssClass="document-list-secondary-column"
                            ItemStyle-CssClass="document-list-secondary-column">
                            <ItemTemplate>
                                <%#: GetPhysicalStorageStatusText(Eval("CanLuuVatLy"), Eval("TrangThaiLuuTru")) %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField
                            HeaderText="File chính thức"
                            HeaderStyle-CssClass="document-list-secondary-column"
                            ItemStyle-CssClass="document-list-secondary-column">
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
                            HeaderStyle-Width="120px"
                            HeaderStyle-CssClass="text-center"
                            ItemStyle-CssClass="text-center">
                            <ItemTemplate>
                                <%# ConvertDateTimeToString(Eval("NgayTao"), false) %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField
                            HeaderText="Thao tác"
                            HeaderStyle-CssClass="document-list-actions text-center"
                            ItemStyle-CssClass="document-list-actions text-center">

                            <ItemTemplate>
                                <div class="document-row-actions">
                                <SweetSoft:SmartLinkButton
                                    runat="server"
                                    ID="btnViewRow"
                                    CommandName="VIEW_ITEM"
                                    CommandArgument='<%# Eval("IdTaiLieu") %>'
                                    CausesValidation="false"
                                    VisibleConditionKey='<%# CanAccessRow(Eval("IdTaiLieu"), "View") %>'
                                    ResourceKey='<%# BackEndResourceKeys.VIEW %>'
                                    ButtonIcon="fas fa-eye">
                                </SweetSoft:SmartLinkButton>

                                <SweetSoft:SmartLinkButton
                                    runat="server"
                                    ID="btnEditRow"
                                    CommandName="EDIT_ITEM"
                                    CommandArgument='<%# Eval("IdTaiLieu") %>'
                                    CausesValidation="false"
                                    VisibleConditionKey='<%# CanAccessRow(Eval("IdTaiLieu"), "Update") %>'
                                    ResourceKey='<%# BackEndResourceKeys.EDIT %>'
                                    ButtonIcon="fas fa-pencil-alt">
                                </SweetSoft:SmartLinkButton>

                                <SweetSoft:SmartLinkButton
                                    runat="server"
                                    ID="btnDeleteRow"
                                    CommandName="DELETE_ITEM"
                                    CommandArgument='<%# Eval("IdTaiLieu") %>'
                                    CausesValidation="false"
                                    VisibleConditionKey='<%# CanAccessRow(Eval("IdTaiLieu"), "Delete") %>'
                                    ResourceKey='<%# BackEndResourceKeys.DELETE %>'
                                    ButtonIcon="fas fa-trash">
                                </SweetSoft:SmartLinkButton>
                                </div>
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

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

<style>
    .document-list-grid thead th,
    .document-list-grid tbody td { white-space: normal !important; overflow-wrap: anywhere; }
    .document-list-grid .document-list-code-column { width: 150px; }
    .document-list-grid .document-list-scope-column { width: 180px; }
    .document-list-grid .document-list-actions { width: 1%; white-space: nowrap !important; }
    .document-list-grid .document-list-actions .document-row-actions { display: flex; align-items: center; justify-content: center; gap: .25rem; flex-wrap: nowrap; white-space: nowrap; }
    .document-list-secondary-column { display: none !important; }
    .document-project-context .document-list-scope-column { display: none !important; }
    @media (max-width: 1199.98px) {
        .document-list-grid { min-width: 1100px; }
    }

    /* ExtraModal inserts an UpdatePanel between dialog and content. Size that wrapper too. */
    #<%= dlDetail.ClientID %> .modal-dialog {
        height: calc(100vh - 24px);
        height: calc(100dvh - 24px);
        min-height: 0;
        margin: 12px auto;
        max-width: min(960px, calc(100vw - 24px));
    }
    #<%= dlDetail.ClientID %> .modal-dialog > div {
        display: flex;
        flex-direction: column;
        width: 100%;
        max-height: 100%;
        min-height: 0;
    }
    #<%= dlDetail.ClientID %> .modal-content {
        display: flex;
        flex-direction: column;
        max-height: 100%;
        min-height: 0;
        overflow: hidden;
    }
    #<%= dlDetail.ClientID %> .modal-header,
    #<%= dlDetail.ClientID %> .modal-footer { flex: 0 0 auto; }
    #<%= dlDetail.ClientID %> .modal-body {
        flex: 1 1 auto;
        min-height: 0;
        overflow-y: auto;
        overflow-x: hidden;
    }
    #<%= dlDetail.ClientID %> .document-content-editor { min-width: 0; }
    #<%= dlDetail.ClientID %> .cke { max-width: 100%; }
    #<%= dlDetail.ClientID %> .document-file-picker { min-width: 0; }
    #<%= dlDetail.ClientID %> .document-file-picker-input {
        position: absolute;
        width: 1px;
        height: 1px;
        padding: 0;
        margin: -1px;
        overflow: hidden;
        clip: rect(0, 0, 0, 0);
        white-space: nowrap;
        border: 0;
    }
    #<%= dlDetail.ClientID %> .document-file-picker-input:focus + .document-file-picker-actions label {
        outline: 2px solid var(--bs-primary);
        outline-offset: 2px;
    }
    #<%= dlDetail.ClientID %> .document-file-selected-list:empty { display: none; }
    #<%= dlDetail.ClientID %> .document-file-selected-row { min-width: 0; }
    #<%= dlDetail.ClientID %> .document-file-selected-name {
        min-width: 0;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }
</style>
<SweetSoft:ExtraModal
    runat="server"
    ID="dlDetail"
    Type="Primary"
    Size="Large"
    Position="modal-dialog-centered modal-dialog-scrollable"
    BodyClass="document-modal-body"
    DefaultButton="btnSave"
    FooterButtonClose="false">

    <ContentTemplate>
            <asp:Panel
                runat="server"
                ID="pnlForm"
                CssClass="js-document-form validationEngineContainer">

                <asp:HiddenField
                    runat="server"
                    ID="hdfIdTaiLieu" />


                <div class="row">

                    <asp:Panel runat="server" ID="pnlCreateProject" CssClass="col-12 mb-3">
                        <label class="form-label">Dự án</label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlCreateProject"
                            ValueIsOfTypeGUID="true" SimpleInit="true" AlowClear="true" />
                        <div class="form-text">Hồ sơ công ty chỉ dành cho tài khoản có quyền tạo trên toàn hệ thống.</div>
                        <asp:Panel runat="server" ID="pnlCreateUnavailable"
                            CssClass="alert alert-info mt-2 mb-0" Visible="false">
                            <i class="fas fa-info-circle me-1"></i>
                            Bạn chưa được phân làm PM của dự án nào nên chưa thể tạo hồ sơ.
                            Hãy liên hệ người quản lý để được phân công dự án.
                        </asp:Panel>
                    </asp:Panel>

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

                    <div class="col-md-4 mb-3" runat="server" visible="false">
                        <label class="form-label label-valid">
                            <%= GetResourceText(BackEndResourceKeys.DOCUMENT_GROUP) %>
                        </label>

                        <SweetSoft:ExtraDropdown
                            runat="server"
                            ID="ddlNhomTaiLieu" Visible="false"
                            Required="false"
                            ValueIsOfTypeGUID="true"
                            SimpleInit="true"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="ddlNhomTaiLieu_SelectedIndexChanged">
                        </SweetSoft:ExtraDropdown>
                    </div>

                    <div class="col-md-4 mb-3">
                        <label class="form-label label-valid">
                            <%= "Loại hồ sơ" %>
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

                    <div class="col-md-4 mb-3">
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

                    <SweetSoft:ExtraTextBox runat="server" ID="txtMoTa" Visible="false" TextMode="MultiLine" MaxLength="1000" />
                    <div class="col-12 mb-3 document-content-editor">
                        <label class="form-label">Nội dung hồ sơ</label>
                        <p class="text-muted small">Có thể soạn nội dung tại đây mà không cần tải file. File đính kèm được quản lý riêng trong bộ hồ sơ.</p>
                        <asp:HiddenField runat="server" ID="hdfDocumentContent" />
                        <textarea id="<%= ClientID %>_contentEditor" class="form-control" rows="8" aria-label="Nội dung hồ sơ"></textarea>
                        <div class="text-muted small mt-1">Có thể định dạng chữ, màu sắc, căn lề, danh sách và bảng. Ảnh/tài liệu đưa vào bộ file đính kèm.</div>
                    </div>

                    <asp:Panel runat="server" ID="pnlInitialFileUpload" CssClass="col-12 mb-3">
                        <label class="form-label">File hồ sơ</label>
                        <div class="document-file-picker border rounded p-3">
                        <asp:FileUpload runat="server" ID="fuInitialFiles" AllowMultiple="true"
                            CssClass="document-file-picker-input"
                            accept=".pdf,.doc,.docx,.xls,.xlsx,.jpg,.jpeg,.png,.gif,.webp" />
                        <div class="d-flex flex-wrap align-items-center gap-2 document-file-picker-actions">
                            <label class="btn btn-outline-primary mb-0" for="<%= fuInitialFiles.ClientID %>">
                                <i class="fas fa-paperclip me-1" aria-hidden="true"></i>
                                <span id="<%= ClientID %>_filePickerButtonText">Chọn file</span>
                            </label>
                            <span id="<%= ClientID %>_filePickerCount" class="text-muted small">Chưa có file nào được chọn.</span>
                        </div>
                        <div id="<%= ClientID %>_initialFileList" class="document-file-selected-list list-group list-group-flush mt-3" aria-live="polite"></div>
                        <div class="form-text mt-2">Có thể chọn tối đa 10 file, mỗi file 10 MB. Chọn loại hồ sơ trước khi chọn file.</div>
                        </div>
                    </asp:Panel>

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
                                    <div class="col-xl-3 col-md-6 mb-3">
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
                                        class="col-xl-3 col-md-6 mb-3">
                                        <label class="form-label">
                                            <%= GetResourceText(BackEndResourceKeys.SIGNING_METHOD) %>
                                        </label>
                                        <SweetSoft:ExtraDropdown
                                            runat="server"
                                            ID="ddlHinhThucKy"
                                            SimpleInit="true">
                                        </SweetSoft:ExtraDropdown>
                                    </div>

                                    <div class="col-xl-3 col-md-6 mb-3">
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

                                    <div class="col-xl-3 col-md-6 mb-3">
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


                    <asp:Panel
                        runat="server"
                        ID="pnlInitialContent"
                        CssClass="col-12 mb-3">
                        <div class="card border shadow-none mb-0">
                            <div class="card-body">
                                <h6 class="text-primary mb-3">
                                    <%= GetResourceText(BackEndResourceKeys.INITIAL_DOCUMENT_CONTENT) %>
                                </h6>

                                <div class="d-flex flex-wrap gap-4 mb-3">
                                    <asp:RadioButton
                                        runat="server"
                                        ID="rbInitialUpload"
                                        GroupName="InitialDocumentSource"
                                        AutoPostBack="true"
                                        OnCheckedChanged="initialSource_CheckedChanged"
                                        CssClass="form-check" />

                                    <asp:RadioButton
                                        runat="server"
                                        ID="rbInitialTemplate"
                                        GroupName="InitialDocumentSource"
                                        AutoPostBack="true"
                                        OnCheckedChanged="initialSource_CheckedChanged"
                                        CssClass="form-check" />
                                </div>

                                <asp:Panel
                                    runat="server"
                                    ID="pnlInitialUploadInfo"
                                    CssClass="alert alert-info py-2 mb-0">
                                    <i class="fas fa-info-circle me-1"></i>
                                    <%= GetResourceText(BackEndResourceKeys.UPLOAD_AFTER_SAVE_NOTICE) %>
                                </asp:Panel>

                                <asp:Panel
                                    runat="server"
                                    ID="pnlInitialTemplate">
                                    <label class="form-label label-valid">
                                        <%= GetResourceText(BackEndResourceKeys.SELECT_DOCUMENT_TEMPLATE) %>
                                    </label>

                                    <SweetSoft:ExtraDropdown
                                        runat="server"
                                        ID="ddlInitialTemplate"
                                        ValueIsOfTypeGUID="true"
                                        SimpleInit="true">
                                    </SweetSoft:ExtraDropdown>

                                    <asp:Panel
                                        runat="server"
                                        ID="pnlNoInitialTemplates"
                                        CssClass="alert alert-warning py-2 mt-2 mb-0">
                                        <i class="fas fa-exclamation-triangle me-1"></i>
                                        <%= GetResourceText(BackEndResourceKeys.NO_TEMPLATE_FOR_DOCUMENT_TYPE) %>
                                    </asp:Panel>
                                </asp:Panel>
                            </div>
                        </div>
                    </asp:Panel>

                </div>
            </asp:Panel>
    </ContentTemplate>

    <FooterTemplate>
        <div class="d-flex gap-2">
                    <SweetSoft:ExtraButton
                        runat="server"
                        ID="btnSave"
                        OnClick="btnSave_Click"
                        OnClientClick="syncDocumentContentEditor(); return CMSMasterJs.ValidElement(
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
    </FooterTemplate>
</SweetSoft:ExtraModal>

<script type="text/javascript">
    (function () {
        var inputId = '<%= fuInitialFiles.ClientID %>';
        var listId = '<%= ClientID %>_initialFileList';
        var buttonTextId = '<%= ClientID %>_filePickerButtonText';
        var countId = '<%= ClientID %>_filePickerCount';

        function bindInitialFilePicker() {
            var input = document.getElementById(inputId);
            var list = document.getElementById(listId);
            var buttonText = document.getElementById(buttonTextId);
            var count = document.getElementById(countId);
            if (!input || !list || !buttonText || !count
                || input.getAttribute('data-file-picker-bound') === 'true')
                return;

            input.setAttribute('data-file-picker-bound', 'true');
            var previousFiles = [];

            input.addEventListener('click', function () {
                previousFiles = Array.prototype.slice.call(input.files || []);
            });

            input.addEventListener('change', function () {
                var selectedFiles = Array.prototype.slice.call(input.files || []);
                var mergedFiles = previousFiles.concat(selectedFiles);

                if (mergedFiles.length > 10) {
                    window.alert('Chỉ được chọn tối đa 10 file cho một hồ sơ.');
                    mergedFiles = mergedFiles.slice(0, 10);
                }

                try {
                    var transfer = new DataTransfer();
                    mergedFiles.forEach(function (file) { transfer.items.add(file); });
                    input.files = transfer.files;
                } catch (error) {
                    // Keep the browser's current selection if FileList cannot be rebuilt.
                    mergedFiles = selectedFiles;
                }

                renderInitialFiles(input, list, buttonText, count);
            });

            renderInitialFiles(input, list, buttonText, count);
        }

        function renderInitialFiles(input, list, buttonText, count) {
            var files = Array.prototype.slice.call(input.files || []);
            while (list.firstChild)
                list.removeChild(list.firstChild);

            buttonText.textContent = files.length > 0 ? 'Chọn thêm file' : 'Chọn file';
            count.textContent = files.length > 0
                ? files.length + ' / 10 file đã chọn'
                : 'Chưa có file nào được chọn.';

            files.forEach(function (file, index) {
                var row = document.createElement('div');
                row.className = 'list-group-item px-0 py-2 d-flex align-items-center gap-2 document-file-selected-row';

                var icon = document.createElement('i');
                icon.className = 'fas fa-file-alt text-primary';
                icon.setAttribute('aria-hidden', 'true');

                var details = document.createElement('div');
                details.className = 'flex-grow-1 document-file-selected-name';
                details.title = file.name;

                var name = document.createElement('div');
                name.className = 'text-body text-truncate';
                name.textContent = file.name;

                var size = document.createElement('div');
                size.className = 'text-muted small';
                size.textContent = formatInitialFileSize(file.size);
                details.appendChild(name);
                details.appendChild(size);

                var remove = document.createElement('button');
                remove.type = 'button';
                remove.className = 'btn btn-outline-danger btn-sm flex-shrink-0';
                remove.setAttribute('aria-label', 'Bỏ file ' + file.name);
                remove.title = 'Bỏ file này';
                remove.innerHTML = '<i class="fas fa-trash" aria-hidden="true"></i>';
                remove.addEventListener('click', function () {
                    var transfer = new DataTransfer();
                    Array.prototype.slice.call(input.files || []).forEach(function (selected, selectedIndex) {
                        if (selectedIndex !== index)
                            transfer.items.add(selected);
                    });
                    input.files = transfer.files;
                    renderInitialFiles(input, list, buttonText, count);
                });

                row.appendChild(icon);
                row.appendChild(details);
                row.appendChild(remove);
                list.appendChild(row);
            });
        }

        function formatInitialFileSize(bytes) {
            if (bytes < 1024)
                return bytes + ' B';
            if (bytes < 1024 * 1024)
                return (bytes / 1024).toFixed(1) + ' KB';
            return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
        }

        bindInitialFilePicker();
        if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(bindInitialFilePicker);
        }
    })();
</script>

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
                                    Opens="Left"
                                    Drops="Down"
                                    AutoUpdateInput="false" />
                            </div>

                        </div>
                    </asp:Panel>

                </ContentTemplate>
            </asp:UpdatePanel>

        </div>
    </div>
</div>

<script src="<%= ResolveUrl("~/Styles/plugins/ckeditor/ckeditor.js") %>"></script>
<script type="text/javascript">
    function syncDocumentContentEditor() {
        var element = document.getElementById('<%= ClientID %>_contentEditor');
        var hidden = document.getElementById('<%= hdfDocumentContent.ClientID %>');
        if (!element || !hidden) return;
        var editor = window.CKEDITOR && CKEDITOR.instances[element.id];
        var value = editor ? editor.getData() : element.value;
        hidden.value = btoa(unescape(encodeURIComponent(value)));
    }

    function initDocumentContentEditor() {
        var id = '<%= ClientID %>_contentEditor';
        var element = document.getElementById(id);
        var hidden = document.getElementById('<%= hdfDocumentContent.ClientID %>');
        if (!element || !hidden || element.getAttribute('data-content-ready')) return;
        if (window.CKEDITOR && CKEDITOR.instances[id]) CKEDITOR.instances[id].destroy(true);
        element.value = hidden.value ? decodeURIComponent(escape(atob(hidden.value))) : '';
        element.setAttribute('data-content-ready', 'true');
        element.addEventListener('input', syncDocumentContentEditor);
        if (!window.CKEDITOR) return;
        CKEDITOR.replace(id, {
            customConfig: '', height: 200, width: '100%', resize_enabled: false,
            language: 'vi', entities: false, basicEntities: true,
            allowedContent: 'p div span strong b em i u s sub sup ul ol li blockquote h1 h2 h3 h4 h5 h6 table thead tbody tfoot tr th td br hr pre code{text-align,margin-left,font-family,font-size,color,background-color}; td th[colspan,rowspan]; a[!href]',
            toolbar: [
                ['Undo', 'Redo', 'PasteText', 'PasteFromWord'], ['Find', 'Replace', 'SelectAll'],
                ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', 'RemoveFormat'], '/',
                ['Format', 'Font', 'FontSize'], ['TextColor', 'BGColor'],
                ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
                ['NumberedList', 'BulletedList', 'Outdent', 'Indent', 'Blockquote'], ['Link', 'Unlink', 'Table', 'HorizontalRule', 'SpecialChar']
            ],
            on: {
                instanceReady: function (event) { event.editor.dataProcessor.writer.selfClosingEnd = ' />'; },
                change: syncDocumentContentEditor
            }
        });
    }
    if (window.Sys && Sys.Application) Sys.Application.add_load(initDocumentContentEditor);
    else document.addEventListener('DOMContentLoaded', initDocumentContentEditor);

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
