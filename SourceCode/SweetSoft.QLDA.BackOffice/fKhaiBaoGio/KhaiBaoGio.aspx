<%@ Page Title="Tính giờ công việc" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="KhaiBaoGio.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fKhaiBaoGio.KhaiBaoGio" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>
<%@ Register Src="~/fTasks/Controls/CtrlStartTask.ascx" TagPrefix="SweetSoft" TagName="CtrlStartTask" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        .ks-page {
            --ks-primary: #5b21b6;
            --ks-primary-soft: #f3efff;
            --ks-border: #e2e8f0;
            --ks-text: #1e293b;
            --ks-muted: #64748b;
            --ks-ok: #15803d;
            --ks-ok-soft: #dcfce7;
            --ks-warn: #b45309;
            --ks-warn-soft: #fef3c7;
            --ks-danger: #b91c1c;
            --ks-danger-soft: #fee2e2;
            font-family: "Nunito", "Segoe UI", Arial, sans-serif;
            font-size: 13px;
            color: var(--ks-text);
            max-width: 1230px;
            margin: 0 auto;
            padding: 8px 4px 28px;
        }
        .ks-page * { box-sizing: border-box; }
        .ks-hidden { display: none !important; }

        /* ---- Đầu trang ---- */
        .ks-header { display: flex; align-items: center; justify-content: space-between; gap: 12px; flex-wrap: wrap; margin-bottom: 14px; }
        .ks-header-left { display: flex; align-items: center; gap: 12px; }
        .ks-avatar { width: 44px; height: 44px; border-radius: 50%; background: var(--ks-primary); color: #fff; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 16px; }
        .ks-hello { font-size: 17px; font-weight: 700; color: #4b168c; }
        .ks-subtitle { color: var(--ks-muted); margin-top: 2px; }
        .ks-link { color: var(--ks-primary); font-weight: 600; text-decoration: none; padding: 6px 12px; border: 1px solid var(--ks-border); border-radius: 8px; background: #fff; }
        .ks-link:hover { background: var(--ks-primary-soft); }

        /* ---- Thẻ tổng hợp ngày làm việc ---- */
        .ks-day-card { background: #fff; border: 1px solid var(--ks-border); border-top: 4px solid #94a3b8; border-radius: 12px; padding: 16px 18px; margin-bottom: 14px; box-shadow: 0 2px 8px rgba(15, 23, 42, 0.04); }
        .ks-day-card.is-open { border-top-color: var(--ks-primary); box-shadow: 0 4px 14px rgba(91, 33, 182, 0.08); }
        .ks-day-card.is-done { border-top-color: var(--ks-ok); }
        .ks-day-card.is-missed { border-top-color: var(--ks-danger); }

        .ks-day-top { display: flex; align-items: flex-start; justify-content: space-between; gap: 12px; flex-wrap: wrap; margin-bottom: 12px; }
        .ks-day-title { font-weight: 700; color: #4b168c; font-size: 15px; display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
        .ks-day-hours { color: var(--ks-text); font-weight: 600; background: #f1f5f9; padding: 2px 10px; border-radius: 6px; font-size: 12.5px; }
        .ks-day-lunch { color: var(--ks-muted); font-size: 12px; font-weight: 500; }

        .ks-chip { display: inline-flex; align-items: center; gap: 5px; padding: 4px 11px; border-radius: 999px; font-size: 11.5px; font-weight: 700; border: 1px solid transparent; background: #f1f5f9; color: #475569; }
        .ks-chip.is-open { background: var(--ks-primary-soft); color: var(--ks-primary); border-color: #ddd0f7; }
        .ks-chip.is-lunch { background: var(--ks-warn-soft); color: var(--ks-warn); border-color: #fde68a; }
        .ks-chip.is-done { background: var(--ks-ok-soft); color: var(--ks-ok); }
        .ks-chip.is-missed { background: var(--ks-danger-soft); color: var(--ks-danger); }

        .ks-day-stats { display: flex; align-items: baseline; justify-content: space-between; gap: 12px; flex-wrap: wrap; margin-bottom: 8px; }
        .ks-day-total-big { font-size: 23px; font-weight: 800; color: var(--ks-primary); font-variant-numeric: tabular-nums; }
        .ks-day-total-sub { font-size: 13px; color: var(--ks-muted); font-weight: 600; margin-left: 6px; }
        .ks-day-countdown { color: var(--ks-muted); font-size: 12.5px; }
        .ks-day-countdown b { color: var(--ks-text); font-variant-numeric: tabular-nums; }

        .ks-progress { height: 9px; border-radius: 999px; background: #eef2f7; overflow: hidden; }
        .ks-progress-bar { height: 100%; background: linear-gradient(90deg, #6d28d9, #4f46e5); border-radius: 999px; transition: width .3s ease; }
        .ks-day-card.is-done .ks-progress-bar { background: var(--ks-ok); }
        .ks-day-card.is-missed .ks-progress-bar { background: var(--ks-danger); }

        /* ---- Thông báo nghỉ trưa / cảnh báo ---- */
        .ks-banner-lunch { display: none; align-items: center; gap: 10px; margin: 10px 14px 0; padding: 10px 14px; border-radius: 8px; background: var(--ks-warn-soft); border: 1px solid #fcd34d; color: var(--ks-warn); font-weight: 600; }
        .ks-warn { display: none; align-items: center; gap: 8px; margin: 10px 14px 0; padding: 9px 12px; border-radius: 8px; background: #fef2f2; border: 1px solid #fecaca; color: var(--ks-danger); font-weight: 600; }

        /* ---- Khung danh sách Task ---- */
        .ks-panel { background: #fff; border: 1px solid var(--ks-border); border-radius: 12px; overflow: hidden; margin-bottom: 12px; }
        .ks-panel-head { padding: 12px 16px; background: var(--ks-primary-soft); border-bottom: 1px solid var(--ks-border); display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 8px; }
        .ks-panel-title { font-weight: 700; color: #4b168c; font-size: 14px; }
        .ks-panel-sub { color: var(--ks-muted); margin-top: 2px; font-size: 12.5px; }
        .ks-panel-body { padding: 10px 14px 14px; background: #f8fafc; }
        .ks-group-title { margin: 10px 0 6px; font-weight: 700; font-size: 12px; text-transform: uppercase; letter-spacing: .03em; color: #475569; }
        .ks-group-title.is-overdue { color: var(--ks-danger); }

        /* ---- Dòng Task & Đồng hồ bấm giờ ---- */
        .ks-task { display: flex; align-items: center; justify-content: space-between; gap: 16px; background: #fff; border: 1px solid var(--ks-border); border-left: 4px solid #60a5fa; border-radius: 10px; padding: 12px 14px; margin-bottom: 8px; transition: box-shadow .2s, border-color .2s; }
        .ks-task.is-overdue { border-left-color: #ef4444; }
        .ks-task.is-extra { border-left-color: #94a3b8; opacity: .85; }
        .ks-task.is-running { border-color: #8b5cf6; border-left: 5px solid var(--ks-primary); background: #fdfcff; box-shadow: 0 4px 14px rgba(91, 33, 182, 0.12); }

        .ks-task-info { min-width: 0; flex: 1 1 auto; }
        .ks-kicker { font-size: 12px; color: var(--ks-muted); font-weight: 600; }
        .ks-task-name { font-weight: 700; font-size: 14px; margin: 3px 0 5px; word-break: break-word; color: var(--ks-text); }
        .ks-task-meta { color: var(--ks-muted); font-size: 12px; display: flex; flex-wrap: wrap; gap: 6px 12px; align-items: center; }

        .ks-badge { display: inline-block; padding: 2px 8px; border-radius: 999px; font-size: 11px; font-weight: 600; background: #e0f2fe; color: #0369a1; }
        .ks-badge.is-todo { background: #f1f5f9; color: #475569; }
        .ks-badge.is-overdue { background: #fee2e2; color: var(--ks-danger); }
        .ks-badge.is-clock-running { background: #e7f7ed; color: #14753a; }
        .ks-badge.is-clock-paused { background: #fff4df; color: #9b5b0b; }

        /* ---- Cụm điều khiển Stopwatch bên phải Task ---- */
        .ks-task-side { flex: 0 0 auto; display: flex; flex-direction: column; align-items: flex-end; gap: 5px; min-width: 225px; }
        .ks-tracker-row { display: flex; align-items: center; gap: 10px; }

        .ks-timer-box { text-align: right; }
        .ks-stopwatch { font-family: "Consolas", "Courier New", monospace; font-size: 20px; font-weight: 800; color: var(--ks-text); letter-spacing: 0.5px; line-height: 1.1; font-variant-numeric: tabular-nums; }
        .ks-task.is-running .ks-stopwatch { color: var(--ks-primary); }
        .ks-decimal-hours { font-size: 11.5px; font-weight: 700; color: var(--ks-muted); margin-top: 2px; }
        .ks-task.is-running .ks-decimal-hours { color: var(--ks-ok); }

        .ks-action-btn { display: inline-flex; align-items: center; justify-content: center; gap: 6px; min-width: 108px; padding: 8px 14px; border-radius: 8px; font-size: 13px; font-weight: 700; border: 1px solid transparent; cursor: pointer; transition: all .15s ease; }
        .ks-action-btn.btn-start { background: var(--ks-primary); color: #fff; }
        .ks-action-btn.btn-start:hover:not(:disabled) { filter: brightness(1.1); transform: translateY(-1px); }
        .ks-action-btn.btn-resume { background: var(--ks-ok); color: #fff; }
        .ks-action-btn.btn-resume:hover:not(:disabled) { filter: brightness(1.08); transform: translateY(-1px); }
        .ks-action-btn.btn-stop { background: var(--ks-danger); color: #fff; box-shadow: 0 2px 8px rgba(185, 28, 28, 0.25); }
        .ks-action-btn.btn-stop:hover:not(:disabled) { filter: brightness(1.08); }
        .ks-action-btn:disabled { background: #e2e8f0 !important; color: #94a3b8 !important; border-color: #cbd5e1 !important; cursor: not-allowed; transform: none !important; box-shadow: none !important; }

        .ks-pulse-dot { width: 8px; height: 8px; border-radius: 50%; background: #fff; display: inline-block; animation: ksPulse 1.2s infinite; }
        @keyframes ksPulse {
            0%, 100% { opacity: 1; transform: scale(1); }
            50% { opacity: 0.35; transform: scale(0.8); }
        }

        .ks-hint { font-size: 12px; color: var(--ks-muted); text-align: right; }
        .ks-hint.is-late { color: var(--ks-danger); font-weight: 600; }
        .ks-readonly { font-size: 11.5px; color: var(--ks-muted); font-weight: 600; text-align: right; background: #f1f5f9; padding: 4px 10px; border-radius: 6px; }

        /* ---- Chân trang trạng thái ---- */
        .ks-footer { display: flex; align-items: center; justify-content: space-between; gap: 10px; flex-wrap: wrap; padding: 10px 14px; background: #fff; border: 1px solid var(--ks-border); border-radius: 12px; }
        .ks-save-status { font-weight: 600; color: var(--ks-muted); }
        .ks-save-status.is-ok { color: var(--ks-ok); }
        .ks-save-status.is-error { color: var(--ks-danger); }
        .ks-save-status.is-pending { color: var(--ks-primary); }

        .ks-empty { text-align: center; padding: 26px 14px; color: var(--ks-muted); background: #fff; border: 1px dashed var(--ks-border); border-radius: 12px; margin-bottom: 12px; }
        .ks-empty i { font-size: 26px; color: #a78bfa; display: block; margin-bottom: 8px; }
        .ks-empty b { color: var(--ks-text); }

        /* Nâng cấp nhẹ: tìm kiếm/lọc theo nhịp giao diện CtrlNhanViens, không tạo postback. */
        .ks-task.is-running { scroll-margin-top: 85px; }
        .ks-group-title.is-running-title { color: var(--ks-primary); font-size: 12.5px; }
        .ks-filter-bar { display:flex; align-items:center; flex-wrap:wrap; gap:8px; padding:11px 14px; background:#fff; border-bottom:1px solid var(--ks-border); }
        .ks-filter-select { flex:0 0 auto; min-width:134px; max-width:220px; height:36px; padding:6px 30px 6px 11px; background-color:#f4f0ff; border:1px solid #d9c7f4; border-radius:7px; color:#552095; font-size:12.5px; font-weight:600; font-family:inherit; cursor:pointer; }
        .ks-filter-select:focus,.ks-filter-input:focus { outline:2px solid rgba(91,33,182,.17); border-color:#7c3aed; }
        .ks-filter-search { min-width:190px; max-width:430px; flex:1 1 240px; display:flex; align-items:center; border:1px solid #a884d9; border-radius:7px; height:36px; background:#fff; overflow:hidden; }
        .ks-filter-input { width:100%; min-width:0; border:0; background:transparent; padding:7px 10px; font:inherit; font-size:12.5px; color:var(--ks-text); outline:0!important; }
        .ks-filter-input:focus { outline:0!important; }
        .ks-filter-search i { color:var(--ks-primary); padding:0 11px; }
        .ks-filter-clear { border:1px solid #dcd5ea; border-radius:7px; background:#fff; color:#5b21b6; font-weight:650; padding:7px 11px; min-height:36px; cursor:pointer; }
        .ks-filter-clear:hover { background:#f6f1ff; }
        .ks-filter-info { color:var(--ks-muted); font-size:12px; margin-left:auto; }
        .ks-filter-tags { display:flex; align-items:center; flex-wrap:wrap; gap:6px; padding:0 14px 9px; background:#fff; }
        .ks-filter-tags:empty { display:none; }
        .ks-filter-tag { display:inline-flex; align-items:center; gap:5px; border:1px solid #e1d4f3; border-radius:6px; padding:3px 8px; background:#f5f0fd; color:#5b218d; font-size:11.5px; }
        .ks-filter-tag button { border:0; background:transparent; color:inherit; padding:0 1px; cursor:pointer; font-size:15px; line-height:1; }
        .ks-filter-empty { display:none; padding:25px 15px; background:#fff; border:1px dashed #d7c7ec; border-radius:9px; text-align:center; color:var(--ks-muted); }
        .ks-journal-btn { border:1px solid #e0d3f1; border-radius:6px; background:#faf7ff; color:#5b21b6; font-size:11.5px; font-weight:650; padding:4px 8px; cursor:pointer; }
        .ks-journal-btn:hover { background:#eee5ff; }
        .ks-inline-notice { margin:10px 14px 0; padding:9px 12px; background:#f9f6ff; border:1px solid #e5dcf4; border-radius:8px; font-size:12.5px; }
        .ks-inline-notice:has(.ks-save-status:empty) { display:none; }
        .ks-save-status { display:block; line-height:1.45; }
        .ks-journal-mask[hidden] { display:none!important; }
        .ks-journal-mask { position:fixed; inset:0; background:rgba(22,16,40,.48); z-index:1085; display:flex; align-items:center; justify-content:center; padding:16px; }
        .ks-journal-dialog { width:min(660px,100%); max-height:calc(100vh - 32px); overflow:auto; background:#fff; border:1px solid #e0d6ed; border-radius:12px; box-shadow:0 22px 58px rgba(35,22,65,.25); font-family:"Nunito","Segoe UI",Arial,sans-serif; color:#263245; }
        .ks-journal-header { display:flex; justify-content:space-between; align-items:start; gap:12px; padding:15px 18px; border-bottom:1px solid #ece5f5; background:#f9f6ff; }
        .ks-journal-header h3 { color:#4b168c; font-size:16px; font-weight:750; margin:0 0 5px; }
        .ks-journal-header p { margin:0; color:#64748b; font-size:12px; }
        .ks-journal-close { border:0; width:30px; height:30px; border-radius:7px; background:#eee6f8; color:#5b21b6; font-size:20px; cursor:pointer; }
        .ks-journal-body { padding:16px 18px; }
        .ks-journal-table { width:100%; border-collapse:collapse; font-size:12.5px; }
        .ks-journal-table th { padding:9px 10px; color:#5b21b6; background:#f5f1fc; border-bottom:1px solid #e6ddf2; text-align:left; }
        .ks-journal-table td { padding:10px; border-bottom:1px solid #eff1f5; vertical-align:middle; }
        .ks-journal-total { display:flex; justify-content:space-between; align-items:center; gap:10px; margin-top:13px; padding:12px; border:1px solid #eee5f6; background:#faf7ff; border-radius:9px; font-size:12px; }
        .ks-journal-total strong { color:#51218a; font-size:17px; font-variant-numeric:tabular-nums; }
        .ks-journal-note { font-size:11.5px; color:#64748b; margin:9px 0 0; line-height:1.5; }
        .ks-hint-label { color:#64748b; }
        .ks-blocked-reason { background:#fff6e7; border-radius:6px; padding:4px 8px; color:#a15b0c; font-size:11.5px; text-align:right; }
        @media (max-width: 680px) {
            .ks-task { flex-direction: column; align-items: stretch; }
            .ks-task-side { align-items: flex-start; width: 100%; min-width: 0; border-top: 1px dashed var(--ks-border); padding-top: 10px; }
            .ks-tracker-row { width: 100%; justify-content: space-between; }
            .ks-timer-box, .ks-hint, .ks-readonly { text-align: left; }
            .ks-filter-select { flex:1 1 135px; max-width:none; }
            .ks-filter-search { flex-basis:100%; max-width:none; }
            .ks-filter-info { width:100%; margin-left:0; }
            .ks-journal-body { padding:10px; }
            .ks-journal-table th,.ks-journal-table td { padding:8px 5px; }
        }
    
        /* Issue time tracking: isolated component; Task selectors unchanged. */
        .gv-switch { display:flex; align-items:center; width:max-content; max-width:100%; gap:3px; padding:4px; margin:2px 0 13px; border:1px solid #e5dbf2; background:#f7f3fc; border-radius:10px; }
.gv-switch-btn { display:inline-flex; align-items:center; justify-content:center; gap:7px; min-height:33px; padding:7px 16px; border:1px solid transparent; border-radius:7px; color:#6b587e; background:transparent; font-size:12.5px; font-weight:750; cursor:pointer; white-space:nowrap; }
.gv-switch-btn.is-active { color:#54248a; background:#fff; border-color:#ded2ef; box-shadow:0 1px 3px #2b124018; }
.gv-switch-btn:hover:not(.is-active) { color:#532688; background:#f0e8fb; }
.gv-switch-count { display:inline-grid; place-items:center; height:18px; min-width:18px; padding:0 5px; background:#eee5fa; color:#5d2e8d; border-radius:10px; font-size:10px; }
#kbg20Page.gv-mode-issue #kbg20WorkView .gv-only-task { display:none!important; }
.kbg20-page .gv-issue[hidden], .kbg20-page .gv-only-task[hidden], .gv-modal [hidden], .gv-issue [hidden] { display:none!important; }
.gv-issue { border:1px solid #e8e3ef; border-radius:10px; background:#fff; overflow:hidden; }
.gv-toolbar { display:flex; gap:12px; align-items:center; justify-content:space-between; padding:13px 15px; background:#fff; border-bottom:1px solid #ebe7f2; }
.gv-toolbar-left { min-width:165px; flex:1 1 auto; }
.gv-heading { display:flex; align-items:center; gap:7px; color:#452278; font-size:14px; font-weight:800; white-space:nowrap; }
.gv-heading span { color:#937eab; font-weight:650; font-size:11px; }
.gv-toolbar-left p { margin:3px 0 0; color:#87909e; font-size:11px; line-height:1.4; }
.gv-toolbar-right { display:flex; align-items:center; justify-content:flex-end; gap:8px; flex-wrap:wrap; }
.gv-filters { display:flex; gap:3px; padding:3px; border:1px solid #e9e4f1; border-radius:8px; background:#f8f7fa; }
.gv-filters button { background:transparent; color:#6b7280; border:1px solid transparent; border-radius:6px; padding:5px 8px; font-size:10.5px; font-weight:700; white-space:nowrap; cursor:pointer; }
.gv-filters button.is-active { background:#eee5fb; color:#56248b; border-color:#e0d0f4; }
.gv-filters button:hover:not(.is-active) { background:#f1eaf9; }
.gv-search { display:flex; align-items:center; gap:7px; padding:0 9px; height:33px; width:192px; color:#8c83a1; border:1px solid #e4ddeb; border-radius:8px; background:#fff; }
.gv-search input { flex:1 1 auto; width:0; min-width:0; border:0; outline:0; box-shadow:none; color:#334155; background:transparent; font-size:11px; }
.gv-icon-button { display:inline-flex; align-items:center; justify-content:center; width:32px; height:32px; border:1px solid #e5dded; border-radius:7px; background:#fff; color:#674090; cursor:pointer; }
.gv-icon-button:disabled { opacity:.35; cursor:not-allowed; }
.gv-icon-button:hover:not(:disabled) { background:#f7f2fc; }
.gv-table-scroll { width:100%; overflow-x:auto; }
.gv-table { min-width:760px; width:100%; }
.gv-th, .gv-tr { display:grid; grid-template-columns:minmax(230px,2.65fr) minmax(105px,.95fr) minmax(105px,.95fr) minmax(120px,1fr) minmax(150px,1.15fr); align-items:center; gap:10px; padding:0 14px; }
.gv-th { min-height:39px; background:#f7f5fb; border-bottom:1px solid #e9e3f1; color:#6d5888; font-size:10.5px; font-weight:800; }
.gv-th span:not(:first-child) { text-align:center; }
.gv-tr { min-height:67px; border-bottom:1px solid #f0edf4; background:#fff; transition:background-color .15s; }
.gv-tr:hover { background:#faf8fe; }
.gv-tr:last-child { border-bottom:0; }
.gv-name { display:flex; flex-direction:column; align-items:flex-start; gap:5px; min-width:0; }
.gv-name-line { display:flex; align-items:center; gap:6px; width:100%; min-width:0; }
.gv-code { flex:0 0 auto; border:1px solid #e8d9f8; border-radius:5px; padding:2px 5px; color:#642b97; background:#f7f1fd; font-size:10.5px; font-weight:800; }
.gv-name-title { display:block; min-width:0; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; color:#334155; font-size:12px; font-weight:750; }
.gv-mini { color:#99a1ad; font-size:10px; }
.gv-time { color:#334155; font-size:12px; font-weight:800; font-variant-numeric:tabular-nums; text-align:center; white-space:nowrap; }
.gv-time small { display:block; margin-top:4px; font-size:10px; font-weight:600; color:#8995a3; }
.gv-time.is-running { color:#198453; }
.gv-status-wrap { text-align:center; }
.gv-state { display:inline-flex; justify-content:center; align-items:center; gap:5px; padding:5px 8px; border:1px solid #e5e7eb; border-radius:7px; background:#f5f6f8; color:#667085; font-size:10.5px; font-weight:750; white-space:nowrap; }
.gv-state.is-active { border-color:#c5d9f8; background:#eef5ff; color:#245cb3; }
.gv-state.is-running { border-color:#bce5ce; background:#edf9f1; color:#168457; }
.gv-state.is-pending { border-color:#f4dea0; background:#fffae8; color:#9b680b; }
.gv-state.is-done { border-color:#bae5c5; background:#effaf2; color:#247c43; }
.gv-state.is-rejected { border-color:#f4c4ca; background:#fff3f3; color:#bc394b; }
.gv-actions { display:flex; align-items:center; justify-content:center; gap:6px; }
.gv-action { display:inline-flex; align-items:center; justify-content:center; gap:5px; padding:6px 10px; border-radius:7px; border:1px solid #d8c9e9; background:#f5effb; color:#592f87; min-height:31px; font-size:11px; font-weight:800; line-height:1.15; white-space:nowrap; cursor:pointer; }
.gv-action:hover:not(:disabled) { filter:brightness(.97); }
.gv-action:disabled { opacity:.45; cursor:not-allowed; }
.gv-actions .gv-action { min-width:102px; }
.gv-actions .gv-icon-button { width:31px; height:31px; }
.gv-action-start { border-color:#b9e3c4; color:#167343; background:#ecf9ef; }
.gv-action-stop { border-color:#f0c4c7; color:#b33448; background:#fff2f3; }
.gv-action-submit { border-color:#d9c4ef; color:#61269c; background:#f6f0fd; }
.gv-empty { padding:36px 16px; color:#93909d; text-align:center; font-size:12px; }
.gv-foot { display:flex; align-items:center; justify-content:space-between; gap:10px; padding:9px 14px; color:#85909e; border-top:1px solid #eeebf3; background:#fdfcff; font-size:10.5px; }
.gv-foot-actions { display:flex; align-items:center; gap:9px; }
.gv-modal { display:flex; flex-direction:column; gap:13px; color:#334155; }
#mdlGioXuLyVanDe .modal-dialog, #mdlGioXuLyVanDe.modal-dialog { width:calc(100vw - 36px)!important; max-width:960px!important; margin-left:auto!important; margin-right:auto!important; }
#mdlGioXuLyVanDe .modal-body { max-height:calc(100vh - 180px); overflow-y:auto; }
.gv-modal-title { font-size:16px; font-weight:800; color:#542589; }
.gv-modal-stats { display:grid; grid-template-columns:repeat(4,minmax(0,1fr)); gap:9px; }
.gv-modal-stats>div { display:flex; flex-direction:column; gap:8px; padding:12px; border:1px solid #e6e0ee; border-radius:8px; min-width:0; background:#faf9fc; }
.gv-modal-stats small { color:#7c8592; font-size:10.5px; font-weight:700; }
.gv-modal-stats strong { color:#283547; font-size:18px; font-variant-numeric:tabular-nums; font-weight:800; }
.gv-modal-state { color:#674096; font-size:12px; font-weight:800; }
.gv-modal-note { padding:10px 12px; border:1px solid #e7dbf3; border-radius:7px; color:#624186; background:#f9f5fd; font-size:11.5px; }
.gv-modal-section { padding:12px; border:1px solid #e7e3ec; border-radius:9px; background:#fff; }
.gv-modal-section h4, .gv-history-head h4 { margin:0 0 7px; font-size:12.5px; color:#374151; font-weight:800; }
.gv-modal-section p { color:#818997; font-size:11.5px; margin:0 0 9px; }
.gv-modal-section textarea { display:block; width:100%; padding:8px 10px; border:1px solid #dedbe6; border-radius:7px; font:inherit; font-size:12px; resize:vertical; box-sizing:border-box; }
.gv-history-head { display:flex; justify-content:space-between; align-items:center; gap:10px; }
.gv-history-head span { color:#9b94a4; font-size:11px; }
.gv-history-scroll { max-height:248px; overflow:auto; border:1px solid #ebe7f0; border-radius:7px; }
.gv-history-scroll table { border-collapse:collapse; width:100%; min-width:590px; font-size:11.5px; }
.gv-history-scroll th { position:sticky; top:0; background:#f7f5fa; text-align:left; padding:9px 11px; color:#7b688e; font-size:10.5px; white-space:nowrap; }
.gv-history-scroll td { padding:9px 11px; border-top:1px solid #f0edf3; color:#4c5564; }
.gv-right { text-align:right!important; font-variant-numeric:tabular-nums; }
.gv-modal-buttons { display:flex; justify-content:flex-end; gap:8px; flex-wrap:wrap; }
.gv-modal-buttons .gv-action { min-width:116px; }
.gv-modal-buttons .gv-action[hidden] { display:none!important; }
@media (max-width:1220px) { .gv-toolbar { align-items:flex-start; flex-wrap:wrap; } .gv-toolbar-right { flex:1 1 100%; justify-content:flex-start; } }
@media (max-width:680px) { .gv-toolbar { padding:10px; } .gv-toolbar-right { flex-wrap:wrap; } .gv-filters { width:100%; overflow-x:auto; } .gv-search { width:calc(100% - 44px); } .gv-modal-stats { grid-template-columns:repeat(2,minmax(0,1fr)); } .gv-switch { width:100%; } .gv-switch-btn { flex:1 1 50%; padding:7px 9px; } }
    
        /* ISSUE: giu style hien tai, chi bo sung ngay trong KhaiBaoGio.aspx */
        .gv-th,.gv-tr { grid-template-columns:minmax(200px,2.3fr) minmax(125px,1.05fr) minmax(100px,.86fr) minmax(110px,1fr) minmax(110px,1fr) minmax(153px,1.3fr); }
        .gv-table { min-width:950px; }
        .gv-state.is-reported { border-color:#bde4c8; background:#eefaf1; color:#147a46; }
        .gv-state.is-unreported { border-color:#e1e5ed; background:#f6f7f9; color:#667085; }
        .gv-personal-panel { background:#fbf9ff; }
        .gv-section-top { display:flex; align-items:center; justify-content:space-between; gap:12px; margin-bottom:8px; }
        .gv-form-grid { display:grid; grid-template-columns:repeat(2,minmax(0,1fr)); gap:12px; margin-bottom:11px; }
        .gv-form-grid label,.gv-form-label { display:flex; flex-direction:column; gap:5px; color:#6b5a85; font-size:11.5px; font-weight:700; }
        .gv-form-grid input { width:100%; height:36px; border:1px solid #dcd4e8; border-radius:7px; padding:6px 10px; background:#fff; color:#312047; font:inherit; font-weight:750; }
        .gv-form-label textarea { margin-top:3px; }
        .gv-report-people { display:flex; flex-direction:column; gap:7px; max-height:195px; overflow:auto; }
        .gv-report-person { display:flex; justify-content:space-between; align-items:center; gap:10px; border:1px solid #eee8f4; border-radius:7px; padding:9px 11px; background:#fff; }
        .gv-report-person strong { color:#413454; font-size:11.5px; }
        .gv-report-person small { display:block; color:#8e91a0; font-size:10.5px; }
        .gv-pm-table-wrap { overflow-x:auto; }
        .gv-pm-table { width:100%; border-collapse:collapse; min-width:680px; font-size:11px; }
        .gv-pm-table th,.gv-pm-table td { padding:8px 7px; border-bottom:1px solid #eee8f4; text-align:left; }
        .gv-pm-table th { background:#f7f4fb; color:#6b5288; }
        .gv-pm-table input[type=number] { width:84px; max-width:100%; border:1px solid #e1d6ed; border-radius:6px; padding:5px; }
        .gv-pm-table input[type=text] { width:145px; max-width:100%; border:1px solid #e1d6ed; border-radius:6px; padding:5px; }
        .gv-confirm-body { padding:6px; color:#4e445a; font-size:12.5px; }
        .gv-confirm-warning { background:#fff6e7; color:#936012; border:1px solid #f6d69b; padding:10px; border-radius:8px; }
        .gv-modal-buttons { gap:7px; flex-wrap:wrap; }
        .gv-modal-buttons .gv-action { min-width:152px; }
        .gv-cross-filter { display:inline-flex; gap:3px; border:1px solid #e7dced; background:#f8f5fc; border-radius:9px; padding:3px; margin:13px 0 8px; }
        .gv-cross-filter button { padding:6px 14px; border:1px solid transparent; border-radius:7px; color:#695683; background:transparent; font-size:11px; font-weight:750; cursor:pointer; }
        .gv-cross-filter button.is-selected { background:#fff; color:#53248b; border-color:#e2d3f2; box-shadow:0 1px 3px #32134810; }
        .gv-cross-title { margin:9px 0 7px; color:#533080; font-size:12.5px; font-weight:800; }
        .gv-type-tag { display:inline-block; padding:2px 6px; border:1px solid #cfe5fb; border-radius:5px; background:#f1f8ff; color:#246eac; font-size:10px; }
        .gv-type-tag.is-issue { border-color:#e7d6f5; color:#6b3094; background:#f9f2ff; }
        .gv-cross-list { display:flex; flex-direction:column; gap:7px; }
        .gv-cross-item { display:flex; justify-content:space-between; align-items:center; gap:10px; padding:11px 13px; background:#fff; border:1px solid #ece5f4; border-radius:8px; font-size:12px; }
        .gv-cross-item strong { color:#4b2d73; }
        .gv-cross-item small { display:block; color:#7f8798; margin-top:4px; }
        .gv-cross-item button { border:1px solid #e2d5f1; background:#f8f2ff; color:#65378f; padding:6px 11px; border-radius:6px; font-size:11px; cursor:pointer; }
        #gvHistoryIssueTitle[hidden],#gvHistoryTaskTitle[hidden],#gvLedgerIssueTitle[hidden],#gvLedgerTaskTitle[hidden],#gvHistoryIssues[hidden],#gvLedgerIssues[hidden],#kbg20History[hidden],#kbg20ManualList[hidden] { display:none!important; }
        /* Dong nhat Issue voi Soft Table V4.1 Task; moi selector/moi khai bao tren mot dong. */
        .gv-issue { border:0; border-radius:0; background:transparent; overflow:visible; }
        .gv-workbar { justify-content:space-between; flex-wrap:wrap; gap:9px; padding:8px 10px; }
        .gv-workbar-summary { display:flex; align-items:flex-start; flex-direction:column; flex:0 0 auto; gap:3px; min-width:120px; }
        .gv-workbar-summary strong { font-size:12px; color:#56258e; font-weight:800; }
        .gv-workbar-summary small { font-size:10.5px; color:#8b93a2; }
        .gv-workbar-tools { min-width:0; flex:1 1 600px; justify-content:flex-end; }
        #gvFilters .kbg20-filter-chip { white-space:nowrap; }
        .gv-tasklike-table { overflow-x:auto; }
        .gv-tasklike-grid { display:grid; grid-template-columns:minmax(205px,2.15fr) minmax(98px,.9fr) minmax(128px,1.04fr) minmax(122px,1.08fr) minmax(122px,1.08fr) minmax(112px,.95fr) minmax(155px,1.25fr); align-items:center; gap:7px; min-width:1040px; padding:0 12px; }
        .gv-tasklike-rows { min-width:1040px; }
        .gv-tasklike-rows .gv-tasklike-grid { min-height:76px; border-bottom:1px solid #e9e8f0; background:#fff; padding-top:9px; padding-bottom:9px; }
        .gv-tasklike-rows .gv-tasklike-grid:hover { background:#fbf9ff; }
        .gv-tasklike-rows .gv-tasklike-grid > div:not(:first-child) { text-align:center; }
        .gv-tasklike-identity { display:flex; flex-direction:column; min-width:0; gap:5px; }
        .gv-tasklike-name { display:flex; align-items:center; gap:6px; min-width:0; }
        .gv-tasklike-name strong { color:#334155; font-size:12px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
        .gv-tasklike-note { font-size:10.5px; color:#93a0ae; }
        .gv-tasklike-hours { display:flex; flex-direction:column; align-items:center; gap:3px; font-variant-numeric:tabular-nums; }
        .gv-tasklike-hours strong { font-size:14px; font-weight:800; color:#53268d; }
        .gv-tasklike-hours.is-overtime strong { color:#73518f; }
        .gv-tasklike-hours small { font-size:10px; color:#8b94a4; }
        .gv-tasklike-clock { display:flex; justify-content:center; gap:4px; align-items:center; }
        .gv-tasklike-clock .gv-timer-button { display:inline-flex; justify-content:center; align-items:center; min-width:27px; height:28px; padding:0 6px; border:1px solid #e1d4ef; border-radius:7px; background:#f7f1ff; color:#64309b; cursor:pointer; }
        .gv-tasklike-clock .gv-timer-button.is-stop { border-color:#f0c7d1; background:#fff0f3; color:#b3394a; }
        .gv-tasklike-clock .gv-timer-button:disabled { opacity:.42; cursor:not-allowed; }
        .gv-row-actions { display:flex; justify-content:center; flex-wrap:wrap; gap:5px; }
        .gv-row-actions .kbg20-btn { font-size:11px; padding:6px 8px; min-height:30px; white-space:nowrap; }
        .gv-workfoot { display:flex; justify-content:space-between; align-items:center; padding:10px 12px; }
        .gv-pager { display:inline-flex; align-items:center; justify-content:center; padding:0; min-width:30px; height:30px; }
        .gv-tasklike-grid .gv-state { font-size:10.5px; }
        .gv-history-grid { display:grid; grid-template-columns:minmax(235px,2.2fr) repeat(3,minmax(120px,1fr)) minmax(100px,.9fr); align-items:center; gap:10px; }
        .gv-history-row { min-height:56px; padding:9px 13px; border-bottom:1px solid #eeeaf3; background:#fff; }
        .gv-history-row > div:not(:first-child) { text-align:center; font-variant-numeric:tabular-nums; }
        .gv-history-row .kbg20-btn { white-space:nowrap; }
        #gvHistoryIssues .gv-history-head { padding:10px 13px; background:#f8f5fb; color:#6f5885; font-size:10.5px; font-weight:800; }
        .gv-issue-modal-head { display:flex; align-items:center; justify-content:space-between; gap:10px; margin-bottom:12px; }
        .gv-issue-modal-head strong { color:#522a88; font-size:15px; }
        .gv-issue-modal-head span { color:#778399; font-size:11px; }
        .gv-issue-form { display:flex; flex-direction:column; gap:12px; }
        .gv-issue-form .kbg20-reconcile-grid { display:grid; grid-template-columns:repeat(2,minmax(0,1fr)); gap:10px; }
        .gv-issue-form input,.gv-issue-form textarea { box-sizing:border-box; }
        .gv-issue-inline-action { display:flex; justify-content:flex-end; flex-wrap:wrap; gap:8px; margin-top:10px; }
        .gv-issue-section { padding:12px; border:1px solid #ece4f4; border-radius:9px; background:#fff; }
        .gv-issue-section h4 { margin:0 0 9px; color:#643792; font-size:12px; }
        .gv-issue-pm-table { width:100%; border-collapse:collapse; font-size:11px; }
        .gv-issue-pm-table th,.gv-issue-pm-table td { border-bottom:1px solid #eee7f6; padding:8px; text-align:left; }
        .gv-issue-pm-table input { max-width:100px; width:100%; padding:5px; border:1px solid #dad5e6; border-radius:6px; }
        .gv-issue-people { display:flex; flex-direction:column; gap:5px; max-height:180px; overflow-y:auto; }
        .gv-issue-person { display:flex; align-items:center; justify-content:space-between; gap:8px; padding:7px 8px; border-bottom:1px solid #eee8f4; }
        .gv-issue-hist-item { border:1px solid #eee8f4; border-radius:8px; padding:10px; margin-top:8px; background:#fff; }
        .gv-issue-hist-item h4 { margin:0 0 7px; color:#5e3986; font-size:11.5px; }
        .gv-issue-hist-fields { display:grid; grid-template-columns:repeat(3,minmax(0,1fr)); gap:10px; }
        .gv-issue-hist-fields small { display:block; font-size:10px; color:#8c91a0; margin-bottom:3px; }
        .gv-issue-hist-fields strong { font-size:12px; color:#344154; }
        #gvHistoryTotalsCombined[hidden] { display:none!important; }
        @media(max-width:1000px) { .gv-workbar-tools { justify-content:flex-start; } .gv-tasklike-table { max-width:100%; } }
        @media(max-width:670px) { .gv-issue-form .kbg20-reconcile-grid { grid-template-columns:1fr; } .gv-issue-hist-fields { grid-template-columns:1fr; } }
        @media(max-width:800px) { .gv-form-grid { grid-template-columns:1fr; } .gv-modal-stats { grid-template-columns:repeat(2,minmax(0,1fr)); } .gv-cross-item { flex-wrap:wrap; } }
</style>
    <style>
        .ks-page.is-project-scoped { max-width:none; width:100%; padding:8px 0 18px; margin:0; }
        .ks-page.is-project-scoped .ks-header { display:none; }
        .kbg-project-shell { min-width:0; }
        .kbg20-feature-off { margin:12px 4px; padding:15px; border:1px solid #dcd4ed; background:#f8f5ff; border-radius:10px; color:#58338a; font-size:13px; }
    </style>
    <style>
    /* Issue dùng đúng các class Task: chỉ định nghĩa 6 cột sau khi bỏ 3 cột ngày. */
    #kbg20Page #gvIssuePane .kbg20-v41-taskgrid { grid-template-columns:minmax(155px,1.36fr) minmax(114px,1fr) minmax(140px,1.12fr) minmax(165px,1.46fr) minmax(165px,1.46fr) minmax(174px,1.40fr)!important;column-gap:8px; }
    #kbg20Page #gvIssuePane .kbg20-v41-actions.is-multi-action { flex-direction:column;gap:5px; }
    /* Ô nhập PM cũng là bộ Giờ/Phút của Task, thu gọn trong bảng duyệt. */
    /* Đồng bộ đúng hành động Task. Issue dùng data-gv-action (không đặt data-action để tránh sự kiện Task bắt nhầm). */
    #kbg20Page #gvIssuePane .kbg20-v41-actions .kbg20-dot2-row-btn[data-gv-action="withdraw"] { background:#fff1f2!important;color:#be123c!important;border:1px solid #fecdd3!important;box-shadow:none; }
    #kbg20Page #gvIssuePane .kbg20-v41-actions .kbg20-dot2-row-btn[data-gv-action="withdraw"]:hover:not(:disabled) { background:#ffe4e6!important;color:#9f1239!important;border-color:#fda4af!important; }
    #kbg20Page #gvIssuePane .kbg20-v41-actions .kbg20-dot2-row-btn[data-gv-action="submit"] { background:#f6f0ff!important;color:#59228f!important;border:1px solid #d7bfed!important;box-shadow:none; }
    #kbg20Page #gvIssuePane .kbg20-v41-actions .kbg20-dot2-row-btn[data-gv-action="submit"]:hover:not(:disabled) { background:#ede1fc!important;color:#491a7c!important;border-color:#ba99e0!important; }
    #kbg20Page #gvIssuePane .kbg20-v41-identity,#kbg20Page #gvIssuePane .kbg20-v41-taskname { min-width:0;max-width:100%; }
    #kbg20Page #gvIssuePane .kbg20-v41-taskname { display:flex;width:100%; }
    #kbg20Page #gvIssuePane .kbg20-v41-tasktext { flex:1 1 auto;min-width:0;max-width:100%;display:block;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;cursor:help; }
    /* Đầu cột, giờ hệ thống/tự khai và hành động đều nằm giữa cột tương ứng ở cả Task/Issue Tab 3. */
    #kbg20Page #kbg20ManualList .kbg20-ui-table-grid,#kbg20Page #gvLedgerIssues .kbg20-ui-table-grid { grid-template-columns:minmax(215px,2.45fr) minmax(145px,1.10fr) minmax(145px,1.10fr) minmax(155px,1.20fr) minmax(145px,1.15fr);column-gap:9px; }
    #kbg20Page #kbg20ManualList .kbg20-ui-table-head>span:not(:first-child),#kbg20Page #gvLedgerIssues .kbg20-ui-table-head>span:not(:first-child) { text-align:center; }
    #kbg20Page #kbg20ManualList .kbg20-ui-cell-sys,#kbg20Page #kbg20ManualList .kbg20-ui-cell-self,#kbg20Page #gvLedgerIssues .kbg20-ui-cell-sys,#kbg20Page #gvLedgerIssues .kbg20-ui-cell-self { display:grid;grid-template-columns:64px minmax(62px,max-content);align-items:baseline;justify-content:center;column-gap:8px; }
    #kbg20Page #kbg20ManualList .kbg20-ui-cell-sys span,#kbg20Page #kbg20ManualList .kbg20-ui-cell-self span,#kbg20Page #gvLedgerIssues .kbg20-ui-cell-sys span,#kbg20Page #gvLedgerIssues .kbg20-ui-cell-self span { text-align:right; }
    #kbg20Page #kbg20ManualList .kbg20-ui-cell-sys strong,#kbg20Page #kbg20ManualList .kbg20-ui-cell-self strong,#kbg20Page #gvLedgerIssues .kbg20-ui-cell-sys strong,#kbg20Page #gvLedgerIssues .kbg20-ui-cell-self strong { text-align:right; }
    #kbg20Page #kbg20ManualList .kbg20-ui-cell-status,#kbg20Page #gvLedgerIssues .kbg20-ui-cell-status { align-items:center;text-align:center; }
    #kbg20Page #kbg20ManualList .kbg20-ui-cell-actions,#kbg20Page #gvLedgerIssues .kbg20-ui-cell-actions { justify-content:center; }

    /* Chỉ Issue Tab 3: các nút dùng đúng bộ is-primary/is-secondary/is-neutral của Task.
       Nút Rút báo vẫn hiện nhưng khóa khi chờ/đã duyệt, theo giới hạn của server. */
    #kbg20Page #gvLedgerIssues .kbg20-ui-cell-actions { gap:7px; flex-wrap:wrap; }
    #kbg20Page #gvLedgerIssues .gv-ledger-action-lock { display:inline-flex; align-items:center; justify-content:center; border-radius:7px; cursor:help; }
    #kbg20Page #gvLedgerIssues .gv-ledger-action-lock .kbg20-ui-action:disabled { opacity:.46;cursor:not-allowed;filter:none;pointer-events:none; }

    /* Task 9 cột ẩn ba cột ngày trên tablet; Issue 6 cột KHÔNG có ba cột ngày đó. */
    @media(max-width:1160px) { #kbg20Page #gvIssuePane .kbg20-v41-taskgrid>:nth-child(2),#kbg20Page #gvIssuePane .kbg20-v41-taskgrid>:nth-child(3),#kbg20Page #gvIssuePane .kbg20-v41-taskgrid>:nth-child(4) { display:flex; } }
    @media(max-width:760px) { #kbg20Page #gvIssuePane .kbg20-v41-table-head { display:none!important; } #kbg20Page #gvIssuePane .gv-tasklike-rows { min-width:0; } #kbg20Page #gvIssuePane .kbg20-v41-task-row { grid-template-columns:repeat(2,minmax(0,1fr))!important;min-width:0; } #kbg20Page #gvIssuePane .kbg20-v41-task-row>:nth-child(6) { grid-column:1/-1;justify-content:center; } #kbg20Page #gvIssuePane .kbg20-v41-task-row .kbg20-v41-taskname { max-width:100%; } }
    #mdlKbg20Manual .gv-issue-workspace { min-height:410px; }
    #mdlKbg20HistoryDetails .gv-issue-session .kbg20-ux-session-grid { grid-template-columns:repeat(4,minmax(0,1fr)); }
    @media(max-width:1190px) and (min-width:781px) { #kbg20Page #kbg20ManualList .kbg20-ui-table-grid,#kbg20Page #gvLedgerIssues .kbg20-ui-table-grid { grid-template-columns:minmax(180px,2.2fr) minmax(132px,1fr) minmax(132px,1fr) minmax(145px,1fr); } #kbg20Page #kbg20ManualList .kbg20-ui-table-grid>:nth-child(4),#kbg20Page #gvLedgerIssues .kbg20-ui-table-grid>:nth-child(4) { display:none; } }
    @media(max-width:780px) { #kbg20Page #kbg20ManualList .kbg20-ui-table-grid,#kbg20Page #gvLedgerIssues .kbg20-ui-table-grid { grid-template-columns:repeat(2,minmax(0,1fr)); } #kbg20Page #kbg20ManualList .kbg20-ui-cell-actions,#kbg20Page #gvLedgerIssues .kbg20-ui-cell-actions { justify-content:flex-start; } #mdlKbg20Manual .gv-issue-workspace { min-height:0; } #mdlKbg20HistoryDetails .gv-issue-session .kbg20-ux-session-grid { grid-template-columns:repeat(2,minmax(0,1fr)); } }
    #mdlKbg20Manual .gv-issue-pm .kbg20-split-heading,#mdlKbg20Manual .gv-issue-pm .kbg20-split-preview { display:none; }
    #mdlKbg20Manual .gv-issue-pm .kbg20-split-time { gap:3px; }
    #mdlKbg20Manual .gv-issue-pm .kbg20-split-row { gap:5px; }
    #mdlKbg20Manual .gv-issue-pm .kbg20-split-minutes { flex:0 0 48px; }
    #mdlKbg20Manual .gv-issue-pm .kbg20-split-input { height:34px;padding:5px 4px;font-size:13px; }
    #mdlKbg20Manual .gv-issue-pm, #mdlKbg20Manual .gv-issue-review { padding:12px 14px; }
    #mdlKbg20Manual .gv-issue-review-actions { display:flex;justify-content:flex-end;gap:8px;padding:12px 0 0; }
    #mdlKbg20Manual .gv-issue-review .kbg20-ui-reason { display:block;max-width:100%; }
    #mdlKbg20Manual .gv-issue-review .kbg20-ui-reason textarea { width:100%; }
    @media(max-width:1160px) { #kbg20Page #gvIssuePane .kbg20-v41-taskgrid { grid-template-columns:minmax(152px,1.36fr) minmax(100px,1fr) minmax(112px,1.1fr) minmax(134px,1.35fr) minmax(134px,1.35fr) minmax(145px,1.35fr)!important; } }
    @media(max-width:1160px) { #kbg20Page #gvIssuePane .kbg20-v41-task-row>:nth-child(2),#kbg20Page #gvIssuePane .kbg20-v41-task-row>:nth-child(3),#kbg20Page #gvIssuePane .kbg20-v41-task-row>:nth-child(4) { display:flex!important; } #kbg20Page #gvIssuePane .kbg20-v41-table-head>:nth-child(2),#kbg20Page #gvIssuePane .kbg20-v41-table-head>:nth-child(3),#kbg20Page #gvIssuePane .kbg20-v41-table-head>:nth-child(4){display:block!important;} }
    @media(max-width:760px) { #kbg20Page #gvIssuePane .kbg20-v41-table-head{display:none;} #kbg20Page #gvIssuePane .kbg20-v41-task-row {grid-template-columns:repeat(2,minmax(0,1fr))!important;} #kbg20Page #gvIssuePane .kbg20-v41-task-row>:first-child{grid-column:1/-1;} #kbg20Page #gvIssuePane .kbg20-v41-task-row>:nth-child(4),#kbg20Page #gvIssuePane .kbg20-v41-task-row>:nth-child(5){display:flex;grid-column:auto;} #kbg20Page #gvIssuePane .kbg20-v41-task-row>:nth-child(6){display:flex;grid-column:1/-1;} }
    /* CLOCK FIX: place the icon at the same x-coordinate inside both time columns.
       Earlier flex + justify-content:center moved the icon when the text length changed (Task 1.2.1).
       Applies to Task and Issue rows only, preserving the exact Task icon styles and all click handlers. */
    #kbg20Page .kbg20-v41-task-row .kbg20-v41-clock-cell { display:grid;grid-template-columns:29px minmax(0,1fr);column-gap:7px;align-items:center;justify-content:stretch;width:100%;min-width:0; }
    #kbg20Page .kbg20-v41-task-row .kbg20-v41-icon-action { grid-column:1;grid-row:1;width:29px;min-width:29px;height:30px;justify-self:start;align-self:center; }
    #kbg20Page .kbg20-v41-task-row .kbg20-v41-clock-main { grid-column:2;grid-row:1;display:flex;flex-direction:column;justify-self:stretch;align-items:flex-start;text-align:left;min-width:0;width:100%;max-width:100%; }
    #kbg20Page .kbg20-v41-task-row .kbg20-v41-clock { white-space:nowrap;font-variant-numeric:tabular-nums; }
    #kbg20Page #gvIssuePane .gv-issue-approval-note { display:block;margin-top:3px;font-size:10px;line-height:1.3;color:#80629c;text-align:center; }
    @media(max-width:760px) { #kbg20Page .kbg20-v41-task-row .kbg20-v41-clock-cell>.kbg20-v41-mobile-label { grid-column:1/-1;grid-row:auto; } #kbg20Page .kbg20-v41-task-row .kbg20-v41-icon-action,#kbg20Page .kbg20-v41-task-row .kbg20-v41-clock-main { grid-row:auto; } #kbg20Page #gvIssuePane .gv-issue-approval-note { text-align:left; } }
    </style>

<style>
/* PM styles preserved from the running page; Task CSS untouched. */
.qlpm-shell{--qlpm-line:#e7e2ed;display:flex;flex-direction:column;min-height:0;max-height:calc(100dvh - 141px);font-size:12.5px;color:#263246;background:#fff;overflow:hidden}
.qlpm-overview{flex:none;padding:11px 17px;background:linear-gradient(110deg,#fbf8ff,#fff);border-bottom:1px solid var(--qlpm-line)}
.qlpm-task-title{font-size:15px;font-weight:800;color:#33224e;line-height:1.35;overflow-wrap:anywhere;margin:0 0 6px}
.qlpm-context{display:flex;align-items:center;gap:7px;flex-wrap:wrap}
.qlpm-task-overdue{font-size:10.5px;font-weight:750;color:#dc2626}
.qlpm-shell .badge-pill-custom{padding:4px 8px!important;border-radius:4px!important;font-size:11px!important;font-weight:600!important;display:inline-block!important;white-space:nowrap!important;line-height:1.2!important}
.qlpm-shell .badge-status-todo{background:#f1f5f9!important;color:#475569!important;border:1px solid #cbd5e1!important}
.qlpm-shell .badge-status-doing{background:#e0f2fe!important;color:#0369a1!important;border:1px solid #bae6fd!important}
.qlpm-shell .badge-status-done{background:#dcfce7!important;color:#15803d!important;border:1px solid #bbf7d0!important}
.qlpm-shell .badge-status-review{background:#fef3c7!important;color:#92400e!important;border:1px solid #fcd34d!important}
.qlpm-pm-status{display:inline-flex;align-items:center;gap:5px;padding:3px 8px;font-size:10.5px;font-weight:750;color:#68408c;border:1px solid #e8d8f4;background:#f6effd;border-radius:5px}
.qlpm-pm-status.is-final{color:#13784b;background:#eefaf2;border-color:#c2e8d4}
.qlpm-count{font-size:11.5px;font-weight:650;color:#718096}
.qlpm-summary{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:8px;margin-top:10px}
.qlpm-stat{display:flex;align-items:center;justify-content:space-between;gap:8px;min-width:0;background:#fff;border:1px solid #ece6f3;border-radius:8px;padding:8px 12px}
.qlpm-stat small{font-size:11px;font-weight:650;color:#727987}
.qlpm-stat strong{font-size:16px;font-weight:800;color:#57228e;font-variant-numeric:tabular-nums;white-space:nowrap}
.qlpm-alert{flex:none;margin:8px 15px 2px;border-radius:7px;padding:8px 10px;background:#fff8e7;border:1px solid #efd9a7;color:#805917;font-size:11.5px;font-weight:600}
.qlpm-list-title{flex:none;display:flex;align-items:center;justify-content:space-between;gap:10px;padding:9px 17px 5px;color:#4a2774;font-weight:800;font-size:12.5px}
.qlpm-list-actions{display:flex;align-items:center;gap:8px;color:#748093;font-size:10.5px;font-weight:500}
.qlpm-collapse-all{border:1px solid #dcd0ea;border-radius:6px;padding:4px 8px;background:#fff;color:#653294;font-size:11px;font-weight:700;cursor:pointer}
.qlpm-collapse-all:hover{background:#f6effd;border-color:#b696d7}
.qlpm-list{flex:1 1 auto;min-height:100px;overflow-y:auto;overscroll-behavior:contain;padding:0 14px 11px;scrollbar-width:thin;scrollbar-color:#d6c6e7 transparent}
.qlpm-employee-grid{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));align-items:start;gap:9px}
.qlpm-employee{min-width:0;border:1px solid #e8e1f0;border-radius:10px;overflow:hidden;background:#fff;transition:border-color .18s ease,box-shadow .18s ease}
.qlpm-employee:hover{border-color:#d8c5ec}
.qlpm-employee.is-open{border-color:#cbb3e8;box-shadow:0 3px 10px rgba(74,35,119,.045)}
.qlpm-employee-head{display:flex;align-items:center;gap:9px;width:100%;text-align:left;padding:10px 11px;border:0;background:#f9f7fc;cursor:pointer;font:inherit;color:inherit}
.qlpm-employee-head:hover{background:#f4eef9}
.qlpm-avatar-wrap{position:relative;width:36px;height:36px;flex:0 0 36px}
.qlpm-avatar-image,.qlpm-avatar-fallback{width:36px;height:36px;border-radius:50%;display:flex;align-items:center;justify-content:center;font-size:11px;font-weight:800;color:#fff;object-fit:cover}
.qlpm-avatar-image{position:absolute;inset:0;background:#fff}
.qlpm-person{display:flex;flex-direction:column;flex:1;min-width:0;gap:1px}
.qlpm-person strong{font-size:12px;font-weight:800;color:#2e3545;line-height:1.25;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.qlpm-person small{color:#7b8291;font-size:10.5px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.qlpm-person-meta{display:flex;align-items:center;flex-wrap:wrap;gap:5px;margin-top:1px}
.qlpm-person-role{color:#61738b;font-size:10px;max-width:150px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
.qlpm-person-pm{color:#b17a00;font-weight:800;font-size:10px;border:1px solid #f3db9b;border-radius:5px;background:#fff7df;padding:1px 5px}
.qlpm-person-state{font-size:9.5px;color:#a35d10;border:1px solid #f6dfac;background:#fff9ec;padding:2px 5px;border-radius:5px;white-space:nowrap}
.qlpm-person-state.is-reported{color:#16784f;background:#eefbf4;border-color:#c8ecd8}
.qlpm-head-right{display:flex;flex-direction:column;align-items:flex-end;gap:4px;flex:none;max-width:40%;color:#806b95;font-size:10px}
.qlpm-card-summary{font-size:10px;color:#6d597f;font-weight:650;white-space:nowrap;font-variant-numeric:tabular-nums}
.qlpm-chevron{font-size:12px;transition:transform .15s ease}
.qlpm-employee.is-open .qlpm-chevron{transform:rotate(180deg)}
.qlpm-employee-body{display:none}
.qlpm-employee.is-open .qlpm-employee-body{display:block}
.qlpm-compare{border-collapse:collapse;width:100%;table-layout:fixed}
.qlpm-compare td,.qlpm-compare th{border-bottom:1px solid #f1edf6;padding:6px 10px;vertical-align:middle}
.qlpm-compare th{color:#778197;font-size:10.5px;font-weight:700;text-align:left;background:#fff}
.qlpm-compare .qlpm-label{width:32%;color:#5c6472;font-size:10.5px;font-weight:680}
.qlpm-compare td{font-size:11.5px;color:#354052;font-variant-numeric:tabular-nums}
.qlpm-compare .qlpm-edit-row{background:#faf6ff}
.qlpm-compare .qlpm-edit-row td{border-bottom:0}
.qlpm-time-group{display:flex;align-items:flex-end;gap:6px;max-width:230px}
.qlpm-part{display:flex;flex-direction:column-reverse;gap:3px;flex:1;min-width:0;color:#776884;font-size:9.5px;font-weight:700}
.qlpm-part span{padding-left:2px}
.qlpm-minutes-part{flex:0 0 54px}
.qlpm-hours{box-sizing:border-box!important;min-width:0;width:100%!important;max-width:none!important;height:34px!important;min-height:34px!important;padding:4px 6px!important;font-size:14px!important;font-family:inherit!important;font-weight:750!important;text-align:center!important;font-variant-numeric:tabular-nums!important;color:#4f237f!important;border:1px solid #ceb7e9!important;border-radius:7px!important;background:#fff!important;outline-offset:2px}
.qlpm-hours:focus{border-color:#8051b2!important;box-shadow:0 0 0 2px rgba(120,73,171,.14)!important}
.qlpm-hours.is-invalid{border-color:#dc3545!important;background:#fff7f7!important}
.qlpm-hours[readonly]{background:#f1f2f5!important;border-color:#e1e1e6!important;color:#596276!important}
.qlpm-reason{padding:7px 10px 9px}
.qlpm-reason label{display:block;font-size:10.5px;font-weight:700;color:#657087;margin-bottom:4px}
.qlpm-reason textarea{min-height:37px!important;max-height:100px!important;resize:vertical!important;font-size:11.5px!important;border-color:#e1d6ef!important;border-radius:7px!important}
.qlpm-hint{flex:none;padding:7px 15px;background:#faf7fd;color:#786c82;border-top:1px solid #f0eaf4;font-size:10px;line-height:1.4}
.qlpm-footer-total{font-size:11px;font-weight:650;color:#768196;margin-right:auto}
@media(max-width:900px){.qlpm-employee-grid{grid-template-columns:1fr}.qlpm-person-state{display:inline-block}.qlpm-list-actions span{display:none}}
#mdlKbg20Manual .gv-issue-pm .qlpm-shell { max-height:70vh; }
#mdlKbg20Manual .gv-issue-pm.qlpm-shell { max-height:65vh;overflow-y:auto; }
#mdlKbg20Manual .gv-issue-pm .qlpm-overview { margin-bottom:8px; }
#mdlKbg20Manual .gv-issue-pm .qlpm-compare .qlpm-hours { max-width:105px!important; }
#mdlKbg20Manual .gv-issue-pm .qlpm-employee-grid { grid-template-columns:repeat(2,minmax(0,1fr)); }
@media(max-width:840px){#mdlKbg20Manual .gv-issue-pm .qlpm-employee-grid { grid-template-columns:1fr; }}
</style>
    <link href="<%= ResolveUrl("~/Styles/css/kbg20-page.css") %>?v=20261027task-popup-label" rel="stylesheet" type="text/css" />
    <%-- KBG20: Styles scoped to name/approval controls; CSS goc giu nguyen. --%>
    <style type="text/css">
        /* Chỉ thêm các quy tắc dưới đây vào CUỐI file kbg20-page.css hiện tại. Không thay toàn bộ CSS. */
        /* Tên công việc co giãn nhưng không tràn; tooltip dùng hệ thống JS đang có. */
        @media (min-width:1161px) { .kbg20-page .kbg20-v41-taskgrid { grid-template-columns:minmax(165px,2.2fr) minmax(70px,.68fr) minmax(56px,.53fr) minmax(83px,.78fr) minmax(103px,.98fr) minmax(123px,1.14fr) minmax(146px,1.42fr) minmax(129px,1.2fr) minmax(172px,1.25fr); column-gap:7px; } }
        .kbg20-page .kbg20-v41-identity, .kbg20-page .kbg20-v41-taskname, .kbg20-page .kbg20-v41-tasktext { min-width:0; }
        .kbg20-page .kbg20-v41-taskname { display:flex; align-items:center; gap:5px; flex-wrap:nowrap; width:max-content; max-width:100%; }
        .kbg20-page .kbg20-v41-taskcode { flex:0 0 auto; }
        .kbg20-page .kbg20-v41-tasktext { flex:0 1 auto; display:block; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; overflow-wrap:normal; cursor:help; }
        .kbg20-page .kbg20-v41-taskname .kbg20-d6-badge { flex:0 0 auto; }
        /* Khôi phục badge cột Báo cáo phần việc, không thay màu Trạng thái công việc. */
        .kbg20-page .kbg20-v41-personal-cell { display:flex; align-items:center; justify-content:center; min-width:0; }
        .kbg20-page .kbg20-v41-personal-cell .kbg20-dot2-personal { display:inline-flex; align-items:center; justify-content:center; text-align:center; max-width:100%; padding:5px 8px; border:1px solid #dfe3e9; border-radius:7px; background:#f6f7fa; color:#657085; font-size:10.4px; line-height:1.25; font-weight:650; }
        .kbg20-page .kbg20-v41-personal-cell .kbg20-dot2-personal.is-reported { background:#edf8ef; border-color:#bce7c9; color:#187347; }
        .kbg20-page .kbg20-v41-personal-cell .kbg20-dot2-personal.is-withdrawn { background:#fff4f4; border-color:#f2d1d1; color:#ab4848; }
        .kbg20-page .kbg20-v41-personal-cell .kbg20-dot2-personal.is-not-reported { background:#f6f7fa; border-color:#e1e3e9; color:#677085; }
        /* Giữ nút Báo hoàn thành một dòng, không tự kéo chiều cao hàng; hai hành động sau báo tách gọn. */
        .kbg20-page .kbg20-v41-actions { min-width:0; width:100%; display:flex; align-items:center; justify-content:center; }
        .kbg20-page .kbg20-v41-actions .kbg20-dot2-row-btn { flex:0 0 auto; display:inline-flex; align-items:center; justify-content:center; box-sizing:border-box; min-width:0; width:164px; max-width:100%; min-height:31px; padding:6px 8px; border-radius:7px; font-size:10.5px; font-weight:750; letter-spacing:-.12px; line-height:1.2; white-space:nowrap; text-align:center; box-shadow:none; }
        .kbg20-page .kbg20-v41-actions.is-multi-action { display:flex; flex-direction:column; align-items:center; gap:5px; }
        .kbg20-page .kbg20-v41-clock-cell { min-width:0; }
        .kbg20-page .kbg20-v41-clock-sub { display:block; min-width:0; max-width:100%; white-space:nowrap; overflow:hidden; text-overflow:ellipsis; }
        .kbg20-page .kbg20-v41-actions .kbg20-dot2-row-btn[data-action="withdrawPersonal"] { background:#fff1f2; color:#be123c; border:1px solid #fecdd3; }
        .kbg20-page .kbg20-v41-actions .kbg20-dot2-row-btn[data-action="withdrawPersonal"]:hover:not(:disabled) { background:#ffe4e6; border-color:#fda4af; }
        .kbg20-page .kbg20-v41-actions .kbg20-dot2-row-btn[data-action="sendTaskApproval"] { background:#f6f0ff; color:#59228f; border:1px solid #d7bfed; }
        .kbg20-page .kbg20-v41-actions .kbg20-dot2-row-btn[data-action="sendTaskApproval"]:hover:not(:disabled) { background:#ede1fc; border-color:#ba99e0; }
        @media (max-width:760px) { .kbg20-page .kbg20-v41-actions .kbg20-dot2-row-btn { white-space:normal; width:164px; max-width:100%; } }
        /* CHỈ popup chi tiết các lần tính giờ: tự co theo nội dung ngắn. */
        #mdlKbg20HistoryDetails .modal-dialog, #mdlKbg20HistoryDetails.modal-dialog { width:calc(100vw - 40px)!important; max-width:930px!important; margin-left:auto!important; margin-right:auto!important; }
        #mdlKbg20HistoryDetails .modal-content { min-height:0!important; height:auto!important; }
        #mdlKbg20HistoryDetails .modal-body { min-height:0!important; height:auto!important; max-height:calc(100vh - 180px); overflow-y:auto; }
        #mdlKbg20HistoryDetails .kbg20-ux-modal-content { min-height:0; padding:10px 12px; }
        #mdlKbg20HistoryDetails .kbg20-ux-modal-title { padding-bottom:9px; margin-bottom:9px; }
        #mdlKbg20HistoryDetails .kbg20-ux-session-list { max-height:min(48vh,400px); }
        #mdlKbg20HistoryDetails .kbg20-ux-session-item { padding:10px 12px; }
        #mdlKbg20HistoryDetails .kbg20-ux-session-grid { grid-template-columns:repeat(4,minmax(0,1fr)); gap:10px; }
        @media (max-width:760px) { #mdlKbg20HistoryDetails .kbg20-ux-session-grid { grid-template-columns:repeat(2,minmax(0,1fr)); } }
        @media (max-width:480px) { #mdlKbg20HistoryDetails .kbg20-ux-session-grid { grid-template-columns:minmax(0,1fr); } }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <%-- Chi trang theo du an moi co breadcrumb/tab. Trang V1 ngoai du an giu bo cuc cu. --%>
    <% if (CurrentProjectId != Guid.Empty) { %>
    <div class="row"><div class="col-xl-12"><div class="card p-2 kbg-project-shell">
        <SweetSoft:Navigation runat="server" ID="NavigationKbg" />
        <SweetSoft:CtrlProjectTabs runat="server" ID="ProjectTabsKbg" />
    <% } %>
    <div class="ks-page <%= CurrentProjectId != Guid.Empty ? "is-project-scoped" : "" %>">

        <%-- V1: greeting / dashboard link; V2 has a separate compact header. --%>
        <% if (!LaGiaoDienKbg20) { %>
        <div class="ks-header">
            <div class="ks-header-left">
                <div class="ks-avatar"><asp:Literal ID="ltrInitial" runat="server"></asp:Literal></div>
                <div>
                    <div class="ks-hello">Xin chào, <asp:Literal ID="ltrName" runat="server"></asp:Literal></div>
                    <div class="ks-subtitle"><asp:Literal ID="ltrDateText" runat="server"></asp:Literal> · Giờ hệ thống: <b id="ksClock">--:--:--</b></div>
                </div>
            </div>
            <a id="lnkDashboard" runat="server" class="ks-link"><i class="fas fa-chart-pie me-1"></i>Xem thống kê tổng quan</a>
        </div>
        <% } %>

        <asp:Panel ID="pnlNotEmployee" runat="server" Visible="false" CssClass="ks-empty">
            <i class="fas fa-user-slash"></i>
            <b>Tài khoản này không phải tài khoản nhân viên.</b><br />
            Trang bấm giờ làm việc chỉ dành cho nhân viên.
        </asp:Panel>

        <% if (CurrentProjectId != Guid.Empty && !KhaiBaoGioManager.KhaiBaoGio20DaKichHoat()) { %>
        <div class="kbg20-feature-off" role="status">
            <i class="fas fa-info-circle" aria-hidden="true"></i>
            Chức năng Tính giờ công việc 2.0 chưa được kích hoạt trên môi trường này.
            Vui lòng kiểm tra cấu hình KhaiBaoGio20.KichHoat trước khi thao tác.
        </div>
        <% } %>

        <%-- V2 chi hien trong project da giai ma duoc; V1 ngoai du an khong bi sua. --%>
        <% if (LaGiaoDienKbg20 && !pnlNotEmployee.Visible) { %>
        <%-- Bridge server-side: postback co WebForms ViewState/EventValidation. KBG20 V1 khong goi. --%>
        <input type="hidden" id="kbg20FlowTask" name="kbg20FlowTask" value="" />
        <input type="hidden" id="kbg20FlowAction" name="kbg20FlowAction" value="" />
        <input type="hidden" id="kbg20FlowOverdue" name="kbg20FlowOverdue" value="0" />
        <input type="hidden" id="kbg20FlowType" name="kbg20FlowType" value="regular" />
        <input type="hidden" id="kbg20FlowStopIds" name="kbg20FlowStopIds" value="" />
        <input type="hidden" id="kbg20FlowEligibleIds" name="kbg20FlowEligibleIds" value="" />
        <%-- Tách bridge của KBG20 thành một UpdatePanel, giống luồng postback
             của CtrlTask bên partner. Không thực hiện full-page postback khi mở popup. --%>
        <asp:UpdatePanel ID="upnlKbg20WorkflowBridge" runat="server" UpdateMode="Conditional">
          <ContentTemplate>
            <asp:LinkButton ID="btnKbg20Workflow" runat="server" CssClass="ks-hidden"
              CausesValidation="false" OnClick="btnKbg20Workflow_Click" style="display:none">Mở xử lý công việc</asp:LinkButton>
          </ContentTemplate>
        </asp:UpdatePanel>
        <script type="text/javascript">
            // Bridge gọi popup Task qua UpdatePanel. Không gửi chồng nhiều async postback:
            // ASP.NET AJAX có thể hủy request cũ hoặc bỏ click mới mà không có feedback.
            (function () {
                var opening = false;
                function showWorkflowMessage(text, isError) {
                    var el = document.getElementById('kbg20Message');
                    if (!el) { if (isError && window.console) console.error(text); return; }
                    el.textContent = text;
                    el.className = isError ? 'kbg20-error' : '';
                }
                function requestManager() {
                    return window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager
                        ? Sys.WebForms.PageRequestManager.getInstance() : null;
                }
                var hookedManager = null;
                function hookEndRequest(manager) {
                    if (!manager || hookedManager === manager) return;
                    manager.add_endRequest(function (sender, args) {
                        if (!opening) return;
                        opening = false;
                        var err = args.get_error && args.get_error();
                        if (err) showWorkflowMessage('Không mở được popup xử lý công việc: ' + err.message, true);
                        // Không đánh dấu lỗi là handled: giữ luồng xử lý lỗi mặc định của site.
                    });
                    hookedManager = manager;
                }
                window.kbg20WorkflowPostback = function (action,taskId,isOverdue,isOvertime,selection) {
                    var manager = requestManager();
                    hookEndRequest(manager);
                    if (opening || (manager && manager.get_isInAsyncPostBack())) {
                        showWorkflowMessage('Đang xử lý yêu cầu trước trên trang. Vui lòng chờ hoàn tất rồi bấm lại.', false);
                        return false;
                    }
                    var trigger = document.querySelector('[id$="btnKbg20Workflow"]');
                    if (!trigger || typeof trigger.click !== 'function') {
                        showWorkflowMessage('Không tìm thấy nút mở popup Task. Vui lòng tải lại trang.', true);
                        return false;
                    }
                    var set = function (id, value) {
                        var el = document.getElementById(id);
                        if (!el) throw new Error('Không tìm thấy trường ' + id);
                        el.value = value;
                    };
                    try {
                        set('kbg20FlowAction', action);
                        set('kbg20FlowTask', taskId);
                        set('kbg20FlowOverdue', isOverdue ? '1' : '0');
                        set('kbg20FlowType', isOvertime ? 'overtime' : 'regular');
                        set('kbg20FlowStopIds', selection && selection.stopIds ? selection.stopIds.join(',') : '');
                        set('kbg20FlowEligibleIds', selection && selection.eligibleIds ? selection.eligibleIds.join(',') : '');
                        opening = true;
                        showWorkflowMessage('Đang mở cửa sổ xử lý công việc…', false);
                        trigger.click();
                        // Nếu browser không sinh async postback, tránh giữ khóa vô thời hạn.
                        window.setTimeout(function () {
                            if (opening && (!manager || !manager.get_isInAsyncPostBack())) {
                                opening = false;
                                showWorkflowMessage('Không nhận được yêu cầu mở popup. Kiểm tra Console/Network rồi thử lại.', true);
                            }
                        }, 1200);
                        return true;
                    } catch (error) {
                        opening = false;
                        showWorkflowMessage('Không thể mở popup xử lý công việc: ' + error.message, true);
                        if (window.console) console.error('KBG20 workflow bridge', error);
                        return false;
                    }
                };
            })();
        </script>
        <section id="kbg20Page" class="kbg20-page"
                 data-api-url="<%= ResolveUrl("~/fKhaiBaoGio/KhaiBaoGio20.ashx") %>"
                 data-kbg20-floating-url="<%= ResolveUrl("~/fKhaiBaoGio/FloatingWorkTimer.ashx") %>"
                 data-kbg20-access-error-url="<%= ResolveUrl("~/fKhaiBaoGio/Kbg20AccessDenied.aspx") %>"
                 data-task-list-url="<%= ResolveUrl(SweetSoft.QLDA.BackOffice.Common.NotificationHelper.BuildNotificationLink(SweetSoft.QLDA.Core.Managers.ThongBaoTypes.CongViec, CurrentProjectId, null)) %>"
                 data-issue-list-url="<%= ResolveUrl(SweetSoft.QLDA.BackOffice.Common.NotificationHelper.BuildNotificationLink(SweetSoft.QLDA.Core.Managers.ThongBaoTypes.VanDe, CurrentProjectId, null)) %>"
                 data-project-id="<%= CurrentProjectId.ToString("D") %>"
                 data-gv-api-url="<%= ResolveUrl("~/fKhaiBaoGio/GioXuLyVanDe.ashx") %>"
                 data-gv-issue-id="<%= System.Web.HttpUtility.HtmlAttributeEncode(Request.QueryString["vanDeId"] ?? "") %>"
                 data-request-token="<%= Kbg20RequestToken() %>">
            <%-- KBG20 V4.1: soft-table layout; preserve V1 branch and Task partner controls. --%>
            <div id="kbg20Message" role="status" aria-live="polite"></div>
            <div class="kbg20-view-switch" role="tablist" aria-label="Các mục theo dõi giờ công">
                <button type="button" id="kbg20TabWork" class="kbg20-view-tab is-selected" role="tab" aria-selected="true" aria-controls="kbg20WorkView"><i class="fas fa-stopwatch" aria-hidden="true"></i> Theo dõi giờ làm</button>
                <button type="button" id="kbg20TabHistory" class="kbg20-view-tab" role="tab" aria-selected="false" aria-controls="kbg20HistoryView"><i class="far fa-calendar-alt" aria-hidden="true"></i> Nhật ký ngày làm việc</button>
                <button type="button" id="kbg20TabManual" class="kbg20-view-tab" role="tab" aria-selected="false" aria-controls="kbg20ManualView"><i class="fas fa-clipboard-list" aria-hidden="true"></i> Bảng kê giờ công</button>
            </div>
            <div id="kbg20WorkView" role="tabpanel" aria-labelledby="kbg20TabWork">
                <%-- Sub-filter of the first existing tab; Task markup/logic below is preserved. --%>
                <div class="gv-switch" role="group" aria-label="Loại đối tượng khai báo giờ">
                    <button type="button" class="gv-switch-btn is-active" id="gvTabTask" aria-pressed="true"><i class="fas fa-tasks" aria-hidden="true"></i> Công việc</button>
                    <button type="button" class="gv-switch-btn" id="gvTabIssue" aria-pressed="false"><i class="fas fa-exclamation-circle" aria-hidden="true"></i> Vấn đề</button>
                </div>

                <div class="kbg20-v41-workbar gv-only-task">
                    <div class="kbg20-schedule" id="kbg20Schedule" aria-label="Lịch làm việc hôm nay"></div>
                    <div class="kbg20-v41-tools">
                        <div id="kbg20TaskFilters" class="kbg20-task-filters" role="group" aria-label="Lọc công việc">
                            <button type="button" class="kbg20-filter-chip is-selected" data-task-filter="today" aria-pressed="true">Hôm nay <span data-filter-count="today">0</span></button>
                            <button type="button" class="kbg20-filter-chip" data-task-filter="all" aria-pressed="false">Tất cả <span data-filter-count="all">0</span></button>
                            <button type="button" class="kbg20-filter-chip" data-task-filter="reported" aria-pressed="false">Đã báo hoàn thành <span data-filter-count="reported">0</span></button>
                            <button type="button" class="kbg20-filter-chip" data-task-filter="open" aria-pressed="false">Đồng hồ đang bật <span data-filter-count="open">0</span></button>
                            <button type="button" class="kbg20-filter-chip" data-task-filter="pending" aria-pressed="false">Công việc chờ duyệt <span data-filter-count="pending">0</span></button>
                            <button type="button" class="kbg20-filter-chip" data-task-filter="overdue" aria-pressed="false">Quá hạn <span data-filter-count="overdue">0</span></button>
                            <button type="button" class="kbg20-filter-chip" data-task-filter="upcoming" aria-pressed="false">Sắp tới <span data-filter-count="upcoming">0</span></button>
                        </div>
                        <div class="kbg20-search"><label class="visually-hidden" for="kbg20Search">Tìm công việc</label>
                            <i class="fas fa-search" aria-hidden="true"></i>
                            <input type="search" id="kbg20Search" placeholder="Tìm mã hoặc tên công việc" autocomplete="off" /></div>
                        <button type="button" id="kbg20Reload" class="kbg20-btn kbg20-outline kbg20-icon-refresh kbg20-v42-reload"
                            aria-label="Làm mới dữ liệu tính giờ" title="Làm mới dữ liệu"><i class="fas fa-sync-alt" aria-hidden="true"></i></button>
                    </div>
                </div>
                <div class="kbg20-v41-table gv-only-task" aria-label="Danh sách công việc tính giờ">
                    <div class="kbg20-v41-table-head kbg20-v41-taskgrid" aria-hidden="true">
                        <span>Công việc</span><span>Ngày bắt đầu</span><span>Thời hạn</span><span>Kết thúc dự kiến</span>
                        <span>Trạng thái công việc</span><span>Báo cáo phần việc</span><span>Giờ làm theo lịch</span><span><i class="fas fa-moon" aria-hidden="true"></i> Giờ tăng ca</span><span>Hành động</span>
                    </div>
                    <div id="kbg20Tasks" class="kbg20-tasks"></div>
                </div>
                <div class="kbg20-v41-foot gv-only-task">
                    <span><span id="kbg20ListTitle">Công việc hôm nay</span> <span id="kbg20TaskCount"></span> · Thời gian đã ghi nhận được cộng từ tất cả các phiên làm việc.</span>
                    <div id="kbg20Summary" class="kbg20-summary" aria-label="Số phiên đang mở"></div>
                </div>
                <div id="kbg20LoadMoreWrap" class="kbg20-load-more gv-only-task" hidden="hidden">
                    <button type="button" class="kbg20-btn kbg20-outline" id="kbg20LoadMore">Xem thêm công việc</button>
                    <span id="kbg20LoadMoreHint"></span>
                </div>
                <%-- Issue dung chung hinh thuc thanh loc + bang + thao tac cua Task, khong su dung bang/popup chi tiet rieng. --%>
                <section class="gv-issue" id="gvIssuePane" hidden="hidden" aria-label="Giờ xử lý vấn đề">
                    <div class="kbg20-v41-workbar gv-workbar">
                        <div class="gv-workbar-summary"><strong><i class="fas fa-exclamation-circle" aria-hidden="true"></i> Vấn đề được phân công</strong><small id="gvCountLabel"></small></div>
                        <div class="kbg20-v41-tools gv-workbar-tools">
                            <div id="gvFilters" class="kbg20-task-filters" role="group" aria-label="Lọc vấn đề">
                                <button type="button" data-gv-filter="active" class="kbg20-filter-chip is-selected" aria-pressed="true">Đang xử lý <span data-gv-count="active">0</span></button>
                                <button type="button" data-gv-filter="all" class="kbg20-filter-chip" aria-pressed="false">Tất cả <span data-gv-count="all">0</span></button>
                                <button type="button" data-gv-filter="reported" class="kbg20-filter-chip" aria-pressed="false">Đã báo hoàn thành <span data-gv-count="reported">0</span></button>
                                <button type="button" data-gv-filter="running" class="kbg20-filter-chip" aria-pressed="false">Đồng hồ đang bật <span data-gv-count="running">0</span></button>
                                <button type="button" data-gv-filter="pending" class="kbg20-filter-chip" aria-pressed="false">Vấn đề chờ duyệt <span data-gv-count="pending">0</span></button>
                                <button type="button" data-gv-filter="done" class="kbg20-filter-chip" aria-pressed="false">Đã xử lý <span data-gv-count="done">0</span></button>
                            </div>
                            <div class="kbg20-search"><label class="visually-hidden" for="gvSearch">Tìm vấn đề</label><i class="fas fa-search" aria-hidden="true"></i><input type="search" id="gvSearch" placeholder="Tìm mã hoặc tên vấn đề" autocomplete="off" /></div>
                            <button type="button" id="gvReload" class="kbg20-btn kbg20-outline kbg20-icon-refresh kbg20-v42-reload" title="Làm mới danh sách vấn đề" aria-label="Làm mới vấn đề"><i class="fas fa-sync-alt" aria-hidden="true"></i></button>
                        </div>
                    </div>
                    <div class="kbg20-v41-table gv-tasklike-table" role="table" aria-label="Danh sách giờ xử lý vấn đề">
                        <div class="kbg20-v41-table-head kbg20-v41-taskgrid gv-tasklike-grid" role="row"><span>Vấn đề</span><span>Trạng thái vấn đề</span><span>Báo cáo phần việc</span><span>Giờ làm theo lịch</span><span><i class="fas fa-moon" aria-hidden="true"></i> Giờ tăng ca</span><span>Hành động</span></div>
                        <div id="gvRows" class="gv-tasklike-rows" aria-live="polite"></div>
                    </div>
                    <div class="kbg20-v41-foot gv-workfoot"><span id="gvListInfo">Đang tải vấn đề...</span><div class="gv-foot-actions"><button type="button" id="gvPrev" class="kbg20-btn kbg20-outline gv-pager" aria-label="Trang trước"><i class="fas fa-chevron-left"></i></button><span id="gvPageNumber">1 / 1</span><button type="button" id="gvNext" class="kbg20-btn kbg20-outline gv-pager" aria-label="Trang sau"><i class="fas fa-chevron-right"></i></button></div></div>
                </section>
            </div>
            <div id="kbg20HistoryView" role="tabpanel" aria-labelledby="kbg20TabHistory" hidden="hidden">
                <div class="kbg20-history-toolbar">
                    <div class="kbg20-history-desc"><h3>Nhật ký ngày làm việc <small>· Dự án hiện tại</small></h3>
                        <p>Xem thời gian làm việc đã được ghi nhận theo từng công việc trong dự án này, vào ngày được chọn.</p></div>
                    <div class="kbg20-history-date-controls">
                        <button type="button" id="kbg20HistoryPrev" class="kbg20-btn kbg20-outline" aria-label="Ngày trước đó" title="Ngày trước đó"><i class="fas fa-chevron-left" aria-hidden="true"></i></button>
                        <label for="kbg20HistoryDate">Ngày làm việc</label>
                        <input type="date" id="kbg20HistoryDate" />
                        <button type="button" id="kbg20HistoryNext" class="kbg20-btn kbg20-outline" aria-label="Ngày kế tiếp" title="Ngày kế tiếp" disabled="disabled"><i class="fas fa-chevron-right" aria-hidden="true"></i></button>
                        <button type="button" id="kbg20HistoryToday" class="kbg20-btn kbg20-outline">Hôm nay</button>
                    </div>
                </div>
                <div id="kbg20HistoryStatus" class="kbg20-history-status" role="status" aria-live="polite"></div>
                <div id="kbg20HistoryTotals" class="kbg20-history-totals"></div>
                <div id="gvHistoryTotalsCombined" class="kbg20-history-totals" hidden="hidden"></div>
                <div class="gv-cross-filter" id="gvHistoryFilter" role="group" aria-label="Lọc loại đối tượng trong nhật ký">
                    <button type="button" data-gv-cross="all" class="is-selected">Tất cả</button>
                    <button type="button" data-gv-cross="task">Công việc</button>
                    <button type="button" data-gv-cross="issue">Vấn đề</button>
                </div>
                <div class="gv-cross-title" id="gvHistoryTaskTitle">Công việc</div>
                <div id="kbg20History" class="kbg20-history-list"></div>
                <div class="gv-cross-title" id="gvHistoryIssueTitle">Vấn đề</div>
                <div id="gvHistoryIssues" class="kbg20-history-list" aria-live="polite"></div>
                <p class="kbg20-history-footnote">Tổng thời gian được ghi nhận theo từng công việc. Nếu hai đồng hồ chạy trùng thời gian, thời lượng vẫn được cộng độc lập cho từng công việc; đây không phải tổng số giờ đồng hồ thực trôi qua.</p>
            </div>
            <div id="kbg20ManualView" role="tabpanel" aria-labelledby="kbg20TabManual" hidden="hidden">
                <div class="kbg20-v41-manual-heading">
                    <div><h3>Bảng kê giờ công <small>· Phần việc được giao</small></h3>
                        <p>Đối chiếu thời gian hệ thống ghi nhận với số giờ đã báo; xem và chỉnh sửa khi chưa được quản lý chốt công.</p></div>
                    <div class="kbg20-v41-manual-filters" id="kbg20ManualFilters" role="group" aria-label="Lọc theo tình trạng báo hoàn thành phần việc">
                        <button type="button" class="kbg20-filter-chip is-selected" data-manual-filter="all">Tất cả</button>
                        <button type="button" class="kbg20-filter-chip" data-manual-filter="reported">Đã báo hoàn thành</button>
                        <button type="button" class="kbg20-filter-chip" data-manual-filter="withdrawn">Đã rút báo hoàn thành</button>
                    </div>
                </div>
                <div class="gv-cross-filter" id="gvLedgerFilter" role="group" aria-label="Lọc loại đối tượng trong bảng kê">
                    <button type="button" data-gv-cross="all" class="is-selected">Tất cả</button>
                    <button type="button" data-gv-cross="task">Công việc</button>
                    <button type="button" data-gv-cross="issue">Vấn đề</button>
                </div>
                <div class="gv-cross-title" id="gvLedgerTaskTitle">Công việc</div>
                <div class="kbg20-v41-manual-table" id="kbg20ManualList" aria-live="polite"></div>
                <div class="gv-cross-title" id="gvLedgerIssueTitle">Vấn đề</div>
                <div id="gvLedgerIssues" class="kbg20-v41-manual-table" aria-live="polite"></div>
                <p class="kbg20-v41-disclaimer">Báo hoàn thành phần việc không làm thay đổi trạng thái chung của công việc. Số giờ đã báo có hiệu lực ngay; sau khi quản lý chốt công sẽ không thể chỉnh sửa.</p>
                <button type="button" id="kbg20ReconcileListOpen" class="kbg20-v41-reconcile-hidden" hidden="hidden" aria-label="Mở bảng kê giờ công">Mở bảng kê giờ công</button>
            </div>
        </section>
        <% } %>

        <%-- V1 CHI render khi V2 khong hoat dong. Tranh 2 trang + 2 bo dem cung luc. --%>
        <% if (!LaGiaoDienKbg20) { %>
        <%-- ===== V1: Thẻ tổng hợp ngày + Danh sách công việc ===== --%>
        <asp:UpdatePanel ID="upnlMain" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnTaskAction" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnRefresh" EventName="Click" />
            </Triggers>
            <ContentTemplate>

                <asp:Panel ID="pnlDayOff" runat="server" Visible="false" CssClass="ks-empty">
                    <i class="fas fa-mug-hot"></i>
                    <asp:Literal ID="ltrDayOffText" runat="server"></asp:Literal>
                </asp:Panel>

                <%-- Thẻ tổng hợp thời gian làm việc cả ngày --%>
                <asp:Panel ID="pnlShifts" runat="server">
                    <asp:Repeater ID="rptShifts" runat="server" EnableViewState="false">
                        <ItemTemplate>
                            <div class="ks-day-card <%# ShiftCss((CaView)Container.DataItem) %>" id="ksDayCard">
                                <div class="ks-day-top">
                                    <div>
                                        <div class="ks-day-title">
                                            <span><i class="fas fa-business-time me-1"></i><%#: ((CaView)Container.DataItem).TenCa %></span>
                                            <span class="ks-day-hours"><%#: ((CaView)Container.DataItem).GioBatDau %> – <%#: ((CaView)Container.DataItem).GioKetThuc %></span>
                                        </div>
                                        <div class="ks-day-lunch mt-1">
                                            <%# ShiftLunchHtml((CaView)Container.DataItem) %>
                                        </div>
                                    </div>
                                    <span class="ks-chip <%# ShiftCss((CaView)Container.DataItem) %>" id="ksDayChip">
                                        <%#: ShiftChipText((CaView)Container.DataItem) %>
                                    </span>
                                </div>

                                <div class="ks-day-stats">
                                    <div>
                                        <span class="ks-day-total-big" id="ksDayTotalWatch"><%# KhaiBaoGioManager.FormatStopwatch(((CaView)Container.DataItem).TongGiayHienTai) %></span>
                                        <span class="ks-day-total-sub" id="ksDayTotalSummary">
                                            (<%# KhaiBaoGioManager.FormatReadableMinutes(((CaView)Container.DataItem).TongPhut) %> / <%# KhaiBaoGioManager.FormatReadableMinutes(((CaView)Container.DataItem).SoPhutCa) %>)
                                        </span>
                                    </div>
                                    <div class="ks-day-countdown"><%# ShiftMetaHtml((CaView)Container.DataItem) %></div>
                                </div>

                                <div class="ks-progress">
                                    <div class="ks-progress-bar" id="ksDayProgressBar" style="width: <%# ShiftPercent((CaView)Container.DataItem) %>%"></div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </asp:Panel>

                <%-- Danh sách công việc hôm nay --%>
                <asp:Panel ID="pnlTasks" runat="server" CssClass="ks-panel">
                    <div class="ks-panel-head">
                        <div>
                            <div class="ks-panel-title"><i class="fas fa-list-check me-1"></i><asp:Literal ID="ltrTasksTitle" runat="server"></asp:Literal></div>
                            <div class="ks-panel-sub"><asp:Literal ID="ltrTasksSub" runat="server"></asp:Literal></div>
                        </div>
                    </div>

                    <div class="ks-banner-lunch" id="ksLunchBanner">
                        <i class="fas fa-utensils"></i>
                        <span>Đang trong khung giờ nghỉ trưa. Đồng hồ đang tạm ngưng cộng giờ và sẽ tự động đếm tiếp khi vào ca chiều.</span>
                    </div>

                    <div class="ks-warn" id="ksWarn">
                        <i class="fas fa-exclamation-triangle"></i>
                        <span>Hôm nay bạn chưa bấm giờ cho công việc nào. Hãy nhấn <b>Bắt đầu</b> vào công việc đang thực hiện.</span>
                    </div>

                    <div class="ks-inline-notice" role="status" aria-live="polite">
                        <asp:Label ID="lblSaveStatus" runat="server" CssClass="ks-save-status"></asp:Label>
                    </div>
                    <div class="ks-filter-bar" aria-label="Tìm kiếm và lọc công việc">
                        <select id="ksFilterStatus" class="ks-filter-select" aria-label="Lọc trạng thái tính giờ">
                            <option value="all">Tất cả trạng thái</option>
                            <option value="running">Đang chạy</option>
                            <option value="unstarted">Chưa bấm giờ</option>
                            <option value="recorded">Đã ghi nhận</option>
                            <option value="overdue">Quá hạn</option>
                        </select>
                        <select id="ksFilterProject" class="ks-filter-select" aria-label="Lọc dự án">
                            <option value="all">Tất cả dự án</option>
                        </select>
                        <label class="ks-filter-search">
                            <input id="ksSearchText" class="ks-filter-input" type="search" autocomplete="off" placeholder="Nhập mã, tên công việc hoặc dự án..." aria-label="Tìm công việc" />
                            <i class="fas fa-search" aria-hidden="true"></i>
                        </label>
                        <button type="button" class="ks-filter-clear" id="ksFilterClear"><i class="fas fa-redo-alt me-1"></i>Xóa lọc</button>
                        <span id="ksFilterCount" class="ks-filter-info" aria-live="polite"></span>
                    </div>
                    <div class="ks-filter-tags" id="ksFilterTags"></div>
                    <div class="ks-panel-body" id="ksTasks">
                        <asp:Repeater ID="rptTasks" runat="server" EnableViewState="false" OnItemDataBound="rptTasks_ItemDataBound">
                            <ItemTemplate>
                                <asp:PlaceHolder ID="phGroup" runat="server" Visible="false">
                                    <div class="ks-group-title <%# ((TaskRowView)Container.DataItem).Nhom == NhomTaskView.QuaHan ? "is-overdue" : "" %>">
                                        <asp:Literal ID="ltrGroup" runat="server"></asp:Literal>
                                    </div>
                                </asp:PlaceHolder>

                                <div class="ks-task <%# TaskCss((TaskRowView)Container.DataItem) %>"
                                     data-task-id="<%# ((TaskRowView)Container.DataItem).IdCongViec.ToString("D") %>"
                                     data-project-id="<%# ((TaskRowView)Container.DataItem).IdDuAn.ToString("D") %>"
                                     data-project-name="<%# System.Web.HttpUtility.HtmlAttributeEncode(((TaskRowView)Container.DataItem).TenDuAn ?? string.Empty) %>"
                                     data-overdue="<%# ((TaskRowView)Container.DataItem).IsOverdue ? "1" : "0" %>"
                                     data-recorded="<%# ((TaskRowView)Container.DataItem).SoLanBamHomNay > 0 || ((TaskRowView)Container.DataItem).TongGiayHienTai > 0 ? "1" : "0" %>"
                                     data-base-sec="<%# ((TaskRowView)Container.DataItem).SoGiayDaTichLuy %>"
                                     data-running="<%# ((TaskRowView)Container.DataItem).TrangThaiDongHo == TrangThaiDongHoTask.DangChay ? "1" : "0" %>"
                                     data-start-ms="<%# ((TaskRowView)Container.DataItem).DangChayTuLuc.HasValue ? ToMs(((TaskRowView)Container.DataItem).DangChayTuLuc.Value) : 0 %>">

                                    <div class="ks-task-info">
                                        <div class="ks-kicker"><%#: ((TaskRowView)Container.DataItem).MaDuAn %> · <%#: ((TaskRowView)Container.DataItem).MaCongViec %> — <%#: ((TaskRowView)Container.DataItem).TenDuAn %></div>
                                        <div class="ks-task-name"><%#: ((TaskRowView)Container.DataItem).TenCongViec %></div>
                                        <div class="ks-task-meta"><%# TaskMetaHtml((TaskRowView)Container.DataItem) %></div>
                                    </div>

                                    <div class="ks-task-side">
                                        <div class="ks-tracker-row">
                                            <div class="ks-timer-box">
                                                <div class="ks-stopwatch"><%# KhaiBaoGioManager.FormatStopwatch(((TaskRowView)Container.DataItem).TongGiayHienTai) %></div>
                                                <div class="ks-decimal-hours"><%# TaskTodaySummaryHtml((TaskRowView)Container.DataItem) %></div>
                                            </div>

                                            <asp:PlaceHolder ID="phActionBtn" runat="server">
                                                <%# RenderTaskActionButton((TaskRowView)Container.DataItem) %>
                                            </asp:PlaceHolder>
                                        </div>

                                        <asp:PlaceHolder ID="phReadOnly" runat="server" Visible="false">
                                            <div class="ks-readonly"><%# TaskReadOnlyHtml((TaskRowView)Container.DataItem) %></div>
                                        </asp:PlaceHolder>

                                        <%# TaskBlockReasonHtml((TaskRowView)Container.DataItem) %>
                                        <div class="ks-hint <%# ((TaskRowView)Container.DataItem).IsOverdue ? "is-late" : "" %>">
                                            <%# TaskRecordHintHtml((TaskRowView)Container.DataItem) %>
                                        </div>
                                        <div class="ks-hint"><%# TaskHistoryHtml((TaskRowView)Container.DataItem) %></div>
                                        <button type="button" class="ks-journal-btn" data-ks-journal="<%# ((TaskRowView)Container.DataItem).IdCongViec.ToString("D") %>"><i class="far fa-clock me-1"></i>Nhật ký hôm nay</button>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>

                        <div class="ks-filter-empty" id="ksFilterEmpty"><i class="fas fa-search me-1"></i>Không tìm thấy công việc phù hợp. Hãy thử thay đổi điều kiện lọc.</div>
                        <asp:Panel ID="pnlNoTasks" runat="server" Visible="false" CssClass="ks-empty">
                            <i class="fas fa-clipboard-check"></i>
                            <b>Hôm nay bạn chưa được giao công việc nào.</b><br />
                            Khi có công việc đến hạn xử lý, chúng sẽ hiện ở đây.
                        </asp:Panel>
                    </div>
                </asp:Panel>

                <%-- Chân trang hiển thị thông báo thao tác gần nhất --%>
                <asp:Panel ID="pnlFooter" runat="server" CssClass="ks-footer">
                    <span class="text-muted" style="font-size:12px;">
                        <i class="fas fa-shield-alt me-1"></i>Tự động trừ giờ nghỉ trưa &amp; tự chốt sổ khi hết giờ làm việc buổi chiều
                    </span>
                </asp:Panel>

            </ContentTemplate>
        </asp:UpdatePanel>

        <%-- Các trường ẩn & nút ẩn phục vụ thao tác Bắt đầu / Dừng / Tiếp tục và Tự làm mới --%>
        <input type="hidden" runat="server" id="hdfActionTaskId" ClientIDMode="Static" value="" />
        <input type="hidden" runat="server" id="hdfActionType" ClientIDMode="Static" value="" />
        <asp:LinkButton ID="btnTaskAction" runat="server" CssClass="ks-hidden" OnClick="btnTaskAction_Click" CausesValidation="false"></asp:LinkButton>
        <asp:LinkButton ID="btnRefresh" runat="server" CssClass="ks-hidden" OnClick="btnRefresh_Click" CausesValidation="false"></asp:LinkButton>
        <% } %>

    </div>
    <% if (CurrentProjectId != Guid.Empty) { %>
    </div></div></div>

    <% } %>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <% if (LaGiaoDienKbg20) { %>
      <%-- Dot 2 chi host CtrlStartTask. Tuyet doi KHONG render control hoan thanh/duyet
           Task CHUNG tren trang KhaiBaoGio: cac control nay dong dong ho ca nhom. --%>
      <asp:UpdatePanel ID="upnlKbg20PartnerModals" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
          <SweetSoft:CtrlStartTask ID="KbgCtrlStartTask" runat="server" />
        </ContentTemplate>
      </asp:UpdatePanel>
    <% } else { %>
    <%-- Modal chỉ đọc: lấy nhật ký từ dữ liệu GetTodayView của chính user đăng nhập. --%>
    <div id="ksJournalModal" class="ks-journal-mask" hidden role="presentation">
        <div class="ks-journal-dialog" role="dialog" aria-modal="true" aria-labelledby="ksJournalTitle" tabindex="-1">
            <div class="ks-journal-header">
                <div><h3 id="ksJournalTitle"><i class="far fa-clock me-2"></i>Nhật ký bấm giờ</h3><p id="ksJournalTask"></p></div>
                <button id="ksJournalClose" class="ks-journal-close" type="button" aria-label="Đóng nhật ký">&times;</button>
            </div>
            <div class="ks-journal-body">
                <div style="overflow-x:auto;">
                    <table class="ks-journal-table">
                        <thead><tr><th>#</th><th>Bắt đầu</th><th>Kết thúc</th><th>Thời gian được tính</th><th>Ghi chú</th></tr></thead>
                        <tbody id="ksJournalRows"></tbody>
                    </table>
                </div>
                <div class="ks-journal-total"><span>Tổng thời gian của các phiên trong ngày</span><strong id="ksJournalTotal">00:00:00</strong></div>
                <p class="ks-journal-note">Thời gian được tính đã loại trừ thời gian nghỉ và thời gian ngoài lịch làm việc. Số phút lưu DB có thể được làm tròn theo quy định bấm giờ.</p>
            </div>
        </div>
    </div>
    <% } %>
    <% if (LaGiaoDienKbg20) { %>
        <%-- Tab 2: chi tiet Start/Stop read-only, theo dung Task/ngay dang xem. --%>
        <SweetSoft:ExtraModal runat="server" ID="mdlKbg20HistoryDetails" ClientIDMode="Static"
            Type="Primary" Size="ExtraLarge" Title="Chi tiết các lần tính giờ">
          <ContentTemplate>
            <div class="kbg20-ux-modal-content">
              <div id="kbg20HistoryDetailTitle" class="kbg20-ux-modal-title"></div>
              <div id="kbg20HistoryDetailBody" aria-live="polite"></div>
              <p class="kbg20-ux-muted">Thời gian được tính theo các khoảng đã kết toán trong ngày.
                 Phiên đang mở có thể còn thời gian chưa được job kết toán.</p>
            </div>
          </ContentTemplate>
        </SweetSoft:ExtraModal>
        <%-- ExtraModal chung cho canh bao qua han / phu thuoc, khong dung window.confirm. --%>
        <SweetSoft:ExtraModal runat="server" ID="mdlKbg20StartWarning" ClientIDMode="Static"
            Type="Primary" Title="Xác nhận bắt đầu công việc">
          <ContentTemplate>
            <div class="kbg20-ux-modal-content">
              <p id="kbg20StartWarningText" aria-live="polite"></p>
            </div>
          </ContentTemplate>
          <FooterTemplate>
            <div class="kbg20-ux-footer-actions">
              <button type="button" id="kbg20StartWarningNo" class="kbg20-btn kbg20-outline">Hủy</button>
              <button type="button" id="kbg20StartWarningYes" class="kbg20-btn">Tiếp tục</button>
            </div>
          </FooterTemplate>
        </SweetSoft:ExtraModal>
        <%-- Xac nhan khi Start Task khac: hai lua chon thuc su + Huy. --%>
        <SweetSoft:ExtraModal runat="server" ID="mdlKbg20Parallel" ClientIDMode="Static"
            Type="Primary" Title="Đang có đồng hồ khác">
          <ContentTemplate>
            <div class="kbg20-ux-modal-content">
              <p class="kbg20-parallel-intro">Bạn đang bắt đầu tính giờ một công việc mới trong khi vẫn có các đồng hồ khác đang mở.</p>
              <div class="kbg20-parallel-legend"><strong>Tích chọn những đồng hồ muốn DỪNG.</strong> Đồng hồ không tích vẫn tiếp tục chạy song song với công việc mới.</div>
              <div id="kbg20ParallelTasks" class="kbg20-ux-parallel-list"></div>
              <p id="kbg20ParallelGuide" class="kbg20-ux-muted" aria-live="polite"></p>
              <div id="kbg20ParallelSummary" class="kbg20-parallel-summary" aria-live="polite"></div>
            </div>
          </ContentTemplate>
          <FooterTemplate>
            <%-- Thao tác của popup phải nằm trong FooterTemplate ExtraModal, không lẫn vào danh sách phiên. --%>
            <div class="kbg20-ux-footer-actions kbg20-ux-parallel-footer-actions">
              <button type="button" class="btn btn-default kbg20-parallel-action" id="kbg20ParallelKeep" title="Dừng những đồng hồ đã tích, giữ các đồng hồ còn lại và bắt đầu công việc mới">Chạy song song với đồng hồ còn lại</button>
              <button type="button" class="btn btn-primary kbg20-parallel-action" id="kbg20ParallelStop" title="Dừng tất cả đồng hồ thủ công có thể dừng, sau đó bắt đầu công việc mới">Dừng tất cả có thể dừng và chạy mới</button>
            </div>
          </FooterTemplate>
        </SweetSoft:ExtraModal>
        <%-- Dùng ExtraModal CHÍNH THỨC giống CtrlFastCompleteTask/Review của partner.
             Dữ liệu đối soát vẫn nạp bằng API V2; không có form WebForms thứ hai. --%>
        <%-- Giong CtrlChonNhanVienTask: mo ExtraModal bang OpenModal(true) tu server;
             UpdatePanel chua modal de AJAX khong reload toan trang. --%>
        <asp:UpdatePanel ID="upnlKbg20ManualHost" runat="server" UpdateMode="Conditional">
          <ContentTemplate>
            <asp:LinkButton ID="btnKbg20OpenManual" runat="server" CausesValidation="false"
              style="display:none">Mở thông tin giờ công</asp:LinkButton>
        <SweetSoft:ExtraModal runat="server" ID="mdlKbg20Manual" ClientIDMode="Static"
            Type="Primary" Size="ExtraLarge" Title="Thông tin giờ công">
          <ContentTemplate>
            <asp:UpdatePanel ID="upnlKbg20ManualContent" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
              <ContentTemplate>
                <div class="js-validation validationEngineContainer p-2">
                  <div id="kbg20ReconcileContent" class="kbg20-reconcile-content"></div>
                </div>
              </ContentTemplate>
            </asp:UpdatePanel>
          </ContentTemplate>
          <FooterTemplate>
            <%-- Nút thao tác nằm tại chân ExtraModal, cạnh nút Đóng mặc định của control. --%>
            <asp:UpdatePanel ID="upnlKbg20ManualFooter" runat="server" UpdateMode="Conditional">
              <ContentTemplate>
                <div id="kbg20ReconcileFooterActions" class="kbg20-reconcile-footer-actions" hidden="hidden">
                  <button type="button" id="kbg20ManualBack" class="kbg20-btn kbg20-outline" hidden="hidden">Quay lại danh sách</button>
                  <button type="button" id="kbg20ManualSave" class="kbg20-btn">Lưu thay đổi</button>
                </div>
              </ContentTemplate>
            </asp:UpdatePanel>
          </FooterTemplate>
        </SweetSoft:ExtraModal>
          </ContentTemplate>
        </asp:UpdatePanel>
        <%-- Khong con modal 'Xem chi tiet Issue' rieng: Issue dung mdlKbg20Manual o Tab 1/3
             va mdlKbg20HistoryDetails o Tab 2 nhu Task. Chi giu popup xac nhan gui duyet ca nhom. --%>
        <SweetSoft:ExtraModal runat="server" ID="mdlGioXuLyVanDeConfirm" ClientIDMode="Static" Type="Primary" Size="Large" Title="Xác nhận gửi duyệt hoàn thành vấn đề">
          <ContentTemplate>
            <div class="gv-confirm-body">
                <p>Thao tác này gửi yêu cầu duyệt cho PM. Đồng hồ đang chạy của nhân viên khác sẽ tự dừng; họ được thông báo theo trạng thái báo phần việc.</p>
                <div id="gvConfirmPeople" class="gv-report-people"></div>
                <p class="gv-confirm-warning">Nhân viên đang bấm giờ hoặc đã dừng nhưng chưa báo phần việc vẫn được báo giờ sau thao tác này; họ sẽ không được Start thêm khi Issue đang chờ duyệt.</p>
            </div>
          </ContentTemplate>
          <FooterTemplate>
            <div class="gv-modal-buttons">
                <button type="button" id="gvCancelConfirm" class="gv-action">Hủy</button>
                <button type="button" id="gvConfirmSubmit" class="gv-action gv-action-submit">Xác nhận gửi duyệt</button>
            </div>
          </FooterTemplate>
        </SweetSoft:ExtraModal>
        <%-- Dot 2: ket qua hien thi tai trang va CSS modal duoc giu nguyen cua ExtraModal. --%>
    <% } %>
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <% if (LaGiaoDienKbg20) { %>
    <link rel="stylesheet" href="<%= ResolveUrl("~/Styles/css/kbg20-hour-split.css") %>?v=20261024p1" />
    <script src="<%= ResolveUrl("~/Styles/js/kbg20-hour-input.js") %>?v=20261024p1"></script>
    <link rel="stylesheet" href="<%= ResolveUrl("~/Styles/css/kbg20-ux-hotfix.css") %>?v=20261025parallelchoice3" />
    <%-- Giữ hai lớp CSS khôi phục đã dùng trong quá trình kiểm thử, không ghi đè stylesheet gốc. --%>
    <link rel="stylesheet" href="<%= ResolveUrl("~/Styles/css/kbg20-doi-soat-ui-restore.css") %>?v=20261026repair1" />
    <link rel="stylesheet" href="<%= ResolveUrl("~/Styles/css/kbg20-regression-badge-journal-restore.css") %>?v=20261026restorefinal1" />
    <script src="<%= ResolveUrl("~/Styles/js/kbg20-page.js") %>?v=20261027task-popup-label"></script>
    <script src="<%= ResolveUrl("~/Styles/js/gio-xu-ly-van-de.js") %>?v=20261028issue-unified-history"></script>
    <script src="<%= ResolveUrl("~/Styles/js/kbg20-doi-soat.js") %>?v=20261028pm-personal-task"></script>
    <% } else { %>
    <script type="text/javascript">
        var KhaiBaoGioJs = (function ($) {
            var cfg = null;
            var filterState = { status: 'all', project: 'all', keyword: '' };
            var journalTaskId = null;
            var journalPreviousFocus = null;
            var offset = 0;            // Độ lệch giờ máy chủ - giờ trình duyệt (ms)
            var ticker = null;
            var hooked = false;
            var bound = false;
            var pendingRefresh = false;
            var lastRefresh = Date.now();
            var fired = {};
            var requestKind = '';
            var actionInFlight = false;
            var pendingAction = null;
            var reloadPending = false;
            var requestStartedAt = 0;                       // thời điểm bắt đầu request đang chạy (cho watchdog)
            var errorCount = 0;                             // số lỗi liên tiếp
            var jitterMs = Math.floor(Math.random() * 4000); // lệch ngẫu nhiên 0-4s để mọi máy không đồng loạt gọi server
            var firedCount = {};                            // số lần đã gọi làm mới cho từng mốc biên
            var refreshEveryMs = 300000 + Math.floor(Math.random() * 60000);

            function nowMs() { return Date.now() + offset; }
            function pad(n) { return (n < 10 ? '0' : '') + n; }

            function fmtStopwatch(sec) {
                sec = Math.max(0, Math.floor(sec));
                var h = Math.floor(sec / 3600);
                var m = Math.floor((sec % 3600) / 60);
                var s = sec % 60;
                return pad(h) + ':' + pad(m) + ':' + pad(s);
            }

            function fmtDecimalHours(minutes) {
                if (minutes <= 0) return '0 giờ';
                var h = Math.round((minutes / 60.0) * 10) / 10;
                return h.toString().replace('.', ',') + ' giờ';
            }

            function fmtReadableMinutes(minutes) {
                minutes = Math.max(0, Math.floor(minutes));
                var hours = Math.floor(minutes / 60);
                var mins = minutes % 60;
                if (hours === 0) return mins + ' phút';
                return hours + ' giờ' + (mins ? ' ' + mins + ' phút' : '');
            }

            // Tính số giây thực làm giữa [startMs, endMs] bằng cách lấy phần giao với các khoảng ca làm việc
            // (Tự động cắt bỏ giờ nghỉ trưa 12:00-13:00 và thời gian ngoài khung làm việc)
            function calcWorkingSeconds(startMs, endMs) {
                if (!cfg || !cfg.segments || endMs <= startMs) return 0;
                var totalSec = 0;
                for (var i = 0; i < cfg.segments.length; i++) {
                    var seg = cfg.segments[i];
                    var s = Math.max(startMs, seg.start);
                    var e = Math.min(endMs, seg.end);
                    if (e > s) {
                        totalSec += (e - s) / 1000.0;
                    }
                }
                return Math.round(totalSec);
            }

            function isBusy() {
                if (typeof Sys === 'undefined' || !Sys.WebForms || !Sys.WebForms.PageRequestManager) return false;
                return Sys.WebForms.PageRequestManager.getInstance().get_isInAsyncPostBack();
            }

            function triggerRefresh() {
                if (!cfg || reloadPending) return;

                // Không gửi yêu cầu làm mới chồng lên một yêu cầu khác.
                if (requestKind || actionInFlight || isBusy()) {
                    pendingRefresh = true;
                    return;
                }

                requestKind = 'refresh';
                requestStartedAt = Date.now();
                lastRefresh = Date.now();

                try {
                    __doPostBack(cfg.refreshId, '');
                } catch (e) {
                    forceReload(
                        'Không thể làm mới dữ liệu. Trang sẽ tải lại để đồng bộ trạng thái.'
                    );
                }
            }
            function sendTaskActionNow(taskId, action) {
                if (!cfg || reloadPending) return;

                // Nếu còn yêu cầu khác, không gửi chồng.
                if (requestKind || actionInFlight || isBusy()) {
                    if ((requestKind === 'refresh' ||
                        (isBusy() && !requestKind)) &&
                        !pendingAction) {

                        pendingAction = {
                            taskId: taskId,
                            action: action
                        };

                        lockActionButtons();

                        setKsStatus(
                            'Đang cập nhật dữ liệu. Thao tác sẽ được kiểm tra sau khi cập nhật xong.',
                            'pending'
                        );
                    } else {
                        setKsStatus(
                            'Hệ thống đang xử lý yêu cầu trước đó. Vui lòng đợi.',
                            'pending'
                        );
                    }

                    return;
                }

                var $btn = findActionButton(taskId, action);

                // Không thực hiện thao tác nếu nút đã biến mất hoặc bị khóa
                // theo trạng thái nghiệp vụ mới nhất.
                if (!$btn.length || $btn.prop('disabled')) {
                    setKsStatus(
                        'Trạng thái công việc vừa thay đổi. Vui lòng kiểm tra và bấm lại.',
                        'error'
                    );
                    return;
                }

                requestKind = 'action';
                actionInFlight = true;
                requestStartedAt = Date.now();

                $('#hdfActionTaskId').val(taskId);
                $('#hdfActionType').val(action);

                lockActionButtons();

                setKsStatus('Đang xử lý thao tác…', 'pending');

                try {
                    __doPostBack(cfg.actionBtnId, '');
                } catch (e) {
                    forceReload(
                        'Không thể gửi thao tác đến máy chủ. Trang sẽ tải lại để kiểm tra trạng thái.'
                    );
                }
            }
            function drainPendingRequests(attempt) {
                if (reloadPending) return;

                attempt = attempt || 0;

                // Chờ ASP.NET kết thúc hoàn toàn yêu cầu trước đó.
                if (isBusy() || requestKind) {
                    if (attempt < 20) {
                        window.setTimeout(function () {
                            drainPendingRequests(attempt + 1);
                        }, 50);
                    } else {
                        forceReload(
                            'Yêu cầu xử lý chưa hoàn tất. Trang sẽ tải lại để đồng bộ trạng thái.'
                        );
                    }

                    return;
                }

                // Ưu tiên thao tác người dùng trước yêu cầu làm mới định kỳ.
                if (pendingAction) {
                    var next = pendingAction;
                    pendingAction = null;

                    var $btn = findActionButton(next.taskId, next.action);

                    if (!$btn.length || $btn.prop('disabled')) {
                        unlockActionButtons();

                        setKsStatus(
                            'Trạng thái công việc đã thay đổi. Thao tác chưa được thực hiện; vui lòng kiểm tra và bấm lại.',
                            'error'
                        );

                        // Dữ liệu vừa nhận từ máy chủ đã được làm mới.
                        pendingRefresh = false;
                        return;
                    }

                    sendTaskActionNow(next.taskId, next.action);
                    return;
                }

                // Chỉ làm mới khi không còn thao tác người dùng đang chờ.
                if (pendingRefresh) {
                    pendingRefresh = false;
                    triggerRefresh();
                }
            }
            // Bộ lọc client: không postback, giữ trạng thái sau UpdatePanel.
            function normalizedText(text) {
                var v = String(text || '').toLowerCase();
                if (v.normalize) v = v.normalize('NFD').replace(/[\u0300-\u036f]/g, '');
                return v.replace(/đ/g, 'd').trim();
            }
            function refreshProjectFilter() {
                var $select = $('#ksFilterProject');
                if (!$select.length) return;
                var options = {};
                $('#ksTasks .ks-task').each(function () {
                    var $row = $(this), id = $row.attr('data-project-id');
                    if (id && !options[id]) options[id] = $row.attr('data-project-name') || id;
                });
                $select.empty().append($('<option>').val('all').text('Tất cả dự án'));
                Object.keys(options).sort(function (a, b) {
                    return options[a].localeCompare(options[b], 'vi');
                }).forEach(function (id) { $select.append($('<option>').val(id).text(options[id])); });
                if (!options[filterState.project]) filterState.project = 'all';
                $select.val(filterState.project);
            }
            function renderFilterTags() {
                var $tags = $('#ksFilterTags');
                if (!$tags.length) return;
                $tags.empty();
                var addTag = function (key, text) {
                    var $tag = $('<span>').addClass('ks-filter-tag').append($('<span>').text(text));
                    $tag.append($('<button type="button">').attr('data-ks-remove-filter', key).attr('aria-label', 'Bỏ ' + text).text('×'));
                    $tags.append($tag);
                };
                if (filterState.status !== 'all') addTag('status', 'Trạng thái: ' + $('#ksFilterStatus option:selected').text());
                if (filterState.project !== 'all') addTag('project', 'Dự án: ' + $('#ksFilterProject option:selected').text());
                if (filterState.keyword) addTag('keyword', 'Từ khóa: ' + filterState.keyword);
            }
            function applyFilters() {
                var keyword = normalizedText(filterState.keyword), shown = 0, pinned = 0;
                $('#ksFilterStatus').val(filterState.status);
                $('#ksFilterProject').val(filterState.project);
                if ($('#ksSearchText').val() !== filterState.keyword) $('#ksSearchText').val(filterState.keyword);
                $('#ksTasks .ks-task').each(function () {
                    var $row = $(this);
                    var running = $row.attr('data-running') === '1';
                    var recorded = $row.attr('data-recorded') === '1';
                    var overdue = $row.attr('data-overdue') === '1';
                    var status = filterState.status;
                    var statusMatch = status === 'all' ||
                        (status === 'running' && running) ||
                        (status === 'unstarted' && !recorded) ||
                        (status === 'recorded' && recorded) ||
                        (status === 'overdue' && overdue);
                    var textMatch = !keyword || normalizedText($row.find('.ks-kicker').text() + ' ' + $row.find('.ks-task-name').text()).indexOf(keyword) !== -1;
                    var projectMatch = filterState.project === 'all' || $row.attr('data-project-id') === filterState.project;
                    var matches = statusMatch && textMatch && projectMatch;
                    // Luôn cho thấy công việc đang chạy để người dùng không bị mất nút Dừng.
                    var visible = matches || running;
                    $row.toggle(visible);
                    if (visible) shown++;
                    if (running && !matches) pinned++;
                });
                $('#ksTasks > .ks-group-title').each(function () {
                    var $items = $(this).nextUntil('.ks-group-title', '.ks-task');
                    $(this).toggle($items.filter(':visible').length > 0);
                });
                $('#ksFilterEmpty').toggle(shown === 0 && $('#ksTasks .ks-task').length > 0);
                $('#ksFilterCount').text(shown + ' công việc hiển thị' + (pinned ? ' · Công việc đang chạy luôn hiển thị' : ''));
                renderFilterTags();
            }
            function refreshFiltersAfterBind() {
                refreshProjectFilter();
                applyFilters();
            }

            // Popup nhật ký chỉ đọc, dữ liệu thuộc chính tài khoản đang đăng nhập.
            function showJournal(taskId) {
                if (!cfg || !cfg.journal) return;
                var $row = $('#ksTasks .ks-task').filter(function () { return $(this).attr('data-task-id') === taskId; }).first();
                if (!$row.length) return;
                journalTaskId = taskId;
                journalPreviousFocus = document.activeElement;
                $('#ksJournalTask').text($row.find('.ks-kicker').text() + ' · ' + $row.find('.ks-task-name').text());
                var modal = document.getElementById('ksJournalModal');
                if (!modal) return;
                modal.hidden = false;
                renderJournal();
                $('#ksJournalClose').trigger('focus');
            }
            function hideJournal() {
                var modal = document.getElementById('ksJournalModal');
                if (modal) modal.hidden = true;
                journalTaskId = null;
                if (journalPreviousFocus && typeof journalPreviousFocus.focus === 'function' && document.documentElement.contains(journalPreviousFocus)) {
                    journalPreviousFocus.focus();
                }
                journalPreviousFocus = null;
            }
            function renderJournal() {
                if (!journalTaskId || !cfg || !cfg.journal) return;
                var sessions = cfg.journal.filter(function (x) { return x.taskId === journalTaskId; });
                var $body = $('#ksJournalRows');
                $body.empty();
                var total = 0;
                var t = nowMs();
                sessions.forEach(function (session, index) {
                    var running = session.end === null || session.end === undefined;
                    var seconds = running ? calcWorkingSeconds(session.start, t) : session.seconds;
                    seconds = Math.max(0, seconds || 0);
                    total += seconds;
                    function timeLabel(ms) { var d = new Date(ms); return pad(d.getHours()) + ':' + pad(d.getMinutes()) + ':' + pad(d.getSeconds()); }
                    var note = running ? 'Đang chạy' : (session.reason === 2 ? 'Tự động kết thúc ca' : 'Nhân viên dừng');
                    $('<tr>').append($('<td>').text(index + 1))
                        .append($('<td>').text(timeLabel(session.start)))
                        .append($('<td>').text(running ? 'Đang chạy' : timeLabel(session.end)))
                        .append($('<td>').addClass(running ? 'ks-journal-live' : '').text(fmtStopwatch(seconds)))
                        .append($('<td>').text(note))
                        .appendTo($body);
                });
                if (!sessions.length) $body.append($('<tr>').append($('<td colspan="5">').text('Chưa có phiên bấm giờ nào trong hôm nay.')));
                $('#ksJournalTotal').text(fmtStopwatch(total));
            }

            function setKsStatus(message, kind) {
                var $status = $('.ks-save-status').first();

                if (!$status.length) return;

                var cssClass = 'ks-save-status';

                if (kind) {
                    cssClass += ' is-' + kind;
                }

                $status.text(message).attr('class', cssClass);
            }


            // Chỉ khóa các nút đang có thể sử dụng.
            // Không thay đổi trạng thái disabled do quy tắc nghiệp vụ đặt ra.
            function lockActionButtons() {
                $('.ks-action-btn').each(function () {
                    var $btn = $(this);

                    if ($btn.prop('disabled')) return;

                    $btn.attr('data-ks-js-locked', '1');
                    $btn.prop('disabled', true);
                });
            }


            // Chỉ mở lại những nút được JavaScript khóa.
            // Các nút server render ở trạng thái disabled vẫn giữ nguyên.
            function unlockActionButtons() {
                $('.ks-action-btn[data-ks-js-locked="1"]').each(function () {
                    $(this)
                        .prop('disabled', false)
                        .removeAttr('data-ks-js-locked');
                });
            }


            // Tìm nút tương ứng với trạng thái thao tác mới nhất trên giao diện.
            function findActionButton(taskId, action) {
                var $found = $();

                $('.ks-action-btn').each(function () {
                    var $btn = $(this);

                    if ($btn.attr('data-task-id') === taskId &&
                        $btn.attr('data-action') === action) {
                        $found = $btn;
                        return false;
                    }
                });

                return $found;
            }


            // Nếu không thể xác định kết quả yêu cầu, tải lại trang để lấy
            // trạng thái mới nhất từ máy chủ, tránh cho giao diện tiếp tục sai lệch.
            function forceReload(message) {
                if (reloadPending) return;

                reloadPending = true;
                actionInFlight = false;
                requestKind = 'error';
                pendingAction = null;
                pendingRefresh = false;

                lockActionButtons();
                setKsStatus(message, 'error');

                window.setTimeout(function () {
                    window.location.reload();
                }, 1200);
            }
            // Watchdog: nếu cờ bận còn bật mà ASP.NET đã rảnh (mất endRequest) hoặc request treo quá lâu
            // thì tự giải phóng để nút không bị khóa vĩnh viễn.
            function watchdog() {
                if (reloadPending || !(requestKind || actionInFlight)) return;
                var age = Date.now() - requestStartedAt;
                if (!isBusy() && age > 3000) {
                    requestKind = '';
                    actionInFlight = false;
                    unlockActionButtons();
                    drainPendingRequests(0);
                } else if (isBusy() && age > 30000) {
                    try { Sys.WebForms.PageRequestManager.getInstance().abortPostBack(); } catch (e) { }
                    forceReload('Máy chủ phản hồi quá lâu. Trang sẽ tải lại để đồng bộ trạng thái.');
                }
            }

            function tick() {
                var t = nowMs();

                hookRequestManager();   // thử lại nếu lần init đầu Sys chưa sẵn sàng
                watchdog();

                // 1. Đồng hồ giờ hệ thống trên Header
                var clock = document.getElementById('ksClock');
                if (clock) {
                    var d = new Date(t);
                    clock.textContent = pad(d.getHours()) + ':' + pad(d.getMinutes()) + ':' + pad(d.getSeconds());
                }

                // 2. Đồng hồ đếm ngược đến giờ mở / đóng ngày làm việc
                $('.ks-count').each(function () {
                    var target = parseInt($(this).attr('data-target'), 10);
                    if (target) $(this).text(fmtStopwatch((target - t) / 1000));
                });

                if (!cfg) return;

                // 3. Kiểm tra có đang trong giờ nghỉ trưa không
                var inLunch = (cfg.lunchStart > 0 && cfg.lunchEnd > 0 && t >= cfg.lunchStart && t < cfg.lunchEnd);
                var lunchBanner = document.getElementById('ksLunchBanner');
                if (lunchBanner) {
                    lunchBanner.style.display = (inLunch && cfg.isDayOpen) ? 'flex' : 'none';
                }

                // 4. Cập nhật trực tiếp đồng hồ của Task đang chạy & Tổng thời gian ngày
                var totalDaySec = 0;
                $('#ksTasks .ks-task').each(function () {
                    var $row = $(this);
                    var baseSec = parseInt($row.attr('data-base-sec'), 10) || 0;
                    var isRunning = $row.attr('data-running') === '1';
                    var startMs = parseInt($row.attr('data-start-ms'), 10) || 0;

                    var currentSec = baseSec;
                    if (isRunning && startMs > 0) {
                        currentSec += calcWorkingSeconds(startMs, t);
                        $row.find('.ks-stopwatch').text(fmtStopwatch(currentSec));
                        var mins = Math.round(currentSec / 60.0);
                        $row.find('.ks-decimal-hours').text(
                            inLunch ? 'Đang tạm dừng nghỉ trưa' : ('Đang chạy · ' + fmtReadableMinutes(mins))
                        );
                    }
                    totalDaySec += currentSec;
                });

                if (cfg.capacitySec > 0 && totalDaySec > cfg.capacitySec) {
                    totalDaySec = cfg.capacitySec;
                }

                if (cfg.isDayOpen) {
                    var dayWatch = document.getElementById('ksDayTotalWatch');
                    if (dayWatch) dayWatch.textContent = fmtStopwatch(totalDaySec);

                    var daySummary = document.getElementById('ksDayTotalSummary');
                    if (daySummary) {
                        var totalMins = Math.round(totalDaySec / 60.0);
                        daySummary.textContent = '(' + fmtReadableMinutes(totalMins) + ' / ' + fmtReadableMinutes(cfg.capacityMin) + ')';
                    }

                    var bar = document.getElementById('ksDayProgressBar');
                    if (bar && cfg.capacitySec > 0) {
                        var pct = Math.min(100, Math.round((totalDaySec * 100) / cfg.capacitySec));
                        bar.style.width = pct + '%';
                    }

                    var warn = document.getElementById('ksWarn');
                    if (warn) {
                        warn.style.display = (totalDaySec === 0 && !cfg.hasRunningTask) ? 'flex' : 'none';
                    }
                }

                // Cập nhật nhật ký khi popup đang mở (phiên đang chạy được tính theo khung ca).
                if (journalTaskId) renderJournal();

                // 5. Khi bước qua các mốc biên (Mở ca sáng, Bắt đầu nghỉ trưa, Hết nghỉ trưa, Hết ca chiều 17:00):
                // Tự động gọi về Server để mở khóa hoặc tự động chốt sổ cuối ngày.
                if (cfg.boundaries && cfg.boundaries.length) {
                    for (var i = 0; i < cfg.boundaries.length; i++) {
                        var b = cfg.boundaries[i];
                        var n = firedCount[b] || 0;
                        if (n < 3 && t >= b + 800 + jitterMs + n * 3000) {
                            firedCount[b] = n + 1;
                            triggerRefresh();
                            return;
                        }
                    }
                }

                // 6. Qua nửa đêm sang ngày mới: tải lại toàn trang
                if (cfg.midnight > 0 && t >= cfg.midnight + 1000) {
                    window.location.reload();
                    return;
                }

                // 7. Làm mới định kỳ mỗi 5 phút để giữ phiên đăng nhập và cập nhật task mới
                if (!requestKind &&
                    !actionInFlight &&
                    !isBusy() &&
                    Date.now() - lastRefresh > refreshEveryMs) {
                    triggerRefresh();
                }
            }

            function hookRequestManager() {
                if (hooked ||
                    typeof Sys === 'undefined' ||
                    !Sys.WebForms ||
                    !Sys.WebForms.PageRequestManager) {
                    return;
                }

                hooked = true;

                var prm = Sys.WebForms.PageRequestManager.getInstance();

                prm.add_endRequest(function (sender, args) {

                    // Trường hợp máy chủ trả lỗi hoặc postback AJAX thất bại.
                    if (args.get_error()) {
                        args.set_errorHandled(true);
                        errorCount++;
                        requestKind = '';
                        actionInFlight = false;
                        pendingAction = null;
                        unlockActionButtons();

                        if (errorCount >= 2) {
                            forceReload('Yêu cầu không hoàn tất. Trang sẽ tự tải lại để đồng bộ trạng thái.');
                        } else {
                            setKsStatus('Yêu cầu chưa hoàn tất, đang đồng bộ lại…', 'error');
                            window.setTimeout(triggerRefresh, 1500);
                        }
                        return;
                    }

                    errorCount = 0;

                    // Yêu cầu đã kết thúc thành công.
                    requestKind = '';
                    actionInFlight = false;

                    // Mở lại các nút bị khóa tạm bằng JavaScript.
                    // Nút disabled do máy chủ tạo ra vẫn được giữ nguyên.
                    unlockActionButtons();

                    // Xử lý thao tác hoặc yêu cầu làm mới còn chờ.
                    window.setTimeout(function () {
                        drainPendingRequests(0);
                    }, 0);
                });
            }
            function bindEvents() {
                if (bound) return;
                bound = true;

                // Sự kiện được gắn ở document nên vẫn hoạt động sau UpdatePanel.
                $(document).on('input.ks', '#ksSearchText', function () {
                    filterState.keyword = $(this).val();
                    applyFilters();
                });
                $(document).on('change.ks', '#ksFilterStatus', function () {
                    filterState.status = $(this).val(); applyFilters();
                });
                $(document).on('change.ks', '#ksFilterProject', function () {
                    filterState.project = $(this).val(); applyFilters();
                });
                $(document).on('click.ks', '#ksFilterClear', function () {
                    filterState = { status: 'all', project: 'all', keyword: '' }; applyFilters();
                });
                $(document).on('click.ks', '[data-ks-remove-filter]', function () {
                    var key = $(this).attr('data-ks-remove-filter');
                    if (key === 'keyword') filterState.keyword = '';
                    else if (key) filterState[key] = 'all';
                    applyFilters();
                });
                $(document).on('click.ks', '.ks-journal-btn', function () {
                    showJournal($(this).attr('data-ks-journal'));
                });
                $(document).on('click.ks', '#ksJournalClose', hideJournal);
                $(document).on('click.ks', '#ksJournalModal', function (e) {
                    if (e.target === this) hideJournal();
                });
                $(document).on('keydown.ks', function (e) {
                    if (e.key === 'Escape' && journalTaskId) hideJournal();
                    if (e.key === 'Tab' && journalTaskId) {
                        // Modal chỉ có một nút tương tác, giữ focus bên trong.
                        e.preventDefault(); $('#ksJournalClose').trigger('focus');
                    }
                });

                $(document).on('click.ks', '.ks-action-btn', function (e) {
                    e.preventDefault();

                    var $btn = $(this);

                    if (reloadPending || $btn.prop('disabled')) {
                        return;
                    }

                    var taskId = $btn.attr('data-task-id');
                    var action = $btn.attr('data-action');

                    if (!taskId || !action || !cfg) {
                        setKsStatus(
                            'Không xác định được công việc cần thao tác. Vui lòng tải lại trang.',
                            'error'
                        );
                        return;
                    }

                    // Không nhận thêm thao tác nếu một thao tác trước vẫn đang chạy.
                    if (actionInFlight ||
                        requestKind === 'action' ||
                        pendingAction) {

                        setKsStatus(
                            'Thao tác trước đó đang được xử lý. Vui lòng đợi.',
                            'pending'
                        );

                        return;
                    }

                    // Nếu đang làm mới dữ liệu, giữ lại lần bấm đầu tiên.
                    if (requestKind === 'refresh' ||
                        (isBusy() && !requestKind)) {

                        if (!pendingAction) {
                            pendingAction = {
                                taskId: taskId,
                                action: action
                            };

                            lockActionButtons();

                            setKsStatus(
                                'Đang cập nhật dữ liệu. Thao tác sẽ được kiểm tra và thực hiện nếu còn hợp lệ.',
                                'pending'
                            );
                        } else {
                            setKsStatus(
                                'Đang có một thao tác chờ xử lý. Vui lòng đợi.',
                                'pending'
                            );
                        }

                        return;
                    }

                    // Trường hợp yêu cầu khác đang chạy mà không phải làm mới của trang.
                    if (isBusy() || requestKind) {
                        setKsStatus(
                            'Trang đang xử lý một yêu cầu khác. Vui lòng đợi rồi bấm lại.',
                            'pending'
                        );
                        return;
                    }

                    sendTaskActionNow(taskId, action);
                });
            }

            return {
                init: function (config) {
                    cfg = config;
                    offset = cfg.serverNow - Date.now();
                    lastRefresh = Date.now();

                    // Không đặt lại pendingRefresh, pendingAction,
                    // actionInFlight hoặc requestKind ở đây.
                    // Các trạng thái này phải tồn tại xuyên suốt partial postback.

                    hookRequestManager();
                    bindEvents();
                    refreshFiltersAfterBind();
                    if (journalTaskId) {
                        // Nhật ký đang mở có thể cập nhật sau Start/Stop mà không mất modal.
                        var exists = $('#ksTasks .ks-task').filter(function () { return $(this).attr('data-task-id') === journalTaskId; }).length > 0;
                        if (exists) renderJournal(); else hideJournal();
                    }

                    if (!ticker) {
                        ticker = setInterval(tick, 1000);
                    }

                    tick();
                }
            };
        })(jQuery);
    </script>
    <% } %>
</asp:Content>