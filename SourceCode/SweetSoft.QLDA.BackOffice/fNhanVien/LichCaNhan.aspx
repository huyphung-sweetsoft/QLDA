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
        .personal-cal .personal-cal-grid { display:grid;grid-template-columns:repeat(7,minmax(0,1fr));gap:1px;min-width:840px;overflow:hidden;border:1px solid #e8e1ef;border-radius:11px;background:#e8e1ef;isolation:isolate; }
        .personal-cal .personal-cal-dow { padding:10px 4px;background:#f3edfa;color:#583486;font-size:12px;font-weight:800;text-align:center; }
        .personal-cal .personal-cal-dow.is-weekend { color:#a56849; }
        .personal-cal .personal-cal-day { position:relative;display:flex;flex-direction:column;gap:7px;min-width:0;min-height:clamp(103px,calc((100dvh - 295px)/6),132px);padding:8px 7px;background:#fff;transition:background-color .17s,box-shadow .17s; }
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
                        <span class="personal-cal-legend-item"><i class="personal-cal-legend-dot free" aria-hidden="true"></i> Ngày trống</span>
                        <span class="personal-cal-legend-item"><i class="personal-cal-legend-dot off" aria-hidden="true"></i> Ngày lễ</span>
                        <span class="personal-cal-legend-item"><i class="personal-cal-legend-dot weekend" aria-hidden="true"></i> Cuối tuần</span>
                        <span class="personal-cal-legend-item personal-cal-legend-hint"><i class="far fa-hand-pointer" aria-hidden="true"></i> Chọn ngày để xem chi tiết</span>
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
                var name = anchor.getAttribute('data-task-name') || '', project = anchor.getAttribute('data-project-name') || '', code = anchor.getAttribute('data-task-code') || '';
                if (!name) return;
                removeTooltip(); toolAnchor = anchor; tooltip = document.createElement('div'); tooltip.className = 'personal-cal-tooltip'; tooltip.setAttribute('role', 'tooltip');
                tooltip.innerHTML = '<strong>' + html(name) + '</strong><span><i class="fas fa-folder-open"></i> ' + html(project) + '</span>' + (code ? '<span><i class="fas fa-hashtag"></i> ' + html(code) + '</span>' : '');
                document.body.appendChild(tooltip); placeTooltip(anchor);
            }
            function render() {
                var jsonNode = byId('<%= hfScheduleDataJSON.ClientID %>'), modeNode = byId('<%= hfViewMode.ClientID %>'), currentNode = byId('<%= hfCurrentDate.ClientID %>'), grid = byId('calendarGrid');
                if (!jsonNode || !modeNode || !grid) return;
                try { calendarData = JSON.parse(jsonNode.value || '[]') || []; } catch (e) { calendarData = []; }
                if (!Array.isArray(calendarData)) calendarData = [];
                removeTooltip();
                var week = modeNode.value === 'week', current = dayKey(currentNode ? currentNode.value : ''), today = todayKey();
                grid.classList.toggle('is-week', week); grid.classList.toggle('is-month', !week);
                // Giữ header cố định, chỉ thay phần các ngày để UpdatePanel render ổn định.
                var existing = grid.querySelectorAll('.personal-cal-day,.personal-cal-empty');
                for (var old = 0; old < existing.length; old++)existing[old].remove();
                if (!calendarData.length) { grid.insertAdjacentHTML('beforeend', '<div class="personal-cal-empty" style="grid-column:1/-1">Không có dữ liệu lịch trong kỳ này.</div>'); return; }
                var frag = document.createDocumentFragment();
                calendarData.forEach(function (day, index) {
                    if (!day) return;
                    var key = dayKey(day.Ngay); if (!key) return;
                    var date = toDate(key), status = String(day.TrangThaiLich || 'free'), tasks = Array.isArray(day.DanhSachCongViec) ? day.DanhSachCongViec : [], taskCount = tasks.length;
                    var outside = !week && current && key.slice(0, 7) !== current.slice(0, 7);
                    var clickable = canOpen(day);
                    var cell = document.createElement('div'); cell.className = 'personal-cal-day' + (clickable ? ' is-clickable' : '') + (outside ? ' is-outside' : '') + (status === 'holiday' ? ' is-holiday' : '') + (status === 'weekend' ? ' is-weekend' : '') + (key === today ? ' is-today' : '');
                    cell.setAttribute('role', 'gridcell'); cell.setAttribute('data-day-index', index);
                    if (clickable) { cell.tabIndex = 0; cell.setAttribute('aria-label', 'Xem lịch ngày ' + key.slice(8, 10) + '/' + key.slice(5, 7) + '/' + key.slice(0, 4)); }
                    var count = taskCount ? '<span class="personal-cal-day-count" title="' + taskCount + ' công việc"><i class="fas fa-briefcase" aria-hidden="true"></i> ' + taskCount + '</span>' : '';
                    var content = '<div class="personal-cal-day-top"><span class="personal-cal-day-number">' + date.getDate() + '</span><span class="personal-cal-day-tags">' + count + '</span></div>';
                    // Ngày không có việc: chiếm trọn phần diện tích trống, không dùng badge co theo độ dài chữ.
                    if (!taskCount) {
                        var panelClass = 'is-free', icon = 'far fa-calendar-check', label = words.free || 'Trống', desc = 'Chưa có công việc';
                        if (status === 'weekend') { panelClass = 'is-weekend'; icon = 'fas fa-mug-hot'; label = words.weekend || 'Cuối tuần'; desc = 'Ngày nghỉ hằng tuần'; }
                        else if (status === 'holiday') { panelClass = 'is-holiday'; icon = 'fas fa-umbrella-beach'; label = day.TenNgoaiLe || words.holiday || 'Ngày lễ'; desc = words.holiday || 'Ngày nghỉ lễ'; }
                        else if (status === 'busy') { panelClass = 'is-busy'; icon = 'fas fa-briefcase'; label = 'Có lịch công việc'; desc = 'Chưa có chi tiết'; }
                        content += '<div class="personal-cal-status-panel ' + panelClass + '" title="' + html(label) + '"><span class="personal-cal-status-icon"><i class="' + icon + '" aria-hidden="true"></i></span><strong class="personal-cal-status-name">' + html(label) + '</strong><small class="personal-cal-status-desc">' + html(desc) + '</small></div>';
                    } else {
                        // Có việc trong ngày lễ/cuối tuần: giữ nhãn toàn chiều rộng phía trên danh sách.
                        if (status === 'holiday' || status === 'weekend') {
                            var holiday = status === 'holiday';
                            content += '<div class="personal-cal-day-state ' + (holiday ? 'is-holiday' : 'is-weekend') + '"><i class="fas ' + (holiday ? 'fa-umbrella-beach' : 'fa-mug-hot') + '" aria-hidden="true"></i><span class="personal-cal-state-text">' + html(holiday ? (day.TenNgoaiLe || words.holiday) : words.weekend) + '</span></div>';
                        }
                        var max = week ? 5 : 2;
                        content += '<div class="personal-cal-day-events">';
                        tasks.slice(0, max).forEach(function (task) {
                            if (!task) return;
                            var name = taskName(task), project = projectName(task), code = String(task.MaCongViec || '');
                            content += '<div class="personal-cal-event" data-task-name="' + html(name) + '" data-project-name="' + html(project) + '" data-task-code="' + html(code) + '">' + (code ? '<b class="personal-cal-event-code">' + html(code) + '</b>' : '<i class="fas fa-check-circle" aria-hidden="true"></i>') + '<span class="personal-cal-event-name">' + html(name) + '</span></div>';
                        });
                        if (taskCount > max) content += '<span class="personal-cal-more"><i class="fas fa-plus-circle" aria-hidden="true"></i> ' + (taskCount - max) + ' công việc nữa</span>';
                        content += '</div>';
                    }
                    cell.innerHTML = content; frag.appendChild(cell);
                });
                grid.appendChild(frag);
            }
            function openDay(index) {
                var day = calendarData[index]; if (!canOpen(day)) return false;
                var input = byId('<%= hfSelectedDateIndex.ClientID %>'), button = byId('<%= btnOpenModalDay.ClientID %>');
                if (!input || !button) return false;
                var prm = window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager ? Sys.WebForms.PageRequestManager.getInstance() : null;
                if (prm && prm.get_isInAsyncPostBack()) return false;
                removeTooltip(); input.value = String(index); button.click(); return false;
            }
            function onClick(e) {
                var target = e.target.closest && e.target.closest('.personal-cal-day.is-clickable');
                if (!target) return;
                e.preventDefault(); openDay(Number(target.getAttribute('data-day-index')));
            }
            function onKey(e) {
                if (e.key !== 'Enter' && e.key !== ' ') return;
                var target = e.target.closest && e.target.closest('.personal-cal-day.is-clickable');
                if (!target) return;
                e.preventDefault(); openDay(Number(target.getAttribute('data-day-index')));
            }
            document.addEventListener('click', onClick);
            document.addEventListener('keydown', onKey);
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
