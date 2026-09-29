<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="TaskList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.TaskList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fTasks/Controls/CtrlTask.ascx" TagPrefix="SweetSoft" TagName="CtrlTask" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
<style>
    .smart-field { display: flex; flex-direction: column; width: 100%; min-width: 0; }
    .smart-field > label { display: flex; align-items: center; margin-bottom: 8px; color: #374151; font-size: 13.5px; font-weight: 700; line-height: 1.3; }
    .smart-field > label i { width: 17px; margin-right: 4px; color: #6b7280 !important; text-align: center; }
    .smart-field > .form-control,
    .smart-field > .form-select,
    .smart-field .input-group > .form-control,
    .smart-field .input-group > .form-select { min-height: 46px !important; padding: 10px 13px !important; border: 1px solid #d1d5db !important; border-radius: 9px !important; background-color: #ffffff !important; color: #1f2937 !important; font-size: 14px !important; font-weight: 500 !important; box-shadow: 0 1px 2px rgba(17, 24, 39, 0.04) !important; transition: border-color .18s ease, box-shadow .18s ease, background-color .18s ease; }
    .smart-field .form-control::placeholder { color: #9ca3af !important; font-weight: 400 !important; }
    .smart-field > .form-control:hover:not(:disabled):not([readonly]),
    .smart-field > .form-select:hover:not(:disabled),
    .smart-field .input-group > .form-control:hover:not(:disabled):not([readonly]) { border-color: #a78bfa !important; background-color: #ffffff !important; }
    .smart-field > .form-control:focus,
    .smart-field > .form-select:focus,
    .smart-field .input-group > .form-control:focus { border-color: #7c3aed !important; background-color: #ffffff !important; box-shadow: 0 0 0 3px rgba(124, 58, 237, 0.12), 0 2px 5px rgba(17, 24, 39, 0.04) !important; outline: none !important; }
    .smart-field textarea.form-control { min-height: 98px !important; resize: vertical; line-height: 1.55; padding-top: 11px !important; }
    .smart-field > .form-select { padding-right: 38px !important; cursor: pointer; }
    .smart-field > .form-select:focus { cursor: pointer; }
    .smart-field:has(input[readonly]),
    .smart-field:has(textarea[readonly]) { padding: 11px 14px 12px 14px; background: #f6f3ff; border: 1px solid #ddd6fe; border-left: 4px solid #8b5cf6; border-radius: 9px; box-shadow: 0 1px 2px rgba(76, 29, 149, 0.04); justify-content: center; }
    .smart-field:has(input[readonly]) > label,
    .smart-field:has(textarea[readonly]) > label { margin-bottom: 4px; color: #6d28d9; font-size: 10.5px; font-weight: 800; letter-spacing: .45px; text-transform: uppercase; }
    .smart-field:has(input[readonly]) > label i,
    .smart-field:has(textarea[readonly]) > label i { color: #8b5cf6 !important; }
    .smart-field:has(input[readonly]) .form-control,
    .smart-field:has(textarea[readonly]) .form-control { min-height: auto !important; height: auto !important; padding: 0 !important; border: none !important; border-radius: 0 !important; background: transparent !important; color: #312e81 !important; font-size: 15px !important; font-weight: 750 !important; box-shadow: none !important; outline: none !important; cursor: default !important; }
    .smart-field:has(input:disabled),
    .smart-field:has(select:disabled) { padding: 11px 14px 12px 14px; background: #f3f4f6; border: 1px solid #d1d5db; border-left: 4px solid #9ca3af; border-radius: 9px; box-shadow: inset 0 1px 0 rgba(255,255,255,.8); justify-content: center; }
    .smart-field:has(input:disabled) > label,
    .smart-field:has(select:disabled) > label { margin-bottom: 4px; color: #6b7280; font-size: 10.5px; font-weight: 800; letter-spacing: .45px; text-transform: uppercase; }
    .smart-field:has(input:disabled) > label i,
    .smart-field:has(select:disabled) > label i { color: #9ca3af !important; }
    .smart-field:has(input:disabled) .form-control,
    .smart-field:has(select:disabled) .form-select { min-height: auto !important; height: auto !important; padding: 0 !important; border: none !important; border-radius: 0 !important; background: transparent !important; color: #374151 !important; font-size: 15px !important; font-weight: 750 !important; box-shadow: none !important; outline: none !important; opacity: 1 !important; cursor: default !important; appearance: none !important; -webkit-appearance: none !important; }
    #colCongViecCha .smart-field:has(select:disabled) { background: #f4f4f5; border-color: #d4d4d8; border-left: 4px solid #a1a1aa; }
    #colCongViecCha .smart-field:has(select:disabled) > label { color: #71717a; }
    #colCongViecCha .smart-field:has(select:disabled) > label i { color: #a1a1aa !important; }
    .smart-field .input-group { display: flex; flex-wrap: nowrap; align-items: stretch; }
    .smart-field .input-group > .form-control { border-radius: 9px 0 0 9px !important; }
    .smart-field .input-group > .input-group-text { min-height: 46px; padding: 0 13px; border: 1px solid #d1d5db; border-left: 0; border-radius: 0 9px 9px 0; background: #f5f3ff; color: #6b7280; font-size: 13px; font-weight: 700; }
    .smart-field:has(input[readonly]) .input-group-text,
    .smart-field:has(input:disabled) .input-group-text { display: none !important; }
    .duration-suffix { display: none; margin-left: 7px; color: #4b5563; font-size: 14px !important; font-weight: 750 !important; white-space: nowrap; align-self: center; }
    .smart-field:has(input[readonly]) .duration-suffix,
    .smart-field:has(input:disabled) .duration-suffix { display: inline-block; }

    /* Đồng bộ chiều cao 3 ô ngày bắt đầu / thời hạn / ngày kết thúc */
    .task-edit-date-row { align-items: stretch !important; }
    .task-edit-date-row > [class*="col-"] { display: flex; align-items: stretch; }
    .task-edit-date-row > [class*="col-"] > .smart-field,
    .task-edit-date-row > [class*="col-"] > .end-date-locked { width: 100%; height: 100%; }
    .task-edit-date-row .smart-field { min-height: 78px; }

    .end-date-locked { min-height: 78px; height: 100%; display: flex; flex-direction: column; justify-content: center; padding: 10px 15px; background: #f0fdf4; border: 1px solid #bbf7d0; border-left: 4px solid #22c55e; border-radius: 9px; box-shadow: 0 1px 2px rgba(22, 101, 52, 0.04); }
    .end-date-locked > label { display: flex; align-items: center; margin-bottom: 4px; color: #15803d; font-size: 10.5px; font-weight: 800; letter-spacing: .45px; text-transform: uppercase; }
    .end-date-locked > label i { color: #16a34a !important; }
    .end-date-value { color: #15803d; font-size: 16px; font-weight: 800; line-height: 1.35; }
    .task-context-card { padding: 14px 16px; background: #ffffff; border: 1px solid #e5e7eb; border-left: 4px solid #6d28d9; border-radius: 10px; box-shadow: 0 2px 6px rgba(17, 24, 39, 0.05); }
    .task-context-breadcrumb { display: flex; align-items: center; gap: 8px; margin-top: 11px; padding-top: 10px; border-top: 1px dashed #d1d5db; color: #4b5563; font-size: 13px; font-weight: 500; }
    .task-context-breadcrumb i { color: #7c3aed; font-size: 13px; }
    .badge-code { min-width: 38px; display: inline-flex; justify-content: center; align-items: center; padding: 5px 10px; background: #ede9fe; border: 1px solid #c4b5fd; border-radius: 7px; color: #5b21b6; font-size: 15px; font-weight: 800; line-height: 1.2; }

    /* ===================================================================
       HEADER DROPDOWN - MÀU SẮC ĐỘNG VÀ CĂN CHỈNH
       =================================================================== */
    .header-dropdown-box { min-height: 38px; display: flex; align-items: center; padding: 0 10px 0 12px; border: 1px solid #d1d5db; border-radius: 8px; transition: border-color .18s ease, box-shadow .18s ease; }
    .header-dropdown-box:hover { border-color: #a78bfa; }
    .header-dropdown-label {
        margin-right: 6px;
        color: #6b7280;
        font-size: 10.5px;
        font-weight: 800;
        letter-spacing: .35px;
        text-transform: uppercase;
        white-space: nowrap !important;
        flex-shrink: 0 !important;
    }

    .header-select {
        width: auto !important;
        min-width: max-content !important;
        height: 30px !important;
        margin: 0 !important;
        padding: 0 19px 0 0 !important;
        border: none !important;
        background-color: transparent !important;
        box-shadow: none !important;
        color: #1f2937 !important;
        font-size: 13px !important;
        font-weight: 750 !important;
        outline: none !important;
        cursor: pointer;
    }
    .header-select:focus { border: none !important; outline: none !important; box-shadow: none !important; }
    .header-select:disabled {
        padding-right: 0 !important;
        background-color: transparent !important;
        background-image: none !important;
        appearance: none !important;
        -webkit-appearance: none !important;
        -moz-appearance: none !important;
        opacity: 1 !important;
        cursor: default !important;
        -webkit-text-fill-color: inherit !important;
    }
    .header-dropdown-box:has(select:disabled) { cursor: default; }

    /* LÀM ĐẸP DANH SÁCH DROPDOWN BÊN TRONG */
    .header-select option {
        padding: 8px 12px !important;
        background-color: #ffffff;
        color: #1e293b;
        font-weight: 600;
        font-size: 14px;
        text-indent: 5px;
    }

    /* MÀU ĐỘNG ƯU TIÊN */
    .priority-default { background: #f8fafc; border-color: #cbd5e1; }
    .priority-default .label-priority { color: #64748b; }
    .priority-default .header-select { color: #334155 !important; }
    .priority-default:focus-within { border-color: #94a3b8; box-shadow: 0 0 0 3px rgba(100, 116, 139, .08); }
    .priority-low { background: #f0f9ff; border-color: #bae6fd; }
    .priority-low .label-priority { color: #0284c7; }
    .priority-low .header-select { color: #0369a1 !important; }
    .priority-low:focus-within { border-color: #7dd3fc; box-shadow: 0 0 0 3px rgba(2, 132, 199, .08); }
    .priority-med { background: #fffbeb; border-color: #fde68a; }
    .priority-med .label-priority { color: #d97706; }
    .priority-med .header-select { color: #b45309 !important; }
    .priority-med:focus-within { border-color: #fcd34d; box-shadow: 0 0 0 3px rgba(217, 119, 6, .08); }
    .priority-high { background: #fef2f2; border-color: #fecaca; }
    .priority-high .label-priority { color: #dc2626; }
    .priority-high .header-select { color: #b91c1c !important; }
    .priority-high:focus-within { border-color: #fca5a5; box-shadow: 0 0 0 3px rgba(220, 38, 38, .08); }

    /* MÀU ĐỘNG TRẠNG THÁI */
    .status-box-0 { background: #f8fafc; border-color: #cbd5e1; }
    .status-box-0 .label-status { color: #64748b; }
    .status-box-0 .header-select { color: #334155 !important; }
    .status-box-0:focus-within { border-color: #94a3b8; box-shadow: 0 0 0 3px rgba(100, 116, 139, .08); }
    .status-box-1 { background: #eff6ff; border-color: #bfdbfe; }
    .status-box-1 .label-status { color: #2563eb; }
    .status-box-1 .header-select { color: #1d4ed8 !important; }
    .status-box-1:focus-within { border-color: #60a5fa; box-shadow: 0 0 0 3px rgba(37, 99, 235, .08); }
    .status-box-2 { background: #f0fdf4; border-color: #bbf7d0; }
    .status-box-2 .label-status { color: #16a34a; }
    .status-box-2 .header-select { color: #15803d !important; }
    .status-box-2:focus-within { border-color: #4ade80; box-shadow: 0 0 0 3px rgba(22, 163, 74, .08); }
    .status-box-3 { background: #fef2f2; border-color: #fecaca; }
    .status-box-3 .label-status { color: #dc2626; }
    .status-box-3 .header-select { color: #b91c1c !important; }
    .status-box-3:focus-within { border-color: #f87171; box-shadow: 0 0 0 3px rgba(220, 38, 38, .08); }
    #divRollUpNotice .badge { display: inline-flex; align-items: center; padding: 6px 9px !important; background: #fff7ed !important; border: 1px solid #fed7aa; border-radius: 7px; color: #9a3412 !important; font-size: 11px !important; font-weight: 700; }
    #divRollUpNotice .badge i { color: #ea580c !important; }
    @media (min-width: 768px) { #<%= mdlEditTask.ClientID %> .modal-dialog { width: 92% !important; max-width: 760px !important; } }
    @media (max-width: 767.98px) { .task-context-card { padding: 12px; } #rowUuTienTrangThai { width: 100%; } .header-dropdown-box { width: 100%; justify-content: space-between; } .end-date-locked { min-height: 72px; } .task-edit-date-row > [class*="col-"] { display: block; } .task-edit-date-row .smart-field, .task-edit-date-row .end-date-locked { min-height: 72px; height: auto; } }
</style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2">
                <SweetSoft:Navigation runat="server" ID="Navigation1" />
                <SweetSoft:CtrlProjectTabs runat="server" ID="CtrlProjectTabs1" />
                <SweetSoft:CtrlTask runat="server" ID="CtrlTask1" />
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="mdlEditTask" Type="Primary" Title="Cập nhật công việc">
        <ContentTemplate>
            <asp:UpdatePanel ID="upModal" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="p-3">
                        <asp:HiddenField ID="hfEditTaskId" runat="server" />
                        <div class="task-context-card mb-4" id="divContextCard" runat="server">
                            <div class="d-flex justify-content-between align-items-center flex-wrap gap-3 mb-2">
                                <div class="d-flex align-items-center gap-2">
                                    <span class="text-muted fw-bold" style="font-size:11px;">MÃ CV:</span>
                                    <asp:Label ID="lblEditMaCv" runat="server" CssClass="badge-code"></asp:Label>
                                    <asp:TextBox ID="txtEditMaCv" runat="server" style="display:none;"></asp:TextBox>
                                </div>
                                <div class="d-flex gap-2 flex-wrap" id="rowUuTienTrangThai" runat="server">
                                    <div class="header-dropdown-box priority-default" id="boxUuTien" runat="server">
                                        <span class="header-dropdown-label label-priority">ƯU TIÊN:</span>
                                        <asp:DropDownList ID="ddlEditDoUuTien" runat="server" CssClass="form-select header-select" onchange="updatePriorityColor(this)"></asp:DropDownList>
                                    </div>
                                    <div class="header-dropdown-box status-box-0" id="boxTrangThai" runat="server">
                                        <span class="header-dropdown-label label-status">TRẠNG THÁI:</span>
                                        <asp:DropDownList ID="ddlEditTrangThai" runat="server" CssClass="form-select header-select" onchange="updateStatusColor(this)"></asp:DropDownList>
                                    </div>
                                </div>
                            </div>
                            <div class="task-context-breadcrumb" id="rowBreadcrumb" runat="server">
                                <i class="fas fa-layer-group"></i>
                                <asp:Label ID="lblEditGiaiDoan" runat="server" CssClass="text-dark fw-bold"></asp:Label>
                                <asp:TextBox ID="txtEditGiaiDoan" runat="server" style="display:none;"></asp:TextBox>
                            </div>
                        </div>
                        <div class="row g-3 mb-3">
                            <div class="col-12">
                                <div class="smart-field">
                                    <label><i class="fas fa-tasks"></i><%= GetResourceText(BackEndResourceKeys.TASK_NAME) %> <span class="text-danger ms-1">*</span></label>
                                    <asp:TextBox ID="txtEditTenCv" runat="server" CssClass="form-control form-control-lg" placeholder="Nhập tên đầu việc..."></asp:TextBox>
                                </div>
                            </div>
                        </div>
                        <div class="row g-3 mb-3" id="rowChaVaPhuThuoc" runat="server">
                            <div class="col-12" id="colCongViecCha" runat="server">
                                <div class="smart-field">
                                    <label><i class="fas fa-sitemap"></i><%= GetResourceText(BackEndResourceKeys.PARENT_TASK) %></label>
                                    <asp:DropDownList ID="ddlEditCongViecCha" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEditCongViecChaSelected"></asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-12" id="colPhuThuoc" runat="server">
                                <div class="smart-field">
                                    <label><i class="fas fa-link"></i><%= GetResourceText(BackEndResourceKeys.DEPENDENT) %></label>
                                    <asp:DropDownList ID="ddlEditPhuThuoc" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEditPhuThuocSelected"></asp:DropDownList>
                                </div>
                            </div>
                        </div>
                        <div id="divRollUpNotice" runat="server" class="mb-2" visible="false">
                            <span class="badge">
                                <i class="fas fa-lock me-1"></i>Ngày tháng và Trạng thái bị khóa do tổng hợp từ task con
                            </span>
                        </div>
                        <div class="row g-3 mb-3 task-edit-date-row">
                            <div class="col-md-4">
                                <div class="smart-field">
                                    <label><i class="far fa-calendar-alt"></i><%= GetResourceText(BackEndResourceKeys.START_DATE) %> <span class="text-danger ms-1">*</span></label>
                                    <asp:TextBox ID="txtEditNgayBatDau" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="smart-field">
                                    <label><i class="fas fa-hourglass-half"></i><%= GetResourceText(BackEndResourceKeys.DURATION) %> <span class="text-danger ms-1">*</span></label>
                                    <div class="input-group">
                                        <asp:TextBox ID="txtEditThoiHan" runat="server" CssClass="form-control" TextMode="Number" min="1"></asp:TextBox>
                                        <span class="input-group-text">Ngày</span>
                                        <span class="duration-suffix">ngày</span>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-5">
                                <div class="end-date-locked">
                                    <label><i class="far fa-calendar-check me-1"></i><%= GetResourceText(BackEndResourceKeys.EXPECTED_COMPLETION_DATE) %></label>
                                    <div class="end-date-value">
                                        <asp:Label ID="lblNgayKetThuc" runat="server">--/--/----</asp:Label>
                                        <asp:TextBox ID="txtEditNgayKetThuc" runat="server" style="display:none;"></asp:TextBox>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="row g-3">
                            <div class="col-12">
                                <div class="smart-field">
                                    <label><i class="fas fa-align-left"></i><%= GetResourceText(BackEndResourceKeys.SUMMARY) %></label>
                                    <asp:TextBox ID="txtEditMoTa" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control"></asp:TextBox>
                                </div>
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </ContentTemplate>
        <FooterTemplate>
            <asp:UpdatePanel ID="upnlFooterEdit" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:LinkButton ID="btnSaveTask" runat="server" CssClass="btn btn-primary waves-effect waves-light" CausesValidation="false" OnClick="btnSaveTask_Click">
                        <i class="fas fa-save me-1"></i> Cập nhật
                    </asp:LinkButton>
                </ContentTemplate>
            </asp:UpdatePanel>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
<script type="text/javascript">
    function updateStatusColor(selectElement) {
        var box = $(selectElement).closest('.header-dropdown-box');
        var val = $(selectElement).val();
        box.removeClass('status-box-0 status-box-1 status-box-2 status-box-3');
        box.addClass('status-box-' + val);
    }

    function updatePriorityColor(selectElement) {
        var box = $(selectElement).closest('.header-dropdown-box');
        var text = $(selectElement).find("option:selected").text().toLowerCase();
        box.removeClass('priority-default priority-low priority-med priority-high');
        if (text.indexOf('cao') > -1) {
            box.addClass('priority-high');
        } else if (text.indexOf('trung bình') > -1) {
            box.addClass('priority-med');
        } else if (text.indexOf('thấp') > -1) {
            box.addClass('priority-low');
        } else {
            box.addClass('priority-default');
        }
    }

    $(document).on('change keyup', '#<%= txtEditNgayBatDau.ClientID %>, #<%= txtEditThoiHan.ClientID %>', function () {
        var startDateStr = $('#<%= txtEditNgayBatDau.ClientID %>').val();
        var durationStr = $('#<%= txtEditThoiHan.ClientID %>').val();
        if (startDateStr && durationStr) {
            var days = parseInt(durationStr);
            if (!isNaN(days) && days > 0) {
                var date = new Date(startDateStr);
                date.setDate(date.getDate() + (days - 1));
                var d = ("0" + date.getDate()).slice(-2);
                var m = ("0" + (date.getMonth() + 1)).slice(-2);
                var y = date.getFullYear();
                $('#<%= txtEditNgayKetThuc.ClientID %>').val(y + '-' + m + '-' + d);
                $('#<%= lblNgayKetThuc.ClientID %>').text(d + '/' + m + '/' + y);
            }
        }
    });
</script>
</asp:Content>
