<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" MaintainScrollPositionOnPostBack="true" CodeBehind="TaskList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.TaskList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fTasks/Controls/CtrlTask.ascx" TagPrefix="SweetSoft" TagName="CtrlTask" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
<style>
    /* ===================================================================
       MASTER FORM STYLING (PREMIUM UI 2024)
       =================================================================== */
    .smart-field { display: flex; flex-direction: column; width: 100%; min-width: 0; }
    .smart-field > label { display: flex; align-items: center; margin-bottom: 8px; color: #374151; font-size: 13.5px; font-weight: 700; line-height: 1.3; }
    .smart-field > label i { width: 17px; margin-right: 4px; color: #6b7280 !important; text-align: center; }
    
    .smart-field > .form-control, .smart-field > .form-select, .smart-field .input-group > .form-control, .smart-field .input-group > .form-select { 
        min-height: 46px !important; padding: 10px 13px !important; border: 1px solid #d1d5db !important; border-radius: 9px !important; 
        background-color: #ffffff !important; color: #1f2937 !important; font-size: 14px !important; font-weight: 500 !important; 
        box-shadow: 0 1px 2px rgba(17, 24, 39, 0.04) !important; transition: border-color .18s ease, box-shadow .18s ease, background-color .18s ease; 
    }
    .smart-field .form-control::placeholder { color: #9ca3af !important; font-weight: 400 !important; }
    .smart-field > .form-control:hover:not(:disabled):not([readonly]), .smart-field > .form-select:hover:not(:disabled), .smart-field .input-group > .form-control:hover:not(:disabled):not([readonly]) { border-color: #a78bfa !important; background-color: #ffffff !important; }
    .smart-field > .form-control:focus, .smart-field > .form-select:focus, .smart-field .input-group > .form-control:focus { border-color: #7c3aed !important; background-color: #ffffff !important; box-shadow: 0 0 0 3px rgba(124, 58, 237, 0.12), 0 2px 5px rgba(17, 24, 39, 0.04) !important; outline: none !important; }
    .smart-field textarea.form-control { min-height: 98px !important; resize: vertical; line-height: 1.55; padding-top: 11px !important; }
    .smart-field > .form-select { padding-right: 38px !important; cursor: pointer; }

    .smart-field .input-group { display: flex; flex-wrap: nowrap; align-items: stretch; }
    .smart-field .input-group > .form-control { border-radius: 9px 0 0 9px !important; }
    .smart-field .input-group > .input-group-text { min-height: 46px; padding: 0 13px; border: 1px solid #d1d5db; border-left: 0; border-radius: 0 9px 9px 0; background: #f8fafc; color: #64748b; font-size: 13px; font-weight: 700; }

    /* ===================================================================
       DISABLED & READONLY FIELDS STYLING
       =================================================================== */
    .smart-field > .form-control:disabled, 
    .smart-field > .form-select:disabled,
    .smart-field .input-group > .form-control:disabled {
        background-color: #f8fafc !important; border: 1px dashed #cbd5e1 !important; color: #64748b !important; font-weight: 600 !important; cursor: not-allowed !important; box-shadow: none !important; background-image: none !important;
    }
    
    .smart-field .input-group:has(.form-control:disabled) > .input-group-text {
        background-color: #f8fafc !important; border: 1px dashed #cbd5e1 !important; border-left: none !important; color: #94a3b8 !important;
    }
    
    .smart-field > .form-control[readonly],
    .smart-field > textarea[readonly] {
        background-color: #f5f3ff !important; border: 1px dashed #d8b4fe !important; color: #6d28d9 !important; font-weight: 600 !important; cursor: default !important; box-shadow: none !important;
    }
    
    .readonly-success { 
        background-color: #f0fdf4 !important; border: 1px solid #bbf7d0 !important; color: #15803d !important; font-weight: 700 !important; cursor: default !important; box-shadow: 0 2px 4px rgba(34, 197, 94, 0.05) !important; font-size: 15px !important; 
    }

    /* ===================================================================
       LAYOUT & CONTEXT CARDS
       =================================================================== */
    .task-context-card { padding: 16px; background: #ffffff; border: 1px solid #e5e7eb; border-left: 4px solid #6d28d9; border-radius: 12px; box-shadow: 0 2px 8px rgba(17, 24, 39, 0.05); }
    .task-context-divider { height: 1px; margin: 16px 0; background: #eef2f7; }
    .task-meta-controls { display: grid; grid-template-columns: minmax(0, 1fr) minmax(0, 1.2fr); gap: 12px; }
    .task-meta-shell { position: relative; min-width: 0; }
    .task-meta-label { display: flex; align-items: center; gap: 6px; margin: 0 0 6px 1px; color: #374151; font-size: 13.5px; font-weight: 700; letter-spacing: normal; line-height: 1.3; text-transform: none !important; }
    .task-meta-label i { width: 17px; color: #6b7280 !important; text-align: center; font-size: 13px; }
    .task-top-label { display: block; margin-bottom: 8px; color: #374151; font-size: 13.5px; font-weight: 700; letter-spacing: normal; line-height: 1.3; text-transform: none !important; white-space: nowrap; }
    /* Một nhãn Tên công việc dùng chung cho mã công việc và tên có thể chỉnh sửa. */
    .task-edit-wrapper label, .task-edit-wrapper .task-meta-label { text-transform: none !important; letter-spacing: normal !important; }
    .task-code-name-control { display: flex; align-items: stretch; width: 100%; min-width: 0; }
    .task-code-prefix { display: flex; align-items: center; justify-content: center; flex: 0 0 110px; width: 110px; min-width: 110px; min-height: 46px; padding: 0 10px; background: #f5f3ff; border: 1px solid #ddd6fe; border-right: 0; border-radius: 9px 0 0 9px; color: #6d28d9; font-size: 16px; font-weight: 800; box-shadow: 0 1px 2px rgba(109, 40, 217, 0.05); }
    .task-code-name-control > .task-name-input { flex: 1 1 auto; width: 1%; min-width: 0; min-height: 46px !important; height: 46px; padding: 10px 13px !important; border: 1px solid #d1d5db !important; border-left: 1px solid #ddd6fe !important; border-radius: 0 9px 9px 0 !important; background: #ffffff !important; color: #1e293b !important; font-size: 15px !important; font-weight: 700 !important; box-shadow: 0 1px 2px rgba(17, 24, 39, 0.04) !important; }
    .task-code-name-control > .task-name-input:focus { position: relative; z-index: 1; border-color: #7c3aed !important; box-shadow: 0 0 0 3px rgba(124, 58, 237, 0.12) !important; }
    .task-code-prefix .task-code-label { display: block; max-width: 100%; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

    /* C.V cha chỉ chiếm một vùng nhỏ như ô Giai đoạn; Phụ thuộc dùng phần còn lại. */
    #<%= colCongViecCha.ClientID %>.task-parent-column { flex: 0 0 155px !important; width: 155px !important; max-width: 155px !important; }
    #<%= colPhuThuoc.ClientID %>.task-dependent-column { flex: 1 1 0 !important; width: auto !important; max-width: none !important; min-width: 0; }
    @media (max-width: 767.98px) {
        #<%= colCongViecCha.ClientID %>.task-parent-column,
        #<%= colPhuThuoc.ClientID %>.task-dependent-column { flex: 0 0 100% !important; width: 100% !important; max-width: 100% !important; }
    }

    /* ===================================================================
       SMART STATUS & PRIORITY
       =================================================================== */
    .task-status-dot { width: 9px; height: 9px; flex: 0 0 9px; border-radius: 50%; background: #94a3b8; box-shadow: 0 0 0 3px rgba(148, 163, 184, .12); margin-right: 4px;}
    
    .btn-status-start { background: #eff6ff !important; color: #1d4ed8 !important; border: 1px solid #bfdbfe !important; transition: all 0.2s !important; box-shadow: 0 2px 4px rgba(37, 99, 235, 0.05) !important; }
    .btn-status-start:hover { background: #dbeafe !important; border-color: #93c5fd !important; transform: translateY(-1px); }
    .btn-status-complete { background: #f0fdf4 !important; color: #15803d !important; border: 1px solid #bbf7d0 !important; transition: all 0.2s !important; box-shadow: 0 2px 4px rgba(34, 197, 94, 0.05) !important; }
    .btn-status-complete:hover { background: #dcfce7 !important; border-color: #86efac !important; transform: translateY(-1px); }
    .btn-status-revert { background: #f8fafc !important; color: #475569 !important; border: 1px dashed #cbd5e1 !important; transition: all 0.2s !important; }
    .btn-status-revert:hover { background: #f1f5f9 !important; color: #1e293b !important; border-style: solid !important; border-color: #94a3b8 !important; transform: translateY(-1px); }
    .btn-status-undo { background: #ffffff !important; border: 1px solid #e2e8f0 !important; color: #64748b !important; transition: all 0.2s !important; display: flex; align-items: center; justify-content: center; width: 44px; padding: 0 !important; }
    .btn-status-undo:hover { background: #fef2f2 !important; border-color: #fca5a5 !important; color: #dc2626 !important; }
    
    .priority-box { min-height: 44px; border-radius: 9px !important; display: flex; align-items: center; justify-content: center; font-size: 13px !important; font-weight: 600 !important; transition: all 0.2s ease !important; padding: 6px !important; flex: 1; }

    .btn-view-schedule { height: 46px; border-radius: 9px !important; display: flex; align-items: center; justify-content: center; font-weight: 750 !important; background-color: #f0f9ff !important; border: 1px dashed #7dd3fc !important; color: #0284c7 !important; transition: all 0.2s; font-size: 13.5px; letter-spacing: 0.2px; padding: 0 10px; }
    .btn-view-schedule:hover { background-color: #e0f2fe !important; border-style: solid !important; border-color: #38bdf8 !important; color: #0369a1 !important; transform: translateY(-1px); }

    #divRollUpNotice .badge { display: inline-flex; align-items: center; padding: 6px 9px !important; background: #fff7ed !important; border: 1px solid #fed7aa; border-radius: 7px; color: #9a3412 !important; font-size: 11px !important; font-weight: 700; }
    #divRollUpNotice .badge i { color: #ea580c !important; }

    /* TOOLTIP THÔNG MINH */
    .st-tooltip { position: relative; cursor: help; transition: all 0.2s; }
    .st-tooltip.phase-tooltip::after, .st-tooltip.parent-tooltip::after {
        content: attr(data-title); position: absolute; bottom: 100%; right: 0; transform: translateY(-3px); 
        background: linear-gradient(135deg, #6d28d9 0%, #2e1065 100%); border: 1px solid rgba(167, 139, 250, 0.35); box-shadow: 0 8px 15px rgba(109, 40, 217, 0.25), inset 0 1px 0 rgba(255,255,255,0.1);
        color: #ffffff; font-size: 12.5px; font-weight: 600; padding: 8px 14px; border-radius: 8px; width: max-content; 
        max-width: 280px; white-space: normal; line-height: 1.45; opacity: 0; visibility: hidden; z-index: 9999; pointer-events: none; 
        transition: all 0.25s cubic-bezier(0.34, 1.56, 0.64, 1); text-align: left;
    }
    .st-tooltip.parent-tooltip::after { right: auto; left: 0; }
    .st-tooltip.phase-tooltip::before, .st-tooltip.parent-tooltip::before {
        content: ""; position: absolute; bottom: 100%; right: 18px; transform: translateY(4px); 
        border: 6px solid transparent; border-top-color: #6d28d9; opacity: 0; visibility: hidden; z-index: 9999; pointer-events: none; transition: all 0.2s ease; 
    }
    .st-tooltip.parent-tooltip::before { right: auto; left: 20px; }
    
    .st-tooltip.phase-tooltip:hover::after, .st-tooltip.parent-tooltip:hover::after { opacity: 1; visibility: visible; transform: translateY(-8px); }
    .st-tooltip.phase-tooltip:hover::before, .st-tooltip.parent-tooltip:hover::before { opacity: 1; visibility: visible; transform: translateY(-2px); }
    .st-tooltip.parent-tooltip:hover { background: #f5f3ff !important; border-color: #d8b4fe !important; }

    /* Hiệu ứng chỉ mở rộng ngang: form cố định chiều rộng, lịch được lộ dần theo popup. */
    .task-edit-wrapper { --ql-task-cal-width:clamp(418px,calc(100vw - 782px),530px);display:flex;align-items:stretch;width:100%;min-width:0;position:relative;overflow:hidden;box-sizing:border-box; }
    .task-form-panel { flex:0 0 750px;width:750px;min-width:0;max-width:100%;transition:none; }
    .task-schedule-panel { box-sizing:border-box;flex:0 0 0;width:0;min-width:0;height:0;max-height:0;align-self:flex-start;overflow:hidden;opacity:0;visibility:hidden;pointer-events:none;transform:translateX(18px);background:#f8fafc;border-left:0 solid #e2e8f0;transition:flex-basis .46s cubic-bezier(.22,1,.36,1),width .46s cubic-bezier(.22,1,.36,1),opacity .24s ease,transform .46s cubic-bezier(.22,1,.36,1),visibility 0s linear .46s,border-width .46s ease; }
    .task-schedule-panel > .p-3 { box-sizing:border-box;width:var(--ql-task-cal-width);min-width:var(--ql-task-cal-width); }
    .task-edit-wrapper.is-split .task-form-panel { flex:0 0 750px;width:750px;min-width:0;max-width:none; }
    .task-edit-wrapper.is-split .task-schedule-panel { flex:0 0 var(--ql-task-cal-width);width:var(--ql-task-cal-width);height:var(--ql-task-form-height,auto);max-height:var(--ql-task-form-height,none);align-self:flex-start;overflow-x:hidden;overflow-y:auto;opacity:1;visibility:visible;pointer-events:auto;transform:translateX(0);border-left-width:1px;transition: flex-basis .46s cubic-bezier(.22,1,.36,1),width .46s cubic-bezier(.22,1,.36,1),opacity .3s ease .1s,transform .46s cubic-bezier(.22,1,.36,1),visibility 0s,border-width .46s ease; }
    #<%= mdlEditTask.ClientID %> .modal-dialog { width:750px!important;max-width:calc(100vw - 32px)!important;transition:width .46s cubic-bezier(.22,1,.36,1),max-width .46s cubic-bezier(.22,1,.36,1),transform .3s ease!important; }
    #<%= mdlEditTask.ClientID %> .modal-dialog.ql-task-schedule-expanded { width:min(1280px,calc(100vw - 32px))!important;max-width:calc(100vw - 32px)!important; }
    @media(max-width:1199.98px) { .task-edit-wrapper { --ql-task-cal-width:100%; } .task-form-panel,.task-edit-wrapper.is-split .task-form-panel { flex:0 0 100%;width:100%;max-width:100%; } .task-edit-wrapper.is-split { flex-direction:column; } .task-schedule-panel { flex:0 0 0;width:100%;max-height:0;transform:translateY(12px);border-left:0;border-top:0 solid #e2e8f0; } .task-schedule-panel > .p-3 { width:100%;min-width:0; } .task-edit-wrapper.is-split .task-schedule-panel { flex:0 0 auto;width:100%;height:auto;max-height:500px;align-self:stretch;border-left:0;border-top-width:1px;transform:translateY(0); } #<%= mdlEditTask.ClientID %> .modal-dialog.ql-task-schedule-expanded { width:calc(100vw - 32px)!important; } }
    @media(prefers-reduced-motion:reduce) { #<%= mdlEditTask.ClientID %> .modal-dialog,.task-schedule-panel,.task-edit-wrapper.is-split .task-schedule-panel { transition:none!important; } }

    /* ===================================================================
       GLOBAL LOADING OVERLAY (MỜ ẢO ĐỈNH CAO)
       =================================================================== */
    #global-loading-overlay { position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; background: rgba(255, 255, 255, 0.6); backdrop-filter: blur(3px); -webkit-backdrop-filter: blur(3px); z-index: 999999; display: flex; flex-direction: column; align-items: center; justify-content: center; opacity: 0; visibility: hidden; transition: opacity 0.3s ease, visibility 0.3s ease; }
    #global-loading-overlay.is-active { opacity: 1; visibility: visible; }
    .st-spinner-ring { width: 54px; height: 54px; border: 4px solid #ede9fe; border-top: 4px solid #7c3aed; border-radius: 50%; animation: stSpinRing 0.85s linear infinite; margin-bottom: 16px; box-shadow: 0 4px 12px rgba(124, 58, 237, 0.15); }
    .st-loading-text { color: #5b21b6; font-weight: 800; font-size: 15px; letter-spacing: 0.5px; animation: stPulseText 1.5s ease-in-out infinite; text-transform: uppercase; }
    @keyframes stSpinRing { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
    @keyframes stPulseText { 0%, 100% { opacity: 1; } 50% { opacity: 0.5; } }

    /* Lịch dự kiến độc lập: không phụ thuộc CMSMasterJs.DrawTaskCalendarGrid. */
    #local-task-timeline-container .ql-local-cal-toolbar { display:flex;align-items:center;justify-content:space-between;gap:8px;margin-bottom:10px; }
    #local-task-timeline-container .ql-local-cal-title { color:#4b1e82;font-size:14px;font-weight:800;text-align:center; }
    #local-task-timeline-container .ql-local-cal-nav { display:inline-grid;place-items:center;width:29px;height:29px;flex:0 0 29px;border:1px solid #ddd2ec;border-radius:7px;background:#fff;color:#54218d;cursor:pointer; }
    #local-task-timeline-container .ql-local-cal-nav:hover { background:#efe6fc;border-color:#bd9be2; }
    #local-task-timeline-container .ql-local-cal-grid { display:grid;grid-template-columns:repeat(7,minmax(0,1fr));gap:4px; }
    #local-task-timeline-container .ql-local-cal-dow { padding:5px 0;text-align:center;color:#725586;font-size:10.5px;font-weight:750; }
    #local-task-timeline-container .ql-local-cal-day { display:flex;flex-direction:column;justify-content:space-between;gap:4px;min-height:47px;min-width:0;padding:5px 3px;border:1px solid #eeeaf4;border-radius:7px;background:#fff;color:#4d4661;text-align:left; }
    #local-task-timeline-container .ql-local-cal-day.is-outside { background:#faf9fc;color:#a5a0ae; }
    #local-task-timeline-container .ql-local-cal-day.is-free { background:#f1fbf5;border-color:#ccedda;color:#176a45; }
    #local-task-timeline-container .ql-local-cal-day.is-busy { background:#eee7fa;border-color:#ceb5e9;color:#54218d; }
    #local-task-timeline-container .ql-local-cal-day.is-holiday { background:#fff4e7;border-color:#f2d4aa;color:#956014; }
    #local-task-timeline-container .ql-local-cal-day.is-weekend { background:#f6f2f9;border-color:#e2d8e9;color:#82658d; }
    #local-task-timeline-container .ql-local-cal-day.is-in-range { cursor:help; }
    #local-task-timeline-container .ql-local-cal-day.is-in-range:hover { outline:2px solid #ad82d7;outline-offset:0; }
    #local-task-timeline-container .ql-local-cal-number { font-size:11px;font-weight:800; }
    #local-task-timeline-container .ql-local-cal-caption { overflow:hidden;text-overflow:ellipsis;white-space:nowrap;font-size:9.5px;font-weight:650; }
    #local-task-timeline-container .ql-local-cal-legend { display:flex;flex-wrap:wrap;gap:6px 12px;margin-top:12px;color:#6f6480;font-size:11px; }
    #local-task-timeline-container .ql-local-cal-legend b { display:inline-block;width:9px;height:9px;margin-right:4px;border-radius:3px;vertical-align:baseline; }
    #local-task-timeline-container .ql-local-cal-empty { padding:18px 9px;text-align:center;color:#857c91;font-size:12px; }
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
                    <div class="task-edit-wrapper" id="taskEditWrapper">
                        
                        <!-- CỘT TRÁI: FORM CẬP NHẬT -->
                        <div id="pnlTaskForm" class="task-form-panel">
                            <div class="px-2 py-3">
                                <asp:HiddenField ID="hfEditTaskId" runat="server" />
                                
                                <div class="task-context-card mb-3" id="divContextCard" runat="server">
                                    <div class="d-flex align-items-end gap-3 mb-3">
                                        <div style="flex: 1 1 auto; min-width: 0;">
                                            <label class="task-top-label"><i class="fas fa-tasks me-1"></i><%= GetResourceText(BackEndResourceKeys.TASK_NAME) %> <span class="text-danger ms-1">*</span></label>
                                            <div class="task-code-name-control">
                                                <div class="task-code-prefix" title="Mã công việc">
                                                    <asp:Label ID="lblEditMaCv" runat="server" CssClass="task-code-label"></asp:Label>
                                                    <asp:TextBox ID="txtEditMaCv" runat="server" style="display:none;"></asp:TextBox>
                                                </div>
                                                <asp:TextBox ID="txtEditTenCv" runat="server" CssClass="form-control task-name-input" placeholder="Nhập tên đầu việc..."></asp:TextBox>
                                            </div>
                                        </div>

                                        <div id="rowBreadcrumb" runat="server" style="flex: 0 0 auto;">
                                            <label class="task-top-label text-center">Giai đoạn</label>
                                            <div class="st-tooltip phase-tooltip d-flex align-items-center justify-content-center" id="phaseTooltipWrapper" data-title="" style="height: 46px; min-width: 46px; border-radius: 9px; margin: 0; padding: 0 10px; background: #f5f3ff; border: 1px dashed #d8b4fe; color: #6d28d9;">
                                                <span id="phaseShortText" style="font-size: 15px; font-weight: 800;"></span>
                                            </div>
                                            <asp:Label ID="lblEditGiaiDoan" runat="server" CssClass="d-none"></asp:Label>
                                            <asp:TextBox ID="txtEditGiaiDoan" runat="server" style="display:none;"></asp:TextBox>
                                        </div>
                                    </div>
                                    
                                    <div class="task-context-divider" style="margin: 0 0 16px 0;"></div>
                                    
                                    <div class="task-meta-controls" id="rowUuTienTrangThai" runat="server">
                                        <div class="task-meta-shell" id="boxUuTien" runat="server">
                                            <div class="task-meta-label"><i class="fas fa-flag"></i> Ưu tiên</div>
                                            <asp:DropDownList ID="ddlEditDoUuTien" runat="server" CssClass="d-none task-priority-select" onchange="initSmartPriority()"></asp:DropDownList>
                                            
                                            <div class="d-flex align-items-center gap-2 w-100" id="prioritySelector"></div>
                                            
                                            <div id="priorityLocked" style="display: none;">
                                                <button type="button" class="btn w-100 fw-bold d-flex align-items-center justify-content-start gap-2" style="min-height: 44px; padding-left: 14px; cursor: not-allowed; pointer-events: none; border-radius: 9px;"></button>
                                            </div>
                                        </div>

                                        <div class="task-meta-shell" id="boxTrangThai" runat="server">
                                            <div class="task-meta-label"><i class="fas fa-check-circle"></i> Trạng thái</div>
                                            <asp:DropDownList ID="ddlEditTrangThai" runat="server" CssClass="d-none task-status-select"></asp:DropDownList>
                                            
                                            <div class="d-flex align-items-center gap-2">
                                                <button type="button" id="btnSmartStatus" class="btn w-100 fw-bold d-flex align-items-center justify-content-center gap-2" onclick="toggleSmartStatus()" style="min-height: 44px; font-size: 13.5px; border-radius: 9px;">
                                                </button>
                                                <button type="button" id="btnUndoStatus" class="btn btn-status-undo" onclick="undoSmartStatus()" title="Hoàn tác trạng thái ban đầu" style="min-height: 44px; border-radius: 9px; display: none;">
                                                    <i class="fas fa-undo"></i>
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <div class="row g-3 mb-3" id="rowChaVaPhuThuoc" runat="server">
                                    <div class="col-12 col-md-auto task-parent-column" id="colCongViecCha" runat="server">
                                        <div class="smart-field">
                                            <label style="white-space: nowrap;"><i class="fas fa-sitemap"></i> Công việc cha</label>
                                            <asp:DropDownList ID="ddlEditCongViecCha" runat="server" CssClass="d-none task-parent-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEditCongViecChaSelected"></asp:DropDownList>
                                            
                                            <div class="st-tooltip parent-tooltip d-flex align-items-center justify-content-center" id="parentTooltipWrapper" data-title="" style="height: 46px; border-radius: 9px; background: #f8fafc; border: 1px dashed #cbd5e1; cursor: help; transition: all 0.2s;">
                                                <span id="parentShortText" style="font-size: 15px; font-weight: 800; color: #475569;"></span>
                                            </div>
                                        </div>
                                    </div>

                                    <div class="col-12 col-md task-dependent-column" id="colPhuThuoc" runat="server">
                                        <div class="smart-field">
                                            <label><i class="fas fa-link"></i><%= GetResourceText(BackEndResourceKeys.DEPENDENT) %></label>
                                            <asp:DropDownList ID="ddlEditPhuThuoc" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEditPhuThuocSelected"></asp:DropDownList>
                                        </div>
                                    </div>
                                </div>

                                <div id="divRollUpNotice" runat="server" class="mb-3" visible="false">
                                    <div style="padding: 10px 14px; background: #fff7ed; border: 1px dashed #fdba74; border-radius: 9px; display: flex; align-items: center; gap: 8px;">
                                        <i class="fas fa-lock text-warning" style="font-size: 16px;"></i>
                                        <span style="color: #9a3412; font-size: 13px; font-weight: 600;">Ngày tháng và Trạng thái bị khóa do tổng hợp tự động từ task con.</span>
                                    </div>
                                </div>
                                
                                <div class="row g-3 mb-3 align-items-end">
                                    <div class="col-md-3">
                                        <div class="smart-field">
                                            <label><i class="far fa-calendar-alt"></i><%= GetResourceText(BackEndResourceKeys.START_DATE) %> <span class="text-danger ms-1">*</span></label>
                                            <asp:TextBox ID="txtEditNgayBatDau" runat="server" CssClass="form-control px-2" TextMode="Date"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-md-3">
                                        <div class="smart-field">
                                            <label><i class="fas fa-hourglass-half"></i><%= GetResourceText(BackEndResourceKeys.DURATION) %> <span class="text-danger ms-1">*</span></label>
                                            <div class="input-group">
                                                <asp:TextBox ID="txtEditThoiHan" runat="server" CssClass="form-control px-2" TextMode="Number" min="1"></asp:TextBox>
                                                <span class="input-group-text px-2">Ngày</span>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-md-3">
                                        <div class="smart-field">
                                            <label style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis;"><i class="far fa-calendar-check me-1 text-success"></i><%= GetResourceText(BackEndResourceKeys.EXPECTED_COMPLETION_DATE) %></label>
                                            <div class="form-control d-flex align-items-center readonly-success px-2">
                                                <asp:Label ID="lblNgayKetThuc" runat="server" CssClass="mb-0">--/--/----</asp:Label>
                                                <asp:TextBox ID="txtEditNgayKetThuc" runat="server" style="display:none;"></asp:TextBox>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-md-3">
                                        <asp:LinkButton ID="btnViewScheduleInEdit" runat="server" CssClass="btn w-100 btn-view-schedule" OnClick="btnViewScheduleInEdit_Click" OnClientClick="return toggleLocalTaskSchedule();" ToolTip="Bật hoặc tắt bảng lịch dự kiến">
                                            <i class="fas fa-calendar-alt me-1"></i> Lịch dự kiến
                                        </asp:LinkButton>
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
                        </div>

                        <!-- CỘT PHẢI: LỊCH BIỂU -->
                        <div id="pnlTaskSchedule" class="task-schedule-panel">
                            <div class="p-3">
                                <div class="apt-detail-header mb-3" style="display: flex; justify-content: space-between; align-items: center; border-bottom: 1px dashed #cbd5e1; padding-bottom: 10px;">
                                    <h6 class="m-0" style="color: #1e40af; font-weight: 700; font-size: 16px;"><i class="fas fa-calendar-alt me-2"></i>Lịch biểu dự kiến</h6>
                                    <button type="button" class="btn btn-sm btn-light border" onclick="closeLocalTaskSchedule(); return false;" style="color: #475569; font-weight: 600;"><i class="fas fa-chevron-right me-1"></i>Đóng lịch</button>
                                </div>
                                <asp:HiddenField ID="hdfLocalScheduleJson" runat="server" />
                                <div class="row-schedule-inner" style="padding: 10px 0;">
                                    <div class="mini-cal-wrap">
                                        <div class="mini-cal" id="local-task-timeline-container"></div>
                                    </div>
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
                    <asp:LinkButton ID="btnSaveTask" runat="server" CssClass="btn btn-primary waves-effect waves-light" CausesValidation="false" OnClick="btnSaveTask_Click" OnClientClick="showGlobalLoading();">
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
                    <asp:LinkButton ID="btnSaveHeSo" runat="server" CssClass="btn btn-primary waves-effect waves-light" CausesValidation="false" OnClick="btnSaveHeSo_Click" OnClientClick="showGlobalLoading();">
                        <i class="fas fa-save me-1"></i> Lưu hệ số
                    </asp:LinkButton>
                </ContentTemplate>
            </asp:UpdatePanel>
        </FooterTemplate>
    </SweetSoft:ExtraModal>

    <!-- POPUP NHẮC VIỆC -->
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

    // =========================================================
    // LỊCH DỰ KIẾN: CHỈ CHUYỂN ĐỘNG THEO CHIỀU NGANG
    // =========================================================
    function getLocalTaskScheduleElements() {
        var wrapper = document.getElementById('taskEditWrapper');
        var modal = document.getElementById('<%= mdlEditTask.ClientID %>');
        var dialog = modal ? modal.querySelector('.modal-dialog') : null;
        var form = document.getElementById('pnlTaskForm');
        return { wrapper: wrapper, modal: modal, dialog: dialog, form: form };
    }

    function setLocalTaskScheduleVisible(visible) {
        var el = getLocalTaskScheduleElements();
        if (!el.wrapper || !el.dialog || !el.form) return false;
        if (el.wrapper.classList.contains('is-split') === visible) return true;
        if (visible) {
            // Vẽ trước khi animation bắt đầu; panel đang ẩn hoàn toàn nên không làm popup dài xuống.
            renderLocalSchedule();
            var formHeight = Math.ceil(el.form.getBoundingClientRect().height);
            if (formHeight > 0) el.wrapper.style.setProperty('--ql-task-form-height', formHeight + 'px');
        }
        // Modal và panel bắt đầu/hoàn tất cùng một nhịp. Không để browser tái căn giữa theo chiều cao.
        el.wrapper.classList.toggle('is-split', visible);
        el.dialog.classList.toggle('ql-task-schedule-expanded', visible);
        return true;
    }

    function toggleLocalTaskSchedule() {
        var el = getLocalTaskScheduleElements();
        if (!el.wrapper) return false;
        if (el.wrapper.classList.contains('is-split')) { setLocalTaskScheduleVisible(false); return false; }
        var hiddenField = document.getElementById('<%= hdfLocalScheduleJson.ClientID %>');
        if(hiddenField && hiddenField.value && hiddenField.value.trim()) { setLocalTaskScheduleVisible(true);return false; }
        // Trường hợp dữ liệu chưa được nạp (ngày không hợp lệ/lỗi truy vấn): dùng fallback server cũ.
        return true;
    }

    function openLocalTaskSchedule() { setLocalTaskScheduleVisible(true); }
    function closeLocalTaskSchedule() { setLocalTaskScheduleVisible(false); }

    function resetLocalTaskScheduleState(clearJson) {
        var el=getLocalTaskScheduleElements();
        if(el.wrapper) {el.wrapper.classList.remove('is-split');el.wrapper.style.removeProperty('--ql-task-form-height');}
        if(el.dialog) el.dialog.classList.remove('ql-task-schedule-expanded');
        var cal=$('#local-task-timeline-container');
        if(cal.length) cal.empty().removeData('mc');
        if(clearJson) { var hiddenField=document.getElementById('<%= hdfLocalScheduleJson.ClientID %>');if(hiddenField) hiddenField.value=''; }
    }

    (function bindTaskEditModalCloseBehavior() {
        var modalSelector='#<%= mdlEditTask.ClientID %>';
        $(document).off('hide.bs.modal.taskEditScheduleClose',modalSelector).on('hide.bs.modal.taskEditScheduleClose',modalSelector,function () {
            var dialog=this.querySelector('.modal-dialog');
            if(!dialog)return;
            var currentWidth=dialog.getBoundingClientRect().width;
            if(currentWidth>0) {dialog.style.setProperty('width',currentWidth+'px','important');dialog.style.setProperty('max-width',currentWidth+'px','important');}
            dialog.style.setProperty('transition','transform .3s ease-out','important');
        }).off('hidden.bs.modal.taskEditScheduleClose',modalSelector).on('hidden.bs.modal.taskEditScheduleClose',modalSelector,function () {
            var dialog=this.querySelector('.modal-dialog');
            resetLocalTaskScheduleState(true);
            if(dialog){dialog.style.removeProperty('width');dialog.style.removeProperty('max-width');dialog.style.removeProperty('transition');}
        });
    })();

    // Renderer nội bộ: nếu Master không có DrawTaskCalendarGrid vẫn vẽ lịch được.
    function renderLocalSchedule() {
        var cal = $('#local-task-timeline-container');
        if (!cal.length) return;
        var hiddenField = document.getElementById('<%= hdfLocalScheduleJson.ClientID %>');
        if (!hiddenField || !hiddenField.value) { cal.html('<div class="ql-local-cal-empty">Chưa có dữ liệu lịch dự kiến.</div>'); return; }
        var data;
        try { data = JSON.parse(hiddenField.value); } catch (e) { cal.html('<div class="ql-local-cal-empty">Dữ liệu lịch không hợp lệ.</div>'); return; }
        var keys = Object.keys(data || {}).sort();
        if (!keys.length) { cal.html('<div class="ql-local-cal-empty">Không có ngày trong lịch.</div>'); return; }
        var parts = keys[0].split('-');
        var baseDate = new Date(+parts[0], +parts[1] - 1, +parts[2]);
        var oldState = cal.data('mc');
        // Sau UpdatePanel, task mới sẽ trở về tháng bắt đầu; task cũ giữ tháng đang xem.
        var cacheId = keys[0] + '|' + keys[keys.length - 1];
        var current = oldState && oldState.cacheId === cacheId ? oldState : {year:baseDate.getFullYear(),month:baseDate.getMonth()};
        current.data = data;current.cacheId = cacheId;current.minKey = keys[0];current.maxKey = keys[keys.length-1];
        cal.data('mc', current);
        drawLocalScheduleMonth();
    }

    function drawLocalScheduleMonth() {
        var cal = $('#local-task-timeline-container'), mc = cal.data('mc');
        if (!mc || !mc.data) return;
        var year = mc.year, month = mc.month;
        var first = new Date(year, month, 1);
        var offset = (first.getDay() + 6) % 7;
        var days = new Date(year, month + 1, 0).getDate();
        var totalCells = Math.ceil((offset + days) / 7) * 7;
        var weekdayNames = ['T2','T3','T4','T5','T6','T7','CN'];
        var html = '<div class="ql-local-cal-toolbar"><button type="button" class="ql-local-cal-nav" data-cal-step="-1" aria-label="Tháng trước"><i class="fas fa-chevron-left"></i></button><span class="ql-local-cal-title">Tháng '+(month+1)+'/'+year+'</span><button type="button" class="ql-local-cal-nav" data-cal-step="1" aria-label="Tháng sau"><i class="fas fa-chevron-right"></i></button></div><div class="ql-local-cal-grid">';
        weekdayNames.forEach(function (n) { html += '<div class="ql-local-cal-dow">'+n+'</div>'; });
        function escapeHtml(v) { return $('<div/>').text(v == null ? '' : String(v)).html(); }
        function dateKey(d) { return d.getFullYear() + '-' + ('0'+(d.getMonth()+1)).slice(-2) + '-' + ('0'+d.getDate()).slice(-2); }
        for (var n = 0; n < totalCells; n++) {
            var date = new Date(year, month, n-offset+1), key=dateKey(date), row=mc.data[key];
            var status = row && /^(busy|free|holiday|weekend)$/.test(row.status) ? row.status : '';
            var caption = row && row.text ? String(row.text) : '';
            var summary = row && Array.isArray(row.tasks) ? row.tasks.map(function(t){return '[' + (t.code||'') + '] ' + (t.name||'');}).join('\n') : '';
            var tip = escapeHtml((caption ? caption : 'Không có lịch') + (summary ? '\n' + summary : ''));
            html += '<div class="ql-local-cal-day '+(row?'is-in-range is-'+status:'is-outside')+'" title="'+tip+'"><span class="ql-local-cal-number">'+date.getDate()+'</span>'+(row ? '<span class="ql-local-cal-caption">'+escapeHtml(caption || (status==='free'?'Trống':''))+'</span>' : '')+'</div>';
        }
        html += '</div><div class="ql-local-cal-legend"><span><b style="background:#a17bd2"></b>Có công việc</span><span><b style="background:#66b38a"></b>Trống</span><span><b style="background:#e6b476"></b>Nghỉ lễ</span><span><b style="background:#b3a4c2"></b>Cuối tuần</span></div>';
        cal.html(html);
    }
    $(document).off('click.qlLocalCalendar').on('click.qlLocalCalendar', '#local-task-timeline-container [data-cal-step]', function () {
        var cal=$('#local-task-timeline-container'), mc=cal.data('mc');
        if (!mc) return;
        var date=new Date(mc.year,mc.month+parseInt(this.getAttribute('data-cal-step'),10),1);
        mc.year=date.getFullYear();mc.month=date.getMonth();drawLocalScheduleMonth();
    });

    // =========================================================
    // TỰ ĐỘNG ĐÓNG NGOẶC VUÔNG CHO MÃ CÔNG VIỆC
    // =========================================================
    function processMaCvUI() {
        var $lblMaCv = $('#<%= lblEditMaCv.ClientID %>');
        if ($lblMaCv.length > 0) {
            var text = $lblMaCv.text().trim();
            if (text && !text.startsWith('[')) {
                $lblMaCv.text('[' + text + ']');
            }
        }
    }

    function processPhaseUI() {
        var $lblPhase = $('#<%= lblEditGiaiDoan.ClientID %>');
        if ($lblPhase.length === 0) return; 
        
        var fullText = $lblPhase.text().trim();
        var $wrapper = $('#phaseTooltipWrapper');
        var $shortText = $('#phaseShortText');
        
        if (!fullText || fullText === '-- --' || fullText === 'Giai đoạn gốc' || fullText.indexOf('---') > -1) {
            $wrapper.attr('data-title', 'Là giai đoạn gốc');
            $wrapper.css({ 'background': '#f8fafc', 'border': '1px dashed #cbd5e1', 'color': '#94a3b8' });
            $shortText.html('<i class="fas fa-minus m-0" style="font-size:14px;"></i>');
            return;
        }

        $wrapper.attr('data-title', fullText);

        var match = fullText.match(/\[(.*?)\]/);
        if (match && match[0]) {
            $shortText.text(match[0]);
        } else {
            $shortText.html('<i class="fas fa-layer-group m-0" style="font-size:14px;"></i>');
        }
    }

    function processParentUI() {
        var $ddlParent = $('#<%= ddlEditCongViecCha.ClientID %>');
        if ($ddlParent.length === 0) return;
        
        var $wrapper = $('#parentTooltipWrapper');
        var $shortText = $('#parentShortText');
        
        var fullText = $ddlParent.find('option:selected').text() || '';
        fullText = fullText.trim();

        if (!fullText || fullText === 'Chưa chọn' || fullText.indexOf('---') > -1) {
            $wrapper.attr('data-title', 'Không có công việc cha');
            $shortText.html('<i class="fas fa-sitemap m-0" style="font-size:14px; color: #94a3b8;"></i>');
            return;
        }

        $wrapper.attr('data-title', fullText);

        var match = fullText.match(/\[(.*?)\]/);
        if (match && match[0]) {
            $shortText.text(match[0]);
        } else {
            $shortText.html('<i class="fas fa-sitemap m-0" style="font-size:14px; color: #475569;"></i>');
        }
    }

    // =========================================================
    // LOGIC NÚT ĐỘ ƯU TIÊN (SMART PRIORITY)
    // =========================================================
    function initSmartPriority() {
        var $ddl = $('.task-priority-select');
        if (!$ddl.length) return;
        
        var isDisabled = $ddl.prop('disabled') || $ddl.attr('disabled') === 'disabled';
        var currentVal = $ddl.val();
        var $selector = $('#prioritySelector');
        var $locked = $('#priorityLocked');
        var $lockedBtn = $locked.find('button');
        
        if (isDisabled) {
            $selector.hide();
            $locked.show();
            
            var text = $ddl.find('option:selected').text();
            if (!text) text = 'Chưa thiết lập';
            
            var color = '#64748b'; var bg = '#f8fafc'; var border = '#e2e8f0';
            if (text.toLowerCase().indexOf('cao') > -1) { color = '#b91c1c'; bg = '#fef2f2'; border = '#fecaca'; }
            else if (text.toLowerCase().indexOf('trung bình') > -1) { color = '#b45309'; bg = '#fffbeb'; border = '#fde68a'; }
            else if (text.toLowerCase().indexOf('thấp') > -1) { color = '#1d4ed8'; bg = '#eff6ff'; border = '#bfdbfe'; }
            
            $lockedBtn.css({ 'background': bg, 'color': color, 'border': '1px dashed ' + border });
            $lockedBtn.html('<span class="task-status-dot" style="background:'+color+'; box-shadow: 0 0 0 3px '+bg+';"></span> ' + text);
        } else {
            $locked.hide();
            $selector.show();
            
            $selector.empty();
            $ddl.find('option').each(function() {
                var val = $(this).val();
                if (!val) return; 
                var text = $(this).text();
                var isSelected = (val === currentVal);
                
                var color = '#64748b'; var border = '#e2e8f0'; var hoverBg = '#f1f5f9';
                var selectedBg = '#ffffff'; var selectedBorder = '#64748b';
                
                if (text.toLowerCase().indexOf('cao') > -1) { color = '#b91c1c'; border = '#fecaca'; hoverBg = '#fef2f2'; selectedBorder = '#ef4444'; selectedBg = '#fff5f5'; }
                else if (text.toLowerCase().indexOf('trung bình') > -1) { color = '#b45309'; border = '#fde68a'; hoverBg = '#fffbeb'; selectedBorder = '#f59e0b'; selectedBg = '#fffbf0'; }
                else if (text.toLowerCase().indexOf('thấp') > -1) { color = '#1d4ed8'; border = '#bfdbfe'; hoverBg = '#eff6ff'; selectedBorder = '#3b82f6'; selectedBg = '#f0f5ff'; }
                
                var $box = $('<button type="button" class="btn priority-box"></button>');
                
                if (isSelected) {
                    $box.css({ 'background': selectedBg, 'color': color, 'border': '2px solid ' + selectedBorder, 'box-shadow': '0 2px 4px rgba(0,0,0,0.03)' });
                    $box.html('<span class="task-status-dot" style="background:'+color+'; box-shadow: 0 0 0 2px '+selectedBg+';"></span> ' + text);
                } else {
                    $box.css({ 'background': '#ffffff', 'color': '#64748b', 'border': '1px solid #e2e8f0' });
                    $box.html(text);
                    $box.hover(function() {
                        $(this).css({ 'background': hoverBg, 'color': color, 'border-color': border });
                    }, function() {
                        $(this).css({ 'background': '#ffffff', 'color': '#64748b', 'border-color': '#e2e8f0' });
                    });
                }
                
                $box.on('click', function() {
                    $ddl.val(val).trigger('change');
                });
                
                $selector.append($box);
            });
        }
    }

    // =========================================================
    // LOGIC NÚT TRẠNG THÁI (SMART STATUS)
    // =========================================================
    function initSmartStatus() {
        var $ddl = $('.task-status-select');
        if (!$ddl.length) return;
        if (typeof $ddl.data('original-value') === 'undefined') {
            $ddl.data('original-value', $ddl.val());
        }
        renderSmartStatus();
    }

    function renderSmartStatus() {
        var $ddl = $('.task-status-select');
        var $btn = $('#btnSmartStatus');
        var $undo = $('#btnUndoStatus');
        if (!$ddl.length) return;
        
        var currentVal = $ddl.val();
        var originalVal = $ddl.data('original-value');
        var isDisabled = $ddl.prop('disabled') || $ddl.attr('disabled') === 'disabled';
        
        $btn.removeClass('btn-status-start btn-status-complete btn-status-revert');
        
        if (isDisabled) {
            $btn.css({ 'cursor': 'not-allowed', 'pointer-events': 'none' });
            
            if (currentVal === '0') {
                $btn.css({ 'background': '#f8fafc', 'color': '#64748b', 'border': '1px dashed #cbd5e1' });
                $btn.html('<i class="fas fa-hourglass-start"></i> Chưa bắt đầu');
            } else if (currentVal === '1') {
                $btn.css({ 'background': '#eff6ff', 'color': '#1d4ed8', 'border': '1px solid #bfdbfe' });
                $btn.html('<i class="fas fa-play"></i> Đang thực hiện');
            } else if (currentVal === '2') {
                $btn.css({ 'background': '#f0fdf4', 'color': '#15803d', 'border': '1px solid #bbf7d0' });
                $btn.html('<i class="fas fa-check"></i> Hoàn thành');
            } else if (currentVal === '3') {
                $btn.css({ 'background': '#fef2f2', 'color': '#b91c1c', 'border': '1px solid #fecaca' });
                $btn.html('<i class="fas fa-exclamation-circle"></i> Hoàn thành <span style="color:#dc2626; margin-left:4px;">(Trễ hạn)</span>');
            } else {
                $btn.css({ 'background': '#f8fafc', 'color': '#64748b', 'border': '1px solid #e2e8f0' });
                $btn.html('<i class="fas fa-info-circle"></i> Không xác định');
            }
            $undo.hide();
        } 
        else {
            $btn.css({ 'background': '', 'color': '', 'border': '', 'cursor': 'pointer', 'pointer-events': 'auto' });
            
            if (currentVal === '0') {
                $btn.addClass('btn-status-start').html('<i class="fas fa-play"></i> Bắt đầu thực hiện');
            } else if (currentVal === '1') {
                $btn.addClass('btn-status-complete').html('<i class="fas fa-check"></i> Đánh dấu hoàn thành');
            } else if (currentVal === '2' || currentVal === '3') {
                $btn.addClass('btn-status-start').html('<i class="fas fa-redo"></i> Tiếp tục thực hiện'); 
            } else {
                $btn.addClass('btn-status-revert').html('<i class="fas fa-exchange-alt"></i> Thay đổi');
            }
            
            if (currentVal !== originalVal && originalVal !== undefined) {
                $undo.fadeIn(200);
            } else {
                $undo.hide();
            }
        }
    }

    function toggleSmartStatus() {
        var $ddl = $('.task-status-select');
        if ($ddl.prop('disabled') || $ddl.attr('disabled') === 'disabled') return;
        
        var currentVal = $ddl.val();
        if (currentVal === '0') {
            $ddl.val('1'); 
        } else if (currentVal === '1') {
            $ddl.val('2'); 
        } else if (currentVal === '2' || currentVal === '3') {
            $ddl.val('1'); 
        }
        renderSmartStatus();
    }

    function undoSmartStatus() {
        var $ddl = $('.task-status-select');
        var originalVal = $ddl.data('original-value');
        if (originalVal !== undefined) {
            $ddl.val(originalVal);
            renderSmartStatus();
        }
    }

    // =========================================================
    // HIỆU ỨNG LOADING TOÀN MÀN HÌNH
    // =========================================================
    function showGlobalLoading() {
        if ($('#global-loading-overlay').length === 0) {
            $('body').append('<div id="global-loading-overlay"><div class="st-spinner-ring"></div><div class="st-loading-text">Đang xử lý dữ liệu...</div></div>');
        }
        $('#global-loading-overlay').addClass('is-active');
    }

    function hideGlobalLoading() {
        $('#global-loading-overlay').removeClass('is-active');
    }

    // =========================================================
    // EVENT BINDINGS
    // =========================================================
    $(document).off('change.taskPrioritySync').on('change.taskPrioritySync', '.task-priority-select', function () {
        initSmartPriority();
    });

    $(function () {
        initSmartStatus();
        initSmartPriority();
        processMaCvUI();
        processPhaseUI();
        processParentUI();
    });

    if (window.Sys && Sys.Application) {
        Sys.Application.add_load(function () {
            initSmartStatus();
            initSmartPriority();
            processMaCvUI();
            processPhaseUI();
            processParentUI();
        });
        
        // Tự động tắt Loading Overlay khi Server xử lý xong Data
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            hideGlobalLoading();
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

        if (!hidden || !button) return;

        var id = chk.value;
        if (!id) return;

        var ids = hidden.value ? hidden.value.split(',').filter(function (x) { return x; }) : [];

        if (chk.checked) {
            if (ids.indexOf(id) === -1) ids.push(id);
        } else {
            ids = ids.filter(function (x) { return x !== id; });
        }

        hidden.value = ids.join(',');
        var disabled = ids.length === 0;

        button.style.pointerEvents = disabled ? 'none' : 'auto';
        button.style.opacity = disabled ? '0.55' : '1';
        button.style.cursor = disabled ? 'default' : 'pointer';
    }
</script>
</asp:Content>