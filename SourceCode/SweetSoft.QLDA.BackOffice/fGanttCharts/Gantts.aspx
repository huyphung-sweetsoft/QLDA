<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="Gantts.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fGanttCharts.Gantts" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>
<%@ Register Src="~/fIssues/Controls/CtrlViewIssueDetail.ascx" TagPrefix="SweetSoft" TagName="CtrlViewIssueDetail" %>
<%@ Register Src="~/fTasks/Controls/CtrlViewTaskDetail.ascx" TagPrefix="SweetSoft" TagName="CtrlViewTaskDetail" %>
<%@ Register Src="~/fGanttCharts/Controls/CtrlShareProgress.ascx" TagPrefix="SweetSoft" TagName="CtrlShareProgress" %>
<%@ Register Src="~/fGanttCharts/Controls/CtrlProgressReportHistory.ascx" TagPrefix="SweetSoft" TagName="CtrlProgressReportHistory" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        .gantt-report-actions { display:inline-flex;align-items:center;gap:8px;margin-left:auto;flex-wrap:wrap; }
        .gantt-report-actions .share-progress-trigger { margin-left:0; }
        .gantt-report-actions .gantt-report-history-trigger { color:#5b2692;background:#faf6ff;border-color:#d9c5f0;box-shadow:none; }
        .gantt-report-actions .gantt-report-history-trigger:hover { color:#452070;background:#efe5fb;border-color:#bfa2de; }
        @media(max-width:700px) { .gantt-report-actions { width:100%;justify-content:flex-end; } }
        /* CSS cho link tên công việc trong Popup */
    .task-name-link {
        color: #542e88 !important; /* Màu tím mặc định */
        text-decoration: none !important;
        font-weight: 500 !important; /* Bỏ in đậm (fw-bold), để mỏng vừa phải */
        transition: color 0.15s ease;
    }
    .task-name-link:hover {
        color: #3b82f6 !important; /* Đổi màu xanh dương khi di chuột vào */
    }
        @keyframes fadeSlideUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
        
        .gantt { background: #fff; border: 1px solid #ccc; border-radius: 6px; animation: fadeSlideUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards; box-shadow: 0 4px 12px rgba(0,0,0,0.05); height: 65vh; overflow: auto; position: relative; isolation: isolate; }
        .gantt-body { display: flex; width: max-content; min-width: 100%; min-height: 100%; position: relative; }
        
        .task-col { width: 350px; flex: 0 0 350px; background: #fff; position: sticky; left: 0; z-index: 80; border-right: 1px solid #ccc; box-shadow: 4px 0 10px rgba(0,0,0,0.07); min-height: 100%; }
        .task-col-header { height: 44px; display: flex; align-items: center; justify-content: center; font-weight: bold; font-size: 13px; background: #f8f9fa; border-bottom: 1px solid #ccc; position: sticky; top: 0; z-index: 90; }

        .task-row-wrap { height: 52px; max-height: 52px; border-bottom: 1px solid #e5e5e5; display: flex; align-items: center; box-sizing: border-box; font-size: 13px; background: #fff; overflow: hidden; transition: max-height 0.35s ease-in-out, opacity 0.35s ease-in-out, background-color 0.2s, border-width 0.35s ease-in-out; opacity: 1; }
        
        .row-hover { background-color: #f9f6fd !important; }
        .row-hover .task-level-1 { transform: translateX(6px); box-shadow: 0 2px 8px rgba(94, 53, 177, 0.15); }
        .row-hover .task-level-child { transform: translateX(6px); }
        .task-level-1 { margin: 0 8px; flex: 1; height: 40px; background-color: #f4ebff; border-left: 4px solid #5e35b1; border-radius: 6px; color: #5e35b1; font-weight: bold; display: flex; align-items: center; padding-left: 10px; user-select: none; transition: transform 0.25s, box-shadow 0.25s; }
        .task-level-child { display: flex; align-items: center; width: 100%; color: #333; transition: transform 0.25s; }
        .level-2 { padding-left: 16px; } .level-3 { padding-left: 42px; } .level-4 { padding-left: 68px; } .level-5 { padding-left: 94px; }
        .tree-branch { display: inline-block; width: 14px; height: 18px; border-left: 1.5px solid #aaa; border-bottom: 1.5px solid #aaa; margin-right: 8px; transform: translateY(-6px); }
        .toggle-icon { transition: transform 0.3s; color: #666; } .task-level-1 .toggle-icon { color: #5e35b1; }
        
        .hidden-task { max-height: 0 !important; opacity: 0 !important; border-bottom-width: 0 !important; }
        
        .chart-col { flex: 1; position: relative; z-index: 1; }
        .chart-header { height: 44px; position: sticky; top: 0; background: #f8f9fa; border-bottom: 1px solid #ccc; z-index: 10; }
        .chart-header .tick-label { position: absolute; top: 0; height: 100%; display: flex; align-items: center; justify-content: center; font-size: 12px; color: #555; white-space: nowrap; }
        .chart-header .tick-line { position: absolute; top: 0; bottom: 0; width: 1px; background: #e0e0e0; }
        .chart-tracks { position: relative; min-height: 100px; z-index: 1; }
        .grid-line { position: absolute; top: 0; bottom: 0; width: 1px; background: #f2f2f2; }
        .task-track { height: 52px; max-height: 52px; position: relative; border-bottom: 1px solid #e5e5e5; overflow: hidden; transition: max-height 0.35s ease-in-out, border-width 0.35s ease-in-out; }
        
        .task-track.hide-bar .bar { opacity: 0 !important; pointer-events: none; }

        .bar { position: absolute; top: 14px; height: 24px; border-radius: 4px; cursor: pointer; transition: width 0.7s cubic-bezier(0.34, 1.56, 0.64, 1), transform 0.2s, box-shadow 0.2s, filter 0.2s, opacity 0.35s ease; }
        .bar:hover { transform: translateY(-2px); box-shadow: 0 4px 10px rgba(0,0,0,0.15); filter: brightness(1.08); z-index: 10; }
        .bar-fill { position: absolute; top: 0; left: 0; bottom: 0; border-radius: 4px; overflow: hidden; }
        
        @keyframes progress-stripes {
            from { background-position: 0 0; }
            to { background-position: 28px 0; }
        }

        .bar-fill.animated-stripes {
            background-image: linear-gradient(-45deg, rgba(255, 255, 255, 0.15) 25%, transparent 25%, transparent 50%, rgba(255, 255, 255, 0.15) 50%, rgba(255, 255, 255, 0.15) 75%, transparent 75%, transparent);
            background-size: 28px 28px;
            animation: progress-stripes 1.5s linear infinite; 
        }

        .bar.status-todo .bar-fill { background-color: #cbd5e1; }
        .bar.status-todo .static-text { color: #334155; }
        .bar.status-done .bar-fill { background-color: #34a853; }
        .gantt-late-completion-text { display:inline-block;margin-top:4px;color:#dc2626;font-size:11px;font-weight:800; }
        .bar.status-doing .bar-fill { background-color: #4285f4; }
        .bar.status-warning .bar-fill { background-color: #f4b400; }
        .bar.status-overdue .bar-fill { background-color: #ea4335; }
        
        @keyframes pulseOverdue { 0% { box-shadow: 0 0 0 0 rgba(234, 67, 53, 0.4); } 70% { box-shadow: 0 0 0 6px rgba(234, 67, 53, 0); } 100% { box-shadow: 0 0 0 0 rgba(234, 67, 53, 0); } }
        .bar.status-overdue { animation: pulseOverdue 2s infinite; }
        
        .static-text { 
            position: absolute; top: 0; left: 8px; width:calc(100% - 14px);overflow:hidden;text-overflow:ellipsis;color: rgba(255, 255, 255, 0.95); font-size: 13px; font-weight: 800; 
            line-height: 24px; white-space: nowrap; letter-spacing: 0.1px; z-index: 10; pointer-events: none; 
        }

        .today-line { position: absolute; top: 0; bottom: 0; width: 2px; background: #ea4335; z-index: 2; }
        .today-label { position: absolute; top: -22px; transform: translateX(-50%); font-size: 11px; color: #ea4335; font-weight: bold; white-space: nowrap; }

        #svgConnections { transition: opacity 0.3s; }
        .dep-line { fill: none; stroke: #ff9800; stroke-width: 1.5; stroke-dasharray: 4, 4; animation: dashMove 10s linear infinite; }
        @keyframes dashMove { to { stroke-dashoffset: -100; } }

        div[id*='modalIssues'] .modal-dialog { max-width: 900px !important; }

        .gantt::-webkit-scrollbar { width: 12px; height: 12px; }
        .gantt::-webkit-scrollbar-track { background: #f8f9fa; border-top: 1px solid #e5e5e5; border-left: 1px solid #e5e5e5; border-radius: 0 0 6px 6px; }
        .gantt::-webkit-scrollbar-thumb { background: #c1c1c1; border-radius: 6px; border: 3px solid #f8f9fa; }
        .gantt::-webkit-scrollbar-thumb:hover { background: #a8a8a8; }
        .gantt::-webkit-scrollbar-corner { background: #f8f9fa; }

        .gantt-badge-done { background-color: #dcfce7 !important; color: #166534 !important; border: 1px solid #bbf7d0 !important; font-weight: 600; padding: 5px 10px; border-radius: 4px; }
        .gantt-badge-doing { background-color: #e0f2fe !important; color: #0369a1 !important; border: 1px solid #bae6fd !important; font-weight: 600; padding: 5px 10px; border-radius: 4px; }
        .gantt-badge-todo { background-color: #f1f5f9 !important; color: #475569 !important; border: 1px solid #e2e8f0 !important; font-weight: 600; padding: 5px 10px; border-radius: 4px; }
        .gantt-badge-danger { background-color: #fee2e2 !important; color: #dc2626 !important; border: 1px solid #fca5a5 !important; font-weight: 600; padding: 5px 10px; border-radius: 4px; }

        /* Gantt 2 chế độ - CSS một dòng trên một selector */
        .gantt-workspace-toolbar { display:flex;align-items:center;justify-content:space-between;gap:12px;flex-wrap:wrap;margin:0 0 13px;position:relative; }
        .gantt-workspace-left { display:flex;align-items:center;gap:10px;min-width:0; }
        .gantt-legend-help { width:34px;height:34px;border:1px solid #d7c3ed;border-radius:9px;background:#f6f0fe;color:#6029a0;font-size:15px;font-weight:800;cursor:pointer; }
        .gantt-legend-help:hover,.gantt-legend-help.is-pinned { background:#632a9e;color:#fff; }
        .gantt-legend-popover { display:none;position:absolute;top:calc(100% + 9px);left:0;z-index:500;min-width:240px;padding:12px;border:1px solid #ddc9f2;border-radius:11px;background:#fff;box-shadow:0 14px 32px rgba(40,23,70,.15); }
        .gantt-legend-popover.is-open { display:grid;grid-template-columns:1fr;gap:8px; }
        .gantt-legend-popover strong { font-size:12px;color:#452077;margin-bottom:2px; }
        .gantt-legend-popover span { display:flex;align-items:center;gap:9px;font-size:12px;color:#4a4155; }
        .gantt-legend-popover small { font-size:10px;color:#8a8197; }
        .gantt-key { display:inline-block;width:13px;height:13px;border-radius:3px;flex:0 0 13px; }
        .gantt-key.done { background:#34a853; }.gantt-key.doing { background:#4285f4; }.gantt-key.warning { background:#f4b400; }.gantt-key.overdue { background:#ea4335; }.gantt-key.todo { background:#cbd5e1; }.gantt-key.today { width:2px;flex-basis:2px;margin-left:5px;margin-right:6px;background:#ea4335; }.gantt-key.dependency { width:auto;color:#ff9800;background:transparent;font-weight:900; }
        .gantt-workspace-tabs { display:inline-flex;align-items:center;gap:0;border:1px solid #dfd0ef;border-radius:9px;overflow:hidden;background:#faf7ff; }
        .gantt-workspace-tabs button { padding:9px 17px;border:0;background:transparent;color:#6c5684;cursor:pointer;font-size:12.5px;font-weight:800;white-space:nowrap; }
        .gantt-workspace-tabs button.is-active { background:#5b2499;color:#fff; }.gantt-workspace-tabs button+button { border-left:1px solid #e4d7f1; }
        #ganttCustomerPanel[hidden],#ganttInternalPanel[hidden] { display:none!important; }
        .gantt-customer-dock button { padding:8px 12px;border:1px solid #c8aae7;border-radius:8px;background:#f4eaff;color:#54218d;font-size:12px;font-weight:800;cursor:pointer; }
        .gantt-customer-preview { min-height:300px; }.gantt-customer-preview .gantt { height:65vh; }
        .gantt-customer-dock { display:flex;align-items:center;justify-content:space-between;gap:12px;padding:9px 3px;color:#736282;font-size:12px; }.gantt-customer-dock button { white-space:nowrap; }
        .gantt-customer-editor { padding:16px;color:#40364d;background:#fff;border:1px solid #ded3ec;border-radius:11px;box-shadow:0 3px 13px rgba(66,33,100,.05); }.gantt-customer-intro { display:flex;align-items:flex-start;gap:9px;padding:11px 13px;margin-bottom:12px;border:1px solid #e6d8f1;border-radius:8px;background:#faf7ff;color:#66517c;font-size:12px; }
        .gantt-customer-editor-tools { display:flex;align-items:center;justify-content:space-between;margin-bottom:9px;gap:12px; }.gantt-customer-editor-tools strong { font-size:12px;color:#54218d; }.gantt-customer-editor-tools button { border:1px solid #d8c6eb;border-radius:7px;background:#fff;color:#603994;padding:6px 9px;font-size:11px;font-weight:750; }
        .gantt-customer-edit-scroll { max-height:min(65vh,610px);overflow:auto;border:1px solid #e4dceb;border-radius:9px; }.gantt-customer-edit-table { width:100%;border-collapse:collapse;min-width:910px;font-size:12.5px; }
        .gantt-customer-edit-table th { position:sticky;top:0;z-index:2;background:#f5f1fb;color:#522882;text-align:left;padding:11px 8px;border-bottom:1px solid #e4d6ef; }.gantt-customer-edit-table td { padding:7px 6px;border-bottom:1px solid #efeaf4; }
        .gantt-customer-edit-table td input,.gantt-customer-edit-table td select { width:100%;min-width:0;padding:9px 8px;border:1px solid #d8cde3;border-radius:6px;background:#fff;color:#45354e;font-size:12.5px; }.gantt-customer-edit-table td input:focus,.gantt-customer-edit-table td select:focus { outline:none;border-color:#8852bc;box-shadow:0 0 0 2px rgba(116,62,166,.11); }
        .gantt-customer-edit-table th:nth-child(1) { width:8%; }.gantt-customer-edit-table th:nth-child(2) { width:34%; }.gantt-customer-edit-table th:nth-child(3),.gantt-customer-edit-table th:nth-child(4) { width:14%; }.gantt-customer-edit-table th:nth-child(5) { width:15%; }.gantt-customer-edit-table th:nth-child(6) { width:15%; }
        .gantt-customer-errors { margin-top:8px;color:#bd3535;font-size:12px;font-weight:700;min-height:18px; }.gantt-customer-footer { display:flex;align-items:center;justify-content:space-between;gap:12px;margin-top:12px;flex-wrap:wrap; }.gantt-customer-footer-note { font-size:12px;color:#756785; }.gantt-customer-generate { border:0;border-radius:8px;padding:10px 17px;background:#5b2692;color:white;font-size:13px;font-weight:750;cursor:pointer;box-shadow:0 3px 9px rgba(91,38,146,.2); }.gantt-customer-generate:hover { background:#472075; }.gantt-customer-editor[hidden],.gantt-customer-preview[hidden],.gantt-customer-dock[hidden] { display:none!important; }
        .gantt-customer-preview .task-col-header,.gantt-customer-preview .chart-header { height:44px; }.gantt-customer-preview .task-col { width:350px;flex:0 0 350px; }.gantt-customer-preview .task-row-wrap { cursor:default; }
        .gantt-customer-preview .task-track .bar { cursor:default; }
        .gantt-report-only .share-progress-list-header,.gantt-report-only .share-progress-task-list,.gantt-report-only .share-progress-preset-status { display:none!important; }
        .gantt-report-only .share-progress-report-name-row { margin-bottom:12px; }
        .gantt-report-only .share-progress-otp-row,.gantt-report-only .share-progress-security-note,.gantt-report-only .share-progress-result-block:has(.share-progress-otp-row) { display:none!important; }
        @media(max-width:800px) { .gantt-workspace-toolbar { align-items:flex-start; }.gantt-report-actions { width:100%;justify-content:flex-start; } }

        /* Tooltip Gantt noi tren body: hien ngay, khong bi cat boi khung cuon. */
        .gantt-bar-tip { position:fixed;display:none;z-index:2147483000;max-width:min(280px,calc(100vw - 20px));padding:9px 13px;border-radius:9px;font-size:13px;font-weight:750;line-height:1.3;white-space:nowrap;pointer-events:none;box-shadow:0 7px 19px rgba(17,24,39,.19);color:#fff;background:#5930a1; }
        .gantt-bar-tip[data-kind="done"] { background:#24884b; }
        .gantt-bar-tip[data-kind="late"] { background:#24884b;border-left:4px solid #dc535b; }
        .gantt-bar-tip[data-kind="doing"] { background:#2563c9; }
        .gantt-bar-tip[data-kind="warning"] { background:#ad7508; }
        .gantt-bar-tip[data-kind="overdue"] { background:#d73939; }
        .gantt-bar-tip[data-kind="todo"] { background:#5c697c; }
        .gantt-bar-tip::after { content:"";position:absolute;left:50%;bottom:-5px;transform:translateX(-50%) rotate(45deg);height:10px;width:10px;background:inherit; }
        .gantt-bar-tip.is-below::after { bottom:auto;top:-5px; }

        /* Tab Khach hang: cuon NOI BO bang, khong phat sinh scroll trang (desktop). */
        .gantt-customer-editor { display:flex;flex-direction:column;min-height:0;overflow:hidden;padding:12px 15px; }
        .gantt-customer-editor-tools { flex:0 0 auto;min-height:38px;margin-bottom:9px; }
        .gantt-customer-editor-actions { display:flex;align-items:center;justify-content:flex-end;gap:9px;flex-wrap:nowrap; }
        .gantt-customer-editor-actions button { white-space:nowrap;min-height:34px;padding:7px 12px;font-size:12px; }
        .gantt-customer-editor-actions button.gantt-customer-generate { border:1px solid #5b2692;background:#5b2692;color:#fff;font-size:12px;font-weight:800;box-shadow:0 3px 9px rgba(91,38,146,.16); }
        .gantt-customer-editor-actions button.gantt-customer-generate:hover { background:#482075;border-color:#482075; }
        .gantt-customer-edit-scroll { flex:1 1 auto;min-height:0;max-height:none;overflow:auto;overscroll-behavior:contain; }
        .gantt-customer-errors { flex:0 0 auto;margin-top:6px;min-height:0; }
        .gantt-customer-errors:empty { display:none; }
        .gantt-customer-preview { min-height:0; }
        .gantt-customer-preview .gantt { min-height:0; }
        .gantt-customer-mode .card.min-h-sreen:has(#ganttCustomerPanel) { min-height:0!important; }
        #ganttReportAction[hidden] { display:none!important; }
        @media(max-width:700px) { .gantt-customer-editor-actions { gap:5px; }.gantt-customer-editor-actions button { padding:6px 8px;font-size:11px; }.gantt-customer-edit-table { min-width:910px; } }

        /* Toolbar dong bo chieu cao cho hai tab va cac nut hanh dong. */
        #ganttWorkspaceToolbar { align-items:center; }
        #ganttWorkspaceToolbar .gantt-workspace-tabs { height:44px;min-height:44px;box-sizing:border-box; }
        #ganttWorkspaceToolbar .gantt-workspace-tabs button { height:42px;min-height:42px;display:inline-flex;align-items:center;justify-content:center;gap:6px;padding:0 18px;line-height:1;box-sizing:border-box;font-size:13px; }
        #ganttWorkspaceToolbar .gantt-legend-help { height:44px;width:44px;min-height:44px;box-sizing:border-box; }
        #ganttWorkspaceToolbar .gantt-report-actions { display:inline-flex;align-items:center;gap:9px; }
        #ganttWorkspaceToolbar .gantt-report-actions .share-progress-trigger { display:inline-flex!important;align-items:center;justify-content:center;gap:7px;height:44px!important;min-height:44px!important;padding:0 16px!important;line-height:1!important;white-space:nowrap;font-size:13px!important;box-sizing:border-box; }
        /* Khung Khach hang: thanh nut hien o ca bang va Gantt; bang chi cuon ben trong. */
        .gantt-customer-surface { display:flex;flex-direction:column;min-height:0;overflow:hidden;padding:14px 15px;background:#fff;border:1px solid #ded3ec;border-radius:11px;box-shadow:0 3px 13px rgba(66,33,100,.05); }
        .gantt-customer-surface .gantt-customer-editor-tools { flex:0 0 auto;min-height:44px;align-items:center;margin-bottom:10px; }
        .gantt-customer-surface .gantt-customer-editor-tools strong { font-size:13px; }
        .gantt-customer-surface .gantt-customer-editor { flex:1 1 auto;display:flex;flex-direction:column;height:auto!important;min-height:0;border:0;border-radius:0;box-shadow:none;padding:0;overflow:hidden; }
        .gantt-customer-surface .gantt-customer-preview { flex:1 1 auto;min-height:0;overflow:hidden; }
        .gantt-customer-surface .gantt-customer-preview .gantt { height:100%!important;max-height:100%;margin:0; }
        .gantt-customer-surface .gantt-customer-editor-actions button { display:inline-flex;align-items:center;justify-content:center;gap:7px;height:40px;min-height:40px;padding:0 14px;box-sizing:border-box;font-size:12.5px;cursor:pointer; }
        .gantt-customer-surface .gantt-customer-editor-actions button.gantt-customer-generate { color:#fff;background:#5b2692;border-color:#5b2692; }
        .gantt-customer-surface .gantt-customer-editor-actions button.gantt-customer-generate:hover { background:#452070;border-color:#452070; }
        @media(max-width:700px) { #ganttWorkspaceToolbar { align-items:stretch; } #ganttWorkspaceToolbar .gantt-report-actions { width:100%;justify-content:flex-start; } #ganttWorkspaceToolbar .gantt-report-actions .share-progress-trigger { padding:0 11px!important; } .gantt-customer-surface { padding:9px; } .gantt-customer-surface .gantt-customer-editor-actions button { padding:0 9px; } }

        /* Gantt: chỉ một hàng nút cao 40px, không chiếm chiều cao thừa. */
        #ganttWorkspaceToolbar { height:40px!important;min-height:40px!important;padding:0!important;margin:0 0 9px!important;align-items:center!important;flex-wrap:nowrap; }
        #ganttWorkspaceToolbar .gantt-workspace-left,#ganttWorkspaceToolbar .gantt-report-actions { height:40px!important;min-height:40px!important;align-items:center!important; }
        #ganttWorkspaceToolbar .gantt-workspace-tabs { height:40px!important;min-height:40px!important; }
        #ganttWorkspaceToolbar .gantt-workspace-tabs button { height:38px!important;min-height:38px!important; }
        #ganttWorkspaceToolbar .gantt-legend-help { width:40px!important;height:40px!important;min-height:40px!important; }
        #ganttWorkspaceToolbar .gantt-report-actions .share-progress-trigger { height:40px!important;min-height:40px!important; }
        .gantt-customer-editor-title { display:flex;align-items:center;gap:10px;min-width:0; }
        .gantt-root-add { display:inline-flex;gap:6px;align-items:center;justify-content:center;height:32px;padding:0 11px;border:1px solid #d8c6eb;border-radius:7px;background:#fbf7ff;color:#54218d;font-size:12px;font-weight:750;white-space:nowrap;cursor:pointer; }
        .gantt-root-add:hover { background:#efe4fa; }
        .gantt-customer-edit-table { min-width:1120px; }
        .gantt-customer-edit-table th:nth-child(1){width:8%;}.gantt-customer-edit-table th:nth-child(2){width:31%;}.gantt-customer-edit-table th:nth-child(3),.gantt-customer-edit-table th:nth-child(4){width:13%;}.gantt-customer-edit-table th:nth-child(5){width:14%;}.gantt-customer-edit-table th:nth-child(6){width:15%;}.gantt-customer-edit-table th:nth-child(7){width:6%;}
        .gantt-task-actions { white-space:nowrap;text-align:center; }
        .gantt-task-actions button { display:inline-flex;align-items:center;justify-content:center;vertical-align:middle;width:30px;height:32px;padding:0;margin:0 2px;border:1px solid #ddcdeb;border-radius:7px;background:#faf7ff;color:#6533a3;cursor:pointer; }
        .gantt-task-actions button.gantt-row-delete { color:#bf3e51;background:#fff8f9;border-color:#f4d2da; }
        .gantt-task-actions button:hover { filter:brightness(.95); }
        .gantt-task-actions button:disabled { opacity:.35;cursor:not-allowed; }
        .gantt-task-actions button.is-delete-pending { background:#ffe5e8;border-color:#e64c67;color:#a1132e; }
        .gantt-customer-edit-table input.gantt-derived-date { background:#f5f3f9!important;color:#655675;cursor:not-allowed; }
        @media(max-width:800px) { #ganttWorkspaceToolbar { height:auto!important;flex-wrap:wrap; } }
        /* Tooltip gon nhu TaskList, khong dung title mac dinh cua browser. */
        .gantt-editor-tip { position:fixed;display:none;z-index:2147483000;max-width:min(330px,calc(100vw - 16px));padding:9px 13px;border:1px solid rgba(167,139,250,.4);border-radius:9px;background:linear-gradient(135deg,#6d28d9,#3b1468);color:#fff;font-size:12.5px;font-weight:700;line-height:1.4;white-space:normal;overflow-wrap:anywhere;pointer-events:none;box-shadow:0 10px 24px rgba(77,30,130,.25); }
        .gantt-editor-tip[data-kind="danger"] { background:linear-gradient(135deg,#cf3754,#9f1833);border-color:rgba(252,165,165,.48);box-shadow:0 10px 24px rgba(167,30,54,.22); }
        .gantt-editor-tip::after { content:"";position:absolute;bottom:-5px;left:50%;width:9px;height:9px;transform:translateX(-50%) rotate(45deg);background:#43166f;border-right:1px solid rgba(167,139,250,.4);border-bottom:1px solid rgba(167,139,250,.4); }
        .gantt-editor-tip[data-kind="danger"]::after { background:#a71c38;border-color:rgba(252,165,165,.48); }
        .gantt-editor-tip.is-below::after { bottom:auto;top:-5px;transform:translateX(-50%) rotate(225deg); }

        /* Gantt UI 2026: che do xem la bo chon; hanh dong chinh mau tim; hanh dong phu nhe va de hieu. */
        #ganttWorkspaceToolbar { gap:12px!important; }
        #ganttWorkspaceToolbar .gantt-workspace-left { gap:10px; }
        #ganttWorkspaceToolbar .gantt-mode-label { color:#7b8394;font-size:12px;font-weight:650;line-height:1;white-space:nowrap; }
        #ganttWorkspaceToolbar .gantt-workspace-tabs { display:inline-flex;align-items:center;gap:2px;height:36px!important;min-height:36px!important;padding:3px;border:1px solid #e4e3eb!important;border-radius:10px!important;background:#f2f2f6!important;box-shadow:none!important;overflow:hidden; }
        #ganttWorkspaceToolbar .gantt-workspace-tabs button { display:inline-flex;align-items:center;justify-content:center;gap:7px;height:28px!important;min-height:28px!important;padding:0 13px!important;border:0!important;border-radius:7px!important;background:transparent!important;color:#667085!important;font-size:12.5px!important;font-weight:650!important;line-height:1.1!important;cursor:pointer;box-shadow:none!important;transition:color .16s ease,background-color .16s ease,box-shadow .16s ease; }
        #ganttWorkspaceToolbar .gantt-workspace-tabs button+button { border-left:0!important; }
        #ganttWorkspaceToolbar .gantt-workspace-tabs button.is-active { background:#fff!important;color:#54218d!important;box-shadow:0 1px 4px rgba(38,27,60,.11)!important;font-weight:750!important; }
        #ganttWorkspaceToolbar .gantt-workspace-tabs button:not(.is-active):hover { background:#eae6f2!important;color:#54218d!important; }
        #ganttWorkspaceToolbar .gantt-workspace-tabs button i { font-size:12px;opacity:.9; }
        #ganttWorkspaceToolbar .gantt-legend-help { display:inline-flex;align-items:center;justify-content:center;width:34px!important;height:34px!important;min-height:34px!important;border:1px solid transparent!important;border-radius:9px!important;background:transparent!important;color:#7d718e!important;box-shadow:none!important;font-size:15px!important;transition:color .16s ease,background .16s ease,border-color .16s ease; }
        #ganttWorkspaceToolbar .gantt-legend-help:hover,#ganttWorkspaceToolbar .gantt-legend-help.is-pinned { border-color:#e8dbf6!important;background:#f2eafb!important;color:#54218d!important; }
        #ganttWorkspaceToolbar .gantt-report-actions { gap:8px!important; }
        #ganttWorkspaceToolbar .gantt-report-actions .share-progress-trigger { height:38px!important;min-height:38px!important;padding:0 14px!important;border-radius:9px!important;font-size:12.5px!important;font-weight:700!important;letter-spacing:0!important;transition:color .16s ease,background-color .16s ease,border-color .16s ease,box-shadow .16s ease!important; }
        #ganttWorkspaceToolbar #ganttReportAction { border:1px solid #54218d!important;background:#54218d!important;color:#fff!important;box-shadow:0 2px 7px rgba(84,33,141,.15)!important;font-weight:750!important; }
        #ganttWorkspaceToolbar #ganttReportAction:hover { border-color:#451775!important;background:#451775!important;color:#fff!important;box-shadow:0 4px 10px rgba(84,33,141,.20)!important; }
        #ganttWorkspaceToolbar .gantt-report-actions .gantt-report-history-trigger { border:1px solid transparent!important;background:transparent!important;color:#626a7b!important;box-shadow:none!important;font-weight:650!important; }
        #ganttWorkspaceToolbar .gantt-report-actions .gantt-report-history-trigger:hover { border-color:#e5e2eb!important;background:#f3f2f7!important;color:#54218d!important; }
        #ganttWorkspaceToolbar button:focus-visible,#ganttCustomerSurface .gantt-customer-editor-tools button:focus-visible { outline:2px solid #9466c5!important;outline-offset:2px!important; }
        /* Thanh thao tac cua bang khach hang: Xem Gantt duoc nhan nhe, Them va Khoi phuc la thao tac phu. */
        #ganttCustomerSurface .gantt-customer-editor-tools { gap:12px; }
        #ganttCustomerSurface .gantt-customer-editor-tools strong { color:#475267;font-size:13px!important;font-weight:750; }
        #ganttCustomerSurface .gantt-root-add { display:inline-flex;align-items:center;justify-content:center;gap:7px;height:34px;min-height:34px;padding:0 11px;border:1px solid #e7dfef;border-radius:8px;background:#fff;color:#64448b;font-size:12px;font-weight:700;box-shadow:none;transition:color .16s ease,background .16s ease,border-color .16s ease; }
        #ganttCustomerSurface .gantt-root-add:hover { border-color:#d5c3e9;background:#f7f3fc;color:#54218d; }
        #ganttCustomerSurface .gantt-customer-editor-actions { gap:8px; }
        #ganttCustomerSurface .gantt-customer-editor-actions button { height:36px;min-height:36px;padding:0 12px;border-radius:9px;font-size:12.5px;font-weight:700;transition:color .16s ease,background .16s ease,border-color .16s ease,box-shadow .16s ease; }
        #ganttCustomerSurface #ganttCustomerReset { border:1px solid transparent;background:transparent;color:#667085;box-shadow:none;font-weight:650; }
        #ganttCustomerSurface #ganttCustomerReset:hover { border-color:#e5e2eb;background:#f4f3f7;color:#4d5669; }
        #ganttCustomerSurface #ganttCustomerPreviewToggle.gantt-customer-generate { border:1px solid #dcccf0!important;border-radius:9px;background:#f1eafa!important;color:#54218d!important;box-shadow:none!important;font-weight:750!important; }
        #ganttCustomerSurface #ganttCustomerPreviewToggle.gantt-customer-generate:hover { border-color:#c9afe5!important;background:#e7dbf7!important;color:#421775!important;box-shadow:0 2px 6px rgba(84,33,141,.08)!important; }
        @media(max-width:850px) { #ganttWorkspaceToolbar { height:auto!important;min-height:40px!important;flex-wrap:wrap!important;row-gap:7px!important; } #ganttWorkspaceToolbar .gantt-report-actions { width:100%;justify-content:flex-start; } #ganttWorkspaceToolbar .gantt-mode-label { display:none; } #ganttCustomerSurface .gantt-customer-editor-tools { flex-wrap:wrap; } #ganttCustomerSurface .gantt-customer-editor-actions { margin-left:auto; } }
        @media(max-width:480px) { #ganttWorkspaceToolbar .gantt-workspace-tabs button { padding:0 9px!important;font-size:12px!important; } #ganttWorkspaceToolbar .gantt-report-actions .share-progress-trigger { padding:0 10px!important;font-size:12px!important; } #ganttCustomerSurface .gantt-customer-editor-actions { flex-wrap:wrap;justify-content:flex-end; } }
        @media(prefers-reduced-motion:reduce) { #ganttWorkspaceToolbar .gantt-workspace-tabs button,#ganttWorkspaceToolbar .gantt-legend-help,#ganttWorkspaceToolbar .gantt-report-actions .share-progress-trigger,#ganttCustomerSurface .gantt-customer-editor-tools button { transition:none!important; } }

        /* V6: Neo popup vao chinh nut ?, o phia tren ben phai nut, khong neo theo mep phai toolbar. */
        #ganttWorkspaceToolbar { position:relative;overflow:visible;isolation:isolate; }
        #ganttLegendDock { position:relative;display:inline-flex;align-items:center;justify-content:center;flex:0 0 auto;width:auto;min-height:0;margin:0;padding:0; }
        #ganttLegendDock .gantt-legend-help { display:inline-flex;align-items:center;justify-content:center;flex:0 0 36px;width:36px!important;height:36px!important;min-height:36px!important;margin:0;padding:0;border:1px solid #e4d7f1!important;border-radius:9px!important;background:#f5effb!important;color:#602993!important;font-size:15px!important;font-weight:800;cursor:pointer;box-shadow:none!important;transition:background-color .16s ease,border-color .16s ease,color .16s ease; }
        #ganttLegendDock .gantt-legend-help:hover { border-color:#cbb4e5!important;background:#ede3f8!important;color:#54218d!important; }
        #ganttLegendDock .gantt-legend-help.is-pinned { border-color:#54218d!important;background:#54218d!important;color:#fff!important; }
        #ganttLegendDock .gantt-legend-help.is-pinned:hover { border-color:#451775!important;background:#451775!important;color:#fff!important; }
        #ganttLegendDock .gantt-legend-help:focus-visible { outline:2px solid #9565c5!important;outline-offset:2px!important; }
        #ganttLegendDock .gantt-legend-popover { position:absolute!important;top:auto!important;bottom:calc(100% + 9px)!important;left:calc(100% + 10px)!important;right:auto!important;z-index:500!important;display:none!important;grid-template-columns:repeat(3,minmax(0,1fr));column-gap:14px;row-gap:9px;width:min(555px,calc(100vw - 44px));min-width:0;max-width:calc(100vw - 44px);margin:0;padding:12px 16px 11px;border:1px solid #ddc9f2;border-radius:12px;background:#fff;box-shadow:0 12px 32px rgba(40,23,70,.15);box-sizing:border-box;pointer-events:none; }
        #ganttLegendDock .gantt-legend-popover.is-open { display:grid!important; }
        #ganttLegendDock .gantt-legend-popover strong { grid-column:1/-1;display:block;margin:0 0 1px;color:#54218d;font-size:13px;font-weight:800;line-height:1.35; }
        #ganttLegendDock .gantt-legend-popover span { display:flex;align-items:center;gap:9px;min-width:0;color:#4b5263;font-size:12.5px;font-weight:600;line-height:1.35;white-space:nowrap; }
        #ganttLegendDock .gantt-legend-popover small { grid-column:1/-1;display:block;margin-top:1px;color:#8c829c;font-size:10.5px;line-height:1.35; }
        #ganttLegendDock .gantt-key { flex-shrink:0; }
        #ganttLegendDock .gantt-key.dependency { margin-right:0; }
        #ganttInternalPanel > .gantt { height:65vh;min-height:290px; }
        @media(max-width:620px) { #ganttLegendDock .gantt-legend-popover { width:min(410px,calc(100vw - 34px));max-width:calc(100vw - 34px);grid-template-columns:repeat(2,minmax(0,1fr));column-gap:10px;row-gap:8px;padding:11px 12px; } #ganttLegendDock .gantt-legend-popover span { font-size:11.5px;white-space:normal; } #ganttInternalPanel > .gantt { height:65vh; } }
        @media(max-width:370px) { #ganttLegendDock .gantt-legend-popover { grid-template-columns:1fr;row-gap:5px; } }
        /* V3: Bảng Gantt khách hàng ưu tiên khả năng đọc: chữ rõ, tiêu đề căn giữa và phân cấp màu sắc. */
        #ganttCustomerSurface .gantt-customer-edit-table { font-size:13.5px; }
        #ganttCustomerSurface .gantt-customer-edit-table thead th { position:sticky;top:0;z-index:3;padding:12px 8px;min-height:46px;background:#eee7f5!important;color:#4e2578!important;border-bottom:2px solid #cbb6e1!important;text-align:center!important;vertical-align:middle;font-size:13.5px;font-weight:800!important;letter-spacing:.08px;line-height:1.3;white-space:nowrap; }
        #ganttCustomerSurface .gantt-customer-edit-table thead th+th { border-left:1px solid #e1d5ec; }
        #ganttCustomerSurface .gantt-customer-edit-table tbody td { padding:7px 6px;border-bottom:1px solid #e8e2ed;vertical-align:middle;color:#303a4d;font-size:13.5px;font-weight:550; }
        #ganttCustomerSurface .gantt-customer-edit-table tbody tr:nth-child(even) { background:#fcfbfe; }
        #ganttCustomerSurface .gantt-customer-edit-table tbody tr:hover { background:#f7f3fc; }
        #ganttCustomerSurface .gantt-customer-edit-table tbody td input,#ganttCustomerSurface .gantt-customer-edit-table tbody td select { min-height:42px;padding:8px 10px;border:1px solid #cec4dd;border-radius:7px;background:#fff;color:#2d3549!important;font-size:13.5px!important;font-weight:550;line-height:1.35;box-shadow:none; }
        #ganttCustomerSurface .gantt-customer-edit-table tbody td input::placeholder { color:#768093;opacity:1; }
        #ganttCustomerSurface .gantt-customer-edit-table tbody td input:focus,#ganttCustomerSurface .gantt-customer-edit-table tbody td select:focus { outline:0;border-color:#9260bd;box-shadow:0 0 0 3px rgba(109,67,177,.13); }
        #ganttCustomerSurface .gantt-customer-edit-table tbody td input:disabled,#ganttCustomerSurface .gantt-customer-edit-table tbody td select:disabled { color:#586374!important;background:#f6f5f8;opacity:1; }
        #ganttCustomerSurface .gantt-customer-edit-table tbody td input.gantt-derived-date { color:#536076!important;background:#f6f4f9!important; }
        /* V8: quan he cha/con de doc hon va tuong tu Gantt Noi bo. */
        .gantt-customer-preview .gantt-preview-toggle { flex:0 0 12px;font-size:11px;margin-right:7px;color:inherit;transition:transform .2s ease; }
        .gantt-customer-preview .gantt-preview-task-name { display:block;min-width:0;overflow:hidden;text-overflow:ellipsis;white-space:nowrap; }
        .gantt-customer-preview .task-level-1 { min-width:0;padding-right:8px; }
        .gantt-customer-preview .task-level-child { min-width:0;padding-right:8px; }
        .gantt-customer-preview .task-level-child .tree-branch { flex:0 0 14px; }
        @media(prefers-reduced-motion:reduce) { .gantt-customer-preview .gantt-preview-toggle { transition:none; } }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-4 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1"/>
                <SweetSoft:CtrlProjectTabs runat="server" ID="CtrlProjectTabs1" />
                <div class="gantt-workspace-toolbar" id="ganttWorkspaceToolbar" data-gantt-project="<%= CurrentProjectId.ToString("N") %>" data-gantt-account="<%= System.Web.HttpUtility.HtmlAttributeEncode(Context.User != null && Context.User.Identity != null ? Context.User.Identity.Name : "") %>">
                    <div class="gantt-workspace-left">
                        <span class="gantt-mode-label">Chế độ xem</span>
                        <div class="gantt-workspace-tabs" role="tablist" aria-label="Chế độ biểu đồ Gantt">
                            <button type="button" role="tab" aria-selected="true" id="ganttTabInternal" class="is-active" onclick="return GanttWorkspace.changeMode('internal');"><i class="fas fa-users"></i> Nội bộ</button>
                            <button type="button" role="tab" aria-selected="false" id="ganttTabCustomer" onclick="return GanttWorkspace.changeMode('customer');"><i class="fas fa-user-tie"></i> Khách hàng</button>
                        </div>
                        <div id="ganttLegendDock" aria-label="Vùng chú thích Gantt">
                            <button type="button" id="ganttLegendHelp" class="gantt-legend-help" aria-label="Xem và ghim chú thích Gantt" aria-controls="ganttLegendPopover" aria-expanded="false" aria-pressed="false"><i class="fas fa-question" aria-hidden="true"></i></button>
                            <div id="ganttLegendPopover" class="gantt-legend-popover" role="tooltip" aria-label="Chú thích màu sắc và trạng thái" aria-hidden="true">
                                <strong>Chú thích biểu đồ</strong>
                                <span><i class="gantt-key done" aria-hidden="true"></i> Hoàn thành</span>
                                <span><i class="gantt-key doing" aria-hidden="true"></i> Đang thực hiện</span>
                                <span><i class="gantt-key warning" aria-hidden="true"></i> Sắp đến hạn</span>
                                <span><i class="gantt-key overdue" aria-hidden="true"></i> Trễ hạn</span>
                                <span><i class="gantt-key todo" aria-hidden="true"></i> Chưa bắt đầu</span>
                                <span><i class="gantt-key today" aria-hidden="true"></i> Hôm nay</span>
                                <span><i class="gantt-key dependency" aria-hidden="true">➜</i> Liên kết phụ thuộc</span>
                                <small>Rê chuột để xem, nhấn dấu ? để ghim hoặc bỏ ghim.</small>
                            </div>
                        </div>
                    </div>
                    <div class="gantt-report-actions">
                        <button type="button" class="share-progress-trigger" id="ganttReportAction" onclick="return GanttWorkspace.openReport(event);"><i class="fas fa-file-alt"></i> <span id="ganttReportActionLabel">Tạo báo cáo</span></button>
                        <button type="button" class="share-progress-trigger gantt-report-history-trigger" onclick="return shareProgressOpenHistory(event);"><i class="fas fa-history"></i> Nhật ký báo cáo</button>
                    </div>
                </div>
                <input type="hidden" id="ganttReportMode" name="ganttReportMode" value="internal" />
                <input type="hidden" id="ganttCustomerPayload" name="ganttCustomerPayload" value="" />
                <div id="ganttInternalPanel" role="tabpanel" aria-labelledby="ganttTabInternal">
                <div class="gantt">
                    <div class="gantt-body">
                        
                        <div class="task-col">
                            <div class="task-col-header"><%= GetResourceText(BackEndResourceKeys.TASK_NAME) %></div>
                            <asp:Repeater ID="rptTaskNames" runat="server">
                                <ItemTemplate>
                                    <div class='task-row-wrap <%# GetRowClass((int)Eval("Level"), (bool)Eval("HasChild")) %>'
                                         data-id='<%# Eval("MaCongViec") %>'
                                         <%# GetClickEvents((bool)Eval("HasChild"), Eval("MaCongViec").ToString()) %>
                                         title='Người thực hiện: <%# Eval("NhanVienThucHien") %>'>

                                        <asp:PlaceHolder runat="server" Visible='<%# (int)Eval("Level") == 1 %>'>
                                            <div class='task-level-1'>
                                                <i class='fas fa-list-ul toggle-icon' style='margin-right: 8px; font-size: 14px;' runat="server" Visible='<%# (bool)Eval("HasChild") %>'></i>
                                                <%# Eval("MaCongViec") %>. <%# Eval("TenCongViec") %>
                                            </div>
                                        </asp:PlaceHolder>

                                        <asp:PlaceHolder runat="server" Visible='<%# (int)Eval("Level") > 1 %>'>
                                            <div class='tree-branch'></div>
                                            <i class='fas fa-chevron-down toggle-icon' 
                                               style='margin-right: 6px; font-size: 12px; <%# (int)Eval("Level") >= 2 && (bool)Eval("HasChild") ? "transform: rotate(-90deg);" : "" %>' 
                                               runat="server" Visible='<%# (bool)Eval("HasChild") %>'></i>
                                            <div><strong><%# Eval("MaCongViec") %>.</strong> <%# Eval("TenCongViec") %></div>
                                        </asp:PlaceHolder>
                                        
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                        
                        <div class="chart-col">
                            <div class="chart-header" id="chartHeader"></div>
                            
                            <div class="chart-tracks" id="chartTracks">
                                <svg id="svgConnections" style="position:absolute;top:0;left:0;width:100%;height:100%;pointer-events:none;z-index:5;">
                                    <defs>
                                        <marker id="arrow" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="6" markerHeight="6" orient="auto">
                                            <path d="M 0 0 L 10 5 L 0 10 z" fill="#ff9800" />
                                        </marker>
                                    </defs>
                                    <g id="linesGroup"></g>
                                </svg>
                                
                                <asp:Repeater ID="rptChartTracks" runat="server">
                                    <ItemTemplate>
                                        <div class='task-track' 
                                             data-id='<%# Eval("MaCongViec") %>' 
                                             data-taskid='<%# Eval("TaskId") %>' 
                                             data-dependson='<%# Eval("DependsOn") %>' 
                                             data-start='<%# Eval("StartDay") %>' 
                                             data-end='<%# Eval("EndDay") %>' 
                                             data-status='<%# Eval("StatusClass") %>'
                                             data-completed-late='<%# Convert.ToBoolean(Eval("IsLateCompleted")) ? "true" : "false" %>'
                                             data-haschild='<%# Eval("HasChild").ToString().ToLower() %>'>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </div>
                </div>
                </div>
                <section id="ganttCustomerPanel" role="tabpanel" aria-labelledby="ganttTabCustomer" hidden>
                    <div class="gantt-customer-surface" id="ganttCustomerSurface">
                        <div class="gantt-customer-editor-tools">
                            <div class="gantt-customer-editor-title"><strong id="ganttCustomerCount">Danh sách công việc</strong><button type="button" class="gantt-root-add" onclick="return GanttWorkspace.addTask(null);" data-gantt-ui-tip="Thêm giai đoạn mới"><i class="fas fa-plus"></i> Thêm giai đoạn</button></div>
                            <div class="gantt-customer-editor-actions">
                                <button type="button" id="ganttCustomerReset" onclick="return GanttWorkspace.resetCustomer();" data-gantt-ui-tip="Khôi phục dữ liệu dự án"><i class="fas fa-undo-alt"></i> Khôi phục</button>
                                <button type="button" id="ganttCustomerPreviewToggle" class="gantt-customer-generate" aria-label="Xem Gantt khách hàng" onclick="return GanttWorkspace.toggleCustomerView();"><i class="fas fa-chart-bar"></i> <span id="ganttCustomerPreviewToggleLabel">Xem Gantt</span></button>
                            </div>
                        </div>
                        <div class="gantt-customer-editor" id="ganttCustomerEditor">
                            <div class="gantt-customer-edit-scroll" role="region" aria-label="Bảng chỉnh dữ liệu Gantt khách hàng" tabindex="0"><table class="gantt-customer-edit-table"><thead><tr><th>Mã</th><th>Tên công việc</th><th>Bắt đầu</th><th>Kết thúc</th><th>Trạng thái</th><th>Phụ thuộc</th><th class="gantt-actions-head">Thao tác</th></tr></thead><tbody id="ganttCustomerEditorRows"></tbody></table></div>
                            <div id="ganttCustomerErrors" class="gantt-customer-errors" role="alert" aria-live="polite"></div>
                        </div>
                        <div id="ganttCustomerPreview" class="gantt-customer-preview" hidden></div>
                    </div>
                </section>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <asp:UpdatePanel ID="upIssues" runat="server">
        <ContentTemplate>
            <asp:HiddenField ID="hdfSelectedTaskId" runat="server" />
            <asp:HiddenField ID="hdfSelectedTaskCode" runat="server" />
            <asp:Button ID="btnLoadIssues" runat="server" OnClick="btnLoadIssues_Click" CssClass="d-none" />

            <SweetSoft:ExtraModal runat="server" ID="modalIssues" Type="Primary" Title="Thông tin công việc">
                <ContentTemplate>
                    
                    <h6 class="fw-bold text-primary mt-2 ms-2" id="h6TaskTitle" runat="server">
                        <i class="fas fa-info-circle me-1"></i> Thông tin công việc
                    </h6>
                    <div class="table-responsive mb-4">
                        <table class="table table-hover table-bordered mb-0 align-middle" style="font-size: 13px;">
                            <thead class="table-light">
                                <tr>
                                    <th style="width: 150px;"><%= GetResourceText(BackEndResourceKeys.TASK_CODE) %></th>
                                    <th><%= GetResourceText(BackEndResourceKeys.TASK_NAME) %></th>
                                    <th class="text-center" style="width: 140px;"><%= GetResourceText(BackEndResourceKeys.END_DATE) %></th>
                                    <th class="text-center" style="width: 130px;"><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptTaskInfo" runat="server">
                                    <ItemTemplate>
                                        <tr>
                                            <td class="fw-bold">
                                                <div style='<%# "padding-left: " + ((Convert.ToInt32(Eval("RelativeLevel")) - 1) * 16) + "px;" %>'>
                                                    <asp:PlaceHolder runat="server" Visible='<%# Convert.ToInt32(Eval("RelativeLevel")) > 1 %>'>
                                                        <div class="tree-branch"></div>
                                                    </asp:PlaceHolder>
                                                    <%# Eval("MaCongViec") %>
                                                </div>
                                            </td>
                                            
                                            <td>
                                                <a href="javascript:;" onclick="openTaskDetail('<%# Eval("IdCongViec") %>'); return false;" class="task-name-link">
                                                    <%# Eval("TenCongViec") %>
                                                </a>
                                            </td>
                                            <td class="text-center fw-bold"><%# Convert.ToDateTime(Eval("NgayKetThuc")).ToString("dd/MM/yyyy") %></td>
                                            <td class="text-center">
                                                <span class="badge <%# GetTaskStatusBadge(Convert.ToInt32(Eval("TrangThai") != DBNull.Value ? Eval("TrangThai") : 0)) %>">
                                                    <%# GetTaskStatusText(Convert.ToInt32(Eval("TrangThai") != DBNull.Value ? Eval("TrangThai") : 0)) %>
                                                </span>
                                                <%# GetTaskWarningHtml(Eval("TrangThai"), Eval("NgayKetThuc")) %>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <asp:PlaceHolder ID="phEmptyTask" runat="server" Visible='<%# rptTaskInfo.Items.Count == 0 %>'>
                                            <tr><td colspan="4" class="text-center text-muted py-2">Không có dữ liệu công việc.</td></tr>
                                        </asp:PlaceHolder>
                                    </FooterTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </div>

                    <h6 class="fw-bold text-danger ms-2"><i class="fas fa-exclamation-triangle me-1"></i> <%= GetResourceText(BackEndResourceKeys.ISSUE) %></h6>
                    
                    <asp:PlaceHolder ID="phHasIssues" runat="server">
                        <div class="table-responsive">
                            <table class="table table-hover table-bordered mb-0 align-middle" style="font-size: 13px;">
                                <thead class="table-light">
                                    <tr>
                                        <th><%= GetResourceText(BackEndResourceKeys.ISSUE_NAME) %></th>
                                        <th class="text-center" style="width: 130px;"><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <asp:Repeater ID="rptIssues" runat="server">
                                        <ItemTemplate>
                                            <tr>
                                                <td>
                                                    <a href="javascript:;" onclick="openIssueDetail('<%# Eval("IdVanDe") %>'); return false;" class="text-primary text-decoration-none fw-bold">
                                                        <%# Eval("TenVanDe") %>
                                                    </a>
                                                </td>
                                                <td class="text-center">
                                                    <span class="badge <%# GetIssueStatusBadge(Convert.ToInt32(Eval("TrangThai") != DBNull.Value ? Eval("TrangThai") : 0)) %>">
                                                        <%# GetIssueStatusText(Convert.ToInt32(Eval("TrangThai") != DBNull.Value ? Eval("TrangThai") : 0)) %>
                                                    </span>
                                                </td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>
                        </div>
                    </asp:PlaceHolder>

                    <asp:PlaceHolder ID="phNoIssues" runat="server" Visible="false">
                        <div class="ms-2 mb-3 text-muted fst-italic">Không có vấn đề!</div>
                    </asp:PlaceHolder>

                </ContentTemplate>
            </SweetSoft:ExtraModal>
        </ContentTemplate>
    </asp:UpdatePanel>

    <!-- Nút ẩn để gọi Server mở Popup 2 -->
    <asp:UpdatePanel ID="upViewIssueTrigger" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:HiddenField ID="hdfIssueIdToView" runat="server" />
            <asp:Button ID="btnTriggerViewIssue" runat="server" CssClass="d-none" OnClick="btnTriggerViewIssue_Click" />
        </ContentTemplate>
    </asp:UpdatePanel>

    <SweetSoft:CtrlViewIssueDetail runat="server" ID="CtrlViewIssueDetail1" />
    <asp:UpdatePanel ID="upViewTaskTrigger" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:HiddenField ID="hdfTaskIdToView" runat="server" />
            <asp:Button ID="btnTriggerViewTask" runat="server" CssClass="d-none" OnClick="btnTriggerViewTask_Click" />
        </ContentTemplate>
    </asp:UpdatePanel>

    <SweetSoft:CtrlViewTaskDetail runat="server" ID="CtrlViewTaskDetail1" />
    <SweetSoft:CtrlShareProgress runat="server" ID="CtrlShareProgress1" />
    <SweetSoft:CtrlProgressReportHistory runat="server" ID="CtrlProgressReportHistory1" />
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script>
var shouldReopenTaskInfo = false;
        function openTaskDetail(taskId) {
            shouldReopenTaskInfo = true;

            // Ép Bootstrap TẮT Popup 1 ngay lập tức
            $('#<%= modalIssues.ClientID %>').modal('hide');

            // Chờ 350ms cho hiệu ứng tắt xong xuôi rồi gọi Server bật Popup Task
            setTimeout(function () {
                document.getElementById('<%= hdfTaskIdToView.ClientID %>').value = taskId;
                document.getElementById('<%= btnTriggerViewTask.ClientID %>').click();
            }, 350);
        }
        function openIssueDetail(issueId) {
            shouldReopenTaskInfo = true;

            // 1. Ép Bootstrap TẮT Popup 1 ngay lập tức
            $('#<%= modalIssues.ClientID %>').modal('hide');

            // 2. Chờ 350ms cho hiệu ứng tắt xong xuôi và lớp nền đen biến mất
            // Rồi mới bấm nút ẩn để gọi Server vẽ lên Popup 2
            setTimeout(function () {
                document.getElementById('<%= hdfIssueIdToView.ClientID %>').value = issueId;
                document.getElementById('<%= btnTriggerViewIssue.ClientID %>').click();
            }, 350);
        }

        function openShareProgressSelector(e) {
            if (e) e.preventDefault();
            if (window.ShareProgressUI) return window.ShareProgressUI.openSelection(e);
            return false;
        }

        function bindModalEvents() {
            // Xóa rác event cũ (phòng khi postback UpdatePanel làm nhân đôi event)
            $(document).off('hidden.bs.modal.ganttReopen');

            // Lắng nghe sự kiện: Lúc nào có 1 cái Modal nào đó VỪA BỊ ĐÓNG LẠI XONG
            $(document).on('hidden.bs.modal.ganttReopen', '.modal', function (e) {
                // Ta soi xem: Nếu cái Modal vừa bị đóng là thằng Popup 2 (CtrlViewIssueDetail) 
                // VÀ cờ shouldReopenTaskInfo đang bật
                if (e.target.id.indexOf('mdlIssueView') !== -1 || e.target.id.indexOf('mdlTaskView') !== -1) {
                    if (shouldReopenTaskInfo) {
                        shouldReopenTaskInfo = false;

                        var btnLoad = document.getElementById('<%= btnLoadIssues.ClientID %>');
                        if (btnLoad) btnLoad.click();
                    }
                }
            });
        }

        if (typeof Sys !== 'undefined' && Sys.Application) {
            Sys.Application.add_load(function () {
                bindModalEvents();
                if (document.getElementById("chartHeader").innerHTML === '') {
                    renderGanttChart();
                }
            });
        } else {
            window.addEventListener('DOMContentLoaded', function () {
                bindModalEvents();
                renderGanttChart();
            });
        }

        // ==========================================
        // CÁC HÀM CŨ GIỮ NGUYÊN (VẼ BIỂU ĐỒ GANTT)
        // ==========================================
        function toggleTask(maCv, element) {
            element.classList.toggle('collapsed');
            const icon = element.querySelector('.toggle-icon');
            if (icon) {
                const isCollapsed = element.classList.contains('collapsed');
                icon.style.transform = isCollapsed ? 'rotate(-90deg)' : 'rotate(0deg)';
            }
            updateVisibility();
        }

        function updateVisibility() {
            const collapsedIds = Array.from(document.querySelectorAll('.task-row-wrap.collapsed')).map(el => el.dataset.id);
            const allRows = document.querySelectorAll('.task-row-wrap');
            const allTracks = document.querySelectorAll('.task-track');

            const svgGroup = document.getElementById("linesGroup");
            if (svgGroup) svgGroup.innerHTML = '';

            allRows.forEach((row, index) => {
                const track = allTracks[index];
                const id = row.dataset.id;

                let isHidden = false;
                for (let cid of collapsedIds) {
                    if (id !== cid && id.startsWith(cid + ".")) { isHidden = true; break; }
                }

                if (isHidden) {
                    row.classList.add('hidden-task');
                    if (track) {
                        track.classList.add('hidden-task');
                        const bar = track.querySelector('.bar');
                        if (bar) bar.style.width = '0%';
                    }
                } else {
                    const wasHidden = row.classList.contains('hidden-task');
                    row.classList.remove('hidden-task');

                    if (track) {
                        track.classList.remove('hidden-task');
                        if (wasHidden) {
                            const bar = track.querySelector('.bar');
                            if (bar) { setTimeout(() => { bar.style.width = bar.dataset.targetwidth; }, 300); }
                        }
                    }
                }

                if (track) {
                    const hasChild = track.dataset.haschild === 'true';
                    if (hasChild) {
                        const isCollapsed = row.classList.contains('collapsed');
                        if (!isCollapsed) track.classList.add('hide-bar');
                        else track.classList.remove('hide-bar');
                    }
                }
            });

            setTimeout(drawDependencies, 400);
        }

        function drawDependencies() {
            const svgGroup = document.getElementById("linesGroup");
            if (!svgGroup) return;
            svgGroup.innerHTML = '';

            const tracks = document.querySelectorAll(".task-track:not(.hidden-task):not(.hide-bar)");
            tracks.forEach(curTrack => {
                const depId = curTrack.dataset.dependson;
                if (!depId || depId === "") return;

                const preTrack = document.querySelector(`.task-track[data-taskid='${depId}']`);
                if (!preTrack || preTrack.classList.contains("hidden-task") || preTrack.classList.contains("hide-bar")) return;

                const curBar = curTrack.querySelector(".bar");
                const preBar = preTrack.querySelector(".bar");
                if (!curBar || !preBar || curBar.style.width === '0%' || preBar.style.width === '0%') return;

                const startX = preBar.offsetLeft + (preBar.offsetWidth / 2);
                const startY = preTrack.offsetTop + preBar.offsetTop + preBar.offsetHeight;

                const endX = curBar.offsetLeft;
                const endY = curTrack.offsetTop + curBar.offsetTop + (curBar.offsetHeight / 2);

                let pathData = "";
                if (endX >= startX) {
                    pathData = `M ${startX} ${startY} L ${startX} ${endY} L ${endX - 3} ${endY}`;
                } else {
                    pathData = `M ${startX} ${startY} L ${startX} ${startY + 10} L ${endX - 10} ${startY + 10} L ${endX - 10} ${endY} L ${endX - 3} ${endY}`;
                }

                const pathEl = document.createElementNS("http://www.w3.org/2000/svg", "path");
                pathEl.setAttribute("d", pathData);
                pathEl.setAttribute("class", "dep-line");
                pathEl.setAttribute("marker-end", "url(#arrow)");
                svgGroup.appendChild(pathEl);
            });
        }

        function renderGanttChart() {
            if (typeof GANTT_TOTAL_DAYS === 'undefined') return;

            const MIN_DAY_WIDTH = 60;
            const ganttViewWidth = document.querySelector('.gantt').clientWidth;
            const chartColWidth = ganttViewWidth - 350;
            const totalWidth = Math.max(GANTT_TOTAL_DAYS * MIN_DAY_WIDTH, chartColWidth);

            const header = document.getElementById("chartHeader");
            const chartTracks = document.getElementById("chartTracks");

            header.style.width = totalWidth + 'px';
            chartTracks.style.width = totalWidth + 'px';

            const pct = (idx) => (idx / GANTT_TOTAL_DAYS) * 100;

            header.innerHTML = '';
            GANTT_DATE_LABELS.forEach((label, i) => {
                const cellWidthPct = (1 / GANTT_TOTAL_DAYS) * 100;
                const tick = document.createElement("div");
                tick.className = "tick-label"; tick.style.left = pct(i) + "%"; tick.style.width = cellWidthPct + "%"; tick.textContent = label;
                header.appendChild(tick);
                const line = document.createElement("div");
                line.className = "tick-line"; line.style.left = pct(i) + "%";
                header.appendChild(line);
            });

            const tracks = document.querySelectorAll(".task-track");
            tracks.forEach((track) => {
                const start = parseFloat(track.dataset.start);
                const end = parseFloat(track.dataset.end);
                const status = track.dataset.status;
                const completedLate = track.dataset.completedLate === "true";

                const taskId = track.dataset.taskid;
                const taskCode = track.dataset.id;

                GANTT_DATE_LABELS.forEach((label, i) => {
                    const gl = document.createElement("div"); gl.className = "grid-line"; gl.style.left = pct(i) + "%";
                    track.appendChild(gl);
                });

                // Trang thai goc quyet dinh noi dung tren thanh Gantt. Canh bao han chi doi MAU voi 'doing'.
                const statusLabel = status === "done" ? "<%= GetResourceText(BackEndResourceKeys.COMPLETED) %>" :
                    (status === "doing" ? "<%= GetResourceText(BackEndResourceKeys.DOING) %>" : "<%= GetResourceText(BackEndResourceKeys.NOT_YET_STARTED) %>");
                let effectiveClass = status;
                let deadlineWarning = "";
                let warningKind = "";
                if (status !== "done") {
                    const remainingDays = end - GANTT_TODAY_INDEX - 1; // EndDay la chi so doc quyen (+1), chuyen ve ngay ket thuc thuc te.
                    if (remainingDays < 0) {
                        deadlineWarning = `Trễ hạn ${Math.abs(remainingDays)} ngày`;
                        warningKind = "overdue";
                    } else if (remainingDays <= 2) {
                        deadlineWarning = remainingDays === 0 ? "Sắp đến hạn (hôm nay)" : `Sắp đến hạn (còn ${remainingDays} ngày)`;
                        warningKind = "warning";
                    }
                    // Chua bat dau giu mau xam ke ca qua han; chi Dang thuc hien moi doi mau.
                    if (status === "doing" && warningKind) effectiveClass = warningKind;
                }

                const targetWidth = (pct(end) - pct(start)) + "%";

                const bar = document.createElement("div");
                bar.className = "bar status-" + effectiveClass;
                bar.style.left = pct(start) + "%";
                bar.style.width = "0%";
                bar.dataset.ganttTip = completedLate ? "Hoàn thành trễ" : (deadlineWarning || statusLabel);
                // Mau tooltip theo canh bao, con mau thanh theo trang thai (tru 'doing').
                bar.dataset.ganttKind = completedLate ? "late" : (warningKind || effectiveClass);
                bar.dataset.targetwidth = targetWidth;

                const fill = document.createElement("div");
                fill.className = "bar-fill animated-stripes";
                fill.style.left = "0"; fill.style.width = "100%";

                const slideTxt = document.createElement("div");
                slideTxt.className = "static-text";
                // Khong thay chu trang thai bang noi dung canh bao han.
                slideTxt.textContent = statusLabel;

                fill.appendChild(slideTxt);
                bar.appendChild(fill);
                if (completedLate) {
                    bar.classList.add("is-completed-late");
                    slideTxt.textContent = "Hoàn thành trễ";
                    slideTxt.style.color = "#ffffff";
                    slideTxt.style.textShadow = "none";
                }

                bar.onclick = function (e) {
                    e.stopPropagation();
                    document.getElementById('<%= hdfSelectedTaskId.ClientID %>').value = taskId;
                    document.getElementById('<%= hdfSelectedTaskCode.ClientID %>').value = taskCode;
                    document.getElementById('<%= btnLoadIssues.ClientID %>').click();
                };

                track.appendChild(bar);
                setTimeout(() => { bar.style.width = targetWidth; }, 100);
            });

            const todayLine = document.createElement("div");
            todayLine.className = "today-line";
            todayLine.style.left = pct(GANTT_TODAY_INDEX + 0.5) + "%";

            const todayLabel = document.createElement("div");
            todayLabel.className = "today-label";
            todayLabel.textContent = "<%= GetResourceText(BackEndResourceKeys.TODAY) %>";
            todayLabel.style.left = pct(GANTT_TODAY_INDEX + 0.5) + "%";

            chartTracks.appendChild(todayLine);
            chartTracks.appendChild(todayLabel);

            updateVisibility();
        }
    </script>
    <script>
(function (w, d) {
    'use strict';
    if (w.__ganttBarTooltipReady) return;
    w.__ganttBarTooltipReady = true;
    var node = null, active = null;
    function getNode() {
        if (!node) {
            node = d.createElement('div');
            node.className = 'gantt-bar-tip';
            node.setAttribute('role', 'tooltip');
            node.setAttribute('aria-hidden', 'true');
            d.body.appendChild(node);
        }
        return node;
    }
    function position(bar, pointerX) {
        var tip = getNode();
        if (!bar || !bar.isConnected || tip.style.display === 'none') return;
        var rect = bar.getBoundingClientRect();
        if (rect.bottom < 0 || rect.top > w.innerHeight) { hide(); return; }
        var width = tip.offsetWidth, height = tip.offsetHeight;
        var x = typeof pointerX === 'number' ? pointerX : (Math.max(0, rect.left) + Math.min(w.innerWidth, rect.right)) / 2;
        var left = Math.max(10, Math.min(w.innerWidth - width - 10, x - width / 2));
        var above = rect.top - height - 12;
        var below = above < 8;
        tip.classList.toggle('is-below', below);
        tip.style.left = Math.round(left) + 'px';
        tip.style.top = Math.round(below ? rect.bottom + 12 : above) + 'px';
    }
    function show(bar, pointerX) {
        if (!bar || !bar.dataset.ganttTip) return;
        active = bar;
        var tip = getNode();
        tip.textContent = bar.dataset.ganttTip;
        tip.dataset.kind = bar.dataset.ganttKind || 'todo';
        tip.style.display = 'block';
        tip.setAttribute('aria-hidden', 'false');
        position(bar, pointerX);
    }
    function hide() {
        active = null;
        if (node) { node.style.display = 'none'; node.setAttribute('aria-hidden', 'true'); }
    }
    function closestBar(target) {
        return target && target.closest ? target.closest('.bar[data-gantt-tip]') : null;
    }
    d.addEventListener('mouseover', function (e) {
        var bar = closestBar(e.target);
        if (bar && bar !== active) show(bar, e.clientX);
    });
    d.addEventListener('mousemove', function (e) {
        if (active && closestBar(e.target) === active) position(active, e.clientX);
    });
    d.addEventListener('mouseout', function (e) {
        if (active && active.contains(e.target) && (!e.relatedTarget || !active.contains(e.relatedTarget))) hide();
    });
    d.addEventListener('focusin', function (e) { var bar = closestBar(e.target); if (bar) show(bar); });
    d.addEventListener('focusout', function (e) { if (active && active.contains(e.target)) hide(); });
    d.addEventListener('scroll', hide, true);
    w.addEventListener('blur', hide);
})(window, document);
    </script>
    <script src="<%= ResolveUrl("~/fGanttCharts/gantt-workspace.js") %>?v=8" type="text/javascript"></script>
    <script>
    // Chú thích cạnh bộ chọn chế độ: hover mở popup, click ghim, click lần nữa đóng.
    (function (w, d) {
        'use strict';
        function initGanttLegendPopup() {
            var dock = d.getElementById('ganttLegendDock');
            var help = d.getElementById('ganttLegendHelp');
            var legend = d.getElementById('ganttLegendPopover');
            if (!dock || !help || !legend || help.getAttribute('data-gantt-legend-ready') === '1') return;
            help.setAttribute('data-gantt-legend-ready', '1');
            help.removeAttribute('title'); // Không sinh tooltip đen mặc định của trình duyệt.
            var pinned = false;
            var hovered = false;
            var keyboardFocus = false;
            var suppressPreview = false;
            function positionLegend() {
                // Vi tri mac dinh: phia tren - ben phai cua NUT ?, khong phai mep phai trang.
                legend.style.removeProperty('left');
                var dockRect = dock.getBoundingClientRect();
                var desiredLeft = help.offsetWidth + 10;
                var minLeft = 12 - dockRect.left;
                var maxLeft = w.innerWidth - 12 - dockRect.left - legend.offsetWidth;
                // Chi dich vao trong neu man hinh hep, tranh popup bi cat o canh phai.
                var actualLeft = Math.max(minLeft, Math.min(desiredLeft, maxLeft));
                legend.style.setProperty('left', Math.round(actualLeft) + 'px', 'important');
            }
            function show(open) {
                legend.classList.toggle('is-open', open);
                if (open) positionLegend();
                legend.setAttribute('aria-hidden', open ? 'false' : 'true');
                help.classList.toggle('is-pinned', pinned);
                help.setAttribute('aria-pressed', pinned ? 'true' : 'false');
                help.setAttribute('aria-expanded', open ? 'true' : 'false');
                help.setAttribute('aria-label', pinned ? 'Bỏ ghim chú thích Gantt' : 'Xem và ghim chú thích Gantt');
            }
            function sync() { show(pinned || ((hovered || keyboardFocus) && !suppressPreview)); }
            // Chỉ hover lên chính nút ? mới xem trước; popup không bắt chuột để tránh nhấp nháy.
            help.addEventListener('mouseenter', function (event) { event.stopImmediatePropagation(); hovered = true; sync(); }, true);
            help.addEventListener('mouseleave', function (event) { event.stopImmediatePropagation(); hovered = false; suppressPreview = false; sync(); }, true);
            ['mouseover', 'mouseout'].forEach(function (type) {
                help.addEventListener(type, function (event) { event.stopImmediatePropagation(); }, true);
            });
            dock.addEventListener('mouseover', function (event) { event.stopPropagation(); }, true);
            dock.addEventListener('mouseout', function (event) { event.stopPropagation(); }, true);
            help.addEventListener('click', function (event) {
                event.preventDefault();
                event.stopImmediatePropagation();
                pinned = !pinned;
                if (!pinned) { suppressPreview = true; keyboardFocus = false; }
                else suppressPreview = false;
                sync();
            }, true);
            help.addEventListener('focus', function () {
                keyboardFocus = !!(help.matches && help.matches(':focus-visible'));
                sync();
            }, true);
            help.addEventListener('blur', function () { keyboardFocus = false; sync(); }, true);
            w.addEventListener('resize', function () { if (legend.classList.contains('is-open')) positionLegend(); });
            show(false);
        }
        if (d.readyState === 'loading') d.addEventListener('DOMContentLoaded', initGanttLegendPopup);
        else initGanttLegendPopup();
        if (typeof Sys !== 'undefined' && Sys.Application) Sys.Application.add_load(initGanttLegendPopup);
    })(window, document);
    </script>
</asp:Content>