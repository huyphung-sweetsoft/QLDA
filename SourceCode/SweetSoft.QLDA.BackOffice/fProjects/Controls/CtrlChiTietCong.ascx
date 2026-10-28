<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlChiTietCong.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.Controls.CtrlChiTietCong" %>
<%@ Register Assembly="SweetSoft.QLDA.Controls" Namespace="SweetSoft.QLDA.Controls" TagPrefix="SweetSoft" %>
<SweetSoft:ExtraModal runat="server" ID="mdlPersonalDetail" Type="Primary" Title="Chi tiết phần việc" Width="92%">
    <ContentTemplate>
        <div class="pd-detail-modal-wrap">
            <div class="pd-detail-modal-heading">
                <h4 id="<%= EmbeddedInTaskView ? "tvdLaborPersonalDetailTitle" : "pdPersonalDetailTitle" %>" class="pd-detail-modal-title">Chi tiết</h4>
                <div id="<%= EmbeddedInTaskView ? "tvdLaborPersonalDetailEquation" : "pdPersonalDetailEquation" %>" class="pd-labor-heading-equation" hidden></div>
                <p id="<%= EmbeddedInTaskView ? "tvdLaborPersonalDetailSub" : "pdPersonalDetailSub" %>" class="pd-detail-modal-sub">Xem thêm thông tin trong phạm vi dự án hiện tại.</p>
            </div>
            <asp:UpdatePanel ID="upDetail" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <asp:HiddenField ID="hfDetailType" runat="server" />
                    <asp:HiddenField ID="hfDetailStaff" runat="server" />
                    <asp:LinkButton ID="btnLoadDetail" runat="server" CausesValidation="false"
                        OnClick="btnLoadDetail_Click" Style="display:none" aria-hidden="true" />
                    <div id="<%= EmbeddedInTaskView ? "tvdLaborPersonalDetailModalBody" : "pdPersonalDetailModalBody" %>">
                        <asp:Panel ID="pnlPdMessage" runat="server" Visible="false" CssClass="pd-personal-empty"><asp:Literal ID="litDetailHtml" runat="server" EnableViewState="false" /></asp:Panel>
                        <asp:Panel ID="pnlPdOverview" runat="server" CssClass="pd-ds-overview" Visible="false">
                            <strong>Tổng hợp giờ công: <asp:Literal ID="litPdTeamName" runat="server" /></strong>
                            <nav class="pd-ds-equation" aria-label="Đi đến bảng giờ công">
                                <button type="button" class="pd-ds-jump" data-pd-ds-jump="task">Công việc (<asp:Literal ID="litPdJumpTask" runat="server" />)</button>
                                <span class="pd-ds-equation-sign">+</span>
                                <button type="button" class="pd-ds-jump" data-pd-ds-jump="meeting">Công họp (<asp:Literal ID="litPdJumpMeeting" runat="server" />)</button>
                                <span class="pd-ds-equation-sign">+</span>
                                <button type="button" class="pd-ds-jump" data-pd-ds-jump="issue">Công vấn đề (<asp:Literal ID="litPdJumpIssue" runat="server" />)</button>
                                <span class="pd-ds-equation-sign">=</span><strong class="pd-ds-equation-total"><asp:Literal ID="litPdJumpTotal" runat="server" /></strong>
                            </nav>
                        </asp:Panel>
                        <asp:Panel ID="pnlPdSummary" runat="server" Visible="false" CssClass="pd-detail-grid">
                            <asp:Repeater ID="rptPdSummaries" runat="server" EnableViewState="false"><ItemTemplate>
                                <div class='<%# Convert.ToBoolean(Eval("IsTotal")) ? "pd-summary-row is-total" : "pd-summary-row" %>'><strong><%#: Eval("Name") %></strong><span><%#: Eval("Hours") %></span></div>
                            </ItemTemplate></asp:Repeater>
                        </asp:Panel>
                        <asp:Panel ID="pnlPdIssueCards" runat="server" Visible="false" CssClass="pd-detail-list">
                            <asp:Repeater ID="rptPdIssueCards" runat="server" EnableViewState="false"><ItemTemplate>
                                <div class="pd-detail-item"><div class="pd-detail-row"><div class="pd-personal-item-title"><%#: Eval("Name") %></div>
                                    <div class="pd-personal-meeting-flags"><span class='pd-personal-tag <%# Eval("StatusCss") %>'><%#: Eval("Status") %></span>
                                        <span class="pd-personal-hours"><i class="far fa-clock"></i><%#: Eval("Hours") %></span></div></div>
                                    <div class="pd-detail-subrow"><span><i class="far fa-calendar-alt me-1"></i><%#: Eval("Created") %></span></div>
                                </div>
                            </ItemTemplate></asp:Repeater>
                        </asp:Panel>
                        <asp:Repeater ID="rptPdSections" runat="server" EnableViewState="false">
                            <ItemTemplate>
                                <asp:PlaceHolder runat="server" Visible='<%# Eval("ShowTitle") %>'>
                                    <h5 class='<%# Convert.ToBoolean(Eval("IsTeamSection")) ? "pd-ds-subtitle pd-ds-section-title" : "pd-ds-subtitle" %>' data-pd-detail-section='<%# Eval("Kind") %>'><%#: Eval("Title") %></h5>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder runat="server" Visible='<%# !Convert.ToBoolean(Eval("IsMeeting")) %>'>
                                    <div class="pd-ds-table-scroller"><table class="pd-ds-table">
                                        <thead><tr><th>Công việc / Vấn đề</th><th>Tự khai thường (phút)</th><th>Tự khai tăng ca (phút)</th>
                                            <th>PM chốt thường (phút)</th><th>PM chốt tăng ca (phút)</th><th>Kết quả cuối cùng</th><th>Đối soát</th></tr></thead>
                                        <tbody>
                                            <asp:Repeater runat="server" DataSource='<%# Eval("LaborRows") %>' EnableViewState="false"><ItemTemplate>
                                                <tr data-pd-task-id='<%# Eval("TaskId") %>' data-pd-issue-id='<%# Eval("IssueId") %>'>
                                                    <td class="pd-ds-name">
                                                        <asp:PlaceHolder runat="server" Visible='<%# !Convert.ToBoolean(Eval("IsIssue")) %>'>
                                                            <button type="button" class="pd-ds-hover-trigger pd-view-object-link" data-pd-view-kind="task" data-pd-view-id='<%# Eval("TaskId") %>' title="Xem chi tiết công việc" aria-label='<%# EncodePdAttr("Xem chi tiết công việc: " + Convert.ToString(Eval("Name"))) %>'>
                                                                <asp:PlaceHolder runat="server" Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("Code"))) %>'><span class="pd-team-record-code"><%#: Convert.ToString(Eval("Code")).Trim().TrimEnd('.') %>.</span> </asp:PlaceHolder><%#: Eval("Name") %>
                                                            </button>
                                                            <div class="pd-ds-hover-template" hidden><div class="pd-detail-item">
                                                                <div class="pd-detail-row"><div class="pd-personal-item-title"><asp:PlaceHolder runat="server" Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("Code"))) %>'><span class="pd-personal-task-code"><%#: Convert.ToString(Eval("Code")).Trim().TrimEnd('.') %>.</span> </asp:PlaceHolder><%#: Eval("Name") %></div>
                                                                    <div class="pd-personal-meeting-flags"><span class='pd-personal-tag <%# Eval("StatusCss") %>'><%#: Eval("Status") %></span>
                                                                        <asp:PlaceHolder runat="server" Visible='<%# Eval("LateFinished") %>'><span class="pd-task-late-note">(trễ hạn)</span></asp:PlaceHolder>
                                                                        <span class="pd-personal-hours"><i class="far fa-clock"></i><%#: Eval("HoursCompact") %></span></div></div>
                                                                <div class="pd-detail-subrow"><span><i class="far fa-calendar-alt me-1"></i><%#: Eval("DueText") %></span>
                                                                    <span class="pd-task-percent" data-pd-progress-tooltip='<%# EncodePdAttr(Eval("ProgressTooltip")) %>' data-pd-progress-tone='<%# Eval("BarCss") %>' tabindex="0" aria-label='<%# EncodePdAttr(Eval("ProgressTooltip")) %>'>Tiến độ <%#: Eval("Percent") %>%</span></div>
                                                                <div class='pd-personal-progress <%# Eval("BarCss") %>' data-pd-progress-tooltip='<%# EncodePdAttr(Eval("ProgressTooltip")) %>' data-pd-progress-tone='<%# Eval("BarCss") %>' tabindex="0" role="progressbar" aria-valuemin="0" aria-valuemax="100" aria-valuenow='<%# Eval("Percent") %>' aria-label='<%# EncodePdAttr(Eval("ProgressTooltip")) %>'>
                                                                    <span style='<%# "width:" + Eval("Percent") + "%" %>'></span></div>
                                                            </div></div>
                                                        </asp:PlaceHolder>
                                                        <asp:PlaceHolder runat="server" Visible='<%# Eval("IsIssue") %>'>
                                                            <%#: Eval("Name") %><small class="pd-ds-link-note">Liên kết công việc bị ảnh hưởng; không phải phiên giờ vấn đề độc lập</small>
                                                        </asp:PlaceHolder>
                                                    </td>
                                                    <td><%#: Eval("SelfRegular") %></td><td><%#: Eval("SelfOvertime") %></td>
                                                    <td><%#: Eval("ApprovedRegular") %></td><td><%#: Eval("ApprovedOvertime") %></td>
                                                    <td><strong><%#: Eval("Result") %></strong></td><td><span class="pd-ds-status"><%#: Eval("Approval") %></span></td>
                                                </tr>
                                            </ItemTemplate></asp:Repeater>
                                        </tbody>
                                    </table></div>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder runat="server" Visible='<%# Eval("IsMeeting") %>'>
                                    <div class="pd-ds-table-scroller"><table class="pd-ds-table"><thead><tr><th>Cuộc họp</th><th>Thời gian</th><th>Thời lượng</th><th>Xác nhận</th><th>Kết quả</th></tr></thead><tbody>
                                        <asp:Repeater runat="server" DataSource='<%# Eval("MeetingRows") %>' EnableViewState="false"><ItemTemplate>
                                            <tr><td><button type="button" class="pd-view-object-link" data-pd-view-kind="meet" data-pd-view-id='<%# Eval("MeetId") %>' title="Xem chi tiết cuộc họp"><%#: Eval("Name") %></button></td><td><%#: Eval("Start") %></td><td><%#: Eval("Duration") %></td>
                                                <td><%#: Eval("Joined") %></td><td><strong><%#: Eval("Result") %></strong></td></tr>
                                        </ItemTemplate></asp:Repeater>
                                    </tbody></table></div>
                                </asp:PlaceHolder>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:Panel ID="pnlPdFootnote" runat="server" Visible="false" CssClass="pd-ds-footnote"><asp:Literal ID="litPdFootnote" runat="server" /></asp:Panel>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </ContentTemplate>
