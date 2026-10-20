<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlTodaySchedule.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.Controls.TodaySchedule.CtrlTodaySchedule" %>
<%@ Register Assembly="SweetSoft.QLDA.Controls" Namespace="SweetSoft.QLDA.Controls" TagPrefix="SweetSoft" %>
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
.today-schedule-companion-bot { display:block;margin:0 auto;width:168px;height:220px;filter:drop-shadow(0 8px 12px rgba(67,27,112,.15));animation:todayBotHello 5s ease-in-out infinite; }
.today-schedule-companion-name { display:table;margin:-3px auto 0;padding:5px 13px;border:1px solid #d9c1ef;border-radius:999px;background:rgba(255,255,255,.96);box-shadow:0 4px 12px rgba(61,32,95,.12);color:#54218d;font-size:12px;line-height:1.4;font-weight:850;letter-spacing:.1px;text-align:center; }
@keyframes todayBotHello { 0%,100% { transform:translateY(0) } 50% { transform:translateY(-5px) } }
@media(max-width:1390px) { .today-schedule-companion { width:180px; } .today-schedule-companion-talk { padding:11px; } .today-schedule-companion-talk strong { font-size:13px; } .today-schedule-companion-talk span { font-size:11px; } .today-schedule-companion-bot { width:115px;height:150px; } }
@media(max-width:1020px) { .today-schedule-companion { display:none!important; } }
@media(max-width:680px) { .today-schedule-grid { grid-template-columns:1fr; } .today-schedule-content { padding:10px; } .today-schedule-list { max-height:36vh; } }
@media(prefers-reduced-motion:reduce) { .today-schedule-companion,.today-schedule-companion-bot { animation:none!important;transition:none!important; } }
</style>
<SweetSoft:ExtraModal runat="server" ID="mdlTodaySchedule" Type="Primary" Size="Large" Title="Lịch trình hôm nay">
    <ContentTemplate>
        <asp:UpdatePanel runat="server" ID="upTodaySchedule" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <asp:LinkButton runat="server" ID="btnTodayReload" OnClick="btnTodayReload_Click" CausesValidation="false" Style="display:none" aria-hidden="true">Tải lịch trình</asp:LinkButton>
                <asp:Literal runat="server" ID="litTodaySchedule" Text="&lt;div class='today-schedule-empty'&gt;Nhấn Q hoặc chọn Lịch trình hôm nay để tải dữ liệu.&lt;/div&gt;" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
</SweetSoft:ExtraModal>
<div id="todayScheduleCompanion" class="today-schedule-companion" aria-hidden="true">
    <div class="today-schedule-companion-talk">
        <strong>Hôm nay có:</strong>
        <span><i class="far fa-calendar-alt"></i><b id="todaySpeechMeetCount">0 cuộc họp</b></span>
        <span><i class="fas fa-tasks"></i><b id="todaySpeechTaskCount">0 công việc cần làm</b></span>
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
    // Render robot trực tiếp dưới body: tránh stacking context của Master, UpdatePanel và backdrop.
    // Robot vẫn nằm BÊN TRÁI dialog nhưng có z-index cao hơn backdrop, cùng độ sáng với modal.
    function placeCompanionAboveBackdrop() {
        var bot=companion(),el=modal();if(!bot || !el)return;
        if(bot.parentNode!==document.body)document.body.appendChild(bot);
        var backdrop=document.querySelector('.modal-backdrop:last-of-type');
        var zModal=parseInt(window.getComputedStyle(el).zIndex,10)||1055;
        var zBackdrop=backdrop ? (parseInt(window.getComputedStyle(backdrop).zIndex,10)||1050) : 1050;
        bot.style.setProperty('z-index',String(Math.max(2000,zModal+2,zBackdrop+2)),'important');
    }
    function isVisible() { var el=modal();return !!(el && (el.classList.contains('show') || el.classList.contains('in'))); }
    function positionBot() {
        var el=modal(),bot=companion(); if(!el || !bot || !isVisible()) return;
        placeCompanionAboveBackdrop();
        var dialog=el.querySelector('.modal-dialog'); if(!dialog) return;
        var rect=dialog.getBoundingClientRect(), bw=bot.offsetWidth || 265;
        if(rect.left < bw+20){bot.style.display='none';return;} bot.style.display='';
        bot.style.left=Math.max(8,rect.left-bw-18)+'px';
        var botHeight=bot.offsetHeight || 365;
        bot.style.top=Math.max(12,Math.min(window.innerHeight-botHeight-14,rect.top+42))+'px';
    }
    function syncData() {
        var data=document.querySelector('.today-schedule-content[data-tasks]'); if(!data) return;
        var tasks=document.getElementById('todaySpeechTaskCount'),meetings=document.getElementById('todaySpeechMeetCount');
        if(tasks) tasks.textContent=(Number(data.getAttribute('data-tasks'))||0)+' công việc cần làm';
        if(meetings) meetings.textContent=(Number(data.getAttribute('data-meetings'))||0)+' cuộc họp';
    }
    function show() {
        var el=modal();if(!el)return;
        if(window.bootstrap && window.bootstrap.Modal) window.bootstrap.Modal.getOrCreateInstance(el).show();
        else if(window.jQuery && window.jQuery.fn.modal) window.jQuery(el).modal('show');
        syncData(); var bot=companion();if(bot){placeCompanionAboveBackdrop();bot.classList.add('is-visible');requestAnimationFrame(positionBot);}
    }
    function hide() {
        var el=modal(); if(!el)return;
        if(window.bootstrap && window.bootstrap.Modal) window.bootstrap.Modal.getOrCreateInstance(el).hide();
        else if(window.jQuery && window.jQuery.fn.modal) window.jQuery(el).modal('hide');
        var bot=companion();if(bot)bot.classList.remove('is-visible');
    }
    function toggle() {
        if(isVisible()){ hide();return; }
        var refresh=document.getElementById('<%= TodayRefreshClientId %>');
            if (refresh) { refresh.click(); return; }
            show();
        }
        window.TodayScheduleUI = { toggle: toggle, show: show, hide: hide, isVisible: isVisible };
        document.addEventListener('keydown', function (event) {
            if (String(event.key || '').toLowerCase() !== 'q' || event.altKey || event.ctrlKey || event.metaKey || event.repeat || event.isComposing) return;
            var t = event.target; if (t && (t.isContentEditable || (t.closest && t.closest('input,textarea,select,[contenteditable],[role="textbox"],.CodeMirror,.monaco-editor')))) return;
            if (document.querySelector('.modal.show:not([id$="mdlTodaySchedule"]),.modal.in:not([id$="mdlTodaySchedule"])')) return;
            event.preventDefault(); event.stopPropagation(); toggle();
        });
        document.addEventListener('click', function (event) { var btn = event.target && event.target.closest && event.target.closest('[data-open-today-schedule]'); if (!btn) return; event.preventDefault(); event.stopPropagation(); toggle(); });
        if (window.jQuery) { window.jQuery(document).on('shown.bs.modal', function (e) { if (e.target === modal()) { syncData(); var bot = companion(); if (bot) { placeCompanionAboveBackdrop(); bot.classList.add('is-visible'); positionBot(); } } }); window.jQuery(document).on('hidden.bs.modal', function (e) { if (e.target === modal()) { var bot = companion(); if (bot) bot.classList.remove('is-visible'); } }); }
        document.addEventListener('shown.bs.modal', function (e) { if (e.target === modal()) { syncData(); var bot = companion(); if (bot) { placeCompanionAboveBackdrop(); bot.classList.add('is-visible'); positionBot(); } } });
        document.addEventListener('hidden.bs.modal', function (e) { if (e.target === modal()) { var bot = companion(); if (bot) bot.classList.remove('is-visible'); } });
        window.addEventListener('resize', positionBot);
        if (window.Sys && Sys.Application) Sys.Application.add_load(syncData);
    })();
</script>
