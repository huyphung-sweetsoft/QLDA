<%@ Control Language="C#" AutoEventWireup="true" 
    CodeBehind="CtrlNhanViens.ascx.cs" 
    Inherits="SweetSoft.QLDA.BackOffice.fNhanVien.Controls.CtrlNhanViens" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<div class="card-header">
    <div class="d-flex flex-column flex-xl-row gap-3">
        <asp:UpdatePanel runat="server" ID="upnlSearchDefault" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel runat="server" ID="pnlSearchDefault">
                    <div class="d-flex">
                        <SweetSoft:BootstrapDropdown ID="ddlSearchPhongBan" runat="server"
                            Text="Phòng Ban"
                            AllowClear="true"
                            AutoPostBack="true"
                            SearchColumn="IdPhongBan"
                            EnableSearch="true"
                            ValueIsOfTypeGUID="True"
                            SearchPlaceholder="Tìm kiếm theo tên phòng ban..."
                            NoResultsText="Không tìm thấy tên phòng ban"  
                            CssClass="border-top-left-radius-1 border-bottom-left-radius-1"
                            OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged">
                        </SweetSoft:BootstrapDropdown>
                        <SweetSoft:BootstrapDropdown ID="ddlSearchChucDanh" runat="server"
                            Text="Chức Danh"
                            AutoPostBack="true"
                            AllowClear="true"
                            SearchColumn="IdChucDanh"
                            EnableSearch="true"
                            ValueIsOfTypeGUID="True"
                            SearchPlaceholder="Tìm kiếm theo tên chức danh..."
                            NoResultsText="Không tìm thấy tên chức danh"
                            CssClass="border-top-right-radius-1 border-bottom-right-radius-1"
                            OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged">
                        </SweetSoft:BootstrapDropdown>
                    </div> 
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        <div class="input-group max-w-500">
            <a class="btn btn-info font-mobile-small btn-search-filter" onclick="CMSMasterJs.ShowOffcanvasSearch();" href="javascript:;">
                <i class='fas fa-filter me-1'></i><%= GetResourceText(BackEndResourceKeys.FILTER) %>
            </a>
            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" PlaceHolder="Nhập từ khóa tìm kiếm..." CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
        </div>
        <div runat="server" id="tagOther" visible="true" class="d-flex justify-content-end gap-3 w-full flex-wrap">
            <asp:UpdatePanel runat="server" ID="pnlButtons" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="d-flex">
                        <SweetSoft:ExtraButton runat="server" ID="btnExport" OnClick="btnExport_Click" ButtonStyle="OutLineInfo"
                            CssClass="waves-effect waves-light flex-btn font-mobile-small me-2" ButtonIcon="Excel" IsSubmit="false" Visible="false">Export Excel</SweetSoft:ExtraButton>
                        <SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click" CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Info" ButtonIcon="Add" Visible="false">Add new</SweetSoft:ExtraButton>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:PostBackTrigger ControlID="btnExport" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </div>
    <div class="listSearchTagBox">
        <asp:UpdatePanel ID="upSearchTagBox" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <SweetSoft:ExtraSearchBox ID="searchTagBox" runat="server" OnTagClosed="searchTagBox_TagClosed"></SweetSoft:ExtraSearchBox>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</div>
