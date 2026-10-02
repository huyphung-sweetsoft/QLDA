<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlIssue.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fIssues.Controls.CtrlIssue" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fIssues/Controls/CtrlViewIssueDetail.ascx" TagPrefix="SweetSoft" TagName="CtrlViewIssueDetail" %>

<div class="card-header">
    <div class="d-flex flex-column flex-xl-row gap-3">
        <asp:UpdatePanel runat="server" ID="pnlSearchDropdowns" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel runat="server" ID="pnlSearchDefaultStatus">
                    <div class="d-flex gap-2">
                        <SweetSoft:BootstrapDropdown ID="ddlSearchMucDoAnhHuong" runat="server" AllowClear="true" AutoPostBack="true" SearchColumn="MucDoAnhHuong" CssClass="border-radius-1" OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged"></SweetSoft:BootstrapDropdown>
                        <SweetSoft:BootstrapDropdown ID="ddlSearchTrangThai" runat="server" AllowClear="true" AutoPostBack="true" SearchColumn="TrangThai" CssClass="border-radius-1" OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged"></SweetSoft:BootstrapDropdown>
                        <SweetSoft:BootstrapDropdown ID="ddlSearchNguonGoc" runat="server" AllowClear="true" AutoPostBack="true" SearchColumn="NguonGocVanDe" CssClass="border-radius-1" OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged"></SweetSoft:BootstrapDropdown>
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        <div class="input-group max-w-500 mb-3">
            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
        </div>
        <div runat="server" id="tagOther" class="d-flex justify-content-end gap-3 w-full flex-wrap">
            <asp:UpdatePanel runat="server" ID="pnlButtons" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="d-flex"><SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click" CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Info" ButtonIcon="Add"></SweetSoft:ExtraButton></div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
    <div class="listSearchTagBox"><asp:UpdatePanel ID="upSearchTagBox" runat="server" UpdateMode="Conditional"><ContentTemplate><SweetSoft:ExtraSearchBox ID="searchTagBox" runat="server" OnTagClosed="searchTagBox_TagClosed"></SweetSoft:ExtraSearchBox></ContentTemplate></asp:UpdatePanel></div>
</div>

<div class="card-body p-0">
    <asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <SweetSoft:GridviewExtension ID="grvData" runat="server" AllowSorting="true" ShowHeader="true" ShowHeaderWhenEmpty="true" AutoGenerateColumns="false" CssClass="table-bordered table-hover align-middle" FocusBtnIcon="fas fa-compress-arrows-alt" DataKeyNames="IdVanDe" ValueField="IdVanDe" DataNameField="TenVanDe" GridLines="None" IsEnableSelectColumn="false" OnNeedDataSource="grvData_NeedDataSource" OnRowCommand="grvData_RowCommand">
                <Columns>
                    <asp:TemplateField HeaderText="IssueName" SortExpression="TenVanDe" HeaderStyle-CssClass="text-center">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbtIssueName" runat="server" CommandName="ITEM_VIEW_DETAIL" CssClass="issue-name-link" Visible='<%# this.IsEdit || this.IsView %>' CausesValidation="false"><%# Eval("TenVanDe") != DBNull.Value && Eval("TenVanDe") != null ? Eval("TenVanDe") : "—" %></asp:LinkButton>
                            <asp:Literal ID="ltrIssueName" runat="server" Visible='<%# !this.IsEdit && !this.IsView %>' Text='<%# Eval("TenVanDe") != DBNull.Value && Eval("TenVanDe") != null ? Eval("TenVanDe") : "—" %>'></asp:Literal>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Impact" SortExpression="MucDoAnhHuong" HeaderStyle-Width="150px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center"><ItemTemplate><%# GetMucDoAnhHuongText(Eval("MucDoAnhHuong")) %></ItemTemplate></asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Status" SortExpression="TrangThai" HeaderStyle-Width="140px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center"><ItemTemplate>
                        <SweetSoft:SmartLinkButton runat="server" ID="lbtCompleteStatus" CommandName="ITEM_COMPLETE_PROCESS" CommandArgument='<%# Eval("IdVanDe") %>' VisibleConditionKey='<%# this.IsEdit && Eval("TrangThai") != DBNull.Value && Eval("TrangThai").ToString() == "0" %>' CssClass="issue-status-button issue-status-doing" Text="Đang xử lý" ToolTip="Xác nhận chuyển vấn đề sang trạng thái Đã xử lý"></SweetSoft:SmartLinkButton>
                        <span runat="server" class="issue-status-view-doing" visible='<%# !this.IsEdit && Eval("TrangThai") != DBNull.Value && Eval("TrangThai").ToString() == "0" %>'>Đang xử lý</span>
                        <span runat="server" class="issue-status-done" title="Vấn đề đã xử lý" visible='<%# Eval("TrangThai") != DBNull.Value && Eval("TrangThai").ToString() == "1" && (this.IsEdit || this.IsView) %>'><i class="fas fa-check me-1"></i>Đã xử lý</span>
                    </ItemTemplate></asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Origin" SortExpression="NguonGocVanDe" HeaderStyle-Width="220px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center"><ItemTemplate><%# GetNguonGocVanDeText(Eval("NguonGocVanDe")) %></ItemTemplate></asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="CreatedBy" SortExpression="NguoiTao" HeaderStyle-Width="120px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center"><ItemTemplate><%# Eval("NguoiTao") != DBNull.Value && Eval("NguoiTao") != null ? Eval("NguoiTao") : "—" %></ItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="CreatedDate" SortExpression="NgayTao" HeaderStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# GetFormattedDate(Eval("NgayTao")) %>
                        </ItemTemplate>
                    </asp:TemplateField>                    
                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="100px">
                        <ItemTemplate>
                            <div class="d-flex justify-content-center align-items-center gap-2">
                                <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsEdit || this.IsView %>' ID="lbtDetail" CommandName="ITEM_DETAIL" CssClass="btn-grid-action text-decoration-underline" ResourceKey='<%# this.IsEdit ? BackEndResourceKeys.EDIT : BackEndResourceKeys.VIEW %>' ButtonIcon='<%# this.IsEdit ? "fas fa-pencil-alt" : "fas fa-eye" %>'></SweetSoft:SmartLinkButton>
                                <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsDelete %>' ID="lbtDelete" CommandName="ITEM_DELETE" CssClass="btn-grid-action text-decoration-underline text-danger" ResourceKey='<%# BackEndResourceKeys.DELETE %>' ButtonIcon="fas fa-trash"></SweetSoft:SmartLinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><%= GetResourceText(BackEndResourceKeys.NO_DATA) %></EmptyDataTemplate>
            </SweetSoft:GridviewExtension>
            <SweetSoft:Paging runat="server" ID="ctrlGridviewPaging" OnPageChanged="ctrlGridviewPaging_PageChanged" />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>

