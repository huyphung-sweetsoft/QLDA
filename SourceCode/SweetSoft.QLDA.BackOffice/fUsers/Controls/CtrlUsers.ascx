<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlUsers.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fUsers.Controls.CtrlUsers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<div class="card-header">
    <div class="d-flex flex-column flex-xl-row gap-3">
        <asp:UpdatePanel runat="server" ID="upnlSearchDefault" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel runat="server" ID="pnlSearchDefault">
                    <div class="d-flex">
                        <SweetSoft:BootstrapDropdown ID="ddlSearchStatus" runat="server"
                            Text="Trạng thái"
                            AllowClear="true"
                            AutoPostBack="true"
                            SearchColumn="IsActivated"
                            CssClass="border-top-left-radius-1 border-bottom-left-radius-1"
                            OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged">
                        </SweetSoft:BootstrapDropdown>
                        <SweetSoft:BootstrapDropdown ID="ddlSearchRole" runat="server"
                            Text="Nhóm người dùng"
                            AutoPostBack="true"
                            AllowClear="true"
                            SearchColumn="RoleId"
                            EnableSearch="true"
                            ValueIsOfTypeGUID="True"
                            SearchPlaceholder="Tìm kiếm nhóm người dùng..."
                            NoResultsText="Không tìm thấy nhóm người dùng"
                            OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged">
                        </SweetSoft:BootstrapDropdown>
                        <SweetSoft:BootstrapDropdown ID="ddlSearchLaNhanVien" runat="server"
                            Text="Loại tài khoản"
                            AutoPostBack="true"
                            AllowClear="true"
                            SearchColumn="LaNhanVien"
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
        <div runat="server" id="tagOther" visible="false" class="d-flex justify-content-end gap-3 w-full flex-wrap">
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
                DataKeyNames="UserId" GridLines="None"
                IsEnableSelectColumn="false"
                OnNeedDataSource="grvData_NeedDataSource"
                OnRowCommand="grvData_RowCommand">
                <Columns>
                    <asp:TemplateField HeaderText="Account" HeaderStyle-CssClass="text-center" SortExpression="UserName">
                        <ItemTemplate>
                            <!-- Bắt buộc cấm rớt dòng xuống dưới avatar bằng flex-nowrap -->
                            <div class="d-flex align-items-center flex-nowrap text-left">
            
                                <!-- 1. KHU VỰC AVATAR (Cố định, không co giãn, chứa Huy hiệu đè lên) -->
                                <div class="position-relative flex-shrink-0 me-3">
                                    <!-- Avatar -->
                                    <!-- Avatar có cơ chế Fallback an toàn -->
                                    <img src='<%# !string.IsNullOrEmpty(Convert.ToString(Eval("Avatar"))) ? Eval("Avatar") : "/Styles/images/user-icon.png" %>' 
                                         class="avatar-sm rounded-circle" 
                                         style="width: 42px; height: 42px; object-fit: cover;" 
                                         onerror="this.onerror=null; this.src='/Styles/images/user-icon.png'">
                
                                    <!-- Huy hiệu (Badge) đè góc phải dưới -->
                                    <span class="position-absolute bottom-0 start-100 translate-middle badge rounded-pill <%# Convert.ToBoolean(Eval("LaNhanVien")) ? "bg-success" : "bg-secondary" %>" 
                                          style="font-size: 0.6rem; padding: 0.3em 0.4em; border: 2px solid white;"
                                          title='<%# Convert.ToBoolean(Eval("LaNhanVien")) ? GetResourceText(BackEndResourceKeys.EMPLOYEE_ACCOUNT) : string.Format("{0}/{1}", GetResourceText(BackEndResourceKeys.SYSTEM_ACCOUNT), GetResourceText(BackEndResourceKeys.GUEST))%>' 
                                          data-bs-toggle="tooltip">
                                        <i class='<%# Convert.ToBoolean(Eval("LaNhanVien")) ? "fas fa-user-tie" : "fas fa-desktop" %>'></i>
                                    </span>
                                </div>
            
                                <!-- 2. KHU VỰC VĂN BẢN (Tự động chiếm phần còn lại, dài quá thì bẻ chữ) -->
                                <div class="flex-grow-1" style="min-width: 0;">
                                    <asp:LinkButton runat="server" CssClass="card-link fw-bold text-primary d-block text-break mb-0" Visible='<%# this.IsEdit %>'
                                        ID="lbtView" CommandName="ITEM_DETAIL" Text='<%# string.Format("{0} ({1})", Eval("UserName"), Eval("DisplayName")) %>'></asp:LinkButton>
                
                                    <span runat="server" id="tagName" class="fw-bold text-primary d-block text-break mb-0" visible='<%# !this.IsEdit %>'>
                                        <%# string.Format("{0} ({1})", Eval("UserName"), Eval("DisplayName")) %>
                                    </span>
                                </div>
            
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Email" HeaderStyle-CssClass="text-center" SortExpression="Email" ItemStyle-CssClass="text-left">
                        <ItemTemplate>
                            <%# Eval("Email") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="PhoneNumber" HeaderStyle-CssClass="text-center" SortExpression="MobileAlias" ItemStyle-CssClass="text-left">
                        <ItemTemplate>
                            <%# Eval("MobileAlias") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Role" HeaderStyle-CssClass="text-center" SortExpression="RoleName" ItemStyle-CssClass="text-left">
                        <ItemTemplate>
                            <%# Eval("RoleName") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="text-center" SortExpression="IsActivated" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# this.CURRENT_PAGE.GetStatusText(Eval("IsActivated")) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="2FA" HeaderStyle-CssClass="text-center" SortExpression="AuthenticatorKey" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <img width="20" class="ignore" src="<%# !string.IsNullOrEmpty(Eval("AuthenticatorKey").ToString()) ? "/Styles/images/check.png" : "/Styles/images/close.png"  %>" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-Width="100px" ItemStyle-CssClass="text-end" HeaderText="Create date" HeaderStyle-CssClass="text-center" SortExpression="LastActivityDate">
                        <ItemTemplate>
                            <%# this.ConvertDateTimeToString(Eval("LastActivityDate"), true) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="150px">
                        <ItemTemplate>
                            <div class="d-flex justify-content-center gap-2">
                                <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsEdit && Convert.ToBoolean(Eval("LaNhanVien"))%>' ID="lbtEmpDetail" CommandName="VIEW_EMP_DETAIL" CssClass="btn-grid-action text-decoration-underline text-success" ResourceKey='<%# BackEndResourceKeys.EMPLOYEE_DETAIL%>' ButtonIcon='<%# "fas fa-eye" %>'>

                                </SweetSoft:SmartLinkButton>
                                <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsEdit %>'
                                    ID="lbtDetail" CommandName="ITEM_DETAIL" CssClass="btn-grid-action text-decoration-underline"
                                    ResourceKey='<%# BackEndResourceKeys.EDIT%>'
                                    ButtonIcon='<%#  "fas fa-pencil-alt" %>'>
                                </SweetSoft:SmartLinkButton>

                                <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsEdit  %>'
                                    ID="lbtResetPassword" CommandName="RESET_PASSWORD" CssClass="btn-grid-action text-decoration-underline ms-2 me-2"
                                    ResourceKey='<%# BackEndResourceKeys.RESET_PASSWORD %>'
                                    ButtonIcon="fas fa-unlock-alt">
                                </SweetSoft:SmartLinkButton>

                                <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsDelete %>'
                                    ID="lbtDelete" CommandName="ITEM_DELETE" CssClass="btn-grid-action text-decoration-underline text-danger"
                                    ResourceKey='<%# BackEndResourceKeys.DELETE %>'
                                    ButtonIcon="fas fa-trash">
                                </SweetSoft:SmartLinkButton>
                            </div>
                            
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
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.USER_NAME) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchUserName" SearchColumn="UserName" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label class="form-label"><%=GetResourceText(BackEndResourceKeys.DISPLAY_NAME) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchFullName" SearchColumn="FullName" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label class="form-label">Email</label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchEmail" SearchColumn="Email" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_CCCD) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchCCCD" SearchColumn="IdCCCD" PlaceHolder="Enter the value"></SweetSoft:ExtraTextBox>
                            </div>
                            <div class="col-lg-6">
                                <div class="mb-3">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CHUC_DANH) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlSearchChucDanh" SimpleInit="true" PlaceHolder="Select the value"></SweetSoft:ExtraDropdown>
                                </div>
                            </div>
                            <div class="col-lg-6">
                                <div class="mb-3">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PHONG_BAN) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlSearchPhongBan" SimpleInit="true" PlaceHolder="Select the value" ValueIsOfTypeGUID="True"></SweetSoft:ExtraDropdown>
                                </div>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchPhone" SearchColumn="PhoneNumber" PlaceHolder="Enter the value" ValueIsOfTypeGUID="True"></SweetSoft:ExtraTextBox>
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
