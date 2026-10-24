<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlChiTietCongCuocHop.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.Controls.CtrlChiTietCongCuocHop" %>
<%@ Register Assembly="SweetSoft.QLDA.Controls" Namespace="SweetSoft.QLDA.Controls" TagPrefix="SweetSoft" %>
<style>
.pmc-layout { display:grid;grid-template-columns:minmax(340px,35%) minmax(0,1fr);gap:14px;height:min(650px,calc(100dvh - 274px));min-height:300px;min-width:0; }
.pmc-pane { display:flex;flex-direction:column;min-height:0;min-width:0;height:100%;background:#fff;border:1px solid #e8dff1;border-radius:14px;overflow:hidden; }
.pmc-toolbar { position:relative;flex:0 0 auto;padding:14px 14px 10px;border-bottom:1px solid #eae2f3;background:#faf8fd; }
.pmc-head-row { display:flex;align-items:flex-start;justify-content:space-between;gap:10px;margin-bottom:12px; }
.pmc-head-left { display:flex;align-items:center;gap:10px;min-width:0; }
.pmc-title { display:inline-flex;align-items:center;gap:8px;color:#54218d;font-size:13.5px;font-weight:800;min-width:0; }
.pmc-title i { color:#6b33a3; }
.pmc-total { display:inline-flex;align-items:center;justify-content:center;min-width:30px;padding:4px 10px;border:1px solid #dcc9ef;border-radius:12px;background:#fff;color:#672c9d;font-size:11px;font-weight:800;white-space:nowrap; }
.pmc-range-launch { display:inline-flex;align-items:center;justify-content:center;gap:6px;flex:0 0 auto;min-height:34px;padding:7px 12px;border:1px solid #d9c6ed;border-radius:10px;background:#fff;color:#68329b;font-size:11.5px;font-weight:750;white-space:nowrap;cursor:pointer; }
.pmc-range-launch.is-active { border-color:#ae7eda;background:#f3eaff; }
.pmc-range-launch:hover { background:#eee3fb; }
.pmc-search-row { display:grid;grid-template-columns:minmax(0,1fr) 190px;gap:10px; }
.pmc-search { position:relative;min-width:0; }
.pmc-search i { position:absolute;left:12px;top:50%;transform:translateY(-50%);color:#8f76a5;font-size:13px;pointer-events:none; }
.pmc-search input { width:100%;height:40px;border:1px solid #ded1ed;border-radius:10px;padding:7px 10px 7px 36px;background:#fff;color:#40304f;font-size:12px;outline:none; }
.pmc-search input:focus,.pmc-filter:focus { border-color:#b68bdc;box-shadow:0 0 0 2px #f0e5fc; }
.pmc-filter { width:100%;height:40px;border:1px solid #ded1ed;border-radius:10px;padding:6px 10px;background:#fff;color:#5e3883;font-size:12px;font-weight:700; }
.pmc-list { flex:1 1 auto;min-height:0;overflow-y:auto;overscroll-behavior:contain;scrollbar-width:thin; }
.pmc-meeting { display:block;width:100%;padding:14px 14px;text-align:left;background:#fff;border:0;border-left:3px solid transparent;border-bottom:1px solid #eee8f4;color:#45374d;cursor:pointer;transition:background .14s,border-color .14s; }
.pmc-meeting:hover { background:#fbf8ff; }
.pmc-meeting.is-selected { background:#f4edfd;border-left-color:#61279b; }
.pmc-meeting-top { display:flex;align-items:flex-start;justify-content:space-between;gap:8px; }
.pmc-meeting-name { font-size:12.5px;font-weight:800;min-width:0;overflow-wrap:anywhere; }
.pmc-meeting-tags { display:flex;align-items:center;gap:6px;flex-wrap:wrap;justify-content:flex-end; }
.pmc-host-chip { display:inline-grid;place-items:center;width:24px;height:24px;border-radius:8px;border:1px solid #efd08a;background:#fff7df;color:#cf9811;font-size:12px; }
.pmc-status { flex:0 0 auto;font-size:10px;font-weight:750;padding:4px 8px;border-radius:8px;background:#edf0f6;color:#66748c;white-space:nowrap;border:1px solid #d8deea; }
.pmc-status-1 { background:#f0e8fb;color:#63318d;border-color:#decdf4; }
.pmc-status-2 { background:#e6f8ed;color:#137e45;border-color:#c2f0d1; }
.pmc-meeting-meta { display:flex;flex-direction:column;gap:5px;margin-top:10px;color:#8c7d98;font-size:11px; }
.pmc-meeting-meta i { width:13px;color:#8662ac; }
.pmc-page { flex:0 0 auto;display:flex;align-items:center;justify-content:center;gap:7px;padding:10px;border-top:1px solid #ebe3f2;background:#faf9fd; }
.pmc-page button { display:inline-grid;place-items:center;width:34px;height:34px;background:#fff;border:1px solid #ded1ef;border-radius:9px;color:#682d9b;font-size:14px;font-weight:800;cursor:pointer; }
.pmc-page button[disabled] { opacity:.35;cursor:default; }
.pmc-page span { font-size:11.5px;color:#7a648f;font-weight:700;white-space:nowrap; }
.pmc-right-head { flex:0 0 auto;display:flex;align-items:flex-start;justify-content:space-between;gap:12px;padding:13px 14px 12px;background:#faf7fe;border-bottom:1px solid #e9ddf2; }
.pmc-right-head h5 { margin:0;color:#4f2589;font-size:17px;font-weight:800;line-height:1.35;overflow-wrap:anywhere; }
.pmc-right-head p { margin:5px 0 0;color:#897b99;font-size:11px; }
.pmc-duration { flex-shrink:0;white-space:nowrap;padding:8px 10px;border:1px solid #e1d1f3;border-radius:10px;background:#fff;color:#623296;font-size:11px;font-weight:800; }
.pmc-roster { flex:1 1 auto;min-height:0;overflow:auto;overscroll-behavior:contain;scrollbar-width:thin; }
.pmc-table { width:100%;min-width:780px;border-collapse:separate;border-spacing:0; }
.pmc-table th { position:sticky;top:0;z-index:2;padding:12px 10px;background:#f5effa;border-bottom:1px solid #e7ddf1;color:#60348a;font-weight:800;font-size:11.5px;text-align:left;white-space:nowrap; }
.pmc-table td { padding:10px;border-bottom:1px solid #f0eaf4;color:#544365;font-size:12px;vertical-align:middle; }
.pmc-table tr:last-child td { border-bottom:none; }
.pmc-staff { display:flex;align-items:center;gap:10px;min-width:0; }
.pmc-avatar { position:relative;display:inline-flex;align-items:center;justify-content:center;width:40px;height:40px;flex:0 0 40px;border-radius:999px;overflow:hidden;background:#7945c1;color:#fff;font-weight:800;font-size:12px; }
.pmc-avatar img { width:100%;height:100%;object-fit:cover;display:block; }
.pmc-avatar-fallback { width:100%;height:100%;display:flex;align-items:center;justify-content:center;color:#fff;font-weight:800;font-size:12px; }
.pmc-staff-name { display:flex;flex-direction:column;min-width:0;gap:3px; }
.pmc-staff-name b { font-size:12.5px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis; }
.pmc-staff-name small { color:#8890a0;font-size:10.8px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis; }
.pmc-host { color:#d7a22c;margin-left:5px; }
.pmc-yes,.pmc-no { display:inline-flex;align-items:center;padding:6px 9px;border-radius:8px;font-size:10.5px;font-weight:800;white-space:nowrap; }
.pmc-yes { background:#e7f8ee;color:#147f46; }
.pmc-no { background:#f1f1f5;color:#7c7e89; }
.pmc-hour { font-weight:750;color:#5b2499;white-space:nowrap; }
.pmc-empty { padding:20px;text-align:center;color:#8d8397;font-size:12px; }
.pmc-range-pop { position:fixed;z-index:2147480001;width:396px;max-width:calc(100vw - 20px);border:1px solid #dcccec;border-radius:14px;background:#fff;color:#40334d;box-shadow:0 18px 42px rgba(45,19,80,.18);padding:14px; }
.pmc-range-pop[hidden] { display:none!important; }
.pmc-range-head { display:flex;align-items:center;justify-content:space-between;gap:10px;margin-bottom:10px;font-size:13.5px;font-weight:800;color:#51218a; }
.pmc-range-close { border:0;background:transparent;color:#947baa;font-size:18px;line-height:1;cursor:pointer; }
.pmc-range-fields { display:grid;grid-template-columns:1fr 1fr;gap:8px;margin-bottom:12px; }
.pmc-range-field { min-width:0;display:flex;flex-direction:column;align-items:flex-start;gap:4px;border:1px solid #e3d8ef;background:#fbf9fe;border-radius:10px;padding:10px 12px;text-align:left;cursor:pointer; }
.pmc-range-field.is-active { border-color:#ad79e1;background:#f3eafa;box-shadow:inset 0 0 0 1px #d5b6f6; }
.pmc-range-field small { color:#8d76a2;font-size:10.5px; }
.pmc-range-field strong { color:#51218a;font-size:12px; }
.pmc-range-month { display:flex;align-items:center;justify-content:space-between;gap:8px;margin-bottom:8px;color:#51218a;font-size:12.5px;font-weight:800; }
.pmc-range-month button { width:34px;height:34px;border:1px solid #e3d5ef;border-radius:9px;background:#fff;color:#6d399a;cursor:pointer; }
.pmc-range-days { display:grid;grid-template-columns:repeat(7,minmax(0,1fr));gap:4px; }
.pmc-range-weekday { text-align:center;padding:4px 0;color:#9781ab;font-size:10.2px;font-weight:750; }
.pmc-range-day { display:grid;place-items:center;aspect-ratio:1/1;min-width:0;border:1px solid transparent;border-radius:9px;background:#fff;color:#54415f;font-size:11.5px;cursor:pointer; }
.pmc-range-day:hover:not(:disabled) { background:#efe4fb;border-color:#d8c4eb; }
.pmc-range-day.is-out { opacity:.36; }
.pmc-range-day.is-between { background:#f2eafa; }
.pmc-range-day.is-picked { background:#6725a0;color:#fff;font-weight:800; }
.pmc-range-day:disabled { color:#c1bac7;cursor:not-allowed; }
.pmc-range-error { min-height:15px;margin:6px 0 0;color:#ba4242;font-size:11px; }
.pmc-range-foot { display:flex;justify-content:space-between;gap:8px;margin-top:10px; }
.pmc-range-foot button { flex:1;padding:10px 12px;border:1px solid #dbc7eb;border-radius:9px;background:#fff;color:#653396;font-size:11.5px;font-weight:800;cursor:pointer; }
.pmc-range-foot .pmc-range-apply { border-color:#54218d;background:#54218d;color:#fff; }
.pmc-popup-modal { overflow-y:hidden!important;padding-right:0!important; }
.pmc-popup-modal .modal-dialog { width:min(1480px,calc(100vw - 36px))!important;max-width:calc(100vw - 36px)!important;margin:24px auto!important;max-height:calc(100dvh - 48px)!important;overflow:visible!important; }
.pmc-popup-modal .modal-content { display:flex!important;flex-direction:column!important;max-height:calc(100dvh - 48px)!important;min-height:0;overflow:visible!important; }
.pmc-popup-modal .modal-header,.pmc-popup-modal .modal-footer { flex:0 0 auto;overflow:visible!important; }
.pmc-popup-modal .modal-body { flex:1 1 auto;min-height:0;overflow:hidden!important;padding:13px!important; }
.pmc-popup-modal .pd-detail-modal-wrap { min-width:0;min-height:0; }
.pmc-popup-modal .pd-detail-modal-heading { margin-bottom:10px; }
body:has(.pmc-popup-modal.show) { overflow:hidden!important; }
@media(max-width:1080px) { .pmc-layout { grid-template-columns:1fr;grid-template-rows:minmax(260px,44%) minmax(0,1fr);height:calc(100dvh - 275px);gap:10px; } }
@media(max-width:800px) { .pmc-search-row { grid-template-columns:1fr; } .pmc-head-row { flex-wrap:wrap; } .pmc-right-head { flex-wrap:wrap; } }
@media(max-width:540px) { .pmc-popup-modal .modal-dialog { width:calc(100vw - 16px)!important;max-width:calc(100vw - 16px)!important;margin:16px auto!important;max-height:calc(100dvh - 32px)!important; } .pmc-popup-modal .modal-content { max-height:calc(100dvh - 32px)!important; } .pmc-popup-modal .modal-body { padding:7px!important; } .pmc-layout { height:calc(100dvh - 265px);min-height:240px; } .pmc-range-pop { width:calc(100vw - 20px); } }
</style>
<SweetSoft:ExtraModal ID="mdlMeetingDetail" runat="server" Type="Primary" Title="Chi tiết cuộc họp" Width="92%">
    <ContentTemplate>
        <div class="pd-detail-modal-wrap" id="pmcPopupRoot">
            <div class="pd-detail-modal-heading"><h4 class="pd-detail-modal-title" id="pmcPopupTitle">Chi tiết cuộc họp</h4><p class="pd-detail-modal-sub" id="pmcPopupSub">Danh sách cuộc họp và giờ công của nhân viên tham gia.</p></div>
            <asp:UpdatePanel ID="upMeeting" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <asp:HiddenField ID="hfAction" runat="server" /><asp:HiddenField ID="hfKind" runat="server" /><asp:HiddenField ID="hfMember" runat="server" />
                    <asp:HiddenField ID="hfSearch" runat="server" /><asp:HiddenField ID="hfState" runat="server" /><asp:HiddenField ID="hfDateFrom" runat="server" /><asp:HiddenField ID="hfDateTo" runat="server" />
                    <asp:HiddenField ID="hfMeetingPage" runat="server" /><asp:HiddenField ID="hfRosterPage" runat="server" /><asp:HiddenField ID="hfMeetingId" runat="server" />
                    <asp:LinkButton ID="btnMeetingAction" runat="server" OnClick="btnMeetingAction_Click" CausesValidation="false" Style="display:none" />
                    <div class="pmc-layout" id="pmcLayout">
                        <section class="pmc-pane" aria-label="Danh sách cuộc họp">
                            <div class="pmc-toolbar">
                                <div class="pmc-head-row">
                                    <div class="pmc-head-left">
                                        <span class="pmc-title"><i class="far fa-calendar-alt"></i> Danh sách cuộc họp</span>
                                        <span class="pmc-total"><asp:Literal ID="litCount" runat="server" /></span>
                                    </div>
                                    <button type="button" class="pmc-range-launch" id="pmcRangeOpen" aria-haspopup="dialog" aria-label="Lọc cuộc họp theo khoảng ngày"><i class="far fa-calendar-alt"></i><span id="pmcRangeOpenLabel">Chọn khoảng ngày</span></button>
                                </div>
                                <div class="pmc-search-row">
                                    <div class="pmc-search"><i class="fas fa-search"></i><input type="search" id="pmcSearchInput" placeholder="Tìm tên cuộc họp..." autocomplete="off" /></div>
                                    <select class="pmc-filter" id="pmcStateFilter" aria-label="Lọc trạng thái"><option value="all">Tất cả</option><option value="active">Chưa kết thúc</option><option value="ended">Đã kết thúc</option></select>
                                </div>
                            </div>
                            <div class="pmc-list" id="pmcList"><asp:Literal ID="litMeetings" runat="server" EnableViewState="false" /></div>
                            <div class="pmc-page" id="pmcMeetingPager"><asp:Literal ID="litMeetingPager" runat="server" EnableViewState="false" /></div>
                        </section>
                        <section class="pmc-pane" aria-label="Nhân viên tham gia và giờ công">
                            <asp:Literal ID="litRosterHeader" runat="server" EnableViewState="false" />
                            <div class="pmc-roster" id="pmcRoster"><asp:Literal ID="litRoster" runat="server" EnableViewState="false" /></div>
                            <div class="pmc-page" id="pmcRosterPager"><asp:Literal ID="litRosterPager" runat="server" EnableViewState="false" /></div>
                        </section>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </ContentTemplate>
</SweetSoft:ExtraModal>
<script type="text/javascript">
    (function () {
        var modalId ='<%= ModalClientId %>';
    var selector={action:'hfAction',kind:'hfKind',member:'hfMember',search:'hfSearch',state:'hfState',from:'hfDateFrom',to:'hfDateTo',page:'hfMeetingPage',rosterPage:'hfRosterPage',meeting:'hfMeetingId'};
    var busy=false, debounceHandle=null, openRequest=false;
    var rangePopup=null, rangeMonth=null, rangeTarget='from', draftFrom='', draftTo='';
    function modal(){var n=document.getElementById(modalId);return n&&n.classList&&n.classList.contains('modal')?n:(n?n.querySelector('.modal'):null);}    
    function root(){return document.getElementById('pmcPopupRoot');}
    function el(name){var m=modal();return m?m.querySelector('input[id$="'+selector[name]+'"]'):null;}
    function val(name,v){var e=el(name);if(e)e.value=v==null?'':String(v);}    
    function get(name){var e=el(name);return e?e.value:'';}    
    function submit(action){if(busy)return false;val('action',action);var btn=document.getElementById('<%= ActionButtonId %>'); if (!btn) return false; busy = true; btn.click(); return false; }
        function isoDate(value) { if (!value || !/^(\d{4})-(\d{2})-(\d{2})$/.test(value)) return null; var p = value.split('-'); return new Date(Number(p[0]), Number(p[1]) - 1, Number(p[2])); }
        function dateISO(date) { var y = date.getFullYear(), m = ('0' + (date.getMonth() + 1)).slice(-2), d = ('0' + date.getDate()).slice(-2); return y + '-' + m + '-' + d; }
        function textDate(value) { if (!value) return 'Chọn ngày'; var p = value.split('-'); if (p.length !== 3) return value; return p[2] + '/' + p[1] + '/' + p[0]; }
        function buttonLabel() { var from = get('from'), to = get('to'); if (from && to) return textDate(from) + ' - ' + textDate(to); if (from) return 'Từ ' + textDate(from); if (to) return 'Đến ' + textDate(to); return 'Chọn khoảng ngày'; }
        function syncFilters() {
            var input = document.getElementById('pmcSearchInput'), select = document.getElementById('pmcStateFilter');
            if (input) input.value = get('search');
            if (select) select.value = get('state') || 'all';
            var label = document.getElementById('pmcRangeOpenLabel'); if (label) label.textContent = buttonLabel();
            var toggle = document.getElementById('pmcRangeOpen'); if (toggle) toggle.classList.toggle('is-active', !!(get('from') || get('to')));
        }
        function requestFilters() {
            var input = document.getElementById('pmcSearchInput'), select = document.getElementById('pmcStateFilter');
            val('search', input ? input.value : '');
            val('state', select ? select.value : 'all');
            val('page', '1');
            val('rosterPage', '1');
            return submit('filter');
        }
        function ensureCalendar() {
            if (rangePopup && document.body.contains(rangePopup)) return rangePopup;
            rangePopup = document.createElement('div');
            rangePopup.className = 'pmc-range-pop';
            rangePopup.hidden = true;
            rangePopup.setAttribute('role', 'dialog');
            rangePopup.setAttribute('aria-label', 'Lọc cuộc họp theo khoảng ngày');
            rangePopup.innerHTML = '<div class="pmc-range-head"><span><i class="far fa-calendar-alt"></i> Khoảng thời gian cuộc họp</span><button type="button" class="pmc-range-close" aria-label="Đóng lịch">×</button></div>' +
                '<div class="pmc-range-fields"><button type="button" class="pmc-range-field" data-target="from"><small>Từ ngày</small><strong>Chọn ngày</strong></button><button type="button" class="pmc-range-field" data-target="to"><small>Đến ngày</small><strong>Chọn ngày</strong></button></div>' +
                '<div class="pmc-range-month"><button type="button" data-month="-1" aria-label="Tháng trước">‹</button><span id="pmcDateMonth"></span><button type="button" data-month="1" aria-label="Tháng sau">›</button></div>' +
                '<div class="pmc-range-days" id="pmcDateDays"></div><div class="pmc-range-error" aria-live="polite"></div>' +
                '<div class="pmc-range-foot"><button type="button" data-range-clear>Xóa lọc</button><button type="button" class="pmc-range-apply" data-range-apply>Áp dụng</button></div>';
            document.body.appendChild(rangePopup);
            rangePopup.addEventListener('click', function (e) {
                // Vẽ lại các ô ngày sẽ loại bỏ target cũ trước khi click nổi lên document.
                // Chặn bubbling để không bị nhận nhầm là click bên ngoài popup.
                e.stopPropagation();
                // Dùng closest('button') để bấm cả chữ Từ ngày / Đến ngày bên trong button.
                var t = e.target.closest && e.target.closest('button');
                if (!t || !rangePopup.contains(t)) return;
                if (t.classList.contains('pmc-range-close')) { hideCalendar(); return; }
                if (t.hasAttribute('data-target')) { rangeTarget = t.getAttribute('data-target'); paintCalendar(); return; }
                if (t.hasAttribute('data-month')) { rangeMonth.setMonth(rangeMonth.getMonth() + Number(t.getAttribute('data-month'))); paintCalendar(); return; }
                if (t.hasAttribute('data-date')) {
                    var valDate = t.getAttribute('data-date');
                    if (rangeTarget === 'from') { draftFrom = valDate; if (draftTo && draftTo < valDate) draftTo = ''; }
                    else { draftTo = valDate; }
                    paintCalendar();
                    return;
                }
                if (t.hasAttribute('data-range-clear')) { hideCalendar(); val('from', ''); val('to', ''); requestFilters(); return; }
                if (t.hasAttribute('data-range-apply')) {
                    var err = rangePopup.querySelector('.pmc-range-error');
                    if (draftFrom && draftTo && draftTo < draftFrom) { if (err) err.textContent = 'Đến ngày không được trước Từ ngày.'; return; }
                    hideCalendar();
                    val('from', draftFrom); val('to', draftTo); val('page', '1'); val('rosterPage', '1');
                    requestFilters();
                    return;
                }
            });
            return rangePopup;
        }
        function positionCalendar() {
            if (!rangePopup || rangePopup.hidden) return;
            var toggle = document.getElementById('pmcRangeOpen'); if (!toggle) return;
            var rect = toggle.getBoundingClientRect();
            var w = rangePopup.offsetWidth, h = rangePopup.offsetHeight, gap = 10, pad = 10;
            var left = rect.right - w, top = rect.bottom + gap;
            if (left < pad) left = pad;
            if (left + w > window.innerWidth - pad) left = window.innerWidth - w - pad;
            if (top + h > window.innerHeight - pad) top = Math.max(pad, rect.top - h - gap);
            rangePopup.style.left = Math.round(left) + 'px';
            rangePopup.style.top = Math.round(top) + 'px';
        }
        function hideCalendar() { if (rangePopup) rangePopup.hidden = true; var toggle = document.getElementById('pmcRangeOpen'); if (toggle) toggle.classList.remove('is-open'); }
        function openCalendar() {
            draftFrom = get('from'); draftTo = get('to'); rangeTarget = 'from';
            var d = isoDate(draftFrom) || isoDate(draftTo) || new Date(); rangeMonth = new Date(d.getFullYear(), d.getMonth(), 1);
            var p = ensureCalendar(); p.hidden = false; paintCalendar(); var toggle = document.getElementById('pmcRangeOpen'); if (toggle) toggle.classList.add('is-open');
        }
        function paintCalendar() {
            var p = ensureCalendar(); if (!rangeMonth) return;
            Array.prototype.forEach.call(p.querySelectorAll('.pmc-range-field'), function (field) {
                field.classList.toggle('is-active', field.getAttribute('data-target') === rangeTarget);
                var target = field.getAttribute('data-target');
                var value = target === 'from' ? draftFrom : draftTo;
                field.querySelector('strong').textContent = textDate(value);
            });
            p.querySelector('#pmcDateMonth').textContent = 'Tháng ' + (rangeMonth.getMonth() + 1) + '/' + rangeMonth.getFullYear();
            var days = p.querySelector('#pmcDateDays'); days.innerHTML = '';
            ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'].forEach(function (x) { var a = document.createElement('span'); a.className = 'pmc-range-weekday'; a.textContent = x; days.appendChild(a); });
            var offset = (rangeMonth.getDay() + 6) % 7;
            for (var i = 0; i < 42; i++) {
                var d = new Date(rangeMonth.getFullYear(), rangeMonth.getMonth(), 1 - offset + i), valDate = dateISO(d);
                var btn = document.createElement('button'); btn.type = 'button'; btn.className = 'pmc-range-day'; btn.textContent = d.getDate(); btn.setAttribute('data-date', valDate);
                if (d.getMonth() !== rangeMonth.getMonth()) btn.classList.add('is-out');
                if (valDate === draftFrom || valDate === draftTo) btn.classList.add('is-picked');
                if (draftFrom && draftTo && valDate > draftFrom && valDate < draftTo) btn.classList.add('is-between');
                if (rangeTarget === 'to' && draftFrom && valDate < draftFrom) { btn.disabled = true; btn.title = 'Đến ngày không được trước Từ ngày'; }
                days.appendChild(btn);
            }
            p.querySelector('.pmc-range-error').textContent = '';
            positionCalendar();
        }
        function connect() {
            var r = root(); if (!r) return; syncFilters();
            var search = document.getElementById('pmcSearchInput'), state = document.getElementById('pmcStateFilter'), toggle = document.getElementById('pmcRangeOpen');
            if (search && !search.__pmcBound) { search.__pmcBound = true; search.addEventListener('input', function () { if (debounceHandle) clearTimeout(debounceHandle); debounceHandle = setTimeout(requestFilters, 350); }); }
            if (state && !state.__pmcBound) { state.__pmcBound = true; state.addEventListener('change', requestFilters); }
            if (toggle && !toggle.__pmcBound) { toggle.__pmcBound = true; toggle.addEventListener('click', function () { if (rangePopup && !rangePopup.hidden) hideCalendar(); else openCalendar(); }); }
            if (!r.__pmcClickBound) {
                r.__pmcClickBound = true;
                r.addEventListener('click', function (event) {
                    var b = event.target.closest ? event.target.closest('[data-pmc-action]') : null; if (!b) return;
                    event.preventDefault();
                    var act = b.getAttribute('data-pmc-action');
                    if (act === 'select') { val('meeting', b.getAttribute('data-pmc-id')); val('rosterPage', '1'); submit('select'); return; }
                    if (act === 'meeting-page') { val('page', b.getAttribute('data-pmc-page')); val('rosterPage', '1'); submit('page'); return; }
                    if (act === 'roster-page') { val('rosterPage', b.getAttribute('data-pmc-page')); submit('roster-page'); return; }
                });
            }
        }
        function show() { var n = modal(); if (!n) return; n.classList.add('pmc-popup-modal'); if (window.bootstrap && window.bootstrap.Modal) { var m = window.bootstrap.Modal.getInstance ? window.bootstrap.Modal.getInstance(n) : null; (m || new window.bootstrap.Modal(n)).show(); } else if (window.jQuery && window.jQuery.fn.modal) { window.jQuery(n).modal('show'); } }
        function open(button) {
            if (busy || !button) return false;
            var type = button.getAttribute('data-pd-detail-source');
            if (type !== 'pdPersonalMeetingDetail' && type !== 'pdTeamDetail-meeting') return false;
            var member = '';
            if (type === 'pdTeamDetail-meeting') {
                var h = document.querySelector('#projectTeamHoursView input[type="hidden"][id$="hfTeamHoursMember"]');
                member = h ? h.value : ''; if (!member) return false;
            }
            var title = document.getElementById('pmcPopupTitle'), sub = document.getElementById('pmcPopupSub');
            if (title) title.textContent = button.getAttribute('data-pd-detail-title') || 'Chi tiết cuộc họp';
            if (sub) sub.textContent = button.getAttribute('data-pd-detail-sub') || 'Danh sách cuộc họp và giờ công của nhân viên tham gia.';
            val('kind', type); val('member', member); val('search', ''); val('state', type === 'pdTeamDetail-meeting' ? 'ended' : 'all'); val('from', ''); val('to', ''); val('page', '1'); val('rosterPage', '1'); val('meeting', '');
            openRequest = true; hideCalendar(); return submit('open');
        }
        function received(isOpen) { busy = false; connect(); syncFilters(); if (isOpen || openRequest) { openRequest = false; show(); var n = modal(), body = n ? n.querySelector('.modal-body') : null; if (body) body.scrollTop = 0; } }
        function failed() { busy = false; openRequest = false; }
        if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager && !window.__pmcEndRequestBound) {
            window.__pmcEndRequestBound = true;
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) { if (busy && args.get_error()) failed(); connect(); syncFilters(); });
        }
        document.addEventListener('click', function (e) {
            var toggle = document.getElementById('pmcRangeOpen');
            if (rangePopup && !rangePopup.hidden && !rangePopup.contains(e.target) && (!toggle || !toggle.contains(e.target))) hideCalendar();
        });
        window.addEventListener('resize', function () { if (rangePopup && !rangePopup.hidden) positionCalendar(); });
        window.addEventListener('scroll', function () { if (rangePopup && !rangePopup.hidden) positionCalendar(); }, true);
        document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && rangePopup && !rangePopup.hidden) hideCalendar(); });
        window.QLDAProjectMeetingPopup = { open: open, received: received, failed: failed };
    })();
</script>
