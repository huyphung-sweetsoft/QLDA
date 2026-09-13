<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlThanhToan.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.Controls.CtrlThanhToan" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<style type="text/css">
    .payment-check-container {
        display: flex;
        justify-content: center;
        align-items: center;
        min-height: 28px;
    }
    .payment-quick-checkbox {
        display: inline-flex !important;
        align-items: center;
        justify-content: center;
        cursor: pointer;
        vertical-align: middle;
    }
    .payment-quick-checkbox input[type="checkbox"] {
        appearance: none;
        -webkit-appearance: none;
        -moz-appearance: none;
        width: 22px;
        height: 22px;
        min-width: 22px;
        min-height: 22px;
        margin: 0;
        cursor: pointer;
        background-color: #ffffff;
        border: 2px solid #5b73e8;
        border-radius: 6px;
        outline: none;
        display: inline-block;
        position: relative;
        vertical-align: middle;
        transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
        box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
    }
    .payment-quick-checkbox:not(.aspNetDisabled) input[type="checkbox"]:hover {
        border-color: #22c55e;
        background-color: #f0fdf4;
        transform: scale(1.15);
        box-shadow: 0 0 0 4px rgba(34, 197, 94, 0.2), 0 2px 4px rgba(0, 0, 0, 0.1);
    }
    .payment-quick-checkbox input[type="checkbox"]:checked,
    .payment-quick-checkbox.aspNetDisabled input[type="checkbox"]:checked,
    .payment-quick-checkbox input[type="checkbox"]:disabled:checked {
        background-color: #22c55e !important;
        border-color: #16a34a !important;
        background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 20 20'%3e%3cpath fill='none' stroke='%23ffffff' stroke-linecap='round' stroke-linejoin='round' stroke-width='3' d='m4 10 4 4 8-8'/%3e%3c/svg%3e") !important;
        background-position: center !important;
        background-repeat: no-repeat !important;
        background-size: 14px 14px !important;
        box-shadow: 0 2px 6px rgba(34, 197, 94, 0.35) !important;
        opacity: 1 !important;
        cursor: default;
    }
    .payment-quick-checkbox.aspNetDisabled input[type="checkbox"]:not(:checked),
    .payment-quick-checkbox input[type="checkbox"]:disabled:not(:checked) {
        background-color: #f3f4f6 !important;
        border-color: #d1d5db !important;
        opacity: 0.5 !important;
        cursor: not-allowed;
        box-shadow: none !important;
    }
</style>
<div class="card-header d-flex justify-content-between gap-3 flex-wrap">
    <div class="input-group max-w-500">
        <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" CssClass="border-primary input-search-filter" />
        <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter"
            IsCustomClass="false" ButtonIcon="Search" CausesValidation="false" OnClick="btnSearch_ServerClick" />
    </div>
    <asp:UpdatePanel runat="server" ID="pnlButtons" UpdateMode="Conditional">
        <ContentTemplate>
            <SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click"
                CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Info"
                ButtonIcon="Add" CausesValidation="false" Visible="false" />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
<div class="card-body p-0">
    <asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <SweetSoft:GridviewExtension ID="grvData" runat="server" AllowSorting="true"
                ShowHeader="true" ShowHeaderWhenEmpty="true" AutoGenerateColumns="false"
                CssClass="table-bordered table-hover" FocusBtnIcon="fas fa-compress-arrows-alt"
                DataKeyNames="IdThanhToan" GridLines="None" IsEnableSelectColumn="false"
                OnNeedDataSource="grvData_NeedDataSource" OnRowCommand="grvData_RowCommand">
                <Columns>
                    <asp:TemplateField HeaderText="Payment code" SortExpression="MaDotThanhToan" HeaderStyle-CssClass="text-center">
                        <ItemTemplate><%#: Eval("MaDotThanhToan") %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Payment name" SortExpression="TenDotThanhToan" HeaderStyle-CssClass="text-center">
                        <ItemTemplate><%#: Eval("TenDotThanhToan") %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Note" SortExpression="GhiChu" HeaderStyle-CssClass="text-center">
                        <ItemTemplate><%#: Eval("GhiChu") %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Amount" SortExpression="SoTien" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-end text-nowrap">
                        <ItemTemplate><%#: CURRENT_PAGE.ConvertNumber(Eval("SoTien")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Due date" SortExpression="HanThanhToan" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center text-nowrap">
                        <ItemTemplate><%#: FormatDate(Eval("HanThanhToan")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actual payment date" SortExpression="NgayThanhToanThucTe" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center text-nowrap">
                        <ItemTemplate><%#: FormatDate(Eval("NgayThanhToanThucTe")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status" SortExpression="TrangThai" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center text-nowrap">
                        <ItemTemplate>
                            <span class='<%# GetStatusCss(Eval("TrangThai"), Eval("HanThanhToan")) %>'><%#: GetStatusText(Eval("TrangThai"), Eval("HanThanhToan")) %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="160px">
                        <ItemTemplate>
                            <div class="d-flex flex-nowrap justify-content-center gap-2">
                                <SweetSoft:SmartLinkButton runat="server" ID="lbtEdit" VisibleConditionKey='<%# CURRENT_PAGE.IsEdit %>'
                                    CommandName="ITEM_EDIT" CssClass="btn-grid-action text-decoration-underline"
                                    ResourceKey='<%# BackEndResourceKeys.EDIT %>' ButtonIcon="fas fa-pencil-alt" />
                                <SweetSoft:SmartLinkButton runat="server" ID="lbtDelete" VisibleConditionKey='<%# CURRENT_PAGE.IsDelete %>'
                                    CommandName="ITEM_DELETE" CssClass="btn-grid-action text-decoration-underline text-danger"
                                    OnClientClick='<%# GetDeleteConfirmScript(Eval("MaDotThanhToan")) %>'
                                    ResourceKey='<%# BackEndResourceKeys.DELETE %>' ButtonIcon="fas fa-trash" />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Đã thanh toán" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center align-middle" HeaderStyle-Width="120px">
                        <ItemTemplate>
                            <div class="payment-check-container">
                                <asp:CheckBox runat="server" ID="chkQuickApprove" AutoPostBack="true" CausesValidation="false"
                                    CssClass="payment-quick-checkbox"
                                    Checked='<%# IsPaid(Eval("TrangThai")) %>'
                                    Enabled='<%# CanQuickApprove(Eval("TrangThai")) %>'
                                    ToolTip='<%# GetResourceText(BackEndResourceKeys.PAYMENT_QUICK_APPROVE) %>'
                                    onclick='<%# GetQuickApproveConfirmScript(Eval("MaDotThanhToan")) %>'
                                    OnCheckedChanged="chkQuickApprove_CheckedChanged" />
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
