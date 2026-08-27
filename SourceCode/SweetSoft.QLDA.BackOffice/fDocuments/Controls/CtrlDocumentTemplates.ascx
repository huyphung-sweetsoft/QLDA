<%@ Control Language="C#"
    AutoEventWireup="true"
    CodeBehind="CtrlDocumentTemplates.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.fDocuments.Controls.CtrlDocumentTemplates" %>
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
                                ID="ddlSearchLoaiTaiLieu"
                                Text="Loại tài liệu"
                                AllowClear="true"
                                AutoPostBack="true"
                                EnableSearch="true"
                                ValueIsOfTypeGUID="true"
                                SearchColumn="IdLoaiTaiLieu"
                                SearchPlaceholder="Tìm kiếm loại tài liệu..."
                                NoResultsText="Không tìm thấy loại tài liệu"
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
                            CssClass="border-primary input-search-filter"
                            PlaceHolder="Nhập tên, phiên bản hoặc mô tả">
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
                CssClass="js-document-template-form validationEngineContainer border rounded p-3 mb-4">

                <asp:HiddenField
                    runat="server"
                    ID="hdfIdMauTaiLieu" />

                <h5 class="text-primary mb-3">
                    <asp:Literal
                        runat="server"
                        ID="litFormTitle" />
                </h5>

                <div class="row">

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
                            PlaceHolder="Chọn loại tài liệu">
                        </SweetSoft:ExtraDropdown>
                    </div>

                    <div class="col-md-6 mb-3">
                        <label class="form-label label-valid">
                            <%= GetResourceText(BackEndResourceKeys.DOCUMENT_TEMPLATE_NAME) %>
                        </label>

                        <SweetSoft:ExtraTextBox
                            runat="server"
                            ID="txtTenMau"
                            Required="true"
                            MaxLength="200">
                        </SweetSoft:ExtraTextBox>
                    </div>

                    <div class="col-md-4 mb-3">
                        <label class="form-label label-valid">
                            <%= GetResourceText(BackEndResourceKeys.TEMPLATE_VERSION) %>
                        </label>

                        <SweetSoft:ExtraTextBox
                            runat="server"
                            ID="txtPhienBanMau"
                            Required="true"
                            MaxLength="20"
                            Text="1.0">
                        </SweetSoft:ExtraTextBox>
                    </div>

                    <div class="col-md-4 mb-3">
                        <label class="form-label">
                            <%= GetResourceText(BackEndResourceKeys.DEFAULT_TEMPLATE) %>
                        </label>

                        <div class="mt-2">
                            <SweetSoft:ExtraCheckbox
                                runat="server"
                                ID="chkLaMauMacDinh"
                                OnText="Có"
                                OffText="Không" />
                        </div>
                    </div>

                    <div class="col-md-4 mb-3">
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

                    <div class="col-md-12 mb-3">
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

                    <asp:Panel
                        runat="server"
                        ID="pnlTemplateFile"
                        Visible="false"
                        CssClass="col-md-12 mb-3">

                        <label class="form-label">
                            <%= GetResourceText(BackEndResourceKeys.TEMPLATE_FILE) %>
                        </label>

                        <div class="alert alert-info py-2">
                            <%= GetResourceText(BackEndResourceKeys.SAVE_TEMPLATE_BEFORE_UPLOAD) %>
                        </div>

                        <SweetSoft:FilesBox
                            runat="server"
                            ID="fbTemplate" />

                    </asp:Panel>

                </div>

                <div class="d-flex gap-2">
                    <SweetSoft:ExtraButton
                        runat="server"
                        ID="btnSave"
                        OnClick="btnSave_Click"
                        OnClientClick="return CMSMasterJs.ValidElement(
                            '.js-document-template-form');"
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
                    DataKeyNames="IdMauTaiLieu"
                    GridLines="None"
                    CssClass="table-bordered table-hover"
                    IsEnableSelectColumn="false"
                    FocusBtnIcon="fas fa-compress-arrows-alt"
                    OnNeedDataSource="grvData_NeedDataSource"
                    OnRowCommand="grvData_RowCommand">

                    <Columns>

                        <asp:BoundField
                            DataField="TenMau"
                            HeaderText="Tên mẫu tài liệu"
                            SortExpression="TenMau" />

                        <asp:TemplateField
                            HeaderText="Loại tài liệu"
                            SortExpression="TenLoai">
                            <ItemTemplate>
                                <%# GetDocumentTypeText(Eval("TenNhom"), Eval("TenLoai")) %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField
                            DataField="PhienBanMau"
                            HeaderText="Phiên bản"
                            SortExpression="PhienBanMau"
                            HeaderStyle-Width="100px" />

                        <asp:TemplateField
                            HeaderText="Mặc định"
                            SortExpression="LaMauMacDinh"
                            ItemStyle-CssClass="text-center"
                            HeaderStyle-Width="100px">
                            <ItemTemplate>
                                <%# GetYesNoText(Convert.ToBoolean(Eval("LaMauMacDinh"))) %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField
                            HeaderText="Tệp mẫu"
                            SortExpression="TenFile">
                            <ItemTemplate>
                                <asp:HyperLink
                                    runat="server"
                                    Visible='<%# HasTemplateFile(Eval("IdFileMau")) %>'
                                    NavigateUrl='<%# GetFileUrl(Eval("FileUrl")) %>'
                                    Text='<%# GetTemplateFileName(Eval("TenFileGoc"), Eval("TenFile")) %>'
                                    Target="_blank"
                                    CssClass="text-primary text-decoration-underline" />
                                <asp:Label
                                    runat="server"
                                    Visible='<%# !HasTemplateFile(Eval("IdFileMau")) %>'
                                    Text='<%# GetResourceText(BackEndResourceKeys.FILE_NOT_UPLOADED) %>'
                                    CssClass="badge bg-secondary" />
                            </ItemTemplate>
                        </asp:TemplateField>

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
                                    CommandArgument='<%# Eval("IdMauTaiLieu") %>'
                                    CausesValidation="false"
                                    Visible='<%# this.IsEdit %>'
                                    CssClass="btn btn-sm btn-outline-primary me-1"
                                    Text='<%# GetResourceText(BackEndResourceKeys.EDIT) %>'>
                                </asp:LinkButton>

                                <asp:LinkButton
                                    runat="server"
                                    ID="btnDeleteRow"
                                    CommandName="DELETE_ITEM"
                                    CommandArgument='<%# Eval("IdMauTaiLieu") %>'
                                    CausesValidation="false"
                                    Visible='<%# this.IsDelete %>'
                                    CssClass="btn btn-sm btn-outline-danger"
                                    OnClientClick="return confirm('Bạn có chắc muốn xóa mẫu tài liệu này?');"
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
                                    <%= GetResourceText(BackEndResourceKeys.DOCUMENT_TEMPLATE_NAME) %>
                                </label>

                                <SweetSoft:ExtraTextBox
                                    runat="server"
                                    ID="txtSearchTenMau"
                                    SearchColumn="TenMau">
                                </SweetSoft:ExtraTextBox>
                            </div>

                            <div class="col-md-6 mb-3">
                                <label class="form-label">
                                    <%= GetResourceText(BackEndResourceKeys.TEMPLATE_VERSION) %>
                                </label>

                                <SweetSoft:ExtraTextBox
                                    runat="server"
                                    ID="txtSearchPhienBanMau"
                                    SearchColumn="PhienBanMau">
                                </SweetSoft:ExtraTextBox>
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
                                    <%= GetResourceText(BackEndResourceKeys.DEFAULT_TEMPLATE) %>
                                </label>

                                <SweetSoft:ExtraDropdown
                                    runat="server"
                                    ID="ddlSearchLaMauMacDinh"
                                    SearchColumn="LaMauMacDinh"
                                    SimpleInit="true"
                                    AlowClear="true">
                                </SweetSoft:ExtraDropdown>
                            </div>

                            <div class="col-md-6 mb-3">
                                <label class="form-label">
                                    <%= GetResourceText(BackEndResourceKeys.TEMPLATE_FILE) %>
                                </label>

                                <SweetSoft:ExtraDropdown
                                    runat="server"
                                    ID="ddlSearchHasFile"
                                    SearchColumn="HasTemplateFile"
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
