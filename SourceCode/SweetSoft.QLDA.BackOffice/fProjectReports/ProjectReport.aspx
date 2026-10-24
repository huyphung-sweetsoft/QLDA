<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="ProjectReport.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjectReports.ProjectReport" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/chartjs-plugin-datalabels@2.2.0/dist/chartjs-plugin-datalabels.min.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
.page-title h2 { font-size: 20px; color: #1e293b; font-weight: bold; margin: 10px 0 25px 0; text-transform: uppercase; }
.report-layout { display: flex; flex-direction: column; gap: 25px; align-items: flex-start; }
@media (min-width: 1200px) { .report-layout { flex-direction: row; } }
.sidebar-left { width: 100%; display: flex; flex-direction: column; gap: 15px; }
@media (min-width: 1200px) { .sidebar-left { width: 220px; flex-shrink: 0; position: sticky; top: 20px; } }
.form-group-vertical { display: flex; flex-direction: column; gap: 6px; }
.form-group-vertical label { font-size: 13.5px; font-weight: 600; color: #475569; margin: 0; }
.form-group-vertical .form-control { width: 100%; padding: 8px 12px; border: 1px solid #cbd5e1; border-radius: 6px; font-size: 13.5px; outline: none; transition: border-color 0.2s; }
.form-group-vertical .form-control:focus { border-color: #3b82f6; }
.btn-filter { width: 100%; background-color: #2563eb; color: white; padding: 10px; border-radius: 6px; font-weight: 600; font-size: 14px; border: none; transition: background 0.2s; text-align: center; text-decoration: none; display: flex; justify-content: center; align-items: center; gap: 8px; }
.btn-filter:hover { background-color: #1d4ed8; color: white; }
.btn-export { width: 100%; display: flex; align-items: center; justify-content: center; gap: 8px; padding: 10px; border-radius: 6px; font-size: 14px; font-weight: 600; border: none; transition: all 0.2s; text-decoration: none; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
.btn-export-pdf { background-color: #ef4444; color: white; margin-top: 5px; }
.btn-export-pdf:hover { background-color: #dc2626; color: white; transform: translateY(-2px); box-shadow: 0 4px 8px rgba(220,38,38,0.25); }
.main-center { flex-grow: 1; width: 100%; min-width: 0; }
.report-paper { background: #ffffff; border-radius: 12px; box-shadow: 0 10px 40px -10px rgba(0,0,0,0.08), 0 1px 3px rgba(0,0,0,0.05); padding: 45px 40px; width: 100%; border: 1px solid #f1f5f9; position: relative; }
.report-paper::before { content: ''; position: absolute; top: 0; left: 0; right: 0; height: 5px; background: linear-gradient(90deg, #3b82f6, #8b5cf6); border-radius: 12px 12px 0 0; }
.report-header { text-align: center; margin-bottom: 35px; padding-bottom: 20px; border-bottom: 1px dashed #cbd5e1; }
.report-header h1 { font-size: 24px; color: #0f172a; font-weight: 800; text-transform: uppercase; letter-spacing: 1px; margin: 0 0 8px 0; }
.report-header .sub-date { font-size: 14px; color: #64748b; font-weight: 500; font-style: italic; }
.kpi-summary-bar { display: flex; flex-wrap: wrap; justify-content: space-between; background-color: #ffffff; border: 1px solid #e2e8f0; border-radius: 10px; padding: 20px 15px; margin-bottom: 30px; box-shadow: 0 2px 4px rgba(0,0,0,0.02); gap: 15px;}
.kpi-item { flex: 1; text-align: center; min-width: 120px; border-right: 1px solid #e2e8f0; padding: 0 10px; }
.kpi-item:last-child { border-right: none; }
.kpi-title { font-size: 12px; color: #64748b; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; display: block; margin-bottom: 8px; }
.kpi-number { font-size: 24px; font-weight: 800; display: block; line-height: 1.1; }
.kpi-number.total { color: #3b82f6; }
.kpi-number.success { color: #10b981; }
.kpi-number.danger { color: #ef4444; }
.kpi-number.warning { color: #f59e0b; }
.section-title { font-size: 16px; font-weight: 700; color: #0f172a; margin-top: 35px; margin-bottom: 15px; padding-left: 12px; border-left: 4px solid #3b82f6; line-height: 1.2; }
.section-title.success { border-left-color: #10b981; color: #047857; }
.section-title.info { border-left-color: #0ea5e9; color: #0369a1; }
.section-title.muted { border-left-color: #94a3b8; color: #475569; }
.section-title.danger { border-left-color: #ef4444; color: #dc2626; }
.section-title.dashboard { border-left-color: #8b5cf6; color: #6d28d9; margin-top: 0; }
.dashboard-container { display: flex; gap: 20px; flex-wrap: wrap; margin-bottom: 30px; }
.chart-box { flex: 1; min-width: 320px; height: 260px; padding: 15px; }
.chart-title { font-size: 13px; font-weight: 700; text-align: center; margin-bottom: 8px; text-transform: uppercase; color: #111827; }
.chart-box canvas { width: 100% !important; height: 230px !important; cursor: default; }
.report-chart-highlight td { background-color: #fff7cc !important; color: #111827 !important; }
.report-chart-highlight { outline: 2px solid #facc15; outline-offset: -2px; }
.section-title { scroll-margin-top: 100px; scroll-margin-block-start: 100px; }
.section-title.report-chart-section-highlight {
    background: #fff7cc; color: #111827 !important; outline: 2px solid #facc15; outline-offset: 5px;
    border-radius: 2px; padding-top: 8px; padding-bottom: 8px;
    animation: reportChartHeadingPulse 1.15s ease-in-out 2;
}
@keyframes reportChartHeadingPulse {
    0%, 100% { background-color: #fff7cc; }
    50% { background-color: #ffec99; }
}
.deadline-label { margin-top: 4px; }
.report-paper .table { table-layout: fixed; width: 100%; border-color: #e2e8f0; margin-bottom: 0; word-wrap: break-word; }
.report-paper .table thead th { background-color: #f8fafc; color: #475569; font-weight: 700; text-transform: uppercase; font-size: 12px; letter-spacing: 0.5px; padding: 14px 12px; border-bottom: 2px solid #cbd5e1; vertical-align: middle; white-space: normal; word-break: break-word; }
.report-paper .table tbody td { padding: 14px 12px; color: #334155; vertical-align: middle; font-size: 13.5px; white-space: normal !important; word-break: break-word; line-height: 1.5; }
.report-paper .table tbody tr:hover { background-color: #f8fafc; }
.task-code-name { line-height: 1.45; }
.task-code-chip { display: inline-block; vertical-align: middle; min-width: 36px; margin-right: 8px; padding: 3px 7px; border-radius: 5px; background: #eff6ff; color: #1d4ed8; border: 1px solid #bfdbfe; font-size: 12px; font-weight: 700; text-align: center; line-height: 1.35; }
.task-name-text { display: inline; vertical-align: middle; color: #334155; font-weight: 500; overflow-wrap: anywhere; }
.report-badge { padding: 4px 10px; border-radius: 20px; font-size: 12px; font-weight: 600; display: inline-block; white-space: nowrap; line-height: 1.2; text-align: center; }
.badge-success { background-color: #dcfce7; color: #166534; border: 1px solid #bbf7d0; }
.badge-doing { background-color: #e0f2fe; color: #0369a1; border: 1px solid #bae6fd; }
.badge-todo { background-color: #f1f5f9; color: #475569; border: 1px solid #e2e8f0; }
.badge-danger { background-color: #fee2e2; color: #dc2626; border: 1px solid #fca5a5; }
.badge-warning { background-color: #fef08a; color: #854d0e; border: 1px solid #fde047; }
.badge-high { background-color: #fef2f2; color: #991b1b; }
.badge-med { background-color: #fffbeb; color: #d97706; }
.badge-low { background-color: #ecfdf5; color: #059669; }
.badge-info { background-color: #e0f2fe; color: #0369a1; }
.empty-state { text-align: center; padding: 60px 20px; border-radius: 8px; border: 1px dashed #cbd5e1; margin-top: 20px; }
.empty-state i { font-size: 50px; color: #94a3b8; margin-bottom: 15px; }
.empty-state h4 { color: #334155; font-weight: 700; font-size: 18px; margin-bottom: 5px; }
.empty-state p { color: #64748b; font-size: 14px; margin:0; }
/* Khung bộ lọc đứng yên; chỉ cuộn vùng giấy báo cáo trên desktop. */
@media (min-width:1200px) { .report-layout { flex-direction:row;align-items:stretch;gap:22px;min-height:240px;overflow:hidden; } }
@media (min-width:1200px) { .report-layout .sidebar-left { position:relative;top:auto;width:230px;flex:0 0 230px;align-self:stretch;min-height:0;overflow-y:auto;overflow-x:hidden;overscroll-behavior:contain;padding:1px 4px 12px 0;scrollbar-width:thin; } }
@media (min-width:1200px) { .report-layout .main-center { flex:1 1 auto;min-width:0;min-height:0;height:100%;overflow-y:auto;overflow-x:hidden;overscroll-behavior:contain;scrollbar-width:thin;scrollbar-gutter:stable; } }
/* Mục lục báo cáo, mở mặc định; mũi tên lên khi mở và xuống khi thu gọn. */
.report-toc { min-width:0;margin-top:8px;border:1px solid #e0d5ef;border-radius:11px;background:#fff;overflow:hidden;box-shadow:0 2px 10px rgba(60,27,111,.05); }
.report-toc-head { display:flex;align-items:center;justify-content:space-between;gap:8px;padding:10px 11px;background:#f7f3fc;color:#54218d;font-size:12.5px;font-weight:800; }
.report-toc-title { display:flex;align-items:center;gap:8px;min-width:0; }
.report-toc-toggle { display:inline-flex;align-items:center;justify-content:center;flex:0 0 31px;width:31px;height:31px;padding:0;border:1px solid #dbcaf1;border-radius:8px;background:#fff;color:#5a2493;font-size:12px;cursor:pointer;transition:background .18s,border-color .18s; }
.report-toc-toggle:hover { background:#eee3fc;border-color:#b991e5; }
.report-toc-toggle:focus-visible,.report-toc-link:focus-visible { outline:2px solid #a57bd9;outline-offset:2px; }
.report-toc-toggle .report-toc-arrow { display:inline-block;transition:transform .2s ease; }
.report-toc.is-collapsed .report-toc-toggle .report-toc-arrow { transform:rotate(180deg); }
.report-toc-list { display:flex;flex-direction:column;gap:2px;max-height:320px;padding:8px;opacity:1;overflow:hidden;transition:max-height .25s ease,padding .2s ease,opacity .18s ease; }
.report-toc.is-collapsed .report-toc-list { max-height:0;padding-top:0;padding-bottom:0;opacity:0;visibility:hidden;pointer-events:none; }
.report-toc-link { display:flex;align-items:flex-start;gap:8px;width:100%;padding:8px 7px;min-height:34px;border:1px solid transparent;border-radius:7px;background:transparent;color:#625675;font-size:11.8px;font-weight:650;line-height:1.45;text-align:left;cursor:pointer;transition:background .15s,color .15s; }
.report-toc-link:hover,.report-toc-link.is-active { border-color:#e2d0f7;background:#f3edfc;color:#54218d; }
.report-toc-index { flex:0 0 21px;color:#7940b0;font-weight:800; }
.report-toc-link-label { flex:1;min-width:0; }
/* Nhân sự báo cáo: phần thu gọn luôn một hàng, toàn ô là vùng tương tác. */
#doingTasksTable td.report-assignee-cell,
#todoTasksTable td.report-assignee-cell { cursor:pointer;vertical-align:middle;transition:background-color .16s ease; }
#doingTasksTable td.report-assignee-cell:hover,
#todoTasksTable td.report-assignee-cell:hover { background:#f8f4fe; }
.report-assignee-box { display:block;width:100%;min-width:0;max-width:100%;border-radius:9px;outline:none;cursor:pointer; }
.report-assignee-box:focus-visible { outline:2px solid #8b5cf6;outline-offset:3px; }
.report-assignee-compact { display:flex;flex-wrap:nowrap;align-items:center;gap:5px;min-width:0;width:100%;overflow:hidden; }
.report-person-chip { display:inline-flex;align-items:center;flex:0 1 auto;min-width:0;max-width:100%;padding:5px 8px;border:1px solid #e0e4ed;border-radius:7px;background:#f8f9fc;color:#48536a;font-size:11.5px;font-weight:650;line-height:1.35;white-space:nowrap;overflow:hidden;text-overflow:ellipsis; }
.report-person-chip-text { display:block;min-width:0;overflow:hidden;text-overflow:ellipsis;white-space:nowrap; }
.report-assignee-more { display:inline-flex;align-items:center;justify-content:center;flex:0 0 31px;width:31px;height:31px;margin-left:auto;padding:0;border:1px solid #d6b9f0;border-radius:9px;background:#f0e7fb;color:#6630a2;font-size:11px;font-weight:800;line-height:1;white-space:nowrap;transition:background .16s,border-color .16s,color .16s; }
.report-assignee-cell:hover .report-assignee-more { background:#e8d9fa;border-color:#bd90e9; }
.report-assignee-box.is-expanded .report-assignee-more { background:#54218d;color:#fff;border-color:#54218d; }
.report-assignee-box.is-expanded .report-assignee-compact { justify-content:space-between; }
.report-assignee-expanded-title { display:none;min-width:0;flex:1 1 auto;color:#6b4398;font-size:11px;font-weight:800;white-space:nowrap;overflow:hidden;text-overflow:ellipsis; }
.report-assignee-box.is-expanded .report-assignee-expanded-title { display:inline-block; }
.report-assignee-box.is-expanded .report-person-chip { display:none; }
.report-assignee-expanded { display:none;flex-direction:column;gap:3px;margin-top:6px;padding:7px;border:1px solid #e4d7f3;border-radius:9px;background:#fdfbff; }
.report-assignee-box.is-expanded .report-assignee-expanded { display:flex; }
.report-person-row { display:flex;align-items:center;justify-content:space-between;gap:8px;min-width:0;padding:6px 4px;border-bottom:1px dashed #e9e1f1;color:#334155; }
.report-person-row:last-child { border-bottom:0; }
.report-person-identity { display:flex;align-items:center;gap:9px;min-width:0;flex:1 1 auto; }
.report-person-avatar { width:32px;height:32px;flex:0 0 32px;display:flex;align-items:center;justify-content:center;overflow:hidden;border:1px solid #d7c6eb;border-radius:50%;background:#f1e9fc;color:#592e8f;font-size:11px;font-weight:850; }
.report-person-avatar img { display:block;width:100%;height:100%;object-fit:cover; }
.report-person-avatar .report-avatar-fallback { display:none;width:100%;height:100%;align-items:center;justify-content:center;color:#fff;font-size:10px;font-weight:800; }
.report-person-avatar.is-fallback { background:var(--report-avatar-bg,#7c3aed);border-color:transparent;color:#fff; }
.report-person-avatar.is-fallback .report-avatar-fallback { display:flex; }
.report-person-copy { display:flex;flex-direction:column;gap:2px;min-width:0;flex:1 1 auto; }
.report-person-name { display:block;min-width:0;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;color:#35415a;font-size:11.8px;font-weight:750;line-height:1.3; }
.report-person-email { display:block;min-width:0;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;color:#8992a4;font-size:10.5px;font-weight:500;line-height:1.25; }
.report-person-hours { flex:0 0 auto;color:#7043a1;font-size:10.8px;font-weight:800;white-space:nowrap; }
.report-task-hours { display:none;margin-left:7px;color:#735391;font-size:11.5px;font-weight:750;white-space:nowrap; }
.report-row-expanded .report-task-hours { display:inline; }
/* Tooltip được đưa ra body nên không bị bảng/cuộn nội bộ cắt mất. */
.report-assignee-tooltip { position:fixed;z-index:99990;box-sizing:border-box;width:320px;max-width:calc(100vw - 20px);max-height:min(420px,calc(100vh - 24px));overflow-y:auto;padding:12px 13px;border:1px solid #dbc7ee;border-radius:12px;background:#fff;color:#342641;box-shadow:0 14px 35px rgba(43,22,76,.18);pointer-events:none;scrollbar-width:thin; }
.report-assignee-tooltip-title { display:flex;align-items:center;gap:7px;margin:0 0 7px;padding-bottom:8px;border-bottom:1px solid #eee7f6;color:#54218d;font-size:12.5px;font-weight:800; }
.report-assignee-tooltip .report-person-row { padding:7px 1px; }
.report-assignee-tooltip .report-person-avatar {width:35px;height:35px;flex-basis:35px;}
.report-assignee-tooltip .report-person-name {font-size:12px;}
.report-assignee-tooltip .report-person-email {font-size:11px;}
.report-assignee-tooltip-note { margin-top:9px;padding-top:8px;border-top:1px dashed #ebe2f4;color:#80579c;font-size:11.2px;font-weight:700;line-height:1.4; }
/* Bố cục báo cáo: ưu tiên nhân sự, hai mốc thời gian và trạng thái gọn. */
#doingTasksTable td.report-assignee-cell,#todoTasksTable td.report-assignee-cell { padding:11px 10px; }
#doingTasksTable td:last-child,#todoTasksTable td:last-child { padding-left:7px;padding-right:7px; }
#doingTasksTable .report-badge,#todoTasksTable .report-badge { max-width:100%;font-size:11.5px;padding:5px 8px; }
.report-task-hours { display:inline-flex !important;align-items:center;gap:5px;margin:5px 0 0 3px;padding:3px 7px;border:1px solid #e4d5f6;border-radius:6px;background:#faf7fe;color:#63328e;font-size:11px;font-weight:750;line-height:1.35;white-space:nowrap;vertical-align:middle; }
.report-task-hours i { font-size:10px;color:#8654bc; }
.report-period-cell { padding:10px !important; }
.report-planned-period { display:flex;flex-direction:column;gap:5px;max-width:100%;min-width:0; }
.report-period-line { display:flex;align-items:baseline;justify-content:space-between;gap:5px;min-width:0; }
.report-period-label { color:#8490a3;font-size:10px;font-weight:700;white-space:nowrap; }
.report-period-date { font-size:11.7px;font-weight:700;color:#334155;white-space:nowrap;font-variant-numeric:tabular-nums; }
.report-period-line.is-deadline { padding-top:5px;border-top:1px dashed #e3e8f0; }
.report-period-line.is-deadline .report-period-label {color:#677487;}
.report-period-line.is-deadline .report-period-date {font-weight:850;}
.report-period-date.text-danger {color:#dc2626;}
.report-period-date.text-warning {color:#b45309;}
.report-period-date.text-dark {color:#334155;}
.report-assignee-compact { gap:5px; }
.report-assignee-compact .report-person-chip { flex:0 1 auto;max-width:calc(50% - 23px); }
.report-assignee-expanded { border-color:#ddd0ef;background:#fcfaff; }
.report-person-row { gap:10px; }
.report-person-hours { align-self:center;padding:4px 7px;border:1px solid #e7d9f4;border-radius:7px;background:#f7f1fc;color:#663493;font-size:10.5px;font-weight:800;white-space:nowrap; }
.report-assignee-tooltip {width:430px;max-width:calc(100vw - 20px);max-height:min(500px,calc(100vh - 24px));padding:15px 16px;border-color:#d4bbea;box-shadow:0 20px 48px rgba(56,30,86,.22);}
.report-assignee-tooltip-title {font-size:13.5px;margin-bottom:9px;padding-bottom:10px;}
.report-assignee-tooltip .report-person-row {padding:9px 2px;}
.report-assignee-tooltip .report-person-avatar {width:40px;height:40px;flex-basis:40px;}
.report-assignee-tooltip .report-person-name {font-size:13px;white-space:normal;overflow:visible;overflow-wrap:anywhere;}
.report-assignee-tooltip .report-person-email {font-size:11.5px;white-space:normal;overflow:visible;overflow-wrap:anywhere;}
.report-assignee-tooltip .report-person-hours {font-size:11px;}
/* Thu gọn (>=3 nhân sự): avatar + tên + email, hai thẻ luôn chung hàng với nút +N. */
.report-assignee-box:not(.is-expanded) .report-assignee-compact {display:flex;flex-wrap:nowrap;gap:6px;align-items:center;overflow:hidden;}
.report-assignee-box:not(.is-expanded) .report-assignee-compact .report-person-chip {display:flex;align-items:center;flex:1 1 0;width:0;max-width:none;min-width:0;min-height:39px;padding:4px 5px;border:1px solid #e0e4ed;border-radius:8px;background:#fafaff;overflow:hidden;}
.report-assignee-compact .report-person-identity {display:flex;align-items:center;gap:5px;width:100%;min-width:0;flex:1 1 0;}
.report-assignee-compact .report-person-avatar {width:24px;height:24px;flex:0 0 24px;border-radius:50%;}
.report-assignee-compact .report-person-avatar .report-avatar-fallback {font-size:9px;}
.report-assignee-compact .report-person-copy {display:flex;flex-direction:column;gap:2px;min-width:0;flex:1 1 0;}
.report-assignee-compact .report-person-name {font-size:10.5px;line-height:1.15;font-weight:750;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;}
.report-assignee-compact .report-person-email {font-size:9px;line-height:1.15;color:#8390a6;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;}
.report-assignee-box:not(.is-expanded) .report-assignee-more {flex:0 0 30px;width:30px;height:30px;margin-left:0;cursor:pointer;}
@media(max-width:760px) {
  .report-assignee-box:not(.is-expanded) .report-assignee-compact {gap:4px;}
  .report-assignee-box:not(.is-expanded) .report-assignee-compact .report-person-chip {padding:3px 4px;min-height:35px;}
  .report-assignee-compact .report-person-avatar {width:21px;height:21px;flex-basis:21px;}
  .report-assignee-compact .report-person-name {font-size:10px;}
  .report-assignee-compact .report-person-email {font-size:8.5px;}
  .report-assignee-box:not(.is-expanded) .report-assignee-more {flex-basis:27px;width:27px;height:28px;}
}
/* Nhân sự ít người: mở cố định, không hiện nút và không tạo hover giả tương tác. */
.report-assignee-box.is-static {cursor:default;}
.report-assignee-box.is-static .report-assignee-expanded {display:flex;margin-top:0;}
.report-assignee-box.is-static .report-assignee-compact,.report-assignee-box.is-static .report-assignee-more {display:none !important;}
.report-assignee-box.is-static .report-person-name,.report-assignee-box.is-static .report-person-email {text-overflow:ellipsis;}
#doingTasksTable td.report-assignee-cell.report-assignee-static,
#todoTasksTable td.report-assignee-cell.report-assignee-static {cursor:default;}
#doingTasksTable td.report-assignee-cell.report-assignee-static:hover,
#todoTasksTable td.report-assignee-cell.report-assignee-static:hover {background:transparent;}
@media (max-width:620px) { .report-person-chip {padding:4px 5px;font-size:10.5px;} .report-assignee-more {flex-basis:28px;width:28px;height:28px;} .report-person-avatar {width:29px;height:29px;flex-basis:29px;} }
@media (max-width:1199.98px) { .report-toc { margin-bottom:8px; } }
@media (prefers-reduced-motion:reduce) { .report-toc-list,.report-toc-toggle .report-toc-arrow,.report-assignee-more { transition:none; } }
    </style>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-4 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1"/>
                <SweetSoft:CtrlProjectTabs runat="server" ID="CtrlProjectTabs1" />
                <asp:UpdatePanel ID="upReport" runat="server">
                    <Triggers>
                        <asp:PostBackTrigger ControlID="btnExportPDF" />
                    </Triggers>
                    <ContentTemplate>
                        <div class="report-layout">
                            <div class="sidebar-left">
                                <div class="form-group-vertical">
                                    <label><%= GetResourceText(BackEndResourceKeys.TIME_PERIOD) %>:</label>
                                    <asp:DropDownList ID="ddlPeriod" runat="server" CssClass="form-control"
                                        AutoPostBack="true"
                                        OnSelectedIndexChanged="ddlPeriod_SelectedIndexChanged"
                                        onchange="toggleCustomDates(this.value)">
                                    </asp:DropDownList>
                                </div>
                                <div class="form-group-vertical custom-date-group" style="display: none;">
                                    <label><%= GetResourceText(BackEndResourceKeys.FROM_DATE) %>:</label>
                                    <asp:TextBox ID="txtFromDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                </div>
                                <div class="form-group-vertical custom-date-group mb-2" style="display: none;">
                                    <label><%= GetResourceText(BackEndResourceKeys.TO_DATE) %>:</label>
                                    <asp:TextBox ID="txtToDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                </div>
                                <asp:LinkButton ID="btnPreview" runat="server" CssClass="btn-filter" OnClick="btnPreview_Click">
                                    <i class="fas fa-search"></i> <%= GetResourceText(BackEndResourceKeys.GENERATE_REPORT) %>
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnExportPDF" runat="server" CssClass="btn-export btn-export-pdf" OnClick="btnExportPDF_Click">
                                    <i class="fas fa-file-pdf"></i> <%= GetResourceText(BackEndResourceKeys.EXPORT_PDF) %>
                                </asp:LinkButton>
                                <nav class="report-toc" aria-label="Mục lục báo cáo">
                                    <div class="report-toc-head">
                                        <span class="report-toc-title"><i class="fas fa-list-ul" aria-hidden="true"></i> Mục lục báo cáo</span>
                                        <button type="button" class="report-toc-toggle" aria-label="Thu gọn mục lục" aria-expanded="true" title="Thu gọn / Mở mục lục"><i class="fas fa-chevron-up report-toc-arrow" aria-hidden="true"></i></button>
                                    </div>
                                    <div class="report-toc-list">
                                        <button type="button" class="report-toc-link is-active" data-report-target="reportTop"><span class="report-toc-index">0.</span><span class="report-toc-link-label">Đến đầu trang</span></button>
                                        <button type="button" class="report-toc-link" data-report-target="doingTasksSection"><span class="report-toc-index">1.</span><span class="report-toc-link-label">Công việc đang thực hiện</span></button>
                                        <button type="button" class="report-toc-link" data-report-target="todoTasksSection"><span class="report-toc-index">2.</span><span class="report-toc-link-label">Công việc chưa bắt đầu</span></button>
                                        <button type="button" class="report-toc-link" data-report-target="issuesSection"><span class="report-toc-index">3.</span><span class="report-toc-link-label">Vấn đề</span></button>
                                        <button type="button" class="report-toc-link" data-report-target="reportBottom"><span class="report-toc-index">4.</span><span class="report-toc-link-label">Cuối trang</span></button>
                                    </div>
                                </nav>
                            </div>
                            <div class="main-center">
                                <asp:PlaceHolder ID="phNotStarted" runat="server" Visible="false">
                                    <div class="empty-state">
                                        <i class="fas fa-folder-open"></i>
                                        <h4><%= GetResourceText(BackEndResourceKeys.PROJECT_NOT_STARTED) %></h4>
                                        <p><%= GetResourceText(BackEndResourceKeys.PROJECT_HAS_NO_DATA) %></p>
                                    </div>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="phReportContent" runat="server">
                                    <div class="report-paper" id="reportTop">
                                        <div class="report-header">
                                            <h1><%= GetResourceText(BackEndResourceKeys.PROJECT_REPORT) %></h1>
                                            <div class="sub-date">
                                                <asp:Literal ID="ltrReportPeriod" runat="server"></asp:Literal>
                                            </div>
                                        </div>
                                        <div class="kpi-summary-bar">
                                            <div class="kpi-item">
                                                <span class="kpi-title"><%= GetResourceText(BackEndResourceKeys.TOTAL_TASKS) %></span>
                                                <span class="kpi-number total"><asp:Literal ID="ltrTotalTasks" runat="server">0</asp:Literal></span>
                                            </div>
                                            <div class="kpi-item">
                                                <span class="kpi-title"><%= GetResourceText(BackEndResourceKeys.OVERDUE_TASKS) %></span>
                                                <span class="kpi-number danger"><asp:Literal ID="ltrOverdueTasks" runat="server">0</asp:Literal></span>
                                            </div>
                                            <div class="kpi-item">
                                                <span class="kpi-title"><%= GetResourceText(BackEndResourceKeys.ISSUES_ARISING) %></span>
                                                <span class="kpi-number warning"><asp:Literal ID="ltrTotalIssues" runat="server">0</asp:Literal></span>
                                            </div>
                                        </div>
                                        <asp:PlaceHolder ID="phDashboard" runat="server" Visible="false">
                                            <div class="section-title dashboard"><i class="fas fa-chart-pie me-2"></i> Tổng quan tiến độ</div>
                                            <div class="dashboard-container">
                                                <div class="chart-box">
                                                    <div class="chart-title">Trạng thái công việc</div>
                                                    <canvas id="taskStatusChart"></canvas>
                                                </div>
                                                <div class="chart-box">
                                                    <div class="chart-title">Tình trạng thời hạn công việc</div>
                                                    <canvas id="doingStatusChart"></canvas>
                                                </div>
                                            </div>
                                        </asp:PlaceHolder>
                                        <div id="doingTasksSection" class="section-title info">1. DANH SÁCH CÔNG VIỆC ĐANG THỰC HIỆN</div>
                                        <div class="mb-4">
                                            <table id="doingTasksTable" class="table table-bordered table-hover w-100">
                                                <thead class="text-center">
                                                    <tr>
                                                        <th width="32%" class="text-start ps-3">CÔNG VIỆC</th>
                                                        <th width="33%" class="text-start"><span id="reportDoingAssigneeHeader">Người thực hiện</span></th>
                                                        <th width="22%">THỜI GIAN DỰ KIẾN</th>
                                                        <th width="13%"><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    <asp:Repeater ID="rptDoingTasks" runat="server">
                                                        <ItemTemplate>
                                                            <tr class="report-doing-row" data-deadline-state='<%# GetDeadlineState(Eval("NgayKetThuc")) %>'>
                                                                <td class="text-start ps-3">
                                                                    <div class="task-code-name"><span class="task-code-chip"><%# Eval("MaCongViec") %></span><span class="task-name-text"><%# Eval("TenCongViec") %></span><span class="report-task-hours"><%# GetTaskWorkHoursHtml(Eval("IdCongViec")) %></span></div>
                                                                </td>
                                                                <td class="text-start report-assignee-cell"><%# RenderReportAssignees(Eval("IdCongViec"), Eval("NhanVienThucHien"), true) %></td>
                                                                <td class="report-period-cell"><%# RenderReportPlannedPeriod(Eval("NgayBatDau"), Eval("NgayKetThuc")) %></td>
                                                                <td class="text-center" style="vertical-align: middle;">
                                                                    <div class="report-badge badge-doing" style="margin-bottom: 4px;">Đang làm</div>
                                                                    <%# GetTaskWarningHtml(Eval("NgayKetThuc")) %>
                                                                </td>
                                                            </tr>
                                                        </ItemTemplate>
                                                        <FooterTemplate>
                                                            <asp:PlaceHolder runat="server" Visible='<%# rptDoingTasks.Items.Count == 0 %>'>
                                                                <tr><td colspan="4" class="text-center text-muted py-4">Không có công việc đang thực hiện</td></tr>
                                                            </asp:PlaceHolder>
                                                        </FooterTemplate>
                                                    </asp:Repeater>
                                                </tbody>
                                            </table>
                                        </div>
                                        <div id="todoTasksSection" class="section-title muted">2. DANH SÁCH CÔNG VIỆC CHƯA BẮT ĐẦU</div>
                                        <div class="mb-4">
                                            <table id="todoTasksTable" class="table table-bordered table-hover w-100">
                                                <thead class="text-center">
                                                    <tr>
                                                        <th width="32%" class="text-start ps-3">CÔNG VIỆC</th>
                                                        <th width="33%" class="text-start"><%= GetResourceText(BackEndResourceKeys.ASSIGNEE) %></th>
                                                        <th width="22%">THỜI GIAN DỰ KIẾN</th>
                                                        <th width="13%"><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    <asp:Repeater ID="rptTodoTasks" runat="server">
                                                        <ItemTemplate>
                                                            <tr data-deadline-state='<%# GetDeadlineState(Eval("NgayKetThuc")) %>'>
                                                                <td class="text-start ps-3">
                                                                    <div class="task-code-name"><span class="task-code-chip"><%# Eval("MaCongViec") %></span><span class="task-name-text"><%# Eval("TenCongViec") %></span><span class="report-task-hours"><%# GetTaskWorkHoursHtml(Eval("IdCongViec")) %></span></div>
                                                                </td>
                                                                <td class="text-start report-assignee-cell"><%# RenderReportAssignees(Eval("IdCongViec"), Eval("NhanVienThucHien"), false) %></td>
                                                                <td class="report-period-cell"><%# RenderReportPlannedPeriod(Eval("NgayBatDau"), Eval("NgayKetThuc")) %></td>
                                                                <td class="text-center">
                                                                    <div class="report-badge badge-todo" style="margin-bottom: 4px;">Chưa bắt đầu</div>
                                                                    <%# GetTaskWarningHtml(Eval("NgayKetThuc")) %>
                                                                </td>
                                                            </tr>
                                                        </ItemTemplate>
                                                        <FooterTemplate>
                                                            <asp:PlaceHolder runat="server" Visible='<%# rptTodoTasks.Items.Count == 0 %>'>
                                                                <tr><td colspan="4" class="text-center text-muted py-4">Không có công việc chưa bắt đầu</td></tr>
                                                            </asp:PlaceHolder>
                                                        </FooterTemplate>
                                                    </asp:Repeater>
                                                </tbody>
                                            </table>
                                        </div>
                                        <div id="issuesSection" class="section-title danger">3. <%= GetResourceText(BackEndResourceKeys.ISSUES_LIST) %></div>
                                        <div class="mb-2">
                                            <table id="issuesTable" class="table table-bordered table-hover w-100">
                                                <thead class="text-center">
                                                    <tr>
                                                        <th width="34%" class="text-start ps-3"><%= GetResourceText(BackEndResourceKeys.ISSUE_NAME) %></th>
                                                        <th width="15%"><%= GetResourceText(BackEndResourceKeys.IMPACT) %></th>
                                                        <th width="18%"><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                                        <th width="33%" class="text-start"><%= GetResourceText(BackEndResourceKeys.HANDLING_PLAN) %></th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    <asp:Repeater ID="rptIssues" runat="server">
                                                        <ItemTemplate>
                                                            <tr>
                                                                <td class="text-start ps-3"><%# Eval("TenVanDe") %></td>
                                                                <td class="text-center">
                                                                    <span class="report-badge <%# GetPriorityBadge(Convert.ToInt32(Eval("MucDoAnhHuong"))) %>"><%# GetPriorityText(Convert.ToInt32(Eval("MucDoAnhHuong"))) %></span>
                                                                </td>
                                                                <td class="text-center">
                                                                    <span class="report-badge <%# GetIssueStatusBadge(Convert.ToInt32(Eval("TrangThai"))) %>"><%# GetIssueStatusText(Convert.ToInt32(Eval("TrangThai"))) %></span>
                                                                </td>
                                                                <td class="text-start"><%# Eval("KeHoachXuLy") %></td>
                                                            </tr>
                                                        </ItemTemplate>
                                                        <FooterTemplate>
                                                            <asp:PlaceHolder runat="server" Visible='<%# rptIssues.Items.Count == 0 %>'>
                                                                <tr><td colspan="4" class="text-center text-muted py-4"><%= GetResourceText(BackEndResourceKeys.NO_ISSUES) %></td></tr>
                                                            </asp:PlaceHolder>
                                                        </FooterTemplate>
                                                    </asp:Repeater>
                                                </tbody>
                                            </table>
                                        </div>
                                        <div id="reportBottom" aria-hidden="true"></div>
                                    </div>
                                </asp:PlaceHolder>
                            </div>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server"></asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script>

        // Độc lập AJAX WebForms; tất cả thao tác mục lục/nhân sự thực hiện trên trình duyệt.
        (function () {
            window.ProjectReportUI = window.ProjectReportUI || {};
            var ui = window.ProjectReportUI;
            var activeTip = null;
            function getPane() { return document.querySelector('.report-layout .main-center'); }
            function reducedMotion() { return window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches; }
            function reportOffset() { return typeof getReportScrollTopOffset === 'function' ? getReportScrollTopOffset() : 88; }
            function isInternal(pane) { return pane && pane.scrollHeight > pane.clientHeight + 3 && window.getComputedStyle(pane).overflowY !== 'visible'; }
            function markActive(id) { document.querySelectorAll('.report-toc-link').forEach(function (node) { node.classList.toggle('is-active', node.getAttribute('data-report-target') === id); }); }
            ui.scrollTo = function (targetId) {
                var target = document.getElementById(targetId);
                if (!target) return;
                var pane = getPane();
                var behavior = reducedMotion() ? 'auto' : 'smooth';
                if (isInternal(pane)) {
                    var top = target.getBoundingClientRect().top - pane.getBoundingClientRect().top + pane.scrollTop - 16;
                    pane.scrollTo({ top: Math.max(0, top), behavior: behavior });
                } else {
                    var topPage = window.pageYOffset + target.getBoundingClientRect().top - reportOffset();
                    window.scrollTo({ top: Math.max(0, topPage), behavior: behavior });
                }
                markActive(targetId);
            };
            function fitViewport() {
                var layout = document.querySelector('.report-layout');
                if (!layout) return;
                if (window.innerWidth < 1200) { layout.style.height = ''; return; }
                var top = layout.getBoundingClientRect().top;
                layout.style.height = Math.max(240, Math.floor(window.innerHeight - top - 22)) + 'px';
            }
            function refreshAssigneeHeader() {
                var head = document.getElementById('reportDoingAssigneeHeader');
                if (head) head.textContent = 'Người thực hiện';
            }
            function closeTooltip() { if (activeTip) { activeTip.remove(); activeTip = null; } }
            function showTooltip(box) {
                if (!box || box.classList.contains('is-expanded') || box.classList.contains('is-static')) return;
                var people = box.querySelectorAll('.report-assignee-expanded .report-person-row');
                if (!people.length) return;
                closeTooltip();
                var tip = document.createElement('div');
                tip.className = 'report-assignee-tooltip';
                tip.setAttribute('role', 'tooltip');
                var title = document.createElement('div');
                title.className = 'report-assignee-tooltip-title';
                var titleIcon = document.createElement('i');
                titleIcon.className = 'fas fa-users';
                titleIcon.setAttribute('aria-hidden', 'true');
                title.appendChild(titleIcon);
                title.appendChild(document.createTextNode('Danh sách người thực hiện (' + people.length + ')'));
                tip.appendChild(title);
                people.forEach(function (person) {
                    var row = document.createElement('div');
                    row.className = 'report-person-row';
                    var identity = person.querySelector('.report-person-identity');
                    if (identity) row.appendChild(identity.cloneNode(true));
                    var hours = person.querySelector('.report-person-hours');
                    if (hours) row.appendChild(hours.cloneNode(true));
                    tip.appendChild(row);
                });
                var note = document.createElement('div');
                note.className = 'report-assignee-tooltip-note';
                note.textContent = 'Nhấp vào ô để mở hoặc thu gọn danh sách và giờ công';
                tip.appendChild(note);
                document.body.appendChild(tip);
                var rect = box.closest('.report-assignee-cell').getBoundingClientRect();
                var width = tip.offsetWidth, height = tip.offsetHeight;
                var x = rect.right + 10;
                if (x + width > window.innerWidth - 12) x = rect.left - width - 10;
                if (x < 10) x = Math.max(10, Math.min(rect.left + (rect.width - width) / 2, window.innerWidth - width - 10));
                // Canh giữa chiều cao tooltip theo hàng đang hover.
                // Nếu chạm mép trên/dưới khung nhìn, đẩy xuống/lên vừa đủ.
                var margin = 10;
                var preferredY = rect.top + rect.height / 2 - height / 2;
                var maxY = Math.max(margin, window.innerHeight - height - margin);
                var y = Math.min(Math.max(preferredY, margin), maxY);
                tip.style.left = x + 'px';
                tip.style.top = y + 'px';
                activeTip = tip;
            }
            function setAssigneeExpanded(box, expanded) {
                if (!box || box.classList.contains('is-static')) return;
                box.classList.toggle('is-expanded', expanded);
                box.setAttribute('aria-expanded', String(expanded));
                box.setAttribute('aria-label', expanded ? 'Thu gọn danh sách người thực hiện' : 'Xem danh sách người thực hiện');
                var more = box.querySelector('.report-assignee-more');
                if (more) {
                    more.textContent = expanded ? '−' : (more.getAttribute('data-collapsed-label') || '⌄');
                    more.setAttribute('aria-hidden', 'true');
                }
                var row = box.closest('.report-doing-row');
                if (row) row.classList.toggle('report-row-expanded', expanded);
                refreshAssigneeHeader();
                closeTooltip();
            }
            if (!window.__projectReportDelegatesAttached) {
                window.__projectReportDelegatesAttached = true;
                document.addEventListener('click', function (event) {
                    var tocBtn = event.target.closest('.report-toc-toggle');
                    if (tocBtn) { var toc = tocBtn.closest('.report-toc'); toc.classList.toggle('is-collapsed'); var open = !toc.classList.contains('is-collapsed'); tocBtn.setAttribute('aria-expanded', String(open)); tocBtn.setAttribute('aria-label', open ? 'Thu gọn mục lục' : 'Mở mục lục'); return; }
                    var jump = event.target.closest('.report-toc-link');
                    if (jump) { ui.scrollTo(jump.getAttribute('data-report-target')); return; }
                    // Mở/đóng khi nhấp BẤT KỲ vị trí nào trong ô người thực hiện.
                    var cell = event.target.closest('td.report-assignee-cell');
                    if (cell) { var box = cell.querySelector('.report-assignee-box'); if (box && !box.classList.contains('is-static')) { setAssigneeExpanded(box, !box.classList.contains('is-expanded')); return; } }
                });
                document.addEventListener('keydown', function (event) {
                    if (event.target.matches('.report-assignee-box') && (event.key === 'Enter' || event.key === ' ')) {
                        event.preventDefault();
                        setAssigneeExpanded(event.target, !event.target.classList.contains('is-expanded'));
                    }
                });
                // Tooltip CHỈ xuất hiện khi con trỏ ở nút +N (không hiện khi rê trên ô/chip).
                // Vẫn giữ nguyên hành vi bấm toàn bộ ô để mở/thu gọn.
                document.addEventListener('mouseover', function (event) {
                    var more = event.target.closest('.report-assignee-more');
                    if (!more || (event.relatedTarget && more.contains(event.relatedTarget))) return;
                    var box = more.closest('.report-assignee-box');
                    if (box && !box.classList.contains('is-expanded')) showTooltip(box);
                });
                document.addEventListener('mouseout', function (event) {
                    var more = event.target.closest('.report-assignee-more');
                    if (more && (!event.relatedTarget || !more.contains(event.relatedTarget))) closeTooltip();
                });
                document.addEventListener('scroll', closeTooltip, true);
                window.addEventListener('resize', function () { closeTooltip(); fitViewport(); });
            }
            ui.initialize = function () {
                fitViewport(); refreshAssigneeHeader(); closeTooltip();
                document.querySelectorAll('.report-assignee-box.is-static').forEach(function (box) {
                    var cell = box.closest('td.report-assignee-cell');
                    if (cell) cell.classList.add('report-assignee-static');
                });
            };
            if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', ui.initialize);
            else ui.initialize();
            if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(ui.initialize);
        })();
        function toggleCustomDates(val) {
            var dateGroups = document.querySelectorAll('.custom-date-group');
            if (val === 'CUSTOM') {
                dateGroups.forEach(el => el.style.display = 'flex');
            } else {
                dateGroups.forEach(el => el.style.display = 'none');
            }
        }

        function clearReportChartHighlights() {
            document.querySelectorAll('.report-chart-highlight').forEach(function (element) {
                element.classList.remove('report-chart-highlight');
            });
            document.querySelectorAll('.report-chart-section-highlight').forEach(function (section) {
                section.classList.remove('report-chart-section-highlight');
            });
        }

        // Cuộn tới tiêu đề nhưng chừa khoảng trống cho thanh điều hướng SweetSoft cố định phía trên.
        function getReportScrollTopOffset() {
            var selectors = 'header, nav, .navbar, .navbar-fixed-top, .fixed-top, #header, #topnav, .main-header';
            var elements = document.querySelectorAll(selectors);
            var bottom = 0;

            elements.forEach(function (element) {
                var style = window.getComputedStyle(element);
                var rect = element.getBoundingClientRect();
                if ((style.position === 'fixed' || style.position === 'sticky') && rect.top <= 12 && rect.bottom > 0) {
                    bottom = Math.max(bottom, rect.bottom);
                }
            });

            // Dự phòng cho layout nếu thanh điều hướng không khớp các selector phía trên.
            return Math.max(bottom, 72) + 16;
        }

        function scrollToReportHeading(section) {
            if (!section) return;
            if (window.ProjectReportUI && window.ProjectReportUI.scrollTo && section.id) {
                window.ProjectReportUI.scrollTo(section.id);
                return;
            }
            var targetTop = window.pageYOffset + section.getBoundingClientRect().top - getReportScrollTopOffset();
            window.scrollTo({ top: Math.max(0, targetTop), behavior: 'smooth' });
        }

        function scrollToReportSection(sectionId) {
            clearReportChartHighlights();
            var section = document.getElementById(sectionId);
            if (!section) return;

            // Chỉ làm nổi bật tiêu đề danh sách và đặt tiêu đề ngay dưới thanh menu.
            section.classList.add('report-chart-section-highlight');
            requestAnimationFrame(function () { scrollToReportHeading(section); });
        }

        function scrollToTaskDeadline(state) {
            clearReportChartHighlights();
            var doingRows = Array.prototype.slice.call(
                document.querySelectorAll('#doingTasksTable tbody tr[data-deadline-state="' + state + '"]')
            ).filter(function (row) { return row.querySelector('td'); });
            var todoRows = Array.prototype.slice.call(
                document.querySelectorAll('#todoTasksTable tbody tr[data-deadline-state="' + state + '"]')
            ).filter(function (row) { return row.querySelector('td'); });

            // A zero-value legend item is disabled; this guard also protects direct calls.
            if (!doingRows.length && !todoRows.length) return;

            var doingSection = document.getElementById('doingTasksSection');
            var todoSection = document.getElementById('todoTasksSection');
            if (doingRows.length && doingSection) doingSection.classList.add('report-chart-section-highlight');
            if (todoRows.length && todoSection) todoSection.classList.add('report-chart-section-highlight');

            // Scroll to the first list title that actually contains tasks in this deadline category.
            var firstSection = doingRows.length ? doingSection : todoSection;
            if (firstSection) requestAnimationFrame(function () { scrollToReportHeading(firstSection); });
        }

        if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function () {
                var ddl = document.getElementById('<%= ddlPeriod.ClientID %>');
                if (ddl) toggleCustomDates(ddl.value);
            });
        }
    </script>
</asp:Content>