<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlRisk.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fRisks.Controls.CtrlRisk" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<div class="card-header">
    <div class="d-flex flex-column flex-xl-row gap-3">
        <div class="input-group max-w-500 mb-3">
            <a class="btn btn-info font-mobile-small btn-search-filter" onclick="CMSMasterJs.ShowOffcanvasSearch();" href="javascript:;">
                <i class='fas fa-filter me-1'></i><%= GetResourceText(BackEndResourceKeys.FILTER) %>
            </a>
            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" PlaceHolder="Nhập từ khóa tìm kiếm..." CssClass="border-primary input-search-filter" ></SweetSoft:ExtraTextBox>
            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
        </div>
        <div runat="server" id="tagOther" class="d-flex justify-content-end gap-3 w-full flex-wrap">
            <asp:UpdatePanel runat="server" ID="pnlButtons" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="d-flex">
                        <SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click" CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Info" ButtonIcon="Add">Add new</SweetSoft:ExtraButton>
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
                DataKeyNames="IdRuiRo_DuAn" ValueField="IdRuiRo_DuAn" DataNameField="TenRuiRo" GridLines="None"
                IsEnableSelectColumn="false" 
                OnNeedDataSource="grvData_NeedDataSource" OnRowCommand="grvData_RowCommand">
                <Columns>
                    <asp:TemplateField HeaderText="RiskName" HeaderStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# Eval("TenRuiRo") != DBNull.Value && Eval("TenRuiRo") != null ? Eval("TenRuiRo") : "—" %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Impact" HeaderStyle-Width="100px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                           <%# GetMucDoAnhHuongText(Eval("MucDoAnhHuong")) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Probability" HeaderStyle-Width="100px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# Eval("XacSuatXayRa") != DBNull.Value ? Eval("XacSuatXayRa") : "—" %>(%)
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="RiskLevel" HeaderStyle-Width="100px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center fw-bold text-danger">
                        <ItemTemplate>
                            <%# GetMucDoRuiRoText(Eval("DiemRuiRo")) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Monitor" HeaderStyle-Width="100px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# Eval("TenNhanVienXuLy") != DBNull.Value ? Eval("TenNhanVienXuLy") : "—" %>
                        </ItemTemplate>
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
                <SweetSoft:ExtraButton runat="server" ID="lbtSearchAdvanced" CssClass="flex-btn" ButtonStyle="Primary" ButtonIcon="Search" OnClick="btnSearchAdvanced_ServerClick">Áp dụng</SweetSoft:ExtraButton>
                <SweetSoft:ExtraButton runat="server" ID="lbtCancel" CssClass="flex-btn" ButtonStyle="OutLineSecondary" ButtonIcon="Refresh" OnClick="btnCancel_Click">Làm mới</SweetSoft:ExtraButton>
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
                            <div class="col-md-12 mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.RISK_NAME) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchTenRuiRo" SearchColumn="TenRuiRo" PlaceHolder="Nhập tên rủi ro..."></SweetSoft:ExtraTextBox>
                            </div>
                        </div>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</div>