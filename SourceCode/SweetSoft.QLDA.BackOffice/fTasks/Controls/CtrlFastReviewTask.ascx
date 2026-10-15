<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlFastReviewTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlFastReviewTask" %>

<style>
    .fr-task-box { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 14px 16px; margin-bottom: 14px; }
    .fr-info-box { background: #ffffff; border: 1px solid #e2e8f0; border-radius: 8px; padding: 8px 16px; margin-bottom: 14px; }
    .fr-info-row { display: flex; justify-content: space-between; align-items: flex-start; gap: 16px; padding: 11px 0; border-bottom: 1px solid #edf2f7; }
    .fr-info-row:last-child { border-bottom: 0; }
    .fr-info-label { flex: 0 0 43%; color: #64748b; font-size: 13px; font-weight: 600; }
    .fr-info-value { flex: 1; min-width: 0; color: #1e293b; font-size: 13px; font-weight: 600; text-align: right; overflow-wrap: anywhere; white-space: pre-line; }
    .fr-badge { display: inline-block; border-radius: 5px; padding: 5px 9px; font-size: 12px; font-weight: 700; }
    .fr-badge-ontime { background: #dcfce7; color: #166534; border: 1px solid #86efac; }
    .fr-badge-late { background: #fee2e2; color: #b91c1c; border: 1px solid #fca5a5; }
    .fr-note { background: #eff6ff; border: 1px solid #bfdbfe; border-radius: 7px; color: #1e40af; padding: 11px 13px; font-size: 13px; line-height: 1.6; }
    .fr-btn-reject { color: #b91c1c !important; background: #fff !important; border: 1px solid #fca5a5 !important; }
    .fr-btn-reject:hover { background: #fef2f2 !important; border-color: #dc2626 !important; color: #991b1b !important; }
    .fr-reject-client-error { display:block; margin-top:6px; color:#b91c1c; font-size:12px; }
    .fr-decision-loading-overlay { position:fixed; inset:0; z-index:2147483646; display:flex; align-items:center; justify-content:center; padding:20px; background:rgba(248,250,252,.78); backdrop-filter:blur(2px); }
    .fr-decision-loading-panel { width:100%; max-width:310px; padding:24px 22px; border:1px solid #e2e8f0; border-radius:14px; background:#fff; box-shadow:0 12px 35px rgba(15,23,42,.14); text-align:center; font-family:Arial,sans-serif; }
    .fr-decision-spinner { width:36px; height:36px; margin:0 auto 12px; border:4px solid #e9e1f8; border-top-color:#6f42c1; border-radius:50%; animation:frDecisionSpin .7s linear infinite; }
    @keyframes frDecisionSpin { to { transform:rotate(360deg); } }
    .fr-decision-busy { opacity:.72 !important; pointer-events:none !important; }
    @media (max-width: 575.98px) { .fr-info-row { gap: 10px; } .fr-info-label { flex-basis: 40%; } }
</style>

<SweetSoft:ExtraModal runat="server" ID="mdlFastReview" Type="Primary" Title="Duyệt nhanh hoàn thành công việc" DefaultButton="btnApproveCompletion">
    <ContentTemplate>
        <asp:UpdatePanel ID="upnlReviewComplete" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:HiddenField ID="hdfReviewTaskId" runat="server" />

                <div class="p-2">
                    <div class="fr-task-box text-center">
                        <div class="text-muted mb-2" style="font-size: 12.5px;">Yêu cầu hoàn thành đang chờ PM duyệt</div>
                        <div class="fw-bold text-dark"><asp:Literal ID="ltrReviewTaskName" runat="server" /></div>
                    </div>

                    <div class="fr-info-box">
                        <div class="fr-info-row">
                            <div class="fr-info-label"><i class="far fa-calendar-check me-1"></i> Ngày báo hoàn thành</div>
                            <div class="fr-info-value text-primary"><asp:Literal ID="ltrActualDate" runat="server" /></div>
                        </div>
                        <div class="fr-info-row">
                            <div class="fr-info-label"><i class="far fa-calendar-alt me-1"></i> Ngày kết thúc dự kiến</div>
                            <div class="fr-info-value"><asp:Literal ID="ltrExpectedEndDate" runat="server" /></div>
                        </div>
                        <div class="fr-info-row">
                            <div class="fr-info-label"><i class="fas fa-chart-line me-1"></i> Kết quả theo thời hạn</div>
                            <div class="fr-info-value"><asp:Literal ID="ltrReviewResult" runat="server" /></div>
                        </div>
                        <div class="fr-info-row">
                            <div class="fr-info-label"><i class="fas fa-pen-alt me-1"></i> Lý do trễ hạn</div>
                            <div class="fr-info-value"><asp:Literal ID="ltrLateReason" runat="server" /></div>
                        </div>
                    </div>

                    <div class="fr-note">
                        <i class="fas fa-info-circle me-1"></i>
                        Kiểm tra kết quả thực tế trước khi duyệt. Chọn <strong>Duyệt hoàn thành</strong> nếu đạt yêu cầu; chọn <strong>Từ chối</strong> nếu cần người thực hiện tiếp tục chỉnh sửa. Khi từ chối, ngày hoàn thành thực tế sẽ được xóa và hạn dự kiến vẫn được giữ nguyên.
                    </div>

                    <div class="fr-reject-reason mt-3">
                        <label for="<%= txtRejectReason.ClientID %>" class="form-label fw-semibold" style="font-size:13px;color:#334155;">
                            Lý do từ chối <span class="text-danger">(bắt buộc khi từ chối)</span>
                        </label>
                        <asp:TextBox ID="txtRejectReason" runat="server" TextMode="MultiLine" Rows="3"
                            MaxLength="1000" CssClass="form-control" placeholder="Nêu rõ nội dung cần chỉnh sửa hoặc lý do chưa đạt yêu cầu..." />
                        <div id="frRejectReasonClientError" class="fr-reject-client-error" style="display:none;">Vui lòng nhập lý do từ chối trước khi trả công việc về thực hiện.</div>
                        <div class="form-text">Lý do này sẽ được gửi trong thông báo kết quả cho các thành viên được giao công việc.</div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
    <FooterTemplate>
        <asp:UpdatePanel ID="upnlReviewFooter" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="d-flex justify-content-end gap-2 flex-wrap">
                    <asp:LinkButton ID="btnRejectCompletion" runat="server" CausesValidation="false" CssClass="btn fr-btn-reject" OnClientClick="return beginFastReviewDecision('reject', this);" OnClick="btnRejectCompletion_Click">
                        <i class="fas fa-undo me-1"></i>Từ chối - trả về thực hiện
                    </asp:LinkButton>
                    <asp:LinkButton ID="btnApproveCompletion" runat="server" CssClass="btn btn-primary px-4" OnClientClick="return beginFastReviewDecision('approve', this);" OnClick="btnApproveCompletion_Click">
                        <i class="fas fa-check-circle me-1"></i>Duyệt hoàn thành
                    </asp:LinkButton>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </FooterTemplate>
</SweetSoft:ExtraModal>


<script type="text/javascript">
    (function () {
        var rejectReasonClientId = '<%= txtRejectReason.ClientID %>';
        var minVisibleMs = 450;

        function getRejectReason() {
            return document.getElementById(rejectReasonClientId);
        }

        function clearRejectReasonError() {
            var input = getRejectReason();
            var error = document.getElementById('frRejectReasonClientError');
            if (input) {
                input.classList.remove('is-invalid');
                input.removeAttribute('aria-invalid');
            }
            if (error) error.style.display = 'none';
        }

        function showRejectReasonError() {
            var input = getRejectReason();
            var error = document.getElementById('frRejectReasonClientError');
            if (input) {
                input.classList.add('is-invalid');
                input.setAttribute('aria-invalid', 'true');
                input.focus();
            }
            if (error) error.style.display = 'block';
        }

        function hideDecisionLoading() {
            var overlay = document.getElementById('fr-decision-loading-overlay');
            if (overlay && overlay.parentNode) overlay.parentNode.removeChild(overlay);
            window.__fastReviewDecisionBusy = false;
            document.querySelectorAll('.fr-decision-busy').forEach(function(el) {
                el.classList.remove('fr-decision-busy');
                el.removeAttribute('aria-disabled');
            });
        }

        function showDecisionLoading(action, clickedButton) {
            if (document.getElementById('fr-decision-loading-overlay')) return;

            var overlay = document.createElement('div');
            overlay.id = 'fr-decision-loading-overlay';
            overlay.className = 'fr-decision-loading-overlay';
            overlay.setAttribute('role', 'status');
            overlay.setAttribute('aria-live', 'polite');

            var panel = document.createElement('div');
            panel.className = 'fr-decision-loading-panel';

            var spinner = document.createElement('div');
            spinner.className = 'fr-decision-spinner';

            var title = document.createElement('div');
            title.style.cssText = 'font-size:15px;font-weight:700;color:#334155;margin-bottom:6px;';
            title.textContent = action === 'approve' ? 'Đang duyệt hoàn thành...' : 'Đang gửi yêu cầu từ chối...';

            var message = document.createElement('div');
            message.style.cssText = 'font-size:12.5px;line-height:1.55;color:#64748b;';
            message.textContent = 'Đang cập nhật trạng thái và gửi thông báo cho thành viên. Vui lòng chờ.';

            panel.appendChild(spinner);
            panel.appendChild(title);
            panel.appendChild(message);
            overlay.appendChild(panel);
            document.body.appendChild(overlay);

            if (clickedButton) {
                clickedButton.classList.add('fr-decision-busy');
                clickedButton.setAttribute('aria-disabled', 'true');
            }
            document.querySelectorAll('[id$="btnApproveCompletion"], [id$="btnRejectCompletion"]').forEach(function(el) {
                el.classList.add('fr-decision-busy');
                el.setAttribute('aria-disabled', 'true');
            });

            window.__fastReviewDecisionStartedAt = Date.now();
        }

        window.beginFastReviewDecision = function(action, clickedButton) {
            if (window.__fastReviewDecisionBusy) return false;

            if (action === 'reject') {
                var reasonInput = getRejectReason();
                if (!reasonInput || !reasonInput.value || !reasonInput.value.trim()) {
                    showRejectReasonError();
                    return false;
                }
                clearRejectReasonError();
            }

            window.__fastReviewDecisionBusy = true;
            showDecisionLoading(action, clickedButton);
            return true;
        };

        var reasonBox = getRejectReason();
        if (reasonBox) reasonBox.addEventListener('input', clearRejectReasonError);

        function attachEndRequestHandler() {
            if (window.__fastReviewDecisionEndRequestAttached) return;
            if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                var manager = Sys.WebForms.PageRequestManager.getInstance();
                manager.add_endRequest(function() {
                    var startedAt = window.__fastReviewDecisionStartedAt || Date.now();
                    var remaining = Math.max(0, minVisibleMs - (Date.now() - startedAt));
                    window.setTimeout(hideDecisionLoading, remaining);
                });
                window.__fastReviewDecisionEndRequestAttached = true;
                return;
            }

            // Trên một số MasterPage, MicrosoftAjax được tải sau UserControl.
            // Thử gắn lại handler một thời gian ngắn để overlay không bị kẹt sau async postback.
            window.__fastReviewDecisionHookAttempts = window.__fastReviewDecisionHookAttempts || 0;
            if (window.__fastReviewDecisionHookAttempts++ < 60) {
                window.setTimeout(attachEndRequestHandler, 100);
            }
        }

        attachEndRequestHandler();
        if (window.Sys && Sys.Application && typeof Sys.Application.add_load === 'function') {
            Sys.Application.add_load(attachEndRequestHandler);
        }
    })();
</script>

<!-- Popup kết quả duyệt/từ chối được mở khi người được giao nhấn thông báo -->
<SweetSoft:ExtraModal runat="server" ID="mdlTaskCompletionResult" Type="Primary" Title="Kết quả xét duyệt hoàn thành công việc">
    <ContentTemplate>
        <asp:UpdatePanel ID="upnlTaskCompletionResult" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="p-3 fr-completion-result">
                    <div class="text-center mb-3">
                        <asp:Literal ID="ltrCompletionDecisionBadge" runat="server" />
                    </div>
                    <div class="fr-task-box text-center">
                        <div class="text-muted mb-2" style="font-size:12.5px;">Công việc</div>
                        <div class="fw-bold text-dark"><asp:Literal ID="ltrCompletionResultTaskName" runat="server" /></div>
                    </div>
                    <div class="fr-info-box">
                        <div class="fr-info-row">
                            <div class="fr-info-label"><i class="far fa-clock me-1"></i> Thời điểm thông báo</div>
                            <div class="fr-info-value"><asp:Literal ID="ltrCompletionResultDate" runat="server" /></div>
                        </div>
                        <div class="fr-result-message" style="white-space:pre-line;line-height:1.7;color:#334155;font-size:13px;padding:12px 2px;">
                            <asp:Literal ID="ltrCompletionResultBody" runat="server" />
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
    <FooterTemplate>
        <div class="d-flex justify-content-end">
        </div>
    </FooterTemplate>
</SweetSoft:ExtraModal>