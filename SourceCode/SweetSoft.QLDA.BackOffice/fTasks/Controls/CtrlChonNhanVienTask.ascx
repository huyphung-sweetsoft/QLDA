<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlChonNhanVienTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlChonNhanVienTask" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<style>
    /* ========================================================
       1. KIẾN TRÚC LAYOUT TỔNG
       ======================================================== */
    .task-modal-body {
        display: flex;
        flex-direction: column;
        /* GIẢM XUỐNG 80vh VÀ TRỪ HAO ĐỦ LỚN ĐỂ KHÔNG BAO GIỜ KHUẤT NÚT LƯU */
        height: calc(100vh - 160px); 
        max-height: 80vh;
        min-height: 500px;
        min-width: 0;
        box-sizing: border-box;
        overflow: hidden;
    }
    
    .task-modal-header-info {
        flex-shrink: 0; 
        margin-bottom: 12px;
    }
    
    .info-row-custom {
        display: flex;
        justify-content: space-between;
        align-items: center;
        flex-wrap: nowrap !important;
        gap: 10px;
        overflow: hidden;
    }
    .info-row-custom > div { margin-bottom: 0 !important; white-space: nowrap; text-overflow: ellipsis; overflow: hidden; }

    .task-member-columns {
        display: grid;
        grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
        gap: 16px;
        flex: 1 1 0;
        min-height: 0;
        min-width: 0;
        overflow: hidden; 
        padding-bottom: 5px;
    }

    @media (max-width: 991px) {
        .task-member-columns {
            grid-template-columns: minmax(0, 1fr);
            grid-template-rows: minmax(0, 1fr) minmax(0, 1fr);
            overflow: hidden;
        }
    }

    /* ========================================================
       2. PANEL DANH SÁCH & BỘ LỌC TÌM KIẾM
       ======================================================== */
    .member-list-panel {
        display: flex;
        flex-direction: column;
        min-height: 0;
        min-width: 0;
        background: #ffffff;
        border: 1px solid #e2e8f0;
        border-radius: 8px;
        overflow: hidden;
    }

    .panel-header {
        padding: 10px 12px;
        font-size: 13px;
        font-weight: 700;
        color: #1e3a8a;
        background: #f1f5f9;
        border-bottom: 1px solid #e2e8f0;
        flex-shrink: 0;
        display: flex;
        justify-content: space-between;
        align-items: center;
    }
    .panel-header.company-header {
        color: #9d174d;
        background: #fdf2f8;
        border-bottom-color: #fbcfe8;
    }

    .panel-filter {
        padding: 10px 10px 0 10px;
        flex-shrink: 0;
        background: #ffffff;
        border-bottom: 1px solid #f1f5f9;
    }

    .panel-list {
        flex: 1 1 0;
        min-height: 0;
        min-width: 0;
        overflow-y: auto;
        overflow-x: hidden;
        box-sizing: border-box;
        padding: 10px;
        background: #f8fafc;
    }
    .panel-list > div { display: block; min-height: 0; }
    .panel-list::-webkit-scrollbar { width: 6px; }
    .panel-list::-webkit-scrollbar-track { background: transparent; }
    .panel-list::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 4px; }
    .panel-list::-webkit-scrollbar-thumb:hover { background: #94a3b8; }

    /* ========================================================
       3. ROW NHÂN VIÊN VÀ LỊCH BIỂU (GIỮ NGUYÊN BẢN TỐT NHẤT)
       ======================================================== */
    .member-item-row { position: relative; background: white; border: 1px solid #e2e8f0; border-radius: 6px; margin-bottom: 6px; overflow: hidden; display: flex; flex-direction: column; align-items: stretch; }
    .member-item-row:last-child { margin-bottom: 0; }
    .member-item-row.show-schedule { border-color: #93c5fd; box-shadow: 0 4px 12px rgba(37, 99, 235, 0.08); }
    .row-default-view { display: flex; align-items: center; justify-content: space-between; padding: 6px 10px; width: 100%; min-height: 46px; box-sizing: border-box; font-size: 13px; }
    .member-info-group { display: flex; align-items: center; gap: 10px; min-width: 0; }
    .member-name-block { display: flex; flex-direction: column; min-width: 0; line-height: 1.2; }
    .member-email {
    font-size: 11px;
    color: #64748b;

    /* Hiển thị đầy đủ email, không cắt bằng dấu ... */
    white-space: normal;
    overflow: visible;
    text-overflow: clip;
    overflow-wrap: anywhere;
    }

    /* Chức danh nằm cùng dòng với tên nhân viên */
    .member-position {
        font-size: 12px;
        font-weight: 500;
        color: #475569;
        min-width: 0;
    }

    .member-position:not(:empty)::before {
        content: "·";
        color: #94a3b8;
        margin: 0 5px 0 2px;
    }

    .member-position:empty {
        display: none;
    }

    /* Thanh tổng quan phân công ở chân popup */
    .task-assignment-footer-row {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 12px;
        width: 100%;
        min-width: 0;
    }

    .task-assignment-summary {
        margin-right: auto;
        min-width: 0;
        font-size: 12px;
        line-height: 1.4;
        color: #475569;
    }

    /*
     * UpdatePanel ở chân popup cần giãn để phần tổng quan
     * nằm bên trái, còn nút Lưu và nút Đóng vẫn ở bên phải.
     */
    div[id$="upnlTaskMemberFooter"] {
        flex: 1 1 auto;
        min-width: 0;
    }

    @media (max-width: 575px) {
        .task-assignment-footer-row {
            gap: 8px;
        }

        .task-assignment-summary {
            font-size: 11px;
        }
    }
    .member-email:empty { display: none; }
    .member-info-group input[type="checkbox"] { width: 16px; height: 16px; cursor: pointer; accent-color: #2563eb; }
    
    .btn-calendar-only { background: #ffffff; border: 1px solid #e2e8f0; width: 28px; height: 28px; border-radius: 6px; cursor: pointer; display: flex; align-items: center; justify-content: center; font-size: 13px; transition: all 0.2s cubic-bezier(0.34, 1.56, 0.64, 1); }
    .btn-calendar-only:hover { background: #eff6ff; border-color: #93c5fd; transform: scale(1.1); }
    .member-item-row.show-schedule .btn-calendar-only { background: #eff6ff; border-color: #2563eb; }
    .single-avatar-circle { width: 26px; height: 26px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 700; color: #ffffff; flex-shrink: 0; box-shadow: 0 1px 2px rgba(0,0,0,0.1); }

    /* TRẢ LẠI LỊCH BỂU GỐC: RỘNG RÃI VÀ CHIỀU CAO CHUẨN */
    .row-schedule-panel { max-height: 0; overflow: hidden; border-top: 1px solid transparent; transition: max-height 0.35s ease, border-color 0.35s ease; }
    .member-item-row.show-schedule .row-schedule-panel { max-height: 460px; border-top-color: #e2e8f0; } 
    .row-schedule-inner { padding: 8px 12px 12px; }
    .row-schedule-panel, .row-schedule-inner, .mini-cal-wrap, .mini-cal { width: 100% !important; box-sizing: border-box; }
    
    /* VỀ LẠI 310PX GỐC */
    .mc-viewport { position: relative; overflow: hidden; height: 310px; width: 100% !important; }
    .mc-weekdays, .mc-grid { display: grid; grid-template-columns: repeat(7, minmax(0, 1fr)); width: 100% !important; }

    .mini-cal-wrap { flex: 1; min-width: 0; display: flex; flex-direction: column; }
    .mini-cal { flex: 1; min-width: 0; user-select: none; }
    .mc-header { display: flex; align-items: center; justify-content: space-between; padding: 0 2px 4px 4px; }
    .mc-title { font-size: 14px; font-weight: 700; color: #1e293b; }
    .mc-nav { display: flex; gap: 2px; }
    .mc-nav-btn { width: 30px; height: 26px; border: 0; background: transparent; border-radius: 6px; color: #64748b; font-size: 10px; line-height: 1; cursor: pointer; transition: background 0.15s, color 0.15s; }
    .mc-nav-btn:hover { background: #eff6ff; color: #2563eb; }
    .mc-nav-btn:focus-visible { outline: 2px solid #2563eb; outline-offset: 1px; }
    .mc-weekdays span { text-align: center; font-size: 11px; font-weight: 700; color: #64748b; padding: 3px 0 5px; }
    .mc-weekdays span.mc-we { color: #94a3b8; }
    
    /* VỀ LẠI 50PX CHO MỖI DÒNG LỊCH GỐC */
    .mc-grid { grid-auto-rows: 50px; gap: 2px; }
    .mc-viewport .mc-grid { position: absolute; top: 0; left: 0; right: 0; will-change: transform, opacity; }
    
    .mc-day { display: flex; flex-direction: column; align-items: center; justify-content: center; border-radius: 6px; border: 1px solid transparent; box-sizing: border-box; font-size: 12px; color: #1e293b; cursor: default; min-width: 0; }
    .mc-day .mc-num { font-weight: 600; line-height: 1; }
    .mc-day.out-month .mc-num { color: #94a3b8; }
    .mc-day.out-range { opacity: 0.32; }
    .mc-day.st-busy    { background: #fee2e2; color: #b91c1c; }
    .mc-day.st-holiday { background: #fef3c7; color: #b45309; }
    .mc-day.st-weekend { background: #f1f5f9; color: #64748b; }
    .mc-day.st-free    { background: #e6f4ea; color: #137333; }
    .mc-day.today { border-color: #2563eb; box-shadow: inset 0 0 0 1px #2563eb; }
    
    .mc-label { margin-top: 3px; max-width: 100%; padding: 0 3px; box-sizing: border-box; font-size: 9.5px; font-weight: 600; line-height: 1.1; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
   /* HIGHLIGHT NHÂN VIÊN GỐC CỦA CÔNG VIỆC */
    .member-item-row.original-member {
        background-color: #f0f8ff; 
        border-color: #bfe0ff;     
    }
    .member-item-row.original-member:hover {
        background-color: #e6f3ff;
    }
    
    /* [BỔ SUNG BƯỚC 1]: ÉP NỀN TRONG SUỐT CHO VÙNG CHỨA LỊCH ĐỂ TRÀN MÀU XANH NHẠT */
    .member-item-row.original-member .row-schedule-panel,
    .member-item-row.original-member .row-schedule-inner,
    .member-item-row.original-member .mini-cal-wrap,
    .member-item-row.original-member .mini-cal,
    .member-item-row.original-member .mc-header,
    .member-item-row.original-member .mc-weekdays,
    .member-item-row.original-member .mc-viewport {
        background-color: transparent !important;
    }

    .member-item-row.original-member .btn-calendar-only {
        background-color: #ffffff;
        border-color: #bfe0ff;
    }
    .member-item-row.original-member .btn-calendar-only:hover,
    .member-item-row.original-member.show-schedule .btn-calendar-only {
        background-color: #dbeafe;
        border-color: #3b82f6;
    }
    /* ============================================================
       TOOLTIP GLOBAL (NẰM TRÊN LAYER CAO NHẤT CỦA TRÌNH DUYỆT)
       ============================================================ */
    .mc-day { position: relative !important; overflow: visible !important; }
    .mc-day.has-tooltip { cursor: pointer; }
    .mc-day:hover, .mc-day.show-tooltip { z-index: 50; }
    
    .global-task-tooltip { 
        position: absolute; /* Định vị tuyệt đối so với toàn màn hình */
        z-index: 9999999 !important; /* Đè lên mọi Modal/Popup */
        background: #ffffff; color: #334155; 
        border: 1px solid #cbd5e1; border-radius: 10px; padding: 11px 16px; 
        min-width: 340px; max-width: 520px; width: max-content; 
        white-space: normal; word-break: break-word; overflow-wrap: anywhere; 
        box-shadow: 0 10px 40px rgba(15, 23, 42, 0.2); font-size: 12.5px; line-height: 1.5; 
        opacity: 0; visibility: hidden; pointer-events: none; 
        transition: opacity 0.15s ease, visibility 0.15s ease; 
    }
    .global-task-tooltip.show { opacity: 1; visibility: visible; pointer-events: auto; }
    
    /* Mũi tên chỉ xuống (Tự động chạy qua lại theo tọa độ JS) */
    .global-task-tooltip::after { 
        content: ''; position: absolute; top: 100%; 
        left: var(--arrow-left, 50%); /* Chạy động bằng CSS Variable */
        transform: translateX(-50%);
        border-width: 6px; border-style: solid; border-color: #ffffff transparent transparent transparent; 
    }

    .tooltip-task-list { list-style: none; margin: 0; padding: 0; text-align: left; }
    .tooltip-task-list li { margin: 0; padding: 8px 0; border-bottom: 1px dashed #e2e8f0; color: #334155; }
    .tooltip-task-list li:last-child { border-bottom: none; padding-bottom: 0; }
    .tooltip-task-list li:first-child { padding-top: 0; }
    .t-project-name { font-size: 10.5px; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 3px; font-weight: 600; color: #64748b; }
    .t-code { display: inline-block; color: #2563eb; font-weight: 700; margin-right: 6px; }


/* === Task member planner 2026: popover + people timeline === */
.task-member-picker-root{position:relative}
.tmp-tabs{display:flex;gap:4px;align-items:center;flex-shrink:0;min-height:34px;margin-top:6px;border-bottom:1px solid #e5e7eb}
.tmp-tab{border:1px solid transparent;border-bottom:2px solid transparent;background:transparent;color:#64748b;font-size:12.5px;font-weight:700;padding:6px 13px;cursor:pointer;white-space:nowrap;border-radius:6px 6px 0 0}
.tmp-tab:hover{background:#f8f5fc;color:#552291}
.tmp-tab.is-active{color:#552291;border-bottom-color:#6630a2;background:#f8f5fc}
.tmp-tab:focus-visible,.tmp-pop-close:focus-visible,.tmp-day:focus-visible,.tmp-timeline-bar button:focus-visible{outline:2px solid #6630a2;outline-offset:2px}
.tmp-auto-note{margin-left:auto;font-size:11px;color:#9a3412;cursor:help;padding-right:5px}
.task-member-picker-root .task-member-columns[hidden],.task-member-picker-root .tmp-timeline[hidden],.task-member-picker-root .tmp-calendar-pop[hidden]{display:none!important}
.task-member-picker-root .task-member-columns{flex:1 1 0}
.tmp-calendar-pop{position:absolute;z-index:100;min-width:240px;display:flex;flex-direction:column;overflow:visible;padding:10px;border:1px solid #d9c5ee;border-radius:10px;background:#fff;box-shadow:0 12px 35px rgba(28,18,58,.22);animation:tmp-pop-in .14s ease-out}
@keyframes tmp-pop-in{from{opacity:.5;transform:scale(.985)}to{opacity:1;transform:scale(1)}}
.tmp-calendar-pop:before,.tmp-calendar-pop:after{position:absolute;content:"";width:0;height:0;border-top:10px solid transparent;border-bottom:10px solid transparent;top:var(--tmp-arrow-y,50%);transform:translateY(-50%)}
.tmp-calendar-pop.tmp-from-left:before{left:-12px;border-right:12px solid #d9c5ee}
.tmp-calendar-pop.tmp-from-left:after{left:-10px;border-right:11px solid #fff}
.tmp-calendar-pop.tmp-from-right:before{right:-12px;border-left:12px solid #d9c5ee}
.tmp-calendar-pop.tmp-from-right:after{right:-10px;border-left:11px solid #fff}
.tmp-pop-head{display:flex;align-items:center;justify-content:space-between;gap:8px;min-height:28px;padding:0 1px 6px;border-bottom:1px solid #f1ebf7}
.tmp-pop-name{font-size:12.5px;font-weight:750;color:#25324a;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.tmp-pop-close{display:inline-flex;align-items:center;justify-content:center;width:24px;height:24px;border-radius:5px;border:0;background:#f7f2fd;color:#61309e;cursor:pointer;font-size:19px}
.tmp-pop-inner{min-height:0;overflow-y:auto;overflow-x:hidden}
.tmp-calendar-pop .mc-header{padding-top:5px}
.tmp-calendar-pop .mc-grid{grid-auto-rows:var(--tmp-day-size,36px);gap:2px}
.tmp-calendar-pop .mc-viewport{height:calc(var(--tmp-day-size,36px)*6 + 10px)}
.tmp-calendar-pop .mc-weekdays span{font-size:10px;padding:2px 0}
.tmp-calendar-pop .mc-day{font-size:11px}
.tmp-calendar-pop .mc-label{font-size:9px}
.tmp-calendar-pop .mc-day.has-current:after{content:"";width:5px;height:5px;border-radius:50%;background:#7c3aed;position:absolute;right:5px;bottom:4px}
.tmp-pop-help{display:flex;gap:8px;flex-wrap:wrap;align-items:center;padding:5px 1px 0;color:#64748b;font-size:10px}
.tmp-pop-help span::before{content:"";display:inline-block;width:8px;height:8px;border-radius:2px;vertical-align:middle;margin-right:4px;background:var(--tmp-legend)}
.tmp-pop-detail{display:none;min-height:0;overflow-y:auto;font-size:12px;line-height:1.5;color:#334155;padding-top:9px}
.tmp-calendar-pop.tmp-detail-mode .tmp-pop-calendar{display:none}
.tmp-calendar-pop.tmp-detail-mode .tmp-pop-detail{display:block}
.tmp-pop-detail-back{border:1px solid #e1d6ee;border-radius:6px;background:#faf7ff;color:#5b2189;padding:5px 9px;font-size:11.5px;cursor:pointer;margin-bottom:8px}
.tmp-pop-task{border-bottom:1px dashed #e2e8f0;padding:8px 0}
.tmp-pop-task:last-child{border:0}
.tmp-pop-task small{display:block;color:#64748b;margin-bottom:2px}
.tmp-timeline{position:relative;display:flex;flex:1 1 0;flex-direction:column;min-height:0;min-width:0;overflow:hidden;border:1px solid #e2e8f0;border-radius:8px;background:#fff}
.tmp-timeline-bar{display:flex;align-items:center;gap:8px;padding:7px 10px;border-bottom:1px solid #e8edf4;background:#f8fafc;flex-shrink:0;min-width:0}
.tmp-timeline-search{height:31px;border:1px solid #dbe2eb;border-radius:6px;padding:5px 10px;max-width:330px;min-width:125px;flex:1 1 auto;font-size:12px;background:#fff}
.tmp-timeline-filter{height:31px;max-width:165px;min-width:110px;border:1px solid #dbe2eb;background:#fff;border-radius:6px;font-size:12px;padding:4px 7px}
.tmp-window-nav{border:1px solid #dbe2eb;border-radius:5px;color:#552291;background:#fff;min-width:24px;height:28px;cursor:pointer}
.tmp-window-nav[hidden]{display:none!important}
.tmp-timeline-tip{margin-left:auto;font-size:10.5px;color:#64748b;white-space:nowrap}
.tmp-scroll{position:relative;flex:1 1 0;overflow:auto;min-height:0;min-width:0;scrollbar-color:#cbd5e1 transparent}
.tmp-table{width:max-content;min-width:100%;border-spacing:0;border-collapse:separate;table-layout:fixed}
.tmp-table th,.tmp-table td{padding:0;border-right:1px solid #eef1f6;border-bottom:1px solid #eef1f6;box-sizing:border-box}
.tmp-table thead th{position:sticky;top:0;z-index:7;background:#f1f5f9;color:#506179;font-weight:700;white-space:nowrap}
.tmp-table .tmp-left{position:sticky;left:0;width:276px;min-width:276px;max-width:276px;z-index:5;background:#fff;box-shadow:2px 0 4px rgba(0,0,0,.035)}
.tmp-table thead tr:nth-child(2) th{top:22px}
.tmp-table thead .tmp-left{z-index:9;background:#f1f5f9;text-align:left;padding-left:12px;font-size:12px}
.tmp-month{height:22px;font-size:10.5px;text-align:center}
.tmp-date{height:29px;min-width:36px;width:36px;text-align:center;font-size:10px;line-height:1.1}
.tmp-date span{display:block;color:#7b8799;font-size:9px;margin-top:2px}
.tmp-date.tmp-today{color:#59249b;background:#ede4fa}
.tmp-group td{height:31px;background:#eaf2fa!important;font-size:11.5px;font-weight:750;color:#234e7d}
.tmp-group.tmp-other td{background:#fceff5!important;color:#9d174d}
.tmp-group .tmp-left{z-index:6;background:inherit!important;padding-left:10px}
.tmp-group button{border:0;color:inherit;background:none;font-weight:750;cursor:pointer;text-align:left;width:100%;height:30px}
.tmp-employee td{height:49px}
.tmp-person-cell{display:flex;align-items:center;gap:7px;padding:6px 8px;min-width:0}
.tmp-person-cell input{width:15px;height:15px;flex:0 0 auto;accent-color:#5b21b6;cursor:pointer}
.tmp-person-text{flex:1 1 auto;min-width:0}
.tmp-person-text strong{display:block;color:#25354b;font-size:11.5px;white-space:nowrap;text-overflow:ellipsis;overflow:hidden}
.tmp-person-text small{display:block;font-size:10px;color:#77849a;white-space:nowrap;text-overflow:ellipsis;overflow:hidden}
.tmp-busy-count{font-size:9.5px;color:#a16207;font-weight:700;white-space:nowrap}
.tmp-employee.tmp-will-add .tmp-left{box-shadow:inset 3px 0 #7c3aed}
.tmp-employee.tmp-will-remove .tmp-left{box-shadow:inset 3px 0 #ef4444}
.tmp-day{width:100%;height:34px;padding:0;border:0;border-radius:3px;display:block;cursor:pointer;background:#e6f4ea;position:relative}
.tmp-day.st-busy{background:#fee2e2}
.tmp-day.st-weekend{background:#f1f5f9}
.tmp-day.st-holiday{background:#fef3c7}
.tmp-day.st-free{background:#e6f4ea}
.tmp-day.out-of-range{background:#f8fafc}
.tmp-day.has-current:after{content:"";width:5px;height:5px;border-radius:50%;background:#7c3aed;position:absolute;bottom:3px;right:3px}
.tmp-day:hover{outline:1px solid #7443b3;outline-offset:-1px}
.tmp-table .tmp-employee.is-selected .tmp-left{background:#faf6ff}
.tmp-empty{padding:35px 15px;text-align:center;color:#64748b;font-size:12px}
.tmp-floating-day-detail{position:absolute;z-index:20;max-width:350px;width:min(350px,calc(100% - 22px));background:#fff;border:1px solid #d9c5ee;border-radius:9px;box-shadow:0 9px 26px rgba(25,12,55,.2);padding:13px;overflow-y:auto;max-height:240px;font-size:12px}
.tmp-floating-day-detail[hidden]{display:none!important}
/* Timeline toolbar: same design vocabulary as BootstrapDropdown + ExtraTextBox and one-row filter tags. */
.tmp-timeline-bar .tmp-filter-target{max-width:186px;min-width:154px}
.tmp-timeline-bar .tmp-role-filter{max-width:156px;min-width:115px;border-color:#7b38b3;color:#445367}
.tmp-search-combo{display:flex;align-items:stretch;flex:1 1 auto;min-width:150px;max-width:360px;height:32px}
.tmp-search-combo .tmp-timeline-search{min-width:80px;width:100%;height:32px;max-width:none;border:1px solid #cfc6e5;border-right:0;border-radius:5px 0 0 5px}
.tmp-search-apply{display:inline-flex;align-items:center;justify-content:center;flex:0 0 34px;height:32px;background:#fff;color:#561b90;border:1px solid #61269f;border-radius:0 5px 5px 0;cursor:pointer}
.tmp-search-apply:hover{background:#f5efff}
.tmp-active-filters{display:flex;gap:5px;align-items:center;overflow-x:auto;flex-shrink:0;height:26px;max-height:26px;padding:2px 9px;border-bottom:1px solid #f0edf5;scrollbar-width:thin;white-space:nowrap}
.tmp-active-filters[hidden]{display:none!important}
.tmp-filter-tag{display:inline-flex;align-items:center;gap:5px;padding:2px 6px;border-radius:5px;background:#f3edfc;color:#54218d;font-size:10.5px;white-space:nowrap;border:1px solid #e6d8f5}
.tmp-filter-tag button{font-size:13px;color:#54218d;background:none;border:0;padding:0 1px;cursor:pointer;line-height:1}
.task-member-picker-root .listSearchTagBox{max-height:30px;min-height:0;overflow-x:auto;overflow-y:hidden;white-space:nowrap;scrollbar-width:thin}
.tmp-calendar-pop .mc-grid{grid-auto-rows:var(--tmp-day-size,48px)}
.tmp-calendar-pop .mc-viewport{height:calc(var(--tmp-day-size,48px)*6 + 10px)}
.tmp-calendar-pop .mc-day{font-size:12px}
.tmp-calendar-pop .mc-label{font-size:10px}
.global-task-tooltip{position:fixed!important;max-width:min(520px,calc(100vw - 24px));min-width:min(340px,calc(100vw - 24px));max-height:min(410px,calc(100vh - 24px));overflow-y:auto;z-index:9999999!important}
.global-task-tooltip.tmp-tooltip-below::after{top:auto;bottom:100%;border-color:transparent transparent #fff transparent}
.tmp-tooltip-heading{color:#334155;font-size:11px;font-weight:750;border-bottom:1px solid #e5e7eb;padding-bottom:5px;margin-bottom:4px}
.tmp-current-task{background:#f5f0ff;padding:7px 8px;border-radius:6px;border:1px solid #ddd0ff;margin:4px 0 7px}
.tmp-current-task strong{display:block;color:#6939aa;font-size:10.5px;margin-bottom:3px}
.tmp-other-heading{font-weight:750;color:#b45309;font-size:11px;margin:5px 0}
@media(max-width:991px){.task-member-picker-root .task-member-columns{grid-template-columns:minmax(0,1fr) minmax(0,1fr);grid-template-rows:none}.tmp-table .tmp-left{min-width:220px;width:220px;max-width:220px}.tmp-timeline-tip{display:none}}
@media(max-width:620px){.tmp-auto-note{display:none}.tmp-timeline-filter{max-width:128px}.tmp-table .tmp-left{min-width:180px;width:180px;max-width:180px}.tmp-pop-head{max-width:100%}}
@media(prefers-reduced-motion:reduce){.tmp-calendar-pop{animation:none}}

/* ===============================================================
   RESOURCE TIMELINE V3 — use the visual vocabulary of Danh sach
   Only .tmp-* rules are overridden; the original list is unchanged.
   =============================================================== */
.task-member-picker-root .tmp-timeline{width:100%;min-width:0;min-height:0;flex:1 1 0}
.task-member-picker-root .tmp-timeline-bar{height:48px;min-height:48px;padding:7px 10px;gap:8px;background:#fff;flex-wrap:nowrap;border-bottom:1px solid #e2e8f0}
.task-member-picker-root .tmp-group-control{flex:0 0 151px;min-width:0;position:relative}
.task-member-picker-root .tmp-target-select{width:100%;height:33px;padding:4px 23px 4px 9px;border-radius:5px;border:1px solid #cbd5e1;background:#fff;color:#475569;font-weight:500;font-size:12px;cursor:pointer}
.task-member-picker-root .tmp-role-control{flex:0 0 145px;min-width:0}
.task-member-picker-root .tmp-role-select{display:block;width:100%;height:33px;padding:4px 21px 4px 8px;border:1px solid #7bc0fa;border-radius:3px;background:#42a5f5;color:white;font-weight:500;font-size:12px;cursor:pointer}
.task-member-picker-root .tmp-role-select option{background:white;color:#334155}
.task-member-picker-root .tmp-search-combo{flex:1 1 280px;width:auto;min-width:140px;max-width:410px;height:33px;display:flex}
.task-member-picker-root .tmp-search-combo .tmp-timeline-search{flex:1 1 auto;min-width:0;height:33px;max-width:none;margin:0;padding:6px 10px;border:1px solid #7c3db5;border-right:0!important;border-radius:4px 0 0 4px;background:white;color:#334155;font-size:12px}
.task-member-picker-root .tmp-search-combo .tmp-search-apply{flex:0 0 34px;height:33px;border:1px solid #6b2aa5;border-radius:0 4px 4px 0;background:white;color:#581c87;font-size:13px;padding:0;cursor:pointer}
.task-member-picker-root .tmp-search-combo .tmp-search-apply:hover{background:#f5edff}
.task-member-picker-root .tmp-only-picked{display:inline-flex;align-items:center;gap:5px;margin:0;white-space:nowrap;font-size:11px;color:#475569;cursor:pointer}
.task-member-picker-root .tmp-only-picked input{width:14px;height:14px;accent-color:#5b21b6}
.task-member-picker-root .tmp-timeline-tip{flex:0 0 auto;max-width:175px;overflow:hidden;text-overflow:ellipsis;margin-left:auto;font-size:10px}
.task-member-picker-root .tmp-active-filters{flex:0 0 27px;min-height:27px;height:27px;max-height:27px;display:flex;gap:5px;align-items:center;padding:3px 10px;background:#fff;overflow-x:auto;overflow-y:hidden;flex-wrap:nowrap;white-space:nowrap;border-bottom:1px solid #e9e5f0}
.task-member-picker-root .tmp-active-filters[hidden]{display:none!important}
.task-member-picker-root .tmp-filter-tag{display:inline-flex;gap:5px;align-items:center;flex:0 0 auto;padding:2px 7px;border:1px solid #e2d1f6;border-radius:5px;background:#f5f0ff;color:#5b218c;font-size:11px;font-weight:600;line-height:18px}
.task-member-picker-root .tmp-filter-tag button{display:inline-flex;align-items:center;justify-content:center;width:17px;height:17px;border:0;background:transparent;color:#5b218c;font-size:15px;cursor:pointer}
/* Fixed column: never let a task with only a few days expand this cell. */
.task-member-picker-root .tmp-table{min-width:0!important;width:max-content!important;table-layout:fixed;border-spacing:0;border-collapse:separate}
.task-member-picker-root .tmp-table col.tmp-person-col{width:360px!important}
.task-member-picker-root .tmp-table col.tmp-date-col{width:56px!important}
.task-member-picker-root .tmp-table .tmp-left{width:360px!important;min-width:360px!important;max-width:360px!important;left:0;position:sticky;z-index:5;background:#fff;box-shadow:2px 0 5px rgba(0,0,0,.055)}
.task-member-picker-root .tmp-table .tmp-date{min-width:56px!important;max-width:56px!important;width:56px!important;height:31px;font-size:11px}
.task-member-picker-root .tmp-table .tmp-month{height:24px}
.task-member-picker-root .tmp-table thead tr:nth-child(2) th{top:24px}
.task-member-picker-root .tmp-table .tmp-group td{height:32px}
.task-member-picker-root .tmp-table .tmp-employee td{height:63px}
.task-member-picker-root .tmp-table .tmp-person-cell{min-height:63px;gap:8px;padding:6px 9px;align-items:center}
.task-member-picker-root .tmp-table .tmp-person-cell input{flex:0 0 auto;width:16px;height:16px;accent-color:#2563eb}
.task-member-picker-root .tmp-person-avatar{display:flex;flex:0 0 26px;width:26px;height:26px;align-items:center;justify-content:center}
.task-member-picker-root .tmp-person-avatar .single-avatar-circle{width:26px;height:26px}
.task-member-picker-root .tmp-person-text{min-width:0;flex:1 1 auto;display:flex;flex-direction:column;gap:2px}
.task-member-picker-root .tmp-person-text strong{font-size:12px;font-weight:700;color:#243247;display:block;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
.task-member-picker-root .tmp-person-text small{font-size:10.5px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
.task-member-picker-root .tmp-person-badges{display:flex;align-items:center;flex-wrap:nowrap;gap:4px;overflow:hidden}
.task-member-picker-root .tmp-person-badges .badge{font-size:10.5px!important;font-weight:600!important;padding:3px 6px!important;white-space:nowrap;max-width:100%;overflow:hidden;text-overflow:ellipsis}
.task-member-picker-root .tmp-day{height:47px;border-radius:5px;margin:4px 2px;width:calc(100% - 4px);display:flex;flex-direction:column;justify-content:center;align-items:center;gap:2px;font-size:10px;font-weight:600;line-height:1.15;white-space:nowrap;color:#137333}
.task-member-picker-root .tmp-day.st-busy{color:#b91c1c;background:#fee2e2}
.task-member-picker-root .tmp-day.st-holiday{color:#b45309;background:#fef3c7}
.task-member-picker-root .tmp-day.st-weekend{color:#64748b;background:#f1f5f9}
.task-member-picker-root .tmp-day.st-free{color:#137333;background:#e6f4ea}
.task-member-picker-root .tmp-day.outside-task{outline:1px dashed rgba(100,116,139,.24);outline-offset:-2px}
.task-member-picker-root .tmp-date.outside-task{background:#f7f8fb;color:#8894a6}
.task-member-picker-root .tmp-date.inside-task{background:#f0e9fa;color:#552291}
.task-member-picker-root .tmp-date.task-boundary{box-shadow:inset 2px 0 #8546be}
.task-member-picker-root .tmp-day.has-current:after{background:#7c3aed;width:6px;height:6px;bottom:2px;right:2px}
.task-member-picker-root .tmp-table .tmp-group .tmp-left{background:#eaf2fa!important}
.task-member-picker-root .tmp-table .tmp-group.tmp-other .tmp-left{background:#fceff5!important}
.task-member-picker-root .tmp-empty{width:100%;min-width:360px;padding:25px 14px}
@media(max-width:1250px){.task-member-picker-root .tmp-table .tmp-left{width:330px!important;min-width:330px!important;max-width:330px!important}.task-member-picker-root .tmp-table col.tmp-person-col{width:330px!important}.task-member-picker-root .tmp-timeline-tip{display:none}}
@media(max-width:800px){.task-member-picker-root .tmp-table .tmp-left{width:300px!important;min-width:300px!important;max-width:300px!important}.task-member-picker-root .tmp-table col.tmp-person-col{width:300px!important}.task-member-picker-root .tmp-timeline-bar{gap:5px;padding:5px}.task-member-picker-root .tmp-group-control{flex-basis:125px}.task-member-picker-root .tmp-role-control{flex-basis:116px}.task-member-picker-root .tmp-only-picked span{display:none}}

/* Timeline filter refinements: exact visual language of the member list. */
.task-member-picker-root .tmp-timeline-bar{display:flex;flex-wrap:nowrap;align-items:center;gap:8px;min-width:0;padding:7px 10px;background:#fff}
.task-member-picker-root .tmp-role-control{flex:0 0 116px;min-width:116px;width:116px}
.task-member-picker-root .tmp-role-select{width:100%;max-width:100%;min-width:0;height:34px;padding:5px 23px 5px 9px;background-color:#42a5f5;border:1px solid #77bff8;border-radius:3px;color:#fff;font-size:12px;font-weight:600;white-space:nowrap;cursor:pointer}
.task-member-picker-root .tmp-search-combo{display:flex!important;flex:1 1 0!important;flex-wrap:nowrap!important;align-items:stretch;min-width:0!important;max-width:none!important;width:auto!important;height:34px!important;gap:0!important;margin:0!important}
.task-member-picker-root .tmp-search-combo .tmp-timeline-search{display:block!important;flex:1 1 0!important;width:0!important;min-width:0!important;max-width:none!important;height:34px!important;margin:0!important;border:1px solid #7c3db5!important;border-right:0!important;border-radius:4px 0 0 4px!important;padding:6px 10px!important;box-shadow:none!important;background:#fff!important;color:#334155!important;font-size:12px!important}
.task-member-picker-root .tmp-search-combo .tmp-search-apply{display:inline-flex!important;flex:0 0 36px!important;width:36px!important;min-width:36px!important;max-width:36px!important;height:34px!important;align-items:center;justify-content:center;padding:0!important;margin:0!important;border:1px solid #6b2aa5!important;border-radius:0 4px 4px 0!important;background:#fff!important;color:#581c87!important;box-shadow:none!important;position:static!important;line-height:1!important}
.task-member-picker-root .tmp-search-combo .tmp-search-apply i{font-size:13px;line-height:1}
.task-member-picker-root .tmp-only-picked{flex:0 0 auto;white-space:nowrap;margin:0 2px 0 4px}
.task-member-picker-root .tmp-timeline-tip{flex:0 1 auto;max-width:185px;min-width:0;margin-left:auto}
/* Match the Original ExtraSearchBox tag: white body, thin purple border and separated X. */
.task-member-picker-root .tmp-active-filters{flex:0 0 34px;min-height:34px;height:34px;max-height:34px;padding:4px 10px;gap:6px;overflow-x:auto;overflow-y:hidden;flex-wrap:nowrap;background:#fff;align-items:center}
.task-member-picker-root .tmp-active-filters[hidden]{display:none!important}
.task-member-picker-root .tmp-filter-tag{display:inline-flex;align-items:center;gap:0;flex:0 0 auto;background:#fff;border:1px solid #7137ae;border-radius:3px;color:#334155;padding:0 0 0 9px;height:25px;line-height:23px;font-size:11px;font-weight:500;white-space:nowrap}
.task-member-picker-root .tmp-filter-tag .tmp-tag-value{color:#64259b;font-weight:650;margin-left:4px}
.task-member-picker-root .tmp-filter-tag button{display:inline-flex;align-items:center;justify-content:center;flex:0 0 23px;width:23px;height:23px;border:0;border-left:1px solid #d5c5e7;border-radius:0;background:#fff;color:#5e2195;padding:0;margin:0 0 0 7px;font-size:16px;line-height:1;cursor:pointer}
.task-member-picker-root .tmp-filter-tag button:hover{background:#f7f1ff}
/* The list's native ExtraSearchBox tags were clipped by max-height:30px + overflow:hidden. */
.task-member-picker-root .panel-filter .listSearchTagBox{height:auto!important;max-height:none!important;min-height:0!important;overflow:visible!important;padding:2px 0 7px!important;margin:0!important;box-sizing:border-box;white-space:normal}
.task-member-picker-root .panel-filter .listSearchTagBox > *{max-width:100%}
@media(max-width:900px){.task-member-picker-root .tmp-timeline-tip{display:none}.task-member-picker-root .tmp-timeline-bar{gap:6px}.task-member-picker-root .tmp-role-control{flex-basis:110px;min-width:110px;width:110px}}
@media(max-width:560px){.task-member-picker-root .tmp-timeline-bar{padding:5px;gap:5px}.task-member-picker-root .tmp-role-control{flex:0 0 98px;min-width:98px;width:98px}.task-member-picker-root .tmp-only-picked span{display:none}}


/* === VIEW MODE + FIXED RESOURCE RAIL (2026-10-15) === */
.task-member-picker-root .tmp-tabs{gap:10px;height:36px;min-height:36px;margin-top:4px;border-bottom:1px solid #e7eaf1}
.task-member-picker-root .tmp-view-caption{color:#334155;font-size:12px;font-weight:750;white-space:nowrap}
.task-member-picker-root .tmp-view-switch{display:inline-flex;align-items:center;gap:0;margin-left:auto;border:1px solid #b9c8e3;border-radius:6px;overflow:hidden;margin-left:2px;background:#fff}
.task-member-picker-root .tmp-view-switch .tmp-tab{display:inline-flex;align-items:center;justify-content:center;width:34px;height:29px;padding:0;margin:0;border:0;border-radius:0;background:#fff;color:#63738a;font-size:14px;line-height:1;transition:background-color .15s,color .15s}
.task-member-picker-root .tmp-view-switch .tmp-tab + .tmp-tab{border-left:1px solid #d5dfec}
.task-member-picker-root .tmp-view-switch .tmp-tab.is-active{color:#1d63cc;background:#e0efff;box-shadow:inset 0 0 0 1px #9cc4ff}
.task-member-picker-root .tmp-view-switch .tmp-tab:hover:not(.is-active){background:#f3f7fd;color:#20599d}
.task-member-picker-root .tmp-view-switch{margin-left:auto}
.task-member-picker-root .tmp-auto-note{margin-left:8px}
/* Selected employees use the same subtle highlight in either view. */
.task-member-picker-root .member-item-row.is-selected{background:#f4effc!important;border-color:#c9b4ec!important;box-shadow:inset 3px 0 #7643b5}
.task-member-picker-root .member-item-row.is-selected:hover{background:#ece3fa!important}
.task-member-picker-root .member-item-row.original-member:not(.is-selected){background:#fff;border-color:#e2e8f0}
.task-member-picker-root .tmp-timeline-bar{justify-content:flex-start;gap:9px}
.task-member-picker-root .tmp-search-combo{flex:0 1 440px!important;width:440px!important;max-width:440px!important;min-width:150px!important}
.task-member-picker-root .tmp-search-combo .tmp-timeline-search{flex:1 1 auto!important}
.task-member-picker-root .tmp-timeline-tip{margin-left:auto}
/* A vertical board shared by two rails; the person rail never moves horizontally. */
.task-member-picker-root .tmp-scroll{overflow-x:hidden!important;overflow-y:auto!important;overscroll-behavior-x:contain;min-height:0}
.task-member-picker-root .tmp-board{--tmp-person-width:360px;--tmp-row-height:68px;--tmp-group-height:32px;--tmp-header-height:56px;display:grid;grid-template-columns:var(--tmp-person-width) minmax(0,1fr);min-width:0;width:100%;align-items:start}
.task-member-picker-root .tmp-board-names{width:var(--tmp-person-width);min-width:0;background:#fff;border-right:1px solid #dfe6ef;position:relative;z-index:2}
.task-member-picker-root .tmp-board-dates{overflow-x:auto;overflow-y:hidden;min-width:0;width:100%;scrollbar-width:none;overscroll-behavior-x:contain}
.task-member-picker-root .tmp-board-dates::-webkit-scrollbar{display:none}
.task-member-picker-root .tmp-board-track{width:max-content;min-width:100%}
.task-member-picker-root .tmp-board-person-head{height:var(--tmp-header-height);box-sizing:border-box;display:flex;align-items:center;padding:0 12px;background:#f1f5f9;color:#506179;font-weight:700;font-size:12px;border-bottom:1px solid #e4eaf0}
.task-member-picker-root .tmp-board-months{height:24px;display:flex;background:#f1f5f9}
.task-member-picker-root .tmp-board-month{height:24px;display:flex;align-items:center;justify-content:center;flex:0 0 auto;background:#f1f5f9;color:#566b89;font-weight:700;font-size:10.5px;border-right:1px solid #e4eaf0;box-sizing:border-box}
.task-member-picker-root .tmp-board-dayheads{height:32px;display:flex;background:#f1f5f9}
.task-member-picker-root .tmp-board-dayhead{width:56px;flex:0 0 56px;height:32px;box-sizing:border-box;display:flex;flex-direction:column;align-items:center;justify-content:center;gap:1px;font-size:10px;color:#506179;border-right:1px solid #e4eaf0;border-bottom:1px solid #e4eaf0}
.task-member-picker-root .tmp-board-dayhead span{font-size:9px;color:#7b8799}
.task-member-picker-root .tmp-board-dayhead.inside-task{background:#f0e9fa;color:#552291}
.task-member-picker-root .tmp-board-dayhead.outside-task{background:#f7f8fb;color:#8996a9}
.task-member-picker-root .tmp-board-dayhead.task-boundary{box-shadow:inset 2px 0 #8546be}
.task-member-picker-root .tmp-board-group,.task-member-picker-root .tmp-board-group-days{height:var(--tmp-group-height);box-sizing:border-box;border-bottom:1px solid #e6edf4;background:#eaf2fa}
.task-member-picker-root .tmp-board-group.tmp-other,.task-member-picker-root .tmp-board-group-days.tmp-other{background:#fceff5}
.task-member-picker-root .tmp-board-group button{display:block;text-align:left;border:0;background:transparent;color:#234e7d;font-size:11px;font-weight:750;padding:0 12px;width:100%;height:100%;cursor:pointer}
.task-member-picker-root .tmp-board-group.tmp-other button{color:#9d174d}
.task-member-picker-root .tmp-board-person-row,.task-member-picker-root .tmp-board-dayrow{height:var(--tmp-row-height);box-sizing:border-box;border-bottom:1px solid #e7edf3;background:#fff}
.task-member-picker-root .tmp-board-person-row{min-width:0}
.task-member-picker-root .tmp-board-person-row.is-selected,.task-member-picker-root .tmp-board-dayrow.is-selected{background:#faf5ff}
.task-member-picker-root .tmp-board-person-row.is-selected{box-shadow:inset 3px 0 #7c3aed}
.task-member-picker-root .tmp-board-person-row .tmp-person-cell{height:100%;display:flex;min-width:0;align-items:center;gap:8px;padding:6px 9px;box-sizing:border-box}
.task-member-picker-root .tmp-board-person-row .tmp-person-cell>input{flex:0 0 16px;width:16px;height:16px;accent-color:#2563eb}
.task-member-picker-root .tmp-board-person-row .tmp-person-avatar{display:flex;align-items:center;justify-content:center;flex:0 0 26px;width:26px;height:26px}
.task-member-picker-root .tmp-board-person-row .tmp-person-text{display:flex;flex:1 1 auto;min-width:0;flex-direction:column;gap:2px}
.task-member-picker-root .tmp-board-person-row .tmp-person-text strong{color:#243247;font-size:12px;white-space:nowrap;text-overflow:ellipsis;overflow:hidden}
.task-member-picker-root .tmp-board-person-row .tmp-person-text small{font-size:10.5px;color:#77849a;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.task-member-picker-root .tmp-board-person-row .tmp-person-badges{display:flex;overflow:hidden;min-width:0}
.task-member-picker-root .tmp-board-person-row .tmp-person-badges .badge{max-width:100%;font-size:10.5px!important;padding:3px 6px!important;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
.task-member-picker-root .tmp-board-dayrow{display:flex;min-width:0}
.task-member-picker-root .tmp-board-daycell{width:56px;flex:0 0 56px;height:var(--tmp-row-height);box-sizing:border-box;border-right:1px solid #f1f3f7;padding:5px 2px;display:flex;align-items:center;justify-content:center}
.task-member-picker-root .tmp-board-daycell .tmp-day{display:flex!important;align-items:center;justify-content:center;flex-direction:column;width:100%!important;height:47px!important;margin:0!important}
.task-member-picker-root .tmp-board-no-results{padding:25px 12px;color:#64748b;font-size:12px}
/* This scrollbar controls only the date rail, never the employee names. */
.task-member-picker-root .tmp-timeline-xbar{flex:0 0 14px;height:14px;margin-left:360px;overflow-x:auto;overflow-y:hidden;background:#fff;border-top:1px solid #eef1f6;scrollbar-color:#b8c5d8 #f1f5f9;overscroll-behavior-x:contain}
.task-member-picker-root .tmp-timeline-xbar[hidden]{display:none!important}
.task-member-picker-root .tmp-timeline-xbar>div{height:1px}
@media(max-width:1250px){.task-member-picker-root .tmp-board{--tmp-person-width:330px}.task-member-picker-root .tmp-timeline-xbar{margin-left:330px}}
@media(max-width:800px){.task-member-picker-root .tmp-board{--tmp-person-width:300px}.task-member-picker-root .tmp-timeline-xbar{margin-left:300px}}
@media(max-width:700px){.task-member-picker-root .tmp-search-combo{flex:1 1 auto!important;width:auto!important;min-width:110px!important}.task-member-picker-root .tmp-timeline-tip{display:none}.task-member-picker-root .tmp-view-caption{display:none}}


/* V5: the two views use exactly the same SweetSoft server controls and styles. */
.task-member-picker-root .tmp-tabs{display:flex;align-items:center;gap:12px;min-height:45px;height:45px;margin-top:3px;padding-bottom:5px;border-bottom:1px solid #e8e6ed}
.task-member-picker-root .tmp-assignment-notice{flex:1 1 auto;min-width:0;display:flex;align-items:center;gap:8px;padding:7px 11px;border:1px solid #fed7aa;border-radius:6px;background:#fff8ef;color:#b45309;font-size:11.5px;line-height:1.35}
.task-member-picker-root .tmp-assignment-notice i{flex:0 0 auto;font-size:13px}
.task-member-picker-root .tmp-view-switch{margin-left:auto!important;margin-right:0!important;flex:0 0 auto}
.task-member-picker-root .tmp-timeline-bar{display:flex!important;align-items:center;gap:8px;height:auto;min-height:49px;padding:7px 10px;background:#fff;flex-wrap:nowrap}
.task-member-picker-root .tmp-native-role{flex:0 0 auto;min-width:110px;max-width:160px;display:flex;align-items:center}
.task-member-picker-root .tmp-native-role > *{max-width:100%}
.task-member-picker-root .tmp-native-search{flex:0 1 450px!important;width:min(450px,100%)!important;min-width:140px!important;max-width:450px!important;margin:0!important;display:flex;flex-wrap:nowrap!important;align-items:stretch}
.task-member-picker-root .tmp-native-search .input-search-filter{flex:1 1 0!important;min-width:0!important;width:0!important}
.task-member-picker-root .tmp-native-search .btn-search-filter{flex:0 0 37px!important;min-width:37px!important;margin:0!important}
.task-member-picker-root .tmp-timeline-tip{margin-left:auto;font-size:10px;color:#64748b}
.task-member-picker-root .tmp-timeline-tags{min-height:0!important;height:auto!important;max-height:42px!important;flex:0 0 auto;padding:2px 9px 5px;background:#fff;overflow-x:auto;overflow-y:hidden;white-space:nowrap;box-sizing:border-box}
.task-member-picker-root .tmp-timeline-tags[hidden]{display:none!important}
.task-member-picker-root .tmp-timeline-tags > span,.task-member-picker-root .tmp-timeline-tags > div{max-width:100%}
/* A real fixed header: top header is OUTSIDE the vertical scroll rails. */
.task-member-picker-root .tmp-scroll{flex:1 1 0;min-height:0;overflow:hidden!important;display:flex!important;min-width:0}
.task-member-picker-root .tmp-board{--tmp-person-width:360px;--tmp-row-height:83px;--tmp-header-height:56px;display:grid!important;grid-template-columns:var(--tmp-person-width) minmax(0,1fr)!important;grid-template-rows:var(--tmp-header-height) minmax(0,1fr)!important;height:100%!important;width:100%!important;min-width:0;min-height:0;overflow:hidden;align-items:stretch}
.task-member-picker-root .tmp-board-head-names{grid-column:1;grid-row:1;background:#f1f5f9;border-right:1px solid #dfe6ef;z-index:3}
.task-member-picker-root .tmp-board-head-dates{grid-column:2;grid-row:1;min-width:0;overflow:hidden;background:#f1f5f9;z-index:3}
.task-member-picker-root .tmp-board-names{grid-column:1;grid-row:2;width:100%;min-width:0;min-height:0;height:100%;overflow-y:auto;overflow-x:hidden;scrollbar-width:none;border-right:1px solid #dfe6ef;z-index:2}
.task-member-picker-root .tmp-board-names::-webkit-scrollbar{display:none}
.task-member-picker-root .tmp-board-dates{grid-column:2;grid-row:2;min-width:0;min-height:0;width:100%;height:100%;overflow:auto!important;scrollbar-color:#b8c5d8 #f1f5f9;overscroll-behavior:contain}
.task-member-picker-root .tmp-board-person-head{height:var(--tmp-header-height)}
.task-member-picker-root .tmp-board-person-row,.task-member-picker-root .tmp-board-dayrow{height:var(--tmp-row-height)}
.task-member-picker-root .tmp-board-daycell{height:var(--tmp-row-height)}
.task-member-picker-root .tmp-board-daycell .tmp-day{height:57px!important}
.task-member-picker-root .tmp-board-person-row .tmp-person-cell{gap:7px;padding:6px 9px;align-items:center}
.task-member-picker-root .tmp-board-person-row .tmp-person-avatar{flex:0 0 27px;width:27px;height:27px}
.task-member-picker-root .tmp-board-person-row .tmp-person-text{gap:2px;justify-content:center;overflow:visible}
.task-member-picker-root .tmp-person-headline{display:flex;align-items:center;flex-wrap:wrap;column-gap:4px;row-gap:1px;min-width:0;font-size:11.5px;line-height:1.2}
.task-member-picker-root .tmp-person-headline strong{display:inline!important;overflow:visible!important;text-overflow:clip!important;white-space:normal!important;max-width:100%;color:#243247}
.task-member-picker-root .tmp-person-position{color:#64748b;font-size:10.5px;white-space:normal;overflow-wrap:anywhere}
.task-member-picker-root .tmp-person-email{font-size:10.3px;color:#71809a;line-height:1.2;white-space:normal;overflow-wrap:anywhere;min-width:0}
.task-member-picker-root .tmp-person-pm.badge{padding:2px 5px!important;font-size:9.5px!important;line-height:1.1;color:#fff!important}
.task-member-picker-root .tmp-board-person-row .tmp-person-badges{min-height:18px;overflow:visible}
.task-member-picker-root .tmp-board-person-row .tmp-person-badges .badge{max-width:100%;text-overflow:ellipsis;font-size:10px!important;padding:3px 6px!important}
@media(max-width:1250px){.task-member-picker-root .tmp-board{--tmp-person-width:330px}.task-member-picker-root .tmp-timeline-xbar{margin-left:330px}}
@media(max-width:800px){.task-member-picker-root .tmp-board{--tmp-person-width:300px}.task-member-picker-root .tmp-timeline-xbar{margin-left:300px}}
@media(max-width:750px){.task-member-picker-root .tmp-assignment-notice{font-size:10px;padding:6px}.task-member-picker-root .tmp-timeline-tip{display:none}}


/* Chỉ giảm nhẹ font trong bộ lọc / tìm kiếm của chế độ Lịch trình. */
.task-member-picker-root #tmpPaneTimeline .tmp-native-role .dropdown-toggle,
.task-member-picker-root #tmpPaneTimeline .tmp-native-role .btn,
.task-member-picker-root #tmpPaneTimeline .tmp-native-role .bootstrap-select > .btn,
.task-member-picker-root #tmpPaneTimeline .tmp-native-role .filter-option-inner-inner {font-size:12px!important;}
.task-member-picker-root #tmpPaneTimeline .tmp-native-search .input-search-filter,
.task-member-picker-root #tmpPaneTimeline .tmp-native-search .input-search-filter::placeholder {font-size:12px!important;}
.task-member-picker-root #tmpPaneTimeline .tmp-native-search .btn-search-filter,
.task-member-picker-root #tmpPaneTimeline .tmp-native-search .btn-search-filter i {font-size:12px!important;}


/* Smaller native ExtraSearchBox tags ONLY in Timeline (the List remains unchanged). */
.task-member-picker-root #tmpPaneTimeline #tmpTimelineTags{font-size:10.5px!important;line-height:18px!important;padding:1px 9px 3px!important;max-height:35px!important}
.task-member-picker-root #tmpPaneTimeline #tmpTimelineTags :is(span,a,button,.btn,.badge,strong,b,small){font-size:10.5px!important;line-height:18px!important}
.task-member-picker-root #tmpPaneTimeline #tmpTimelineTags :is(.tag,.tag-item,.badge,.btn-group,[class*="search-tag"],[class*="SearchTag"],[class*="tag-item"]){min-height:21px!important;height:auto!important;line-height:18px!important}
.task-member-picker-root #tmpPaneTimeline #tmpTimelineTags :is(.tag,.tag-item,.badge,.btn){padding-top:1px!important;padding-bottom:1px!important}
.task-member-picker-root #tmpPaneTimeline #tmpTimelineTags :is(button,.close,[class*="close"],.btn-close){font-size:12px!important;line-height:18px!important;padding-top:0!important;padding-bottom:0!important}

/* V7: Timeline filter uses the SAME wrapper/layout as List filter, not an inline UpdatePanel.
   Responsive group follows the half-width List column; long BootstrapDropdown labels grow naturally. */
.task-member-picker-root #tmpPaneTimeline .tmp-native-filter-row{
    flex:0 1 calc(50% - 8px);
    width:calc(50% - 8px);
    max-width:650px;
    min-width:0;
    margin:0!important;
    flex-wrap:nowrap!important;
    gap:8px!important;
    align-items:stretch;
}
.task-member-picker-root #tmpPaneTimeline .tmp-native-filter-row > [id$="upnlTimelineRole"]{
    flex:0 0 auto;
    width:auto!important;
    min-width:0!important;
    max-width:none!important;
}
.task-member-picker-root #tmpPaneTimeline .tmp-native-filter-row .tmp-native-role{
    display:block!important;
    flex:none!important;
    width:auto!important;
    min-width:0!important;
    max-width:none!important;
}
.task-member-picker-root #tmpPaneTimeline .tmp-native-filter-row .tmp-native-role > *{
    max-width:none!important;
}
.task-member-picker-root #tmpPaneTimeline .tmp-native-filter-row .tmp-native-search{
    display:flex!important;
    flex:1 1 0!important;
    width:auto!important;
    min-width:0!important;
    max-width:none!important;
    margin:0!important;
    flex-wrap:nowrap!important;
}
.task-member-picker-root #tmpPaneTimeline .tmp-native-filter-row .tmp-native-search .input-search-filter{
    flex:1 1 auto!important;
    width:1%!important;
    min-width:0!important;
}
@media(max-width:1050px){
    .task-member-picker-root #tmpPaneTimeline .tmp-native-filter-row{
        flex:1 1 auto;
        width:auto;
        max-width:calc(100% - 16px);
    }
}

/* Nền tím nhạt cho các ngày thuộc thời hạn task */
.task-member-picker-root .tmp-board-daycell.inside-task {
    background: #f7f2ff;
}

/* Nền trung tính cho các ngày ngoài thời hạn */
.task-member-picker-root .tmp-board-daycell.outside-task {
    background: #fbfcfe;
}

/* Viền nhẹ đánh dấu ngày bắt đầu và kết thúc */
.task-member-picker-root .tmp-board-daycell.task-boundary-start {
    box-shadow: inset 2px 0 0 rgba(124,58,237,.18);
}

.task-member-picker-root .tmp-board-daycell.task-boundary-end {
    box-shadow: inset -2px 0 0 rgba(124,58,237,.18);
}


/* V3 stable redesign: real SweetSoft controls; card template cloned from the original Repeaters. */
.task-member-picker-root .tmp-legacy-sources{display:none!important}
.task-member-picker-root .tmp-v3-columns{grid-template-columns:repeat(2,minmax(0,1fr))!important;gap:14px!important}
.task-member-picker-root .tmp-v3-columns .member-list-panel{min-width:0;overflow:hidden}
.task-member-picker-root .tmp-v3-columns .panel-header{height:45px;min-height:45px;padding:9px 12px;background:#f1f5f9;color:#334e75}
.task-member-picker-root .tmp-v3-columns .tmp-v3-picked-header{background:#f4edfc;color:#58248a}
.task-member-picker-root .tmp-v3-columns .tmp-v3-available-header{background:#edf3fa;color:#225685}
.task-member-picker-root .tmp-v3-list{flex:1;min-height:0;overflow:auto;background:#f8fafc;padding:10px}
.task-member-picker-root .tmp-v3-list .member-item-row{margin-bottom:8px;min-height:58px;border-radius:8px}
.task-member-picker-root .tmp-v3-list .row-default-view{display:flex;align-items:center;gap:8px;min-height:58px!important;padding:7px 9px!important;box-sizing:border-box}
.task-member-picker-root .tmp-v3-list .member-info-group{display:flex;align-items:center;gap:8px;min-width:0;flex:1 1 0}
.task-member-picker-root .tmp-v3-list .member-name-block{min-width:0;flex:1 1 0}
.task-member-picker-root .tmp-v3-list .member-name-block>div{display:flex!important;align-items:center!important;gap:4px!important;flex-wrap:nowrap!important;margin-bottom:2px!important;min-width:0}
.task-member-picker-root .tmp-v3-list .member-name-block .fw-bold,.task-member-picker-root .tmp-v3-list .member-position{font-size:11.5px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;flex:0 1 auto;min-width:0;margin:0!important}
.task-member-picker-root .tmp-v3-list .member-email{display:block;font-size:10.5px;line-height:1.3;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
.task-member-picker-root .tmp-v3-list .row-default-view>.d-flex{display:flex!important;align-items:center!important;gap:7px!important;flex:0 0 auto;flex-wrap:nowrap!important;min-width:0;margin:0!important}
.task-member-picker-root .tmp-v3-list .row-default-view>.d-flex .badge{font-size:10px!important;padding:4px 6px!important;line-height:1.15!important;white-space:nowrap!important;max-width:125px;overflow:hidden;text-overflow:ellipsis}
.task-member-picker-root .tmp-v3-list .btn-calendar-only{flex:0 0 31px;width:31px;height:31px}
.task-member-picker-root .tmp-v3-list .member-item-row.is-selected{background:#f7f1ff;border-color:#d8c3f6;box-shadow:inset 3px 0 #7c3aed}
.task-member-picker-root .tmp-v3-legend{height:26px;flex:0 0 26px;padding:5px 11px;display:flex;gap:15px;align-items:center;justify-content:flex-start;font-size:10.5px;color:#64748b;background:#fff;border-bottom:1px solid #edf2f7}
.task-member-picker-root .tmp-v3-dot{display:inline-block;width:8px;height:8px;border-radius:50%;vertical-align:middle;margin:0 5px 1px 0}
.task-member-picker-root .tmp-v3-dot.project{background:#16a34a}.task-member-picker-root .tmp-v3-dot.other{background:#94a3b8}
.task-member-picker-root .tmp-v3-empty{padding:34px 12px;text-align:center;color:#8793a6;font-size:12px;line-height:1.5}
.task-member-picker-root .tmp-v3-toolbar{flex:0 0 auto;display:flex;align-items:center;gap:8px;width:100%;min-width:0;height:47px;padding:5px 9px;background:#fff;border:1px solid #e7eaf2;border-radius:7px;margin:1px 0 8px;box-sizing:border-box}
.task-member-picker-root .tmp-v3-filter-label{flex:0 0 auto;color:#64748b;font-size:10.5px;font-weight:700;white-space:nowrap}
.task-member-picker-root .tmp-v3-filter-controls{display:flex!important;align-items:center;flex:1 1 auto!important;min-width:0;max-width:none!important;width:auto!important;gap:8px!important;margin:0!important;flex-wrap:nowrap!important}
.task-member-picker-root .tmp-v3-dropdown-pair{display:flex!important;align-items:center!important;gap:8px!important;flex:0 0 auto;min-width:0;max-width:390px}
.task-member-picker-root .tmp-v3-dropdown-pair>div,.task-member-picker-root .tmp-v3-dropdown-pair>.bootstrap-select{min-width:0}
.task-member-picker-root .tmp-v3-dropdown-pair .dropdown-toggle,.task-member-picker-root .tmp-v3-dropdown-pair .btn{font-size:11.5px!important;white-space:nowrap}
.task-member-picker-root .tmp-v3-filter-controls .tmp-native-search{flex:1 1 170px!important;min-width:110px!important;max-width:360px!important;width:auto!important;margin:0!important}
.task-member-picker-root .tmp-v3-toolbar #tmpTimelineTags{flex:0 1 260px!important;min-width:0!important;max-width:260px!important;height:31px!important;max-height:31px!important;padding:3px 0!important;border:0!important;margin:0!important;overflow:auto hidden!important;white-space:nowrap!important}
.task-member-picker-root .tmp-v3-toolbar #tmpTimelineTags:empty{display:none!important}
.task-member-picker-root .tmp-v3-toolbar .listSearchTagBox{flex:0 1 260px}
.task-member-picker-root .tmp-timeline-window-nav{display:flex;justify-content:flex-end;align-items:center;gap:5px;position:absolute;top:3px;right:10px;z-index:4;pointer-events:none}
.task-member-picker-root .tmp-timeline-window-nav button{pointer-events:auto}
.task-member-picker-root .tmp-timeline-window-nav .tmp-timeline-tip{display:none}
.task-member-picker-root #tmpPaneTimeline{min-height:0}
.task-member-picker-root .tmp-board-group.tmp-selected,.task-member-picker-root .tmp-board-group-days.tmp-selected{background:#f3eafb!important}
.task-member-picker-root .tmp-board-group.tmp-selected button{color:#58248a!important}
.task-member-picker-root .tmp-board-person-row .tmp-person-source{display:inline-flex;align-items:center}
.task-member-picker-root .tmp-board-daycell.outside-task{background:#fafbfe!important}
.task-member-picker-root .tmp-board-daycell.outside-task .tmp-day{background:#f1f3f7!important;border:1px solid #e5e9f0!important;color:#94a3b8!important}
.task-member-picker-root .tmp-board-daycell.outside-task .tmp-day .tmp-v3-context-dot{width:6px;height:6px;background:#a9b5c9;border-radius:50%}
.task-member-picker-root .tmp-board-daycell.inside-task{background:#f7f2ff!important}
@media(max-width:1000px){.task-member-picker-root .tmp-v3-filter-label{display:none}.task-member-picker-root .tmp-v3-dropdown-pair{gap:5px}.task-member-picker-root .tmp-v3-toolbar #tmpTimelineTags{max-width:140px!important}.task-member-picker-root .tmp-v3-list .row-default-view>.d-flex .badge{max-width:105px}}

/* 3.0 hotfix: task hiện tại có lịch riêng; không còn nhầm với ngày Trống. */
.task-member-picker-root .tmp-calendar-pop .mc-day.st-current-only,
.task-member-picker-root .tmp-board-daycell .tmp-day.st-current-only {
    background:#efe5ff!important;color:#6633a3!important;border:1px solid #d4baf5!important;
}
.task-member-picker-root .tmp-calendar-pop .mc-day.st-current-only .mc-label {
    color:#6633a3!important;font-weight:700;
}
/* Chuẩn hóa toàn bộ thanh lọc: không còn flex trống lớn trước SearchTag. */
.task-member-picker-root .tmp-v3-toolbar {justify-content:flex-start;gap:8px!important;}
.task-member-picker-root .tmp-v3-toolbar .tmp-v3-filter-controls {
    display:flex!important;flex:0 1 auto!important;width:auto!important;min-width:0!important;
    max-width:calc(100% - 200px)!important;gap:8px!important;align-items:center!important;
}
.task-member-picker-root .tmp-v3-toolbar .tmp-v3-dropdown-pair {
    flex:0 0 auto!important;max-width:none!important;width:auto!important;gap:8px!important;
}
.task-member-picker-root .tmp-v3-dropdown-pair > *:first-child,
.task-member-picker-root .tmp-v3-dropdown-pair > *:first-child .bootstrap-select {min-width:104px!important;width:auto!important;}
.task-member-picker-root .tmp-v3-dropdown-pair > *:nth-child(2),
.task-member-picker-root .tmp-v3-dropdown-pair > *:nth-child(2) .bootstrap-select {min-width:130px!important;width:auto!important;}
.task-member-picker-root .tmp-v3-dropdown-pair .bootstrap-select > button,
.task-member-picker-root .tmp-v3-dropdown-pair .dropdown-toggle {min-height:33px!important;height:33px!important;padding:5px 10px!important;font-size:12px!important;}
.task-member-picker-root .tmp-v3-toolbar .tmp-native-search {
    flex:0 1 335px!important;width:335px!important;min-width:195px!important;max-width:335px!important;
}
.task-member-picker-root .tmp-v3-toolbar #tmpTimelineTags {
    flex:0 1 360px!important;width:auto!important;min-width:0!important;max-width:360px!important;
    height:33px!important;max-height:33px!important;overflow-x:auto!important;overflow-y:hidden!important;
    margin:0!important;padding:3px 0!important;text-align:left!important;
}
.task-member-picker-root .tmp-v3-toolbar #tmpTimelineTags > * {max-width:none!important;min-width:max-content;}
@media(max-width:1100px){
    .task-member-picker-root .tmp-v3-toolbar .tmp-native-search{flex-basis:240px!important;width:240px!important;min-width:165px!important}
    .task-member-picker-root .tmp-v3-toolbar #tmpTimelineTags{max-width:185px!important}
}

/* Timeline v3.1: hai nhóm có header luôn thấy và cuộn dọc riêng.
   Vẫn chỉ có một thanh cuộn ngang điều khiển đồng bộ hai nhóm. */
.task-member-picker-root .tmp-board-group-stack{grid-column:1 / -1;grid-row:2;display:flex;flex-direction:column;min-width:0;min-height:0;overflow:hidden}
.task-member-picker-root .tmp-board-section{display:grid;grid-template-columns:var(--tmp-person-width) minmax(0,1fr);grid-template-rows:var(--tmp-group-height) minmax(0,1fr);flex:0 0 auto;min-width:0;min-height:var(--tmp-group-height);width:100%;overflow:hidden}
.task-member-picker-root .tmp-board-section > .tmp-board-group{grid-column:1;grid-row:1;min-width:0;z-index:1}
.task-member-picker-root .tmp-board-section > .tmp-board-group-days{grid-column:2;grid-row:1;min-width:0}
.task-member-picker-root .tmp-board-section-names{grid-column:1;grid-row:2;min-height:0;min-width:0;overflow-y:auto;overflow-x:hidden;scrollbar-width:thin;scrollbar-color:#b5c3d7 transparent;border-right:1px solid #dfe6ef}
.task-member-picker-root .tmp-board-section-dates{grid-column:2;grid-row:2;min-height:0;min-width:0;overflow:auto;scrollbar-width:none;overscroll-behavior:contain}
.task-member-picker-root .tmp-board-section-dates::-webkit-scrollbar{display:none}
.task-member-picker-root .tmp-board-group.tmp-selected,.task-member-picker-root .tmp-board-group-days.tmp-selected{background:#f3eafb!important}
.task-member-picker-root .tmp-board-section .tmp-board-group button{height:100%}
.task-member-picker-root .tmp-board-section.is-collapsed{height:var(--tmp-group-height)!important}
.task-member-picker-root .tmp-board-section.is-collapsed .tmp-board-section-names,.task-member-picker-root .tmp-board-section.is-collapsed .tmp-board-section-dates{display:none}
.task-member-picker-root .tmp-board-section .tmp-board-no-results{padding:13px 12px;font-size:11px}

/* ===============================================================
   UX V1 (2026-10-21): khong doi palette, badge hoac logic phan cong.
   Tach rieng nang cap de co the rollback dung pham vi.
   =============================================================== */
.task-member-picker-root{--tmp-ux-purple:#64259b;--tmp-ux-purple-border:#d8c3f1;--tmp-ux-muted:#64748b}
.task-member-picker-root .tmp-tabs{gap:10px;min-height:42px;height:auto;padding-bottom:5px}
.task-member-picker-root .tmp-assignment-notice{padding:7px 10px;min-width:0;line-height:1.35}
.task-member-picker-root .tmp-assignment-notice span{display:block;min-width:0}
.task-member-picker-root .tmp-view-switch{flex:0 0 auto!important;border-color:#cbb6e5;border-radius:8px;box-shadow:0 1px 3px rgba(60,25,108,.055)}
.task-member-picker-root .tmp-view-switch .tmp-tab{width:auto!important;min-width:103px;height:33px;min-height:33px;padding:0 12px!important;display:inline-flex;align-items:center;justify-content:center;gap:7px;font-size:12px;font-weight:700;color:#675b78;transition:color .18s ease,background-color .18s ease,box-shadow .18s ease}
.task-member-picker-root .tmp-view-switch .tmp-tab i{font-size:12px}
.task-member-picker-root .tmp-view-switch .tmp-tab.is-active{background:#f0e8fc!important;color:#64259b!important;box-shadow:inset 0 -2px #7c3aed!important}
.task-member-picker-root .tmp-view-switch .tmp-tab:hover:not(.is-active){background:#f8f4fe;color:#64259b}
.task-member-picker-root .tmp-view-switch .tmp-tab:focus-visible{outline:2px solid #7c3aed;outline-offset:-2px}
.task-member-picker-root .tmp-v3-columns{gap:12px!important}
.task-member-picker-root .tmp-v3-columns>.member-list-panel{border-color:#e3e6ed;border-radius:9px;box-shadow:0 1px 4px rgba(25,40,70,.035)}
.task-member-picker-root .tmp-v3-columns .panel-header{min-height:42px;gap:8px}
.task-member-picker-root .tmp-v3-columns .panel-header>span:first-child{display:flex;align-items:center;gap:4px;flex-wrap:wrap;min-width:0}
.task-member-picker-root .tmp-ux-panel-hint{display:inline-block;color:#8691a5;font-size:10px;font-weight:500;margin-left:5px;white-space:nowrap}
.task-member-picker-root .tmp-v3-list{padding:9px!important}
.task-member-picker-root .tmp-v3-list .member-item-row{margin-bottom:7px;border-radius:8px;transition:border-color .17s ease,box-shadow .17s ease,background-color .17s ease,transform .17s ease}
.task-member-picker-root .tmp-v3-list .member-item-row.tmp-ux-clickable{cursor:pointer}
.task-member-picker-root .tmp-v3-list .member-item-row.tmp-ux-clickable:hover{border-color:#d0b9ea;box-shadow:0 4px 12px rgba(68,33,120,.075);transform:translateY(-1px)}
.task-member-picker-root .tmp-v3-list .member-item-row.tmp-ux-clickable.is-selected:hover{background:#f3ebff!important;border-color:#bfa3e4!important}
.task-member-picker-root .tmp-v3-list .member-item-row.tmp-ux-clickable:focus-within{outline:2px solid #b79bdc;outline-offset:0}
.task-member-picker-root .tmp-v3-list .btn-calendar-only{color:#64259b;border-color:#ded1ed;background:#fff;transition:background .17s ease,border-color .17s ease,color .17s ease,transform .17s ease}
.task-member-picker-root .tmp-v3-list .btn-calendar-only:hover{background:#f4edfd;border-color:#a982d3;color:#54218d;transform:translateY(-1px)}
.task-member-picker-root .tmp-v3-list .btn-calendar-only:focus-visible{outline:2px solid #7c3aed;outline-offset:2px}
.task-member-picker-root .tmp-v3-list .member-name-block .fw-bold{font-weight:700!important;color:#263247!important}
.task-member-picker-root .tmp-v3-empty{padding:28px 16px;line-height:1.6;border:1px dashed #e1d9ee;background:#fcfbff;border-radius:8px;margin:3px;color:#758397}
.task-member-picker-root .tmp-board-dayhead.is-today{position:relative;background:#ede0ff!important;color:#58248a!important;box-shadow:inset 0 2px #7c3aed,inset 1px 0 #d8c3ef}
.task-member-picker-root .tmp-board-dayhead.is-today b{color:#58248a;font-size:12px}
.task-member-picker-root .tmp-board-dayhead.is-today:after{content:'';position:absolute;bottom:1px;width:4px;height:4px;border-radius:50%;background:#7c3aed}
.task-member-picker-root .tmp-board-dayhead.is-weekend:not(.is-today) span{color:#8693a5}
.task-member-picker-root .tmp-board-person-row:hover,.task-member-picker-root .tmp-board-dayrow:hover{background:#fbf8ff}
.task-member-picker-root .tmp-board-person-row.is-selected:hover,.task-member-picker-root .tmp-board-dayrow.is-selected:hover{background:#f5edff}
.task-member-picker-root .tmp-board-daycell .tmp-day:hover{filter:brightness(.965);outline:1px solid rgba(100,37,155,.25);outline-offset:-1px}
.task-assignment-footer-row .tmp-ux-pending{flex:0 0 auto;display:inline-flex;align-items:center;justify-content:center;gap:6px;border:1px solid #e2e8f0;border-radius:6px;padding:5px 8px;background:#f8fafc;color:#64748b;font-size:11px;font-weight:650;white-space:nowrap}
.task-assignment-footer-row .tmp-ux-pending[hidden]{display:none!important}
.task-assignment-footer-row .tmp-ux-pending:before{content:'';width:6px;height:6px;border-radius:50%;background:#a8b4c2}
.task-assignment-footer-row .tmp-ux-pending.has-changes{background:#fff8ed;border-color:#f7ddb3;color:#9a5a11}
.task-assignment-footer-row .tmp-ux-pending.has-changes:before{background:#ea9a27}
.task-assignment-footer-row .task-assignment-summary{overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
@media(max-width:1050px){.task-member-picker-root .tmp-ux-panel-hint{display:none}}
@media(max-width:750px){.task-member-picker-root .tmp-tabs{align-items:flex-start;flex-wrap:wrap}.task-member-picker-root .tmp-assignment-notice{flex:1 1 100%;order:2}.task-member-picker-root .tmp-view-switch{order:1;margin-left:auto!important}.task-member-picker-root .tmp-v3-columns{gap:8px!important}.task-member-picker-root .tmp-v3-list{padding:6px!important}.task-member-picker-root .tmp-view-switch .tmp-tab{min-width:85px;padding:0 8px!important}}
@media(max-width:600px){.task-assignment-footer-row{flex-wrap:wrap}.task-assignment-footer-row .task-assignment-summary{flex:1 1 140px}.task-member-picker-root .tmp-v3-columns .panel-header{font-size:12px}}
@media(prefers-reduced-motion:reduce){.task-member-picker-root .tmp-v3-list .member-item-row,.task-member-picker-root .tmp-view-switch .tmp-tab,.task-member-picker-root .tmp-v3-list .btn-calendar-only{transition:none!important;transform:none!important}}

/* UX V2: original employee stays visible until Save, even when unchecked.
   Keep the existing purple/green badges; amber is reserved for pending removals. */
.task-member-picker-root .tmp-v3-list .member-item-row.tmp-pending-removal {
    background:#fff8ef!important;
    border:1px solid #f5ccaa!important;
    box-shadow:inset 3px 0 #e69a44!important;
}
.task-member-picker-root .tmp-v3-list .member-item-row.tmp-pending-removal:hover {
    background:#fff2e3!important;
    border-color:#e9b477!important;
    box-shadow:inset 3px 0 #dd8a2d,0 4px 12px rgba(138,75,20,.07)!important;
}
.task-member-picker-root .tmp-v3-list .member-item-row.tmp-pending-removal .row-default-view {
    opacity:1;
}
.task-member-picker-root .tmp-pending-removal-badge {
    display:inline-flex;align-items:center;gap:4px;
    color:#974c17!important;background:#fff0dc!important;
    border:1px solid #f1c391!important;
    font-weight:700!important;
}
.task-member-picker-root .tmp-v3-columns .tmp-pending-removal-total {
    display:inline-flex;align-items:center;gap:4px;
    background:#fff0dc;border:1px solid #f1c391;color:#974c17;
    font-size:10.5px;font-weight:700;padding:3px 7px;border-radius:6px;
    white-space:nowrap;margin-left:auto;
}
.task-member-picker-root .tmp-v3-columns .tmp-pending-removal-total[hidden]{display:none!important}
.task-member-picker-root .tmp-board-person-row.tmp-will-remove {
    background:#fff7ed!important;
    box-shadow:inset 3px 0 #e69a44!important;
}
.task-member-picker-root .tmp-board-person-row.tmp-will-remove:hover {background:#fff0dc!important}
.task-member-picker-root .tmp-board-dayrow.tmp-will-remove {background:#fffaf4!important}
.task-member-picker-root .tmp-board-dayrow.tmp-will-remove:hover {background:#fff4e9!important}
/* UX V3: hành động khôi phục, giữ màu tím của control hiện tại. */
.task-assignment-footer-row .task-assignment-restore {
    display:inline-flex;align-items:center;justify-content:center;gap:6px;
    flex:0 0 auto;min-height:32px;padding:6px 12px;
    border:1px solid #d9c7ed;border-radius:7px;background:#fff;
    color:#632c93;font-size:12px;font-weight:700;line-height:1.3;
    cursor:pointer;white-space:nowrap;
    transition:background-color .16s,border-color .16s,color .16s;
}
.task-assignment-footer-row .task-assignment-restore:not(:disabled):hover {
    background:#f5effc;border-color:#ae8cd5;color:#4b1d7a;
}
.task-assignment-footer-row .task-assignment-restore:focus-visible {
    outline:2px solid #8756bf;outline-offset:2px;
}
.task-assignment-footer-row .task-assignment-restore:disabled {
    background:#f8f9fc;border-color:#e2e6ed;color:#a0a8b7;cursor:not-allowed;
}
@media(max-width:650px) {
    .task-assignment-footer-row .task-assignment-restore {padding:6px 8px;font-size:11px}
}

/* UX V4 - Lịch phân bổ: thu hồi không gian dọc, KHÔNG đổi bảng màu/logic. */
.task-member-picker-root .task-modal-header-info{margin-bottom:4px!important}
.task-member-picker-root .task-modal-header-info .info-row-custom{margin-bottom:3px!important}
.task-member-picker-root .tmp-v3-toolbar{
    height:43px!important;min-height:43px!important;margin:1px 0 5px!important;
    gap:9px!important;padding:4px 7px!important;flex:0 0 auto;justify-content:flex-start!important;
}
.task-member-picker-root .tmp-v3-toolbar .tmp-v3-filter-controls{
    flex:1 1 auto!important;width:auto!important;max-width:none!important;min-width:0!important;
}
.task-member-picker-root .tmp-v3-toolbar .tmp-native-search{
    flex:0 1 335px!important;width:335px!important;min-width:160px!important;max-width:335px!important;
}
.task-member-picker-root .tmp-v3-toolbar .tmp-view-switch{
    display:inline-flex;flex:0 0 auto!important;margin:0 0 0 auto!important;
    align-items:center;height:33px;min-height:33px;border:1px solid #d4bdec;border-radius:6px;
    background:#fff;
}
.task-member-picker-root .tmp-v3-toolbar .tmp-view-switch .tmp-tab{
    min-height:31px;height:31px;padding:0 10px!important;min-width:93px;
}
.task-member-picker-root .tmp-parked-tags{display:none!important}
.task-member-picker-root .tmp-timeline-window-nav{top:3px;right:8px}
.task-member-picker-root .tmp-board{--tmp-row-height:74px!important;--tmp-group-height:31px!important;--tmp-header-height:53px!important}
.task-member-picker-root .tmp-board-person-head{height:53px!important}
.task-member-picker-root .tmp-board-months,.task-member-picker-root .tmp-board-month{height:22px!important}
.task-member-picker-root .tmp-board-dayheads,.task-member-picker-root .tmp-board-dayhead{height:31px!important}
.task-member-picker-root .tmp-board-section > .tmp-board-group,
.task-member-picker-root .tmp-board-section > .tmp-board-group-days{height:31px!important}
.task-member-picker-root .tmp-board-group{display:flex;align-items:stretch;gap:3px}
.task-member-picker-root .tmp-board-group .tmp-toggle-group{
    flex:1 1 auto;min-width:0!important;width:auto!important;
    overflow:hidden;text-overflow:ellipsis;white-space:nowrap;
}
.task-member-picker-root .tmp-board-group.tmp-focused{
    box-shadow:inset 3px 0 #7c3aed;
}
.task-member-picker-root .tmp-board-group.tmp-focused .tmp-toggle-group{font-weight:800!important}
.task-member-picker-root .tmp-board-group .tmp-split-mode{
    flex:0 0 auto!important;width:auto!important;height:24px!important;
    align-self:center;border:1px solid #d8c9ec;border-radius:5px;
    margin-right:6px;padding:0 6px!important;
    font-size:10px!important;font-weight:700!important;color:#66339a;background:#fff;
}
.task-member-picker-root .tmp-board-group .tmp-split-mode:hover,
.task-member-picker-root .tmp-board-group .tmp-split-mode[aria-pressed="true"]{background:#e8dcfb;color:#5b2490;border-color:#ad8ed9}
.task-member-picker-root .tmp-board-person-row,.task-member-picker-root .tmp-board-dayrow,
.task-member-picker-root .tmp-board-daycell{height:74px!important;min-height:74px!important}
.task-member-picker-root .tmp-board-person-row .tmp-person-cell{padding:6px 8px!important;gap:7px!important;min-height:0!important;overflow:hidden}
.task-member-picker-root .tmp-board-person-row .tmp-person-avatar{flex-basis:29px!important;width:29px!important;height:29px!important}
.task-member-picker-root .tmp-board-person-row .tmp-person-avatar .single-avatar-circle{width:29px!important;height:29px!important}
.task-member-picker-root .tmp-board-person-row .tmp-person-text{
    display:flex!important;flex:1 1 0;min-width:0;flex-direction:column;
    justify-content:center!important;gap:4px!important;overflow:hidden!important;
}
.task-member-picker-root .tmp-person-headline{display:flex!important;flex-wrap:nowrap!important;align-items:center;gap:4px!important;min-width:0;min-height:19px}
.task-member-picker-root .tmp-person-headline .tmp-person-name{
    min-width:0;flex:0 1 auto;max-width:100%;display:block!important;overflow:hidden!important;
    white-space:nowrap!important;text-overflow:ellipsis!important;font-size:11.5px!important;
}
.task-member-picker-root .tmp-person-headline .tmp-person-source{flex:0 0 auto}
.task-member-picker-root .tmp-person-headline .tmp-person-pm{flex:0 0 auto}
.task-member-picker-root .tmp-person-headline .tmp-person-badges{
    flex:0 0 auto;display:inline-flex!important;align-items:center;min-height:0!important;overflow:visible!important;
    margin-left:auto;white-space:nowrap;
}
.task-member-picker-root .tmp-person-headline .tmp-person-badges .badge{
    font-size:9.2px!important;max-width:122px!important;padding:3px 5px!important;
    overflow:hidden;white-space:nowrap;text-overflow:ellipsis;
}
.task-member-picker-root .tmp-person-subline{display:flex;align-items:center;min-width:0;gap:4px;line-height:1.25}
.task-member-picker-root .tmp-person-subline .tmp-person-position,
.task-member-picker-root .tmp-person-subline .tmp-person-email{
    flex:0 1 auto;min-width:0;display:block;white-space:nowrap!important;
    text-overflow:ellipsis;overflow:hidden!important;font-size:10.5px!important;
}
.task-member-picker-root .tmp-person-subline .tmp-person-position{max-width:49%;color:#64748b}
.task-member-picker-root .tmp-person-subline .tmp-person-email{flex:1 1 0;color:#71809a}
.task-member-picker-root .tmp-board-daycell{padding:5px 2px!important}
.task-member-picker-root .tmp-board-daycell .tmp-day{
    height:60px!important;max-height:60px!important;margin:0!important;
    font-size:10px!important;line-height:1.22!important;
    white-space:normal!important;text-align:center;overflow-wrap:normal;
}
.task-assignment-footer-row{gap:8px!important}
.task-assignment-footer-row .tmp-footer-notice{
    display:inline-flex;align-items:center;gap:5px;min-width:0;flex:0 1 315px;
    color:#9a5b14;font-size:10.5px;line-height:1.2;white-space:nowrap;
}
.task-assignment-footer-row .tmp-footer-notice .tmp-v3-dot{display:inline-block!important;flex:0 0 8px;width:8px;height:8px;border-radius:50%;margin:0!important;background:#94a3b8!important}
.task-assignment-footer-row .tmp-footer-notice span{overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.task-assignment-footer-row .task-assignment-summary{font-size:11px!important;flex:1 1 175px}
@media(max-width:1150px){
    .task-member-picker-root .tmp-v3-toolbar .tmp-native-search{min-width:125px!important;width:205px!important;flex-basis:205px!important}
    .task-member-picker-root .tmp-v3-toolbar .tmp-view-switch .tmp-tab{min-width:82px!important;padding:0 7px!important}
    .task-assignment-footer-row .tmp-footer-notice{flex-basis:195px}
}
@media(max-width:800px){
    .task-member-picker-root .tmp-v3-toolbar .tmp-v3-filter-label{display:none}
    .task-member-picker-root .tmp-v3-toolbar .tmp-view-switch .tmp-tab{min-width:34px!important;padding:0 7px!important}
    .task-member-picker-root .tmp-v3-toolbar .tmp-view-switch .tmp-tab span{display:none}
    .task-assignment-footer-row .tmp-footer-notice{flex-basis:24px;overflow:hidden}
    .task-assignment-footer-row .tmp-footer-notice span{display:none}
}


/* UX V4.1: khôi phục lưu ý nguyên văn, không cắt chữ bằng ellipsis. */
.task-assignment-footer-row{flex-wrap:wrap;align-items:center;row-gap:6px!important}
.task-assignment-footer-row .tmp-footer-notice{
    flex:1 1 410px!important;min-width:min(100%,340px)!important;max-width:none!important;
    align-items:flex-start!important;white-space:normal!important;
    line-height:1.35!important;font-size:10.8px!important;
}
.task-assignment-footer-row .tmp-footer-notice span{
    overflow:visible!important;text-overflow:clip!important;
    white-space:normal!important;line-height:1.35!important;
}
.task-assignment-footer-row .tmp-footer-notice .tmp-v3-dot{margin-top:3px!important}
@media(max-width:800px){
    .task-assignment-footer-row .tmp-footer-notice{flex:1 1 100%!important;min-width:0!important;overflow:visible!important}
    .task-assignment-footer-row .tmp-footer-notice span{display:inline!important}
}
/* Nhấn header thu/mở, không hiển thị nút chia 2+2 gây khó hiểu. */
.task-member-picker-root .tmp-board-group .tmp-split-mode{display:none!important}
.task-member-picker-root .tmp-board-group .tmp-toggle-group{
    text-align:left;cursor:pointer;transition:background-color .14s,color .14s;
}
.task-member-picker-root .tmp-board-group .tmp-toggle-group:hover{filter:brightness(.97)}
.task-member-picker-root .tmp-board-group .tmp-toggle-group:focus-visible{outline:2px solid #8c56c0;outline-offset:-2px}
/* Tooltip lịch ngắn cao đúng theo nội dung; trường hợp rất dài JS mới bật scrollbar. */
.global-task-tooltip{overflow:visible;max-height:none}
/* Tooltip nhân viên thay thế title mặc định. Nổi khỏi grid để không bị cắt. */
.tmp-member-tooltip{
    position:fixed;z-index:10000030;left:0;top:0;pointer-events:none;
    max-width:min(375px,calc(100vw - 20px));min-width:230px;
    padding:11px 13px;border:1px solid #dacbea;border-top:3px solid #7136b0;
    border-radius:9px;background:#fff;color:#334155;
    box-shadow:0 10px 28px rgba(37,22,62,.16);opacity:0;visibility:hidden;
    transition:opacity .12s ease,visibility .12s ease;
    font-size:11.5px;line-height:1.5;overflow-wrap:anywhere;
}
.tmp-member-tooltip.is-visible{opacity:1;visibility:visible}
.tmp-member-tip-name{font-weight:750;color:#3c2656;font-size:12.4px;border-bottom:1px solid #ece7f4;padding-bottom:5px;margin-bottom:5px}
.tmp-member-tip-role,.tmp-member-tip-email{padding:2px 0;color:#58677b}
.tmp-member-tip-role i,.tmp-member-tip-email i{width:17px;color:#834cbb}
@media(prefers-reduced-motion:reduce){.tmp-member-tooltip{transition:none}}

</style>

<SweetSoft:ExtraModal runat="server" ID="mdlTaskMemberPicker" Type="Primary" Size="ExtraLarge">
    <ContentTemplate>
        <asp:UpdatePanel ID="upnlMemberPicker" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
            <ContentTemplate>
                 <asp:HiddenField ID="hdfPickedIdsClient" runat="server" />
                <asp:HiddenField ID="hdfOriginalAssignedIdsClient" runat="server" />
                <input type="hidden" id="tmpFilterRevision" value="<%= ClientFilterRevision %>" />
                <input type="hidden" id="hdfTaskPickerId" value="<%= ClientTaskId %>" />
                <input type="hidden" id="hdfTimelineJson" value="<%= System.Web.HttpUtility.HtmlAttributeEncode(ClientTimelineJson) %>" />
                <div class="js-validation validationEngineContainer p-2 task-modal-body task-member-picker-root">
                    <!-- HEADER CỐ ĐỊNH -->
                    <div class="task-modal-header-info">
                        <div class="info-row-custom" style="background: #eff6ff; padding: 8px 12px; border-radius: 6px; border: 1px solid #bfdbfe; margin-bottom: 8px;">
                            <asp:Literal runat="server" ID="ltrTaskInfoNote"></asp:Literal>
                        </div>
                    </div>

                    <div class="tmp-v3-toolbar" aria-label="Lọc danh sách ứng viên">
                        <span class="tmp-v3-filter-label">Lọc ứng viên</span>
                            <div class="d-flex flex-row gap-2 tmp-native-filter-row tmp-v3-filter-controls">
                                <asp:UpdatePanel runat="server" ID="upnlTimelineRole" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:Panel runat="server" ID="pnlSearchTimeline" CssClass="tmp-v3-dropdown-pair">
                                            <SweetSoft:BootstrapDropdown ID="ddlSourceTimeline" runat="server" Text="Tất cả" AutoPostBack="true" AllowClear="false" SearchColumn="NguonNhanSu" EnableSearch="false" ValueIsOfTypeGUID="False" OnSelectedValueChanged="ddlSourceTimeline_SelectedValueChanged"></SweetSoft:BootstrapDropdown>
                                            <SweetSoft:BootstrapDropdown ID="ddlChucDanhTimeline" runat="server" Text="Chức Danh" AutoPostBack="true" AllowClear="true" SearchColumn="IdChucDanh" EnableSearch="true" ValueIsOfTypeGUID="True" SearchPlaceholder="Tìm kiếm theo tên chức danh..." NoResultsText="Không tìm thấy tên chức danh" OnSelectedValueChanged="ddlChucDanhTimeline_SelectedValueChanged"></SweetSoft:BootstrapDropdown>
                                        </asp:Panel>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                                <div class="input-group flex-grow-1 tmp-native-search">
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtSearchTimeline" CssClass="border-primary input-search-filter" PlaceHolder="Nhập từ khóa tìm kiếm..."></SweetSoft:ExtraTextBox>
                                    <SweetSoft:ExtraButton runat="server" ID="lbtSearchTimeline" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearchTimeline_ServerClick"></SweetSoft:ExtraButton>
                                </div>
                            </div>
                            <div class="tmp-view-switch" role="tablist" aria-label="Chuyển cách hiển thị">
                                <button type="button" id="tmpTabList" class="tmp-tab is-active" role="tab" aria-selected="true" aria-controls="tmpPaneList" tabindex="0" title="Hiển thị dưới dạng danh sách" aria-label="Hiển thị dưới dạng danh sách"><i class="fas fa-list-ul" aria-hidden="true"></i><span>Danh sách</span></button>
                                <button type="button" id="tmpTabTimeline" class="tmp-tab" role="tab" aria-selected="false" aria-controls="tmpPaneTimeline" tabindex="-1" title="Hiển thị dưới dạng lịch trình" aria-label="Hiển thị dưới dạng lịch trình"><i class="fas fa-stream" aria-hidden="true"></i><span>Lịch phân bổ</span></button>
                            </div>
                    </div>
                    <!-- Các server control tag được giữ trong cây điều khiển nhưng không chiếm diện tích UI. -->
                    <div class="tmp-parked-tags" aria-hidden="true">
                        <div class="tmp-timeline-tags listSearchTagBox" id="tmpTimelineTags">
                            <asp:UpdatePanel ID="upnlTimelineTags" runat="server" UpdateMode="Conditional" RenderMode="Inline">
                                <ContentTemplate>
                                    <SweetSoft:ExtraSearchBox ID="searchTagBoxTimeline" runat="server" OnTagClosed="searchTagBoxTimeline_TagClosed"></SweetSoft:ExtraSearchBox>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </div>
                    <!-- Hidden ASP.NET repeater sources: keep server control lifecycle and postback contract. -->
                    <div id="tmpLegacySources" class="tmp-legacy-sources" aria-hidden="true">
                    <!-- GRID 2 CỘT TỰ CO GIÃN -->
                    <div class="task-member-columns" id="tmpLegacySourcePanels">

                        <!-- CỘT 1: NHÂN SỰ DỰ ÁN -->
                        <div class="member-list-panel" id="sourcePanelProject">
                            <div class="panel-header">
                                <span>📁 <%= GetResourceText(BackEndResourceKeys.PROJECT_MEMBERS) %></span>
                                <span class="badge bg-primary rounded-pill">
                                    <asp:UpdatePanel ID="upCountProj" runat="server" UpdateMode="Conditional" RenderMode="Inline">
                                        <ContentTemplate><asp:Literal ID="ltrCountProj" runat="server">0</asp:Literal></ContentTemplate>
                                    </asp:UpdatePanel>
                                </span>
                            </div>
                            
                            <!-- BỘ LỌC ĐƯỢC TÁCH RỜI NHƯ BÊN CHỌN NHÂN VIÊN GỐC -->
                            <div class="panel-filter">
                                <div class="d-flex flex-column flex-md-row gap-2 mb-2">
                                    <asp:UpdatePanel runat="server" ID="upnlSearchProj" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <asp:Panel runat="server" ID="pnlSearchProj">
                                                <SweetSoft:BootstrapDropdown ID="ddlChucDanhProj" runat="server" Text="Chức Danh" AutoPostBack="true" AllowClear="true" SearchColumn="IdChucDanh" EnableSearch="true" ValueIsOfTypeGUID="True" SearchPlaceholder="Tìm kiếm theo tên chức danh..." NoResultsText="Không tìm thấy tên chức danh" OnSelectedValueChanged="ddlChucDanhProj_SelectedValueChanged">
                                                </SweetSoft:BootstrapDropdown>
                                            </asp:Panel>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                    
                                    <div class="input-group flex-grow-1">
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtSearchProj" CssClass="border-primary input-search-filter" PlaceHolder="Nhập từ khóa tìm kiếm..."></SweetSoft:ExtraTextBox>
                                        <SweetSoft:ExtraButton runat="server" ID="lbtSearchProj" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearchProj_ServerClick"></SweetSoft:ExtraButton>
                                    </div>
                                </div>
                                <div class="listSearchTagBox">
                                    <asp:UpdatePanel ID="upSearchTagProj" runat="server" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <SweetSoft:ExtraSearchBox ID="searchTagBoxProj" runat="server" OnTagClosed="searchTagBoxProj_TagClosed"></SweetSoft:ExtraSearchBox>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>
                            </div>
                            
                            <!-- VÙNG CÓ THANH CUỘN -->
                            <div class="panel-list">
                                <asp:UpdatePanel ID="upListProj" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:Repeater ID="rptProjectMembers" runat="server" OnItemDataBound="rptMembers_ItemDataBound">
                                            <ItemTemplate>
                                                <div class='member-item-row <%# Convert.ToBoolean(Eval("IsOriginal")) ? "original-member" : "" %>' id='mem-row-<%# Eval("UserId") %>' data-tmp-group='project'>
                                                    <div class="row-default-view">
                                                        <!-- Cụm thông tin bên trái (Thêm flex-grow-1 để nó đẩy phần lịch sang phải) -->
                                                        <div class="member-info-group flex-grow-1">
                                                            <asp:CheckBox runat="server" ID="chkSelect" />
                                                            <asp:HiddenField runat="server" ID="hdfUserId" Value='<%# Eval("UserId") %>' />
                                                            <%# Eval("AvatarHtml") %>
                                                            <div class="member-name-block">
                                                                <div class="d-flex align-items-center flex-wrap gap-1 mb-1">
                                                                    <span class="fw-bold text-dark me-1"><%#: Eval("DisplayName") %></span>
                                                                    <span class="member-position"><%#: Eval("ChucDanh") %></span>
                                                                    <%# Convert.ToBoolean(Eval("IsPM")) ? "<span class='badge bg-danger' style='font-size: 9.5px; padding: 2px 5px;'>PM</span>" : "" %>
                                                                    <%# Convert.ToBoolean(Eval("IsInactive")) ? "<span class='badge bg-secondary' style='font-size: 9.5px; padding: 2px 5px;'>Đã nghỉ</span>" : "" %>
                                                                </div>
                                                                <span class="member-email"><%#: Eval("Email") %></span>
                                                            </div>
                                                        </div>
    
                                                       <!-- Cụm cảnh báo & Lịch biểu nằm dạt sang phải -->
                                                        <div class="d-flex align-items-center gap-2 flex-shrink-0">
        
                                                            <%# Convert.ToBoolean(Eval("IsOriginal")) ? 
                                                                "<span class='badge px-2 py-1' style='font-size: 11.5px; background-color: #e0f2fe; color: #0284c7; border: 1px solid #bae6fd;'><i class='fas fa-user-check me-1'></i>Đã phân công</span>" 
                                                                : (Convert.ToInt32(Eval("BusyDays")) > 0 ? 
                                                                    string.Format("<span class='badge bg-warning text-dark px-2 py-1' style='font-size: 11.5px;'><i class='fas fa-briefcase me-1'></i>Bận {0} ngày</span>", Eval("BusyDaysDisplay")) 
                                                                    : "<span class='badge bg-success px-2 py-1' style='font-size: 11.5px;'><i class='fas fa-check-circle me-1'></i>Trống lịch</span>") %>
                                                            <button type="button" class="btn-calendar-only" title="Xem lịch" onclick="return TaskMemberPlanner.openCalendar(this, '<%# Eval("UserId") %>');">📅</button>
                                                        </div>
                                                    </div>
                                                    <asp:HiddenField runat="server" ID="hdfScheduleJson" Value='<%# Eval("ScheduleJson") %>' />
                                                    <div class="row-schedule-panel" id='overlay-<%# Eval("UserId") %>'>
                                                        <div class="row-schedule-inner">
                                                            <div class="mini-cal-wrap">
                                                                <div class="mini-cal" id='timeline-<%# Eval("UserId") %>'></div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                        <asp:Panel ID="pnlNoDataProj" runat="server" CssClass="member-empty" Visible="false">
                                            <asp:Literal ID="ltrNoDataProj" runat="server"></asp:Literal>
                                        </asp:Panel>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>

                        <!-- CỘT 2: NHÂN SỰ KHÁC TRONG CÔNG TY -->
                        <div class="member-list-panel" id="sourcePanelCompany">
                            <div class="panel-header company-header">
                                <span>🏢 <%= GetResourceText(BackEndResourceKeys.OTHER_EMPLOYEES) %></span>
                                <span class="badge bg-danger rounded-pill">
                                    <asp:UpdatePanel ID="upCountCompany" runat="server" UpdateMode="Conditional" RenderMode="Inline">
                                        <ContentTemplate><asp:Literal ID="ltrCountCompany" runat="server">0</asp:Literal></ContentTemplate>
                                    </asp:UpdatePanel>
                                </span>
                            </div>
                            
                            <!-- BỘ LỌC ĐƯỢC TÁCH RỜI NHƯ BÊN CHỌN NHÂN VIÊN GỐC -->
                            <div class="panel-filter">
                                <div class="d-flex flex-column flex-md-row gap-2 mb-2">
                                    <asp:UpdatePanel runat="server" ID="upnlSearchCompany" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <asp:Panel runat="server" ID="pnlSearchCompany">
                                                <SweetSoft:BootstrapDropdown ID="ddlChucDanhCompany" runat="server" Text="Chức Danh" AutoPostBack="true" AllowClear="true" SearchColumn="IdChucDanh" EnableSearch="true" ValueIsOfTypeGUID="True" SearchPlaceholder="Tìm kiếm theo tên chức danh..." NoResultsText="Không tìm thấy tên chức danh" OnSelectedValueChanged="ddlChucDanhCompany_SelectedValueChanged">
                                                </SweetSoft:BootstrapDropdown>
                                            </asp:Panel>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                    
                                    <div class="input-group flex-grow-1">
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtSearchCompany" CssClass="border-primary input-search-filter" PlaceHolder="Nhập từ khóa tìm kiếm..."></SweetSoft:ExtraTextBox>
                                        <SweetSoft:ExtraButton runat="server" ID="lbtSearchCompany" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearchCompany_ServerClick"></SweetSoft:ExtraButton>
                                    </div>
                                </div>
                                <div class="listSearchTagBox">
                                    <asp:UpdatePanel ID="upSearchTagCompany" runat="server" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <SweetSoft:ExtraSearchBox ID="searchTagBoxCompany" runat="server" OnTagClosed="searchTagBoxCompany_TagClosed"></SweetSoft:ExtraSearchBox>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>
                            </div>
                            
                            <!-- VÙNG CÓ THANH CUỘN -->
                            <div class="panel-list">
                                <asp:UpdatePanel ID="upListCompany" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:Repeater ID="rptCompanyMembers" runat="server" OnItemDataBound="rptMembers_ItemDataBound">
                                            <ItemTemplate>
                                                <div class="member-item-row" id='mem-row-<%# Eval("UserId") %>' data-tmp-group='other'>
                                                    <div class="row-default-view">
                                                        <!-- Cụm thông tin bên trái (Thêm flex-grow-1 để nó đẩy phần lịch sang phải) -->
                                                        <div class="member-info-group flex-grow-1">
                                                            <asp:CheckBox runat="server" ID="chkSelect" />
                                                            <asp:HiddenField runat="server" ID="hdfUserId" Value='<%# Eval("UserId") %>' />
                                                            <%# Eval("AvatarHtml") %>
                                                            <div class="member-name-block">
                                                                <div class="d-flex align-items-center flex-wrap gap-1 mb-1">
                                                                    <span class="fw-bold text-dark me-1"><%#: Eval("DisplayName") %></span>
                                                                    <span class="member-position"><%#: Eval("ChucDanh") %></span>
                                                                    <%# Convert.ToBoolean(Eval("IsPM")) ? "<span class='badge bg-danger' style='font-size: 9.5px; padding: 2px 5px;'>PM</span>" : "" %>
                                                                    <%# Convert.ToBoolean(Eval("IsInactive")) ? "<span class='badge bg-secondary' style='font-size: 9.5px; padding: 2px 5px;'>Đã nghỉ</span>" : "" %>
                                                                </div>
                                                                <span class="member-email"><%#: Eval("Email") %></span>
                                                            </div>
                                                        </div>
    
                                                       <!-- Cụm cảnh báo & Lịch biểu nằm dạt sang phải -->
                                                        <div class="d-flex align-items-center gap-2 flex-shrink-0">
        
                                                          <%# Convert.ToBoolean(Eval("IsOriginal")) ? 
                                                                "<span class='badge px-2 py-1' style='font-size: 11.5px; background-color: #e0f2fe; color: #0284c7; border: 1px solid #bae6fd;'><i class='fas fa-user-check me-1'></i>Đã phân công</span>" 
                                                                : (Convert.ToInt32(Eval("BusyDays")) > 0 ? 
                                                                    string.Format("<span class='badge bg-warning text-dark px-2 py-1' style='font-size: 11.5px;'><i class='fas fa-briefcase me-1'></i>Bận {0} ngày</span>", Eval("BusyDaysDisplay")) 
                                                                    : "<span class='badge bg-success px-2 py-1' style='font-size: 11.5px;'><i class='fas fa-check-circle me-1'></i>Trống lịch</span>") %>
                                                            <button type="button" class="btn-calendar-only" title="Xem lịch" onclick="return TaskMemberPlanner.openCalendar(this, '<%# Eval("UserId") %>');">📅</button>
                                                        </div>
                                                    </div>
                                                    <asp:HiddenField runat="server" ID="hdfScheduleJson" Value='<%# Eval("ScheduleJson") %>' />
                                                    <div class="row-schedule-panel" id='overlay-<%# Eval("UserId") %>'>
                                                        <div class="row-schedule-inner">
                                                            <div class="mini-cal-wrap">
                                                                <div class="mini-cal" id='timeline-<%# Eval("UserId") %>'></div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                        <asp:Panel ID="pnlNoDataCompany" runat="server" CssClass="member-empty" Visible="false">
                                            <asp:Literal ID="ltrNoDataCompany" runat="server"></asp:Literal>
                                        </asp:Panel>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>

                    </div>
                    </div> <!-- End hidden ASP.NET Repeater sources -->
                    <div id="tmpPaneList" class="task-member-columns tmp-v3-columns" role="tabpanel">
                        <section class="member-list-panel" id="panelProject" aria-label="Nhân sự được chọn">
                            <div class="panel-header tmp-v3-picked-header"><span><i class="fas fa-user-check me-1"></i> Nhân sự được chọn <small class="tmp-ux-panel-hint">Bỏ tích để gỡ sau khi lưu</small></span><span class="tmp-pending-removal-total" id="tmpPendingRemovalCount" title="Nhân viên bỏ chọn nhưng vẫn được phân công cho đến khi Lưu" hidden></span><span class="badge bg-primary rounded-pill" id="tmpSelectedCount" title="Số nhân viên sẽ được phân công sau khi lưu">0</span></div>
                            <div class="panel-list tmp-v3-list" id="tmpSelectedList"></div>
                        </section>
                        <section class="member-list-panel" id="panelCompany" aria-label="Nhân sự có thể phân công">
                            <div class="panel-header tmp-v3-available-header"><span><i class="fas fa-users me-1"></i> Nhân sự có thể phân công <small class="tmp-ux-panel-hint">Ưu tiên ngày trống</small></span><span class="badge bg-primary rounded-pill" id="tmpAvailableCount">0</span></div>
                            <div class="tmp-v3-legend"><span><i class="tmp-v3-dot project"></i>Thuộc dự án</span><span><i class="tmp-v3-dot other"></i>Ngoài dự án</span></div>
                            <div class="panel-list tmp-v3-list" id="tmpAvailableList"></div>
                        </section>
                    </div>
                    <div id="tmpPaneTimeline" class="tmp-timeline" role="tabpanel" hidden>
                        <div class="tmp-timeline-window-nav"><span id="tmpWindowLabel" class="tmp-timeline-tip">Cuộn ngang để xem thêm ngày</span><button type="button" class="tmp-window-nav" id="tmpWindowPrev" hidden title="Khoảng ngày trước">◀</button><button type="button" class="tmp-window-nav" id="tmpWindowNext" hidden title="Khoảng ngày tiếp">▶</button></div>
                        <div class="tmp-scroll" id="tmpTimelineScroll">
                            <div class="tmp-board" id="tmpTimelineTable" role="table" aria-label="Lịch phân bổ theo nhân viên"></div>
                        </div>
                        <div class="tmp-timeline-xbar" id="tmpTimelineXBar" aria-label="Cuộn ngang vùng lịch" tabindex="0"><div id="tmpTimelineXTrack"></div></div>
                        <div id="tmpDayDetail" class="tmp-floating-day-detail" hidden></div>
                    </div>
                    <div id="tmpCalendarPop" class="tmp-calendar-pop" role="dialog" aria-label="Lịch làm việc của nhân viên" hidden>
                        <div class="tmp-pop-head"><span id="tmpPopName" class="tmp-pop-name"></span><button id="tmpPopClose" class="tmp-pop-close" type="button" aria-label="Đóng lịch">&times;</button></div>
                        <div class="tmp-pop-inner"><div class="tmp-pop-calendar" id="tmpPopCalendar"><div class="mini-cal" id="tmpCalendar"></div>
                            <div class="tmp-pop-help"><span style="--tmp-legend:#fee2e2">Bận</span><span style="--tmp-legend:#e6f4ea">Trống</span><span style="--tmp-legend:#fef3c7">Nghỉ lễ</span><span style="--tmp-legend:#f1f5f9">Nghỉ tuần</span></div>
                        </div></div>
                    </div> <!-- End Grid -->
                </div> <!-- End Modal Body -->

            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>

    <FooterTemplate>
        <asp:UpdatePanel ID="upnlTaskMemberFooter" runat="server"
            UpdateMode="Conditional">
            <ContentTemplate>

                <div class="task-assignment-footer-row">
                    <span class="tmp-footer-notice" title="Nhân sự ngoài dự án được ký hiệu bằng chấm xám. Khi phân công, hệ thống sẽ tự thêm nhân viên vào dự án sau khi lưu.">
                        <i class="fas fa-exclamation-triangle" aria-hidden="true"></i>
                        <i class="tmp-v3-dot other" aria-hidden="true"></i>
                        <span>Lưu ý: Khi phân công nhân sự chưa thuộc dự án (ký hiệu xám), hệ thống sẽ tự cấp quyền tham gia dự án sau khi lưu.</span>
                    </span>
                    <!-- Tóm tắt lựa chọn, nằm bên trái -->
                    <span id="tmpPendingChanges" class="tmp-ux-pending" role="status" aria-live="polite">Chưa có thay đổi</span>
                    <asp:Label
                        ID="lblAssignmentSummary"
                        runat="server"
                        CssClass="task-assignment-summary" />

                    <!-- Chỉ khôi phục lựa chọn trên giao diện, không ghi cơ sở dữ liệu. -->
                    <button type="button" id="tmpRestoreOriginal"
                        class="task-assignment-restore" disabled
                        title="Khôi phục danh sách phân công như lúc mở cửa sổ"
                        aria-label="Khôi phục ban đầu">
                        <i class="fas fa-undo-alt" aria-hidden="true"></i>
                        Khôi phục ban đầu
                    </button>
                    <!-- Nút hậu kiểm gọi MessageBox hệ thống nếu người dùng đóng khi chưa lưu. -->
                    <asp:LinkButton ID="lbtRequestDiscardTaskAssign" runat="server"
                        CssClass="tmp-discard-server-trigger" Style="display:none"
                        CausesValidation="false"
                        OnClick="lbtRequestDiscardTaskAssign_Click">Yêu cầu xác nhận đóng</asp:LinkButton>

                    <!-- Giữ nguyên nút Lưu hiện tại -->
                    <SweetSoft:ExtraButton
                        runat="server"
                        ID="btnConfirmTaskAssign"
                        CssClass="waves-effect waves-light"
                        ButtonStyle="Primary"
                        ButtonIcon="Save"
                        IsPace="true"
                        OnClick="btnConfirmTaskAssign_Click">
                        <%= GetResourceText(BackEndResourceKeys.SAVE) %>
                    </SweetSoft:ExtraButton>

                </div>

            </ContentTemplate>
        </asp:UpdatePanel>
    </FooterTemplate> 
</SweetSoft:ExtraModal>

<script type="text/javascript">
    window.CMSMasterJs = window.CMSMasterJs || {};

    $(document).ready(function () {
        var WEEKDAYS = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];

        function pad(n) { return n < 10 ? '0' + n : '' + n; }
        function toKey(y, month, d) { return y + '-' + pad(month + 1) + '-' + pad(d); }
        function parseKey(k) { var p = k.split('-'); return new Date(+p[0], +p[1] - 1, +p[2]); }
        function escAttr(s) { return String(s).replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;'); }
        function titleText(st) { return 'Tháng ' + (st.month + 1) + ', ' + st.year; }

        function taskDayTooltip(info, key, employee, outside) {
            if (!info) return '';
            var hasTasks = !!(info.tasks && info.tasks.length);
            var current = !!info.current;
            var p = key.split('-');
            var html = '<div class="tmp-tooltip-heading">' + (employee ? escAttr(employee) + ' · ' : 'Ngày ') + p[2] + '/' + p[1] + '/' + p[0] + '</div>';
            if (outside) html += '<div style="color:#64748b;font-size:10.5px;margin-bottom:5px">Ngoài thời hạn công việc đang phân công</div>';
            if (current) {
                var own = info.currentTask || {};
                html += '<div class="tmp-current-task"><strong>● Công việc đang phân công (không tính trùng lịch)</strong>' +
                    (own.code ? '<span class="t-code">[' + escAttr(own.code) + ']</span>' : '') +
                    escAttr(own.name || 'Công việc hiện tại') + '</div>';
            }
            if (hasTasks) {
                html += '<div class="tmp-other-heading">Công việc khác (' + info.tasks.length + ')</div><ul class="tooltip-task-list">';
                info.tasks.forEach(function(t) {
                    html += '<li><div class="t-project-name"><i class="fas fa-cube me-1"></i>' + escAttr(t.projectName || '') + '</div>' +
                        '<span class="t-code">[' + escAttr(t.code || '') + ']</span>' + escAttr(t.name || '') + '</li>';
                });
                html += '</ul>';
            }
            if (!hasTasks && !current) html += '<div style="font-size:11.5px;color:#64748b">' + escAttr(info.text || 'Chưa có lịch công việc') + '</div>';
            return html;
        }
        function buildGridHtml(st) {
            var now = new Date();
            var todayKey = toKey(now.getFullYear(), now.getMonth(), now.getDate());
            var offset = (new Date(st.year, st.month, 1).getDay() + 6) % 7;
            var html = '<div class="mc-grid">';

            for (var i = 0; i < 42; i++) {
                var d = new Date(st.year, st.month, 1 - offset + i);
                var key = toKey(d.getFullYear(), d.getMonth(), d.getDate());
                var inRange = !!st.minKey && key >= st.minKey && key <= st.maxKey;
                var info = inRange ? st.data[key] : null;

                var cls = 'mc-day';
                if (d.getMonth() !== st.month) cls += ' out-month';
                if (!inRange) cls += ' out-range';
                else if (info && info.status) cls += ' st-' + info.status;
                // Có task hiện tại mà không vướng task khác: màu tím, không ghi là 'Trống'.
                if (info && info.current && info.status === 'free') cls += ' st-current-only';
                if (key === todayKey) cls += ' today';
                if (info && info.current) cls += ' has-current';

                var extra = '';
                var tooltipHtml = '';
                var clickAttr = '';

                if (info) {
                    var label = info.current && info.status === 'free' ? 'Task này' : String(info.text || '').replace(/^[^A-Za-z0-9\u00C0-\u1EF9]+/, '');
                    if (label) extra = '<span class="mc-label">' + escAttr(label) + '</span>';
                    var hasOthers = !!(info.tasks && info.tasks.length);
                    var hasCurrent = !!info.current;
                    if (hasOthers || hasCurrent) {
                        cls += ' has-tooltip';
                        var listHtml = taskDayTooltip(info, key, '', false);
                        clickAttr = ' data-tooltip-content="' + encodeURIComponent(listHtml) + '"';
                    } else if (info.text) {
                        clickAttr = ' title="' + escAttr(d.getDate() + '/' + (d.getMonth() + 1) + ' - ' + info.text) + '"';
                    }
                }
                html += '<div class="' + cls + '" data-day-key="' + key + '"' + clickAttr + '><span class="mc-num">' + d.getDate() + '</span>' + extra + '</div>';
            }
            return html + '</div>';
        }
        function renderMiniCalendar(cal) {
            var st = cal.data('mc');
            var html = '<div class="mc-header">' +
                '<span class="mc-title">' + titleText(st) + '</span>' +
                '<div class="mc-nav">' +
                '<button type="button" class="mc-nav-btn" title="Tháng trước" onclick="CMSMasterJs.ChangeScheduleMonth(this, -1)">&#9650;</button>' +
                '<button type="button" class="mc-nav-btn" title="Tháng sau" onclick="CMSMasterJs.ChangeScheduleMonth(this, 1)">&#9660;</button>' +
                '</div></div>';

            html += '<div class="mc-weekdays">';
            for (var w = 0; w < 7; w++) {
                html += '<span' + (w >= 5 ? ' class="mc-we"' : '') + '>' + WEEKDAYS[w] + '</span>';
            }
            html += '</div>';

            html += '<div class="mc-viewport">' + buildGridHtml(st) + '</div>';
            cal.html(html);
        }

        window.TaskMemberCalendarEngine = { render: renderMiniCalendar, parseKey: parseKey, tooltipHtml: taskDayTooltip };

        // BẢN NÂNG CẤP: CHỜ TRƯỢT XONG MỚI BỎ VÁCH NGĂN
        CMSMasterJs.ToggleRowSchedule = function (btnElement, userId, isShow) {
            var rowEl = $(btnElement).closest('.member-item-row');
            var currentPanel = rowEl.closest('.member-list-panel');

            if (typeof isShow !== 'boolean') isShow = !rowEl.hasClass('show-schedule');

            if (isShow) {
                // Tắt các lịch cũ và đóng luôn vách ngăn của tụi nó
                currentPanel.find('.member-item-row.show-schedule').not(rowEl).removeClass('show-schedule schedule-opened');

                var jsonString = rowEl.find('input[type="hidden"][id*="hdfScheduleJson"]').val();
                var cal = rowEl.find('#timeline-' + userId);
                var data = {};

                if (jsonString) {
                    try {
                        data = JSON.parse(jsonString) || {};
                    } catch (e) {
                        console.error("Lỗi parse JSON lịch biểu: ", e);
                    }
                }

                var keys = Object.keys(data).sort();
                var minKey = keys.length ? keys[0] : null;
                var maxKey = keys.length ? keys[keys.length - 1] : null;

                var now = new Date();
                var todayKey = toKey(now.getFullYear(), now.getMonth(), now.getDate());
                var baseDate = (minKey && todayKey >= minKey && todayKey <= maxKey) ? now
                    : (minKey ? parseKey(minKey) : now);

                cal.data('mc', {
                    data: data,
                    minKey: minKey,
                    maxKey: maxKey,
                    year: baseDate.getFullYear(),
                    month: baseDate.getMonth()
                });
                renderMiniCalendar(cal);

                // Bắt đầu kéo rèm xuống
                rowEl.addClass('show-schedule');

                // Đợi 380ms cho rèm kéo xong -> Mở khóa Vách ngăn để Tooltip đâm thủng trần
                setTimeout(function () {
                    if (rowEl.hasClass('show-schedule')) {
                        rowEl.addClass('schedule-opened'); // [CHÌA KHÓA NẰM Ở ĐÂY]
                    }
                    if (rowEl.hasClass('show-schedule') && rowEl[0].scrollIntoView) {
                        rowEl[0].scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                    }
                }, 380);

            } else {
                rowEl.removeClass('show-schedule schedule-opened');
            }
        };

        // BẢN NÂNG CẤP: KHÓA VÁCH NGĂN LẠI KHI ĐANG TRƯỢT THÁNG
        CMSMasterJs.ChangeScheduleMonth = function (btnElement, delta) {
            var cal = $(btnElement).closest('.mini-cal');
            var st = cal.data('mc');
            if (!st) return;

            var viewport = cal.find('.mc-viewport');

            // Đóng tooltip đã ghim trước khi đổi tháng.
            if (CMSMasterJs.HideTaskTooltip) CMSMasterJs.HideTaskTooltip();
            viewport.addClass('is-animating');
            $('.mc-day,.tmp-day').removeClass('show-tooltip');

            var grids = viewport.children('.mc-grid');
            if (grids.length > 1) {
                grids.not(':last').remove();
                var lastEl = grids.last()[0];
                if (lastEl.getAnimations) lastEl.getAnimations().forEach(function (a) { a.cancel(); });
            }

            var d = new Date(st.year, st.month + delta, 1);
            st.year = d.getFullYear();
            st.month = d.getMonth();
            cal.find('.mc-title').text(titleText(st));

            var oldGrid = viewport.children('.mc-grid').last();
            var newGrid = $(buildGridHtml(st));
            viewport.append(newGrid);

            var reduceMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
            if (reduceMotion || !newGrid[0].animate) {
                oldGrid.remove();
                viewport.removeClass('is-animating'); // Tháo khóa nếu không có animation
                return;
            }

            var dir = delta > 0 ? 1 : -1;
            var opts = { duration: 260, easing: 'cubic-bezier(0.22, 1, 0.36, 1)', fill: 'both' };

            oldGrid[0].animate([
                { transform: 'translateY(0)', opacity: 1 },
                { transform: 'translateY(' + (-dir * 40) + '%)', opacity: 0 }
            ], opts).onfinish = function () {
                oldGrid.remove();
            };

            newGrid[0].animate([
                { transform: 'translateY(' + (dir * 40) + '%)', opacity: 0 },
                { transform: 'translateY(0)', opacity: 1 }
            ], opts).onfinish = function () {
                // [THÁO KHÓA VÁCH NGĂN]: Trượt lịch xong thì mở lại để Tooltip ngoi lên
                viewport.removeClass('is-animating');
            };
        };
        // ============================================================
        // ĐỘNG CƠ TOOLTIP GLOBAL (ĐỊNH VỊ TUYỆT ĐỐI THEO TỌA ĐỘ CHUỘT)
        // ============================================================

        // Sinh ra 1 cái khung Tooltip duy nhất nằm ở ngoài cùng màn hình
        if ($('#global-task-tooltip').length === 0) {
            $('body').append('<div id="global-task-tooltip" class="global-task-tooltip"></div>');
        }
        var $globalTooltip = $('#global-task-tooltip');
        var pinnedDay = null; // Biến nhớ xem ông có đang click ghim nó lại không

        function showGlobalTooltip(dayEl) {
            var $day = $(dayEl), raw = $day.attr('data-tooltip-content');
            if (!raw) return;
            try { $globalTooltip.html(decodeURIComponent(raw)); } catch(e) { return; }
            $globalTooltip.css({maxHeight: 'none', overflowY: 'visible'}).addClass('show');
            // Tooltip 1-2 nội dung tự cao theo nội dung, TUYỆT ĐỐI không tạo thanh cuộn giả.
            // Chỉ tooltip thật sự vượt quá giới hạn màn hình mới cuộn bên trong.
            var maxTooltipHeight = Math.max(120, Math.min(410, window.innerHeight - 24));
            var naturalHeight = $globalTooltip[0].offsetHeight;
            if (naturalHeight > maxTooltipHeight) {
                $globalTooltip.css({maxHeight: maxTooltipHeight + 'px', overflowY: 'auto'});
            }
            var rect = dayEl.getBoundingClientRect();
            var ttWidth = $globalTooltip.outerWidth(), ttHeight = $globalTooltip.outerHeight();
            var left = Math.max(10, Math.min(window.innerWidth - ttWidth - 10, rect.left + rect.width / 2 - ttWidth / 2));
            var top = rect.top - ttHeight - 10, below = false;
            if (top < 10) { top = Math.max(10, Math.min(window.innerHeight - ttHeight - 10, rect.bottom + 10)); below = true; }
            $globalTooltip.toggleClass('tmp-tooltip-below', below);
            $globalTooltip.css({top: top + 'px', left: left + 'px', '--arrow-left': Math.max(5,Math.min(95,(rect.left + rect.width/2-left)/ttWidth*100))+'%'});
        }
        function hideGlobalTooltip() { $globalTooltip.removeClass('show tmp-tooltip-below'); }
        function clearPin() { pinnedDay = null; $('.mc-day,.tmp-day').removeClass('show-tooltip'); hideGlobalTooltip(); }
        CMSMasterJs.HideTaskTooltip = clearPin;
        $(document).on('mouseenter.taskMiniTip', '.mc-day.has-tooltip,.tmp-day.has-tooltip', function () {
            if (!pinnedDay) showGlobalTooltip(this);
        }).on('mouseleave.taskMiniTip', '.mc-day.has-tooltip,.tmp-day.has-tooltip', function () {
            if (!pinnedDay) hideGlobalTooltip();
        });
        $(document).on('click.taskMiniTip', '.mc-day.has-tooltip,.tmp-day.has-tooltip', function (e) {
            e.stopPropagation();
            if (pinnedDay === this) { clearPin(); return; }
            clearPin(); pinnedDay = this; $(this).addClass('show-tooltip'); showGlobalTooltip(this);
        });
        // Giữ tooltip được ghim khi tương tác nội dung để có thể copy chữ.
        $(document).on('click.taskMiniTip', function(e) {
            if (!$(e.target).closest('#global-task-tooltip,.mc-day.has-tooltip,.tmp-day.has-tooltip').length) clearPin();
        });
        $(document).on('keydown.taskMiniTip', function(e) { if (e.key === 'Escape') clearPin(); });
        // WebForms timeline scroll is a separate scroll container; pinned tips must not drift.
        document.addEventListener('scroll', function(e) {
            if (e.target && e.target.closest && e.target.closest('.panel-list,.tmp-scroll,.tmp-board-dates,.tmp-timeline-xbar')) clearPin();
        }, true);
    });
        // Xử lý Z-Index để MessageBox không bị chìm dưới Popup
        (function () {
            var BASE_Z = 106000;

            function stackBusinessModals() {
                var $modals = $('.modal:visible').not('#modal-notify');
                if ($modals.length > 1) {
                    $('.modal-backdrop').each(function (i) {
                        $(this).css('z-index', BASE_Z + (i * 10));
                    });
                    $modals.each(function (i) {
                        $(this).css('z-index', BASE_Z + (i * 10) + 5);
                    });
                }
            }

            function raiseNotify() {
                var notify = document.getElementById('modal-notify');
                if (!notify || !$(notify).is(':visible')) return;

                var $others = $('.modal:visible').not(notify);
                if ($others.length === 0) return;

                var maxZ = BASE_Z;
                $others.each(function () {
                    maxZ = Math.max(maxZ, parseInt(window.getComputedStyle(this).zIndex, 10) || 0);
                });

                notify.style.setProperty('z-index', String(maxZ + 20), 'important');
                var bds = document.querySelectorAll('.modal-backdrop');
                if (bds.length) bds[bds.length - 1].style.setProperty('z-index', String(maxZ + 15), 'important');
            }

            function raiseNotifyRepeatedly() {
                [0, 100, 250, 500, 900].forEach(function (ms) {
                    setTimeout(function () { stackBusinessModals(); raiseNotify(); }, ms);
                });
            }

            ['show.bs.modal', 'shown.bs.modal'].forEach(function (evt) {
                document.addEventListener(evt, function (e) {
                    if (e.target && e.target.id === 'modal-notify') raiseNotifyRepeatedly();
                });
            });

            document.addEventListener('hidden.bs.modal', function (e) {
                if (e.target && e.target.id === 'modal-notify') {
                    e.target.style.removeProperty('z-index');
                }
            });

            if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(raiseNotifyRepeatedly);
            }

            // Chỉ server đóng popup sau khi UpdateAssignments() thành công.
    })();
</script>
<script type="text/javascript">
    (function () {
        var pickedFieldId =
        '<%= hdfPickedIdsClient.ClientID %>';

        var originalFieldId =
        '<%= hdfOriginalAssignedIdsClient.ClientID %>';

        var summaryLabelId =
        '<%= lblAssignmentSummary.ClientID %>';

        function getElement(id) {
            return document.getElementById(id);
        }

        function readIds(raw) {
            var ids = new Set();

            (raw || '').split(',').forEach(function (part) {
                var id = part.trim().toLowerCase();

                if (id) {
                    ids.add(id);
                }
            });

            return ids;
        }

        function updateSummary() {
            var pickedField = getElement(pickedFieldId);
            var originalField = getElement(originalFieldId);

            // Ưu tiên ID của ASP.NET Label, dự phòng tìm theo CSS class.
            var summary = getElement(summaryLabelId)
                || document.querySelector('.task-assignment-summary');

            if (!pickedField || !originalField || !summary) {
                console.warn(
                    '[TaskMemberPicker] Không tìm thấy trường dữ liệu hoặc footer.',
                    {
                        pickedFieldFound: !!pickedField,
                        originalFieldFound: !!originalField,
                        summaryFound: !!summary
                    }
                );
                return;
            }

            var picked = readIds(pickedField.value);
            var original = readIds(originalField.value);

            var kept = 0;

            picked.forEach(function (id) {
                if (original.has(id)) {
                    kept++;
                }
            });

            var added = picked.size - kept;
            var removed = original.size - kept;

            summary.textContent =
                picked.size + ' nhân viên sau lưu' +
                ' · Giữ ' + kept +
                ' · Thêm ' + added +
                ' · Gỡ ' + removed;
        }

        function handleMemberCheckboxChange(event) {
            var checkbox = event.target;

            if (!checkbox ||
                checkbox.tagName !== 'INPUT' ||
                checkbox.type !== 'checkbox') {
                return;
            }

            // Chỉ xử lý checkbox nằm trong dòng nhân viên.
            var row = checkbox.closest(
                '.member-item-row[id^="mem-row-"]'
            );

            if (!row) {
                return;
            }

            var pickedField = getElement(pickedFieldId);

            if (!pickedField) {
                console.error(
                    '[TaskMemberPicker] Không tìm thấy hdfPickedIdsClient.'
                );
                return;
            }

            // ID đã có sẵn trong id="mem-row-{GUID}".
            // Không cần phụ thuộc ID render của HiddenField trong Repeater.
            var prefix = 'mem-row-';
            var userId = row.id
                .substring(prefix.length)
                .trim()
                .toLowerCase();

            if (!userId) {
                return;
            }

            var picked = readIds(pickedField.value);

            if (checkbox.checked) {
                picked.add(userId);
            } else {
                picked.delete(userId);
            }

            // Lưu trạng thái lựa chọn, kể cả người bị ẩn do lọc.
            pickedField.value = Array.from(picked).join(',');

            // Cập nhật ngay trên trình duyệt, không cần postback.
            updateSummary();
        }

        // Đăng ký duy nhất một lần.
        // Listener trên document vẫn hoạt động khi UpdatePanel
        // thay thế các phần tử Repeater.
        if (!window.__taskMemberSelectionListenerBound) {
            document.addEventListener(
                'change',
                handleMemberCheckboxChange,
                true
            );

            window.__taskMemberSelectionListenerBound = true;
        }

        // Sau khi UpdatePanel hoàn tất postback, lấy lại dữ liệu mới.
        if (window.Sys &&
            window.Sys.WebForms &&
            window.Sys.WebForms.PageRequestManager) {

            window.Sys.WebForms.PageRequestManager
                .getInstance()
                .add_endRequest(updateSummary);
        }

        updateSummary();
    })();
</script>
<script type="text/javascript">
    /* Task member planner: one selection store; popover preview + resource timeline. */
    (function (window, document, $) {
        'use strict';
        if (!$ || (window.TaskMemberPlanner && window.TaskMemberPlanner.__v4)) return;
        var state = { taskId: '', data: null, json: '', tab: 'list', onlyPicked: false, filters: { project: { keyword: '', role: '' }, other: { keyword: '', role: '' } }, collapsed: { selected: false, available: true }, filterRevision: null, dayStart: 0, calendarButton: null, calendarData: null };
        var DAY_MS = 86400000;
        function root() { return document.querySelector('.task-member-picker-root'); }
        function get(id) { var r = root(); return r ? r.querySelector('#' + id) : null; }
        function f(suffix) { var r = root(), parent = r && r.parentElement; return parent ? parent.querySelector('input[type="hidden"][id$="' + suffix + '"]') : null; }
        function parseIds(text) { var result = [], seen = {}; (text || '').split(',').forEach(function (s) { s = s.trim().toLowerCase(); if (s && !seen[s]) { seen[s] = true; result.push(s); } }); return result; }
        function picked() { return parseIds(f('hdfPickedIdsClient') ? f('hdfPickedIdsClient').value : ''); }
        function originals() { return parseIds(f('hdfOriginalAssignedIdsClient') ? f('hdfOriginalAssignedIdsClient').value : ''); }
        function esc(str) { return String(str == null ? '' : str).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;'); }
        function dateUtc(s) { var a = s.split('-'); return new Date(Date.UTC(+a[0], +a[1] - 1, +a[2])); }
        function dayKey(d) { return d.toISOString().slice(0, 10); }
        function fmtDate(s) { var p = s.split('-'); return p[2] + '/' + p[1] + '/' + p[0]; }
        function findPerson(id) { if (!state.data) return null; id = String(id).toLowerCase(); return state.data.members.find(function (m) { return String(m.id).toLowerCase() === id; }) || null; }
        function isReadonly() { return !!(state.data && state.data.readonlyMode); }
        function syncSelection() {
            var r = root(), chosen = picked(), initial = originals(); if (!r) return;
            Array.prototype.forEach.call(r.querySelectorAll('.member-item-row[id^="mem-row-"]'), function (row) {
                var id = row.id.slice('mem-row-'.length).toLowerCase();
                var box = row.querySelector('input[type="checkbox"]');
                var active = chosen.indexOf(id) >= 0;
                if (box) box.checked = active;
                row.classList.toggle('is-selected', active);
            });
            Array.prototype.forEach.call(r.querySelectorAll('.tmp-employee'), function (row) {
                var id = row.getAttribute('data-id').toLowerCase(), active = chosen.indexOf(id) >= 0, old = initial.indexOf(id) >= 0;
                row.classList.toggle('is-selected', active);
                row.classList.toggle('tmp-will-add', active && !old);
                row.classList.toggle('tmp-will-remove', old && !active);
                var calendarRow = r.querySelector('.tmp-board-dayrow[data-id="' + id + '"]');
                if (calendarRow) { calendarRow.classList.toggle('is-selected', active); calendarRow.classList.toggle('tmp-will-remove', old && !active); }
                var box = row.querySelector('input[type="checkbox"]');
                if (box) box.checked = active;
            });
            // Có thay đổi thật mới cho phép khôi phục; hỗ trợ cả khi footer được UpdatePanel thay mới.
            var restoreButton = document.getElementById('tmpRestoreOriginal');
            if (restoreButton) {
                var same = chosen.length === initial.length &&
                    chosen.every(function (id) { return initial.indexOf(id) >= 0; });
                restoreButton.disabled = isReadonly() || same;
                restoreButton.title = restoreButton.disabled ?
                    'Danh sách hiện tại trùng với phân công ban đầu' :
                    'Hoàn tác mọi thay đổi phân công chưa lưu';
            }
            var pending = document.getElementById('tmpPendingChanges');
            if (pending) {
                pending.hidden = isReadonly();
                var nAdded = chosen.filter(function (id) { return initial.indexOf(id) < 0; }).length;
                var nRemoved = initial.filter(function (id) { return chosen.indexOf(id) < 0; }).length;
                var changed = nAdded + nRemoved > 0;
                pending.classList.toggle('has-changes', changed);
                pending.textContent = changed ? 'Chưa lưu · +' + nAdded + ' / −' + nRemoved : 'Chưa có thay đổi';
                pending.title = changed ? ('Thêm ' + nAdded + ' · Gỡ ' + nRemoved + ' nhân viên') : 'Lựa chọn trùng với phân công hiện tại';
            }
            var label = document.querySelector('[id$="lblAssignmentSummary"]');
            if (label) {
                var kept = chosen.filter(function (id) { return initial.indexOf(id) >= 0; }).length;
                var outsideAdded = chosen.filter(function (id) { var m = findPerson(id); return initial.indexOf(id) < 0 && m && m.group === 'other'; }).length;
                label.textContent = chosen.length + ' nhân viên sau lưu · Giữ ' + kept + ' · Thêm ' + (chosen.length - kept) + ' · Gỡ ' + (initial.length - kept) + (outsideAdded ? ' · Thêm ' + outsideAdded + ' vào dự án' : '');
            }
        }
        function restoreOriginal() {
            if (isReadonly()) return;
            var field = f('hdfPickedIdsClient');
            if (!field) return;
            // Bảo toàn tập người phân công khi mở popup: không đụng DB, bộ lọc hay tab.
            field.value = originals().join(',');
            closePopover();
            syncSelection(); // Cập nhật luôn Repeater ẩn: postback sau đó không phục hồi lựa chọn cũ.
            renderNewList(true);
            if (state.tab === 'timeline') renderTimeline(true);
        }
        function setPicked(id, on) {
            if (isReadonly()) return;
            id = String(id || '').toLowerCase(); var chosen = picked().filter(function (x) { return x !== id; });
            if (on) chosen.push(id);
            var el = f('hdfPickedIdsClient'); if (!el) return;
            el.value = chosen.join(','); syncSelection(); renderNewList(true);
            if (state.tab === 'timeline') renderTimeline(true);
        }
        function readData() {
            var el = f('hdfTimelineJson'), idField = f('hdfTaskPickerId'), id = idField ? idField.value : '';
            if (!el || !el.value) return;
            if (id !== state.taskId) {
                state.tab = 'list'; state.onlyPicked = false; state.filters = { project: { keyword: '', role: '' }, other: { keyword: '', role: '' } }; state.collapsed = { selected: false, available: true }; state.filterRevision = null; state.dayStart = 0;
                closePopover(); state.taskId = id;
            }
            if (el.value !== state.json) {
                try {
                    state.data = JSON.parse(el.value); state.json = el.value;
                    if (state.data.filters) {
                        ['project', 'other'].forEach(function (g) {
                            var v = state.data.filters[g] || {};
                            state.filters[g] = { keyword: v.keyword || '', role: (v.role || '').toLowerCase() };
                        });
                    }
                } catch (ex) { console.error('Không đọc được dữ liệu lịch phân bổ.', ex); state.data = null; }
            }
            var only = get('tmpOnlyPicked'); if (only) only.checked = state.onlyPicked;
            var tags = get('tmpTimelineTags');
            if (tags) {
                var pp = state.filters.project || {}, oo = state.filters.other || {};
                var filterData = state.data && state.data.filters || {};
                tags.hidden = !((pp.keyword && pp.keyword === oo.keyword) || (pp.role && pp.role === oo.role) || (filterData.source && filterData.source !== 'all'));
            }
            // Chỉ phản hồi các thao tác lọc do server xác nhận, không tự đổi
            // trạng thái mở/đóng khi tick nhân viên hay chuyển chế độ.
            var revisionField = f('tmpFilterRevision');
            var revision = revisionField ? revisionField.value : '0';
            if (state.filterRevision !== null && revision !== state.filterRevision) {
                state.collapsed.selected = true;
                state.collapsed.available = false;
            }
            state.filterRevision = revision;
            showTab(state.tab, true); syncSelection();
        }
        function showTab(mode, avoidReload) {
            var list = get('tmpPaneList'), timeline = get('tmpPaneTimeline'), b1 = get('tmpTabList'), b2 = get('tmpTabTimeline');
            if (!list || !timeline) return;
            state.tab = mode === 'timeline' ? 'timeline' : 'list'; closePopover();
            list.hidden = state.tab !== 'list'; timeline.hidden = state.tab !== 'timeline';
            if (state.tab === 'list') renderNewList(true);
            if (b1) { b1.classList.toggle('is-active', state.tab === 'list'); b1.setAttribute('aria-selected', state.tab === 'list' ? 'true' : 'false'); b1.tabIndex = state.tab === 'list' ? 0 : -1; }
            if (b2) { b2.classList.toggle('is-active', state.tab === 'timeline'); b2.setAttribute('aria-selected', state.tab === 'timeline' ? 'true' : 'false'); b2.tabIndex = state.tab === 'timeline' ? 0 : -1; }
            if (state.tab === 'timeline') renderTimeline(avoidReload);
        }
        function calendarPosition() {
            var r = root(), pop = get('tmpCalendarPop'), btn = state.calendarButton;
            if (!r || !pop || !btn || !document.contains(btn)) { closePopover(); return; }
            var ownPanel = btn.closest('.member-list-panel'), other = get(ownPanel.id === 'panelProject' ? 'panelCompany' : 'panelProject');
            if (!other) return;
            var ownList = btn.closest('.panel-list');
            var br = btn.getBoundingClientRect(), pr = other.getBoundingClientRect(), rr = r.getBoundingClientRect(), lr = ownList.getBoundingClientRect();
            if (br.bottom < lr.top || br.top > lr.bottom) { closePopover(); return; }
            var left = pr.left - rr.left + 5, top = pr.top - rr.top + 5, width = Math.max(215, pr.width - 10), height = Math.max(235, pr.height - 10);
            pop.style.left = Math.round(left) + 'px'; pop.style.top = Math.round(top) + 'px';
            pop.style.width = Math.round(width) + 'px'; pop.style.height = Math.round(height) + 'px';
            pop.classList.toggle('tmp-from-left', ownPanel.id === 'panelProject');
            pop.classList.toggle('tmp-from-right', ownPanel.id !== 'panelProject');
            pop.style.setProperty('--tmp-arrow-y', Math.max(16, Math.min(height - 16, br.top + br.height / 2 - pr.top)) + 'px');
            var available = height - 125;
            var size = Math.max(33, Math.min(59, Math.floor((available - 10) / 6)));
            pop.style.setProperty('--tmp-day-size', size + 'px');
        }
        function closePopover() {
            if (window.CMSMasterJs && window.CMSMasterJs.HideTaskTooltip) window.CMSMasterJs.HideTaskTooltip();
            var pop = get('tmpCalendarPop'); if (pop) { pop.hidden = true; pop.classList.remove('tmp-detail-mode'); }
            if (state.calendarButton) state.calendarButton.setAttribute('aria-expanded', 'false');
            state.calendarButton = null; state.calendarData = null;
        }
        function showDayDetails(person, info, key, destination, showBack) {
            if (!destination) return;
            var html = (showBack ? '<button type="button" class="tmp-pop-detail-back" id="tmpBackMonth">← Quay lại lịch</button>' : '')
                + '<strong>' + esc(fmtDate(key)) + ' · ' + esc(person.name) + '</strong>'
                + '<div style="color:#64748b;margin-top:5px">' + esc((info && info.text) || 'Không có thông tin') + '</div>';
            if (info && info.current) html += '<div class="tmp-pop-task"><small>Công việc đang phân công</small>Nhân viên đã tham gia task hiện tại (không tính trùng lịch).</div>';
            if (info && Array.isArray(info.tasks)) info.tasks.forEach(function (t) { html += '<div class="tmp-pop-task"><small>' + esc(t.projectName) + '</small><strong>[' + esc(t.code) + ']</strong> ' + esc(t.name) + '</div>'; });
            destination.innerHTML = html;
        }
        function openCalendar(button, id) {
            var r = root(), pop = get('tmpCalendarPop'); if (!r || !pop) return false;
            if (state.calendarButton === button && !pop.hidden) { closePopover(); return false; }
            closePopover();
            var row = button.closest('.member-item-row');
            var input = row ? row.querySelector('input[type="hidden"][id*="hdfScheduleJson"]') : null;
            var data = {}; try { data = input ? JSON.parse(input.value || '{}') : ((findPerson(id) || {}).schedule || {}); } catch (e) { console.error('Lịch nhân viên không hợp lệ', e); }
            var person = findPerson(id) || { id: id, name: (row ? $(row).find('.fw-bold.text-dark').first().text() : '') || 'Nhân viên', schedule: data };
            state.calendarButton = button; button.setAttribute('aria-expanded', 'true');
            get('tmpPopName').textContent = 'Lịch · ' + person.name;
            // Lịch mini chỉ lấy NGÀY CỦA TASK. Timeline vẫn được xem các ngày trước/sau.
            var taskStart = state.data && state.data.start, taskEnd = state.data && state.data.end;
            if (taskStart && taskEnd) {
                var taskData = {};
                Object.keys(data).forEach(function (key) {
                    if (key >= taskStart && key <= taskEnd) taskData[key] = data[key];
                });
                data = taskData;
            }
            var keys = Object.keys(data).sort();
            // Chọn tháng có nhiều ngày thuộc thời hạn thực của task nhất.
            var monthly = {}, bestMonth = (taskStart || keys[0] || new Date().toISOString().slice(0, 10)).slice(0, 7), bestCount = 0;
            keys.forEach(function (key) { var m = key.slice(0, 7); monthly[m] = (monthly[m] || 0) + 1; if (monthly[m] > bestCount) { bestCount = monthly[m]; bestMonth = m; } });
            state.calendarData = data;
            var d = dateUtc(bestMonth + '-01'); var cal = $('#tmpCalendar');
            cal.data('mc', { data: data, minKey: taskStart || keys[0] || null, maxKey: taskEnd || keys[keys.length - 1] || null, year: d.getUTCFullYear(), month: d.getUTCMonth() });
            if (window.TaskMemberCalendarEngine) window.TaskMemberCalendarEngine.render(cal);
            pop.hidden = false; calendarPosition();
            return false;
        }
        function dateWindow() {
            if (!state.data || !state.data.start || !state.data.end) return [];
            var a = dateUtc(state.data.displayStart || state.data.start).getTime();
            var b = dateUtc(state.data.displayEnd || state.data.end).getTime();
            var count = Math.floor((b - a) / DAY_MS) + 1;
            if (!count || count < 0) return [];
            var windowSize = count > 100 ? 90 : count;
            state.dayStart = Math.max(0, Math.min(state.dayStart, count - windowSize));
            var out = []; for (var i = state.dayStart; i < state.dayStart + windowSize; i++)out.push(dayKey(new Date(a + i * DAY_MS)));
            return out;
        }
        function renderTimeline(preserveScroll) {
            var container = get('tmpTimelineScroll'), board = get('tmpTimelineTable');
            if (!container || !board || !state.data) return;
            var oldRail = board.querySelector('.tmp-board-section-dates');
            var oldBar = get('tmpTimelineXBar');
            var oldX = oldRail ? oldRail.scrollLeft : (oldBar ? oldBar.scrollLeft : 0);
            var oldYs = {};
            ['selected', 'available'].forEach(function (g) {
                var el = board.querySelector('.tmp-board-section-names[data-group="' + g + '"]');
                oldYs[g] = el ? el.scrollTop : 0;
            });
            var days = dateWindow(), chosen = picked(), initial = originals();
            var a = dateUtc(state.data.displayStart || state.data.start).getTime();
            var b = dateUtc(state.data.displayEnd || state.data.end).getTime();
            var total = Math.floor((b - a) / DAY_MS) + 1;
            var showWindows = total > 100, prev = get('tmpWindowPrev'), next = get('tmpWindowNext'), lbl = get('tmpWindowLabel');
            if (prev) { prev.hidden = !showWindows; prev.disabled = state.dayStart === 0; }
            if (next) { next.hidden = !showWindows; next.disabled = state.dayStart + days.length >= total; }
            if (lbl) lbl.textContent = showWindows && days.length ? fmtDate(days[0]) + ' – ' + fmtDate(days[days.length - 1]) : 'Cuộn ngang để xem thêm ngày';
            if (state.data.timelineError) {
                board.innerHTML = '<div class="tmp-empty">Không tải được Lịch phân bổ. Vẫn có thể phân công tại chế độ Danh sách; hãy kiểm tra log máy chủ.</div>';
                var errorBar = get('tmpTimelineXBar'); if (errorBar) errorBar.hidden = true;
                return;
            }
            var members = (state.data.members || []).filter(function (m) {
                return chosen.indexOf(String(m.id).toLowerCase()) >= 0 || initial.indexOf(String(m.id).toLowerCase()) >= 0 || filterCandidate(m);
            });
            members.sort(function (a, b) {
                var ai = String(a.id).toLowerCase(), bi = String(b.id).toLowerCase();
                var sa = chosen.indexOf(ai) >= 0, sb = chosen.indexOf(bi) >= 0;
                var ao = initial.indexOf(ai) >= 0, bo = initial.indexOf(bi) >= 0;
                // Original staff keep their position when unchecked, avoiding a jump out of view.
                var ga = ao ? 0 : (sa ? 1 : 2);
                var gb = bo ? 0 : (sb ? 1 : 2);
                if (ga !== gb) return ga - gb;
                if (ga === 2) return freeCount(b) - freeCount(a) || Number(b.group === 'project') - Number(a.group === 'project') || (a.name || '').localeCompare(b.name || '');
                return (a.name || '').localeCompare(b.name || '');
            });
            var nowLocal = new Date();
            var todayKey = nowLocal.getFullYear() + '-' + ('0' + (nowLocal.getMonth() + 1)).slice(-2) + '-' + ('0' + nowLocal.getDate()).slice(-2);
            var dateHeadHtml = '<div class="tmp-board-months" role="row">';
            for (var k = 0; k < days.length;) {
                var mon = days[k].slice(0, 7), j = k;
                while (j < days.length && days[j].slice(0, 7) === mon) j++;
                dateHeadHtml += '<div class="tmp-board-month" style="width:' + ((j - k) * 56) + 'px">Tháng ' + esc(mon.slice(5)) + '/' + esc(mon.slice(0, 4)) + '</div>';
                k = j;
            }
            dateHeadHtml += '</div><div class="tmp-board-dayheads" role="row">';
            days.forEach(function (day) {
                var d = dateUtc(day), weekdays = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'];
                var inside = day >= state.data.start && day <= state.data.end, boundary = day === state.data.start || day === state.data.end;
                dateHeadHtml += '<div class="tmp-board-dayhead ' + (inside ? 'inside-task' : 'outside-task') + (boundary ? ' task-boundary' : '') + (day === todayKey ? ' is-today' : '') + (d.getUTCDay() === 0 || d.getUTCDay() === 6 ? ' is-weekend' : '') + '" title="' + esc(fmtDate(day) + (day === todayKey ? ' · Hôm nay' : '')) + '"><b>' + d.getUTCDate() + '</b><span>' + weekdays[d.getUTCDay()] + '</span></div>';
            });
            dateHeadHtml += '</div>';
            var groups = ['selected', 'available'].map(function (group) {
                var people = members.filter(function (m) {
                    var checked = chosen.indexOf(String(m.id).toLowerCase()) >= 0;
                    var existed = initial.indexOf(String(m.id).toLowerCase()) >= 0;
                    return group === 'selected' ? (checked || existed) : (!checked && !existed);
                });
                var nameHtml = '', dateHtml = '';
                if (!people.length) nameHtml = '<div class="tmp-board-no-results">' + (group === 'selected' ? 'Chưa chọn nhân viên.' : 'Không có ứng viên phù hợp bộ lọc.') + '</div>';
                people.forEach(function (m) {
                    var id = String(m.id).toLowerCase(), active = chosen.indexOf(id) >= 0, old = initial.indexOf(id) >= 0;
                    var status = badgeFor(m, active, old);
                    nameHtml += '<div class="tmp-board-person-row tmp-employee ' + (active ? 'is-selected ' : '') + (old && !active ? 'tmp-will-remove ' : '') + '" data-id="' + esc(id) + '">' +
                        '<div class="tmp-person-cell"><input type="checkbox" data-tmp-pick="' + esc(id) + '" ' + (active ? 'checked ' : '') + (isReadonly() ? 'disabled ' : '') + 'aria-label="' + (old && !active ? 'Tích lại để giữ phân công ' : active ? 'Bỏ phân công ' : 'Phân công ') + esc(m.name) + '"/>' +
                        '<span class="tmp-person-avatar">' + (m.avatarHtml || '') + '</span>' +
                        '<span class="tmp-person-text tmp-person-hover" data-tip-name="' + esc(m.name) + '" data-tip-role="' + esc(m.role || '') + '" data-tip-email="' + esc(m.email || '') + '"><span class="tmp-person-headline"><strong class="tmp-person-name">' + esc(m.name) + '</strong><span class="tmp-person-source" title="' + (m.group === 'project' ? 'Đã tham gia dự án' : 'Chưa tham gia dự án') + '"><i class="tmp-v3-dot ' + (m.group === 'project' ? 'project' : 'other') + '"></i></span>' +
                        (m.isPM ? ' <span class="badge bg-danger tmp-person-pm">PM</span>' : '') +
                        '<span class="tmp-person-badges">' + status + '</span></span>' +
                        '<span class="tmp-person-subline">' +
                        (m.role ? '<span class="tmp-person-position">' + esc(m.role) + '</span>' : '') +
                        (m.role && m.email ? '<span aria-hidden="true">·</span>' : '') +
                        '<span class="tmp-person-email">' + esc(m.email || '') + '</span></span></span></div></div>';
                    dateHtml += '<div class="tmp-board-dayrow ' + (active ? 'is-selected ' : '') + (old && !active ? 'tmp-will-remove' : '') + '" data-id="' + esc(id) + '">';
                    days.forEach(function (day) {
                        var entry = m.schedule && m.schedule[day] || null, inside = day >= state.data.start && day <= state.data.end;
                        var st = entry && entry.status || 'weekend', taskCount = entry && entry.tasks ? entry.tasks.length : 0, current = !!(entry && entry.current);
                        var label = inside ? (st === 'busy' ? (taskCount + ' công việc') : st === 'holiday' ? 'Nghỉ lễ' : st === 'weekend' ? 'Nghỉ' : current ? 'Công việc' : 'Trống') : (st === 'busy' ? '<span class="tmp-v3-context-dot"></span>' : '');
                        var tooltip = entry && window.TaskMemberCalendarEngine ? window.TaskMemberCalendarEngine.tooltipHtml(entry, day, m.name, !inside) : '';
                        dateHtml += '<div class="tmp-board-daycell ' + (inside ? 'inside-task' : 'outside-task') + (day === state.data.start ? ' task-boundary-start' : '') + (day === state.data.end ? ' task-boundary-end' : '') + '"><button type="button" class="tmp-day st-' + esc(st) + (inside ? '' : ' outside-task') + (current ? ' has-current' : '') + (inside && current && st === 'free' ? ' st-current-only' : '') + (entry ? ' has-tooltip' : '') + '" data-id="' + esc(id) + '" data-day="' + esc(day) + '"' + (tooltip ? ' data-tooltip-content="' + encodeURIComponent(tooltip) + '"' : '') + ' aria-label="' + esc(m.name + ' · ' + fmtDate(day) + ' · ' + (entry ? entry.text : 'Không có dữ liệu')) + '">' + label + '</button></div>';
                    });
                    dateHtml += '</div>';
                });
                return {
                    key: group, people: people, names: nameHtml, dates: dateHtml
                };
            });
            // UX V4.1: tiêu đề hai nhóm luôn nằm trong khung.
            // Bấm header để ĐÓNG/MỞ thực sự, không còn ép chế độ ưu tiên hoặc nút 2+2.
            var rowHeight = 74, headerHeight = 53, groupHeight = 31;
            var available = Math.max(0, (board.clientHeight || 390) - headerHeight - 2 * groupHeight);
            var slots = Math.max(0, Math.floor(available / rowHeight));
            var selectedGroup = groups[0], candidateGroup = groups[1];
            var selectedOpen = !state.collapsed.selected, candidateOpen = !state.collapsed.available;
            var selectedCount = selectedGroup.people.length, candidateCount = candidateGroup.people.length;
            var selectedRows = 0, candidateRows = 0;
            // Một nhóm đang đóng thì nhóm mở được hưởng trọn tối đa 4 dòng.
            if (selectedOpen && !candidateOpen) {
                selectedRows = Math.min(4, selectedCount, slots);
            } else if (!selectedOpen && candidateOpen) {
                candidateRows = Math.min(4, candidateCount, slots);
            } else if (selectedOpen && candidateOpen) {
                // Khi cả hai mở, chia số dòng theo số người đang có; không tạo nút/chế độ riêng.
                // Dành ít nhất một dòng cho mỗi nhóm có dữ liệu nếu màn hình đủ chỗ.
                var remainder = slots;
                if (selectedCount && remainder > 0) { selectedRows++; remainder--; }
                if (candidateCount && remainder > 0) { candidateRows++; remainder--; }
                while (remainder > 0 && (selectedRows < Math.min(4, selectedCount) || candidateRows < Math.min(4, candidateCount))) {
                    var canSelected = selectedRows < Math.min(4, selectedCount);
                    var canCandidate = candidateRows < Math.min(4, candidateCount);
                    if (canSelected && (!canCandidate || selectedRows <= candidateRows)) selectedRows++;
                    else if (canCandidate) candidateRows++;
                    remainder--;
                }
            }
            selectedGroup.bodyHeight = selectedRows * rowHeight;
            candidateGroup.bodyHeight = candidateRows * rowHeight;
            // Với nhóm mở nhưng không có nhân sự, dùng khoảng trống nhỏ cho dòng thông báo.
            var sparePixels = Math.max(0, available - selectedGroup.bodyHeight - candidateGroup.bodyHeight);
            if (selectedOpen && !selectedCount && sparePixels >= 34) {
                selectedGroup.bodyHeight = 34; sparePixels -= 34;
            }
            if (candidateOpen && !candidateCount && sparePixels >= 34) {
                candidateGroup.bodyHeight = 34;
            }
            var width = days.length * 56, groupsHtml = '';
            groups.forEach(function (g) {
                var selected = g.key === 'selected', title = selected ? 'Nhân sự được chọn' : 'Nhân sự có thể phân công';
                var keptChecked = g.people.filter(function (m) { return chosen.indexOf(String(m.id).toLowerCase()) >= 0; }).length;
                var pendingRemoval = g.people.length - keptChecked;
                var className = selected ? ' tmp-selected' : ' tmp-other';
                var folded = !!state.collapsed[g.key];
                var label = title + ' (' + (selected ? keptChecked : g.people.length) + ')' + (selected && pendingRemoval ? ' · Sẽ gỡ ' + pendingRemoval : '');
                groupsHtml += '<section class="tmp-board-section' + (folded ? ' is-collapsed' : '') + '" data-group="' + g.key + '" style="height:' + (groupHeight + Math.floor(g.bodyHeight || 0)) + 'px">' +
                    '<div class="tmp-board-group' + className + '"><button type="button" class="tmp-toggle-group" data-group="' + g.key + '" aria-expanded="' + (!folded) + '" title="' + (folded ? 'Mở' : 'Thu gọn') + ' ' + title.toLowerCase() + '">' + (folded ? '▸' : '▾') + ' ' + label + '</button></div>' +
                    '<div class="tmp-board-group-days' + className + '"></div>' +
                    '<div class="tmp-board-section-names" data-group="' + g.key + '">' + g.names + '</div>' +
                    '<div class="tmp-board-section-dates" data-group="' + g.key + '"><div class="tmp-board-track" style="width:' + width + 'px">' + g.dates + '</div></div></section>';
            });
            board.innerHTML = '<div class="tmp-board-head-names"><div class="tmp-board-person-head" role="columnheader">Danh sách nhân sự</div></div>' +
                '<div class="tmp-board-head-dates" id="tmpDatesHeadScroll"><div class="tmp-board-track" style="width:' + width + 'px">' + dateHeadHtml + '</div></div>' +
                '<div class="tmp-board-group-stack">' + groupsHtml + '</div>';
            var bar = get('tmpTimelineXBar'), track = get('tmpTimelineXTrack');
            if (track) track.style.width = width + 'px';
            if (bar) bar.hidden = !days.length;
            var offset = days.length ? Math.floor((dateUtc(state.data.start).getTime() - dateUtc(days[0]).getTime()) / DAY_MS) : 0;
            var x = preserveScroll ? oldX : Math.max(0, (offset - 5) * 56);
            board.querySelectorAll('.tmp-board-section-dates').forEach(function (rail) { rail.scrollLeft = x; });
            var head = get('tmpDatesHeadScroll'); if (head) head.scrollLeft = x;
            if (bar) bar.scrollLeft = x;
            ['selected', 'available'].forEach(function (group) {
                var y = preserveScroll ? (oldYs[group] || 0) : 0;
                var names = board.querySelector('.tmp-board-section-names[data-group="' + group + '"]');
                var dates = board.querySelector('.tmp-board-section-dates[data-group="' + group + '"]');
                if (names) names.scrollTop = y;
                if (dates) dates.scrollTop = y;
            });
            var detail = get('tmpDayDetail'); if (detail) detail.hidden = true;
            syncSelection();
        }
        function sourceRow(id) {
            var src = get('tmpLegacySources');
            if (!src) return null;
            var rows = src.querySelectorAll('.member-item-row');
            for (var i = 0; i < rows.length; i++) if (rows[i].id === 'mem-row-' + id) return rows[i];
            return null;
        }
        function filterCandidate(m) {
            var source = state.data && state.data.filters && state.data.filters.source || 'all';
            if (source !== 'all' && m.group !== source) return false;
            var p = state.filters.project || {}, q = state.filters.other || {};
            var kw = p.keyword || q.keyword || '', role = p.role || q.role || '';
            return (!kw || (m.name + ' ' + (m.role || '') + ' ' + (m.email || '')).toLocaleLowerCase().indexOf(kw.toLocaleLowerCase()) >= 0) &&
                (!role || String(m.roleId || '').toLowerCase() === String(role).toLowerCase());
        }
        function freeCount(m) { return Math.max(0, Number(state.data.workingDays || 0) - Number(m.busy || 0)); }
        function badgeFor(m, selected, old) {
            if (old && !selected) return '<span class="badge tmp-pending-removal-badge" title="Chưa gỡ thực tế. Tích lại để giữ nhân viên."><i class="fas fa-user-minus me-1"></i>Sẽ gỡ phân công</span>';
            if (selected) return old ? '<span class="badge" style="color:#0284c7;background:#e0f2fe;border:1px solid #bae6fd"><i class="fas fa-user-check me-1"></i>Đã phân công</span>' : '<span class="badge" style="color:#64259b;background:#f0e8fc;border:1px solid #ddcdf3">Sẽ phân công</span>';
            var total = Number(state.data.workingDays || 0), free = freeCount(m);
            if (!total) return '<span class="badge bg-secondary">Không có ngày làm việc</span>';
            if (!free) return '<span class="badge bg-secondary">Trống 0/' + total + ' ngày</span>';
            return '<span class="badge bg-success"><i class="fas fa-calendar-check me-1"></i>Trống ' + free + '/' + total + ' ngày</span>';
        }
        function cardFromServer(m, selected, old) {
            var id = String(m.id).toLowerCase(), source = sourceRow(id), card;
            if (source) {
                card = source.cloneNode(true);
                Array.prototype.forEach.call(card.querySelectorAll('[id],[name],[for]'), function (n) { n.removeAttribute('id'); n.removeAttribute('name'); n.removeAttribute('for'); });
                card.removeAttribute('id');
                Array.prototype.forEach.call(card.querySelectorAll('input[type="hidden"],.row-schedule-panel'), function (n) { n.remove(); });
                var check = card.querySelector('input[type="checkbox"]');
                if (check) { check.removeAttribute('onclick'); check.removeAttribute('onchange'); check.setAttribute('data-tmp-pick', id); check.checked = selected; check.disabled = isReadonly(); }
                var button = card.querySelector('.btn-calendar-only');
                if (button) { button.removeAttribute('onclick'); button.setAttribute('data-tmp-calendar', id); button.setAttribute('aria-label', 'Xem lịch của ' + (m.name || 'nhân viên')); button.setAttribute('title', 'Xem lịch của ' + (m.name || 'nhân viên')); button.innerHTML = '<i class="far fa-calendar-alt" aria-hidden="true"></i>'; }
                var actions = button && button.parentElement;
                if (actions) { Array.prototype.slice.call(actions.children).forEach(function (n) { if (n !== button) n.remove(); }); actions.insertAdjacentHTML('afterbegin', badgeFor(m, selected, old)); }
            } else {
                // Fallback if an unusual historical user was not rendered by its Repeater.
                card = document.createElement('div'); card.className = 'member-item-row';
                card.innerHTML = '<div class="row-default-view"><div class="member-info-group flex-grow-1"><input type="checkbox" data-tmp-pick="' + esc(id) + '" ' + (selected ? 'checked ' : '') + (isReadonly() ? 'disabled ' : '') + '/>' + (m.avatarHtml || '') + '<div class="member-name-block"><div class="d-flex align-items-center flex-wrap gap-1 mb-1"><span class="fw-bold text-dark me-1">' + esc(m.name) + '</span><span class="member-position">' + esc(m.role || '') + '</span>' + (m.isPM ? '<span class="badge bg-danger" style="font-size:9.5px;padding:2px 5px">PM</span>' : '') + '</div><span class="member-email">' + esc(m.email || '') + '</span></div></div><div class="d-flex align-items-center gap-2 flex-shrink-0">' + badgeFor(m, selected, old) + '<button type="button" class="btn-calendar-only" data-tmp-calendar="' + esc(id) + '" title="Xem lịch" aria-label="Xem lịch của ' + esc(m.name) + '"><i class="far fa-calendar-alt" aria-hidden="true"></i></button></div></div>';
            }
            card.classList.toggle('is-selected', selected); card.classList.toggle('tmp-pending-removal', old && !selected); card.classList.remove('original-member'); card.setAttribute('data-tmp-id', id);
            card.classList.add('tmp-ux-clickable');
            var cb = card.querySelector('input[data-tmp-pick]');
            if (cb) cb.setAttribute('aria-label', (old && !selected ? 'Tích lại để giữ phân công ' : selected ? 'Bỏ phân công ' : 'Phân công ') + (m.name || 'nhân viên'));
            var name = card.querySelector('.fw-bold.text-dark');
            if (name) name.removeAttribute('title');
            var email = card.querySelector('.member-email');
            if (email) email.removeAttribute('title');
            var meta = card.querySelector('.member-name-block');
            if (meta) { meta.classList.add('tmp-person-hover'); meta.setAttribute('data-tip-name', m.name || 'Nhân viên'); meta.setAttribute('data-tip-role', m.role || ''); meta.setAttribute('data-tip-email', m.email || ''); }
            if (name) { var dot = document.createElement('i'); dot.className = 'tmp-v3-dot ' + (m.group === 'project' ? 'project' : 'other'); dot.title = m.group === 'project' ? 'Đã tham gia dự án' : 'Chưa tham gia dự án'; dot.setAttribute('aria-label', dot.title); name.insertAdjacentElement('afterend', dot); }
            return card;
        }
        function renderNewList(preserve) {
            var list = get('tmpSelectedList'), available = get('tmpAvailableList');
            if (!state.data || !list || !available) return;
            var oldL = list.scrollTop, oldR = available.scrollTop, chosen = picked(), original = originals();
            var members = (state.data.members || []), selected = [], candidates = [];
            members.forEach(function (m) { var id = String(m.id).toLowerCase(); (chosen.indexOf(id) >= 0 || original.indexOf(id) >= 0 ? selected : candidates).push(m); });
            // Keep the relative order of previously assigned employees when they are unchecked.
            // Otherwise the unchecked row would jump to the bottom and look as if it vanished.
            selected.sort(function (a, b) {
                function rank(m) { return original.indexOf(String(m.id).toLowerCase()) >= 0 ? 0 : 1; }
                return rank(a) - rank(b) || (a.name || '').localeCompare(b.name || '') || String(a.id).localeCompare(String(b.id));
            });
            candidates = candidates.filter(filterCandidate);
            candidates.sort(function (a, b) { return freeCount(b) - freeCount(a) || Number(b.group === 'project') - Number(a.group === 'project') || (a.name || '').localeCompare(b.name || '') || String(a.id).localeCompare(String(b.id)); });
            list.textContent = ''; available.textContent = '';
            var frag1 = document.createDocumentFragment(), frag2 = document.createDocumentFragment();
            selected.forEach(function (m) { var id = String(m.id).toLowerCase(); frag1.appendChild(cardFromServer(m, chosen.indexOf(id) >= 0, original.indexOf(id) >= 0)); });
            candidates.forEach(function (m) { frag2.appendChild(cardFromServer(m, false, false)); });
            list.appendChild(frag1); available.appendChild(frag2);
            if (!selected.length) list.innerHTML = '<div class="tmp-v3-empty"><i class="far fa-user fa-lg"></i><div style="margin-top:10px">Chưa chọn nhân viên cho công việc.<br/>Chọn từ danh sách bên phải.</div></div>';
            if (!candidates.length) available.innerHTML = '<div class="tmp-v3-empty">Không có ứng viên phù hợp bộ lọc.</div>';
            var a = get('tmpSelectedCount'), b = get('tmpAvailableCount'), warn = get('tmpPendingRemovalCount');
            if (a) a.textContent = chosen.length;
            if (b) b.textContent = candidates.length;
            if (warn) {
                var removedCount = original.filter(function (id) { return chosen.indexOf(id) < 0; }).length;
                warn.hidden = removedCount === 0;
                warn.textContent = removedCount ? removedCount + ' sẽ gỡ' : '';
            }
            if (preserve) { list.scrollTop = oldL; available.scrollTop = oldR; }
        }
        function setupWorkingDays() {
            if (!state.data) return;
            // Y is sent from the server in the same calculation used by the list badge.
            state._workingDays = Number(state.data.workingDays) || 0;
        }
        // Delegate events: UpdatePanel can replace toolbar/repeater markup after every postback.
        // `scroll` does not bubble reliably; capturing at document survives UpdatePanel replacements.
        // Hai khu vực cuộn dọc riêng, nhưng đồng bộ theo cặp tên/lịch trong từng nhóm.
        // Phần ngày và header tháng dùng một toạ độ cuộn ngang cho cả hai nhóm.
        document.addEventListener('scroll', function (e) {
            var target = e.target;
            if (!target || !target.classList) return;
            var group = target.getAttribute('data-group');
            var board = get('tmpTimelineTable'); if (!board) return;
            var isDate = target.classList.contains('tmp-board-section-dates');
            var isName = target.classList.contains('tmp-board-section-names');
            if (isDate || isName) {
                var names = board.querySelector('.tmp-board-section-names[data-group="' + group + '"]');
                var dates = board.querySelector('.tmp-board-section-dates[data-group="' + group + '"]');
                if (isDate && names && Math.abs(names.scrollTop - target.scrollTop) > 1) names.scrollTop = target.scrollTop;
                if (isName && dates && Math.abs(dates.scrollTop - target.scrollTop) > 1) dates.scrollTop = target.scrollTop;
                if (isDate) {
                    var x = target.scrollLeft, bar = get('tmpTimelineXBar'), head = get('tmpDatesHeadScroll');
                    if (bar && Math.abs(bar.scrollLeft - x) > 1) bar.scrollLeft = x;
                    if (head && Math.abs(head.scrollLeft - x) > 1) head.scrollLeft = x;
                    board.querySelectorAll('.tmp-board-section-dates').forEach(function (other) {
                        if (other !== target && Math.abs(other.scrollLeft - x) > 1) other.scrollLeft = x;
                    });
                }
            } else if (target.id === 'tmpTimelineXBar') {
                var newX = target.scrollLeft, header = get('tmpDatesHeadScroll');
                if (header && Math.abs(header.scrollLeft - newX) > 1) header.scrollLeft = newX;
                board.querySelectorAll('.tmp-board-section-dates').forEach(function (other) {
                    if (Math.abs(other.scrollLeft - newX) > 1) other.scrollLeft = newX;
                });
            }
        }, true);
        $(document).on('keydown.taskPlanner', '#tmpTabList,#tmpTabTimeline', function (e) {
            if (['ArrowLeft', 'ArrowRight', 'Home', 'End'].indexOf(e.key) < 0) return;
            e.preventDefault();
            var next = e.key === 'Home' ? 'list' : e.key === 'End' ? 'timeline' : this.id === 'tmpTabList' ? 'timeline' : 'list';
            showTab(next);
            var target = get(next === 'list' ? 'tmpTabList' : 'tmpTabTimeline');
            if (target) target.focus();
        });
        // Tooltip nhân viên sáng, thay tooltip đen native của browser.
        // Dùng data-* nên chạy ổn cả khi renderTimeline/renderNewList thay lại các row.
        var memberTip = document.createElement('div');
        memberTip.className = 'tmp-member-tooltip';
        memberTip.setAttribute('role', 'tooltip');
        memberTip.setAttribute('aria-hidden', 'true');
        document.body.appendChild(memberTip);
        function hideMemberTip() {
            memberTip.classList.remove('is-visible');
            memberTip.setAttribute('aria-hidden', 'true');
        }
        function showMemberTip(host) {
            if (!host || !document.body.contains(host)) return;
            var nm = host.getAttribute('data-tip-name') || '';
            var role = host.getAttribute('data-tip-role') || '';
            var email = host.getAttribute('data-tip-email') || '';
            if (!nm && !role && !email) return;
            memberTip.innerHTML = '<div class="tmp-member-tip-name">' + esc(nm) + '</div>' +
                (role ? '<div class="tmp-member-tip-role"><i class="far fa-id-badge"></i> ' + esc(role) + '</div>' : '') +
                (email ? '<div class="tmp-member-tip-email"><i class="far fa-envelope"></i> ' + esc(email) + '</div>' : '');
            memberTip.style.left = '0px'; memberTip.style.top = '0px';
            memberTip.classList.add('is-visible'); memberTip.setAttribute('aria-hidden', 'false');
            var rect = host.getBoundingClientRect(), w = memberTip.offsetWidth, h = memberTip.offsetHeight;
            var x = Math.max(10, Math.min(window.innerWidth - w - 10, rect.left));
            var y = rect.top - h - 9;
            if (y < 10) y = Math.min(window.innerHeight - h - 10, rect.bottom + 9);
            memberTip.style.left = Math.round(x) + 'px'; memberTip.style.top = Math.round(Math.max(10, y)) + 'px';
        }
        $(document).on('mouseenter.taskMemberTip focusin.taskMemberTip', '.tmp-person-hover', function () { showMemberTip(this); })
            .on('mouseleave.taskMemberTip focusout.taskMemberTip', '.tmp-person-hover', hideMemberTip);
        $(document).on('mouseenter.taskMemberTip', '.tmp-person-hover .tmp-person-source, .tmp-person-hover .tmp-v3-dot', hideMemberTip);
        $(document).on('scroll.taskMemberTip', '.tmp-board-section-names,.tmp-v3-list', hideMemberTip);
        // Click the unobstructed space on a staff card to select/deselect it.
        // Ignore the checkbox itself and calendar button so neither action runs twice.
        $(document).on('click.taskPlanner', '#tmpPaneList .tmp-ux-clickable .row-default-view', function (e) {
            if ($(e.target).closest('input,button,a,label,[role=button]').length) return;
            var box = this.querySelector('input[data-tmp-pick]');
            if (!box || box.disabled) return;
            box.checked = !box.checked;
            $(box).trigger('change');
        });
        $(document).on('click.taskPlanner', '#tmpRestoreOriginal', restoreOriginal);
        $(document).on('click.taskPlanner', '#tmpTabList', function () { showTab('list'); });
        $(document).on('click.taskPlanner', '#tmpTabTimeline', function () { showTab('timeline'); });
        $(document).on('change.taskPlanner', '#tmpOnlyPicked', function () { state.onlyPicked = this.checked; renderTimeline(true); });
        $(document).on('click.taskPlanner', '#tmpWindowPrev,#tmpWindowNext', function () { state.dayStart = Math.max(0, state.dayStart + (this.id === 'tmpWindowNext' ? 90 : -90)); renderTimeline(false); });
        $(document).on('click.taskPlanner', '.tmp-toggle-group', function () {
            var g = this.getAttribute('data-group');
            if (g !== 'selected' && g !== 'available') return;
            state.collapsed[g] = !state.collapsed[g];
            renderTimeline(true);
        });
        $(document).on('change.taskPlanner', '[data-tmp-pick]', function () { setPicked(this.getAttribute('data-tmp-pick'), this.checked); });
        $(document).on('click.taskPlanner', '[data-tmp-calendar]', function () { return openCalendar(this, this.getAttribute('data-tmp-calendar')); });
        $(document).on('change.taskPlanner', '.task-member-picker-root .member-item-row input[type="checkbox"]', function () { window.setTimeout(syncSelection, 0); });
        $(document).on('click.taskPlanner', '#tmpPopClose', closePopover);
        $(document).on('click.taskPlanner', function (e) {
            if (!e.target.closest('#tmpCalendarPop') && !e.target.closest('#global-task-tooltip') && !e.target.closest('.task-member-picker-root .btn-calendar-only')) closePopover();
        });
        document.addEventListener('keydown', function (e) { if (e.key === 'Escape') closePopover(); });
        document.addEventListener('scroll', function (e) {
            if (state.calendarButton && e.target.classList && e.target.classList.contains('panel-list') && e.target.contains(state.calendarButton)) closePopover();
        }, true);
        window.addEventListener('resize', function () { if (state.calendarButton) calendarPosition(); if (state.tab === 'timeline') renderTimeline(true); });
        function afterRequest() { readData(); setupWorkingDays(); renderNewList(true); if (state.tab === 'timeline') renderTimeline(true); }
        if (window.Sys && window.Sys.WebForms && window.Sys.WebForms.PageRequestManager) { window.Sys.WebForms.PageRequestManager.getInstance().add_endRequest(afterRequest); }
        $(function () { afterRequest(); });
        window.TaskMemberPlanner = { __v4: true, openCalendar: openCalendar, refresh: afterRequest, close: closePopover };
    })(window, document, window.jQuery);
</script>

<script type="text/javascript">
/* UX V3: chặn thao tác người dùng đóng popup khi có thay đổi chưa lưu.
 * Không chặn lệnh đóng từ server (Lưu thành công / ConfirmRequest).
 * Xác nhận sử dụng MessageBox sẵn có, không dùng window.confirm hoặc toast.
 */
(function (window, document) {
    'use strict';
    if (window.TaskMemberDiscardGuard && window.TaskMemberDiscardGuard.__v3) return;
    var asking = false;
    var closeTriggerId = '<%= lbtRequestDiscardTaskAssign.ClientID %>';
        function getRoot() { return document.querySelector('.task-member-picker-root'); }
        function getModal() {
            var r = getRoot();
            return r && r.closest ? r.closest('.modal') : null;
        }
        function parseIds(raw) {
            var result = {};
            (raw || '').split(',').forEach(function (entry) {
                var id = entry.trim().toLowerCase();
                if (id) result[id] = true;
            });
            return result;
        }
        function changed() {
            var root = getRoot();
            if (!root) return false;
            var original = root.parentElement.querySelector('input[id$="hdfOriginalAssignedIdsClient"]');
            var picked = root.parentElement.querySelector('input[id$="hdfPickedIdsClient"]');
            if (!original || !picked) return false;
            var first = parseIds(original.value), second = parseIds(picked.value);
            var x = Object.keys(first), y = Object.keys(second);
            return x.length !== y.length || x.some(function (id) { return !second[id]; });
        }
        function visible(el) {
            if (!el) return false;
            var style = window.getComputedStyle(el);
            return style.display !== 'none' && style.visibility !== 'hidden' &&
                !!(el.offsetWidth || el.offsetHeight || el.getClientRects().length);
        }
        function pickerIsTopModal() {
            var modal = getModal();
            if (!visible(modal)) return false;
            // Nếu MessageBox khác đang hiển thị, phím ESC phải thuộc về MessageBox đó.
            var modals = Array.prototype.filter.call(document.querySelectorAll('.modal'), visible);
            if (!modals.length) return true;
            var max = modals.map(function (el, i) {
                var z = parseInt(window.getComputedStyle(el).zIndex, 10);
                return { el: el, z: isNaN(z) ? 0 : z, index: i };
            }).sort(function (a, b) { return b.z - a.z || b.index - a.index; })[0];
            return max.el === modal;
        }
        function askDiscard() {
            if (asking || !changed()) return;
            var trigger = document.getElementById(closeTriggerId);
            if (!trigger) return;
            asking = true;
            trigger.click(); // ASP.NET async postback -> OpenMessageBox(Warning, AcceptCancel).
        }
        function catchClose(e) {
            var modal = getModal();
            if (!modal || !pickerIsTopModal() || !changed()) return;
            var target = e.target;
            if (!target || !target.closest) return;
            var dismiss = target.closest('[data-bs-dismiss="modal"], [data-dismiss="modal"], ' +
                '.modal-header .close, .modal-header .btn-close, ' +
                '.modal-header button, .modal-header a');
            var backdropClick = target === modal;
            if (!backdropClick && (!dismiss || !modal.contains(dismiss))) return;
            e.preventDefault();
            e.stopImmediatePropagation();
            askDiscard();
        }
        function catchEscape(e) {
            if ((e.key !== 'Escape' && e.keyCode !== 27) || !pickerIsTopModal() || !changed()) return;
            var pop = document.querySelector('#tmpCalendarPop');
            if (pop && !pop.hidden) return; // Escape trước tiên đóng mini calendar.
            e.preventDefault();
            e.stopImmediatePropagation();
            askDiscard();
        }
        // Bắt ở capture phase trước các sự kiện đóng do Bootstrap / ExtraModal đăng ký.
        document.addEventListener('click', catchClose, true);
        document.addEventListener('keydown', catchEscape, true);
        // Trong hệ thống, người dùng hủy MessageBox có thể chỉ đóng client-side.
        // Sau khi MessageBox đóng, cho phép thử đóng popup một lần nữa.
        document.addEventListener('hidden.bs.modal', function (event) {
            if (event.target !== getModal()) asking = false;
        }, true);
        if (window.jQuery) {
            window.jQuery(document).on('hidden.bs.modal.taskMemberDiscard', function (event) {
                if (event.target !== getModal()) asking = false;
            });
        }
        window.TaskMemberDiscardGuard = {
            __v3: true,
            cancelClose: function () { asking = false; }
        };
    })(window, document);
</script>
