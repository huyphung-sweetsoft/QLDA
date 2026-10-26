<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="NhanVienDetail.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fNhanVien.NhanVienDetail" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fNhanVien/Controls/CtrlNhanVienPopup.ascx" TagPrefix="SweetSoft" TagName="CtrlNhanVienPopup" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        /* ============================================================
           GENERAL
           ============================================================ */
        .icon-box { width: 48px; height: 48px; display: flex; align-items: center; justify-content: center; border-radius: 12px; font-size: 20px; flex-shrink: 0; }
        .spec-tile { background-color: #f8f9fa; border: 1px solid #dee2e6; border-radius: 0.5rem; padding: 1rem; height: 100%; min-width: 0; overflow-wrap: anywhere; word-break: break-word; }
        .col-xl-8.d-flex.flex-column, .col-xl-8.d-flex.flex-column .card, .col-xl-8.d-flex.flex-column .card-body { min-width: 0; }
        .spec-label { font-size: 11px; font-weight: 700; color: #6c757d; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 0.25rem; }

        /* ============================================================
           PROJECT LIST SIDE PANEL
           ============================================================ */
        .side-panel-overlay { position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; background: rgba(0, 0, 0, 0.5); z-index: 1040; display: none; backdrop-filter: blur(2px); }
        
        /* [ĐÃ SỬA]: TĂNG WIDTH TỪ 850px LÊN 1100px ĐỂ BẢNG TASK RỘNG RÃI HƠN */
        .side-panel { position: fixed; top: 0; right: -100%; width: 1100px; max-width: 100vw; height: 100vh; background: #f4f5f7; box-shadow: -5px 0 25px rgba(0, 0, 0, 0.15); z-index: 1050; transition: right 0.3s cubic-bezier(0.82, 0.085, 0.395, 0.895); display: flex; flex-direction: column; }
        
        .side-panel.open { right: 0; }
        .side-panel-header { padding: 16px 24px; background: #ffffff; border-bottom: 1px solid #dee2e6; display: flex; justify-content: space-between; align-items: center; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.02); z-index: 10; flex-shrink: 0; }
        .side-panel-title { font-size: 16px; font-weight: 700; color: #232220; margin: 0; text-transform: uppercase; }
        .btn-close-panel { background: none; border: none; font-size: 20px; color: #6c757d; cursor: pointer; transition: color 0.2s; }
        .btn-close-panel:hover { color: #dc3545; }
        .side-panel-body { padding: 24px; overflow-y: auto; flex-grow: 1; scroll-behavior: smooth; }
        .side-panel-body::-webkit-scrollbar { width: 8px; }
        .side-panel-body::-webkit-scrollbar-track { background: #f8f9fa; }
        .side-panel-body::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 4px; }
        .side-panel-body::-webkit-scrollbar-thumb:hover { background: #94a3b8; }

        /* ============================================================
           PROJECT LIST ================================ */
        .section-card { margin-bottom: 32px; }
        .project-box { border: 1px solid #dee2e6; border-radius: 8px; margin-bottom: 16px; overflow: hidden; background: #ffffff; box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.05); transition: all 0.2s ease; }
        .project-box:hover { border-color: #3a6ea5; box-shadow: 0 0.4rem 1rem rgba(0, 0, 0, 0.08); transform: translateY(-1px); }
        .project-box:last-child { margin-bottom: 0; }
        .project-summary-head { background: #ffffff; padding: 16px 18px; }
        .project-title { color: #232220; font-size: 15px; font-weight: 700; line-height: 1.4; }
        .project-code { color: #6c757d; font-size: 12px; font-weight: 500; }
        .project-meta { font-size: 12px; color: #6c757d; margin-top: 5px; }
        .project-meta strong { color: #343a40; }
        .project-status-tag { display: inline-flex; align-items: center; justify-content: center; padding: 5px 11px; border-radius: 12px; font-size: 11px; font-weight: 700; background: #f3f6f9; color: #5e6278; border: 1px solid #e1e6eb; }
        .project-contribution { font-size: 12px; font-weight: 700; color: #3a6ea5; background: #e9f2fb; padding: 5px 10px; border-radius: 12px; display: inline-flex; align-items: center; justify-content: center; }
        .btn-project-detail { border-radius: 6px; font-size: 12px; font-weight: 700; }

        /* ============================================================
           PROJECT DETAIL SIDE PANEL
           ============================================================ */
        .project-detail-overlay { z-index: 1060; }
        .project-detail-panel { z-index: 1070; background: #f4f5f7; }
        /* Chỉ hạ lớp 2 side panel khi popup lý do trễ của ExtraModal mở; không thay đổi giao diện bình thường. */
        body.nvd-delay-reason-active #sidePanelOverlay { z-index: 1010; }
        body.nvd-delay-reason-active #projectSidePanel { z-index: 1020; }
        body.nvd-delay-reason-active #projectDetailOverlay { z-index: 1025; }
        body.nvd-delay-reason-active #projectDetailPanel { z-index: 1030; }

        .detail-project-header { background: #ffffff; border: 1px solid #e0e4e8; border-radius: 10px; padding: 18px; margin-bottom: 18px; box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.03); }
        .detail-project-title { font-size: 18px; font-weight: 700; color: #232220; }
        .detail-project-code { color: #6c757d; font-size: 12px; }
        .detail-info-item { background: #f8f9fa; border: 1px solid #e5e7eb; border-radius: 7px; padding: 10px 12px; height: 100%; display: flex; flex-direction: column; justify-content: center; }
        .detail-info-label { display: block; font-size: 10px; font-weight: 700; color: #7e8299; text-transform: uppercase; margin-bottom: 4px; }
        .detail-info-value { display: block; font-size: 13px; font-weight: 700; color: #343a40; overflow-wrap: anywhere; }

        /* ============================================================
           PHASE / TASK
           ============================================================ */
        .phase-item { border: 1px solid #e4e6ef; border-radius: 8px; padding: 16px; margin-bottom: 16px; background: #ffffff; box-shadow: 0 0 10px 0 rgba(82, 63, 105, 0.02); }
        .phase-item:last-child { margin-bottom: 0; }
        .phase-header-line { display: flex; justify-content: space-between; align-items: center; gap: 12px; margin-bottom: 16px; background: #f4f6f9; border-left: 4px solid #6f42c1; padding: 12px 16px; border-radius: 6px; }
        .phase-title-name { font-weight: 700; font-size: 13px; color: #232220; min-width: 0; }
        .phase-title-name span { font-weight: 400; }
        .task-sub-table { width: 100%; border-collapse: collapse; font-size: 13px; background: #ffffff; border-radius: 6px; overflow: hidden; border: 1px solid #e4e6ef; }
        .task-sub-table th { background: #f8f9fa; padding: 10px 12px; text-align: left; color: #6c757d; font-weight: 600; border-bottom: 2px solid #e4e6ef; }
        .task-sub-table td { padding: 10px 12px; border-bottom: 1px solid #e4e6ef; vertical-align: middle; }
        .task-sub-table tr:last-child td { border-bottom: none; }
        .task-row:hover { background-color: #f8fafd; }
        .task-row td:first-child { border-left: 3px solid transparent; transition: all 0.2s ease; }
        .task-row[data-status="0"] td:first-child { border-left-color: #cbd5e1; }
        .task-row[data-status="1"] td:first-child { border-left-color: #bae6fd; }
        .task-row[data-status="2"] td:first-child, .task-row[data-status="3"] td:first-child { border-left-color: #bbf7d0; }
        .task-row[data-status="4"] td:first-child { border-left-color: #fcd34d; }
        .ui-badge { padding: 4px 10px; border-radius: 12px; font-size: 11px; font-weight: 700; display: inline-flex; align-items: center; white-space: nowrap; }
        .ui-badge[data-prio="Cao"] { background: #ffe2e5; color: #f64e60; }
        .ui-badge[data-prio="Trung bình"] { background: #fff4de; color: #ffa800; }
        .ui-badge[data-prio="Thấp"] { background: #e1f0ff; color: #3699ff; }
        /* Đồng bộ hoàn toàn trạng thái công việc theo CtrlTask mới nhất; riêng lý do trễ là dòng cảnh báo đỏ bên dưới badge hoàn thành xanh. */
        .nvd-task-status { display:inline-flex; align-items:center; padding:4px 8px; border-radius:4px; font-size:11px; font-weight:600; white-space:nowrap; line-height:1.2; border:1px solid transparent; }
        .nvd-task-status.badge-status-todo { background-color:#f1f5f9 !important; color:#475569 !important; border-color:#cbd5e1 !important; }
        .nvd-task-status.badge-status-doing { background-color:#e0f2fe !important; color:#0369a1 !important; border-color:#bae6fd !important; }
        .nvd-task-status.badge-status-review { background-color:#fef3c7 !important; color:#92400e !important; border-color:#fcd34d !important; }
        .nvd-task-status.badge-status-done { background-color:#dcfce7 !important; color:#15803d !important; border-color:#bbf7d0 !important; }
        .nvd-task-late-note { display:block; margin-top:4px; font-size:11px; line-height:1.35; font-weight:700; color:#dc2626; }
        .nvd-task-status-cell { min-width:116px; }
        .empty-detail { background: #ffffff; border: 1px dashed #d7dce1; border-radius: 8px; padding: 30px 20px; text-align: center; color: #7e8299; }

        /* ============================================================
           STATIC MINI CALENDAR (LỊCH THÁNG TĨNH)
           ============================================================ */
        .mc-weekdays, .mc-grid { display: grid; grid-template-columns: repeat(7, minmax(0, 1fr)); width: 100%; }
        .mc-weekdays span { text-align: center; font-size: 11px; font-weight: 700; color: #64748b; padding: 3px 0 5px; }
        .mc-weekdays span.mc-we { color: #94a3b8; }
        .mc-grid { grid-auto-rows: 50px; gap: 2px; }
        
        .mc-day { display: flex; flex-direction: column; align-items: center; justify-content: center; border-radius: 6px; border: 1px solid transparent; box-sizing: border-box; font-size: 12px; color: #1e293b; cursor: default; min-width: 0; }
        .mc-day .mc-num { font-weight: 600; line-height: 1; }
        .mc-day.out-month .mc-num { color: #94a3b8; opacity: 0.85; } /* Tăng từ 0.4 lên 0.85 để dễ nhìn hơn */
        .mc-day.out-range { opacity: 0.7; } /* Tăng từ 0.32 lên 0.7 */

        .mc-day.st-busy    { background: #fee2e2; color: #b91c1c; }
        .mc-day.st-holiday { background: #fef3c7; color: #b45309; }
        .mc-day.st-weekend { background: #f1f5f9; color: #64748b; }
        .mc-day.st-free    { background: #e6f4ea; color: #137333; }
        .mc-day.today { border-color: #2563eb; box-shadow: inset 0 0 0 1px #2563eb; }

        .mc-label { margin-top: 3px; max-width: 100%; padding: 0 3px; box-sizing: border-box; font-size: 9.5px; font-weight: 600; line-height: 1.1; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
        /* ============================================================
           MOBILE
           ============================================================ */
        @media (max-width: 767.98px) {
            .side-panel { width: 100vw; }
            .side-panel-body { padding: 14px; }
            .phase-header-line { flex-direction: column; align-items: flex-start; }
            .task-sub-table { font-size: 12px; display: block; overflow-x: auto; }
            .project-summary-head { padding: 14px; }
        }
        /* ============================================================
           TOOLTIP GLOBAL (NẰM TRÊN LAYER CAO NHẤT CỦA TRÌNH DUYỆT)
           ============================================================ */
        .mc-day { position: relative !important; overflow: visible !important; }
        .mc-day.has-tooltip { cursor: pointer; }
        .mc-day:hover, .mc-day.show-tooltip { z-index: 50; }
        
        .global-task-tooltip { 
            position: absolute; z-index: 9999999 !important; 
            background: #ffffff; color: #334155; 
            border: 1px solid #cbd5e1; border-radius: 10px; padding: 11px 16px; 
            min-width: 340px; max-width: 520px; width: max-content; 
            white-space: normal; word-break: break-word; overflow-wrap: anywhere; 
            box-shadow: 0 10px 40px rgba(15, 23, 42, 0.2); font-size: 12.5px; line-height: 1.5; 
            opacity: 0; visibility: hidden; pointer-events: none; 
            transition: opacity 0.15s ease, visibility 0.15s ease; 
        }
        .global-task-tooltip.show { opacity: 1; visibility: visible; pointer-events: auto; }
        
        .global-task-tooltip::after { 
            content: ''; position: absolute; top: 100%; 
            left: var(--arrow-left, 50%); transform: translateX(-50%);
            border-width: 6px; border-style: solid; border-color: #ffffff transparent transparent transparent; 
        }

        .tooltip-task-list { list-style: none; margin: 0; padding: 0; text-align: left; }
        .tooltip-task-list li { margin: 0; padding: 8px 0; border-bottom: 1px dashed #e2e8f0; color: #334155; }
        .tooltip-task-list li:last-child { border-bottom: none; padding-bottom: 0; }
        .tooltip-task-list li:first-child { padding-top: 0; }
        .t-project-name { font-size: 10.5px; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 3px; font-weight: 600; color: #64748b; }
        .t-code { display: inline-block; color: #2563eb; font-weight: 700; margin-right: 6px; }

        /* ============================================================
           NHAN VIEN DETAIL — HEADER PHẲNG + CARD CÂN CHIỀU CAO
           Phạm vi giới hạn vào các class nvd- của trang chi tiết.
           ============================================================ */
        .nvd-detail-layout { align-items: stretch !important; height: auto; overflow: visible; }
        .nvd-left-column { min-width: 0; min-height: 0; height: auto; overflow: visible; }

        /* Hồ sơ phía trên chỉ cao theo nội dung, không kéo giãn. */
        .nvd-left-column > .nvd-profile-card {
            flex: 0 0 auto !important;
            height: auto !important;
            min-height: 0 !important;
            overflow: visible;
        }
        .nvd-profile-card > .nvd-profile-body {
            flex: 0 0 auto !important;
            padding: 13px 18px !important;
            min-height: 0 !important;
        }
        .nvd-profile-card .nvd-profile-avatar { width: 78px !important; height: 78px !important; }
        .nvd-profile-name { font-size: 19px; line-height: 1.3; }
        .nvd-profile-details { font-size: 12.5px !important; line-height: 1.35; }
        .nvd-profile-details > div { min-width: 0; overflow-wrap: anywhere; }

        /* Card dưới tự giãn tới đáy cột bên phải, nhưng KHÔNG thu nhỏ/cắt nội dung. */
        .nvd-left-column > .nvd-info-card {
            display: flex;
            flex-direction: column;
            flex: 1 0 auto !important;
            height: auto !important;
            min-height: 0 !important;
            overflow: visible !important;
            min-width: 0;
            background: #fff !important;
            border-radius: 10px;
        }
        /* Tiêu đề cùng nền card, chỉ có đường ngăn mảnh — bỏ hẳn thanh card-header trắng. */
        .nvd-info-card > .nvd-info-header {
            flex: 0 0 auto;
            min-height: 49px;
            margin: 0 18px;
            padding: 14px 0 12px !important;
            gap: 10px;
            background: transparent !important;
            border: 0 !important;
            border-bottom: 1px solid #e9eaf1 !important;
            border-radius: 0 !important;
        }
        .nvd-info-header h6 {
            margin: 0;
            min-width: 0;
            font-size: 13.5px;
            line-height: 1.4;
            color: #344054 !important;
        }
        .nvd-info-card > .nvd-info-body {
            flex: 1 0 auto !important;
            display: flex;
            flex-direction: column;
            gap: 12px;
            padding: 13px 18px 16px !important;
            height: auto !important;
            min-height: 0 !important;
            overflow: visible !important;
            background: transparent !important;
        }
        .nvd-info-grid { display: grid; grid-template-columns: repeat(2,minmax(0,1fr)); gap: 11px; }
        .nvd-info-tile {
            display: flex;
            align-items: center;
            gap: 11px;
            padding: 12px 13px;
            min-height: 61px;
            min-width: 0;
            border: 1px solid #e7eaf1;
            border-radius: 8px;
            background: #f8f9fc;
            transition: border-color .16s ease,background-color .16s ease;
        }
        .nvd-info-tile:hover { border-color: #d5caec; background: #fcfaff; }
        .nvd-info-tile--accent { background: #f8f5fd; border-color: #e8ddf7; }
        .nvd-info-icon {
            display: inline-flex;
            justify-content: center;
            align-items: center;
            flex: 0 0 32px;
            width: 32px;
            height: 32px;
            background: #eae2f7;
            color: #6b3aa4;
            border-radius: 8px;
            font-size: 12px;
        }
        .nvd-info-icon--neutral { background: #e9edf4; color: #627187; }
        .nvd-info-content { min-width: 0; display: flex; flex-direction: column; gap: 3px; justify-content: center; }
        .nvd-info-label { display: block; color: #768195; text-transform: uppercase; font-size: 10px; letter-spacing: .35px; font-weight: 700; line-height: 1.3; }
        .nvd-info-value { display: block; color: #283142; font-size: 13px; line-height: 1.4; font-weight: 700; overflow-wrap: anywhere; }
        .nvd-info-value--accent { color: #633298; }

        /* Địa chỉ có cấu trúc theo từng trường, tự giãn phần hữu ích khi còn diện tích. */
        .nvd-address-panel {
            flex: 1 0 auto;
            display: flex;
            flex-direction: column;
            gap: 11px;
            min-width: 0;
            min-height: 0;
            border: 1px solid #e4e7ef;
            border-radius: 9px;
            padding: 14px 15px 15px;
            background: #fff;
        }
        .nvd-address-title { display: flex; align-items: center; gap: 8px; color: #344054; font-size: 13px; font-weight: 700; margin: 0; }
        .nvd-address-title i { color: #6b3aa4; }
        .nvd-address-grid { display: grid; grid-template-columns: repeat(2,minmax(0,1fr)); gap: 10px; }
        .nvd-address-location {
            min-width: 0;
            min-height: 60px;
            background: #f8f9fc;
            border-radius: 7px;
            border: 1px solid #edf0f5;
            padding: 10px 12px;
        }
        .nvd-address-value { margin-top: 4px; display: block; color: #344054; font-size: 12.5px; font-weight: 700; line-height: 1.4; overflow-wrap: anywhere; }
        .nvd-address-details {
            flex: 1 0 auto;
            display: flex;
            flex-direction: column;
            justify-content: center;
            min-width: 0;
            min-height: 63px;
            margin: 0;
            padding: 10px 12px;
            background: #f8f9fc;
            border: 1px solid #edf0f5;
            border-radius: 7px;
        }
        .nvd-address-details .nvd-address-value { font-weight: 650; }
        .nvd-address-summary {
            display: flex;
            align-items: flex-start;
            gap: 9px;
            margin-top: 0;
            padding: 10px 12px;
            background: #faf8ff;
            border: 1px solid #eee7f8;
            border-radius: 7px;
            color: #605777;
            font-size: 12px;
            line-height: 1.45;
            overflow-wrap: anywhere;
        }
        .nvd-address-summary i { color: #8761b7; margin-top: 2px; flex: 0 0 auto; }
        .nvd-info-empty { color: #98a2b3; font-weight: 500; }

        @media (max-width: 991.98px) {
            .nvd-left-column > .nvd-info-card { flex-grow: 0 !important; }
            .nvd-address-panel, .nvd-address-details { flex-grow: 0; }
        }
        @media (max-width: 767.98px) {
            .nvd-profile-card > .nvd-profile-body { padding: 15px !important; }
            .nvd-profile-details { font-size: 12px !important; }
        }
        @media (max-width: 575.98px) {
            .nvd-profile-card .nvd-profile-avatar { width: 70px !important; height: 70px !important; }
            .nvd-profile-name { font-size: 17px; }
            .nvd-info-card > .nvd-info-header { margin: 0 13px; padding: 12px 0 11px !important; }
            .nvd-info-card > .nvd-info-body { padding: 12px 13px 14px !important; gap: 9px; }
            .nvd-info-grid { gap: 8px; }
            .nvd-info-tile { min-height: 57px; padding: 9px; gap: 8px; }
            .nvd-info-icon { flex-basis: 27px; width: 27px; height: 27px; }
            .nvd-info-label { font-size: 9.5px; }
            .nvd-info-value { font-size: 12px; }
            .nvd-address-panel { padding: 11px; gap: 9px; }
            .nvd-address-location { padding: 9px; min-height: 55px; }
            .nvd-address-details { padding: 10px; min-height: 57px; }
            .nvd-address-value { font-size: 12px; }
        }
        @media (max-width: 380px) {
            .nvd-info-grid, .nvd-address-grid { grid-template-columns: 1fr; }
        }

        /* NhanVienDetail KBG 2.0 - selector/declarations kept on one line. */
        .nvd-project-card { border-color:#e5ddf0; border-radius:12px; box-shadow:0 3px 12px rgba(65,36,104,.04); }
        .nvd-project-card:hover { border-color:#a985d7; box-shadow:0 7px 18px rgba(95,52,153,.10); }
        .nvd-project-card .project-summary-head { padding:16px 19px; }
        .nvd-project-card .project-title { font-size:15px; color:#312544; margin:2px 0 6px; }
        .nvd-status-stack { display:flex; gap:6px; justify-content:flex-end; align-items:flex-end; flex-direction:column; }
        .nvd-time-state { display:inline-flex; align-items:center; padding:4px 9px; border-radius:7px; font-size:11px; font-weight:700; background:#f3eefb; color:#7040a9; border:1px solid #e6d7fa; white-space:nowrap; }
        .nvd-time-final { background:#e8f8ee; color:#168249; border-color:#c8ebd8; }
        .nvd-time-pending { background:#fff6e6; color:#ae6a10; border-color:#f5dfbd; }
        .nvd-time-running { background:#f2eafa; color:#7543b2; border-color:#e6d7fa; }
        .nvd-card-metrics { display:grid; grid-template-columns:repeat(3,minmax(0,1fr)); gap:9px; margin-top:13px; }
        .nvd-metric { background:#faf8fd; border:1px solid #ece5f5; border-radius:9px; padding:9px 12px; min-width:0; }
        .nvd-metric span { display:block; color:#81748e; font-size:11px; font-weight:600; }
        .nvd-metric strong { display:block; color:#492378; font-size:18px; font-weight:750; margin:2px 0; white-space:nowrap; }
        .nvd-metric small { display:block; font-size:10.5px; color:#7d788a; }
        .nvd-card-footer { display:flex; align-items:center; gap:12px; justify-content:space-between; border-top:1px solid #eee8f3; padding-top:11px; margin-top:12px; }
        .nvd-card-note { color:#81758b; font-size:11.5px; line-height:1.5; flex:1; }
        .nvd-share { display:inline-block; margin-left:8px; font-weight:600; color:#5b2f92; }
        .nvd-detail-heading { padding:16px; }
        .nvd-detail-metrics { display:grid; grid-template-columns:repeat(3,minmax(0,1fr)); gap:10px; margin:12px 0; }
        .nvd-detail-metric { background:#f9f6fd; border:1px solid #e8ddf3; border-radius:9px; padding:12px; }
        .nvd-detail-metric .nvd-metric-name { font-size:11px; font-weight:700; color:#7b6c89; }
        .nvd-detail-metric strong { display:block; font-size:20px; font-weight:750; color:#4f2783; margin:4px 0; }
        .nvd-detail-metric small { font-size:11px; color:#756d7f; }
        .nvd-detail-status { display:flex; align-items:center; flex-wrap:wrap; gap:8px; font-size:11.5px; color:#776b80; line-height:1.5; }
        .nvd-detail-status .nvd-share { margin-left:0; }
        .nvd-phase { padding:0; overflow:hidden; border-radius:10px; }
        .nvd-phase-summary { cursor:pointer; user-select:none; margin:0; border-radius:0; min-height:54px; }
        .nvd-phase-summary:hover { background:#f0eafa; }
        .nvd-phase-summary::marker { color:#6f42c1; }
        .nvd-phase-hours { display:flex; flex-wrap:wrap; gap:9px; font-size:11px; color:#746484; }
        .nvd-phase-hours strong { color:#502986; }
        .nvd-task-scroll { overflow-x:auto; }
        .nvd-task-table { min-width:820px; }
        .nvd-task-table th:first-child { width:36%; }
        .nvd-task-table th:nth-child(2) { width:16%; }
        .nvd-task-table td { padding:12px 10px; }
        .nvd-task-name a { text-decoration:none; font-size:12px; }
        .nvd-task-name a:hover { text-decoration:underline; }
        .nvd-task-mini { font-size:10px; color:#7f688e; margin-top:5px; }
        .nvd-task-more { margin-top:5px; font-size:11px; color:#6b6574; }
        .nvd-task-more summary { cursor:pointer; color:#7540a7; font-weight:600; }
        .nvd-task-more div { padding:3px 0; line-height:1.45; }
        .nvd-hours strong { display:block; font-size:13px; color:#4c287e; white-space:nowrap; }
        .nvd-hours small { display:block; font-size:10.5px; color:#84798c; line-height:1.5; margin-top:3px; }
        @media(max-width:720px) { .nvd-card-metrics { grid-template-columns:1fr 1fr; } .nvd-detail-metrics { grid-template-columns:1fr; } .nvd-card-footer { flex-direction:column; align-items:flex-start; } .nvd-status-stack { align-items:flex-start; } }


        /* UX V2: Chỉ thay giao diện dự án; không tác động hồ sơ cá nhân và popup ExtraModal. */
        .nvd-project-card { overflow:visible; }
        .nvd-project-card .project-summary-head { border-radius:12px; }
        .nvd-project-card:hover { border-color:#9b70cf; }
        .project-status-tag.nvd-project-in-progress { color:#5e329c; background:#f1e9fc; border-color:#ddc9f5; }
        .project-status-tag.nvd-project-complete { color:#188650; background:#eaf9f0; border-color:#cbecd9; }
        .project-status-tag.nvd-project-paused { color:#98621c; background:#fff6e8; border-color:#f2debc; }
        .project-status-tag.nvd-project-not-started { color:#737e90; background:#f1f4f8; border-color:#dfe5ed; }
        .project-status-tag.nvd-project-ended { color:#596171; background:#eaedf4; border-color:#d5dbe6; }
        .nvd-former-member { display:inline-flex; align-items:center; font-size:10.5px; line-height:1.4; color:#8155a7; margin-top:5px; padding:3px 8px; border:1px solid #e5d9f0; background:#faf6ff; border-radius:6px; }
        .nvd-card-metrics { grid-template-columns:repeat(2,minmax(0,1fr)); gap:10px; margin-top:14px; }
        .nvd-metric-featured { padding:13px 14px; border-radius:11px; background:#f9f6fe; }
        .nvd-metric-featured span { font-size:11.5px; color:#6d5983; font-weight:700; line-height:1.3; }
        .nvd-metric-featured strong { font-size:23px; line-height:1.25; margin:5px 0 4px; color:#47227b; }
        .nvd-metric-featured small { color:#756982; font-size:11px; line-height:1.4; }
        .nvd-metric-final { border-color:#d9eadf; background:#f5fbf7; }
        .nvd-metric-final span, .nvd-metric-final strong { color:#24754b; }
        .nvd-card-report { display:flex; align-items:center; flex-wrap:wrap; column-gap:12px; row-gap:5px; padding:10px 12px; margin-top:9px; background:#fcfbfd; border:1px solid #e9e6ef; border-radius:8px; font-size:11.5px; color:#71677e; }
        .nvd-card-report strong { color:#4f3273; font-size:14px; }
        .nvd-card-report small { margin-left:auto; color:#81788c; font-size:11px; }
        .nvd-card-footer { align-items:flex-start; }
        .nvd-card-note { max-width:calc(100% - 116px); color:#766b83; line-height:1.55; font-size:11.5px; }
        .nvd-help-details { margin-top:5px; font-size:11px; }
        .nvd-help-details summary { cursor:pointer; display:inline-flex; color:#6d419b; font-weight:650; padding:3px 2px; }
        .nvd-help-details div { margin-top:4px; padding:9px 11px; border:1px solid #e5dbf0; border-radius:7px; background:#faf7fe; color:#635773; }
        .nvd-detail-topbar { min-height:62px; }
        .nvd-detail-top-left { display:flex; flex-wrap:wrap; align-items:center; gap:12px; min-width:0; }
        .nvd-back-btn { border:1px solid #d9c9eb; border-radius:8px; background:#faf6ff; color:#63348e; padding:7px 12px; font-size:12px; font-weight:700; cursor:pointer; }
        .nvd-back-btn:hover, .nvd-back-btn:focus-visible { background:#eadcf8; border-color:#b790d9; outline:0; }
        .nvd-topbar-divider { height:24px; width:1px; background:#e2dbe9; }
        .nvd-audience-hint { display:flex; align-items:center; flex-wrap:wrap; gap:9px; color:#655581; font-size:11.5px; padding:10px 13px; background:#f4edfc; border:1px solid #e4d6f5; border-radius:8px; margin-bottom:11px; }
        .nvd-audience-hint strong { color:#582b89; font-size:12px; }
        .nvd-audience-hint span { color:#776885; }
        .nvd-detail-metrics { grid-template-columns:repeat(2,minmax(0,1fr)); gap:10px; }
        .nvd-detail-metric { padding:13px 14px; min-width:0; }
        .nvd-detail-metric .nvd-metric-name { font-size:12px; }
        .nvd-detail-metric strong { font-size:24px; line-height:1.25; margin:5px 0; }
        .nvd-detail-metric small { display:block; line-height:1.5; }
        .nvd-detail-metric--final { background:#f3fbf5; border-color:#cae9d3; }
        .nvd-detail-metric--final .nvd-metric-name, .nvd-detail-metric--final strong { color:#26794d; }
        .nvd-detail-self-report { display:flex; align-items:center; gap:11px; flex-wrap:wrap; padding:10px 13px; border:1px solid #e7e2ee; background:#fbfafc; border-radius:8px; margin-bottom:11px; font-size:12px; color:#695f77; }
        .nvd-detail-self-report strong { font-size:16px; color:#4f2d79; }
        .nvd-detail-self-report small { margin-left:auto; color:#786f83; }
        .nvd-work-progress { display:flex; flex-direction:column; gap:7px; margin:10px 0; padding:12px 14px; border:1px solid #e4dcf0; background:#fbf9fe; border-radius:9px; }
        .nvd-work-progress-head { display:flex; align-items:center; justify-content:space-between; gap:10px; font-size:12px; }
        .nvd-work-progress-head strong { color:#433454; }
        .nvd-work-progress-head span { color:#633594; font-weight:700; }
        .nvd-work-progress-track { background:#e7e0ef; height:7px; border-radius:20px; overflow:hidden; }
        .nvd-work-progress-fill { display:block; height:100%; background:#7b4ac0; border-radius:20px; }
        .nvd-work-progress-note { font-size:11.5px; color:#70667a; line-height:1.45; }
        .nvd-detail-status { gap:10px; }
        .nvd-tasks-toolbar { display:flex; align-items:center; justify-content:space-between; gap:12px; flex-wrap:wrap; padding:10px 12px; margin-bottom:10px; border:1px solid #e5e0eb; background:#fff; border-radius:9px; }
        .nvd-tasks-toolbar strong { color:#3c3248; font-size:13px; }
        .nvd-tasks-toolbar small { display:block; font-size:11px; color:#8b8393; margin-top:2px; }
        .nvd-pending-filter { cursor:pointer; border:1px solid #d7c2f1; border-radius:8px; padding:7px 10px; background:#fbf8ff; color:#69339d; font-size:11px; font-weight:700; white-space:nowrap; }
        .nvd-pending-filter:hover, .nvd-pending-filter[aria-pressed="true"] { background:#f0e5fc; border-color:#a980d9; }
        .nvd-filter-empty { background:#fff; border:1px dashed #ded4e9; border-radius:9px; padding:20px; font-size:12px; color:#665575; text-align:center; margin-bottom:10px; }
        .nvd-filter-empty[hidden] { display:none !important; }
        .nvd-phase-summary { gap:14px; padding:12px 14px; }
        .nvd-phase-summary:focus-visible { outline:2px solid #956fc9; outline-offset:-2px; }
        .nvd-task-table { min-width:850px; }
        .nvd-task-table th { color:#645272; font-size:11.5px; background:#f5f0fb; border-bottom:1px solid #e4d7f1; }
        .nvd-task-table td { vertical-align:top; }
        .nvd-task-name a { color:#542a8d !important; font-size:12.5px; line-height:1.45; }
        .nvd-task-progress { display:flex; align-items:flex-start; gap:6px; margin-top:6px; font-size:11px; line-height:1.45; color:#97651c; }
        .nvd-task-progress i { flex-shrink:0; margin-top:2px; }
        .nvd-task-progress-final { color:#29824f; }
        .nvd-task-actions { display:flex; align-items:center; flex-wrap:wrap; gap:6px 12px; margin-top:6px; }
        .nvd-reason-btn { background:#fff5f4; border:1px solid #f1cecc; color:#b33d3a; padding:5px 8px; font-size:11px; font-weight:650; border-radius:7px; cursor:pointer; }
        .nvd-reason-btn:hover, .nvd-reason-btn:focus-visible { background:#fee8e6; border-color:#e9a9a6; outline:0; }
        .nvd-task-more { margin-top:0; }
        .nvd-task-more summary { display:list-item; line-height:1.7; }
        .nvd-hours-guide { margin:-1px 0 12px; border:1px solid #e3d9ee; background:#fff; border-radius:9px; overflow:hidden; }
        .nvd-hours-guide summary { display:flex; align-items:center; gap:6px; padding:9px 13px; color:#5c3286; cursor:pointer; font-size:12px; font-weight:700; list-style:none; }
        .nvd-hours-guide summary::-webkit-details-marker { display:none; }
        .nvd-hours-guide summary:hover, .nvd-hours-guide[open] summary { background:#f8f3fd; }
        .nvd-guide-chevron { margin-left:auto; transition:transform .18s ease; font-size:10px; }
        .nvd-hours-guide[open] .nvd-guide-chevron { transform:rotate(180deg); }
        .nvd-hours-guide-content { display:grid; grid-template-columns:repeat(3,minmax(0,1fr)); gap:10px; padding:12px; border-top:1px solid #e9e0f2; }
        .nvd-hours-guide-content > div { background:#faf8fd; border:1px solid #eee6f4; padding:10px 11px; border-radius:7px; color:#5c5267; font-size:12px; line-height:1.55; }
        .nvd-hours-guide-content strong { color:#50366c; }
        .nvd-task-table th { font-size:12px; font-weight:700; }
        .nvd-hours small { font-size:11.5px; line-height:1.55; color:#6c6274; }
        .nvd-task-progress { font-size:11.5px; }
        .nvd-hours-final strong { color:#23784b; }
        @media(max-width:850px) { .nvd-hours-guide-content { grid-template-columns:1fr; } }
        @media(max-width:720px) { .nvd-card-metrics { grid-template-columns:1fr; } .nvd-card-note { max-width:none; } .nvd-detail-metrics { grid-template-columns:1fr; } .nvd-card-footer { flex-direction:column; } .nvd-card-report small { margin-left:0; } .nvd-topbar-divider { display:none; } .nvd-work-progress-head { flex-wrap:wrap; } }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpMain" runat="server">
    <asp:UpdatePanel ID="upnlMainDetail" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="container-fluid px-0">
                <SweetSoft:Navigation ID="Navigation1" runat="server" />

                <!-- ====================================================
                     MAIN LAYOUT
                     ==================================================== -->
                <div class="row align-items-stretch nvd-detail-layout">

                    <!-- =================================================
                         LEFT COLUMN
                         ================================================= -->
                    <div class="col-xl-8 col-lg-7 d-flex flex-column gap-3 nvd-left-column">

                        <!-- ================================
                             EMPLOYEE SUMMARY
                             ================================ -->
                        <div class="card shadow-sm border-0 nvd-profile-card">
                            <div class="card-body d-flex flex-column flex-md-row align-items-center gap-3 nvd-profile-body">
                                <div class="flex-shrink-0">
                                    <img id="imgAvatar" runat="server" src="/Styles/images/user-icon.png" class="rounded-circle border border-3 border-primary shadow-sm nvd-profile-avatar" style="width:78px;height:78px;object-fit:cover;" onerror="this.src='/Styles/images/user-icon.png'" />
                                </div>
                                <div class="flex-grow-1 w-100 text-center text-md-start">
                                    <div class="d-flex flex-column flex-md-row align-items-center gap-2 mb-1">
                                        <h4 class="mb-0 fw-bold text-dark nvd-profile-name">
                                            <asp:Literal ID="ltrTenNhanVien" runat="server" />
                                        </h4>
                                    </div>
                                    <div class="row g-1 text-muted nvd-profile-details" style="font-size:13px;">
                                        <div class="col-sm-6">
                                            <%= GetResourceText(BackEndResourceKeys.CHUC_DANH) %>: <strong class="text-dark"><asp:Literal ID="ltrChucDanh" runat="server" /></strong>
                                        </div>
                                        <div class="col-sm-6">
                                            <%= GetResourceText(BackEndResourceKeys.PHONG_BAN) %>: <strong class="text-dark"><asp:Literal ID="ltrPhongBan" runat="server" /></strong>
                                        </div>
                                        <div class="col-sm-6">
                                            Email: <strong class="text-dark"><asp:Literal ID="ltrEmail" runat="server" /></strong>
                                        </div>
                                        <div class="col-sm-6">
                                            <%= GetResourceText(BackEndResourceKeys.PHONE_NUMBER) %>: <strong class="text-dark"><asp:Literal ID="ltrPhone" runat="server" /></strong>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- ================================
                             PERSONAL / WORK INFO — ĐỒNG BỘ POPUP THÊM/SỬA
                             ================================ -->
                        <div class="card shadow-sm border-0 flex-grow-1 nvd-info-card">
                            <div class="d-flex justify-content-between align-items-center nvd-info-header">
                                <h6 class="mb-0 fw-bold text-dark">
                                    <i class="fas fa-id-card text-primary me-2"></i><%= GetResourceText(BackEndResourceKeys.PERSONAL_AND_WORK_INFORMATION) %>
                                </h6>
                                <SweetSoft:ExtraButton runat="server" ID="btnEditProfile" CssClass="btn-sm btn-light border text-primary fw-bold" ButtonIcon="Edit" OnClick="btnEditProfile_Click"></SweetSoft:ExtraButton>
                            </div>
                            <div class="card-body nvd-info-body">
                                <!-- Công tác: hai chỉ số cân đối, nổi bật ở đầu khối -->
                                <div class="nvd-info-grid">
                                    <div class="nvd-info-tile nvd-info-tile--accent">
                                        <span class="nvd-info-icon"><i class="far fa-calendar-check"></i></span>
                                        <div class="nvd-info-content">
                                            <span class="nvd-info-label"><%= GetResourceText(BackEndResourceKeys.JOIN_DATE) %></span>
                                            <span class="nvd-info-value"><asp:Literal ID="ltrNgayGiaNhap" runat="server" /></span>
                                        </div>
                                    </div>
                                    <div class="nvd-info-tile nvd-info-tile--accent">
                                        <span class="nvd-info-icon"><i class="far fa-clock"></i></span>
                                        <div class="nvd-info-content">
                                            <span class="nvd-info-label"><%= GetResourceText(BackEndResourceKeys.SENIORITY) %></span>
                                            <span class="nvd-info-value nvd-info-value--accent"><asp:Literal ID="ltrThamNien" runat="server" /></span>
                                        </div>
                                    </div>
                                </div>

                                <!-- Cá nhân: không hiển thị CCCD / Trạng thái công tác -->
                                <div class="nvd-info-grid">
                                    <div class="nvd-info-tile">
                                        <span class="nvd-info-icon nvd-info-icon--neutral"><i class="fas fa-birthday-cake"></i></span>
                                        <div class="nvd-info-content">
                                            <span class="nvd-info-label"><%= GetResourceText(BackEndResourceKeys.DATE_OF_BIRTH) %></span>
                                            <span class="nvd-info-value"><asp:Literal ID="ltrNgaySinh" runat="server" /></span>
                                        </div>
                                    </div>
                                    <div class="nvd-info-tile">
                                        <span class="nvd-info-icon nvd-info-icon--neutral"><i class="far fa-user"></i></span>
                                        <div class="nvd-info-content">
                                            <span class="nvd-info-label"><%= GetResourceText(BackEndResourceKeys.GIOI_TINH) %></span>
                                            <span class="nvd-info-value"><asp:Literal ID="ltrGioiTinh" runat="server" /></span>
                                        </div>
                                    </div>
                                </div>

                                <!-- Địa chỉ: hiển thị riêng cấp tỉnh / cấp xã và địa chỉ chi tiết -->
                                <div class="nvd-address-panel">
                                    <div class="nvd-address-title"><i class="fas fa-map-marker-alt"></i> Địa chỉ liên hệ</div>
                                    <div class="nvd-address-grid">
                                        <div class="nvd-address-location">
                                            <span class="nvd-info-label">Tỉnh / Thành phố</span>
                                            <span class="nvd-address-value"><asp:Literal ID="ltrTinhThanh" runat="server" /></span>
                                        </div>
                                        <div class="nvd-address-location">
                                            <span class="nvd-info-label">Xã / Phường / Đặc khu</span>
                                            <span class="nvd-address-value"><asp:Literal ID="ltrXaPhuong" runat="server" /></span>
                                        </div>
                                    </div>
                                    <div class="nvd-address-details">
                                        <span class="nvd-info-label">Địa chỉ chi tiết</span>
                                        <span class="nvd-address-value"><asp:Literal ID="ltrDiaChi" runat="server" /></span>
                                    </div>
                                    <div class="nvd-address-summary" id="boxDiaChiDayDu" runat="server">
                                        <i class="fas fa-map"></i>
                                        <span><asp:Literal ID="ltrDiaChiDayDu" runat="server" /></span>
                                    </div>
                                </div>
                            </div>
                        </div>

                    </div>

<!-- =================================================
                         RIGHT COLUMN (CHIA 2 KHỐI RÕ RÀNG)
                         ================================================= -->
                    <div class="col-xl-4 col-lg-5 d-flex flex-column mt-3 mt-lg-0 gap-3">
                        
                        <!-- Box 1: Tổng quan dự án (Clickable) -->
                        <div class="card shadow-sm border bg-white mb-0" 
                             style="cursor:pointer; border-color:#e0e4e8 !important;" 
                             onclick="openSidePanel();"
                             onmouseover="this.style.borderColor='#3a6ea5';"
                             onmouseout="this.style.borderColor='#e0e4e8';">
                            <div class="card-body p-4 d-flex justify-content-between align-items-center">
                                <div class="d-flex align-items-center gap-3">
                                    <div class="icon-box shadow-sm text-primary" style="background-color:#e9f2fb;">
                                        <i class="fas fa-layer-group"></i>
                                    </div>
                                    <div>
                                        <h6 class="fw-bold text-dark mb-1"><%= GetResourceText(BackEndResourceKeys.PROJECT_ALLOCATION_DETAILS) %></h6>
                                        <span class="text-muted" style="font-size:12px;">
                                            <asp:Literal ID="ltrCountAllProj" runat="server">0</asp:Literal> <%= GetResourceText(BackEndResourceKeys.PROJECT).ToLower() %>
                                        </span>
                                    </div>
                                </div>
                                <i class="fas fa-chevron-right text-muted"></i>
                            </div>
                        </div>

                        <!-- Box 2: Lịch tĩnh tháng hiện tại -->
                        <div class="card shadow-sm border flex-grow-1 bg-white" style="border-color:#e0e4e8 !important;">
                            <!-- Header: Đã dời nút Mở Lịch Biểu lên ngang hàng với Tiêu đề -->
                            <div class="card-header bg-white border-0 pt-4 pb-0 px-4 d-flex justify-content-between align-items-center">
                                <div class="d-flex align-items-center gap-3">
                                    <div class="icon-box shadow-sm" style="color:#681da8; background-color:#f3e8fd;">
                                        <i class="fas fa-calendar-alt"></i>
                                    </div>
                                    <div>
                                        <!-- GỌI ĐÚNG BACKEND RESOURCE KEY CHUẨN -->
                                        <h6 class="fw-bold text-dark mb-1"><%= GetResourceText(BackEndResourceKeys.WORK_SCHEDULE_THIS_MONTH) %></h6>
                                        <span class="text-muted" style="font-size:12px;">
                                            <%= GetResourceText(BackEndResourceKeys.PERSONAL_SCHEDULE) %>
                                        </span>
                                    </div>
                                </div>
                                
                                <!-- Nút liên kết ra trang Lịch biểu đầy đủ -->
                                <a id="lnkSchedule" runat="server" class="text-primary fw-bold text-decoration-none" style="font-size:13px; transition:all .2s;"
                                   onmouseover="this.style.color='#681da8';" onmouseout="this.style.color='#3a6ea5';">
                                    <%= GetResourceText(BackEndResourceKeys.OPEN_SCHEDULE) %> <i class="fas fa-arrow-right ms-1"></i>
                                </a>
                            </div>
                            
                            <!-- Body chứa Grid Lịch -->
                            <div class="card-body p-4 pt-3 pb-4">
                                <asp:HiddenField ID="hdfScheduleJson" runat="server" ClientIDMode="Static" />
                                <div class="mini-cal-wrap">
                                    <div class="mini-cal" id="employeeMonthCalendar"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- =====================================================
                     PROJECT LIST SIDE PANEL
                     ===================================================== -->
                <div id="sidePanelOverlay" class="side-panel-overlay" onclick="closeSidePanel();"></div>

                <div id="projectSidePanel" class="side-panel">
                    <div class="side-panel-header">
                        <h5 class="side-panel-title">
                            <i class="fas fa-layer-group text-primary me-2"></i><%= GetResourceText(BackEndResourceKeys.PROJECT_ALLOCATION_DETAILS) %>
                        </h5>
                        <button type="button" class="btn-close-panel" onclick="closeSidePanel();">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>

                    <div class="side-panel-body">
                        <!-- ================================================
                             SEARCH / FILTER
                             ================================================ -->
                        <asp:UpdatePanel runat="server" ID="upnlSearchDuAn" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div class="d-flex flex-column flex-md-row gap-2 mb-2">
                                    <SweetSoft:BootstrapDropdown ID="ddlSearchTrangThaiDuAn" runat="server" Text="Trạng thái dự án" AllowClear="true" AutoPostBack="true" SearchColumn="TrangThai" EnableSearch="false" CausesValidation="false" OnSelectedValueChanged="ddlSearchTrangThaiDuAn_SelectedValueChanged" CssClass="border-radius-1" Style="min-width:180px;"></SweetSoft:BootstrapDropdown>
                                    <asp:Panel runat="server" DefaultButton="btnSearchDuAn" CssClass="input-group flex-grow-1">
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtSearchDuAn" CssClass="border-primary input-search-filter" PlaceHolder="Nhập tên dự án..." CausesValidation="false"></SweetSoft:ExtraTextBox>
                                        <SweetSoft:ExtraButton runat="server" ID="btnSearchDuAn" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearchDuAn_Click" CausesValidation="false" IsSubmit="false"></SweetSoft:ExtraButton>
                                    </asp:Panel>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>

                        <!-- ================================================
                             SEARCH TAGS
                             ================================================ -->
                        <div class="listSearchTagBox">
                            <asp:UpdatePanel ID="upSearchTagDuAn" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <SweetSoft:ExtraSearchBox ID="searchTagBoxDuAn" runat="server" OnTagClosed="searchTagBoxDuAn_TagClosed"></SweetSoft:ExtraSearchBox>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>

                        <!-- ================================================
                             PROJECT LIST
                             ================================================ -->
                        <asp:UpdatePanel ID="upListDuAn" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div id="section_all_projects" class="section-card" runat="server" ClientIDMode="Static">
                                    <div id="emptyAll" runat="server" visible="false" class="text-center text-muted p-4 border rounded bg-white mb-3 shadow-sm">
                                        <i class="far fa-folder-open mb-2" style="font-size:24px;color:#ced4da;"></i><br />
                                        <%= GetResourceText(BackEndResourceKeys.NO_DATA) %>
                                    </div>

                                    <asp:Repeater ID="rptAllProjects" runat="server" OnItemCommand="rptAllProjects_ItemCommand">
                                        <ItemTemplate>
                                            <div class="project-box nvd-project-card">
                                                <div class="project-summary-head">
                                                    <div class="d-flex justify-content-between align-items-start gap-2 flex-wrap">
                                                        <div class="flex-grow-1 min-w-0">
                                                            <div class="project-code"><%# System.Web.HttpUtility.HtmlEncode(Convert.ToString(Eval("MaDuAn"))) %></div>
                                                            <div class="project-title"><%# System.Web.HttpUtility.HtmlEncode(Convert.ToString(Eval("TenDuAn"))) %></div>
                                                            <div class="project-meta"><i class="fas fa-user-tag me-1"></i><%# System.Web.HttpUtility.HtmlEncode(Convert.ToString(Eval("VaiTro"))) %>
                                                                <span class="mx-1">·</span><i class="far fa-calendar me-1"></i><%# FormatProjectPeriod(Eval("ProjectStartDate"), Eval("ProjectEndDate"), Eval("ProjectActualEndDate")) %>
                                                            </div>
                                                            <asp:PlaceHolder runat="server" Visible='<%# !Convert.ToBoolean(Eval("IsCurrentMember")) %>'><span class="nvd-former-member"><i class="fas fa-history me-1"></i>Đã từng tham gia dự án</span></asp:PlaceHolder>
                                                        </div>
                                                        <div class="nvd-status-stack">
                                                            <span class='project-status-tag <%# GetProjectStatusCss(Eval("TrangThai")) %>'><%# GetDuAnStatusText(Eval("TrangThai")) %></span>
                                                            <span class='nvd-time-state <%# Eval("TrangThaiGioCss") %>'><%# Eval("TinhTrangGioText") %></span>
                                                        </div>
                                                    </div>
                                                    <div class="nvd-card-metrics">
                                                        <div class="nvd-metric nvd-metric-featured"><span><i class="far fa-clock me-1"></i>Giờ hệ thống ghi nhận</span><strong><%# Eval("GioHeThongText") %></strong><small>Thời gian đã ghi, chưa tính phiên đang chạy</small></div>
                                                        <div class="nvd-metric nvd-metric-featured nvd-metric-final"><span><i class="fas fa-check-circle me-1"></i>Giờ của nhân viên được PM chốt</span><strong><%# Eval("GioPmChotText") %></strong><small><%# Eval("SoTaskDaChot") %>/<%# Eval("SoTaskLienQuan") %> công việc của nhân viên đã chốt</small></div>
                                                    </div>
                                                    <div class="nvd-card-report"><span><i class="fas fa-file-signature me-1"></i>Giờ nhân viên tự khai</span><strong><%# Eval("GioTuKhaiText") %></strong><small><%# Eval("SoTaskCoTuKhai") %>/<%# Eval("SoTaskLienQuan") %> công việc có dữ liệu tự khai</small></div>
                                                    <div class="nvd-card-footer">
                                                        <div class="nvd-card-note"><i class="fas fa-info-circle me-1"></i><%# GetProjectCardInsight(Container.DataItem) %>
                                                            <asp:PlaceHolder runat="server" Visible='<%# Convert.ToBoolean(Eval("HienThiTyTrong")) %>'>
                                                                <span class="nvd-share"><i class="fas fa-chart-pie me-1"></i>Tỷ trọng giờ đã quyết toán: <strong><%# Eval("TyTrongText") %></strong></span>
                                                            </asp:PlaceHolder>
                                                            <details class="nvd-help-details"><summary>Vì sao số giờ khác nhau?</summary><div><%# System.Web.HttpUtility.HtmlEncode(Convert.ToString(Eval("GhiChuGioText"))) %></div></details>
                                                        </div>
                                                        <asp:LinkButton ID="btnViewProject" runat="server" CommandName="ViewProject" CommandArgument='<%# Eval("IdDuAn") %>' CssClass="btn btn-sm btn-outline-primary btn-project-detail"><i class="fas fa-eye me-1"></i>Xem chi tiết</asp:LinkButton>
                                                    </div>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>

                <!-- =====================================================
                     PROJECT DETAIL PANEL
                     ===================================================== -->
                <div id="projectDetailOverlay" class="side-panel-overlay project-detail-overlay" onclick="closeProjectDetailPanel();"></div>

                <div id="projectDetailPanel" class="side-panel project-detail-panel">
                    <div class="side-panel-header nvd-detail-topbar">
                        <div class="nvd-detail-top-left">
                            <button type="button" class="nvd-back-btn" onclick="closeProjectDetailPanel();" aria-label="Quay lại danh sách dự án"><i class="fas fa-arrow-left me-1"></i>Danh sách dự án</button>
                            <span class="nvd-topbar-divider"></span>
                            <h5 class="side-panel-title"><i class="fas fa-project-diagram text-primary me-2"></i>Chi tiết dự án</h5>
                        </div>
                        <button type="button" class="btn-close-panel" onclick="closeProjectDetailPanel();" aria-label="Đóng chi tiết dự án"><i class="fas fa-times"></i></button>
                    </div>

                    <div class="side-panel-body">
                        <asp:UpdatePanel ID="upProjectDetail" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div class="nvd-audience-hint"><i class="far fa-user-circle"></i><strong><%= IsOwnEmployeeProfile ? "Giờ công của bạn" : "Theo dõi giờ công nhân viên" %></strong><span><%= IsOwnEmployeeProfile ? "Xem số giờ đã ghi, đã báo và đã được PM chốt." : "Theo dõi mức độ chốt của nhân viên này; chi tiết theo từng công việc bên dưới." %></span></div>
                                <div class="detail-project-header nvd-detail-heading">
                                    <div class="d-flex justify-content-between align-items-start flex-wrap gap-2 mb-3">
                                        <div>
                                            <div class="detail-project-code"><asp:Literal ID="ltrDetailMaDuAn" runat="server" /></div>
                                            <div class="detail-project-title"><asp:Literal ID="ltrDetailTenDuAn" runat="server" /></div>
                                            <div class="project-meta">Vai trò: <strong><asp:Literal ID="ltrDetailVaiTro" runat="server" /></strong> · Thời gian: <strong><asp:Literal ID="ltrDetailProjectTime" runat="server" /></strong></div>
                                        </div>
                                        <asp:Literal ID="ltrDetailTrangThai" runat="server" />
                                    </div>
                                    <asp:Literal ID="ltrDetailContribution" runat="server" />
                                </div>
                                <details class="nvd-hours-guide">
                                    <summary><i class="far fa-question-circle me-1"></i>Phân biệt 3 loại giờ công <i class="fas fa-chevron-down nvd-guide-chevron" aria-hidden="true"></i></summary>
                                    <div class="nvd-hours-guide-content">
                                        <div><strong>Giờ hệ thống:</strong> Thời gian các đồng hồ đã được hệ thống ghi nhận, có thể chưa bao gồm phiên đang chạy.</div>
                                        <div><strong>Giờ nhân viên báo:</strong> Số giờ nhân viên tự khai để đối soát, chưa đồng nghĩa PM đã chốt.</div>
                                        <div><strong>Giờ PM chốt:</strong> Số giờ PM đã xác nhận trên các công việc được chốt. Công việc chưa chốt vẫn có thể còn giờ hệ thống hoặc giờ tự khai.</div>
                                    </div>
                                </details>
                                <div class="nvd-tasks-toolbar"><div><strong><i class="fas fa-tasks me-1"></i>Công việc liên quan</strong><small>Nhấn tên giai đoạn để xem công việc, tình trạng giờ và lý do trễ hạn khi có.</small></div><button type="button" id="nvdPendingFilter" class="nvd-pending-filter" aria-pressed="false" onclick="nvdTogglePendingTasks(this);" title="Chỉ hiện các công việc chưa được PM chốt giờ"><i class="fas fa-filter me-1"></i>Chỉ xem chưa chốt</button></div>
                                <div id="nvdPendingEmpty" class="nvd-filter-empty" hidden>Không có công việc chưa được PM chốt giờ trong danh sách này.</div>
                                <asp:Repeater ID="rptDetailPhases" runat="server">
                                    <ItemTemplate>
                                        <details class="phase-item nvd-phase">
                                            <summary class="phase-header-line nvd-phase-summary">
                                                <div class="phase-title-name"><i class="far fa-folder-open me-1"></i><%# System.Web.HttpUtility.HtmlEncode(Convert.ToString(Eval("MaPhase"))) %> · <%# System.Web.HttpUtility.HtmlEncode(Convert.ToString(Eval("TenPhase"))) %>
                                                    <span class="text-muted fw-normal ms-1">(<%# Eval("SoTaskLienQuan") %> công việc)</span>
                                                </div>
                                                <div class="nvd-phase-hours"><span>Hệ thống: <strong><%# Eval("GioHeThongText") %></strong></span><span>PM chốt: <strong><%# Eval("GioPmChotText") %></strong></span></div>
                                            </summary>
                                            <div class="nvd-task-scroll">
                                                <table class="task-sub-table nvd-task-table">
                                                    <thead><tr><th>Công việc / xác nhận giờ</th><th>Trạng thái công việc</th><th>Giờ hệ thống</th><th>Nhân viên báo</th><th>PM đã chốt</th></tr></thead>
                                                    <tbody>
                                                        <asp:Repeater ID="rptDetailTasks" runat="server" DataSource='<%# Eval("MyTasks") %>'>
                                                            <ItemTemplate>
                                                                <tr class="task-row" data-status='<%# Eval("TrangThaiTask") %>' data-hour-final='<%# Convert.ToBoolean(Eval("DaChotTask")) ? "1" : "0" %>'>
                                                                    <td class="nvd-task-name">
                                                                        <a href='<%# GetTaskListUrl() %>' target="_blank" rel="noopener" class="fw-bold text-primary">[<%# System.Web.HttpUtility.HtmlEncode(Convert.ToString(Eval("MaTask"))) %>] <%# System.Web.HttpUtility.HtmlEncode(Convert.ToString(Eval("TenTask"))) %></a>
                                                                        <div class='nvd-task-progress <%# Convert.ToBoolean(Eval("DaChotTask")) ? "nvd-task-progress-final" : "" %>'><i class='<%# Convert.ToBoolean(Eval("DaChotTask")) ? "fas fa-check-circle" : "far fa-clock" %>'></i><%# GetTaskWorkProgressText(Eval("TrangThaiTask"),Eval("DaChotTask")) %></div>
                                                                        <div class="nvd-task-actions">
                                                                            <asp:PlaceHolder runat="server" Visible='<%# IsTaskCompletedLate(Eval("TrangThaiTask"), Eval("NgayKetThuc"), Eval("NgayHoanThanhThucTe")) %>'>
                                                                                <button type="button" class="nvd-reason-btn" data-task-code="<%# System.Web.HttpUtility.HtmlAttributeEncode(Convert.ToString(Eval("MaTask"))) %>" data-task-name="<%# System.Web.HttpUtility.HtmlAttributeEncode(Convert.ToString(Eval("TenTask"))) %>" data-reason="<%# System.Web.HttpUtility.HtmlAttributeEncode(Convert.ToString(Eval("LyDoTreHan"))) %>" onclick="showDelayReasonFromButton(this);"><i class="far fa-file-alt me-1"></i>Xem lý do trễ</button>
                                                                            </asp:PlaceHolder>
                                                                            <details class="nvd-task-more"><summary>Thông tin công việc</summary>
                                                                                <div>Bắt đầu: <%# FormatDateSafe(Eval("NgayBatDau"), "—") %> · Dự kiến: <%# FormatDateSafe(Eval("NgayKetThuc"), "—") %> · Thực tế: <%# FormatDateSafe(Eval("NgayHoanThanhThucTe"), "—") %></div>
                                                                                <div>Ưu tiên: <%# System.Web.HttpUtility.HtmlEncode(Convert.ToString(Eval("TenDoUuTien"))) %> · <%# Eval("ThoiHanNgay") %> ngày</div>
                                                                            </details>
                                                                        </div>
                                                                    </td>
                                                                    <td class="nvd-task-status-cell"><%# GetTaskStatusDisplay(Eval("TrangThaiTask"),Eval("NgayKetThuc"),Eval("NgayHoanThanhThucTe")) %></td>
                                                                    <td class="nvd-hours"><strong><%# Eval("GioHeThongText") %></strong><small>Theo lịch <%# Eval("GioHanhChinhText") %> · Tăng ca <%# Eval("GioTangCaText") %></small></td>
                                                                    <td class="nvd-hours"><strong><%# Eval("GioTuKhaiText") %></strong><small>Theo lịch <%# Eval("GioTuKhaiThuongText") %> · Tăng ca <%# Eval("GioTuKhaiTangCaText") %></small></td>
                                                                    <td class="nvd-hours nvd-hours-final"><strong><%# Eval("GioPmChotText") %></strong><small><%# Convert.ToBoolean(Eval("DaChotTask")) ? "PM đã chốt" : "Chưa chốt giờ" %></small></td>
                                                                </tr>
                                                            </ItemTemplate>
                                                        </asp:Repeater>
                                                    </tbody>
                                                </table>
                                            </div>
                                        </details>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script>
        function openSidePanel() {
            var overlay = document.getElementById('sidePanelOverlay');
            var panel = document.getElementById('projectSidePanel');
            if (!overlay || !panel) return;
            overlay.style.display = 'block';
            setTimeout(function () {
                panel.classList.add('open');
                var body = panel.querySelector('.side-panel-body');
                if (body) { body.scrollTop = 0; }
            }, 10);
        }

        function closeSidePanel() {
            var panel = document.getElementById('projectSidePanel');
            var overlay = document.getElementById('sidePanelOverlay');
            if (!panel || !overlay) return;
            panel.classList.remove('open');
            setTimeout(function () { overlay.style.display = 'none'; }, 300);
        }

        function nvdPreparePhases() {
            var panel = document.getElementById('projectDetailPanel');
            if (!panel) return;
            var phases = panel.querySelectorAll('details.nvd-phase');
            if (!phases.length) return;
            // Chỉ mở giai đoạn đầu tiên khi một tập dữ liệu mới được render.
            var anyReady = Array.prototype.some.call(phases, function (p) { return p.getAttribute('data-nvd-initialized') === '1'; });
            if (!anyReady) {
                Array.prototype.forEach.call(phases, function (p, i) { p.open = i === 0; p.setAttribute('data-nvd-initialized','1'); });
            }
        }
        function nvdTogglePendingTasks(button) {
            var panel = document.getElementById('projectDetailPanel');
            if (!panel || !button) return;
            var active = button.getAttribute('aria-pressed') !== 'true';
            button.setAttribute('aria-pressed', active ? 'true' : 'false');
            button.innerHTML = active ? '<i class="fas fa-times me-1"></i>Xem tất cả công việc' : '<i class="fas fa-filter me-1"></i>Chỉ xem chưa chốt';
            var phases = panel.querySelectorAll('details.nvd-phase');
            var any = false;
            Array.prototype.forEach.call(phases, function (phase) {
                var rows = phase.querySelectorAll('tr.task-row');
                var visible = 0;
                Array.prototype.forEach.call(rows, function (row) {
                    var show = !active || row.getAttribute('data-hour-final') !== '1';
                    row.style.display = show ? '' : 'none';
                    if (show) visible++;
                });
                phase.style.display = !active || visible > 0 ? '' : 'none';
                if (active && visible > 0) { any = true; phase.open = true; }
            });
            var empty = document.getElementById('nvdPendingEmpty');
            if (empty) empty.hidden = !active || any;
        }
        function nvdResetTaskFilter() {
            var button = document.getElementById('nvdPendingFilter');
            if (button && button.getAttribute('aria-pressed') === 'true') nvdTogglePendingTasks(button);
        }
        function openProjectDetailPanel() {
            var overlay = document.getElementById('projectDetailOverlay');
            var panel = document.getElementById('projectDetailPanel');
            if (!overlay || !panel) return;
            overlay.style.display = 'block';
            nvdResetTaskFilter();
            nvdPreparePhases();
            setTimeout(function () {
                panel.classList.add('open');
                var body = panel.querySelector('.side-panel-body');
                if (body) { body.scrollTop = 0; }
            }, 10);
        }

        function closeProjectDetailPanel() {
            var panel = document.getElementById('projectDetailPanel');
            var overlay = document.getElementById('projectDetailOverlay');
            if (!panel || !overlay) return;
            panel.classList.remove('open');
            setTimeout(function () { overlay.style.display = 'none'; }, 300);
        }
        // ==========================================
        // VẼ LỊCH LÀM VIỆC TĨNH CỦA THÁNG HIỆN TẠI
        // ==========================================
        function renderEmployeeMonthCalendar() {
            var hiddenField = document.getElementById('hdfScheduleJson');
            var calDiv = document.getElementById('employeeMonthCalendar');

            // Chốt chặn: Tránh báo lỗi nếu UpdatePanel chưa render kịp các thẻ này
            if (!hiddenField || !calDiv) return;

            var WEEKDAYS = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];
            function pad(n) { return n < 10 ? '0' + n : '' + n; }
            function toKey(y, month, d) { return y + '-' + pad(month + 1) + '-' + pad(d); }
            function escAttr(s) { return String(s).replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;'); }

            var jsonString = hiddenField.value;
            var data = {};
            if (jsonString) {
                try { data = JSON.parse(jsonString) || {}; } catch (e) { console.error("Lỗi parse Lịch", e); }
            }

            var now = new Date();
            var year = now.getFullYear();
            var month = now.getMonth();
            var todayKey = toKey(year, month, now.getDate());

            // Rút khoảng Min/Max từ JSON
            var keys = Object.keys(data).sort();
            var minKey = keys.length ? keys[0] : null;
            var maxKey = keys.length ? keys[keys.length - 1] : null;

            var offset = (new Date(year, month, 1).getDay() + 6) % 7;
            var html = '<div class="mc-weekdays">';
            for (var w = 0; w < 7; w++) {
                html += '<span' + (w >= 5 ? ' class="mc-we"' : '') + '>' + WEEKDAYS[w] + '</span>';
            }
            html += '</div><div class="mc-grid">';

            // Vẽ cố định 6 hàng = 42 ô
            for (var i = 0; i < 42; i++) {
                var d = new Date(year, month, 1 - offset + i);
                var key = toKey(d.getFullYear(), d.getMonth(), d.getDate());
                var inRange = !!minKey && key >= minKey && key <= maxKey;
                var info = inRange ? data[key] : null;

                var cls = 'mc-day';
                if (d.getMonth() !== month) cls += ' out-month';
                if (!inRange) cls += ' out-range';
                else if (info && info.status) cls += ' st-' + info.status;
                if (key === todayKey) cls += ' today';

                var extra = '';
                var clickAttr = ''; // [MỚI]

                if (info) {
                    var label = String(info.text || '').replace(/^[^A-Za-z0-9\u00C0-\u1EF9]+/, '');
                    if (label) extra = '<span class="mc-label">' + escAttr(label) + '</span>';

                    // [MỚI]: Xử lý Tooltip y hệt như bên Chọn Nhân Viên
                    var hasTasks = (info.status === "busy" && info.tasks && info.tasks.length > 0);
                    if (hasTasks) {
                        cls += ' has-tooltip';
                        var listHtml = '<ul class="tooltip-task-list">';
                        for (var t = 0; t < info.tasks.length; t++) {
                            listHtml += '<li><div class="t-project-name"><i class="fas fa-cube me-1"></i>' + escAttr(info.tasks[t].projectName) + '</div><span class="t-code">[' + escAttr(info.tasks[t].code) + ']</span>' + escAttr(info.tasks[t].name) + '</li>';
                        }
                        listHtml += '</ul>';

                        // Ép data html vào thuộc tính ẩn
                        clickAttr = ' data-tooltip-content="' + encodeURIComponent(listHtml) + '"';
                    } else if (info.text) {
                        clickAttr = ' title="' + escAttr(d.getDate() + '/' + (d.getMonth() + 1) + ' - ' + info.text) + '"';
                    }
                }

                html += '<div class="' + cls + '"' + clickAttr + '><span class="mc-num">' + d.getDate() + '</span>' + extra + '</div>';
            }
            html += '</div>';
            calDiv.innerHTML = html;
        }
        // ============================================================
        // ĐỘNG CƠ TOOLTIP GLOBAL (ĐỊNH VỊ TUYỆT ĐỐI THEO TỌA ĐỘ CHUỘT)
        // ============================================================
        if ($('#global-task-tooltip').length === 0) {
            $('body').append('<div id="global-task-tooltip" class="global-task-tooltip"></div>');
        }
        var $globalTooltip = $('#global-task-tooltip');
        var pinnedDay = null;

        function showGlobalTooltip(dayEl) {
            var $day = $(dayEl);
            var contentRaw = $day.attr('data-tooltip-content');
            if (!contentRaw) return;

            $globalTooltip.html(decodeURIComponent(contentRaw));

            var offset = $day.offset();
            var ttWidth = $globalTooltip.outerWidth();
            var ttHeight = $globalTooltip.outerHeight();
            var dayWidth = $day.outerWidth();

            var topPos = offset.top - ttHeight - 10;
            var leftPos = offset.left + (dayWidth / 2) - (ttWidth / 2);

            var windowWidth = $(window).width();
            var arrowLeft = 50;

            if (leftPos < 10) {
                var overstep = 10 - leftPos;
                leftPos = 10;
                arrowLeft = 50 - (overstep / ttWidth * 100);
            } else if (leftPos + ttWidth > windowWidth - 10) {
                var overstep = (leftPos + ttWidth) - (windowWidth - 10);
                leftPos = windowWidth - ttWidth - 10;
                arrowLeft = 50 + (overstep / ttWidth * 100);
            }

            $globalTooltip.css({
                top: topPos + 'px',
                left: leftPos + 'px',
                '--arrow-left': arrowLeft + '%'
            });

            $globalTooltip.addClass('show');
        }

        function hideGlobalTooltip() {
            $globalTooltip.removeClass('show');
        }

        $(document).on('mouseenter', '.mc-day.has-tooltip', function () {
            if (pinnedDay) return;
            showGlobalTooltip(this);
        }).on('mouseleave', '.mc-day.has-tooltip', function () {
            if (pinnedDay) return;
            hideGlobalTooltip();
        });

        // Click để Ghim
        $(document).on('click', '.mc-day.has-tooltip', function (e) {
            e.stopPropagation();
            if (pinnedDay === this) {
                pinnedDay = null;
                hideGlobalTooltip();
                $(this).removeClass('show-tooltip');
            } else {
                $('.mc-day').removeClass('show-tooltip');
                pinnedDay = this;
                $(this).addClass('show-tooltip');
                showGlobalTooltip(this);
            }
        });

        // Click ra ngoài tắt ghim
        $(document).on('click', function () {
            pinnedDay = null;
            $('.mc-day').removeClass('show-tooltip');
            hideGlobalTooltip();
        });

        // Cuộn chuột tắt ghim (Trường hợp Lịch nằm trong vùng có thanh cuộn)
        $('.side-panel-body').on('scroll', function () {
            if (pinnedDay) {
                pinnedDay = null;
                $('.mc-day').removeClass('show-tooltip');
                hideGlobalTooltip();
            }
        });
        // 1. Chạy lần đầu khi load trang (F5)
        $(document).ready(function () {
            renderEmployeeMonthCalendar();
        });

        // 2. Chạy lại MỖI KHI UpdatePanel load xong ngầm (Đóng/Mở popup, lọc trạng thái...)
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) {
                renderEmployeeMonthCalendar();
            });
        }
        function showDelayReasonFromButton(button) {
            if (!button) return;
            showDelayReason(button.getAttribute('data-task-code') || '', button.getAttribute('data-task-name') || '', button.getAttribute('data-reason') || '');
        }
        // Popup lý do trễ dùng đúng SweetSoft:ExtraModal trong cpModalMain.
        // Khi đóng bằng nút X, ESC hoặc backdrop, khôi phục độ nổi của hai side panel.
        $(document).on('hidden.bs.modal', '#<%= mdlDelayReason.ClientID %>', function () {
            document.body.classList.remove('nvd-delay-reason-active');
        });
        function showDelayReason(taskCode, taskName, reason) {
            // Dùng .text() để dữ liệu lý do do người dùng nhập không bị diễn giải thành HTML.
            $('#delayTaskCode').text('[' + taskCode + '] ' + taskName);
            $('#delayTaskReason').text(reason && reason.trim() ? reason : 'Chưa có lý do được ghi nhận cho công việc này.');
            var $modal = $('#<%= mdlDelayReason.ClientID %>');
            if (!$modal.length) return;
            // ExtraModal/Bootstrap giữ nguyên header, nút X, backdrop và animation mặc định.
            document.body.classList.add('nvd-delay-reason-active');
            try {
                $modal.modal('show');
            } catch (err) {
                document.body.classList.remove('nvd-delay-reason-active');
                throw err;
            }
        }
    </script>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:CtrlNhanVienPopup runat="server" ID="CtrlNhanVienPopup1" />
    
    <SweetSoft:ExtraModal runat="server" ID="mdlDelayReason" Type="Primary" Title="Lí do hoàn thành trễ hạn">
        <ContentTemplate>
            <div class="p-4">
                <!-- Khu vực Header của Popup: Có Icon nổi bật và Tên Task -->
                <div class="d-flex align-items-center gap-3 mb-3 pb-3" style="border-bottom: 1px dashed #e2e8f0;">
                    <div style="width: 48px; height: 48px; background-color: #fee2e2; color: #ef4444; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 22px; flex-shrink: 0; box-shadow: 0 2px 4px rgba(239, 68, 68, 0.1);">
                        <i class="fas fa-history"></i>
                    </div>
                    <div>
                        <span class="d-block text-muted fw-bold" style="font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px;">Phát sinh từ công việc</span>
                        <div class="fw-bolder text-dark mt-1" id="delayTaskCode" style="font-size: 16px;"></div>
                    </div>
                </div>
                
                <!-- Khu vực Nội dung: Khung nổi, chữ bự, có viền đỏ cảnh báo bên trái -->
                <div style="background-color: #f8fafc; border: 1px solid #e2e8f0; border-left: 4px solid #ef4444; border-radius: 8px; padding: 18px;">
                    <label class="fw-bold d-block mb-2" style="font-size: 11.5px; color: #64748b; text-transform: uppercase;">Lí do:</label>
                    <div id="delayTaskReason" class="text-dark fw-medium" style="font-size: 15px; white-space: pre-wrap; line-height: 1.6; max-height: 300px; overflow-y: auto;"></div>
                </div>
            </div>
        </ContentTemplate>
    </SweetSoft:ExtraModal>
</asp:Content>