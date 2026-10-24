<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlAddPhase.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlAddPhase" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<style>
    .phase-context {
        background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
        border: 1px solid #e2e8f0;
        border-radius: 8px;
        padding: 15px;
        margin-bottom: 20px;
        border-left: 4px solid #3b82f6;
    }
    .end-date-box {
        background-color: #f0fdf4; border: 1px solid #bbf7d0;
        border-radius: 6px; padding: 8px 12px; height: 100%;
        display: flex; flex-direction: column; justify-content: center;
    }
    .end-date-label { font-size: 11px; color: #166534; font-weight: 700; text-transform: uppercase; margin-bottom: 2px;}
    .end-date-value { font-size: 16px; color: #15803d; font-weight: 800; }

/* Lich du kien khi tao moi - chi tac dong den hai ExtraModal nay. */
.ql-new-schedule{display:grid;grid-template-columns:minmax(0,1.08fr) minmax(0,.92fr);width:100%;min-width:0;align-items:stretch}
.ql-new-schedule .ql-new-form{padding:15px;min-width:0}
.ql-new-schedule .ql-new-calendar-panel{padding:15px;background:#f8fafc;border-left:1px solid #e5ddee;min-width:0}
.ql-new-schedule .ql-new-title{display:flex;align-items:center;justify-content:space-between;gap:8px;border-bottom:1px dashed #d7c5ec;padding-bottom:10px;margin-bottom:10px;font-size:15px;font-weight:800;color:#52218a}
.ql-new-schedule .ql-new-modes{display:flex;gap:7px;margin-bottom:9px}
.ql-new-schedule .ql-new-mode{flex:1 1 0;padding:8px 6px;border:1px solid #ddd0ec;border-radius:8px;background:#fff;color:#664785;font-size:12px;font-weight:750;cursor:pointer}
.ql-new-schedule .ql-new-mode.is-active{background:#f2e9fc;color:#54218d;border-color:#ae88d9}
.ql-new-schedule .ql-new-cal-heading{display:flex;align-items:center;justify-content:space-between;gap:8px;margin-bottom:7px}
.ql-new-schedule .ql-new-cal-month{font-weight:800;font-size:14px;color:#50307d}
.ql-new-schedule .ql-new-cal-nav{width:29px;height:29px;border:1px solid #d9c8eb;border-radius:7px;background:#fff;color:#6b35a2;cursor:pointer}
.ql-new-schedule .ql-new-grid{display:grid;grid-template-columns:repeat(7,minmax(0,1fr));gap:4px}
.ql-new-schedule .ql-new-dow{text-align:center;color:#69587e;font-size:10.5px;font-weight:800;padding:6px 0}
.ql-new-schedule .ql-new-day{position:relative;display:flex;flex-direction:column;justify-content:space-between;min-width:0;min-height:49px;padding:4px;border:1px solid #e9e3f0;border-radius:8px;background:#fff;text-align:left;color:#443451;cursor:pointer;transition:border-color .15s,background .15s}
.ql-new-schedule .ql-new-day:hover:not([aria-disabled="true"]){border-color:#8b5cc6;background:#f8f2ff}
.ql-new-schedule .ql-new-day[aria-disabled="true"]{cursor:not-allowed}
.ql-new-schedule .ql-new-day.is-outside{background:#f9f8fb;color:#a59bb3}
.ql-new-schedule .ql-new-day.is-weekend{background:#edf4fd;color:#527097}
.ql-new-schedule .ql-new-day.is-holiday{background:#fff7e9;color:#a36216}
.ql-new-schedule .ql-new-day.is-span{background:#ece5f8;border-color:#d3bfec;color:#53238e}
.ql-new-schedule .ql-new-day.is-start{box-shadow:inset 0 0 0 2px #7c3aed}
.ql-new-schedule .ql-new-day.is-end{box-shadow:inset 0 0 0 2px #2563eb}
.ql-new-schedule .ql-new-day.is-locked{background:repeating-linear-gradient(-45deg,#eeeef2 0 8px,#d8d8e2 8px 16px);color:#777687}
.ql-new-schedule .ql-new-day-number{font-size:11.5px;font-weight:800}
.ql-new-schedule .ql-new-day-desc{font-size:9px;font-weight:700;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;width:100%}
.ql-new-schedule .ql-new-legend{display:flex;flex-wrap:wrap;gap:6px 12px;margin-top:9px;font-size:10.5px;color:#746384}
.ql-new-schedule .ql-new-legend span{display:inline-flex;align-items:center;gap:5px}
.ql-new-schedule .ql-new-legend i{height:9px;width:9px;border:1px solid #cfbedf;border-radius:3px}
.ql-new-schedule .ql-new-legend .span{background:#c7a9eb}.ql-new-schedule .ql-new-legend .off{background:#f5c572}.ql-new-schedule .ql-new-legend .end{background:#2563eb}
.ql-new-schedule .ql-new-message{min-height:32px;margin-top:9px;font-size:11.5px;color:#665677;line-height:1.5}
.ql-new-schedule .ql-new-message.is-error{color:#b45309;font-weight:700}
.ql-new-schedule .ql-new-date-input{background:#f9f7fe!important;cursor:pointer!important}
.ql-new-schedule .ql-new-end-date-box{background:#f0fdf4;border:1px solid #bbf7d0;border-radius:8px;padding:8px 11px;height:100%;display:flex;flex-direction:column;justify-content:center}
.ql-new-schedule .ql-new-end-date-label{font-size:11px;color:#166534;font-weight:750;margin-bottom:3px}
.ql-new-schedule .ql-new-end-date-value{font-size:16px;color:#15803d;font-weight:800}
.ql-new-schedule .ql-new-help{margin-top:6px;font-size:11.5px;color:#736582}
@media(max-width:1100px){.ql-new-schedule{grid-template-columns:1fr}.ql-new-schedule .ql-new-calendar-panel{border-left:0;border-top:1px solid #e5ddee}}
@media(max-width:575px){.ql-new-schedule .ql-new-form,.ql-new-schedule .ql-new-calendar-panel{padding:10px}.ql-new-schedule .ql-new-day{min-height:43px}}

#<%= mdlAddPhase.ClientID %> .modal-dialog {width:min(1180px,calc(100vw - 28px))!important;max-width:calc(100vw - 28px)!important;}
</style>

<SweetSoft:ExtraModal runat="server" ID="mdlAddPhase" Type="Primary" Title="Thêm Giai đoạn mới">
    <ContentTemplate>
        <asp:UpdatePanel ID="upAddPhase" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="ql-new-schedule"><div class="ql-new-form">
                    <asp:HiddenField ID="hfScheduleEndSelected" runat="server" />
                    <div class="phase-context">
                        <div class="d-flex justify-content-between align-items-center">
                            <div>
                                <span class="text-muted" style="font-size: 11px; font-weight: 700;">MÃ GIAI ĐOẠN</span><br />
                                <span class="text-primary" style="font-size: 18px; font-weight: 800;"><asp:Literal ID="ltrPhaseCode" runat="server"></asp:Literal></span>
                            </div>
                            <div class="text-end">
                                <span class="badge bg-secondary">Trạng thái: Chưa bắt đầu</span>
                            </div>
                        </div>
                    </div>

                    <div class="row g-3 mb-3">
                        <div class="col-12">
                            <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.TASK_NAME) %> <span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtTenGiaiDoan" runat="server" CssClass="form-control form-control-lg" placeholder="Nhập tên giai đoạn chính..."></asp:TextBox>
                        </div>
                    </div>

                    <!-- THÊM DÒNG PHỤ THUỘC TẠI ĐÂY -->
                    <div class="row g-3 mb-3">
                        <div class="col-12">
                            <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.DEPENDENT) %></label>
                            <asp:DropDownList ID="ddlPhuThuoc" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlPhuThuoc_SelectedIndexChanged"></asp:DropDownList>
                        </div>
                    </div>

                    <div class="row g-3 mb-3">
                        <div class="col-md-4">
                            <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.START_DATE) %><span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtNgayBatDau" runat="server" CssClass="form-control ql-new-date-input" TextMode="Date"></asp:TextBox>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.DURATION) %></label>
                            <div class="input-group">
                                <asp:TextBox ID="txtThoiHan" runat="server" CssClass="form-control text-center fw-bold" TextMode="Number" min="1" max="3500" Text="1"></asp:TextBox>
                                <span class="input-group-text bg-light">Ngày</span>
                            </div>
                        </div>
                        <div class="col-md-5">
                            <div class="ql-new-end-date-box">
                                <div class="ql-new-end-date-label"><i class="far fa-calendar-check me-1"></i><%= GetResourceText(BackEndResourceKeys.EXPECTED_COMPLETION_DATE) %></div>
                                <div class="ql-new-end-date-value"><asp:Label ID="lblNgayKetThuc" runat="server">--/--/----</asp:Label></div>
                                <asp:TextBox ID="txtNgayKetThuc" runat="server" style="display:none;"></asp:TextBox>
                            </div>
                        </div>
                    </div>

                    <div class="row g-3">
                        <div class="col-12">
                            <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.SUMMARY) %></label>
                            <asp:TextBox ID="txtMoTa" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" placeholder="Mô tả tóm tắt..."></asp:TextBox>
                        </div>
                    </div>
                </div>

<div class="ql-new-calendar-panel">
    <div class="ql-new-title"><span><i class="far fa-calendar-alt me-2"></i>Lịch biểu dự kiến</span><span class="text-muted" style="font-size:11px;font-weight:600">Theo lịch làm việc chung</span></div>
    <div class="ql-new-modes">
        <button type="button" class="ql-new-mode is-active" data-ql-mode="start"><i class="far fa-calendar-alt me-1"></i>Chọn ngày bắt đầu</button>
        <button type="button" class="ql-new-mode" data-ql-mode="end"><i class="far fa-calendar-check me-1"></i>Chọn ngày kết thúc</button>
    </div>
    <div class="ql-new-calendar" aria-label="Chọn mốc lịch dự kiến"></div>
    <div class="ql-new-legend"><span><i class="span"></i>Trong thời gian</span><span><i class="off"></i>Ngày nghỉ</span><span><i class="end"></i>Ngày kết thúc</span></div>
    <div class="ql-new-message" aria-live="polite" role="status"></div>
</div>
</div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
    <FooterTemplate>
        <asp:UpdatePanel ID="upnlFooterAddSubTask" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:LinkButton ID="btnSaveSubTask" runat="server" CssClass="btn btn-primary" OnClick="btnSavePhase_Click">
                    <i class="fas fa-save me-1"></i> <%= GetResourceText(BackEndResourceKeys.SAVE) %>
                </asp:LinkButton>
            </ContentTemplate>
        </asp:UpdatePanel>
    </FooterTemplate>
</SweetSoft:ExtraModal>
<script type="text/javascript">
(function(){
    // Noi bo rieng cua CtrlAddPhase: khong can sua TaskList.aspx / WebMethod hien co.
    var api = '<%= ResolveUrl("~/fTasks/TaskList.aspx/LayLichDuKienCongViec") %>';
    var modalId = '<%= mdlAddPhase.ClientID %>';
    var startId = '<%= txtNgayBatDau.ClientID %>';
    var durationId = '<%= txtThoiHan.ClientID %>';
    var endId = '<%= txtNgayKetThuc.ClientID %>';
    var labelId = '<%= lblNgayKetThuc.ClientID %>';
    var selectedEndId = '<%= hfScheduleEndSelected.ClientID %>';
        // Huy event cu trong UpdatePanel; neu khong, handler cu van tu chuyen sang 'end'.
        var previousSchedule = window.QLDAAddPhaseSchedule;
        if (previousSchedule && typeof previousSchedule.destroy === 'function') previousSchedule.destroy();
        $(document).off('.ql_phase').off('.ql_addphase_schedule');
        var state = { mode: 'start', year: 0, month: 0, days: {}, req: 0, timer: null, poller: null, lastModal: false };
        function id(v) { return document.getElementById(v); }
        function root() { var m = id(modalId); return m ? m.querySelector('.ql-new-schedule') : null; }
        function within(sel) { var r = root(); return r ? r.querySelector(sel) : null; }
        function validDate(s) { var m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(s || ''); if (!m) return null; var d = new Date(+m[1], +m[2] - 1, +m[3]); return d.getFullYear() === +m[1] && d.getMonth() === +m[2] - 1 && d.getDate() === +m[3] ? d : null; }
        function key(d) { return d.getFullYear() + '-' + ('0' + (d.getMonth() + 1)).slice(-2) + '-' + ('0' + d.getDate()).slice(-2); }
        function nice(s) { var d = validDate(s); return d ? ('0' + d.getDate()).slice(-2) + '/' + ('0' + (d.getMonth() + 1)).slice(-2) + '/' + d.getFullYear() : '--/--/----'; }
        function escapeHtml(s) { return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]; }); }
        function say(t, err) { var el = within('.ql-new-message'); if (el) { el.textContent = t || ''; el.classList.toggle('is-error', !!err); } }
        function visible() { var m = id(modalId); return !!m && (m.classList.contains('show') || m.getAttribute('aria-hidden') === 'false' || $(m).is(':visible')); }
        function mode(next, directClick) {
            if (next !== 'start' && next !== 'end') return; if (next === 'end' && !directClick && state.mode !== 'end') next = 'start'; state.mode = next; var r = root(); if (!r) return; $(r).find('[data-ql-mode]').each(function () { this.classList.toggle('is-active', this.getAttribute('data-ql-mode') === next); });
            say(next === 'end' ? 'Chọn ngày kết thúc là ngày làm việc. Thời hạn sẽ tự tính theo lịch công ty.' : 'Chọn ngày bắt đầu. Ngày kết thúc sẽ được tính lại theo số ngày làm việc.');
        }
        function draw() {
            var host = within('.ql-new-calendar'); if (!host || !state.year) return;
            var start = (id(startId) || {}).value || '', end = (id(endId) || {}).value || '';
            var first = new Date(state.year, state.month, 1), offset = (first.getDay() + 6) % 7, daysInMonth = new Date(state.year, state.month + 1, 0).getDate();
            var count = Math.max(35, Math.ceil((offset + daysInMonth) / 7) * 7);
            var html = '<div class="ql-new-cal-heading"><button type="button" class="ql-new-cal-nav" data-ql-step="-1" aria-label="Tháng trước"><i class="fas fa-chevron-left"></i></button><div class="ql-new-cal-month">Tháng ' + (state.month + 1) + '/' + state.year + '</div><button type="button" class="ql-new-cal-nav" data-ql-step="1" aria-label="Tháng sau"><i class="fas fa-chevron-right"></i></button></div><div class="ql-new-grid">';
            ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'].forEach(function (x) { html += '<div class="ql-new-dow">' + x + '</div>'; });
            for (var i = 0; i < count; i++) {
                var d = new Date(state.year, state.month, i - offset + 1), k = key(d), info = state.days[k];
                var isHoliday = info && info.status === 'holiday', isWeekend = info && info.status === 'weekend', working = !!(info && info.working === true), min = (id(startId) || {}).min || '';
                var locked = !!min && k < min, disabled = locked || (state.mode === 'end' && !working), span = start && end && k >= start && k <= end;
                var label = isHoliday ? 'Nghỉ lễ' : isWeekend ? 'Cuối tuần' : info && info.text ? info.text : !info ? 'Đang tải…' : '';
                var status = locked ? ' is-locked' : isHoliday ? ' is-holiday' : isWeekend ? ' is-weekend' : span ? ' is-span' : '';
                if (d.getMonth() !== state.month) status += ' is-outside';
                if (k === start) status += ' is-start'; if (k === end) status += ' is-end';
                var title = locked ? 'Ngoài giới hạn tiến độ (từ ' + nice(min) + ')' : (info && info.text ? info.text : (working ? 'Ngày làm việc' : 'Chưa có dữ liệu lịch'));
                html += '<button type="button" class="ql-new-day' + status + '" data-ql-date="' + k + '" aria-disabled="' + (disabled ? 'true' : 'false') + '" title="' + escapeHtml(title) + '"><span class="ql-new-day-number">' + d.getDate() + '</span><span class="ql-new-day-desc">' + escapeHtml(label) + '</span></button>';
            }
            host.innerHTML = html + '</div>';
        }
        function refresh(chosenEnd) {
            if (state.timer) { clearTimeout(state.timer); state.timer = null; }
            var start = id(startId), duration = id(durationId); if (!start || !duration) return;
            var day = validDate(start.value), raw = (duration.value || '').trim(), length = /^\d+$/.test(raw) ? parseInt(raw, 10) : 0;
            if (!day || length < 1 || length > 3500) { say('Ngày bắt đầu hoặc thời hạn phải hợp lệ (1–3500 ngày làm việc).', true); return; }
            var req = ++state.req;
            say('Đang đồng bộ lịch biểu chung…');
            $.ajax({
                type: 'POST', url: api, contentType: 'application/json; charset=utf-8', dataType: 'json',
                data: JSON.stringify({ ngayBatDau: start.value, thoiHan: length, ngayKetThucChon: chosenEnd || '', nam: state.year, thang: state.month + 1 })
            }).done(function (response) {
                if (req !== state.req) return;
                var r = response && response.d ? response.d : response;
                if (!r || !r.success) { say(r && r.message ? r.message : 'Không tính được lịch làm việc.', true); return; }
                if (r.days) $.extend(state.days, r.days);
                if (r.duration != null) duration.value = String(r.duration);
                if (r.endDate) { id(endId).value = r.endDate; var label = id(labelId); if (label) label.textContent = nice(r.endDate); }
                if (chosenEnd) { id(selectedEndId).value = chosenEnd; }
                draw(); say(state.mode === 'end' ? 'Chọn ngày kết thúc khác hoặc thay đổi thời hạn để tính lại.' : 'Lịch đã cập nhật theo ngày làm việc.');
            }).fail(function () { if (req !== state.req) return; say('Không tải được lịch làm việc. Kiểm tra kết nối, sau đó thử lại.', true); });
        }
        function init(reset) {
            var r = root(), start = id(startId); if (!r || !start) return;
            if (reset) { state.days = {}; id(selectedEndId).value = ''; }
            ++state.req;
            var d = validDate(start.value) || new Date(); state.year = d.getFullYear(); state.month = d.getMonth();
            mode(reset ? 'start' : state.mode, false); draw(); refresh('');
        }
        function bind() {
            $(document).on('click.ql_addphase_schedule', '#' + modalId + ' [data-ql-mode]', function () { mode(this.getAttribute('data-ql-mode'), true); draw(); });
            $(document).on('click.ql_addphase_schedule', '#' + modalId + ' [data-ql-step]', function () { var d = new Date(state.year, state.month + (parseInt(this.getAttribute('data-ql-step'), 10) || 0), 1); state.year = d.getFullYear(); state.month = d.getMonth(); draw(); refresh(''); });
            $(document).on('click.ql_addphase_schedule', '#' + modalId + ' [data-ql-date]', function () {
                if (this.getAttribute('aria-disabled') === 'true') return;
                var chosen = this.getAttribute('data-ql-date'), start = id(startId);
                if (state.mode === 'start') {
                    if (start.min && chosen < start.min) { say('Ngày bắt đầu không thể trước ' + nice(start.min), true); return; }
                    start.value = chosen; id(selectedEndId).value = ''; mode('start', false); draw(); refresh('');
                } else {
                    if (chosen < start.value) { say('Ngày kết thúc không thể trước ngày bắt đầu.', true); return; }
                    var day = state.days[chosen]; if (!day) { say('Ngày này chưa tải xong thông tin lịch, vui lòng thử lại.', true); return; }
                    if (day.working !== true) { say('Chỉ chọn ngày kết thúc là ngày làm việc.', true); return; }
                    refresh(chosen);
                }
            });
            $(document).on('input.ql_addphase_schedule change.ql_addphase_schedule', '#' + durationId, function () { id(selectedEndId).value = ''; if (state.timer) clearTimeout(state.timer); state.timer = setTimeout(function () { refresh(''); }, 240); });
            $(document).on('change.ql_addphase_schedule', '#' + startId, function () { id(selectedEndId).value = ''; var d = validDate(this.value); if (!d) return; if (this.min && this.value < this.min) { this.value = this.min; d = validDate(this.min); } state.year = d.getFullYear(); state.month = d.getMonth(); draw(); refresh(''); });
            $(document).on('shown.bs.modal.ql_addphase_schedule', '#' + modalId, function () { init(true); });
            $(document).on('hidden.bs.modal.ql_addphase_schedule', '#' + modalId, function () { state.req++; if (state.timer) clearTimeout(state.timer); state.lastModal = false; });
            if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) { Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () { if (visible()) { init(false); state.lastModal = true; } }); }
            // ExtraModal co the mo bang he thong JS rieng, khong phat shown.bs.modal.
            $(document).ready(function () { if (visible()) { init(false); state.lastModal = true; } });
            state.poller = setInterval(function () { var shown = visible(); if (shown && !state.lastModal) { init(true); } state.lastModal = shown; }, 450);
        }
        bind();
        window.QLDAAddPhaseSchedule = {
            __bound: true, version: 2, refresh: function () { init(false); }, destroy: function () {
                ++state.req;
                if (state.timer) clearTimeout(state.timer);
                if (state.poller) clearInterval(state.poller);
                $(document).off('.ql_addphase_schedule');
            }
        };
    })();
</script>
