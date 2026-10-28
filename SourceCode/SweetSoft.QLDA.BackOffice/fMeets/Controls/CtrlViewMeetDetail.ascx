<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlViewMeetDetail.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fMeets.Controls.CtrlViewMeetDetail" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<style type="text/css">
    #<%= mdlMeetView.ClientID %> .modal-dialog { width: calc(100% - 30px) !important; max-width: 1360px !important; }
    #<%= mdlMeetView.ClientID %> .modal-body { padding: 16px 18px 12px; max-height: calc(100dvh - 126px); overflow-y: auto; scrollbar-width: thin; }
    body.modal-open:has(#<%= mdlMeetView.ClientID %>.show), body.modal-open:has(#<%= mdlMeetView.ClientID %>.in) { overflow-y: hidden !important; }
    .meet-view { color: #334155; }
    .meet-view-layout { display: grid; grid-template-columns: minmax(590px, 1.23fr) minmax(0, 1.27fr); gap: 15px; align-items: stretch; }
    .meet-view-left { min-width: 0; display: flex; flex-direction: column; gap: 16px; }
    .meet-view-right { display: flex; min-width: 0; }
    .meet-view-card { position: relative; min-width: 0; padding: 22px 14px 13px; border: 1px solid #cbd5e1; border-radius: 11px; background: #fff; box-shadow: 0 1px 2px rgba(15, 23, 42, .02); box-sizing: border-box; }
    .meet-view .fieldlegend { position: absolute; top: -15px; left: 18px; display: inline-flex; align-items: center; gap: 8px; margin: 0; padding: 0 10px; background: #fff; color: #334155; font-size: 13px; font-weight: 800; line-height: 1.2; letter-spacing: .04em; text-transform: uppercase; z-index: 2; }
    .meet-view .fieldlegend i { width: 26px; height: 26px; display: inline-flex; align-items: center; justify-content: center; flex: 0 0 26px; border: 1px solid #cbd5e1; border-radius: 6px; background: #f7faff; color: #475569; font-size: 12px; }
    .meet-view-summary-card { display: flex; flex-direction: column; min-height: 0; padding: 23px 16px 14px; }
    .meet-view-summary-main { display: flex; align-items: flex-start; justify-content: space-between; gap: 8px; }
    .meet-view-summary-info { min-width: 0; flex: 1 1 auto; }
    .meet-view-title { color: #4c1d95; font-size: 16px; font-weight: 750; line-height: 1.35; word-break: break-word; }
    .meet-view-summary-meta { display: flex; flex-wrap: wrap; align-items: center; gap: 7px 12px; min-width: 0; margin-top: 8px; }
    .meet-view-location { display: inline-flex; align-items: center; gap: 6px; min-width: 0; color: #475569; font-size: 12px; line-height: 1.4; }
    .meet-view-meta-divider { width: 1px; height: 15px; flex: 0 0 1px; background: #e0e6ef; }
    .meet-view-time-pair { display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:8px;margin-top:9px; }
    .meet-view-time-block { display:flex;flex-direction:column;gap:4px;min-width:0;padding:8px 10px;border:1px solid #e0e7f1;border-radius:9px;background:#fafbff; }
    .meet-view-time-block.is-actual { border-color:#e2d5f4;background:#fbf8ff; }
    .meet-view-time-caption { display:flex;align-items:center;gap:6px;color:#718096;font-size:10px;font-weight:800;text-transform:uppercase;letter-spacing:.02em; }
    .meet-view-time-block.is-actual .meet-view-time-caption { color:#7246a5; }
    .meet-view-time-block .meet-view-time-inline { font-size:12px;min-width:0; }
    .meet-view-time-missing { color:#94a3b8;font-size:12px;font-weight:650; }
    .meet-view-labor-footnote { justify-content:space-between; }
    .meet-view-labor-note { display:inline-flex;align-items:center;gap:5px;min-width:0; }
    .meet-view-labor-detail-btn { display:inline-flex;align-items:center;justify-content:center;gap:6px;flex:0 0 auto;padding:6px 10px;border:1px solid #d4c2ea;border-radius:7px;background:#f7f2ff;color:#54218d;font-size:11px;font-weight:800;white-space:nowrap;cursor:pointer; }
    .meet-view-labor-detail-btn:hover { background:#ece2fc;border-color:#bfa1e0; }

    .meet-view-time-inline { display: inline-flex; flex-wrap: wrap; align-items: center; gap: 7px; min-width: 0; color: #334155; font-size: 12.5px; font-weight: 750; line-height: 1.4; font-variant-numeric: tabular-nums; }
    .meet-view-time-inline i { color: #64748b; font-size: 13px; }
    .meet-view-time-dot { color: #64748b; padding: 0 2px; font-weight: 700; }
    .meet-view-time-dash { color: #64748b; padding: 0 2px; }

    .meet-view-location i { color: #64748b; font-size: 12px; }
    .meet-view-status { display: inline-flex; align-items: center; gap: 6px; flex: 0 0 auto; margin-top: 1px; padding: 5px 8px; border: 1px solid transparent; border-radius: 999px; font-size: 10px; font-weight: 750; line-height: 1.2; white-space: nowrap; }
    .meet-view-status::before { content: ""; width: 7px; height: 7px; flex: 0 0 7px; border-radius: 50%; background: currentColor; }
    .meet-view-status-0 { color: #2563eb; background: #eff6ff; border-color: #bfdbfe; }
    .meet-view-status-1 { color: #d97706; background: #fffbeb; border-color: #fde68a; }
    .meet-view-status-2 { color: #16a34a; background: #f0fdf4; border-color: #bbf7d0; }
    .meet-view-status-3 { color: #475569; background: #f8fafc; border-color: #cbd5e1; }
    .meet-view-content-card { display: flex; flex: 1 1 auto; flex-direction: column; min-height: 0; }
    .meet-view-content-body { flex: 1 1 auto; min-height: 0; max-height: calc(100dvh - 235px); color: #334155; font-size: 13.5px; line-height: 1.6; overflow-y: auto; overflow-wrap: anywhere; scrollbar-width: thin; }
    .meet-view-empty { color: #64748b; font-size: 13px; font-style: italic; }
    .meet-view-labor-card { display: flex; flex: 0 0 auto; flex-direction: column; min-height: 0; padding: 24px 15px 15px; }
    .meet-view-labor-body { display: flex; flex-direction: column; flex: 1 1 auto; min-height: 0; width: 100%; }
    .meet-view-labor-pending { display: flex; align-items: center; justify-content: center; flex-direction: column; min-height: 170px; gap: 8px; padding: 16px; color: #718096; text-align: center; border: 1px dashed #dce5f0; border-radius: 10px; background: #fafbff; }
    .meet-view-labor-pending i { display: flex; align-items: center; justify-content: center; width: 48px; height: 48px; border-radius: 14px; background: #f1ebff; color: #6635ad; font-size: 20px; }
    .meet-view-labor-pending strong { color: #475569; font-size: 14px; }
    .meet-view-labor-pending span { max-width: 380px; font-size: 12px; line-height: 1.55; }
    .meet-view-labor-root { display: flex; flex-direction: column; min-height: 0; }
    .meet-view-labor-overview { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 8px; margin-bottom: 11px; }
    .meet-view-labor-stat { min-width: 0; padding: 9px 10px; background: #f8faff; border: 1px solid #d7e0ec; border-radius: 8px; }
    .meet-view-labor-stat span { display: block; color: #475569; font-size: 10.5px; font-weight: 650; letter-spacing: .01em; line-height: 1.3; }
    .meet-view-labor-stat strong { display: block; margin-top: 4px; font-size: 14.5px; font-weight: 800; color: #1e293b; line-height: 1.2; white-space: nowrap; }
    .meet-view-labor-stat.is-violet { background: #f7f2ff; border-color: #eaddff; }
    .meet-view-labor-stat.is-violet strong { color: #54218d; }
    .meet-view-labor-stat.is-green { background: #f1fcf6; border-color: #d9f1e1; }
    .meet-view-labor-stat.is-green strong { color: #167349; }
    .meet-view-labor-stat.is-muted strong { color: #94a3b8; }
    .meet-view-labor-scroll { width: 100%; max-height: 360px; min-height: 0; overflow-x: auto; overflow-y: auto; border: 1px solid #cbd5e1; border-radius: 9px; scrollbar-width: thin; }
    .meet-view-labor-table { width: 100%; border-spacing: 0; border-collapse: separate; table-layout: fixed; color: #334155; font-size: 12.5px; }
    .meet-view-labor-table thead th { position: sticky; z-index: 1; top: 0; padding: 12px 8px; border-bottom: 2px solid #d9caef; background: #f1ebfa; color: #492477; font-size: 12.5px; font-weight: 800; text-align: center; white-space: nowrap; letter-spacing: .015em; }
    .meet-view-labor-table tbody td { height: 62px; padding: 10px 9px; border-bottom: 1px solid #e9edf5; vertical-align: middle; text-align: center; }
    .meet-view-labor-table tbody td:first-child { text-align: left; }
    .meet-view-labor-table tbody tr:last-child td { border-bottom: 0; }
    .meet-view-labor-table tbody tr:hover td { background: #faf8ff; }
    .meet-view-labor-person { display: flex; align-items: center; gap: 8px; min-width: 0; }
    .meet-view-participant-avatar { width: 36px; height: 36px; flex: 0 0 36px; display: inline-flex; align-items: center; justify-content: center; border: 1px solid rgba(15,23,42,.05); border-radius: 50%; overflow: hidden; object-fit: cover; font-size: 10px; font-weight: 800; color: #fff; background: #64748b; }
    .meet-view-labor-person .meet-view-participant-avatar { width: 36px; height: 36px; flex: 0 0 36px; }
    .meet-view-labor-person-info { display: flex; flex-direction: column; gap: 2px; min-width: 0; }
    .meet-view-labor-name { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; font-size: 12.6px; font-weight: 750; color: #1f2937; }
    .meet-view-labor-name i { margin-left: 4px; color: #d3a12d; font-size: 11px; }
    .meet-view-labor-email { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; font-size: 11px; color: #64748b; }
    .meet-view-labor-join { display: inline-flex; align-items: center; justify-content: center; max-width: 100%; gap: 5px; border-radius: 7px; border: 1px solid #d9e9de; background: #f1fbf5; padding: 5px 7px; color: #248254; font-size: 11.3px; font-weight: 750; white-space: nowrap; }
    .meet-view-labor-join.no { color: #64748b; background: #f7f8fa; border-color: #e6e9ee; }
    .meet-view-labor-join i { font-size: 9px; }
    .meet-view-labor-hour { white-space: nowrap; font-size: 12.5px; font-weight: 750; color: #334155; }
    .meet-view-labor-hour.actual { color: #54218d; }
    .meet-view-labor-hour.missing { color: #a5aebb; font-weight: 600; font-style: italic; }
    .meet-view-labor-pager { display: flex; align-items: center; justify-content: center; gap: 5px; margin-top: 8px; flex-wrap: wrap; }
    .meet-view-labor-page-btn { min-width: 29px; height: 29px; padding: 0 8px; border: 1px solid #d7e0ee; border-radius: 7px; background: #fff; color: #5b6b81; font-size: 11px; font-weight: 700; cursor: pointer; transition: background-color .16s ease, border-color .16s ease, color .16s ease; }
    .meet-view-labor-page-btn:hover { background: #f7f3ff; border-color: #d5c7ee; color: #54218d; }
    .meet-view-labor-page-btn[disabled] { cursor: default; opacity: .45; background: #f8fafc; color: #9aa8ba; border-color: #e5ebf3; }
    .meet-view-labor-page-btn.is-active { background: #54218d; border-color: #54218d; color: #fff; }
    .meet-view-labor-page-status { min-width: 78px; color: #475569; font-size: 11.5px; font-weight: 700; text-align: center; }
    .meet-view-labor-footnote { display: flex; align-items: center; gap: 5px; margin-top: 10px; color: #64748b; font-size: 11px; line-height: 1.35; }
    .meet-view-labor-footnote i { color: #9473c7; }
    .meet-view-labor-error { padding: 15px; border: 1px solid #f3d6d6; border-radius: 9px; background: #fff9f9; color: #a84e4e; font-size: 12px; line-height: 1.5; }
    .meet-view-labor-empty { padding: 25px 12px; text-align: center; color: #94a3b8; font-size: 12px; }
    @media (max-width: 1240px) { .meet-view-layout { grid-template-columns: 1fr; } .meet-view-content-body { min-height: 160px; } }
    @media (max-width: 900px) { .meet-view-labor-overview { grid-template-columns: repeat(2, minmax(0, 1fr)); } .meet-view-labor-table { min-width: 580px; } }
    @media (max-height: 760px) and (min-width: 1201px) { #<%= mdlMeetView.ClientID %> .modal-body { padding-top: 12px; padding-bottom: 9px; max-height: calc(100dvh - 110px); } .meet-view-summary-card { min-height: 0; } .meet-view-labor-table tbody td { padding-top: 8px; padding-bottom: 8px; height: 54px; } .meet-view-labor-stat { padding-top: 5px; padding-bottom: 5px; } .meet-view-labor-overview { margin-bottom: 7px; } }
    @media (max-width: 767.98px) { .meet-view-time-pair { grid-template-columns:1fr; } .meet-view-labor-footnote { align-items:flex-start;flex-wrap:wrap; } .meet-view-meta-divider { display: none; } #<%= mdlMeetView.ClientID %> .modal-dialog { width: calc(100% - 16px) !important; } #<%= mdlMeetView.ClientID %> .modal-body { padding: 12px 14px 10px; } .meet-view-layout { grid-template-columns: 1fr; } .meet-view-summary-main { flex-direction: column; } .meet-view-labor-overview { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
</style>
<script type="text/javascript">
    window.SweetMeetViewLaborPager = window.SweetMeetViewLaborPager || {
        findRoot: function (element) { while (element && (!element.classList || !element.classList.contains('meet-view-labor-root'))) element = element.parentNode; return element; },
        showPage: function (element, page) { var root = this.findRoot(element); if (!root) return false; var total = parseInt(root.getAttribute('data-total-pages') || '1', 10); if (isNaN(total) || total < 1) total = 1; if (page < 1) page = 1; if (page > total) page = total; root.setAttribute('data-current-page', page.toString()); var rows = root.querySelectorAll('tr[data-labor-page]'); for (var i = 0; i < rows.length; i++) rows[i].style.display = rows[i].getAttribute('data-labor-page') === page.toString() ? '' : 'none'; var buttons = root.querySelectorAll('.meet-view-labor-page-btn[data-page]'); for (var j = 0; j < buttons.length; j++) { var active = buttons[j].getAttribute('data-page') === page.toString(); if (active) buttons[j].classList.add('is-active'); else buttons[j].classList.remove('is-active'); } var prev = root.querySelector('.meet-view-labor-page-btn.prev'); var next = root.querySelector('.meet-view-labor-page-btn.next'); if (prev) prev.disabled = page <= 1; if (next) next.disabled = page >= total; var status = root.querySelector('.meet-view-labor-page-status'); if (status) status.innerHTML = page + ' / ' + total; return false; },
        move: function (element, delta) { var root = this.findRoot(element); if (!root) return false; var current = parseInt(root.getAttribute('data-current-page') || '1', 10); if (isNaN(current) || current < 1) current = 1; return this.showPage(root, current + delta); }
    };
</script>
<script type="text/javascript">
    (function () {
        if (window.SweetMeetViewNavigate) return;
        window.SweetMeetViewNavigate = {
            go: function (meetId) {
                if (!/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(meetId || '')) return false;
                if (!window.SweetMeetCalendar || !window.QLDAMeetingLaborTab || typeof window.QLDAMeetingLaborTab.focusMeeting !== 'function') return false;
                var el = document.getElementById('<%= mdlMeetView.ClientID %>');
            var finished = false;
            function proceed() { if (finished) return; finished = true; window.QLDAMeetingLaborTab.focusMeeting(meetId); }
            if (!el) { proceed(); return false; }
            el.addEventListener('hidden.bs.modal', proceed, { once: true });
            var hidden = false;
            try {
                if (window.bootstrap && bootstrap.Modal) { var instance = bootstrap.Modal.getInstance ? bootstrap.Modal.getInstance(el) : null; if (instance) { instance.hide(); hidden = true; } }
                if (!hidden && window.jQuery && jQuery.fn && typeof jQuery.fn.modal === 'function') { jQuery(el).modal('hide'); hidden = true; }
            } catch (ignore) { }
            if (!hidden) { var close = el.querySelector('[data-bs-dismiss="modal"],[data-dismiss="modal"],.modal-header .btn-close,.modal-header .close'); if (close) { close.click(); hidden = true; } }
            if (hidden) window.setTimeout(proceed, 420);
            return false;
        }
        };
    })();
</script>
<SweetSoft:ExtraModal runat="server" ID="mdlMeetView" Type="Primary" Title="Thông tin cuộc họp">
    <ContentTemplate>
        <asp:UpdatePanel runat="server" ID="upMeetView" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="meet-view">
                    <div class="meet-view-layout">
                        <div class="meet-view-left">
                            <div class="meet-view-card meet-view-summary-card">
                                <div class="fieldlegend"><i class="fas fa-calendar-alt"></i><span>Cuộc họp</span></div>
                                <div class="meet-view-summary-main">
                                    <div class="meet-view-summary-info">
                                        <div class="meet-view-title"><asp:Literal runat="server" ID="lblMeetName"></asp:Literal></div>
                                    </div>
                                    <asp:Literal runat="server" ID="lblMeetStatus"></asp:Literal>
                                </div>
                                <div class="meet-view-summary-meta">
                                    <div class="meet-view-location"><i class="fas fa-map-marker-alt" aria-hidden="true"></i><asp:Literal runat="server" ID="lblMeetRoom"></asp:Literal></div>
                                </div>
                                <div class="meet-view-time-pair" aria-label="Thời gian dự kiến và thực tế của cuộc họp">
                                    <div class="meet-view-time-block"><span class="meet-view-time-caption"><i class="far fa-calendar-alt" aria-hidden="true"></i> Dự kiến</span><div class="meet-view-time-inline"><asp:Literal runat="server" ID="lblMeetStart"></asp:Literal></div></div>
                                    <div class="meet-view-time-block is-actual"><span class="meet-view-time-caption"><i class="far fa-check-circle" aria-hidden="true"></i> Thực tế</span><div class="meet-view-time-inline"><asp:Literal runat="server" ID="lblMeetActual"></asp:Literal></div></div>
                                </div>
                            </div>

                            <div class="meet-view-card meet-view-labor-card">
                                <div class="fieldlegend"><i class="far fa-clock"></i><span>Giờ công cuộc họp</span></div>
                                <div class="meet-view-labor-body"><asp:Literal runat="server" ID="ltrMeetLabor"></asp:Literal></div>
                            </div>
                        </div>

                        <div class="meet-view-right">
                            <div class="meet-view-card meet-view-content-card">
                                <div class="fieldlegend"><i class="fas fa-align-left"></i><span>Nội dung cuộc họp</span></div>
                                <div class="meet-view-content-body"><asp:Literal runat="server" ID="ltrMeetContent"></asp:Literal></div>
                            </div>
                        </div>
                    </div>

                    <div style="display:none;">
                        <asp:HiddenField runat="server" ID="hdfMeetParticipantIds" />
                        <SweetSoft:ExtraTextBox runat="server" ID="txtMeetParticipantNames" />
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
</SweetSoft:ExtraModal>
