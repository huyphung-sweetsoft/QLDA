<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlRisk.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fRisks.Controls.CtrlRisk" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fRisks/Controls/CtrlViewRiskDetail.ascx" TagPrefix="SweetSoft" TagName="CtrlViewRiskDetail" %>

<style>
    #mdlRiskView .modal-dialog,
    #dlDetail .modal-dialog,
    .modal-dialog {
        width: calc(100% - 24px) !important; 
        max-width: 1180px !important; 
        margin: 1.8rem auto 1.5rem !important; 
    }

    .risk-name-link { 
        color: #542e88 !important; 
        font-size: 16px; 
        font-weight: 500; 
        text-decoration: none !important; 
        transition: color 0.15s ease; 
    }
    
    .risk-name-link:hover { 
        color: #3b82f6 !important; 
        text-decoration: none !important; 
    }
</style>

<div class="card-header">
    <div class="d-flex flex-column flex-xl-row gap-3">
        <asp:UpdatePanel runat="server" ID="pnlSearchDropdowns" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel runat="server" ID="pnlSearchDefaultStatus">
                    <div class="d-flex gap-2">
                        <SweetSoft:BootstrapDropdown ID="ddlSearchMucDoAnhHuong" runat="server"
                            Text="Mức độ ảnh hưởng" AllowClear="true" AutoPostBack="true"
                            SearchColumn="MucDoAnhHuong" CssClass="border-radius-1"
                            OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged">
                        </SweetSoft:BootstrapDropdown>

                        <SweetSoft:BootstrapDropdown ID="ddlSearchMucDoRuiRo" runat="server"
                            Text="Mức độ rủi ro" AllowClear="true" AutoPostBack="true"
                            SearchColumn="MucDoRuiRo" CssClass="border-radius-1"
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
            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" PlaceHolder="Nhập từ khóa tìm kiếm..." CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
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
                    <asp:TemplateField HeaderText="RiskName" SortExpression="TenRuiRo" HeaderStyle-CssClass="text-center">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" ID="lbtViewDetail" CommandName="ITEM_VIEW" CommandArgument='<%# Eval("IdRuiRo_DuAn") %>'
                                CssClass="risk-name-link" style="cursor: pointer;">
                                <%# Eval("TenRuiRo") != DBNull.Value && Eval("TenRuiRo") != null ? Eval("TenRuiRo") : "—" %>
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Impact" SortExpression="MucDoAnhHuong" HeaderStyle-Width="100px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# GetMucDoAnhHuongText(Eval("MucDoAnhHuong")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Probability" SortExpression="XacSuatXayRa" HeaderStyle-Width="100px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# GetXacSuatRuiRoText(Eval("XacSuatXayRa")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="RiskLevel" SortExpression="DiemRuiRo" HeaderStyle-Width="100px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center fw-bold text-danger">
                        <ItemTemplate><%# GetMucDoRuiRoText(Eval("XacSuatXayRa"), Eval("MucDoAnhHuong"), Eval("DiemRuiRo")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Monitor" SortExpression="TenNhanVienXuLy" HeaderStyle-Width="200px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# Eval("TenNhanVienXuLy") != DBNull.Value ? Eval("TenNhanVienXuLy") : "—" %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="CreatedDate" SortExpression="NgayTao" HeaderStyle-Width="100px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# GetFormattedDate(Eval("NgayTao")) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="150px">
                        <ItemTemplate>
                            <SweetSoft:SmartLinkButton runat="server"
                                VisibleConditionKey='<%# this.IsEdit || this.IsView %>'
                                ID="lbtAction"
                                CommandName='<%# this.IsEdit ? "ITEM_DETAIL" : "ITEM_VIEW" %>'
                                CommandArgument='<%# Eval("IdRuiRo_DuAn") %>'
                                CssClass="btn-grid-action text-decoration-underline"
                                ResourceKey='<%# this.IsEdit ? BackEndResourceKeys.EDIT : BackEndResourceKeys.VIEW %>'
                                ButtonIcon='<%# this.IsEdit ? "fas fa-pencil-alt" : "fas fa-eye" %>'>
                            </SweetSoft:SmartLinkButton>
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

<div class="offcanvas offcanvas-end offcanvas-form-search" id="search-offcanvas" aria-hidden="true">
    <div class="offcanvas-header">
        <h5 class="offcanvas-title"><%= GetResourceText(BackEndResourceKeys.ADVANCED_SEARCH) %></h5>
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
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchTenRuiRo" SearchColumn="TenRuiRo"></SweetSoft:ExtraTextBox>
                            </div>
                            <div class="col-md-12 mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.MONITOR) %></label>
                                <asp:DropDownList ID="ddlSearchNhanVien" runat="server" CssClass="form-select" SearchColumn="IdNhanVienXuLy"></asp:DropDownList>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PROBABILITY) %> (Từ %)</label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchXacSuatMin" SearchColumn="XacSuatMin" TextMode="Number" min="0" max="100"></SweetSoft:ExtraTextBox>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PROBABILITY) %> (Đến %)</label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchXacSuatMax" SearchColumn="XacSuatMax" TextMode="Number" min="0" max="100"></SweetSoft:ExtraTextBox>
                            </div>
                        </div>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
            <div class="d-flex align-items-center gap-2 mt-3">
                <SweetSoft:ExtraButton runat="server" ID="lbtSearchAdvanced" CssClass="flex-btn" ButtonStyle="Primary" ButtonIcon="Search" OnClick="btnSearchAdvanced_ServerClick"><%= GetResourceText(BackEndResourceKeys.APPLY) %></SweetSoft:ExtraButton>
                <SweetSoft:ExtraButton runat="server" ID="lbtCancel" CssClass="flex-btn" ButtonStyle="OutLineSecondary" ButtonIcon="Refresh" OnClick="btnCancel_Click"><%= GetResourceText(BackEndResourceKeys.REFRESH) %></SweetSoft:ExtraButton>
            </div>
        </div>
    </div>
</div>
<SweetSoft:CtrlViewRiskDetail ID="CtrlViewRiskDetail1" runat="server" />