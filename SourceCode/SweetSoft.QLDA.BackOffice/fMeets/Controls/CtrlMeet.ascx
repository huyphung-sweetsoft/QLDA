<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlMeet.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fMeets.Controls.CtrlMeet" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fMeets/Controls/CtrlXemNhanVienMeet.ascx" TagPrefix="SweetSoft" TagName="CtrlXemNhanVienMeet" %>

<style>
    .avatar-group { display: inline-flex !important; align-items: center; justify-content: center; gap: 6px !important; flex-wrap: nowrap !important; white-space: nowrap !important; }  
    .avatar-stack-container { display: flex; align-items: center; }    
    .avatar-circle { width: 28px; height: 28px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 700; color: #ffffff; border: 2px solid #ffffff; margin-left: -8px; position: relative; z-index: 1; box-shadow: 0 1px 2px rgba(0,0,0,0.1); }    
    .avatar-circle:first-child { margin-left: 0; }    
    .avatar-more { background-color: #f1f5f9; color: #475569; border-color: #cbd5e1; z-index: 0; font-weight: 800; font-size: 10px; }    
    .btn-assign-task { width: 26px; height: 26px; border-radius: 6px; background-color: #2563eb; color: white; display: flex; align-items: center; justify-content: center; border: none; cursor: pointer; text-decoration: none; font-size: 12px; transition: background 0.2s, transform 0.1s; flex-shrink: 0; }
    .btn-assign-task:hover { background-color: #1d4ed8; color: white; transform: scale(1.05); }
    .btn-assign-task.view-only { background-color: #64748b; }
    .btn-assign-task.view-only:hover { background-color: #475569; }
</style>

<div class="card-header">
    <div class="d-flex flex-column flex-xl-row gap-3">
        <asp:UpdatePanel runat="server" ID="pnlSearchDropdowns" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel runat="server" ID="pnlSearchDefaultStatus">
                    <div class="d-flex">
                        <SweetSoft:BootstrapDropdown ID="ddlSearchTrangThai" runat="server"
                            Text="Trạng thái" AllowClear="true" AutoPostBack="true"
                            SearchColumn="TrangThai" CssClass="border-top-left-radius-1 border-bottom-left-radius-1"
                            OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged">
                        </SweetSoft:BootstrapDropdown>
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>

        <div class="input-group max-w-500 mb-3">
            <a class="btn btn-info font-mobile-small btn-search-filter" onclick="CMSMasterJs.ShowOffcanvasSearch();" href="javascript:;">
                <i class='fas fa-filter me-1'></i><%= GetResourceText(BackEndResourceKeys.FILTER) %>
            </a>
            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
        </div>
        <div runat="server" id="tagOther" class="d-flex justify-content-end gap-3 w-full flex-wrap">
            <asp:UpdatePanel runat="server" ID="pnlButtons" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="d-flex">
                        <SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click" CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Info" ButtonIcon="Add">Thêm mới</SweetSoft:ExtraButton>
                    </div>
                </ContentTemplate>
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
                AllowSorting="true" ShowHeader="true" ShowHeaderWhenEmpty="true" AutoGenerateColumns="false"
                CssClass="table table-bordered table-hover align-middle w-100" FocusBtnIcon="fas fa-compress-arrows-alt"
                DataKeyNames="IdLichHop" ValueField="IdLichHop" DataNameField="TenCuocHop" GridLines="None"
                IsEnableSelectColumn="false" 
                OnNeedDataSource="grvData_NeedDataSource" OnRowCommand="grvData_RowCommand">
                <Columns>
                    <asp:TemplateField HeaderText="MeetingCode" SortExpression="MaCuocHop" HeaderStyle-Width="120px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# Eval("MaCuocHop") ?? "—" %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="MeetingName" SortExpression="TenCuocHop" HeaderStyle-CssClass="text-center">
                        <ItemTemplate><%# Eval("TenCuocHop") ?? "—" %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Người tham gia" HeaderStyle-Width="160px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" ItemStyle-Wrap="false">
                        <ItemTemplate>
                            <div class="avatar-group">
                                <div class="avatar-stack-container">
                                    <%# GetAssigneeDisplay(Eval("TenNhanVien"), Eval("Avatars")) %>
                                </div>
                                <asp:LinkButton runat="server" ID="lbtViewMembers" 
                                    CommandName="VIEW_MEMBERS" 
                                    CommandArgument='<%# Eval("IdLichHop") %>' 
                                    CssClass="btn-assign-task view-only" 
                                    ToolTip="Xem thành viên"
                                    Visible='<%# this.IsView || this.IsEdit %>'>
                                    <i class="fas fa-user-friends"></i>
                                </asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="StartTime" SortExpression="ThoiGianBatDau" HeaderStyle-Width="130px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# Eval("ThoiGianBatDau", "{0:dd/MM/yyyy HH:mm}") %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="EndTime" SortExpression="ThoiGianKetThuc" HeaderStyle-Width="130px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# Eval("ThoiGianKetThuc", "{0:dd/MM/yyyy HH:mm}") %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="MeetingRoom" SortExpression="DiaDiemHop" HeaderStyle-CssClass="text-center">
                        <ItemTemplate><%# Eval("DiaDiemHop") ?? "—" %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Status" SortExpression="TrangThai" HeaderStyle-Width="120px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# GetTrangThaiCuocHopText(Eval("TrangThai")) %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="150px">
                        <ItemTemplate>
                            <div class="d-flex justify-content-center align-items-center gap-1">
                                <asp:LinkButton runat="server"
                                    ID="lbtMeetingFiles"
                                    Visible='<%# this.IsView %>'
                                    CommandName="MEETING_FILES"
                                    CommandArgument='<%# Eval("IdLichHop") %>'
                                    CausesValidation="false"
                                    CssClass="btn btn-outline-primary btn-sm text-center btn-smart-link"
                                    ToolTip="File đính kèm lịch họp">
                                    <i class="fas fa-folder-open"></i>
                                </asp:LinkButton>

                                <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsView %>' 
                                    ID="lbtDetail" CommandName="ITEM_DETAIL" CssClass="btn-grid-action text-decoration-underline" 
                                    ResourceKey='<%# this.IsEdit ? BackEndResourceKeys.EDIT : BackEndResourceKeys.VIEW %>' 
                                    ButtonIcon='<%# this.IsView ? "fas fa-pencil-alt" : "fas fa-eye" %>'></SweetSoft:SmartLinkButton>

                                <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsDelete %>' 
                                    ID="lbtDelete" CommandName="ITEM_DELETE" CssClass="btn-grid-action text-decoration-underline text-danger" 
                                    ResourceKey='<%# BackEndResourceKeys.DELETE %>' ButtonIcon="fas fa-trash"></SweetSoft:SmartLinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><%= GetResourceText(BackEndResourceKeys.NO_DATA) %></EmptyDataTemplate>
            </SweetSoft:GridviewExtension>
            
            <SweetSoft:Paging runat="server" ID="ctrlGridviewPaging" OnPageChanged="ctrlGridviewPaging_PageChanged" />

            <SweetSoft:CtrlXemNhanVienMeet runat="server" ID="CtrlXemNhanVienMeet1" />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>

<!-- Khung Search Popup -->
<div class="offcanvas offcanvas-end offcanvas-form-search" id="search-offcanvas" aria-hidden="true">
    <div class="offcanvas-header">
        <h5 class="offcanvas-title">Tìm kiếm nâng cao</h5>
        <button class="btn-close" type="button" data-bs-dismiss="offcanvas" aria-label="Close"></button>
    </div>
    <div class="div offcanvas-body pt-0">
        <div class="card shadow-none card-body text-muted mb-0">
            <asp:UpdatePanel runat="server" ID="pnlSearch" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel runat="server" ID="pnlSearchPopup">
                        <div class="row">
                            <div class="col-md-12 mb-3">
                                <label class="form-label">Tên cuộc họp</label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchTenCuocHop" SearchColumn="TenCuocHop"></SweetSoft:ExtraTextBox>
                            </div>
                        </div>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
            <div class="d-flex align-items-center gap-1 mt-3">
                <SweetSoft:ExtraButton runat="server" ID="lbtSearchAdvanced" CssClass="flex-btn" ButtonStyle="Primary" ButtonIcon="Search" OnClick="btnSearchAdvanced_ServerClick">Áp dụng</SweetSoft:ExtraButton>
                <SweetSoft:ExtraButton runat="server" ID="lbtCancel" CssClass="flex-btn" ButtonStyle="OutLineSecondary" ButtonIcon="Refresh" OnClick="btnCancel_Click">Làm mới</SweetSoft:ExtraButton>
            </div>
        </div>
    </div>
</div>
