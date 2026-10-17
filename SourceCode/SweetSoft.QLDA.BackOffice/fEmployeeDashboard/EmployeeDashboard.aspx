<%@ Page Title="Dashboard nhân viên" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="EmployeeDashboard.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fEmployeeDashboard.EmployeeDashboard" %>
<%@ Register Assembly="SweetSoft.QLDA.Controls" Namespace="SweetSoft.QLDA.Controls" TagPrefix="SweetSoft" %>
<asp:Content ID="ContentHead" ContentPlaceHolderID="cpHead" runat="server">
<style>
.ed-dashboard { --ed-primary: #5b21b6; --ed-primary-hover: #4c1d95; --ed-bg: #f8fafc; --ed-surface: #ffffff; --ed-border: #e2e8f0; --ed-text-main: #1e293b; --ed-text-muted: #64748b; --ed-radius-lg: 12px; --ed-radius-md: 8px; color: var(--ed-text-main); font-family: "Segoe UI", system-ui, sans-serif; font-size: 13px; line-height: 1.4; padding: 0 0 20px; }
.ed-dashboard * { box-sizing: border-box; }
.ed-dashboard h1, .ed-dashboard h2, .ed-dashboard h3, .ed-dashboard p { font-family: inherit; margin: 0; }
.ed-header-row { display: grid; grid-template-columns: minmax(0, 3fr) repeat(4, minmax(0, 1fr)) minmax(0, 3fr); gap: 16px; margin-bottom: 16px; align-items: stretch; animation: ed-fade-in 0.3s ease; }
.ed-header-profile { min-width: 0; background: var(--ed-surface); border: 1px solid var(--ed-border); border-radius: var(--ed-radius-lg); padding: 14px 18px; display: flex; align-items: center; gap: 14px; box-shadow: 0 2px 5px rgba(0,0,0,0.03); }
.ed-header-profile .ed-avatar, .ed-header-profile .ed-avatar-fallback { width: 60px; height: 60px; flex: 0 0 60px; border-radius: var(--ed-radius-md); object-fit: cover; border: 1px solid var(--ed-border); }
.ed-header-profile .ed-avatar-fallback { display: flex; align-items: center; justify-content: center; background: #f3e8ff; color: var(--ed-primary); font-size: 20px; font-weight: 800; }
.ed-header-profile .ed-eyebrow { font-size: 11px; text-transform: uppercase; font-weight: 750; color: #7c4dff; margin-bottom: 4px; letter-spacing: 0.45px; }
.ed-header-profile h1 { font-size: 18px; font-weight: 750; color: var(--ed-text-main); margin-bottom: 3px; white-space: normal; overflow-wrap: anywhere; line-height: 1.25; }
.ed-header-profile .ed-profile-meta { font-size: 12px; color: var(--ed-text-muted); font-weight: 550; display: flex; flex-direction: column; gap: 3px; }
.ed-header-stat { position: relative; overflow: hidden; display: flex; align-items: center; justify-content: center; gap: 8px; background: linear-gradient(135deg, #fff 20%, var(--soft, #f8fafc) 100%); border: 1px solid var(--ed-border); border-top: 3px solid var(--c); border-radius: var(--ed-radius-md); padding: 10px; box-shadow: 0 1px 3px rgba(15, 23, 42, .035); transition: transform .18s ease, box-shadow .18s ease, border-color .18s ease; }
.ed-header-stat:hover { transform: translateY(-2px); box-shadow: 0 5px 12px rgba(15, 23, 42, .075); }
.ed-header-stat > div { position: relative; z-index: 1; }
.ed-stat-val { font-size: 26px; font-weight: 800; color: var(--c); line-height: 1; }
.ed-stat-lbl { font-size: 11px; font-weight: 650; color: #566176; text-transform: uppercase; line-height: 1.25; text-align: left; }
.ed-stat-ic { position: absolute; right: -4px; bottom: -8px; z-index: 0; font-size: 61px; opacity: .14; color: var(--c); transform: rotate(-7deg); pointer-events: none; }
.ed-header-tools { display: grid; grid-template-columns: minmax(0, 4fr) minmax(0, 6fr); align-items: stretch; gap: 0; min-width: 0; min-height: 124px; background: linear-gradient(140deg, #ffffff 0%, #faf8ff 100%); border: 1px solid #e5def7; border-radius: var(--ed-radius-lg); box-shadow: 0 2px 5px rgba(49, 46, 129, .045); }
.ed-header-time { position: relative; min-width: 0; min-height: 122px; background: transparent; border: 0; border-radius: 0; padding: 10px 12px; display: flex; flex-direction: column; justify-content: center; align-items: flex-end; gap: 4px; box-shadow: none; text-align: right; }
.ed-header-time .ed-week-label { display: inline-flex; align-items: center; gap: 6px; font-size: 10.5px; color: #5b6475; text-transform: uppercase; font-weight: 700; margin-bottom: 2px; letter-spacing: .35px; }
.ed-header-time .ed-week-label i { color: #6d4bc3; font-size: 12px; }
.ed-header-time .ed-week-title { display: inline-flex; align-items: center; justify-content: center; max-width: 100%; padding: 7px 10px; border: 1px solid #e6ddfb; border-radius: 8px; background: #f3efff; font-size: 15px; font-weight: 800; color: #4c319b; margin-bottom: 5px; font-variant-numeric: tabular-nums; line-height: 1.25; }
.ed-header-time .ed-week-actions { display: flex; flex-wrap: wrap; gap: 6px; justify-content: flex-end; }
.ed-header-time .ed-week-btn { display: inline-flex; align-items: center; justify-content: center; min-height: 31px; padding: 5px 10px; border: 1px solid #dfe3eb; background: #fff; color: #374151; border-radius: 7px; font-size: 11.5px; font-weight: 650; text-decoration: none !important; transition: background-color .18s ease, border-color .18s ease, color .18s ease; }
.ed-header-time .ed-week-btn:hover { border-color: #c9b8f0; background: #f6f2ff; color: #4c319b; }
.ed-header-time .ed-week-btn.ed-current { background: #5b35ad; border-color: #5b35ad; color: #fff; }
.ed-header-time .ed-week-btn.ed-current:hover { background: #49278f; border-color: #49278f; }
.ed-charts-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; margin-bottom: 16px; }
.ed-chart-container { display: flex; align-items: center; justify-content: center; gap: 30px; padding: 15px 10px; }
.ed-donut-wrapper { position: relative; width: 170px; height: 170px; flex: 0 0 170px; border-radius: 50%; display: flex; align-items: center; justify-content: center; box-shadow: 0 4px 15px rgba(0,0,0,0.08); }
.ed-donut-wrapper::before { content: ""; position: absolute; inset: 26px; background: var(--ed-surface); border-radius: 50%; box-shadow: inset 0 2px 6px rgba(0,0,0,0.04); }
.ed-donut-center { position: relative; z-index: 10; text-align: center; display: flex; flex-direction: column; }
.ed-donut-number { font-size: 34px; font-weight: 800; color: var(--ed-text-main); line-height: 1; margin-bottom: 2px; }
.ed-donut-label { font-size: 11px; color: var(--ed-text-muted); font-weight: 600; text-transform: uppercase; }
.ed-chart-legend { display: flex; flex-direction: column; gap: 10px; flex: 1; max-width: 220px; }
.ed-legend-item { display: flex; align-items: center; gap: 10px; font-size: 13px; color: var(--ed-text-muted); font-weight: 600; background: var(--ed-surface); padding: 10px 14px; border-radius: 8px; border: 1px solid var(--ed-border); }
.ed-dot { display: inline-block; width: 12px; height: 12px; border-radius: 50%; flex: 0 0 12px; }
.ed-dashboard .ed-layout { display: grid; grid-template-columns: minmax(320px, 0.9fr) minmax(0, 1.1fr); gap: 16px; align-items: stretch; margin-bottom: 16px; }
.ed-dashboard .ed-panel { display: flex; flex-direction: column; background: var(--ed-surface); border: 1px solid var(--ed-border); border-radius: var(--ed-radius-lg); box-shadow: 0 2px 6px rgba(0,0,0,0.03); overflow: hidden; }
.ed-panel-project { border-top: 4px solid #d6a20a; }
.ed-panel-project .ed-panel-head { background: linear-gradient(to right, #fff8df, #fff); }
.ed-panel-task { border-top: 4px solid #2563eb; }
.ed-panel-task .ed-panel-head { background: linear-gradient(to right, #eff6ff, #fff); }
.ed-dashboard .ed-panel-head { display: flex; justify-content: space-between; align-items: flex-start; padding: 14px 18px; border-bottom: 1px solid var(--ed-border); }
.ed-dashboard .ed-panel-heading { display: flex; align-items: flex-start; gap: 12px; flex: 1; min-width:0; }
.ed-dashboard .ed-panel-icon { display: flex; align-items: center; justify-content: center; width: 34px; height: 34px; border-radius: 8px; font-size: 15px; margin-top: 2px; }
.ed-panel-project .ed-panel-icon { background: #fff0b8; color: #8a5a00; }
.ed-panel-task .ed-panel-icon { background: #dbeafe; color: #2563eb; }
.ed-dashboard .ed-panel-title { font-size: 16px; font-weight: 700; color: var(--ed-text-main); line-height: 1.2; margin-top:2px; }
.ed-dashboard .ed-panel-subtitle { display: flex; gap: 8px; flex-wrap: wrap; margin-top: 6px; font-size: 11.5px; color: var(--ed-text-muted); }
.ed-dashboard .ed-count { display: inline-flex; align-items: center; justify-content: center; padding: 2px 8px; border-radius: 12px; background: #e2e8f0; color: #475569; font-size: 11.5px; font-weight: 700; }
.ed-dashboard .ed-panel-body { padding: 16px; background: var(--ed-bg); flex: 1; display: flex; flex-direction: column; gap: 8px; }
.ed-more-btn { display: block; width: 100%; padding: 12px; text-align: center; border-radius: 8px; font-weight: 700; font-size: 13px; text-decoration: none !important; transition: 0.2s; margin-top: auto; }
.ed-btn-project { background: #fff9e7; color: #805500 !important; border: 1px solid #f0d98b; }
.ed-btn-project:hover { background: #fff1bd; border-color: #dfbd4e; }
.ed-btn-task { background: #eff6ff; color: #2563eb !important; border: 1px dashed #bfdbfe; }
.ed-btn-task:hover { background: #dbeafe; border-style: solid; }
.ed-tab-btn { display: inline-flex; align-items: center; gap: 5px; padding: 4px 10px; font-size: 11.5px; font-weight: 600; border-radius: 6px; background: #f1f5f9; color: #64748b; text-decoration: none !important; border: 1px solid transparent; transition: all 0.2s; cursor: pointer; }
.ed-tab-btn.active { background: #e0e7ff; color: #4338ca; border-color: #c7d2fe; }
.ed-tab-btn.filtered { background: #fef3c7; color: #b45309; border-color: #fde68a; }
.ed-tab-btn:hover { background: #e2e8f0; color: #475569; }
.ed-tab-btn.active:hover { background: #c7d2fe; color: #3730a3; }
.ed-tab-btn.filtered:hover { background: #fde68a; color: #92400e; }
.ed-dashboard .ed-list, .ed-dashboard .ed-project-list { display: flex; flex-direction: column; gap: 10px; }
.ed-dashboard .ed-card-item { background: var(--ed-surface); border: 1px solid var(--ed-border); border-radius: var(--ed-radius-md); padding: 14px; box-shadow: 0 1px 3px rgba(0,0,0,0.025); transition: border-color .18s ease, box-shadow .18s ease, transform .18s ease; }
.ed-dashboard .ed-card-item:hover { border-color: #cbd5e1; box-shadow: 0 4px 12px rgba(15, 23, 42, .065); }
.ed-dashboard .ed-project { display: flex; gap: 12px; align-items: flex-start; border-left: 3px solid #dfb52b; background: linear-gradient(105deg, #fffdf5 0%, #fff 48%); }
.ed-dashboard .ed-project-icon { width: 36px; height: 36px; flex: 0 0 36px; display: flex; align-items: center; justify-content: center; border: 1px solid #f0dc91; border-radius: 8px; background: #fff3c4; color: #8a5b00; font-size: 15px; }
.ed-dashboard .ed-project-content { flex: 1; min-width: 0; }
.ed-dashboard .ed-project-code { font-size: 10.5px; font-weight: 700; color: #8b7a4c; margin-bottom: 2px; }
.ed-dashboard .ed-project-name { display: block; font-size: 14px; font-weight: 750; color: #785500; margin-bottom: 5px; line-height: 1.3; text-decoration: none !important; transition: color .15s ease; cursor: pointer; }
.ed-dashboard .ed-project-name:hover { color: #5b4000; text-decoration: underline !important; }
.ed-dashboard .ed-project-progress .ed-progress span { background: #d4a20a; }
.ed-dashboard .ed-project-meta { display: flex; flex-wrap: wrap; gap: 6px 12px; font-size: 11px; color: var(--ed-text-muted); }
.ed-dashboard .ed-task { position: relative; border-left: 3px solid #60a5fa; background: linear-gradient(105deg, #f7fbff 0%, #fff 46%); }
.ed-dashboard .ed-task.is-overdue { border-left-color: #ef4444; }
.ed-dashboard .ed-task-top { display: flex; justify-content: space-between; align-items: flex-start; gap: 10px; }
.ed-dashboard .ed-kicker { font-size: 10.5px; font-weight: 700; color: #94a3b8; margin-bottom: 3px; letter-spacing: 0.3px; }
.ed-dashboard .ed-task-name { font-size: 14px; font-weight: 700; color: #1e3a8a; margin-bottom: 5px; line-height: 1.3; }
.ed-dashboard .ed-task-dates { font-size: 11.5px; color: var(--ed-text-muted); display: flex; align-items: center; gap: 5px; font-weight: 500; }
.ed-dashboard .ed-badge { display: inline-flex; padding: 3px 8px; border-radius: 4px; font-size: 10.5px; font-weight: 700; text-align: center; border: 1px solid transparent; }
.ed-dashboard .ed-badge.overdue { background: #fef2f2; color: #b91c1c; border-color: #fecaca; }
.ed-dashboard .ed-badge.done { background: #f0fdf4; color: #15803d; border-color: #bbf7d0; }
.ed-dashboard .ed-badge.inprogress { background: #eff6ff; color: #1d4ed8; border-color: #bfdbfe; }
.ed-dashboard .ed-progress { height: 5px; background: #e2e8f0; border-radius: 3px; margin-top: 10px; overflow: hidden; }
.ed-dashboard .ed-progress span { display: block; height: 100%; border-radius: 3px; background: var(--ed-primary); transition: width 0.4s ease; }
.ed-dashboard .ed-progress-meta { display: flex; justify-content: space-between; font-size: 10.5px; color: #64748b; margin-top: 5px; }
.ed-dashboard .ed-progress-meta strong { color: var(--ed-text-main); font-weight: 700; }
.ed-dashboard .ed-empty { text-align: center; padding: 20px 15px; background: #fff; border: 1px dashed #cbd5e1; border-radius: 8px; color: #64748b; font-size: 12.5px; margin-bottom: 10px; }
.ed-dashboard .ed-empty strong { display: block; font-size: 13.5px; color: var(--ed-text-main); margin-bottom: 2px; }
.ed-dashboard .ed-calendar-panel, .ed-dashboard .ed-charts-grid { grid-column: 1 / -1; }
.ed-dashboard .ed-week-calendar-panel { display: flex; flex-direction: column; max-height: 0; margin: 0; border: 0 solid var(--ed-border); overflow: hidden; opacity: 0; transform: translateY(-18px); visibility: hidden; pointer-events: none; transition: max-height .48s ease, margin .42s ease, border-width .42s ease, opacity .30s ease, transform .48s ease, visibility 0s linear .48s; }
.ed-dashboard .ed-week-calendar-panel.is-open { max-height: 1800px; margin-bottom: 16px; border-width: 1px; opacity: 1; transform: translateY(0); visibility: visible; pointer-events: auto; transition: max-height .52s ease, margin .42s ease, border-width .42s ease, opacity .32s ease, transform .48s ease, visibility 0s linear 0s; }
.ed-dashboard .ed-days { display: grid; grid-template-columns: repeat(7, 1fr); gap: 10px; }
.ed-dashboard .ed-day { background: #fff; border: 1px solid var(--ed-border); border-radius: 8px; padding: 12px; display: flex; flex-direction: column; min-height: 180px; }
.ed-dashboard .ed-day.is-today { border-color: #8b5cf6; box-shadow: 0 0 0 1.5px #8b5cf6; }
.ed-dashboard .ed-day.is-day-off { background: #f8fafc; opacity: 0.8; }
.ed-dashboard .ed-day-head { display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid #f1f5f9; padding-bottom: 6px; margin-bottom: 6px; }
.ed-dashboard .ed-day-name { font-size: 11.5px; font-weight: 600; color: var(--ed-text-muted); }
.ed-dashboard .ed-day-date { font-size: 16px; font-weight: 700; color: var(--ed-text-main); }
.ed-dashboard .ed-today-tag { font-size: 9px; padding: 2px 4px; background: #e0e7ff; color: #4338ca; border-radius: 4px; font-weight: 700; text-transform: uppercase; }
.ed-dashboard .ed-day-type { font-size: 11px; font-weight: 600; color: #059669; margin-bottom: 2px; }
.ed-dashboard .ed-day-type.off { color: #dc2626; }
.ed-dashboard .ed-day-hours { display: inline-flex; align-items: center; align-self: flex-start; gap: 5px; font-size: 10.5px; color: #475569; background: #f1f5f9; border: 1px solid #e2e8f0; border-radius: 5px; padding: 3px 6px; font-weight: 650; margin-bottom: 8px; }
.ed-dashboard .ed-day-hours i { color: #64748b; }
.ed-dashboard .ed-day-events { display: flex; flex-direction: column; gap: 6px; flex: 1; }
.ed-dashboard .ed-event { padding: 6px 8px; border-radius: 6px; border-left: 3px solid #cbd5e1; background: #f8fafc; font-size: 11.5px; line-height: 1.3; }
.ed-dashboard .ed-event.meeting { border-left-color: #3b82f6; background: #eff6ff; }
.ed-dashboard .ed-event.task { border-left-color: #10b981; background: #ecfdf5; }
.ed-dashboard .ed-event.special { border-left-color: #f59e0b; background: #fffbeb; }
.ed-dashboard .ed-event-time { display: inline-flex; max-width: 100%; font-size: 10px; font-weight: 750; margin-bottom: 5px; padding: 2px 5px; border-radius: 4px; color: #475569; background: rgba(255, 255, 255, .82); font-variant-numeric: tabular-nums; }
.ed-dashboard .ed-event strong { display: block; color: var(--ed-text-main); font-weight: 600; }
.ed-dashboard .ed-day-empty { font-size: 11.5px; color: #94a3b8; text-align: center; padding: 10px 0; }
.ed-dashboard .ed-item-title { font-size: 13.5px; font-weight: 600; margin-bottom: 4px; color: var(--ed-text-main); }
.ed-dashboard .ed-item-meta { font-size: 11.5px; color: var(--ed-text-muted); display: flex; flex-wrap: wrap; gap: 6px 10px; margin-bottom: 4px; }
.ed-dashboard .ed-chip { padding: 2px 6px; background: #f1f5f9; border-radius: 4px; font-weight: 600; }
.ed-dashboard .ed-chip.impact { background: #fff7ed; color: #c2410c; }
.ed-dashboard .ed-modal-open-trigger { display: flex; width: 100%; margin-top: auto; }
.ed-dashboard .ed-modal-open-trigger > div { display: block; width: 100%; }
.ed-dashboard .ed-modal-open-trigger .ed-more-btn { width: 100%; margin-top: 0; }
@keyframes ed-fade-in {
    from { opacity: 0; transform: translateY(5px); }
    to { opacity: 1; transform: translateY(0); }
}
@media (max-width: 1300px) {
    .ed-header-row { grid-template-columns: repeat(2, minmax(0, 1fr)); }
    .ed-header-profile, .ed-header-tools { grid-column: 1 / -1; }
}
@media (max-width: 1250px) {
    .ed-dashboard .ed-days { grid-template-columns: repeat(4, 1fr); }
}
@media (max-width: 1100px) {
    .ed-dashboard .ed-layout { grid-template-columns: 1fr; }
    .ed-dashboard .ed-charts-grid { grid-template-columns: 1fr; }
}
@media (max-width: 768px) {
    .ed-header-row { grid-template-columns: minmax(0, 1fr); }
    .ed-header-profile, .ed-header-tools { grid-column: 1 / -1; }
    .ed-header-time { align-items: stretch; text-align: left; }
    .ed-header-time .ed-week-actions { justify-content: flex-start; }
    .ed-dashboard .ed-days { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
@media (max-width: 500px) {
    .ed-header-tools { grid-template-columns: minmax(0, 1fr); }
    .ed-header-time { align-items: center; text-align: center; }
    .ed-header-time .ed-week-actions { justify-content: center; }
    .ed-dashboard .ed-days { grid-template-columns: 1fr; }
    .ed-chart-container { flex-direction: column; gap: 20px; }
    .ed-chart-legend { max-width: 100%; width: 100%; }
}
/* Popup: chỉ căn bố cục, không ghi đè màu chữ, nền trang, bóng hay giao diện ExtraModal gốc. */
.ed-header-stat { text-decoration:none!important;cursor:pointer; }
.ed-header-stat:link, .ed-header-stat:visited { color:inherit; }
.ed-header-stat:focus-visible { outline:2px solid currentColor;outline-offset:2px; }
.ed-dashboard .ed-charts-grid { margin-top:0; }
.ed-popup-inner { padding:16px 18px 18px;min-width:0; }
.ed-popup-top { display:flex;align-items:flex-start;justify-content:space-between;gap:14px;margin-bottom:14px;padding-bottom:12px;border-bottom:1px solid #e2e8f0; }
.ed-popup-heading { min-width:0; }
.ed-popup-heading h2 { margin:0 0 5px;font-size:17px;font-weight:700; }
.ed-popup-heading p { margin:0; }
.ed-popup-count { flex:none;white-space:nowrap; }
.ed-popup-actions { display:flex;align-items:center;gap:9px;flex-wrap:wrap;margin-bottom:14px; }
.ed-popup-search { flex:1 1 250px;min-width:180px; }
.ed-popup-search input { display:block;width:100%;min-height:38px; }
.ed-popup-filters { display:flex;align-items:center;gap:6px;flex-wrap:wrap; }
.ed-popup-results { display:grid;grid-template-columns:repeat(2,minmax(0,1fr));align-content:start;gap:12px;max-height:min(64vh,610px);overflow:auto;padding:1px 3px 3px 1px; }
.ed-popup-card { min-width:0;border:1px solid #e2e8f0;border-radius:8px;padding:14px;background:#fff; }
.ed-popup-card[hidden] { display:none!important; }
.ed-popup-card-header { display:flex;align-items:flex-start;gap:10px; }
.ed-popup-card-icon { display:flex;align-items:center;justify-content:center;flex:0 0 28px;width:28px;height:28px; }
.ed-popup-card-main { flex:1;min-width:0; }
.ed-popup-card-code { margin:0 0 4px; }
.ed-popup-card-name { display:block;margin:0;font-size:14px;font-weight:700;line-height:1.4;overflow-wrap:anywhere; }
.ed-popup-card-titleline { display:flex;align-items:flex-start;justify-content:space-between;gap:10px; }
.ed-popup-meta { display:flex;align-items:center;gap:7px 16px;flex-wrap:wrap;margin-top:10px; }
.ed-popup-meta i { margin-right:4px; }
.ed-popup-badge { display:inline-flex;align-items:center;gap:5px;flex-shrink:0;padding:4px 7px;border:1px solid #e2e8f0;border-radius:6px; }
.ed-popup-progress { height:6px;margin-top:13px;background:#e2e8f0;border-radius:9px;overflow:hidden; }
.ed-popup-progress span { display:block;height:100%;background:#5b21b6;border-radius:9px; }
.ed-popup-progress-caption { display:flex;justify-content:space-between;gap:8px;margin-top:5px; }
.ed-popup-empty { padding:20px;text-align:center;border:1px dashed #e2e8f0;border-radius:8px; }
.ed-popup-no-match { display:none;padding:16px;text-align:center; }
.ed-popup-no-match.is-visible { display:block; }
.ed-popup-project-filter { display:flex;align-items:center;flex-wrap:wrap;gap:10px;margin-bottom:12px; }
.ed-popup-note { padding-top:12px;margin-top:12px;border-top:1px solid #e2e8f0; }
.ed-popup-calendar-event { display:block;padding:7px 0;border-bottom:1px solid #e2e8f0; }
.ed-popup-calendar-event:last-child { border-bottom:0; }
@media(max-width:1050px) { .ed-header-stat { min-height:100px; } }
@media(max-width:800px) { .ed-popup-results { grid-template-columns:1fr; } }
@media(max-width:550px) { .ed-popup-inner { padding:12px; } }
@media(max-width:550px) { .ed-popup-top { flex-wrap:wrap; } }
@media(max-width:550px) { .ed-popup-card-titleline { flex-wrap:wrap; } }
.ed-dashboard .ed-week-calendar-panel.is-open { max-height:none; }

/* Lịch cố định, chỉ hai dòng đếm; cân chiều cao với hai biểu đồ trên desktop. */
.ed-header-row { gap:12px;margin-bottom:10px; }
.ed-header-profile { padding:10px 14px; }
.ed-header-profile .ed-avatar,.ed-header-profile .ed-avatar-fallback { width:52px;height:52px;flex-basis:52px; }
.ed-header-stat { min-height:94px;padding:7px; }
.ed-header-tools { display:flex;align-items:center;justify-content:flex-end;min-height:94px; }
.ed-header-time { min-height:94px;padding:8px 12px; }
.ed-dashboard .ed-week-calendar-panel,.ed-dashboard .ed-week-calendar-panel.is-open { display:flex;max-height:none;margin:0 0 10px;border-width:1px;overflow:visible;opacity:1;transform:none;visibility:visible;pointer-events:auto;transition:none; }
.ed-dashboard .ed-panel-head { padding:10px 14px;align-items:center; }
.ed-dashboard .ed-panel-body { padding:10px 12px;gap:6px; }
.ed-dashboard .ed-days { gap:8px; }
.ed-dashboard .ed-day { min-height:120px;padding:9px 9px 8px;gap:6px; }
.ed-dashboard .ed-day-head { margin-bottom:3px;padding-bottom:5px;gap:6px; }
.ed-dashboard .ed-day-name { font-size:11px; }
.ed-dashboard .ed-day-date { font-size:14px; }
.ed-dashboard .ed-day-summary { display:flex;flex-direction:column;gap:6px;margin-top:auto; }
.ed-dashboard .ed-day-count { display:flex;align-items:center;justify-content:space-between;gap:5px;min-width:0;min-height:32px;padding:5px 8px;border:1px solid #dce5f0;border-radius:6px;background:#f8fafc;color:#475569;font-size:11.5px;font-weight:650;white-space:nowrap; }
.ed-dashboard .ed-day-count span { display:inline-flex;align-items:center;gap:6px;min-width:0; }
.ed-dashboard .ed-day-count.is-meeting { border-color:#cfe0fd;background:#eff6ff;color:#2459a8; }
.ed-dashboard .ed-day-count.is-task { border-color:#ccece0;background:#effaf5;color:#117654; }
.ed-dashboard .ed-day-count .ed-day-value { flex:0 0 auto;min-width:23px;padding:2px 5px;border:1px solid currentColor;border-radius:5px;text-align:center;background:#fff;font-size:12px;font-weight:800;font-variant-numeric:tabular-nums; }
.ed-dashboard .ed-day.is-day-off { opacity:1; }
.ed-dashboard .ed-charts-grid { gap:12px;margin-bottom:0; }
.ed-chart-container { padding:8px 10px;gap:24px; }
.ed-donut-wrapper { width:140px;height:140px;flex:0 0 140px; }
.ed-donut-wrapper::before { inset:22px; }
.ed-donut-number { font-size:30px; }
.ed-chart-legend { gap:7px;max-width:240px; }
.ed-legend-item { padding:8px 11px; }
.ed-dashboard { padding-bottom:6px; }
@media(max-width:1300px) { .ed-header-row { gap:10px; } }
@media(max-width:1250px) { .ed-dashboard .ed-days { grid-template-columns:repeat(4,minmax(0,1fr)); } }
@media(max-width:768px) { .ed-dashboard .ed-days { grid-template-columns:repeat(2,minmax(0,1fr)); } }
@media(max-width:500px) { .ed-dashboard .ed-days { grid-template-columns:1fr; } }
@media(min-width:1101px) and (max-width:1300px) { .ed-header-row { grid-template-columns:minmax(0,3fr) repeat(4,minmax(0,1fr)) minmax(0,3fr); } .ed-header-profile,.ed-header-tools { grid-column:auto; } }
@media(min-width:1101px) and (max-width:1250px) { .ed-dashboard .ed-days { grid-template-columns:repeat(7,minmax(0,1fr)); } }
</style>
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="cpMain" runat="server">
<div class="ed-dashboard" id="edEmployeeDashboard">
    <asp:Panel ID="pnlDashboardMessage" runat="server" Visible="false" CssClass="ed-alert"><i class="fas fa-exclamation-circle"></i> <asp:Label ID="lblDashboardMessage" runat="server" /></asp:Panel>
    <div class="ed-header-row">
        <div class="ed-header-profile">
            <asp:Image ID="imgAvatar" runat="server" Visible="false" CssClass="ed-avatar" AlternateText="Ảnh đại diện" />
            <asp:Label ID="lblAvatarFallback" runat="server" CssClass="ed-avatar-fallback" />
            <div style="min-width:0;flex:1"><div class="ed-eyebrow">Không gian làm việc</div><h1><asp:Label ID="lblDisplayName" runat="server" Text="Dashboard" /></h1><div class="ed-profile-meta"><asp:PlaceHolder ID="phDepartment" runat="server" Visible="false"><span><i class="fas fa-building"></i> <asp:Label ID="lblDepartment" runat="server" /></span></asp:PlaceHolder><asp:PlaceHolder ID="phPosition" runat="server" Visible="false"><span><i class="fas fa-id-badge"></i> <asp:Label ID="lblPosition" runat="server" /></span></asp:PlaceHolder></div></div>
        </div>
        <asp:LinkButton ID="btnDashboardProjects" runat="server" CssClass="ed-header-stat" style="--c:#8b5cf6;--soft:#f3efff" CausesValidation="false" OnClientClick="return edOpenDashboardModal('mdlProjects');" OnClick="lnkMoreProjects_Click" ToolTip="Xem danh sách dự án tham gia" aria-label="Xem dự án tham gia">
            <div class="ed-stat-val"><asp:Label ID="lblProjectCount" runat="server" /></div><div class="ed-stat-lbl">Dự án<br/>tham gia</div><i class="fas fa-layer-group ed-stat-ic"></i>
        </asp:LinkButton>
        <asp:LinkButton ID="btnDashboardTasks" runat="server" CssClass="ed-header-stat" style="--c:#3b82f6;--soft:#eef6ff" CausesValidation="false" OnClientClick="edSetTaskScope('all');return edOpenDashboardModal('mdlTasks');" OnClick="lnkMoreTasks_Click" ToolTip="Xem công việc trong tuần" aria-label="Xem công việc trong tuần">
            <div class="ed-stat-val"><asp:Label ID="lblTaskCount" runat="server" /></div><div class="ed-stat-lbl">Công việc<br/>trong tuần</div><i class="fas fa-tasks ed-stat-ic"></i>
        </asp:LinkButton>
        <asp:LinkButton ID="btnDashboardOverdue" runat="server" CssClass="ed-header-stat" style="--c:#ef4444;--soft:#fff1f0" CausesValidation="false" OnClientClick="edSetTaskScope('overdue');return edOpenDashboardModal('mdlTasks');" OnClick="lnkMoreTasks_Click" ToolTip="Xem các công việc quá hạn" aria-label="Xem công việc quá hạn">
            <div class="ed-stat-val"><asp:Label ID="lblOverdueCount" runat="server" /></div><div class="ed-stat-lbl">Việc<br/>quá hạn</div><i class="fas fa-exclamation-triangle ed-stat-ic"></i>
        </asp:LinkButton>
        <asp:LinkButton ID="btnDashboardIssues" runat="server" CssClass="ed-header-stat" style="--c:#10b981;--soft:#edfbf5" CausesValidation="false" OnClientClick="return edOpenDashboardModal('mdlIssues');" OnClick="lnkMoreIssues_Click" ToolTip="Xem các vấn đề được giao" aria-label="Xem vấn đề được giao">
            <div class="ed-stat-val"><asp:Label ID="lblIssueCount" runat="server" /></div><div class="ed-stat-lbl">Vấn đề<br/>được giao</div><i class="fas fa-life-ring ed-stat-ic"></i>
        </asp:LinkButton>
        <div class="ed-header-tools">
            
            <div class="ed-header-time"><div class="ed-week-label"><i class="far fa-calendar-alt"></i> Tổng quan theo tuần</div><div class="ed-week-title"><asp:Label ID="lblWeekRange" runat="server" /></div><div class="ed-week-actions"><asp:HyperLink ID="lnkPreviousWeek" runat="server" CssClass="ed-week-btn" Text="‹ Trước" /><asp:HyperLink ID="lnkCurrentWeek" runat="server" CssClass="ed-week-btn ed-current" Text="Hiện tại" /><asp:HyperLink ID="lnkNextWeek" runat="server" CssClass="ed-week-btn" Text="Sau ›" /></div></div>
        </div>
    </div>
    <section id="edWeekCalendar" class="ed-panel ed-calendar-panel ed-week-calendar-panel is-open" aria-labelledby="edCalendarHeading" aria-hidden="false">
        <div class="ed-panel-head"><div class="ed-panel-heading"><span class="ed-panel-icon"><i class="far fa-calendar-alt"></i></span><div><h2 id="edCalendarHeading" class="ed-panel-title">Lịch làm việc trong tuần</h2><div class="ed-panel-subtitle">Tổng hợp công việc, lịch họp và ngày nghỉ trong tuần</div></div></div><asp:LinkButton ID="lnkCalendarMore" runat="server" CssClass="ed-week-btn" OnClientClick="return edOpenDashboardModal('mdlCalendar');" OnClick="lnkCalendar_Click" CausesValidation="false"><i class="fas fa-expand-alt"></i> Xem lịch chi tiết</asp:LinkButton></div>
        <div class="ed-panel-body"><asp:Repeater ID="rptDays" runat="server"><HeaderTemplate><div class="ed-days"></HeaderTemplate><ItemTemplate>
            <div class='ed-day <%#: Eval("CssClass") %>'><div class="ed-day-head"><span class="ed-day-name"><%#: Eval("DayName") %></span><span class="ed-day-date"><%#: Eval("DayNumber") %></span><asp:PlaceHolder ID="phToday" runat="server" Visible='<%# (bool)Eval("IsToday") %>'><span class="ed-today-tag">Hôm nay</span></asp:PlaceHolder></div><div class="ed-day-summary"><div class="ed-day-count is-meeting"><span><i class="far fa-calendar-alt" aria-hidden="true"></i> Cuộc họp</span><strong class="ed-day-value"><%#: Eval("MeetingCount") %></strong></div><div class="ed-day-count is-task"><span><i class="fas fa-tasks" aria-hidden="true"></i> Công việc</span><strong class="ed-day-value"><%#: Eval("TaskCount") %></strong></div></div></div>
        </ItemTemplate><FooterTemplate></div></FooterTemplate></asp:Repeater></div>
    </section>
    <div class="ed-charts-grid">
        <section class="ed-panel" aria-labelledby="edTaskChartHeading"><div class="ed-panel-head"><div class="ed-panel-heading"><span class="ed-panel-icon" style="background:#e9eeff;color:#465bd3"><i class="fas fa-chart-pie"></i></span><h2 id="edTaskChartHeading" class="ed-panel-title">Trạng thái công việc</h2></div></div><div class="ed-panel-body"><div class="ed-chart-container"><div class="ed-donut-wrapper" id="divTaskChart" runat="server"><div class="ed-donut-center"><asp:Label ID="lblTotalTasksChart" runat="server" CssClass="ed-donut-number" /><span class="ed-donut-label">công việc</span></div></div><div class="ed-chart-legend"><div class="ed-legend-item"><span class="ed-dot" style="background:#ef4444"></span> Trễ hạn <asp:Label ID="lblTaskOverdueChart" runat="server" style="margin-left:auto;color:#1e293b;font-size:15px;font-weight:800" /></div><div class="ed-legend-item"><span class="ed-dot" style="background:#f59e0b"></span> Sắp đến hạn <asp:Label ID="lblTaskDueSoonChart" runat="server" style="margin-left:auto;color:#1e293b;font-size:15px;font-weight:800" /></div><div class="ed-legend-item"><span class="ed-dot" style="background:#3b82f6"></span> Trong tiến độ <asp:Label ID="lblTaskNormalChart" runat="server" style="margin-left:auto;color:#1e293b;font-size:15px;font-weight:800" /></div></div></div></div></section>
        <section class="ed-panel" aria-labelledby="edProjectChartHeading"><div class="ed-panel-head"><div class="ed-panel-heading"><span class="ed-panel-icon" style="background:#f5eafa;color:#a443c4"><i class="fas fa-chart-pie"></i></span><h2 id="edProjectChartHeading" class="ed-panel-title">Trạng thái dự án</h2></div></div><div class="ed-panel-body"><div class="ed-chart-container"><div class="ed-donut-wrapper" id="divProjectChart" runat="server"><div class="ed-donut-center"><asp:Label ID="lblTotalProjectsChart" runat="server" CssClass="ed-donut-number" /><span class="ed-donut-label">dự án</span></div></div><div class="ed-chart-legend"><div class="ed-legend-item"><span class="ed-dot" style="background:#ef4444"></span> Trễ hạn <asp:Label ID="lblProjectOverdueChart" runat="server" style="margin-left:auto;color:#1e293b;font-size:15px;font-weight:800" /></div><div class="ed-legend-item"><span class="ed-dot" style="background:#f59e0b"></span> Sắp đến hạn <asp:Label ID="lblProjectDueSoonChart" runat="server" style="margin-left:auto;color:#1e293b;font-size:15px;font-weight:800" /></div><div class="ed-legend-item"><span class="ed-dot" style="background:#3b82f6"></span> Trong tiến độ <asp:Label ID="lblProjectNormalChart" runat="server" style="margin-left:auto;color:#1e293b;font-size:15px;font-weight:800" /></div></div></div></div></section>
    </div>
</div>

<div class="ed-modal-host">
    <SweetSoft:ExtraModal ID="mdlProjects" runat="server" Title="Các dự án đang tham gia" Width="80%">
        <ContentTemplate>
            <asp:UpdatePanel ID="upnlModalProjects" runat="server" UpdateMode="Conditional"><ContentTemplate>
                <div class="ed-popup-inner" data-ed-modal-kind="projects">
                    <div class="ed-popup-top"><div class="ed-popup-heading"><h2>Danh sách dự án</h2><p class="text-muted small">Dự án bạn tham gia, số công việc và tiến độ hoàn thành.</p></div><span class="ed-popup-count badge bg-light text-dark border" data-ed-count>0 mục</span></div>
                    <div class="ed-popup-actions"><div class="ed-popup-search"><input class="form-control" type="search" placeholder="Tìm mã hoặc tên dự án..." aria-label="Tìm dự án" oninput="edFilterDashboardModal(this)" /></div></div>
                    <asp:Repeater ID="rptProjectsAll" runat="server" OnItemCommand="rptProjectsAll_ItemCommand">
                        <HeaderTemplate><div class="ed-popup-results"></HeaderTemplate><ItemTemplate>
                            <article class="ed-popup-card" data-ed-item data-ed-search='<%#: Eval("Code") %> <%#: Eval("Name") %> <%#: Eval("TaskSummary") %>'>
                                <div class="ed-popup-card-header"><span class="ed-popup-card-icon"><i class="fas fa-folder-open"></i></span><div class="ed-popup-card-main"><div class="ed-popup-card-code small text-muted"><%#: Eval("Code") %></div><asp:LinkButton ID="lnkProjectModal" runat="server" CssClass="ed-popup-card-name" CommandName="FILTER_TASKS" CommandArgument='<%# Eval("Id") %>' OnClientClick="edSetTaskScope('all');" ToolTip="Xem công việc của dự án"><%#: Eval("Name") %> <i class="fas fa-arrow-right fa-xs"></i></asp:LinkButton></div></div>
                                <div class="ed-popup-meta small text-muted"><span><i class="far fa-calendar-alt"></i><%#: Eval("Deadline") %></span><span><i class="fas fa-tasks"></i><%#: Eval("TaskSummary") %></span></div>
                                <div class="ed-popup-progress"><span style='width:<%#: Eval("Progress") %>%'></span></div><div class="ed-popup-progress-caption small"><span class="text-muted">Tiến độ dự án</span><strong><%#: Eval("Progress") %>%</strong></div>
                            </article>
                        </ItemTemplate><FooterTemplate></div></FooterTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoProjects" runat="server" Visible="false" CssClass="ed-popup-empty">Chưa có dự án cần theo dõi.</asp:Panel>
                    <div class="ed-popup-no-match text-muted" data-ed-empty-search>Không tìm thấy dự án phù hợp.</div>
                    <div class="ed-popup-note small text-muted">Bấm tên dự án để xem danh sách công việc thuộc dự án đó.</div>
                </div>
            </ContentTemplate></asp:UpdatePanel>
        </ContentTemplate>
    </SweetSoft:ExtraModal>
    <SweetSoft:ExtraModal ID="mdlTasks" runat="server" Title="Công việc của tôi" Width="80%">
        <ContentTemplate>
            <asp:UpdatePanel ID="upnlModalTasks" runat="server" UpdateMode="Conditional"><ContentTemplate>
                <div class="ed-popup-inner" data-ed-modal-kind="tasks">
                    <div class="ed-popup-top"><div class="ed-popup-heading"><h2 data-ed-task-heading>Công việc cần theo dõi</h2><p class="text-muted small">Theo dõi thời hạn, trạng thái và tiến độ thực hiện.</p></div><span class="ed-popup-count badge bg-light text-dark border" data-ed-count>0 mục</span></div>
                    <div class="ed-popup-project-filter"><asp:Label ID="lblProjectFilterName" runat="server" Visible="false" CssClass="badge bg-light text-dark border" /><asp:LinkButton ID="btnAttentionTasks" runat="server" Visible="false" CausesValidation="false" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnAttentionTasks_Click"><i class="fas fa-times"></i> Bỏ lọc dự án</asp:LinkButton></div>
                    <div class="ed-popup-actions"><div class="ed-popup-search"><input class="form-control" type="search" placeholder="Tìm công việc, mã hoặc dự án..." aria-label="Tìm công việc" oninput="edFilterDashboardModal(this)" /></div><div class="ed-popup-filters"><button type="button" class="btn btn-primary btn-sm ed-modal-filter is-active" data-ed-filter="all" onclick="edSetTaskScope('all',this)">Tất cả</button><button type="button" class="btn btn-outline-primary btn-sm ed-modal-filter" data-ed-filter="overdue" onclick="edSetTaskScope('overdue',this)">Quá hạn</button></div></div>
                    <asp:Repeater ID="rptTasksAll" runat="server"><HeaderTemplate><div class="ed-popup-results"></HeaderTemplate><ItemTemplate>
                        <article class="ed-popup-card" data-ed-item data-ed-status='<%#: Eval("StatusCss") %>' data-ed-search='<%#: Eval("ProjectCode") %> <%#: Eval("TaskCode") %> <%#: Eval("TaskName") %> <%#: Eval("StatusText") %>'>
                            <div class="ed-popup-card-header"><span class="ed-popup-card-icon"><i class="fas fa-tasks"></i></span><div class="ed-popup-card-main"><div class="ed-popup-card-code small text-muted"><%#: Eval("ProjectCode") %> · <%#: Eval("TaskCode") %></div><div class="ed-popup-card-titleline"><strong class="ed-popup-card-name"><%#: Eval("TaskName") %></strong><span class="ed-popup-badge small"><%#: Eval("StatusText") %></span></div></div></div>
                            <div class="ed-popup-meta small text-muted"><span><i class="far fa-calendar-alt"></i><%#: Eval("DateRange") %></span></div>
                            <div class="ed-popup-progress"><span style='width:<%#: Eval("Progress") %>%'></span></div><div class="ed-popup-progress-caption small"><span class="text-muted">Tiến độ công việc</span><strong><%#: Eval("Progress") %>%</strong></div>
                        </article>
                    </ItemTemplate><FooterTemplate></div></FooterTemplate></asp:Repeater>
                    <asp:Panel ID="pnlNoAllTasks" runat="server" Visible="false" CssClass="ed-popup-empty">Không có công việc cần theo dõi.</asp:Panel>
                    <div class="ed-popup-no-match text-muted" data-ed-empty-search>Không có công việc phù hợp với bộ lọc.</div>
                    <div class="ed-popup-note small text-muted">Dùng bộ lọc “Quá hạn” để tập trung vào công việc cần ưu tiên.</div>
                </div>
            </ContentTemplate></asp:UpdatePanel>
        </ContentTemplate>
    </SweetSoft:ExtraModal>
    <SweetSoft:ExtraModal ID="mdlIssues" runat="server" Title="Vấn đề được giao" Width="80%">
        <ContentTemplate>
            <asp:UpdatePanel ID="upnlModalIssues" runat="server" UpdateMode="Conditional"><ContentTemplate>
                <div class="ed-popup-inner" data-ed-modal-kind="issues">
                    <div class="ed-popup-top"><div class="ed-popup-heading"><h2>Vấn đề được giao</h2><p class="text-muted small">Theo dõi vấn đề theo dự án, mức ảnh hưởng và trạng thái.</p></div><span class="ed-popup-count badge bg-light text-dark border" data-ed-count>0 mục</span></div>
                    <div class="ed-popup-actions"><div class="ed-popup-search"><input class="form-control" type="search" placeholder="Tìm vấn đề hoặc dự án..." aria-label="Tìm vấn đề" oninput="edFilterDashboardModal(this)" /></div></div>
                    <asp:Repeater ID="rptIssuesAll" runat="server"><HeaderTemplate><div class="ed-popup-results"></HeaderTemplate><ItemTemplate>
                        <article class="ed-popup-card" data-ed-item data-ed-search='<%#: Eval("Code") %> <%#: Eval("Name") %> <%#: Eval("ProjectName") %> <%#: Eval("Impact") %> <%#: Eval("StatusText") %>'>
                            <div class="ed-popup-card-header"><span class="ed-popup-card-icon"><i class="fas fa-exclamation-circle"></i></span><div class="ed-popup-card-main"><div class="ed-popup-card-code small text-muted"><%#: Eval("Code") %></div><strong class="ed-popup-card-name"><%#: Eval("Name") %></strong></div></div>
                            <div class="ed-popup-meta small text-muted"><span><i class="fas fa-folder"></i><%#: Eval("ProjectName") %></span></div><div class="ed-popup-meta small"><span class="ed-popup-badge"><%#: Eval("Impact") %></span><span class="ed-popup-badge"><%#: Eval("StatusText") %></span></div>
                        </article>
                    </ItemTemplate><FooterTemplate></div></FooterTemplate></asp:Repeater>
                    <asp:Panel ID="pnlNoIssues" runat="server" Visible="false" CssClass="ed-popup-empty">Bạn chưa có vấn đề được phân công.</asp:Panel>
                    <div class="ed-popup-no-match text-muted" data-ed-empty-search>Không tìm thấy vấn đề phù hợp.</div>
                </div>
            </ContentTemplate></asp:UpdatePanel>
        </ContentTemplate>
    </SweetSoft:ExtraModal>
    <SweetSoft:ExtraModal ID="mdlCalendar" runat="server" Title="Lịch làm việc chi tiết" Width="80%">
        <ContentTemplate>
            <asp:UpdatePanel ID="upnlModalCalendar" runat="server" UpdateMode="Conditional"><ContentTemplate>
                <div class="ed-popup-inner" data-ed-modal-kind="calendar"><div class="ed-popup-top"><div class="ed-popup-heading"><h2>Lịch làm việc chi tiết</h2><p class="text-muted small">Toàn bộ sự kiện, công việc và cuộc họp trong tuần.</p></div></div>
                    <asp:Repeater ID="rptCalendarAll" runat="server"><HeaderTemplate><div class="ed-popup-results"></HeaderTemplate><ItemTemplate>
                        <article class="ed-popup-card"><div class="ed-popup-card-code small text-muted"><%#: Eval("DayName") %> · <%#: Eval("DayNumber") %></div><strong class="ed-popup-card-name"><%#: Eval("DayType") %></strong><div class="ed-popup-meta small text-muted"><%#: Eval("WorkHours") %></div>
                            <asp:Repeater ID="rptEventsAll" runat="server" DataSource='<%# Eval("Events") %>'><ItemTemplate><div class="ed-popup-calendar-event small"><strong><%#: Eval("TimeLabel") %> — <%#: Eval("Title") %></strong><div class="text-muted"><%#: Eval("Subtitle") %></div></div></ItemTemplate></asp:Repeater>
                        </article>
                    </ItemTemplate><FooterTemplate></div></FooterTemplate></asp:Repeater>
                </div>
            </ContentTemplate></asp:UpdatePanel>
        </ContentTemplate>
    </SweetSoft:ExtraModal>
</div>
<script type="text/javascript">
    function edFindDashboardModal(name) {
        var direct = document.getElementById(name);
        var container = direct || document.querySelector('[id$="_' + name + '"]');
        if (!container) return null;
        return container.classList.contains('modal') ? container : container.querySelector('.modal');
    }
    function edOpenDashboardModal(name) {
        var modal = edFindDashboardModal(name);
        if (!modal) { console.error('Không tìm thấy ExtraModal:', name); return false; }
        var panel = modal.querySelector('.ed-popup-inner');
        if (panel) edApplyDashboardFilter(panel);
        if (window.bootstrap && window.bootstrap.Modal) {
            (window.bootstrap.Modal.getInstance && window.bootstrap.Modal.getInstance(modal) || new window.bootstrap.Modal(modal)).show();
        } else if (window.jQuery && typeof window.jQuery.fn.modal === 'function') {
            window.jQuery(modal).modal('show');
        } else {
            console.error('Bootstrap Modal chưa được tải; không thể mở popup:', name);
        }
        return false;
    }
    function edSwitchDashboardModal(fromName, toName) {
        var previous = edFindDashboardModal(fromName);
        if (previous && previous.classList.contains('show') && window.bootstrap && window.bootstrap.Modal) {
            previous.addEventListener('hidden.bs.modal', function () { edOpenDashboardModal(toName); }, { once: true });
            (window.bootstrap.Modal.getInstance && window.bootstrap.Modal.getInstance(previous) || new window.bootstrap.Modal(previous)).hide();
        } else if (previous && previous.classList.contains('show') && window.jQuery && typeof window.jQuery.fn.modal === 'function') {
            window.jQuery(previous).one('hidden.bs.modal', function () { edOpenDashboardModal(toName); }).modal('hide');
        } else {
            edOpenDashboardModal(toName);
        }
        return false;
    }
    function edSetTaskScope(scope, button) {
        window.edTaskPopupScope = scope === 'overdue' ? 'overdue' : 'all';
        try { sessionStorage.setItem('SweetSoftEmployeeTaskPopupScope', window.edTaskPopupScope); } catch (error) { }
        var panel = document.querySelector('[data-ed-modal-kind="tasks"]');
        if (!panel) return;
        var title = panel.querySelector('[data-ed-task-heading]');
        if (title) title.textContent = window.edTaskPopupScope === 'overdue' ? 'Công việc quá hạn' : 'Công việc cần theo dõi';
        panel.querySelectorAll('[data-ed-filter]').forEach(function (control) { var active = control.getAttribute('data-ed-filter') === window.edTaskPopupScope; control.classList.toggle('is-active', active); control.classList.toggle('btn-primary', active); control.classList.toggle('btn-outline-primary', !active); });
        edApplyDashboardFilter(panel);
    }
    function edFilterDashboardModal(input) {
        var panel = input && input.closest('.ed-popup-inner');
        if (panel) edApplyDashboardFilter(panel);
    }
    function edApplyDashboardFilter(panel) {
        if (!panel) return;
        var input = panel.querySelector('.ed-popup-search input');
        var term = input ? input.value.trim().toLocaleLowerCase('vi') : '';
        var taskFilter = panel.getAttribute('data-ed-modal-kind') === 'tasks' ? (window.edTaskPopupScope || 'all') : 'all';
        var all = panel.querySelectorAll('[data-ed-item]');
        var count = 0;
        all.forEach(function (card) {
            var matchesSearch = !term || (card.getAttribute('data-ed-search') || card.textContent || '').toLocaleLowerCase('vi').includes(term);
            var matchesStatus = taskFilter !== 'overdue' || card.getAttribute('data-ed-status') === 'overdue';
            card.hidden = !(matchesSearch && matchesStatus);
            if (!card.hidden) count++;
        });
        var badge = panel.querySelector('[data-ed-count]');
        if (badge) badge.textContent = count + ' mục';
        var noResults = panel.querySelector('[data-ed-empty-search]');
        if (noResults) noResults.classList.toggle('is-visible', count === 0 && all.length > 0);
    }
    function edInitDashboardPopups() {
        try { window.edTaskPopupScope = sessionStorage.getItem('SweetSoftEmployeeTaskPopupScope') === 'overdue' ? 'overdue' : 'all'; } catch (error) { window.edTaskPopupScope = window.edTaskPopupScope || 'all'; }
        document.querySelectorAll('.ed-popup-inner').forEach(function (panel) {
            if (panel.getAttribute('data-ed-modal-kind') === 'tasks') {
                var heading = panel.querySelector('[data-ed-task-heading]');
                if (heading) heading.textContent = window.edTaskPopupScope === 'overdue' ? 'Công việc quá hạn' : 'Công việc cần theo dõi';
                panel.querySelectorAll('[data-ed-filter]').forEach(function (control) { var active = control.getAttribute('data-ed-filter') === window.edTaskPopupScope; control.classList.toggle('is-active', active); control.classList.toggle('btn-primary', active); control.classList.toggle('btn-outline-primary', !active); });
            }
            edApplyDashboardFilter(panel);
        });
    }
    if (window.Sys && Sys.Application) Sys.Application.add_load(edInitDashboardPopups);
    else if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', edInitDashboardPopups);
    else edInitDashboardPopups();
</script>

</asp:Content>