<SweetSoft:CtrlViewIssueDetail ID="CtrlViewIssueDetail1" runat="server" />
<style>
    .issue-name-link { 
        color: #542e88 !important; 
        font-size: 16px; 
        font-weight: 500; 
        text-decoration: none !important; 
        transition: color 0.15s ease; 
    }
    
    .issue-name-link:hover { 
        color: #3b82f6 !important; 
        text-decoration: none !important; 
    }

    /* --- DÙNG CHUNG CHO CẢ 3 NHÃN --- */
    .issue-status-button,
    .issue-status-view-doing,
    .issue-status-done {
        display: inline-flex !important; 
        align-items: center; 
        justify-content: center; 
        padding: 2px 6px !important; /* Đã giảm padding để nhãn gọn hơn */
        border-radius: 999px !important; 
        line-height: 1.3; 
        font-size: 13px !important; 
        min-width: 95px !important; /* Gắn cứng 1 chiều rộng chung để các nhãn bằng nhau */
        box-sizing: border-box; /* Đảm bảo padding không làm phình kích thước */
    }

    /* --- TRẠNG THÁI: ĐANG XỬ LÝ --- */
    .issue-status-button,
    .issue-status-view-doing {
        background: #eff6ff !important; 
        border: 1px solid #bfdbfe !important; 
        color: #1d4ed8 !important; 
        font-weight: 600;
    }

    .issue-status-button {
        position: relative; 
        text-decoration: none !important; 
        transition: all .15s ease; 
        overflow: hidden; 
    }

    .issue-status-view-doing {
        cursor: default; 
    }

    /* --- TRẠNG THÁI: ĐÃ XỬ LÝ --- */
    .issue-status-done {
        background: #dcfce7 !important; 
        border: 1px solid #86efac !important; 
        color: #15803d !important; 
        font-weight: 700; 
        cursor: default; 
    }

    /* --- HOVER CHO NÚT ĐANG XỬ LÝ --- */
    .issue-status-button:hover { 
        background: #16a34a !important; 
        border-color: #15803d !important; 
        color: transparent !important; 
        box-shadow: 0 3px 8px rgba(22, 163, 74, .2); 
        transform: translateY(-1px); 
    }
    
    .issue-status-button:hover::after { 
        content: "✓  Đã xử lý"; 
        position: absolute; 
        inset: 0; 
        display: flex; 
        align-items: center; 
        justify-content: center; 
        color: #fff; 
        font-weight: 700; 
        font-size: 13px !important;
    }
</style>
