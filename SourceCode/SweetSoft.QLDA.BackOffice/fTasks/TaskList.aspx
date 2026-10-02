<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" MaintainScrollPositionOnPostBack="true" CodeBehind="TaskList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.TaskList" %>
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
       TASK HEADER / PRIORITY / STATUS - CUSTOM DROPDOWN
       Không dùng native <select> cho 2 control đầu vì popup native của
       browser/Windows không thể style đồng bộ với giao diện.
       =================================================================== */
    .task-context-card {
        padding: 16px;
        background: #ffffff;
        border: 1px solid #e5e7eb;
        border-left: 4px solid #6d28d9;
        border-radius: 12px;
        box-shadow: 0 2px 8px rgba(17, 24, 39, 0.05);
    }

    .task-context-top {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 18px;
        min-width: 0;
    }

    .task-context-identity,
    .task-context-phase {
        min-width: 0;
        display: flex;
        align-items: center;
    }

    .task-context-identity { gap: 10px; }
    .task-context-phase { gap: 9px; }

    .task-context-kicker {
        display: block;
        margin-bottom: 3px;
        color: #94a3b8;
        font-size: 10px;
        font-weight: 800;
        letter-spacing: .55px;
        line-height: 1.2;
        text-transform: uppercase;
    }

    .task-context-phase i {
        width: 30px;
        height: 30px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        flex: 0 0 30px;
        border-radius: 8px;
        background: #f5f3ff;
        color: #7c3aed;
        font-size: 12px;
    }

    .task-context-phase-value {
        min-width: 0;
        color: #374151;
        font-size: 13.5px;
        font-weight: 700;
        line-height: 1.35;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .task-context-divider {
        height: 1px;
        margin: 14px 0;
        background: #eef2f7;
    }

    .task-meta-controls {
        display: grid;
        grid-template-columns: minmax(0, 1fr) minmax(0, 1.2fr);
        gap: 12px;
    }

    .task-meta-shell {
        position: relative;
        min-width: 0;
    }

    .task-meta-label {
        display: flex;
        align-items: center;
        gap: 6px;
        margin: 0 0 6px 1px;
        color: #64748b;
        font-size: 10px;
        font-weight: 800;
        letter-spacing: .55px;
        line-height: 1.2;
        text-transform: uppercase;
    }

    .task-meta-label i {
        width: 14px;
        color: #94a3b8 !important;
        text-align: center;
        font-size: 11px;
    }

    .task-dropdown {
        position: relative;
        min-width: 0;
    }

    .task-native-select {
        display: none !important;
    }

    .task-dropdown-toggle {
        width: 100%;
        min-height: 44px;
        display: flex;
        align-items: center;
        gap: 9px;
        padding: 9px 12px;
        border: 1px solid #dbe1ea;
        border-radius: 9px;
        background: #ffffff;
        color: #1f2937;
        text-align: left;
        font-size: 13.5px;
        font-weight: 750;
        box-shadow: 0 1px 2px rgba(17, 24, 39, 0.035);
        transition: border-color .18s ease, box-shadow .18s ease, background-color .18s ease;
        cursor: pointer;
    }

    .task-dropdown-toggle:hover {
        border-color: #c7d2fe;
        background: #fcfcff;
    }

    .task-dropdown-toggle:focus-visible {
        outline: none;
        border-color: #8b5cf6;
        box-shadow: 0 0 0 3px rgba(124, 58, 237, .10);
    }

    .task-dropdown-toggle:disabled {
        background: #f8fafc;
        color: #94a3b8;
        cursor: default;
        box-shadow: none;
    }

    .task-dropdown.open .task-dropdown-toggle {
        border-color: #8b5cf6;
        box-shadow: 0 0 0 3px rgba(124, 58, 237, .09);
    }

    .task-meta-value {
        min-width: 0;
        flex: 1 1 auto;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .task-dropdown-chevron {
        flex: 0 0 auto;
        margin-left: auto;
        color: #94a3b8;
        font-size: 10px;
        transition: transform .18s ease;
    }

    .task-dropdown.open .task-dropdown-chevron {
        transform: rotate(180deg);
    }

    .task-status-dot {
        width: 9px;
        height: 9px;
        flex: 0 0 9px;
        border-radius: 50%;
        background: #94a3b8;
        box-shadow: 0 0 0 3px rgba(148, 163, 184, .12);
    }

    .task-dropdown-menu {
        position: absolute;
        z-index: 2000;
        top: calc(100% + 6px);
        left: 0;
        min-width: 100%;
        max-height: 230px;
        overflow-y: auto;
        padding: 5px;
        border: 1px solid #e2e8f0;
        border-radius: 10px;
        background: #ffffff;
        box-shadow: 0 14px 30px rgba(15, 23, 42, .14), 0 2px 8px rgba(15, 23, 42, .06);
        opacity: 0;
        visibility: hidden;
        transform: translateY(-3px);
        transition: opacity .14s ease, transform .14s ease, visibility .14s ease;
    }

    .task-dropdown-menu.drop-up {
        top: auto;
        bottom: calc(100% + 6px);
        transform: translateY(3px);
    }

    .task-dropdown.open .task-dropdown-menu {
        opacity: 1;
        visibility: visible;
        transform: translateY(0);
    }

    .task-dropdown-item {
        min-height: 36px;
        display: flex;
        align-items: center;
        gap: 9px;
        padding: 8px 9px;
        border-radius: 7px;
        color: #334155;
        font-size: 13.5px;
        font-weight: 600;
        line-height: 1.25;
        cursor: pointer;
        user-select: none;
        transition: background-color .12s ease, color .12s ease;
    }

    .task-dropdown-item:hover {
        background: #f8fafc;
        color: #111827;
    }

    .task-dropdown-item.active {
        background: #f8fafc;
        color: #334155;
    }

    .task-dropdown-item .task-check {
        margin-left: auto;
        color: #7c3aed;
        font-size: 11px;
    }

    /* ===================================================================
       MÀU CHO VALUE ĐANG CHỌN
       Chỉ áp dụng cho nút đang hiển thị, tránh ảnh hưởng các item trong menu.
       =================================================================== */
    .priority-low .task-dropdown-toggle .task-status-dot,
    .dot-low {
        background: #3b82f6;
        box-shadow: 0 0 0 3px rgba(59, 130, 246, .12);
    }
    .priority-low .task-dropdown-toggle .task-meta-value { color: #0369a1; }

    .priority-med .task-dropdown-toggle .task-status-dot,
    .dot-medium {
        background: #f59e0b;
        box-shadow: 0 0 0 3px rgba(245, 158, 11, .12);
    }
    .priority-med .task-dropdown-toggle .task-meta-value { color: #b45309; }

    .priority-high .task-dropdown-toggle .task-status-dot,
    .dot-high {
        background: #ef4444;
        box-shadow: 0 0 0 3px rgba(239, 68, 68, .12);
    }
    .priority-high .task-dropdown-toggle .task-meta-value { color: #b91c1c; }

    .priority-default .task-dropdown-toggle .task-status-dot,
    .dot-default {
        background: #94a3b8;
        box-shadow: 0 0 0 3px rgba(148, 163, 184, .12);
    }
    .priority-default .task-dropdown-toggle .task-meta-value { color: #334155; }

    /* Status colors cho value đang chọn */
    .status-box-0 .task-dropdown-toggle .task-status-dot,
    .status-default .task-dropdown-toggle .task-status-dot,
    .dot-status-0 {
        background: #94a3b8;
        box-shadow: 0 0 0 3px rgba(148, 163, 184, .12);
    }
    .status-box-0 .task-dropdown-toggle .task-meta-value,
    .status-default .task-dropdown-toggle .task-meta-value { color: #475569; }

    .status-box-1 .task-dropdown-toggle .task-status-dot,
    .status-running .task-dropdown-toggle .task-status-dot,
    .dot-status-1 {
        background: #3b82f6;
        box-shadow: 0 0 0 3px rgba(59, 130, 246, .12);
    }
    .status-box-1 .task-dropdown-toggle .task-meta-value,
    .status-running .task-dropdown-toggle .task-meta-value { color: #1d4ed8; }

    .status-box-2 .task-dropdown-toggle .task-status-dot,
    .status-done .task-dropdown-toggle .task-status-dot,
    .dot-status-2 {
        background: #22c55e;
        box-shadow: 0 0 0 3px rgba(34, 197, 94, .12);
    }
    .status-box-2 .task-dropdown-toggle .task-meta-value,
    .status-done .task-dropdown-toggle .task-meta-value { color: #15803d; }

    .status-box-3 .task-dropdown-toggle .task-status-dot,
    .status-error .task-dropdown-toggle .task-status-dot,
    .dot-status-3 {
        background: #ef4444;
        box-shadow: 0 0 0 3px rgba(239, 68, 68, .12);
    }
    .status-box-3 .task-dropdown-toggle .task-meta-value,
    .status-error .task-dropdown-toggle .task-meta-value { color: #b91c1c; }

    /* ===================================================================
       MÀU TỪNG OPTION TRONG MENU
       Mỗi mức có màu riêng, item được chọn chỉ dùng nền rất nhạt tương ứng.
       =================================================================== */
    .task-dropdown-item.priority-option-low,
    .task-dropdown-item.status-option-not-started {
        --option-color: #3b82f6;
        --option-text: #0369a1;
        --option-bg: #eff6ff;
    }

    .task-dropdown-item.priority-option-medium {
        --option-color: #f59e0b;
        --option-text: #b45309;
        --option-bg: #fffbeb;
    }

    .task-dropdown-item.priority-option-high {
        --option-color: #ef4444;
        --option-text: #b91c1c;
        --option-bg: #fef2f2;
    }

    .task-dropdown-item.status-option-running {
        --option-color: #3b82f6;
        --option-text: #1d4ed8;
        --option-bg: #eff6ff;
    }

    .task-dropdown-item.status-option-done {
        --option-color: #22c55e;
        --option-text: #15803d;
        --option-bg: #f0fdf4;
    }

    .task-dropdown-item.status-option-other {
        --option-color: #ef4444;
        --option-text: #b91c1c;
        --option-bg: #fef2f2;
    }

    .task-dropdown-item.priority-option-low .task-status-dot,
    .task-dropdown-item.status-option-running .task-status-dot {
        background: #3b82f6 !important;
        box-shadow: 0 0 0 3px rgba(59, 130, 246, .12) !important;
    }

    .task-dropdown-item.priority-option-medium .task-status-dot {
        background: #f59e0b !important;
        box-shadow: 0 0 0 3px rgba(245, 158, 11, .12) !important;
    }

    .task-dropdown-item.priority-option-high .task-status-dot {
        background: #ef4444 !important;
        box-shadow: 0 0 0 3px rgba(239, 68, 68, .12) !important;
    }

    .task-dropdown-item.status-option-not-started .task-status-dot {
        background: #94a3b8 !important;
        box-shadow: 0 0 0 3px rgba(148, 163, 184, .12) !important;
    }

    .task-dropdown-item.status-option-done .task-status-dot {
        background: #22c55e !important;
        box-shadow: 0 0 0 3px rgba(34, 197, 94, .12) !important;
    }

    .task-dropdown-item.status-option-other .task-status-dot {
        background: #ef4444 !important;
        box-shadow: 0 0 0 3px rgba(239, 68, 68, .12) !important;
    }

    .task-dropdown-item[class*="option-"] > span:not(.task-status-dot) {
        color: #334155;
    }

    .task-dropdown-item[class*="option-"].active {
        background: var(--option-bg, #f8fafc) !important;
        color: var(--option-text, #334155) !important;
    }

    .task-dropdown-item[class*="option-"].active > span:not(.task-status-dot) {
        color: var(--option-text, #334155) !important;
        font-weight: 750;
    }

    .task-dropdown-item[class*="option-"].active .task-check {
        color: var(--option-color, #7c3aed) !important;
    }

    .task-dropdown-item[class*="option-"]:hover {
        background: var(--option-bg, #f8fafc);
    }

    .task-context-breadcrumb {
        display: none;
    }

    #divRollUpNotice .badge { display: inline-flex; align-items: center; padding: 6px 9px !important; background: #fff7ed !important; border: 1px solid #fed7aa; border-radius: 7px; color: #9a3412 !important; font-size: 11px !important; font-weight: 700; }
    #divRollUpNotice .badge i { color: #ea580c !important; }

    @media (min-width: 768px) {
        #<%= mdlEditTask.ClientID %> .modal-dialog {
            width: 92% !important;
            max-width: 800px !important;
        }
    }

    @media (max-width: 767.98px) {
        .task-context-card { padding: 13px; }
        .task-context-top { align-items: flex-start; flex-direction: column; gap: 12px; }
        .task-context-phase { width: 100%; }
        .task-meta-controls { grid-template-columns: 1fr; }
        #rowUuTienTrangThai { width: 100%; }
        .end-date-locked { min-height: 72px; }
        .task-edit-date-row > [class*="col-"] { display: block; }
        .task-edit-date-row .smart-field, .task-edit-date-row .end-date-locked { min-height: 72px; height: auto; }
    }

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
                            <div class="task-context-top">
                                <div class="task-context-identity">
                                    <div>
                                        <span class="task-context-kicker">MÃ CÔNG VIỆC</span>
                                        <asp:Label ID="lblEditMaCv" runat="server" CssClass="badge-code"></asp:Label>
                                        <asp:TextBox ID="txtEditMaCv" runat="server" style="display:none;"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="task-context-phase" id="rowBreadcrumb" runat="server">
                                    <i class="fas fa-layer-group"></i>
                                    <div>
                                        <span class="task-context-kicker">GIAI ĐOẠN</span>
                                        <asp:Label ID="lblEditGiaiDoan" runat="server" CssClass="task-context-phase-value"></asp:Label>
                                        <asp:TextBox ID="txtEditGiaiDoan" runat="server" style="display:none;"></asp:TextBox>
                                    </div>
                                </div>
                            </div>

                            <div class="task-context-divider"></div>

                            <div class="task-meta-controls" id="rowUuTienTrangThai" runat="server">
                                <div class="task-meta-shell header-dropdown-box priority-default" id="boxUuTien" runat="server">
                                    <div class="task-meta-label"><i class="fas fa-flag"></i> ƯU TIÊN</div>
                                    <div class="task-dropdown priority-default" data-select-id="<%= ddlEditDoUuTien.ClientID %>">
                                        <asp:DropDownList ID="ddlEditDoUuTien" runat="server" CssClass="task-native-select" onchange="updatePriorityColor(this)"></asp:DropDownList>
                                        <button type="button" class="task-dropdown-toggle" aria-haspopup="listbox" aria-expanded="false">
                                            <span class="task-status-dot"></span>
                                            <span class="task-meta-value">Đang tải...</span>
                                            <i class="fas fa-chevron-down task-dropdown-chevron"></i>
                                        </button>
                                        <div class="task-dropdown-menu" role="listbox"></div>
                                    </div>
                                </div>

                                <div class="task-meta-shell header-dropdown-box status-box-0" id="boxTrangThai" runat="server">
                                    <div class="task-meta-label"><i class="fas fa-check-circle"></i> TRẠNG THÁI</div>
                                    <div class="task-dropdown status-default" data-select-id="<%= ddlEditTrangThai.ClientID %>">
                                        <asp:DropDownList ID="ddlEditTrangThai" runat="server" CssClass="task-native-select" onchange="updateStatusColor(this)"></asp:DropDownList>
                                        <button type="button" class="task-dropdown-toggle" aria-haspopup="listbox" aria-expanded="false">
                                            <span class="task-status-dot"></span>
                                            <span class="task-meta-value">Đang tải...</span>
                                            <i class="fas fa-chevron-down task-dropdown-chevron"></i>
                                        </button>
                                        <div class="task-dropdown-menu" role="listbox"></div>
                                    </div>
                                </div>
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
<!-- POPUP CẤU HÌNH HỆ SỐ ĐÓNG GÓP -->
    <SweetSoft:ExtraModal runat="server" ID="mdlHeSoDongGop" Type="Primary" Title="Cấu hình hệ số đóng góp">
        <ContentTemplate>
            <asp:UpdatePanel ID="upHeSoDongGop" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="p-3">
                        <div class="alert alert-info" style="font-size:13px; background-color: #eff6ff; border: 1px solid #bfdbfe; color: #1e40af;">
                            <i class="fas fa-info-circle me-1"></i> Các hệ số dưới đây là phiên bản độc lập của riêng dự án này. Việc thay đổi sẽ tự động <strong>áp dụng hồi tố</strong> để tính lại điểm cho toàn bộ công việc.
                        </div>

                        <!-- Cấu trúc HTML y chang Settings.aspx, đã xóa thẻ <i> thừa -->
                        <div class="row">
                            <asp:Repeater ID="rptHeSoDongGop" runat="server">
                                <ItemTemplate>
                                    <div class="col-lg-4">
                                        <div class="mt-3">
                                            <label class="form-label">Hệ số <%# Eval("TenDoUuTien").ToString().ToLower() %></label>
                                            
                                            <asp:HiddenField ID="hdfIdDoUuTien" runat="server" Value='<%# Eval("IdDoUuTien") %>' />
                                            <asp:HiddenField ID="hdfIdHeSoDongGop" runat="server" Value='<%# Eval("IdHeSoDongGop") %>' />
                                            
                                            <asp:TextBox ID="txtHeSo" runat="server" CssClass="form-control" TextMode="Number" step="0.01" min="0" 
                                                Text='<%# Convert.ToDecimal(Eval("HeSoDongGop")).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) %>'>
                                            </asp:TextBox>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </ContentTemplate>
        <FooterTemplate>
            <asp:UpdatePanel ID="upnlFooterHeSo" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:LinkButton ID="btnSaveHeSo" runat="server" CssClass="btn btn-primary waves-effect waves-light" CausesValidation="false" OnClick="btnSaveHeSo_Click">
                        <i class="fas fa-save me-1"></i> Lưu hệ số
                    </asp:LinkButton>
                </ContentTemplate>
            </asp:UpdatePanel>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
    <SweetSoft:ExtraModal runat="server" ID="mdlTaskReminder" Type="Primary">
        <ContentTemplate>
            <asp:UpdatePanel runat="server" ID="upTaskReminder" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="p-3">
                        <div class="mb-3 p-2 rounded bg-light border">
                            <div class="small text-muted mb-1">CÔNG VIỆC</div>
                            <div class="fw-bold fs-6 text-dark">
                                <asp:Literal runat="server" ID="ltrReminderTaskName" />
                            </div>
                        </div>
                        <asp:HiddenField runat="server" ID="hdfReminderTaskId" />
                        <asp:HiddenField runat="server" ID="hdfSelectedReminderIds" />
                        <asp:Panel runat="server" ID="pnlNoReminder" CssClass="text-center text-muted p-2" Visible="false">
                            Không còn nhắc việc chưa xử lý.
                        </asp:Panel>

                       <asp:Repeater runat="server" ID="rptTaskReminders">
                            <ItemTemplate>
                                <div class="d-flex align-items-center border-bottom py-2 gap-2">
                                    <!-- [SỬA LỖI]: Dùng input thuần thay vì asp:CheckBox -->
                                    <input type="checkbox" class="form-check-input mt-0" 
                                           value='<%# Eval("IdNhacViec") %>' 
                                           onchange="toggleReminderSelection(this);" 
                                           style="cursor: pointer; width: 18px; height: 18px; flex-shrink: 0;" />
                                    
                                    <asp:HiddenField runat="server" ID="hdfIdNhacViec" Value='<%# Eval("IdNhacViec") %>' />
                                    <div class="flex-grow-1">
                                        <div class="fw-semibold"><%# Eval("NoiDung") %></div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </ContentTemplate>

        <FooterTemplate>
            <asp:UpdatePanel runat="server" ID="upTaskReminderFooter" UpdateMode="Conditional">
                <ContentTemplate>
                    <SweetSoft:ExtraButton runat="server" ID="btnProcessReminders" ButtonStyle="Primary" ButtonIcon="Check" Enabled="true" OnClick="btnProcessReminders_Click">
                        Đã xử lý
                    </SweetSoft:ExtraButton>
                </ContentTemplate>
            </asp:UpdatePanel>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
<script type="text/javascript">
    function closeAllTaskDropdowns(exceptDropdown) {
        $('.task-dropdown.open').each(function () {
            if (!exceptDropdown || this !== exceptDropdown) {
                $(this).removeClass('open');
                $(this).find('.task-dropdown-toggle').attr('aria-expanded', 'false');
            }
        });
    }

    function getPriorityClass(text) {
        text = (text || '').toLowerCase();
        if (text.indexOf('cao') > -1) return 'priority-high';
        if (text.indexOf('trung bình') > -1) return 'priority-med';
        if (text.indexOf('thấp') > -1) return 'priority-low';
        return 'priority-default';
    }

    function getStatusClass(text, value) {
        text = (text || '').toLowerCase();
        if (text.indexOf('hoàn thành') > -1) return 'status-done';
        if (text.indexOf('đang thực hiện') > -1) return 'status-running';
        if (value === '3') return 'status-error';
        if (value === '2') return 'status-done';
        if (value === '1') return 'status-running';
        return 'status-default';
    }

    function getPriorityDotClass(text) {
        return getPriorityClass(text)
            .replace('priority-med', 'dot-medium')
            .replace('priority-low', 'dot-low')
            .replace('priority-high', 'dot-high')
            .replace('priority-default', 'dot-default');
    }

    function getStatusDotClass(text, value) {
        var cls = getStatusClass(text, value);
        if (cls === 'status-running') return 'dot-status-1';
        if (cls === 'status-done') return 'dot-status-2';
        if (cls === 'status-error') return 'dot-status-3';
        return 'dot-status-0';
    }

    function getPriorityOptionClass(text) {
        text = (text || '').toLowerCase();
        if (text.indexOf('cao') > -1) return 'priority-option-high';
        if (text.indexOf('trung bình') > -1) return 'priority-option-medium';
        if (text.indexOf('thấp') > -1) return 'priority-option-low';
        return 'priority-option-low';
    }

    function getStatusOptionClass(text, value) {
        text = (text || '').toLowerCase();
        if (text.indexOf('chưa bắt đầu') > -1 || text.indexOf('chua bat dau') > -1) return 'status-option-not-started';
        if (text.indexOf('đang thực hiện') > -1 || text.indexOf('dang thuc hien') > -1) return 'status-option-running';
        if (text.indexOf('hoàn thành') > -1 || text.indexOf('hoan thanh') > -1) return 'status-option-done';
        if (value === '1') return 'status-option-running';
        if (value === '2') return 'status-option-done';
        return 'status-option-other';
    }

    function syncTaskDropdown(selectElement) {
        if (!selectElement) return;

        var $select = $(selectElement);
        var $dropdown = $('.task-dropdown[data-select-id="' + selectElement.id + '"]');
        if (!$dropdown.length) return;

        var value = $select.val();
        var $selectedOption = $select.find('option:selected');
        var text = $selectedOption.text() || 'Chưa chọn';
        var isPriority = $select.attr('id') === '<%= ddlEditDoUuTien.ClientID %>';
        var valueClass = isPriority ? getPriorityClass(text) : getStatusClass(text, value);
        var dotClass = isPriority ? getPriorityDotClass(text) : getStatusDotClass(text, value);
        var menuHtml = '';

        $select.find('option').each(function () {
            var optionText = $(this).text().trim();
            var optionValue = $(this).val();
            var optionSelected = this.selected;
            var optionDotClass = isPriority
                ? getPriorityDotClass(optionText)
                : getStatusDotClass(optionText, optionValue);
            var optionTypeClass = isPriority
                ? getPriorityOptionClass(optionText)
                : getStatusOptionClass(optionText, optionValue);

            menuHtml += '<div class="task-dropdown-item ' + optionTypeClass + (optionSelected ? ' active' : '') + '" role="option" aria-selected="' + optionSelected + '" data-value="' + $('<div/>').text(optionValue).html() + '">';
            menuHtml += '<span class="task-status-dot ' + optionDotClass + '"></span>';
            menuHtml += '<span>' + $('<div/>').text(optionText).html() + '</span>';
            if (optionSelected) {
                menuHtml += '<i class="fas fa-check task-check"></i>';
            }
            menuHtml += '</div>';
        });

        var $shell = $dropdown.closest('.task-meta-shell');
        $shell.removeClass('priority-default priority-low priority-med priority-high status-box-0 status-box-1 status-box-2 status-box-3 status-default status-running status-done status-error');
        $shell.addClass(valueClass);

        $dropdown.removeClass('priority-default priority-low priority-med priority-high status-box-0 status-box-1 status-box-2 status-box-3 status-default status-running status-done status-error');
        $dropdown.addClass(valueClass);

        $dropdown.find('.task-meta-value').text(text);
        $dropdown.find('.task-status-dot').removeClass('dot-low dot-medium dot-high dot-default dot-status-0 dot-status-1 dot-status-2 dot-status-3').addClass(dotClass);
        $dropdown.find('.task-dropdown-menu').html(menuHtml);

        var $toggle = $dropdown.find('.task-dropdown-toggle');
        $toggle.prop('disabled', $select.prop('disabled'));
        $toggle.attr('aria-disabled', $select.prop('disabled') ? 'true' : 'false');
    }

    function initTaskHeaderDropdown(selectElement) {
        if (!selectElement) return;
        syncTaskDropdown(selectElement);
    }

    function initAllTaskHeaderDropdowns() {
        var prioritySelect = document.getElementById('<%= ddlEditDoUuTien.ClientID %>');
        var statusSelect = document.getElementById('<%= ddlEditTrangThai.ClientID %>');
        initTaskHeaderDropdown(prioritySelect);
        initTaskHeaderDropdown(statusSelect);
    }

    function updateStatusColor(selectElement) {
        syncTaskDropdown(selectElement);
    }

    function updatePriorityColor(selectElement) {
        syncTaskDropdown(selectElement);
    }

    $(document).off('click.taskHeaderToggle').on('click.taskHeaderToggle', '.task-dropdown-toggle', function (e) {
        e.preventDefault();
        e.stopPropagation();

        var dropdown = $(this).closest('.task-dropdown')[0];
        if (!dropdown || $(this).prop('disabled')) return;

        var isOpen = $(dropdown).hasClass('open');
        closeAllTaskDropdowns(dropdown);

        if (!isOpen) {
            $(dropdown).addClass('open');
            $(this).attr('aria-expanded', 'true');

            var menu = $(dropdown).find('.task-dropdown-menu')[0];
            if (menu) {
                $(menu).removeClass('drop-up');
                var rect = dropdown.getBoundingClientRect();
                var estimatedMenuHeight = Math.min(menu.scrollHeight || 180, 230);
                var spaceBelow = window.innerHeight - rect.bottom;
                if (spaceBelow < estimatedMenuHeight + 18 && rect.top > estimatedMenuHeight + 18) {
                    $(menu).addClass('drop-up');
                }
            }
        }
    });

    $(document).off('click.taskHeaderItem').on('click.taskHeaderItem', '.task-dropdown-item', function (e) {
        e.preventDefault();
        e.stopPropagation();

        var $item = $(this);
        var $dropdown = $item.closest('.task-dropdown');
        var selectId = $dropdown.attr('data-select-id');
        var select = document.getElementById(selectId);
        var value = $item.attr('data-value');

        if (!select) return;

        select.value = value;
        $(select).trigger('change');

        $dropdown.removeClass('open');
        $dropdown.find('.task-dropdown-toggle').attr('aria-expanded', 'false');
    });

    $(document).off('click.taskHeaderOutside').on('click.taskHeaderOutside', function () {
        closeAllTaskDropdowns();
    });

    $(document).off('keydown.taskHeaderEscape').on('keydown.taskHeaderEscape', function (e) {
        if (e.key === 'Escape') {
            closeAllTaskDropdowns();
        }
    });

    $(document).off('change.taskHeaderSync').on('change.taskHeaderSync', '.task-native-select', function () {
        syncTaskDropdown(this);
    });

    $(window).off('resize.taskHeader').on('resize.taskHeader', function () {
        closeAllTaskDropdowns();
    });

    $(function () {
        initAllTaskHeaderDropdowns();
    });

    if (window.Sys && Sys.Application) {
        Sys.Application.add_load(function () {
            closeAllTaskDropdowns();
            initAllTaskHeaderDropdowns();
        });
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
    function toggleReminderSelection(chk) {
        var hidden = document.getElementById('<%= hdfSelectedReminderIds.ClientID %>');
        var button = document.getElementById('<%= btnProcessReminders.ClientID %>');

        if (!hidden || !button)
            return;

        // [SỬA LỖI]: Lấy trực tiếp từ thuộc tính value
        var id = chk.value;
        if (!id)
            return;

        var ids = hidden.value
            ? hidden.value.split(',').filter(function (x) { return x; })
            : [];

        if (chk.checked) {
            if (ids.indexOf(id) === -1)
                ids.push(id);
        }
        else {
            ids = ids.filter(function (x) { return x !== id; });
        }

        hidden.value = ids.join(',');

        var disabled = ids.length === 0;

        // Bật/tắt nút bằng CSS
        button.style.pointerEvents = disabled ? 'none' : 'auto';
        button.style.opacity = disabled ? '0.55' : '1';
        button.style.cursor = disabled ? 'default' : 'pointer';
    }
</script>
</asp:Content>
