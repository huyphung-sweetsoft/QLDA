<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlMeetGioCongTab.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fMeets.Controls.CtrlMeetGioCongTab" %>
<style>
.mlt-layout { display:grid;grid-template-columns:minmax(0,30%) minmax(0,1fr);gap:14px;width:100%;max-width:100%;min-width:0;box-sizing:border-box;min-height:510px;padding:10px 14px 14px;background:#f8f9fc; }
.mlt-side,.mlt-main { display:flex;flex-direction:column;min-width:0;min-height:0;width:100%;max-width:100%;box-sizing:border-box;background:#fff;border:1px solid #e4dcea;border-radius:12px;overflow:hidden; }
.mlt-selected-content { width:100%;max-width:100%;min-width:0;box-sizing:border-box;overflow:hidden; }
.mlt-side-head { display:flex;align-items:center;justify-content:space-between;gap:10px;flex-wrap:wrap;min-width:0;padding:13px 14px 12px;border-bottom:1px solid #e9e0f1;background:#faf7ff; }
.mlt-side-title { display:flex;gap:8px;align-items:center;color:#51218a;font-weight:800;font-size:14px; }
.mlt-meeting-count { display:inline-flex;align-items:center;justify-content:center;min-width:24px;height:22px;padding:1px 7px;margin-left:1px;border:1px solid #e1d4f2;border-radius:8px;background:#fff;color:#54218d;font-size:11px;font-weight:800;white-space:nowrap; }
.mlt-filter { display:grid;grid-template-columns:1.35fr 1fr 1fr;width:100%;max-width:100%;min-width:0;box-sizing:border-box;border:1px solid #dac8ee;border-radius:9px;overflow:hidden;background:#fff;color:#54218d;padding:3px;gap:3px; }
.mlt-filter a,.mlt-filter .aspNetDisabled { display:inline-flex;align-items:center;justify-content:center;min-width:0;gap:5px;padding:8px 5px;border-radius:6px;text-decoration:none;color:#6f4b91;font-size:11px;font-weight:750;white-space:normal;text-align:center;line-height:1.25;overflow-wrap:break-word;transition:background-color .15s,color .15s; }
.mlt-filter a:hover { background:#f4edfd; }
.mlt-filter a.is-filtered { color:#fff;background:#54218d; }
.mlt-page-footer { display:flex;align-items:center;justify-content:space-between;gap:9px;flex-wrap:wrap;min-width:0;padding:10px 12px;border-top:1px solid #e9e0f1;background:#fbf9fe; }
.mlt-page-info { color:#81748c;font-size:11px;font-weight:650;white-space:nowrap; }
.mlt-page-controls { display:inline-flex;align-items:center;justify-content:flex-end;flex-wrap:wrap;gap:4px;margin-left:auto; }
.mlt-page-button { display:inline-grid;place-items:center;min-width:28px;height:29px;padding:4px 7px;border:1px solid #e4d9f0;border-radius:7px;background:#fff;color:#60338c;font-size:11.5px;font-weight:750;cursor:pointer;transition:background .15s,border-color .15s; }
.mlt-page-button:hover:not([disabled]):not(.is-active) { background:#f1e9fc;border-color:#cbb6e5; }
.mlt-page-button.is-active { background:#54218d;color:#fff;border-color:#54218d;cursor:default; }
.mlt-page-button[disabled] { opacity:.42;cursor:not-allowed; }
.mlt-page-ellipsis { color:#8d7f9a;padding:0 2px;font-size:11px; }
.mlt-meeting-list { flex:1;max-height:min(72vh,700px);overflow-y:auto;scrollbar-width:thin; }
.mlt-meeting-link { display:block;padding:13px 15px;text-decoration:none!important;color:#443651;border-bottom:1px solid #eee9f3;border-left:3px solid transparent;transition:background .15s,border-color .15s; }
.mlt-meeting-link:hover { color:#54218d;background:#fbf8ff; }
.mlt-meeting-link.is-active { background:#f2ebfb;border-left-color:#6127a4;color:#4d1988; }
.mlt-meeting-link strong { display:block;line-height:1.45;font-size:13px;font-weight:750;overflow-wrap:anywhere; }
.mlt-meeting-link small { display:block;margin-top:6px;color:#84768f;font-size:11.5px;line-height:1.45; }
.mlt-meeting-link .mlt-done { font-size:10px;color:#8891a2;background:#edf0f5;border-radius:5px;padding:2px 6px;margin-left:5px; }
.mlt-main-head { display:flex;align-items:flex-start;justify-content:space-between;gap:14px 18px;flex-wrap:wrap;padding:16px 18px;border-bottom:1px solid #ebe3f3;background:linear-gradient(100deg,#f8f3ff,#fff); }
.mlt-main-title { color:#4d1b83;font-size:19px;font-weight:800;line-height:1.45;overflow-wrap:anywhere; }
.mlt-main-head-info { min-width:0;flex:1 1 320px; }
.mlt-main-head-side { display:flex;align-items:flex-start;justify-content:flex-end;flex:0 0 auto; }
.mlt-main-meta { display:flex;align-items:center;gap:11px 20px;flex-wrap:wrap;margin-top:7px;color:#83718f;font-size:12px; }
.mlt-main-meta i { margin-right:5px;color:#8352b8; }
.mlt-duration { display:inline-flex;gap:6px;align-items:center;margin-top:0;padding:6px 10px;background:#fff;border:1px solid #e3d5ef;border-radius:8px;color:#663298;font-weight:800;font-size:11px;white-space:nowrap; }
.mlt-table-scroll { flex:1;width:100%;max-width:100%;min-width:0;min-height:0;box-sizing:border-box;max-height:min(54vh,520px);overflow-x:auto;overflow-y:auto;scrollbar-width:thin; }
.mlt-table { width:100%;border-collapse:separate;border-spacing:0;margin:0;table-layout:fixed; }
.mlt-table th { position:sticky;top:0;z-index:2;background:#f4eff9;color:#603487;text-align:left;white-space:nowrap;padding:13px 11px;font-size:11.5px;font-weight:800;border-bottom:1px solid #e4d6ef; }
.mlt-table td { padding:13px 11px;border-bottom:1px solid #f0eaf3;vertical-align:middle;font-size:12.5px;color:#4c4156; }
.mlt-table tbody tr:last-child td { border-bottom:0; }
.mlt-table tbody tr:hover td { background:#fdfbff; }
.mlt-user { display:flex;align-items:center;gap:6px;flex-wrap:wrap;font-weight:750;line-height:1.4;overflow-wrap:anywhere; }
.mlt-user.is-host { color:#66279d; }
.mlt-user .fa-crown { color:#daa021; }
.mlt-host-tag { font-size:10px;color:#99660f;background:#fff0cd;padding:2px 5px;border-radius:5px; }
.mlt-join { width:auto!important;border-collapse:collapse;border:1px solid #d7c3ec;border-radius:8px;overflow:hidden;background:#fff; }
.mlt-join td { padding:0!important;border-bottom:0!important;border-right:1px solid #d7c3ec!important;vertical-align:middle;position:relative; }
.mlt-join td:last-child { border-right:0!important; }
.mlt-join input[type=radio] { position:absolute;opacity:0;width:1px;height:1px; }
.mlt-join label { display:block;cursor:pointer;padding:8px 9px;font-weight:800;font-size:10.5px;color:#7d6396;white-space:nowrap;line-height:1.4; }
.mlt-join td:has(input:checked) label { background:#5b2499;color:#fff; }
.mlt-join input:disabled + label,.mlt-join label:has(input:disabled) { cursor:default; }
.mlt-join td:has(input:focus-visible) { outline:2px solid #8d60be;outline-offset:-2px; }
.mlt-join[data-edit='0'] label { cursor:default; }
.mlt-theory { font-weight:800;color:#5b298f;white-space:nowrap; }
.mlt-actual { width:105px;max-width:100%;padding:8px 9px;text-align:right;border:1px solid #d9c6ec;border-radius:8px;color:#572c86;font-size:12px;font-weight:750; }
.mlt-actual:focus { outline:2px solid #ac83d5;outline-offset:1px; }
.mlt-actual[readonly] { color:#9c96a4;background:#f2f2f5;border-color:#e5e4e9; }
/* Non-ended meetings: the ENTIRE right pane is locked (not just the save button). */
.mlt-locked-panel { position:relative;isolation:isolate;flex:1;min-width:0;min-height:500px;width:100%;display:flex;align-items:center;justify-content:center;padding:40px 24px;overflow:hidden;box-sizing:border-box;background:linear-gradient(140deg,#f3f3f6,#faf9fc 55%,#f0eff4);text-align:center; }
.mlt-locked-panel::before,.mlt-locked-panel::after { content:"";pointer-events:none;position:absolute;z-index:-1;left:-23%;top:48%;width:146%;height:16px;border:1px solid rgba(87,88,102,.22);border-radius:100px;background:repeating-linear-gradient(90deg,#c2c4cc 0,#c2c4cc 17px,#ececf0 17px,#ececf0 25px,#999eab 25px,#999eab 28px);opacity:.42; }
.mlt-locked-panel::before { transform:rotate(24deg); }
.mlt-locked-panel::after { transform:rotate(-24deg); }
.mlt-lock-card { position:relative;z-index:1;max-width:425px;padding:26px 30px;border:1px solid #dfdce7;border-radius:18px;background:rgba(255,255,255,.96);box-shadow:0 14px 45px rgba(43,33,59,.06); }
.mlt-lock-symbol { position:relative;display:flex;align-items:center;justify-content:center;width:86px;height:86px;margin:0 auto 18px;border:2px solid #c9c4d1;border-radius:50%;background:#f5f3f7;color:#727080;font-size:30px; }
.mlt-lock-cross { position:absolute;right:-7px;bottom:-3px;display:grid;place-items:center;width:34px;height:34px;border:3px solid #fff;border-radius:50%;background:#c84b5a;color:#fff;font-size:17px; }
.mlt-lock-heading { display:block;margin:0;color:#41364d;font-size:19px;font-weight:800;line-height:1.4; }
.mlt-lock-meeting { display:block;margin:9px 0 0;color:#5c2a91;font-size:13px;font-weight:750;overflow-wrap:anywhere; }
.mlt-lock-note { display:block;margin:8px 0 0;color:#797486;font-size:12.5px;line-height:1.7; }
.mlt-main-foot { display:flex;align-items:center;justify-content:space-between;gap:12px;flex-wrap:wrap;padding:13px 17px;border-top:1px solid #eae3f0;background:#faf8fc; }
.mlt-total { display:flex;gap:12px;flex-wrap:wrap;color:#7c698a;font-size:12px;font-weight:700; }
.mlt-total b { color:#57258f; }
.mlt-save { display:inline-flex;align-items:center;gap:7px;background:#54218d!important;border-color:#54218d!important;color:#fff!important;font-size:12px!important;font-weight:800!important; }
.mlt-empty { display:grid;place-items:center;gap:9px;min-height:250px;padding:32px 18px;color:#8e859b;text-align:center;font-size:13px; }
.mlt-empty i { font-size:28px;color:#bba6d1; }
/* Empty labels must take zero height: prevents the Labor tab from sitting lower than Calendar. */
.mlt-error { display:block;color:#be3546;font-size:12px;padding:8px 14px; }
.mlt-error:empty,.mlt-success:empty { display:none!important;padding:0!important;border:0!important; }
.meet-view-shell .meet-panel-labor { padding:0!important;margin:0!important; }
.meet-view-shell .meet-panel-labor .mlt-layout { padding-top:10px; }
.mlt-success { display:block;color:#16803c;background:#f1fbf4;border-top:1px solid #d8eedf;font-size:12px;font-weight:750;padding:9px 14px; }
@media(max-width:1200px) { .mlt-layout { grid-template-columns:minmax(0,32%) minmax(0,1fr); } .mlt-table { min-width:660px; } }
@media(max-width:780px) { .mlt-layout { grid-template-columns:minmax(0,1fr); } .mlt-meeting-list { max-height:245px; } .mlt-main-head { padding:12px; } .mlt-main-head-side { width:100%;justify-content:flex-start; } .mlt-table-scroll { max-height:450px; } }

/* Hộp xác nhận riêng của giờ công, không dùng alert/confirm của browser. */
.mlt-confirm-backdrop {position:fixed;inset:0;z-index:2147480000;display:flex;align-items:center;justify-content:center;padding:20px;background:rgba(25,19,38,.55);backdrop-filter:blur(2px);}
.mlt-confirm-box {width:min(100%,440px);padding:24px;border:1px solid #e2d7ef;border-radius:15px;background:#fff;box-shadow:0 22px 70px rgba(28,12,53,.25);color:#382748;}
.mlt-confirm-title {display:flex;align-items:center;gap:10px;font-size:17px;font-weight:800;color:#52218a;}
.mlt-confirm-title i {display:grid;place-items:center;width:36px;height:36px;border-radius:10px;background:#f4ebff;}
.mlt-confirm-text {margin:15px 0 22px;color:#665778;font-size:13px;line-height:1.65;}
.mlt-confirm-actions {display:flex;flex-wrap:wrap;justify-content:flex-end;gap:8px;}
.mlt-confirm-actions button {min-height:37px;padding:8px 12px;border-radius:8px;border:1px solid #d8cbe7;cursor:pointer;font-weight:750;font-size:12px;}
.mlt-confirm-cancel {background:#fff;color:#655476;}
.mlt-confirm-discard {background:#f8f5fb;color:#54218d;}
.mlt-confirm-save {background:#54218d;color:#fff;border-color:#54218d!important;}
.mlt-confirm-actions button:disabled {opacity:.55;cursor:wait;}
@media(max-width:500px){.mlt-confirm-actions button {flex:1 1 100%;}.mlt-confirm-box{padding:18px;}}
</style>
<asp:UpdatePanel runat="server" ID="upLaborTab" UpdateMode="Conditional" ChildrenAsTriggers="true">
<ContentTemplate>
    <asp:LinkButton runat="server" ID="btnLoadLaborTab" CausesValidation="false" OnClick="btnLoadLaborTab_Click" Style="display:none">Tải giờ công</asp:LinkButton>
    <input type="hidden" name="QLDA_MeetLabor_SelectId" value="" />
    <input type="hidden" name="QLDA_MeetLabor_Page" value="" />
    <asp:HiddenField ID="hfSaveResult" runat="server" Value="idle" />
    <asp:Label ID="lblTabError" runat="server" CssClass="mlt-error" EnableViewState="false" Visible="false" />
    <div id="laborRoot" runat="server" class="mlt-layout" data-loaded="0">
        <aside class="mlt-side">
            <div class="mlt-side-head">
                <span class="mlt-side-title"><i class="far fa-calendar-alt"></i> Danh sách cuộc họp <span class="mlt-meeting-count" aria-label="Tổng số cuộc họp">(<asp:Literal ID="litMeetingCount" runat="server" Text="0" />)</span></span>
                <div class="mlt-filter" role="group" aria-label="Lọc cuộc họp theo thời gian">
                    <asp:LinkButton ID="btnFilterAll" runat="server" CausesValidation="false" OnClick="btnFilterMeetings_Click" CommandArgument="all" OnClientClick="return QLDAMeetingLaborTab.beforeFilter('all');" CssClass="is-filtered">Tất cả cuộc họp</asp:LinkButton>
                    <asp:LinkButton ID="btnFilterActive" runat="server" CausesValidation="false" OnClick="btnFilterMeetings_Click" CommandArgument="active" OnClientClick="return QLDAMeetingLaborTab.beforeFilter('active');">Chưa kết thúc</asp:LinkButton>
                    <asp:LinkButton ID="btnFilterEnded" runat="server" CausesValidation="false" OnClick="btnFilterMeetings_Click" CommandArgument="ended" OnClientClick="return QLDAMeetingLaborTab.beforeFilter('ended');">Đã kết thúc</asp:LinkButton>
                </div>
            </div>
            <div class="mlt-meeting-list">
                <asp:Repeater ID="rptMeetings" runat="server" OnItemCommand="rptMeetings_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton runat="server" ID="btnSelectMeeting" CommandName="select" CommandArgument='<%# Eval("IdLichHop") %>' CausesValidation="false" CssClass='<%# MeetingLinkCss(Eval("IdLichHop")) %>' OnClientClick='<%# "return QLDAMeetingLaborTab.selectMeeting(\"" + Eval("IdLichHop") + "\");" %>'>
                            <strong><%#: Eval("TenCuocHop") %></strong>
                            <small><i class="far fa-clock"></i> <%# MeetingTimeText(Eval("ThoiGianBatDau"), Eval("ThoiGianKetThuc")) %> <%# MeetingEndedBadge(Eval("ThoiGianKetThuc")) %></small>
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel runat="server" ID="pnlNoMeetings" CssClass="mlt-empty" Visible="true"><i class="far fa-calendar-times"></i><span><asp:Literal ID="litNoMeetings" runat="server" Text="Chưa có cuộc họp nào trong dự án." /></span></asp:Panel>
            </div>
            <nav class="mlt-page-footer" aria-label="Phân trang cuộc họp"><asp:Literal ID="litMeetingPager" runat="server" /></nav>
        </aside>
        <section class="mlt-main">
            <asp:Panel ID="pnlLockedMeeting" runat="server" CssClass="mlt-locked-panel" Visible="false">
                <div class="mlt-lock-card" role="status" aria-live="polite">
                    <div class="mlt-lock-symbol"><i class="fas fa-lock" aria-hidden="true"></i><span class="mlt-lock-cross"><i class="fas fa-times" aria-hidden="true"></i></span></div>
                    <strong class="mlt-lock-heading">Chưa thể chấm giờ công</strong>
                    <span class="mlt-lock-meeting"><asp:Literal ID="litLockedMeetingName" runat="server" /></span>
                    <span class="mlt-lock-note">Cuộc họp chưa kết thúc. Giờ công chỉ được chỉnh sửa và lưu sau khi cuộc họp kết thúc.</span>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlSelectedMeeting" runat="server" CssClass="mlt-selected-content" Visible="false">
                <div class="mlt-main-head"><div class="mlt-main-head-info"><div class="mlt-main-title"><asp:Literal ID="litMeetName" runat="server" /></div>
                    <div class="mlt-main-meta"><span><i class="far fa-calendar-alt"></i> <asp:Literal ID="litMeetDate" runat="server" /></span><span><i class="far fa-clock"></i> <asp:Literal ID="litMeetTime" runat="server" /></span></div></div>
                    <div class="mlt-main-head-side"><div class="mlt-duration"><i class="far fa-clock"></i> <asp:Literal ID="litDuration" runat="server" /></div></div>
                </div>
                <asp:HiddenField ID="hfSelectedMeetId" runat="server" />
                <asp:HiddenField ID="hfDuration" runat="server" />
                <div class="mlt-table-scroll">
                    <table class="mlt-table"><thead><tr><th style="width:29%">Tên nhân viên</th><th style="width:31%">Đã tham gia cuộc họp</th><th style="width:18%">Công lý thuyết</th><th style="width:22%">Công thực tế</th></tr></thead>
                    <tbody><asp:Repeater ID="rptLabor" runat="server" OnItemDataBound="rptLabor_ItemDataBound"><ItemTemplate>
                        <tr class="mlt-labor-row" data-duration="<%# CurrentDurationInvariant %>" data-can-edit="<%# CanEditLabor ? "1" : "0" %>">
                            <td><asp:HiddenField runat="server" ID="hfUserId" /><span id="personWrap" runat="server" class="mlt-user"><i id="iconHost" runat="server" class="fas fa-crown" title="Chủ trì"></i><asp:Label runat="server" ID="lblName" /><span id="tagHost" runat="server" class="mlt-host-tag">Chủ trì</span></span></td>
                            <td><asp:RadioButtonList runat="server" ID="rblAttendance" CssClass="mlt-join mlt-attendance-select" RepeatDirection="Horizontal" RepeatLayout="Table"><asp:ListItem Value="1">Có tham gia</asp:ListItem><asp:ListItem Value="0">Không tham gia</asp:ListItem></asp:RadioButtonList></td>
                            <td><asp:Label runat="server" ID="lblTheory" CssClass="mlt-theory" /></td>
                            <td><asp:TextBox ID="txtActual" runat="server" MaxLength="8" CssClass="mlt-actual" ToolTip="Nhập giờ thực tế" /></td>
                        </tr>
                    </ItemTemplate></asp:Repeater>
                    <asp:PlaceHolder runat="server" ID="phNoPeople" Visible="false"><tr><td colspan="4"><div class="mlt-empty">Cuộc họp này chưa có nhân viên.</div></td></tr></asp:PlaceHolder></tbody></table>
                </div>
                <asp:Label runat="server" ID="lblLaborError" CssClass="mlt-error" EnableViewState="false" />
                <div class="mlt-main-foot"><div class="mlt-total"><span>Người tham gia: <b id="mltJoinedCount"><asp:Literal ID="litJoinedCount" runat="server" /></b></span><span>Tổng lý thuyết: <b id="mltTheoryTotal"><asp:Literal ID="litTheoryTotal" runat="server" /></b></span></div><asp:LinkButton runat="server" ID="btnSaveLabor" OnClick="btnSaveLabor_Click" CausesValidation="false" CssClass="btn mlt-save"><i class="fas fa-save"></i> Lưu giờ công</asp:LinkButton></div>
            </asp:Panel>
            <asp:Panel runat="server" ID="pnlNoSelected" CssClass="mlt-empty"><i class="far fa-calendar-check"></i><span>Chọn cuộc họp ở bên trái để xem giờ công.</span></asp:Panel>
        </section>
    </div>
</ContentTemplate>
</asp:UpdatePanel>
<script type="text/javascript">
    (function () {
        if (window.QLDAMeetingLaborTab) return;
        var baseline = null, baselineRows = null, lastRenderedPane = null, lastMeetingId = null;
        var pendingAfterSave = null, savingForNavigation = false, bypassGuard = false;
        var dialog = null;

        function root() { return document.querySelector('.meet-panel-labor'); }
        function laborActive() { return !!document.querySelector('.meet-view-shell.meet-mode-labor'); }
        function rows() { var r = root(); return r ? r.querySelectorAll('.mlt-labor-row') : []; }
        function currentMeetingId() { var r = root(), h = r && r.querySelector('input[id$="_hfSelectedMeetId"]'); return h ? h.value : ''; }
        function editable() { var rr = rows(); return laborActive() && rr.length > 0 && rr[0].getAttribute('data-can-edit') === '1'; }
        function normalizeHours(v) {
            var s = String(v == null ? '' : v).trim().replace(',', '.');
            if (!s) return '';
            var n = Number(s);
            return isFinite(n) ? String(n) : s;
        }
        function readRows() {
            var result = [];
            Array.prototype.forEach.call(rows(), function (row) {
                var id = row.querySelector('input[id$="_hfUserId"]'), yes = row.querySelector('input[type="radio"][value="1"]'), actual = row.querySelector('.mlt-actual');
                var joined = !!(yes && yes.checked);
                result.push({ id: id ? id.value : '', joined: joined ? '1' : '0', raw: actual ? actual.value : '', hours: joined ? normalizeHours(actual && actual.value) : '0' });
            });
            return result;
        }
        function compact(v) { return JSON.stringify(v.map(function (i) { return [i.id, i.joined, i.hours]; })); }
        function setBaseline() { baselineRows = readRows(); baseline = compact(baselineRows); lastRenderedPane = root() && root().querySelector('.mlt-selected-content'); lastMeetingId = currentMeetingId(); }
        function dirty() { return editable() && baseline !== null && currentMeetingId() === lastMeetingId && compact(readRows()) !== baseline; }
        function fmt(v) { v = Math.round((Number(v) || 0) * 100) / 100; return v.toLocaleString('vi-VN', { maximumFractionDigits: 2 }) + ' giờ'; }
        function syncRow(row) {
            var yes = row.querySelector('input[type="radio"][value="1"]'), joined = !!(yes && yes.checked), isEditable = row.getAttribute('data-can-edit') === '1';
            var table = row.querySelector('.mlt-join'), theory = row.querySelector('.mlt-theory'), actual = row.querySelector('.mlt-actual');
            var duration = Number(row.getAttribute('data-duration') || 0);
            if (table) table.setAttribute('data-edit', isEditable ? '1' : '0');
            if (theory) theory.textContent = fmt(joined ? duration : 0);
            if (actual) { actual.readOnly = !isEditable || !joined; if (!joined) actual.value = '0'; }
        }
        function update() {
            var r = root(); if (!r) return;
            var rr = rows(), joined = 0, total = 0;
            Array.prototype.forEach.call(rr, function (row) { syncRow(row); var yes = row.querySelector('input[type="radio"][value="1"]'); if (yes && yes.checked) { joined++; total += Number(row.getAttribute('data-duration') || 0); } });
            var count = r.querySelector('#mltJoinedCount'), sum = r.querySelector('#mltTheoryTotal');
            if (count) count.textContent = String(joined); if (sum) sum.textContent = fmt(total);
        }
        function discardChanges() {
            if (!baselineRows) return;
            var originals = {}; baselineRows.forEach(function (x) { originals[x.id] = x; });
            Array.prototype.forEach.call(rows(), function (row) {
                var id = row.querySelector('input[id$="_hfUserId"]'); var old = id && originals[id.value]; if (!old) return;
                var input = row.querySelector('input[type="radio"][value="' + old.joined + '"]'); if (input) input.checked = true;
                var actual = row.querySelector('.mlt-actual'); if (actual) actual.value = old.raw;
            });
            update();
        }
        function createDialog() {
            if (dialog && document.body.contains(dialog)) return dialog;
            dialog = document.createElement('div'); dialog.className = 'mlt-confirm-backdrop'; dialog.style.display = 'none';
            dialog.setAttribute('role', 'presentation');
            dialog.innerHTML = '<div class="mlt-confirm-box" role="dialog" aria-modal="true" aria-label="Xác nhận lưu giờ công">' +
                '<div class="mlt-confirm-title"><i class="far fa-save" aria-hidden="true"></i>Thay đổi chưa được lưu</div>' +
                '<div class="mlt-confirm-text">Bạn đã thay đổi thông tin chấm giờ công. Bạn có muốn lưu lại trước khi tiếp tục không?</div>' +
                '<div class="mlt-confirm-actions">' +
                '<button type="button" class="mlt-confirm-cancel">Ở lại</button>' +
                '<button type="button" class="mlt-confirm-discard">Không lưu</button>' +
                '<button type="button" class="mlt-confirm-save">Lưu và tiếp tục</button></div></div>';
            document.body.appendChild(dialog);
            dialog.querySelector('.mlt-confirm-cancel').addEventListener('click', closeDialog);
            dialog.querySelector('.mlt-confirm-discard').addEventListener('click', function () { var action = dialog._action; closeDialog(); discardChanges(); dispatch(action); });
            dialog.querySelector('.mlt-confirm-save').addEventListener('click', function () {
                var action = dialog._action;
                var r = root(), save = r && r.querySelector('a[id$="_btnSaveLabor"]');
                if (!save) return;
                pendingAfterSave = action; savingForNavigation = true;
                dialog.querySelector('.mlt-confirm-save').disabled = true;
                dialog.querySelector('.mlt-confirm-discard').disabled = true;
                dialog.querySelector('.mlt-confirm-cancel').disabled = true;
                save.click();
            });
            dialog.addEventListener('click', function (e) { if (e.target === dialog && !savingForNavigation) closeDialog(); });
            return dialog;
        }
        function closeDialog() { if (!dialog) return; dialog.style.display = 'none'; dialog._action = null; }
        function ask(action) {
            var d = createDialog(); d._action = action;
            Array.prototype.forEach.call(d.querySelectorAll('button'), function (b) { b.disabled = false; });
            d.style.display = 'flex'; d.querySelector('.mlt-confirm-save').focus();
        }
        function dispatch(action) {
            if (!action) return;
            bypassGuard = true;
            try {
                if (action.type === 'meeting') {
                    var r = root(), target = r && r.querySelector('input[name="QLDA_MeetLabor_SelectId"]'), page = r && r.querySelector('input[name="QLDA_MeetLabor_Page"]');
                    var button = document.getElementById('<%= btnLoadLaborTab.ClientID %>');
                if (target && button) { target.value = action.id; if (page) page.value = ''; button.click(); }
            } else if (action.type === 'page') {
                var r2 = root(), pageTarget = r2 && r2.querySelector('input[name="QLDA_MeetLabor_Page"]'), selected = r2 && r2.querySelector('input[name="QLDA_MeetLabor_SelectId"]');
                var pageButton = document.getElementById('<%= btnLoadLaborTab.ClientID %>');
                if (pageTarget && pageButton) { pageTarget.value = String(action.page); if (selected) selected.value = ''; pageButton.click(); }
            } else if (action.type === 'filter') {
                var selectors = { all:'<%= btnFilterAll.ClientID %>', active:'<%= btnFilterActive.ClientID %>', ended:'<%= btnFilterEnded.ClientID %>' };
                    var filter = document.getElementById(selectors[action.mode]); if (filter) filter.click();
                } else if (action.type === 'view') {
                    if (window.SweetMeetCalendar && typeof window.SweetMeetCalendar.changeView === 'function') window.SweetMeetCalendar.changeView(action.view);
                } else if (action.type === 'url') {
                    window.location.assign(action.href);
                } else if (action.type === 'refresh') {
                    // GET, not a replay of the ASP.NET POST.
                    window.location.replace(window.location.pathname + window.location.search + window.location.hash);
                }
            } finally { bypassGuard = false; }
        }
        function navigate(action) { if (bypassGuard || !dirty()) { dispatch(action); return; } ask(action); }
        function selectMeeting(id) { navigate({ type: 'meeting', id: id }); return false; }
        function changePage(page) {
            page = Number(page);
            if (!isFinite(page) || Math.floor(page) !== page || page < 1) return false;
            navigate({ type: 'page', page: page }); return false;
        }
        function beforeFilter(mode) { if (bypassGuard) return true; if (!dirty()) return true; ask({ type: 'filter', mode: mode }); return false; }
        function afterRender() {
            update();
            if (lastRenderedPane !== (root() && root().querySelector('.mlt-selected-content')) || lastMeetingId !== currentMeetingId()) setBaseline();
            if (!savingForNavigation) return;
            var r = root(), state = r && r.querySelector('input[id$="_hfSaveResult"]');
            if (state && state.value === 'success') {
                var action = pendingAfterSave; pendingAfterSave = null; savingForNavigation = false; closeDialog(); setBaseline();
                // Defer next async request until the current UpdatePanel request fully ends.
                setTimeout(function () { dispatch(action); }, 40);
            } else if (state && state.value === 'error') {
                savingForNavigation = false; pendingAfterSave = null; closeDialog();
                // Data is still in the current form; user can fix the error and retry.
            }
        }
        document.addEventListener('change', function (e) { if (e.target.matches && e.target.matches('.meet-panel-labor .mlt-join input[type="radio"]')) update(); });
        // Prevent Enter in an hours input from submitting the entire WebForms page.
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && e.target.matches && e.target.matches('.meet-panel-labor .mlt-actual')) { e.preventDefault(); return; }
            if (e.key === 'Escape' && dialog && dialog.style.display === 'flex' && !savingForNavigation) { e.preventDefault(); closeDialog(); return; }
            if (!laborActive() || !(e.key === 'F5' || ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'r'))) return;
            e.preventDefault();
            navigate({ type: 'refresh' });
        }, true);
        document.addEventListener('click', function (e) {
            if (!laborActive() || !dirty() || bypassGuard || savingForNavigation) return;
            var t = e.target && e.target.closest ? e.target : null; if (!t) return;
            var view = t.closest('[data-meet-view]');
            if (view && view.getAttribute('data-meet-view') !== 'labor') {
                e.preventDefault(); e.stopImmediatePropagation(); ask({ type: 'view', view: view.getAttribute('data-meet-view') }); return;
            }
            var link = t.closest('a[href]'); if (!link || link.closest('.meet-panel-labor') || link.target === '_blank' || link.hasAttribute('download')) return;
            var href = link.getAttribute('href'); if (!href || href.charAt(0) === '#' || /^javascript:/i.test(href)) return;
            var url; try { url = new URL(href, window.location.href); } catch (ignore) { return; }
            if (!/^https?:$/i.test(url.protocol) || url.origin !== window.location.origin) return;
            e.preventDefault(); e.stopImmediatePropagation(); ask({ type: 'url', href: url.href });
        }, true);
        if (window.Sys && Sys.Application) Sys.Application.add_load(afterRender);
        if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', afterRender); else afterRender();
        window.QLDAMeetingLaborTab = { refresh: afterRender, selectMeeting: selectMeeting, changePage: changePage, beforeFilter: beforeFilter, hasChanges: dirty };
    })();
</script>
