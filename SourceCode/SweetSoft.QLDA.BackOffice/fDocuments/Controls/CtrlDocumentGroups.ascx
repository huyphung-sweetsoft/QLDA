<%@ Control Language="C#"
    AutoEventWireup="true"
    CodeBehind="CtrlDocumentGroups.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.fDocuments.Controls.CtrlDocumentGroups" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

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

                        <SweetSoft:BootstrapDropdown
                            runat="server"
                            ID="ddlSearchStatus"
                            Text="Trạng thái"
                            AllowClear="true"
                            AutoPostBack="true"
                            SearchColumn="KichHoat"
                            CssClass="border-top-left-radius-1 border-bottom-left-radius-1 border-top-right-radius-1 border-bottom-right-radius-1"
                            OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged">
                        </SweetSoft:BootstrapDropdown>
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
            <div class="table-responsive">
                <SweetSoft:GridviewExtension
                    runat="server"
                    ID="grvData"
                    AllowSorting="true"
                    ShowHeader="true"
                    ShowHeaderWhenEmpty="true"
                    AutoGenerateColumns="false"
                    DataKeyNames="IdNhomTaiLieu"
                    GridLines="None"
                    CssClass="table-bordered table-hover"
                    IsEnableSelectColumn="false"
                    FocusBtnIcon="fas fa-compress-arrows-alt"
                    OnNeedDataSource="grvData_NeedDataSource"
                    OnRowCommand="grvData_RowCommand">

                    <Columns>
                        <asp:BoundField
                            DataField="TenNhom"
                            HeaderText="Tên nhóm tài liệu"
                            SortExpression="TenNhom" />

                        <asp:TemplateField
                            HeaderText="Mô tả"
                            SortExpression="MoTa">
                            <ItemTemplate>
                                <%#: Eval("MoTa") %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField
                            DataField="ThuTuHienThi"
                            HeaderText="Thứ tự"
                            SortExpression="ThuTuHienThi"
                            ItemStyle-CssClass="text-center"
                            HeaderStyle-Width="90px" />

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
                            HeaderText="Ngày tạo"
                            SortExpression="NgayTao"
                            ItemStyle-CssClass="text-end"
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
                                <SweetSoft:SmartLinkButton
                                    runat="server"
                                    ID="btnEditRow"
                                    CommandName="EDIT_ITEM"
                                    CommandArgument='<%# Eval("IdNhomTaiLieu") %>'
                                    CausesValidation="false"
                                    VisibleConditionKey='<%# this.IsEdit %>'
                                    ResourceKey='<%# BackEndResourceKeys.EDIT %>'
                                    ButtonIcon="fas fa-pencil-alt">
                                </SweetSoft:SmartLinkButton>

                                <SweetSoft:SmartLinkButton
                                    runat="server"
                                    ID="btnDeleteRow"
                                    CommandName="DELETE_ITEM"
                                    CommandArgument='<%# Eval("IdNhomTaiLieu") %>'
                                    CausesValidation="false"
                                    VisibleConditionKey='<%# this.IsDelete %>'
                                    ResourceKey='<%# BackEndResourceKeys.DELETE %>'
                                    ButtonIcon="fas fa-trash">
                                </SweetSoft:SmartLinkButton>
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

<SweetSoft:ExtraModal
    runat="server"
    ID="dlDetail"
    Type="Primary"
    Size="Large"
    DefaultButton="btnSave"
    FooterButtonClose="false">

    <ContentTemplate>
        <asp:Panel
            runat="server"
            ID="pnlForm"
            CssClass="js-document-group-form validationEngineContainer">

            <asp:HiddenField
                runat="server"
                ID="hdfIdNhomTaiLieu" />

            <div class="row">
                <div class="col-md-12 mb-3">
                    <label class="form-label label-valid">
                        <%= GetResourceText(BackEndResourceKeys.DOCUMENT_GROUP_NAME) %>
                    </label>

                    <SweetSoft:ExtraTextBox
                        runat="server"
                        ID="txtTenNhom"
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
            </div>
        </asp:Panel>
    </ContentTemplate>

    <FooterTemplate>
        <div class="d-flex gap-2">
            <SweetSoft:ExtraButton
                runat="server"
                ID="btnSave"
                OnClick="btnSave_Click"
                OnClientClick="return CMSMasterJs.ValidElement('.js-document-group-form');"
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
                            <div class="col-md-12 mb-3">
                                <label class="form-label">
                                    <%= GetResourceText(BackEndResourceKeys.DOCUMENT_GROUP_NAME) %>
                                </label>

                                <SweetSoft:ExtraTextBox
                                    runat="server"
                                    ID="txtSearchTenNhom"
                                    SearchColumn="TenNhom"
                                    PlaceHolder="Nhập tên nhóm tài liệu">
                                </SweetSoft:ExtraTextBox>
                            </div>

                            <div class="col-md-12 mb-3">
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
                        </div>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</div>
