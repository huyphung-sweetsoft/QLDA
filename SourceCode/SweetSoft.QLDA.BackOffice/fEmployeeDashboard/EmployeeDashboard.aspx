<%@ Page Title="Dashboard nhân viên" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="EmployeeDashboard.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fEmployeeDashboard.EmployeeDashboard" %>
<%@ Register Assembly="SweetSoft.QLDA.Controls" Namespace="SweetSoft.QLDA.Controls" TagPrefix="SweetSoft" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Infrastructure" %>

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
.ed-dashboard .ed-event.meeting { border-left-color: #7c3aed; background: #f5f0ff; }
.ed-dashboard .ed-event.task { border-left-color: #2563eb; background: #eff6ff; }
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
.ed-popup-progress span { display:block;height:100%;background:#3b82f6;border-radius:9px; }
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
.ed-dashboard .ed-day-count.is-meeting { border-color:#d9c5fb;background:#f5efff;color:#6b21a8; }
.ed-dashboard .ed-day-count.is-task { border-color:#bfdbfe;background:#eff6ff;color:#1d4ed8; }
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

/* Nút khai báo giờ: dùng palette hiện có, không đổi màu các KPI và biểu đồ. */
.ed-header-tools { gap:0; }
.ed-time-entry { display:flex;align-items:center;justify-content:center;gap:9px;flex:1 1 43%;min-width:0;border:0;border-right:1px solid #e5def7;background:transparent;color:#54218d;font:inherit;font-size:12px;text-align:left;padding:8px 9px;cursor:pointer;transition:background .18s ease; }
.ed-time-entry:hover,.ed-time-entry:focus-visible { background:#f6f2ff;outline:none; }
.ed-time-entry-icon { display:flex;align-items:center;justify-content:center;width:43px;height:43px;border:1px solid #d7c5f2;background:#f1eafe;border-radius:10px;flex:none;font-size:19px; }
.ed-time-entry strong,.ed-time-entry small { display:block;line-height:1.3; }
.ed-time-entry strong { font-size:13px;color:#4c319b;font-weight:800; }
.ed-time-entry small { color:#64748b;font-size:10.5px;margin-top:3px; }
.ed-time-entry-arrow { color:#946dc0;margin-left:auto;font-size:11px; }
.ed-header-time { flex:1 1 57%; }
/* Tooltip nổi được chuyển ra document.body để tránh bị overflow của panel cắt mất. */
.ed-dashboard .ed-day-count { cursor:help;transition:border-color .17s ease,background .17s ease,transform .17s ease; }
.ed-dashboard .ed-day-count:hover,.ed-dashboard .ed-day-count:focus-visible { border-color:#9e81d0;transform:translateY(-1px);outline:none; }
.ed-tip-source[hidden] { display:none!important; }
.ed-floating-tip { position:fixed;z-index:1090;width:min(315px,calc(100vw - 24px));max-height:min(370px,calc(100vh - 24px));overflow-y:auto;padding:11px 13px;border:1px solid #d8caeb;border-radius:11px;background:#fff;color:#273345;box-shadow:0 12px 30px rgba(37,27,62,.15);pointer-events:none;opacity:0;visibility:hidden;transform:translateX(5px);transition:opacity .18s ease,transform .18s ease,visibility 0s linear .18s;font-size:12.5px;line-height:1.5; }
.ed-floating-tip.is-visible { opacity:1;visibility:visible;transform:translateX(0);transition:opacity .18s ease,transform .18s ease,visibility 0s; }
.ed-floating-tip.ed-tip-left { transform:translateX(-5px); }
.ed-floating-tip.ed-tip-left.is-visible { transform:translateX(0); }
.ed-tip-heading { display:flex;align-items:center;gap:7px;font-size:13px;font-weight:800;color:#54218d;padding-bottom:9px;border-bottom:1px solid #ece6f4; }
.ed-tip-item { display:flex;flex-direction:column;gap:3px;padding:10px 0;border-bottom:1px solid #f0edf5;overflow-wrap:anywhere; }
.ed-tip-item:last-child { border:0;padding-bottom:0; }
.ed-tip-item strong { color:#253248;font-size:12.5px; }
.ed-tip-item span { color:#64748b;font-size:11.5px; }
.ed-tip-item small { color:#725299;font-size:11px;font-weight:700; }
.ed-tip-empty { padding:17px 8px 8px;text-align:center;color:#69758a; }
/* ExtraModal mới: không thay viền, header hoặc nút X của control công ty. */
.ed-time-modal { padding:18px; }
.ed-time-intro { display:flex;align-items:flex-start;gap:13px;border-bottom:1px solid #e8e3ed;padding-bottom:14px;margin-bottom:14px; }
.ed-time-intro-icon { display:flex;align-items:center;justify-content:center;width:43px;height:43px;background:#f3efff;border-radius:9px;color:#5b21b6;font-size:19px;flex:none; }
.ed-time-intro h3 { margin:0 0 4px;font-size:17px;font-weight:800;color:#243149; }
.ed-time-intro p { margin:0;color:#64748b;font-size:12.5px;line-height:1.5; }
.ed-time-projects { display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:11px;max-height:min(54vh,510px);overflow:auto;padding:1px; }
.ed-time-project { display:flex;align-items:center;gap:11px;text-decoration:none!important;border:1px solid #e1dbe9;border-radius:9px;padding:13px;background:#fff;min-width:0;transition:border-color .16s ease,background .16s ease; }
.ed-time-project:hover,.ed-time-project:focus-visible { background:#faf7ff;border-color:#a78bdb;outline:none; }
.ed-time-project-mark { display:flex;align-items:center;justify-content:center;flex:none;width:36px;height:36px;border-radius:8px;background:#f2ebfd;color:#5b21b6; }
.ed-time-project-content { display:flex;flex-direction:column;gap:3px;min-width:0;flex:1; }
.ed-time-project-content small { color:#795b9d;font-size:11px;font-weight:750; }
.ed-time-project-content strong { color:#263347;font-size:13.5px;line-height:1.35;overflow-wrap:anywhere; }
.ed-time-project-content>span { color:#617189;font-size:11.5px; }
.ed-time-project-arrow { color:#7c52ae;margin-left:auto; }
.ed-time-no-project { grid-column:1/-1;display:flex;align-items:center;flex-direction:column;gap:7px;padding:26px 15px;border:1px dashed #e0d8ec;border-radius:9px;text-align:center;color:#64748b; }
.ed-time-no-project i { font-size:24px;color:#7654aa; }
.ed-time-no-project strong { color:#283349; }
.ed-time-footnote { font-size:11.5px;color:#69758a;margin:13px 0 0; }
/* Cân vùng biểu đồ theo chiều cao khung trình duyệt; không ép trên màn hình nhỏ. */
@media(min-width:1101px) and (min-height:780px) { .ed-dashboard { display:flex;flex-direction:column;min-height:calc(100dvh - 130px);padding-bottom:0; } .ed-dashboard .ed-charts-grid { flex:1;min-height:215px; } .ed-dashboard .ed-charts-grid>.ed-panel { min-height:0; } .ed-dashboard .ed-charts-grid>.ed-panel>.ed-panel-body { justify-content:center; } }
@media(max-width:1300px) { .ed-time-entry { padding:6px;gap:5px; } .ed-time-entry-icon { width:34px;height:34px;font-size:15px; } .ed-time-entry small { display:none; } .ed-time-entry-arrow { display:none; } }
@media(max-width:720px) { .ed-time-projects { grid-template-columns:1fr; } .ed-time-entry { min-height:64px; } }
@media(prefers-reduced-motion:reduce) { .ed-floating-tip,.ed-dashboard .ed-day-count { transition:none!important;transform:none!important; } }

/* Only layout dimensions for ExtraModal; no modal-header, modal-content, border or X styles. */
[id$="mdlProjects"].modal .modal-dialog,[id$="mdlProjects"] .modal-dialog { width:calc(100vw - 64px)!important;max-width:1260px!important; }
[id$="mdlTasks"].modal .modal-dialog,[id$="mdlTasks"] .modal-dialog,[id$="mdlIssues"].modal .modal-dialog,[id$="mdlIssues"] .modal-dialog,[id$="mdlCalendar"].modal .modal-dialog,[id$="mdlCalendar"] .modal-dialog { width:calc(100vw - 64px)!important;max-width:1060px!important; }
.ed-pop-headline { display:flex;align-items:center;justify-content:space-between;gap:14px;margin-bottom:15px;padding-bottom:13px;border-bottom:1px solid #e6e8ee; }
.ed-pop-headline h2 { margin:0 0 4px;font-size:19px;font-weight:750;color:#263246; }
.ed-pop-headline p { margin:0;color:#64748b;font-size:12.5px; }
.ed-pop-total { flex:none;padding:5px 9px;border:1px solid #e3e7ed;background:#f8fafc;border-radius:7px;color:#64748b;font-size:12px;font-weight:700; }
.ed-pop-filter { display:flex;align-items:center;gap:8px;flex-wrap:wrap;margin-bottom:12px; }
.ed-pop-search { position:relative;flex:1 1 260px;min-width:190px; }
.ed-pop-search i { position:absolute;left:13px;top:50%;transform:translateY(-50%);color:#948ba7;pointer-events:none; }
.ed-pop-search input { width:100%;height:41px;border:1px solid #d9dfeb;border-radius:8px;padding:9px 12px 9px 37px;outline:none;background:#fff;color:#263246;font:inherit;font-size:13px; }
.ed-pop-search input:focus { border-color:#a891d7;box-shadow:0 0 0 3px rgba(106,63,155,.09); }
.ed-pop-help { display:flex;align-items:center;gap:8px;margin-top:12px;font-size:12px;color:#748096; }
.ed-pop-help i { color:#7954a9; }
.ed-project-split { display:grid;grid-template-columns:minmax(340px,0.82fr) minmax(0,1.68fr);gap:0;min-height:480px;max-height:min(74vh,690px);overflow:hidden; }
.ed-project-sidebar { position:relative;display:flex;flex-direction:column;min-width:0;padding:19px 15px 16px 19px;border-right:1px solid #e7e9f1;background:#fbfcff; }
.ed-project-list { display:flex;flex-direction:column;gap:9px;overflow:auto;min-height:0;max-height:100%;padding:2px 9px 3px 1px; }
.ed-project-picker { position:relative;display:block;width:100%;min-width:0;text-align:left;border:1px solid #e0e5ed;background:#fff;padding:12px 13px;border-radius:9px;color:#263246;cursor:pointer;transition:background .15s ease,border-color .15s ease; }
.ed-project-picker:hover,.ed-project-picker:focus-visible { border-color:#bda5dc;background:#faf7ff;outline:none; }
.ed-project-picker.is-selected { border-color:#9972cf;background:#f8f3ff;box-shadow:inset 3px 0 #7142ab; }
.ed-project-picker-head { display:flex;align-items:center;justify-content:space-between;gap:8px;margin-bottom:7px;color:#7d599f;font-size:11px;font-weight:800; }
.ed-project-picker-name { display:block;width:100%;font-size:14px;line-height:1.35;font-weight:730;color:#263246;overflow-wrap:break-word; }
.ed-project-picker-meta { display:flex;align-items:center;gap:5px 11px;flex-wrap:wrap;font-size:11.5px;color:#677589;margin-top:9px; }
.ed-project-picker-meta i { color:#8f79b3; }
.ed-project-mini-progress { margin-top:11px;display:flex;align-items:center;gap:9px;font-size:11px;color:#65547d; }
.ed-project-mini-track { flex:1;min-width:0;height:6px;border-radius:20px;background:#e7e4ef;overflow:hidden; }
.ed-project-mini-track span { display:block;height:100%;background:#7942b9;border-radius:20px; }
.ed-project-detail { min-width:0;display:flex;flex-direction:column;padding:19px 20px 18px 25px; }
.ed-project-focus { flex:none;display:grid;grid-template-columns:minmax(0,1fr) auto;gap:13px;align-items:start;padding:13px 15px;border:1px solid #e5dbea;border-radius:10px;background:#faf8fd;margin-bottom:15px; }
.ed-project-focus-code { display:block;font-size:11px;font-weight:800;color:#7f51b7;letter-spacing:.3px;margin-bottom:4px; }
.ed-project-focus-name { font-size:17px;color:#293247;line-height:1.35;font-weight:800;margin:0 0 7px;overflow-wrap:break-word; }
.ed-project-focus-meta { display:flex;gap:6px 15px;align-items:center;flex-wrap:wrap;color:#69788b;font-size:12px; }
.ed-project-quick { display:inline-flex;align-items:center;justify-content:center;gap:7px;min-height:36px;padding:8px 12px;border:1px solid #ceb6eb;border-radius:8px;background:#fff;color:#5c288e;font-size:12px;font-weight:750;white-space:nowrap;text-decoration:none!important;transition:background .15s ease; }
.ed-project-quick:hover,.ed-project-quick:focus-visible { background:#f2eafc;color:#4c177e;outline:none; }
.ed-project-tasks-top { display:flex;align-items:center;justify-content:space-between;gap:12px;margin-bottom:10px; }
.ed-project-tasks-top h3 { margin:0;color:#283247;font-size:14px;font-weight:800; }
.ed-project-tasks-scroll { display:flex;flex-direction:column;gap:8px;min-height:0;overflow-y:auto;padding:2px 5px 4px 1px; }
.ed-project-task { display:grid;grid-template-columns:minmax(0,1fr) auto;gap:7px 11px;border:1px solid #e4e9f1;border-radius:8px;padding:11px 13px;background:#fff; }
.ed-project-task[hidden] { display:none!important; }
.ed-project-task-code { display:block;font-size:11px;color:#7b6d94;font-weight:750; }
.ed-project-task-name { display:block;font-size:13px;color:#25334a;font-weight:750;line-height:1.4;margin-top:3px; }
.ed-project-task-when { display:block;font-size:11.5px;color:#718096;margin-top:5px; }
.ed-project-task-status { align-self:start;padding:5px 8px;border:1px solid #dce4ee;background:#f8fafc;color:#64748b;border-radius:6px;white-space:nowrap;font-size:11px;font-weight:750; }
.ed-project-task[data-status="overdue"] .ed-project-task-status { background:#fff1f1;border-color:#f6caca;color:#c0393a; }
.ed-project-task[data-status="done"] .ed-project-task-status { background:#edf9f1;border-color:#ccebd8;color:#178456; }
.ed-project-task-progress { grid-column:1/-1;display:flex;align-items:center;gap:10px;font-size:11px;color:#718096; }
.ed-project-task-progress>span:first-child { flex:1;height:5px;background:#ecedf4;border-radius:20px;overflow:hidden; }
.ed-project-task-progress>span:first-child>i { display:block;height:100%;background:#3b82f6;border-radius:20px; }
.ed-project-task-count { font-size:11.5px;font-weight:700;color:#7b6d94; }
.ed-project-no-tasks { padding:31px 14px;text-align:center;color:#8290a3;border:1px dashed #dce2ec;border-radius:9px; }
.ed-project-no-tasks i { display:block;font-size:23px;margin-bottom:9px;color:#9d86b9; }
.ed-project-list-empty { display:none;padding:15px;font-size:12px;color:#748096;text-align:center;border:1px dashed #dce2ec;border-radius:8px; }
.ed-project-list-empty.is-visible { display:block; }
.ed-dashboard-popup-body { padding:19px 22px 20px; }
.ed-summary-strip { display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:9px;margin-bottom:14px; }
.ed-summary-stat { border:1px solid #e2e7ef;background:#f8fafc;border-radius:8px;padding:11px 13px; }
.ed-summary-stat small { display:block;color:#77859a;font-size:11px;font-weight:700;margin-bottom:5px; }
.ed-summary-stat strong { display:block;font-size:18px;font-weight:800;color:#28324a; }
.ed-popup-results.ed-updated-results { display:grid;grid-template-columns:repeat(2,minmax(0,1fr));align-content:start;gap:12px;max-height:min(57vh,520px);overflow-y:auto;padding:2px; }
.ed-popup-results.ed-updated-results .ed-popup-card { display:flex;flex-direction:column;min-width:0;padding:15px;border-radius:9px; }
.ed-popup-results.ed-updated-results .ed-popup-card[hidden] { display:none!important; }
.ed-updated-results .ed-popup-card-name { font-size:14px;line-height:1.45; }
.ed-updated-results .ed-popup-card-titleline { align-items:center; }
.ed-updated-results .ed-popup-card-icon { border-radius:8px;background:#eff6ff;color:#2563eb; }
.ed-updated-results .ed-popup-meta { margin-top:11px; }
.ed-updated-results .ed-popup-progress { margin-top:auto;position:relative;top:9px; }
.ed-updated-results .ed-popup-progress-caption { margin-top:13px; }
.ed-issue-highlight { margin-top:auto;padding-top:10px;display:flex;gap:8px;flex-wrap:wrap; }
.ed-issue-highlight span { display:inline-flex;align-items:center;gap:5px;font-size:11.5px;color:#67758a;background:#f7f9fc;padding:6px 9px;border:1px solid #e4e8f0;border-radius:6px; }
.ed-calendar-week-detail { display:grid;grid-template-columns:repeat(7,minmax(0,1fr));gap:8px;max-height:63vh;overflow-y:auto;align-items:stretch;padding:1px; }
.ed-calendar-week-day { display:flex;min-width:0;flex-direction:column;gap:8px;padding:11px 9px;border:1px solid #e3e8f0;border-radius:9px;background:#fff;min-height:260px; }
.ed-calendar-week-day.is-today { border-color:#a78bda;background:#f9f6fd; }
.ed-calendar-week-head { display:flex;flex-direction:column;gap:4px;padding-bottom:9px;border-bottom:1px solid #e9eaf0; }
.ed-calendar-week-head b { font-size:12px;color:#313e54; }
.ed-calendar-week-head small { color:#75819a;font-size:11px; }
.ed-calendar-event-line { padding:8px;background:#f6f8fc;border-left:3px solid #a897be;border-radius:6px; }
.ed-calendar-event-line.is-task { background:#eff6ff;border-left-color:#2563eb; }
.ed-calendar-event-line.is-meeting { background:#f4edff;border-left-color:#7c3aed; }
.ed-calendar-event-line strong { display:block;font-size:11.5px;color:#34425a;line-height:1.4; }
.ed-calendar-event-line span { display:block;font-size:10.5px;color:#66778b;margin-top:3px; }
.ed-calendar-empty-day { margin:auto;text-align:center;color:#99a3b2;font-size:11px; }
@media(max-width:920px) { .ed-project-split { grid-template-columns:minmax(250px,.9fr) minmax(0,1.1fr); } .ed-project-detail { padding:14px; } .ed-project-sidebar { padding:14px; } .ed-project-focus { grid-template-columns:1fr; } .ed-calendar-week-detail { grid-template-columns:repeat(4,minmax(0,1fr)); } }
@media(max-width:680px) { .ed-project-split { display:flex;flex-direction:column;max-height:min(80vh,780px); } .ed-project-sidebar { flex:0 0 auto;max-height:235px;border-right:0;border-bottom:1px solid #e7e9f1; } .ed-project-detail { flex:1;min-height:0; } .ed-popup-results.ed-updated-results { grid-template-columns:1fr; } .ed-calendar-week-detail { grid-template-columns:repeat(2,minmax(0,1fr)); } .ed-summary-strip { grid-template-columns:repeat(3,minmax(0,1fr)); } .ed-dashboard-popup-body { padding:13px; } }
@media(max-width:480px) { [id$="mdlProjects"] .modal-dialog,[id$="mdlTasks"] .modal-dialog,[id$="mdlIssues"] .modal-dialog,[id$="mdlCalendar"] .modal-dialog { width:calc(100vw - 16px)!important; } .ed-pop-headline { flex-wrap:wrap; } .ed-project-quick { white-space:normal; } }
.ed-pop-search input { box-sizing:border-box; }
.ed-project-detail { position:relative; }
.ed-project-quick { position:relative; }
.ed-project-quick::after { content:"Đi đến trang Tính giờ công việc của dự án";position:absolute;right:0;bottom:calc(100% + 9px);width:220px;padding:8px 11px;border-radius:7px;background:#352344;color:#fff;font-size:11px;font-weight:600;line-height:1.4;text-align:center;white-space:normal;opacity:0;visibility:hidden;transform:translateY(5px);pointer-events:none;transition:opacity .15s ease,transform .15s ease,visibility .15s ease; }
.ed-project-quick:hover::after,.ed-project-quick:focus-visible::after { opacity:1;visibility:visible;transform:translateY(0); }
.ed-time-updated { grid-template-columns:1fr;max-height:min(52vh,460px); }
.ed-time-updated .ed-time-project { padding:14px;min-height:81px; }
.ed-time-updated .ed-time-project-content strong { font-size:14px; }
.ed-time-go-label { display:inline-flex;align-items:center;gap:7px;padding:8px 10px;border:1px solid #dfccef;border-radius:7px;color:#633296;font-size:12px;font-weight:750;white-space:nowrap; }
.ed-time-search-empty { padding:16px;text-align:center;color:#748096;font-size:12px; }
.ed-time-search-empty[hidden] { display:none!important; }
@media(max-width:520px) { .ed-time-go-label { font-size:0;padding:9px; } .ed-time-go-label i { font-size:13px; } }


/* Phần khung ngoài, header và viền ExtraModal giữ nguyên theo thư viện công ty. */
.ed-project-overview-grid { grid-template-columns:minmax(0,1fr) minmax(0,1.13fr);gap:12px; }
.ed-project-overview-grid>.ed-panel { min-height:0; }
.ed-project-legend { width:100%;text-align:left;cursor:pointer;font-family:inherit;transition:background .16s ease,border-color .16s ease,transform .16s ease; }
.ed-project-legend:hover,.ed-project-legend:focus-visible { background:#f7f2ff;border-color:#d0b5ed;transform:translateX(2px);outline:none; }
.ed-project-legend.is-active[data-ed-status-filter="overdue"] { background:#fff1f1;border-color:#f1b6b6;color:#a12e36; }
.ed-project-legend.is-active[data-ed-status-filter="soon"] { background:#fff7e8;border-color:#f3d3a0;color:#9b5d03; }
.ed-project-legend.is-active[data-ed-status-filter="inprogress"] { background:#ecf3ff;border-color:#bdd3ff;color:#285eaf; }
.ed-project-legend-number { margin-left:auto;font-size:15px;font-weight:800;color:#1e293b; }
.ed-board-head-icon { background:#edf3ff;color:#346cbe; }
.ed-board-status-label { font-size:12px;font-weight:750;color:#9f3340; }
.ed-board-count { white-space:nowrap;border:1px solid #e4e8f0;border-radius:8px;padding:5px 9px;font-size:11.5px;color:#62718a;font-weight:750;background:#fff; }
.ed-board-body { padding:10px 12px!important;min-height:0; }
.ed-project-board-list { display:flex;flex-direction:column;gap:7px;width:100%;height:100%;min-height:0;max-height:270px;overflow-y:auto;overscroll-behavior:contain;padding:2px 5px 2px 1px; }
.ed-board-row { width:100%;display:grid;grid-template-columns:minmax(0,1fr) auto;align-items:center;gap:6px 13px;text-align:left;border:1px solid #e0e6f0;border-left:3px solid #aab5c9;border-radius:8px;padding:9px 11px;background:#fff;color:#26344c;cursor:pointer;font-family:inherit;transition:border-color .16s ease,background .16s ease,transform .16s ease; }
.ed-board-row[hidden] { display:none!important; }
.ed-board-row:hover,.ed-board-row:focus-visible { border-color:#b9a2de;background:#faf7ff;transform:translateX(2px);outline:none; }
.ed-board-row[data-project-status="overdue"] { border-left-color:#ea5558; }
.ed-board-row[data-project-status="soon"] { border-left-color:#f5a623; }
.ed-board-row[data-project-status="inprogress"] { border-left-color:#4386ef; }
.ed-board-row-info { display:flex;flex-direction:column;gap:3px;min-width:0; }
.ed-board-row-code { color:#8350a9;font-size:10.5px;font-weight:800; }
.ed-board-row-name { color:#23324b;font-size:12.5px;font-weight:750;line-height:1.25;overflow-wrap:break-word; }
.ed-board-row-meta { color:#7a8799;font-size:11px; }
.ed-board-row-side { display:flex;align-items:center;gap:13px;color:#6f4da8; }
.ed-board-row-percent { color:#42516c;font-weight:800;font-size:12px; }
.ed-board-row-track { grid-column:1/-1;height:4px;background:#e9edf5;overflow:hidden;border-radius:99px; }
.ed-board-row-track>i { display:block;height:100%;background:#8360b9;border-radius:99px; }
.ed-board-row[data-project-status="overdue"] .ed-board-row-track>i { background:#e76a70; }
.ed-board-row[data-project-status="soon"] .ed-board-row-track>i { background:#eda943; }
.ed-board-row[data-project-status="inprogress"] .ed-board-row-track>i { background:#5895e9; }
.ed-board-empty { display:flex;flex-direction:column;align-items:center;justify-content:center;gap:7px;min-height:180px;text-align:center;color:#78869a;padding:18px; }
.ed-board-empty[hidden] { display:none!important; }
.ed-board-empty>i { color:#5fa781;font-size:24px; }
.ed-board-empty>strong { color:#344259;font-size:13px; }
/* Bố cục cuộn của hai cột bên trong popup Dự án: không sửa modal-header, modal-content hoặc nút X. */
.ed-project-split { height:min(70dvh,680px);max-height:calc(100dvh - 155px);min-height:370px;overflow:hidden;box-sizing:border-box; }
.ed-project-split * { box-sizing:border-box; }
.ed-project-sidebar,.ed-project-detail { height:100%;min-height:0;overflow:hidden; }
.ed-project-list { flex:1 1 0;min-height:0;max-height:none;overflow-y:auto!important;overscroll-behavior:contain;scrollbar-gutter:stable;touch-action:pan-y;padding-right:10px; }
.ed-project-tasks-scroll { flex:1 1 0;min-height:0;max-height:none;overflow-y:auto;scrollbar-gutter:stable; }
.ed-project-picker { border-left:3px solid #bcade3;background:linear-gradient(90deg,#fcfbff,#fff); }
.ed-project-picker.is-selected { border-left-color:#6b34ba;background:#f2eaff; }
.ed-project-task { border-left:3px solid #60a5fa;background:#fff;transition:background .15s ease; }
.ed-project-task:hover { background:#f6faff; }
.ed-project-task[data-status="overdue"] { border-left-color:#e65d63;background:#fffafa; }
.ed-project-task[data-status="done"] { border-left-color:#37a578;background:#f8fffb; }
.ed-project-focus { background:#f5efff;border-color:#d9c7f2; }
.ed-project-tasks-top { padding-bottom:8px;border-bottom:1px solid #ebe7f3; }
.ed-popup-results.ed-updated-results .ed-popup-card { border-top:3px solid #60a5fa;background:#fff; }
.ed-popup-results.ed-updated-results .ed-popup-card[data-ed-status="overdue"] { border-top-color:#e65d63;background:#fffafa; }
.ed-popup-results.ed-updated-results .ed-popup-card[data-ed-status="done"] { border-top-color:#3daf82;background:#f8fffb; }
/* Tooltip click để ghim, click lần hai hoặc click ra ngoài để đóng. */
.ed-dashboard .ed-day-count.is-pinned { border-color:#7e52c2;box-shadow:0 0 0 2px rgba(112,68,186,.14); }
.ed-floating-tip.is-pinned { pointer-events:auto;box-shadow:0 14px 34px rgba(45,29,82,.2); }
.ed-floating-tip .ed-tip-pin-hint { display:flex;align-items:center;gap:6px;margin-top:10px;padding-top:8px;border-top:1px solid #eee9f5;color:#83738f;font-size:10.5px; }
@media(max-width:1100px) { .ed-project-overview-grid { grid-template-columns:1fr; } .ed-project-board-list { max-height:260px; } }
@media(max-width:680px) { .ed-project-split { height:min(80dvh,750px);max-height:calc(100dvh - 126px); } .ed-project-sidebar { flex:0 0 auto;height:240px;max-height:240px;overflow:hidden; } .ed-project-detail { flex:1 1 auto;height:auto;min-height:0; } .ed-project-overview-grid { grid-template-columns:1fr; } }

.ed-project-task-filter-note { display:flex;align-items:center;justify-content:space-between;flex-wrap:wrap;gap:10px;background:#f2ebfc;border:1px solid #d6c2ee;color:#522c8d;border-radius:8px;padding:9px 12px;font-size:12.5px;font-weight:750;margin-bottom:12px; }
.ed-project-task-filter-note[hidden],[data-ed-selected-project-tasks][hidden],.ed-popup-results[hidden] { display:none!important; }
[data-ed-selected-project-tasks] .ed-project-task { min-width:0; }
[data-ed-selected-project-tasks] .ed-project-task[hidden] { display:none!important; }

/* Chinh sua bo cuc popup; khong can thiep modal-header, nut X hay vien ExtraModal. */
[id$="mdlTasks"].modal .modal-dialog,[id$="mdlTasks"] .modal-dialog { width:calc(100vw - 48px)!important;max-width:900px!important; }


[id$="mdlTasks"] .ed-popup-results.ed-updated-results .ed-popup-card { width:100%;min-width:0; }
.ed-project-split .ed-project-sidebar { overflow:hidden; }
.ed-project-split .ed-project-list { overflow-x:hidden!important;overflow-y:auto!important;overscroll-behavior:contain;scrollbar-width:thin;scrollbar-color:#c6b2df transparent;width:100%;max-width:100%;flex:1 1 auto; }
.ed-project-list .ed-project-picker { max-width:100%;box-sizing:border-box; }
.ed-project-task-filter-note { display:flex;align-items:center;justify-content:space-between;gap:10px;flex-wrap:wrap;margin-bottom:14px;padding:11px 12px;border:1px solid #e0e4ef;border-left:4px solid #7544ab;border-radius:8px;background:#fff;color:#27334a;font-size:13px;font-weight:650; }
.ed-filter-project-title { display:inline-flex;align-items:center;gap:9px;min-width:0;flex:1 1 180px;overflow-wrap:anywhere; }
.ed-filter-project-title i { flex:0 0 29px;display:inline-flex;align-items:center;justify-content:center;width:29px;height:29px;border-radius:7px;background:#f1eafa;color:#7036a5; }
.ed-filter-project-clear { flex:0 0 auto;display:inline-flex;align-items:center;justify-content:center;gap:7px;min-height:34px;padding:6px 12px;border:1px solid #d6c0ee;border-radius:7px;background:#faf7ff;color:#5a278b;font-size:12px;font-weight:750;cursor:pointer;transition:background .15s ease,border-color .15s ease; }
.ed-filter-project-clear:hover,.ed-filter-project-clear:focus-visible { background:#ede3fa;border-color:#a783d5;color:#451a73;outline:none; }
/* Bộ lọc theo dự án: chip gọn bên dưới hàng tìm kiếm, không đổi khung ExtraModal. */
.ed-project-task-filter-note { display:inline-flex;align-items:stretch;justify-content:flex-start;flex-wrap:nowrap;gap:0;max-width:100%;width:max-content;min-height:31px;margin:0 0 12px;padding:0;border:1px solid #6c3a9e;border-left:1px solid #6c3a9e;border-radius:6px;background:#fff;color:#44236c;font-size:12px;font-weight:500;vertical-align:top;overflow:hidden; }
.ed-project-task-filter-note[hidden] { display:none!important; }
.ed-filter-project-title { display:inline-flex;align-items:center;gap:4px;flex:0 1 auto;min-width:0;max-width:100%;padding:6px 10px;white-space:nowrap;overflow:hidden;font-size:12px;line-height:1.45;color:#393047; }
.ed-filter-project-title strong { color:#59288e;font-weight:750;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;min-width:0; }
.ed-filter-project-clear { display:inline-flex;flex:0 0 32px;width:32px;height:31px;min-height:0;align-items:center;justify-content:center;margin:0;padding:0;border:0;border-left:1px solid #d5c6e9;border-radius:0;background:#faf7ff;color:#622999;font-size:15px;cursor:pointer;transition:background-color .15s ease,color .15s ease; }
.ed-filter-project-clear:hover,.ed-filter-project-clear:focus-visible { background:#eee4fb;color:#401566;outline:none; }
/* Giữ nguyên kích thước và vị trí dialog khi bắt đầu đóng; chỉ còn fade của Bootstrap. */
[id$="mdlProjects"].ed-popup-is-closing .modal-dialog,[id$="mdlTasks"].ed-popup-is-closing .modal-dialog,[id$="mdlIssues"].ed-popup-is-closing .modal-dialog,[id$="mdlCalendar"].ed-popup-is-closing .modal-dialog { width:var(--ed-popup-close-width)!important;max-width:none!important;transform:none!important;transition:none!important;animation:none!important; }

@media(max-width:680px) { [id$="mdlTasks"].modal .modal-dialog,[id$="mdlTasks"] .modal-dialog, }

/* TrangThai cua TblDuAn; doc lap voi nhom danh gia tre han/sap den han. */
.ed-project-status { display:inline-flex;align-items:center;justify-content:center;gap:5px;max-width:100%;min-height:23px;padding:3px 8px;border:1px solid #dce4ec;border-radius:7px;background:#f1f5f9;color:#475569;font-size:10.5px;font-weight:800;line-height:1.3;white-space:nowrap; }
.ed-project-status[hidden] { display:none!important; }
.ed-project-status:before { content:'';width:6px;height:6px;flex:none;border-radius:50%;background:currentColor; }
.ed-project-status.is-not-started { color:#935b0f;background:#fff7e6;border-color:#f3dba2; }
.ed-project-status.is-in-progress { color:#2563a6;background:#eff6ff;border-color:#bfdbfe; }
.ed-project-status.is-paused { color:#6b7280;background:#f1f5f9;border-color:#cbd5e1; }
.ed-project-status.is-completed { color:#147d4e;background:#eafaf0;border-color:#b6e8ca; }
.ed-project-status.is-ended { color:#7c5362;background:#f8edf1;border-color:#ebced9; }
.ed-board-row-code-line { display:flex;flex-wrap:wrap;align-items:center;gap:6px; }
.ed-board-row-code-line .ed-project-status { font-size:10px;min-height:20px;padding:2px 6px; }
.ed-project-picker-head .ed-project-status { font-size:10px;padding:2px 7px; }
.ed-project-focus-heading { display:flex;align-items:center;flex-wrap:wrap;gap:8px;margin-bottom:6px; }
.ed-project-focus-heading .ed-project-focus-code { margin-bottom:0; }
.ed-time-group-title { display:flex;align-items:center;gap:9px;min-width:0;padding:10px 12px 8px;margin-top:4px;border:1px solid #eee9f5;border-radius:9px;background:#f9f7fd;color:#4b366c;font-size:12px;font-weight:800; }
.ed-time-group-title:first-child { margin-top:0; }
.ed-time-group-title:after { content:'';height:1px;background:#e5dcee;flex:1;min-width:10px; }
.ed-time-group-title[hidden] { display:none!important; }
.ed-time-group-count { order:2;color:#7c6b91;font-size:10.5px;font-weight:700;white-space:nowrap; }
.ed-time-project-heading { display:flex;align-items:center;flex-wrap:wrap;gap:7px;min-width:0; }
.ed-time-project-heading small { flex:1 1 auto; }
.ed-time-project-heading .ed-project-status { font-size:10px;min-height:21px;padding:2px 7px; }
.ed-time-project[hidden] { display:none!important; }
@media(max-width:640px) { .ed-time-group-title { padding:9px; } .ed-time-project { flex-wrap:wrap; } .ed-time-go-label { margin-left:auto; } .ed-project-focus-heading { gap:6px; } }

/* V26: màu đúng ngữ nghĩa công việc. Chỉ tác động các popup Dashboard nhân viên. */
.ed-project-task[data-status="soon"] { border-left-color:#f59e0b;background:#fffdf7; }
.ed-project-task[data-status="soon"] .ed-project-task-status { color:#9a6700;background:#fffbeb;border-color:#fde68a; }
.ed-popup-results.ed-updated-results .ed-popup-card[data-ed-status="soon"] { border-top-color:#f59e0b;background:#fffdf7; }
/* V28: mau canh bao sap han chi nam o nhan han, khong lam doi mau nhan trang thai. */
/* Bỏ hiệu ứng thay đổi chiều rộng khi lọc; một mục thì trải rộng trong cùng một modal. */
[id$="mdlTasks"].modal .modal-dialog,[id$="mdlTasks"] .modal-dialog { width:calc(100vw - 48px)!important;max-width:900px!important; }
[id$="mdlTasks"] .ed-popup-results.ed-updated-results.ed-one-result { grid-template-columns:minmax(0,1fr); }
@media(max-width:680px) { [id$="mdlTasks"].modal .modal-dialog,[id$="mdlTasks"] .modal-dialog { width:calc(100vw - 16px)!important; } }
.ed-time-entry { cursor:pointer; }
.ed-dashboard .ed-days .ed-day { cursor:pointer;transition:border-color .15s ease,box-shadow .15s ease; }
.ed-dashboard .ed-days .ed-day:focus-visible,.ed-dashboard .ed-days .ed-day:hover { border-color:#a7bde2;outline:2px solid rgba(59,130,246,.15);outline-offset:1px; }

/* V28 - nhan trang thai doc lap canh bao thoi han: khong suy trang thai tu mau viền. */
[id$="mdlTasks"] .ed-task-labels { display:flex;flex:0 0 auto;flex-direction:column;align-items:flex-end;gap:6px;max-width:44%; }
[id$="mdlTasks"] .ed-popup-badge.ed-task-state { display:inline-flex;align-items:center;justify-content:center;min-height:26px;padding:5px 9px;border:1px solid #d7e1ec;border-radius:7px;background:#f8fafc;color:#475569;font-size:11.5px;font-weight:750;line-height:1.2;white-space:nowrap; }
[id$="mdlTasks"] .ed-popup-badge.ed-task-state.is-in-progress { border-color:#bfdbfe;background:#eff6ff;color:#1d4ed8; }
[id$="mdlTasks"] .ed-popup-badge.ed-task-state.is-review { border-color:#e4d4f5;background:#f7f0ff;color:#63309d; }
[id$="mdlTasks"] .ed-popup-badge.ed-task-state.is-completed { border-color:#bbf7d0;background:#f0fdf4;color:#15803d; }
[id$="mdlTasks"] .ed-task-deadline { display:inline-flex;align-items:center;justify-content:center;padding:4px 8px;border:1px solid;border-radius:6px;font-size:10.5px;font-weight:800;white-space:nowrap;line-height:1.2; }
[id$="mdlTasks"] .ed-task-deadline.is-overdue { border-color:#fecaca;background:#fff1f2;color:#b91c1c; }
[id$="mdlTasks"] .ed-task-deadline.is-soon { border-color:#fde68a;background:#fffbeb;color:#a16207; }
@media(max-width:520px) { [id$="mdlTasks"] .ed-task-labels { max-width:100%; } }

/* V29: Title code and name on a single flowing line; status retains its business color. */
.ed-task-inline-title { display:block;min-width:0;overflow-wrap:anywhere;line-height:1.45;color:#243247;font-size:13.5px; }
.ed-task-inline-code { color:#475569;font-size:13px;font-weight:800;white-space:nowrap;margin-right:3px; }
.ed-project-task-name { display:inline!important;font-size:13.5px!important;margin:0!important;line-height:inherit!important;color:#25334a; }
.ed-project-task-main { min-width:0; }
.ed-project-task-labels { display:flex;flex-direction:column;align-items:flex-end;gap:5px;min-width:0; }
.ed-project-task-labels .ed-project-task-status { align-self:flex-end; }
.ed-project-task-status.is-not-started { background:#f1f5f9!important;border-color:#dbe3ed!important;color:#52647d!important; }
.ed-project-task-status.is-in-progress { background:#eff6ff!important;border-color:#bfdbfe!important;color:#1d4ed8!important; }
.ed-project-task-status.is-completed { background:#edf9f1!important;border-color:#bbe5cd!important;color:#178456!important; }
.ed-project-task-status.is-review { background:#fff8e8!important;border-color:#f0d79c!important;color:#96630d!important; }
.ed-project-task-deadline { display:inline-flex;align-items:center;justify-content:center;padding:3px 8px;border:1px solid #fcd4a2;border-radius:6px;background:#fff8e7;color:#946200;font-size:10px;font-weight:750;white-space:nowrap; }
.ed-project-task-deadline.is-overdue { border-color:#fecaca;background:#fff1f2;color:#b91c1c; }
.ed-project-task-deadline.is-soon { border-color:#fde68a;background:#fffbeb;color:#a16207; }
.ed-project-task[data-status="overdue"] .ed-project-task-labels .ed-project-task-status,.ed-project-task[data-status="soon"] .ed-project-task-labels .ed-project-task-status { box-shadow:none; }
.ed-popup-card-titleline .ed-task-inline-title { flex:1 1 auto;min-width:0;font-size:14px; }
.ed-popup-card-titleline .ed-popup-card-name { display:inline!important;line-height:inherit!important; }
@media(max-width:640px) { .ed-project-task { grid-template-columns:minmax(0,1fr); } .ed-project-task-labels { flex-direction:row;align-items:center;justify-content:flex-start;flex-wrap:wrap; } }

/* V33 | Thanh tien do TASK dong bo mau voi DuAnDetail - khong doi tien do DU AN. */
.ed-task-progress-color { --ed-task-bar:#94a3b8;cursor:help; }
.ed-task-progress-color.ed-task-bar-0 { --ed-task-bar:#94a3b8; }
.ed-task-progress-color.ed-task-bar-1 { --ed-task-bar:#0ea5e9; }
.ed-task-progress-color.ed-task-bar-2 { --ed-task-bar:#22c55e; }
.ed-task-progress-color.ed-task-bar-3 { --ed-task-bar:#ef4444; }
.ed-task-progress-color.ed-task-bar-4 { --ed-task-bar:#f59e0b; }
.ed-task-progress-color.ed-task-bar-soon { --ed-task-bar:#f59e0b; }
.ed-task-progress-color.ed-task-bar-late { --ed-task-bar:#ef4444; }
.ed-project-task-progress.ed-task-progress-color > span:first-child > i,.ed-popup-progress.ed-task-progress-color > span { background:var(--ed-task-bar)!important; }
.ed-task-progress-color:focus-visible { outline:2px solid #8e67ca;outline-offset:4px;border-radius:5px; }

/* V34 - Bộ lọc dự án trong popup Công việc trong tuần, không thay đổi ExtraModal. */
[id$="mdlTasks"] .ed-pop-filter { flex-wrap:wrap;gap:8px; }
[id$="mdlTasks"] .ed-pop-filter .ed-pop-search { flex:1 1 240px;min-width:190px; }
[id$="mdlTasks"] .ed-task-project-select-wrap { position:relative;display:flex;align-items:center;flex:0 1 235px;min-width:175px;height:41px;margin:0;border:1px solid #d9dfeb;border-radius:8px;background:#fff;color:#5b21b6;overflow:hidden; }
[id$="mdlTasks"] .ed-task-project-select-wrap>i:first-child { position:absolute;left:11px;top:50%;transform:translateY(-50%);font-size:12px;pointer-events:none; }
[id$="mdlTasks"] .ed-task-project-select-wrap select { appearance:none;display:block;width:100%;height:100%;padding:7px 29px 7px 32px;border:0;outline:0;background:transparent;color:#334155;font-size:12.5px;font-weight:650;cursor:pointer;text-overflow:ellipsis; }
[id$="mdlTasks"] .ed-task-project-select-wrap:focus-within { border-color:#9473c8;box-shadow:0 0 0 3px rgba(106,63,155,.09); }
[id$="mdlTasks"] .ed-task-project-chevron { position:absolute;right:11px;top:50%;transform:translateY(-50%);font-size:10px;color:#7c6994;pointer-events:none; }
@media(max-width:680px) { [id$="mdlTasks"] .ed-pop-filter .ed-pop-search { flex-basis:100%; } [id$="mdlTasks"] .ed-task-project-select-wrap { flex:1 1 100%; } }

/* Chuyển tuần AJAX: không điều hướng trình duyệt, giữ nguyên các vùng khác. */
.ed-dashboard.ed-week-is-loading .ed-week-calendar-panel .ed-panel-body { opacity:.52;transition:opacity .18s ease; }
.ed-dashboard.ed-week-is-loading .ed-header-time .ed-week-actions { opacity:.65; }
/* Khong cho ngay thang xuong dong va khong cho loading chiem them chieu ngang. */
.ed-header-time .ed-week-title { white-space:nowrap;flex-wrap:nowrap; }
.ed-header-time .ed-week-title [id$="lblWeekRange"] { white-space:nowrap; }
.ed-dashboard.ed-week-is-loading .ed-header-time .ed-week-label i { animation:ed-week-label-pulse .85s ease-in-out infinite alternate; }
@keyframes ed-week-label-pulse { from { opacity:1; } to { opacity:.35; } }
.ed-week-ajax-error { position:absolute;right:8px;top:calc(100% + 6px);z-index:20;max-width:350px;padding:9px 11px;border:1px solid #fecaca;border-radius:8px;background:#fff7f7;color:#991b1b;box-shadow:0 5px 16px rgba(15,23,42,.11);font-size:12px;line-height:1.45;text-align:left; }
@keyframes ed-week-spin { to { transform:rotate(360deg); } }
@media(prefers-reduced-motion:reduce) { .ed-dashboard.ed-week-is-loading .ed-header-time .ed-week-label i { animation:none; } }
</style>
<script type="text/javascript">
    /* EmployeeDashboard: bắt click từ capture phase trước cơ chế điều hướng/loading của MasterTemplate. */
    (function () {
        var requestSerial = 0;
        var activeController = null;
        var displayedUrl = window.location.href;
        var parts = [
            '#edWeekCalendar .ed-panel-body',
            '[id$="btnDashboardTasks"] .ed-stat-val',
            '[id$="btnDashboardOverdue"] .ed-stat-val',
            '.ed-charts-grid',
            '[data-ed-modal-kind="tasks"]',
            '[data-ed-modal-kind="calendar"]'
        ];

        function root() { return document.getElementById('edEmployeeDashboard'); }
        function showWeekError(message) {
            var container = root() && root().querySelector('.ed-header-time');
            if (!container) return;
            var error = container.querySelector('.ed-week-ajax-error');
            if (!error) { error = document.createElement('div'); error.className = 'ed-week-ajax-error'; error.setAttribute('role', 'alert'); container.appendChild(error); }
            error.textContent = message;
        }
        function clearWeekError() {
            var error = root() && root().querySelector('.ed-week-ajax-error');
            if (error && error.parentNode) error.parentNode.removeChild(error);
        }
        function loading(isLoading) {
            var container = root();
            if (!container) return;
            container.classList.toggle('ed-week-is-loading', isLoading);
            var controls = container.querySelector('.ed-week-actions');
            if (controls) controls.setAttribute('aria-busy', isLoading ? 'true' : 'false');
        }
        function syncWebFormsState(parsed, finalUrl) {
            var localForm = document.forms[0], remoteForm = parsed.forms[0];
            if (!localForm || !remoteForm) throw new Error('Không tìm thấy form WebForms trong phản hồi.');
            var stateField = function (name) { return /^__(?:VIEWSTATE(?:\d+|GENERATOR|FIELDCOUNT|ENCRYPTED)?|EVENTVALIDATION|EVENTTARGET|EVENTARGUMENT|LASTFOCUS)$/.test(name); };
            var oldFields = Array.prototype.filter.call(localForm.querySelectorAll('input[type="hidden"][name]'), function (el) { return stateField(el.name); });
            var newFields = Array.prototype.filter.call(remoteForm.querySelectorAll('input[type="hidden"][name]'), function (el) { return stateField(el.name); });
            var fragment = document.createDocumentFragment();
            newFields.forEach(function (el) { fragment.appendChild(document.importNode(el, true)); });
            oldFields.forEach(function (el) { if (el.parentNode) el.parentNode.removeChild(el); });
            localForm.insertBefore(fragment, localForm.firstChild);
            var newAction = remoteForm.getAttribute('action');
            localForm.action = new URL(newAction || finalUrl, finalUrl).href;
        }
        function changeWeek(href, addToHistory) {
            var url;
            try { url = new URL(href, window.location.href); }
            catch (error) { showWeekError('Đường dẫn chuyển tuần không hợp lệ.'); return; }
            if (url.origin !== window.location.origin) { showWeekError('Không thể tải dữ liệu từ địa chỉ khác.'); return; }
            if (!window.fetch || !window.DOMParser || !window.history || !window.history.pushState) {
                showWeekError('Trình duyệt chưa hỗ trợ cập nhật tuần không tải lại trang.');
                return;
            }
            var serial = ++requestSerial;
            if (activeController) activeController.abort();
            activeController = window.AbortController ? new AbortController() : null;
            clearWeekError();
            loading(true);
            fetch(url.href, { method: 'GET', credentials: 'same-origin', cache: 'no-store', redirect: 'follow', signal: activeController ? activeController.signal : undefined })
                .then(function (response) {
                    if (!response.ok) throw new Error('HTTP ' + response.status);
                    return response.text().then(function (html) { return { html: html, finalUrl: response.url || url.href }; });
                })
                .then(function (result) {
                    if (serial !== requestSerial) return;
                    var parsed = new DOMParser().parseFromString(result.html, 'text/html');
                    if (!parsed.getElementById('edEmployeeDashboard') || !parsed.forms.length) throw new Error('Phản hồi không chứa Dashboard (có thể đã hết phiên đăng nhập).');
                    var pairs = parts.map(function (selector) { return { current: document.querySelector(selector), incoming: parsed.querySelector(selector) }; });
                    var missing = parts.filter(function (selector, index) { return !pairs[index].current || !pairs[index].incoming; });
                    if (missing.length) throw new Error('Thiếu vùng dữ liệu: ' + missing.join(', '));
                    // Giữ nguyên DOM của thanh chuyển tuần để không mất kích thước/focus khi cập nhật.
                    var currentHeader = root().querySelector('.ed-header-time');
                    var incomingRoot = parsed.getElementById('edEmployeeDashboard');
                    var incomingHeader = incomingRoot && incomingRoot.querySelector('.ed-header-time');
                    var currentRange = currentHeader && currentHeader.querySelector('[id$="lblWeekRange"]');
                    var incomingRange = incomingHeader && incomingHeader.querySelector('[id$="lblWeekRange"]');
                    var currentLinks = currentHeader && currentHeader.querySelectorAll('.ed-week-actions a.ed-week-btn');
                    var incomingLinks = incomingHeader && incomingHeader.querySelectorAll('.ed-week-actions a.ed-week-btn');
                    if (!currentRange || !incomingRange || !currentLinks || !incomingLinks || currentLinks.length !== 3 || incomingLinks.length !== 3)
                        throw new Error('Thiếu nhãn tuần hoặc các nút điều hướng.');
                    var hrefs = Array.prototype.map.call(incomingLinks, function (link) { return link.getAttribute('href'); });
                    if (hrefs.some(function (href) { return !href || /^javascript:/i.test(href); }))
                        throw new Error('Phản hồi có đường dẫn chuyển tuần không hợp lệ.');
                    // Cập nhật đồng bộ trong một lượt chạy JS, không thay cả header gây nhấp nháy.
                    if (typeof window.edResetDashboardTips === 'function') window.edResetDashboardTips();
                    syncWebFormsState(parsed, result.finalUrl);
                    currentRange.textContent = incomingRange.textContent;
                    Array.prototype.forEach.call(currentLinks, function (link, index) { link.setAttribute('href', hrefs[index]); });
                    pairs.forEach(function (pair) { pair.current.replaceWith(document.importNode(pair.incoming, true)); });
                    if ('edActiveProjectFilter' in window) window.edActiveProjectFilter = null;
                    if ('edTaskProjectId' in window) window.edTaskProjectId = '';
                    try { if (typeof window.edInitDashboardPopups === 'function') window.edInitDashboardPopups(); }
                    catch (initError) { console.warn('Không khởi tạo lại được bộ lọc popup:', initError); }
                    if (addToHistory) window.history.pushState({ edEmployeeDashboardWeek: true }, '', url.href);
                    displayedUrl = url.href;
                })
                .catch(function (error) {
                    if (serial !== requestSerial || (error && error.name === 'AbortError')) return;
                    console.error('[EmployeeDashboard] AJAX chuyển tuần thất bại:', error);
                    // Không dùng location.assign: thất bại AJAX tuyệt đối không được reload cả trang.
                    showWeekError('Chưa thể cập nhật tuần. Vui lòng thử lại. Chi tiết lỗi có trong Console (F12).');
                })
                .then(function () {
                    if (serial !== requestSerial) return;
                    activeController = null;
                    loading(false);
                });
        }

        function intercept(anchor, event) {
            if (!anchor) return false;
            if (event) {
                if (event.button !== undefined && event.button !== 0) return true;
                if (event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return true;
                event.preventDefault();
                event.stopPropagation();
                if (event.stopImmediatePropagation) event.stopImmediatePropagation();
            }
            var href = anchor.getAttribute('href');
            if (!href || href === '#' || /^javascript:/i.test(href)) { showWeekError('Nút chuyển tuần chưa được gán đường dẫn.'); return false; }
            changeWeek(href, true);
            return false;
        }

        window.edNavigateDashboardWeek = intercept;
        // Capture tại WINDOW để master không khởi chạy animation điều hướng trang.
        window.addEventListener('click', function (event) {
            var anchor = event.target && event.target.closest ? event.target.closest('#edEmployeeDashboard .ed-week-actions a.ed-week-btn') : null;
            if (!anchor) return;
            intercept(anchor, event);
        }, true);
        window.addEventListener('popstate', function () {
            if (displayedUrl !== window.location.href) changeWeek(window.location.href, false);
        });
    })();
    // Chi trang EmployeeDashboard moi tu dong mo lich. Phim Q va nut mo thu cong van doc lap.
    (function () {
        function autoOpenToday() {
            var dashboard = document.getElementById('edEmployeeDashboard');
            if (!dashboard || dashboard.getAttribute('data-ed-auto-today-done') === '1') return;
            dashboard.setAttribute('data-ed-auto-today-done', '1');
            if (window.TodayScheduleUI && typeof window.TodayScheduleUI.autoOpenEmployee === 'function')
                window.TodayScheduleUI.autoOpenEmployee();
        }
        if (document.readyState === 'complete') window.setTimeout(autoOpenToday, 80);
        else window.addEventListener('load', function () { window.setTimeout(autoOpenToday, 80); }, { once: true });
    })();
</script>

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
        <asp:LinkButton ID="btnDashboardTasks" runat="server" CssClass="ed-header-stat" style="--c:#3b82f6;--soft:#eef6ff" CausesValidation="false" OnClientClick="edClearDashboardProjectFilter();return edOpenDashboardModal('mdlTasks');" OnClick="lnkMoreTasks_Click" ToolTip="Xem công việc trong tuần" aria-label="Xem công việc trong tuần">
            <div class="ed-stat-val"><asp:Label ID="lblTaskCount" runat="server" /></div><div class="ed-stat-lbl">Công việc<br/>trong tuần</div><i class="fas fa-tasks ed-stat-ic"></i>
        </asp:LinkButton>
        <asp:LinkButton ID="btnDashboardOverdue" runat="server" CssClass="ed-header-stat" style="--c:#ef4444;--soft:#fff1f0" CausesValidation="false" OnClientClick="edClearDashboardProjectFilter();edSetTaskScope('overdue');return edOpenDashboardModal('mdlTasks');" OnClick="lnkMoreTasks_Click" ToolTip="Xem các công việc quá hạn" aria-label="Xem công việc quá hạn">
            <div class="ed-stat-val"><asp:Label ID="lblOverdueCount" runat="server" /></div><div class="ed-stat-lbl">Việc<br/>quá hạn</div><i class="fas fa-exclamation-triangle ed-stat-ic"></i>
        </asp:LinkButton>
        <asp:LinkButton ID="btnDashboardIssues" runat="server" CssClass="ed-header-stat" style="--c:#10b981;--soft:#edfbf5" CausesValidation="false" OnClientClick="return edOpenDashboardModal('mdlIssues');" OnClick="lnkMoreIssues_Click" ToolTip="Xem các vấn đề được giao" aria-label="Xem vấn đề được giao">
            <div class="ed-stat-val"><asp:Label ID="lblIssueCount" runat="server" /></div><div class="ed-stat-lbl">Vấn đề<br/>được giao</div><i class="fas fa-life-ring ed-stat-ic"></i>
        </asp:LinkButton>
        <div class="ed-header-tools">
            <button type="button" class="ed-time-entry" onclick="return edOpenSharedDaySchedule('');" aria-label="Xem lịch trình hôm nay"><span class="ed-time-entry-icon"><i class="far fa-calendar-check" aria-hidden="true"></i></span><span><strong>Lịch trình hôm nay</strong><small>Công việc và cuộc họp</small></span><i class="fas fa-angle-right ed-time-entry-arrow" aria-hidden="true"></i></button>
            <div class="ed-header-time"><div class="ed-week-label"><i class="far fa-calendar-alt"></i> Tổng quan theo tuần</div><div class="ed-week-title"><asp:Label ID="lblWeekRange" runat="server" /></div><div class="ed-week-actions"><asp:HyperLink ID="lnkPreviousWeek" runat="server" CssClass="ed-week-btn" Text="‹ Trước" onclick="return edNavigateDashboardWeek(this, event);" /><asp:HyperLink ID="lnkCurrentWeek" runat="server" CssClass="ed-week-btn ed-current" Text="Hiện tại" onclick="return edNavigateDashboardWeek(this, event);" /><asp:HyperLink ID="lnkNextWeek" runat="server" CssClass="ed-week-btn" Text="Sau ›" onclick="return edNavigateDashboardWeek(this, event);" /></div></div>
        </div>
    </div>
    <section id="edWeekCalendar" class="ed-panel ed-calendar-panel ed-week-calendar-panel is-open" aria-labelledby="edCalendarHeading" aria-hidden="false">
        <div class="ed-panel-head"><div class="ed-panel-heading"><span class="ed-panel-icon"><i class="far fa-calendar-alt"></i></span><div><h2 id="edCalendarHeading" class="ed-panel-title">Lịch làm việc trong tuần</h2><div class="ed-panel-subtitle">Tổng hợp công việc, lịch họp và ngày nghỉ trong tuần</div></div></div><asp:LinkButton ID="lnkCalendarMore" runat="server" CssClass="ed-week-btn" OnClientClick="return edOpenDashboardModal('mdlCalendar');" OnClick="lnkCalendar_Click" CausesValidation="false"><i class="fas fa-expand-alt"></i> Xem lịch chi tiết</asp:LinkButton></div>
        <div class="ed-panel-body"><asp:Repeater ID="rptDays" runat="server"><HeaderTemplate><div class="ed-days"></HeaderTemplate><ItemTemplate>
            <div class='ed-day <%#: Eval("CssClass") %>' data-ed-schedule-date='<%# ((CalendarDayItem)Container.DataItem).Date.ToString("yyyy-MM-dd") %>' tabindex="0" role="button" aria-label='Xem lịch trình <%# ((CalendarDayItem)Container.DataItem).Date.ToString("dd/MM/yyyy") %>'><div class="ed-day-head"><span class="ed-day-name"><%#: Eval("DayName") %></span><span class="ed-day-date"><%#: Eval("DayNumber") %></span><asp:PlaceHolder ID="phToday" runat="server" Visible='<%# (bool)Eval("IsToday") %>'><span class="ed-today-tag">Hôm nay</span></asp:PlaceHolder></div><div class="ed-day-summary">
                <div class="ed-day-count is-task" tabindex="0" data-ed-tip-trigger="task" aria-label="Xem công việc của ngày này"><span><i class="fas fa-tasks" aria-hidden="true"></i> Công việc</span><strong class="ed-day-value"><%#: Eval("TaskCount") %></strong><div class="ed-tip-source" hidden><div class="ed-tip-heading"><i class="fas fa-tasks"></i> Công việc · <%#: Eval("DayNumber") %></div><asp:PlaceHolder runat="server" Visible='<%# ((CalendarDayItem)Container.DataItem).TaskCount == 0 %>'><div class="ed-tip-empty">Không có công việc!</div></asp:PlaceHolder><asp:Repeater ID="rptTaskTip" runat="server" DataSource='<%# ((CalendarDayItem)Container.DataItem).TaskEvents %>'><ItemTemplate><div class="ed-tip-item"><strong><%#: Eval("Title") %></strong><span><%#: Eval("Subtitle") %></span><small><%#: Eval("TimeLabel") %></small></div></ItemTemplate></asp:Repeater></div></div>
                <div class="ed-day-count is-meeting" tabindex="0" data-ed-tip-trigger="meeting" aria-label="Xem cuộc họp của ngày này"><span><i class="far fa-calendar-alt" aria-hidden="true"></i> Cuộc họp</span><strong class="ed-day-value"><%#: Eval("MeetingCount") %></strong><div class="ed-tip-source" hidden><div class="ed-tip-heading"><i class="far fa-calendar-alt"></i> Cuộc họp · <%#: Eval("DayNumber") %></div><asp:PlaceHolder runat="server" Visible='<%# ((CalendarDayItem)Container.DataItem).MeetingCount == 0 %>'><div class="ed-tip-empty">Không có cuộc họp</div></asp:PlaceHolder><asp:Repeater ID="rptMeetTip" runat="server" DataSource='<%# ((CalendarDayItem)Container.DataItem).MeetingEvents %>'><ItemTemplate><div class="ed-tip-item"><strong><%#: Eval("Title") %></strong><span><%#: Eval("Subtitle") %></span><small><%#: Eval("TimeLabel") %></small></div></ItemTemplate></asp:Repeater></div></div>
            </div></div>
        </ItemTemplate><FooterTemplate></div></FooterTemplate></asp:Repeater></div>
    </section>
    <div class="ed-charts-grid ed-project-overview-grid">
        <section class="ed-panel ed-chart-project-panel" aria-labelledby="edProjectChartHeading">
            <div class="ed-panel-head"><div class="ed-panel-heading"><span class="ed-panel-icon" style="background:#f5eafa;color:#a443c4"><i class="fas fa-chart-pie"></i></span><div><h2 id="edProjectChartHeading" class="ed-panel-title">Tình hình thời hạn dự án</h2><div class="ed-panel-subtitle">Phân loại theo thời hạn; trạng thái dự án hiển thị trên từng thẻ</div></div></div></div>
            <div class="ed-panel-body"><div class="ed-chart-container"><div class="ed-donut-wrapper ed-project-donut" id="divProjectChart" runat="server"><div class="ed-donut-center"><asp:Label ID="lblTotalProjectsChart" runat="server" CssClass="ed-donut-number" /><span class="ed-donut-label">dự án</span></div></div>
            <div class="ed-chart-legend" role="group" aria-label="Lọc dự án theo thời hạn">
                <button type="button" class="ed-legend-item ed-project-legend is-active" data-ed-status-filter="overdue" aria-pressed="true" onclick="return edFilterProjectDashboard('overdue');"><span class="ed-dot" style="background:#ef4444"></span><span>Trễ hạn</span><asp:Label ID="lblProjectOverdueChart" runat="server" CssClass="ed-project-legend-number" /></button>
                <button type="button" class="ed-legend-item ed-project-legend" data-ed-status-filter="soon" aria-pressed="false" onclick="return edFilterProjectDashboard('soon');"><span class="ed-dot" style="background:#f59e0b"></span><span>Sắp đến hạn</span><asp:Label ID="lblProjectDueSoonChart" runat="server" CssClass="ed-project-legend-number" /></button>
                <button type="button" class="ed-legend-item ed-project-legend" data-ed-status-filter="inprogress" aria-pressed="false" onclick="return edFilterProjectDashboard('inprogress');"><span class="ed-dot" style="background:#3b82f6"></span><span>Còn lại</span><asp:Label ID="lblProjectNormalChart" runat="server" CssClass="ed-project-legend-number" /></button>
            </div></div></div>
        </section>
        <section class="ed-panel ed-project-board-panel" aria-labelledby="edProjectBoardHeading">
            <div class="ed-panel-head"><div class="ed-panel-heading"><span class="ed-panel-icon ed-board-head-icon"><i class="fas fa-folder-open"></i></span><div><h2 id="edProjectBoardHeading" class="ed-panel-title">Danh sách dự án <span class="ed-board-status-label" data-ed-board-label>trễ hạn</span></h2><div class="ed-panel-subtitle">Nhấn dự án để xem các công việc được giao</div></div></div><span class="ed-board-count" data-ed-board-count>0 dự án</span></div>
            <div class="ed-panel-body ed-board-body"><div class="ed-project-board-list" data-ed-project-board aria-live="polite">
                <asp:Repeater ID="rptProjectStatusRows" runat="server"><ItemTemplate>
                    <button type="button" class="ed-board-row" data-ed-board-project data-project-status='<%# (bool)Eval("IsOverdue") ? "overdue" : ((bool)Eval("IsDueSoon") ? "soon" : "inprogress") %>' data-project-id='<%# Eval("Id") %>' data-project-name='<%# System.Web.HttpUtility.HtmlAttributeEncode(Convert.ToString(Eval("Name"))) %>' onclick="return edOpenProjectTasksFromBoard(this);" hidden>
                        <span class="ed-board-row-info"><span class="ed-board-row-code-line"><span class="ed-board-row-code"><i class="fas fa-folder-open"></i> <%#: Eval("Code") %></span><span class='<%# "ed-project-status " + Eval("StatusCss") %>'><%#: Eval("StatusText") %></span></span><strong class="ed-board-row-name"><%#: Eval("Name") %></strong><span class="ed-board-row-meta"><i class="far fa-calendar-alt"></i> <%#: Eval("Deadline") %></span></span>
                        <span class="ed-board-row-side"><span class="ed-board-row-percent"><%#: Eval("Progress") %>%</span><i class="fas fa-chevron-right" aria-hidden="true"></i></span>
                        <span class="ed-board-row-track"><i style='width:<%#: Eval("Progress") %>%'></i></span>
                    </button>
                </ItemTemplate></asp:Repeater>
                <div class="ed-board-empty" data-ed-board-empty hidden><i class="far fa-check-circle"></i><strong>Không có dự án thuộc trạng thái này</strong><span>Chọn trạng thái khác để tiếp tục xem.</span></div>
            </div></div>
        </section>
    </div>
</div>

<div class="ed-modal-host">

<SweetSoft:ExtraModal ID="mdlProjects" runat="server" Type="Primary" Title="Các dự án đang tham gia" Width="90%">
    <ContentTemplate>
        <asp:UpdatePanel ID="upnlModalProjects" runat="server" UpdateMode="Conditional"><ContentTemplate>
            <div class="ed-project-split ed-popup-inner" data-ed-modal-kind="projects" style="padding:0">
                <aside class="ed-project-sidebar" aria-label="Danh sách dự án">
                    <div class="ed-pop-headline"><div><h2>Dự án tham gia</h2><p>Chọn dự án để xem công việc bên cạnh</p></div><span class="ed-pop-total" data-ed-count>0 mục</span></div>
                    <div class="ed-pop-filter"><label class="ed-pop-search"><i class="fas fa-search" aria-hidden="true"></i><input type="search" autocomplete="off" placeholder="Tìm dự án..." aria-label="Tìm dự án" oninput="edFilterDashboardModal(this)" /></label></div>
                    <div class="ed-project-list" role="listbox" aria-label="Chọn dự án" data-ed-project-list>
                        <asp:Repeater ID="rptProjectsAll" runat="server"><ItemTemplate>
                            <button type="button" class="ed-project-picker" role="option" aria-selected="false" data-ed-item data-ed-project-choice data-project-id='<%# Eval("Id") %>' data-project-code='<%# Server.HtmlEncode(Convert.ToString(Eval("Code"))) %>' data-project-name='<%# Server.HtmlEncode(Convert.ToString(Eval("Name"))) %>'  data-project-summary='<%# Server.HtmlEncode(Convert.ToString(Eval("AssignedSummary"))) %>' data-project-status-text='<%# Server.HtmlEncode(Convert.ToString(Eval("StatusText"))) %>' data-project-status-css='<%# Server.HtmlEncode(Convert.ToString(Eval("StatusCss"))) %>' data-ed-search='<%# Server.HtmlEncode(Convert.ToString(Eval("Code")) + " " + Convert.ToString(Eval("Name"))) %>' onclick="return edSelectDashboardProject(this)">
                                <span class="ed-project-picker-head"><span><i class="far fa-folder-open"></i> <%#: Eval("Code") %></span><span class='<%# "ed-project-status " + Eval("StatusCss") %>'><%#: Eval("StatusText") %></span></span>
                                <strong class="ed-project-picker-name"><%#: Eval("Name") %></strong>
                                <span class="ed-project-picker-meta"><span><i class="fas fa-list-check"></i> <%#: Eval("AssignedSummary") %></span></span>
                                <span class="ed-project-mini-progress"><span class="ed-project-mini-track"><span style='width:<%# Eval("AssignedProgress") %>%'></span></span><b><%#: Eval("AssignedPercentText") %></b></span>
                            </button>
                        </ItemTemplate></asp:Repeater>
                        <asp:Panel ID="pnlNoProjects" runat="server" CssClass="ed-popup-empty" Visible="false">Chưa có dự án để theo dõi.</asp:Panel>
                        <div class="ed-project-list-empty" data-ed-empty-search>Không tìm thấy dự án phù hợp.</div>
                    </div>
                    <div class="ed-pop-help"><i class="fas fa-info-circle"></i> Chọn dự án ở đây, công việc sẽ hiện ngay bên phải.</div>
                </aside>
                <section class="ed-project-detail" aria-live="polite" aria-label="Công việc thuộc dự án đang chọn">
                    <div class="ed-project-focus">
                        <div><div class="ed-project-focus-heading"><span class="ed-project-focus-code" data-ed-selected-code>CHỌN DỰ ÁN</span><span class="ed-project-status" data-ed-selected-status hidden></span></div><h3 class="ed-project-focus-name" data-ed-selected-name>Danh sách công việc</h3><div class="ed-project-focus-meta"><span><i class="fas fa-tasks"></i> <span data-ed-selected-summary>—</span></span></div></div>
                    </div>
                    <div class="ed-project-tasks-top"><h3><i class="fas fa-list-check me-1"></i> Công việc được giao trong dự án</h3><span class="ed-project-task-count" data-ed-project-task-count>0 công việc</span></div>
                    <div class="ed-project-tasks-scroll" data-ed-project-tasks>
                        <asp:Repeater ID="rptProjectTasks" runat="server"><ItemTemplate>
                            <article class="ed-project-task" data-ed-project-task data-project-id='<%# Eval("ProjectId") %>' data-status='<%# (bool)Eval("IsOverdue") ? "overdue" : (bool)Eval("IsDueSoon") ? "soon" : (bool)Eval("IsCompleted") ? "done" : "inprogress" %>'>
                                <div class="ed-project-task-main"><div class="ed-task-inline-title"><span class="ed-task-inline-code"><%#: Eval("TaskCode") %>.</span> <strong class="ed-project-task-name"><%#: Eval("TaskName") %></strong></div><span class="ed-project-task-when"><i class="far fa-calendar-alt"></i> <%#: Eval("DateRange") %></span></div><span class="ed-project-task-labels"><span class='<%# "ed-project-task-status " + Eval("StatusCss") %>'><%#: Eval("StatusText") %></span><asp:PlaceHolder runat="server" Visible='<%# !string.IsNullOrEmpty(Convert.ToString(Eval("DeadlineText"))) %>'><span class='<%# (bool)Eval("IsOverdue") ? "ed-project-task-deadline is-overdue" : "ed-project-task-deadline is-soon" %>'><%#: Eval("DeadlineText") %></span></asp:PlaceHolder></span>
                                <div class='<%# "ed-project-task-progress ed-task-progress-color " + Eval("ProgressToneCss") %>' title='<%# System.Web.HttpUtility.HtmlAttributeEncode(Convert.ToString(Eval("AssignedTaskTooltip"))) %>' aria-label='<%# System.Web.HttpUtility.HtmlAttributeEncode(Convert.ToString(Eval("AssignedTaskTooltip"))) %>' tabindex="0"><span><i style='width:<%#: Eval("Progress") %>%'></i></span><b><%#: Eval("Progress") %>%</b></div>
                            </article>
                        </ItemTemplate></asp:Repeater>
                        <div class="ed-project-no-tasks" data-ed-no-project-tasks hidden><i class="far fa-clipboard"></i>Không có công việc được giao trong dự án này.</div>
                    </div>
                </section>
            </div>
        </ContentTemplate></asp:UpdatePanel>
    </ContentTemplate>
</SweetSoft:ExtraModal>

<SweetSoft:ExtraModal ID="mdlTasks" runat="server" Title="Công việc trong tuần" Type="Primary" Width="88%">
    <ContentTemplate><asp:UpdatePanel ID="upnlModalTasks" runat="server" UpdateMode="Conditional"><ContentTemplate>
        <div class="ed-dashboard-popup-body ed-popup-inner" data-ed-modal-kind="tasks" style="padding:19px 22px 20px">
            <div class="ed-pop-headline"><div><h2 data-ed-task-heading>Công việc trong tuần</h2><p>Những công việc bạn được phân công trong tuần đang xem và các việc cần xử lý quá hạn</p></div><span class="ed-pop-total" data-ed-count>0 mục</span></div>
            <div class="ed-pop-filter"><label class="ed-pop-search"><i class="fas fa-search"></i><input type="search" placeholder="Tìm tên hoặc mã công việc..." aria-label="Tìm công việc" oninput="edFilterDashboardModal(this)" /></label><label class="ed-task-project-select-wrap" title="Lọc công việc theo dự án"><i class="fas fa-folder-open" aria-hidden="true"></i><select data-ed-project-select aria-label="Lọc theo dự án" onchange="edSetTaskProject(this.value)"><option value="">Tất cả dự án</option></select><i class="fas fa-chevron-down ed-task-project-chevron" aria-hidden="true"></i></label><button type="button" class="btn btn-primary btn-sm ed-modal-filter" data-ed-filter="all" onclick="edSetTaskScope('all',this)">Tất cả</button><button type="button" class="btn btn-outline-primary btn-sm ed-modal-filter" data-ed-filter="overdue" onclick="edSetTaskScope('overdue',this)">Quá hạn</button><asp:Label ID="lblProjectFilterName" runat="server" Visible="false" CssClass="badge bg-light text-dark border" /><asp:LinkButton ID="btnAttentionTasks" runat="server" Visible="false" CausesValidation="false" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnAttentionTasks_Click">Bỏ lọc dự án</asp:LinkButton></div>
            <div class="ed-project-task-filter-note" data-ed-project-filter-note hidden><span class="ed-filter-project-title">Dự án: <strong data-ed-project-filter-label></strong></span><button type="button" class="ed-filter-project-clear" onclick="return edClearDashboardProjectFilter();" title="Bỏ lọc dự án" aria-label="Bỏ lọc dự án"><i class="fas fa-times" aria-hidden="true"></i></button></div>
            <div class="ed-popup-results ed-updated-results" data-ed-selected-project-tasks hidden></div>
            <asp:Repeater ID="rptTasksAll" runat="server"><HeaderTemplate><div class="ed-popup-results ed-updated-results"></HeaderTemplate><ItemTemplate>
                <article class="ed-popup-card" data-ed-item data-project-id='<%# Eval("ProjectId") %>' data-project-name='<%# System.Web.HttpUtility.HtmlAttributeEncode(Convert.ToString(Eval("ProjectName"))) %>' data-ed-status='<%# (bool)Eval("IsOverdue") ? "overdue" : (bool)Eval("IsDueSoon") ? "soon" : (bool)Eval("IsCompleted") ? "done" : "inprogress" %>' data-ed-search='<%# Server.HtmlEncode(Convert.ToString(Eval("ProjectCode")) + " " + Convert.ToString(Eval("TaskCode")) + " " + Convert.ToString(Eval("TaskName"))) %>'>
                    <div class="ed-popup-card-header"><span class="ed-popup-card-icon"><i class="fas fa-list-check"></i></span><div class="ed-popup-card-main"><div class="ed-popup-card-titleline"><div class="ed-task-inline-title"><span class="ed-task-inline-code"><%#: Eval("TaskCode") %>.</span> <strong class="ed-popup-card-name"><%#: Eval("TaskName") %></strong></div><span class="ed-task-labels"><span class='<%# "ed-popup-badge ed-task-state " + Eval("StatusCss") %>'><%#: Eval("StatusText") %></span><asp:PlaceHolder runat="server" Visible='<%# !string.IsNullOrEmpty(Convert.ToString(Eval("DeadlineText"))) %>'><span class='<%# (bool)Eval("IsOverdue") ? "ed-task-deadline is-overdue" : "ed-task-deadline is-soon" %>'><%#: Eval("DeadlineText") %></span></asp:PlaceHolder></span></div></div></div>
                    <div class="ed-popup-meta small text-muted"><span><i class="far fa-calendar-alt"></i> <%#: Eval("DateRange") %></span><span><i class="fas fa-folder-open"></i> <%#: Eval("ProjectName") %></span></div>
                    <div class='<%# "ed-popup-progress ed-task-progress-color " + Eval("ProgressToneCss") %>' title='<%# System.Web.HttpUtility.HtmlAttributeEncode(Convert.ToString(Eval("AssignedTaskTooltip"))) %>' aria-label='<%# System.Web.HttpUtility.HtmlAttributeEncode(Convert.ToString(Eval("AssignedTaskTooltip"))) %>' tabindex="0"><span style='width:<%#: Eval("Progress") %>%'></span></div><div class="ed-popup-progress-caption small"><span class="text-muted">Tiến độ công việc</span><strong><%#: Eval("Progress") %>%</strong></div>
                </article>
            </ItemTemplate><FooterTemplate></div></FooterTemplate></asp:Repeater>
            <asp:Panel ID="pnlNoAllTasks" runat="server" Visible="false" CssClass="ed-popup-empty">Không có công việc cần theo dõi.</asp:Panel>
            <div class="ed-popup-no-match" data-ed-empty-search>Không tìm thấy công việc phù hợp.</div>
            <div class="ed-pop-help"><i class="fas fa-info-circle"></i> Có thể lọc nhanh những công việc đã quá hạn để ưu tiên xử lý.</div>
        </div>
    </ContentTemplate></asp:UpdatePanel></ContentTemplate>
</SweetSoft:ExtraModal>

<SweetSoft:ExtraModal ID="mdlIssues" runat="server" Title="Vấn đề được giao" Type="Primary" Width="88%">
    <ContentTemplate><asp:UpdatePanel ID="upnlModalIssues" runat="server" UpdateMode="Conditional"><ContentTemplate>
        <div class="ed-dashboard-popup-body ed-popup-inner" data-ed-modal-kind="issues" style="padding:19px 22px 20px">
            <div class="ed-pop-headline"><div><h2>Vấn đề cần theo dõi</h2><p>Thông tin dự án, ảnh hưởng và trạng thái xử lý</p></div><span class="ed-pop-total" data-ed-count>0 mục</span></div>
            <div class="ed-pop-filter"><label class="ed-pop-search"><i class="fas fa-search"></i><input type="search" placeholder="Tìm mã, tên vấn đề hoặc dự án..." aria-label="Tìm vấn đề" oninput="edFilterDashboardModal(this)" /></label></div>
            <asp:Repeater ID="rptIssuesAll" runat="server"><HeaderTemplate><div class="ed-popup-results ed-updated-results"></HeaderTemplate><ItemTemplate>
                <article class="ed-popup-card" data-ed-item data-ed-search='<%# Server.HtmlEncode(Convert.ToString(Eval("Code")) + " " + Convert.ToString(Eval("Name")) + " " + Convert.ToString(Eval("ProjectName"))) %>'>
                    <div class="ed-popup-card-header"><span class="ed-popup-card-icon"><i class="fas fa-exclamation-circle"></i></span><div class="ed-popup-card-main"><div class="ed-popup-card-code small text-muted"><%#: Eval("Code") %></div><strong class="ed-popup-card-name"><%#: Eval("Name") %></strong></div></div>
                    <div class="ed-popup-meta small text-muted"><span><i class="fas fa-folder-open"></i> <%#: Eval("ProjectName") %></span><span><i class="far fa-calendar-alt"></i> <%#: Eval("CreatedDate") %></span></div>
                    <div class="ed-issue-highlight"><span><i class="fas fa-bolt"></i> <%#: Eval("Impact") %></span><span><i class="far fa-check-circle"></i> <%#: Eval("StatusText") %></span><span><i class="fas fa-sitemap"></i> <%#: Eval("AffectedTask") %></span></div>
                </article>
            </ItemTemplate><FooterTemplate></div></FooterTemplate></asp:Repeater>
            <asp:Panel ID="pnlNoIssues" runat="server" Visible="false" CssClass="ed-popup-empty">Bạn chưa có vấn đề được giao.</asp:Panel>
            <div class="ed-popup-no-match" data-ed-empty-search>Không tìm thấy vấn đề phù hợp.</div>
            <div class="ed-pop-help"><i class="fas fa-info-circle"></i> Tên vấn đề và công việc liên quan được hiển thị ngay trong từng thẻ.</div>
        </div>
    </ContentTemplate></asp:UpdatePanel></ContentTemplate>
</SweetSoft:ExtraModal>

<SweetSoft:ExtraModal ID="mdlCalendar" runat="server" Title="Lịch làm việc chi tiết" Type="Primary" Width="88%">
    <ContentTemplate><asp:UpdatePanel ID="upnlModalCalendar" runat="server" UpdateMode="Conditional"><ContentTemplate>
        <div class="ed-dashboard-popup-body ed-popup-inner" data-ed-modal-kind="calendar" style="padding:19px 22px 20px">
            <div class="ed-pop-headline"><div><h2>Chi tiết lịch tuần</h2><p>Công việc, cuộc họp và lịch nghỉ theo từng ngày</p></div></div>
            <asp:Repeater ID="rptCalendarAll" runat="server"><HeaderTemplate><div class="ed-calendar-week-detail"></HeaderTemplate><ItemTemplate>
                <article class='ed-calendar-week-day <%# (bool)Eval("IsToday") ? "is-today" : "" %>'><div class="ed-calendar-week-head"><b><%#: Eval("DayName") %> · <%#: Eval("DayNumber") %></b><small><%#: Eval("DayType") %></small></div>
                    <asp:Repeater ID="rptEventsAll" runat="server" DataSource='<%# Eval("Events") %>'><ItemTemplate><div class='ed-calendar-event-line is-<%#: Eval("CssClass") %>'><strong><%#: Eval("Title") %></strong><span><%#: Eval("TimeLabel") %></span><span><%#: Eval("Subtitle") %></span></div></ItemTemplate></asp:Repeater>
                    <asp:PlaceHolder runat="server" Visible='<%# !((CalendarDayItem)Container.DataItem).HasEvents %>'><div class="ed-calendar-empty-day">Không có lịch</div></asp:PlaceHolder>
                </article>
            </ItemTemplate><FooterTemplate></div></FooterTemplate></asp:Repeater>
        </div>
    </ContentTemplate></asp:UpdatePanel></ContentTemplate>
</SweetSoft:ExtraModal>
</div>
<script type="text/javascript">

    // Mở duy nhất ExtraModal CtrlTodaySchedule từ Master; không sinh popup khác.
    function edOpenSharedDaySchedule(day) {
        if (!window.TodayScheduleUI || typeof window.TodayScheduleUI.openDate !== 'function') return false;
        window.TodayScheduleUI.openDate(day || '');
        return false;
    }
    (function () {
        function openFromDay(event) {
            var day = event.target && event.target.closest && event.target.closest('#edWeekCalendar [data-ed-schedule-date]');
            if (!day) return;
            if (event.type === 'keydown' && event.key !== 'Enter' && event.key !== ' ') return;
            event.preventDefault();
            event.stopImmediatePropagation();
            var tip = document.querySelector('.ed-floating-tip');
            if (tip) tip.classList.remove('is-visible', 'is-pinned');
            edOpenSharedDaySchedule(day.getAttribute('data-ed-schedule-date'));
        }
        document.addEventListener('click', openFromDay, true);
        document.addEventListener('keydown', openFromDay, true);
    })();
    function edFindDashboardModal(name) {
        var direct = document.getElementById(name);
        var container = direct || document.querySelector('[id$="_' + name + '"]');
        if (!container) return null;
        return container.classList.contains('modal') ? container : container.querySelector('.modal');
    }
    function edOpenDashboardModal(name) {
        var modal = edFindDashboardModal(name);
        if (!modal) { console.error('Không tìm thấy ExtraModal:', name); return false; }
        if (typeof edInstallPopupCloseGuards === 'function') edInstallPopupCloseGuards();
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

    var edProjectDashboardStatus = 'overdue';
    var edActiveProjectFilter = null;
    var edTaskProjectId = '';
    function edPopulateTaskProjectSelect(panel) {
        if (!panel || panel.getAttribute('data-ed-modal-kind') !== 'tasks') return;
        var select = panel.querySelector('[data-ed-project-select]'); if (!select) return;
        var desired = edTaskProjectId || '';
        var projects = {}, cards = panel.querySelectorAll('.ed-popup-results [data-ed-item]');
        // Toàn bộ dự án của nhân viên: có thể chọn cả dự án chưa có task trong tuần.
        document.querySelectorAll('[data-ed-project-choice][data-project-id]').forEach(function (item) {
            var key = item.getAttribute('data-project-id') || '', label = item.getAttribute('data-project-name') || '';
            if (key && label) projects[key] = label;
        });
        cards.forEach(function (card) {
            var id = card.getAttribute('data-project-id') || '';
            var name = card.getAttribute('data-project-name') || '';
            if (!name) { var label = card.querySelector('.ed-popup-meta .fa-folder-open'); name = label && label.parentNode ? label.parentNode.textContent.trim() : ''; }
            if (id && name) projects[id] = name;
        });
        // Nếu đang chuyển từ popup dự án, giữ cả lựa chọn của dự án vừa mở.
        if (edActiveProjectFilter && edActiveProjectFilter.id && !projects[edActiveProjectFilter.id]) projects[edActiveProjectFilter.id] = edActiveProjectFilter.name;
        var entries = Object.keys(projects).sort(function (a, b) { return projects[a].localeCompare(projects[b], 'vi'); });
        select.innerHTML = '';
        var option = document.createElement('option'); option.value = ''; option.textContent = 'Tất cả dự án'; select.appendChild(option);
        entries.forEach(function (id) { var el = document.createElement('option'); el.value = id; el.textContent = projects[id]; select.appendChild(el); });
        select.value = projects[desired] ? desired : '';
        if (!projects[desired]) edTaskProjectId = '';
    }
    function edSetTaskProject(projectId) {
        var panel = document.querySelector('[data-ed-modal-kind="tasks"]'); if (!panel) return false;
        // Chọn trong bộ lọc tuần: trở về nguồn task tuần, không dùng thẻ clone từ popup dự án.
        if (edActiveProjectFilter) {
            edActiveProjectFilter = null;
            var container = panel.querySelector('[data-ed-selected-project-tasks]'); if (container) { container.hidden = true; container.innerHTML = ''; }
            var original = panel.querySelector('.ed-popup-results:not([data-ed-selected-project-tasks])'); if (original) original.hidden = false;
            var note = panel.querySelector('[data-ed-project-filter-note]'); if (note) note.hidden = true;
        }
        edTaskProjectId = projectId || '';
        var select = panel.querySelector('[data-ed-project-select]'); if (select) select.value = edTaskProjectId;
        edSetTaskScope(window.edTaskPopupScope || 'all');
        return false;
    }
    function edFilterProjectDashboard(status) {
        edProjectDashboardStatus = ['overdue', 'soon', 'inprogress'].indexOf(status) >= 0 ? status : 'overdue';
        var names = { overdue: 'trễ hạn', soon: 'sắp đến hạn', inprogress: 'còn lại' };
        document.querySelectorAll('[data-ed-status-filter]').forEach(function (button) { var active = button.getAttribute('data-ed-status-filter') === edProjectDashboardStatus; button.classList.toggle('is-active', active); button.setAttribute('aria-pressed', active ? 'true' : 'false'); });
        var count = 0;
        document.querySelectorAll('[data-ed-board-project]').forEach(function (row) { var visible = row.getAttribute('data-project-status') === edProjectDashboardStatus; row.hidden = !visible; if (visible) count++; });
        var label = document.querySelector('[data-ed-board-label]'); if (label) label.textContent = names[edProjectDashboardStatus];
        var countBox = document.querySelector('[data-ed-board-count]'); if (countBox) countBox.textContent = count + ' dự án';
        var empty = document.querySelector('[data-ed-board-empty]'); if (empty) empty.hidden = count > 0;
        return false;
    }
    function edOpenProjectTasksFromBoard(button) {
        if (!button) return false;
        var id = button.getAttribute('data-project-id'), name = button.getAttribute('data-project-name') || 'Dự án';
        return edOpenDashboardProjectTasks(id, name);
    }
    function edOpenDashboardProjectTasks(id, name) {
        var panel = document.querySelector('[data-ed-modal-kind="tasks"]');
        var projectPanel = document.querySelector('[data-ed-modal-kind="projects"]');
        if (!panel || !projectPanel || !id) return false;
        var allContainer = panel.querySelector('[data-ed-selected-project-tasks]');
        var original = panel.querySelector('.ed-popup-results:not([data-ed-selected-project-tasks])');
        if (!allContainer || !original) return false;
        edActiveProjectFilter = { id: id, name: name };
        edTaskProjectId = id;
        allContainer.innerHTML = '';
        projectPanel.querySelectorAll('[data-ed-project-task]').forEach(function (item) {
            if (item.getAttribute('data-project-id') !== id) return;
            var card = item.cloneNode(true);
            card.removeAttribute('hidden'); card.hidden = false; card.setAttribute('data-ed-item', ''); card.setAttribute('data-ed-status', item.getAttribute('data-status') || 'inprogress');
            card.setAttribute('data-ed-search', card.textContent || '');
            allContainer.appendChild(card);
        });
        original.hidden = true; allContainer.hidden = false;
        var note = panel.querySelector('[data-ed-project-filter-note]'); if (note) note.hidden = false;
        var label = panel.querySelector('[data-ed-project-filter-label]'); if (label) label.textContent = name;
        edPopulateTaskProjectSelect(panel);
        var title = panel.querySelector('[data-ed-task-heading]'); if (title) title.textContent = 'Công việc của dự án';
        var search = panel.querySelector('.ed-pop-search input'); if (search) search.value = '';
        edSetTaskScope('all');
        edOpenDashboardModal('mdlTasks');
        return false;
    }
    function edClearDashboardProjectFilter() {
        edActiveProjectFilter = null;
        edTaskProjectId = '';
        var panel = document.querySelector('[data-ed-modal-kind="tasks"]'); if (!panel) return false;
        var container = panel.querySelector('[data-ed-selected-project-tasks]'); if (container) { container.hidden = true; container.innerHTML = ''; }
        var original = panel.querySelector('.ed-popup-results:not([data-ed-selected-project-tasks])'); if (original) original.hidden = false;
        var note = panel.querySelector('[data-ed-project-filter-note]'); if (note) note.hidden = true;
        var selector = panel.querySelector('[data-ed-project-select]'); if (selector) selector.value = '';
        var search = panel.querySelector('.ed-pop-search input'); if (search) search.value = '';
        edSetTaskScope('all');
        return false;
    }
    function edSelectDashboardProject(button) {
        if (!button || button.hidden) return false;
        var panel = button.closest('[data-ed-modal-kind="projects"]');
        if (!panel) return false;
        panel.querySelectorAll('[data-ed-project-choice]').forEach(function (choice) {
            var chosen = choice === button;
            choice.classList.toggle('is-selected', chosen);
            choice.setAttribute('aria-selected', chosen ? 'true' : 'false');
        });
        var key = button.getAttribute('data-project-id') || '';
        [['[data-ed-selected-code]', 'data-project-code'], ['[data-ed-selected-name]', 'data-project-name'], ['[data-ed-selected-summary]', 'data-project-summary']].forEach(function (pair) {
            var el = panel.querySelector(pair[0]);
            if (el) el.textContent = button.getAttribute(pair[1]) || '—';
        });
        var statusBadge = panel.querySelector('[data-ed-selected-status]');
        if (statusBadge) {
            statusBadge.textContent = button.getAttribute('data-project-status-text') || '';
            statusBadge.className = 'ed-project-status ' + (button.getAttribute('data-project-status-css') || '');
            statusBadge.hidden = !statusBadge.textContent;
        }
        var visibleCount = 0;
        panel.querySelectorAll('[data-ed-project-task]').forEach(function (task) {
            var show = task.getAttribute('data-project-id') === key;
            task.hidden = !show;
            if (show) visibleCount++;
        });
        var total = panel.querySelector('[data-ed-project-task-count]');
        if (total) total.textContent = visibleCount + ' công việc';
        var empty = panel.querySelector('[data-ed-no-project-tasks]');
        if (empty) empty.hidden = visibleCount > 0;
        return false;
    }
    function edEnsureDashboardProjectSelection(panel) {
        if (!panel || panel.getAttribute('data-ed-modal-kind') !== 'projects') return;
        var current = panel.querySelector('[data-ed-project-choice].is-selected:not([hidden])');
        var first = Array.prototype.find.call(panel.querySelectorAll('[data-ed-project-choice]'), function (x) { return !x.hidden; });
        if (current || first) edSelectDashboardProject(current || first);
        else {
            panel.querySelectorAll('[data-ed-project-task]').forEach(function (x) { x.hidden = true; });
            var total = panel.querySelector('[data-ed-project-task-count]');
            if (total) total.textContent = '0 công việc';
            var empty = panel.querySelector('[data-ed-no-project-tasks]');
            if (empty) empty.hidden = false;
        }
    }

    function edSetTaskScope(scope, button) {
        window.edTaskPopupScope = scope === 'overdue' ? 'overdue' : 'all';
        try { sessionStorage.setItem('SweetSoftEmployeeTaskPopupScope', window.edTaskPopupScope); } catch (error) { }
        var panel = document.querySelector('[data-ed-modal-kind="tasks"]');
        if (!panel) return;
        var title = panel.querySelector('[data-ed-task-heading]');
        if (title) title.textContent = edActiveProjectFilter ? 'Công việc của dự án' : (window.edTaskPopupScope === 'overdue' ? 'Công việc quá hạn' : 'Công việc trong tuần');
        panel.querySelectorAll('[data-ed-filter]').forEach(function (control) { var active = control.getAttribute('data-ed-filter') === window.edTaskPopupScope; control.classList.toggle('is-active', active); control.classList.toggle('btn-primary', active); control.classList.toggle('btn-outline-primary', !active); });
        edApplyDashboardFilter(panel);
    }
    function edFilterDashboardModal(input) {
        var panel = input && input.closest('.ed-popup-inner');
        if (panel) edApplyDashboardFilter(panel);
    }
    function edApplyDashboardFilter(panel) {
        if (!panel) return;
        var input = panel.querySelector('.ed-pop-search input');
        var term = input ? input.value.trim().toLocaleLowerCase('vi') : '';
        var taskFilter = panel.getAttribute('data-ed-modal-kind') === 'tasks' ? (window.edTaskPopupScope || 'all') : 'all';
        var all = panel.getAttribute('data-ed-modal-kind') === 'tasks' ? (edActiveProjectFilter ? panel.querySelectorAll('[data-ed-selected-project-tasks] [data-ed-item]') : panel.querySelectorAll('.ed-popup-results:not([data-ed-selected-project-tasks]) [data-ed-item]')) : panel.querySelectorAll('[data-ed-item]');
        var count = 0;
        all.forEach(function (card) {
            var matchesSearch = !term || (card.getAttribute('data-ed-search') || card.textContent || '').toLocaleLowerCase('vi').includes(term);
            var matchesStatus = taskFilter !== 'overdue' || card.getAttribute('data-ed-status') === 'overdue';
            var matchesProject = panel.getAttribute('data-ed-modal-kind') !== 'tasks' || !edTaskProjectId || card.getAttribute('data-project-id') === edTaskProjectId;
            card.hidden = !(matchesSearch && matchesStatus && matchesProject);
            if (!card.hidden) count++;
        });
        var badge = panel.querySelector('[data-ed-count]');
        if (badge) badge.textContent = count + ' mục';
        if (panel.getAttribute('data-ed-modal-kind') === 'tasks') {
            var resultGrid = panel.querySelector('.ed-popup-results:not([data-ed-selected-project-tasks])');
            var projectGrid = panel.querySelector('[data-ed-selected-project-tasks]');
            if (resultGrid) resultGrid.classList.toggle('ed-one-result', !edActiveProjectFilter && count === 1);
            if (projectGrid) projectGrid.classList.toggle('ed-one-result', !!edActiveProjectFilter && count === 1);
        }
        var noResults = panel.querySelector('[data-ed-empty-search]');
        if (noResults) noResults.classList.toggle('is-visible', count === 0 && all.length > 0);
        if (panel.getAttribute('data-ed-modal-kind') === 'projects') edEnsureDashboardProjectSelection(panel);
    }
    function edInitDashboardPopups() {
        try { window.edTaskPopupScope = sessionStorage.getItem('SweetSoftEmployeeTaskPopupScope') === 'overdue' ? 'overdue' : 'all'; } catch (error) { window.edTaskPopupScope = window.edTaskPopupScope || 'all'; }
        document.querySelectorAll('.ed-popup-inner').forEach(function (panel) {
            if (panel.getAttribute('data-ed-modal-kind') === 'tasks') {
                edPopulateTaskProjectSelect(panel);
                var heading = panel.querySelector('[data-ed-task-heading]');
                if (heading) heading.textContent = edActiveProjectFilter ? 'Công việc của dự án' : (window.edTaskPopupScope === 'overdue' ? 'Công việc quá hạn' : 'Công việc trong tuần');
                panel.querySelectorAll('[data-ed-filter]').forEach(function (control) { var active = control.getAttribute('data-ed-filter') === window.edTaskPopupScope; control.classList.toggle('is-active', active); control.classList.toggle('btn-primary', active); control.classList.toggle('btn-outline-primary', !active); });
            }
            edApplyDashboardFilter(panel);
        });
    }
    /* Bootstrap/ExtraModal: đóng bằng fade, không thu/zoom chiều rộng trước khi ẩn. */
    function edInstallPopupCloseGuards() {
        ['mdlProjects', 'mdlTasks', 'mdlIssues', 'mdlCalendar'].forEach(function (name) {
            var modal = edFindDashboardModal(name);
            if (!modal || modal.getAttribute('data-ed-close-guard') === '1') return;
            modal.setAttribute('data-ed-close-guard', '1');
            function startClose() {
                var dialog = modal.querySelector('.modal-dialog');
                if (dialog) {
                    var width = dialog.getBoundingClientRect().width;
                    if (width > 0) modal.style.setProperty('--ed-popup-close-width', width + 'px');
                }
                modal.classList.add('ed-popup-is-closing');
            }
            function finishClose() { modal.classList.remove('ed-popup-is-closing'); modal.style.removeProperty('--ed-popup-close-width'); }
            modal.addEventListener('hide.bs.modal', startClose);
            modal.addEventListener('hidden.bs.modal', finishClose);
            modal.addEventListener('show.bs.modal', finishClose);
            if (window.jQuery && window.jQuery.fn) window.jQuery(modal).on('hide.bs.modal.edPopupClose', startClose).on('hidden.bs.modal.edPopupClose show.bs.modal.edPopupClose', finishClose);
        });
    }

    var edOriginalInitDashboardPopups = edInitDashboardPopups;
    edInitDashboardPopups = function () { edOriginalInitDashboardPopups(); edFilterProjectDashboard(edProjectDashboardStatus); edInstallPopupCloseGuards(); };
    if (window.Sys && Sys.Application) Sys.Application.add_load(edInitDashboardPopups);
    else if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', edInitDashboardPopups);
    else edInitDashboardPopups();

    (function () {
        var activeTip = null, pinnedTip = null, floatTip = null;
        function getTip() { if (!floatTip) { floatTip = document.createElement('div'); floatTip.className = 'ed-floating-tip'; floatTip.setAttribute('role', 'tooltip'); document.body.appendChild(floatTip); } return floatTip; }
        function positionTip() { if (!activeTip || !document.documentElement.contains(activeTip)) { hideTip(true); return; } var el = getTip(), rect = activeTip.getBoundingClientRect(), w = el.offsetWidth, h = el.offsetHeight, right = rect.right + 11, left = rect.left - w - 11, useRight = right + w <= window.innerWidth - 10, x = useRight ? right : left; el.classList.toggle('ed-tip-left', !useRight); x = Math.max(10, Math.min(x, window.innerWidth - w - 10)); el.style.left = x + 'px'; el.style.top = Math.max(10, Math.min(rect.top, window.innerHeight - h - 10)) + 'px'; }
        function showTip(target) { if (!target || (pinnedTip && pinnedTip !== target)) return; var source = target.querySelector('.ed-tip-source'); if (!source) return; activeTip = target; var tip = getTip(); tip.innerHTML = source.innerHTML + (pinnedTip === target ? '<div class="ed-tip-pin-hint"><i class="fas fa-thumbtack"></i> Đang ghim · Bấm lại thẻ hoặc bên ngoài để đóng</div>' : ''); tip.classList.toggle('is-pinned', pinnedTip === target); tip.classList.remove('is-visible'); positionTip(); window.requestAnimationFrame(function () { if (activeTip === target) tip.classList.add('is-visible'); }); }
        function hideTip(force) { if (pinnedTip && !force) return; if (pinnedTip) pinnedTip.classList.remove('is-pinned'); pinnedTip = null; activeTip = null; if (floatTip) { floatTip.classList.remove('is-visible', 'is-pinned'); } }
        window.edResetDashboardTips = function () { hideTip(true); };
        document.addEventListener('mouseover', function (event) { var target = event.target.closest('[data-ed-tip-trigger]'); if (!pinnedTip && target && target !== activeTip) showTip(target); });
        document.addEventListener('mouseout', function (event) { var target = event.target.closest('[data-ed-tip-trigger]'); if (target && !pinnedTip && (!event.relatedTarget || !target.contains(event.relatedTarget))) hideTip(false); });
        document.addEventListener('focusin', function (event) { var target = event.target.closest('[data-ed-tip-trigger]'); if (target && !pinnedTip) showTip(target); });
        document.addEventListener('focusout', function (event) { var target = event.target.closest('[data-ed-tip-trigger]'); if (target && !pinnedTip) hideTip(false); });
        document.addEventListener('click', function (event) { var target = event.target.closest('[data-ed-tip-trigger]'); if (!target) { if (pinnedTip && (!floatTip || !floatTip.contains(event.target))) hideTip(true); return; } event.preventDefault(); if (pinnedTip === target) { hideTip(true); return; } hideTip(true); pinnedTip = target; pinnedTip.classList.add('is-pinned'); showTip(target); });
        document.addEventListener('keydown', function (event) { if (event.key === 'Escape' && pinnedTip) { var old = pinnedTip; hideTip(true); old.focus(); } else if ((event.key === 'Enter' || event.key === ' ') && event.target.matches('[data-ed-tip-trigger]')) { event.preventDefault(); event.target.click(); } });
        window.addEventListener('scroll', positionTip, true); window.addEventListener('resize', positionTip);

    })();

</script>

</asp:Content>
