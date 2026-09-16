<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlIssue.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fIssues.Controls.CtrlIssue" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<div class="card-header">
    <div class="d-flex flex-column flex-xl-row gap-3">
        <asp:UpdatePanel runat="server" ID="pnlSearchDropdowns" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel runat="server" ID="pnlSearchDefaultStatus">
                    <div class="d-flex gap-2">
                        <SweetSoft:BootstrapDropdown ID="ddlSearchMucDoAnhHuong" runat="server"
                            AllowClear="true" AutoPostBack="true"
                            SearchColumn="MucDoAnhHuong" CssClass="border-radius-1"
                            OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged">
                        </SweetSoft:BootstrapDropdown>

                        <SweetSoft:BootstrapDropdown ID="ddlSearchTrangThai" runat="server"
                            AllowClear="true" AutoPostBack="true"
                            SearchColumn="TrangThai" CssClass="border-radius-1"
                            OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged">
                        </SweetSoft:BootstrapDropdown>

                        <SweetSoft:BootstrapDropdown ID="ddlSearchNguonGoc" runat="server"
                            AllowClear="true" AutoPostBack="true"
                            SearchColumn="NguonGocVanDe" CssClass="border-radius-1"
                            OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged">
                        </SweetSoft:BootstrapDropdown>
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>

        <div class="input-group max-w-500 mb-3">
            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" CssClass="border-primary input-search-filter" ></SweetSoft:ExtraTextBox>
            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
        </div>
        <div runat="server" id="tagOther" class="d-flex justify-content-end gap-3 w-full flex-wrap">
            <asp:UpdatePanel runat="server" ID="pnlButtons" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="d-flex">
                        <SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click" CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Info" ButtonIcon="Add"></SweetSoft:ExtraButton>
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
                CssClass="table-bordered table-hover align-middle" FocusBtnIcon="fas fa-compress-arrows-alt"
                DataKeyNames="IdVanDe" ValueField="IdVanDe" DataNameField="TenVanDe" GridLines="None"
                IsEnableSelectColumn="false" 
                OnNeedDataSource="grvData_NeedDataSource" OnRowCommand="grvData_RowCommand">
                <Columns>
                    <asp:TemplateField HeaderText="IssueCode" HeaderStyle-Width="120px" SortExpression="MaVanDe" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# Eval("MaVanDe") != DBNull.Value && Eval("MaVanDe") != null ? Eval("MaVanDe") : "—" %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="IssueName" SortExpression="TenVanDe" HeaderStyle-CssClass="text-center">
                        <ItemTemplate><%# Eval("TenVanDe") != DBNull.Value && Eval("TenVanDe") != null ? Eval("TenVanDe") : "—" %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Impact" SortExpression="MucDoAnhHuong" HeaderStyle-Width="130px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# GetMucDoAnhHuongText(Eval("MucDoAnhHuong")) %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Status" SortExpression="TrangThai" HeaderStyle-Width="130px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# GetTrangThaiVanDeText(Eval("TrangThai")) %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Origin" SortExpression="NguonGocVanDe" HeaderStyle-Width="130px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# GetNguonGocVanDeText(Eval("NguonGocVanDe")) %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="CreatedBy" SortExpression="NguoiTao" HeaderStyle-Width="130px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# Eval("NguoiTao") != DBNull.Value && Eval("NguoiTao") != null ? Eval("NguoiTao") : "—" %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="150px">
                        <ItemTemplate>
                            <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsView %>' 
                                ID="lbtDetail" CommandName="ITEM_DETAIL" CssClass="btn-grid-action text-decoration-underline" 
                                ResourceKey='<%# this.IsEdit ? BackEndResourceKeys.EDIT : BackEndResourceKeys.VIEW %>' 
                                ButtonIcon='<%# this.IsView ? "fas fa-pencil-alt" : "fas fa-eye" %>'></SweetSoft:SmartLinkButton>

                            <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsDelete %>' 
                                ID="lbtDelete" CommandName="ITEM_DELETE" CssClass="btn-grid-action text-decoration-underline text-danger" 
                                ResourceKey='<%# BackEndResourceKeys.DELETE %>' ButtonIcon="fas fa-trash"></SweetSoft:SmartLinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><%= GetResourceText(BackEndResourceKeys.NO_DATA) %></EmptyDataTemplate>
            </SweetSoft:GridviewExtension>
            
            <SweetSoft:Paging runat="server" ID="ctrlGridviewPaging" OnPageChanged="ctrlGridviewPaging_PageChanged" />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>