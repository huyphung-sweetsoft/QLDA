<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="LichCaNhan.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fNhanVien.LichCaNhan" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        /* Giao diện giữ bố cục CtrlMeet; ngữ nghĩa trạng thái kế thừa lịch nhân viên: đỏ / xanh / vàng / xám. */
        .personal-cal { --pc-primary:#54218d;--pc-primary-dark:#42166f;--pc-border:#e7dfef;--pc-muted:#786e84;--pc-surface:#faf8fd;color:#382a49;min-width:0; }
        .personal-cal .personal-cal-surface { padding:13px 14px 15px;border:1px solid #e9e2f0;border-radius:12px;background:#fff;box-shadow:0 3px 15px rgba(52,29,80,.045); }
        .personal-cal .personal-cal-toolbar { display:grid;grid-template-columns:minmax(0,1fr) auto minmax(0,1fr);align-items:center;gap:12px;margin-bottom:11px; }
        .personal-cal .personal-cal-heading { display:flex;align-items:center;gap:10px;min-width:0; }
        .personal-cal .personal-cal-heading-icon { display:grid;place-items:center;width:40px;height:40px;flex:0 0 40px;border:1px solid #e2d3f1;border-radius:11px;background:#f3ebfd;color:var(--pc-primary);font-size:17px; }
        .personal-cal .personal-cal-heading-copy { display:flex;flex-direction:column;min-width:0; }
        .personal-cal .personal-cal-period { display:block;margin:0;color:#352048;font-size:20px;font-weight:800;line-height:1.25;white-space:nowrap; }
        .personal-cal .personal-cal-subtitle { display:block;margin-top:3px;max-width:100%;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;color:var(--pc-muted);font-size:11.5px;font-weight:600; }
        .personal-cal .personal-cal-navigation { display:flex;align-items:center;justify-content:center;gap:5px; }
        .personal-cal .personal-cal-views { display:flex;align-items:center;justify-content:flex-end;min-width:0; }
        .personal-cal .personal-cal-view-switch { display:inline-flex;align-items:center;gap:3px;padding:3px;border:1px solid #e0d3ee;border-radius:9px;background:#f8f4fc; }
        .personal-cal .personal-cal-btn { display:inline-flex;align-items:center;justify-content:center;gap:6px;min-height:34px;min-width:35px;padding:6px 11px;border:1px solid #e0d4ed;border-radius:8px;background:#fff;color:#5a367c;font-size:12px;font-weight:750;line-height:1.2;text-decoration:none!important;white-space:nowrap;transition:background-color .16s,border-color .16s,transform .16s,color .16s; }
        .personal-cal .personal-cal-btn:hover { background:#f3ebfd;border-color:#c6a9e2;color:var(--pc-primary); }
        .personal-cal .personal-cal-btn:focus-visible { outline:2px solid #8a60bf;outline-offset:2px; }
        .personal-cal .personal-cal-view-switch .personal-cal-btn { min-width:69px;border-color:transparent;background:transparent;border-radius:6px; }
        .personal-cal .personal-cal-view-switch .personal-cal-btn.active { background:var(--pc-primary);border-color:var(--pc-primary);color:#fff;box-shadow:0 2px 6px rgba(84,33,141,.15); }
        .personal-cal .personal-cal-view-switch .personal-cal-btn:hover:not(.active) { background:#eee4f8; }
        .personal-cal .personal-cal-legend { display:flex;align-items:center;flex-wrap:wrap;gap:7px 16px;margin-bottom:11px;padding:8px 11px;border:1px solid #eee8f5;border-radius:9px;background:#faf8fd;color:#71667d;font-size:11.5px; }
        .personal-cal .personal-cal-legend-item { display:inline-flex;align-items:center;gap:6px;white-space:nowrap; }
        .personal-cal .personal-cal-legend-dot { width:9px;height:9px;flex:0 0 9px;border-radius:50%;background:#ef4444; }
        .personal-cal .personal-cal-legend-dot.off { background:#f59e0b; }
        .personal-cal .personal-cal-legend-dot.free { background:#22c55e; }
        .personal-cal .personal-cal-legend-dot.weekend { background:#94a3b8; }
        .personal-cal .personal-cal-legend-hint { margin-left:auto;color:#857990; }
        .personal-cal .personal-cal-scroll { overflow-x:auto;padding:1px 1px 4px;scrollbar-width:thin; }
        .personal-cal .personal-cal-grid { display:grid;grid-template-columns:repeat(7,minmax(0,1fr));gap:1px;min-width:840px;overflow:hidden;border:1px solid #ddd4e8;border-radius:11px;background:#ddd4e8;isolation:isolate; }
        .personal-cal .personal-cal-dow { padding:10px 4px;background:#f3edfa;color:#583486;font-size:12px;font-weight:800;text-align:center; }
        .personal-cal .personal-cal-dow.is-weekend { color:#a56849; }
        .personal-cal .personal-cal-day { position:relative;display:flex;flex-direction:column;gap:7px;min-width:0;min-height:clamp(103px,calc((100dvh - 295px)/6),132px);padding:8px 7px;background:#fff;box-shadow:inset 0 0 0 1px #eee7f5;transition:background-color .17s,box-shadow .17s; }
        .personal-cal .personal-cal-grid.is-week .personal-cal-day { min-height:clamp(200px,calc(100dvh - 315px),390px); }
        .personal-cal .personal-cal-day.is-outside { background:#fbfafc; }
        .personal-cal .personal-cal-day.is-holiday { background:#fffcf6; }
        .personal-cal .personal-cal-day.is-weekend { background:#f8fafc; }
        .personal-cal .personal-cal-day.is-today { z-index:1;background:#faf5ff;box-shadow:inset 0 0 0 2px #aa84d6; }
        .personal-cal .personal-cal-day.is-clickable { cursor:pointer; }
        .personal-cal .personal-cal-day:hover { z-index:2;background:#faf6ff;box-shadow:inset 0 0 0 2px #d2baed; }
        .personal-cal .personal-cal-day.is-holiday:hover { background:#fff7e8; }
        .personal-cal .personal-cal-day.is-weekend:hover { background:#f1f5f9; }
        .personal-cal .personal-cal-day.is-today:hover { box-shadow:inset 0 0 0 2px #7440b1; }
        .personal-cal .personal-cal-day:focus-visible { outline:2px solid #7948b8;outline-offset:-3px;z-index:3; }
        .personal-cal .personal-cal-day-top { display:flex;align-items:center;justify-content:space-between;gap:3px;min-width:0;min-height:23px; }
        .personal-cal .personal-cal-day-number { display:inline-grid;place-items:center;flex:0 0 auto;min-width:26px;height:26px;padding:0 5px;border-radius:8px;color:#40344e;font-size:13px;font-weight:800; }
        .personal-cal .personal-cal-day.is-today .personal-cal-day-number { background:var(--pc-primary);color:#fff; }
        .personal-cal .personal-cal-day.is-outside .personal-cal-day-number { color:#aaa2b1; }
        .personal-cal .personal-cal-day-tags { display:flex;align-items:center;justify-content:flex-end;gap:4px;min-width:0; }
        /* Badge có công việc: theo màu đỏ của lịch nhân viên, không dùng tím cho trạng thái. */
        .personal-cal .personal-cal-day-count { display:inline-flex;align-items:center;justify-content:center;gap:4px;min-width:27px;padding:4px 7px;border:1px solid #fecaca;border-radius:7px;background:#fee2e2;color:#b91c1c;font-size:10.5px;font-weight:850;line-height:1.15; }
        /* Tháng: chỉ thống kê số lượng, không liệt kê tên task/họp. */
        .personal-cal .personal-cal-month-summary { display:flex;flex:1 1 auto;flex-direction:column;justify-content:center;gap:6px;width:100%;min-width:0; }
        .personal-cal .personal-cal-month-line { display:flex;align-items:center;gap:7px;min-width:0;min-height:34px;padding:7px 9px;border:1px solid transparent;border-left-width:3px;border-radius:7px;font-size:11px;font-weight:800;line-height:1.3; }
        .personal-cal .personal-cal-month-line i { flex:0 0 14px;font-size:11px;text-align:center; }
        .personal-cal .personal-cal-month-line span { min-width:0;overflow-wrap:anywhere; }
        .personal-cal .personal-cal-month-line.is-task { border-color:#fecdd3;border-left-color:#ef4444;background:#fff1f2;color:#b91c1c; }
        .personal-cal .personal-cal-month-line.is-meeting { border-color:#e0cef5;border-left-color:#7440ae;background:#f5effd;color:#622e97; }
        .personal-cal .personal-cal-month-line.is-meeting i { color:#7440ae; }
        .personal-cal .personal-cal-day.is-outside .personal-cal-month-summary { opacity:.78; }
        /* Tuần: mỗi hoạt động hiện đầy đủ, ô ngày tự dài theo dữ liệu. */
        .personal-cal .personal-cal-grid.is-week .personal-cal-day { height:auto; }
        .personal-cal .personal-cal-grid.is-week .personal-cal-day-events { padding-bottom:6px; }
        /* Ngày nghỉ có công việc: ribbon gọn nhưng luôn phủ kín chiều rộng. */
        .personal-cal .personal-cal-day-state { display:flex;align-items:center;justify-content:center;gap:6px;width:100%;min-width:0;padding:6px 7px;overflow:hidden;border:1px solid transparent;border-radius:7px;font-size:10.5px;font-weight:750;line-height:1.35; }
        .personal-cal .personal-cal-day-state .personal-cal-state-text { min-width:0;overflow:hidden;text-overflow:ellipsis;white-space:nowrap; }
        .personal-cal .personal-cal-day-state.is-holiday { border-color:#fcd58c;background:#fffbeb;color:#a16207; }
        .personal-cal .personal-cal-day-state.is-weekend { border-color:#dbe3ee;background:#f1f5f9;color:#64748b; }
        /* Trạng thái khi không có việc: thay badge nhỏ bằng panel phủ đủ ô ngày. */
        .personal-cal .personal-cal-status-panel { display:flex;flex:1 1 auto;flex-direction:column;align-items:center;justify-content:center;gap:4px;width:100%;min-width:0;min-height:62px;padding:9px 6px;border:1px solid transparent;border-radius:8px;text-align:center; }
        .personal-cal .personal-cal-status-icon { display:grid;place-items:center;flex:0 0 auto;width:27px;height:27px;border-radius:9px;font-size:12px; }
        .personal-cal .personal-cal-status-name { display:block;max-width:100%;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;font-size:11.5px;font-weight:800;line-height:1.25; }
        .personal-cal .personal-cal-status-desc { display:block;max-width:100%;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;font-size:10px;font-weight:550;line-height:1.25;opacity:.79; }
        .personal-cal .personal-cal-status-panel.is-free { border-color:#d5f0df;background:#f0fdf4;color:#15803d; }
        .personal-cal .personal-cal-status-panel.is-free .personal-cal-status-icon { background:#dcfce7;color:#16a34a; }
        .personal-cal .personal-cal-status-panel.is-weekend { border-color:#e2e8f0;background:#f1f5f9;color:#64748b; }
        .personal-cal .personal-cal-status-panel.is-weekend .personal-cal-status-icon { background:#e2e8f0;color:#64748b; }
        .personal-cal .personal-cal-status-panel.is-holiday { border-color:#fde1a8;background:#fffbeb;color:#a16207; }
        .personal-cal .personal-cal-status-panel.is-holiday .personal-cal-status-icon { background:#fef3c7;color:#b45309; }
        .personal-cal .personal-cal-status-panel.is-busy { border-color:#fecaca;background:#fff1f2;color:#b91c1c; }
        .personal-cal .personal-cal-status-panel.is-busy .personal-cal-status-icon { background:#fee2e2;color:#dc2626; }
        .personal-cal .personal-cal-day.is-outside .personal-cal-status-panel { opacity:.72; }
        .personal-cal .personal-cal-day-events { display:flex;flex-direction:column;gap:5px;min-width:0; }
        .personal-cal .personal-cal-event { display:flex;align-items:center;gap:5px;min-width:0;min-height:28px;padding:5px 6px;border:1px solid #fecdd3;border-left:3px solid #ef4444;border-radius:6px;background:#fff1f2;color:#a51d32;font-size:11px;font-weight:700;line-height:1.3;transition:background-color .16s,border-color .16s; }
        .personal-cal .personal-cal-event:hover,.personal-cal .personal-cal-event:focus-visible { background:#ffe4e6;border-color:#fda4af; }
        .personal-cal .personal-cal-event i { flex:0 0 auto;font-size:10px;color:#e11d48; }
        .personal-cal .personal-cal-event-code { flex:0 1 auto;max-width:42px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;font-size:10px;font-weight:850; }
        .personal-cal .personal-cal-event-name { display:block;min-width:0;overflow:hidden;text-overflow:ellipsis;white-space:nowrap; }
        .personal-cal .personal-cal-more { display:inline-flex;align-items:center;gap:5px;align-self:flex-start;max-width:100%;padding:1px 3px;border:0;background:transparent;color:#b91c1c;font-size:10.5px;font-weight:800; }
        .personal-cal .personal-cal-day-note { margin-top:2px;color:#b91c1c;font-size:10.5px;font-weight:600;line-height:1.4;overflow:hidden;text-overflow:ellipsis; }
        .personal-cal .personal-cal-grid.is-week .personal-cal-event { min-height:33px;padding:7px 8px;font-size:12px; }
        .personal-cal .personal-cal-grid.is-week .personal-cal-day-events { gap:7px; }
        .personal-cal .personal-cal-grid.is-week .personal-cal-day { gap:10px;padding:10px; }
        .personal-cal .personal-cal-grid.is-week .personal-cal-status-panel { min-height:120px; }
        .personal-cal .personal-cal-grid.is-week .personal-cal-status-icon { width:34px;height:34px;font-size:15px; }
        .personal-cal .personal-cal-grid.is-week .personal-cal-status-name { font-size:12px; }
        .personal-cal .personal-cal-empty { padding:26px 12px;color:#7a6e86;text-align:center;font-size:13px; }
        /* Tooltip nổi kiểu CtrlMeet: gắn vào body để không bị khung lịch cắt mất. */
        .personal-cal-tooltip { position:fixed;z-index:1080;width:min(300px,calc(100vw - 24px));padding:12px 13px;border:1px solid #d7c4eb;border-radius:10px;background:#fff;box-shadow:0 12px 30px rgba(53,28,82,.20);color:#453650;pointer-events:none;animation:personalCalTipIn .14s ease-out; }
        .personal-cal-tooltip strong { display:block;margin-bottom:7px;color:#9f1239;font-size:13px;font-weight:800;line-height:1.4;overflow-wrap:anywhere; }
        .personal-cal-tooltip span { display:flex;align-items:flex-start;gap:7px;margin-top:5px;color:#766b80;font-size:11.5px;line-height:1.5;overflow-wrap:anywhere; }
        .personal-cal-tooltip i { margin-top:3px;color:#e11d48; }
        @keyframes personalCalTipIn { from { opacity:0;transform:translateY(4px); } to { opacity:1;transform:translateY(0); } }
        /* ExtraModal: tách rõ nhóm dự án và danh sách công việc. */
        .personal-day-modal { max-height:min(66vh,620px);padding:14px 16px;overflow-y:auto;background:#fff; }
        .personal-day-intro { display:flex;align-items:center;gap:10px;margin-bottom:13px;padding:11px 13px;border:1px solid #fecaca;border-radius:10px;background:#fff1f2;color:#9f1239; }
        .personal-day-intro-icon { display:grid;place-items:center;width:33px;height:33px;flex:0 0 33px;border-radius:9px;background:#ffe4e6;font-size:15px; }
        .personal-day-intro-title { display:block;font-size:13px;font-weight:800; }
        .personal-day-intro-subtitle { display:block;margin-top:2px;color:#9f5e6b;font-size:11.5px; }
        .personal-day-project { margin-bottom:11px;overflow:hidden;border:1px solid #e8e1ef;border-radius:10px;background:#fff; }
        .personal-day-project-head { display:flex;align-items:center;justify-content:space-between;gap:8px;padding:10px 12px;border-bottom:1px solid #ebe3f3;background:#f7f2fd;color:#563082;font-size:13px;font-weight:800; }
        .personal-day-project-head span { min-width:0;overflow-wrap:anywhere; }
        .personal-day-project-count { flex:0 0 auto;padding:3px 7px;border:1px solid #e4d6f3;border-radius:6px;background:#fff;color:#6f4897;font-size:11px; }
        .personal-day-task { display:flex;align-items:flex-start;gap:9px;padding:10px 12px;color:#433d4d;font-size:12.5px;line-height:1.5; }
        .personal-day-task + .personal-day-task { border-top:1px dashed #e7e1ed; }
        .personal-day-task-code { flex:0 0 auto;padding:3px 7px;border:1px solid #fecdd3;border-radius:6px;background:#fff1f2;color:#be123c;font-size:11px;font-weight:800; }
        .personal-day-task-name { min-width:0;overflow-wrap:anywhere;font-weight:650; }
        .personal-day-notice { display:flex;align-items:center;gap:10px;padding:17px;border:1px solid #e9dfef;border-radius:10px;background:#faf8fd;color:#746780;font-size:13px;font-weight:650; }
        .personal-day-notice.is-holiday { border-color:#fde1a8;background:#fffbeb;color:#a16207; }
        .personal-day-notice.is-weekend { border-color:#e2e8f0;background:#f1f5f9;color:#64748b; }
        .personal-day-notice i { font-size:19px; }
        @media(max-width:1150px) { .personal-cal .personal-cal-toolbar { grid-template-columns:minmax(0,1fr) auto; } .personal-cal .personal-cal-navigation { grid-column:1/-1;grid-row:2; } .personal-cal .personal-cal-views { grid-column:2;grid-row:1; } }
        @media(max-width:640px) { .personal-cal .personal-cal-surface { padding:10px 9px 12px; } .personal-cal .personal-cal-toolbar { grid-template-columns:minmax(0,1fr) auto;gap:9px; } .personal-cal .personal-cal-heading-icon { width:34px;height:34px;flex-basis:34px; } .personal-cal .personal-cal-period { font-size:17px; } .personal-cal .personal-cal-subtitle { max-width:210px; } .personal-cal .personal-cal-navigation { grid-column:1/-1;grid-row:2; } .personal-cal .personal-cal-view-switch .personal-cal-btn { min-width:59px;padding:6px 8px; } .personal-cal .personal-cal-legend { gap:6px 10px; } .personal-cal .personal-cal-legend-hint { margin-left:0; } .personal-day-modal { padding:10px; } }
        @media(prefers-reduced-motion:reduce) { .personal-cal .personal-cal-btn,.personal-cal .personal-cal-day,.personal-cal .personal-cal-event { transition:none; } .personal-cal-tooltip { animation:none; } }

        /* Lịch cá nhân: tất cả cuộc họp chưa kết thúc đều dùng màu tím; họp đang diễn ra thêm chấm pulse, đã kết thúc màu xám.
           Không đổi màu đỏ/xanh/vàng/xám của ngày và công việc trong LichCaNhan. */
        .personal-cal .personal-cal-legend-dot.meeting { background:#7440ae; }
        .personal-cal .personal-cal-day-meeting-count { display:inline-flex;align-items:center;justify-content:center;gap:4px;padding:4px 7px;border:1px solid #d8c6f0;border-radius:7px;background:#f1e9fb;color:#633498;font-size:10.5px;font-weight:850;line-height:1.15; }
        .personal-cal .personal-cal-event.is-meeting { position:relative;border-color:#d6c2ed;border-left-color:#6431a0;background:#f2ecfa;color:#54227e; }
        .personal-cal .personal-cal-event.is-meeting:hover { background:#eae0f7;border-color:#c8afe8; }
        .personal-cal .personal-cal-event.is-meeting.is-status-1 { border-color:#c6a6ed;border-left-color:#8247c3;background:#efe5ff;color:#542083; }
        .personal-cal .personal-cal-event.is-meeting.is-status-2 { border-color:#d6c2ed;border-left-color:#6431a0;background:#f2ecfa;color:#54227e; }
        .personal-cal .personal-cal-event.is-meeting.is-status-3 { border-color:#d8dfe8;border-left-color:#94a3b8;background:#f3f5f8;color:#637287; }
        .personal-cal .personal-cal-event.is-meeting .personal-meeting-host { color:#d69a19!important;font-size:11px; }
        .personal-cal .personal-cal-meeting-time { flex:0 0 auto;font-weight:850;font-size:10.5px; }
        .personal-cal .personal-cal-event.is-meeting .personal-meeting-pulse { position:absolute;right:5px;bottom:5px;width:8px;height:8px;border-radius:50%;background:#874ac8; }
        .personal-cal .personal-cal-event.is-meeting.is-status-1 .personal-meeting-pulse { animation:personalSoonPulse 2s ease-in-out infinite; }
        .personal-cal .personal-cal-event.is-meeting.is-status-2 .personal-meeting-pulse { background:#874ac8;animation:personalLivePulse 1.7s ease-in-out infinite; }
        @keyframes personalSoonPulse { 50% { box-shadow:0 0 0 4px rgba(135,74,200,.19); } }
        @keyframes personalLivePulse { 50% { box-shadow:0 0 0 4px rgba(135,74,200,.24); } }
        .personal-cal-tooltip.is-meeting { width:min(390px,calc(100vw - 24px));border-color:#c9b1e5; }
        .personal-cal-tooltip.is-meeting strong { color:#4b187a;font-size:14px; }
        .personal-cal-tooltip.is-meeting i { color:#7440ae; }
        .personal-cal-tooltip .personal-cal-tooltip-status { display:inline-block!important;margin:1px 0 8px!important;padding:4px 8px;border-radius:7px;background:#f1e9fb;color:#633498!important;font-weight:800; }
        .personal-cal-tooltip .personal-cal-tooltip-status.is-status-2 { color:#633498!important;background:#f1e9fb; }
        .personal-cal-tooltip .personal-cal-tooltip-status.is-status-3 { color:#637287!important;background:#f3f5f8; }
        .personal-day-meeting-head { display:flex;align-items:center;gap:9px;margin:12px 0 9px;padding:9px 12px;border:1px solid #e1d4f0;border-radius:9px;background:#f7f2fd;color:#552087;font-weight:800;font-size:13px; }
        .personal-day-meeting { margin-bottom:9px;padding:11px 13px;border:1px solid #d6c2ed;border-left:4px solid #6431a0;border-radius:10px;background:#fbf9fe; }
        .personal-day-meeting.is-status-1 { border-left-color:#8247c3;background:#f7f2ff; }
        .personal-day-meeting.is-status-2 { border-color:#d6c2ed;border-left-color:#6431a0;background:#fbf9fe; }
        .personal-day-meeting.is-status-3 { border-color:#d8dfe8;border-left-color:#94a3b8;background:#f8fafc; }
        .personal-day-meeting-top { display:flex;align-items:center;justify-content:space-between;gap:8px;margin-bottom:8px;color:#6431a0;font-size:11.5px; }
        .personal-day-meeting.is-status-2 .personal-day-meeting-top { color:#6431a0; }
        .personal-day-meeting.is-status-3 .personal-day-meeting-top { color:#64748b; }
        .personal-day-meeting-status { display:inline-flex;align-items:center;gap:7px;white-space:nowrap;padding:4px 8px;border-radius:30px;background:rgba(110,60,150,.09);font-size:10.5px;font-weight:800; }
        .personal-day-meeting-status .personal-meeting-pulse { display:inline-block;flex:0 0 8px;width:8px;height:8px;border-radius:50%;background:#874ac8; }
        .personal-day-meeting.is-status-2 .personal-day-meeting-status .personal-meeting-pulse { animation:personalLivePulse 1.7s ease-in-out infinite; }
        .personal-day-meeting-name { color:#322840;font-size:13px;font-weight:800;line-height:1.45;overflow-wrap:anywhere; }
        .personal-day-meeting-name .personal-meeting-host { color:#d69a19; }
        .personal-day-meeting-meta { display:flex;gap:8px;align-items:flex-start;margin-top:6px;color:#62576d;font-size:11.5px;line-height:1.45;overflow-wrap:anywhere; }
        .personal-day-meeting-meta i { flex:0 0 14px;margin-top:3px;color:#7440ae; }
        .personal-day-join { display:inline-flex;align-items:center;gap:6px;margin-top:10px;padding:6px 10px;border:1px solid #d9c6f0;border-radius:7px;background:#f0e8fb;color:#54218d;font-size:11px;font-weight:750;text-decoration:none!important; }
        .personal-day-join:hover { background:#e7d8fa;color:#42166f; }
        @media(prefers-reduced-motion:reduce) { .personal-cal .personal-meeting-pulse,.personal-day-meeting .personal-meeting-pulse { animation:none!important; } }


        /* ============================================================
           FLIP 3D - chỉ lịch tháng: giữ nguyên mặt trước, lật trên trục X.
           Lịch tuần vẫn hiện đầy đủ công việc và hover tooltip như hiện tại.
           ============================================================ */
        .personal-cal .personal-cal-day.has-flip {
            display:flex; flex-direction:column; gap:0;
            /* Chừa 1px nền lưới ở đáy để viền ngang không bị lớp 3D phủ mất. */
            padding:0 0 1px;
            background:#ddd4e8!important; box-shadow:none!important;
            perspective:1200px; -webkit-perspective:1200px;
            isolation:isolate;
        }
        /* Cả ngày chỉ có công việc / chỉ có lịch họp / có cả hai:
           các dòng đều bắt đầu ngay bên dưới số ngày, không căn giữa ô nữa. */
        .personal-cal .personal-cal-grid.is-month .personal-cal-day.has-flip .personal-cal-month-summary {
            flex:0 0 auto;
            justify-content:flex-start;
            margin-top:0;
        }
        /* Viền dưới ngày hiện tại giữ tím, các ngày khác theo màu đường lưới. */
        .personal-cal .personal-cal-day.has-flip.is-today {
            background:#aa84d6!important;
        }
        .personal-cal .personal-cal-day.has-flip .personal-cal-flip-inner {
            position:relative; display:flex; flex:1 1 auto; flex-direction:column;
            width:100%; min-height:0;
            transform:rotateX(0deg); transform-style:preserve-3d;
            -webkit-transform-style:preserve-3d;
            transition:transform .74s cubic-bezier(.22,.70,.19,1);
            will-change:transform;
        }
        @media(hover:hover) and (pointer:fine) {
            .personal-cal .personal-cal-day.has-flip:hover .personal-cal-flip-inner {
                transform:rotateX(180deg);
            }
        }
        .personal-cal .personal-cal-day.has-flip:focus-within .personal-cal-flip-inner,
        .personal-cal .personal-cal-day.has-flip.is-flip-open .personal-cal-flip-inner {
            transform:rotateX(180deg);
        }
        .personal-cal .personal-cal-day.has-flip .personal-cal-flip-face {
            box-sizing:border-box; width:100%;
            backface-visibility:hidden; -webkit-backface-visibility:hidden;
        }
        .personal-cal .personal-cal-day.has-flip .personal-cal-flip-front {
            position:relative; display:flex; flex:1 1 auto; flex-direction:column;
            gap:7px; min-height:0; padding:8px 7px;
            background:#fff; box-shadow:inset 0 0 0 1px #eee7f5;
            transform:translateZ(1px);
        }
        .personal-cal .personal-cal-day.has-flip.is-outside .personal-cal-flip-front {background:#fbfafc;}
        .personal-cal .personal-cal-day.has-flip.is-holiday .personal-cal-flip-front {background:#fffcf6;}
        .personal-cal .personal-cal-day.has-flip.is-weekend .personal-cal-flip-front {background:#f8fafc;}
        .personal-cal .personal-cal-day.has-flip.is-today .personal-cal-flip-front {
            background:#faf5ff;box-shadow:inset 0 0 0 2px #aa84d6;
        }
        .personal-cal .personal-cal-day.has-flip .personal-cal-flip-back {
            position:absolute; inset:0;display:flex; flex-direction:column;
            align-items:center; justify-content:center; gap:5px;
            padding:8px 6px;
            overflow:visible;
            color:#fff;
            background:#6f3aa8;
            border:1px solid #7f55b5;
            box-shadow:inset 0 0 0 1px rgba(255,255,255,.05);
            transform:rotateX(180deg) translateZ(1px);
            transform-style:preserve-3d;
            -webkit-transform-style:preserve-3d;
            text-align:center;
        }
        .personal-cal .personal-cal-flip-back::before {
            content:none;
        }
        .personal-cal .personal-cal-flip-date {
            position:relative;display:block;max-width:100%;
            font-size:11.5px;font-weight:850;line-height:1.15;
            white-space:nowrap;
            transform:translateZ(15px);
        }
        .personal-cal .personal-cal-flip-summary {
            position:relative;max-width:100%;font-size:10px;
            font-weight:650;line-height:1.15;color:#f0e6fb;
            white-space:nowrap;overflow:hidden;text-overflow:ellipsis;
            transform:translateZ(18px);
        }
        .personal-cal .personal-cal-flip-action {
            position:relative;z-index:1;display:inline-flex;align-items:center;justify-content:center;
            gap:6px;flex:0 0 auto;min-height:33px;max-width:100%;
            margin-top:3px;padding:7px 10px;
            border:1px solid rgba(255,255,255,.85);border-radius:8px;
            color:#4d197f;background:linear-gradient(180deg,#fff,#f1e8fb);
            box-shadow:0 5px 0 rgba(47,11,89,.29),0 12px 19px rgba(30,9,67,.31),
                       0 1px 1px rgba(255,255,255,.8) inset;
            font-size:11px;font-weight:850;line-height:1.1;
            white-space:nowrap;cursor:pointer;
            transform:translateZ(38px);
            transition:background-color .16s,box-shadow .16s,transform .16s;
        }
        .personal-cal .personal-cal-flip-action:hover,
        .personal-cal .personal-cal-flip-action:focus-visible {
            background:#fff;
            box-shadow:0 7px 0 rgba(47,11,89,.24),0 14px 20px rgba(30,9,67,.35);
            transform:translateZ(43px) translateY(-2px);
        }
        .personal-cal .personal-cal-flip-action:focus-visible {
            outline:2px solid #fff;outline-offset:2px;
        }
        /* Tuần: giữ tương tác tooltip theo từng task/cuộc họp,
           dùng nút chi tiết trực tiếp thay vì lật toàn bộ danh sách dài. */
        .personal-cal .personal-cal-week-detail {
            display:inline-flex;align-items:center;justify-content:center;gap:6px;
            align-self:center;margin-top:auto;padding:6px 9px;min-height:30px;
            border:1px solid #d6c0ed;border-radius:7px;
            background:#f4edfd;color:#59278c;
            font-size:11px;font-weight:800;white-space:nowrap;cursor:pointer;
            transition:border-color .16s,background-color .16s,transform .16s;
        }
        .personal-cal .personal-cal-week-detail:hover,
        .personal-cal .personal-cal-week-detail:focus-visible {
            background:#e9ddf9;border-color:#b990de;transform:translateY(-1px);
        }
        .personal-cal .personal-cal-week-detail:focus-visible {outline:2px solid #8155bd;outline-offset:2px;}
        @media(prefers-reduced-motion:reduce) {
            .personal-cal .personal-cal-day.has-flip .personal-cal-flip-inner,
            .personal-cal .personal-cal-flip-action,
            .personal-cal .personal-cal-week-detail {transition:none!important;}
        }

        .personal-cal .personal-cal-day.is-clickable {cursor:default;}
        .personal-cal .personal-cal-day.has-flip .personal-cal-flip-front {cursor:pointer;}

        /* V11: panel lịch tháng tô kín như ngày trống/cuối tuần; không thay đổi chế độ tuần. */
        .personal-cal .personal-cal-grid.is-month .personal-cal-month-tile { position:relative;display:flex;flex:1 1 auto;align-items:center;justify-content:center;min-height:64px;width:100%;min-width:0;overflow:hidden;border:1px solid transparent;border-radius:9px;isolation:isolate; }
        .personal-cal .personal-cal-month-tile.is-tasks-only { background:#fff1f2;border-color:#fecaca;color:#b91c1c; }
        .personal-cal .personal-cal-month-tile.is-meetings-only { background:#f5effd;border-color:#dfcafa;color:#633498; }
        .personal-cal .personal-cal-month-tile.is-mixed { background:linear-gradient(135deg,#fff1f2 0%,#fff1f2 49.4%,#debfea 49.6%,#debfea 50.4%,#f4edfc 50.6%,#f4edfc 100%);border-color:#ead6ef; }
        .personal-cal .personal-cal-month-tile-content { display:flex;flex-direction:column;align-items:center;justify-content:center;gap:5px;min-width:0;text-align:center; }
        .personal-cal .personal-cal-month-tile-content strong { display:block;font-size:11px;font-weight:800;line-height:1.3;white-space:normal;text-wrap:balance; }
        .personal-cal .personal-cal-month-tile-icon { display:grid;place-items:center;width:29px;height:29px;flex:0 0 29px;border-radius:9px;font-size:13px; }
        .personal-cal .personal-cal-month-tile-content.is-task { color:#b91c1c; }
        .personal-cal .personal-cal-month-tile-content.is-meeting { color:#633498; }
        .personal-cal .personal-cal-month-tile-content.is-task .personal-cal-month-tile-icon { background:#ffe0e3;color:#d9273e; }
        .personal-cal .personal-cal-month-tile-content.is-meeting .personal-cal-month-tile-icon { background:#eadbfa;color:#713cad; }
        .personal-cal .personal-cal-month-tile.is-mixed .personal-cal-month-tile-content { position:absolute;gap:3px;width:54%;min-width:0; }
        .personal-cal .personal-cal-month-tile.is-mixed .personal-cal-month-tile-content.is-task { left:4%;top:8%;height:53%;align-items:center;justify-content:center; }
        .personal-cal .personal-cal-month-tile.is-mixed .personal-cal-month-tile-content.is-meeting { right:4%;bottom:8%;height:53%;align-items:center;justify-content:center; }
        .personal-cal .personal-cal-month-tile.is-mixed .personal-cal-month-tile-icon { width:24px;height:24px;flex-basis:24px;font-size:11px; }
        .personal-cal .personal-cal-month-tile.is-mixed .personal-cal-month-tile-content strong { font-size:10px; }
        .personal-cal .personal-cal-month-tile.is-mixed .personal-cal-month-tile-content.is-task strong { transform:translateY(-3px); }
        .personal-cal .personal-cal-day.is-outside .personal-cal-month-tile { opacity:.78; }
        .personal-cal .personal-cal-day.has-flip .personal-cal-month-tile { margin:0; }
        .personal-cal .personal-cal-day.is-today .personal-cal-month-tile { box-shadow:0 0 0 1px rgba(116,64,177,.12); }
        @media(max-width:1050px) { .personal-cal .personal-cal-month-tile.is-mixed .personal-cal-month-tile-content { width:58%; } .personal-cal .personal-cal-month-tile.is-mixed .personal-cal-month-tile-content strong { font-size:9.5px; } }
        /* V11: modal lịch ngày: thông tin họp được tổ chức thành khối theo dự án, không có link tham gia. */
        .personal-day-section-head { display:flex;align-items:center;justify-content:space-between;gap:8px;margin:12px 0 10px;padding:10px 12px;border:1px solid #fbd1d7;border-radius:9px;background:#fff1f2;color:#a61d35;font-size:13px;font-weight:800; }
        .personal-day-section-head.is-meeting { border-color:#e5d5f4;background:#f7f2fd;color:#633498; }
        .personal-day-section-head-title { display:flex;align-items:center;gap:8px;min-width:0; }
        .personal-day-section-head-desc { color:inherit;opacity:.75;font-size:11px;font-weight:650;white-space:nowrap; }
        .personal-day-overview { display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:9px;margin-bottom:15px; }
        .personal-day-overview-item { display:flex;align-items:center;gap:10px;padding:11px 12px;border:1px solid #fbd1d7;border-radius:10px;background:#fff6f7;color:#b91c1c; }
        .personal-day-overview-item.is-meeting { border-color:#e6d9f4;background:#faf7fe;color:#633498; }
        .personal-day-overview-item i { display:grid;place-items:center;flex:0 0 32px;width:32px;height:32px;border-radius:9px;background:#ffe2e6; }
        .personal-day-overview-item.is-meeting i { background:#ede0fa; }
        .personal-day-overview-item strong { display:block;font-size:15px;line-height:1.2; }
        .personal-day-overview-item small { display:block;margin-top:2px;font-size:11px;color:#76647d; }
        .personal-day-meeting-project { margin:0 0 12px;overflow:hidden;border:1px solid #e6daf1;border-radius:11px;background:#fff; }
        .personal-day-meeting-project-head { display:flex;align-items:center;justify-content:space-between;gap:12px;padding:11px 13px;border-bottom:1px solid #e8dbf3;background:#f6effd;color:#552087; }
        .personal-day-meeting-project-name { display:flex;align-items:center;gap:8px;min-width:0;font-size:12.5px;font-weight:850;overflow-wrap:anywhere; }
        .personal-day-meeting-project-name i { flex:0 0 auto;color:#7440ae; }
        .personal-day-meeting-project-count { flex:0 0 auto;padding:4px 7px;border:1px solid #dcc7f1;border-radius:6px;background:#fff;color:#6b3b9f;font-size:10.5px;font-weight:800; }
        .personal-day-meeting-entry { display:grid;grid-template-columns:82px minmax(0,1fr);gap:13px;padding:13px 14px; }
        .personal-day-meeting-entry + .personal-day-meeting-entry { border-top:1px dashed #e8e2ef; }
        .personal-day-meeting-clock { display:flex;flex-direction:column;align-items:center;justify-content:center;align-self:start;gap:3px;padding:10px 4px;border:1px solid #e4d7f0;border-radius:9px;background:#faf6fe;color:#673797;font-size:12.5px;font-weight:850;font-variant-numeric:tabular-nums; }
        .personal-day-meeting-clock small { color:#9a8ba7;font-size:10px;font-weight:700; }
        .personal-day-meeting-content { min-width:0; }
        .personal-day-meeting-heading { display:flex;align-items:flex-start;justify-content:space-between;flex-wrap:wrap;gap:7px;margin-bottom:9px; }
        .personal-day-meeting-entry .personal-day-meeting-name { flex:1 1 180px;color:#352843;font-size:13px;font-weight:850;line-height:1.45; }
        .personal-day-meeting-entry .personal-day-meeting-status { align-self:flex-start;flex:0 0 auto;padding:4px 8px;color:#6c389e;font-size:10.5px; }
        .personal-day-meeting-entry.is-status-3 .personal-day-meeting-status { background:#f0f2f5;color:#64748b; }
        .personal-day-meeting-entry.is-status-1 .personal-day-meeting-status { background:#ede2fc;color:#6b329d; }
        .personal-day-meeting-meta-grid { display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:7px 12px; }
        .personal-day-meeting-detail { display:flex;align-items:flex-start;gap:7px;min-width:0;color:#655b71;font-size:11.5px;line-height:1.5;overflow-wrap:anywhere; }
        .personal-day-meeting-detail i { flex:0 0 14px;margin-top:3px;color:#8053ad; }
        .personal-day-meeting-detail strong { color:#4a3c58;font-weight:750; }
        .personal-day-meeting-detail.is-location { grid-column:1/-1; }
        .personal-day-meeting-detail.is-online { color:#6b5982; }
        .personal-day-meeting-entry.is-status-3 .personal-day-meeting-clock { background:#f8fafc;border-color:#dfe5eb;color:#64748b; }
        .personal-day-meeting-entry .personal-meeting-host { color:#d69a19; }
        .personal-day-meeting-entry.is-status-2 .personal-day-meeting-status .personal-meeting-pulse { animation:personalLivePulse 1.7s ease-in-out infinite; }
        @media(max-width:590px) { .personal-day-meeting-entry { grid-template-columns:66px minmax(0,1fr);gap:9px;padding:10px; } .personal-day-meeting-clock { font-size:11px; } .personal-day-meeting-meta-grid { grid-template-columns:1fr; } .personal-day-overview-item { gap:7px;padding:9px; } .personal-day-overview-item i { width:26px;height:26px;flex-basis:26px; } .personal-day-section-head-desc { display:none; } }

        /* V12: hai danh sách ngang, tiêu đề co dinh, chi cuon noi bo tung cot. */
        .modal-dialog:has(.personal-day-modal) { width:min(1120px,calc(100vw - 32px))!important;max-width:min(1120px,calc(100vw - 32px))!important; }
        .personal-day-modal { max-height:none;overflow:visible;padding:12px 14px;background:#fff; }
        .personal-day-dual { display:grid;grid-template-columns:minmax(0,1fr) minmax(0,1fr);gap:13px;align-items:stretch; }
        .personal-day-pane { display:flex;flex-direction:column;min-width:0;min-height:140px;max-height:min(65vh,570px);overflow:hidden;border:1px solid #eadfeb;border-radius:11px;background:#fff; }
        .personal-day-pane.is-task { border-color:#f3d7dd; }
        .personal-day-pane.is-meeting { border-color:#e5d8f0; }
        .personal-day-pane .personal-day-section-head { flex:0 0 auto;min-height:48px;margin:0;padding:11px 13px;border:0;border-bottom:1px solid #f3d7dd;border-radius:0;background:#fff1f2; }
        .personal-day-pane .personal-day-section-head.is-meeting { border-bottom-color:#e5d8f0;background:#f7f2fd; }
        .personal-day-pane-total { flex:0 0 auto;padding:4px 8px;border:1px solid #f7c7cf;border-radius:7px;background:#fff;color:#af1d38;font-size:11px;font-weight:800;white-space:nowrap; }
        .personal-day-pane .is-meeting .personal-day-pane-total { border-color:#e1d1ee;color:#673499; }
        .personal-day-pane-scroll { flex:1 1 auto;min-height:0;overflow-y:auto;overflow-x:hidden;overscroll-behavior:contain;padding:11px;scrollbar-width:thin;scrollbar-color:#cab6df transparent; }
        .personal-day-pane.is-task .personal-day-pane-scroll { scrollbar-color:#ebacb7 transparent; }
        .personal-day-pane .personal-day-project,.personal-day-pane .personal-day-meeting-project { margin-bottom:9px; }
        .personal-day-pane .personal-day-project-head,.personal-day-pane .personal-day-meeting-project-head { padding:9px 10px;gap:7px; }
        .personal-day-pane .personal-day-project-head span { line-height:1.4; }
        .personal-day-pane .personal-day-project-count,.personal-day-pane .personal-day-meeting-project-count { font-size:10px; }
        .personal-day-pane .personal-day-task { padding:9px 10px;font-size:12px; }
        .personal-day-pane .personal-day-meeting-entry { grid-template-columns:67px minmax(0,1fr);gap:9px;padding:11px 10px; }
        .personal-day-pane .personal-day-meeting-clock { padding:8px 3px;font-size:11.5px; }
        .personal-day-pane .personal-day-meeting-heading { gap:6px;margin-bottom:7px; }
        .personal-day-pane .personal-day-meeting-entry .personal-day-meeting-name { flex-basis:140px;font-size:12.5px; }
        .personal-day-pane .personal-day-meeting-meta-grid { grid-template-columns:1fr;gap:5px; }
        .personal-day-pane-empty { display:flex;flex-direction:column;align-items:center;justify-content:center;gap:7px;min-height:155px;padding:18px;text-align:center;color:#ad4051; }
        .personal-day-pane-empty i { display:grid;place-items:center;width:42px;height:42px;border-radius:11px;background:#fff1f2;font-size:19px; }
        .personal-day-pane-empty strong { font-size:13px; }
        .personal-day-pane-empty small { font-size:11px;color:#93818b; }
        .personal-day-pane-empty.is-meeting { color:#70409f; }
        .personal-day-pane-empty.is-meeting i { background:#f3ebfb; }
        .personal-day-modal > .personal-day-notice { margin-bottom:11px;padding:10px 13px; }
        @media(max-width:720px) { .personal-day-modal { padding:9px; } .personal-day-dual { grid-template-columns:minmax(0,1fr);gap:9px; } .personal-day-pane { min-height:0;max-height:min(30vh,240px); } .personal-day-pane-scroll { padding:8px; } .personal-day-pane .personal-day-meeting-meta-grid { grid-template-columns:repeat(2,minmax(0,1fr)); } .personal-day-pane .personal-day-meeting-detail.is-location { grid-column:1/-1; } }
        @media(max-width:420px) { .personal-day-pane .personal-day-meeting-meta-grid { grid-template-columns:1fr; } }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <asp:UpdatePanel ID="upCalendar" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <SweetSoft:Navigation ID="Navigation1" runat="server" />
            <section class="personal-cal" aria-label="Lịch cá nhân">
                <div class="personal-cal-surface">
                    <asp:HiddenField ID="hfScheduleDataJSON" runat="server" />
                    <asp:HiddenField ID="hfViewMode" runat="server" Value="month" />
                    <asp:HiddenField ID="hfCurrentDate" runat="server" />
                    <div class="personal-cal-toolbar">
                        <div class="personal-cal-heading">
                            <span class="personal-cal-heading-icon"><i class="far fa-calendar-alt" aria-hidden="true"></i></span>
                            <span class="personal-cal-heading-copy">
                                <strong class="personal-cal-period"><asp:Literal ID="litDateRange" runat="server" /></strong>
                                <small class="personal-cal-subtitle"><asp:Literal ID="litTitle" runat="server" /></small>
                            </span>
                        </div>
                        <nav class="personal-cal-navigation" aria-label="Điều hướng lịch">
                            <asp:LinkButton ID="btnPrev" runat="server" CssClass="personal-cal-btn" CausesValidation="false" OnClick="btnPrev_Click" ToolTip="Kỳ trước"><i class="fas fa-chevron-left" aria-hidden="true"></i></asp:LinkButton>
                            <asp:LinkButton ID="btnToday" runat="server" CssClass="personal-cal-btn" CausesValidation="false" OnClick="btnToday_Click"><%= GetResourceText(BackEndResourceKeys.TODAY) %></asp:LinkButton>
                            <asp:LinkButton ID="btnNext" runat="server" CssClass="personal-cal-btn" CausesValidation="false" OnClick="btnNext_Click" ToolTip="Kỳ sau"><i class="fas fa-chevron-right" aria-hidden="true"></i></asp:LinkButton>
                        </nav>
                        <div class="personal-cal-views">
                            <div class="personal-cal-view-switch" aria-label="Chế độ xem lịch">
                                <asp:LinkButton ID="btnViewMonth" runat="server" CssClass="personal-cal-btn" CausesValidation="false" OnClick="btnViewMonth_Click"><i class="far fa-calendar" aria-hidden="true"></i> <%= GetResourceText(BackEndResourceKeys.MONTH) %></asp:LinkButton>
                                <asp:LinkButton ID="btnViewWeek" runat="server" CssClass="personal-cal-btn" CausesValidation="false" OnClick="btnViewWeek_Click"><i class="fas fa-calendar-week" aria-hidden="true"></i> <%= GetResourceText(BackEndResourceKeys.WEEK) %></asp:LinkButton>
                            </div>
                        </div>
                    </div>
                    <div class="personal-cal-legend" aria-label="Chú giải lịch">
                        <span class="personal-cal-legend-item"><i class="personal-cal-legend-dot" aria-hidden="true"></i> Có công việc</span>
                        <span class="personal-cal-legend-item"><i class="personal-cal-legend-dot meeting" aria-hidden="true"></i> Lịch họp</span>
                        <span class="personal-cal-legend-item"><i class="personal-cal-legend-dot free" aria-hidden="true"></i> Ngày trống</span>
                        <span class="personal-cal-legend-item"><i class="personal-cal-legend-dot off" aria-hidden="true"></i> Ngày lễ</span>
                        <span class="personal-cal-legend-item"><i class="personal-cal-legend-dot weekend" aria-hidden="true"></i> Cuối tuần</span>
                        <span class="personal-cal-legend-item personal-cal-legend-hint"><i class="far fa-hand-pointer" aria-hidden="true"></i> Rê chuột vào ô ngày để lật thẻ</span>
                    </div>
                    <div class="personal-cal-scroll">
                        <div id="calendarGrid" class="personal-cal-grid is-month" role="grid" aria-label="Lưới lịch cá nhân">
                            <div class="personal-cal-dow" role="columnheader"><%= GetResourceText(BackEndResourceKeys.MONDAY) %></div>
                            <div class="personal-cal-dow" role="columnheader"><%= GetResourceText(BackEndResourceKeys.TUESDAY) %></div>
                            <div class="personal-cal-dow" role="columnheader"><%= GetResourceText(BackEndResourceKeys.WEDNESDAY) %></div>
                            <div class="personal-cal-dow" role="columnheader"><%= GetResourceText(BackEndResourceKeys.THURSDAY) %></div>
                            <div class="personal-cal-dow" role="columnheader"><%= GetResourceText(BackEndResourceKeys.FRIDAY) %></div>
                            <div class="personal-cal-dow is-weekend" role="columnheader"><%= GetResourceText(BackEndResourceKeys.SATURDAY) %></div>
                            <div class="personal-cal-dow is-weekend" role="columnheader"><%= GetResourceText(BackEndResourceKeys.SUNDAY) %></div>
                        </div>
                    </div>
                </div>
            </section>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <asp:UpdatePanel ID="upHiddenAction" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div style="display:none;">
                <asp:Button ID="btnOpenModalDay" runat="server" CausesValidation="false" OnClick="btnOpenModalDay_Click" />
                <asp:HiddenField ID="hfSelectedDateIndex" runat="server" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <SweetSoft:ExtraModal runat="server" ID="mdlDayDetail" Type="Primary" Size="Large" FooterButtonClose="true">
        <ContentTemplate>
            <asp:UpdatePanel ID="upModal" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="personal-day-modal"><asp:Literal ID="litModalContent" runat="server" /></div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </ContentTemplate>
    </SweetSoft:ExtraModal>
</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        window.PersonalCalendar = window.PersonalCalendar || (function () {
            var calendarData = [];
            var tooltip = null;
            var toolAnchor = null;
            var words = {
                holiday:'<%= System.Web.HttpUtility.JavaScriptStringEncode(GetResourceText(BackEndResourceKeys.HOLIDAY)) %>',
                weekend:'<%= System.Web.HttpUtility.JavaScriptStringEncode(GetResourceText(BackEndResourceKeys.WEEKEND)) %>',
                free:'<%= System.Web.HttpUtility.JavaScriptStringEncode(GetResourceText(BackEndResourceKeys.FREE)) %>'
            };
            function byId(id) { return document.getElementById(id); }
            function html(value) { return String(value == null ? '' : value).replace(/[&<>"']/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]; }); }
            function dayKey(raw) { var match = String(raw || '').match(/^(\d{4})-(\d{2})-(\d{2})/); return match ? match[1] + '-' + match[2] + '-' + match[3] : ''; }
            function toDate(key) { var p = key.split('-'); return new Date(+p[0], +p[1] - 1, +p[2]); }
            function todayKey() { var d = new Date(); return [d.getFullYear(), ('0' + (d.getMonth() + 1)).slice(-2), ('0' + d.getDate()).slice(-2)].join('-'); }
            function canOpen(day) { return !!day && (day.ChoPhepClick === true || day.ChoPhepClick === 1 || String(day.ChoPhepClick).toLowerCase() === 'true'); }
            function taskName(task) { return String(task.TenCongViec || 'Công việc chưa đặt tên'); }
            function projectName(task) { return String(task.TenDuAn || task.MaDuAn || 'Dự án khác'); }
            function removeTooltip() { if (tooltip && tooltip.parentNode) tooltip.parentNode.removeChild(tooltip); tooltip = null; toolAnchor = null; }
            function placeTooltip(anchor) {
                if (!tooltip || !anchor) return;
                var rect = anchor.getBoundingClientRect(), width = tooltip.offsetWidth, height = tooltip.offsetHeight, gap = 10;
                var x = window.innerWidth - rect.right >= width + gap + 8 ? rect.right + gap : rect.left - width - gap;
                x = Math.max(10, Math.min(x, window.innerWidth - width - 10));
                var y = Math.max(10, Math.min(rect.top + (rect.height - height) / 2, window.innerHeight - height - 10));
                tooltip.style.left = Math.round(x) + 'px'; tooltip.style.top = Math.round(y) + 'px';
            }
            function showTooltip(anchor) {
                var meetTitle = anchor.getAttribute('data-meet-title');
                removeTooltip(); toolAnchor = anchor; tooltip = document.createElement('div');
                tooltip.className = 'personal-cal-tooltip' + (meetTitle ? ' is-meeting' : ''); tooltip.setAttribute('role','tooltip');
                if (meetTitle) {
                    var code = Number(anchor.getAttribute('data-meet-status') || 0), label = meetingStatusText(code);
                    tooltip.innerHTML = '<strong>' + html(meetTitle) + '</strong><span class="personal-cal-tooltip-status is-status-' + code + '">' + html(label) + '</span>' +
                        '<span><i class="fas fa-folder-open"></i> ' + html(anchor.getAttribute('data-project-name') || 'Dự án khác') + '</span>' +
                        '<span><i class="fas fa-user-tie"></i> ' + html(anchor.getAttribute('data-meet-host') || 'Chưa rõ người chủ trì') + '</span>' +
                        '<span><i class="far fa-clock"></i> ' + html(anchor.getAttribute('data-meet-time') || '') + '</span>' +
                        '<span><i class="fas ' + (anchor.getAttribute('data-meet-online') === '1' ? 'fa-video' : 'fa-map-marker-alt') + '"></i> ' + html(anchor.getAttribute('data-meet-place') || 'Chưa có địa điểm') + '</span>';
                } else {
                    var name = anchor.getAttribute('data-task-name') || '', project = anchor.getAttribute('data-project-name') || '', code = anchor.getAttribute('data-task-code') || '';
                    if (!name) { removeTooltip(); return; }
                    tooltip.innerHTML = '<strong>' + html(name) + '</strong><span><i class="fas fa-folder-open"></i> ' + html(project) + '</span>' + (code ? '<span><i class="fas fa-hashtag"></i> ' + html(code) + '</span>' : '');
                }
                document.body.appendChild(tooltip); placeTooltip(anchor);
            }
            function meetingStatus(meeting) {
                var start = new Date(meeting.ThoiGianBatDau), end = new Date(meeting.ThoiGianKetThuc), now = new Date();
                if (now >= end) return 3;
                if (now >= start) return 2;
                if (start - now <= 15 * 60000) return 1;
                return 0;
            }
            function meetingStatusText(code) { return ['Đã lên lịch','Sắp diễn ra','Đang diễn ra','Đã kết thúc'][code] || 'Đã lên lịch'; }
            function shortTime(raw) {
                var date = new Date(raw);
                return isNaN(date.getTime()) ? '--:--' : ('0'+date.getHours()).slice(-2) + ':' + ('0'+date.getMinutes()).slice(-2);
            }
            function renderMeeting(meeting, dayKeyValue) {
                var status = meetingStatus(meeting), name = String(meeting.TenCuocHop || 'Cuộc họp'), project = String(meeting.TenDuAn || 'Dự án khác');
                var start = new Date(meeting.ThoiGianBatDau), end = new Date(meeting.ThoiGianKetThuc);
                var dayStart = toDate(dayKeyValue), next = new Date(dayStart.getTime()); next.setDate(next.getDate()+1);
                var from = start < dayStart ? dayStart : start, to = end > next ? next : end;
                var displayTime = shortTime(from), endTime = to.getTime() === next.getTime() ? '24:00' : shortTime(to);
                var host = meeting.LaChuTri ? 'Chủ trì cuộc họp' : (meeting.TenNguoiChuTri || 'Chưa rõ người chủ trì');
                return '<div class="personal-cal-event is-meeting is-status-' + status + '" data-meet-title="' + html(name) + '" data-project-name="' + html(project) + '" data-meet-id="' + html(meeting.IdLichHop || '') + '" data-meet-status="' + status + '" data-meet-host="' + html(host) + '" data-meet-time="' + html(displayTime + '–' + endTime) + '" data-meet-online="' + (meeting.LaHopOnline ? 1 : 0) + '" data-meet-place="' + html(meeting.LaHopOnline ? 'Xem thông tin phòng họp tại Lịch họp' : (meeting.DiaDiemHop || '')) + '">' +
                    (meeting.LaChuTri ? '<i class="fas fa-crown personal-meeting-host" title="Bạn là người chủ trì" aria-hidden="true"></i>' : '') +
                    '<b class="personal-cal-meeting-time">' + html(displayTime) + '</b><span class="personal-cal-event-name">' + html(name) + '</span>' +
                    ((status === 1 || status === 2) ? '<i class="personal-meeting-pulse" aria-hidden="true"></i>' : '') + '</div>';
            }
            function renderTask(task) {
                var name = taskName(task), project = projectName(task), code = String(task.MaCongViec || '');
                return '<div class="personal-cal-event" data-task-name="' + html(name) + '" data-project-name="' + html(project) + '" data-task-code="' + html(code) + '">' +
                    (code ? '<b class="personal-cal-event-code">' + html(code) + '</b>' : '<i class="fas fa-check-circle" aria-hidden="true"></i>') +
                    '<span class="personal-cal-event-name">' + html(name) + '</span></div>';
            }
            function render() {
                var jsonNode = byId('<%= hfScheduleDataJSON.ClientID %>'), modeNode = byId('<%= hfViewMode.ClientID %>'), currentNode = byId('<%= hfCurrentDate.ClientID %>'), grid = byId('calendarGrid');
                if (!jsonNode || !modeNode || !grid) return;
                try { calendarData = JSON.parse(jsonNode.value || '[]') || []; } catch (e) { calendarData = []; }
                if (!Array.isArray(calendarData)) calendarData = [];
                removeTooltip();
                var week = modeNode.value === 'week', current = dayKey(currentNode ? currentNode.value : ''), today = todayKey();
                grid.classList.toggle('is-week', week); grid.classList.toggle('is-month', !week);
                var existing = grid.querySelectorAll('.personal-cal-day,.personal-cal-empty');
                for (var old = 0; old < existing.length; old++) existing[old].remove();
                if (!calendarData.length) { grid.insertAdjacentHTML('beforeend', '<div class="personal-cal-empty" style="grid-column:1/-1">Không có dữ liệu lịch trong kỳ này.</div>'); return; }
                var frag = document.createDocumentFragment();
                calendarData.forEach(function (day, index) {
                    if (!day) return;
                    var key = dayKey(day.Ngay); if (!key) return;
                    var date = toDate(key), status = String(day.TrangThaiLich || 'free');
                    var tasks = Array.isArray(day.DanhSachCongViec) ? day.DanhSachCongViec : [];
                    var meetings = Array.isArray(day.DanhSachCuocHop) ? day.DanhSachCuocHop : [];
                    var taskCount = tasks.length, meetingCount = meetings.length;
                    var outside = !week && current && key.slice(0, 7) !== current.slice(0, 7);
                    var clickable = canOpen(day);
                    // Chỉ lật card của lịch tháng. Lịch tuần có tooltip riêng nên không lật.
                    var flip = !week && clickable;
                    var cell = document.createElement('div'); cell.className = 'personal-cal-day' + (clickable ? ' is-clickable' : '') + (flip ? ' has-flip' : '') + (outside ? ' is-outside' : '') + (status === 'holiday' ? ' is-holiday' : '') + (status === 'weekend' ? ' is-weekend' : '') + (key === today ? ' is-today' : '');
                    cell.setAttribute('role','gridcell'); cell.setAttribute('data-day-index',index);
                    if (clickable) cell.setAttribute('aria-label','Lịch ngày ' + key.slice(8,10) + '/' + key.slice(5,7) + '/' + key.slice(0,4));
                    // Lịch tháng: ngày nằm riêng phía trên; màu, biểu tượng và số lượng được trình bày trong panel phủ kín.
                    // Lịch tuần giữ badge hiện tại, danh sách chi tiết và tooltip của từng hoạt động.
                    var count = week ? ((taskCount ? '<span class="personal-cal-day-count" title="' + taskCount + ' công việc"><i class="fas fa-briefcase" aria-hidden="true"></i> ' + taskCount + '</span>' : '') +
                        (meetingCount ? '<span class="personal-cal-day-meeting-count" title="' + meetingCount + ' cuộc họp"><i class="far fa-calendar-alt" aria-hidden="true"></i> ' + meetingCount + '</span>' : '')) : '';
                    var content = '<div class="personal-cal-day-top"><span class="personal-cal-day-number">' + date.getDate() + '</span><span class="personal-cal-day-tags">' + count + '</span></div>';
                    if (!taskCount && !meetingCount) {
                        var panelClass = 'is-free', icon = 'far fa-calendar-check', label = words.free || 'Trống', desc = 'Chưa có công việc và cuộc họp';
                        if (status === 'weekend') { panelClass = 'is-weekend'; icon = 'fas fa-mug-hot'; label = words.weekend || 'Cuối tuần'; desc = 'Ngày nghỉ hằng tuần'; }
                        else if (status === 'holiday') { panelClass = 'is-holiday'; icon = 'fas fa-umbrella-beach'; label = day.TenNgoaiLe || words.holiday || 'Ngày lễ'; desc = words.holiday || 'Ngày nghỉ lễ'; }
                        else if (status === 'busy') { panelClass = 'is-busy'; icon = 'fas fa-briefcase'; label = 'Có lịch công việc'; desc = 'Chưa có chi tiết'; }
                        content += '<div class="personal-cal-status-panel ' + panelClass + '" title="' + html(label) + '"><span class="personal-cal-status-icon"><i class="' + icon + '" aria-hidden="true"></i></span><strong class="personal-cal-status-name">' + html(label) + '</strong><small class="personal-cal-status-desc">' + html(desc) + '</small></div>';
                    } else {
                        if (status === 'holiday' || status === 'weekend') {
                            var holiday = status === 'holiday';
                            content += '<div class="personal-cal-day-state ' + (holiday ? 'is-holiday' : 'is-weekend') + '"><i class="fas ' + (holiday ? 'fa-umbrella-beach' : 'fa-mug-hot') + '" aria-hidden="true"></i><span class="personal-cal-state-text">' + html(holiday ? (day.TenNgoaiLe || words.holiday) : words.weekend) + '</span></div>';
                        }
                        if (!week) {
                            // Một hoạt động: panel tô kín. Hai hoạt động: chia chéo đỏ (trên-trái) và tím (dưới-phải).
                            var tileMode = taskCount && meetingCount ? 'is-mixed' : taskCount ? 'is-tasks-only' : 'is-meetings-only';
                            content += '<div class="personal-cal-month-tile ' + tileMode + '" aria-label="' +
                                html((taskCount ? taskCount + ' công việc' : '') + (taskCount && meetingCount ? ', ' : '') + (meetingCount ? meetingCount + ' cuộc họp' : '')) + '">';
                            if (taskCount) content += '<div class="personal-cal-month-tile-content is-task"><span class="personal-cal-month-tile-icon"><i class="fas fa-briefcase" aria-hidden="true"></i></span><strong>' + taskCount + ' công việc</strong></div>';
                            if (meetingCount) content += '<div class="personal-cal-month-tile-content is-meeting"><span class="personal-cal-month-tile-icon"><i class="far fa-calendar-alt" aria-hidden="true"></i></span><strong>' + meetingCount + ' cuộc họp</strong></div>';
                            content += '</div>';
                        } else {
                            // Lịch tuần: hiển thị HẾT công việc, sau đó HẾT cuộc họp.
                            content += '<div class="personal-cal-day-events">';
                            tasks.forEach(function(task) { if (task) content += renderTask(task); });
                            meetings.slice().sort(function(a,b) { return new Date(a.ThoiGianBatDau) - new Date(b.ThoiGianBatDau); })
                                .forEach(function(meeting) { if (meeting) content += renderMeeting(meeting,key); });
                            content += '</div>';
                        }
                    }
                    if (flip) {
                        // Sử dụng lại HTML mặt trước cũ để không đổi badge, ngày trống/nghỉ, số lượng.
                        var dateLabel = ('0' + date.getDate()).slice(-2) + '/' + ('0' + (date.getMonth() + 1)).slice(-2);
                        var brief = taskCount && meetingCount ? taskCount + ' công việc · ' + meetingCount + ' họp'
                            : taskCount ? taskCount + ' công việc'
                            : meetingCount ? meetingCount + ' cuộc họp'
                            : status === 'holiday' ? (day.TenNgoaiLe || words.holiday || 'Ngày lễ')
                            : status === 'weekend' ? (words.weekend || 'Cuối tuần')
                            : status === 'busy' ? 'Có lịch làm việc' : 'Ngày trống';
                        content = '<div class="personal-cal-flip-inner">' +
                            '<div class="personal-cal-flip-face personal-cal-flip-front">' + content + '</div>' +
                            '<div class="personal-cal-flip-face personal-cal-flip-back">' +
                            '<strong class="personal-cal-flip-date">Ngày ' + dateLabel + '</strong>' +
                            '<span class="personal-cal-flip-summary" title="' + html(brief) + '">' + html(brief) + '</span>' +
                            '<button type="button" class="personal-cal-flip-action" data-personal-action="open-day" aria-label="Xem chi tiết ngày ' + dateLabel + '">' +
                            '<i class="far fa-eye" aria-hidden="true"></i> Xem chi tiết</button>' +
                            '</div></div>';
                    } else if (week && clickable) {
                        content += '<button type="button" class="personal-cal-week-detail" data-personal-action="open-day" aria-label="Xem chi tiết ngày ' + key.slice(8,10) + '/' + key.slice(5,7) + '">' +
                            '<i class="far fa-eye" aria-hidden="true"></i> Xem chi tiết</button>';
                    }
                    cell.innerHTML = content; frag.appendChild(cell);
                });
                grid.appendChild(frag);
            }
            // Cập nhật thẻ hiện có, không dựng lại lưới: tránh mất focus và vị trí cuộn.
            window.setInterval(function () {
                var eventNodes = document.querySelectorAll('#calendarGrid .personal-cal-event.is-meeting');
                if (!eventNodes.length) return;
                var lookup = {};
                calendarData.forEach(function (day) {
                    (day && day.DanhSachCuocHop || []).forEach(function (meeting) {
                        if (meeting && meeting.IdLichHop) lookup[meeting.IdLichHop] = meeting;
                    });
                });
                Array.prototype.forEach.call(eventNodes, function (node) {
                    var meeting = lookup[node.getAttribute('data-meet-id')]; if (!meeting) return;
                    var code = meetingStatus(meeting), previous = Number(node.getAttribute('data-meet-status'));
                    if (code === previous) return;
                    node.classList.remove('is-status-' + previous);
                    node.classList.add('is-status-' + code);
                    node.setAttribute('data-meet-status', String(code));
                    var dot = node.querySelector('.personal-meeting-pulse');
                    if ((code === 1 || code === 2) && !dot) {
                        dot = document.createElement('i'); dot.className = 'personal-meeting-pulse';
                        dot.setAttribute('aria-hidden', 'true'); node.appendChild(dot);
                    } else if (code !== 1 && code !== 2 && dot) { dot.remove(); }
                    if (toolAnchor === node) removeTooltip();
                });
            }, 60000);
            function openDay(index) {
                var day = calendarData[index]; if (!canOpen(day)) return false;
                var input = byId('<%= hfSelectedDateIndex.ClientID %>'), button = byId('<%= btnOpenModalDay.ClientID %>');
                if (!input || !button) return false;
                var prm = window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager ? Sys.WebForms.PageRequestManager.getInstance() : null;
                if (prm && prm.get_isInAsyncPostBack()) return false;
                removeTooltip(); input.value = String(index); button.click(); return false;
            }
            function onClick(e) {
                var action = e.target.closest && e.target.closest('[data-personal-action="open-day"]');
                if (action) {
                    var cell = action.closest('.personal-cal-day.is-clickable');
                    if (!cell) return;
                    e.preventDefault();
                    openDay(Number(cell.getAttribute('data-day-index')));
                    return;
                }
                // Điện thoại/màn hình cảm ứng: chạm mặt trước một lần để lật,
                // rồi nhấn nút 'Xem chi tiết' ở mặt sau.
                var front = e.target.closest && e.target.closest('.personal-cal-flip-front');
                var clickedCard = front && front.closest('.personal-cal-day.has-flip');
                var isTouch = window.matchMedia && window.matchMedia('(hover:none)').matches;
                var opened = document.querySelectorAll('#calendarGrid .personal-cal-day.is-flip-open');
                for (var i = 0; i < opened.length; i++) {
                    if (opened[i] !== clickedCard) opened[i].classList.remove('is-flip-open');
                }
                if (clickedCard && isTouch) {
                    e.preventDefault(); clickedCard.classList.add('is-flip-open');
                }
            }
            document.addEventListener('click', onClick);
            document.addEventListener('keydown', function (e) {
                if (e.key !== 'Escape') return;
                var opened = document.querySelectorAll('#calendarGrid .personal-cal-day.is-flip-open');
                for (var i = 0; i < opened.length; i++) opened[i].classList.remove('is-flip-open');
            });
            document.addEventListener('pointerover', function (e) { var el = e.target.closest && e.target.closest('.personal-cal-event'); if (el && el !== toolAnchor) showTooltip(el); });
            document.addEventListener('pointerout', function (e) { var el = e.target.closest && e.target.closest('.personal-cal-event'); if (el && (!e.relatedTarget || !el.contains(e.relatedTarget))) removeTooltip(); });
            document.addEventListener('focusin', function (e) { var el = e.target.closest && e.target.closest('.personal-cal-event'); if (el) showTooltip(el); });
            document.addEventListener('scroll', removeTooltip, true); window.addEventListener('resize', removeTooltip);
            if (window.Sys && Sys.Application) Sys.Application.add_load(render);
            if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', render); else render();
            return { render: render, openDay: openDay };
        })();
    </script>
</asp:Content>
