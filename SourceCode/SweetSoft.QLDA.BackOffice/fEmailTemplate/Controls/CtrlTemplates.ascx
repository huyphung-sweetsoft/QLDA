<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlTemplates.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fEmailTemplate.Controls.CtrlTemplates" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<div class="card-header">
    <div class="d-flex flex-column flex-xl-row gap-3">
        <div class="input-group max-w-500">
            <a class="btn btn-info font-mobile-small btn-search-filter" onclick="CMSMasterJs.ShowOffcanvasSearch();" href="javascript:;">
                <i class='fas fa-filter me-1'></i><%= GetResourceText(BackEndResourceKeys.FILTER) %>
            </a>
            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" PlaceHolder="Enter the keyword search..." CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
        </div>
        <div class="d-flex justify-content-end gap-3 w-full flex-wrap">
            <div class="d-block">
                <div class="dropdown action-button">
                    <asp:UpdatePanel runat="server" ID="pnlButtons" UpdateMode="Conditional">
                        <ContentTemplate>
                            <button class="btn btn-primary dropdown-toggle ignore font-mobile-small" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false"><%= GetResourceText(BackEndResourceKeys.ACTION) %></button>
                            <ul class="list-action-dropdown dropdown-menu" aria-labelledby="dropdownMenuButton1">
                                <li runat="server" id="liDeleteMultiple">
                                    <SweetSoft:ExtraButton runat="server" ID="lbtDeleteMultiple" OnClick="lbtDeleteMultiple_Click"
                                        CssClass="waves-effect waves-light flex-btn font-mobile-small" ButtonStyle="Danger" ButtonIcon="Remove" Visible="false">Delete multiple
                                    </SweetSoft:ExtraButton>
                                </li>
                            </ul>
                            <SweetSoft:ExtraButton runat="server" ID="lbtAdd" CssClass="waves-effect waves-light ms-2 font-mobile-small"
                                NavigateUrl="/email-template/add" ButtonStyle="Info" ButtonIcon="Add">Add new</SweetSoft:ExtraButton>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
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
                VisibledColumns="0,1,2,3,4,5,9"
                FocusBtnIcon="fas fa-compress-arrows-alt"
                DataKeyNames="Id" GridLines="None"
                IsEnableSelectColumn="true"
                ValueField="Id"
                DataNameField="Name"
                OnNeedDataSource="grvData_NeedDataSource"
                OnRowCommand="grvData_RowCommand">
                <Columns>
                    <asp:TemplateField HeaderText="Name" HeaderStyle-CssClass="text-center" SortExpression="Name">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" CssClass="card-link" Visible='<%# this.CURRENT_PAGE.IsView
                            && !Convert.ToBoolean(Eval("IsDeleted")) %>'
                                ID="lbtView" CommandName="ITEM_DETAIL" Text='<%# Eval("Name") %>'></asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Template key" HeaderStyle-CssClass="text-center" SortExpression="TemplateKey" ItemStyle-CssClass="text-start">
                        <ItemTemplate>
                            <%# SweetSoft.QLDA.Core.MailManager.EmailTemplateKeys.GetText((string)Eval("TemplateKey")) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Email format type" HeaderStyle-CssClass="text-center" SortExpression="EmailType" ItemStyle-CssClass="text-start">
                        <ItemTemplate>
                            <%# SweetSoft.QLDA.Core.EnumHelper.EnumHelpers.GetDisplayTextSafe<SweetSoft.QLDA.Core.MailManager.EmailFormatTypes>(Eval("EmailType").ToString()) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="text-center" SortExpression="IsActivated" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# this.CURRENT_PAGE.GetStatusText(Eval("IsActivated")) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Created by" HeaderStyle-CssClass="text-center" SortExpression="CreatedUser">
                        <ItemTemplate>
                            <%# this.CURRENT_PAGE.DisplayName(Eval("CreatedUser")) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-Width="100px" ItemStyle-CssClass="text-center" HeaderText="Created date" HeaderStyle-CssClass="text-center" SortExpression="CreatedDate">
                        <ItemTemplate>
                            <%# ConvertDateTimeToString(Eval("CreatedDate"),false) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Update by" HeaderStyle-CssClass="text-center" SortExpression="UpdatedUser">
                        <ItemTemplate>
                            <%# this.CURRENT_PAGE.DisplayName(Eval("UpdatedUser")) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-Width="100px" ItemStyle-CssClass="text-center" HeaderText="Updated date" HeaderStyle-CssClass="text-center" SortExpression="UpdatedDate">
                        <ItemTemplate>
                            <%# ConvertDateTimeToString(Eval("UpdatedDate"),false) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="120px">
                        <ItemTemplate>
                            <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.CURRENT_PAGE.IsView %>'
                                ID="lbtDetail" CommandName="ITEM_DETAIL" CssClass="btn-grid-action text-decoration-underline"
                                ResourceKey='<%# this.CURRENT_PAGE.IsEdit ? BackEndResourceKeys.EDIT : BackEndResourceKeys.VIEW %>'
                                ButtonIcon='<%# this.CURRENT_PAGE.IsView ? "fas fa-pencil-alt" : "fas fa-eye" %>'>
                            </SweetSoft:SmartLinkButton>

                            <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.CURRENT_PAGE.IsDelete %>'
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
            <div class="d-flex align-items-center gap-3">
                <SweetSoft:ExtraButton runat="server" ID="lbtSearchAdvanced" CssClass="flex-btn" ButtonStyle="Primary" ButtonIcon="Search" OnClick="btnSearchAdvanced_ServerClick">Search</SweetSoft:ExtraButton>
                <SweetSoft:ExtraButton runat="server" ID="lbtCancel" CssClass="flex-btn" ButtonStyle="OutLineSecondary" ButtonIcon="Refresh" OnClick="btnCancel_Click">Refresh</SweetSoft:ExtraButton>
            </div>
        </div>
        <button class="btn-close" type="button" data-bs-dismiss="offcanvas" aria-label="Close"></button>
    </div>
    <div class="div offcanvas-body">
        <div class="card shadow-none card-body text-muted mb-0">
            <asp:UpdatePanel runat="server" ID="pnlSearch" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel runat="server" ID="pnlSearchPopup" DefaultButton="lbtSearchAdvanced">
                        <div class="row">
                            <div class="col-12 mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.NAME) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchName" SearchColumn="Name" PlaceHolder="Enter the search keyword..."></SweetSoft:ExtraTextBox>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.TEMPLATE_KEY) %></label>
                                <SweetSoft:ExtraDropdown runat="server" ID="ddlSearchTemplateKey" SearchColumn="TemplateKey" SimpleInit="true" AlowClear="true"></SweetSoft:ExtraDropdown>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                                <SweetSoft:ExtraDropdown runat="server" ID="ddlSearchStatus" SearchColumn="IsActivated" SimpleInit="true" AlowClear="true"></SweetSoft:ExtraDropdown>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CREATED_BY) %></label>
                                <SweetSoft:ExtraDropdown runat="server" ID="ddlSearchCreatedUser" SearchColumn="CreatedUser" EmptyItemText="Select value" SimpleInit="true" AlowClear="true"></SweetSoft:ExtraDropdown>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.UPDATED_BY) %></label>
                                <SweetSoft:ExtraDropdown runat="server" ID="ddlSearchUpdatedUser" SearchColumn="UpdatedUser" EmptyItemText="Select value" SimpleInit="true" AlowClear="true"></SweetSoft:ExtraDropdown>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CREATED_DATE) %></label>
                                <SweetSoft:ExtraDateTime runat="server" ID="dtSearchCreatedDate" SearchColumn="CreatedDate" SingleDatePicker="false" IsPredefinedDateRanges="true" AutoUpdateInput="false" />
                            </div>
                            <div class="col-md-6 mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.UPDATED_DATE) %></label>
                                <SweetSoft:ExtraDateTime runat="server" ID="dtSearchUpdatedDate" SearchColumn="UpdatedDate" SingleDatePicker="false" IsPredefinedDateRanges="true" AutoUpdateInput="false" />
                            </div>
                        </div>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</div>
