<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlStartTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlStartTask" %>
<style>
    div[id*="mdlStartTask"] .modal-dialog, .modal-dialog:has(.st-modal-body) { max-width: 800px !important; width: 800px !important; }

    .st-modal-body { padding: 8px 20px 16px !important; background: #ffffff; color: #1e293b; text-align: center; font-size: 14.5px; }
    .st-modal-body * { box-sizing: border-box; }
    
    .st-target-wrapper { 
        display: inline-block; 
        width: fit-content; 
        margin: 0 auto 12px; 
        padding: 10px 24px; 
        border: 1px solid #d8b4fe; 
        border-radius: 12px; 
        background: linear-gradient(135deg, #fefeff 0%, #f5f3ff 100%); 
        box-shadow: 0 4px 12px rgba(139, 92, 246, 0.08); 
        max-width: 95%; 
    }
    .st-target-label { display: block; font-size: 12px; font-weight: 800; color: #8b5cf6; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 6px; }
    .st-target-title { font-size: 16.5px; color: #334155; line-height: 1.4; font-weight: 600; display: flex; align-items: center; justify-content: center; flex-wrap: wrap; gap: 8px; }
    .st-hero-code { font-weight: 900; color: #ffffff; background: #7c3aed; padding: 3px 8px; border-radius: 6px; font-size: 14.5px; box-shadow: 0 2px 5px rgba(124, 58, 237, 0.25); }
    .st-hero-name { color: #1e293b; font-weight: 800; font-size: 16.5px; }

    .st-action-hero { text-align: center; margin-bottom: 14px; }
    .st-hero-icon { display: flex; align-items: center; justify-content: center; width: 44px; height: 44px; margin: 0 auto 8px; border-radius: 50%; color: #fff; background: linear-gradient(135deg, #7c3aed 0%, #4c1d95 100%); font-size: 18px; box-shadow: 0 6px 12px rgba(109, 40, 217, 0.25); animation: stPulse 2s infinite; }
    .st-hero-heading { margin: 0 0 4px; color: #0f172a; font-size: 19px; font-weight: 800; letter-spacing: -0.2px; }
    .st-hero-subtitle { margin: 0; color: #64748b; font-size: 14.5px; }
    .st-hero-subtitle strong { color: #6d28d9; font-weight: 750; }

    .st-alert-box { text-align: left; background: #fff; border: 1px solid #fecaca; border-radius: 10px; box-shadow: 0 4px 12px rgba(220, 38, 38, 0.05); animation: stFadeIn 0.4s ease; margin-bottom: 4px; }
    
    .st-flow-diagram { display: flex; align-items: flex-start; justify-content: space-between; padding: 14px 18px; background: #fafafa; border-bottom: 1px solid #f1f5f9; gap: 12px; border-radius: 10px 10px 0 0; }
    .st-flow-diagram.no-parent { align-items: center; }
    
    .st-flow-col { flex: 1; display: flex; flex-direction: column; gap: 8px; min-width: 0; }
    .st-flow-connector-horizontal { flex: 0 0 24px; display: flex; align-items: center; justify-content: center; color: #94a3b8; font-size: 18px; padding-top: 18px; }
    .st-flow-diagram.no-parent .st-flow-connector-horizontal { padding-top: 0; }

    .st-flow-node { background: #fff; border: 1px solid #cbd5e1; border-radius: 6px; padding: 8px 10px; text-align: center; box-shadow: 0 2px 3px rgba(0,0,0,0.02); }
    .st-flow-node.st-waiting { border-color: #fca5a5; background: #fef2f2; }
    .st-flow-node.st-target { border-color: #93c5fd; background: #eff6ff; }
    .st-flow-node.st-parent { border-color: #c4b5fd; background: #f5f3ff; }
    
    .st-node-kicker { display: block; font-size: 10.5px; font-weight: 800; color: #94a3b8; text-transform: uppercase; margin-bottom: 4px; letter-spacing: 0.5px; }
    .st-flow-node.st-waiting .st-node-kicker { color: #ef4444; }
    .st-flow-node.st-target .st-node-kicker { color: #3b82f6; }
    .st-flow-node.st-parent .st-node-kicker { color: #8b5cf6; }
    
    .st-node-title { display: block; font-size: 14px; font-weight: 600; color: #1e293b; margin-bottom: 6px; line-height: 1.35; overflow-wrap: anywhere; }
    .st-node-title strong { font-weight: 800; font-size: 14.5px; }
    .st-flow-node.st-waiting .st-node-title strong { color: #b91c1c; }
    .st-flow-node.st-target .st-node-title strong { color: #1d4ed8; }
    .st-flow-node.st-parent .st-node-title strong { color: #6d28d9; }
    
    .st-node-badge { display: inline-flex; align-items: center; gap: 4px; padding: 2px 6px; border-radius: 4px; font-size: 10.5px; font-weight: 800; text-transform: uppercase; letter-spacing: 0.2px; }
    .st-flow-node.st-waiting .st-node-badge { background: #fee2e2; color: #b91c1c; }
    .st-flow-node.st-target .st-node-badge { background: #dbeafe; color: #1d4ed8; }
    .st-flow-node.st-parent .st-node-badge { background: #ede9fe; color: #6d28d9; }
    
    .st-alert-msg { display: flex; align-items: flex-start; gap: 10px; padding: 12px 18px; background: #fffcfc; color: #991b1b; font-size: 14.5px; line-height: 1.5; border-radius: 0 0 10px 10px; font-weight: 500; }
    .st-alert-msg > i { font-size: 18px; color: #ef4444; margin-top: 1px; }
    
    .st-tooltip { position: relative; font-weight: 800; cursor: help; padding: 1px 5px; border-radius: 4px; display: inline-flex; transition: all 0.2s; margin: 0 2px;}
    .st-tooltip.waiting { background: #fee2e2; color: #b91c1c; border: 1px solid #fca5a5; }
    .st-tooltip.target { background: #dbeafe; color: #1d4ed8; border: 1px solid #93c5fd; }
    .st-tooltip.parent { background: #f3e8ff; color: #6d28d9; border: 1px solid #d8b4fe; }
    
    .st-tooltip::after { 
        content: attr(data-title); position: absolute; bottom: 100%; left: 50%; transform: translateX(-25%) translateY(-3px); 
        color: #ffffff; font-size: 13.5px; font-weight: 600; padding: 6px 10px; border-radius: 6px; width: max-content; 
        max-width: 250px; white-space: normal; line-height: 1.4; opacity: 0; visibility: hidden; z-index: 9999; pointer-events: none; 
        transition: all 0.25s cubic-bezier(0.34, 1.56, 0.64, 1); 
    }
    .st-tooltip::before { 
        content: ""; position: absolute; bottom: 100%; left: 50%; transform: translateX(-50%) translateY(4px); 
        border: 5px solid transparent; opacity: 0; visibility: hidden; z-index: 9999; pointer-events: none; transition: all 0.2s ease; 
    }
    
    .st-tooltip.waiting::after { background: linear-gradient(135deg, #991b1b 0%, #450a0a 100%); border: 1px solid rgba(248, 113, 113, 0.35); box-shadow: 0 8px 15px rgba(153, 27, 27, 0.25), inset 0 1px 0 rgba(255,255,255,0.1); }
    .st-tooltip.waiting::before { border-top-color: #991b1b; }

    .st-tooltip.target::after { background: linear-gradient(135deg, #1e40af 0%, #172554 100%); border: 1px solid rgba(96, 165, 250, 0.35); box-shadow: 0 8px 15px rgba(30, 64, 175, 0.25), inset 0 1px 0 rgba(255,255,255,0.1); }
    .st-tooltip.target::before { border-top-color: #1e40af; }

    .st-tooltip.parent::after { background: linear-gradient(135deg, #6d28d9 0%, #2e1065 100%); border: 1px solid rgba(167, 139, 250, 0.35); box-shadow: 0 8px 15px rgba(109, 40, 217, 0.25), inset 0 1px 0 rgba(255,255,255,0.1); }
    .st-tooltip.parent::before { border-top-color: #6d28d9; }
    
    .st-tooltip:hover::after { opacity: 1; visibility: visible; transform: translateX(-25%) translateY(-6px); }
    .st-tooltip:hover::before { opacity: 1; visibility: visible; transform: translateX(-50%) translateY(-2px); }

    /* FOOTER BUTTONS */
    .st-footer-actions { display: flex; justify-content: flex-end; gap: 8px; }
    .st-cancel-btn { border: 1px solid #cbd5e1 !important; color: #475569 !important; background: #fff !important; border-radius: 8px !important; font-weight: 600 !important; transition: all 0.2s !important; padding: 6px 16px !important; font-size: 14.5px !important;}
    .st-cancel-btn:hover { background: #f1f5f9 !important; color: #0f172a !important; border-color: #94a3b8 !important; }
    .st-confirm-btn { border: none !important; border-radius: 8px !important; background: linear-gradient(135deg, #7c3aed 0%, #4c1d95 100%) !important; color: #fff !important; font-weight: 700 !important; box-shadow: 0 4px 10px rgba(109, 40, 217, 0.2) !important; transition: all 0.2s !important; padding: 6px 16px !important; font-size: 14.5px !important;}
    .st-confirm-btn:hover { transform: translateY(-1px); box-shadow: 0 6px 12px rgba(109, 40, 217, 0.3) !important; }

    @keyframes stPulse { 0% { box-shadow: 0 0 0 0 rgba(124, 58, 237, 0.3); } 70% { box-shadow: 0 0 0 10px rgba(124, 58, 237, 0); } 100% { box-shadow: 0 0 0 0 rgba(124, 58, 237, 0); } }
    @keyframes stFadeIn { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }

    @media(max-width: 768px) {
        div[id*="mdlStartTask"] .modal-dialog, .modal-dialog:has(.st-modal-body) { max-width: 95vw !important; width: 95vw !important; margin: 10px auto; }
    }
    @media(max-width: 575.98px) {
        .st-modal-body { padding: 12px !important; }
        .st-target-wrapper { padding: 10px; margin-bottom: 10px; }
        .st-flow-diagram { flex-direction: column; gap: 8px; padding: 12px; }
        .st-flow-col { width: 100%; }
        .st-flow-connector-horizontal { transform: rotate(90deg); padding-top:0; padding-left: 20px; margin: -5px 0;}
        .st-alert-msg { padding: 10px 12px; font-size: 14px; }
        .st-tooltip::after { transform: translateX(-10%) translateY(-3px); }
        .st-tooltip:hover::after { transform: translateX(-10%) translateY(-8px); }
    }

    /* Loading khi xác nhận bắt đầu: phong cách TaskList, độc lập với modal và UpdatePanel. */
    #st-start-processing-overlay { position:fixed;inset:0;z-index:2147483000;display:flex;flex-direction:column;align-items:center;justify-content:center;gap:0;background:rgba(255,255,255,.6);backdrop-filter:blur(3px);-webkit-backdrop-filter:blur(3px);opacity:0;visibility:hidden;pointer-events:none;transition:opacity .25s ease,visibility .25s ease; }
    #st-start-processing-overlay.is-active { opacity:1;visibility:visible;pointer-events:auto; }
    #st-start-processing-overlay .st-start-loading-ring { width:54px;height:54px;margin-bottom:16px;border:4px solid #ede9fe;border-top-color:#7c3aed;border-radius:50%;box-shadow:0 4px 12px rgba(124,58,237,.15);animation:stStartLoadingSpin .85s linear infinite; }
    #st-start-processing-overlay .st-start-loading-title { color:#5b21b6;font-size:15px;font-weight:800;letter-spacing:.45px;text-transform:uppercase;animation:stStartLoadingText 1.5s ease-in-out infinite; }
    #st-start-processing-overlay .st-start-loading-subtitle { margin-top:6px;color:#64748b;font-size:12px;font-weight:600; }
    .st-confirm-btn.st-start-busy { cursor:progress!important; }
    @keyframes stStartLoadingSpin { to { transform:rotate(360deg); } }
    @keyframes stStartLoadingText { 0%,100% { opacity:1; } 50% { opacity:.55; } }
    @media(prefers-reduced-motion:reduce) { #st-start-processing-overlay .st-start-loading-ring,#st-start-processing-overlay .st-start-loading-title { animation:none; } }
</style>


<script type="text/javascript">
    // Chặn bấm lặp, phản hồi ngay, nhưng KHÔNG chặn postback WebForms của LinkButton.
    window.SweetStartTaskLoading = window.SweetStartTaskLoading || (function () {
        var inProgress = false, activeButton = null, oldButtonHtml = '', currentRequestManager = null;

        function getOverlay() {
            var overlay = document.getElementById('st-start-processing-overlay');
            if (!overlay) {
                overlay = document.createElement('div');
                overlay.id = 'st-start-processing-overlay';
                overlay.setAttribute('role', 'status');
                overlay.setAttribute('aria-live', 'polite');
                overlay.innerHTML = '<span class="st-start-loading-ring" aria-hidden="true"></span>' +
                    '<strong class="st-start-loading-title">Đang bắt đầu công việc...</strong>' +
                    '<span class="st-start-loading-subtitle">Hệ thống đang ghi nhận thao tác</span>';
                document.body.appendChild(overlay);
            }
            return overlay;
        }

        function finish() {
            inProgress = false;
            var overlay = document.getElementById('st-start-processing-overlay');
            if (overlay) overlay.classList.remove('is-active');
            if (activeButton && document.documentElement.contains(activeButton)) {
                activeButton.innerHTML = oldButtonHtml;
                activeButton.classList.remove('st-start-busy');
                activeButton.removeAttribute('aria-disabled');
            }
            activeButton = null;
            oldButtonHtml = '';
        }

        function begin(button) {
            if (inProgress || !button) return false;
            // Request khác đang chạy: WebForms sẽ không xử lý đồng thời hai async postback.
            var prm = currentRequestManager;
            if (prm && typeof prm.get_isInAsyncPostBack === 'function' && prm.get_isInAsyncPostBack()) return false;
            inProgress = true;
            activeButton = button;
            oldButtonHtml = button.innerHTML;
            button.innerHTML = '<i class="fas fa-circle-notch fa-spin me-2" aria-hidden="true"></i> Đang bắt đầu...';
            button.classList.add('st-start-busy');
            button.setAttribute('aria-disabled', 'true');
            getOverlay().classList.add('is-active');
            // Trả về true để LinkButton tiếp tục __doPostBack / WebForm_DoPostBackWithOptions.
            return true;
        }

        function wirePostback() {
            if (!window.Sys || !Sys.WebForms || !Sys.WebForms.PageRequestManager) return;
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            if (!prm || currentRequestManager === prm) return;
            currentRequestManager = prm;
            // Dù server thành công hay báo lỗi, khi AJAX kết thúc thì gỡ loading.
            prm.add_endRequest(function () { if (inProgress) finish(); });
        }

        wirePostback();
        if (window.Sys && Sys.Application) Sys.Application.add_load(wirePostback);
        if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', wirePostback);
        window.addEventListener('pageshow', function (e) { if (e.persisted) finish(); });
        return { begin: begin, finish: finish };
    })();
</script>

<SweetSoft:ExtraModal runat="server" ID="mdlStartTask" Type="Primary" Title="Xác nhận thao tác" Width="800px">
    <ContentTemplate>
        <asp:UpdatePanel ID="upnlStartTask" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:HiddenField ID="hdfStartTaskId" runat="server" />
                <div class="st-modal-body text-center">
                    
                    <div class="st-target-wrapper">
                        <span class="st-target-label">Đang thao tác trên công việc</span>
                        <div class="st-target-title"><asp:Literal ID="ltrTaskName" runat="server"></asp:Literal></div>
                    </div>

                    <div class="st-action-hero">
                        <div class="st-hero-icon"><i class="fas fa-play" aria-hidden="true"></i></div>
                        <h5 class="st-hero-heading">Sẵn sàng bắt đầu thực hiện?</h5>
                        <p class="st-hero-subtitle">Công việc này sẽ được chuyển sang trạng thái <strong>Đang thực hiện</strong>.</p>
                    </div>
                    
                    <asp:Panel ID="pnlDependencyWarning" runat="server" Visible="false">
                        <asp:Literal ID="ltrDependencyWarning" runat="server"></asp:Literal>
                    </asp:Panel>

                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
    <FooterTemplate>
        <asp:UpdatePanel ID="upnlFooterStartTask" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="st-footer-actions">
                    <asp:LinkButton ID="btnConfirmStart" runat="server" CssClass="btn px-4 st-confirm-btn" CausesValidation="false" OnClientClick="if (!SweetStartTaskLoading.begin(this)) return false;" OnClick="btnConfirmStart_Click"><i class="fas fa-play me-2" aria-hidden="true"></i> Bắt đầu ngay</asp:LinkButton>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </FooterTemplate>
</SweetSoft:ExtraModal>