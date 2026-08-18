<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="RoleList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fUsers.RoleList" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card min-h-sreen p-2">
                <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="Nhóm người dùng" />
                <div class="card-header">
                    <div class="d-flex flex-column flex-xl-row gap-3">
                        <div class="input-group max-w-500">
                            <a class="btn btn-info font-mobile-small btn-search-filter" onclick="CMSMasterJs.ShowOffcanvasSearch();" href="javascript:;">
                                <i class='fas fa-filter me-1'></i><%= GetResourceText(BackEndResourceKeys.FILTER) %>
                            </a>
                            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" PlaceHolder="Enter the keyword search..." CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
                            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
                        </div>
                        <div class="d-flex justify-content-end gap-3 w-full flex-wrap">
                            <asp:UpdatePanel runat="server" ID="pnlButtons" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <div class="d-flex">
                                        <SweetSoft:ExtraButton runat="server" ID="lbtDeleteMultiple" OnClick="lbtDeleteMultiple_Click"
                                            CssClass="waves-effect waves-light flex-btn font-mobile-small me-2" ButtonStyle="Danger" ButtonIcon="Remove">Xóa hàng loạt
                                        </SweetSoft:ExtraButton>
                                        <SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click" CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Info" ButtonIcon="Add" Visible="false">Add new</SweetSoft:ExtraButton>
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
                                AllowSorting="true"
                                AutoGenerateColumns="false"
                                CssClass="table-bordered"
                                IsEnableSelectColumn="true"
                                ValueField="RoleId"
                                DataNameField="RoleName"
                                FocusBtnIcon="fas fa-compress-arrows-alt"
                                DataKeyNames="RoleId" GridLines="None"
                                OnNeedDataSource="grvData_NeedDataSource"
                                OnRowCommand="grvData_RowCommand">
                                <Columns>
                                    <asp:TemplateField HeaderText="Tên nhóm" HeaderStyle-CssClass="text-center" SortExpression="RoleName">
                                        <ItemTemplate>
                                            <asp:LinkButton runat="server" CssClass="card-link" Visible='<%# this.IsEdit %>'
                                                ID="lbtView" CommandName="ITEM_DETAIL" Text='<%# Eval("RoleName") %>'></asp:LinkButton>
                                            <span runat="server" visible='<%# !this.IsEdit %>'><%# Eval("RoleName") %></span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Description" HeaderStyle-CssClass="text-center" SortExpression="Description">
                                        <ItemTemplate>
                                            <%# Eval("Description") %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Trạng thái" HeaderStyle-CssClass="text-center" SortExpression="IsActivated" ItemStyle-CssClass="text-center">
                                        <ItemTemplate>
                                            <%# GetStatusText(Eval("IsActivated")) %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Ngày tạo" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-end" SortExpression="CreatedDate">
                                        <ItemTemplate>
                                            <%# ConvertDateTimeToString(Eval("CreatedDate")) %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Thao tác" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="120px">
                                        <ItemTemplate>
                                            <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsView %>'
                                                ID="lbtDetail" CommandName="ITEM_DETAIL" CssClass="btn-grid-action text-decoration-underline me-2"
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
            </div>
        </div>
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
                                    <label class="form-label"><%=GetResourceText(BackEndResourceKeys.NAME) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtSearchRoleName" SearchColumn="RoleName" PlaceHolder="Nhập từ khóa..."></SweetSoft:ExtraTextBox>
                                </div>
                                <div class="col-md-6 mb-3 d-none">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.SUMMARY) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSummary" SearchColumn="Description" PlaceHolder="Nhập từ khóa..."></SweetSoft:ExtraTextBox>
                                </div>
                                <div class="col-md-6 mb-3">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlSearchStatus" SearchColumn="IsActivated" SimpleInit="true" AlowClear="true"></SweetSoft:ExtraDropdown>
                                </div>
                                <div class="col-md-6 mb-3">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CREATED_BY) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlSearchCreatedBy" SearchColumn="CreatedBy" SimpleInit="true"></SweetSoft:ExtraDropdown>
                                </div>
                                <div class="col-md-6 mb-3">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.UPDATED_BY) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlSearchUpdatedBy" SearchColumn="UpdatedBy" SimpleInit="true"></SweetSoft:ExtraDropdown>
                                </div>
                                <div class="col-md-6 mb-3">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CREATED_DATE) %></label>
                                    <SweetSoft:ExtraDateTime runat="server" ID="txtSearchCreatedDate" SearchColumn="CreatedDate" SingleDatePicker="false" IsPredefinedDateRanges="true" AutoUpdateInput="false" />
                                </div>
                                <div class="col-md-6 mb-3">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.UPDATED_DATE) %></label>
                                    <SweetSoft:ExtraDateTime runat="server" ID="txtSearchUpdatedDate" SearchColumn="UpdatedDate" SingleDatePicker="false" IsPredefinedDateRanges="true" AutoUpdateInput="false" />
                                </div>
                            </div>
                        </asp:Panel>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            CMSMasterJs.AddEndRequest(CMSMasterJs.DisableContentChanged);
        });
    </script>
</asp:Content>
