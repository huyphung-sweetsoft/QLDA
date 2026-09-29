<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlCost.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fCosts.Controls.CtrlCost" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Assembly="SweetSoft.QLDA.Controls" Namespace="SweetSoft.QLDA.Controls" TagPrefix="SweetSoft" %>

<style>
    /* =========================================================
       COST LIST
       ========================================================= */
    /* Grid giữ nguyên style như CtrlIssue: header + border từng ô/cell */
    .cost-list-table { margin-bottom: 0 !important; }
    .cost-list-table th,
    .cost-list-table td {
        vertical-align: middle !important;
    }
    .cost-code {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-width: 78px;
        padding: 4px 8px;
        border-radius: 6px;
        background: #f5f3ff;
        color: #6d28d9;
        border: 1px solid #ddd6fe;
        font-size: 11px;
        font-weight: 700;
    }
    .cost-name { color: #334155; font-weight: 600; line-height: 1.45; }
    .cost-number { font-variant-numeric: tabular-nums; white-space: nowrap; }
    .cost-total { color: #111827; font-weight: 700; font-variant-numeric: tabular-nums; white-space: nowrap; }

    /* Trạng thái Cost: dùng cùng một layout cho cả trạng thái tương tác và trạng thái readonly. */
    .cost-status {
        margin: 0 auto !important;
        width: 116px !important;
        min-width: 116px !important;
        max-width: 116px !important;
        height: 30px !important;
        box-sizing: border-box;
        padding: 4px 8px !important;
        display: flex !important;
        align-items: center;
        justify-content: center;
        border-radius: 999px !important;
        font-size: 12px;
        font-weight: 700;
        line-height: 1.2;
        text-align: center;
        white-space: nowrap;
    }

    .cost-status-pending {
        border: 1px solid #93c5fd !important;
        background: #f0f7ff !important;
        color: #1d4ed8 !important;
    }

    .cost-status-button {
        position: relative;
        text-decoration: none !important;
        overflow: hidden;
        transition: all .15s ease;
        cursor: pointer;
    }
    .cost-status-button:hover {
        background: #6d28d9 !important;
        border-color: #5b21b6 !important;
        color: transparent !important;
        box-shadow: 0 2px 6px rgba(109, 40, 217, .20);
        transform: translateY(-1px);
    }
    .cost-status-button:hover::after {
        content: "⚡ Duyệt nhanh";
        position: absolute;
        inset: 0;
        display: flex;
        align-items: center;
        justify-content: center;
        color: #fff;
        font-size: 12px;
        font-weight: 700;
    }

    .cost-status-done {
        border: 1px solid #4ade80 !important;
        background: #dcfce7 !important;
        color: #15803d !important;
        cursor: default;
    }

    .cost-status-rejected {
        border: 1px solid #f87171 !important;
        background: #fef2f2 !important;
        color: #dc2626 !important;
        cursor: default;
    }

    .cost-actions {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 4px;
        white-space: nowrap;
    }
    .cost-actions .btn-grid-action {
        min-width: 34px;
        height: 32px;
        padding: 5px 8px;
        border-radius: 6px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
    }
    .cost-actions .btn-grid-action:hover {
        background: #f5f3ff;
        color: #7c3aed !important;
        text-decoration: none !important;
    }
    .cost-actions .btn-grid-action.text-danger:hover { background: #fef2f2; color: #dc2626 !important; }
    .cost-attachment-btn {
        width: 34px;
        height: 32px;
        border-radius: 6px !important;
        display: inline-flex;
        align-items: center;
        justify-content: center;
    }
    .cost-list-table .empty-data { padding: 40px 20px !important; color: #94a3b8; }

    /* =========================================================
       ADD / EDIT MODAL
       ========================================================= */
    .cost-form { padding: 2px 2px 4px; }
    .cost-form-section {
        background: #fff;
        border: 1px solid #e5e7eb;
        border-radius: 10px;
        padding: 18px;
        margin-bottom: 16px;
    }
    .cost-form-section:last-child { margin-bottom: 0; }
    .cost-form-section-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-bottom: 16px;
        padding-bottom: 12px;
        border-bottom: 1px solid #f1f5f9;
    }
    .cost-form-section-title { color: #334155; font-size: 14px; font-weight: 700; }
    .cost-form-section-title i { color: #7c3aed; }
    .cost-form-section-subtitle { margin-top: 3px; color: #94a3b8; font-size: 11px; }
    .cost-form .form-label { color: #475569; font-size: 12px; font-weight: 600; margin-bottom: 6px; }
    .cost-form .form-control, .cost-form .form-select {
        border-color: #e2e8f0;
        border-radius: 7px;
        min-height: 38px;
        font-size: 13px;
        transition: all .15s ease;
    }
    .cost-form .form-control:focus, .cost-form .form-select:focus {
        border-color: #a78bfa;
        box-shadow: 0 0 0 3px rgba(124, 58, 237, .08);
    }
    .cost-input-money { position: relative; }
    .cost-input-money input { padding-right: 35px !important; }
    .cost-input-money > span {
        position: absolute;
        right: 12px;
        top: 50%;
        transform: translateY(-50%);
        color: #94a3b8;
        font-size: 12px;
        font-weight: 600;
        pointer-events: none;
    }
    .cost-total-box {
        min-height: 38px;
        display: flex;
        align-items: center;
        border: 1px solid #ddd6fe;
        background: #faf5ff;
        border-radius: 7px;
        overflow: hidden;
    }
    .cost-total-icon {
        width: 38px;
        align-self: stretch;
        display: flex;
        align-items: center;
        justify-content: center;
        background: #f5f3ff;
        color: #7c3aed;
        border-right: 1px solid #ddd6fe;
    }
    .cost-total-input { flex: 1; }
    .cost-total-input input {
        border: 0 !important;
        background: transparent !important;
        box-shadow: none !important;
        text-align: right;
        color: #6d28d9 !important;
        font-size: 15px !important;
        font-weight: 700 !important;
    }
    .cost-total-unit { padding-right: 12px; color: #7c3aed; font-size: 12px; font-weight: 700; }

    /* Requester person card - same visual language as Issue assignees. */
    .cost-person-card {
        min-height: 64px;
        display: flex;
        align-items: center;
        gap: 11px;
        padding: 9px 11px;
        border: 1px solid #e5e7eb;
        border-radius: 8px;
        background: #f8fafc;
    }
    .cost-person-avatar {
        width: 42px;
        height: 42px;
        flex: 0 0 42px;
        border-radius: 50%;
        overflow: hidden;
        display: flex;
        align-items: center;
        justify-content: center;
        color: #fff;
        font-size: 13px;
        font-weight: 700;
        background: #7c3aed;
    }
    .cost-person-avatar img { width: 100%; height: 100%; object-fit: cover; display: block; }
    .cost-person-main { min-width: 0; }
    .cost-person-name {
        color: #334155;
        font-size: 13px;
        font-weight: 700;
        line-height: 1.3;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }
    .cost-person-email {
        margin-top: 2px;
        color: #94a3b8;
        font-size: 11px;
        line-height: 1.3;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }
    .cost-readonly-field {
        min-height: 38px;
        display: flex;
        align-items: center;
        padding-left: 10px;
        border: 1px solid #e5e7eb;
        border-radius: 7px;
        background: #f8fafc;
    }
    .cost-readonly-field > i { width: 25px; color: #94a3b8; font-size: 12px; }
    .cost-readonly-field input { min-height: 36px !important; padding-left: 2px !important; }
    .cost-reject-box {
        padding: 14px;
        border: 1px solid #fecaca;
        border-left: 3px solid #ef4444;
        border-radius: 8px;
        background: #fff7f7;
    }
    .cost-reject-title { margin-bottom: 8px; color: #dc2626; font-size: 12px; font-weight: 700; }
    .cost-reject-title span { color: #dc2626; }
    .cost-reject-box textarea { background: #fff !important; }
    .cost-form-hint { margin-top: 8px; color: #94a3b8; font-size: 11px; }
    .cost-form-hint i { color: #7c3aed; }

    /* Fast approve modal */
    .cost-approve-modal { padding: 8px 10px 4px; }
    .cost-approve-icon {
        width: 54px;
        height: 54px;
        margin: 8px auto 14px;
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: 50%;
        background: #f5f3ff;
        color: #7c3aed;
        font-size: 21px;
    }
    .cost-approve-title { margin-bottom: 6px; color: #334155; font-weight: 700; }
    .cost-approve-description { max-width: 480px; margin: 0 auto; color: #94a3b8; font-size: 13px; line-height: 1.5; }
    .cost-approve-reject { border-top: 1px solid #e5e7eb; padding-top: 18px; }

    @media (max-width: 991px) {
        .cost-list-table { min-width: 1050px; }
        .cost-table-wrapper { overflow-x: auto; }
    }
    @media (max-width: 767px) {
        .cost-form-section { padding: 14px; }
        .cost-form-section-title { font-size: 13px; }
    }
</style>

<div class="card-header">
    <div class="d-flex flex-column flex-xl-row gap-3">
        <asp:UpdatePanel runat="server" ID="pnlSearchDropdowns" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel runat="server" ID="pnlSearchDefaultStatus">
                    <div class="d-flex">
                        <SweetSoft:BootstrapDropdown ID="ddlSearchTrangThaiChiPhi" runat="server"
                            Text="Trạng thái" AllowClear="true" AutoPostBack="true" SearchColumn="TrangThai"
                            CssClass="border-top-left-radius-1 border-bottom-left-radius-1"
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
            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
        </div>
        
        <div runat="server" id="tagOther" class="d-flex justify-content-end gap-3 w-full flex-wrap">
            <asp:UpdatePanel runat="server" ID="pnlButtons" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="d-flex">
                        <SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click" CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Info" ButtonIcon="Add">Thêm mới</SweetSoft:ExtraButton>
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


<div class="card-body p-0 cost-table-wrapper">
    <asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <SweetSoft:GridviewExtension ID="grvData" runat="server"
                AllowSorting="true" ShowHeader="true" ShowHeaderWhenEmpty="true" AutoGenerateColumns="false"
                CssClass="table-bordered table-hover align-middle cost-list-table" FocusBtnIcon="fas fa-compress-arrows-alt"
                DataKeyNames="IdChiPhi" ValueField="IdChiPhi" DataNameField="TenKhoanChi" GridLines="None"
                IsEnableSelectColumn="false" OnNeedDataSource="grvData_NeedDataSource" OnRowCommand="grvData_RowCommand">
                <Columns>
                    <asp:TemplateField HeaderText="CostCode" SortExpression="MaChiPhi" HeaderStyle-Width="105px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><span class="cost-code"><%# Eval("MaChiPhi") != DBNull.Value ? Eval("MaChiPhi") : "—" %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="CostName" SortExpression="TenKhoanChi">
                        <ItemTemplate><div class="cost-name"><%# Eval("TenKhoanChi") != DBNull.Value ? Eval("TenKhoanChi") : "—" %></div></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Price" SortExpression="DonGia" HeaderStyle-Width="120px" HeaderStyle-CssClass="text-end" ItemStyle-CssClass="text-end">
                        <ItemTemplate><span class="cost-number"><%# Eval("DonGia") != DBNull.Value ? Convert.ToDecimal(Eval("DonGia")).ToString("N0") : "0" %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Quantity" SortExpression="SoLuong" HeaderStyle-Width="75px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><span class="cost-number"><%# Eval("SoLuong") != DBNull.Value ? Eval("SoLuong") : "0" %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="TotalAmount" SortExpression="SoTien" HeaderStyle-Width="135px" HeaderStyle-CssClass="text-end" ItemStyle-CssClass="text-end">
                        <ItemTemplate><span class="cost-total"><%# Eval("SoTien") != DBNull.Value ? Convert.ToDecimal(Eval("SoTien")).ToString("N0") : "0" %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Requester" SortExpression="NhanVienYeuCau" HeaderStyle-Width="145px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# Eval("NhanVienYeuCau") != DBNull.Value ? Eval("NhanVienYeuCau") : "—" %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="DateCreated" SortExpression="NgayTao" HeaderStyle-Width="105px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# Eval("NgayTao") != DBNull.Value ? Convert.ToDateTime(Eval("NgayTao")).ToString("dd/MM/yyyy") : "—" %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Status" SortExpression="TrangThai" HeaderStyle-Width="150px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <!-- PM: trạng thái Chờ duyệt chính là nút xử lý nhanh -->
                            <asp:LinkButton runat="server" ID="lbtApproveStatus" CommandName="OPEN_APPROVE_MODAL"
                                CommandArgument='<%# Eval("IdChiPhi") %>' CausesValidation="false"
                                Visible='<%# this.IsPM && Eval("TrangThai") != DBNull.Value && Eval("TrangThai").ToString() == "0" %>'
                                CssClass="cost-status cost-status-pending cost-status-button" ToolTip="Duyệt/Từ chối nhanh khoản chi">
                                <i class="fas fa-clock me-1"></i><span>Chờ duyệt</span>
                            </asp:LinkButton>

                            <span runat="server"
                                visible='<%# !this.IsPM && Eval("TrangThai") != DBNull.Value && Eval("TrangThai").ToString() == "0" %>'
                                class="cost-status cost-status-pending">
                                Chờ duyệt
                            </span>

                            <span runat="server"
                                visible='<%# Eval("TrangThai") != DBNull.Value && Eval("TrangThai").ToString() == "1" %>'
                                class="cost-status cost-status-done" title="Khoản chi đã được phê duyệt">
                                <i class="fas fa-check me-1"></i>Đã duyệt
                            </span>

                            <span runat="server"
                                visible='<%# Eval("TrangThai") != DBNull.Value && Eval("TrangThai").ToString() == "2" %>'
                                class="cost-status cost-status-rejected" title="Khoản chi đã bị từ chối">
                                <i class="fas fa-times me-1"></i>Từ chối
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="145px">
                        <ItemTemplate>
                            <div class="cost-actions">
                                <asp:LinkButton runat="server" ID="lbtCostFiles" Visible='<%# this.IsView %>'
                                    CommandName="COST_FILES" CommandArgument='<%# Eval("IdChiPhi") %>' CausesValidation="false"
                                    CssClass="btn btn-outline-primary btn-sm cost-attachment-btn" ToolTip="File đính kèm chi phí">
                                    <i class="fas fa-folder-open"></i>
                                </asp:LinkButton>
                                <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsView %>'
                                    ID="lbtDetail" CommandName="ITEM_DETAIL" CssClass="btn-grid-action"
                                    ResourceKey='<%# this.IsEdit ? BackEndResourceKeys.EDIT : BackEndResourceKeys.VIEW %>'
                                    ButtonIcon='<%# this.IsView ? "fas fa-pencil-alt" : "fas fa-eye" %>'>
                                </SweetSoft:SmartLinkButton>
                                <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsDelete %>'
                                    ID="lbtDelete" CommandName="ITEM_DELETE" CssClass="btn-grid-action text-danger"
                                    ResourceKey='<%# BackEndResourceKeys.DELETE %>' ButtonIcon="fas fa-trash">
                                </SweetSoft:SmartLinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="empty-data text-center"><i class="fas fa-receipt fa-2x mb-2 d-block"></i><%= GetResourceText(BackEndResourceKeys.NO_DATA) %></div>
                </EmptyDataTemplate>
            </SweetSoft:GridviewExtension>
            <SweetSoft:Paging runat="server" ID="ctrlGridviewPaging" OnPageChanged="ctrlGridviewPaging_PageChanged" />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>


<asp:HiddenField runat="server" ID="hdfApproveCostId" />

<!-- POPUP DUYỆT / TỪ CHỐI NHANH -->
<SweetSoft:ExtraModal runat="server" ID="mdlFastApprove" Type="Primary" Title="Xử lý khoản chi">
    <ContentTemplate>
        <div class="cost-approve-modal">
            <div id="divApproveMode">
                <div class="cost-approve-icon"><i class="fas fa-file-invoice-dollar"></i></div>
                <div class="text-center">
                    <h5 class="cost-approve-title">Xác nhận xử lý khoản chi</h5>
                    <p class="cost-approve-description">Khoản chi này đang chờ phê duyệt. Vui lòng chọn Duyệt hoặc Từ chối.</p>
                </div>
                <div id="grpActionButtons" class="d-flex justify-content-center gap-2 mt-4">
                    <asp:LinkButton ID="btnQuickApprove" runat="server" CssClass="btn btn-success px-4" CausesValidation="false" OnClick="btnQuickApprove_Click">
                        <i class="fas fa-check me-2"></i> Duyệt
                    </asp:LinkButton>
                    <button type="button" class="btn btn-outline-danger px-4" onclick="$('#grpActionButtons').hide(); $('#divRejectReason').fadeIn();">
                        <i class="fas fa-times me-2"></i> Từ chối
                    </button>
                </div>
            </div>
            <div id="divRejectReason" style="display:none;" class="cost-approve-reject">
                <div class="cost-reject-box">
                    <div class="cost-reject-title"><i class="fas fa-exclamation-circle me-1"></i> Lý do từ chối <span>*</span></div>
                    <asp:TextBox runat="server" ID="txtRejectReason" CssClass="form-control" TextMode="MultiLine" Rows="4" placeholder="Nhập lý do để nhân viên điều chỉnh..."></asp:TextBox>
                </div>
                <div class="d-flex justify-content-end gap-2 mt-3">
                    <button type="button" class="btn btn-outline-secondary" onclick="$('#divRejectReason').hide(); $('#grpActionButtons').fadeIn(); $('#<%= txtRejectReason.ClientID %>').val('');">Quay lại</button>
                    <asp:LinkButton ID="btnConfirmReject" runat="server" CssClass="btn btn-danger" CausesValidation="false" OnClick="btnConfirmReject_Click">
                        <i class="fas fa-paper-plane me-1"></i> Xác nhận từ chối
                    </asp:LinkButton>
                </div>
            </div>
        </div>
    </ContentTemplate>
</SweetSoft:ExtraModal>

<div class="offcanvas offcanvas-end offcanvas-form-search" id="search-offcanvas" aria-hidden="true">
    <div class="offcanvas-header">
        <div class="flex flex-column flex-md-row align-items-center gap-3">
            <h5 class="offcanvas-title"><%= GetResourceText(BackEndResourceKeys.ADVANCED_SEARCH) %></h5>
        </div>
        <button class="btn-close" type="button" data-bs-dismiss="offcanvas" aria-label="Close"></button>
    </div>
    <div class="div offcanvas-body pt-0">
        <div class="card shadow-none card-body text-muted mb-0">
            <asp:UpdatePanel runat="server" ID="pnlSearch" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel runat="server" ID="pnlSearchPopup">
                        <div class="row g-3 mb-3">
                            <div class="col-md-6">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.COST_NAME) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchTenKhoanChi" SearchColumn="TenKhoanChi"></SweetSoft:ExtraTextBox>
                            </div>
                            <div class="col-md-6">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.REQUESTER) %></label>
                                <asp:DropDownList ID="ddlSearchNhanVienYeuCau" runat="server" CssClass="form-select" SearchColumn="IdNhanVienDeNghi"></asp:DropDownList>
                            </div>
                        </div>

                        <div class="row g-3 mb-3">
                            <div class="col-md-6">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.LOWEST_TOTAL_AMOUNT) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSoTienMin" SearchColumn="SoTienMin" CssClass="format-currency"></SweetSoft:ExtraTextBox>
                            </div>
                            <div class="col-md-6">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.HIGHEST_TOTAL_AMOUNT) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSoTienMax" SearchColumn="SoTienMax" CssClass="format-currency"></SweetSoft:ExtraTextBox>
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
