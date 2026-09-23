<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlThanhToan.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fThanhToan.Controls.CtrlThanhToan" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<style type="text/css">
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
                        <ItemTemplate>
                            <span class='<%# GetDueDateCss(Eval("TrangThai"), Eval("HanThanhToan")) %>'
                                title='<%# GetDueDateTitle(Eval("TrangThai"), Eval("HanThanhToan")) %>'>
                                <asp:PlaceHolder runat="server" Visible='<%# IsOverdue(Eval("TrangThai"), Eval("HanThanhToan")) %>'>
                                    <i class="fas fa-exclamation-triangle me-1" aria-hidden="true"></i>
                                </asp:PlaceHolder>
                                <%#: FormatDate(Eval("HanThanhToan")) %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actual payment date" SortExpression="NgayThanhToanThucTe" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center text-nowrap">
                        <ItemTemplate><%#: FormatDate(Eval("NgayThanhToanThucTe")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status" SortExpression="TrangThai" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center text-nowrap">
                        <ItemTemplate>
                            <span class='<%# GetStatusCss(Eval("TrangThai")) %>'><%#: GetStatusText(Eval("TrangThai")) %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center align-middle" HeaderStyle-Width="210px">
                        <ItemTemplate>
                            <div class="d-flex flex-nowrap justify-content-center gap-2">
                                <SweetSoft:SmartLinkButton runat="server" ID="lbtEdit" VisibleConditionKey='<%# CURRENT_PAGE.IsEdit %>'
                                    CommandName="ITEM_EDIT" CssClass="btn-grid-action text-decoration-underline"
                                    ResourceKey='<%# BackEndResourceKeys.EDIT %>' ButtonIcon="fas fa-pencil-alt" />
                                <SweetSoft:SmartLinkButton runat="server" ID="lbtDelete" VisibleConditionKey='<%# CURRENT_PAGE.IsDelete %>'
                                    CommandName="ITEM_DELETE" CssClass="btn-grid-action text-decoration-underline text-danger"
                                    OnClientClick='<%# GetDeleteConfirmScript(Eval("MaDotThanhToan")) %>'
                                    ResourceKey='<%# BackEndResourceKeys.DELETE %>' ButtonIcon="fas fa-trash" />
                                <asp:LinkButton runat="server" ID="lbtQuickApprove"
                                    CommandName="ITEM_QUICK_APPROVE" CausesValidation="false"
                                    Visible='<%# CURRENT_PAGE.IsEdit %>'
                                    Enabled='<%# !IsPaid(Eval("TrangThai")) %>'
                                    CssClass="btn btn-outline-success btn-sm text-center btn-smart-link"
                                    ToolTip='<%# GetResourceText(BackEndResourceKeys.PAYMENT_QUICK_APPROVE) %>'
                                    OnClientClick='<%# GetQuickApproveConfirmScript(Eval("MaDotThanhToan")) %>'>
                                    <i class="fas fa-check-circle"></i>
                                </asp:LinkButton>
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
