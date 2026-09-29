<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlTask" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<%@ Register Src="~/fTasks/Controls/CtrlChonNhanVienTask.ascx" TagPrefix="SweetSoft" TagName="CtrlChonNhanVienTask" %>
<%@ Register Src="~/fTasks/Controls/CtrlXemNhanVienTask.ascx" TagPrefix="SweetSoft" TagName="CtrlXemNhanVienTask" %>
<%@ Register Src="~/fTasks/Controls/CtrlSwapPhase.ascx" TagPrefix="SweetSoft" TagName="CtrlSwapPhase" %>
<%@ Register Src="~/fTasks/Controls/CtrlAddPhase.ascx" TagPrefix="SweetSoft" TagName="CtrlAddPhase" %>
<%@ Register Src="~/fTasks/Controls/CtrlAddSubTask.ascx" TagPrefix="SweetSoft" TagName="CtrlAddSubTask" %>
<%@ Register Src="~/fTasks/Controls/CtrlViewTaskDetail.ascx" TagPrefix="SweetSoft" TagName="CtrlViewTaskDetail" %>
<%@ Register Src="~/fTasks/Controls/CtrlFastCompleteTask.ascx" TagPrefix="SweetSoft" TagName="CtrlFastCompleteTask" %>
<%@ Register Src="~/fTasks/Controls/CtrlStartTask.ascx" TagPrefix="SweetSoft" TagName="CtrlStartTask" %>
<style>
    .card-body { overflow-x: auto !important; -webkit-overflow-scrolling: touch; }
    
    .table-task-grid { 
        table-layout: fixed !important; 
        word-wrap: break-word; 
        min-width: 1000px !important;  
    }

    /* Giữ nguyên cơ chế phân bổ width hiện tại của từng TemplateField. */

    .table-task-grid th:first-child,
    .table-task-grid td:first-child {
        display: none !important;
    }

    /*
       THEAD: KHÔNG override tại đây.
       GridviewExtension/corporate stylesheet + sticky behavior của hệ thống
       sẽ tự xử lý phần header. Điều này giúp CtrlTask dùng cùng header như CtrlDuAn.
       Các HeaderStyle/ItemStyle của từng TemplateField giữ nguyên width hiện tại.
    */

    /* Chỉ áp nowrap cho BODY; để THEAD tự kế thừa style corporate. */
    .table-task-grid tbody td {
        white-space: nowrap;
    }

    /*
       THEAD: chỉ bổ sung khả năng tự xuống dòng cho tiêu đề dài.
       Không thay đổi màu, font, border, padding hoặc width của corporate header.
    */
    .table-task-grid thead th {
        white-space: normal !important;
        word-break: normal !important;
        overflow-wrap: break-word;
    }

    .table-task-grid tbody td:nth-child(2) {
        white-space: normal !important;
        word-break: break-word;
    }

    .btn-swap-custom { background-color: #ffffff !important; color: #64748b !important; border: 1px solid #cbd5e1 !important; border-radius: 4px !important; font-weight: 500 !important; transition: all 0.2s ease !important; padding: 5px 12px !important; }
    .btn-swap-custom i { color: #4b1c71 !important;  }
    .btn-swap-custom:hover { background-color: #f1f5f9 !important; color: #475569 !important; border-color: #94a3b8 !important; box-shadow: 0 2px 4px rgba(0,0,0,0.05) !important; }
    
    .avatar-group { display: inline-flex !important; align-items: center; justify-content: center; gap: 6px !important; flex-wrap: nowrap !important; white-space: nowrap !important; }  
    .avatar-stack-container { display: flex; align-items: center; }    
    .avatar-circle { width: 28px; height: 28px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 700; color: #ffffff; border: 2px solid #ffffff; margin-left: -8px; position: relative; z-index: 1; box-shadow: 0 1px 2px rgba(0,0,0,0.1); }    
    .avatar-circle:first-child { margin-left: 0; }    
    .avatar-more { background-color: #f1f5f9; color: #475569; border-color: #cbd5e1; z-index: 0; font-weight: 800; font-size: 10px; }    
    
    .btn-assign-task { width: 26px; height: 26px; border-radius: 6px; background-color: #2563eb; color: white; display: flex; align-items: center; justify-content: center; border: none; cursor: pointer; text-decoration: none; font-size: 12px; transition: background 0.2s, transform 0.1s; flex-shrink: 0;  }
    .btn-assign-task:hover { background-color: #1d4ed8; color: white; transform: scale(1.05); }
    .btn-assign-task.view-only { background-color: #64748b; }
    .btn-assign-task.view-only:hover { background-color: #475569; }

    .btn-add-subtask-right { background-color: #eff6ff !important; color: #2563eb !important; border: 1px dashed #93c5fd !important; width: 24px; height: 24px; border-radius: 6px; font-size: 11px; display: inline-flex; align-items: center; justify-content: center; transition: all 0.2s ease; text-decoration: none; flex-shrink: 0; }
    .btn-add-subtask-right:hover { background-color: #2563eb !important; color: #ffffff !important; border-color: #2563eb !important; }

    .sched-day-card { position: relative; cursor: pointer; overflow: visible !important; -webkit-user-select: none; user-select: none; }
    .sd-header { border-radius: 5px 5px 0 0; }
    .sd-body { border-radius: 0 0 5px 5px; }
    .custom-task-tooltip { position: fixed !important; z-index: 999999 !important; background: #ffffff; color: #334155; border: 1px solid #cbd5e1; border-radius: 10px; padding: 11px 14px; min-width: 180px; max-width: 420px; white-space: normal; word-break: break-word; overflow-wrap: anywhere; box-shadow: 0 10px 30px rgba(15, 23, 42, 0.18); font-size: 12px; line-height: 1.5; opacity: 0; visibility: hidden; pointer-events: none; transition: opacity 0.2s ease; }
    .custom-task-tooltip::after { content: ''; position: absolute; top: 100%; left: 50%; margin-left: -6px; border-width: 6px; border-style: solid; border-color: #ffffff transparent transparent transparent; }
    .sched-day-card:hover .custom-task-tooltip, .sched-day-card.show-tooltip .custom-task-tooltip { opacity: 1; visibility: visible; pointer-events: auto; }
    .tooltip-task-list { list-style: none; margin: 0; padding: 0; text-align: left; }
    .tooltip-task-list li { margin: 0; padding: 7px 0; border-bottom: 1px solid #e2e8f0; color: #334155; }
    .tooltip-task-list li:last-child { border-bottom: none; padding-bottom: 0; }
    .tooltip-task-list li:first-child { padding-top: 0; }
    .t-code { display: inline-block; color: #2563eb; font-weight: 700; margin-right: 6px; }

    /* ===================================================================
       2 NÚT CÔNG CỤ: NHẸ HƠN, GỌN HƠN, VẪN GIỮ ĐÚNG MÀU CHỨC NĂNG
       - Trễ hạn: đỏ nhạt + outline; active mới chuyển đỏ đậm
       - Thu gọn: tím nhạt + outline; active mới chuyển tím đậm
       - Không ảnh hưởng Search / Add new / các control dùng chung
       =================================================================== */
    .btn-filter-overdue,
    .btn-tool-folder {
        border-radius: 7px;
        padding: 6px 12px;
        min-height: 36px;
        font-size: 12.5px;
        font-weight: 600;
        line-height: 1.2;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: 6px;
        cursor: pointer;
        transition: background-color 0.2s ease, border-color 0.2s ease,
                    color 0.2s ease, box-shadow 0.2s ease, transform 0.15s ease;
        box-shadow: none;
    }

    .btn-filter-overdue:hover,
    .btn-tool-folder:hover {
        transform: translateY(-1px);
    }

    /* 1. Trễ hạn - mặc định nhẹ, chỉ nổi mạnh khi đang filter */
    .btn-filter-overdue {
        background-color: #fff1f2 !important;
        color: #e11d48 !important;
        border: 1px solid #fda4af !important;
    }

    .btn-filter-overdue:hover {
        background-color: #ffe4e6 !important;
        color: #be123c !important;
        border-color: #fb7185 !important;
        box-shadow: 0 2px 6px rgba(225, 29, 72, 0.12);
    }

    .btn-filter-overdue.active-filter {
        background-color: #f43f5e !important;
        color: #ffffff !important;
        border-color: #f43f5e !important;
        box-shadow: 0 2px 6px rgba(244, 63, 94, 0.22);
    }

    /* Số lượng trễ hạn: tạo điểm nhấn nhỏ thay vì dấu ngoặc */
    .btn-filter-overdue .overdue-count {
        min-width: 20px;
        height: 20px;
        padding: 0 5px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        border-radius: 10px;
        background-color: #fecdd3;
        color: #be123c;
        font-size: 11px;
        font-weight: 700;
        line-height: 1;
    }

    .btn-filter-overdue.active-filter .overdue-count {
        background-color: rgba(255, 255, 255, 0.22);
        color: #ffffff;
    }

    /* 2. Thu gọn / Mở rộng - tím nhạt, đồng bộ màu thương hiệu */
    .btn-tool-folder {
        background-color: #f5f3ff !important;
        color: #6d28d9 !important;
        border: 1px solid #c4b5fd !important;
    }

    .btn-tool-folder:hover {
        background-color: #ede9fe !important;
        color: #5b21b6 !important;
        border-color: #a78bfa !important;
        box-shadow: 0 2px 6px rgba(109, 40, 217, 0.12);
    }

    .btn-tool-folder.active-filter {
        background-color: #8b5cf6 !important;
        color: #ffffff !important;
        border-color: #8b5cf6 !important;
        box-shadow: 0 2px 6px rgba(139, 92, 246, 0.22);
    }

    .row-overdue-bg > td { background-color: #fef2f2 !important; transition: background-color 0.2s ease; }
    .table-hover > tbody > tr.row-overdue-bg:hover > td { background-color: #fee2e2 !important; }
    .row-warning-bg > td { background-color: #fffbeb !important; transition: background-color 0.2s ease; }
    .table-hover > tbody > tr.row-warning-bg:hover > td { background-color: #fef3c7 !important; } 

    .task-context-card { background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 12px 16px; margin-bottom: 20px; }
    .task-context-breadcrumb { font-size: 13px; color: #475569; font-weight: 500; display: flex; align-items: center; gap: 8px; margin-top: 8px; padding-top: 8px; border-top: 1px dashed #cbd5e1; }
    .badge-code { font-size: 16px; font-weight: 800; color: #1e40af; background: #dbeafe; padding: 4px 10px; border-radius: 6px; display: inline-block; }
    
    .badge-pill-custom { padding: 4px 8px !important; border-radius: 4px !important; font-size: 11px !important; font-weight: 600 !important; display: inline-block !important; white-space: nowrap !important; line-height: 1.2 !important; }
    .badge-status-doing { background-color: #e0f2fe !important; color: #0369a1 !important; border: 1px solid #bae6fd !important; }
    .badge-status-todo  { background-color: #f1f5f9 !important; color: #475569 !important; border: 1px solid #cbd5e1 !important; }
    .badge-status-done  { background-color: #dcfce7 !important; color: #15803d !important; border: 1px solid #bbf7d0 !important; }
    .badge-pri-low  { background-color: #e0f2fe !important; color: #0369a1 !important; border: 1px solid #bae6fd !important; font-weight: 600 !important; }
    .badge-pri-med  { background-color: #fef3c7 !important; color: #b45309 !important; border: 1px solid #fde68a !important; font-weight: 600 !important; }
    .badge-pri-high { background-color: #fee2e2 !important; color: #dc2626 !important; border: 1px solid #fca5a5 !important; font-weight: 700 !important; }
    .table-task-grid td:last-child { background-color: #fffbeb !important; border-left: 1px solid #fef08a !important; }

    /* Hiệu ứng Hover chuyển trạng thái thành nút xác nhận nhanh */
    .badge-status-btn {
        position: relative;
        cursor: pointer;
        overflow: hidden;
        display: inline-flex !important;
        justify-content: center;
        align-items: center;
        min-width: 100px;
        text-decoration: none !important;
        vertical-align: middle;
    }
    .badge-status-btn .status-normal {
        display: inline-block;
        transition: transform 0.25s cubic-bezier(0.4, 0, 0.2, 1), opacity 0.25s ease;
        width: 100%;
    }
    .badge-status-btn .status-hover {
        position: absolute;
        top: 0; left: 0; right: 0; bottom: 0;
        display: flex;
        align-items: center;
        justify-content: center;
        transform: translateY(100%);
        opacity: 0;
        transition: transform 0.25s cubic-bezier(0.4, 0, 0.2, 1), opacity 0.25s ease;
        background-color: #15803d !important;
        color: white !important;
        border-radius: 4px;
        font-weight: 700;
        font-size: 11px;
    }
    .badge-status-btn:hover .status-normal {
        transform: translateY(-100%);
        opacity: 0;
    }
    .badge-status-btn:hover .status-hover {
        transform: translateY(0);
        opacity: 1;
    }
    .badge-status-btn:hover {
        border-color: #15803d !important;
        box-shadow: 0 3px 6px rgba(21, 128, 61, 0.3);
    }
    
    .badge-status-btn.btn-start-task .status-hover {
        background-color: #2563eb !important;
        color: white !important;
    }
    .badge-status-btn.btn-start-task:hover {
        border-color: #2563eb !important;
        box-shadow: 0 3px 6px rgba(37, 99, 235, 0.3);
    }

    /* Định dạng cây Task con và Phase (Không làm vỡ layout bảng) */
    .task-sub-box { display: block; padding: 3px 0; line-height: 1.55; }
    .task-sub-code { font-weight: 700 !important; color: #334155 !important; margin-right: 2px; }
    .task-tree-branch { color: #94a3b8 !important; margin-right: 5px; user-select: none; font-family: monospace; font-size: 13px; }

    .task-phase-box { 
        background: linear-gradient(90deg, #f3e8ff 0%, #faf5ff 72%, #ffffff 100%) !important;
        border-left: 4px solid #7c3aed !important; 
        padding: 8px 12px !important; 
        border-radius: 0 7px 7px 0 !important;
        display: block !important;
        width: 100% !important;
        box-sizing: border-box;
    }
    .task-phase-text { 
        font-size: 15px !important; 
        font-weight: 700 !important; 
        color: #4c1d95 !important; 
        letter-spacing: .05px;
    }
    .task-phase-box i { 
        color: #6d28d9 !important; 
        font-size: 15px !important; 
        margin-right: 6px;
    }

    /* ================================================================
       POLISH UI - CHỈ ÁP DỤNG CHO PHẦN BODY CỦA BẢNG TASK
       Không đụng THEAD, cột ACTION và các control dùng chung của công ty.
       ================================================================ */

    /* Nhịp dòng gọn và dễ quét hơn */
    .table-task-grid tbody td:not(:last-child) {
        padding: 10px 12px !important;
        vertical-align: middle !important;
        font-size: 13px;
        color: #334155;
        line-height: 1.45;
    }

    /* Cột tên công việc là vùng thông tin chính */
    .table-task-grid tbody td:nth-child(2) {
        padding-top: 9px !important;
        padding-bottom: 9px !important;
    }

    .table-task-grid tbody td:nth-child(2) .text-dark {
        color: #27364a !important;
    }

    .table-task-grid tbody td:nth-child(2) .fw-semibold {
        font-weight: 600 !important;
    }

    /* Căn số/ngày thẳng hàng */
    .table-task-grid tbody td:nth-child(3),
    .table-task-grid tbody td:nth-child(4),
    .table-task-grid tbody td:nth-child(5),
    .table-task-grid tbody td:nth-child(6),
    .table-task-grid tbody td:nth-child(7),
    .table-task-grid tbody td:nth-child(8) {
        font-variant-numeric: tabular-nums;
    }

    .table-task-grid tbody td:nth-child(3),
    .table-task-grid tbody td:nth-child(4),
    .table-task-grid tbody td:nth-child(5),
    .table-task-grid tbody td:nth-child(6) {
        color: #475569;
    }

    .table-task-grid tbody td:nth-child(6) {
        font-weight: 600;
        color: #334155;
    }

    /* Phụ thuộc: rõ ràng nhưng không lấn át tên task */
    .table-task-grid tbody td:nth-child(8) {
        color: #475569;
        font-weight: 600;
    }

    /* Trạng thái: tạo khoảng thở mà không đổi kiểu nút/badge hiện có */
    .table-task-grid tbody td:nth-child(7) {
        padding-left: 8px !important;
        padding-right: 8px !important;
    }

    .badge-pill-custom {
        min-height: 25px;
        box-sizing: border-box;
        letter-spacing: .05px;
    }

    /* Avatar được căn giữa ổn định, không thay đổi nút assign */
    .avatar-group {
        min-height: 30px;
    }

    /* Dòng cảnh báo vẫn giữ nguyên màu quy ước */
    .row-overdue-bg > td:not(:last-child),
    .row-warning-bg > td:not(:last-child) {
        color: #475569;
    }

    /* ================================================================
       DRAG & DROP GIAI ĐOẠN
       - Chỉ áp dụng cho Phase (root task / level 1)
       - Không đụng THEAD, ACTION hay các control dùng chung
       ================================================================ */
    .table-task-grid tbody tr.phase-draggable-row > td:nth-child(2) {
        cursor: grab;
    }

    .table-task-grid tbody tr.phase-draggable-row.phase-dragging > td:nth-child(2) {
        cursor: grabbing;
    }

    .table-task-grid tbody tr.phase-dragging {
        opacity: 0.58;
        transition: opacity .18s ease, transform .18s ease, filter .18s ease;
    }

    .table-task-grid tbody tr.phase-dragging .task-phase-box {
        transform: translateY(-1px);
    }

    .table-task-grid tbody tr.phase-dragging .task-phase-box {
        filter: saturate(0.92);
    }

    /*
       Khi đang đưa chuột vào một Phase:
       - Không còn vạch liền ở trên / dưới từng ô.
       - Không vẽ border riêng cho từng cell.
       - Một lớp overlay duy nhất sẽ ôm toàn bộ row.
       - Các nét đứt chạy ngang như băng xích, nhẹ và liên tục.
    */
    .table-task-grid tbody tr.phase-drag-target {
        position: relative;
    }

    .table-task-grid tbody tr.phase-drag-target > td:not(:first-child) {
        background-color: rgba(124, 58, 237, .018) !important;
        transition: background-color .16s ease;
    }

    .table-task-grid tbody tr.phase-drag-noop > td:not(:first-child) {
        background-color: rgba(100, 116, 139, .028) !important;
    }

    .phase-row-border-overlay.noop {
        background-image:
            repeating-linear-gradient(90deg, rgba(100, 116, 139, .72) 0 8px, transparent 8px 15px),
            repeating-linear-gradient(0deg, rgba(100, 116, 139, .72) 0 8px, transparent 8px 15px),
            repeating-linear-gradient(90deg, rgba(100, 116, 139, .72) 0 8px, transparent 8px 15px),
            repeating-linear-gradient(0deg, rgba(100, 116, 139, .72) 0 8px, transparent 8px 15px);
        filter: drop-shadow(0 0 4px rgba(100, 116, 139, .10));
    }

    .phase-row-border-overlay {
        position: fixed;
        z-index: 99990;
        pointer-events: none;
        box-sizing: border-box;
        border-radius: 5px;
        background-image:
            repeating-linear-gradient(90deg, rgba(124, 58, 237, .96) 0 8px, transparent 8px 15px),
            repeating-linear-gradient(0deg, rgba(124, 58, 237, .96) 0 8px, transparent 8px 15px),
            repeating-linear-gradient(90deg, rgba(124, 58, 237, .96) 0 8px, transparent 8px 15px),
            repeating-linear-gradient(0deg, rgba(124, 58, 237, .96) 0 8px, transparent 8px 15px);
        background-size: calc(100% + 15px) 2px, 2px calc(100% + 15px), calc(100% + 15px) 2px, 2px calc(100% + 15px);
        background-position: 0 0, 100% 0, 0 100%, 0 0;
        background-repeat: no-repeat;
        filter: drop-shadow(0 0 5px rgba(124, 58, 237, .12));
        opacity: 0;
        transition: opacity .14s ease, top .08s linear, left .08s linear, width .08s linear, height .08s linear;
        animation: phaseRowTread 0.95s linear infinite;
    }

    .phase-row-border-overlay.show {
        opacity: 1;
    }

    .phase-row-border-overlay.noop {
        opacity: .9;
    }

    @keyframes phaseRowTread {
        0% {
            background-position: 0 0, 100% 0, 0 100%, 0 0;
        }
        100% {
            background-position: 15px 0, 100% 15px, -15px 100%, 0 -15px;
        }
    }

    @media (prefers-reduced-motion: reduce) {
        .phase-row-border-overlay {
            animation: none !important;
        }
    }

    /* Thông báo nổi khi đang kéo để người dùng luôn biết đang đổi Phase nào với Phase nào. */
    .phase-drag-hint {
        position: fixed;
        top: 88px;
        left: 50%;
        transform: translateX(-50%) translateY(-14px) scale(.96);
        z-index: 99998;
        min-width: 460px;
        max-width: min(760px, calc(100vw - 32px));
        padding: 13px 18px 14px;
        border: 1px solid #c4b5fd;
        border-radius: 15px;
        background: rgba(255,255,255,.985);
        box-shadow: 0 16px 42px rgba(76,29,149,.20), 0 0 0 1px rgba(124,58,237,.035);
        backdrop-filter: blur(10px);
        -webkit-backdrop-filter: blur(10px);
        opacity: 0;
        visibility: hidden;
        pointer-events: none;
        transition: opacity .22s ease, transform .22s cubic-bezier(.2,.8,.2,1), visibility .22s ease;
        overflow: hidden;
    }

    .phase-drag-hint::before {
        content: "";
        position: absolute;
        left: 0;
        right: 0;
        top: 0;
        height: 3px;
        background: linear-gradient(90deg, #6d28d9, #8b5cf6, #6d28d9);
        background-size: 200% 100%;
        animation: phaseDragHintShine 2.4s linear infinite;
    }

    @media (prefers-reduced-motion: reduce) {
        .table-task-grid tbody tr.phase-drag-target > td:not(:first-child) {
            animation: none !important;
        }
    }

    .phase-drag-hint.show {
        opacity: 1;
        visibility: visible;
        transform: translateX(-50%) translateY(0) scale(1);
        animation: phaseDragHintPop .24s cubic-bezier(.2,.8,.2,1);
    }

    .phase-drag-hint-main {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 10px;
        color: #334155;
        font-size: 13.5px;
        line-height: 1.3;
        white-space: nowrap;
    }

    .phase-drag-hint-label {
        display: inline-flex;
        align-items: center;
        gap: 6px;
        color: #64748b;
        font-weight: 800;
        font-size: 11.5px;
        text-transform: uppercase;
        letter-spacing: .45px;
        flex: 0 0 auto;
    }

    .phase-drag-hint-label::before {
        content: "";
        width: 7px;
        height: 7px;
        border-radius: 50%;
        background: #7c3aed;
        box-shadow: 0 0 0 4px rgba(124,58,237,.10);
        animation: phaseDragPulse 1.2s ease-in-out infinite;
    }

    .phase-drag-hint-phase {
        display: inline-flex;
        align-items: center;
        gap: 6px;
        padding: 6px 10px;
        border-radius: 9px;
        background: #f6f3ff;
        border: 1px solid #d8b4fe;
        color: #5b21b6;
        font-weight: 750;
        max-width: 215px;
        overflow: hidden;
        text-overflow: ellipsis;
        box-shadow: inset 0 1px 0 rgba(255,255,255,.85);
    }

    .phase-drag-hint-code {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-width: 22px;
        height: 22px;
        padding: 0 6px;
        border-radius: 7px;
        background: #ede9fe;
        color: #6d28d9;
        font-size: 10.5px;
        font-weight: 850;
        flex: 0 0 auto;
    }

    .phase-drag-hint-arrow {
        color: #7c3aed;
        font-size: 15px;
        flex: 0 0 auto;
        filter: drop-shadow(0 2px 4px rgba(124,58,237,.18));
        animation: phaseDragArrowMove .9s ease-in-out infinite;
    }

    .phase-drag-hint-action {
        display: block;
        margin-top: 7px;
        padding-top: 7px;
        border-top: 1px solid #f0ecff;
        text-align: center;
        color: #64748b;
        font-size: 11.5px;
        font-weight: 600;
    }

    .phase-drag-hint-action strong {
        color: #5b21b6;
        font-weight: 800;
    }

    @keyframes phaseDragHintPop {
        0% { opacity: 0; transform: translateX(-50%) translateY(-10px) scale(.97); }
        60% { opacity: 1; transform: translateX(-50%) translateY(2px) scale(1.01); }
        100% { opacity: 1; transform: translateX(-50%) translateY(0) scale(1); }
    }

    @keyframes phaseDragHintShine {
        0% { background-position: 0% 50%; }
        100% { background-position: 200% 50%; }
    }

    @keyframes phaseDragPulse {
        0%, 100% { box-shadow: 0 0 0 3px rgba(124,58,237,.09); transform: scale(1); }
        50% { box-shadow: 0 0 0 6px rgba(124,58,237,.02); transform: scale(1.12); }
    }

    @keyframes phaseDragArrowMove {
        0%, 100% { transform: translateX(0); }
        50% { transform: translateX(3px); }
    }

    @media (max-width: 640px) {
        .phase-drag-hint {
            top: 68px;
            min-width: 0;
            width: calc(100vw - 20px);
            padding: 10px 11px 11px;
        }

        .phase-drag-hint-main {
            font-size: 11.5px;
            gap: 5px;
        }

        .phase-drag-hint-phase {
            max-width: 132px;
            padding: 5px 7px;
        }

        .phase-drag-hint-code {
            min-width: 20px;
            height: 20px;
        }

        .phase-drag-hint-action {
            font-size: 10.5px;
        }
    }

    @media (max-width: 992px) {
        .table-task-grid tbody td:not(:last-child) {
            padding: 9px 10px !important;
        }
    }
</style>

<div class="card-body p-0 mt-2">
    <asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:HiddenField runat="server" ID="hfDeletingTaskId" />
            <asp:HiddenField runat="server" ID="hfDragPhaseId" />
            <asp:HiddenField runat="server" ID="hfDragTargetPhaseId" />
            <asp:HiddenField runat="server" ID="hfDragDropPosition" />
            <asp:LinkButton ID="lbtApplyPhaseReorder" runat="server" OnClick="lbtApplyPhaseReorder_Click" Style="display:none;" CausesValidation="false"></asp:LinkButton>
            
            <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-3">
                <div class="d-flex gap-2 align-items-center flex-wrap flex-grow-1">
                    <button type="button" class="btn-filter-overdue" id="btnFilterOverdue" onclick="toggleOverdueFilter()" title="Lọc các công việc trễ hạn">
                        <i class="fas fa-exclamation-triangle"></i>
                        <span>Trễ hạn</span>
                        <span class="overdue-count" id="lblOverdueCount" runat="server">0</span>
                    </button>
                    
                    <button type="button" class="btn-tool-folder" id="btnToggleTree" onclick="toggleTaskTree()" 
                            data-expand-text="<%= GetResourceText(BackEndResourceKeys.EXPAND_ALL) %>" 
                            data-collapse-text="<%= GetResourceText(BackEndResourceKeys.COLLAPSE_ALL) %>">
                        <i class="far fa-folder-open"></i> <span id="lblToggleText">Thu gọn</span>
                    </button>
                    
                    <div class="input-group mb-0" style="max-width: 350px;">
                        <SweetSoft:ExtraTextBox CssClass="border-primary input-search-filter" ID="txtSearchSingle" runat="server"></SweetSoft:ExtraTextBox>
                        <SweetSoft:ExtraButton ButtonIcon="Search" CssClass="btn-outline-primary btn-search-filter" ID="lbtSearchSingle" IsCustomClass="false" OnClick="btnSearch_ServerClick" runat="server"></SweetSoft:ExtraButton>
                    </div>
                </div>

                <div class="d-flex gap-3 align-items-center flex-wrap">
                    <div class="d-flex align-items-center gap-3 font-mobile-small fw-medium">
                        <div class="d-flex align-items-center gap-2">
                            <span style="width: 16px; height: 16px; background-color: #fef2f2; border: 1px solid #fca5a5; border-radius: 4px;"></span>
                            <span class="text-danger"><%= GetResourceText("OVERDUE") %></span>
                        </div>
                        <div class="d-flex align-items-center gap-2">
                            <span style="width: 16px; height: 16px; background-color: #fffbeb; border: 1px solid #fcd34d; border-radius: 4px;"></span>
                            <span class="text-warning text-dark"><%= GetResourceText("DUE_SOON") %></span>
                        </div>
                    </div>
                    <asp:LinkButton ID="lbtConfigHeSo" runat="server" OnClick="lbtConfigHeSo_Click" CssClass="btn-swap-custom font-mobile-small me-2 pt-1 pb-1 px-2">
                        <i class="fas fa-star text-warning me-1"></i> Hệ số đóng góp
                    </asp:LinkButton>
                    <SweetSoft:ExtraButton ButtonIcon="Add" ButtonStyle="Info" CssClass="waves-effect waves-light font-mobile-small" ID="lbtAdd" OnClick="lbtAdd_Click" Visible="false" runat="server">Add new</SweetSoft:ExtraButton>
                </div>
            </div>

            <asp:Panel runat="server" ID="pnlNoTask" Visible="false" CssClass="text-center p-5 bg-white border rounded shadow-sm my-3">
                <i class="fas fa-inbox fs-1 text-muted opacity-50 mb-2"></i>
                <div class="fw-semibold text-secondary"><%= GetResourceText(BackEndResourceKeys.NO_TASK_FOR_YOU)%></div>
            </asp:Panel>

            <SweetSoft:GridviewExtension AllowSorting="false" AutoGenerateColumns="false" CssClass="table table-bordered table-task-grid table-hover align-middle w-100" DataKeyNames="IdCongViec" DataNameField="TenCongViec" GridLines="None" ID="grvData" IsEnableIndex="false" IsEnableSelectColumn="false" OnNeedDataSource="grvData_NeedDataSource" OnRowCommand="grvData_RowCommand" OnRowDataBound="grvData_RowDataBound" ShowHeader="true" ShowHeaderWhenEmpty="true" ValueField="IdCongViec" runat="server">
                <Columns>
                    <asp:TemplateField HeaderText="TaskName" HeaderStyle-Width="28%" ItemStyle-Width="28%" HeaderStyle-CssClass="text-center">
                        <HeaderStyle Width="28%"/>
                        <ItemStyle Width="28%"/>
                        <ItemTemplate>
                            <div class="d-flex align-items-center justify-content-between py-1 px-1">
                                <div style="width: 88%; word-break: break-word; white-space: normal;">
                                    <asp:LinkButton runat="server" ID="lbtTaskName" 
                                        CommandName="ITEM_DETAIL" 
                                        CommandArgument='<%# Eval("IdCongViec") %>'
                                        CssClass="text-decoration-none text-dark fw-semibold"
                                        Visible='<%# this.IsView || this.IsEdit %>'>
                                        <%# GetFormattedTaskName(Eval("MaCongViec"), Eval("TenCongViec")) %>
                                    </asp:LinkButton>
                                </div>
                                <div style="width: 10%; text-align: right;" class="flex-shrink-0">
                                    <asp:LinkButton runat="server" ID="lbtAddChild" 
                                        CommandName="ITEM_ADD_CHILD" 
                                        CommandArgument='<%# Eval("IdCongViec") %>'
                                        CssClass="btn-add-subtask-right"
                                        ToolTip="Thêm công việc con"
                                        Visible='<%# this.IsAdd %>'>
                                        <i class="fas fa-plus"></i>
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Owner" HeaderStyle-Width="150px" ItemStyle-Width="150px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" ItemStyle-Wrap="false">
                        <HeaderStyle Width="150px"/>
                        <ItemStyle Width="150px"/>
                        <ItemTemplate>
                            <div class="avatar-group">
                                <div class="avatar-stack-container">
                                    <%# GetAssigneeDisplay(Eval("TenNhanVien"), Eval("Avatars")) %>
                                </div>
                                <asp:LinkButton runat="server" ID="lbtAssign" 
                                    CommandName="ASSIGN_TASK" 
                                    CommandArgument='<%# Eval("IdCongViec") %>' 
                                    CssClass='<%# this.IsEdit ? "btn-assign-task" : "btn-assign-task view-only" %>' 
                                    ToolTip='<%# GetResourceText(BackEndResourceKeys.PERSONEL_ASSIGNMENT) %>'
                                    Visible='<%# this.IsEdit || this.IsView %>'>
                                    <i class='<%# this.IsEdit ? "fas fa-plus" : "fas fa-user-friends" %>'></i>
                                </asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Duration" HeaderStyle-Width="90px" ItemStyle-Width="90px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <HeaderStyle Width="90px"/>
                        <ItemStyle Width="90px"/>
                        <ItemTemplate>
                            <%# Eval("ThoiHanNgay") != DBNull.Value ? Eval("ThoiHanNgay") + " ngày" : "—" %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="StartDate" HeaderStyle-Width="110px" ItemStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <HeaderStyle Width="110px"/>
                        <ItemStyle Width="110px"/>
                        <ItemTemplate>
                            <%# FormatDateTime(Eval("NgayBatDau")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="EndDate" HeaderStyle-Width="110px" ItemStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <HeaderStyle Width="110px"/>
                        <ItemStyle Width="110px"/>
                        <ItemTemplate>
                            <%# FormatDateTime(Eval("NgayKetThuc")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="ActualCompletionDate" HeaderStyle-Width="110px" ItemStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <HeaderStyle Width="110px"/>
                        <ItemStyle Width="110px"/>
                        <ItemTemplate>
                            <%# FormatDateTime(Eval("NgayHoanThanhThucTe")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Status" HeaderStyle-Width="110px" ItemStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <HeaderStyle Width="110px"/>
                        <ItemStyle Width="110px"/>
                        <ItemTemplate>
                            <!-- 1. Nhãn tĩnh: Hiện khi Chỉ xem, LÀ task cha, HOẶC Trạng thái = 2, 3 (Đã HT) -->
                            <asp:Literal runat="server" 
                                Visible='<%# !this.IsEdit || CheckIsFatherTask(Eval("IdCongViec")) || Eval("TrangThai").ToString() == "2" || Eval("TrangThai").ToString() == "3" %>'
                                Text='<%# GetTaskStatusBadge(Eval("TrangThai")) %>'></asp:Literal>

                            <!-- 2. Nút "Bắt đầu!": Hiện khi Trạng thái = 0 (Chưa bắt đầu) -->
                            <asp:LinkButton runat="server" ID="lbtStatusStart" 
                                CommandName="START_TASK" 
                                CommandArgument='<%# Eval("IdCongViec") %>'
                                Visible='<%# this.IsEdit && !CheckIsFatherTask(Eval("IdCongViec")) && Eval("TrangThai").ToString() == "0" %>'
                                CssClass="badge-pill-custom badge-status-btn badge-status-todo btn-start-task" 
                                ToolTip="Bắt đầu thực hiện">
                                <span class="status-normal"><%# GetTaskStatusTextOnly(Eval("TrangThai")) %></span>
                                <span class="status-hover"><i class="fas fa-play me-1"></i>Bắt đầu!</span>
                            </asp:LinkButton>

                            <!-- 3. Nút "Chốt xong!": Hiện khi Trạng thái = 1 (Đang thực hiện) -->
                            <asp:LinkButton runat="server" ID="lbtStatusFastComplete" 
                                CommandName="FAST_COMPLETE" 
                                CommandArgument='<%# Eval("IdCongViec") %>'
                                Visible='<%# this.IsEdit && !CheckIsFatherTask(Eval("IdCongViec")) && Eval("TrangThai").ToString() == "1" %>'
                                CssClass="badge-pill-custom badge-status-btn badge-status-doing" 
                                ToolTip="Xác nhận hoàn thành">
                                <span class="status-normal"><%# GetTaskStatusTextOnly(Eval("TrangThai")) %></span>
                                <span class="status-hover"><i class="fas fa-check-double me-1"></i>Chốt xong!</span>
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Dependent" HeaderStyle-Width="90px" ItemStyle-Width="90px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center fw-bold">
                        <HeaderStyle Width="90px"/>
                        <ItemStyle Width="90px"/>
                        <ItemTemplate>
                            <%# GetPhuThuoc(Eval("IdCongViecPhuThuoc")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="130px">
                        <HeaderStyle Width="130px"/>
                        <ItemStyle Width="130px"/>
                        <ItemTemplate>
                            <SweetSoft:SmartLinkButton runat="server" 
                                VisibleConditionKey='<%# this.IsView %>'
                                ID="lbtDetail" 
                                CommandName="ITEM_DETAIL" 
                                CssClass="btn-grid-action text-decoration-underline me-1"
                                ResourceKey='<%# this.IsEdit ? BackEndResourceKeys.EDIT : BackEndResourceKeys.VIEW %>'
                                ButtonIcon='<%# this.IsEdit ? "fas fa-pencil-alt" : "fas fa-eye" %>'>
                            </SweetSoft:SmartLinkButton>
                          
                            <SweetSoft:SmartLinkButton runat="server" 
                                VisibleConditionKey='<%# this.IsDelete %>'
                                ID="lbtDelete" 
                                CommandName="ITEM_DELETE" 
                                CssClass="btn-grid-action text-decoration-underline text-danger me-1"
                                ResourceKey='<%# BackEndResourceKeys.DELETE %>'
                                ButtonIcon="fas fa-trash">
                            </SweetSoft:SmartLinkButton>
                
                            <SweetSoft:SmartLinkButton runat="server" 
                                VisibleConditionKey='<%# this.IsView %>'
                                ID="lbtViewSchedule" 
                                CommandName="VIEW_SCHEDULE" 
                                CssClass="btn-grid-action text-decoration-none text-info"
                                ResourceKey='<%# BackEndResourceKeys.VIEW %>' 
                                ButtonIcon="fas fa-calendar-alt">
                            </SweetSoft:SmartLinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="text-center p-3 text-muted">
                        <%= GetResourceText(BackEndResourceKeys.NO_DATA) %>
                    </div>
                </EmptyDataTemplate>
            </SweetSoft:GridviewExtension>
            <SweetSoft:CtrlStartTask ID="CtrlStartTask1" runat="server" />
            <SweetSoft:CtrlFastCompleteTask ID="CtrlFastCompleteTask1" runat="server" />
            <SweetSoft:CtrlViewTaskDetail ID="CtrlViewTaskDetail1" runat="server" />
            <SweetSoft:CtrlChonNhanVienTask ID="CtrlChonNhanVienTask1" runat="server"/>
            <SweetSoft:CtrlXemNhanVienTask ID="CtrlXemNhanVienTask1" runat="server"/>
            <SweetSoft:CtrlSwapPhase ID="CtrlSwapPhase1" runat="server"/>
        </ContentTemplate>
    </asp:UpdatePanel>

    <SweetSoft:CtrlAddPhase ID="CtrlAddPhase1" runat="server" />
    <SweetSoft:CtrlAddSubTask ID="CtrlAddSubTask1" runat="server" />

    <SweetSoft:ExtraModal DefaultButton="btnCloseTaskSchedule" ID="mdlTaskSchedule" Type="Primary" runat="server">
        <ContentTemplate>
            <asp:UpdatePanel ID="upnlTaskSchedule" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="p-3">
                        <div style="font-size: 13px; color: #1e40af; background: #eff6ff; padding: 10px 12px; border-radius: 6px; border: 1px solid #bfdbfe; margin-bottom: 12px;">
                            <i class="fas fa-calendar-alt me-1"></i>
                            <%= GetResourceText(BackEndResourceKeys.EXECUTION_TIME) %>:
                            <strong>
                                <asp:Literal ID="ltrScheduleTaskName" runat="server"></asp:Literal>
                            </strong>
                        </div>
                        <asp:HiddenField ID="hdfSingleTaskScheduleJson" runat="server" />
                        <div style="max-height:60vh; overflow-y:auto; padding:15px 5px 40px 5px;">
                            <div id="task-timeline-container" class="row-sched-timeline-grid-7col"></div>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </ContentTemplate>
    </SweetSoft:ExtraModal>

    <script type="text/javascript">
// ==========================================
// LOGIC 1: LỌC CÔNG VIỆC TRỄ HẠN
// ==========================================
function getTaskTreeState() {
    return {
        overdueFiltered: $('#btnFilterOverdue').hasClass('active-filter'),
        collapsed: $('#btnToggleTree').hasClass('active-filter')
    };
}

function isTaskOverdue(row) {
    var isDoingLate = row.attr('data-overdue') === '1';
    var isDoneLate = row.find('.task-late-label').length > 0 || row.text().indexOf('Trễ hạn') > -1;
    return isDoingLate || isDoneLate;
}

/*
   Một nguồn dữ liệu duy nhất cho UI:
   luôn duyệt TOÀN BỘ row trong DOM rồi quyết định row nào được hiện.
   Vì vậy Filter + Thu gọn không bao giờ lấy "danh sách đang visible" làm dữ liệu đầu vào.
*/
function applyTaskViewState(animate) {
    var state = getTaskTreeState();
    var $rows = $('.table-task-grid tbody tr[data-level]');

    $rows.each(function() {
        var row = $(this);
        var level = parseInt(row.attr('data-level'), 10);
        if (isNaN(level)) level = 1;

        var isRootPhase = level <= 1;
        var isOverdue = isTaskOverdue(row);

        var shouldShow;

        if (state.collapsed) {
            // Thu gọn = chỉ giữ Phase/root, bất kể đang filter hay không.
            shouldShow = isRootPhase;
        } else if (state.overdueFiltered) {
            // Đang filter trễ hạn = lấy lại từ FULL DOM, chỉ hiện row trễ hạn.
            shouldShow = isOverdue;
        } else {
            // Trạng thái bình thường = hiện toàn bộ.
            shouldShow = true;
        }

        if (animate) {
            if (shouldShow) row.stop(true, true).fadeIn(160);
            else row.stop(true, true).fadeOut(160);
        } else {
            row.toggle(shouldShow);
        }
    });
}

// ==========================================
// LOGIC 1: LỌC CÔNG VIỆC TRỄ HẠN
// ==========================================
function toggleOverdueFilter() {
    var btn = $('#btnFilterOverdue');
    btn.toggleClass('active-filter');

    applyTaskViewState(true);
}

// ==========================================
// LOGIC 2: THU GỌN / MỞ RỘNG CÂY CÔNG VIỆC
// ==========================================
function toggleTaskTree() {
    var btn = $('#btnToggleTree');
    var isCollapsed = !btn.hasClass('active-filter');

    var expandText = btn.attr('data-expand-text') || 'Mở rộng tất cả';
    var collapseText = btn.attr('data-collapse-text') || 'Thu gọn tất cả';

    function getShortToggleText(text) {
        return String(text || '').replace(/\s+tất cả\s*$/i, '').trim();
    }

    btn.toggleClass('active-filter', isCollapsed);
    btn.find('i').toggleClass('fa-folder', isCollapsed).toggleClass('fa-folder-open', !isCollapsed);
    btn.find('#lblToggleText').text(getShortToggleText(isCollapsed ? expandText : collapseText));

    // Luôn tính lại từ FULL DOM, không dựa vào row đang visible.
    applyTaskViewState(true);
}

        // ==========================================
        // LOGIC 3: KÉO THẢ ĐỔI VỊ TRÍ GIAI ĐOẠN
        // ==========================================
        var phaseDragState = {
            phaseId: null,
            targetId: null,
            position: 'before',
            sourceInfo: null,
            targetInfo: null,
            targetRow: null,
            isNoOp: false
        };

        function ensurePhaseRowBorderOverlay() {
            if ($('#phase-row-border-overlay').length) return;

            $('body').append('<div id="phase-row-border-overlay" class="phase-row-border-overlay" aria-hidden="true"></div>');
        }

        function positionPhaseRowBorder(row) {
            if (!row) return;

            ensurePhaseRowBorderOverlay();

            var rect = row.getBoundingClientRect();
            var $border = $('#phase-row-border-overlay');
            if (!$border.length) return;

            $border.css({
                left: Math.max(0, rect.left - 1) + 'px',
                top: Math.max(0, rect.top - 1) + 'px',
                width: Math.max(0, rect.width + 2) + 'px',
                height: Math.max(0, rect.height + 2) + 'px'
            }).toggleClass('noop', !!phaseDragState.isNoOp).addClass('show');
        }

        function hidePhaseRowBorder() {
            ensurePhaseRowBorderOverlay();
            $('#phase-row-border-overlay').removeClass('show noop');
        }

        function ensurePhaseDragHint() {
            if ($('#phase-drag-hint').length) return;

            $('body').append(
                '<div id="phase-drag-hint" class="phase-drag-hint" aria-live="polite">' +
                    '<div class="phase-drag-hint-main">' +
                        '<span class="phase-drag-hint-label">Đang sắp xếp</span>' +
                        '<span id="phase-drag-source" class="phase-drag-hint-phase"></span>' +
                        '<i class="fas fa-arrow-right phase-drag-hint-arrow"></i>' +
                        '<span id="phase-drag-target" class="phase-drag-hint-phase"></span>' +
                    '</div>' +
                    '<div id="phase-drag-action" class="phase-drag-hint-action"></div>' +
                '</div>'
            );
        }

        function getPhaseInfo(row) {
            var code = $.trim($(row).attr('data-code') || '');
            var fullText = $.trim($(row).find('td:nth-child(2) a').first().text() || '');
            var name = fullText;

            if (code && fullText.indexOf(code) === 0) {
                name = $.trim(fullText.substring(code.length).replace(/^[\s.:-]+/, ''));
            }

            return {
                code: code || '?',
                name: name || 'Giai đoạn'
            };
        }

        function renderPhaseDragHint() {
            ensurePhaseDragHint();

            var source = phaseDragState.sourceInfo;
            var target = phaseDragState.targetInfo;
            if (!source) return;

            var $hint = $('#phase-drag-hint');
            var sourceHtml = '<span class="phase-drag-hint-code">' + $('<div/>').text(source.code).html() + '</span>' + $('<div/>').text(source.name).html();
            $('#phase-drag-source').html(sourceHtml);

            if (target) {
                var targetHtml = '<span class="phase-drag-hint-code">' + $('<div/>').text(target.code).html() + '</span>' + $('<div/>').text(target.name).html();
                $('#phase-drag-target').html(targetHtml);
                if (phaseDragState.isNoOp) {
                    var relationText = phaseDragState.position === 'after' ? 'sau' : 'trước';
                    $('#phase-drag-action').html(
                        '<strong>Không có thay đổi:</strong> Giai đoạn <strong>' +
                        $('<div/>').text(source.code).html() +
                        '</strong> đã ở <strong>' +
                        relationText +
                        '</strong> giai đoạn <strong>' +
                        $('<div/>').text(target.code).html() +
                        '</strong>'
                    );
                } else {
                    $('#phase-drag-action').html(
                        'Thả chuột để đặt <strong>' +
                        (phaseDragState.position === 'after' ? 'sau' : 'trước') +
                        '</strong> giai đoạn <strong>' +
                        $('<div/>').text(target.code).html() +
                        '</strong>'
                    );
                }
            } else {
                $('#phase-drag-target').html('<span style="color:#94a3b8; font-weight:600;">Chọn giai đoạn đích…</span>');
                $('#phase-drag-action').text('Di chuyển lên trên một giai đoạn để chọn vị trí.');
            }

            $hint.addClass('show');
        }

        function hidePhaseDragHint() {
            ensurePhaseDragHint();
            $('#phase-drag-hint').removeClass('show');
        }

        function isPhaseReorderBlocked() {
            var overdueActive = $('#btnFilterOverdue').hasClass('active-filter');
            var searchValue = $.trim($('#<%= txtSearchSingle.ClientID %>').val() || '');
            return overdueActive || searchValue.length > 0;
        }

        function clearPhaseDropIndicators() {
            $('.table-task-grid tbody tr.phase-drop-before, .table-task-grid tbody tr.phase-drop-after')
                .removeClass('phase-drop-before phase-drop-after');
            $('.table-task-grid tbody tr.phase-drag-target')
                .removeClass('phase-drag-target phase-drag-noop');
            hidePhaseRowBorder();
            phaseDragState.targetRow = null;
            phaseDragState.isNoOp = false;
        }

        function getPhaseRows() {
            return $('.table-task-grid tbody tr.phase-draggable-row').toArray();
        }

        function isPhaseDropNoOp(targetRow, before) {
            var rows = getPhaseRows();
            var sourceIndex = rows.indexOf($('.table-task-grid tbody tr.phase-draggable-row.phase-dragging')[0]);
            var targetIndex = rows.indexOf(targetRow);

            if (sourceIndex < 0 || targetIndex < 0 || sourceIndex === targetIndex) return true;

            var newIndex = targetIndex;
            if (sourceIndex < targetIndex) newIndex--;
            if (!before) newIndex++;

            return newIndex === sourceIndex;
        }

        function initPhaseDragDrop() {
            var $document = $(document);

            // Delegated events để UpdatePanel render lại tbody vẫn hoạt động.
            $document.off('.phaseDragDrop');
            // Giữ overlay bám đúng row khi trang cuộn / thay đổi kích thước.
            $(window).off('.phaseDragBorder')
                .on('scroll.phaseDragBorder resize.phaseDragBorder', function () {
                    if (phaseDragState.targetRow) {
                        window.requestAnimationFrame(function () {
                            positionPhaseRowBorder(phaseDragState.targetRow);
                        });
                    }
                });


            $document.on('dragstart.phaseDragDrop', '.table-task-grid tbody tr.phase-draggable-row', function (e) {
                if (isPhaseReorderBlocked()) {
                    e.preventDefault();
                    return false;
                }

                var row = $(this);
                var phaseId = row.attr('data-phase-id');
                if (!phaseId) {
                    e.preventDefault();
                    return false;
                }

                phaseDragState.phaseId = phaseId;
                phaseDragState.targetId = null;
                phaseDragState.position = 'before';
                phaseDragState.sourceInfo = getPhaseInfo(row);
                phaseDragState.targetInfo = null;
                phaseDragState.targetRow = null;

                row.addClass('phase-dragging');
                clearPhaseDropIndicators();
                renderPhaseDragHint();

                var original = e.originalEvent;
                if (original && original.dataTransfer) {
                    original.dataTransfer.effectAllowed = 'move';
                    original.dataTransfer.setData('text/plain', phaseId);
                }
            });

            $document.on('dragover.phaseDragDrop', '.table-task-grid tbody tr.phase-draggable-row', function (e) {
                if (!phaseDragState.phaseId || isPhaseReorderBlocked()) return;

                var row = $(this);
                var targetId = row.attr('data-phase-id');
                if (!targetId || targetId === phaseDragState.phaseId) return;

                e.preventDefault();
                e.originalEvent.dataTransfer.dropEffect = 'move';

                var rect = this.getBoundingClientRect();
                var before = e.originalEvent.clientY < rect.top + (rect.height / 2);

                $('.table-task-grid tbody tr.phase-drag-target')
                    .not(row)
                    .removeClass('phase-drag-target phase-drag-noop');
                $('.table-task-grid tbody tr.phase-drop-before, .table-task-grid tbody tr.phase-drop-after')
                    .not(row)
                    .removeClass('phase-drop-before phase-drop-after');

                var isNoOp = isPhaseDropNoOp(this, before);

                row.addClass('phase-drag-target');
                row.toggleClass('phase-drag-noop', isNoOp);
                if (!isNoOp)
                    row.addClass(before ? 'phase-drop-before' : 'phase-drop-after');

                phaseDragState.targetId = targetId;
                phaseDragState.position = before ? 'before' : 'after';
                phaseDragState.targetInfo = getPhaseInfo(row);
                phaseDragState.targetRow = this;
                phaseDragState.isNoOp = isNoOp;
                positionPhaseRowBorder(this);
                renderPhaseDragHint();
            });

            $document.on('drop.phaseDragDrop', '.table-task-grid tbody tr.phase-draggable-row', function (e) {
                if (!phaseDragState.phaseId || isPhaseReorderBlocked()) return;

                e.preventDefault();

                var row = $(this);
                var targetId = row.attr('data-phase-id');
                if (!targetId || targetId === phaseDragState.phaseId) {
                    clearPhaseDropIndicators();
                    hidePhaseDragHint();
                    return;
                }

                if (phaseDragState.isNoOp) {
                    clearPhaseDropIndicators();
                    $('.phase-dragging').removeClass('phase-dragging');
                    hidePhaseDragHint();
                    phaseDragState.phaseId = null;
                    phaseDragState.targetId = null;
                    phaseDragState.position = 'before';
                    phaseDragState.sourceInfo = null;
                    phaseDragState.targetInfo = null;
                    phaseDragState.targetRow = null;
                    phaseDragState.isNoOp = false;
                    return;
                }

                $('#<%= hfDragPhaseId.ClientID %>').val(phaseDragState.phaseId);
                $('#<%= hfDragTargetPhaseId.ClientID %>').val(targetId);
                $('#<%= hfDragDropPosition.ClientID %>').val(phaseDragState.position);

                clearPhaseDropIndicators();
                $('.phase-dragging').removeClass('phase-dragging');
                hidePhaseDragHint();

                // PostBack qua LinkButton nằm trong UpdatePanel để lưu thứ tự xuống server.
                __doPostBack('<%= lbtApplyPhaseReorder.UniqueID %>', '');
            });

            $document.on('dragend.phaseDragDrop', '.table-task-grid tbody tr.phase-draggable-row', function () {
                clearPhaseDropIndicators();
                $('.phase-dragging').removeClass('phase-dragging');
                hidePhaseDragHint();
                phaseDragState.phaseId = null;
                phaseDragState.targetId = null;
                phaseDragState.position = 'before';
                phaseDragState.sourceInfo = null;
                phaseDragState.targetInfo = null;
                phaseDragState.targetRow = null;
                phaseDragState.isNoOp = false;
            });
        }

        ensurePhaseDragHint();
        initPhaseDragDrop();

        window.CMSMasterJs = window.CMSMasterJs || {};
        CMSMasterJs.RenderSingleTaskSchedule = function () {
            var container = $('#task-timeline-container');
            container.empty();
            
            var jsonString = $('#<%= hdfSingleTaskScheduleJson.ClientID %>').val();
            if (!jsonString) return;

            try {
                var decodedJson = $('<textarea/>').html(jsonString).text();
                var scheduleData = JSON.parse(decodedJson);

                for (var dateKey in scheduleData) {
                    var dayData = scheduleData[dateKey];
                    var dateParts = dateKey.split('-');
                    var formattedDate = dateParts[2] + '/' + dateParts[1];

                    var tooltipHtml = "";
                    var hasTasks = (dayData.status === "busy" && dayData.tasks && dayData.tasks.length > 0);

                    if (hasTasks) {
                        tooltipHtml = '<div class="custom-task-tooltip"><ul class="tooltip-task-list">';
                        for (var i = 0; i < dayData.tasks.length; i++) {
                            tooltipHtml += '<li><span class="t-code">[' + dayData.tasks[i].code + ']</span>' + dayData.tasks[i].name + '</li>';
                        }
                        tooltipHtml += '</ul></div>';
                    }

                    var clickAttr = hasTasks ? 'onclick="CMSMasterJs.PinTooltip(this, event)"' : '';

                    var html = '<div class="sched-day-card" ' + clickAttr + '>' +
                        '<div class="sd-header">' + formattedDate + '<small>' + dayData.dayName + '</small></div>' +
                        '<div class="sd-body ' + dayData.status + '">' + dayData.displayText + '</div>' +
                        tooltipHtml +
                        '</div>';
                    container.append(html);
                }
            } catch (e) {
                console.error("Lỗi vẽ JSON Lịch biểu Task: ", e);
            }
        };

        CMSMasterJs.PinTooltip = function (element, event) {
            event.stopPropagation();
            var isPinned = $(element).hasClass('show-tooltip'); $('.sched-day-card').removeClass('show-tooltip');
            if (!isPinned) $(element).addClass('show-tooltip');
        };

        $(document).on('click', function () {
            $('.sched-day-card').removeClass('show-tooltip');
        });

        // UpdatePanel có thể render lại tbody. Sau khi render xong, áp lại state hiện tại.
        if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                hidePhaseDragHint();
                clearPhaseDropIndicators();
                ensurePhaseDragHint();
                if ($('#btnFilterOverdue').length || $('#btnToggleTree').length) {
                    applyTaskViewState(false);
                }
            });
        }
    </script>
</div>