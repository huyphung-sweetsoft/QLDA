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
                                                ID="ddlSearchScope"
                                                Text="Phạm vi hồ sơ"
                                                ClearText="Tất cả phạm vi"
                                                AllowClear="true"
                                                AutoPostBack="true"
                                                SearchColumn="PhamViHoSo"
                                                CssClass="border-top-right-radius-1 border-bottom-right-radius-1"
                                                OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged">
                                            </SweetSoft:BootstrapDropdown>

                                        </div>
                                    </asp:Panel>

                                    <%-- Tìm kiếm từ khóa --%>
                                    <div class="input-group max-w-500">

                                        <SweetSoft:ExtraTextBox
                                            runat="server"
                                            ID="txtSearch"
                                            CssClass="border-primary border-top-left-radius-1 border-bottom-left-radius-1"
                                            PlaceHolder="Nhập tên hoặc mô tả">
                                        </SweetSoft:ExtraTextBox>

                                        <SweetSoft:ExtraButton
                                            runat="server"
                                            ID="btnSearch"
                                            OnClick="btnSearch_Click"
                                            CssClass="btn-outline-primary"
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
                                            HeaderText="Tên loại hồ sơ"
                                            SortExpression="TenLoai" />

                                        <asp:BoundField
                                            DataField="TenNhom" Visible="false"
                                            HeaderText="Nhóm tài liệu"
                                            SortExpression="TenNhom" />

                                        <asp:TemplateField HeaderText="Phạm vi hồ sơ">
                                            <ItemTemplate><%#: Convert.ToString(Eval("PhamViHoSo")) == "DU_AN" ? "Hồ sơ dự án" : "Hồ sơ chung" %></ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField Visible="false"
                                            HeaderText="Trình ký"
                                            SortExpression="CanTrinhKy">
                                            <ItemTemplate>
                                                <%# GetSigningText(Eval("CanTrinhKy"), Eval("HinhThucKyMacDinh")) %>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField
                                            Visible="false" HeaderText="Gửi khách"
                                            SortExpression="CanGuiKhachHang"
                                            ItemStyle-CssClass="text-center">
                                            <ItemTemplate>
                                                <%# GetYesNoText(Convert.ToBoolean(Eval("CanGuiKhachHang"))) %>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField
                                            Visible="false" HeaderText="Lưu bản cứng"
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
                                                <SweetSoft:SmartLinkButton
                                                    runat="server"
                                                    ID="btnEditRow"
                                                    CommandName="EDIT_ITEM"
                                                    CommandArgument='<%# Eval("IdLoaiTaiLieu") %>'
                                                    CausesValidation="false"
                                                    VisibleConditionKey='<%# this.IsEdit %>'
                                                    ResourceKey='<%# BackEndResourceKeys.EDIT %>'
                                                    ButtonIcon="fas fa-pencil-alt">
                                                </SweetSoft:SmartLinkButton>

                                                <SweetSoft:SmartLinkButton
                                                    runat="server"
                                                    ID="btnDeleteRow"
                                                    CommandName="DELETE_ITEM"
                                                    CommandArgument='<%# Eval("IdLoaiTaiLieu") %>'
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

    <style>
        #<%= dlDetail.ClientID %> .modal-dialog { max-width: min(760px, calc(100vw - 24px)); }
        #<%= dlDetail.ClientID %> .js-document-type-form .mb-3 { margin-bottom: .8rem !important; }
    </style>
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
                CssClass="js-document-type-form validationEngineContainer">

                <asp:HiddenField
                    runat="server"
                    ID="hdfIdLoaiTaiLieu" />

                <div class="row">
                    <div class="col-md-6 mb-3" runat="server" visible="false">
                        <label class="form-label label-valid">
                            <%= GetResourceText(BackEndResourceKeys.DOCUMENT_GROUP) %>
                        </label>

                        <SweetSoft:ExtraDropdown
                            runat="server"
                            ID="ddlNhomTaiLieu" Visible="false"
                            Required="false"
                            ValueIsOfTypeGUID="true"
                            SimpleInit="true"
                            PlaceHolder="Chọn nhóm tài liệu">
                        </SweetSoft:ExtraDropdown>
                    </div>

                    <div class="col-12 mb-3">
                        <label class="form-label label-valid">
                            <%= "Tên loại hồ sơ" %>
                        </label>

                        <SweetSoft:ExtraTextBox
                            runat="server"
                            ID="txtTenLoai"
                            Required="true"
                            MaxLength="150">
                        </SweetSoft:ExtraTextBox>
                    </div>

                    <div class="col-md-6 mb-3">
                        <label class="form-label label-valid">Phạm vi áp dụng</label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlTypeScope" SimpleInit="true">
                            <asp:ListItem Value="DU_AN" Text="Hồ sơ dự án" />
                            <asp:ListItem Value="CHUNG" Text="Hồ sơ chung" />
                        </SweetSoft:ExtraDropdown>
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Nơi lưu trữ mặc định (không bắt buộc)</label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlDefaultStorage" SimpleInit="true" />
                    </div>
                    <%-- Preserve existing descriptions in ViewState without exposing an edit field. --%>
                    <SweetSoft:ExtraTextBox runat="server" ID="txtMoTa" Visible="false" />

                    <div class="col-sm-6 mb-3">
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

                    <div class="col-sm-6 mb-3">
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

                    <div class="col-md-3 mb-3" runat="server" visible="false">
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
                        id="divHinhThucKy" visible="false"
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

                    <div class="col-md-3 mb-3" runat="server" visible="false">
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

                    <div class="col-md-3 mb-3" runat="server" visible="false">
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
            </asp:Panel>
        </ContentTemplate>

        <FooterTemplate>
            <div class="d-flex gap-2">
                <SweetSoft:ExtraButton
                    runat="server"
                    ID="btnSave"
                    OnClick="btnSave_Click"
                    OnClientClick="return CMSMasterJs.ValidElement('.js-document-type-form');"
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
