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
            <SweetSoft:GridviewExtension ID="grvData" runat="server" AllowSorting="true" ShowHeader="true" ShowHeaderWhenEmpty="true" AutoGenerateColumns="false" CssClass="table-bordered table-hover align-middle issue-grid" FocusBtnIcon="fas fa-compress-arrows-alt" DataKeyNames="IdVanDe" ValueField="IdVanDe" DataNameField="TenVanDe" GridLines="None" IsEnableSelectColumn="false" OnNeedDataSource="grvData_NeedDataSource" OnRowCommand="grvData_RowCommand" OnRowDataBound="grvData_RowDataBound">
                <Columns>
                    <asp:TemplateField HeaderText="IssueName" SortExpression="TenVanDe" HeaderStyle-CssClass="text-center">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbtIssueName" runat="server" CommandName="ITEM_VIEW_DETAIL" CssClass="issue-name-link" Visible='<%# this.IsEdit || this.IsView %>' CausesValidation="false"><%# Eval("TenVanDe") != DBNull.Value && Eval("TenVanDe") != null ? Eval("TenVanDe") : "—" %></asp:LinkButton>
                            <asp:Literal ID="ltrIssueName" runat="server" Visible='<%# !this.IsEdit && !this.IsView %>' Text='<%# Eval("TenVanDe") != DBNull.Value && Eval("TenVanDe") != null ? Eval("TenVanDe") : "—" %>'></asp:Literal>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Impact" SortExpression="MucDoAnhHuong" HeaderStyle-Width="150px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center"><ItemTemplate><%# GetMucDoAnhHuongText(Eval("MucDoAnhHuong")) %></ItemTemplate></asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Status" SortExpression="TrangThai" HeaderStyle-Width="140px" ItemStyle-Width="140px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%-- Giống Task: CHỈ MỘT nhãn hoặc một nút ứng với trạng thái hiện tại. --%>
                            <asp:Literal runat="server" ID="ltrIssueStatus" Visible='<%# !CoNutTrangThaiIssue(Eval("TrangThai"),Eval("TrangThaiDuyetGio"),Eval("LaNguoiXuLy"),Eval("DaBaoGioVanDe")) && (LaKbg20Issue || !IsEdit || Convert.ToInt32(Eval("TrangThai")) != 0) %>' Text='<%# GetIssueStatusBadge(Eval("TrangThai"),Eval("TrangThaiDuyetGio")) %>' />
                            <asp:LinkButton runat="server" ID="lbtStatusFastCompleteIssue" CausesValidation="false" CommandName="FAST_COMPLETE_ISSUE" CommandArgument='<%# Eval("IdVanDe") %>' Visible='<%# DuocGuiDuyetIssue(Eval("TrangThai"),Eval("TrangThaiDuyetGio"),Eval("LaNguoiXuLy"),Eval("DaBaoGioVanDe")) %>' CssClass="badge-pill-custom badge-status-btn badge-status-doing" ToolTip="Gửi yêu cầu hoàn thành vấn đề">
                                <span class="status-normal">Đang xử lý</span>
                                <span class="status-hover"><i class="fas fa-check-double me-1"></i>Chốt xong!</span>
                            </asp:LinkButton>
                            <asp:LinkButton runat="server" ID="lbtStatusFastReviewIssue" CausesValidation="false" CommandName="REVIEW_COMPLETE_ISSUE" CommandArgument='<%# Eval("IdVanDe") %>' Visible='<%# DuocDuyetIssue(Eval("TrangThai"),Eval("TrangThaiDuyetGio")) %>' CssClass="badge-pill-custom badge-status-btn badge-status-review" ToolTip="Duyệt hoặc từ chối yêu cầu hoàn thành">
                                <span class="status-normal">Chờ duyệt</span>
                                <span class="status-hover"><i class="fas fa-clipboard-check me-1"></i>Duyệt nhanh</span>
                            </asp:LinkButton>
                            <%-- Giữ đường hoàn thành V1 nếu KBG20 chưa kích hoạt. --%>
                            <asp:LinkButton runat="server" ID="lbtLegacyCompleteIssue" CausesValidation="false" CommandName="ITEM_COMPLETE_PROCESS" CommandArgument='<%# Eval("IdVanDe") %>' Visible='<%# !LaKbg20Issue && IsEdit && Convert.ToInt32(Eval("TrangThai")) == 0 %>' CssClass="badge-pill-custom badge-status-btn badge-status-doing" ToolTip="Đánh dấu đã xử lý theo luồng cũ">
                                <span class="status-normal">Đang xử lý</span>
                                <span class="status-hover"><i class="fas fa-check-double me-1"></i>Chốt xong!</span>
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Origin" SortExpression="NguonGocVanDe" HeaderStyle-Width="220px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center"><ItemTemplate><%# GetNguonGocVanDeText(Eval("NguonGocVanDe")) %></ItemTemplate></asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="CreatedBy" SortExpression="NguoiTao" HeaderStyle-Width="120px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center"><ItemTemplate><%# Eval("NguoiTao") != DBNull.Value && Eval("NguoiTao") != null ? Eval("NguoiTao") : "—" %></ItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="CreatedDate" SortExpression="NgayTao" HeaderStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <%# GetFormattedDate(Eval("NgayTao")) %>
                        </ItemTemplate>
                    </asp:TemplateField>                    
                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="190px" ItemStyle-Width="190px">
                        <ItemTemplate>
                            <div class="d-flex justify-content-center align-items-center gap-2 flex-wrap issue-workflow-actions">
                                <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsEdit || this.IsView %>' ID="lbtDetail" CommandName="ITEM_DETAIL" CssClass="btn-grid-action text-decoration-underline" ResourceKey='<%# this.IsEdit ? BackEndResourceKeys.EDIT : BackEndResourceKeys.VIEW %>' ButtonIcon='<%# this.IsEdit ? "fas fa-pencil-alt" : "fas fa-eye" %>'></SweetSoft:SmartLinkButton>
                                <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsDelete %>' ID="lbtDelete" CommandName="ITEM_DELETE" CssClass="btn-grid-action text-decoration-underline text-danger" ResourceKey='<%# BackEndResourceKeys.DELETE %>' ButtonIcon="fas fa-trash"></SweetSoft:SmartLinkButton>
                                <%-- Y hệt Task: nút Tính công luôn có mặt khi xem được bảng; RowDataBound sẽ khóa/mở + tooltip. --%>
                                <span runat="server" id="qlpmActionWrap" class="qlpm-action-wrap">
                                    <SweetSoft:SmartLinkButton runat="server" ID="lbtDuyetCong" CommandName="ISSUE_PM_HOURS" CommandArgument='<%# Eval("IdVanDe") %>' VisibleConditionKey='<%# this.IsView || this.IsEdit || this.CURRENT_PAGE.IsPM || this.CURRENT_PAGE.IsAdministrator %>' ResourceKey='<%# BackEndResourceKeys.VIEW %>' ButtonIcon="fas fa-file-invoice" CssClass="btn-grid-action btn-qlpm-review text-decoration-none" CausesValidation="false" ToolTip="Tính công vấn đề"></SweetSoft:SmartLinkButton>
                                    <asp:Label ID="lblPmPending" runat="server" CssClass="btn-qlpm-pending" Text="!" Visible="false" />
                                </span>
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
    .issue-name-link { color:#542e88!important;font-size:16px;font-weight:500;text-decoration:none!important;transition:color .15s ease; }
    .issue-name-link:hover { color:#3b82f6!important;text-decoration:none!important; }
    /* Bản sao các quy tắc badge/status-hover từ CtrlTask. Scope trong CtrlIssue để không chạm Task. */
    .issue-grid .badge-pill-custom { padding:4px 8px!important;border-radius:4px!important;font-size:11px!important;font-weight:600!important;display:inline-block!important;white-space:nowrap!important;line-height:1.2!important; }
    .issue-grid .badge-status-doing { background-color:#e0f2fe!important;color:#0369a1!important;border:1px solid #bae6fd!important; }
    .issue-grid .badge-status-done { background-color:#dcfce7!important;color:#15803d!important;border:1px solid #bbf7d0!important; }
    .issue-grid .badge-status-review { background-color:#fef3c7!important;color:#92400e!important;border:1px solid #fcd34d!important; }
    .issue-grid .badge-status-btn { position:relative;cursor:pointer;overflow:hidden;display:inline-flex!important;justify-content:center;align-items:center;min-width:100px;text-decoration:none!important;vertical-align:middle; }
    .issue-grid .badge-status-btn .status-normal { display:inline-block;transition:transform .25s cubic-bezier(.4,0,.2,1),opacity .25s ease;width:100%; }
    .issue-grid .badge-status-btn .status-hover { position:absolute;top:0;left:0;right:0;bottom:0;display:flex;align-items:center;justify-content:center;transform:translateY(100%);opacity:0;transition:transform .25s cubic-bezier(.4,0,.2,1),opacity .25s ease;background-color:#15803d!important;color:#fff!important;border-radius:4px;font-weight:700;font-size:11px; }
    .issue-grid .badge-status-btn:hover .status-normal { transform:translateY(-100%);opacity:0; }
    .issue-grid .badge-status-btn:hover .status-hover { transform:translateY(0);opacity:1; }
    .issue-grid .badge-status-btn:hover { border-color:#15803d!important;box-shadow:0 3px 6px rgba(21,128,61,.3); }
    .issue-grid .badge-status-btn.badge-status-review .status-hover { background-color:#b45309!important;color:#fff!important; }
    .issue-grid .badge-status-btn.badge-status-review:hover { border-color:#b45309!important;box-shadow:0 3px 6px rgba(180,83,9,.24)!important; }
    /* Đúng khuôn nút PM của CtrlTask: luôn hiện, khóa + chú thích nếu chưa đủ điều kiện. */
    .issue-grid .qlpm-action-wrap { display:inline-flex;position:relative;align-items:center;justify-content:center;vertical-align:middle;margin-left:3px; }
    .issue-grid .qlpm-action-wrap .btn-qlpm-review { display:inline-flex!important;align-items:center;justify-content:center;min-width:38px;height:27px;padding:2px 7px!important;color:#6635a5!important;border:1px solid #9973c7!important;border-radius:4px;line-height:1.1;text-decoration:none!important;background:#fff;transition:background .15s,border-color .15s,color .15s; }
    .issue-grid .qlpm-action-wrap .btn-qlpm-review:hover { background:#f3ebff!important;color:#4b1a79!important;border-color:#7a4baa!important; }
    .issue-grid .qlpm-action-wrap .btn-qlpm-review.is-final { color:#197a55!important;border-color:#86bea4!important; }
    .issue-grid .qlpm-action-wrap .btn-qlpm-review.is-disabled,.issue-grid .qlpm-action-wrap .btn-qlpm-review.aspNetDisabled { color:#a1a8b3!important;border-color:#d8dce3!important;background:#f9fafb!important;cursor:not-allowed;opacity:.65;pointer-events:none; }
    .issue-grid .qlpm-action-wrap .btn-qlpm-pending,.issue-grid .qlpm-action-wrap .btn-qlpm-final-mark { position:absolute;right:-4px;top:-7px;display:inline-flex;align-items:center;justify-content:center;width:14px;height:14px;border-radius:50%;font-weight:900;font-size:10px;line-height:1;box-shadow:0 0 0 1px #fff;pointer-events:none; }
    .issue-grid .qlpm-action-wrap .btn-qlpm-pending { background:#fff1df;color:#bd6414;border:1px solid #ffe0b5; }
    .issue-grid .qlpm-action-wrap .btn-qlpm-final-mark { background:#e4f7eb;color:#177a49;border:1px solid #9bd8b2;font-size:10.5px; }
    .issue-grid .qlpm-action-wrap .is-disabled~.btn-qlpm-pending,.issue-grid .qlpm-action-wrap .is-disabled~.btn-qlpm-final-mark { display:none!important; }
    .issue-grid .issue-workflow-actions { min-width:0;max-width:100%; }
</style>
