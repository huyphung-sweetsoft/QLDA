<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlTodaySchedule.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.Controls.TodaySchedule.CtrlTodaySchedule" %>
<%@ Register Assembly="SweetSoft.QLDA.Controls" Namespace="SweetSoft.QLDA.Controls" TagPrefix="SweetSoft" %>
<%@ Register Src="~/fTasks/Controls/CtrlViewTaskDetail.ascx" TagPrefix="SweetSoft" TagName="CtrlViewTaskDetail" %>
<%@ Register Src="~/fMeets/Controls/CtrlViewMeetDetail.ascx" TagPrefix="SweetSoft" TagName="CtrlViewMeetDetail" %>
<style>
/* Chỉ trang trí NỘI DUNG của ExtraModal, không thay header, viền, backdrop hoặc nút X mặc định. */
.today-schedule-content { padding:16px 20px 20px;color:#362d48;font-size:13px; }
.today-schedule-intro { display:flex;align-items:center;gap:13px;padding:14px 16px;margin-bottom:15px;border:1px solid #e7d9f5;border-radius:13px;background:linear-gradient(110deg,#f8f2ff,#fff); }
.today-schedule-intro-icon { display:grid;place-items:center;flex:0 0 43px;width:43px;height:43px;border-radius:12px;background:#e9dcfa;color:#6733aa;font-size:20px; }
.today-schedule-intro strong { display:block;color:#472175;font-size:17px;font-weight:850; }
.today-schedule-intro span { display:block;margin-top:3px;color:#82758e;font-size:12px; }
.today-schedule-kpis { display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:10px;margin-bottom:18px; }
.today-schedule-kpi { display:flex;align-items:center;justify-content:space-between;padding:12px 15px;border:1px solid #e8dff3;border-radius:12px;background:#fbf9ff;color:#675279;font-weight:750; }
.today-schedule-kpi b { font-size:22px;color:#602c9f;line-height:1; }
.today-schedule-grid { display:grid;grid-template-columns:1fr 1fr;align-items:start;gap:15px; }
.today-schedule-block { min-width:0;border:1px solid #e6dfee;border-radius:14px;background:#fff;overflow:hidden; }
.today-schedule-block-head { display:flex;align-items:center;justify-content:space-between;gap:8px;padding:13px 14px;border-bottom:1px solid #eee7f4;background:#faf7ff; }
.today-schedule-block-head strong { display:inline-flex;align-items:center;gap:8px;font-size:13.5px;color:#55218b;font-weight:800; }
.today-schedule-block-head small { font-size:10.5px;color:#8767a1;font-weight:750;white-space:nowrap; }
.today-schedule-list { display:flex;flex-direction:column;gap:0;max-height:min(49vh,420px);overflow-y:auto;scrollbar-width:thin; }
.today-schedule-item { padding:12px 13px;border-bottom:1px solid #f2edf7; }
.today-schedule-item:last-child { border-bottom:0; }
.today-schedule-item-top { display:flex;align-items:center;justify-content:space-between;gap:8px;margin-bottom:5px; }
.today-schedule-code { display:inline-block;padding:2px 7px;border:1px solid #ddd0ed;border-radius:6px;background:#f5efff;color:#6732a3;font-size:10.5px;font-weight:800; }
.today-schedule-time { white-space:nowrap;color:#7544a5;font-size:11px;font-weight:800; }
.today-schedule-name { display:block;color:#352742;font-size:13px;line-height:1.5;font-weight:750;overflow-wrap:anywhere; }
.today-schedule-meta { display:flex;align-items:center;gap:6px;margin-top:6px;color:#7f738b;font-size:11.5px;line-height:1.4;overflow-wrap:anywhere; }
.today-schedule-pill { display:inline-flex;align-items:center;margin-top:7px;padding:3px 7px;border-radius:6px;background:#eaf8ee;color:#187b4b;font-size:10.5px;font-weight:800; }
.today-schedule-pill.is-overdue { background:#fff0ed;color:#ba4a38; }
.today-schedule-pill.is-soon { background:#fff5e2;color:#a06911; }
.today-schedule-empty { display:flex;align-items:center;justify-content:center;flex-direction:column;gap:8px;padding:40px 18px;color:#93889c;text-align:center;font-size:12px; }
.today-schedule-empty i { font-size:26px;color:#b69bd8; }
.today-schedule-note { margin:13px 0 0;color:#8b8298;font-size:11px;line-height:1.5; }
.today-schedule-fail { padding:18px;border:1px solid #f2c9c9;border-radius:10px;background:#fff6f6;color:#a53b3b; }
/* Robot toàn thân nằm ngoài ExtraModal, neo theo vị trí modal-dialog bằng JavaScript. */
.today-schedule-companion { position:fixed;z-index:2000;width:265px;pointer-events:none;opacity:0;visibility:hidden;transform:translateY(9px);transition:opacity .2s,transform .2s; }
.today-schedule-companion.is-visible { opacity:1;visibility:visible;transform:translateY(0); }
.today-schedule-companion-talk { position:relative;margin:0 0 11px;padding:15px 16px 14px;border:1px solid #cdb4e9;border-radius:16px;background:linear-gradient(140deg,#ffffff,#f5eeff);box-shadow:0 15px 38px rgba(47,21,87,.17);color:#49267e; }
.today-schedule-companion-talk:after { content:'';position:absolute;left:55px;bottom:-10px;width:17px;height:17px;transform:rotate(45deg);border-right:1px solid #cdb4e9;border-bottom:1px solid #cdb4e9;background:#f5eeff; }
.today-schedule-companion-talk strong { display:block;font-size:16px;margin-bottom:8px;color:#4b1d81; }
.today-schedule-companion-talk span { display:flex;align-items:center;gap:8px;margin-top:6px;font-size:13px;line-height:1.45;font-weight:750; }
.today-schedule-companion-talk span i { width:18px;color:#8c51bc;text-align:center; }
.today-schedule-companion-talk .today-schedule-overdue i { color:#c24141; }
.today-schedule-companion-talk .today-schedule-overdue b { color:#a93232; }
.today-schedule-companion-talk span[hidden] { display:none!important; }
.today-schedule-companion-bot { display:block;margin:0 auto;width:168px;height:220px;filter:drop-shadow(0 8px 12px rgba(67,27,112,.15));animation:todayBotHello 5s ease-in-out infinite; }
.today-schedule-companion-name { display:table;margin:-3px auto 0;padding:5px 13px;border:1px solid #d9c1ef;border-radius:999px;background:rgba(255,255,255,.96);box-shadow:0 4px 12px rgba(61,32,95,.12);color:#54218d;font-size:12px;line-height:1.4;font-weight:850;letter-spacing:.1px;text-align:center; }
@keyframes todayBotHello { 0%,100% { transform:translateY(0) } 50% { transform:translateY(-5px) } }
@media(max-width:1390px) { .today-schedule-companion { width:180px; } .today-schedule-companion-talk { padding:11px; } .today-schedule-companion-talk strong { font-size:13px; } .today-schedule-companion-talk span { font-size:11px; } .today-schedule-companion-bot { width:115px;height:150px; } }
@media(max-width:1020px) { .today-schedule-companion { display:none!important; } }
@media(max-width:680px) { .today-schedule-grid { grid-template-columns:1fr; } .today-schedule-content { padding:10px; } .today-schedule-list { max-height:36vh; } }
@media(prefers-reduced-motion:reduce) { .today-schedule-companion,.today-schedule-companion-bot { animation:none!important;transition:none!important; } }

/* V26: task xanh duong, meeting tim; giữ nguyên ExtraModal và robot đang dùng. */
.today-schedule-block--tasks { border-color:#bfd7fc;background:#fff; }
.today-schedule-block--tasks .today-schedule-block-head { background:#eff6ff;border-bottom-color:#cfe2ff; }
.today-schedule-block--tasks .today-schedule-block-head strong,.today-schedule-block--tasks .today-schedule-block-head small { color:#1d4ed8; }
.today-schedule-block--tasks .today-schedule-code { border-color:#bfdbfe;background:#eff6ff;color:#1d4ed8; }
.today-schedule-block--tasks .today-schedule-time { color:#2563eb; }
.today-schedule-block--tasks .today-schedule-item { border-bottom-color:#e5eefb; }
.today-schedule-block--tasks .today-schedule-empty i { color:#60a5fa; }
.today-schedule-block--meetings { border-color:#dfd0f5;background:#fff; }
.today-schedule-block--meetings .today-schedule-block-head { background:#f8f3ff;border-bottom-color:#e6d8f5; }
.today-schedule-block--meetings .today-schedule-block-head strong,.today-schedule-block--meetings .today-schedule-block-head small { color:#54218d; }
.today-schedule-block--meetings .today-schedule-code { border-color:#ddc7f3;background:#f6edff;color:#632a9e; }
.today-schedule-block--meetings .today-schedule-time { color:#7136a1; }
.today-schedule-block--meetings .today-schedule-empty i { color:#9e72cf; }

/* V28 | Ba bảng đồng kích thước, việc quá hạn đỏ, task xanh, cuộc họp tím. */
.today-schedule-block,.today-schedule-block * { box-sizing:border-box; }
#<%= mdlTodaySchedule.ClientID %> .modal-dialog { width:min(1140px,calc(100vw - 32px))!important;max-width:calc(100vw - 32px)!important; }
#<%= mdlTodaySchedule.ClientID %> .modal-body { min-height:0;overflow-y:auto;max-height:calc(100dvh - 142px); }
.today-schedule-content { padding:15px 17px 13px;color:#334155; }
.today-schedule-intro { padding:12px 14px;margin-bottom:13px; }
.today-schedule-grid { display:grid;grid-template-columns:repeat(2,minmax(0,1fr));align-items:stretch;gap:12px;max-width:730px;margin:0 auto; }
.today-schedule-grid.has-overdue { grid-template-columns:repeat(3,minmax(0,1fr));max-width:1080px; }
.today-schedule-block { display:flex;flex-direction:column;min-width:0;height:min(51dvh,465px);min-height:335px;border-radius:11px; }
.today-schedule-block-head { flex:0 0 60px;min-height:60px;align-items:center;padding:9px 12px; }
.today-schedule-block-head strong { font-size:13px;line-height:1.35;min-width:0; }
.today-schedule-block-head strong i { flex:0 0 auto; }
.today-schedule-block-head small { flex:0 0 auto;font-size:10.5px; }
.today-schedule-list { flex:1 1 auto;min-height:0;max-height:none;overflow-y:auto;overscroll-behavior:contain;scrollbar-width:thin; }
.today-schedule-item { display:flex;flex:0 0 116px;flex-direction:column;justify-content:center;height:116px;min-height:116px;padding:9px 12px;overflow:hidden;border-bottom:1px solid #e8eef5; }
.today-schedule-item[hidden] { display:none!important; }
.today-schedule-item-top { gap:5px;margin-bottom:6px; }
.today-schedule-code { max-width:53%;overflow:hidden;text-overflow:ellipsis;white-space:nowrap; }
.today-schedule-time { max-width:60%;font-size:10.5px; }
.today-schedule-name { display:-webkit-box;-webkit-box-orient:vertical;-webkit-line-clamp:2;overflow:hidden;font-size:12.5px;line-height:1.35;color:#24354c; }
.today-schedule-meta { gap:6px;min-width:0;margin-top:4px;color:#64748b;font-size:11px;white-space:nowrap; }
.today-schedule-meta span { display:block;min-width:0;overflow:hidden;text-overflow:ellipsis;white-space:nowrap; }
.today-schedule-pill { align-self:flex-start;margin-top:5px;padding:2px 7px;font-size:10px; }
.today-schedule-empty { flex:1 1 auto;min-height:0;padding:18px 12px;color:#64748b; }
.today-schedule-page { flex:0 0 43px;display:flex;align-items:center;justify-content:center;gap:9px;padding:6px 9px;border-top:1px solid #dfe7f0;background:#f8fafc; }
.today-schedule-page button { display:grid;place-items:center;width:27px;height:27px;padding:0;border:1px solid #cbd5e1;border-radius:7px;background:#fff;color:#475569;font-size:11px;cursor:pointer; }
.today-schedule-page button:hover:not(:disabled) { border-color:#8b5cf6;background:#f6f0ff;color:#54218d; }
.today-schedule-page button:disabled { opacity:.35;cursor:default; }
.today-schedule-page span { min-width:77px;text-align:center;color:#475569;font-size:11px;font-weight:750;white-space:nowrap;font-variant-numeric:tabular-nums; }
.today-schedule-block--overdue { border-color:#fecaca;background:#fff; }
.today-schedule-block--overdue .today-schedule-block-head { background:#fff3f3;border-bottom-color:#fecaca; }
.today-schedule-block--overdue .today-schedule-block-head strong,.today-schedule-block--overdue .today-schedule-block-head small { color:#b91c1c; }
.today-schedule-block--overdue .today-schedule-code { border-color:#fecaca;background:#fff1f2;color:#b91c1c; }
.today-schedule-block--overdue .today-schedule-time { color:#b91c1c; }
.today-schedule-block--overdue .today-schedule-item { border-bottom-color:#fee2e2; }
.today-schedule-block--overdue .today-schedule-page { border-top-color:#fee2e2;background:#fff9f9; }
.today-schedule-block--overdue .today-schedule-empty i { color:#16a34a; }
.today-schedule-note { margin-top:12px;color:#64748b; }
/* Vị trí của Hibrid cố định: không trượt theo transform mở modal; z-index vẫn cao hơn backdrop. */
.today-schedule-companion,.today-schedule-companion.is-visible { transform:none!important;transition:opacity .18s ease,visibility 0s linear .18s!important; }
.today-schedule-companion.is-visible { transition:opacity .18s ease,visibility 0s!important; }
@media(max-width:1050px) { .today-schedule-grid.has-overdue { gap:8px; } .today-schedule-block-head { padding:8px; } .today-schedule-block-head strong { font-size:12px; } .today-schedule-block-head small { font-size:10px; } }
@media(max-width:760px) { .today-schedule-grid.has-overdue { grid-template-columns:1fr;max-width:600px; } .today-schedule-grid { grid-template-columns:1fr;max-width:600px; } .today-schedule-block { height:350px;min-height:350px; } #<%= mdlTodaySchedule.ClientID %> .modal-body { max-height:calc(100dvh - 125px); } }

/* V29: less modal width means reserved room for Hibrid; keep ExtraModal and its X unchanged. */
#<%= mdlTodaySchedule.ClientID %> .modal-dialog { width:min(1010px,calc(100vw - 390px))!important;max-width:calc(100vw - 24px)!important; }
.today-schedule-grid.has-overdue { max-width:958px;gap:10px; }
.today-schedule-grid:not(.has-overdue) { max-width:675px;gap:10px; }
.today-schedule-content { padding:13px 15px 12px; }
.today-schedule-intro { padding:11px 13px;margin-bottom:12px; }
.today-schedule-item { padding:8px 10px; }
.today-schedule-task-heading { display:flex;align-items:flex-start;gap:7px;min-width:0;line-height:1.35; }
.today-schedule-task-title { display:block;min-width:0;max-width:100%;font-size:12.5px;color:#26354b;line-height:1.45;overflow-wrap:anywhere; }
.today-schedule-task-title strong { display:inline;font-size:12.5px;font-weight:800;color:#24354c; }
.today-schedule-task-code { color:#344966;font-weight:800;white-space:nowrap;margin-right:3px; }
.today-schedule-status-line { display:flex;align-items:center;gap:6px;flex-wrap:wrap;margin-top:5px; }
.today-schedule-task-status { display:inline-flex;align-items:center;gap:4px;padding:3px 7px;border:1px solid #d7e1ec;border-radius:6px;background:#f4f7fa;color:#52647a;font-size:10px;line-height:1.25;font-weight:800;white-space:nowrap; }
.today-schedule-task-status.is-in-progress { border-color:#bed7ff;background:#edf5ff;color:#1d4ed8; }
.today-schedule-task-status.is-completed { border-color:#bfe6d0;background:#eaf8f0;color:#167548; }
.today-schedule-task-status.is-review { border-color:#efd69a;background:#fff9e9;color:#95620c; }
.today-schedule-task-status.is-default { border-color:#d6e0e9;background:#f3f6fa;color:#52647a; }
.today-schedule-deadline { margin-left:auto;color:#66778b;font-size:10.5px;font-weight:750;white-space:nowrap; }
.today-schedule-deadline.is-overdue { color:#b91c1c; }
.today-schedule-item .today-schedule-meta { margin-top:5px; }
.today-schedule-task-alert { display:inline-flex;align-items:center;align-self:flex-start;margin-top:5px;padding:2px 6px;border-radius:5px;background:#fff1f1;border:1px solid #fecaca;color:#b91c1c;font-size:10px;font-weight:750; }
.today-schedule-block--overdue .today-schedule-task-code { color:#b91c1c; }
.today-schedule-block--tasks .today-schedule-task-code { color:#1d4ed8; }
.today-schedule-companion { pointer-events:none; }
@media(max-width:1390px) { #<%= mdlTodaySchedule.ClientID %> .modal-dialog { width:min(940px,calc(100vw - 400px))!important;max-width:calc(100vw - 20px)!important; } .today-schedule-companion { width:170px; } }
@media(max-width:1020px) { #<%= mdlTodaySchedule.ClientID %> .modal-dialog { width:calc(100vw - 28px)!important;max-width:calc(100vw - 28px)!important; } }
@media(max-width:760px) { #<%= mdlTodaySchedule.ClientID %> .modal-dialog { width:calc(100vw - 18px)!important;max-width:calc(100vw - 18px)!important; } }

/* V30 | Hai kich thuoc ExtraModal theo so bang, giu robot dung ben trai va nut X goc. */
#<%= mdlTodaySchedule.ClientID %>.today-schedule-mode-three .modal-dialog { width:min(960px,calc(100vw - 34px))!important;max-width:calc(100vw - 34px)!important; }
#<%= mdlTodaySchedule.ClientID %>.today-schedule-mode-two .modal-dialog { width:min(675px,calc(100vw - 34px))!important;max-width:calc(100vw - 34px)!important; }
.today-schedule-grid.has-overdue { max-width:900px;grid-template-columns:repeat(3,minmax(0,1fr));gap:10px; }
.today-schedule-grid:not(.has-overdue) { max-width:615px;grid-template-columns:repeat(2,minmax(0,1fr));gap:10px; }
.today-schedule-content { padding:13px 14px 12px; }
.today-schedule-item { padding:10px 11px; }
.today-schedule-task-top { display:flex;align-items:center;justify-content:space-between;gap:8px;min-width:0;width:100%;margin-bottom:7px; }
.today-schedule-task-top .today-schedule-task-title { display:block;flex:1 1 auto;min-width:0;max-width:none;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;font-size:12.5px;line-height:1.5; }
.today-schedule-task-top .today-schedule-task-title strong { display:inline;font-size:12.5px;white-space:nowrap; }
.today-schedule-task-top .today-schedule-task-status { display:inline-flex;align-items:center;justify-content:center;flex:0 0 auto;max-width:46%;margin:0;padding:4px 6px;font-size:10px;white-space:nowrap; }
.today-schedule-task-details { display:flex;align-items:center;justify-content:space-between;gap:6px;min-height:23px;min-width:0; }
.today-schedule-task-details .today-schedule-deadline { display:inline-flex;align-items:center;gap:4px;flex:1 1 auto;min-width:0;margin:0;color:#64748b;font-size:10.5px;line-height:1.35;font-weight:700;white-space:nowrap; }
.today-schedule-task-details .today-schedule-deadline.is-overdue { color:#b91c1c; }
.today-schedule-task-details .today-schedule-task-alert { display:inline-flex;align-items:center;gap:4px;flex:0 0 auto;align-self:center;margin:0;padding:3px 6px;border:1px solid #fecaca;border-radius:6px;background:#fff1f2;color:#b91c1c;font-size:10px;font-weight:800;line-height:1.25;white-space:nowrap;cursor:help; }
.today-schedule-task-details .today-schedule-task-alert.is-due-soon { background:#fffbeb;border-color:#fde68a;color:#a16207; }
.today-schedule-task-details .today-schedule-task-alert:focus-visible { outline:2px solid #9871cc;outline-offset:2px; }
.today-schedule-deadline i { font-size:10px; }
.today-schedule-task-tooltip { position:fixed;z-index:2147483100;max-width:245px;padding:9px 12px;border:1px solid #d6c4ec;border-radius:9px;background:#fff;color:#3d2859;box-shadow:0 11px 26px rgba(40,23,66,.23);font-size:12px;font-weight:750;line-height:1.4;pointer-events:none;white-space:normal; }
@media(max-width:1020px) { #<%= mdlTodaySchedule.ClientID %>.today-schedule-mode-three .modal-dialog { width:min(920px,calc(100vw - 24px))!important;max-width:calc(100vw - 24px)!important; } #<%= mdlTodaySchedule.ClientID %>.today-schedule-mode-two .modal-dialog { width:min(675px,calc(100vw - 24px))!important;max-width:calc(100vw - 24px)!important; } }
@media(max-width:760px) { #<%= mdlTodaySchedule.ClientID %>.today-schedule-mode-three .modal-dialog,#<%= mdlTodaySchedule.ClientID %>.today-schedule-mode-two .modal-dialog { width:calc(100vw - 18px)!important;max-width:calc(100vw - 18px)!important; } .today-schedule-grid.has-overdue,.today-schedule-grid:not(.has-overdue) { grid-template-columns:1fr;max-width:100%; } .today-schedule-task-top .today-schedule-task-title { white-space:normal;overflow-wrap:anywhere; } }

/* Lịch trình: các tên bấm được, không thay đổi chiều cao và bố cục các dòng hiện tại. */
.today-schedule-link { appearance:none;-webkit-appearance:none;border:0;background:transparent;padding:0;margin:0;color:inherit;font-family:inherit;font-size:inherit;font-weight:inherit;line-height:inherit;text-align:left;cursor:pointer; }
.today-schedule-task-top button.today-schedule-task-title { color:#24354c; }
.today-schedule-task-top button.today-schedule-task-title:hover,.today-schedule-task-top button.today-schedule-task-title:focus-visible { color:#2563eb; }
.today-schedule-task-top button.today-schedule-task-title:hover strong,.today-schedule-task-top button.today-schedule-task-title:focus-visible strong { color:#2563eb; }
.today-schedule-name.today-schedule-link { width:100%; }
.today-schedule-name.today-schedule-link:hover,.today-schedule-name.today-schedule-link:focus-visible { color:#6d28d9; }
.today-schedule-meta .today-schedule-project-link { display:block;min-width:0;max-width:100%;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;color:#63528b;font-size:11px;font-weight:750; }
.today-schedule-meta .today-schedule-project-link:hover,.today-schedule-meta .today-schedule-project-link:focus-visible { color:#5b21b6;text-decoration:underline; }
.today-schedule-link:focus-visible { outline:2px solid #9b7bd3;outline-offset:2px;border-radius:3px; }
/* Khi chi tiết nằm trên lịch trình: backdrop mới chỉ phủ lớp lịch trình, không đóng nó. */
body.today-schedule-detail-layer #<%= TodayModalClientId %> { z-index:1055!important; }
body.today-schedule-detail-layer #<%= TodayTaskModalClientId %>,body.today-schedule-detail-layer #<%= TodayMeetModalClientId %> { z-index:2105!important; }
body.today-schedule-detail-layer .modal-backdrop:last-of-type { z-index:2100!important; }
</style>
<SweetSoft:ExtraModal runat="server" ID="mdlTodaySchedule" Type="Primary" Size="Large" Title="Lịch trình hôm nay">
    <ContentTemplate>
        <asp:UpdatePanel runat="server" ID="upTodaySchedule" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <asp:HiddenField runat="server" ID="hfScheduleSelectedDate" />
                <asp:LinkButton runat="server" ID="btnTodayReload" OnClick="btnTodayReload_Click" CausesValidation="false" Style="display:none" aria-hidden="true">Tải lịch trình</asp:LinkButton>
                <asp:Literal runat="server" ID="litTodaySchedule" Text="&lt;div class='today-schedule-empty'&gt;Nhấn Q hoặc chọn Lịch trình hôm nay để tải dữ liệu.&lt;/div&gt;" />
            </ContentTemplate>
        </asp:UpdatePanel>
        <%-- Async trigger nằm ngoài panel danh sách, tránh reset scroll và phân trang. --%>
        <asp:HiddenField runat="server" ID="hfTodayOpenKind" />
        <asp:HiddenField runat="server" ID="hfTodayOpenId" />
        <asp:LinkButton runat="server" ID="btnTodayOpenDetail" OnClick="btnTodayOpenDetail_Click" CausesValidation="false" Style="display:none" aria-hidden="true">Xem chi tiết</asp:LinkButton>
    </ContentTemplate>
</SweetSoft:ExtraModal>
<%-- Hai ExtraModal riêng biệt, nằm cùng cấp với popup lịch trình để có thể xếp lớp. --%>
<SweetSoft:CtrlViewTaskDetail runat="server" ID="CtrlTodayTaskDetail" />
<SweetSoft:CtrlViewMeetDetail runat="server" ID="CtrlTodayMeetDetail" />
<div id="todayScheduleCompanion" class="today-schedule-companion" aria-hidden="true">
    <div class="today-schedule-companion-talk">
        <strong>Hôm nay có:</strong>
        <span><i class="far fa-calendar-alt"></i><b id="todaySpeechMeetCount">0 cuộc họp</b></span>
        <span><i class="fas fa-tasks"></i><b id="todaySpeechTaskCount">0 công việc cần làm</b></span>
        <span class="today-schedule-overdue" id="todaySpeechOverdueRow" hidden><i class="fas fa-exclamation-triangle"></i><b id="todaySpeechOverdueCount">0 công việc quá hạn</b></span>
    </div>
    <svg class="today-schedule-companion-bot" viewBox="0 0 180 240" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
        <ellipse cx="92" cy="229" rx="62" ry="8" fill="#b1a0cd" opacity=".30"/>
        <path d="M90 40V24" stroke="#9063c8" stroke-width="6" stroke-linecap="round"/><circle cx="90" cy="18" r="9" fill="#ffc568" stroke="#e9a63b" stroke-width="2"/>
        <path d="M50 122c-16 5-22 16-24 41-1 14 7 26 22 26h84c14 0 22-12 22-26-2-25-10-37-25-41Z" fill="#c4a7eb" stroke="#9d76d1" stroke-width="4"/>
        <path d="M65 167h50l5 24-5 23H65l-5-23Z" fill="#8b53c2"/><rect x="68" y="173" width="44" height="28" rx="12" fill="#60309d"/><circle cx="90" cy="187" r="7" fill="#f7e5ff"/>
        <path d="M70 211v17m40-17v17" stroke="#a283ca" stroke-width="14" stroke-linecap="round"/><path d="M61 228h26m21 0h26" stroke="#65309c" stroke-width="12" stroke-linecap="round"/>
        <path d="M39 141c-15 11-18 29-15 45" stroke="#ab83d8" stroke-width="16" stroke-linecap="round" fill="none"/><circle cx="24" cy="189" r="13" fill="#e4cfff" stroke="#9b6cc9" stroke-width="3"/>
        <g style="transform-origin:140px 142px;animation:todayRobotWave 5s ease-in-out infinite"><path d="M142 141c21-3 22-17 24-28" stroke="#ae82d8" stroke-width="16" stroke-linecap="round" fill="none"/><rect x="154" y="100" width="19" height="25" rx="9" fill="#e4cfff" stroke="#9b6cc9" stroke-width="3"/></g>
        <rect x="24" y="46" width="132" height="90" rx="38" fill="#e8d9fb" stroke="#ad84d9" stroke-width="5"/><rect x="39" y="61" width="102" height="57" rx="27" fill="#562490"/><ellipse cx="67" cy="86" rx="11" ry="14" fill="#fff"/><ellipse cx="112" cy="86" rx="11" ry="14" fill="#fff"/><circle cx="70" cy="82" r="3" fill="#c8a6ed"/><circle cx="115" cy="82" r="3" fill="#c8a6ed"/><path d="M77 103q13 15 27 0" stroke="#fff" stroke-width="5" fill="none" stroke-linecap="round"/><circle cx="25" cy="88" r="11" fill="#bd94e8"/><circle cx="155" cy="88" r="11" fill="#bd94e8"/>
        <style>@keyframes todayRobotWave { 0%,16%,100% { transform:rotate(0deg) } 4%,9% { transform:rotate(-17deg) } 7%,13% { transform:rotate(12deg) } }</style>
    </svg>
    <span class="today-schedule-companion-name">Trợ lý Hibrid</span>
</div>
<script type="text/javascript">
    (function () {
        if (window.TodayScheduleUI) return;
        function modal() { return document.getElementById('<%= TodayModalClientId %>') || document.querySelector('[id$="mdlTodaySchedule"].modal'); }
        function companion() { return document.getElementById('todayScheduleCompanion'); }
        function taskDetailModal() { return document.getElementById('<%= TodayTaskModalClientId %>'); }
        function meetDetailModal() { return document.getElementById('<%= TodayMeetModalClientId %>'); }
        function isDetailModal(el) { return !!(el && (el === taskDetailModal() || el === meetDetailModal())); }
        function isDetailVisible() { return [taskDetailModal(), meetDetailModal()].some(function (el) { return el && (el.classList.contains('show') || el.classList.contains('in')); }); }
        // Render robot trực tiếp dưới body: tránh stacking context của Master, UpdatePanel và backdrop.
        // Robot vẫn nằm BÊN TRÁI dialog nhưng có z-index cao hơn backdrop, cùng độ sáng với modal.
        function placeCompanionAboveBackdrop() {
            var bot = companion(), el = modal(); if (!bot || !el) return;
            if (bot.parentNode !== document.body) document.body.appendChild(bot);
            var backdrop = document.querySelector('.modal-backdrop:last-of-type');
            var zModal = parseInt(window.getComputedStyle(el).zIndex, 10) || 1055;
            var zBackdrop = backdrop ? (parseInt(window.getComputedStyle(backdrop).zIndex, 10) || 1050) : 1050;
            bot.style.setProperty('z-index', String(Math.max(2000, zModal + 2, zBackdrop + 2)), 'important');
        }
        function isVisible() { var el = modal(); return !!(el && (el.classList.contains('show') || el.classList.contains('in'))); }
        // V31: Hibrid được đặt vào vị trí CUỐI CÙNG khi đang ẩn, rồi mới hiện.
        // Không lấy getBoundingClientRect của modal-dialog khi ExtraModal đang trượt vào.
        function positionBot() {
            var el = modal(), bot = companion(); if (!el || !bot) return false;
            placeCompanionAboveBackdrop();
            var dialog = el.querySelector('.modal-dialog'); if (!dialog || !dialog.offsetWidth) return false;
            // Khôi phục display trước khi đo: nếu màn hình hẹp từng ẩn Hibrid,
            // lúc rộng lại phải đo đúng chiều rộng mới, không dùng fallback 265px.
            bot.style.display = '';
            var bw = bot.offsetWidth || 265;
            var stableDialogLeft = Math.round((window.innerWidth - dialog.offsetWidth) / 2);
            var stableDialogTop = dialog.offsetTop;
            var available = stableDialogLeft - 16;
            if (window.innerWidth <= 1020 || available < bw) { bot.style.display = 'none'; return false; }
            bot.style.display = '';
            bot.style.left = Math.round(stableDialogLeft - bw - 18) + 'px';
            var botHeight = bot.offsetHeight || 365;
            bot.style.top = Math.max(12, Math.min(window.innerHeight - botHeight - 14, stableDialogTop + 42)) + 'px';
            return true;
        }
        // V34: do sẵn tọa độ khi modal vẫn ẩn, hiển thị robot đúng khi modal bắt đầu có class show/in.
        // Không đợi requestAnimationFrame hay shown.bs.modal (hai đường này gây Hibrid xuất hiện muộn).
        var robotReady = false, robotObservedModal = null, robotVisibilityObserver = null;
        function hideRobot() { var bot = companion(); if (bot) bot.classList.remove('is-visible'); }
        function prepareRobot() {
            var el = modal(), bot = companion(); if (!el || !bot) return false;
            // Modal đang display:none thì đo kích thước bằng display:block + visibility:hidden tạm thời.
            // Khôi phục inline style NGAY trong cùng call stack; người dùng không thấy frame trung gian.
            var needMeasure = !el.querySelector('.modal-dialog') || !el.querySelector('.modal-dialog').offsetWidth;
            var oldDisplay = el.style.getPropertyValue('display'), oldDisplayPriority = el.style.getPropertyPriority('display');
            var oldVisibility = el.style.getPropertyValue('visibility'), oldVisibilityPriority = el.style.getPropertyPriority('visibility');
            if (needMeasure) { el.style.setProperty('visibility', 'hidden', 'important'); el.style.setProperty('display', 'block', 'important'); }
            try { robotReady = positionBot(); } finally {
                if (needMeasure) {
                    if (oldDisplay) el.style.setProperty('display', oldDisplay, oldDisplayPriority); else el.style.removeProperty('display');
                    if (oldVisibility) el.style.setProperty('visibility', oldVisibility, oldVisibilityPriority); else el.style.removeProperty('visibility');
                }
            }
            return robotReady;
        }
        function revealRobotWithModal() {
            if (!isVisible() || isDetailVisible()) return;
            var bot = companion(); if (!bot) return;
            if (!robotReady && !prepareRobot()) return;
            bot.classList.add('is-visible');
        }
        function observeModalOpen() {
            var el = modal(); if (!el || typeof MutationObserver === 'undefined') return;
            if (robotObservedModal === el) return;
            if (robotVisibilityObserver) robotVisibilityObserver.disconnect();
            robotObservedModal = el;
            robotVisibilityObserver = new MutationObserver(function () { if (isVisible()) revealRobotWithModal(); });
            robotVisibilityObserver.observe(el, { attributes: true, attributeFilter: ['class'] });
        }
        function onScheduleOpening() { hideRobot(); robotReady = false; syncData(); prepareRobot(); observeModalOpen(); }
        function onScheduleOpened() { syncData(); revealRobotWithModal(); }
        function onScheduleClosing() { hideRobot(); robotReady = false; }
        function niceDay(dateString) {
            var raw = String(dateString || '');
            var match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(raw);
            if (!match) return 'Lịch trình hôm nay';
            var day = new Date(+match[1], +match[2] - 1, +match[3]);
            if (day.getFullYear() !== +match[1] || day.getMonth() !== +match[2] - 1 || day.getDate() !== +match[3]) return 'Lịch trình hôm nay';
            var now = new Date();
            if (day.getFullYear() === now.getFullYear() && day.getMonth() === now.getMonth() && day.getDate() === now.getDate()) return 'Lịch trình hôm nay';
            var names = ['Chủ nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'];
            return 'Lịch trình ' + names[day.getDay()] + ', ' + match[3] + '/' + match[2] + '/' + match[1];
        }
        function syncTitle() {
            var el = modal(), content = document.querySelector('.today-schedule-content[data-schedule-date]');
            if (!el) return;
            var day = content ? content.getAttribute('data-schedule-date') : '';
            var title = el.querySelector('.modal-title,.modal-header h4,.modal-header h5');
            if (title) title.textContent = niceDay(day);
            var talk = document.querySelector('.today-schedule-companion-talk > strong');
            if (talk) talk.textContent = niceDay(day) === 'Lịch trình hôm nay' ? 'Hôm nay có:' : 'Ngày được chọn có:';
        }
        function openDate(dateString) {
            var raw = dateString || '';
            if (raw && !/^(\d{4})-(\d{2})-(\d{2})$/.test(raw)) return false;
            var selected = document.getElementById('<%= SelectedDateClientId %>');
            if (selected) selected.value = raw;
            var refresh = document.getElementById('<%= TodayRefreshClientId %>');
            if (refresh) refresh.click();
            else show();
            return false;
        }
        function syncScheduleLayout() {
            var el = modal(); if (!el) return;
            var grid = el.querySelector('.today-schedule-grid');
            var three = !!(grid && grid.classList.contains('has-overdue'));
            el.classList.toggle('today-schedule-mode-three', three);
            el.classList.toggle('today-schedule-mode-two', !three);
            if (isVisible()) positionBot();
        }
        function syncData() {
            var data = document.querySelector('.today-schedule-content[data-tasks]'); if (!data) return;
            syncScheduleLayout();
            var tasks = document.getElementById('todaySpeechTaskCount'), meetings = document.getElementById('todaySpeechMeetCount');
            var overdueRow = document.getElementById('todaySpeechOverdueRow'), overdueCount = document.getElementById('todaySpeechOverdueCount');
            var isToday = !!data.querySelector('.today-schedule-grid.has-overdue');
            if (tasks) tasks.textContent = (Number(data.getAttribute('data-tasks')) || 0) + ' công việc cần làm';
            if (meetings) meetings.textContent = (Number(data.getAttribute('data-meetings')) || 0) + ' cuộc họp';
            if (overdueCount) overdueCount.textContent = (Number(data.getAttribute('data-overdue')) || 0) + ' công việc quá hạn';
            if (overdueRow) overdueRow.hidden = !isToday;
            syncTitle();
        }
        function show() {
            var el = modal(); if (!el) return;
            syncData(); // Chốt ngay chiều rộng popup 2/3 bảng, trước khi đo vị trí Hibrid.
            if (isVisible()) { revealRobotWithModal(); return; }
            hideRobot(); robotReady = false; prepareRobot(); observeModalOpen();
            if (window.bootstrap && window.bootstrap.Modal) window.bootstrap.Modal.getOrCreateInstance(el).show();
            else if (window.jQuery && window.jQuery.fn.modal) window.jQuery(el).modal('show');
            // Fallback cho ExtraModal không phát show.bs.modal (không dùng animation frame).
            if (isVisible()) revealRobotWithModal();
        }
        function hide() {
            var el = modal(); if (!el) return;
            hideRobot(); // Hibrid tắt NGAY KHI popup bắt đầu đóng, không đợi hidden.bs.modal.
            if (window.bootstrap && window.bootstrap.Modal) window.bootstrap.Modal.getOrCreateInstance(el).hide();
            else if (window.jQuery && window.jQuery.fn.modal) window.jQuery(el).modal('hide');
        }
        function toggle() {
            if (isVisible()) { hide(); return; }
            openDate('');
        }
        // Các tên của task/meet/project là nút HTML do SQL render; dùng một async LinkButton
        // thật của WebForms để gọi OpenModal(Guid), không đóng ExtraModal lịch trình.
        document.addEventListener('click', function (event) {
            var link = event.target && event.target.closest && event.target.closest('[data-today-open-kind][data-today-open-id]');
            if (!link || !link.closest('.today-schedule-content')) return;
            event.preventDefault(); event.stopPropagation();
            var kind = link.getAttribute('data-today-open-kind');
            var id = link.getAttribute('data-today-open-id');
            if (!/^(task|meet|project)$/.test(kind) || !/^[0-9a-f]{8}-[0-9a-f-]{27,28}$/i.test(id)) return;
            var kindField = document.getElementById('<%= TodayOpenKindClientId %>');
            var idField = document.getElementById('<%= TodayOpenIdClientId %>');
            var trigger = document.getElementById('<%= TodayOpenButtonClientId %>');
            if (!kindField || !idField || !trigger) return;
            kindField.value = kind; idField.value = id;
            if (kind !== 'project') { hideRobot(); hideScheduleTip(); }
            trigger.click();
        });
        function onDetailLayerShow(el) {
            if (!isDetailModal(el) || !isVisible()) return;
            document.body.classList.add('today-schedule-detail-layer');
            hideRobot(); hideScheduleTip();
        }
        function onDetailLayerHidden(el) {
            if (!isDetailModal(el)) return;
            if (isDetailVisible()) return;
            document.body.classList.remove('today-schedule-detail-layer');
            if (isVisible()) {
                // Bootstrap 4 bỏ modal-open sau khi đóng modal trên, dù modal dưới vẫn mở.
                document.body.classList.add('modal-open');
                revealRobotWithModal();
            }
        }
        document.addEventListener('show.bs.modal', function (e) { onDetailLayerShow(e.target); });
        document.addEventListener('shown.bs.modal', function (e) { onDetailLayerShow(e.target); });
        document.addEventListener('hidden.bs.modal', function (e) { onDetailLayerHidden(e.target); });
        if (window.jQuery) window.jQuery(document).on('show.bs.modal shown.bs.modal hidden.bs.modal', function (e) {
            if (e.type === 'show' || e.type === 'shown') onDetailLayerShow(e.target);
            else if (e.type === 'hidden') onDetailLayerHidden(e.target);
        });
        // Client-side pagination: ba vùng hoạt động độc lập, không ASP.NET postback.
        function applySchedulePage(block, requested) {
            if (!block) return;
            var total = Math.max(0, Number(block.getAttribute('data-today-count')) || 0);
            var size = Math.max(1, Number(block.getAttribute('data-today-size')) || 3);
            var pages = Math.max(1, Math.ceil(total / size));
            var page = Math.max(1, Math.min(pages, requested));
            block.setAttribute('data-today-page', String(page));
            block.querySelectorAll('[data-today-row-index]').forEach(function (row) {
                var i = Number(row.getAttribute('data-today-row-index'));
                row.hidden = i < (page - 1) * size || i >= page * size;
            });
            var prev = block.querySelector('[data-today-page-action="prev"]');
            var next = block.querySelector('[data-today-page-action="next"]');
            if (prev) prev.disabled = page <= 1;
            if (next) next.disabled = page >= pages;
            var label = block.querySelector('[data-today-page-label]');
            if (label) label.textContent = total ? (((page - 1) * size + 1) + '–' + Math.min(total, page * size) + ' / ' + total) : '0 mục';
            var list = block.querySelector('.today-schedule-list');
            if (list) list.scrollTop = 0;
        }
        document.addEventListener('click', function (e) {
            var button = e.target && e.target.closest && e.target.closest('[data-today-page-action]');
            if (!button) return;
            var block = button.closest('[data-today-block]');
            if (!block || !block.closest('.today-schedule-content')) return;
            e.preventDefault();
            if (button.disabled) return;
            var current = Number(block.getAttribute('data-today-page')) || 1;
            applySchedulePage(block, current + (button.getAttribute('data-today-page-action') === 'next' ? 1 : -1));
        });
        // Tooltip for due/overdue days: floated under body to avoid clipping inside scrollable rows.
        var scheduleTip = null, scheduleTipSource = null;
        function hideScheduleTip() { if (scheduleTip && scheduleTip.parentNode) scheduleTip.parentNode.removeChild(scheduleTip); scheduleTip = null; scheduleTipSource = null; }
        function placeScheduleTip() {
            if (!scheduleTip || !scheduleTipSource || !document.body.contains(scheduleTipSource)) { hideScheduleTip(); return; }
            var r = scheduleTipSource.getBoundingClientRect(), w = scheduleTip.offsetWidth, h = scheduleTip.offsetHeight, pad = 10;
            var left = Math.min(window.innerWidth - w - pad, Math.max(pad, r.right - w));
            var above = r.top - h - 9, top = above >= pad ? above : Math.min(window.innerHeight - h - pad, r.bottom + 9);
            scheduleTip.style.left = Math.round(left) + 'px'; scheduleTip.style.top = Math.round(top) + 'px';
        }
        function showScheduleTip(target) {
            if (!target || !target.closest('.today-schedule-content')) return;
            if (scheduleTipSource === target) return;
            hideScheduleTip(); scheduleTipSource = target;
            scheduleTip = document.createElement('div'); scheduleTip.className = 'today-schedule-task-tooltip';
            scheduleTip.setAttribute('role', 'tooltip'); scheduleTip.textContent = target.getAttribute('data-today-tooltip') || '';
            document.body.appendChild(scheduleTip); placeScheduleTip();
        }
        document.addEventListener('mouseover', function (e) { var t = e.target.closest && e.target.closest('.today-schedule-task-alert[data-today-tooltip]'); if (t) showScheduleTip(t); });
        document.addEventListener('mouseout', function (e) { if (scheduleTipSource && scheduleTipSource.contains(e.target) && (!e.relatedTarget || !scheduleTipSource.contains(e.relatedTarget))) hideScheduleTip(); });
        document.addEventListener('focusin', function (e) { var t = e.target.closest && e.target.closest('.today-schedule-task-alert[data-today-tooltip]'); if (t) showScheduleTip(t); });
        document.addEventListener('focusout', function (e) { if (e.target === scheduleTipSource) hideScheduleTip(); });
        document.addEventListener('scroll', hideScheduleTip, true);
        window.addEventListener('resize', hideScheduleTip);
        document.addEventListener('hidden.bs.modal', function (e) { if (e.target === modal()) hideScheduleTip(); });
        window.TodayScheduleUI = { toggle: toggle, openDate: openDate, show: show, hide: hide, isVisible: isVisible };
        document.addEventListener('keydown', function (event) {
            if (String(event.key || '').toLowerCase() !== 'q' || event.altKey || event.ctrlKey || event.metaKey || event.repeat || event.isComposing) return;
            var t = event.target; if (t && (t.isContentEditable || (t.closest && t.closest('input,textarea,select,[contenteditable],[role="textbox"],.CodeMirror,.monaco-editor')))) return;
            if (document.querySelector('.modal.show:not([id$="mdlTodaySchedule"]),.modal.in:not([id$="mdlTodaySchedule"])')) return;
            event.preventDefault(); event.stopPropagation(); toggle();
        });
        document.addEventListener('click', function (event) { var btn = event.target && event.target.closest && event.target.closest('[data-open-today-schedule]'); if (!btn) return; event.preventDefault(); event.stopPropagation(); toggle(); });
        // Hỗ trợ cả Bootstrap jQuery (v4) và native Bootstrap (v5), kể cả ExtraModal mở từ C#.
        ['show.bs.modal', 'shown.bs.modal', 'hide.bs.modal', 'hidden.bs.modal'].forEach(function (name) {
            document.addEventListener(name, function (event) {
                if (event.target !== modal()) return;
                if (name === 'show.bs.modal') onScheduleOpening();
                else if (name === 'shown.bs.modal') onScheduleOpened();
                else onScheduleClosing();
            });
        });
        if (window.jQuery) window.jQuery(document).on('show.bs.modal shown.bs.modal hide.bs.modal hidden.bs.modal', function (event) {
            if (event.target !== modal()) return;
            if (event.type === 'show') onScheduleOpening();
            else if (event.type === 'shown') onScheduleOpened();
            else onScheduleClosing();
        });
        window.addEventListener('resize', function () { if (isVisible()) { robotReady = positionBot(); if (!robotReady) hideRobot(); } });
        observeModalOpen();
        if (window.Sys && Sys.Application) Sys.Application.add_load(syncData);
    })();
</script>
