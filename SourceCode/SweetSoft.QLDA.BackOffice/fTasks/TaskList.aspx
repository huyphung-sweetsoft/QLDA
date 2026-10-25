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

    .task-lock-notice {padding:10px 14px;background:#fff7ed;border:1px dashed #fdba74;border-radius:9px;display:flex;align-items:center;gap:8px;}
    .task-lock-notice i {font-size:16px;color:#f59e0b;}
    .task-lock-notice span {color:#9a3412;font-size:13px;font-weight:600;}

    .task-lockable {position:relative;}
    .task-lockable.is-locked {cursor:help;}
    .ql-local-cal-mode-wrap {flex:1 1 145px;min-width:0;position:relative;}
    .ql-local-cal-mode-wrap .ql-local-cal-mode {width:100%;height:100%;}
    .ql-local-cal-mode-wrap.is-locked {cursor:help;}
    .ql-local-cal-mode-wrap.is-locked > .ql-local-cal-mode {pointer-events:none;}
    .ql-task-lock-tip {position:fixed;z-index:2147483000;box-sizing:border-box;width:max-content;max-width:min(310px,calc(100vw - 24px));padding:11px 13px;
        background:#fff7ed;border:1px dashed #fdba74;border-radius:10px;box-shadow:0 12px 32px rgba(104,51,9,.19);
        color:#9a3412;pointer-events:none;opacity:0;visibility:hidden;transform:translateY(4px);transition:opacity .15s ease,transform .15s ease;}
    .ql-task-lock-tip.is-visible {opacity:1;visibility:visible;transform:translateY(0);}
    .ql-task-lock-tip-head {display:flex;align-items:center;gap:7px;margin-bottom:5px;font-size:12px;font-weight:800;color:#b45309;}
    .ql-task-lock-tip-head i {font-size:12px;color:#f59e0b;}
    .ql-task-lock-tip-text {font-size:12px;font-weight:650;line-height:1.5;overflow-wrap:anywhere;}

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

    /* Lịch dự kiến luôn mở. Thu form ~100px, lịch giữ bề ngang cũ. */
    .task-edit-wrapper {
        display:grid;grid-template-columns:minmax(0,650fr) minmax(0,530fr);
        width:100%;min-width:0;align-items:stretch;overflow:hidden;box-sizing:border-box;
    }
    .task-form-panel {min-width:0;width:auto;max-width:none;}
    .task-schedule-panel {min-width:0;width:auto;min-height:0;max-height:none;overflow:visible;background:#f8fafc;border-left:1px solid #e2e8f0;}
    .task-schedule-panel > .p-3 {width:100%;min-width:0;box-sizing:border-box;}
    #<%= mdlEditTask.ClientID %> .modal-dialog {
        width:min(1180px,calc(100vw - 32px))!important;
        max-width:calc(100vw - 32px)!important;
        /* Không tăng chiều cao popup theo nội dung vượt quá màn hình. */
        max-height:calc(100vh - 64px)!important;
        margin:40px auto 16px!important;
        transition:transform .3s ease!important;
    }
    #<%= mdlEditTask.ClientID %> .modal-content {
        display:flex;
        flex-direction:column;
        max-height:calc(100vh - 64px);
    }
    #<%= mdlEditTask.ClientID %> .modal-body {
        min-height:0;
        overflow-y:auto;
        overscroll-behavior:contain;
    }
    #pnlTaskForm .row > [class*="col-"] {min-width:0;}
    .task-edit-wrapper .task-context-card {padding:14px;}
    .task-edit-wrapper .task-code-prefix {flex-basis:92px;width:92px;min-width:92px;}
    .task-edit-wrapper .task-meta-controls {grid-template-columns:minmax(0,1fr) minmax(0,1fr);gap:10px;}
    /* Cột ngày thực tế luôn hiện; chỉ được chỉnh khi Đang thực hiện -> Hoàn thành. */
    .task-edit-wrapper .task-meta-controls {grid-template-columns:minmax(0,1.28fr) minmax(0,1.06fr) minmax(0,.96fr);gap:8px;}
    .task-edit-wrapper .task-meta-controls .priority-box {padding:5px 3px!important;font-size:12px!important;}
    .task-edit-wrapper .task-meta-controls #btnSmartStatus {padding:6px!important;font-size:12px!important;white-space:nowrap;}
    .task-edit-wrapper .ql-complete-date-shell .task-meta-label {white-space:nowrap;font-size:12px;}
    .task-edit-wrapper .ql-complete-date-input {width:100%;height:44px;min-height:44px;padding:8px 9px;border:1px solid #c5b4df;border-radius:9px;background:#fff;color:#34204f;font-size:13px;font-weight:700;font-family:inherit;box-sizing:border-box;}
    .task-edit-wrapper .ql-complete-date-input:focus {outline:none;border-color:#7c3aed;box-shadow:0 0 0 3px rgba(124,58,237,.12);}
    .task-edit-wrapper .ql-complete-date-input:disabled {background:#f8fafc;color:#64748b;border-color:#d8dee9;opacity:1;cursor:default;box-shadow:none;}
    .task-edit-wrapper .ql-complete-date-shell.is-locked {cursor:help;}
    .task-edit-wrapper .ql-complete-date-shell.is-locked .ql-complete-date-input {pointer-events:none;}
    .task-edit-wrapper .ql-complete-date-hint {display:block;margin-top:5px;font-size:11px;font-weight:650;line-height:1.35;color:#64748b;}
    .task-edit-wrapper .ql-complete-date-hint.is-on-time {color:#15803d;}
    .task-edit-wrapper .ql-complete-date-hint.is-late {color:#b91c1c;}
    .task-edit-wrapper .ql-late-reason-field {margin-top:12px;padding:11px 12px;border:1px solid #fecaca;border-radius:9px;background:#fff8f7;}
    .task-edit-wrapper .ql-late-reason-field label {display:flex;align-items:center;gap:7px;margin-bottom:7px;font-size:12.5px;font-weight:750;color:#b91c1c;}
    .task-edit-wrapper .ql-late-reason-field textarea {display:block;width:100%;min-height:65px;resize:vertical;border:1px solid #f0b7b7;border-radius:8px;padding:9px 10px;color:#334155;font-size:13px;line-height:1.45;box-sizing:border-box;}
    .task-edit-wrapper .ql-late-reason-field textarea:focus {outline:none;border-color:#dc2626;box-shadow:0 0 0 3px rgba(220,38,38,.09);}
    .task-edit-wrapper .ql-complete-error {display:none;margin-top:6px;color:#b91c1c;font-size:11.5px;font-weight:650;}
    .task-edit-wrapper .task-meta-controls .priority-box {min-width:0;}
    .task-edit-wrapper .task-meta-controls .d-flex {min-width:0;}
    .task-edit-wrapper .task-meta-controls #prioritySelector {flex-wrap:nowrap;min-width:0;}
    .task-edit-wrapper .task-meta-controls #prioritySelector > * {min-width:0;}
    .task-edit-wrapper .task-dates-row > [class*="col-"] {min-width:0;}
    .task-edit-wrapper .task-date-readonly {cursor:pointer!important;background:#f8fafc!important;border:1px dashed #cbd5e1!important;color:#475569!important;}
    .task-edit-wrapper .task-date-readonly:focus {box-shadow:none!important;}
    .task-edit-wrapper .task-date-readonly:disabled {cursor:not-allowed!important;}
    .task-edit-wrapper #pnlTaskSchedule .apt-detail-header {min-height:40px;}
    @media(max-width:1199.98px) {
        .task-edit-wrapper {grid-template-columns:minmax(0,1fr);}
        .task-schedule-panel {border-left:0;border-top:1px solid #e2e8f0;}
        #<%= mdlEditTask.ClientID %> .modal-dialog {width:min(750px,calc(100vw - 32px))!important;}
    }
    @media(max-width:575.98px) {
        .task-edit-wrapper .task-meta-controls {grid-template-columns:1fr;}
        .task-edit-wrapper .task-code-prefix {flex-basis:68px;width:68px;min-width:68px;}
    }

    /* ===================================================================
       GLOBAL LOADING OVERLAY (MỜ ẢO ĐỈNH CAO)
       =================================================================== */
    #global-loading-overlay { position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; background: rgba(255, 255, 255, 0.6); backdrop-filter: blur(3px); -webkit-backdrop-filter: blur(3px); z-index: 999999; display: flex; flex-direction: column; align-items: center; justify-content: center; opacity: 0; visibility: hidden; transition: opacity 0.3s ease, visibility 0.3s ease; }
    #global-loading-overlay.is-active { opacity: 1; visibility: visible; }
    .st-spinner-ring { width: 54px; height: 54px; border: 4px solid #ede9fe; border-top: 4px solid #7c3aed; border-radius: 50%; animation: stSpinRing 0.85s linear infinite; margin-bottom: 16px; box-shadow: 0 4px 12px rgba(124, 58, 237, 0.15); }
    .st-loading-text { color: #5b21b6; font-weight: 800; font-size: 15px; letter-spacing: 0.5px; animation: stPulseText 1.5s ease-in-out infinite; text-transform: uppercase; }
    @keyframes stSpinRing { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
    @keyframes stPulseText { 0%, 100% { opacity: 1; } 50% { opacity: 0.5; } }

    /* Lịch tương tác. Tím = công việc, vàng cam = lễ/nghỉ, mốc ngày = viền. */
    #local-task-timeline-container {min-width:0;}
    #local-task-timeline-container .ql-local-cal-toolbar {display:flex;align-items:center;justify-content:space-between;gap:8px;margin-bottom:10px;}
    #local-task-timeline-container .ql-local-cal-title {color:#4b1e82;font-size:14px;font-weight:800;text-align:center;}
    #local-task-timeline-container .ql-local-cal-nav {display:inline-grid;place-items:center;width:30px;height:30px;flex:0 0 30px;border:1px solid #ddd2ec;border-radius:8px;background:#fff;color:#54218d;cursor:pointer;}
    #local-task-timeline-container .ql-local-cal-nav:hover {background:#efe6fc;border-color:#bd9be2;}
    #local-task-timeline-container .ql-local-cal-grid {display:grid;grid-template-columns:repeat(7,minmax(0,1fr));gap:4px;}
    #local-task-timeline-container .ql-local-cal-dow {padding:6px 0;text-align:center;color:#725586;font-size:10.5px;font-weight:750;}
    #local-task-timeline-container .ql-local-cal-day {position:relative;display:flex;flex-direction:column;justify-content:space-between;gap:3px;min-height:57px;min-width:0;padding:5px 4px;border:1px solid #eeeaf4;border-radius:8px;background:#fff;color:#4d4661;text-align:left;font:inherit;transition:background .15s,border-color .15s,box-shadow .15s;overflow:hidden;}
    #local-task-timeline-container button.ql-local-cal-day {cursor:pointer;}
    #local-task-timeline-container .ql-local-cal-day:disabled {cursor:default;opacity:1;}
    #local-task-timeline-container .ql-local-cal-day.is-locked {color:#7b7b8b !important;border-color:#d7d7df !important;background:repeating-linear-gradient(-45deg,#f1f1f5 0 10px,#e5e5ec 10px 20px) !important;box-shadow:none !important;pointer-events:none;}
    #local-task-timeline-container .ql-local-cal-day.is-locked .ql-local-cal-number,
    #local-task-timeline-container .ql-local-cal-day.is-locked .ql-local-cal-caption {color:#767686 !important;}
    #local-task-timeline-container .ql-local-cal-day.is-locked .ql-local-cal-off-badge {border-color:#d3d3da !important;background:#efeff3 !important;color:#6f6f7d !important;}
    #local-task-timeline-container .ql-local-cal-day.is-outside {background:#faf9fc;color:#a5a0ae;}
    /* Chỉ làm mờ ngày THÁNG TRƯỚC nằm ở đầu lưới; ngày THÁNG SAU ở cuối lưới vẫn rõ màu. */
    #local-task-timeline-container .ql-local-cal-day.is-previous-month {opacity:.58;}
    #local-task-timeline-container .ql-local-cal-day.is-free {background:#fff;border-color:#e7e4ec;color:#3b2e4e;}
    #local-task-timeline-container .ql-local-cal-day.is-free .ql-local-cal-caption {color:#a49aaf;font-weight:600;}
    #local-task-timeline-container .ql-local-cal-day.is-busy {background:#eee7fa;border-color:#ceb5e9;color:#54218d;}
    #local-task-timeline-container .ql-local-cal-day.is-holiday {background:#fff7e7;border-color:#f6d9a9;color:#9c5112;}
    #local-task-timeline-container .ql-local-cal-day.is-weekend {background:#f1f6fd;border-color:#d9e5f5;color:#496886;}
    #local-task-timeline-container .ql-local-cal-day.is-weekend .ql-local-cal-number {color:#496886;}
    #local-task-timeline-container .ql-local-cal-day.is-start {box-shadow:inset 0 0 0 2px #7c3aed;}
    #local-task-timeline-container .ql-local-cal-day.is-end {box-shadow:inset 0 0 0 2px #2563eb;}
    #local-task-timeline-container .ql-local-cal-day.is-start.is-end {box-shadow:inset 0 0 0 2px #7c3aed,0 0 0 1px #2563eb;}
    #local-task-timeline-container button.ql-local-cal-day:not([aria-disabled="true"]):hover {z-index:1;border-color:#9b72ca;box-shadow:inset 0 0 0 1px #8b5cc6;}
    #local-task-timeline-container .ql-local-cal-day:focus-visible {outline:2px solid #6d28d9;outline-offset:1px;z-index:2;}
    #local-task-timeline-container .ql-local-cal-number {font-size:11.5px;font-weight:850;line-height:1.35;}
    #local-task-timeline-container .ql-local-cal-caption {overflow:hidden;text-overflow:ellipsis;white-space:nowrap;font-size:9.5px;font-weight:700;line-height:1.3;}
    #local-task-timeline-container .ql-local-cal-off-badge {display:inline-flex;gap:3px;align-items:center;justify-content:center;width:max-content;max-width:100%;padding:2px 4px;border:1px solid #f5d6a3;border-radius:5px;background:#ffedcf;color:#a6500b;font-size:9px;font-weight:800;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;}
    #local-task-timeline-container .ql-local-cal-off-badge.is-weekend {background:#e5eef9;border-color:#cfddf0;color:#526d8d;}
    .ql-local-holiday-tooltip {position:fixed;z-index:20000;max-width:min(280px,calc(100vw - 24px));padding:11px 13px;border:1px solid #ead2ad;border-radius:10px;background:#fffefa;box-shadow:0 10px 28px rgba(48,34,64,.18);color:#51405a;pointer-events:none;opacity:0;transform:translateY(3px);transition:opacity .12s ease,transform .12s ease;}
    .ql-local-holiday-tooltip.is-visible {opacity:1;transform:translateY(0);}
    .ql-local-holiday-tooltip-head {display:flex;align-items:center;gap:7px;margin-bottom:5px;color:#a8560b;font-size:11px;font-weight:850;}
    .ql-local-holiday-tooltip-head i {color:#c86a11;}
    .ql-local-holiday-tooltip-name {font-size:12px;font-weight:650;line-height:1.5;overflow-wrap:anywhere;}
    .ql-local-holiday-tooltip-date {margin-top:4px;color:#8f8096;font-size:10.5px;}
    .ql-local-holiday-tooltip::before {content:'';position:absolute;top:calc(50% - 6px);left:-7px;width:11px;height:11px;background:#fffefa;border-left:1px solid #ead2ad;border-bottom:1px solid #ead2ad;transform:rotate(45deg);}
    .ql-local-holiday-tooltip.is-left::before {left:auto;right:-7px;transform:rotate(225deg);}
    #local-task-timeline-container .ql-local-cal-legend {display:flex;flex-wrap:wrap;gap:6px 12px;margin-top:11px;color:#6f6480;font-size:10.5px;}
    #local-task-timeline-container .ql-local-cal-legend span {display:inline-flex;align-items:center;gap:5px;}
    #local-task-timeline-container .ql-local-cal-legend b {display:inline-block;width:9px;height:9px;border-radius:3px;vertical-align:middle;flex:0 0 9px;}
    #local-task-timeline-container .ql-local-cal-legend b.is-locked {background:repeating-linear-gradient(-45deg,#e6e6ed 0 4px,#cfcfd9 4px 8px);border:1px solid #cfcfd9;box-sizing:border-box;}
    #local-task-timeline-container .ql-local-cal-legend b.is-free {background:#fff;border:1px solid #d8d3df;box-sizing:border-box;}
    #local-task-timeline-container .ql-local-cal-empty {padding:18px 9px;text-align:center;color:#857c91;font-size:12px;}
    .ql-local-cal-actions {display:flex;flex-wrap:wrap;gap:7px;margin:8px 0 6px;}
    .ql-local-cal-mode {min-width:0;flex:1 1 145px;border:1px solid #d9cbe9;background:#fff;color:#5a397d;border-radius:8px;padding:7px 8px;font-size:12px;font-weight:750;cursor:pointer;}
    .ql-local-cal-mode.is-active {background:#f1e8fc;border-color:#a880d5;color:#57218e;box-shadow:inset 0 0 0 1px #d4b8f3;}
    .ql-local-cal-mode:disabled {opacity:.54;cursor:not-allowed;}
    .ql-local-cal-hint {min-height:33px;display:flex;align-items:center;gap:6px;margin:7px 0 0;color:#675979;font-size:11.5px;line-height:1.35;}
    .ql-local-cal-hint.is-error {color:#b45309;font-weight:650;}
    #qlLocalCalMessage:empty {display:none;}
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

                                        <%-- HTML input thay vì server control: tránh yêu cầu chỉnh designer.cs; giá trị vẫn gửi qua Request.Form. --%>
                                        <div class="task-meta-shell ql-complete-date-shell task-lockable" id="qlTaskCompleteDateWrap">
                                            <div class="task-meta-label"><i class="far fa-calendar-check"></i> Hoàn thành thực tế</div>
                                            <input type="date" id="qlTaskActualCompleteDate" name="qlTaskActualCompleteDate" class="ql-complete-date-input" disabled="disabled" aria-label="Ngày hoàn thành thực tế" />
                                            <span id="qlTaskCompleteDateHint" class="ql-complete-date-hint" aria-live="polite"></span>
                                            <span id="qlTaskCompleteDateError" class="ql-complete-error" role="alert"></span>
                                        </div>
                                    </div>
                                    <div class="ql-late-reason-field" id="qlTaskLateReasonWrap" style="display:none;">
                                        <label for="qlTaskLateReason"><i class="fas fa-exclamation-triangle"></i> Lý do hoàn thành trễ hạn <span class="text-danger">*</span></label>
                                        <textarea id="qlTaskLateReason" name="qlTaskLateReason" rows="2" disabled="disabled" placeholder="Nhập lý do hoàn thành trễ hạn..."></textarea>
                                        <span id="qlTaskLateReasonError" class="ql-complete-error" role="alert"></span>
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
                                    <div class="task-lock-notice">
                                        <i class="fas fa-lock"></i>
                                        <span>Ngày tháng và Trạng thái bị khóa do tổng hợp tự động từ task con.</span>
                                    </div>
                                </div>
                                <div id="divChildCompletedNotice" class="mb-3" style="display:none;">
                                    <div class="task-lock-notice">
                                        <i class="fas fa-check-circle"></i>
                                        <span>Ngày tháng đã bị khóa vì công việc đã hoàn thành, không thể chỉnh sửa thêm.</span>
                                    </div>
                                </div>
                                <div id="divChildInProgressNotice" class="mb-3" style="display:none;">
                                    <div class="task-lock-notice">
                                        <i class="fas fa-play-circle"></i>
                                        <span>Ngày bắt đầu đã bị khóa vì công việc đang thực hiện. Thời hạn và ngày kết thúc chỉ được điều chỉnh nếu quy tắc tiến độ cho phép.</span>
                                    </div>
                                </div>
                                
                                <div class="row g-3 mb-3 align-items-end task-dates-row">
                                    <div class="col-md-4">
                                        <div class="smart-field task-lockable" id="wrapNgayBatDau">
                                            <label><i class="far fa-calendar-alt"></i><%= GetResourceText(BackEndResourceKeys.START_DATE) %> <span class="text-danger ms-1">*</span></label>
                                            <asp:TextBox ID="txtEditNgayBatDau" runat="server" CssClass="form-control px-2 task-date-readonly" TextMode="Date"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-md-4">
                                        <div class="smart-field task-lockable" id="wrapThoiHan">
                                            <label><i class="fas fa-hourglass-half"></i><%= GetResourceText(BackEndResourceKeys.DURATION) %> <span class="text-danger ms-1">*</span></label>
                                            <div class="input-group">
                                                <asp:TextBox ID="txtEditThoiHan" runat="server" CssClass="form-control px-2" TextMode="Number" min="1"></asp:TextBox>
                                                <span class="input-group-text px-2">Ngày</span>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-md-4">
                                        <div class="smart-field">
                                            <label style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis;"><i class="far fa-calendar-check me-1 text-success"></i><%= GetResourceText(BackEndResourceKeys.EXPECTED_COMPLETION_DATE) %></label>
                                            <div class="form-control d-flex align-items-center readonly-success px-2">
                                                <asp:Label ID="lblNgayKetThuc" runat="server" CssClass="mb-0">--/--/----</asp:Label>
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
                        </div>

                        <!-- CỘT PHẢI: LỊCH BIỂU -->
                        <div id="pnlTaskSchedule" class="task-schedule-panel">
                            <div class="p-3">
                                <div class="apt-detail-header mb-3" style="display: flex; justify-content: space-between; align-items: center; border-bottom: 1px dashed #cbd5e1; padding-bottom: 10px;">
                                    <h6 class="m-0" style="color: #1e40af; font-weight: 700; font-size: 16px;"><i class="fas fa-calendar-alt me-2"></i>Lịch biểu dự kiến</h6>

                                </div>
                                <asp:HiddenField ID="hdfLocalScheduleJson" runat="server" />
                                <div class="ql-local-cal-actions" id="qlLocalCalActions">
                                    <div class="ql-local-cal-mode-wrap" id="qlWrapCalStart">
                                        <button type="button" class="ql-local-cal-mode is-active" data-select-boundary="start"><i class="far fa-calendar-alt me-1"></i> Chọn ngày bắt đầu</button>
                                    </div>
                                    <div class="ql-local-cal-mode-wrap" id="qlWrapCalEnd">
                                        <button type="button" class="ql-local-cal-mode" data-select-boundary="end"><i class="far fa-calendar-check me-1"></i> Chọn ngày kết thúc</button>
                                    </div>
                                </div>
                                <div class="ql-local-cal-hint" id="qlLocalCalMessage" role="status" aria-live="polite"></div>
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
                    <asp:LinkButton ID="btnSaveTask" runat="server" CssClass="btn btn-primary waves-effect waves-light" CausesValidation="false" OnClick="btnSaveTask_Click" OnClientClick="if (!qlValidateTaskCompletionBeforeSave()) return false; showGlobalLoading();">
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
    // LỊCH DỰ KIẾN: luôn mở, điều chỉnh hai đầu mốc ngay trên lịch.
    // Lịch & ngày kết thúc nhận trực tiếp từ CalculateTaskEndDate trên server.
    // =========================================================
    var qlTaskScheduleState = { mode: 'start', data: null, days: {}, tasks: [], year: 0, month: 0, requestId: 0, timer: null, initialized: false };
    var qlTaskScheduleApi = '<%= ResolveUrl("~/fTasks/TaskList.aspx/LayLichDuKienCongViec") %>';
    var qlTaskStartId = '<%= txtEditNgayBatDau.ClientID %>';
    var qlTaskDurationId = '<%= txtEditThoiHan.ClientID %>';
    var qlTaskEndId = '<%= txtEditNgayKetThuc.ClientID %>';
    var qlTaskEndLabelId = '<%= lblNgayKetThuc.ClientID %>';
    function qlGet(id) {return document.getElementById(id);}
    function qlDate(s) {var m=/^(\d{4})-(\d{2})-(\d{2})$/.exec(s||'');if(!m)return null;var d=new Date(+m[1],+m[2]-1,+m[3]);return d.getFullYear()===+m[1]&&d.getMonth()===+m[2]-1&&d.getDate()===+m[3]?d:null;}
    function qlKey(d) {return d.getFullYear()+'-'+('0'+(d.getMonth()+1)).slice(-2)+'-'+('0'+d.getDate()).slice(-2);}
    function qlTextDate(s) {var d=qlDate(s);return d?('0'+d.getDate()).slice(-2)+'/'+('0'+(d.getMonth()+1)).slice(-2)+'/'+d.getFullYear():'--/--/----';}
    function qlEscape(s) {return $('<div/>').text(s == null ? '' : String(s)).html().replace(/"/g,'&quot;');}
    function qlMessage(s,error) {$('#qlLocalCalMessage').toggleClass('is-error',!!error).text(s||'');}
    function qlCanStart() {var el=qlGet(qlTaskStartId);return !!el&&!el.disabled;}
    function qlCanEnd() {var el=qlGet(qlTaskDurationId);return !!el&&!el.disabled;}
    // Đọc metadata từ server (không dùng trạng thái vừa click nhưng chưa lưu).
    function qlGetTaskLockState() {
        var startEl=qlGet(qlTaskStartId);
        var phase=!!startEl && startEl.getAttribute('data-ql-task-phase')==='1';
        var hasChildren=!!startEl && startEl.getAttribute('data-ql-task-children')==='1';
        var originalStatus=startEl ? startEl.getAttribute('data-ql-task-original-status') : '';
        var completed=(originalStatus==='2'||originalStatus==='3');
        var doing=originalStatus==='1';
        var rollup=phase&&hasChildren;
        var state={rollup:rollup,completed:completed,inProgress:doing,
            startLocked:false,durationLocked:false,startReason:'',durationReason:''};
        if(phase&&hasChildren) {
            state.startReason='Ngày bắt đầu của giai đoạn đang chịu quy tắc tổng hợp tự động từ các công việc con.';
            state.durationReason='Thời hạn và ngày kết thúc được tổng hợp từ các công việc con, không thể sửa trực tiếp.';
        } else if(completed) {
            state.startReason='Công việc đã hoàn thành nên không thể chỉnh sửa ngày bắt đầu.';
            state.durationReason='Công việc đã hoàn thành nên không thể chỉnh sửa thời hạn hoặc ngày kết thúc.';
        } else if(doing) {
            state.startReason='Công việc đang thực hiện nên ngày bắt đầu đã bị khóa.';
            if(hasChildren) state.durationReason='Thời hạn được tổng hợp từ các công việc con, không thể sửa trực tiếp.';
        } else if(hasChildren) {
            state.durationReason='Thời hạn được tổng hợp từ các công việc con, nhưng ngày bắt đầu vẫn có thể thay đổi khi chưa thực hiện.';
        }
        return state;
    }
    // Không dùng các bộ nhớ disabled của nút lịch: chế độ start/end thay đổi động.
    function qlRememberBaseDisabled($el){
        if($el.length && typeof $el.data('ql-base-disabled')==='undefined') $el.data('ql-base-disabled',$el.prop('disabled'));
    }
    function qlApplyLockable($wrap, locked, reason){
        if(!$wrap.length)return;
        $wrap.toggleClass('is-locked',!!locked);
        if(locked) $wrap.attr('data-lock-reason',reason||'Tính năng này hiện bị khóa.');
        else $wrap.removeAttr('data-lock-reason');
    }
    function qlApplyTaskLockUI(){
        var state=qlGetTaskLockState();
        var $start=$('#'+qlTaskStartId),$duration=$('#'+qlTaskDurationId);
        qlRememberBaseDisabled($start);qlRememberBaseDisabled($duration);
        // C# đặt quyền gốc khi mở task; JS chỉ có thể siết chặt, không được mở khóa trái quyền server.
        state.startLocked=!!($start.data('ql-base-disabled') || state.rollup || state.completed || state.inProgress);
        state.durationLocked=!!($duration.data('ql-base-disabled') || state.completed || state.rollup);
        // Trạng thái đang thực hiện trên client chỉ dùng để diễn giải lock đã áp ở server.
        if(state.inProgress)state.startLocked=true;
        if(state.completed) {state.startLocked=true;state.durationLocked=true;}
        $start.prop('disabled',state.startLocked);
        $duration.prop('disabled',state.durationLocked);
        qlApplyLockable($('#wrapNgayBatDau'),state.startLocked,
            state.startReason || 'Ngày bắt đầu đang bị khóa theo quy tắc tiến độ hoặc quyền chỉnh sửa.');
        qlApplyLockable($('#wrapThoiHan'),state.durationLocked,
            state.durationReason || 'Thời hạn hiện không được phép chỉnh sửa.');
        var $startButton=$('#qlLocalCalActions [data-select-boundary="start"]');
        var $endButton=$('#qlLocalCalActions [data-select-boundary="end"]');
        $startButton.prop('disabled',state.startLocked);
        $endButton.prop('disabled',state.durationLocked);
        qlApplyLockable($('#qlWrapCalStart'),state.startLocked,
            state.startReason || 'Không thể chọn lại ngày bắt đầu vì ngày này đã bị khóa.');
        qlApplyLockable($('#qlWrapCalEnd'),state.durationLocked,
            state.durationReason || 'Không thể chọn ngày kết thúc vì thời hạn đang bị khóa.');
        // Mỗi lần chỉ hiện MỘT lý do khóa, giữ đúng quy tắc ưu tiên ban đầu.
        // Phase có con: tổng hợp tự động (kể cả khi đã hoàn thành).
        // Các trường hợp còn lại: hoàn thành > đang thực hiện.
        var showRollupNotice=state.rollup;
        var showCompletedNotice=!showRollupNotice&&state.completed;
        var showInProgressNotice=!showRollupNotice&&!showCompletedNotice&&state.inProgress;
        $('#divChildCompletedNotice').toggle(showCompletedNotice);
        $('#divChildInProgressNotice').toggle(showInProgressNotice);
        return state;
    }
    // Tooltip tông cam, nổi bên ngoài modal và nằm ở đúng vùng được hover.
    function qlShowLockTooltip(target){
        var reason=target.getAttribute('data-lock-reason');if(!reason)return;
        var $tip=$('#qlTaskLockTooltip');
        if(!$tip.length)$tip=$('<div id="qlTaskLockTooltip" class="ql-task-lock-tip" role="tooltip"/>').appendTo('body');
        $tip.removeClass('is-visible').empty()
            .append($('<div class="ql-task-lock-tip-head"/>').append('<i class="fas fa-lock" aria-hidden="true"></i>').append($('<span/>').text('Thông tin ràng buộc')))
            .append($('<div class="ql-task-lock-tip-text"/>').text(reason)).css({left:'0px',top:'0px'}).show();
        var rect=target.getBoundingClientRect(),w=$tip.outerWidth(),h=$tip.outerHeight();
        var modal=document.getElementById('<%= mdlEditTask.ClientID %>');
        var modalTop=modal?modal.getBoundingClientRect().top:0;
        // Nếu phía trên đụng header popup, hạ tooltip xuống dưới control.
        var top=rect.top-h-10;
        if(top<Math.max(12,modalTop+65))top=rect.bottom+10;
        if(top+h>window.innerHeight-12)top=Math.max(12,rect.top-h-10);
        top=Math.max(12,Math.min(top,window.innerHeight-h-12));
        var left=rect.left+(rect.width-w)/2;
        left=Math.max(12,Math.min(left,window.innerWidth-w-12));
        $tip.css({left:left+'px',top:top+'px'}).addClass('is-visible');
    }
    function qlHideLockTooltip(){$('#qlTaskLockTooltip').removeClass('is-visible').hide();}
    $(document).off('mouseenter.qlTaskLock focusin.qlTaskLock','[data-lock-reason]')
        .on('mouseenter.qlTaskLock focusin.qlTaskLock','[data-lock-reason]',function(){qlShowLockTooltip(this);});
    $(document).off('mouseleave.qlTaskLock focusout.qlTaskLock','[data-lock-reason]')
        .on('mouseleave.qlTaskLock focusout.qlTaskLock','[data-lock-reason]',qlHideLockTooltip);
    $(window).off('scroll.qlTaskLock resize.qlTaskLock').on('scroll.qlTaskLock resize.qlTaskLock',qlHideLockTooltip);
    function qlSetMode(m) {
        var lockState=qlApplyTaskLockUI();
        if(m==='start'&&!qlCanStart()&&qlCanEnd())m='end';
        if(m==='end'&&!qlCanEnd()&&qlCanStart())m='start';
        qlTaskScheduleState.mode=m;
        $('#qlLocalCalActions [data-select-boundary]').each(function(){
            var isStart=this.getAttribute('data-select-boundary')==='start';
            $(this).toggleClass('is-active',isStart?(m==='start'):(m==='end'))
                .prop('disabled',isStart?!qlCanStart():!qlCanEnd());
        });
        if(!qlCanStart()&&!qlCanEnd()) {
            qlMessage(lockState.rollup?'Lịch giai đoạn được tổng hợp từ các công việc con.':
                lockState.completed?'Công việc đã hoàn thành nên không thể chỉnh lại ngày trên lịch.':
                'Không thể chỉnh ngày do quyền hạn hoặc ràng buộc tiến độ.',false);
        } else if(m==='end') qlMessage('Chọn ngày kết thúc (ngày làm việc); thời hạn sẽ tự tính lại.',false);
        else qlMessage('Chọn ngày bắt đầu; hạn kết thúc sẽ tính lại theo lịch làm việc.',false);
    }
    function qlInitialSchedule() {
        var hidden=qlGet('<%= hdfLocalScheduleJson.ClientID %>');var model=null;
        try {model=JSON.parse(hidden&&hidden.value||'null');}catch(ex){}
        qlTaskScheduleState.data=model||{};
        qlTaskScheduleState.days=(model&&model.days)||{};
        qlTaskScheduleState.tasks=(model&&model.tasks)||[];
        qlTaskScheduleState.requestId++;
        var d=qlDate(qlGet(qlTaskStartId)&&qlGet(qlTaskStartId).value)||new Date();
        qlTaskScheduleState.year=d.getFullYear();qlTaskScheduleState.month=d.getMonth();
        qlSetMode(qlCanStart()?'start':'end');
        qlDrawScheduleMonth();
        qlRefreshSchedule(false);
    }
    function qlRefreshSchedule(chosenEnd) {
        if(qlTaskScheduleState.timer)clearTimeout(qlTaskScheduleState.timer);
        var start=qlGet(qlTaskStartId),duration=qlGet(qlTaskDurationId);
        if(!start||!duration||!qlDate(start.value)||!/^\d+$/.test(duration.value||'')||parseInt(duration.value,10)<1){qlMessage('Ngày bắt đầu hoặc thời hạn không hợp lệ.',true);return;}
        var st=qlTaskScheduleState,req=++st.requestId;
        qlMessage('Đang đồng bộ với lịch làm việc của dự án…',false);
        $.ajax({type:'POST',url:qlTaskScheduleApi,contentType:'application/json; charset=utf-8',dataType:'json',
            data:JSON.stringify({ngayBatDau:start.value,thoiHan:parseInt(duration.value,10),ngayKetThucChon:chosenEnd||'',nam:st.year,thang:st.month+1})
        }).done(function(response){
            if(req!==st.requestId)return;
            var r=response&&response.d?response.d:response;
            if(!r||!r.success){qlMessage(r&&r.message?r.message:'Không tính được ngày theo lịch làm việc.',true);return;}
            if(r.days) $.extend(st.days,r.days);
            if(r.duration!=null) duration.value=String(r.duration);
            if(r.endDate) {qlGet(qlTaskEndId).value=r.endDate;$(qlGet(qlTaskEndLabelId)).text(qlTextDate(r.endDate));}
            qlRefreshTaskCompletionLate();
            qlDrawScheduleMonth();
            qlSetMode(st.mode);
        }).fail(function(xhr){if(req!==st.requestId)return;qlMessage('Không kết nối được bộ tính lịch. Chưa thay đổi dữ liệu lưu; vui lòng thử lại.',true);});
    }
    // Chỉ gạch chéo do RÀNG BUỘC NGHIỆP VỤ cha/phụ thuộc, không theo
    // quyền sửa/trạng thái hoàn thành và không theo mốc bắt đầu đã chọn.
    // GetMinStartDate (server) lấy max(ngày cha bắt đầu, ngày phụ thuộc kết thúc + 1).
    function qlGetLockedDateReason(key, row) {
        var startEl=qlGet(qlTaskStartId);
        var minStart=(startEl&&startEl.getAttribute('min'))||'';
        // Ngày nào TRƯỚC mốc tiến độ đều bị sọc (kể cả lễ/nghỉ),
        // nhưng lễ/nghỉ SAU mốc vẫn giữ màu của chính nó.
        if(minStart&&key<minStart)
            return 'Ngoài giới hạn tiến độ: chỉ được bắt đầu từ '+qlTextDate(minStart)+'.';
        return '';
    }
    function qlHideHolidayTooltip() {
        $('.ql-local-holiday-tooltip').removeClass('is-visible').hide();
    }
    function qlPositionHolidayTooltip(target) {
        var $tip=$('.ql-local-holiday-tooltip'),$cell=$(target);
        if(!$tip.length||!$cell.length)return;
        var r=target.getBoundingClientRect(),w=$tip.outerWidth(),h=$tip.outerHeight();
        var left=r.right+10,isLeft=left+w>window.innerWidth-12;
        if(isLeft)left=r.left-w-10;
        left=Math.max(12,Math.min(left,window.innerWidth-w-12));
        var top=Math.max(12,Math.min(r.top+(r.height-h)/2,window.innerHeight-h-12));
        $tip.toggleClass('is-left',isLeft).css({left:left+'px',top:top+'px'});
    }
    // Tooltip nằm ngoài popup và ô lịch, tránh bị overflow:hidden cắt nội dung.
    $(document).off('mouseenter.qlHolidayTip focusin.qlHolidayTip','#local-task-timeline-container .ql-local-cal-day.is-holiday')
      .on('mouseenter.qlHolidayTip focusin.qlHolidayTip','#local-task-timeline-container .ql-local-cal-day.is-holiday',function(){
        var name=this.getAttribute('data-ql-holiday')||'';
        if(!name)return;
        var $tip=$('.ql-local-holiday-tooltip');
        if(!$tip.length)$tip=$('<div class="ql-local-holiday-tooltip" role="tooltip"/>').appendTo('body');
        $tip.removeClass('is-visible is-left').empty()
            .append($('<div class="ql-local-holiday-tooltip-head"><i class="fas fa-calendar-day"></i><span>Nghỉ lễ</span></div>'))
            .append($('<div class="ql-local-holiday-tooltip-name"/>').text(name))
            .append($('<div class="ql-local-holiday-tooltip-date"/>').text('Ngày '+qlTextDate(this.getAttribute('data-cal-date'))))
            .show();
        qlPositionHolidayTooltip(this);
        $tip.addClass('is-visible');
      });
    $(document).off('mouseleave.qlHolidayTip focusout.qlHolidayTip','#local-task-timeline-container .ql-local-cal-day.is-holiday')
      .on('mouseleave.qlHolidayTip focusout.qlHolidayTip','#local-task-timeline-container .ql-local-cal-day.is-holiday',qlHideHolidayTooltip);
    $(window).off('scroll.qlHolidayTip resize.qlHolidayTip').on('scroll.qlHolidayTip resize.qlHolidayTip',qlHideHolidayTooltip);

    function qlDrawScheduleMonth() {
        var container=$('#local-task-timeline-container'),st=qlTaskScheduleState;
        if(!container.length||!st.year)return;
        var year=st.year,month=st.month,first=new Date(year,month,1);
        var offset=(first.getDay()+6)%7,totalCells=Math.max(35,Math.ceil((offset+new Date(year,month+1,0).getDate())/7)*7);
        var start=(qlGet(qlTaskStartId)||{}).value||'',end=(qlGet(qlTaskEndId)||{}).value||'';
        var canSelect=st.mode==='start'?qlCanStart():qlCanEnd();
        var html='<div class="ql-local-cal-toolbar"><button type="button" class="ql-local-cal-nav" data-cal-step="-1" aria-label="Tháng trước"><i class="fas fa-chevron-left"></i></button><span class="ql-local-cal-title">Tháng '+(month+1)+'/'+year+'</span><button type="button" class="ql-local-cal-nav" data-cal-step="1" aria-label="Tháng sau"><i class="fas fa-chevron-right"></i></button></div><div class="ql-local-cal-grid">';
        ['T2','T3','T4','T5','T6','T7','CN'].forEach(function(n){html+='<div class="ql-local-cal-dow">'+n+'</div>';});
        for(var n=0;n<totalCells;n++) {
            var d=new Date(year,month,n-offset+1),key=qlKey(d),row=st.days[key];
            var selected=!!start&&!!end&&key>=start&&key<=end,isPreviousMonth=d<first;
            var working=row&&row.working===true,isHoliday=row&&row.status==='holiday',isWeekend=row&&row.status==='weekend';
            var tasks=st.tasks.filter(function(t){return t.start&&t.end&&key>=t.start&&key<=t.end;});
            var isBusy=working&&(selected||tasks.length>0);
            var status=!row?'is-outside':isHoliday?'is-holiday':isWeekend?'is-weekend':isBusy?'is-busy':'is-free';
            var caption=isHoliday?(row.text||'Ngày lễ'):isWeekend?'Cuối tuần':isBusy?(tasks.length?(tasks.length===1?'['+tasks[0].code+']':tasks.length+' CV'):'Có công việc'):row&&!working?'Ngày nghỉ':row?'Trống':'';
            var lockedReason=qlGetLockedDateReason(key,row);
            var locked=!!lockedReason;
            // Không thể click khác với bị sọc: lễ/cuối tuần trong vùng hợp lệ giữ màu riêng.
            var disabled=!canSelect||locked||(st.mode==='end'&&(!row||row.working!==true));
            var holidayName=isHoliday?(row.text||'Ngày nghỉ lễ'):'';
            var tip=(lockedReason||(isHoliday?'Nghỉ lễ':(row&&row.text?row.text:caption)))+(tasks.length?'\n'+tasks.map(function(t){return '['+t.code+'] '+t.name;}).join('\n'):'');
            // aria-disabled cho ngày không thể chọn: vẫn bắt được hover để hiện tooltip lễ.
            // Sự kiện click kiểm tra aria-disabled bên dưới, không dùng native disabled.
            html+='<button type="button" class="ql-local-cal-day '+status+(isPreviousMonth?' is-previous-month':'')+(key===start?' is-start':'')+(key===end?' is-end':'')+(locked?' is-locked':'')+'" data-cal-date="'+key+'"'+(isHoliday?' data-ql-holiday="'+qlEscape(holidayName)+'"':'')+((locked||isHoliday)?'':' title="'+qlEscape(tip||key)+'"')+' aria-label="Ngày '+qlTextDate(key)+(isHoliday?' - Nghỉ lễ: '+qlEscape(holidayName):'')+(locked?(' - '+lockedReason):'')+'" aria-disabled="'+(disabled?'true':'false')+'"'+(disabled?' tabindex="-1"':'')+'>';
            html+='<span class="ql-local-cal-number">'+d.getDate()+'</span>';
            if(isHoliday) html+='<span class="ql-local-cal-off-badge"><i class="fas fa-mug-hot"></i> Nghỉ lễ</span>';
            else if(isWeekend) html+='<span class="ql-local-cal-off-badge is-weekend">Cuối tuần</span>';
            else html+='<span class="ql-local-cal-caption">'+qlEscape(caption)+'</span>';
            html+='</button>';
        }
        html+='</div><div class="ql-local-cal-legend"><span><b style="background:#a17bd2"></b>Có công việc</span><span><b class="is-free"></b>Trống</span><span><b style="background:#f0bd68"></b>Nghỉ lễ</span><span><b style="background:#aac5e7"></b>Cuối tuần</span><span><b style="background:#7c3aed"></b>Ngày bắt đầu</span><span><b style="background:#2563eb"></b>Ngày kết thúc</span><span><b class="is-locked"></b>Ngoài giới hạn tiến độ</span></div>';
        qlHideHolidayTooltip();
        container.html(html);
    }
    $(document).off('click.qlScheduleSelect').on('click.qlScheduleSelect','#qlLocalCalActions [data-select-boundary]',function(){qlSetMode(this.getAttribute('data-select-boundary'));qlDrawScheduleMonth();});
    $(document).off('click.qlScheduleDate').on('click.qlScheduleDate','#local-task-timeline-container [data-cal-date]',function(){
        if(this.getAttribute('aria-disabled')==='true')return;
        var key=this.getAttribute('data-cal-date'),st=qlTaskScheduleState;
        if(st.mode==='start') {
            if(!qlCanStart())return;
            var min=(qlGet(qlTaskStartId)||{}).min;
            if(min&&key<min){qlMessage('Ngày bắt đầu không được trước '+qlTextDate(min)+' do ràng buộc công việc.',true);return;}
            qlGet(qlTaskStartId).value=key;
            qlDrawScheduleMonth();qlRefreshSchedule(false);
        }else{
            if(!qlCanEnd())return;
            if(key<(qlGet(qlTaskStartId)||{}).value){qlMessage('Ngày kết thúc không được trước ngày bắt đầu.',true);return;}
            if(!st.days[key]){qlMessage('Đang tải thông tin ngày làm việc. Vui lòng chọn lại sau.',true);qlRefreshSchedule(false);return;}
            if(st.days[key].working!==true){qlMessage('Ngày kết thúc phải là ngày làm việc theo lịch biểu chung.',true);return;}
            qlRefreshSchedule(key);
        }
    });
    $(document).off('click.qlScheduleNavigate').on('click.qlScheduleNavigate','#local-task-timeline-container [data-cal-step]',function(){
        var st=qlTaskScheduleState,d=new Date(st.year,st.month+parseInt(this.getAttribute('data-cal-step'),10),1);
        st.year=d.getFullYear();st.month=d.getMonth();qlDrawScheduleMonth();qlRefreshSchedule(false);
    });
    $(document).off('input.qlScheduleDuration').on('input.qlScheduleDuration','#'+qlTaskDurationId,function(){
        var st=qlTaskScheduleState;
        if(st.timer)clearTimeout(st.timer);
        st.timer=setTimeout(function(){qlRefreshSchedule(false);},260);
    });
    // HTML readonly vẫn gửi ngày qua WebForms postback; server quyết định quyền sửa.
    $(document).off('shown.bs.modal.qlScheduleAuto','[id="<%= mdlEditTask.ClientID %>"]')
      .on('shown.bs.modal.qlScheduleAuto','[id="<%= mdlEditTask.ClientID %>"]',function(){qlInitialSchedule();});
    $(document).off('hidden.bs.modal.qlScheduleAuto','[id="<%= mdlEditTask.ClientID %>"]')
      .on('hidden.bs.modal.qlScheduleAuto','[id="<%= mdlEditTask.ClientID %>"]',function(){qlTaskScheduleState.requestId++;if(qlTaskScheduleState.timer)clearTimeout(qlTaskScheduleState.timer);qlHideHolidayTooltip();$('#local-task-timeline-container').empty();});
    function qlTryInitVisible() {
        var modal=qlGet('<%= mdlEditTask.ClientID %>');
        if(modal&&($(modal).hasClass('show')||$(modal).is(':visible'))) qlInitialSchedule();
    }
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
            $lockedBtn.html('<span class="task-status-dot" style="background:' + color + '; box-shadow: 0 0 0 3px ' + bg + ';"></span> ' + text);
        } else {
            $locked.hide();
            $selector.show();

            $selector.empty();
            $ddl.find('option').each(function () {
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
                    $box.html('<span class="task-status-dot" style="background:' + color + '; box-shadow: 0 0 0 2px ' + selectedBg + ';"></span> ' + text);
                } else {
                    $box.css({ 'background': '#ffffff', 'color': '#64748b', 'border': '1px solid #e2e8f0' });
                    $box.html(text);
                    $box.hover(function () {
                        $(this).css({ 'background': hoverBg, 'color': color, 'border-color': border });
                    }, function () {
                        $(this).css({ 'background': '#ffffff', 'color': '#64748b', 'border-color': '#e2e8f0' });
                    });
                }

                $box.on('click', function () {
                    $ddl.val(val).trigger('change');
                });

                $selector.append($box);
            });
        }
    }

    // =========================================================
    // NGÀY HOÀN THÀNH THỰC TẾ VÀ LÝ DO TRỄ (TASKLIST)
    // =========================================================
    // Chỉ đổi UI; quyền và tính đúng hạn/trễ hạn được kiểm tra lại trong btnSaveTask_Click.
    function qlTaskCompletionOriginalStatus() {
        var el = qlGet(qlTaskStartId);
        return el ? (el.getAttribute('data-ql-task-original-status') || '') : '';
    }
    function qlTaskCompletionAllowed() {
        var $ddl = $('.task-status-select');
        return qlTaskCompletionOriginalStatus() === '1' && $ddl.length && !$ddl.prop('disabled') && ($ddl.val() === '2' || $ddl.val() === '3');
    }
    function qlUpdateTaskCompletionFields() {
        var active = qlTaskCompletionAllowed();
        var originalStatus = qlTaskCompletionOriginalStatus();
        var $wrap = $('#qlTaskCompleteDateWrap');
        var $date = $('#qlTaskActualCompleteDate'), $reasonWrap = $('#qlTaskLateReasonWrap'), $reason = $('#qlTaskLateReason');
        if (!$date.length || !$wrap.length) return;
        var wasActive = $wrap.attr('data-ql-active') === '1';
        var startEl = qlGet(qlTaskStartId);
        // Ngày của task đã hoàn thành phải lấy từ CSDL, không lấy từ input cũ hoặc ngày hiện tại.
        var savedActualDate = startEl ? (startEl.getAttribute('data-ql-task-actual-date') || '') : '';
        var isCompleted = originalStatus === '2' || originalStatus === '3';
        var today = qlKey(new Date());
        var startDate = startEl ? (startEl.value || '') : '';

        $wrap.attr('data-ql-active', active ? '1' : '0');
        $date.prop('disabled', !active).attr('max', today);
        if (qlDate(startDate)) $date.attr('min', startDate); else $date.removeAttr('min');
        $('.ql-complete-error').hide().text('');

        if (!active) {
            // Task hoàn thành: chỉ HIỂN THỊ ngày hoàn thành thực tế đã lưu. Các task khác: để trống.
            $date.val(isCompleted && qlDate(savedActualDate) ? savedActualDate : '');
            $reason.val('').prop('disabled', true);
            $reasonWrap.hide();
            var $hint = $('#qlTaskCompleteDateHint');
            $hint.removeClass('is-late is-on-time').text('');
            var plannedEl = qlGet(qlTaskEndId);
            var planned = plannedEl ? plannedEl.value : '';
            if (isCompleted && qlDate(savedActualDate) && qlDate(planned)) {
                var late = savedActualDate > planned;
                $hint.text(late ? 'Trễ hạn' : 'Đúng hạn')
                    .toggleClass('is-late', late).toggleClass('is-on-time', !late);
            }
        } else {
            if (!wasActive || !qlDate($date.val())) { $date.val(today); $reason.val(''); }
            qlRefreshTaskCompletionLate();
        }

        // Dùng nguyên tooltip cam "Thông tin ràng buộc" đang có trên trang.
        var lockReason = isCompleted
            ? 'Công việc đã hoàn thành nên không thể chỉnh sửa ngày hoàn thành thực tế.'
            : originalStatus === '0'
                ? 'Công việc chưa bắt đầu. Chỉ được chọn ngày thực tế khi chuyển từ Đang thực hiện sang Hoàn thành.'
                : 'Chỉ được chọn ngày thực tế khi chuyển công việc Đang thực hiện sang Hoàn thành.';
        if (!active && !isCompleted && originalStatus !== '0' && originalStatus !== '1')
            lockReason = 'Không thể chỉnh sửa ngày hoàn thành thực tế ở trạng thái hiện tại.';
        qlApplyLockable($wrap, !active, lockReason);
        if (active) qlHideLockTooltip();
    }
    function qlRefreshTaskCompletionLate() {
        if (!qlTaskCompletionAllowed()) return;
        var actual = $('#qlTaskActualCompleteDate').val();
        var plannedField = qlGet(qlTaskEndId), expected = plannedField ? plannedField.value : '';
        var isLate = !!qlDate(actual) && !!qlDate(expected) && actual > expected;
        var $hint = $('#qlTaskCompleteDateHint');
        if (!qlDate(actual)) $hint.text('Vui lòng chọn ngày hợp lệ.').removeClass('is-late is-on-time');
        else if (!qlDate(expected)) $hint.text('Đang cập nhật ngày kết thúc dự kiến…').removeClass('is-late is-on-time');
        else $hint.text(isLate ? ('Trễ hạn · Dự kiến ' + qlTextDate(expected)) : 'Đúng hạn')
            .toggleClass('is-late', isLate).toggleClass('is-on-time', !isLate);
        $('#qlTaskLateReasonWrap').toggle(isLate);
        $('#qlTaskLateReason').prop('disabled', !isLate);
        if (!isLate) { $('#qlTaskLateReason').val(''); $('#qlTaskLateReasonError').hide().text(''); }
        $('#qlTaskCompleteDateError').hide().text('');
    }
    function qlValidateTaskCompletionBeforeSave() {
        if (!qlTaskCompletionAllowed()) return true;
        var value = $('#qlTaskActualCompleteDate').val();
        var startEl = qlGet(qlTaskStartId), start = startEl ? startEl.value : '';
        var today = qlKey(new Date());
        if (!qlDate(value) || (qlDate(start) && value < start) || value > today) {
            $('#qlTaskCompleteDateError').text('Ngày thực tế phải từ ngày bắt đầu đến hôm nay.').show();
            $('#qlTaskActualCompleteDate').trigger('focus');
            return false;
        }
        var plannedEl = qlGet(qlTaskEndId), planned = plannedEl ? plannedEl.value : '';
        if (qlDate(planned) && value > planned && !$('#qlTaskLateReason').val().trim()) {
            $('#qlTaskLateReasonError').text('Vui lòng nhập lý do hoàn thành trễ hạn.').show();
            $('#qlTaskLateReason').trigger('focus');
            return false;
        }
        return true;
    }
    $(document).off('change.qlTaskComplete input.qlTaskComplete', '#qlTaskActualCompleteDate')
        .on('change.qlTaskComplete input.qlTaskComplete', '#qlTaskActualCompleteDate', qlRefreshTaskCompletionLate);
    $(document).off('input.qlTaskComplete', '#qlTaskLateReason')
        .on('input.qlTaskComplete', '#qlTaskLateReason', function () { $('#qlTaskLateReasonError').hide().text(''); });

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
        qlApplyTaskLockUI();
        qlUpdateTaskCompletionFields();
    }

    function toggleSmartStatus() {
        var $ddl = $('.task-status-select');
        if ($ddl.prop('disabled') || $ddl.attr('disabled') === 'disabled') return;

        var currentVal = $ddl.val();
        if (currentVal === '0') {
            $ddl.val('1');
        } else if (currentVal === '1') {
            // Không chuyển 0 -> 1 -> 2 trong cùng lần sửa khi trạng thái gốc chưa bắt đầu.
            if (qlTaskCompletionOriginalStatus() !== '1') {
                qlMessage('Hãy lưu trạng thái Đang thực hiện trước khi xác nhận hoàn thành.', true);
                return;
            }
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
        qlApplyTaskLockUI();
        qlTryInitVisible();
    });

    if (window.Sys && Sys.Application) {
        Sys.Application.add_load(function () {
            initSmartStatus();
            initSmartPriority();
            processMaCvUI();
            processPhaseUI();
            processParentUI();
            qlApplyTaskLockUI();
            setTimeout(qlTryInitVisible, 80);
        });

        // Tự động tắt Loading Overlay khi Server xử lý xong Data
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            hideGlobalLoading();
        });
    }

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