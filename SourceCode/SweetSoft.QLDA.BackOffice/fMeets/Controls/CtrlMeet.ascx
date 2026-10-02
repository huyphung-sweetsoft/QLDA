<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlMeet.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fMeets.Controls.CtrlMeet" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fMeets/Controls/CtrlXemNhanVienMeet.ascx" TagPrefix="SweetSoft" TagName="CtrlXemNhanVienMeet" %>
<%@ Register Src="~/fMeets/Controls/CtrlViewMeetDetail.ascx" TagPrefix="SweetSoft" TagName="CtrlViewMeetDetail" %>
<style>
    .avatar-group { display: inline-flex !important; align-items: center; justify-content: center; gap: 6px !important; flex-wrap: nowrap !important; white-space: nowrap !important; }
    .avatar-stack-container { display: flex; align-items: center; min-width: 0; }
    .avatar-circle { width: 28px; height: 28px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 700; color: #ffffff; border: 2px solid #ffffff; margin-left: -8px; position: relative; z-index: 1; box-shadow: 0 1px 2px rgba(0,0,0,0.1); }
    .avatar-circle:first-child { margin-left: 0; }
    .avatar-more { background-color: #f1f5f9; color: #475569; border-color: #cbd5e1; z-index: 0; font-weight: 800; font-size: 10px; }

    .btn-assign-task {
        width: 28px;
        height: 28px;
        border-radius: 7px;
        background-color: #2563eb;
        color: #ffffff;
        display: flex;
        align-items: center;
        justify-content: center;
        border: none;
        cursor: pointer;
        text-decoration: none;
        font-size: 12px;
        transition: background 0.2s, transform 0.15s, box-shadow 0.15s;
        flex-shrink: 0;
    }

    .btn-assign-task:hover {
        background-color: #1d4ed8;
        color: #ffffff;
        transform: translateY(-1px);
        box-shadow: 0 3px 8px rgba(37,99,235,0.2);
    }

    .btn-assign-task.view-only { background-color: #64748b; }

    .btn-assign-task.view-only:hover {
        background-color: #475569;
        box-shadow: 0 3px 8px rgba(71,85,105,0.2);
    }

    .meeting-code-cell {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-width: 78px;
        padding: 5px 10px;
        border: 1px solid #dbe3ee;
        border-radius: 7px;
        background: #f8fafc;
        color: #334155;
        font-size: 12px;
        font-weight: 700;
        letter-spacing: .2px;
        white-space: nowrap;
    }

    .meeting-name-cell {
        display: flex;
        align-items: center;
        width: 100%;
        min-width: 0;
        text-align: left;
    }

    .meeting-name-text {
        min-width: 0;
        width: 100%;
    }

    /* =======================================================
       CSS TÊN CUỘC HỌP (ĐỒNG BỘ GIỐNG ẢNH: CHỮ TÍM, GỌN GÀNG)
       ======================================================= */
    .meeting-name-main {
        display: block;
        width: 100%;
        color: #542e88 !important;
        font-size: 16px;
        font-weight: 500;
        line-height: 1.45;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        text-decoration: none !important;
        transition: color .15s ease;
    }
    .meeting-name-main:hover {
        color: #3b82f6 !important;
        text-decoration: none !important;
    }

    .meeting-name-sub {
        margin-top: 2px;
        color: #94a3b8;
        font-size: 11px;
        line-height: 1.2;
    }

    .meeting-datetime {
        display: inline-flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        gap: 3px;
        min-width: 104px;
        line-height: 1.25;
    }

    .meeting-date {
        color: #334155;
        font-size: 12px;
        font-weight: 600;
        white-space: nowrap;
    }

    .meeting-time {
        color: #64748b;
        font-size: 11px;
        white-space: nowrap;
    }

    .meeting-time i { font-size: 10px; }

    .meeting-room-cell {
        display: flex;
        align-items: center;
        justify-content: flex-start;
        gap: 8px;
        max-width: 220px;
        color: #475569;
        font-size: 12px;
        line-height: 1.35;
        text-align: left;
        min-width: 0;
    }

    .meeting-room-icon {
        width: 28px;
        height: 28px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        flex: 0 0 28px;
        border-radius: 7px;
        background: #f8fafc;
        border: 1px solid #e2e8f0;
        color: #64748b;
        font-size: 12px;
    }

    .meeting-room-text {
        min-width: 0;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .meeting-status-badge {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: 7px;
        min-width: 110px;
        padding: 6px 11px;
        border: 1px solid transparent;
        border-radius: 999px;
        font-size: 11px;
        font-weight: 700;
        line-height: 1.2;
        white-space: nowrap;
        transition: all 0.2s ease;
    }

    .meeting-status-badge::before {
        content: "";
        width: 7px;
        height: 7px;
        border-radius: 50%;
        background: currentColor;
        flex: 0 0 7px;
    }

    .meeting-status-0 {
        color: #2563eb;
        background: #eff6ff;
        border-color: #bfdbfe;
    }

    .meeting-status-1 {
        color: #d97706;
        background: #fffbeb;
        border-color: #fde68a;
    }

    .meeting-status-2 {
        color: #16a34a;
        background: #f0fdf4;
        border-color: #bbf7d0;
    }

    .meeting-status-3 {
        color: #64748b;
        background: #f8fafc;
        border-color: #cbd5e1;
    }

    .meeting-action-group {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: 5px;
    }

    .meeting-action-group .btn-grid-action {
        width: 32px;
        min-width: 32px;
        height: 32px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        border-radius: 7px;
    }

    .meeting-action-group .btn-smart-link {
        width: 32px;
        min-width: 32px;
        height: 32px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        border-radius: 7px;
    }

    .meeting-table-cell { vertical-align: middle !important; }

    @media (max-width: 991.98px) {
        .meeting-name-main { font-size: 16px; }
        .meeting-status-badge { min-width: 100px; padding: 5px 9px; }
        .meeting-room-cell { max-width: 160px; }
    }
</style>

<div class="card-header">
    <div class="d-flex flex-column flex-xl-row gap-3">
        <asp:UpdatePanel runat="server" ID="pnlSearchDropdowns" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel runat="server" ID="pnlSearchDefaultStatus">
                    <div class="d-flex">
                        <SweetSoft:BootstrapDropdown ID="ddlSearchTrangThai" runat="server" Text="Trạng thái" AllowClear="true" AutoPostBack="true" SearchColumn="TrangThai" CssClass="border-top-left-radius-1 border-bottom-left-radius-1" OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged"></SweetSoft:BootstrapDropdown>
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>

        <div class="input-group max-w-500 mb-3">
            <a class="btn btn-info font-mobile-small btn-search-filter" onclick="CMSMasterJs.ShowOffcanvasSearch();" href="javascript:;">
                <i class="fas fa-filter me-1"></i>
                <%= GetResourceText(BackEndResourceKeys.FILTER) %>
            </a>
            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
        </div>

        <div runat="server" id="tagOther" class="d-flex justify-content-end gap-3 w-full flex-wrap">
            <asp:UpdatePanel runat="server" ID="pnlButtons" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="d-flex">
                        <SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click" CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Info" ButtonIcon="Add">
                            Thêm mới
                        </SweetSoft:ExtraButton>
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
            <SweetSoft:GridviewExtension ID="grvData" runat="server" AllowSorting="true" ShowHeader="true" ShowHeaderWhenEmpty="true" AutoGenerateColumns="false" CssClass="table table-bordered table-hover align-middle w-100" FocusBtnIcon="fas fa-compress-arrows-alt" DataKeyNames="IdLichHop" ValueField="IdLichHop" DataNameField="TenCuocHop" GridLines="None" IsEnableSelectColumn="false" OnNeedDataSource="grvData_NeedDataSource" OnRowCommand="grvData_RowCommand">
                <Columns>
                    <asp:TemplateField HeaderText="MeetingName" SortExpression="TenCuocHop" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="meeting-table-cell">
                        <ItemTemplate>
                            <div class="meeting-name-cell">
                                <div class="meeting-name-text">
                                    <asp:LinkButton
                                        runat="server"
                                        ID="lbtMeetingName"
                                        CommandName="ITEM_VIEW"
                                        CommandArgument='<%# Eval("IdLichHop") %>'
                                        CausesValidation="false"
                                        CssClass="meeting-name-main"
                                        ToolTip="Xem thông tin cuộc họp">
                                        <%# Eval("TenCuocHop") ?? "—" %>
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Người tham gia" HeaderStyle-Width="180px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center meeting-table-cell" ItemStyle-Wrap="false">
                        <ItemTemplate>
                            <div class="avatar-group">
                                <div class="avatar-stack-container">
                                    <%# GetAssigneeDisplay(Eval("TenNhanVien"), Eval("Avatars")) %>
                                </div>
                                <asp:LinkButton runat="server" ID="lbtViewMembers" CommandName="VIEW_MEMBERS" CommandArgument='<%# Eval("IdLichHop") %>' CssClass="btn-assign-task view-only" ToolTip="Xem thành viên" Visible='<%# this.IsView || this.IsEdit %>'>
                                    <i class="fas fa-user-friends"></i>
                                </asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="StartTime" SortExpression="ThoiGianBatDau" HeaderStyle-Width="130px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center meeting-table-cell">
                        <ItemTemplate>
                            <div class="meeting-datetime">
                                <span class="meeting-date"><%# Eval("ThoiGianBatDau", "{0:dd/MM/yyyy}") %></span>
                                <span class="meeting-time"><i class="far fa-clock me-1"></i><%# Eval("ThoiGianBatDau", "{0:HH:mm}") %></span>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="EndTime" SortExpression="ThoiGianKetThuc" HeaderStyle-Width="130px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center meeting-table-cell">
                        <ItemTemplate>
                            <div class="meeting-datetime">
                                <span class="meeting-date"><%# Eval("ThoiGianKetThuc", "{0:dd/MM/yyyy}") %></span>
                                <span class="meeting-time"><i class="far fa-clock me-1"></i><%# Eval("ThoiGianKetThuc", "{0:HH:mm}") %></span>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="MeetingRoom" SortExpression="DiaDiemHop" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="meeting-table-cell">
                        <ItemTemplate>
                            <div class="meeting-room-cell">
                                <span class="meeting-room-icon">
                                    <i class="fas fa-map-marker-alt"></i>
                                </span>
                                <span class="meeting-room-text" title='<%# Eval("DiaDiemHop") %>'>
                                    <%# Eval("DiaDiemHop") ?? "—" %>
                                </span>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Status" SortExpression="TrangThai" HeaderStyle-Width="150px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center meeting-table-cell">
                        <ItemTemplate>
                            <span class='<%# "meeting-status-badge meeting-status-" + Convert.ToString(Eval("TrangThai")) %>'>
                                <%# GetTrangThaiCuocHopText(Eval("TrangThai")) %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center meeting-table-cell" HeaderStyle-Width="150px">
                        <ItemTemplate>
                            <div class="meeting-action-group">
                                <asp:LinkButton runat="server" ID="lbtMeetingFiles" Visible='<%# this.IsView %>' CommandName="MEETING_FILES" CommandArgument='<%# Eval("IdLichHop") %>' CausesValidation="false" CssClass="btn btn-outline-primary btn-sm text-center btn-smart-link" ToolTip="File đính kèm lịch họp">
                                    <i class="fas fa-folder-open"></i>
                                </asp:LinkButton>
                                <SweetSoft:SmartLinkButton runat="server"
                                    VisibleConditionKey='<%# this.IsEdit || this.IsView %>'
                                    ID="lbtDetail"
                                    CommandName='<%# this.IsEdit ? "ITEM_DETAIL" : "ITEM_VIEW" %>'
                                    CommandArgument='<%# Eval("IdLichHop") %>'
                                    CssClass="btn-grid-action text-decoration-underline"
                                    ResourceKey='<%# this.IsEdit ? BackEndResourceKeys.EDIT : BackEndResourceKeys.VIEW %>'
                                    ButtonIcon='<%# this.IsEdit ? "fas fa-pencil-alt" : "fas fa-eye" %>'>
                                </SweetSoft:SmartLinkButton>                                <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsDelete %>' ID="lbtDelete" CommandName="ITEM_DELETE" CssClass="btn-grid-action text-decoration-underline text-danger" ResourceKey='<%# BackEndResourceKeys.DELETE %>' ButtonIcon="fas fa-trash"></SweetSoft:SmartLinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>

                <EmptyDataTemplate>
                    <%= GetResourceText(BackEndResourceKeys.NO_DATA) %>
                </EmptyDataTemplate>
            </SweetSoft:GridviewExtension>

            <SweetSoft:Paging runat="server" ID="ctrlGridviewPaging" OnPageChanged="ctrlGridviewPaging_PageChanged" />

            <asp:LinkButton runat="server" ID="btnRefreshMeetingStatuses" OnClick="btnRefreshMeetingStatuses_Click" CausesValidation="false" Style="display:none;"></asp:LinkButton>
            <SweetSoft:CtrlViewMeetDetail runat="server" ID="CtrlViewMeetDetail1" />
            <SweetSoft:CtrlXemNhanVienMeet runat="server" ID="CtrlXemNhanVienMeet1" />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>

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
                <SweetSoft:ExtraButton runat="server" ID="lbtSearchAdvanced" CssClass="flex-btn" ButtonStyle="Primary" ButtonIcon="Search" OnClick="btnSearchAdvanced_ServerClick">
                    Áp dụng
                </SweetSoft:ExtraButton>
                <SweetSoft:ExtraButton runat="server" ID="lbtCancel" CssClass="flex-btn" ButtonStyle="OutLineSecondary" ButtonIcon="Refresh" OnClick="btnCancel_Click">
                    Làm mới
                </SweetSoft:ExtraButton>
            </div>
        </div>
    </div>
</div>

<script type="text/javascript">
    (function () {
        if (window.meetStatusAutoRefreshTimer) return;

        window.meetStatusAutoRefreshTimer = window.setInterval(function () {
            if ($('.modal.show:visible, .modal.in:visible').length > 0) return;

            var refreshButton = document.getElementById('<%= btnRefreshMeetingStatuses.ClientID %>');
            if (refreshButton) {
                refreshButton.click();
            }
        }, 60000);
    })();
</script>