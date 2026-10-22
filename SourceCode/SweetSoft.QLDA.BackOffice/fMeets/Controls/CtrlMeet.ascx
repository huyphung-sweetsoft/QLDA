<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlMeet.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fMeets.Controls.CtrlMeet" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fMeets/Controls/CtrlXemNhanVienMeet.ascx" TagPrefix="SweetSoft" TagName="CtrlXemNhanVienMeet" %>
<%@ Register Src="~/fMeets/Controls/CtrlViewMeetDetail.ascx" TagPrefix="SweetSoft" TagName="CtrlViewMeetDetail" %>
<%@ Register Src="~/fMeets/Controls/CtrlMeetGioCongTab.ascx" TagPrefix="SweetSoft" TagName="CtrlMeetGioCongTab" %>
<style>
    .avatar-group { display: inline-flex !important; align-items: center; justify-content: center; gap: 6px !important; flex-wrap: nowrap !important; white-space: nowrap !important; }
    .avatar-stack-container { display: flex; align-items: center; min-width: 0; }
    .avatar-circle { width: 28px; height: 28px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 700; color: #ffffff; border: 2px solid #ffffff; margin-left: -8px; position: relative; z-index: 1; box-shadow: 0 1px 2px rgba(0,0,0,0.1); }
    .avatar-circle:first-child { margin-left: 0; }
    .avatar-more { background-color: #f1f5f9; color: #475569; border-color: #cbd5e1; z-index: 0; font-weight: 800; font-size: 10px; }
    .btn-assign-task { width: 28px; height: 28px; border-radius: 7px; background-color: #2563eb; color: #ffffff; display: flex; align-items: center; justify-content: center; border: none; cursor: pointer; text-decoration: none; font-size: 12px; transition: background 0.2s, transform 0.15s, box-shadow 0.15s; flex-shrink: 0; }
    .btn-assign-task:hover { background-color: #1d4ed8; color: #ffffff; transform: translateY(-1px); box-shadow: 0 3px 8px rgba(37,99,235,0.2); }
    .btn-assign-task.view-only { background-color: #64748b; }
    .btn-assign-task.view-only:hover { background-color: #475569; box-shadow: 0 3px 8px rgba(71,85,105,0.2); }
    .meeting-code-cell { display: inline-flex; align-items: center; justify-content: center; min-width: 78px; padding: 5px 10px; border: 1px solid #dbe3ee; border-radius: 7px; background: #f8fafc; color: #334155; font-size: 12px; font-weight: 700; letter-spacing: .2px; white-space: nowrap; }
    .meeting-name-cell { display: flex; align-items: center; width: 100%; min-width: 0; text-align: left; }
    .meeting-name-text { min-width: 0; width: 100%; }
    /* =======================================================
       CSS TÊN CUỘC HỌP (ĐỒNG BỘ GIỐNG ẢNH: CHỮ TÍM, GỌN GÀNG)
       ======================================================= */
    .meeting-name-main { display: block; width: 100%; color: #542e88 !important; font-size: 16px; font-weight: 500; line-height: 1.45; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; text-decoration: none !important; transition: color .15s ease; }
    .meeting-name-main:hover { color: #3b82f6 !important; text-decoration: none !important; }
    .meeting-name-sub { margin-top: 2px; color: #94a3b8; font-size: 11px; line-height: 1.2; }
    .meeting-datetime { display: inline-flex; flex-direction: column; align-items: center; justify-content: center; gap: 3px; min-width: 104px; line-height: 1.25; }
    .meeting-date { color: #334155; font-size: 12px; font-weight: 600; white-space: nowrap; }
    .meeting-time { color: #64748b; font-size: 11px; white-space: nowrap; }
    .meeting-time i { font-size: 10px; }
    .meeting-room-cell { display: flex; align-items: center; justify-content: flex-start; gap: 8px; max-width: 220px; color: #475569; font-size: 12px; line-height: 1.35; text-align: left; min-width: 0; }
    .meeting-room-icon { width: 28px; height: 28px; display: inline-flex; align-items: center; justify-content: center; flex: 0 0 28px; border-radius: 7px; background: #f8fafc; border: 1px solid #e2e8f0; color: #64748b; font-size: 12px; }
    .meeting-room-text { min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .meeting-status-badge { display: inline-flex; align-items: center; justify-content: center; gap: 7px; min-width: 110px; padding: 6px 11px; border: 1px solid transparent; border-radius: 999px; font-size: 11px; font-weight: 700; line-height: 1.2; white-space: nowrap; transition: all 0.2s ease; }
    .meeting-status-badge::before { content: ""; width: 7px; height: 7px; border-radius: 50%; background: currentColor; flex: 0 0 7px; }
    .meeting-status-0 { color: #2563eb; background: #eff6ff; border-color: #bfdbfe; }
    .meeting-status-1 { color: #d97706; background: #fffbeb; border-color: #fde68a; }
    .meeting-status-2 { color: #16a34a; background: #f0fdf4; border-color: #bbf7d0; }
    .meeting-status-3 { color: #64748b; background: #f8fafc; border-color: #cbd5e1; }
    .meeting-action-group { display: inline-flex; align-items: center; justify-content: center; gap: 5px; }
    .meeting-action-group .btn-grid-action { width: 32px; min-width: 32px; height: 32px; display: inline-flex; align-items: center; justify-content: center; border-radius: 7px; }
    .meeting-action-group .btn-smart-link { width: 32px; min-width: 32px; height: 32px; display: inline-flex; align-items: center; justify-content: center; border-radius: 7px; }
    .meeting-table-cell { vertical-align: middle !important; }


    @media (max-width: 991.98px) {
        .meeting-name-main { font-size: 16px; }
        .meeting-status-badge { min-width: 100px; padding: 5px 9px; }
        .meeting-room-cell { max-width: 160px; }
    }
    /* MONTH / WEEK MEETING CALENDAR — each CSS rule occupies one line. */
    .meet-view-shell .meet-toolbar-row { display:flex;align-items:center;justify-content:space-between;gap:10px;flex-wrap:wrap; }
    .meet-view-shell .meet-list-filters { display:flex;align-items:center;flex:1 1 520px;gap:12px;flex-wrap:wrap;min-width:0; }
    .meet-view-shell .meet-list-filters .input-group { flex:1 1 270px;max-width:500px;min-width:230px; }
    .meet-view-shell .meet-common-actions { display:flex;align-items:center;justify-content:flex-end;gap:8px;flex:0 0 auto;margin-left:auto; }
    .meet-view-shell .meet-common-actions .meet-view-switcher,.meet-view-shell .meet-cal-toolbar-actions .meet-view-switcher { display:inline-flex;align-items:center;flex-shrink:0;gap:3px;padding:3px;border:1px solid #dfd1f0;border-radius:9px;background:#f8f5fd; }
    .meet-view-shell .meet-view-button { display:inline-flex;align-items:center;justify-content:center;gap:7px;min-height:33px;padding:6px 12px;border:1px solid transparent;border-radius:7px;background:transparent;color:#654b83;font-size:12.5px;font-weight:700;line-height:1.2;white-space:nowrap;cursor:pointer; }
    .meet-view-shell .meet-view-button:hover:not(.is-active) { background:#ede4f9;color:#4a187c; }
    .meet-view-shell .meet-view-button.is-active { border-color:#54218d;background:#54218d;color:#fff;cursor:default; }
    .meet-view-shell .meet-view-button:focus-visible { outline:2px solid #9f74cb;outline-offset:2px; }
     .meet-view-shell.meet-mode-calendar .meet-panel-list,.meet-view-shell.meet-mode-labor .meet-panel-list { display:none; }
    .meet-view-shell.meet-mode-list .meet-panel-calendar,.meet-view-shell.meet-mode-labor .meet-panel-calendar { display:none; }
    .meet-view-shell.meet-mode-list .meet-panel-labor,.meet-view-shell.meet-mode-calendar .meet-panel-labor { display:none; }
    .meet-panel-labor { min-width:0;width:100%; }
    .meet-display-stage { position:relative;min-height:180px; }
    .meet-view-panel { width:100%; }
    /* Tab Gio cong khong duoc day rong container chung cua Danh sach/Lich. */
    .meet-view-shell { width:100%;max-width:100%;min-width:0;box-sizing:border-box; }
    .meet-view-shell .meet-display-stage { width:100%;max-width:100%;min-width:0;box-sizing:border-box; }
    .meet-view-shell .meet-display-stage>.meet-view-panel { width:100%;max-width:100%;min-width:0;box-sizing:border-box; }
    .meet-view-shell.meet-mode-labor .meet-panel-labor { overflow-x:clip; }

    .meet-calendar-area { padding:13px 14px 16px;background:#fff; }
    .meet-cal-toolbar { display:flex;flex-wrap:wrap;align-items:center;justify-content:space-between;gap:9px;margin-bottom:12px; }
    .meet-cal-heading { display:flex;align-items:center;gap:10px;min-width:0; }
    .meet-cal-heading-icon { display:grid;place-items:center;width:39px;height:39px;border:1px solid #e5d9f3;border-radius:11px;background:#f4eefe;color:#54218d;font-size:17px; }
    .meet-cal-period { display:block;color:#322048;font-size:20px;font-weight:800;line-height:1.35; }
    .meet-cal-subtitle { display:block;margin-top:1px;color:#7d718b;font-size:11.5px; }
    .meet-cal-navigation { display:flex;align-items:center;gap:6px;flex-wrap:wrap; }
    .meet-cal-navigation .meet-cal-nav,.meet-cal-navigation .meet-cal-period-btn { display:inline-flex;align-items:center;justify-content:center;gap:6px;min-width:35px;min-height:34px;padding:6px 10px;border:1px solid #dfd4ed;border-radius:8px;background:#fff;color:#55347a;font-size:12px;font-weight:750;text-decoration:none;transition:background .15s,border-color .15s; }
    .meet-cal-navigation .meet-cal-nav:hover,.meet-cal-navigation .meet-cal-period-btn:hover { background:#f5effe;border-color:#c6addf;color:#54218d; }
    .meet-cal-navigation .meet-cal-period-btn.is-active { background:#54218d;border-color:#54218d;color:#fff; }
    .meet-cal-legend { display:flex;align-items:center;gap:12px;flex-wrap:wrap;margin-bottom:10px;padding:8px 11px;border:1px solid #eae5f1;border-radius:9px;background:#faf8fe;color:#665974;font-size:11.5px; }
    .meet-cal-legend>span { display:inline-flex;align-items:center;gap:6px; }
    .meet-cal-legend .meet-cal-dot { display:inline-block;width:9px;height:9px;border-radius:50%;background:#6a35a9; }
    .meet-cal-legend .meet-cal-dot.pm { background:#188a88; }
    .meet-cal-legend .meet-cal-dot.off { background:#e08d55; }
    .meet-cal-scroll { overflow-x:auto;padding:1px 1px 16px;scrollbar-width:thin; }
    .meet-calendar-grid { display:grid;grid-template-columns:repeat(7,minmax(0,1fr));min-width:870px;border:1px solid #e6e1ee;border-radius:11px;background:#e6e1ee;gap:1px;isolation:isolate; }
    .meet-cal-dow { padding:10px 4px;background:#f2ecfa;color:#54218d;font-size:12px;font-weight:800;text-align:center; }
    .meet-cal-day { position:relative;min-width:0;min-height:145px;padding:8px 7px;background:#fff; }
    .meet-calendar-grid.is-week .meet-cal-day { min-height:345px; }
    .meet-cal-day.is-outside { background:#faf9fc; }
    .meet-cal-day.is-off { background:#fff9f5; }
    .meet-cal-day.is-today { background:#faf6ff;box-shadow:inset 0 0 0 2px #af8cdb;z-index:1; }
    .meet-cal-day-top { display:flex;align-items:center;justify-content:space-between;gap:3px;min-height:23px;margin-bottom:5px; }
    .meet-cal-day-number { display:inline-grid;place-items:center;width:27px;height:27px;border-radius:7px;color:#40334f;font-size:12.5px;font-weight:800; }
    .meet-cal-day.is-today .meet-cal-day-number { background:#54218d;color:#fff; }
    .meet-cal-day.is-outside .meet-cal-day-number { color:#a5a1ae; }
    .meet-cal-day-badge { display:inline-flex;align-items:center;gap:3px;max-width:93px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;padding:2px 5px;border-radius:5px;background:#fff0df;color:#a05e21;font-size:9.5px;font-weight:750; }
    /* Ngày chỉ làm một ca: nửa nghỉ được tô rất nhẹ, vẫn giữ đầy đủ nội dung cuộc họp. */
    .meet-cal-day.is-morning-only { background:linear-gradient(to bottom,#fff 0%,#fff 49%,#fff7ef 49%,#fff7ef 100%); }
    .meet-cal-day.is-afternoon-only { background:linear-gradient(to bottom,#fff7ef 0%,#fff7ef 49%,#fff 49%,#fff 100%); }
    .meet-cal-day.is-morning-only:hover,.meet-cal-day.is-afternoon-only:hover { box-shadow:inset 0 0 0 2px #d0b7ed; }
    .meet-cal-day.is-today.is-morning-only,.meet-cal-day.is-today.is-afternoon-only { box-shadow:inset 0 0 0 2px #af8cdb; }
    .meet-cal-day.is-today.is-morning-only:hover,.meet-cal-day.is-today.is-afternoon-only:hover { box-shadow:inset 0 0 0 2px #7440b1; }
    .meet-cal-day-badge.is-half-day { max-width:124px;border:1px solid #f2d6b4;background:#fff0df;color:#945516;font-size:9.5px;font-weight:750; }
    .meet-cal-day-badge.is-half-day i { font-size:10px; }
    .meet-cal-shift { margin-top:5px;padding:5px;border:1px solid #eee9f5;border-radius:6px;background:#fbfaff; }
    .meet-cal-shift.pm { background:#f4fafb;border-color:#e1eff0; }
    .meet-cal-shift.off { opacity:.72;background:#faf7f5; }
    .meet-cal-shift-title { display:flex;justify-content:space-between;gap:2px;margin-bottom:4px;color:#78668b;font-size:10px;font-weight:800; }
    .meet-cal-shift.pm .meet-cal-shift-title { color:#247d83; }
    .meet-cal-shift-count { color:#9b8eaa;font-weight:650; }
    .meet-cal-empty { display:block;color:#a7a1ad;font-size:10px;line-height:1.5; }
    .meet-cal-event { position:relative;display:flex;align-items:center;gap:4px;width:100%;min-height:23px;margin-top:3px;padding:3px 22px 3px 5px;border:1px solid #e1d5f3;border-left:3px solid #6730a5;border-radius:5px;background:#f0eafa;color:#4c237c;font-size:10.5px;font-weight:700;text-align:left;cursor:pointer; }
    .meet-cal-shift.pm .meet-cal-event { border-color:#ccece8;border-left-color:#168a8c;background:#e4f7f5;color:#156d70; }
    .meet-cal-event:hover,.meet-cal-event:focus-visible { z-index:6;filter:brightness(.98);outline:2px solid #ccb4ed;outline-offset:1px; }
    .meet-cal-event-text { display:block;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;min-width:0; }
    .meet-cal-event-time { flex:0 0 auto;font-weight:800; }
    .meet-cal-popover { display:none;position:absolute;top:calc(100% + 7px);left:0;width:260px;padding:12px;border:1px solid #d8c7ec;border-radius:10px;background:#fff;color:#3d3350;text-align:left;box-shadow:0 12px 27px rgba(40,23,66,.19);white-space:normal;pointer-events:none;z-index:100; }
    .meet-cal-event:hover .meet-cal-popover,.meet-cal-event:focus-visible .meet-cal-popover { display:block; }
    .meet-cal-day:nth-child(7n) .meet-cal-popover,.meet-cal-day:nth-child(7n - 1) .meet-cal-popover { left:auto;right:0; }
    .meet-cal-popover strong { display:block;color:#50218b;font-size:12px;line-height:1.4;margin-bottom:7px; }
    .meet-cal-popover span { display:block;margin-top:4px;font-size:11px;font-weight:500;line-height:1.4; }
    .meet-cal-popover small { display:block;margin-top:8px;padding-top:7px;border-top:1px solid #eee7f4;color:#726b7e;font-size:10px;line-height:1.4; }
    .meet-cal-more { display:inline-flex;margin-top:4px;padding:1px 3px;border:0;background:transparent;color:#6639a1;font-size:10px;font-weight:800;cursor:pointer; }
    .meet-cal-hidden { display:none; }
    .meet-cal-day.is-expanded .meet-cal-hidden { display:flex; }
    .meet-cal-off-hours { margin-top:5px;font-size:10px;color:#99663a; }
    .meet-cal-empty-state { padding:30px 14px;text-align:center;color:#8b8396; }
    .meet-cal-warning { padding:8px 11px;margin:0 0 10px;border:1px solid #eed5aa;border-radius:8px;background:#fff9e9;color:#805824;font-size:12px;font-weight:600; }
    @media(max-width:767.98px) {
        .meet-calendar-area { padding:10px 8px 12px; }
        .meet-cal-heading-icon { width:34px;height:34px; }
        .meet-cal-period { font-size:17px; }
        .meet-cal-day { min-height:140px; }
        .meet-cal-toolbar { gap:7px; }
        .meet-cal-legend { gap:7px; }
        .meet-view-shell .meet-list-filters { flex-basis:100%; }
        .meet-view-shell .meet-list-filters .input-group { max-width:100%; }
        .meet-view-shell .meet-common-actions { margin-left:auto; }
    }
    @media(prefers-reduced-motion:reduce) {
        .meet-view-shell .meet-view-button,.meet-cal-navigation .meet-cal-nav,.meet-cal-navigation .meet-cal-period-btn { transition:none; }
    }
    /* Month/week calendar: hai cuộc họp mỗi ngày, các cuộc họp còn lại dùng ExtraModal. */
    .meet-calendar-grid.is-month .meet-cal-day { min-height:130px;transition:background-color .18s ease,box-shadow .18s ease; }
    .meet-calendar-grid.is-week .meet-cal-day { min-height:185px;transition:background-color .18s ease,box-shadow .18s ease; }
    .meet-cal-day.has-meetings { cursor:pointer; }
    .meet-cal-day:hover { z-index:2;background:#f9f5ff;box-shadow:inset 0 0 0 2px #d0b7ed; }
    .meet-cal-day.is-today:hover { box-shadow:inset 0 0 0 2px #7440b1; }
    .meet-cal-day.is-off:hover { background:#fff3eb; }
    .meet-cal-day-tools { display:flex;align-items:center;gap:5px;min-width:0; }
    .meet-cal-create { position:relative;display:inline-flex;align-items:center;justify-content:center;flex:0 0 25px;width:25px;height:25px;padding:0;border:1px solid #d9c6f1;border-radius:7px;background:#f4ecff;color:#6430a0;font-size:12px;opacity:0;pointer-events:none;transform:translateY(3px);cursor:pointer;transition:opacity .18s ease,visibility .18s ease,transform .18s ease,background .18s ease,border-color .18s ease; }
    .meet-cal-day:hover .meet-cal-create,.meet-cal-day:focus-within .meet-cal-create,.meet-cal-create:focus-visible { opacity:1;pointer-events:auto;border-color:#a57bd9;background:#e9dcfb;transform:translateY(0); }
    .meet-cal-create:focus-visible { outline:2px solid #7950b9;outline-offset:2px; }
    .meet-cal-create::after { content:'Tạo cuộc họp';position:absolute;left:50%;bottom:calc(100% + 8px);transform:translate(-50%,4px);padding:7px 11px;border-radius:8px;background:#4f158d;color:#fff;box-shadow:0 6px 18px rgba(61,26,114,.19);white-space:nowrap;font-size:11.5px;font-weight:750;opacity:0;pointer-events:none;transition:opacity .15s ease,transform .15s ease;z-index:30; }
    .meet-cal-create:hover::after,.meet-cal-create:focus-visible::after { opacity:1;transform:translate(-50%,0); }
    @media(hover:none) {
        .meet-cal-create { opacity:1;pointer-events:auto;transform:none; }
    }
    .meet-cal-day:focus-visible { outline:2px solid #7946b6;outline-offset:-2px; }
    .meet-cal-day-items { display:flex;flex-direction:column;gap:5px;min-width:0;margin-top:8px; }
    .meet-cal-day-items .meet-cal-event { margin:0;min-height:29px;padding:5px 7px;font-size:11px; }
    .meet-cal-event { margin-top:0;transition:background-color .15s,border-color .15s; }
    /* Người tạo chỉ nhận vương miện vàng; màu nền/viền luôn lấy theo trạng thái cuộc họp. */
    .meet-cal-event.is-hosted { position:relative; }
    .meet-cal-event .meet-cal-host-icon { color:#d69a19;font-size:11px;flex:0 0 auto;filter:none; }
    /* 0: Đã lên lịch — tím SweetSoft. */
    .meet-cal-event.is-status-0 { border-color:#d6c2ed;border-left-color:#6431a0;background:#f2ecfa;color:#52227e; }
    /* 1: Sắp diễn ra — vẫn tím, có chấm nhịp nhẹ. */
    .meet-cal-event.is-status-1 { border-color:#c6a6ed;border-left-color:#8247c3;background:#efe5ff;color:#542083; }
    /* 2: Đang diễn ra — xanh lá, chấm nhịp nhẹ để dễ nhận biết. */
    .meet-cal-event.is-status-2 { border-color:#a6e8c3;border-left-color:#16a465;background:#eafaf0;color:#167746; }
    /* 3: Đã kết thúc — xám trung tính. */
    .meet-cal-event.is-status-3 { border-color:#d8dfe8;border-left-color:#94a3b8;background:#f3f5f8;color:#637287; }
    .meet-cal-event.is-status-3:hover,.meet-cal-event.is-status-3:focus-visible { background:#eaeef4;border-color:#bfcbd8; }
    .meet-cal-event.is-status-0:hover,.meet-cal-event.is-status-0:focus-visible { background:#eae0f7; }
    .meet-cal-event.is-status-1:hover,.meet-cal-event.is-status-1:focus-visible { background:#e7dafc; }
    .meet-cal-event.is-status-2:hover,.meet-cal-event.is-status-2:focus-visible { background:#dcf6e8; }
    .meet-cal-legend-status { display:inline-block;width:9px;height:9px;border-radius:50%;margin-right:3px;vertical-align:middle; }
    .meet-cal-legend-status.scheduled { background:#7440ae; }
    .meet-cal-legend-status.soon { background:#7440ae;box-shadow:0 0 0 3px rgba(133,77,198,.17); }
    .meet-cal-legend-status.live { background:#1ca86d;box-shadow:0 0 0 3px rgba(28,168,109,.14); }
    .meet-cal-legend-status.done { background:#94a3b8; }
    .meet-cal-status-dot { position:absolute;right:8px;bottom:7px;display:inline-block;width:12px;height:12px;min-width:12px;border-radius:50%;box-shadow:0 0 0 0 rgba(135,74,200,0); }
    .meet-day-modal-events .meet-cal-status-dot { right:10px;bottom:10px;width:13px;height:13px;min-width:13px; }
    .meet-cal-event.is-status-1 .meet-cal-status-dot { background:#874ac8;animation:meetCalSoonPulse 2s ease-in-out infinite; }
    .meet-cal-event.is-status-2 .meet-cal-status-dot { background:#14a266;animation:meetCalLivePulse 1.7s ease-in-out infinite; }
    @keyframes meetCalSoonPulse {
        0%,100% { opacity:1;transform:scale(1);box-shadow:0 0 0 0 rgba(135,74,200,.34); }
        55% { opacity:.88;transform:scale(1.06);box-shadow:0 0 0 7px rgba(135,74,200,0); }
    }
    @keyframes meetCalLivePulse {
        0%,100% { opacity:1;transform:scale(1);box-shadow:0 0 0 0 rgba(20,162,102,.34); }
        50% { opacity:.86;transform:scale(1.07);box-shadow:0 0 0 8px rgba(20,162,102,0); }
    }
    .meet-cal-no-meetings { padding:12px 6px;color:#81778b;font-size:11.5px;font-weight:550;text-align:center; }
    .meet-cal-no-meetings i { margin-right:4px;opacity:.8; }
    .meet-cal-view-more { display:inline-flex;align-items:center;justify-content:center;gap:5px;min-height:26px;padding:4px 6px;border:1px dashed #cbb6e6;border-radius:6px;background:#f7f2ff;color:#592b8d;font-size:11px;font-weight:750;cursor:pointer; }
    .meet-cal-view-more:hover { background:#ebe0fc;border-color:#ac85da; }
    .meet-cal-day-all { display:none!important; }
    .meet-cal-popover,.meet-cal-event:hover .meet-cal-popover,.meet-cal-event:focus-visible .meet-cal-popover { display:none!important; }
    .meet-cal-float-tip { position:fixed;z-index:1200;box-sizing:border-box;width:390px;max-width:calc(100vw - 20px);padding:15px 17px;border:1px solid #c9b1e5;border-radius:12px;background:#fff;color:#322840;box-shadow:0 15px 38px rgba(40,23,66,.23);pointer-events:none; }
    .meet-cal-float-tip strong { display:block;margin:0;color:#4b187a;font-size:15.5px;line-height:1.45; }
    .meet-cal-float-tip span { display:block;margin-top:7px;font-size:12.5px;line-height:1.5;color:#453a53; }
    .meet-cal-float-tip span.meet-cal-tooltip-head { display:flex;align-items:center;justify-content:space-between;gap:12px;margin:0 0 11px;min-width:0; }
    .meet-cal-float-tip strong.meet-cal-tooltip-title { flex:1 1 auto;min-width:0;overflow-wrap:anywhere;color:#4b187a;font-size:15.5px;font-weight:800;line-height:1.4; }
    .meet-cal-float-tip span.meet-cal-tooltip-status { display:inline-flex;align-items:center;justify-content:center;gap:8px;flex:0 0 auto;margin:0;padding:6px 10px;border:1px solid transparent;border-radius:999px;font-size:11.5px;font-weight:800;line-height:1.2;white-space:nowrap; }
    .meet-cal-float-tip span.meet-cal-tooltip-status.is-status-0 { background:#f2ecfa;border-color:#d6c2ed;color:#6431a0; }
    .meet-cal-float-tip span.meet-cal-tooltip-status.is-status-1 { background:#efe5ff;border-color:#c6a6ed;color:#8247c3; }
    .meet-cal-float-tip span.meet-cal-tooltip-status.is-status-2 { background:#eafaf0;border-color:#a6e8c3;color:#12834f; }
    .meet-cal-float-tip span.meet-cal-tooltip-status.is-status-3 { background:#f3f5f8;border-color:#d8dfe8;color:#637287; }
    .meet-cal-float-tip .meet-cal-tooltip-status-dot { display:block;flex:0 0 10px;width:10px;height:10px;margin:0;border-radius:50%;background:currentColor; }
    .meet-cal-float-tip .meet-cal-tooltip-status.is-status-1 .meet-cal-tooltip-status-dot { animation:meetCalSoonPulse 2s ease-in-out infinite; }
    .meet-cal-float-tip .meet-cal-tooltip-status.is-status-2 .meet-cal-tooltip-status-dot { animation:meetCalLivePulse 1.7s ease-in-out infinite; }
    .meet-cal-float-tip span.meet-cal-chair { padding:8px 10px;border-radius:8px;background:#f1eafb;color:#552087;font-weight:750; }
    .meet-cal-float-tip span.meet-cal-chair.is-self { background:#fff0d1;color:#875408; }
    .meet-cal-float-tip small { display:block;margin-top:9px;padding-top:8px;border-top:1px solid #eee7f4;color:#61596c;font-size:12px;line-height:1.5; }
    .meet-cal-toolbar { display:grid;grid-template-columns:minmax(195px,1fr) auto minmax(195px,1fr);gap:12px;align-items:center; }
    .meet-cal-navigation { justify-content:center; }
    .meet-cal-toolbar-actions { display:flex;align-items:center;justify-content:flex-end;gap:8px;min-width:0; }
    .meet-cal-toolbar-actions .btn { white-space:nowrap; }
    .meet-calendar-area { background:#fff;padding:13px 14px 16px; }
    .meet-cal-subtitle { font-size:12px;color:#746987; }
    .meet-cal-legend { font-size:12px; }
    .meet-day-modal { padding:15px 17px 18px; }
    .meet-day-modal-heading { display:flex;align-items:center;gap:9px;margin-bottom:12px;color:#50218b;font-size:15px;font-weight:800; }
    .meet-day-modal-count { display:inline-flex;align-items:center;padding:4px 8px;border-radius:7px;background:#f0e8fb;color:#54218d;font-size:11.5px;font-weight:700; }
    .meet-day-modal-events { display:flex;flex-direction:column;gap:8px;max-height:min(58vh,520px);overflow-y:auto;padding:3px 3px 9px; }
    .meet-day-modal-events .meet-cal-event { display:flex;min-height:46px;margin:0;padding:11px 30px 11px 12px;font-size:13px; }
    .meet-day-modal-events .meet-cal-event-text { flex:1; }
    .meet-day-modal-events .meet-cal-event-time { font-size:13px; }
    @media(max-width:1150px) {
        .meet-cal-toolbar { grid-template-columns:minmax(0,1fr) minmax(0,1fr); }
        .meet-cal-heading { grid-column:1; }
        .meet-cal-toolbar-actions { grid-column:2;grid-row:1; }
        .meet-cal-navigation { grid-column:1/-1;grid-row:2; }
    }
    @media(max-width:570px) {
        .meet-cal-toolbar { grid-template-columns:1fr; }
        .meet-cal-heading,.meet-cal-toolbar-actions,.meet-cal-navigation { grid-column:1;grid-row:auto; }
        .meet-cal-toolbar-actions { justify-content:flex-start; }
        .meet-cal-navigation { justify-content:center; }
        .meet-day-modal { padding:12px 10px; }
        .meet-cal-day { min-height:135px; }
    }
    @media(prefers-reduced-motion:reduce) {
        .meet-cal-day,.meet-cal-event { transition:none!important; }
        .meet-cal-status-dot,.meet-cal-tooltip-status-dot { animation:none!important; }
    }
    /* Một thanh công cụ cố định cho Danh sách, Lịch và Giờ công; không nhảy vị trí. */
    .meet-view-shell .meet-shared-toolbar { display:grid;grid-template-columns:minmax(0,1fr) auto minmax(0,1fr);align-items:center;column-gap:14px;row-gap:8px;min-height:68px;padding:10px 14px!important;margin:0;background:transparent;border:0!important; }
    .meet-view-shell .meet-shared-toolbar>.meet-list-filters { grid-column:1/3;grid-row:1;min-width:0; }
    .meet-view-shell .meet-shared-toolbar>.meet-cal-heading { grid-column:1;grid-row:1;min-width:0; }
    .meet-view-shell .meet-shared-toolbar>.meet-cal-navigation { grid-column:2;grid-row:1;justify-content:center;min-width:0; }
    .meet-view-shell .meet-shared-toolbar>.meet-common-actions { grid-column:3;grid-row:1;justify-self:end;display:flex;align-items:center;gap:8px;margin:0;min-width:0;white-space:nowrap; }
    .meet-view-shell.meet-mode-list .meet-shared-toolbar>.meet-cal-heading,.meet-view-shell.meet-mode-list .meet-shared-toolbar>.meet-cal-navigation { display:none!important; }
    .meet-view-shell.meet-mode-calendar .meet-shared-toolbar>.meet-list-filters { display:none!important; }
    .meet-view-shell.meet-mode-labor .meet-shared-toolbar>.meet-list-filters,.meet-view-shell.meet-mode-labor .meet-shared-toolbar>.meet-cal-heading,.meet-view-shell.meet-mode-labor .meet-shared-toolbar>.meet-cal-navigation { display:none!important; }
    /* Không co thanh công cụ ở chế độ Giờ công: giữ cùng chiều cao/cột với hai tab khác. */
    .meet-view-shell .meet-shared-toolbar>.meet-labor-heading { display:none!important; }
    .meet-view-shell.meet-mode-labor .meet-shared-toolbar>.meet-labor-heading { display:flex!important;grid-column:1;grid-row:1;min-width:0; }
    .meet-view-shell.meet-mode-labor .meet-shared-toolbar>.meet-common-actions { grid-column:3;grid-row:1; }
    .meet-view-shell .meet-panel-calendar .meet-calendar-area { padding:10px 14px 16px;background:#fff; }
    .meet-view-shell .meet-panel-list>.listSearchTagBox { padding:0 14px; }
    @media(max-width:1280px) {
        .meet-view-shell .meet-shared-toolbar { grid-template-columns:minmax(0,1fr) auto;grid-template-rows:minmax(68px,auto) auto; }
        .meet-view-shell .meet-shared-toolbar>.meet-list-filters { grid-column:1; }
        .meet-view-shell .meet-shared-toolbar>.meet-cal-heading { grid-column:1; }
        .meet-view-shell .meet-shared-toolbar>.meet-cal-navigation { grid-column:1/-1;grid-row:2;justify-self:center; }
        .meet-view-shell .meet-shared-toolbar>.meet-common-actions { grid-column:2;grid-row:1; }
        .meet-view-shell.meet-mode-labor .meet-shared-toolbar>.meet-common-actions { grid-column:2;grid-row:1; }
    }
    @media(max-width:760px) {
        .meet-view-shell .meet-shared-toolbar { grid-template-columns:minmax(0,1fr);grid-template-rows:auto auto auto;padding:9px 10px!important; }
        .meet-view-shell .meet-shared-toolbar>.meet-list-filters,.meet-view-shell .meet-shared-toolbar>.meet-cal-heading,.meet-view-shell .meet-shared-toolbar>.meet-cal-navigation,.meet-view-shell .meet-shared-toolbar>.meet-common-actions { grid-column:1;grid-row:auto; }
        .meet-view-shell .meet-shared-toolbar>.meet-common-actions { grid-row:1;justify-self:end; }
        .meet-view-shell .meet-shared-toolbar>.meet-list-filters,.meet-view-shell .meet-shared-toolbar>.meet-cal-heading { grid-row:2; }
        .meet-view-shell .meet-shared-toolbar>.meet-cal-navigation { grid-row:3; }
        .meet-view-shell .meet-shared-toolbar>.meet-common-actions { justify-self:end; }
        .meet-view-shell .meet-shared-toolbar>.meet-cal-navigation { justify-self:stretch; }
        .meet-view-shell.meet-mode-labor .meet-shared-toolbar>.meet-labor-heading { grid-column:1;grid-row:2; }
        .meet-view-shell.meet-mode-labor .meet-shared-toolbar>.meet-common-actions { grid-column:1;grid-row:1; }
        .meet-view-shell .meet-shared-toolbar>.meet-list-filters .input-group { min-width:0; }
        .meet-view-shell .meet-panel-calendar .meet-calendar-area { padding:8px 10px 12px; }
    }

    /* Flip card tham chiếu kết cấu hai mặt từ SweetSoft Portfolio.
       Giữ lật NGANG (rotateY), chỉ tác động các thẻ cuộc họp. */
    .meet-calendar-area .meet-cal-event.meet-cal-flip-card {
        display:block;position:relative;width:100%;height:46px;min-height:46px;max-height:46px;
        margin:0;padding:0;overflow:visible;perspective:960px;
        cursor:default;box-sizing:border-box;filter:none;
    }
    .meet-calendar-area .meet-cal-day-items .meet-cal-flip-card { padding:0; }
    .meet-cal-flip-card.is-status-0 { --meet-flip-bg:#f2ecfa; --meet-flip-text:#52227e; }
    .meet-cal-flip-card.is-status-1 { --meet-flip-bg:#efe5ff; --meet-flip-text:#542083; }
    .meet-cal-flip-card.is-status-2 { --meet-flip-bg:#eafaf0; --meet-flip-text:#167746; }
    .meet-cal-flip-card.is-status-3 { --meet-flip-bg:#f3f5f8; --meet-flip-text:#637287; }
    .meet-cal-flip-inner {
        position:relative;display:block;width:100%;height:100%;border-radius:5px;
        transform:rotateY(0deg);transform-style:preserve-3d;
        -webkit-transform-style:preserve-3d;
        transition:transform .76s cubic-bezier(.22,.61,.36,1);
        will-change:transform;
    }
    .meet-cal-flip-card:hover .meet-cal-flip-inner,
    .meet-cal-flip-card:focus-within .meet-cal-flip-inner { transform:rotateY(180deg); }
    .meet-cal-flip-face {
        position:absolute;inset:0;box-sizing:border-box;display:flex;min-width:0;
        border-radius:5px;color:var(--meet-flip-text,#52227e);
        backface-visibility:hidden;-webkit-backface-visibility:hidden;
        transform-style:preserve-3d;-webkit-transform-style:preserve-3d;
    }
    /* Mặt trước giống thẻ lịch hiện có, vẫn click để mở chi tiết. */
    .meet-cal-flip-front {
        align-items:center;justify-content:flex-start;gap:4px;width:100%;padding:5px 6px;
        border:1px solid rgba(113,76,156,.19);background:var(--meet-flip-bg,#f2ecfa);
        appearance:none;color:inherit;font:inherit;text-align:left;cursor:pointer;
        box-shadow:inset 0 1px rgba(255,255,255,.8),0 1px 4px rgba(51,23,86,.08);
        transform:rotateY(0deg) translateZ(1px);
    }
    .meet-cal-flip-front .meet-cal-event-time { flex:0 0 auto;font-weight:800; }
    .meet-cal-flip-front .meet-cal-event-text { flex:1;min-width:0; }
    .meet-cal-flip-front .meet-cal-status-dot { pointer-events:none; }
    /* Mặt sau gồm nền chứa tên và lớp thao tác nổi phía trước. */
    .meet-cal-flip-back {
        display:block;padding:0;background:transparent;
        transform:rotateY(180deg) translateZ(1px);
    }
    .meet-cal-flip-backplate {
        position:absolute;inset:0;display:flex;align-items:center;justify-content:center;
        padding:16px 7px 2px;min-width:0;box-sizing:border-box;border-radius:6px;
        border:1px solid rgba(116,73,168,.26);
        background:linear-gradient(145deg,rgba(255,255,255,.93),var(--meet-flip-bg,#f2ecfa));
        box-shadow:inset 0 1px rgba(255,255,255,.9),0 4px 12px rgba(51,22,90,.14);
        transform:translateZ(0px);
    }
    .meet-cal-flip-back .meet-cal-flip-title {
        display:block;max-width:100%;min-width:0;overflow:hidden;text-overflow:ellipsis;
        white-space:nowrap;text-align:center;color:var(--meet-flip-text,#52227e);
        font-size:10.5px;font-weight:750;line-height:1.25;
    }
    /* Các nút nổi (lớp trên) xuất hiện dần trong nửa sau vòng lật.
       Không xoay chữ/nút riêng nên không bị hiệu ứng phản chiếu ngược. */
    .meet-cal-flip-actions {
        position:absolute;z-index:2;left:50%;top:2px;
        display:flex;align-items:center;justify-content:center;gap:7px;
        width:max-content;max-width:calc(100% - 8px);min-width:0;
        padding:2px 6px;border:1px solid rgba(135,98,184,.23);
        border-radius:8px;background:rgba(255,255,255,.98);
        box-shadow:0 7px 16px rgba(52,27,94,.23);line-height:1;
        opacity:0;pointer-events:none;
        transform:translateX(-50%) translateY(7px) translateZ(30px) scale(.88);
        transition:opacity .18s ease,transform .30s cubic-bezier(.22,.8,.24,1);
    }
    .meet-cal-flip-card:hover .meet-cal-flip-actions,
    .meet-cal-flip-card:focus-within .meet-cal-flip-actions {
        opacity:1;pointer-events:auto;
        transform:translateX(-50%) translateY(0px) translateZ(30px) scale(1);
        transition-delay:.29s,.29s;
    }
    .meet-cal-flip-action {
        display:inline-flex;align-items:center;justify-content:center;
        flex:0 0 24px;width:24px;height:21px;padding:0;
        border:1px solid rgba(106,55,157,.24);border-radius:5px;
        background:#fff;color:#5c2a94;font-size:11px;line-height:1;cursor:pointer;
        box-shadow:0 2px 6px rgba(63,28,111,.08);
        transition:background-color .15s,border-color .15s,transform .15s,box-shadow .15s;
    }
    .meet-cal-flip-action:hover { background:#fff;border-color:#9b73c8;transform:translateY(-1px);box-shadow:0 5px 12px rgba(63,28,111,.18); }
    .meet-cal-flip-action.is-delete { color:#cb3750;border-color:#f0c6ce; }
    .meet-cal-flip-action.is-delete:hover { background:#fff0f2;border-color:#e892a2; }
    .meet-cal-flip-action:focus-visible,.meet-cal-flip-front:focus-visible { outline:2px solid #7842b5;outline-offset:1px; }
    .meet-calendar-grid.is-month .meet-cal-day { min-height:161px; }
    /* Cùng hiệu ứng trong popup danh sách cuộc họp của một ngày. */
    .meet-day-modal-events .meet-cal-event.meet-cal-flip-card {
        position:relative;display:block;height:58px;min-height:58px;max-height:58px;
        padding:0;margin:0;overflow:visible;perspective:960px;
    }
    .meet-day-modal-events .meet-cal-flip-backplate { padding-top:22px; }
    .meet-day-modal-events .meet-cal-flip-actions { top:5px;gap:10px; }
    .meet-day-modal-events .meet-cal-flip-action { width:28px;height:26px;flex-basis:28px;font-size:12px; }
    .meet-day-modal-events .meet-cal-flip-title { font-size:12px; }
    @media(hover:none) {
        .meet-cal-flip-card:focus-within .meet-cal-flip-inner { transform:rotateY(180deg); }
    }
    @media(prefers-reduced-motion:reduce) {
        .meet-cal-flip-inner { transition:none!important;will-change:auto; }
        .meet-cal-flip-actions { transition:none!important;transition-delay:0s!important; }
    }

</style>

<div id="meetViewShell" runat="server" class="meet-view-shell meet-mode-list">
<asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional"><ContentTemplate>
<div class="card-header meet-shared-toolbar" role="group" aria-label="Thanh công cụ lịch họp">
<div class="meet-list-filters">
        <asp:UpdatePanel runat="server" ID="pnlSearchDropdowns" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel runat="server" ID="pnlSearchDefaultStatus">
                    <div class="d-flex">
                        <SweetSoft:BootstrapDropdown ID="ddlSearchTrangThai" runat="server" Text="Trạng thái" AllowClear="true" AutoPostBack="true" SearchColumn="TrangThai" CssClass="border-top-left-radius-1 border-bottom-left-radius-1" OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged"></SweetSoft:BootstrapDropdown>
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>

        <div class="input-group max-w-500 mb-0">
            <a class="btn btn-info font-mobile-small btn-search-filter" onclick="CMSMasterJs.ShowOffcanvasSearch();" href="javascript:;">
                <i class="fas fa-filter me-1"></i>
                <%= GetResourceText(BackEndResourceKeys.FILTER) %>
            </a>
            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
        </div>
        </div>

<div class="meet-cal-heading"><span class="meet-cal-heading-icon"><i class="far fa-calendar-alt"></i></span><span><strong class="meet-cal-period" id="meetCalPeriodLabel">Lịch họp</strong><small class="meet-cal-subtitle">Lịch họp theo ngày · Hiển thị tối đa 2 cuộc họp/ngày</small></span></div>
<div class="meet-cal-heading meet-labor-heading"><span class="meet-cal-heading-icon"><i class="far fa-clock"></i></span><span><strong class="meet-cal-period">Giờ công cuộc họp</strong><small class="meet-cal-subtitle">Theo dõi người tham gia và công thực tế</small></span></div>

<div class="meet-cal-navigation">
                        <asp:LinkButton runat="server" ID="btnCalendarPrev" CssClass="meet-cal-nav" OnClick="btnCalendarNavigate_Click" CommandArgument="prev" CausesValidation="false" ToolTip="Kỳ trước"><i class="fas fa-chevron-left"></i></asp:LinkButton>
                        <asp:LinkButton runat="server" ID="btnCalendarToday" CssClass="meet-cal-nav" OnClick="btnCalendarNavigate_Click" CommandArgument="today" CausesValidation="false">Hôm nay</asp:LinkButton>
                        <asp:LinkButton runat="server" ID="btnCalendarNext" CssClass="meet-cal-nav" OnClick="btnCalendarNavigate_Click" CommandArgument="next" CausesValidation="false" ToolTip="Kỳ sau"><i class="fas fa-chevron-right"></i></asp:LinkButton>
                        <asp:LinkButton runat="server" ID="btnCalendarMonth" CssClass="meet-cal-period-btn" OnClick="btnCalendarMode_Click" CommandArgument="month" CausesValidation="false"><i class="far fa-calendar"></i> Tháng</asp:LinkButton>
                        <asp:LinkButton runat="server" ID="btnCalendarWeek" CssClass="meet-cal-period-btn" OnClick="btnCalendarMode_Click" CommandArgument="week" CausesValidation="false"><i class="fas fa-calendar-week"></i> Tuần</asp:LinkButton>
                    </div>

<div runat="server" id="tagOther" class="meet-common-actions">
            <div id="meetViewSwitcher" runat="server" class="meet-view-switcher" aria-label="Chuyển chế độ xem cuộc họp">
                <button type="button" class="meet-view-button" data-meet-view="list" onclick="return SweetMeetCalendar.changeView('list');" aria-label="Xem danh sách cuộc họp" title="Danh sách cuộc họp" aria-pressed="false"><i class="fas fa-list-ul" aria-hidden="true"></i><span>Danh sách</span></button>
                <button type="button" class="meet-view-button" data-meet-view="calendar" onclick="return SweetMeetCalendar.changeView('calendar');" aria-label="Xem lịch cuộc họp" title="Lịch cuộc họp" aria-pressed="false"><i class="far fa-calendar-alt" aria-hidden="true"></i><span>Lịch</span></button>
                <button type="button" class="meet-view-button" data-meet-view="labor" onclick="return SweetMeetCalendar.changeView('labor');" aria-label="Xem giờ công tham gia cuộc họp" title="Giờ công cuộc họp" aria-pressed="false"><i class="far fa-clock" aria-hidden="true"></i><span>Giờ công</span></button>
            </div>
            <asp:UpdatePanel runat="server" ID="pnlButtons" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="d-flex">
                        <SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click" CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Info" ButtonIcon="Add">
                            Thêm mới
                        </SweetSoft:ExtraButton>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
</div>
<div class="meet-display-stage">
<div class="meet-view-panel meet-panel-list">
<div class="listSearchTagBox">
        <asp:UpdatePanel ID="upSearchTagBox" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <SweetSoft:ExtraSearchBox ID="searchTagBox" runat="server" OnTagClosed="searchTagBox_TagClosed"></SweetSoft:ExtraSearchBox>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

<div class="card-body p-0 meet-list-area">

            <SweetSoft:GridviewExtension ID="grvData" runat="server" AllowSorting="true" ShowHeader="true" ShowHeaderWhenEmpty="true" AutoGenerateColumns="false" CssClass="table table-bordered table-hover align-middle w-100" FocusBtnIcon="fas fa-compress-arrows-alt" DataKeyNames="IdLichHop" ValueField="IdLichHop" DataNameField="TenCuocHop" GridLines="None" IsEnableSelectColumn="false" OnNeedDataSource="grvData_NeedDataSource" OnRowCommand="grvData_RowCommand">
                <Columns>
                    <asp:TemplateField HeaderText="MeetingName" SortExpression="TenCuocHop" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="meeting-table-cell">
                        <ItemTemplate>
                            <div class="meeting-name-cell">
                                <div class="meeting-name-text">
                                    <asp:LinkButton
                                        runat="server"
                                        ID="lbtMeetingName"
                                        CommandName="ITEM_VIEW"
                                        CommandArgument='<%# Eval("IdLichHop") %>'
                                        CausesValidation="false"
                                        CssClass="meeting-name-main"
                                        ToolTip="Xem thông tin cuộc họp">
                                        <%# Eval("TenCuocHop") ?? "—" %>
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Người tham gia" HeaderStyle-Width="180px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center meeting-table-cell" ItemStyle-Wrap="false">
                        <ItemTemplate>
                            <div class="avatar-group">
                                <div class="avatar-stack-container">
                                    <%# GetAssigneeDisplay(Eval("TenNhanVien"), Eval("Avatars")) %>
                                </div>
                                <asp:LinkButton runat="server" ID="lbtViewMembers" CommandName="VIEW_MEMBERS" CommandArgument='<%# Eval("IdLichHop") %>' CssClass="btn-assign-task view-only" ToolTip="Xem thành viên" Visible='<%# this.IsView || this.IsEdit %>'>
                                    <i class="fas fa-user-friends"></i>
                                </asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="StartTime" SortExpression="ThoiGianBatDau" HeaderStyle-Width="130px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center meeting-table-cell">
                        <ItemTemplate>
                            <div class="meeting-datetime">
                                <span class="meeting-date"><%# Eval("ThoiGianBatDau", "{0:dd/MM/yyyy}") %></span>
                                <span class="meeting-time"><i class="far fa-clock me-1"></i><%# Eval("ThoiGianBatDau", "{0:HH:mm}") %></span>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="EndTime" SortExpression="ThoiGianKetThuc" HeaderStyle-Width="130px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center meeting-table-cell">
                        <ItemTemplate>
                            <div class="meeting-datetime">
                                <span class="meeting-date"><%# Eval("ThoiGianKetThuc", "{0:dd/MM/yyyy}") %></span>
                                <span class="meeting-time"><i class="far fa-clock me-1"></i><%# Eval("ThoiGianKetThuc", "{0:HH:mm}") %></span>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="MeetingRoom" SortExpression="DiaDiemHop" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="meeting-table-cell">
                        <ItemTemplate>
                            <div class="meeting-room-cell">
                                <span class="meeting-room-icon">
                                    <i class="fas fa-map-marker-alt"></i>
                                </span>
                                <span class="meeting-room-text" title='<%# Eval("DiaDiemHop") %>'>
                                    <%# Eval("DiaDiemHop") ?? "—" %>
                                </span>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Status" SortExpression="TrangThai" HeaderStyle-Width="150px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center meeting-table-cell">
                        <ItemTemplate>
                            <span class='<%# "meeting-status-badge meeting-status-" + Convert.ToString(Eval("TrangThai")) %>'>
                                <%# GetTrangThaiCuocHopText(Eval("TrangThai")) %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center meeting-table-cell" HeaderStyle-Width="150px">
                        <ItemTemplate>
                            <div class="meeting-action-group">

                                <asp:LinkButton runat="server" ID="lbtMeetingFiles" Visible='<%# this.IsView %>' CommandName="MEETING_FILES" CommandArgument='<%# Eval("IdLichHop") %>' CausesValidation="false" CssClass="btn btn-outline-primary btn-sm text-center btn-smart-link" ToolTip="File đính kèm lịch họp">
                                    <i class="fas fa-folder-open"></i>
                                </asp:LinkButton>
                                <SweetSoft:SmartLinkButton runat="server"
                                    VisibleConditionKey='<%# this.IsEdit || this.IsView %>'
                                    ID="lbtDetail"
                                    CommandName='<%# this.IsEdit ? "ITEM_DETAIL" : "ITEM_VIEW" %>'
                                    CommandArgument='<%# Eval("IdLichHop") %>'
                                    CssClass="btn-grid-action text-decoration-underline"
                                    ResourceKey='<%# this.IsEdit ? BackEndResourceKeys.EDIT : BackEndResourceKeys.VIEW %>'
                                    ButtonIcon='<%# this.IsEdit ? "fas fa-pencil-alt" : "fas fa-eye" %>'>
                                </SweetSoft:SmartLinkButton>                                <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsDelete %>' ID="lbtDelete" CommandName="ITEM_DELETE" CssClass="btn-grid-action text-decoration-underline text-danger" ResourceKey='<%# BackEndResourceKeys.DELETE %>' ButtonIcon="fas fa-trash"></SweetSoft:SmartLinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>

                <EmptyDataTemplate>
                    <%= GetResourceText(BackEndResourceKeys.NO_DATA) %>
                </EmptyDataTemplate>
            </SweetSoft:GridviewExtension>

            <SweetSoft:Paging runat="server" ID="ctrlGridviewPaging" OnPageChanged="ctrlGridviewPaging_PageChanged" />

</div>
</div>
<div class="meet-view-panel meet-panel-calendar">
            <div class="meet-calendar-area">
                <div class="meet-cal-legend"><span><i class="meet-cal-legend-status scheduled"></i> Đã lên lịch</span><span><i class="meet-cal-legend-status soon"></i> Sắp diễn ra</span><span><i class="meet-cal-legend-status live"></i> Đang diễn ra</span><span><i class="meet-cal-legend-status done"></i> Đã kết thúc</span><span><i class="fas fa-crown" style="color:#d69a19"></i> Bạn chủ trì</span><span><i class="meet-cal-dot off"></i> Ngày nghỉ / ngày lễ</span><span><i class="far fa-hand-pointer"></i> Bấm ngày để xem đủ cuộc họp · Bấm cuộc họp để mở chi tiết</span></div>
                <asp:PlaceHolder runat="server" ID="phMeetingCalendar"></asp:PlaceHolder>
                <input type="hidden" id="meetCalendarSelectedId" name="meetCalendarSelectedId" value="" />
                <input type="hidden" id="meetCalendarActionType" name="meetCalendarActionType" value="" />
                <asp:HiddenField runat="server" ID="hdfCalendarCreateDate" />
                <asp:LinkButton runat="server" ID="btnCreateCalendarMeeting" OnClick="btnCreateCalendarMeeting_Click" CausesValidation="false" Style="display:none;"></asp:LinkButton>
            </div>
</div>
<div class="meet-view-panel meet-panel-labor">
    <SweetSoft:CtrlMeetGioCongTab runat="server" ID="CtrlMeetGioCongTab1" />
</div>
</div>
            <asp:LinkButton runat="server" ID="btnRefreshMeetingStatuses" OnClick="btnRefreshMeetingStatuses_Click" CausesValidation="false" Style="display:none;"></asp:LinkButton>
            <SweetSoft:CtrlViewMeetDetail runat="server" ID="CtrlViewMeetDetail1" />
            <SweetSoft:CtrlXemNhanVienMeet runat="server" ID="CtrlXemNhanVienMeet1" />
        </ContentTemplate>
    </asp:UpdatePanel>
    <SweetSoft:ExtraModal runat="server" ID="mdlCalendarDayMeetings" Type="Primary" Title="Các cuộc họp trong ngày">
        <ContentTemplate>
            <div class="meet-day-modal">
                <div class="meet-day-modal-heading"><i class="far fa-calendar-alt" aria-hidden="true"></i><span id="meetDayModalDate">Ngày --/--/----</span><span id="meetDayModalCount" class="meet-day-modal-count"></span></div>
                <div id="meetDayModalMeetings" class="meet-day-modal-events" role="group" aria-label="Các cuộc họp trong ngày"></div>
            </div>
        </ContentTemplate>
        <FooterTemplate>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
</div>

<div class="offcanvas offcanvas-end offcanvas-form-search" id="search-offcanvas" aria-hidden="true">
    <div class="offcanvas-header">
        <h5 class="offcanvas-title">Tìm kiếm nâng cao</h5>
        <button class="btn-close" type="button" data-bs-dismiss="offcanvas" aria-label="Close"></button>
    </div>

    <div class="div offcanvas-body pt-0">
        <div class="card shadow-none card-body text-muted mb-0">
            <asp:UpdatePanel runat="server" ID="pnlSearch" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel runat="server" ID="pnlSearchPopup">
                        <div class="row">
                            <div class="col-md-12 mb-3">
                                <label class="form-label">Tên cuộc họp</label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchTenCuocHop" SearchColumn="TenCuocHop"></SweetSoft:ExtraTextBox>
                            </div>
                        </div>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>

            <div class="d-flex align-items-center gap-1 mt-3">
                <SweetSoft:ExtraButton runat="server" ID="lbtSearchAdvanced" CssClass="flex-btn" ButtonStyle="Primary" ButtonIcon="Search" OnClick="btnSearchAdvanced_ServerClick">
                    Áp dụng
                </SweetSoft:ExtraButton>
                <SweetSoft:ExtraButton runat="server" ID="lbtCancel" CssClass="flex-btn" ButtonStyle="OutLineSecondary" ButtonIcon="Refresh" OnClick="btnCancel_Click">
                    Làm mới
                </SweetSoft:ExtraButton>
            </div>
        </div>
    </div>
</div>

<script type="text/javascript">
    // Hai giao diện độc lập trong UpdatePanel; đổi tức thì, không sử dụng hiệu ứng chuyển cảnh.
    window.SweetMeetCalendar = window.SweetMeetCalendar || (function () {
        function shell() { return document.querySelector('.meet-view-shell'); }
        function storageKey() { var node = shell(); return 'SweetSoft_MeetView_' + (node ? node.getAttribute('data-project') || 'default' : 'default'); }
        function updateLabels(node, view) {
            var buttons = node.querySelectorAll('[data-meet-view]');
            for (var i = 0; i < buttons.length; i++) {
                var button = buttons[i];
                var active = button.getAttribute('data-meet-view') === view;
                button.classList.toggle('is-active', active);
                button.setAttribute('aria-pressed', active ? 'true' : 'false');
            }
        }
        function changeView(view, skipLoad) {
            var node = shell(); if (!node) return false;
            var canList = node.getAttribute('data-can-list') === '1';
            if (view !== 'list' && view !== 'calendar' && view !== 'labor') return false;
            if (!canList) view = 'calendar';
            node.classList.toggle('meet-mode-list', view === 'list');
            node.classList.toggle('meet-mode-calendar', view === 'calendar');
            node.classList.toggle('meet-mode-labor', view === 'labor');
            updateLabels(node, view);
            if (canList) { try { sessionStorage.setItem(storageKey(), view); } catch (e) { } }
            if (view === 'labor' && (!skipLoad || !node.querySelector('.meet-panel-labor .mlt-layout[data-loaded="1"]'))) { var load = document.getElementById('<%= LaborLoadButtonId %>'); if (load) load.click(); }
            return false;
        }
        // Giữ alias cũ cho những vị trí khác có thể vẫn đang gọi toggleView().
        function toggleView() {
            var node = shell(); if (!node || node.getAttribute('data-can-list') !== '1') return false;
            return changeView(node.classList.contains('meet-mode-calendar') ? 'list' : 'calendar');
        }
        function initialize() {
            var node = shell(); if (!node) return;
            var period = node.querySelector('.meet-cal-period-data');
            var label = node.querySelector('#meetCalPeriodLabel');
            if (period && label) label.textContent = period.getAttribute('data-cal-period') || 'Lịch họp';
            var grid = node.querySelector('.meet-calendar-grid'), buttons = node.querySelectorAll('.meet-cal-period-btn');
            for (var j = 0; j < buttons.length; j++) {
                var id = buttons[j].id || '';
                var week = !!(grid && grid.classList.contains('is-week'));
                buttons[j].classList.toggle('is-active', id.indexOf('btnCalendarWeek') !== -1 ? week : !week);
            }
            var view = node.getAttribute('data-can-list') === '1' ? 'list' : 'calendar';
            if (view === 'list') { try { view = sessionStorage.getItem(storageKey()) || 'list'; } catch (e) { } }
            changeView(view, true);
        }
        function createMeeting(dateText) {
            var chosen = document.getElementById('<%= CalendarCreateHidden.ClientID %>');
            var button = document.getElementById('<%= CalendarCreateButton.ClientID %>');
            if (!chosen || !button || !/^(\d{4})-(\d{2})-(\d{2})$/.test(dateText || '')) return false;
            chosen.value = dateText;
            button.click();
            return false;
        }
        function openAction(action, id) {
            var hidden = document.getElementById('meetCalendarSelectedId');
            var kind = document.getElementById('meetCalendarActionType');
            var button = document.getElementById('<%= btnRefreshMeetingStatuses.ClientID %>');
            if (!hidden || !kind || !button || !/^[a-f0-9-]{36}$/i.test(id)) return false;
            if (action !== 'view' && action !== 'edit' && action !== 'delete') return false;
            function submitAction() {
                hidden.value = id;
                kind.value = action;
                button.click();
            }
            var $modal = $('#<%= mdlCalendarDayMeetings.ClientID %>');
            if ($modal.hasClass('show') || $modal.is(':visible')) {
                $modal.one('hidden.bs.modal', submitAction);
                $modal.modal('hide');
            } else submitAction();
            return false;
        }
        function openDetail(id) { return openAction('view', id); }
        function openDay(day) {
            if (!day || Number(day.getAttribute('data-meet-count') || 0) < 1) return false;
            var source = day.querySelector('.meet-cal-day-all'); if (!source) return false;
            var $modal = $('#<%= mdlCalendarDayMeetings.ClientID %>'); if (!$modal.length) return false;
            $('#meetDayModalDate').text('Ngày ' + (day.getAttribute('data-day-label') || ''));
            $('#meetDayModalCount').text(day.getAttribute('data-meet-count') + ' cuộc họp');
            $('#meetDayModalMeetings').html(source.innerHTML);
            $modal.modal('show'); return false;
        }
        function closeDay() { $('#<%= mdlCalendarDayMeetings.ClientID %>').modal('hide'); return false; }
        if (window.Sys && Sys.Application) Sys.Application.add_load(initialize);
        if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', initialize); else initialize();
        return { changeView: changeView, toggleView: toggleView, openDetail: openDetail, openAction: openAction, openDay: openDay, closeDay: closeDay, createMeeting: createMeeting };
    })();
</script>
<script type="text/javascript">
    // Tooltip ưu tiên phải, nếu không đủ chỗ thì chuyển trái; không bị cắt bởi grid/scroll.
    (function () {
        if (window.meetCalendarTooltipDelegated) return;
        window.meetCalendarTooltipDelegated = true;
        var floatTip = null;
        function removeTip() { if (floatTip && floatTip.parentNode) floatTip.parentNode.removeChild(floatTip); floatTip = null; }
        function placeTip(el, anchor) {
            var rect = anchor.getBoundingClientRect(), w = el.offsetWidth, h = el.offsetHeight, gap = 12;
            var rightAvailable = window.innerWidth - rect.right - 12, leftAvailable = rect.left - 12;
            var x = rightAvailable >= w + gap ? rect.right + gap : (leftAvailable >= w + gap ? rect.left - w - gap : Math.max(10, Math.min(rect.right + gap, window.innerWidth - w - 10)));
            var y = Math.max(10, Math.min(rect.top + (rect.height - h) / 2, window.innerHeight - h - 10));
            el.style.left = Math.round(x) + 'px'; el.style.top = Math.round(y) + 'px'; el.classList.toggle('is-left', x < rect.left);
        }
        function showTip(button) {
            var content = button.querySelector('.meet-cal-popover'); if (!content) return;
            removeTip(); floatTip = document.createElement('div'); floatTip.className = 'meet-cal-float-tip'; floatTip.setAttribute('role', 'tooltip');
            floatTip.innerHTML = content.innerHTML; document.body.appendChild(floatTip); placeTip(floatTip, button);
        }
        document.addEventListener('pointerover', function (e) { var ev = e.target.closest && e.target.closest('.meet-cal-event'); if (ev && (!e.relatedTarget || !ev.contains(e.relatedTarget))) showTip(ev); });
        document.addEventListener('pointerout', function (e) { var ev = e.target.closest && e.target.closest('.meet-cal-event'); if (ev && (!e.relatedTarget || !ev.contains(e.relatedTarget))) removeTip(); });
        document.addEventListener('focusin', function (e) { var ev = e.target.closest && e.target.closest('.meet-cal-event'); if (ev) showTip(ev); });
        document.addEventListener('focusout', function (e) { if (e.target.closest && e.target.closest('.meet-cal-event')) removeTip(); });
        document.addEventListener('click', function (e) {
            if (e.target.closest && e.target.closest('.meet-cal-event')) { removeTip(); return; }
            var day = e.target.closest && e.target.closest('.meet-cal-day.has-meetings');
            if (day) { e.preventDefault(); SweetMeetCalendar.openDay(day); }
        });
        document.addEventListener('keydown', function (e) {
            if ((e.key === 'Enter' || e.key === ' ') && e.target.matches('.meet-cal-day.has-meetings')) { e.preventDefault(); SweetMeetCalendar.openDay(e.target); }
            if (e.key === 'Escape') removeTip();
        });
        window.addEventListener('scroll', removeTip, true); window.addEventListener('resize', removeTip);
        if (window.Sys && Sys.Application) Sys.Application.add_load(removeTip);
    })();
</script>
<script type="text/javascript">
    (function () {
        if (window.meetStatusAutoRefreshTimer) return;

        window.meetStatusAutoRefreshTimer = window.setInterval(function () {
            // Tránh mất các giờ công PM đang nhập khi bộ đếm tự đồng bộ trạng thái.
            if (document.querySelector('.meet-view-shell.meet-mode-labor')) return;
            // Không thay giao diện đang xem trong lúc popup mở hoặc một async postback khác còn chạy.
            if ($('.modal.show:visible, .modal.in:visible, .offcanvas.show:visible').length > 0) return;
            if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                var requestManager = Sys.WebForms.PageRequestManager.getInstance();
                if (requestManager && requestManager.get_isInAsyncPostBack()) return;
            }
            // Không gửi lại ID của cuộc họp vừa mở khiến timer tự mở lại popup chi tiết.
            var selected = document.getElementById('meetCalendarSelectedId');
            if (selected) selected.value = '';
            var kind = document.getElementById('meetCalendarActionType');
            if (kind) kind.value = '';
            var refreshButton = document.getElementById('<%= btnRefreshMeetingStatuses.ClientID %>');
            if (refreshButton) refreshButton.click();
        }, 60000);
    })();
</script>