</SweetSoft:ExtraModal>

<style type="text/css">
/* V17 - Bảng công đầy đủ: chỉ tác động popup CtrlChiTietCong, không tác động popup cuộc họp riêng. */
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-detail-modal-heading { padding:17px 21px 15px!important;margin:-12px -14px 15px!important;border-bottom:1px solid #cbd7e7!important;background:linear-gradient(112deg,#f3f7ff 0%,#f9f7ff 63%,#fff 100%)!important;box-shadow:0 3px 12px rgba(25,48,87,.045); }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-detail-modal-title { display:block;margin:0 0 13px;color:#26364b!important;font-size:19px;font-weight:800;line-height:1.4;letter-spacing:-.2px; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-detail-modal-title::before { content:"";display:inline-block;width:4px;height:20px;margin-right:10px;vertical-align:-4px;border-radius:5px;background:#54218d; }
#<%= mdlPersonalDetail.ClientID %> .pd-labor-heading-equation[hidden] { display:none!important; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-labor-heading-equation { display:flex;align-items:center;flex-wrap:wrap;gap:9px;min-width:0; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-labor-heading-equation .pd-ds-equation { display:flex;align-items:center;flex-wrap:wrap;gap:8px;min-width:0;font-size:13px; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-labor-heading-equation .pd-ds-jump { display:inline-flex;align-items:center;justify-content:center;min-height:37px;padding:8px 11px;border-radius:8px;border:1px solid #bfd2ef;background:#eef4ff;color:#25549b;font-size:12.5px;font-weight:800;box-shadow:0 1px 2px rgba(25,48,87,.03); }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-labor-heading-equation .pd-ds-jump[data-pd-ds-jump="meeting"] { border-color:#b4dfd3;background:#eaf8f2;color:#136b51; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-labor-heading-equation .pd-ds-jump[data-pd-ds-jump="issue"] { border-color:#efd9b2;background:#fff7e9;color:#945b17; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-labor-heading-equation .pd-ds-jump:hover { filter:brightness(.975);box-shadow:0 2px 6px rgba(25,48,87,.08); }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-labor-heading-equation .pd-ds-jump:focus-visible { outline:2px solid #7055ad;outline-offset:2px; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-labor-heading-equation .pd-ds-equation-sign { color:#718096;font-size:16px;font-weight:800; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-labor-heading-equation .pd-ds-equation-total { display:inline-flex;align-items:center;min-height:37px;padding:8px 13px;border:1px solid #cfb9ec;border-radius:8px;background:#f1eafa;color:#54218d;font-size:14px;font-weight:850; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup #pdPersonalDetailModalBody { color:#334155; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-ds-section-title { position:relative;display:flex;align-items:center;min-height:43px;padding:10px 14px;margin:19px 0 9px;border:1px solid #d4e0ef;border-left:4px solid #4278be;border-radius:8px;background:#f0f5fc;color:#244a7c!important;font-size:14px;font-weight:800;line-height:1.4; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-ds-section-title[data-pd-detail-section="meeting"] { border-color:#c5e5dc;border-left-color:#168568;background:#eefaf6;color:#176c55!important; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-ds-section-title[data-pd-detail-section="issue"] { border-color:#efdcbf;border-left-color:#c58b31;background:#fff9ef;color:#825713!important; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-ds-table-scroller { margin:0 0 14px;border:1px solid #d6dfea;border-radius:10px;background:#fff;box-shadow:0 2px 6px rgba(25,48,87,.04); }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-ds-table { color:#334155;font-size:12.5px; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-ds-table th { padding:13px 12px;background:#edf3fb!important;color:#244b7b!important;border-right:1px solid #dce6f1;border-bottom:1px solid #cbd9ea;font-size:12px;font-weight:800; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-ds-section-title[data-pd-detail-section="meeting"] + .pd-ds-table-scroller .pd-ds-table th { background:#ebf7f2!important;color:#176c55!important;border-bottom-color:#c7e4d7; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-ds-section-title[data-pd-detail-section="issue"] + .pd-ds-table-scroller .pd-ds-table th { background:#fff6e9!important;color:#825713!important;border-bottom-color:#e8d7b8; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-ds-table td { padding:12px 12px;border-bottom-color:#e4eaf2;color:#334155;line-height:1.45; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-ds-table tbody tr:nth-child(even) { background:#f9fbfe; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-ds-table tbody tr:hover { background:#f3f6fc; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-ds-table tbody td strong { color:#54218d; }
#<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-ds-footnote { margin:13px 3px 4px;padding:10px 13px;border-left:3px solid #b8cae3;border-radius:5px;background:#f8fafc;color:#64748b;font-size:11.5px;line-height:1.6; }
@media(max-width:767.98px) { #<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-detail-modal-heading { padding:14px 12px!important;margin:-12px -14px 13px!important; } #<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-detail-modal-title { font-size:17px; } #<%= mdlPersonalDetail.ClientID %>.pd-team-full-popup .pd-labor-heading-equation .pd-ds-jump { min-height:33px;font-size:11.5px;padding:7px 8px; } }

/* Tên đối tượng trong bảng giờ công: không thay đổi row/tooltip hiện hữu. */
#<%= mdlPersonalDetail.ClientID %> .pd-ds-table .pd-view-object-link { appearance:none;border:0;padding:0;margin:0;background:transparent;color:#34445b;font:inherit;font-weight:750;text-align:left;cursor:pointer; }
#<%= mdlPersonalDetail.ClientID %> .pd-ds-table .pd-view-object-link:hover { color:#2563eb;text-decoration:none;border-bottom:0; }
#<%= mdlPersonalDetail.ClientID %> .pd-ds-table .pd-view-object-link:focus-visible { text-decoration:none; }
#<%= mdlPersonalDetail.ClientID %> .pd-ds-table .pd-team-record-code,#<%= mdlPersonalDetail.ClientID %> .pd-ds-hover-template .pd-personal-task-code { display:inline;margin:0 4px 0 0;padding:0;border:0;border-radius:0;background:none;color:inherit;font:inherit;line-height:inherit;vertical-align:baseline; }
#<%= mdlPersonalDetail.ClientID %> .pd-ds-table .pd-ds-hover-trigger.pd-view-object-link:hover { border-bottom:0;color:#2563eb;text-decoration:none; }
#<%= mdlPersonalDetail.ClientID %> .pd-ds-table .pd-view-object-link:focus-visible { outline:2px solid #ad88d4;outline-offset:2px;border-radius:4px; }
</style>

<script type="text/javascript">
    (function () {
        if (<%= EmbeddedInTaskView ? "true" : "false" %>) return;
        if (window.QLDAProjectDetailPopup && window.QLDAProjectDetailPopup.version === 17) return;
        var clientId = '<%= ModalClientId %>';
        var typeId = '<%= RequestTypeId %>';
        var staffId = '<%= RequestStaffId %>';
        var triggerId = '<%= LoadButtonId %>';
        var pending = false;
        var preview = null, activeTrigger = null;
        function modalNode() {
            var el = document.getElementById(clientId);
            return el && el.classList && el.classList.contains('modal') ? el : (el ? el.querySelector('.modal') : null);
        }
        function showModal() {
            var modal = modalNode();
            if (!modal) return;
            if (window.bootstrap && window.bootstrap.Modal) {
                var instance = window.bootstrap.Modal.getInstance ? window.bootstrap.Modal.getInstance(modal) : null;
                (instance || new window.bootstrap.Modal(modal)).show();
            } else if (window.jQuery && typeof window.jQuery.fn.modal === 'function') {
                window.jQuery(modal).modal('show');
            }
        }
        function hidePreview() {
            if (preview && preview.parentNode) preview.parentNode.removeChild(preview);
            preview = null; activeTrigger = null;
        }
        function open(button) {
            if (!button || pending) return false;
            var source = button.getAttribute('data-pd-detail-source') || '';
            var valid = /^(pdPersonal(Task|Issue|Hour)Detail|pdTeamFullDetail|pdTeamDetail-(task|issue))$/.test(source);
            if (!valid) return false;
            var isTeam = source.indexOf('pdTeam') === 0;
            // JS never authorizes a member: C# checks PM/Admin and project membership again.
            var member = '';
            if (isTeam) {
                var memberInput = document.querySelector('#projectTeamHoursView input[type="hidden"][id$="hfTeamHoursMember"]');
                member = memberInput ? memberInput.value : '';
                if (!member) return false;
            }
            var title = document.getElementById('pdPersonalDetailTitle');
            var sub = document.getElementById('pdPersonalDetailSub');
            if (title) title.textContent = button.getAttribute('data-pd-detail-title') || 'Chi tiết';
            if (sub) { sub.textContent = button.getAttribute('data-pd-detail-sub') || ''; sub.hidden = false; }
            var headingEquation = document.getElementById('pdPersonalDetailEquation');
            if (headingEquation) { headingEquation.textContent = ''; headingEquation.hidden = true; }
            var currentModal = modalNode();
            if (currentModal) currentModal.classList.remove('pd-team-full-popup');
            var t = document.getElementById(typeId), s = document.getElementById(staffId), load = document.getElementById(triggerId);
            if (!t || !s || !load) return false;
            t.value = source; s.value = member;
            hidePreview();
            pending = true;
            load.click(); // asynchronous UpdatePanel request: only loads when clicked.
            return false;
        }
        // Di chuyển đúng phép tính đã được C# sinh ra lên tiêu đề; không tự tính lại giờ công.
        // Chỉ áp dụng cho bảng công đầy đủ, giữ nguyên các popup công việc/vấn đề khác.
        function formatFullLaborHeading(content, modal) {
            var source = content ? content.querySelector('.pd-ds-overview') : null;
            var title = document.getElementById('pdPersonalDetailTitle');
            var subtitle = document.getElementById('pdPersonalDetailSub');
            var holder = document.getElementById('pdPersonalDetailEquation');
            var isFull = !!source && (document.getElementById(typeId) || {}).value === 'pdTeamFullDetail';
            if (modal) modal.classList.toggle('pd-team-full-popup', isFull);
            if (holder) { holder.textContent = ''; holder.hidden = !isFull; }
            if (!isFull) { if (subtitle) subtitle.hidden = false; return; }
            var heading = source.querySelector('strong');
            var name = heading ? heading.textContent.replace(/^Tổng hợp giờ công:\s*/i, '').trim() : '';
            if (title) title.textContent = 'Bảng công của' + (name ? ' ' + name : ' thành viên');
            if (subtitle) subtitle.hidden = true;
            var calculation = source.querySelector('.pd-ds-equation');
            if (holder && calculation) holder.appendChild(calculation);
            // Xóa vùng tổng hợp cũ sau khi chuyển nội dung, tránh trùng với tiêu đề.
            source.parentNode.removeChild(source);
        }
        function received() {
            pending = false;
            hidePreview();
            var content = document.getElementById('pdPersonalDetailModalBody');
            var modal = modalNode();
            if (modal && content) {
                modal.classList.toggle('pd-work-detail-modal', !!content.querySelector('.pd-ds-table'));
                formatFullLaborHeading(content, modal);
            }
            showModal();
            var body = modal && modal.querySelector('.modal-body');
            if (body) body.scrollTop = 0;
        }
        function showPreview(trigger) {
            if (!trigger || trigger === activeTrigger) return;
            hidePreview();
            var template = trigger.parentElement && trigger.parentElement.querySelector('.pd-ds-hover-template');
            if (!template) return;
            var floating = document.createElement('div');
            floating.className = 'pd-ds-floating-preview'; floating.setAttribute('role', 'tooltip');
            floating.innerHTML = template.innerHTML; document.body.appendChild(floating);
            preview = floating; activeTrigger = trigger;
            var rect = trigger.getBoundingClientRect();
            var width = floating.getBoundingClientRect().width, height = floating.getBoundingClientRect().height;
            var left = rect.right + 12;
            if (left + width > window.innerWidth - 10) left = rect.left - width - 12;
            if (left < 10) left = Math.max(10, (window.innerWidth - width) / 2);
            floating.style.left = left + 'px';
            floating.style.top = Math.max(10, Math.min(rect.top, window.innerHeight - height - 10)) + 'px';
        }
        function jump(button) {
            var modalBody = button.closest('.modal-body'), scope = document.getElementById('pdPersonalDetailModalBody');
            if (!modalBody || !scope) return;
            var type = button.getAttribute('data-pd-ds-jump');
            if (!/^(task|meeting|issue)$/.test(type || '')) return;
            var target = scope.querySelector('[data-pd-detail-section="' + type + '"]');
            if (!target) return;
            var modal = modalNode(), heading = modal ? modal.querySelector('.pd-detail-modal-heading') : null;
            var reserved = heading ? heading.getBoundingClientRect().height + 18 : 18;
            var offset = target.getBoundingClientRect().top - modalBody.getBoundingClientRect().top;
            modalBody.scrollTo({ top: Math.max(0, modalBody.scrollTop + offset - reserved), behavior: 'smooth' });
        }
        if (!window.__pdPopupHandlers17) {
            window.__pdPopupHandlers17 = true;
            document.addEventListener('mouseover', function (e) {
                var t = e.target.closest && e.target.closest('#pdPersonalDetailModalBody .pd-ds-hover-trigger');
                if (t) showPreview(t);
            });
            document.addEventListener('mouseout', function (e) {
                if (activeTrigger && activeTrigger.contains(e.target) && (!e.relatedTarget || !activeTrigger.contains(e.relatedTarget))) hidePreview();
            });
            document.addEventListener('focusin', function (e) {
                var t = e.target.closest && e.target.closest('#pdPersonalDetailModalBody .pd-ds-hover-trigger');
                if (t) showPreview(t);
            });
            document.addEventListener('focusout', function (e) { if (e.target === activeTrigger) hidePreview(); });
            document.addEventListener('scroll', hidePreview, true);
            document.addEventListener('click', function (e) {
                var b = e.target.closest && e.target.closest('#pdPersonalDetailModalBody [data-pd-ds-jump], #pdPersonalDetailEquation [data-pd-ds-jump]');
                if (b) { e.preventDefault(); jump(b); }
            });
            document.addEventListener('hidden.bs.modal', function (e) { if (e.target === modalNode()) hidePreview(); });
            // In case AJAX fails, don't leave the popup permanently locked.
            if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                var prm = Sys.WebForms.PageRequestManager.getInstance();
                prm.add_endRequest(function (sender, args) {
                    if (pending && args.get_error()) {
                        pending = false;
                        var body = document.getElementById('pdPersonalDetailModalBody');
                        if (body) body.textContent = 'Không tải được dữ liệu chi tiết. Vui lòng thử lại.';
                        // Let global error handler own the ASP.NET AJAX failure; no alert.
                    }
                });
            }
        }
        window.QLDAProjectDetailPopup = { version: 17, open: open, received: received };
    })();
</script>