<div class="card-body p-0">
    <asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <SweetSoft:GridviewExtension ID="grvData" runat="server"
                AllowSorting="true"
                ShowHeader="true"
                ShowHeaderWhenEmpty="true"
                AutoGenerateColumns="false"
                CssClass="table-bordered table-hover"
                FocusBtnIcon="fas fa-compress-arrows-alt"
                DataKeyNames="IdNhanVien" GridLines="None"
                IsEnableSelectColumn="false"
                OnNeedDataSource="grvData_NeedDataSource"
                OnRowCommand="grvData_RowCommand">
          <Columns>
    <asp:TemplateField HeaderText="Nhân viên" HeaderStyle-CssClass="text-center" SortExpression="TenNhanVien" ItemStyle-CssClass="text-left">
        <ItemTemplate>
            <div class="d-flex align-items-center">
                <img src='<%# !string.IsNullOrEmpty(Convert.ToString(Eval("AnhDaiDien"))) ? Eval("AnhDaiDien") : "/Styles/images/user-icon.png" %>' 
                     class="avatar-sm rounded-circle me-3" 
                     style="width: 45px; height: 45px; object-fit: cover;" 
                     onerror="this.src='/Styles/images/user-icon.png'">
                <div>
                    <asp:LinkButton runat="server" CssClass="card-link fw-bold d-block text-primary" Visible='<%# this.IsEdit %>'
                        ID="lbtView" CommandName="VIEW_DETAIL" Text='<%# Eval("TenNhanVien")%>'></asp:LinkButton>
                    <span runat="server" id="tagName" class="fw-bold d-block text-primary" visible='<%# !this.IsEdit %>'><%# Eval("TenNhanVien") %></span>
                    <small class="text-muted"><%# Eval("GioiTinh") %></small>
                </div>
            </div>
        </ItemTemplate>
    </asp:TemplateField>
    <asp:TemplateField HeaderText="Liên hệ" HeaderStyle-CssClass="text-center" SortExpression="Email" ItemStyle-CssClass="text-left">
        <ItemTemplate>
            <div class="d-block mb-1" title="Email">
                <i class="fas fa-envelope text-muted me-1"></i> <%# Eval("Email") %>
            </div>
            <div class="d-block text-muted" title="Số điện thoại">
                <i class="fas fa-phone-alt me-1"></i> <%# Eval("PhoneNumber") %>
            </div>
        </ItemTemplate>
    </asp:TemplateField>
    <asp:TemplateField HeaderText="Phòng ban" HeaderStyle-CssClass="text-center" SortExpression="TenPhongBan" ItemStyle-CssClass="text-left">
        <ItemTemplate><%# Eval("TenPhongBan") %></ItemTemplate>
    </asp:TemplateField>

    <asp:TemplateField HeaderText="Chức danh" HeaderStyle-CssClass="text-center" SortExpression="TenChucDanh" ItemStyle-CssClass="text-left">
        <ItemTemplate><%# Eval("TenChucDanh") %></ItemTemplate>
    </asp:TemplateField>

    <asp:TemplateField HeaderText="Định danh" HeaderStyle-CssClass="text-center" SortExpression="IdCCCD" ItemStyle-CssClass="text-left">
        <ItemTemplate>
            <div class="d-block mb-1">
                <strong>ID:</strong> <%# Eval("IdCCCD") %>
            </div>
            <div class="d-block text-muted" title="Ngày sinh">
                <i class="fas fa-birthday-cake me-1"></i> <%# this.ConvertDateTimeToString(Eval("NgaySinh"), false) %>
            </div>
        </ItemTemplate>
    </asp:TemplateField>
    <asp:TemplateField HeaderStyle-Width="120px" ItemStyle-CssClass="text-center" HeaderText="Join date" HeaderStyle-CssClass="text-center" SortExpression="NgayGiaNhap">
        <ItemTemplate>
            <%# this.ConvertDateTimeToString(Eval("NgayGiaNhap"), false) %>
        </ItemTemplate>
    </asp:TemplateField>
    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="100px">
        <ItemTemplate>
            <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsView %>'
                ID="lbtDetail" CommandName="ITEM_DETAIL" CssClass="btn-grid-action text-decoration-underline"
                ResourceKey='<%# this.IsEdit ? BackEndResourceKeys.EDIT : BackEndResourceKeys.VIEW %>'
                ButtonIcon='<%# this.IsEdit ? "fas fa-pencil-alt" : "fas fa-eye" %>'>
            </SweetSoft:SmartLinkButton>
            <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsDelete %>'
                ID="lbtDelete" CommandName="ITEM_DELETE" CssClass="btn-grid-action text-decoration-underline text-danger"
                ResourceKey='<%# BackEndResourceKeys.DELETE %>'
                ButtonIcon="fas fa-trash">
            </SweetSoft:SmartLinkButton>
        </ItemTemplate>
    </asp:TemplateField>
</Columns>
                <EmptyDataTemplate>
                    <%= GetResourceText(BackEndResourceKeys.NO_DATA) %>
                </EmptyDataTemplate>
            </SweetSoft:GridviewExtension>
            <SweetSoft:Paging runat="server" ID="ctrlGridviewPaging" OnPageChanged="ctrlGridviewPaging_PageChanged" />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
<div class="offcanvas offcanvas-end offcanvas-form-search" id="search-offcanvas" aria-hidden="true">
    <div class="offcanvas-header">
        <div class="flex flex-column flex-md-row align-items-center gap-3">
            <h5 class="offcanvas-title"><%= GetResourceText(BackEndResourceKeys.ADVANCED_SEARCH) %></h5>
            <div class="d-flex align-items-center gap-1">
                <SweetSoft:ExtraButton runat="server" ID="lbtSearchAdvanced" CssClass="flex-btn" ButtonStyle="Primary" ButtonIcon="Search" OnClick="btnSearchAdvanced_ServerClick">Search</SweetSoft:ExtraButton>
                <SweetSoft:ExtraButton runat="server" ID="lbtCancel" CssClass="flex-btn" ButtonStyle="OutLineSecondary" ButtonIcon="Refresh" OnClick="btnCancel_Click">Refresh</SweetSoft:ExtraButton>
            </div>
        </div>
        <button class="btn-close" type="button" data-bs-dismiss="offcanvas" aria-label="Close"></button>
    </div>
    <div class="div offcanvas-body pt-0">
        <div class="card shadow-none card-body text-muted mb-0">
            <asp:UpdatePanel runat="server" ID="pnlSearch" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel runat="server" ID="pnlSearchPopup">
                        <div class="row">
                            <div class="col-md-6 mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_NAME) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchTenNhanVien" SearchColumn="TenNhanVien" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label class="form-label"><%=GetResourceText(BackEndResourceKeys.EMPLOYEE_CCCD) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchIdCCCD" SearchColumn="IdCCCD" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label class="form-label">Email</label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchEmail" SearchColumn="Email" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchPhone" SearchColumn="PhoneNumber" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                            </div>
                            <div runat="server" visible="false" class="col-md-6 mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CREATED_DATE) %></label>
                                <SweetSoft:ExtraDateTime runat="server" ID="txtSearchCreatedDate" SearchColumn="CreatedDate" SingleDatePicker="false" IsPredefinedDateRanges="true" AutoUpdateInput="false" AutoApply="true" />
                            </div>
                        </div>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</div>
