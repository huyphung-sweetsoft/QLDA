<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlEmployeeDashboard.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fEmployeeDashboard.Controls.CtrlEmployeeDashboard" %>
<style>
    /* =========================================================
       EMPLOYEE DASHBOARD - NEW LAYOUT (PROFILE + 4 CARDS + TIME)
       ========================================================= */
    .ed-dashboard { --ed-primary: #5b21b6; --ed-primary-hover: #4c1d95; --ed-bg: #f8fafc; --ed-surface: #ffffff; --ed-border: #e2e8f0; --ed-text-main: #1e293b; --ed-text-muted: #64748b; --ed-radius-lg: 12px; --ed-radius-md: 8px; color: var(--ed-text-main); font-family: "Segoe UI", system-ui, sans-serif; font-size: 13px; line-height: 1.4; padding: 0 0 20px; }
    .ed-dashboard * { box-sizing: border-box; }
    .ed-dashboard h1, .ed-dashboard h2, .ed-dashboard h3, .ed-dashboard p { font-family: inherit; margin: 0; }
    
    /* 1. TOP HEADER ROW: PROFILE + 4 STATS + RIGHT AREA (4:6) */
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

    /* Ô cuối 3fr của header: lịch và thời gian dùng chung một khung, chia 4:6. */
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
    .ed-calendar-entry { display: flex; min-width: 0; min-height: 122px; padding: 8px; border-right: 1px solid #ece6f8; }
    .ed-calendar-toggle { position: relative; z-index: 2; display: flex; width: 100%; min-width: 0; min-height: 100%; flex-direction: column; align-items: center; justify-content: center; gap: 8px; padding: 8px 7px; border: 1px solid transparent; border-radius: 9px; background: transparent; color: #51349f; font: inherit; font-size: 12.5px; font-weight: 750; line-height: 1.25; text-align: center; cursor: pointer; box-shadow: none; transition: background-color .18s ease, border-color .18s ease, color .18s ease, transform .18s ease; }
    .ed-calendar-toggle .ed-calendar-toggle-icon { display: inline-flex; align-items: center; justify-content: center; width: 46px; height: 46px; flex: 0 0 46px; border: 1px solid #e5dcfb; border-radius: 12px; background: #f0eaff; color: #5838ad; font-size: 24px; transition: background-color .18s ease, color .18s ease, border-color .18s ease; }
    .ed-calendar-toggle:hover, .ed-calendar-toggle[aria-expanded="true"] { background: #f3edff; border-color: #d8caf8; color: #3f257f; }
    .ed-calendar-toggle:hover .ed-calendar-toggle-icon, .ed-calendar-toggle[aria-expanded="true"] .ed-calendar-toggle-icon { background: #e4d9ff; border-color: #c8b5fa; color: #482895; }
    .ed-calendar-toggle:focus-visible { outline: 2px solid #8b70d2; outline-offset: 2px; }
    .ed-calendar-toggle::after { content: attr(data-tooltip); position: absolute; left: 50%; bottom: calc(100% + 9px); z-index: 20; padding: 7px 10px; border-radius: 6px; background: #20243a; color: #fff; font-size: 11px; font-weight: 600; white-space: nowrap; box-shadow: 0 3px 10px rgba(15, 23, 42, .14); opacity: 0; visibility: hidden; transform: translate(-50%, 3px); pointer-events: none; transition: opacity .16s ease, transform .16s ease, visibility .16s ease; }
    .ed-calendar-toggle::before { content: ""; position: absolute; left: calc(50% - 6px); bottom: calc(100% + 3px); z-index: 20; border: 6px solid transparent; border-top-color: #20243a; opacity: 0; visibility: hidden; transition: opacity .16s ease, visibility .16s ease; pointer-events: none; }
    .ed-calendar-toggle:hover::after, .ed-calendar-toggle:focus-visible::after { opacity: 1; visibility: visible; transform: translate(-50%, 0); }
    .ed-calendar-toggle:hover::before, .ed-calendar-toggle:focus-visible::before { opacity: 1; visibility: visible; }

    /* 2. CHARTS (Cải thiện màu sắc rực rỡ hơn) */
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

    /* 3. MAIN PANELS LAYOUT & NÚT CHI TIẾT */
    .ed-dashboard .ed-layout { display: grid; grid-template-columns: minmax(320px, 0.9fr) minmax(0, 1.1fr); gap: 16px; align-items: stretch; margin-bottom: 16px;}
    .ed-dashboard .ed-panel { display: flex; flex-direction: column; background: var(--ed-surface); border: 1px solid var(--ed-border); border-radius: var(--ed-radius-lg); box-shadow: 0 2px 6px rgba(0,0,0,0.03); overflow: hidden; }
    
    /* Thiết kế nổi bật 2 tông màu */
    .ed-panel-project { border-top: 4px solid #d6a20a; }
    .ed-panel-project .ed-panel-head { background: linear-gradient(to right, #fff8df, #fff); }
    .ed-panel-task { border-top: 4px solid #2563eb; }
    .ed-panel-task .ed-panel-head { background: linear-gradient(to right, #eff6ff, #fff); }

    .ed-dashboard .ed-panel-head { display: flex; justify-content: space-between; align-items: flex-start; padding: 14px 18px; border-bottom: 1px solid var(--ed-border); }
    .ed-dashboard .ed-panel-heading { display: flex; align-items: flex-start; gap: 12px; flex: 1; min-width:0; }
    .ed-dashboard .ed-panel-icon { display: flex; align-items: center; justify-content: center; width: 34px; height: 34px; border-radius: 8px; font-size: 15px; margin-top: 2px; }
    .ed-panel-project .ed-panel-icon { background: #fff0b8; color: #8a5a00; }
    .ed-panel-task .ed-panel-icon { background: #dbeafe; color: #2563eb; }
    
    .ed-dashboard .ed-panel-title { font-size: 16px; font-weight: 700; color: var(--ed-text-main); line-height: 1.2; margin-top:2px;}
    .ed-dashboard .ed-panel-subtitle { display: flex; gap: 8px; flex-wrap: wrap; margin-top: 6px; font-size: 11.5px; color: var(--ed-text-muted); }
    .ed-dashboard .ed-count { display: inline-flex; align-items: center; justify-content: center; padding: 2px 8px; border-radius: 12px; background: #e2e8f0; color: #475569; font-size: 11.5px; font-weight: 700; }
    .ed-dashboard .ed-panel-body { padding: 16px; background: var(--ed-bg); flex: 1; display: flex; flex-direction: column; gap: 8px; }

    /* NÚT XEM CHI TIẾT TO BẢN */
    .ed-more-btn { display: block; width: 100%; padding: 12px; text-align: center; border-radius: 8px; font-weight: 700; font-size: 13px; text-decoration: none !important; transition: 0.2s; margin-top: auto; }
    .ed-btn-project { background: #fff9e7; color: #805500 !important; border: 1px solid #f0d98b; }
    .ed-btn-project:hover { background: #fff1bd; border-color: #dfbd4e; }
    .ed-btn-task { background: #eff6ff; color: #2563eb !important; border: 1px dashed #bfdbfe; }
    .ed-btn-task:hover { background: #dbeafe; border-style: solid; }

    /* NÚT TAB TRONG PHẦN CÔNG VIỆC */
    .ed-tab-btn { display: inline-flex; align-items: center; gap: 5px; padding: 4px 10px; font-size: 11.5px; font-weight: 600; border-radius: 6px; background: #f1f5f9; color: #64748b; text-decoration: none !important; border: 1px solid transparent; transition: all 0.2s; cursor: pointer; }
    .ed-tab-btn.active { background: #e0e7ff; color: #4338ca; border-color: #c7d2fe; }
    .ed-tab-btn.filtered { background: #fef3c7; color: #b45309; border-color: #fde68a; }
    .ed-tab-btn:hover { background: #e2e8f0; color: #475569; }
    .ed-tab-btn.active:hover { background: #c7d2fe; color: #3730a3; }
    .ed-tab-btn.filtered:hover { background: #fde68a; color: #92400e; }

    /* 4. ITEMS (TASKS, PROJECTS, ISSUES) */
    .ed-dashboard .ed-list, .ed-dashboard .ed-project-list { display: flex; flex-direction: column; gap: 10px; }
    .ed-dashboard .ed-card-item { background: var(--ed-surface); border: 1px solid var(--ed-border); border-radius: var(--ed-radius-md); padding: 14px; box-shadow: 0 1px 3px rgba(0,0,0,0.025); transition: border-color .18s ease, box-shadow .18s ease, transform .18s ease; }
    .ed-dashboard .ed-card-item:hover { border-color: #cbd5e1; box-shadow: 0 4px 12px rgba(15, 23, 42, .065); }
    
    .ed-dashboard .ed-project { display: flex; gap: 12px; align-items: flex-start; border-left: 3px solid #dfb52b; background: linear-gradient(105deg, #fffdf5 0%, #fff 48%); }
    .ed-dashboard .ed-project-icon { width: 36px; height: 36px; flex: 0 0 36px; display: flex; align-items: center; justify-content: center; border: 1px solid #f0dc91; border-radius: 8px; background: #fff3c4; color: #8a5b00; font-size: 15px; }
    .ed-dashboard .ed-project-content { flex: 1; min-width: 0; }
    .ed-dashboard .ed-project-code { font-size: 10.5px; font-weight: 700; color: #8b7a4c; margin-bottom: 2px; }
    .ed-dashboard .ed-project-name { display: block; font-size: 14px; font-weight: 750; color: #785500; margin-bottom: 5px; line-height: 1.3; text-decoration: none !important; transition: color .15s ease; cursor: pointer;}
    .ed-dashboard .ed-project-name:hover { color: #5b4000; text-decoration: underline !important; }
    .ed-dashboard .ed-project-progress .ed-progress span { background: #d4a20a; }
    .ed-dashboard .ed-project-meta { display: flex; flex-wrap: wrap; gap: 6px 12px; font-size: 11px; color: var(--ed-text-muted); }

    .ed-dashboard .ed-task { position: relative; border-left: 3px solid #60a5fa; background: linear-gradient(105deg, #f7fbff 0%, #fff 46%); }
    .ed-dashboard .ed-task.is-overdue { border-left-color: #ef4444; }
    .ed-dashboard .ed-task-top { display: flex; justify-content: space-between; align-items: flex-start; gap: 10px; }
    .ed-dashboard .ed-kicker { font-size: 10.5px; font-weight: 700; color: #94a3b8; margin-bottom: 3px; letter-spacing: 0.3px; }
    .ed-dashboard .ed-task-name { font-size: 14px; font-weight: 700; color: #1e3a8a; margin-bottom: 5px; line-height: 1.3; }
    .ed-dashboard .ed-task-dates { font-size: 11.5px; color: var(--ed-text-muted); display: flex; align-items: center; gap: 5px; font-weight: 500;}
    .ed-dashboard .ed-badge { display: inline-flex; padding: 3px 8px; border-radius: 4px; font-size: 10.5px; font-weight: 700; text-align: center; border: 1px solid transparent; }
    .ed-dashboard .ed-badge.overdue { background: #fef2f2; color: #b91c1c; border-color: #fecaca; }
    .ed-dashboard .ed-badge.done { background: #f0fdf4; color: #15803d; border-color: #bbf7d0; }
    .ed-dashboard .ed-badge.inprogress { background: #eff6ff; color: #1d4ed8; border-color: #bfdbfe; }
    
    .ed-dashboard .ed-progress { height: 5px; background: #e2e8f0; border-radius: 3px; margin-top: 10px; overflow: hidden; }
    .ed-dashboard .ed-progress span { display: block; height: 100%; border-radius: 3px; background: var(--ed-primary); transition: width 0.4s ease; }
    .ed-dashboard .ed-progress-meta { display: flex; justify-content: space-between; font-size: 10.5px; color: #64748b; margin-top: 5px; }
    .ed-dashboard .ed-progress-meta strong { color: var(--ed-text-main); font-weight: 700; }

    .ed-dashboard .ed-empty { text-align: center; padding: 20px 15px; background: #fff; border: 1px dashed #cbd5e1; border-radius: 8px; color: #64748b; font-size: 12.5px; margin-bottom: 10px;}
    .ed-dashboard .ed-empty strong { display: block; font-size: 13.5px; color: var(--ed-text-main); margin-bottom: 2px; }

    /* 5. CALENDAR & LOWER INSIGHTS */
    .ed-dashboard .ed-calendar-panel, .ed-dashboard .ed-charts-grid, .ed-dashboard .ed-insights-grid { grid-column: 1 / -1; }
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
    
    .ed-dashboard .ed-insights-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; margin-bottom: 16px;}
    .ed-dashboard .ed-issue, .ed-dashboard .ed-risk { padding: 12px; border: 1px solid var(--ed-border); border-radius: 8px; margin-bottom: 8px; }
    .ed-dashboard .ed-issue { border-left: 3px solid #f08a3c; background: linear-gradient(105deg, #fff8f1 0%, #fff 45%); }
    .ed-dashboard .ed-risk { border-left: 3px solid #e2bd36; background: linear-gradient(105deg, #fffdf0 0%, #fff 45%); }
    .ed-dashboard .ed-item-title { font-size: 13.5px; font-weight: 600; margin-bottom: 4px; color: var(--ed-text-main); }
    .ed-dashboard .ed-item-meta { font-size: 11.5px; color: var(--ed-text-muted); display: flex; flex-wrap: wrap; gap: 6px 10px; margin-bottom: 4px; }
    .ed-dashboard .ed-chip { padding: 2px 6px; background: #f1f5f9; border-radius: 4px; font-weight: 600; }
    .ed-dashboard .ed-chip.impact { background: #fff7ed; color: #c2410c; }
    .ed-dashboard .ed-risk-grid { display: flex; gap: 8px; margin-top: 6px; }
    .ed-dashboard .ed-risk-score { font-size: 10.5px; padding: 2px 6px; background: #fefce8; border: 1px solid #fef08a; border-radius: 4px; color: #a16207; font-weight: 600; }
    .ed-dashboard .ed-modal-open-trigger { display: flex; width: 100%; margin-top: auto; }
    .ed-dashboard .ed-modal-open-trigger > div { display: block; width: 100%; }
    .ed-dashboard .ed-modal-open-trigger .ed-more-btn { width: 100%; margin-top: 0; }

    /* ExtraModal của dashboard: giữ phạm vi riêng, không tác động popup toàn hệ thống. */
    .ed-modal-host .modal-dialog { width: 80vw !important; max-width: 1600px !important; min-width: 0; margin-left: auto !important; margin-right: auto !important; }
    .ed-modal-host .modal-content { width: 100%; }
    .ed-modal-host .modal-header, .ed-modal-host .extra-modal-header, .ed-modal-host .modal-titlebar { background: #501391 !important; background-image: none !important; border-color: #501391 !important; color: #fff !important; }
    .ed-modal-host .modal-header .modal-title, .ed-modal-host .modal-header h1, .ed-modal-host .modal-header h2, .ed-modal-host .modal-header h3, .ed-modal-host .modal-header h4, .ed-modal-host .modal-header h5, .ed-modal-host .extra-modal-header, .ed-modal-host .modal-titlebar { color: #fff !important; }
    .ed-modal-host .modal-header .close, .ed-modal-host .modal-header .btn-close { color: #fff !important; opacity: .95; }
    @media(max-width: 768px) { .ed-modal-host .modal-dialog { width: 94vw !important; max-width: 94vw !important; } }

    @keyframes ed-fade-in { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }

    /* Responsive adjustments */
    @media(max-width: 1300px) {
        .ed-header-row { grid-template-columns: repeat(2, minmax(0, 1fr)); }
        .ed-header-profile, .ed-header-tools { grid-column: 1 / -1; }
    }
    @media(max-width: 1250px) { .ed-dashboard .ed-days { grid-template-columns: repeat(4, 1fr); } }
    @media(max-width: 1100px) {
        .ed-dashboard .ed-layout { grid-template-columns: 1fr; }
        .ed-dashboard .ed-charts-grid { grid-template-columns: 1fr; }
        .ed-dashboard .ed-insights-grid { grid-template-columns: 1fr; }
    }
    @media(max-width: 768px) {
        .ed-header-row { grid-template-columns: minmax(0, 1fr); }
        .ed-header-profile, .ed-header-tools { grid-column: 1 / -1; }
        .ed-header-time { align-items: stretch; text-align: left; }
        .ed-header-time .ed-week-actions { justify-content: flex-start; }
        .ed-dashboard .ed-days { grid-template-columns: repeat(2, minmax(0, 1fr)); }
    }
    @media(max-width: 500px) {
        .ed-header-tools { grid-template-columns: minmax(0, 1fr); }
        .ed-calendar-entry { min-height: 92px; padding: 6px; border-right: 0; border-bottom: 1px solid #ece6f8; }
        .ed-calendar-toggle, .ed-header-time { min-height: 92px; }
        .ed-header-time { align-items: center; text-align: center; }
        .ed-header-time .ed-week-actions { justify-content: center; }
        .ed-calendar-toggle::after { left: 50%; }
        .ed-calendar-toggle::before { left: calc(50% - 6px); }
        .ed-dashboard .ed-days { grid-template-columns: 1fr; }
        .ed-chart-container { flex-direction: column; gap: 20px; }
        .ed-chart-legend { max-width: 100%; width: 100%; }
    }
</style>

<div class="ed-dashboard" id="edEmployeeDashboard">
    <asp:Panel ID="pnlDashboardMessage" runat="server" Visible="false" CssClass="ed-alert"><i class="fas fa-exclamation-circle" aria-hidden="true"></i> <asp:Label ID="lblDashboardMessage" runat="server" /></asp:Panel>
    
    <!-- 1. TOP HEADER ROW (RATIO 30 - 10 - 10 - 10 - 10 - 30) -->
    <div class="ed-header-row">
        <!-- 30% Profile -->
        <div class="ed-header-profile">
            <asp:Image ID="imgAvatar" runat="server" Visible="false" CssClass="ed-avatar" AlternateText="Ảnh đại diện" />
            <asp:Label ID="lblAvatarFallback" runat="server" CssClass="ed-avatar-fallback" />
            <div style="min-width:0; flex: 1;">
                <div class="ed-eyebrow">Không gian làm việc</div>
                <h1><asp:Label ID="lblDisplayName" runat="server" Text="Dashboard" /></h1>
                <div class="ed-profile-meta">
                    <asp:PlaceHolder ID="phDepartment" runat="server" Visible="false"><span><i class="fas fa-building" aria-hidden="true"></i> <asp:Label ID="lblDepartment" runat="server" /></span></asp:PlaceHolder>
                    <asp:PlaceHolder ID="phPosition" runat="server" Visible="false"><span><i class="fas fa-id-badge" aria-hidden="true"></i> <asp:Label ID="lblPosition" runat="server" /></span></asp:PlaceHolder>
                </div>
            </div>
        </div>

        <!-- 40% Stats (4 Cards x 10%) -->
        <div class="ed-header-stat" style="--c:#8b5cf6; --soft:#f3efff;"><div class="ed-stat-val"><asp:Label ID="lblProjectCount" runat="server"/></div><div class="ed-stat-lbl">Dự án<br/>tham gia</div><i class="fas fa-layer-group ed-stat-ic"></i></div>
        <div class="ed-header-stat" style="--c:#3b82f6; --soft:#eef6ff;"><div class="ed-stat-val"><asp:Label ID="lblTaskCount" runat="server"/></div><div class="ed-stat-lbl">Công việc<br/>trong tuần</div><i class="fas fa-tasks ed-stat-ic"></i></div>
        <div class="ed-header-stat is-alert" style="--c:#ef4444; --soft:#fff1f0;"><div class="ed-stat-val" style="color:#ef4444"><asp:Label ID="lblOverdueCount" runat="server"/></div><div class="ed-stat-lbl">Việc<br/>quá hạn</div><i class="fas fa-exclamation-triangle ed-stat-ic"></i></div>
        <div class="ed-header-stat" style="--c:#10b981; --soft:#edfbf5;"><div class="ed-stat-val"><asp:Label ID="lblIssueCount" runat="server"/></div><div class="ed-stat-lbl">Vấn đề<br/>được giao</div><i class="fas fa-life-ring ed-stat-ic"></i></div>

        <!-- VÙNG CUỐI HÀNG 1: CHIA 4:6, LỊCH BÊN TRÁI - THỜI GIAN BÊN PHẢI -->
        <div class="ed-header-tools">
            <div class="ed-calendar-entry">
                <button type="button" id="btnToggleWeekCalendar" class="ed-calendar-toggle" aria-controls="edWeekCalendar" aria-expanded="false" data-tooltip="Mở lịch tuần này" aria-label="Mở lịch tuần này" onclick="return edToggleWeekCalendar(this);">
                    <span class="ed-calendar-toggle-icon"><i class="fas fa-calendar-alt" aria-hidden="true"></i></span>
                    <span>Lịch tuần này</span>
                </button>
            </div>
            <div class="ed-header-time">
                <div class="ed-week-label"><i class="far fa-calendar-alt" aria-hidden="true"></i> Tổng quan theo tuần</div>
                <div class="ed-week-title"><asp:Label ID="lblWeekRange" runat="server" /></div>
                <div class="ed-week-actions">
                    <asp:HyperLink ID="lnkPreviousWeek" runat="server" CssClass="ed-week-btn" Text="‹ Trước" />
                    <asp:HyperLink ID="lnkCurrentWeek" runat="server" CssClass="ed-week-btn ed-current" Text="Hiện tại" />
                    <asp:HyperLink ID="lnkNextWeek" runat="server" CssClass="ed-week-btn" Text="Sau ›" />
                </div>
            </div>
        </div>
    </div>

    <!-- LỊCH TUẦN: ĐẶT NGAY DƯỚI HÀNG ĐẦU, MẶC ĐỊNH ĐÓNG -->
    <section id="edWeekCalendar" class="ed-panel ed-calendar-panel ed-week-calendar-panel" aria-labelledby="edCalendarHeading" aria-hidden="true">
        <div class="ed-panel-head">
            <div class="ed-panel-heading"><span class="ed-panel-icon" style="background:#ecfdf5;color:#059669"><i class="far fa-calendar-alt" aria-hidden="true"></i></span><div><h2 id="edCalendarHeading" class="ed-panel-title">Lịch làm việc trong tuần</h2><div class="ed-panel-subtitle">Công việc và cuộc họp</div></div></div>
            <asp:LinkButton ID="lnkCalendarMore" runat="server" CssClass="ed-btn-task" style="padding: 6px 12px; border-radius: 6px; text-decoration:none; font-weight:bold; font-size:12px;" OnClick="lnkCalendar_Click">Lịch chi tiết <i class="fas fa-arrow-right"></i></asp:LinkButton>
        </div>
        <div class="ed-panel-body">
            <asp:Repeater ID="rptDays" runat="server"><HeaderTemplate><div class="ed-days"></HeaderTemplate><ItemTemplate>
                <div class='ed-day <%#: Eval("CssClass") %>'>
                    <div class="ed-day-head"><span class="ed-day-name"><%#: Eval("DayName") %></span><span class="ed-day-date"><%#: Eval("DayNumber") %></span><asp:PlaceHolder ID="phToday" runat="server" Visible='<%# (bool)Eval("IsToday") %>'><span class="ed-today-tag">Hôm nay</span></asp:PlaceHolder></div>
                    <div class='ed-day-type <%#: Eval("DayTypeCss") %>'><%#: Eval("DayType") %></div><div class="ed-day-hours"><i class="far fa-clock" aria-hidden="true"></i> <%#: Eval("WorkHours") %></div>
                    <div class="ed-day-events"><asp:Repeater ID="rptEvents" runat="server" DataSource='<%# Eval("PreviewEvents") %>'><ItemTemplate><div class='ed-event <%#: Eval("CssClass") %>'><span class="ed-event-time"><%#: Eval("TimeLabel") %></span><strong><%#: Eval("Title") %></strong></div></ItemTemplate></asp:Repeater><asp:PlaceHolder ID="phNoEvents" runat="server" Visible='<%# !(bool)Eval("HasEvents") %>'><div class="ed-day-empty">Trống</div></asp:PlaceHolder></div>
                </div>
            </ItemTemplate><FooterTemplate></div></FooterTemplate></asp:Repeater>
        </div>
    </section>

    <!-- 2. CHARTS -->
    <div class="ed-charts-grid">
        <section class="ed-panel" aria-labelledby="edTaskChartHeading">
            <div class="ed-panel-head"><div class="ed-panel-heading"><span class="ed-panel-icon" style="background:#e0e7ff;color:#4f46e5"><i class="fas fa-chart-pie" aria-hidden="true"></i></span><div><h2 id="edTaskChartHeading" class="ed-panel-title">Trạng thái công việc</h2></div></div></div>
            <div class="ed-panel-body">
                <div class="ed-chart-container">
                    <div class="ed-donut-wrapper" id="divTaskChart" runat="server">
                        <div class="ed-donut-center">
                            <asp:Label ID="lblTotalTasksChart" runat="server" CssClass="ed-donut-number" />
                            <span class="ed-donut-label">công việc</span>
                        </div>
                    </div>
                    <div class="ed-chart-legend">
                        <div class="ed-legend-item"><span class="ed-dot" style="background:#ef4444"></span> Trễ hạn <asp:Label ID="lblTaskOverdueChart" runat="server" style="margin-left: auto; color:#1e293b; font-size: 15px; font-weight: 800;"/></div>
                        <div class="ed-legend-item"><span class="ed-dot" style="background:#f59e0b"></span> Sắp đến hạn <asp:Label ID="lblTaskDueSoonChart" runat="server" style="margin-left: auto; color:#1e293b; font-size: 15px; font-weight: 800;"/></div>
                        <div class="ed-legend-item"><span class="ed-dot" style="background:#3b82f6"></span> Trong tiến độ <asp:Label ID="lblTaskNormalChart" runat="server" style="margin-left: auto; color:#1e293b; font-size: 15px; font-weight: 800;"/></div>
                    </div>
                </div>
            </div>
        </section>

        <section class="ed-panel" aria-labelledby="edProjectChartHeading">
            <div class="ed-panel-head"><div class="ed-panel-heading"><span class="ed-panel-icon" style="background:#fae8ff;color:#c026d3"><i class="fas fa-chart-pie" aria-hidden="true"></i></span><div><h2 id="edProjectChartHeading" class="ed-panel-title">Trạng thái dự án</h2></div></div></div>
            <div class="ed-panel-body">
                <div class="ed-chart-container">
                    <div class="ed-donut-wrapper" id="divProjectChart" runat="server">
                        <div class="ed-donut-center">
                            <asp:Label ID="lblTotalProjectsChart" runat="server" CssClass="ed-donut-number" />
                            <span class="ed-donut-label">dự án</span>
                        </div>
                    </div>
                    <div class="ed-chart-legend">
                        <div class="ed-legend-item"><span class="ed-dot" style="background:#ef4444"></span> Trễ hạn <asp:Label ID="lblProjectOverdueChart" runat="server" style="margin-left: auto; color:#1e293b; font-size: 15px; font-weight: 800;"/></div>
                        <div class="ed-legend-item"><span class="ed-dot" style="background:#f59e0b"></span> Sắp đến hạn <asp:Label ID="lblProjectDueSoonChart" runat="server" style="margin-left: auto; color:#1e293b; font-size: 15px; font-weight: 800;"/></div>
                        <div class="ed-legend-item"><span class="ed-dot" style="background:#3b82f6"></span> Trong tiến độ <asp:Label ID="lblProjectNormalChart" runat="server" style="margin-left: auto; color:#1e293b; font-size: 15px; font-weight: 800;"/></div>
                    </div>
                </div>
            </div>
        </section>
    </div>

    <!-- 3. MAIN LAYOUT (DỰ ÁN BÊN TRÁI - CÔNG VIỆC BÊN PHẢI) -->
    <asp:UpdatePanel ID="upnlMainLayout" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="ed-layout">
                
                <!-- PROJECTS (MÀU VÀNG) -->
                <section class="ed-panel ed-panel-project" aria-labelledby="edProjectsHeading">
                    <div class="ed-panel-head">
                        <div class="ed-panel-heading"><span class="ed-panel-icon"><i class="fas fa-folder-open" aria-hidden="true"></i></span><div><h2 id="edProjectsHeading" class="ed-panel-title">Dự án của tôi</h2><div class="ed-panel-subtitle">Gồm các dự án có công việc chưa xong</div></div></div>
                        <span class="ed-count"><asp:Label ID="lblProjectPanelCount" runat="server" Text="0" /></span>
                    </div>
                    <div class="ed-panel-body">
                        <asp:Repeater ID="rptProjects" runat="server" OnItemCommand="rptProjects_ItemCommand"><HeaderTemplate><div class="ed-project-list"></HeaderTemplate><ItemTemplate>
                        <div class="ed-card-item ed-project">
                            <div class="ed-project-icon"><i class="fas fa-folder" aria-hidden="true"></i></div>
                            <div class="ed-project-content">
                                <div class="ed-project-code"><%#: Eval("Code") %></div>
                                <asp:LinkButton ID="lnkProject" runat="server" CommandName="FILTER_TASKS" CommandArgument='<%# Eval("Id") %>' CssClass="ed-project-name" ToolTip="Lọc công việc thuộc dự án này">
                                    <%#: Eval("Name") %>
                                </asp:LinkButton>
                                <div class="ed-project-meta"><span><%#: Eval("TaskSummary") %></span><span><%#: Eval("Deadline") %></span></div>
                                <div class="ed-project-progress"><div class="ed-progress"><span style='width:<%#: Eval("Progress") %>%'></span></div></div>
                                <div class="ed-progress-meta"><span>Tiến độ</span><strong><%#: Eval("Progress") %>%</strong></div>
                            </div>
                        </div>
                        </ItemTemplate><FooterTemplate></div></FooterTemplate></asp:Repeater>
                        
                        <asp:Panel ID="pnlNoProjects" runat="server" Visible="false" CssClass="ed-empty"><strong>Chưa có dự án</strong>Bạn đã hoàn thành xong mọi công việc.</asp:Panel>
                        
                        <asp:LinkButton ID="lnkMoreProjects" runat="server" CssClass="ed-more-btn ed-btn-project" OnClick="lnkMoreProjects_Click">
                            <i class="fas fa-th-list"></i> Xem chi tiết
                        </asp:LinkButton>
                    </div>
                </section>

                <!-- TASKS (MÀU XANH) -->
                <section class="ed-panel ed-panel-task" aria-labelledby="edTasksHeading">
                    <div class="ed-panel-head">
                        <div class="ed-panel-heading">
                            <span class="ed-panel-icon"><i class="fas fa-bolt" aria-hidden="true"></i></span>
                            <div>
                                <h2 id="edTasksHeading" class="ed-panel-title">Công việc cần theo dõi</h2>
                                <div class="ed-panel-subtitle">
                                    <asp:LinkButton ID="btnAttentionTasks" runat="server" CssClass="ed-tab-btn active" OnClick="btnAttentionTasks_Click"><i class="fas fa-star"></i> Cần chú ý</asp:LinkButton>
                                    <asp:Label ID="lblProjectFilterName" runat="server" CssClass="ed-tab-btn filtered" Visible="false" />
                                </div>
                            </div>
                        </div>
                        <span class="ed-count"><asp:Label ID="lblTaskPanelCount" runat="server" Text="0" /></span>
                    </div>
                    <div class="ed-panel-body">
                        <asp:Repeater ID="rptTasks" runat="server"><HeaderTemplate><div class="ed-list"></HeaderTemplate><ItemTemplate>
                        <article class='ed-card-item ed-task <%#: Eval("StatusCss").ToString() == "overdue" ? "is-overdue" : String.Empty %>'>
                            <div class="ed-task-top">
                                <div style="min-width:0;flex:1">
                                    <div class="ed-kicker"><%#: Eval("ProjectCode") %> · <%#: Eval("TaskCode") %></div>
                                    <h3 class="ed-task-name"><%#: Eval("TaskName") %></h3>
                                    <div class="ed-task-dates"><i class="far fa-calendar-alt" aria-hidden="true"></i><%#: Eval("DateRange") %></div>
                                </div>
                                <span class='ed-badge <%#: Eval("StatusCss") %>'><%#: Eval("StatusText") %></span>
                            </div>
                            <div class="ed-progress"><span style='width:<%#: Eval("Progress") %>%'></span></div>
                            <div class="ed-progress-meta"><span>Tiến độ</span><strong><%#: Eval("Progress") %>%</strong></div>
                        </article>
                        </ItemTemplate><FooterTemplate></div></FooterTemplate></asp:Repeater>
                        
                        <asp:Panel ID="pnlNoTasks" runat="server" Visible="false" CssClass="ed-empty"><strong><i class="fas fa-check-circle" aria-hidden="true"></i> Tuyệt vời!</strong>Bạn không có công việc nào ở mục này.</asp:Panel>
                        
                        <asp:LinkButton ID="lnkMoreTasks" runat="server" CssClass="ed-more-btn ed-btn-task" OnClick="lnkMoreTasks_Click">
                            <i class="fas fa-th-list"></i> Xem chi tiết
                        </asp:LinkButton>
                    </div>
                </section>
                
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <!-- INSIGHTS: ISSUES, RISKS (BỎ THÔNG BÁO) -->
    <div class="ed-insights-grid">
        <section class="ed-panel" aria-labelledby="edIssuesHeading">
            <div class="ed-panel-head"><div class="ed-panel-heading"><span class="ed-panel-icon" style="background:#fff7ed;color:#ea580c"><i class="fas fa-life-ring" aria-hidden="true"></i></span><div><h2 id="edIssuesHeading" class="ed-panel-title">Vấn đề</h2></div></div><span class="ed-count"><asp:Label ID="lblIssuePanelCount" runat="server" Text="0" /></span></div>
            <div class="ed-panel-body"><asp:Repeater ID="rptIssues" runat="server"><HeaderTemplate><div></HeaderTemplate><ItemTemplate>
                <div class="ed-card-item ed-issue"><h3 class="ed-item-title"><%#: Eval("Code") %> · <%#: Eval("Name") %></h3><div class="ed-item-meta"><span><%#: Eval("ProjectName") %></span><span class="ed-chip impact"><%#: Eval("Impact") %></span></div><div class="ed-item-meta"><span><%#: Eval("StatusText") %></span></div></div>
            </ItemTemplate><FooterTemplate></div></FooterTemplate></asp:Repeater><asp:Panel ID="pnlNoIssues" runat="server" Visible="false" CssClass="ed-empty">Trống</asp:Panel>
            <div class="ed-modal-open-trigger"><asp:UpdatePanel ID="upnlOpenIssues" runat="server" UpdateMode="Conditional"><ContentTemplate><asp:LinkButton ID="lnkMoreIssues" runat="server" CssClass="ed-more-btn" style="background:#fff7ed; color:#ea580c; border: 1px dashed #fed7aa;" OnClick="lnkMoreIssues_Click"><i class="fas fa-th-list"></i> Xem chi tiết</asp:LinkButton></ContentTemplate></asp:UpdatePanel></div></div>
        </section>

        <section class="ed-panel" aria-labelledby="edRisksHeading">
            <div class="ed-panel-head"><div class="ed-panel-heading"><span class="ed-panel-icon" style="background:#fef3c7;color:#d97706"><i class="fas fa-flag" aria-hidden="true"></i></span><div><h2 id="edRisksHeading" class="ed-panel-title">Rủi ro</h2></div></div><span class="ed-count"><asp:Label ID="lblRiskPanelCount" runat="server" Text="0" /></span></div>
            <div class="ed-panel-body"><asp:Repeater ID="rptRisks" runat="server"><HeaderTemplate><div></HeaderTemplate><ItemTemplate>
                <div class="ed-card-item ed-risk"><h3 class="ed-item-title"><%#: Eval("Name") %></h3><div class="ed-item-meta"><span><%#: Eval("ProjectName") %></span><span>Điểm: <%#: Eval("Score") %></span></div><div class="ed-risk-grid"><span class="ed-risk-score"><%#: Eval("Probability") %></span><span class="ed-risk-score"><%#: Eval("Impact") %></span></div></div>
            </ItemTemplate><FooterTemplate></div></FooterTemplate></asp:Repeater><asp:Panel ID="pnlNoRisks" runat="server" Visible="false" CssClass="ed-empty">Trống</asp:Panel>
            <div class="ed-modal-open-trigger"><asp:UpdatePanel ID="upnlOpenRisks" runat="server" UpdateMode="Conditional"><ContentTemplate><asp:LinkButton ID="lnkMoreRisks" runat="server" CssClass="ed-more-btn" style="background:#fef3c7; color:#d97706; border: 1px dashed #fde68a;" OnClick="lnkMoreRisks_Click"><i class="fas fa-th-list"></i> Xem chi tiết</asp:LinkButton></ContentTemplate></asp:UpdatePanel></div></div>
        </section>
    </div>

    </div>

    <div class="ed-modal-host">
    <!-- ExtraModal nằm ngoài .ed-dashboard để CSS của dashboard không lọt vào popup. -->
    <SweetSoft:ExtraModal ID="mdlTasks" runat="server" Title="Tất cả công việc cần theo dõi" Width="80%">
        <ContentTemplate>
            <asp:UpdatePanel ID="upnlModalTasks" runat="server" UpdateMode="Conditional"><ContentTemplate>
                <asp:Repeater ID="rptTasksAll" runat="server"><HeaderTemplate><div class="table-responsive"><table class="table table-striped table-hover"><thead><tr><th>Công việc</th><th>Thời gian</th><th>Trạng thái</th><th>Tiến độ</th></tr></thead><tbody></HeaderTemplate><ItemTemplate>
                    <tr><td><strong><%#: Eval("TaskName") %></strong><br /><small><%#: Eval("ProjectCode") %> · <%#: Eval("TaskCode") %></small></td><td><%#: Eval("DateRange") %></td><td><%#: Eval("StatusText") %></td><td><%#: Eval("Progress") %>%</td></tr>
                </ItemTemplate><FooterTemplate></tbody></table></div></FooterTemplate></asp:Repeater>
                <asp:Panel ID="pnlNoAllTasks" runat="server" Visible="false">Không có công việc nào.</asp:Panel>
            </ContentTemplate></asp:UpdatePanel>
        </ContentTemplate>
    </SweetSoft:ExtraModal>

    <SweetSoft:ExtraModal ID="mdlProjects" runat="server" Title="Tất cả dự án có công việc chưa hoàn thành" Width="80%">
        <ContentTemplate>
            <asp:UpdatePanel ID="upnlModalProjects" runat="server" UpdateMode="Conditional"><ContentTemplate>
                <asp:Repeater ID="rptProjectsAll" runat="server" OnItemCommand="rptProjectsAll_ItemCommand"><HeaderTemplate><div class="table-responsive"><table class="table table-striped table-hover"><thead><tr><th>Mã dự án</th><th>Tên dự án</th><th>Công việc</th><th>Thời hạn</th><th>Tiến độ</th></tr></thead><tbody></HeaderTemplate><ItemTemplate>
                    <tr><td><%#: Eval("Code") %></td><td><asp:LinkButton ID="lnkProjectModal" runat="server" CommandName="FILTER_TASKS" CommandArgument='<%# Eval("Id") %>' ToolTip="Lọc công việc thuộc dự án này"><%#: Eval("Name") %></asp:LinkButton></td><td><%#: Eval("TaskSummary") %></td><td><%#: Eval("Deadline") %></td><td><%#: Eval("Progress") %>%</td></tr>
                </ItemTemplate><FooterTemplate></tbody></table></div></FooterTemplate></asp:Repeater>
            </ContentTemplate></asp:UpdatePanel>
        </ContentTemplate>
    </SweetSoft:ExtraModal>

    <SweetSoft:ExtraModal ID="mdlCalendar" runat="server" Title="Lịch làm việc chi tiết" Width="80%">
        <ContentTemplate>
            <asp:UpdatePanel ID="upnlModalCalendar" runat="server" UpdateMode="Conditional"><ContentTemplate>
                <asp:Repeater ID="rptCalendarAll" runat="server"><HeaderTemplate><div class="table-responsive"><table class="table table-striped table-hover"><thead><tr><th>Ngày</th><th>Loại ngày</th><th>Lịch công việc / cuộc họp</th></tr></thead><tbody></HeaderTemplate><ItemTemplate>
                    <tr><td><strong><%#: Eval("DayName") %> · <%#: Eval("DayNumber") %></strong></td><td><%#: Eval("DayType") %></td><td><asp:Repeater ID="rptEventsAll" runat="server" DataSource='<%# Eval("Events") %>'><ItemTemplate><div><strong><%#: Eval("TimeLabel") %></strong> — <%#: Eval("Title") %><small> <%#: Eval("Subtitle") %></small></div></ItemTemplate></asp:Repeater></td></tr>
                </ItemTemplate><FooterTemplate></tbody></table></div></FooterTemplate></asp:Repeater>
            </ContentTemplate></asp:UpdatePanel>
        </ContentTemplate>
    </SweetSoft:ExtraModal>

    <SweetSoft:ExtraModal ID="mdlIssues" runat="server" Title="Tất cả vấn đề" Width="80%">
        <ContentTemplate>
            <asp:UpdatePanel ID="upnlModalIssues" runat="server" UpdateMode="Conditional"><ContentTemplate>
                <asp:Repeater ID="rptIssuesAll" runat="server"><HeaderTemplate><div class="table-responsive"><table class="table table-striped table-hover"><thead><tr><th>Mã vấn đề</th><th>Tên vấn đề</th><th>Dự án</th><th>Mức ảnh hưởng</th><th>Trạng thái</th></tr></thead><tbody></HeaderTemplate><ItemTemplate>
                    <tr><td><%#: Eval("Code") %></td><td><%#: Eval("Name") %></td><td><%#: Eval("ProjectName") %></td><td><%#: Eval("Impact") %></td><td><%#: Eval("StatusText") %></td></tr>
                </ItemTemplate><FooterTemplate></tbody></table></div></FooterTemplate></asp:Repeater>
            </ContentTemplate></asp:UpdatePanel>
        </ContentTemplate>
    </SweetSoft:ExtraModal>

    <SweetSoft:ExtraModal ID="mdlRisks" runat="server" Title="Tất cả rủi ro" Width="80%">
        <ContentTemplate>
            <asp:UpdatePanel ID="upnlModalRisks" runat="server" UpdateMode="Conditional"><ContentTemplate>
                <asp:Repeater ID="rptRisksAll" runat="server"><HeaderTemplate><div class="table-responsive"><table class="table table-striped table-hover"><thead><tr><th>Tên rủi ro</th><th>Dự án</th><th>Xác suất</th><th>Mức ảnh hưởng</th><th>Điểm rủi ro</th></tr></thead><tbody></HeaderTemplate><ItemTemplate>
                    <tr><td><%#: Eval("Name") %></td><td><%#: Eval("ProjectName") %></td><td><%#: Eval("Probability") %></td><td><%#: Eval("Impact") %></td><td><%#: Eval("Score") %></td></tr>
                </ItemTemplate><FooterTemplate></tbody></table></div></FooterTemplate></asp:Repeater>
            </ContentTemplate></asp:UpdatePanel>
        </ContentTemplate>
    </SweetSoft:ExtraModal>
    </div>

<script type="text/javascript">
    function edToggleWeekCalendar(button) {
        if (!button) return false;
        var calendar = document.getElementById("edWeekCalendar");
        if (!calendar) return false;

        var isOpen = calendar.classList.contains("is-open");
        if (isOpen) {
            calendar.classList.remove("is-open");
            calendar.setAttribute("aria-hidden", "true");
            button.setAttribute("aria-expanded", "false");
            button.setAttribute("data-tooltip", "Mở lịch tuần này");
            button.setAttribute("aria-label", "Mở lịch tuần này");
        } else {
            calendar.classList.add("is-open");
            calendar.setAttribute("aria-hidden", "false");
            button.setAttribute("aria-expanded", "true");
            button.setAttribute("data-tooltip", "Đóng lịch tuần này");
            button.setAttribute("aria-label", "Đóng lịch tuần này");
        }
        return false;
    }
</script>
